using Mathf = System.Math;

namespace Dirt.Game.Math
{
    public static class AreaOfEffect
    {
        public static bool IsPointInAOE(float3 point, float3 aoeCenter, float aoeRadius)
        {
            float rSquare = aoeRadius * aoeRadius;
            return (point - aoeCenter).sqrMagnitude <= rSquare;
        }

        public static bool IsPointInAOESquared(float3 point, float3 aoeCenter, float squareRadius)
        {
            return (point - aoeCenter).sqrMagnitude <= squareRadius;
        }

        public static float3 GetRandomPointInAoe(float3 origin, float aoeRadius, RNG rng)
        {
            float angle = rng.Value() * 2f * (float) System.Math.PI;
            float r = (float) Mathf.Sqrt(rng.Value()) * aoeRadius;

            return new float3((float)Mathf.Cos(angle), 0f, (float)Mathf.Sin(angle))*r + origin;
        }
    }
}
