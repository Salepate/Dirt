using Dirt.Simulation.Actor;

namespace Dirt.Simulation.View
{
    public interface ISimulationView
    {
        /// <summary>
        /// Overrides to notify when to remove
        /// </summary>
        bool Destroy { get; }
        int ActorID { get; }
        void SetActor(GameActor actor, ActorFilter filter);
        /// <summary>
        /// Main routine to adjust view
        /// </summary>
        /// <param name="deltaTime">elapsed time since last call (in ms)</param>
        void UpdateView(float deltaTime);
        /// <summary>
        /// Called when Actor is destroyed
        /// </summary>
        /// <returns>true if view is to be removed immediately</returns>
        bool NotifyActorDestroyed();
    }
}