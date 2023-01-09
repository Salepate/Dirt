using Microsoft.VisualStudio.TestTools.UnitTesting;
using Mud;

namespace Dirt.Tests.Simulation
{
    [TestClass]
    public class CircularBufferTests
    {
        [TestMethod]
        public void Test_SingleCapacity()
        {
            CircularBuffer<int> buffer = new CircularBuffer<int>(1);
            buffer.Add(1);
            Assert.AreEqual(1, buffer[0]);
            buffer.Add(255);
            Assert.AreEqual(255, buffer[0]);
            Assert.AreEqual(1, buffer.Capacity);
        }

        [TestMethod]
        public void Test_IndexOf()
        {
            CircularBuffer<int> buffer = new CircularBuffer<int>(32);

            for(int i = 0; i < 16; ++i)
            {
                int value = i * 2;
                buffer.Add(value);
                Assert.AreEqual(i, buffer.IndexOf(value));
            }

            for(int i = 0; i < 32; ++i)
            {
                int value = 128 + i;
                buffer.Add(value);
                int index = buffer.IndexOf(value);
                Assert.AreEqual(value, buffer[index]);
            }
        }

        [TestMethod]
        public void Test_Add()
        {
            const int buffer_size = 4;
            CircularBuffer<int> buffer = new CircularBuffer<int>(buffer_size);
            int defaultValue = 0;
            int valueA = 1;
            int valueB = 255;


            buffer.Add(valueA);
            Assert.AreEqual(1, buffer.Count);
            Assert.AreEqual(valueA, buffer[0]);

            buffer.Add(valueB);
            Assert.AreEqual(2, buffer.Count);
            Assert.AreEqual(valueB, buffer[1]);

            for(int i = 0; i < buffer_size; ++i)
            {
                buffer.Add(defaultValue);
            }

            Assert.AreEqual(buffer.Count, buffer_size);
            for(int i = 0; i < buffer.Count; ++i)
            {
                Assert.AreEqual(defaultValue, buffer[i]);
            }
        }
    }
}

