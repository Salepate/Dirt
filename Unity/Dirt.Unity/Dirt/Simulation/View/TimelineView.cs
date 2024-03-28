using Dirt.Simulation.Actor;
using UnityEngine;
using UnityEngine.Playables;

namespace Dirt.Simulation.View
{
    /// <summary>
    /// A View mapped to a PlayableDirector
    /// </summary>
    public class TimelineView : GenericView, ISimulationView
    {
        [Header(":: References")]
        public PlayableDirector Director;
        [Header(":: Settings")]
        public float Duration = 1f;
        private float m_Clock;

        bool ISimulationView.Destroy => m_Clock <= 0f;

        public override bool NotifyActorDestroyed(int reason)
        {
            return false;
        }

        public override void SetActor(GameActor actor, ActorFilter filter)
        {
            base.SetActor(actor, filter);
            m_Clock = Duration;
            Director.Play();
        }

        public override void UpdateView(float deltaTime)
        {
            base.UpdateView(deltaTime);
            m_Clock -= deltaTime;
        }
    }
}