using Dirt.Log;
using Dirt.Network.Internal;
using Dirt.Network.Managers;
using Dirt.Network.Model;
using Dirt.Network.Simulation.Components;
using Dirt.Simulation;
using Dirt.Simulation.Exceptions;
using System.Collections.Generic;


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
        public static void SyncActor(GameActor actor, ref NetInfo netInfo, SyncInfo syncDesc, int owner)
        {
            if (actor.GetComponentIndex<NetInfo>() == -1)
            {
                throw new ComponentNotFoundException(typeof(NetInfo));
            }

            List<ComponentFieldInfo> fields = new List<ComponentFieldInfo>();
            netInfo.Owner = owner;

            //Console.Message($"Sync actor {actor.ID}");

            for (int i = 0; i < syncDesc.SyncedComponents.Length; ++i)
            {
                string syncComp = syncDesc.SyncedComponents[i];
                System.Type compType = System.Type.GetType(syncComp);
                int compIndex = actor.GetComponentLocalIndex(compType);

                if (NetworkSerializer.TryGetSetters(compType, out ObjectFieldAccessor[] accessors))
                {
                    for (int j = 0; j < accessors.Length; ++j)
                    {
                        ComponentFieldInfo fieldSync = new ComponentFieldInfo()
                        {
                            Component = compIndex,
                            Accessor = j,
                            Owner = CanWrite(compType.Name, accessors[j].Name, syncDesc),
                            Debug = $"{compType.Name}.{accessors[j].Name}"
                        };
                        //Console.Message($"Component Sync {compType.Name}: [Owner:{fieldSync.Owner}] [Accessor:{accessors[j].Name}]");
                        fields.Add(fieldSync);
                    }
                }
                else
                {
                    Console.Message($"Unable to sync component {compType.Name}: Component not registered");
                }
            }
            netInfo.Fields = fields;
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
