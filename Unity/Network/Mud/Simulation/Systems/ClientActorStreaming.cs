using Dirt.Network.Systems;

namespace Dirt.Network.Simulations.Systems
{
    public class ClientActorStreaming : ActorStreaming
    {
        protected override bool ShouldSerialize(bool isOwner)
        {
            return isOwner;
        }

        protected override bool ShouldDeserialize(bool isOwner)
        {
            return !isOwner;
        }
    }
}