using System.Diagnostics;
using NativeConsole = System.Console;

namespace Dirt.Log
{
    public class BasicLogger : IConsoleLogger
    {
        private const int s_IgnoreFrameCount = 3; // public + internal

        public void Message(string tag, string message, string uniqueColor)
        {
            NativeConsole.WriteLine($"[{tag}] {message}");
        }

        public void Warning(string tag, string message, string uniqueColor)
        {
            NativeConsole.WriteLine($"<Warning> [{tag}] {message}");
        }

        public void Error(string tag, string message, string uniqueColor)
        {
            NativeConsole.WriteLine($"<Error> [{tag}] {message}");
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
