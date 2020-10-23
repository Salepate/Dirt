using Dirt.Simulation;

namespace Dirt.GameServer
{
    public class SimulationProxy
    {
        public GameSimulation Simulation { get; private set; }
        public SimulationSpan Span { get; private set; }
        public SystemContainer Systems { get; private set; }


        public SimulationProxy(GameSimulation simulation, SimulationSpan span)
        {
            Simulation = simulation;
            Span = span;
        }

        public void AttachSystems(SystemContainer systems)
        {
            Systems = systems;
        }
    }
}
