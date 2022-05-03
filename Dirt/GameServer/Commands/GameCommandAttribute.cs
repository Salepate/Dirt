namespace Dirt.GameServer.GameCommand
{
    public class GameCommandAttribute : System.Attribute
    {
        public string Name;
        public int Parameters;
        public bool IsPost {get; set;}
        public GameCommandAttribute(string commandName, int paramCount = 0, bool postCommand = false)
        {
            Name = commandName;
            Parameters = paramCount;
            IsPost = postCommand;
        }
    }
}
