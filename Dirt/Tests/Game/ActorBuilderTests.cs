using Dirt.Simulation;
using Dirt.Simulation.Builder;
using Dirt.Tests.Framework;
using Dirt.Tests.Samples;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;

namespace Dirt.Tests.Simulation
{
    [TestClass]
    public class ActorBuilderTests : DirtTest
    {
        public const int MaxActor = 100;
        private ActorBuilder m_Builder;

        [TestInitialize]
        public override void Initialize()
        {
            base.Initialize();
            m_Builder = new ActorBuilder(MaxActor);
            m_Builder.Components.AllowLazy = true;
        }

        [TestMethod]
        public void Test_Builder_Limit()
        {
            for (int i = 0; i < MaxActor; ++i)
            {
                m_Builder.CreateActor();
            }

            Assert.ThrowsException<Exception>(() =>
            {
                m_Builder.CreateActor();
            });
        }

        [TestMethod]
        public void Test_Builder_Recycle()
        {
            Stack<GameActor> actors = new Stack<GameActor>(MaxActor);


            for (int i = 0; i < 100; ++i)
            {
                for (int j = 0; j < MaxActor; ++j)
                {
                    actors.Push(m_Builder.CreateActor());
                }
                for (int j = 0; j < MaxActor; ++j)
                {
                    m_Builder.DestroyActor(actors.Pop());
                }
            }
        }

        [TestMethod]
        public void Test_Builder_IDS()
        {
            HashSet<int> ids = new HashSet<int>();

            for (int i = 0; i < MaxActor; ++i)
            {
                var actor = m_Builder.CreateActor();
                Assert.IsFalse(ids.Contains(actor.ID));
                ids.Add(actor.ID);
            }
        }

        [TestMethod]
        public void Test_Builder_AddComponent_Generic()
        {
            GameActor actor = m_Builder.CreateActor();
            ref SampleComponent comp = ref m_Builder.AddComponent<SampleComponent>(actor);
            Assert.AreNotEqual(-1, actor.GetComponentIndex<SampleComponent>());
        }


        [TestMethod]
        public void Test_Builder_Add_SameComponentTwice_Generic()
        {
            GameActor actor = m_Builder.CreateActor();
            ref SampleComponent comp = ref m_Builder.AddComponent<SampleComponent>(actor);
            m_Builder.AddComponent<SampleComponent>(actor);
            Assert.AreEqual(1, actor.ComponentCount);
        }

        [TestMethod]
        public void Test_Builder_RemoveComponent_Generic()
        {
            GameActor actor = m_Builder.CreateActor();
            ref SampleComponent comp = ref m_Builder.AddComponent<SampleComponent>(actor);
            m_Builder.RemoveComponent<SampleComponent>(actor);
            Assert.AreEqual(-1, actor.GetComponentIndex<SampleComponent>());
        }

        [TestMethod]
        public void Test_Builder_AddComponent_Type()
        {
            GameActor actor = m_Builder.CreateActor();
            m_Builder.AddComponent(actor, typeof(SampleComponent));
            Assert.AreNotEqual(-1, actor.GetComponentIndex(typeof(SampleComponent)));
        }

        [TestMethod]
        public void Test_Builder_RemoveComponent_Type()
        {
            GameActor actor = m_Builder.CreateActor();
            m_Builder.AddComponent(actor, typeof(SampleComponent));
            m_Builder.RemoveComponent(actor, typeof(SampleComponent));
            Assert.AreEqual(-1, actor.GetComponentIndex(typeof(SampleComponent)));
        }

        [TestMethod]
        public void Test_Builder_Component_Mixed()
        {
            GameActor actor = m_Builder.CreateActor();
            m_Builder.AddComponent<SampleComponent>(actor);
            m_Builder.RemoveComponent(actor, typeof(SampleComponent));
            Assert.AreEqual(-1, actor.GetComponentIndex(typeof(SampleComponent)));
        }
    }
}
