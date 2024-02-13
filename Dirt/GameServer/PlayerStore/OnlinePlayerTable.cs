using Dirt.GameServer.PlayerStore.Model;
using Dirt.Log;
using System.Collections.Generic;

namespace Dirt.GameServer.PlayerStore
{
    public class OnlinePlayerTable
    {
        private Dictionary<int, int> m_Session;
        private Dictionary<int, int> m_ClientToSession;
        private Dictionary<int, PlayerCredential> m_Table;
        private Dictionary<uint, int> m_OnlineUsers;

        public OnlinePlayerTable()
        {
            m_Table = new Dictionary<int, PlayerCredential>();
            m_Session = new Dictionary<int, int>();
            m_ClientToSession = new Dictionary<int, int>();
            m_OnlineUsers = new Dictionary<uint, int>();
        }

        public void SetSessionNumber(int clientNumber, int sessionNumber)
        {
            m_Session[sessionNumber] = clientNumber;
            m_ClientToSession[clientNumber] = sessionNumber;
        }

        public bool TryGetPlayerNumber(uint userID, out int playerNumber)
        {
            return m_OnlineUsers.TryGetValue(userID, out playerNumber);
        }

        public bool TryGetPlayerNumber(int sessionNumber, out int playerNumber)
        {
            return m_Session.TryGetValue(sessionNumber, out playerNumber);
        }
        public void RemoveSession(int sessionNumber)
        {
            if (m_Session.TryGetValue(sessionNumber, out int clientNumber))
            {
                m_ClientToSession.Remove(clientNumber);
                m_Session.Remove(sessionNumber);
            }
        }

        public bool HasCredentials(uint userID) => m_OnlineUsers.ContainsKey(userID);

        public void SetCredentials(int number, PlayerCredential creds)
        {
            m_Table[number] = creds;
            m_OnlineUsers[creds.ID] = number;
        }

        public void RemoveCredentials(int number)
        {
            if (m_Table.TryGetValue(number, out PlayerCredential creds))
            {
                m_OnlineUsers.Remove(creds.ID);
                m_Table.Remove(number);
            }
            else
            {
                Console.Warning($"Player {number} is not authed");
            }
        }

        public bool TryGetCredentials(int number, out PlayerCredential creds)
        {
            return m_Table.TryGetValue(number, out creds);
        }

        internal int GetSession(int number)
        {
            m_ClientToSession.TryGetValue(number, out int session);
            return session;
        }
    }
}
