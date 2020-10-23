using Dirt.Simulation;
using Dirt.Simulation.Actor.Components;
using Dirt.Tests.Mocks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dirt.Tests.Simulation
{
    [TestClass]
    public class SimulationTests
    {
        [TestMethod]
        public void TestEmptySystems()
        {
            var content = new MockContentProvider();
            var mgr = new Mocks.MockManagerProvider();
            var sim = new GameSimulation();

            SystemContainer systems = new SystemContainer(content, mgr);
            
            for(int i = 0; i < 1000; ++i)
            {
                systems.UpdateSystems(sim, 1);
            }
        }

        [TestMethod]
        public void TestAddActor()
        {
            var sim = new GameSimulation();
            sim.Builder.CreateActor();
            Assert.AreEqual(1, sim.World.Actors.Count, "1 Actor expected in simulation");
        }

        [TestMethod]
        public void TestDestroyActor()
        {
            var sim = new GameSimulation();
            var actor = sim.Builder.CreateActor();
            sim.Builder.DestroyActor(actor);
            Assert.AreEqual(0, sim.World.Actors.Count, "0 Actor expected in simulation");
        }

        [TestMethod]
        public void TestActorDestroyComponent()
        {
            var content = new MockContentProvider();
            var mgr = new Mocks.MockManagerProvider();
            var sim = new GameSimulation();

            SystemContainer systems = new SystemContainer(content, mgr);

            var actor = sim.Builder.CreateActor();
            actor.AddComponent<Destroy>(new Destroy());
            Assert.AreEqual(1, sim.World.Actors.Count, "1 Actor expected in simulation");
            systems.UpdateSystems(sim, 1f);
            Assert.AreEqual(0, sim.World.Actors.Count, "0 Actor expected in simulation");
        }
    }
}
