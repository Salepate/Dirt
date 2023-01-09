using Dirt.Game;
using Dirt.Network.Simulation.Components;
using Dirt.Network.Simulation.Events;
using Dirt.Simulation;
using Dirt.Simulation.Action;
using Dirt.Simulation.Actor;
using Dirt.Simulation.Actor.Components;
using Dirt.Simulation.Context;
using Dirt.Simulation.SystemHelper;
using Mud;
using Mud.Managers;
using System.Collections.Generic;
using System.IO;

namespace Dirt.Network.Simulation.Systems
{
    public class ClientActorSynchronization : ISimulationSystem, IEventReader, IManagerAccess, IContextReader
    {
        private HashSet<int> m_ToDestroy;
        private ServerProxy m_Server;

        // Actions
        private ActorActionContext m_ActionContext;
        private List<ActionParameter> m_ParameterBuffer;
        private MemoryStream m_BufferStream;
        private BinaryWriter m_BufferWriter;
        private GameSimulation m_Simulation;
        private SimulationContext m_Context;
        private ActorFilter Filter => m_Simulation.Filter;

        public ClientActorSynchronization()
        {
            m_ToDestroy = new HashSet<int>();
            m_BufferStream = new MemoryStream();
            m_BufferWriter = new BinaryWriter(m_BufferStream);
            m_ParameterBuffer = new List<ActionParameter>();
        }

        [SimulationListener(typeof(ActorNetCullEvent), 0)]
        private void OnActorRemoved(ActorNetCullEvent removeEvent)
        {
            m_ToDestroy.Add(removeEvent.NetID);
        }

        [SimulationListener(typeof(RemoteActionRequestEvent), 0)]
        private void OnRemoteActionRequested(RemoteActionRequestEvent requestEvent)
        {
            GameActor actor;
            ActorAction action = null;
            //Dirt.Log.Console.Message("Net Action Request");
            if ( Filter.TryGetActor(requestEvent.SourceActor, out actor) &&
                m_ActionContext.TryGetAction(requestEvent.ActionIndex, out action))
            {
                m_ParameterBuffer.Clear();
                action.FetchParameters(m_Simulation, m_Context, actor, m_ParameterBuffer);
                int netID = Filter.Get<NetInfo>(actor).ID;
                m_BufferStream.Flush();
                m_BufferWriter.Seek(0, SeekOrigin.Begin);
                m_BufferWriter.Write(netID);
                m_BufferWriter.Write((byte)requestEvent.ActionIndex);
                for (int i = 0; i < m_ParameterBuffer.Count; ++i)
                {
                    m_BufferWriter.Write(m_ParameterBuffer[i].intValue);
                }
                MudMessage mudMessage = MudMessage.Create((int)NetworkOperation.ActionRequest, m_BufferStream.ToArray());
                m_Server.Send(mudMessage);
            }
            else
            {
                Dirt.Log.Console.Warning($"Could not send action (Actor: {actor != null}, Action: {action != null})");
            }
        }

        public void Initialize(GameSimulation sim)
        {
            m_Simulation = sim;
        }

        public void UpdateActors(GameSimulation sim, float deltaTime)
        {
            foreach(var t in sim.Filter.GetAll<NetInfo>())
            {
                ref NetInfo netBhv = ref t.Get();

                if (m_ToDestroy.Contains(netBhv.ID))
                {
                    sim.Builder.AddComponent<Destroy>(t.Actor);
                    m_ToDestroy.Remove(netBhv.ID);
                }
                else
                {
                    if (netBhv.Owner == m_Server.LocalPlayer) // has some ownership
                    {
                        if (netBhv.LastOutBuffer != null && netBhv.LastOutBuffer.Length > 0)
                        {
                            using (MemoryStream st = new MemoryStream())
                            {
                                st.WriteByte((byte)netBhv.ID);
                                st.Write(netBhv.LastOutBuffer, 0, netBhv.LastOutBuffer.Length);
                                m_Server.Send(MudMessage.Create((byte)NetworkOperation.ActorSync, st.ToArray()));
                            }
                            netBhv.LastOutBuffer = null;
                        }
                    }
                }
            }
        }

        public void SetManagers(IManagerProvider provider)
        {
            m_Server = provider.GetManager<ServerProxy>();
        }

        public void SetContext(SimulationContext context)
        {
            m_Context = context;
            m_ActionContext = context.GetContext<ActorActionContext>();
        }
    }
}