using Dirt.Simulation.Actor;
using Dirt.Simulation.Components;
using Dirt.Simulation.SystemHelper;
using System.Collections.Generic;

namespace Dirt.Simulation.Systems
{
    public class ActorTracker : ISimulationSystem
    {
        public void Initialize(GameSimulation sim)
        {
        }

        public void UpdateActors(GameSimulation sim, float deltaTime)
        {
            var trackers = sim.Filter.GetAll<Position, Tracker>();

            foreach(ActorTuple<Position, Tracker> tracker in trackers)
            {
                ref Tracker trackerData = ref tracker.GetC2();
                ref Position trackerPos = ref tracker.GetC1();
                if (trackerData.TargetActor != -1)
                {
                    if ( sim.Filter.TryGetActor(trackerData.TargetActor, out GameActor targetActor))
                    {
                        Position pos = sim.Filter.Get<Position>(targetActor);
                        trackerPos.Origin = pos.Origin;
                    }
                    else
                    {
                        trackerData.TargetActor = -1;
                    }
                }
            }
        }
    }
}
