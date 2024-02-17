using Dirt.Simulation.Builder;
using Dirt.Simulation.Exceptions;
using System;
using System.Collections.Generic;

namespace Dirt.Simulation.Actor
{
    public class ActorFilter
    {
        public List<GameActor> Actors { get;  private set; }
        public SimulationPool Components { get; private set; }

        private ActorBuilder m_Builder;
        public ActorFilter(ActorBuilder builder, int querySize, int maxQueries)
        {
            Actors = new List<GameActor>(builder.ActorPoolSize);
            m_Builder = builder;
            Resize(querySize, maxQueries);
        }

        public void SetComponentPool(SimulationPool pool)
        {
            Components = pool;
        }

        public delegate bool ActorMatch<T>(T data) where T : struct;


        public ref C Get<C>(int actorID) where C: struct
        {
            GameActor actor = m_Builder.GetActorByID(actorID);
            ComponentArray<C> pool = Components.GetPool<C>();

            if ( actor != null )
            {
                return ref Get<C>(actor);
            }
            Log.Console.Error($"Invalid actor ID {actorID}");
            return ref pool.Fallback;
        } 

        public ref C Get<C>(GameActor actor) where C: struct
        {
            int compIdx = actor.GetComponentIndex<C>();
            if (compIdx != -1)
            {
                ComponentArray<C> pool = Components.GetPool<C>();
                return ref pool.Components[compIdx];
            }

            throw new ComponentNotFoundException(typeof(C));
        }

        public bool TryGetActor(int actorID, out GameActor actor) => (actor = m_Builder.GetActorByID(actorID)) != null;

        public List<ActorTuple<C1>> GetAll<C1>() where C1 : struct
        {
            List<ActorTuple<C1>> res = new List<ActorTuple<C1>>();
            ComponentArray<C1> pool = Components.GetPool<C1>();
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
                        tuple.SetC1(Components.GetPool<C1>(), compIdx);
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
            ComponentArray<C1> pool = Components.GetPool<C1>();
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
                        actorTuple.SetC2(Components.GetPool<C2>(), c2Idx);
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
                    actorTuple.SetC1(Components.GetPool<C1>(), c1Idx);
                    actorTuple.SetC2(Components.GetPool<C2>(), c2Idx);
                    actorTuple.SetC3(Components.GetPool<C3>(), c3Idx);
                    res.Add(actorTuple);
                }
            }
            return res;
        }

        // Garbage Free (WIP)
        private ActorQuery[] m_Queries;
        private int m_CurrentQuery;

        public GameActor GetSingle<C1>(ActorMatch<C1> matchCondition) where C1: struct, IComponent
        {
            ComponentArray<C1> pool = Components.GetPool<C1>();
            for (int i = 0; i < pool.Actors.Length; ++i)
            {
                if (pool.Actors[i] != -1 && matchCondition(pool.Components[i]) )
                {
                    return m_Builder.GetActorByID(pool.Actors[i]);
                }
            }
            return null;
        }
        public ActorList<C1> GetActors<C1>() where C1 : struct, IComponent
        {
            ActorQuery query = GetQuery();
            ActorQuery queryc1 = GetQuery();
            ComponentArray<C1> compPool = Components.GetPool<C1>();
            ActorList<C1> value = new ActorList<C1>(Actors, query, queryc1, compPool);

            for (int i = 0; compPool.Allocated > value.Count && i < Actors.Count; ++i)
            {
                int compIndex = Actors[i].GetComponentIndex<C1>();
                if (compIndex != -1)
                {
                    query.Add(i);
                    queryc1.Add(compIndex);
                }
            }
            return value;
        }

        public ActorList<C1, C2> GetActors<C1, C2>()
            where C1 : struct, IComponent
            where C2 : struct, IComponent
        {
            ActorQuery query = GetQuery();
            ActorQuery queryc1 = GetQuery();
            ActorQuery queryc2 = GetQuery();
            ActorList<C1, C2> value = new ActorList<C1, C2>(Actors, query,
                queryc1, Components.GetPool<C1>(),
                queryc2, Components.GetPool<C2>());

            for (int i = 0; value.C1Components.Allocated > value.Count
                && value.C2Components.Allocated > value.Count
                && i < Actors.Count; ++i)
            {
                GameActor actor = Actors[i];
                int compIndex = actor.GetComponentIndex<C1>();
                if (compIndex != -1)
                {
                    int c2Idx = actor.GetComponentIndex<C2>();
                    if (c2Idx != -1)
                    {
                        query.Add(i);
                        queryc1.Add(compIndex);
                        queryc2.Add(c2Idx);
                    }
                }
            }
            return value;
        }

        public ActorList<C1, C2, C3> GetActors<C1, C2, C3>()
            where C1 : struct, IComponent
            where C2 : struct, IComponent
            where C3 : struct, IComponent
        {
            ActorQuery query = GetQuery();
            ActorQuery queryc1 = GetQuery();
            ActorQuery queryc2 = GetQuery();
            ActorQuery queryc3 = GetQuery();
            ActorList<C1, C2, C3> value = new ActorList<C1, C2, C3>(Actors, query,
                queryc1, Components.GetPool<C1>(),
                queryc2, Components.GetPool<C2>(),
                queryc3, Components.GetPool<C3>());


            for (int i = 0; value.C1Components.Allocated > value.Count
                && value.C2Components.Allocated > value.Count
                && value.C3Components.Allocated > value.Count
                && i < Actors.Count; ++i)
            {
                GameActor actor = Actors[i];
                int c1Idx = actor.GetComponentIndex<C1>();
                if (c1Idx != -1)
                {
                    int c2Idx = actor.GetComponentIndex<C2>();
                    int c3Idx = actor.GetComponentIndex<C3>();
                    if (c2Idx != -1 && c3Idx != -1)
                    {
                        query.Add(i);
                        queryc1.Add(c1Idx);
                        queryc2.Add(c2Idx);
                        queryc3.Add(c3Idx);
                    }
                }
            }
            return value;
        }

        internal ActorQuery GetQuery()
        {
            if ( m_CurrentQuery >= m_Queries.Length)
            {
                throw new QueryLimitException() { MaxQueries = m_Queries.Length };
            }

            ActorQuery query = m_Queries[m_CurrentQuery++];
            query.Reset();
            return query;
        }
        public void ResetQueries()
        {
            m_CurrentQuery = 0;
        }

        internal void Resize(int querySize, int maxQueries)
        {
            m_CurrentQuery = 0;
            m_Queries = new ActorQuery[maxQueries];
            for (int i = 0; i < m_Queries.Length; ++i)
            {
                m_Queries[i] = new ActorQuery(querySize);
            }
        }
    }
}