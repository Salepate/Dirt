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
        private ActorFilter m_Filter;

        public ActorReceiver()
        {
            m_LastInState = new Dictionary<int, MessageHeader>();
        }

        [SimulationListener(typeof(ActorSyncEvent), 0)]
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

        [SimulationListener(typeof(ActorEvent), ActorEvent.Removed)]
        private void OnActorRemoved(ActorEvent removeEvent)
        {
            int syncIndex = removeEvent.Actor.GetComponentIndex<NetInfo>();
            if (syncIndex != -1)
            {
                ref NetInfo net = ref m_Filter.Get<NetInfo>(removeEvent.Actor);
                if (m_LastInState.ContainsKey(net.ID))
                    m_LastInState.Remove(net.ID);
            }
        }

        public void Initialize(GameSimulation sim)
        {
            m_Filter = sim.Filter;
        }

        public void UpdateActors(GameSimulation sim, float deltaTime)
        {
            ActorList<NetInfo> netActors = m_Filter.GetActors<NetInfo>();
            for(int i = 0; i < netActors.Count; ++i)
            {
                ref NetInfo netBhv = ref netActors.GetC1(i);
                if (m_LastInState.ContainsKey(netBhv.ID))
                {
                    netBhv.LastInBuffer = m_LastInState[netBhv.ID];
                    m_LastInState.Remove(netBhv.ID);
                }
            }
        }

        public void SetManagers(IManagerProvider provider)
        {
            m_Serializer = provider.GetManager<NetworkSerializer>();
        }
    }
}