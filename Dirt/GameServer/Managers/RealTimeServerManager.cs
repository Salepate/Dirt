using Dirt.Game;
using Mud.Server;

namespace Dirt.GameServer.Managers
{
    public class RealTimeServerManager : IGameManager
    {
        public RealTimeServer Server { get; private set; }

        public int NetTickrate { get; private set; }

        public RealTimeServerManager(RealTimeServer server, int netTickrate)
        {
            Server = server;
            NetTickrate = netTickrate;
        }
        public void Update(float deltaTime) {}
    }
}
