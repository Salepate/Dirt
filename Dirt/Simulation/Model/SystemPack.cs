namespace Dirt.Simulation.Model
{
    [System.Serializable]
    public class SystemPack
    {
        public string[] Shared;
        public string[] Client;
        public string[] Server;

        public SystemPack()
        {
            Shared = new string[0];
            Client = new string[0];
            Server = new string[0];
        }
    }
}
