using Dirt.Game.Content;
using Dirt.Simulation.Context;
using System.Collections.Generic;

namespace Dirt.Simulation.Action
{
    public class ActorAction
    {
        public int Index { get; private set; }

        public virtual void Initialize(GameSimulation simulation, SimulationContext simContext, IContentProvider contentProvider) { }
        /// <summary>
        /// lookup data from context and simulation before performing action
        /// </summary>
        /// <param name="simulation"></param>
        /// <param name="simContext"></param>
        /// <param name="sourceActor"></param>
        /// <param name="parameters"></param>
        public virtual void FetchGameData(GameSimulation simulation, SimulationContext simContext, GameActor sourceActor, List<ActionParameter> parameters) { }
        public virtual void PerformAction(GameSimulation simulation, SimulationContext simContext, in ActionExecutionData data) { }
        public void SetIndex(int idx)
        {
            Index = idx;
        }
    }
}
