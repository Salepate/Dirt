using Dirt.Simulation;
using Dirt.Simulation.Actor;
using Dirt.Simulation.Components;
using Dirt.Tests.Samples;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dirt.Tests.Simulation
{
    [TestClass]
    public class FilterTests : Framework.BaseSimulation
    {
        private ActorFilter m_Filter => Simulation.Filter;

        [TestInitialize]
        public override void Initialize()
        {
            base.Initialize();
            Builder.Components.AllowLazy = true;
        }


        [TestMethod]
        public void Test_Filter_SingleActor()
        {
            GameActor actor = Builder.CreateActor();
            bool found = m_Filter.TryGetActor(actor.ID, out GameActor resActor);
            Assert.IsTrue(found, "Actor not found");
            Assert.AreSame(actor, resActor, "Actor found mismatch");
        }

        [TestMethod]
        public void Test_Filter_ActorList_1Comp()
        {
            const int actorWithComp = 20;
            const int actorWithoutComp = 10;
            for (int i = 0; i < actorWithComp; ++i)
            {
                GameActor actor = Builder.CreateActor();
                Builder.AddComponent<SampleComponent>(actor);
            }

            for (int i = 0; i < actorWithoutComp; ++i)
            {
                GameActor actor = Builder.CreateActor();
            }

            ActorList<SampleComponent> res = m_Filter.GetActors<SampleComponent>();
            Assert.AreEqual(actorWithComp, res.Count);
        }

        [TestMethod]
        public void Test_Filter_ActorList_1Comp_CompIndex()
        {
            const int sampleValue = 127;
            const int actorCount = 100;
            int matchingActors = 0;

            for (int i = 0; i < actorCount; ++i)
            {
                GameActor actor = Builder.CreateActor();
                ref SampleComponent comp = ref Builder.AddComponent<SampleComponent>(actor);
                if ( i == 0 ) // only first actor
                {
                    comp.Value = sampleValue;
                }
            }

            ActorList<SampleComponent> actors = m_Filter.GetActors<SampleComponent>();
            for(int i = 0; i < actors.Count; ++i)
            {
                if (actors.GetC1(i).Value == sampleValue)
                    ++matchingActors;

            }

            Assert.AreEqual(1, matchingActors);
        }

        [TestMethod]
        public void Test_Filter_WriteComponent()
        {
            const int customValue = 888;
            GameActor actor = Builder.CreateActor();
            ref SampleComponent comp1 = ref Builder.AddComponent<SampleComponent>(actor);
            ref SampleComponent comp2 = ref m_Filter.Get<SampleComponent>(actor);
            comp2.Value = customValue;

            Assert.AreEqual(comp1.Value, comp2.Value, "Filter object different");
        }

        [TestMethod]
        public void Test_Filter_GetAll()
        {
            const int actorWithComp = 20;
            const int actorWithoutComp = 10;
            for (int i = 0; i < actorWithComp; ++i)
            {
                GameActor actor = Builder.CreateActor();
                Builder.AddComponent<SampleComponent>(actor);
            }

            for (int i = 0; i < actorWithoutComp; ++i)
            {
                GameActor actor = Builder.CreateActor();
            }

            var res = m_Filter.GetAll<SampleComponent>();
            Assert.AreEqual(actorWithComp, res.Count);
        }

        [TestMethod]
        public void Test_Filter_GetAll_2()
        {
            const int actorWithComp = 20;
            const int actorWith2Comps = 10;
            for (int i = 0; i < actorWithComp; ++i)
            {
                GameActor actor = Builder.CreateActor();
                Builder.AddComponent<SampleComponent>(actor);
            }

            for (int i = 0; i < actorWith2Comps; ++i)
            {
                GameActor actor = Builder.CreateActor();
                Builder.AddComponent<SampleComponent>(actor);
                Builder.AddComponent<Position>(actor);
            }

            var res = m_Filter.GetAll<SampleComponent, Position>();
            Assert.AreEqual(actorWith2Comps, res.Count);
        }
    }
}
