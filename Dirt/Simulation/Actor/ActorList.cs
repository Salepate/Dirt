using System;
using System.Collections.Generic;
using System.Threading;

namespace Dirt.Simulation.Actor
{
    public struct ActorList<C1> where C1 : struct, IComponent
    {
        internal ActorQuery Query;
        internal ActorQuery C1Query;

        public List<GameActor> Actors;
        public ComponentArray<C1> C1Components;

        public int Count => Query.Count;

        internal ActorList(List<GameActor> worldActors, ActorQuery indiceTable, ActorQuery c1Indices, ComponentArray<C1> c1Comps)
        {
            Query = indiceTable;
            C1Query = c1Indices;
            C1Components = c1Comps;
            Actors = worldActors;
        }

        // legacy compatibility 
        public ActorTuple<C1> GetTuple(int actorIndex)
        {
            ActorTuple<C1> res = new ActorTuple<C1>(Actors[actorIndex]);
            res.SetC1(C1Components, C1Query.Indices[actorIndex]);
            return res;
        }

        public GameActor GetActor(int index) => Actors[Query.Indices[index]];
        public ref C1 GetC1(int index)
        {
            return ref C1Components.Components[C1Query.Indices[index]];
        }
    }

    public struct ActorList<C1, C2>
        where C1 : struct, IComponent
        where C2 : struct, IComponent
    {
        internal ActorQuery Query;
        internal ActorQuery C1Query;
        internal ActorQuery C2Query;

        public List<GameActor> Actors;
        public ComponentArray<C1> C1Components;
        public ComponentArray<C2> C2Components;

        public int Count => Query.Count;

        internal ActorList(List<GameActor> worldActors, ActorQuery indiceTable,
            ActorQuery c1Indices, ComponentArray<C1> c1Comps,
            ActorQuery c2Indices, ComponentArray<C2> c2Comps)
        {
            Query = indiceTable;
            C1Query = c1Indices;
            C1Components = c1Comps;
            C2Query = c2Indices;
            C2Components = c2Comps;
            Actors = worldActors;
        }

        // legacy compatibility 
        public ActorTuple<C1, C2> GetTuple(int actorIndex)
        {
            ActorTuple<C1, C2> res = new ActorTuple<C1, C2>(Actors[actorIndex]);
            res.SetC1(C1Components, C1Query.Indices[actorIndex]);
            res.SetC2(C2Components, C2Query.Indices[actorIndex]);
            return res;
        }

        public GameActor GetActor(int index) => Actors[Query.Indices[index]];
        public ref C1 GetC1(int index)
        {
            return ref C1Components.Components[C1Query.Indices[index]];
        }
        public ref C2 GetC2(int index)
        {
            return ref C2Components.Components[C2Query.Indices[index]];
        }
    }

    public struct ActorList<C1, C2, C3>
        where C1 : struct, IComponent
        where C2 : struct, IComponent
        where C3 : struct, IComponent
    {
        internal ActorQuery Query;
        internal ActorQuery C1Query;
        internal ActorQuery C2Query;
        internal ActorQuery C3Query;

        public List<GameActor> Actors;
        public ComponentArray<C1> C1Components;
        public ComponentArray<C2> C2Components;
        public ComponentArray<C3> C3Components;

        public int Count => Query.Count;

        internal ActorList(List<GameActor> worldActors, ActorQuery indiceTable,
            ActorQuery c1Indices, ComponentArray<C1> c1Comps,
            ActorQuery c2Indices, ComponentArray<C2> c2Comps,
            ActorQuery c3Indices, ComponentArray<C3> c3Comps)
        {
            Query = indiceTable;
            C1Query = c1Indices;
            C1Components = c1Comps;
            C2Query = c2Indices;
            C2Components = c2Comps;
            C3Query = c3Indices;
            C3Components = c3Comps;
            Actors = worldActors;
        }

        // legacy compatibility 
        public ActorTuple<C1, C2, C3> GetTuple(int actorIndex)
        {
            ActorTuple<C1, C2, C3> res = new ActorTuple<C1, C2, C3>(Actors[actorIndex]);
            res.SetC1(C1Components, C1Query.Indices[actorIndex]);
            res.SetC2(C2Components, C2Query.Indices[actorIndex]);
            res.SetC3(C3Components, C3Query.Indices[actorIndex]);
            return res;
        }

        public GameActor GetActor(int index) => Actors[Query.Indices[index]];
        public ref C1 GetC1(int index)
        {
            return ref C1Components.Components[C1Query.Indices[index]];
        }
        public ref C2 GetC2(int index)
        {
            return ref C2Components.Components[C2Query.Indices[index]];
        }

        public ref C3 GetC3(int index)
        {
            return ref C3Components.Components[C3Query.Indices[index]];
        }
    }

    public struct OpaqueTable
    {
        public int[] Actors;
        public int Count;
    }
}
