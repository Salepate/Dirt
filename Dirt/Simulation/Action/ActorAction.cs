using Dirt.Simulation.Context;
using System.Collections.Generic;

namespace Dirt.Simulation.Action
{
    public class ActorAction
    {
        public virtual void FetchParameters(GameSimulation simulation, SimulationContext simContext, GameActor sourceActor, List<ActionParameter> parameters) { }
        public virtual void PerformAction(GameSimulation simulation, SimulationContext simContext, in ActionExecutionData data) { }
    }
}
