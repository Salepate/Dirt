namespace Dirt.Log
{
    public interface IConsoleLogger
    {
        string GetTag();
        void Message(string tag, string message, string uniqueColor);
        void Warning(string tag, string message, string uniqueColor);
        void Error(string tag, string message, string uniqueColor);
    }
}
