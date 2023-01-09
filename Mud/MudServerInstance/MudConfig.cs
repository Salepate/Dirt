using Dirt.Log;
using Mud.Server;
using System.Configuration;

namespace Mud.ServerInstance
{
    public class MudConfig : IConfigurationReader
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
