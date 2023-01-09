using Dirt.Game.Model;

namespace Dirt.Network.Events
{
    [System.Serializable]
    public class PlayerConnectionEvent : NetworkEvent
    {
        public GamePlayer Player;
    }

    [System.Serializable]
    public class PlayerRenameEvent : NetworkEvent
    {
        public int Number;
        public string NewName;

        public PlayerRenameEvent(int playerNumber, string playerName)
        {
            Number = playerNumber;
            NewName = playerName;
        }
    }

    [System.Serializable]
    public class PlayerListEvent : NetworkEvent
    {
        public GamePlayer[] Players;
    }
}
