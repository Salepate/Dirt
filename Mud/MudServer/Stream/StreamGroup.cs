using Mud.Server;
using System.Collections.Generic;

namespace Mud.Server.Stream
{
    public class StreamGroup
    {
        public List<GameClient> Clients { get; private set; }

        private StreamGroupManager m_GroupManager;

        internal StreamGroup(StreamGroupManager groupManager)
        {
            Clients = new List<GameClient>();
            m_GroupManager = groupManager;
        }

        public bool RegisterClient(GameClient client)
        {
            if ( !Clients.Contains(client) )
            {
                Clients.Add(client);
                m_GroupManager.ClientJoined?.Invoke(this, client);
                return true;
            }
            return false;
        }

        public bool UnregisterClient(GameClient client)
        {
            int clientIdx = Clients.IndexOf(client);
            if (clientIdx != -1)
            {
                Clients.RemoveAt(clientIdx);
                m_GroupManager.ClientLeft?.Invoke(this, client);
                return true;
            }
            return false;
        }

        public void Broadcast(MudMessage message, GameClient ignoreClient)
        {
            Clients.ForEach(client =>
            {
                if ( client != ignoreClient )
                    client.Send(message);
            });
        }

        public void Broadcast(MudMessage message, bool reliable)
        {
            Clients.ForEach(client =>
            {
                client.Send(message, reliable);
            });
        }
    }
}
