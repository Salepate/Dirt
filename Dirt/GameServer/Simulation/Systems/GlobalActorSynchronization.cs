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
        public void Initialize(GameSimulation sim)
        {
            m_StaticListChanged = true;
            m_Removed = new List<int>();
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
                m_Removed.Add(((NetInfo)actorEvent.Actor.Components[netIdx]).ID);
            }
        }

        public void UpdateActors(List<GameActor> actors, float deltaTime)
        {
            if (m_StaticListChanged)
            {
                HashSet<GameActor> staticActors = actors.ExcludeActors<Position>();
                var netActors = actors.GetActors<NetInfo>();
                m_StaticActors = netActors.Where(actor => staticActors.Contains(actor.Item1)).ToList();
                m_StaticListChanged = false;
            }

            var receivers = actors.GetActors<GlobalSyncInfo>();

            List<int> inRange = new List<int>();


            receivers.ForEach(t =>
            {
                int clientIndex = t.Item2.Client;

                inRange.Clear();

                PlayerProxy player = m_Players.FindPlayer(clientIndex);
                if (player != null)
                {
                    GameClient client = player.Client;
                    m_StaticActors.ForEach(t2 =>
                    {
                        if (t2.Item2.ID == -1)
                            return;

                        NetInfo targetSync = t2.Item2;

                        bool isOld = t.Item2.SynchronizedActors.Contains(targetSync.ID);

                        if (isOld)
                        {
                            if (t2.Item2.LastOutBuffer != null)
                            {
                                List<byte> message = new List<byte>(t2.Item2.LastOutBuffer.Length + 1);
                                message.Add((byte)targetSync.ID);
                                message.AddRange(t2.Item2.LastOutBuffer);
                                client.Send(MudMessage.Create((int)NetworkOperation.ActorSync, message.ToArray()));
                            }
                        }
                        else
                        {
                            SendActorState(client, t2.Item1);
                            t.Item2.SynchronizedActors.Add(targetSync.ID);
                        }
                    });

                    m_Removed.ForEach(removedId =>
                    {
                        int idx = t.Item2.SynchronizedActors.IndexOf(removedId);
                        if (idx != -1)
                            t.Item2.SynchronizedActors.RemoveAt(removedId);
                    });
                }
            });

            m_StaticActors.ForEach(sync => sync.Item2.LastOutBuffer = null);
            m_Removed.Clear();
        }

        private void SendActorState(GameClient client, GameActor actor)
        {
            //BinaryFormatter bf = new BinaryFormatter();
            byte[] serializedData;
            using (MemoryStream ms = new MemoryStream())
            {
                m_Serializer.Serialize(ms, actor);
                serializedData = ms.ToArray();
            }

            client.Send(MudMessage.Create((int)NetworkOperation.ActorState, serializedData));
        }
    }
}
