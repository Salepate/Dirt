using System;

namespace Dirt.ServerApplication.Clock
{
    using Dirt.Log;
    public class GameClock
    {
        private delegate int TimeGetter();
        private System.Action m_StartTimeSetter;
        private TimeGetter m_TimeGetter;
        private long m_StartTime;

        public GameClock()
        {
            bool hasHighClock = false;
            try
            {
                hasHighClock = HighResolutionClock.IsAvailable;
            }
            catch (System.Exception _) { }

            if (!hasHighClock)
            {
                Console.Message($"Using Stopwatch Clock");
                m_StartTimeSetter = SetLowPrecisionStartTime;
                m_TimeGetter = GetLowPrecisionTime;
            }
            else
            {
                m_StartTimeSetter = SetHighPrecisionStartTime;
                m_TimeGetter = GetHighPrecisionTime;
            }
        }

        public void Reset()
        {
            m_StartTimeSetter();
        }

        public int GetTick()
        {
            return m_TimeGetter();
        }

        private void SetHighPrecisionStartTime()
        {
            m_StartTime = HighResolutionClock.UtcNow.Ticks;
        }
        private int GetHighPrecisionTime()
        {
            return (int)(HighResolutionClock.UtcNow.Ticks - m_StartTime);
        }

        private void SetLowPrecisionStartTime()
        {
            m_StartTime = DateTime.UtcNow.Ticks;
        }

        private int GetLowPrecisionTime()
        {
            return (int)(DateTime.UtcNow.Ticks - m_StartTime);
        }
    }
}