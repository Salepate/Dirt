using Dirt.Simulation.Actor;
using Dirt.Simulation.Exceptions;
using Dirt.Tests.Samples;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dirt.Tests.Simulation
{
    [TestClass]
    public class ComponentArrayTests : Framework.DirtTest
    {
        [TestMethod]
        public void Test_ComponentArray_Size()
        {
            ComponentArray<int> compArray = new ComponentArray<int>();
            compArray.SetSize(10);
            for (int i = 0; i < 10; ++i)
                compArray.Allocate(i);

            Assert.ThrowsException<ComponentLimitException>(() =>
            {
                compArray.Allocate(10);
            });
        }

        [TestMethod]
        public void Test_ComponentArray_Recycle()
        {
            ComponentArray<int> compArray = new ComponentArray<int>();
            compArray.SetSize(1);
            for(int i = 0; i < 10; ++i)
            {
                int idx = compArray.Allocate(0);
                compArray.Free(idx);
            }
        }

        [TestMethod]
        public void Test_ComponentArray_Assign()
        {
            ComponentArray<SampleComponent> compArray = new ComponentArray<SampleComponent>();
            compArray.SetSize(1);
            int idx = compArray.Allocate(0);
            Assert.AreEqual(SampleComponent.DefaultValue, compArray.Components[idx].Value);
        }
    }
}
