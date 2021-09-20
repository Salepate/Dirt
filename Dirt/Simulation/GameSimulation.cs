using System.Collections.Generic;
using Dirt.Simulation.Actor;
using Dirt.Simulation.Builder;
using Dirt.Simulation.Events;

namespace Dirt.Simulation
{
    public class GameSimulation
    {
        public GameWorld World;
        public int ID { get; private set; }

        public string Archetype;
        public Queue<SimulationEvent> Events { get; private set; }
        public ActorBuilder Builder { get; private set; }
        public ActorFilter Filter { get; private set; }
        public GameSimulation(int id = 0)
        {
            ID = id;
            Builder = new ActorBuilder(1000);
            World = new GameWorld();
            Events = new Queue<SimulationEvent>();

            Builder.ActorCreateAction += OnActorBuilt;
            Builder.ActorDestroyAction += OnActorDestroyed;
            Filter = new ActorFilter(Builder.Components, World.Actors);
        }

        public GameSimulation(ActorBuilder builder, int id = 0)
        {
            ID = id;
            Builder = builder;
            World = new GameWorld();
            Events = new Queue<SimulationEvent>();

            Builder.ActorCreateAction += OnActorBuilt;
            Builder.ActorDestroyAction += OnActorDestroyed;
            Filter = new ActorFilter(Builder.Components, World.Actors);
        }

        private void OnActorBuilt(GameActor actor)
        {
            World.Actors.Add(actor);
            Events.Enqueue(new ActorEvent(actor, ActorEvent.Created));
        }

        private void OnActorDestroyed(GameActor actor)
        {
            World.Actors.Remove(actor);
            Events.Enqueue(new ActorEvent(actor, ActorEvent.Removed));
        }
    }
}
