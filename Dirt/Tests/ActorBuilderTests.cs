using System;
using System.Collections.Generic;
using Dirt.Simulation;
using Dirt.Simulation.Actor;
using Dirt.Simulation.Builder;
using Dirt.Simulation.Model;
using Dirt.Tests.Framework;
using Dirt.Tests.Samples;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dirt.Tests.Simulation
{
    [TestClass]
    public class ActorBuilderTests : DirtTest
    {
        [TestInitialize]
        public override void Initialize()
        {
            base.Initialize();
        }

        [TestMethod]
        public void Test_Builder_Limit()
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
        public void Test_Builder_Recycle()
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
        public void Test_Builder_IDS()
        {
            const int actorLimit = 100;
            ActorBuilder builder = new ActorBuilder(actorLimit);

            HashSet<int> ids = new HashSet<int>();

            for (int i = 0; i < actorLimit; ++i)
            {
                var actor = builder.CreateActor();
                Assert.IsFalse(ids.Contains(actor.ID));
                ids.Add(actor.ID);
            }
        }

        [TestMethod]
        public void Test_Builder_AddComponent()
        {
            ActorBuilder builder = new ActorBuilder(100);
            builder.Components.AllowLazy = true;
            GameActor actor = builder.CreateActor();
            ref SampleComponent comp = ref builder.AddComponent<SampleComponent>(actor);
            Assert.AreNotEqual(-1, actor.GetComponentIndex<SampleComponent>());
        }

        [TestMethod]
        public void Test_Builder_RemoveComponent()
        {
            ActorBuilder builder = new ActorBuilder(100);
            GameActor actor = builder.CreateActor();
            ref SampleComponent comp = ref builder.AddComponent<SampleComponent>(actor);
            builder.RemoveComponent<SampleComponent>(actor);
            Assert.AreEqual(-1, actor.GetComponentIndex<SampleComponent>());
        }
    }
}
