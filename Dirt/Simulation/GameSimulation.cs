using System.Collections.Generic;
using Dirt.Simulation.Actor;
using Dirt.Simulation.Builder;
using Dirt.Simulation.Events;
using Dirt.Simulation.Model;

namespace Dirt.Simulation
{
    public class GameSimulation
    {
        public int ID { get; private set; }

        public SimulationArchetype Archetype;
        public string ArchetypeName;
        public Queue<SimulationEvent> Events { get; private set; }
        public ActorBuilder Builder { get; private set; }
        public ActorFilter Filter { get; private set; }
        public GameSimulation(int id, int maxActor, int maxQueries)
        {
            ID = id;
            Builder = new ActorBuilder();
            Events = new Queue<SimulationEvent>();

            Builder.ActorCreateAction += OnActorBuilt;
            Builder.ActorDestroyAction += OnActorDestroyed;
            Filter = new ActorFilter(Builder, maxActor, maxQueries);
        }

        public GameSimulation(ActorBuilder builder, int id, int maxQueries)
        {
            ID = id;
            Builder = builder;
            Events = new Queue<SimulationEvent>();

            Builder.ActorCreateAction += OnActorBuilt;
            Builder.ActorDestroyAction += OnActorDestroyed;
            Filter = new ActorFilter(Builder, Builder.ActorPoolSize, maxQueries);
        }

        public void Resize(int maximumActor, int maximumQueries)
        {
            Builder.InitializePool(maximumActor);
            Filter.Resize(maximumActor, maximumQueries);
            Filter.SetComponentPool(Builder.Components);
        }

        private void OnActorBuilt(GameActor actor)
        {
            Filter.Actors.Add(actor);
            Events.Enqueue(new ActorEvent(actor, ActorEvent.Created));
        }

        private void OnActorDestroyed(GameActor actor)
        {
            Filter.Actors.Remove(actor);
            Events.Enqueue(new ActorEvent(actor, ActorEvent.Removed));
        }
    }
}
