using Dirt.Game;
using Dirt.GameServer;
using Dirt.GameServer.Managers;
using Dirt.GameServer.Simulation.Components;
using Dirt.Log;
using Dirt.Network.Managers;
using Dirt.Network.Simulation;
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
    using Array = System.Array;
    public class NetworkCulling : ISimulationSystem, IManagerAccess
    {
        private NetworkSerializer m_Serializer;
        private PlayerManager m_Players;

        private GameSimulation m_Simulation;
        public void Initialize(GameSimulation sim)
        {
            m_Simulation = sim;
        }

        public void SetManagers(IManagerProvider provider)
        {
            m_Players = provider.GetManager<PlayerManager>();
            m_Serializer = provider.GetManager<NetworkSerializer>();
        }

        public void UpdateActors(GameSimulation sim, float deltaTime)
        {
            ActorFilter filter = sim.Filter;
            var syncable = filter.GetAll<NetInfo, Position>();
            var cullAreas = filter.GetAll<CullArea, Position>();
            List<int> inRange = new List<int>();

            foreach (ActorTuple<CullArea, Position> cullActor in cullAreas)
            {
                ref CullArea cull = ref cullActor.GetC1();
                ref Position cullPos = ref cullActor.GetC2();
                inRange.Clear();

                PlayerProxy player = m_Players.FindPlayer(cull.Client);
                if (player != null)
                {
                    foreach(ActorTuple<NetInfo, Position> syncActor in syncable)
                    {
                        ref Position syncPos = ref syncActor.GetC2();
                        ref NetInfo syncInfo = ref syncActor.GetC1();
                        if (syncInfo.ID == -1)
                            return;

                        float sqrRad = cull.Radius * cull.Radius;
                        float sqrRadOut = (cull.Radius + cull.Threshold) * (cull.Radius + cull.Threshold);
                        float sqrMag = (cullPos.Origin - syncPos.Origin).sqrMagnitude;
                        bool isOld =  cull.ProximityActors.Contains(syncInfo.ID);

                        if (sqrMag <= sqrRad || isOld && sqrMag < sqrRadOut)
                        {
                            inRange.Add(syncInfo.ID);

                            if (isOld)
                            {
                                if (syncInfo.LastOutBuffer != null)
                                {
                                    List<byte> message = new List<byte>(syncInfo.LastOutBuffer.Length + 1);
                                    message.Add((byte)syncInfo.ID);
                                    message.AddRange(syncInfo.LastOutBuffer);
                                    player.Client.Send(MudMessage.Create((int)NetworkOperation.ActorSync, message.ToArray()));
                                }
                            }
                            else
                            {
                                SendActorState(player.Client, syncActor.Actor);
                            }
                        }
                    }

                    foreach(int old in cull.ProximityActors)
                    {
                        if (!inRange.Contains(old))
                        {
                            player.Client.Send(MudMessage.Create((int)NetworkOperation.ActorRemove, new byte[] { (byte)old }));
                        }
                    }

                    cull.ProximityActors.Clear();
                    cull.ProximityActors.AddRange(inRange);
                }
            }
            foreach(var syncActor in syncable)
            {
                ref NetInfo netinfo = ref syncActor.GetC1();
                netinfo.LastOutBuffer = null;
            }
        }

        private void SendActorState(GameClient client, GameActor actor)
        {
            //BinaryFormatter bf = new BinaryFormatter();
            byte[] serializedData;
            SimulationPool compPool = m_Simulation.Builder.Components;

            using (MemoryStream ms = new MemoryStream())
            {
                //m_Serializer.Serialize(ms, actor);
                ActorState actorState = new ActorState()
                {
                    Components = new IComponent[actor.ComponentCount]
                };

                int compId = 0;

                for(int i = 0; i < actor.Components.Length; ++i)
                {
                    if ( actor.Components[i] != -1 )
                    {
                        GenericArray compArray = compPool.GetPool(actor.ComponentTypes[i]);
                        actorState.Components[compId++] = (IComponent) compArray.Get(actor.Components[i]);
                    }
                }
                m_Serializer.Serialize(ms, actorState);
                serializedData = ms.ToArray();
            }

            client.Send(MudMessage.Create((int)NetworkOperation.ActorState, serializedData));
        }
    }
}
