using Dirt.Game;
using Dirt.Game.Math;
using Dirt.GameServer.Managers;
using Dirt.GameServer.PlayerStore.Helpers;
using Dirt.GameServer.PlayerStore.Model;
using Dirt.Log;
using Mud.Server;
using System.Security.Cryptography;
using System.Text;

namespace Dirt.GameServer.PlayerStore
{
    public class PlayerStoreManager : IGameManager
    {
        public const string SimpleIDFile = "_id";
        private const int GenerationAttempts = 20;
        private RNG m_IDGenerator;
        private HashAlgorithm m_HashAlgorithm;

        private SimpleID m_UniqueID;
        private GameInstance m_Game;
        private RealTimeServerManager m_RTServer;

        public PersistentStore Store { get; private set; }
        public OnlinePlayerTable Table { get; private set; }
        public PlayerStoreManager(GameInstance game)
        {
            Table = new OnlinePlayerTable();
            m_Game = game;
            m_IDGenerator = new RNG();
            Store = new PersistentStore();
            m_HashAlgorithm = SHA256.Create();
            m_RTServer = game.GetManager<RealTimeServerManager>();
            if ( !Store.Exists(SimpleIDFile) || !Store.TryRead(SimpleIDFile, out m_UniqueID))
            {
                m_UniqueID = new SimpleID();
            }
        }
        public void Update(float deltaTime)
        {
        }

        /// <summary>
        /// Get the player unique ID
        /// </summary>
        /// <param name="playerNumber">Player Number</param>
        /// <param name="id">(out) unique player id</param>
        /// <returns>false if the player does not have an account</returns>
        public bool TryGetPlayerID(int playerNumber, out uint id)
        {
            id = 0;
            if (Table.TryGetCredentials(playerNumber, out PlayerCredential credential))
            {
                id = credential.ID;
                return true;
            }
            return false;
        }
        /// <summary>
        /// Attempt to generate a random Number that isnt currently used by any one
        /// May fail
        /// </summary>
        /// <param name="userName">user name that precedes the #</param>
        /// <param name="number">(out) number assigned in case of success</param>
        /// <returns>false if no id could be assigned</returns>
        public bool TryGetFreeID(string userName, out uint number)
        {
            int tryNb = GenerationAttempts;
            number = 0;
            while(tryNb-- > 0 )
            {
                int ranNumber = m_IDGenerator.Range(1000, 9999);
                string key = $"{userName}_{ranNumber}";
                if (!Store.Exists(key))
                {
                    number = (uint) ranNumber;
                    return true;
                }
            }
            
            return false;
        }
        /// <summary>
        /// Generate a SHA256 Hash 
        /// </summary>
        /// <param name="input">utf8 string to hash</param>
        /// <returns></returns>
        public string GetHash(string input)
        {
            byte[] data = m_HashAlgorithm.ComputeHash(Encoding.UTF8.GetBytes(input));
            StringBuilder str = new StringBuilder();
            for (int i = 0; i < data.Length; i++)
            {
                str.Append(data[i].ToString("x2"));
            }
            return str.ToString();
        }


        /// <summary>
        /// Register a player account
        /// </summary>
        /// <param name="userName">User Name (precedes the #)</param>
        /// <param name="passwordHash">SHA256 hash</param>
        /// <param name="number">(supercedes the #)</param>
        /// <returns></returns>
        public bool RegisterUser(string userName, string passwordHash, uint number)
        {
            PlayerCredential cred = new PlayerCredential()
            {
                ID = m_UniqueID.GetUnique(),
                PasswordHash = passwordHash,
                UserName = userName,
                UserNumber = number
            };
            string key = $"{userName}_{number}";

            if (Store.Write(key, cred, false))
            {
                Store.Write(SimpleIDFile, m_UniqueID, true);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Attempt to authenticate an user
        /// </summary>
        /// <param name="playerNumber">Player Number (see PlayerManager) that will be authed</param>
        /// <param name="playerTag">Player Account name tag (Saucisse#1234)</param>
        /// <param name="hashedPassword">SHA256 hash of the player password</param>
        /// <returns>false if the credentials are not valid. true if the player has been authed</returns>
        public bool AttemptUserAuth(int playerNumber, string playerTag, string hashedPassword)
        {
            string name;
            uint id;

            if (!PlayerName.FromTag(playerTag, out name, out id))
            {
                return false;
            }

            string key = $"{name}_{id}";
            if (Store.Exists(key))
            {
                PlayerCredential creds;
                if (Store.TryRead(key, out creds))
                {
                    if (string.Compare(creds.PasswordHash, hashedPassword) == 0)
                    {
                        // login
                        return AuthUser(playerNumber, creds);
                    }
                }
            }
            return false;
        }
        private bool AuthUser(int playerNumber, PlayerCredential credential)
        {
            GameClient client = m_RTServer.Server.GetClient(playerNumber);
            if ( !Table.HasCredentials(playerNumber))
            {
                Console.Message($"Player {credential.UserName} authed");
                client.ChangeClientName(credential.UserName);
                Table.SetCredentials(playerNumber, credential);
                return true;
            }
            return false;
        }
    }
}
