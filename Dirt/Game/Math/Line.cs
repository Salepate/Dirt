namespace Dirt.Game.Math
{
    public static class Line
    {
        /// <summary>
        /// Line / Circle Intersection
        /// </summary>
        /// <remarks>
        /// based on: https://stackoverflow.com/questions/1073336/circle-line-segment-collision-detection-algorithm
        /// </remarks>
        /// <param name="lineStart"></param>
        /// <param name="lineDir"></param>
        /// <param name="circleOrigin"></param>
        /// <param name="circleRadius"></param>
        /// <param name="maxDistance"></param>
        /// <returns></returns>
        public static bool Intersect(float3 lineStart, float3 lineDir, float3 circleOrigin, float circleRadius, float maxDistance = 100f)
        {
            float3 f = lineStart - circleOrigin;
            float3 lineRay = lineDir.normalized() * maxDistance;
            float a = lineRay.x * lineRay.x + lineRay.z * lineRay.z;
            float b = 2 * (f.x * lineRay.x + f.z * lineRay.z);
            float c = (f.x * f.x + f.z * f.z) - circleRadius * circleRadius;
            float disc = b * b - 4 * a * c;

            if (disc >= 0)
            {
                disc = (float)System.Math.Sqrt(disc);
                float t1 = (-b - disc) / (2 * a);
                float t2 = (-b + disc) / (2 * a);

                if (t1 >= 0 && t1 <= 1)
                    return true;

                if (t2 >= 0 && t2 <= 1)
                    return true;
            }
            return false;
        }

        private static readonly float2[] s_Line_Points = new float2[4];

        public static bool GetHalfLineRectangleIntersection(float2 halfline_start, float2 halfline_dir,
            float2 rectA, float2 rectB, float2 dirI, float2 dirJ, 
            out float2 intersection)
        {
            intersection = float2.zero;
            float2 tmpI;
            float closestIntersectionDistance = float.MaxValue;
            bool intersected = false;

            s_Line_Points[0] = rectA;
            s_Line_Points[1] = rectB;
            s_Line_Points[2] = dirI;
            s_Line_Points[3] = dirJ;

            for (int i = 0; i < 4; ++i)
            {
                int pt = i / 2;
                int dir = 2 + (i % 2);
                int sign = 1 - pt * 2;
                float2 s_start = s_Line_Points[pt];
                float2 s_end = s_Line_Points[pt] + s_Line_Points[dir] * sign;

                if (GetHalfLineSegmentIntersection(halfline_start, halfline_dir, s_start, s_end, out tmpI))
                {
                    intersected = true;
                    float squareMag = (halfline_start - tmpI).sqrMagnitude;
                    if (squareMag < closestIntersectionDistance)
                    {
                        closestIntersectionDistance = squareMag;
                        intersection = tmpI;
                    }
                }
            }

            return intersected;
        }

        public static bool GetHalfLineAARectangleIntersection(float2 halfline_start, float2 halfline_dir, 
            float2 rectangle_center, float2 rectangle_size, 
            out float2 intersection)
        {
            intersection = float2.zero;
            float2 min = rectangle_center - rectangle_size * 0.5f;
            float2 max = rectangle_center + rectangle_size * 0.5f;
            float2 dirI = new float2(max.x - min.x, 0f);
            float2 dirJ = new float2(0f, max.y - min.y);
            return GetHalfLineRectangleIntersection(halfline_start, halfline_dir, min, max, dirI, dirJ, out intersection);
        }

        public static bool GetHalfLineSegmentIntersection(float2 half_start, float2 half_dir, float2 s_start, float2 s_end, out float2 intersection)
        {
            if ( GetLinesIntersection(half_start, half_start + half_dir, s_start, s_end, out intersection) )
            {
                float2 segDir = s_end - s_start;
                float2 segDirRev = s_start - s_end;
                float2 startToInter = intersection - s_start;
                float2 endTointer = intersection - s_end;
                float dot1 = float2.dot(segDir, startToInter);
                float dot2 = float2.dot(segDirRev, endTointer);
                float dot3 = float2.dot(half_dir, intersection - half_start);
                return dot1 > 0f && dot2 > 0f && dot3 > 0f;
            }
            return false;
        }

        public static bool GetLinesIntersection(float2 l1_start, float2 l1_end, float2 l2_start, float2 l2_end, out float2 intersection)
        {
            float A1 = l1_end.y - l1_start.y;
            float B1 = l1_start.x - l1_end.x;
            float A2 = l2_end.y - l2_start.y;
            float B2 = l2_start.x - l2_end.x;
            float delta = A1 * B2 - A2 * B1;
            intersection = float2.zero;

            if (delta == 0)
            {
                return false;
            }

            float C2 = A2 * l2_start.x + B2 * l2_start.y;
            float C1 = A1 * l1_start.x + B1 * l1_start.y;
            float invdelta = 1 / delta;
            intersection = new float2((B2 * C1 - B1 * C2) * invdelta, (A1 * C2 - A2 * C1) * invdelta);
            return true;
        }
    }
}