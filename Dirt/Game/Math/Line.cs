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
        /// <param name="lineSart"></param>
        /// <param name="lineDir"></param>
        /// <param name="circleOrigin"></param>
        /// <param name="circleRadius"></param>
        /// <param name="maxDistance"></param>
        /// <returns></returns>
        public static bool Intersect(float3 lineSart, float3 lineDir, float3 circleOrigin, float circleRadius, float maxDistance = 100f)
        {
            float3 f = lineSart - circleOrigin;
            float3 lineRay = lineDir.normalized() * maxDistance;
            float a = lineRay.x * lineRay.x + lineRay.z * lineRay.z;
            float b = 2 * (f.x * lineRay.x + f.z * lineRay.z);
            float c = (f.x * f.x + f.z * f.z) - circleRadius * circleRadius;
            float disc = b * b - 4 * a * c;

            if ( disc >= 0 )
            {
                disc = (float) System.Math.Sqrt(disc);
                float t1 = (-b - disc) / (2 * a);
                float t2 = (-b + disc) / (2 * a);

                if (t1 >= 0 && t1 <= 1)
                    return true;

                if (t2 >= 0 && t2 <= 1)
                    return true;
            }
            return false;
        }
    }
}