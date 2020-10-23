namespace Dirt.Simulation.Events
{
    public class ActorEvent : SimulationEvent
    {
        public const int Created = 0;
        public const int Removed = 1;
        public GameActor Actor { get; private set; }
        public ActorEvent(GameActor actor, int actorEvent)
        {
            Actor = actor;
            Event = actorEvent;
        }
    }
}
