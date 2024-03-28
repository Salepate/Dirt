using System;

namespace Dirt.Game.Math
{
    /// <summary>
    /// Based on https://learn.microsoft.com/en-us/archive/msdn-magazine/2016/august/test-run-lightweight-random-number-generation
    /// </summary>
    public class RNGLite
    {
        private const long A = 25214903917;
        private const long B = 11;
        private long m_Seed;
        public RNGLite(long seed = 0)
        {
            SetSeed(seed);
        }

        public void SetSeed(long seed)
        {
            if (seed < 0)
            {
                seed = 0;
            }

            m_Seed = seed;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="min">min (inclusive)</param>
        /// <param name="max">max (inclusive)</param>
        /// <returns></returns>
        public int Range(int min, int max) 
        {
            return min + NextInt(31) % (max - min + 1);
        }
        private int NextInt(int bits) // helper
        {
            m_Seed = (m_Seed * A + B) & ((1L << 48) - 1);
            return (int)(m_Seed >> (48 - bits));
        }
        public float Next()
        {
            return (((long)NextInt(26) << 27) + NextInt(27)) / (float)(1L << 53);
        }
    }
}
