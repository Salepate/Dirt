using Dirt.Network.Simulation.Components;
using Dirt.Simulation;
using Dirt.Simulation.Actor;
using Dirt.Simulation.SystemHelper;

namespace Dirt.Network.Simulations.Systems
{
    public class TimedServerControl : ISimulationSystem
    {
        public void Initialize(GameSimulation sim)
        {
        }

        public void UpdateActors(GameSimulation sim, float deltaTime)
        {
            ActorList<NetInfo> netActors = sim.Filter.GetActors<NetInfo>();
            for(int i = 0; i < netActors.Count; ++i)
            {
                ref NetInfo info = ref netActors.GetC1(i);
                if (info.ServerControlTime > 0)
                {
                    info.ServerControlTime -= 1;

                    if ( info.ServerControlTime <= 0 )
                    {
                        info.ServerControl = false;
                    }
                }
            }
        }
    }
}