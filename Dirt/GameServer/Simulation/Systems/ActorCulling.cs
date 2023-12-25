using Dirt.Game;
using Dirt.Game.Content;
using Dirt.GameServer;
using Dirt.GameServer.Managers;
using Dirt.GameServer.Simulation;
using Dirt.GameServer.Simulation.Components;
using Dirt.Network.Managers;
using Dirt.Network.Simulation;
using Dirt.Network.Simulation.Components;
using Dirt.Simulation;
using Dirt.Simulation.Actor;
using Dirt.Simulation.Components;
using Dirt.Simulation.Model;
using Dirt.Simulation.SystemHelper;
using Dirt.Simulation.Utility;
using Mud;
using Mud.Server;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace Dirt.Network.Simulations.Systems
{
    public class NetworkCulling : ISimulationSystem, IManagerAccess, IContentReader
    {
        private NetworkSerializer m_Serializer;
        private PlayerManager m_Players;
        private GameSimulation m_Simulation;
        private HashSet<Type> m_SkippedTypes;
        private List<IComponent> m_ComponentBuffer;
        private float m_NetTickDeltaTime;
        private float m_SyncClock;
        public void Initialize(GameSimulation sim)
        {
            m_Simulation = sim;
            m_ComponentBuffer = new List<IComponent>(10);
            m_SyncClock = 0f;
        }

        public void SetContent(IContentProvider content)
        {
            var asms = content.LoadContent<AssemblyCollection>(GameInstance.AssemblyCollection);
            Dictionary<string, Type> disabledSync = AssemblyReflection.BuildTypeMap<IComponent>(asms.Assemblies, (t) => t.GetCustomAttribute<DisableSyncAttribute>() != null);
            m_SkippedTypes = new HashSet<Type>();

            foreach (Type t in disabledSync.Values)
            {
                m_SkippedTypes.Add(t);
            }
        }

        public void SetManagers(IManagerProvider provider)
        {
            m_Players = provider.GetManager<PlayerManager>();
            m_Serializer = provider.GetManager<NetworkSerializer>();
            m_NetTickDeltaTime = 1f / provider.GetManager<RealTimeServerManager>().NetTickrate;
        }

        public void UpdateActors(GameSimulation sim, float deltaTime)
        {
            ActorFilter filter = sim.Filter;
            var cullAreas = filter.GetActors<CullArea, Position>();
            ActorList<NetInfo, Position> syncable = default;
            if ( cullAreas.Count > 0 )
            {
                syncable = filter.GetActors<NetInfo, Position>();
            }
            List<int> inRange = new List<int>();

            for(int i = 0; i < cullAreas.Count; ++i)
            {
                ref CullArea cull = ref cullAreas.GetC1(i);
                ref Position cullPos = ref cullAreas.GetC2(i);
                inRange.Clear();

                PlayerProxy player = m_Players.FindPlayer(cull.Client);
                if (player != null)
                {
                    for(int j = 0; j < syncable.Count; ++j)
                    {
                        ref NetInfo syncInfo = ref syncable.GetC1(j);
                        ref Position syncPos = ref syncable.GetC2(j);
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
                                SendActorState(player.Client, syncable.GetActor(j));
                            }
                        }
                    }

                    foreach(int old in cull.ProximityActors)
                    {
                        if (!inRange.Contains(old))
                        {
                            player.Client.Send(MudMessage.Create((int)NetworkOperation.ActorRemove, new byte[] { (byte)old }), true);
                        }
                    }

                    cull.ProximityActors.Clear();
                    cull.ProximityActors.AddRange(inRange);
                }
            }

            for(int i = 0; cullAreas.Count > 0 && i < syncable.Count; ++i)
            {
                ref NetInfo netinfo = ref syncable.GetC1(i);

                if ( netinfo.LastOutBuffer != null )
                {
                    netinfo.LastOutBuffer = null;
                    netinfo.SyncClock = m_NetTickDeltaTime;
                }
                else if ( netinfo.SyncClock > 0f )
                {
                    netinfo.SyncClock -= deltaTime;
                }
            }
        }

        private void SendActorState(GameClient client, GameActor actor)
        {
            byte[] serializedData;
            SimulationPool compPool = m_Simulation.Builder.Components;
            m_ComponentBuffer.Clear();

            using (MemoryStream ms = new MemoryStream())
            {
                for(int i = 0; i < actor.Components.Length; ++i)
                {
                    if ( actor.Components[i] != -1 && !m_SkippedTypes.Contains(actor.ComponentTypes[i]))
                    {
                        GenericArray compArray = compPool.GetPool(actor.ComponentTypes[i]);
                        m_ComponentBuffer.Add((IComponent)compArray.Get(actor.Components[i]));
                    }
                }

                ActorState actorState = new ActorState()
                {
                    Components = m_ComponentBuffer.ToArray()
                };

                m_Serializer.Serialize(ms, actorState);
                serializedData = ms.ToArray();
            }

            client.Send(MudMessage.Create((int)NetworkOperation.ActorState, serializedData), true);
        }
    }
}
