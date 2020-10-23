namespace Dirt.Game.Math
{
    public class RNG
    {
        private MersenneTwister m_NativeGen;

        public RNG()
        {
            m_NativeGen = new MersenneTwister();
        }

        public RNG(int seed)
        {
            m_NativeGen = new MersenneTwister(seed);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="min">min vallue</param>
        /// <param name="max">max value</param>
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
