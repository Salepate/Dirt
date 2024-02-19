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
        /// <param name="stream">stream to read and write data</param>
        /// <param name="client">which side the serialization is happening on</param>
        void Serialize(NetworkStream stream, bool client);
        void Deserialize(NetworkStream stream, bool client, bool authority);
    }
}
