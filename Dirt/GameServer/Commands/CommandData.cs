using System.Reflection;

namespace Dirt.GameServer.GameCommand
{
    public class CommandData
    {
        public MethodInfo CommandMethod;
        public GameCommandAttribute Attribute;
    }
}
