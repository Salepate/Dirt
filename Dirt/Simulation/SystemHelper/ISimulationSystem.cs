using Dirt.Simulation.Actor;
using System.Collections.Generic;

namespace Dirt.Simulation.SystemHelper
{
    public interface ISimulationSystem
    {
        void Initialize(GameSimulation sim);
        void UpdateActors(GameSimulation sim, float deltaTime);
    }
}
