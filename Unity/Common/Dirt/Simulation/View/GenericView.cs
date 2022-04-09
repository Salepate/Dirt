using Dirt.Simulation.Actor;
using Dirt.Simulation.Components;
using UnityEngine;

namespace Dirt.Simulation.View
{
    public class GenericView : MonoBehaviour, ISimulationView
    {
        [Header(":: Settings")]
        public float DestroyClock = 0f;
        public bool Static;

        private float m_DecayClock;
        protected bool Destroyed { get; private set; }
        bool ISimulationView.Destroy => Destroyed && DestroyClock <= 0f;

        public int ActorID => m_Actor.ID;

        protected ref Position ActorPosition => ref m_Filter.Get<Position>(m_Actor);
        private ActorFilter m_Filter;
        private GameActor m_Actor;
        public virtual bool NotifyActorDestroyed()
        {
            Destroyed = true;
            m_DecayClock = DestroyClock;
            return DestroyClock <= 0f;
        }

        public virtual void SetActor(GameActor actor, ActorFilter filter)
        {
            m_Actor = actor;
            m_Filter = filter;
            Destroyed = false;
            transform.position = ActorPosition.Origin.toVector();
        }

        public virtual void UpdateView(float deltaTime)
        {
            if ( !Static && !Destroyed )
            {
                transform.position = ActorPosition.Origin.toVector();
            }
            if (Destroyed && m_DecayClock > 0f)
            {
                m_DecayClock -= deltaTime;
            }
        }
    }
}