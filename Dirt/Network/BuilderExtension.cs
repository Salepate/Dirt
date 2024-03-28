using Dirt.Network.Managers;
using Dirt.Network.Model;
using Dirt.Network.Simulation;
using Dirt.Network.Simulation.Components;
using Dirt.Simulation;
using Dirt.Simulation.Actor;
using Dirt.Simulation.Builder;
using Dirt.Simulation.Exceptions;
using System.Collections.Generic;


namespace Dirt.Network
{
    using Dirt.Log;
    public static class BuilderExtension
    {
        public const string OwnerAuthority = "Owner";

        public static void GenerateActorSyncData(this ActorBuilder builder, GameActor actor, ref NetInfo netInfo, SyncInfo syncInfo, int owner)
        {
            if (actor.GetComponentIndex<NetInfo>() == -1)
            {
                throw new ComponentNotFoundException(typeof(NetInfo));
            }

            List<ComponentSerializer> serializers = new List<ComponentSerializer>();
            netInfo.Owner = owner;

            for (int i = 0; i < syncInfo.SyncedComponents.Length; ++i)
            {
                System.Type compType = System.Type.GetType(syncInfo.SyncedComponents[i]);
                GenericArray compPool = builder.Components.GetPool(compType);
                int compIndex = actor.GetComponentLocalIndex(compType);

                if (NetworkSerializer.TryGetSerializer(compType, out ComponentSerializer serializer))
                {
                    serializer.PoolIndex = compPool.Index;
                    serializer.ComponentIndex = compIndex;
                    serializer.AuthoredByOwner = syncInfo.OwnerAuthority.ContainsKey(compType.Name);
                    serializers.Add(serializer);
                }
                else
                {
                    Console.Message($"Unable to sync component {compType.Name}: Component not registered");
                }
            }
            netInfo.Serializers = serializers.ToArray();
        }

        public static void SetComponentPoolIndex(this ActorBuilder builder, ref NetInfo netInfo)
        {
            Console.Assert(netInfo.Serializers.Length == netInfo.Synced.Length, "Size mismatch");
            for(int i = 0; i < netInfo.Synced.Length; ++i)
            {
                System.Type compType = System.Type.GetType(netInfo.Synced[i]);
                GenericArray compPool = builder.Components.GetPool(compType);
                ref ComponentSerializer serializer = ref netInfo.Serializers[i];
                serializer.PoolIndex = compPool.Index;
            }
        }
    }
}
