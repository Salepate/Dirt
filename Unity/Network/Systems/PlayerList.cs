using Dirt.Game.Model;
using Dirt.Network.Events;
using System.Collections.Generic;

namespace Dirt.Systems
{
    public class PlayerList : DirtSystem
    {
        public System.Action<GamePlayer> PlayerRenameAction;

        private Dictionary<int, GamePlayer> m_PlayerMap;
        public override void Initialize(DirtMode mode)
        {
            m_PlayerMap = new Dictionary<int, GamePlayer>();
            var dispatcher = mode.FindSystem<NetworkEventDispatcher>();
            dispatcher.Listen<PlayerConnectionEvent>(PlayerEvent);
            dispatcher.Listen<PlayerListEvent>(PlayerListEvent);
            dispatcher.Listen<PlayerRenameEvent>(PlayerRenameEvent);
        }

        public bool TryGetPlayer(int playerNumber, out GamePlayer player)
        {
            return m_PlayerMap.TryGetValue(playerNumber, out player);
        }

        private void PlayerEvent(PlayerConnectionEvent pEvent)
        {
            m_PlayerMap[pEvent.Player.Number] = pEvent.Player;
        }

        private void PlayerRenameEvent(PlayerRenameEvent pEvent)
        {
            GamePlayer player = m_PlayerMap[pEvent.Number];
            player.Name = pEvent.NewName;
            PlayerRenameAction?.Invoke(player);
        }

        private void PlayerListEvent(PlayerListEvent pListEvent)
        {
            for (int i = 0; i < pListEvent.Players.Length; ++i)
            {
                GamePlayer player = pListEvent.Players[i];
                m_PlayerMap[player.Number] = player;
            }
        }
    }
}