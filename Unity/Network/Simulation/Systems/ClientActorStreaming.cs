using Dirt.Network.Simulation.Components;
using Dirt.Network.Systems;

namespace Dirt.Network.Simulations.Systems
{
    public class ClientActorStreaming : ActorStreaming
    {
        protected override bool ShouldSerializeField(bool isOwner)
        {
            return isOwner;
        }

        protected override bool ShouldSerializeActor(ref NetInfo info)
        {
            return info.Owned;
        }

        protected override bool ShouldDeserialize(bool serverAuthor, bool isOwner)
        {
            return serverAuthor || !isOwner;
        }

        /// <summary>
        /// Client always deserialize, partial state sync supports dual ownership on entities.
        /// </summary>
        /// <param name="info"></param>
        /// <returns></returns>
        protected override bool ShouldDeserialize(ref NetInfo info) => true;
    }
}