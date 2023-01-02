using Dirt.Simulation;

namespace Dirt.Network.Simulation.Events
{
    public class RemoteActionRequestEvent : SimulationEvent
    {
        public int SourceActor { get; private set; }
        public int ActionIndex { get; private set; }
        public RemoteActionRequestEvent(int actorID, int idx)
        {
            SourceActor = actorID;
            ActionIndex = idx;
        }
    }
}
