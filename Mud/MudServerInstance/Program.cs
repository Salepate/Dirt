using Mud.Server;
using System.Configuration;

namespace Mud.ServerInstance
{
    public class Program
    {
        static void Main(string[] args)
        {
            Dirt.Log.Console.Logger = new Dirt.Log.BasicLogger();
            int port = int.Parse(ConfigurationManager.AppSettings["ServerPort"]);
            int maxClient = int.Parse(ConfigurationManager.AppSettings["MaxClient"]);

            RealTimeServer server = new RealTimeServer(maxClient, port);
            server.Run();
            while(true)
            {
                server.ProcessMessages(1f);
            }

            server.Stop();
        }
    }
}
