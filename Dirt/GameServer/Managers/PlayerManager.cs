using Dirt.Game;
using Dirt.Game.Model;
using Dirt.Network;
using Dirt.Network.Managers;
using Mud;
using Mud.Server;
using Mud.Server.Stream;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Console = Dirt.Log.Console;

namespace Dirt.GameServer.Managers
{
    public class PlayerManager : IGameManager
    {

        private Dictionary<int, PlayerProxy> m_PlayerMap;

        public IEnumerable<GamePlayer> ActivePlayers => m_PlayerMap.Values.Select(p => p.Player);

        private NetworkSerializer m_NetSerializer;
        public PlayerManager(NetworkSerializer serializer)
        {
            m_NetSerializer = serializer;
            m_PlayerMap = new Dictionary<int, PlayerProxy>();
        }


        public PlayerProxy CreatePlayer(GameClient client)
        {
            Console.Assert(!m_PlayerMap.ContainsKey(client.Number), $"Player already set for client {client.ID}");
            GamePlayer player = new GamePlayer();
            player.Name = client.ID;
            player.Number = client.Number;
            return LinkPlayer(player, client);
        }

        internal PlayerProxy LinkPlayer(GamePlayer player, GameClient client)
        {
            Console.Assert(player.Number == client.Number, $"Player Number mismatch {player.Number}/{client.Number}");
            PlayerProxy proxy = new PlayerProxy(player, client);
            m_PlayerMap.Add(client.Number, proxy);
            return proxy;
        }

        public void RemovePlayer(GamePlayer player)
        {
            if ( m_PlayerMap.ContainsKey(player.Number))
            {
                m_PlayerMap.Remove(player.Number);
            }
        }

        public void SendEvent<T>(T gameEvent) where T : NetworkEvent
        {
            byte[] eventBuffer;
            using (MemoryStream st = new MemoryStream())
            {
                m_NetSerializer.Serialize(st, gameEvent);
                eventBuffer = st.ToArray();
            }

            MudMessage message = MudMessage.Create((int)NetworkOperation.GameEvent, eventBuffer);
            foreach(KeyValuePair<int, PlayerProxy> kvp in m_PlayerMap)
            {
                kvp.Value.Client.Send(message);
            }
        }

        public void SendEventTo<T>(T gameEvent, StreamGroup group) where T: NetworkEvent
        {
            byte[] eventBuffer;
            using (MemoryStream st = new MemoryStream())
            {
                m_NetSerializer.Serialize(st, gameEvent);
                eventBuffer = st.ToArray();
            }
            MudMessage message = MudMessage.Create((int)NetworkOperation.GameEvent, eventBuffer);
            group.Broadcast(message);
        }

        public void SendEventTo<T>(GamePlayer player, T gameEvent) where T : NetworkEvent
        {
            if (m_PlayerMap.TryGetValue(player.Number, out PlayerProxy proxy))
            {
                GameClient client = proxy.Client;
                byte[] eventBuffer;
                using (MemoryStream st = new MemoryStream())
                {
                    m_NetSerializer.Serialize(st, gameEvent);
                    eventBuffer = st.ToArray();
                }
                MudMessage message = MudMessage.Create((int)NetworkOperation.GameEvent, eventBuffer);
                client.Send(message);
            }
        }

        public PlayerProxy FindPlayer(int playerNumber)
        {
            if ( m_PlayerMap.TryGetValue(playerNumber, out PlayerProxy proxy) )
            {
                return proxy;
            }

            return null;
        }

        public PlayerProxy FindPlayer(string playerID)
        {
            return m_PlayerMap.Where(v => v.Value.Player.Name == playerID).Select(v => v.Value).FirstOrDefault();
        }

        public void RemoveClient(GameClient client)
        {
            int id = client.Number;
            if (m_PlayerMap.ContainsKey(client.Number))
                m_PlayerMap.Remove(client.Number);
        }

        public void Update(float deltaTime)
        {
        }
    }
}
