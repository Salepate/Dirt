namespace Dirt.Game.Math
{
    public static class TrajectorySolver
    {
        public static float3 SolveLinear(float3 position, float3 direction, float speed, float elapsedTime)
        {
            return position + speed * direction * elapsedTime;
        }
    }
}
