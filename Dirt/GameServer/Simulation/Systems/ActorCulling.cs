using Dirt.Game;
using Dirt.GameServer;
using Dirt.GameServer.Managers;
using Dirt.GameServer.Simulation.Components;
using Dirt.Network.Managers;
using Dirt.Network.Simulation.Components;
using Dirt.Simulation;
using Dirt.Simulation.Actor;
using Dirt.Simulation.Components;
using Dirt.Simulation.SystemHelper;
using Mud;
using Mud.Server;
using System.Collections.Generic;
using System.IO;

namespace Dirt.Network.Simulations.Systems
{
    public class NetworkCulling : ISimulationSystem, IManagerAccess
    {
        private NetworkSerializer m_Serializer;
        private PlayerManager m_Players;
        public void Initialize(GameSimulation sim)
        {
        }

        public void SetManagers(IManagerProvider provider)
        {
            m_Players = provider.GetManager<PlayerManager>();
            m_Serializer = provider.GetManager<NetworkSerializer>();
        }

        public void UpdateActors(List<GameActor> actors, float deltaTime)
        {
            var syncable = actors.GetActors<NetInfo, Position>();

            List<int> inRange = new List<int>();

            actors.GetActors<CullArea, Position>().ForEach(t =>
            {
                CullArea cull = t.Item2;
                inRange.Clear();

                PlayerProxy player = m_Players.FindPlayer(cull.Client);
                if (player != null)
                {
                    syncable.ForEach(t2 =>
                    {
                        if (t2.Item2.ID == -1)
                            return;

                        NetInfo targetSync = t2.Item2;

                        float sqrRad = cull.Radius * cull.Radius;
                        float sqrRadOut = (cull.Radius + cull.Threshold) * (cull.Radius + cull.Threshold);
                        float sqrMag = (t.Item3.Origin - t2.Item3.Origin).sqrMagnitude;
                        bool isOld = cull.ProximityActors.Contains(targetSync.ID);

                        if (sqrMag <= sqrRad || isOld && sqrMag < sqrRadOut)
                        {
                            inRange.Add(targetSync.ID);

                            if (isOld)
                            {
                                if (t2.Item2.LastOutBuffer != null)
                                {
                                    List<byte> message = new List<byte>(t2.Item2.LastOutBuffer.Length + 1);
                                    message.Add((byte)targetSync.ID);
                                    message.AddRange(t2.Item2.LastOutBuffer);
                                    player.Client.Send(MudMessage.Create((int)NetworkOperation.ActorSync, message.ToArray()));
                                }
                            }
                            else
                            {
                                SendActorState(player.Client, t2.Item1);
                            }
                        }
                    });


                    //
                    cull.ProximityActors.ForEach(old =>
                    {
                        if (!inRange.Contains(old))
                        {
                            player.Client.Send(MudMessage.Create((int)NetworkOperation.ActorRemove, new byte[] { (byte)old }));
                        }
                    });

                    cull.ProximityActors.Clear();
                    cull.ProximityActors.AddRange(inRange);
                }
            });
            syncable.ForEach(sync => sync.Item2.LastOutBuffer = null);
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
