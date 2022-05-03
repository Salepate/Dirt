namespace Dirt.Simulation.Action
{
    public class ActorAction
    {
        public struct ExecutionContext
        {
            public GameActor SourceActor;
        }
        public virtual void PerformAction(GameSimulation simulation, in ExecutionContext ctx) { }
    }
}
