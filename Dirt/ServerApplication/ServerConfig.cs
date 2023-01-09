using Mud.Server;
using System.Configuration;
using Console = Dirt.Log.Console;

namespace Dirt.ServerApplication
{
    public class ServerConfig : IConfigurationReader
    {
        public int GetInt(string name)
        {
            bool exist = int.TryParse(ConfigurationManager.AppSettings[name], out int val);
            if (!exist)
            {
                Console.Message($"Unable to get {name} configuration value");
            }
            return val;
        }


        public string GetString(string name)
        {
            return ConfigurationManager.AppSettings[name];
        }
    }
}