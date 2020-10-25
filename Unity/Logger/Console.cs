using Dirt.Log;
using UnityEngine;

namespace Dirt
{
    public class UnityLogger : IConsoleLogger
    {
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
    }
}