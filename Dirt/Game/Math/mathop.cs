using System.Runtime.CompilerServices;

namespace Dirt.Game.Math
{
    public static class mathop
    {

        #region Integer
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
        public static int clamp(int value, int min, int max)
        {
            return value < min ? min : value > max ? max : value;
        }
        #endregion


        #region Float
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
        #endregion

        #region Signed Long
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static long min(long a, long b)
        {
            return a <= b ? a : b;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static long max(long a, long b)
        {
            return a > b ? a : b;
        }
        #endregion
        #region Unsigned Long
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong min(ulong a, ulong b)
        {
            return a <= b ? a : b;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong max(ulong a, ulong b)
        {
            return a > b ? a : b;
        }
        #endregion
    }
}
