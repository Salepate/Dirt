namespace Dirt.Network.Simulation
{
    /// <summary>
    /// Manually serialize and deserialize a component for network sync.
    /// </summary>
    public interface INetComponent
    {
        /// <summary>
        /// Called when the component is getting serialized.
        /// </summary>
        /// <param name="destination"></param>
        /// <param name="offset">current offset</param>
        void Serialize(NetworkStream stream);
        void Deserialize(NetworkStream stream);
    }
}
