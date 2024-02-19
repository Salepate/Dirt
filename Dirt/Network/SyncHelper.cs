using Dirt.Log;
using Dirt.Network.Internal;
using Dirt.Network.Managers;
using Dirt.Network.Model;
using Dirt.Network.Simulation;
using Dirt.Network.Simulation.Components;
using Dirt.Simulation;
using Dirt.Simulation.Actor;
using Dirt.Simulation.Builder;
using Dirt.Simulation.Exceptions;
using System.Collections.Generic;
using System.Reflection;


namespace Dirt.Network
{
    public static class SyncHelper
    {
        public const string OwnerAuthority = "Owner";

        /// <summary>
        /// 
        /// </summary>
        /// <param name="actor"></param>
        /// <param name="syncDesc"></param>
        /// <param name="owner">-1 is server, client id otherwise</param>
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

        private static bool CanWrite(string comp, string fieldName, SyncInfo sync)
        {
            if (sync.Fields.TryGetValue(comp, out SyncInfo.FieldSettings field))
            {
                if (field.TryGetValue(fieldName, out string ownership))
                {
                    return ownership == OwnerAuthority;
                }
            }
            return false;
        }
    }
}
