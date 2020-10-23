using System;
using System.Collections.Generic;
using Dirt.Simulation;
using Dirt.Simulation.Builder;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dirt.Tests.Simulation
{
    [TestClass]
    public class ActorBuilderTests
    {
        [TestMethod]
        public void TestActorLimit()
        {
            const int actorLimit = 10;

            ActorBuilder builder = new ActorBuilder(actorLimit);

            for (int i = 0; i < actorLimit; ++i)
            {
                builder.CreateActor();
            }

            Assert.ThrowsException<Exception>(() =>
            {
                builder.CreateActor();
            });
        }

        [TestMethod]
        public void TestActorPool()
        {
            const int actorLimit = 10;

            ActorBuilder builder = new ActorBuilder(actorLimit);

            Stack<GameActor> actors = new Stack<GameActor>(actorLimit);


            for (int i = 0; i < 100; ++i)
            {
                for (int j = 0; j < actorLimit; ++j)
                {
                    actors.Push(builder.CreateActor());
                }
                for (int j = 0; j < actorLimit; ++j)
                {
                    builder.DestroyActor(actors.Pop());
                }
            }
        }

        [TestMethod]
        public void TestUniqueID()
        {
            const int actorLimit = 100;
            ActorBuilder builder = new ActorBuilder(actorLimit);

            HashSet<int> ids = new HashSet<int>();

            for(int i = 0; i < actorLimit; ++i)
            {
                var actor = builder.CreateActor();
                Assert.IsFalse(ids.Contains(actor.ID));
                ids.Add(actor.ID);
            }
        }
    }
}
