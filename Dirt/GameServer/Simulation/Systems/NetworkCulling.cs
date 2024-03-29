﻿using Dirt.Game;
using Dirt.Game.Content;
using Dirt.GameServer;
using Dirt.GameServer.Managers;
using Dirt.GameServer.Simulation.Components;
using Dirt.Network.Managers;
using Dirt.Network.Simulation;
using Dirt.Network.Simulation.Components;
using Dirt.Network.Systems;
using Dirt.Simulation;
using Dirt.Simulation.Actor;
using Dirt.Simulation.Actor.Components;
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
        private ActorStream m_Stream;
        private List<int> m_NetIDs;
        private List<int> m_LocalIDS;
        private byte[] m_DestroyTable;
        private int m_Frame;
        public void Initialize(GameSimulation sim)
        {
            m_Simulation = sim;
            m_ComponentBuffer = new List<IComponent>(10);
            m_NetIDs = new List<int>(50);
            m_LocalIDS = new List<int>(50);
            m_Stream = new ActorStream();
            m_Stream.Initialize(sim, false);

            m_DestroyTable = new byte[sim.Builder.ActorPoolSize];

            for (int i = 0; i < m_DestroyTable.Length; ++i)
            {
                m_DestroyTable[i] = ActorStreaming.Destroyed;
            }

            sim.Builder.ActorCreateAction += OnActorCreated;
            sim.Builder.ActorDestroyAction += OnActorDestroyed;
            m_Frame = 0;
        }

        private void OnActorCreated(GameActor actor)
        {
            m_DestroyTable[actor.InternalID] = ActorStreaming.Destroyed;
        }

        private void OnActorDestroyed(GameActor actor)
        {
            Destroy destroy = m_Simulation.Filter.Get<Destroy>(actor);
            if (destroy.Reason > 0 && destroy.Reason < ActorStreaming.Culled)
            {
                m_DestroyTable[actor.InternalID] = (byte)destroy.Reason;
            }
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
            ++m_Frame;
            ActorFilter filter = sim.Filter;
            var cullAreas = filter.GetActors<CullArea, Position>();
            ActorList<NetInfo, Position> syncable = default;
            if ( cullAreas.Count > 0 )
            {
                syncable = filter.GetActors<NetInfo, Position>();
            }

            for(int i = 0; i < cullAreas.Count; ++i)
            {
                ref CullArea cull = ref cullAreas.GetC1(i);
                ref Position cullPos = ref cullAreas.GetC2(i);

                m_NetIDs.Clear();
                m_LocalIDS.Clear();

                PlayerProxy player = m_Players.FindPlayer(cull.Client);
                if (player != null)
                {
                    for(int j = 0; j < syncable.Count; ++j)
                    {
                        ref NetInfo syncInfo = ref syncable.GetC1(j);
                        ref Position syncPos = ref syncable.GetC2(j);
                        if (syncInfo.ID == -1)
                            continue;

                        float sqrRad = cull.Radius * cull.Radius;
                        float sqrRadOut = (cull.Radius + cull.Threshold) * (cull.Radius + cull.Threshold);
                        float sqrMag = (cullPos.Origin - syncPos.Origin).sqrMagnitude;
                        bool isOld =  cull.ProximityActors.Contains(syncInfo.ID);

                        if (sqrMag <= sqrRad || isOld && sqrMag < sqrRadOut)
                        {
                            m_NetIDs.Add(syncInfo.ID);
                            m_LocalIDS.Add(syncable.GetActor(j).ID);

                            if (isOld)
                            {
                                m_Stream.SerializeActor(syncable.GetActor(j), ref syncInfo, m_Frame);
                                if (syncInfo.LastOutStamp == m_Frame)
                                {
                                    player.Client.SendRaw(syncInfo.LastOutBuffer, syncInfo.BufferSize);
                                }
                            }
                            else
                            {
                                SendActorState(player.Client, syncable.GetActor(j));
                            }
                        }
                    }

                    for(int j = 0; j < cull.ProximityActors.Count; ++j)
                    {
                        byte netID = (byte) cull.ProximityActors[j];
                        if (!m_NetIDs.Contains(netID))
                        {
                            byte reason = ActorStreaming.Culled;
                            if ( !filter.TryGetActor(cull.LocalIDs[j], out _))
                            {
                                reason = m_DestroyTable[GameActor.GetInternalID(cull.LocalIDs[j])];
                            }
                            player.Client.Send(MudMessage.Create((int)NetworkOperation.ActorRemove, new byte[] {netID, reason}), true);
                        }
                    }

                    cull.ProximityActors.Clear();
                    cull.ProximityActors.AddRange(m_NetIDs);
                    cull.LocalIDs.Clear();
                    cull.LocalIDs.AddRange(m_LocalIDS);
                }
            }

            for(int i = 0; cullAreas.Count > 0 && i < syncable.Count; ++i)
            {
                ref NetInfo netinfo = ref syncable.GetC1(i);
                if (netinfo.LastOutStamp == m_Frame)
                {
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
