using Dirt.Game;
using Dirt.Game.Content;
using Dirt.Simulation.Context;
using System.Collections.Generic;

namespace Dirt.Simulation.Action
{
    public enum ReplicationType
    {
        All,    // every player
        Others, // every other player (useful for self simulated action)
        Self, // self only
    }

    public class ActorAction
    {
        //=====================================================================
        //= Server Only
        //=====================================================================

        public ReplicationType Replication = ReplicationType.All;
        public virtual bool ValidateAction(GameSimulation simulation, SimulationContext simContext, in ActionExecutionData data) => true;

        //=====================================================================
        //= Server/Client
        //=====================================================================
        public int Index { get; private set; }
        public virtual void Initialize(GameSimulation simulation, SimulationContext simContext, IManagerProvider managers, IContentProvider contentProvider) { }
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
