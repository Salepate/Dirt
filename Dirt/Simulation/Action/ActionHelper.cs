namespace Dirt.Simulation.Action
{
    public static partial class ActionHelper
    {
        public static void RequestAction(this GameSimulation simulation, GameActor sourceActor, int actionIndex)
        {
            simulation.Events.Enqueue(new ActorActionEvent(sourceActor.ID, actionIndex));
        }
    }
}