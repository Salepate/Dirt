using Dirt.Network;
using Dirt.Network.Model;
using Dirt.Simulation;
using Dirt.Simulation.Builder;

namespace Dirt.GameServer
{
    public class ServerActorBuilder : ActorBuilder
    {
        public override GameActor BuildActor(string archetype)
        {
            GameActor builtActor = base.BuildActor(archetype);
            string syncContent = $"sync.{archetype}";
            if ( Content.HasContent(syncContent)) {
                SyncInfo syncInfo = Content.LoadContent<SyncInfo>($"sync.{archetype}");
                SyncHelper.SyncActor(builtActor, syncInfo, -1);
            }
            return builtActor;
        }

        public GameActor BuildRemoteActor(string archetype, int owner)
        {
            GameActor builtActor = base.BuildActor(archetype);
            string syncContent = $"sync.{archetype}";
            if (Content.HasContent(syncContent))
            {
                SyncInfo syncInfo = Content.LoadContent<SyncInfo>($"sync.{archetype}");
                SyncHelper.SyncActor(builtActor, syncInfo, owner);
            }
            return builtActor;
        }
    }
}
