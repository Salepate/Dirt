using Dirt.Game;
using Mud.Server;

namespace Dirt.GameServer.Managers
{
    public class RealTimeServerManager : IGameManager
    {
        public RealTimeServer Server { get; private set; }

        public RealTimeServerManager(RealTimeServer server)
        {
            Server = server;
        }
        public void Update(float deltaTime) {}
    }
}
