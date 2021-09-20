using Microsoft.VisualStudio.TestTools.UnitTesting;
using NetSerializer;
using System;
using System.Collections.Generic;
using System.IO;

namespace Dirt.Tests.Simulation
{
    [TestClass]
    public class SerializerTests
    {
        [TestMethod]
        public void Test_Serializer_Identity()
        {
            Serializer serializer = new Serializer(new Type[] { typeof(TestSerialization) });

            TestSerialization testObj = new TestSerialization()
            {
                Field = "Saucisse"
            };


            using (MemoryStream ms = new MemoryStream())
            {
                serializer.Serialize(ms, testObj);
                byte[] byteArray = ms.ToArray();

                using (MemoryStream readStream = new MemoryStream(byteArray))
                {
                    TestSerialization testObj2 = (TestSerialization)serializer.Deserialize(readStream);

                    Assert.AreEqual(testObj.Field, testObj2.Field);
                }
            }

        }


        [TestMethod]
        public void Test_Serializer_MultipleItems()
        {
            const int firstInputValue = 5;
            const int secondInputValue = 3;
            const string thirdInputValue = "bah";

            Serializer serializer = new Serializer(new Type[] { typeof(System.Int32), typeof(System.String) });
            using (MemoryStream ms = new MemoryStream())
            {
                serializer.Serialize(ms, firstInputValue);
                serializer.Serialize(ms, secondInputValue);
                serializer.Serialize(ms, thirdInputValue);
                byte[] byteArray = ms.ToArray();

                using (MemoryStream readStream = new MemoryStream(byteArray))
                {
                    int firstInput = (int) serializer.Deserialize(readStream);
                    int secondInput = (int)serializer.Deserialize(readStream);
                    string thirdInput = (string)serializer.Deserialize(readStream);

                    Assert.AreEqual(firstInputValue, firstInput);
                    Assert.AreEqual(secondInputValue, secondInput);
                    Assert.AreEqual(0, thirdInputValue.CompareTo(thirdInput));
                }
            }

        }

        [TestMethod]
        public void Test_Serializer_Asymmetric()
        {
            Serializer serializer = new Serializer(new Type[] { typeof(TestSerialization) });
            Serializer serializer2 = new Serializer(new Type[] { typeof(TestSerialization2), typeof(TestSerialization) });

            TestSerialization testObj = new TestSerialization()
            {
                Field = "Saucisse"
            };


            using (MemoryStream ms = new MemoryStream())
            {
                serializer.Serialize(ms, testObj);
                byte[] byteArray = ms.ToArray();

                using (MemoryStream readStream = new MemoryStream(byteArray))
                {
                    TestSerialization testObj2 = (TestSerialization)serializer2.Deserialize(readStream);

                    Assert.AreEqual(testObj.Field, testObj2.Field);
                }
            }

        }
    }

    [System.Serializable]
    public class TestSerialization
    {
        public string Field;
    }


    [System.Serializable]
    public class TestSerialization2
    {
        public string Field;
    }

    [System.Serializable]
    public class HorribleClass
    {
        public Type T;
    }
}

