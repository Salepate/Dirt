namespace Dirt.Simulation.Events
{
    public class LocalSimulationEvent : SimulationEvent
    {
        public const int SimulationLoaded = 0;
        public const int SimulationDestroyed = 1;
        public LocalSimulationEvent(int eventType) : base()
        {
            Event = eventType;
        }
    }
}
