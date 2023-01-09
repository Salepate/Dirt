using Mud.Server;
using System.Configuration;

namespace Mud.ServerInstance
{
    public class Program
    {
        static void Main(string[] args)
        {
            Dirt.Log.Console.Logger = new Dirt.Log.BasicLogger();
            RealTimeServer server = new RealTimeServer(new MudConfig());
            server.Run();
            while(true)
            {
                server.ProcessMessages(1f);
            }
            server.Stop();
        }
    }
}
