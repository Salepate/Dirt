using Dirt.Network.Simulation.Components;
using Dirt.Network.Systems;

namespace Dirt.GameServer.Simulation.Systems
{
    public class ServerActorStreaming : ActorStreaming
    {
        protected override bool ShouldSerializeActor(ref NetInfo info)
        {
            return info.SyncClock <= 0f && base.ShouldSerializeActor(ref info);
        }
    }
}
