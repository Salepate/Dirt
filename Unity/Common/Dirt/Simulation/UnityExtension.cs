using Dirt.Game.Math;

namespace Dirt.Simulation
{
    public static class UnityExtension
    {
        public static UnityEngine.Vector3 toVector(this float3 p) { return new UnityEngine.Vector3(p.x, p.y, p.z); }
        public static float3 toFloat3(this UnityEngine.Vector3 v) { return new float3(v.x, v.y, v.z); }
    }
}