using System.Collections.Generic;

namespace Dirt.Simulation.SystemHelper
{
    public interface ISimulationSystem
    {
        void Initialize(GameSimulation sim);
        void UpdateActors(List<GameActor> actors, float deltaTime);
    }
}
