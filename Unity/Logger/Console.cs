using Dirt.Log;
using System.Diagnostics;
using UnityEngine;


namespace Dirt
{
    using Debug = UnityEngine.Debug;
    public class UnityLogger : IConsoleLogger
    {
        private const int s_IgnoreFrameCount = 3; // public + internal

        public void Message(string tag, string message, string color)
        {
            Debug.Log($"[<color=#{color}>{tag}</color>] {message}");
        }

        public void Warning(string tag, string message, string color)
        {
            Debug.LogWarning($"[<color=#{color}>{tag}</color>] {message}");
        }

        public void Error(string tag, string message, string color)
        {
            Debug.LogError($"[<color=#{color}>{tag}</color>] {message}");
        }

        public string GetTag()
        {
            StackTrace currentTrace = new StackTrace();
            StackFrame[] frames = currentTrace.GetFrames();
            StackFrame frame = frames[s_IgnoreFrameCount];
            System.Type callingType = frame.GetMethod().DeclaringType;
            return callingType.Name;
        }
    }
}