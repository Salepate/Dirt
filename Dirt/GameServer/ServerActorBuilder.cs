using Dirt.Network;
using Dirt.Network.Model;
using Dirt.Simulation;
using Dirt.Simulation.Builder;

namespace Dirt.GameServer
{
    internal class ServerActorBuilder : ActorBuilder
    {
        public override GameActor BuildActor(string archetype)
        {
            GameActor builtActor = base.BuildActor(archetype);
            string syncContent = $"sync.{archetype}";
            if ( Content.HasContent(syncContent)) {
                SyncInfo syncInfo = Content.LoadContent<SyncInfo>($"sync.{archetype}");
                SyncHelper.SyncActor(builtActor, syncInfo);
            }
            return builtActor;
        }
    }
}
