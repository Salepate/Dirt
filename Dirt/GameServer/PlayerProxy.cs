using Dirt.Game.Model;
using Mud.Server;

namespace Dirt.GameServer
{
    public class PlayerProxy
    {
        public GameClient Client { get; private set; }
        public GamePlayer Player { get; private set; }

        public int Simulation { get; set; }

        public PlayerProxy(GamePlayer player, GameClient client)
        {
            Player = player;
            Client = client;
        }
    }
}
