using Microsoft.VisualStudio.TestTools.UnitTesting;
using Dirt.Game.Math;

namespace Dirt.Tests.Math
{
    [TestClass]
    public class Math_Line_Tests
    {
        [TestMethod]
        public void Line_Intersect_Line()
        {
            float2 a = new float2(0f, 2f);
            float2 b = new float2(2f, 2f);
            float2 line1A = new float2(2f, 0f);
            float2 result;

            Assert.IsTrue(Line.GetLinesIntersection(a, b, line1A, b, out result));
        }

        [TestMethod]
        public void Line_Dont_Intersect_Line()
        {
            float2 a = new float2(0f, 2f);
            float2 b = new float2(2f, 2f);
            float2 line1A = new float2(0f, 0f);
            float2 line1B = new float2(2f, 0f);

            Assert.IsFalse(Line.GetLinesIntersection(a, b, line1A, line1B, out _));
        }

        [TestMethod]
        public void HalfLine_Intersect_Segment()
        {
            float2 start = new float2(0f, 2f);
            float2 dir = new float2(1f, -1f).normalized();
            float2 segA = new float2(0f, 0f);
            float2 segB = new float2(2f, 2f);

            Assert.IsTrue(Line.GetHalfLineSegmentIntersection(start, dir, segA, segB, out float2 intersection));
            Assert.IsTrue((intersection - float2.one).magnitude < 0.01f);
        }

        [TestMethod]
        public void HalfLine_Dont_Intersect_Segment()
        {
            float2 start = new float2(0f, 12f);
            float2 dir = new float2(-1f, -1f).normalized();
            float2 segA = new float2(0f, 0f);
            float2 segB = new float2(2f, 2f);

            Assert.IsFalse(Line.GetHalfLineSegmentIntersection(start, dir, segA, segB, out float2 intersection));
        }

        [TestMethod]
        public void HalfLine_Intersect_AASquare_Once_Threshold()
        {
            float2 hafline_start = new float2(1f, 1f);
            float2 halfline_dir = new float2(1f, 0f);
            float2 rect_center = new float2(5f, 0f);
            float2 rect_size = new float2(0.5f, 2.5f);

            Assert.IsTrue(Line.GetHalfLineAARectangleIntersection(hafline_start, halfline_dir, rect_center, rect_size, out float2 intersection));
            Assert.IsTrue((intersection - new float2(rect_center.x - rect_size.x / 2f, hafline_start.y)).magnitude < 0.01f);
        }

        [TestMethod]
        public void HalfLine_Dont_Intersect_AASquare_Once_Threshold()
        {
            float2 hafline_start = new float2(1f, 1f);
            float2 halfline_dir = new float2(-1f, 0f);
            float2 rect_center = new float2(5f, 0f);
            float2 rect_size = new float2(0.5f, 2.5f);
            Assert.IsFalse(Line.GetHalfLineAARectangleIntersection(hafline_start, halfline_dir, rect_center, rect_size, out _));
        }
    }
}
