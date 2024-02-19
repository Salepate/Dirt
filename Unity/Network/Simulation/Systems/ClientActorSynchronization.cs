using Dirt.Game;
using Dirt.Network.Simulation.Components;
using Dirt.Network.Simulation.Events;
using Dirt.Simulation;
using Dirt.Simulation.Action;
using Dirt.Simulation.Actor;
using Dirt.Simulation.Context;
using Dirt.Simulation.SystemHelper;
using Mud;
using Mud.Managers;
using System.Collections.Generic;
using System.IO;
using System.Net.NetworkInformation;

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
        private ActorFilter Filter => m_Simulation.Filter;
        private ActorStream m_Stream;
        private int m_Frame;

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
            m_Stream = new ActorStream();
            m_Stream.Initialize(sim, true);

        }

        public void UpdateActors(GameSimulation sim, float deltaTime)
        {
            m_Frame++;

            var netActors = sim.Filter.GetActors<NetInfo>();
            for(int i = 0; i < netActors.Count; ++i)
            {
                ref NetInfo sync = ref netActors.GetC1(i);
                if (sync.Owned)
                {
                    m_Stream.SerializeActor(netActors.GetActor(i), ref sync, m_Frame);
                    if (sync.LastOutBuffer != null)
                    {
                        m_Server.SendRaw(sync.LastOutBuffer, sync.BufferSize);
                        sync.LastOutBuffer = null;
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
            m_ActionContext = context.GetContext<ActorActionContext>();
        }
    }
}