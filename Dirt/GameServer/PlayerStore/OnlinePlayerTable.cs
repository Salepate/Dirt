using Dirt.GameServer.PlayerStore.Model;
using System.Collections.Generic;

namespace Dirt.GameServer.PlayerStore
{
    public class OnlinePlayerTable
    {
        private Dictionary<int, int> m_Session;
        private Dictionary<int, PlayerCredential> m_Table;

        public OnlinePlayerTable()
        {
            m_Table = new Dictionary<int, PlayerCredential>();
            m_Session = new Dictionary<int, int>();
        }

        public void SetSessionNumber(int clientNumber, int sessionNumber)
        {
            m_Session[sessionNumber] = clientNumber;
        }

        public bool TryGetPlayerNumber(int sessionNumber, out int playerNumber)
        {
            return m_Session.TryGetValue(sessionNumber, out playerNumber);
        }
        public void RemoveSession(int sessionNumber)
        {
            m_Session.Remove(sessionNumber);
        }

        public bool HasCredentials(int number) => m_Table.ContainsKey(number);

        public void SetCredentials(int number, PlayerCredential creds)
        {
            m_Table[number] = creds;
        }

        public void RemoveCredentials(int number)
        {
            m_Table.Remove(number);
        }

        public bool TryGetCredentials(int number, out PlayerCredential creds)
        {
            return m_Table.TryGetValue(number, out creds);
        }
    }
}
