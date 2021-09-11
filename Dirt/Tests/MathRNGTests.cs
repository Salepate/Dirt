using Microsoft.VisualStudio.TestTools.UnitTesting;
using Dirt.Game.Math;

namespace Dirt.Tests.Math
{
    [TestClass]
    public class Math_RNG_Tests
    {
        [TestMethod]
        public void Test_RNG_Serialization()
        {
            RNG rng = new RNG(255);
            RNG rng2;
            var state = rng.CreateState();
            rng2 = new RNG(0);
            rng2.SetState(state);

            Assert.AreEqual(rng.Value(), rng2.Value(), 0.0);
        }
    }
}
