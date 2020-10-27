using Dirt.Log;
using Dirt.Network.Internal;
using Dirt.Network.Managers;
using Dirt.Network.Model;
using Dirt.Network.Simulation.Components;
using Dirt.Simulation;
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
        public static void SyncActor(GameActor actor, SyncInfo syncDesc, int owner)
        {
            if (actor.GetComponentIndex<NetInfo>() == -1)
                actor.AddComponent(new NetInfo());

            NetInfo sync = actor.GetComponent<NetInfo>();
            List<ComponentFieldInfo> fields = new List<ComponentFieldInfo>();
            sync.Owner = owner;

            for (int i = 0; i < syncDesc.SyncedComponents.Length; ++i)
            {
                string syncComp = syncDesc.SyncedComponents[i];
                System.Type compType = System.Type.GetType(syncComp);
                int compIndex = GetComponentIndex(actor, compType);

                if (NetworkSerializer.TryGetSetters(compType, out ObjectFieldAccessor[] accessors))
                {
                    for (int j = 0; j < accessors.Length; ++j)
                    {
                        ComponentFieldInfo fieldSync = new ComponentFieldInfo()
                        {
                            Component = compIndex,
                            Accessor = j,
                            Owner = CanWrite(compType.Name, accessors[j].Name, syncDesc)
                        };

                        fields.Add(fieldSync);
                    }
                }
                else
                {
                    Console.Message($"Unable to sync component {compType.Name}: Component not registered");
                }
            }
            sync.Fields = fields;
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

        private static int GetComponentIndex(GameActor actor, System.Type compType)
        {
            if (compType != null)
            {
                for (int i = 0; i < actor.ComponentCount; ++i)
                {
                    if (actor.ComponentTypes[i] == compType)
                    {
                        return i;
                    }
                }
            }
            return -1;
        }
    }
}
