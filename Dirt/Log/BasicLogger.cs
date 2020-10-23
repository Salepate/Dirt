using NativeConsole = System.Console;

namespace Dirt.Log
{
    public class BasicLogger : IConsoleLogger
    {
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

    }
}
