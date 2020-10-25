using Dirt.Simulation;

namespace Dirt.Network.Simulation.Events
{
    public class ActorNetCullEvent : SimulationEvent
    {
        public int NetID { get; private set; }
        public ActorNetCullEvent(int netID)
        {
            NetID = netID;
        }
    }
}
