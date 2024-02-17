using Dirt.Log;
using Dirt.Network;
using Dirt.Network.Model;
using Dirt.Network.Simulation.Components;
using Dirt.Simulation;
using Dirt.Simulation.Builder;

namespace Dirt.GameServer
{
    public class ServerActorBuilder : ActorBuilder
    {
        public override GameActor BuildActor(string archetype) => BuildRemoteActor(archetype, -1);

        public GameActor BuildRemoteActor(string archetype, int owner)
        {
            GameActor builtActor = base.BuildActor(archetype);
            string syncContent = $"sync.{archetype}";
            if (Content.HasContent(syncContent))
            {

                SyncInfo syncInfo = Content.LoadContent<SyncInfo>($"sync.{archetype}");
                int netInfoIdx = builtActor.GetComponentIndex<NetInfo>();
                if (netInfoIdx == -1)
                {
                    ref NetInfo netInfo = ref AddComponent<NetInfo>(builtActor);
                    SyncHelper.GenerateActorSyncData(builtActor, ref netInfo, syncInfo, owner);
                }
                else
                {
                    ref NetInfo netInfo = ref Components.GetPool<NetInfo>().Components[netInfoIdx];
                    SyncHelper.GenerateActorSyncData(builtActor, ref netInfo, syncInfo, owner);
                }

            }
            return builtActor;
        }
    }
}
