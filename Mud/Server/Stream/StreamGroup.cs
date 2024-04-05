using Mud.Server;
using System.Collections.Generic;
using System.IO;

namespace Mud.Server.Stream
{
    public class StreamGroup
    {
        public List<GameClient> Clients { get; private set; }

        public StreamGroup()
        {
            Clients = new List<GameClient>();
        }

        public bool RegisterClient(GameClient client)
        {
            if ( !Clients.Contains(client) )
            {
                Clients.Add(client);
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
