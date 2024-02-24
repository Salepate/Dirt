namespace Dirt.Game.Math
{
    public class RNG
    {
        private MersenneTwister m_NativeGen;

        public readonly uint Seed;

        public RNG()
        {
            Seed = (uint)System.DateTime.Now.Millisecond;
            m_NativeGen = new MersenneTwister(Seed);
        }

        public RNG(int seed)
        {
            Seed = (uint)seed;
            m_NativeGen = new MersenneTwister(Seed);
        }

        public RNG(uint seed)
        {
            Seed = seed;
            m_NativeGen = new MersenneTwister(Seed);
        }

        public byte[] CreateState()
        {
            byte[] state;
            m_NativeGen.CopyState(out state);
            return state;
        }

        public void SetState(byte[] state)
        {
            m_NativeGen.SetState(state);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="min">min value</param>
        /// <param name="max">max value (inclusive)</param>
        /// <returns></returns>
        public int Range(int min, int max)
        {
            return m_NativeGen.Next(min, max);
        }

        /// <summary>
        /// </summary>
        /// <returns>Random value [0; 1[</returns>
        public float Value()
        {
            return m_NativeGen.NextFloat();
        }
    }
}
