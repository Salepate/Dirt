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
            m_BufferStream = new MemoryStream();
            m_BufferWriter = new BinaryWriter(m_BufferStream);
            m_ParameterBuffer = new List<ActionParameter>();
        }

        [SimulationListener(typeof(RemoteActionRequestEvent), 0)]
        private void OnRemoteActionRequested(RemoteActionRequestEvent requestEvent)
        {
            GameActor actor;
            ActorAction action = null;
            if ( Filter.TryGetActor(requestEvent.SourceActor, out actor) &&
                m_ActionContext.TryGetAction(requestEvent.ActionIndex, out action))
            {
                m_ParameterBuffer.Clear();
                m_ParameterBuffer.AddRange(requestEvent.Parameters);
                int netID = Filter.Get<NetInfo>(actor).ID;
                MudMessage msg = NetworkActionHelper.CreateNetworkMessage(true, netID, requestEvent.ActionIndex, m_ParameterBuffer, m_BufferStream, m_BufferWriter);
                m_BufferStream.SetLength(0);
                m_Server.Send(msg);
            }
            else
            {
                Log.Console.Warning($"Could not send action (Actor: {actor != null}, Action: {action != null})");
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