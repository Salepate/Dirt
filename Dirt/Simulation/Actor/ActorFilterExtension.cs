﻿using System;
using System.Collections.Generic;
using static Dirt.Simulation.Actor.ActorFilter;

namespace Dirt.Simulation.Actor
{
    public static class ActorFilterExtension
    {
        //public static List<ActorTuple<C1>> GetActors<C1>(this List<GameActor> gameActors) where C1 : IComponent
        //{
        //    var results = new List<ActorTuple<C1>>();
        //    gameActors.ForEach(actor =>
        //    {
        //        var compIdx = actor.GetComponentIndex<C1>();
        //        if (compIdx != -1)
        //            results.Add(new ActorTuple<C1>(actor, (C1)actor.Components[compIdx]));

        //    });
        //    return results;
        //}

        //public static List<ActorTuple<C1, C2>> GetActors<C1, C2>(this List<GameActor> gameActors) 
        //    where C1 : IComponent 
        //    where C2 : IComponent
        //{
        //    var results = new List<ActorTuple<C1, C2>>();
        //    gameActors.ForEach(actor =>
        //    {
        //        var compIdx = actor.GetComponentIndex<C1>();
        //        var compIdx2 = actor.GetComponentIndex<C2>();
        //        if (compIdx != -1 && compIdx2 != -1)
        //            results.Add(new ActorTuple<C1, C2>(actor, (C1)actor.Components[compIdx], (C2) actor.Components[compIdx2]));

        //    });
        //    return results;
        //}

        //public static List<ActorTuple<C1, C2, C3>> GetActors<C1, C2, C3>(this List<GameActor> gameActors)
        //    where C1 : IComponent
        //    where C2 : IComponent
        //    where C3 : IComponent
        //{
        //    var results = new List<ActorTuple<C1, C2, C3>>();
        //    gameActors.ForEach(actor =>
        //    {
        //        var compIdx = actor.GetComponentIndex<C1>();
        //        var compIdx2 = actor.GetComponentIndex<C2>();
        //        var compIdx3 = actor.GetComponentIndex<C3>();
        //        if (compIdx != -1 && compIdx2 != -1 && compIdx3 != -1)
        //            results.Add(new ActorTuple<C1, C2, C3>(actor, (C1)actor.Components[compIdx], (C2)actor.Components[compIdx2], (C3)actor.Components[compIdx3]));

        //    });
        //    return results;
        //}

        //public static List<ActorTuple<C1>> GetActorsMatching<C1>(this List<GameActor> gameActors, ActorMatch<C1> matchCondition) where C1 : IComponent
        //{
        //    var results = new List<ActorTuple<C1>>();
        //    gameActors.ForEach(actor =>
        //    {
        //        var compIdx = actor.GetComponentIndex<C1>();
        //        if (compIdx != -1)
        //        {
        //            C1 comp = (C1)actor.Components[compIdx];
        //            if (matchCondition(comp))
        //                results.Add(new ActorTuple<C1>(actor, comp));
        //        }

        //    });
        //    return results;
        //}

        //public static HashSet<GameActor> ExcludeActors<C1>(this List<GameActor> gameActors) where C1 : IComponent
        //{
        //    var results = new HashSet<GameActor>();

        //    gameActors.ForEach(actor =>
        //    {
        //        var compIdx = actor.GetComponentIndex<C1>();
        //        if (compIdx == -1)
        //            results.Add(actor);

        //    });
        //    return results;
        //}

    }
}