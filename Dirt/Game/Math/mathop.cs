using System.Runtime.CompilerServices;

namespace Dirt.Game.Math
{
    public static class mathop
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int min(int a, int b)
        {
            return a <= b ? a : b;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int max(int a, int b)
        {
            return a > b ? a : b;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float min(float a, float b)
        {
            return a <= b ? a : b;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float max(float a, float b)
        {
            return a > b ? a : b;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float abs(float a)
        {
            return a < 0 ? -a : a;
        }
    }
}
