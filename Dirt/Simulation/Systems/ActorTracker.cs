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
            ActorList<Position, Tracker> trackers = sim.Filter.GetActors<Position, Tracker>();

            for(int i = 0; i < trackers.Count; ++i)
            {
                ref Tracker trackerData = ref trackers.GetC2(i);
                ref Position trackerPos = ref trackers.GetC1(i);
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
