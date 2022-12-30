﻿using Dirt.Game.Math;
using Dirt.GameServer.Simulation.Components;
using Dirt.Network.Simulation.Components;
using Dirt.Simulation;
using Dirt.Simulation.Actor.Components;
using Dirt.Simulation.Components;

namespace Dirt.GameServer.Simulation.Helpers
{
    public static class ActorSpawnHelper
    {
        public static GameActor SpawnPlayerCharacter(this GameSimulation simulation, int playerNumber, string archetypeName, float3 atPosition)
        {
            ServerActorBuilder builder = (ServerActorBuilder)simulation.Builder;
            GameActor playerChar = builder.BuildRemoteActor(archetypeName, playerNumber);
            ref Position pos = ref simulation.Filter.Get<Position>(playerChar);
            pos.Origin = atPosition;


            var existCullReq = simulation.Filter.GetActorsMatching<CullArea>(cull => cull.Client == playerNumber);
            if (existCullReq.Count > 0)
            {
                ref Tracker tracker = ref simulation.Filter.Get<Tracker>(existCullReq[0].Actor);
                tracker.TargetActor = playerChar.ID;
            }
            else
            {
                GameActor cullArea = builder.BuildActor("playerculling");
                ref CullArea cullData = ref simulation.Filter.Get<CullArea>(cullArea);
                ref Tracker trackerData = ref simulation.Filter.Get<Tracker>(cullArea);
                cullData.Client = playerNumber;
                trackerData.TargetActor = playerChar.ID;
            }


            return playerChar;
        }

        public static void RemovePlayerActors(this GameSimulation simulation, int playerNumber, bool keepPlayerCull = false)
        {
            var actors = simulation.Filter.GetActorsMatching<NetInfo>(c => c.Owner == playerNumber);

            foreach (var tuple in actors)
                simulation.Builder.AddComponent<Destroy>(tuple.Actor);


            if (!keepPlayerCull)
            {
                var cullings = simulation.Filter.GetActorsMatching<CullArea>(c => c.Client == playerNumber);
                foreach (var tuple in cullings)
                    simulation.Builder.AddComponent<Destroy>(tuple.Actor);
            }
        }
    }
}