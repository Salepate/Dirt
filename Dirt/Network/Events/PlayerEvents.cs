using Dirt.Game.Model;

namespace Dirt.Network.Events
{
    [System.Serializable]
    public class PlayerConnectionEvent : NetworkEvent
    {
        public GamePlayer Player;
    }

    [System.Serializable]
    public class PlayerListEvent : NetworkEvent
    {
        public GamePlayer[] Players;
    }
}
