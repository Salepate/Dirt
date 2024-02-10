using Dirt.Simulation;

namespace Dirt.Network.Simulation.Events
{
    public class ActorNetCullEvent : SimulationEvent
    {
        public const int Culled      = 255;
        public const int Destroyed   = 0;
        public int NetID { get; private set; }
        public int Reason { get; private set; }
        public ActorNetCullEvent(int netID, int reason)
        {
            NetID = netID;
            Reason = reason;
        }
    }
}
