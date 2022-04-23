namespace Dirt.Simulation.Events
{
    public class LocalSimulationEvent : SimulationEvent
    {
        public const int SimulationLoaded = 0;
        public const int SimulationDestroyed = 1;

        public string Archetype { get; private set; }
        public LocalSimulationEvent(string archetype, int eventType) : base()
        {
            Archetype = archetype;
            Event = eventType;
        }
    }
}
