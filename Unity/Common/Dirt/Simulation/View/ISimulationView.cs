using Dirt.Simulation.Actor;

namespace Dirt.Simulation.View
{
    public interface ISimulationView
    {
        /// <summary>
        /// Overrides to notify when to remove
        /// </summary>
        bool Destroy { get; }
        /// <summary>
        /// Used in conjuction with SimulationViewDispatcher.SpawnCustomView
        /// return true if you want to handle prefab instantiation yourself
        /// </summary>
        bool UseCustomLoader { get; }
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
        bool NotifyActorDestroyed(int reason);

        /// <summary>
        /// Called when view is cleaned (will be invoked after actor is destroyed)
        /// </summary>
        /// <returns>true if view is to be removed immediately</returns>
        void CleanView();
    }
}