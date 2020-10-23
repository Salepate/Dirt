using System;
using System.Runtime.InteropServices;

namespace Dirt.ServerApplication.Clock
{
    public static class HighResolutionClock
    {
        public static bool IsAvailable { get; private set; }

        [DllImport("Kernel32.dll", CallingConvention = CallingConvention.Winapi)]
        private static extern void GetSystemTimePreciseAsFileTime(out long filetime);
        public static DateTime UtcNow
        {
            get
            {
                if (!IsAvailable)
                {
                    throw new InvalidOperationException("High resolution clock is not available.");
                }
                long filetime;
                GetSystemTimePreciseAsFileTime(out filetime);
                return DateTime.FromFileTimeUtc(filetime);

            }
        }
        static HighResolutionClock()
        {
            try
            {
                long filetime;
                GetSystemTimePreciseAsFileTime(out filetime);
                IsAvailable = true;
            }
            catch (EntryPointNotFoundException)
            { // Not running Windows 8 or higher.
                IsAvailable = false;
            }
        }
    }
}