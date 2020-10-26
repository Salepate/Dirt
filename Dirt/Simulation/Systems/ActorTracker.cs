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

        public void UpdateActors(List<GameActor> actors, float deltaTime)
        {
            var trackers = actors.GetActors<Position, Tracker>();
            var targets = actors.GetActors<Position>();

            trackers.ForEach(tracker =>
            {
                if (tracker.Item3.TargetActor != -1)
                {
                    var target = targets.Find(t => t.Item1.ID == tracker.Item3.TargetActor);
                    if (target == null)
                    {
                        tracker.Item3.TargetActor = -1;
                    }
                    else
                    {
                        tracker.Item2.Origin = target.Item2.Origin;
                    }
                }
            });
        }
    }
}
