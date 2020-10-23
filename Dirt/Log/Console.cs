using System.Collections.Generic;
using FileStream = System.IO.FileStream;
using Random = System.Random;
using StackFrame = System.Diagnostics.StackFrame;
using StackTrace = System.Diagnostics.StackTrace;
using UTF8Encoding = System.Text.UTF8Encoding;

namespace Dirt.Log
{
    internal enum LogLevel
    {
        Info,
        Warning,
        Error
    }

    public static class Console
    {
        private const int s_IgnoreFrameCount = 2; // public + internal

        public static IConsoleLogger Logger { get; set; }

        public static bool Dump;

        #region cache
        private static Dictionary<System.Type, string> s_ColorMap;
        private static Random s_Random;
        private static bool s_Dumping;
        private static FileStream s_DumpFile;
        private static UTF8Encoding s_Encoder;
        #endregion

        static Console()
        {
            s_Random = new Random(255);
            s_ColorMap = new Dictionary<System.Type, string>();
            s_Dumping = false;
        }

        public static void Assert(bool test, string message)
        {
            if (!test)
            {
                InternalLog(LogLevel.Error, message);
                throw new System.Exception(message);
            }
        }

        public static void Assert(bool test, string message, params object[] args)
        {
            if (!test)
            {
                InternalLog(LogLevel.Error, string.Format(message, args));
                throw new System.Exception(message);
            }
        }

        public static void Message(string message) { InternalLog(LogLevel.Info, message); }
        public static void Message(string message, params object[] args) { InternalLog(LogLevel.Info, string.Format(message, args)); }
        public static void Warning(string message) { InternalLog(LogLevel.Warning, message); }
        public static void Warning(string message, params object[] args) { InternalLog(LogLevel.Warning, string.Format(message, args)); }
        public static void Error(string message) { InternalLog(LogLevel.Error, message); }
        public static void Error(string message, params object[] args) { InternalLog(LogLevel.Error, string.Format(message, args)); }

        #region internal
        private static void InternalLog(LogLevel logLevel, string message)
        {
            StackTrace currentTrace = new StackTrace();
            StackFrame[] frames = currentTrace.GetFrames();
            StackFrame frame = frames[s_IgnoreFrameCount];
            System.Type callingType = frame.GetMethod().DeclaringType;
            string color = GetColor(callingType);

            if (Logger == null)
                throw new System.Exception($"No Logger set");

            switch (logLevel)
            {
                case LogLevel.Info: 
                    Logger.Message(callingType.Name, message, color);
                    break;
                case LogLevel.Warning:
                    Logger.Warning(callingType.Name, message, color);
                    break;
                case LogLevel.Error:
                    Logger.Error(callingType.Name, message, color);
                    break;
                default:
                    break;
            }
        }

        private static string GetColor(System.Type t)
        {
            string col;
            if (!s_ColorMap.TryGetValue(t, out col))
            {
                int r = s_Random.Next(255);
                int g = s_Random.Next(255);
                int b = s_Random.Next(255);
                col = string.Format("{0}{1}{2}", r.ToString("X"), g.ToString("X"), b.ToString("X"));
                s_ColorMap.Add(t, col);
            }
            return col;
        }
        #endregion
    }
}