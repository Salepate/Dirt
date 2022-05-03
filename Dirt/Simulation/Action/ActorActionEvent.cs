namespace Dirt.Simulation.Action
{
    public class ActorActionEvent : SimulationEvent
    {
        public int SourceActor { get; private set; }
        public int ActionIndex { get; private set; }

        public ActorActionEvent(int actorID, int idx)
        {
            SourceActor = actorID;
            ActionIndex = idx;
        }
    }
}
