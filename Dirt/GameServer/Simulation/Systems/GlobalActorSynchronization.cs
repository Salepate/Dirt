using Dirt.Game;
using Dirt.GameServer;
using Dirt.GameServer.Managers;
using Dirt.GameServer.Simulation.Components;
using Dirt.Network.Managers;
using Dirt.Network.Simulation.Components;
using Dirt.Simulation;
using Dirt.Simulation.Actor;
using Dirt.Simulation.Components;
using Dirt.Simulation.Events;
using Dirt.Simulation.SystemHelper;
using Mud;
using Mud.Server;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Dirt.Network.Simulations.Systems
{
    public class GlobalActorSynchronization : ISimulationSystem, IManagerAccess
    {
        private bool m_StaticListChanged;
        private PlayerManager m_Players;
        private NetworkSerializer m_Serializer;
        private List<ActorTuple<NetInfo>> m_StaticActors;
        private List<int> m_Removed;
        private ActorFilter m_Filter;
        private GameSimulation m_Simulation;
        public void Initialize(GameSimulation sim)
        {
            m_StaticListChanged = true;
            m_Removed = new List<int>();
            m_Filter = sim.Filter;
            m_Simulation = sim;
        }

        public void SetManagers(IManagerProvider provider)
        {
            m_Players = provider.GetManager<PlayerManager>();
            m_Serializer = provider.GetManager<NetworkSerializer>();
        }


        [SimulationListener(typeof(ActorEvent), ActorEvent.Removed)]
        private void OnActorRemoved(ActorEvent actorEvent)
        {
            int netIdx = actorEvent.Actor.GetComponentIndex<NetInfo>();

            if (netIdx != -1)
            {
                ref NetInfo netInfo = ref m_Filter.Get<NetInfo>(actorEvent.Actor);
                m_Removed.Add(netInfo.ID);
            }
        }

        public void UpdateActors(GameSimulation sim, float deltaTime)
        {
            if (m_StaticListChanged)
            {
                HashSet<GameActor> staticActors = m_Filter.ExcludeActors<Position>();
                var netActors = m_Filter.GetAll<NetInfo>();
                m_StaticActors = netActors.Where(actor => staticActors.Contains(actor.Actor)).ToList();
                m_StaticListChanged = false;
            }

            var receivers = m_Filter.GetAll<GlobalSyncInfo>();

            List<int> inRange = new List<int>();


            foreach(ActorTuple<GlobalSyncInfo> receiveActor in receivers)
            {
                ref GlobalSyncInfo receiverNet = ref receiveActor.Get();

                inRange.Clear();

                PlayerProxy player = m_Players.FindPlayer(receiverNet.Client);
                if (player != null)
                {
                    GameClient client = player.Client;
                    foreach(ActorTuple<NetInfo> staticActor in m_StaticActors)
                    {
                        ref NetInfo staticNet = ref staticActor.Get();
                        if (staticNet.ID == -1)
                            return;

                        bool isOld = receiveActor.Get().SynchronizedActors.Contains(staticNet.ID);

                        if (isOld)
                        {
                            if (staticNet.LastOutBuffer != null)
                            {
                                List<byte> message = new List<byte>(staticNet.LastOutBuffer.Length + 1);
                                message.Add((byte)staticNet.ID);
                                message.AddRange(staticNet.LastOutBuffer);
                                client.Send(MudMessage.Create((int)NetworkOperation.ActorSync, message.ToArray()));
                            }
                        }
                        else
                        {
                            SendActorState(client, staticActor.Actor);
                            receiverNet.SynchronizedActors.Add(staticNet.ID);
                        }
                    }

                    foreach(int removedId in m_Removed)
                    {
                        int idx = receiverNet.SynchronizedActors.IndexOf(removedId);
                        if (idx != -1)
                            receiverNet.SynchronizedActors.RemoveAt(removedId);
                    }
                }
            }

            foreach(var sync in m_StaticActors)
            {
                ref NetInfo syncNet = ref sync.Get();
                syncNet.LastOutBuffer = null;
            }
            m_Removed.Clear();
        }

        private void SendActorState(GameClient client, GameActor actor)
        {
            //BinaryFormatter bf = new BinaryFormatter();
            byte[] serializedData;
            SimulationPool compPool = m_Simulation.Builder.Components;

            using (MemoryStream ms = new MemoryStream())
            {
                m_Serializer.Serialize(ms, actor);
                for (int i = 0; i < actor.Components.Length; ++i)
                {
                    if (actor.Components[i] != -1)
                    {
                        GenericArray compArray = compPool.GetPool(actor.ComponentTypes[i]);
                        m_Serializer.Serialize(ms, compArray.Get(actor.Components[i]));
                    }
                }
                serializedData = ms.ToArray();
            }

            client.Send(MudMessage.Create((int)NetworkOperation.ActorState, serializedData));
        }
    }
}
