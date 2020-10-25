using Dirt.Game;
using Dirt.Network.Managers;
using Dirt.Network.Simulation.Components;
using Dirt.Network.Simulation.Events;
using Dirt.Simulation;
using Dirt.Simulation.Actor;
using Dirt.Simulation.Events;
using Dirt.Simulation.SystemHelper;
using System.Collections.Generic;
using System.IO;

namespace Dirt.Network.Simulation.Systems
{
    public class ActorReceiver : ISimulationSystem, IEventReader, IManagerAccess
    {
        private Dictionary<int, MessageHeader> m_LastInState;

        private NetworkSerializer m_Serializer;
        public ActorReceiver()
        {
            m_LastInState = new Dictionary<int, MessageHeader>();
        }

        [EventListener(typeof(ActorSyncEvent), 0)]
        private void OnActorSync(ActorSyncEvent syncEvent)
        {
            using (MemoryStream ms = new MemoryStream(syncEvent.Buffer))
            {
                MessageHeader state = (MessageHeader)m_Serializer.Deserialize(ms);

                if (m_LastInState.TryGetValue(syncEvent.NetID, out MessageHeader oldState))
                {
                    MessageStateHelper.MergeState(state, oldState);
                }
                else
                {
                    m_LastInState.Add(syncEvent.NetID, state);
                }
            }
        }

        [EventListener(typeof(ActorEvent), ActorEvent.Removed)]
        private void OnActorRemoved(ActorEvent removeEvent)
        {
            int syncIndex = removeEvent.Actor.GetComponentIndex<NetInfo>();
            if (syncIndex != -1)
            {
                int netId = removeEvent.Actor.GetComponent<NetInfo>().ID;
                if (m_LastInState.ContainsKey(netId))
                    m_LastInState.Remove(netId);
            }
        }

        public void Initialize(GameSimulation sim)
        {
        }

        public void UpdateActors(List<GameActor> actors, float deltaTime)
        {
            actors.GetActors<NetInfo>().ForEach(t =>
            {
                NetInfo netBhv = t.Item2;

                if (m_LastInState.ContainsKey(netBhv.ID))
                {
                    netBhv.LastInBuffer = m_LastInState[netBhv.ID];
                    m_LastInState.Remove(netBhv.ID);
                }
            });
        }

        public void SetManagers(IManagerProvider provider)
        {
            m_Serializer = provider.GetManager<NetworkSerializer>();
        }
    }
}