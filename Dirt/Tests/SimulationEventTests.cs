using Dirt.Simulation;
using Dirt.Simulation.SystemHelper;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dirt.Tests.Simulation
{
    public class TestEvent : SimulationEvent
    {
        public TestEvent(int eventID) { Event = eventID; }
    }
    public class ListenerClass : IEventReader
    {
        [SimulationListener(typeof(TestEvent), 0)]
        public void MessageListener(TestEvent message)
        {
            message.Consume();
        }
    }

    public abstract class AListenerClass : IEventReader
    {
        [SimulationListener(typeof(TestEvent), 0)]
        public void MessageListener(TestEvent message)
        {
            message.Consume();
        }
    }

    public class SubClass : ListenerClass { }

    public class SubClassAbstract : AListenerClass { }

    [TestClass]
    public class SimulationEventTests : Framework.BaseSimulation
    {
        [TestInitialize]
        public override void Initialize()
        {
            base.Initialize();
        }

        [TestMethod]
        public void TestInjection_SimpleClass()
        {
            TestEvent testEvt = new TestEvent(0);
            Container.InjectEventDispatchers(new ListenerClass());
            Simulation.Events.Enqueue(testEvt);
            Container.UpdateSystems(Simulation, 1f);
            Assert.IsTrue(testEvt.Consumed, "Event message not consumed");
        }

        [TestMethod]
        public void TestInjection_SubClass()
        {
            TestEvent testEvt = new TestEvent(0);
            Container.InjectEventDispatchers(new SubClass());
            Simulation.Events.Enqueue(testEvt);
            Container.UpdateSystems(Simulation, 1f);
            Assert.IsTrue(testEvt.Consumed, "Event message not consumed");
        }

        [TestMethod]
        public void TestInjection_SubClassFromAbstract()
        {
            TestEvent testEvt = new TestEvent(0);
            Container.InjectEventDispatchers(new SubClassAbstract());
            Simulation.Events.Enqueue(testEvt);
            Container.UpdateSystems(Simulation, 1f);
            Assert.IsTrue(testEvt.Consumed, "Event message not consumed");
        }
    }
}
