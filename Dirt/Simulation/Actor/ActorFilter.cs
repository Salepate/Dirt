using Dirt.Simulation.Builder;
using Dirt.Simulation.Exceptions;
using System;
using System.Collections.Generic;

namespace Dirt.Simulation.Actor
{
    public class ActorFilter
    {
        public GameSimulation Simulation { get; private set; }
        public List<GameActor> Actors { get;  private set; }
        private SimulationPool m_Components;
        public ActorFilter(GameSimulation sim)
        {
            Simulation = sim;
            m_Components = sim.Builder.Components;
            Actors = sim.World.Actors;
        }

        public delegate bool ActorMatch<T>(T data) where T : IComponent;

        public ref C Get<C>(GameActor actor) where C: new()
        {
            int compIdx = actor.GetComponentIndex<C>();
            if (compIdx != -1)
            {
                ComponentArray<C> pool = m_Components.GetPool<C>();
                return ref pool.Components[compIdx];
            }

            throw new ComponentNotFoundException(typeof(C));
        }


        public bool TryGetActor(int actorID, out GameActor actor)
        {
            actor = null;
            for (int i = 0; i < Actors.Count; ++i)
            {
                if (Actors[i].ID == actorID)
                {
                    actor = Actors[i];
                    return true;
                }
            }
            return false;
        }

        public List<ActorTuple<C1>> GetAll<C1>() where C1 : new()
        {
            List<ActorTuple<C1>> res = new List<ActorTuple<C1>>();
            ComponentArray<C1> pool = m_Components.GetPool<C1>();
            for (int i = 0; i < pool.Actors.Length; ++i)
            {
                int actorIndex = pool.Actors[i];
                if (actorIndex != -1)
                {
                    ActorTuple<C1> actorTuple = new ActorTuple<C1>(Actors[actorIndex]);
                    actorTuple.SetC1(pool, i);
                    res.Add(actorTuple);
                }
            }

            return res;
        }

        public List<ActorTuple<C1, C2>> GetAll<C1, C2>() 
            where C1 : new()
            where C2 : new()
        {
            List<ActorTuple<C1, C2>> res = new List<ActorTuple<C1, C2>>();
            ComponentArray<C1> pool = m_Components.GetPool<C1>();
            for (int i = 0; i < pool.Actors.Length; ++i)
            {
                int actorIndex = pool.Actors[i];
                if (actorIndex != -1)
                {
                    GameActor actor = Actors[actorIndex];
                    int c2Idx = actor.GetComponentIndex<C2>();
                    if (c2Idx != -1)
                    {
                        var actorTuple = new ActorTuple<C1, C2>(actor);
                        actorTuple.SetC1(pool, i);
                        actorTuple.SetC2(m_Components.GetPool<C2>(), c2Idx);
                        res.Add(actorTuple);
                    }
                }
            }
            return res;
        }

    }
}