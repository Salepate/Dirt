namespace Dirt.Game.Math
{
    public static class Line
    {
        /// <summary>
        /// 3D distance to the shortest point on line
        /// </summary>
        /// <param name="pointOrigin"></param>
        /// <param name="lineStart"></param>
        /// <param name="lineDir"></param>
        /// <returns></returns>
        public static float PointToLineDistance(float3 pointOrigin, float3 lineStart, float3 lineDir)
        {
            float3 linePoint = lineStart + lineDir;
            float3 deltaPoint = pointOrigin - lineStart;
            float3 deltaPoint2 = pointOrigin - linePoint;
            return float3.cross(deltaPoint,deltaPoint2).magnitude / lineDir.magnitude;
        }

        /// <summary>
        /// Line / Circle Intersection (X/Z)
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

        /// <summary>
        /// Get the x/z intersection point (X/Z) space of 2 lines.
        /// </summary>
        /// <param name="l1_start"></param>
        /// <param name="l1_end"></param>
        /// <param name="l2_start"></param>
        /// <param name="l2_end"></param>
        /// <param name="intersection"></param>
        /// <returns></returns>
        public static bool GetLinesIntersectionXZ(float3 l1_start, float3 l1_end, float3 l2_start, float3 l2_end, out float3 intersection)
        {
            float A1 = l1_end.z - l1_start.z;
            float B1 = l1_start.x - l1_end.x;
            float A2 = l2_end.z - l2_start.z;
            float B2 = l2_start.x - l2_end.x;
            float delta = A1 * B2 - A2 * B1;
            intersection = float3.zero;

            if (delta == 0)
            {
                return false;
            }

            float C2 = A2 * l2_start.x + B2 * l2_start.z;
            float C1 = A1 * l1_start.x + B1 * l1_start.z;
            float invdelta = 1 / delta;
            intersection = new float3((B2 * C1 - B1 * C2) * invdelta, 0f, (A1 * C2 - A2 * C1) * invdelta);
            return true;
        }

        /// <summary>
        /// Find the x/z intersection point between 2 segments s1 and s2. (X/Z space)
        /// </summary>
        /// <param name="seg1_start"></param>
        /// <param name="seg1_end"></param>
        /// <param name="seg2_start"></param>
        /// <param name="seg2_end"></param>
        /// <param name="intersection">intersection point</param>
        /// <param name="endPointThreshold">extra threshold for edge case intersections</param>
        /// <returns>true if segments intersection</returns>
        public static bool GetSegmentsIntersectionXZ(float3 seg1_start, float3 seg1_end, float3 seg2_start, float3 seg2_end, out float3 intersection, float endPointThreshold = 0f)
        {
            float A1 = seg1_end.z - seg1_start.z;
            float B1 = seg1_start.x - seg1_end.x;
            float A2 = seg2_end.z - seg2_start.z;
            float B2 = seg2_start.x - seg2_end.x;
            float delta = A1 * B2 - A2 * B1;
            intersection = float3.zero;

            if (delta == 0)
            {
                return false;
            }
            float C2 = A2 * seg2_start.x + B2 * seg2_start.z;
            float C1 = A1 * seg1_start.x + B1 * seg1_start.z;
            float invdelta = 1 / delta;
            intersection = new float3((B2 * C1 - B1 * C2) * invdelta, 0f, (A1 * C2 - A2 * C1) * invdelta);

            float3 min_1 = float3.min(seg1_start, seg1_end) - float3.one * endPointThreshold;
            float3 max_1 = float3.max(seg1_start, seg1_end) + float3.one * endPointThreshold;
            float3 min_2 = float3.min(seg2_start, seg2_end) - float3.one * endPointThreshold;
            float3 max_2 = float3.max(seg2_start, seg2_end) + float3.one * endPointThreshold;

            return  intersection.x >= min_1.x && intersection.x >= min_2.x
                    && intersection.z >= min_1.z && intersection.z >= min_2.z
                    && intersection.x <= max_1.x && intersection.x <= max_2.x
                    && intersection.z <= max_1.z && intersection.z <= max_2.z;
        }

        /// <summary>
        /// Project the x/z of pt on the segment s1,s2 (X/Z space)
        /// </summary>
        /// <param name="s1">segment start</param>
        /// <param name="s2">segment end</param>
        /// <param name="pt">point to project</param>
        /// <param name="projection">projected point</param>
        /// <returns>true if the projection is on the segment directly</returns>
        public static bool ProjectXZ(float3 s1, float3 s2, float3 pt, out float3 projection)
        {
            float A1 = s2.z - s1.z;
            float B1 = s1.x - s2.x;
            float C1 = A1 * s1.x + B1 * s1.z;
            float C2 = -B1 * pt.x + A1 * pt.z;
            float det = A1 * A1 - -B1 * B1;
            float cx, cz;
            if (det != 0)
            {
                cx = (float)((A1 * C1 - B1 * C2) / det);
                cz = (float)((A1 * C2 - -B1 * C1) / det);
            }
            else
            {
                cx = pt.x;
                cz = pt.z;
            }
            projection = new float3(cx, 0f, cz);

            float3 min = float3.min(s1, s2) - float3.one * 0.01f;
            float3 max = float3.max(s1, s2) + float3.one * 0.01f;
            return projection.x >= min.x && projection.x <= max.x && projection.z >= min.z && projection.z <= max.z;
        }
    }
}