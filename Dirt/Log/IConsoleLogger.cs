namespace Dirt.Log
{
    public interface IConsoleLogger
    {
        void Message(string tag, string message, string uniqueColor);
        void Warning(string tag, string message, string uniqueColor);
        void Error(string tag, string message, string uniqueColor);
    }
}
