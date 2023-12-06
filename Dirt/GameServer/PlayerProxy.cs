using Dirt.Game.Model;
using Mud.Server;
using Mud.Server.Stream;

namespace Dirt.GameServer
{
    public class PlayerProxy
    {
        public GameClient Client { get; private set; }
        public GamePlayer Player { get; private set; }

        public int Simulation { get; internal set; }
        public StreamGroup Group { get; internal set; }

        public PlayerProxy(GamePlayer player, GameClient client)
        {
            Player = player;
            Client = client;
            Simulation = -1;
        }
    }
}
