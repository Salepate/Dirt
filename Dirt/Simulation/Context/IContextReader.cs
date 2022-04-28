namespace Dirt.Simulation.Context
{
    /// <summary>
    /// Allow context access
    /// Works on ISimulationSystem, ISimulationView
    /// </summary>
    public interface IContextReader
    {
        void SetContext(SimulationContext context);
    }
}
