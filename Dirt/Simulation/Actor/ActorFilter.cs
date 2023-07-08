using Dirt.Simulation.Builder;
using Dirt.Simulation.Exceptions;
using System;
using System.Collections.Generic;

namespace Dirt.Simulation.Actor
{
    public class ActorFilter
    {
        public List<GameActor> Actors { get;  private set; }
        private SimulationPool m_Components;
        public ActorFilter(SimulationPool componentPool, List<GameActor> activeActors)
        {
            m_Components = componentPool;
            Actors = activeActors;
        }

        public delegate bool ActorMatch<T>(T data) where T : struct;

        public ref C Get<C>(GameActor actor) where C: struct
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

        public List<ActorTuple<C1>> GetAll<C1>() where C1 : struct
        {
            List<ActorTuple<C1>> res = new List<ActorTuple<C1>>();
            ComponentArray<C1> pool = m_Components.GetPool<C1>();
            for(int i = 0; i < Actors.Count; ++i)
            {
                int compIndex = Actors[i].GetComponentIndex<C1>();
                if ( compIndex != -1 )
                {
                    ActorTuple<C1> actorTuple = new ActorTuple<C1>(Actors[i]);
                    actorTuple.SetC1(pool, compIndex);
                    res.Add(actorTuple);
                }
            }

            return res;
        }

        public List<ActorTuple<C1>> GetActorsMatching<C1>(ActorMatch<C1> matchCondition) where C1 : struct
        {
            List<ActorTuple<C1>> results = new List<ActorTuple<C1>>();

            for(int i = 0; i < Actors.Count; ++i)
            {
                GameActor actor = Actors[i];
                int compIdx = actor.GetComponentIndex<C1>();
                if (compIdx != -1)
                {
                    ref C1 comp = ref Get<C1>(actor);
                    if (matchCondition(comp))
                    {
                        ActorTuple<C1> tuple = new ActorTuple<C1>(actor);
                        tuple.SetC1(m_Components.GetPool<C1>(), compIdx);
                        results.Add(tuple);

                    }
                }

            }
            return results;
        }

        public HashSet<GameActor> ExcludeActors<T>()
        {
            HashSet<GameActor> excludedActors = new HashSet<GameActor>();
            for(int i = 0; i < Actors.Count; ++i)
            {
                if (Actors[i].GetComponentIndex<T>() == -1)
                    excludedActors.Add(Actors[i]);
            }
            return excludedActors;
        }

        public List<ActorTuple<C1, C2>> GetAll<C1, C2>()
            where C1 : struct
            where C2 : struct
        {
            List<ActorTuple<C1, C2>> res = new List<ActorTuple<C1, C2>>();
            ComponentArray<C1> pool = m_Components.GetPool<C1>();
            for (int i = 0; i < Actors.Count; ++i)
            {
                GameActor actor = Actors[i];
                int c1Idx = actor.GetComponentIndex<C1>();
                if (c1Idx != -1)
                {
                    int c2Idx = actor.GetComponentIndex<C2>();
                    if (c2Idx != -1)
                    {
                        var actorTuple = new ActorTuple<C1, C2>(actor);
                        actorTuple.SetC1(pool, c1Idx);
                        actorTuple.SetC2(m_Components.GetPool<C2>(), c2Idx);
                        res.Add(actorTuple);
                    }
                }
            }
            return res;
        }

        public List<ActorTuple<C1, C2, C3>> GetAll<C1, C2, C3>()
        where C1 : struct
        where C2 : struct
        where C3 : struct
        {
            List<ActorTuple<C1, C2, C3>> res = new List<ActorTuple<C1, C2, C3>>();
            for (int i = 0; i < Actors.Count; ++i)
            {
                GameActor actor = Actors[i];
                int c1Idx = actor.GetComponentIndex<C1>();
                int c2Idx = actor.GetComponentIndex<C2>();
                int c3Idx = actor.GetComponentIndex<C3>();
                if (c1Idx != -1 && c2Idx != -1 && c3Idx != -1)
                {
                    var actorTuple = new ActorTuple<C1, C2, C3>(actor);
                    actorTuple.SetC1(m_Components.GetPool<C1>(), c1Idx);
                    actorTuple.SetC2(m_Components.GetPool<C2>(), c2Idx);
                    actorTuple.SetC3(m_Components.GetPool<C3>(), c3Idx);
                    res.Add(actorTuple);
                }
            }
            return res;
        }

    }
}