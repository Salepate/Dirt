using Dirt.Simulation;
using Dirt.Simulation.Actor;
using Dirt.Simulation.Actor.Components;
using Dirt.Simulation.Components;
using Dirt.Simulation.Model;
using Dirt.Tests.Samples;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;

namespace Dirt.Tests.Simulation
{
    [TestClass]
    public class SimulationTests : Framework.BaseSimulation
    {
        [TestInitialize]
        public override void Initialize()
        {
            base.Initialize();
        }

        [TestMethod]
        public void TestEmptySystems()
        {
            for(int i = 0; i < 1000; ++i)
            {
                Container.UpdateSystems(Simulation, 1);
            }
        }

        [TestMethod]
        public void TestCreateActor()
        {
            Builder.CreateActor();
            Assert.AreEqual(1, Simulation.Filter.Actors.Count, "1 Actor expected in simulation");
        }

        [TestMethod]
        public void TestDestroyActor()
        {
            GameActor actor = Simulation.Builder.CreateActor();
            Builder.DestroyActor(actor);
            Assert.AreEqual(0, Simulation.Filter.Actors.Count, "0 Actor expected in simulation");
        }

        [TestMethod]
        public void TestActorDestroyComponent()
        {
            Builder.Components.AllowLazy = true;

            var actor = Builder.CreateActor();
            Builder.AddComponent<Destroy>(actor);
            Assert.AreEqual(1, Simulation.Filter.Actors.Count, "1 Actor expected in simulation");
            Container.UpdateSystems(Simulation, 1f);
            Assert.AreEqual(0, Simulation.Filter.Actors.Count, "0 Actor expected in simulation");
        }

        [TestMethod]
        public void TestBuildActor()
        {
            SimulationPool pool = Simulation.Builder.Components;
            var actorArchetype = new ActorArchetype()
            {
                Components = new string[] { "Position" },
            };

            GameActor actor = Builder.BuildActor(actorArchetype);

            Assert.AreNotEqual(actor.GetComponentIndex<Position>(), -1, "Missing Component Position");
        }

        [TestMethod]
        public void Test_Build_Actor_WithSettings()
        {
            SimulationPool pool = Builder.Components;
            var actorArchetype = new ActorArchetype()
            {
                Components = new string[] { "SampleComponent" },
                Settings = new Dictionary<string, ComponentParameters>()
                {
                    { "SampleComponent", new ComponentParameters()
                    {
                        { "Value", "1337" }
                    } }
                }
            };

            GameActor actor = Builder.BuildActor(actorArchetype);
            int compIdx = actor.GetComponentIndex<SampleComponent>();
            var samplePool = pool.GetPool<SampleComponent>();
            int storedValue = samplePool.Components[compIdx].Value;
            Assert.AreEqual(1337, storedValue);
        }
    }
}
