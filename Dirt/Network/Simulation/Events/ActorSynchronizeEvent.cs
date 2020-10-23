using Dirt.Simulation;

namespace Dirt.Network.Simulation.Events
{
    public class ActorSyncEvent : SimulationEvent
    {
        public int NetID { get; private set; }
        public byte[] Buffer { get; private set; }
        public ActorSyncEvent(int netID, byte[] syncBuffer)
        {
            NetID = netID;
            Buffer = syncBuffer;
        }

    }
}
