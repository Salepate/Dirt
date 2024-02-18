using Dirt.Log;
using Dirt.Network;
using Dirt.Network.Model;
using Dirt.Network.Simulation.Components;
using Dirt.Simulation;
using Dirt.Simulation.Builder;
using System.Collections.Generic;

namespace Dirt.GameServer
{
    public class ServerActorBuilder : ActorBuilder
    {
        private Stack<byte> m_AvailableIDs;

        public override void InitializePool(int poolSize)
        {
            base.InitializePool(poolSize);

            m_AvailableIDs = new Stack<byte>(NetInfo.MaxID);
            for(byte i = NetInfo.MaxID; i >= 0;) 
            {
                m_AvailableIDs.Push(i);
                if (i == 0)
                    break;
                i--;
            }
        }

        public override GameActor BuildActor(string archetype) => BuildRemoteActor(archetype, -1);

        public override void DestroyActor(GameActor actor)
        {
            int netIdx = actor.GetComponentIndex<NetInfo>();
            if (netIdx != -1)
            {
                ref NetInfo netInfo = ref Components.GetPool<NetInfo>().Components[netIdx];
                m_AvailableIDs.Push((byte)netInfo.ID);
            }
            base.DestroyActor(actor);
        }

        public GameActor BuildRemoteActor(string archetype, int owner)
        {
            if (m_AvailableIDs.Count == 0)
            {
                Console.Error($"Cannot add more net entity, max limit reached: {NetInfo.MaxID}");
                return null;
            }

            GameActor builtActor = base.BuildActor(archetype);
            string syncContent = $"sync.{archetype}";
            if (Content.HasContent(syncContent))
            {

                SyncInfo syncInfo = Content.LoadContent<SyncInfo>($"sync.{archetype}");
                int netInfoIdx = builtActor.GetComponentIndex<NetInfo>();
                if (netInfoIdx == -1)
                {
                    ref NetInfo netInfo = ref AddComponent<NetInfo>(builtActor);
                    this.GenerateActorSyncData(builtActor, ref netInfo, syncInfo, owner);
                    netInfo.ID = m_AvailableIDs.Pop();
                }
                else
                {
                    ref NetInfo netInfo = ref Components.GetPool<NetInfo>().Components[netInfoIdx];
                    this.GenerateActorSyncData(builtActor, ref netInfo, syncInfo, owner);
                    netInfo.ID = m_AvailableIDs.Pop();
                }

            }
            return builtActor;
        }
    }
}
