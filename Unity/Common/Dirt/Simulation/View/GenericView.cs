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
        public bool UseCustomLoader => false;
        bool ISimulationView.Destroy => Destroyed && DestroyClock <= 0f;

        public int ActorID => Actor.ID;

        protected ref Position ActorPosition => ref m_Filter.Get<Position>(Actor);
        private ActorFilter m_Filter;
        protected GameActor Actor { get; private set; }
        public virtual bool NotifyActorDestroyed()
        {
            Destroyed = true;
            m_DecayClock = DestroyClock;
            return DestroyClock <= 0f;
        }

        public virtual void SetActor(GameActor actor, ActorFilter filter)
        {
            Actor = actor;
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

        public virtual void CleanView() {}
    }
}