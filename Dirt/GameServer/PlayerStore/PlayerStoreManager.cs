using Dirt.Game;
using Dirt.Game.Math;
using Dirt.GameServer.Managers;
using Dirt.GameServer.PlayerStore.Helpers;
using Dirt.GameServer.PlayerStore.Model;
using Dirt.Log;
using Dirt.Network;
using Mud;
using Mud.Server;
using System.Security.Cryptography;
using System.Text;

namespace Dirt.GameServer.PlayerStore
{
    using BitConverter = System.BitConverter;
    public class PlayerStoreManager : IGameManager
    {
        public const string DataSep = "data";
        public const string SimpleIDFile = "_id";
        private const int GenerationAttempts = 20;
        private RNG m_IDGenerator;
        private HashAlgorithm m_HashAlgorithm;

        private SimpleID m_UniqueID;
        private GameInstance m_Game;
        private RealTimeServerManager m_RTServer;

        public bool AllowPlayerReconnect { get; set; }
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
        /// Get the player tag number
        /// </summary>
        /// <param name="playerIndex">Player Index</param>
        /// <param name="number">(out) player tag number (Tag#Number) </param>
        /// <returns>false if the player does not have an account</returns>
        public bool TryGetPlayerTagNumber(int playerIndex, out uint number)
        {
            number = 0;
            if (Table.TryGetCredentials(playerIndex, out PlayerCredential credential))
            {
                number = credential.UserNumber;
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


        public bool RenameUser(int playerNumber, string newUserName)
        {
            bool renamed = false;
            if (Table.TryGetCredentials(playerNumber, out PlayerCredential cred))
            {
                cred.UserName = newUserName;
                if(TryGetUserCredentialFile(cred.Tag, out string key))
                {
                    Store.Write(key, cred, true);
                    renamed = true;
                }   
            }
            return renamed;
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
        /// 
        internal bool DryAuth(int playerNumber, string playerTag, string hashedPassword)
        {
            if (!TryGetUserCredentialFile(playerTag, out string credFile))
            {
                return false;
            }

            if (Store.Exists(credFile))
            {
                PlayerCredential creds;
                if (Store.TryRead(credFile, out creds))
                {
                    return string.Compare(creds.PasswordHash, hashedPassword) == 0;
                }
            }
            return false;
        }
        public bool AttemptUserAuth(int playerNumber, string playerTag, string hashedPassword)
        {
            if (!TryGetUserCredentialFile(playerTag, out string credFile))
            {
                return false;
            }

            if (Store.Exists(credFile))
            {
                PlayerCredential creds;
                if (Store.TryRead(credFile, out creds))
                {
                    if (string.Compare(creds.PasswordHash, hashedPassword) == 0)
                    {
                        creds.Tag = playerTag;
                        // login
                        return AuthUser(playerNumber, creds);
                    }
                }
            }
            return false;
        }

        internal bool TryGetUniqueID(string playerTag, out uint uid)
        {
            uid = 0;

            if (!TryGetUserCredentialFile(playerTag, out string credFile))
            {
                return false;
            }
            if (Store.Exists(credFile) && Store.TryRead(credFile, out PlayerCredential creds))
            {
                uid = creds.ID;
                return true;
            }
            return false;
        }

        private bool TryGetUserCredentialFile(string playerTag, out string credentialName)
        {
            if (PlayerName.FromTag(playerTag, out string name, out uint id))
            {
                credentialName = $"{name}_{id}";
            }
            else
            {
                credentialName = string.Empty;
            }

            return !string.IsNullOrEmpty(credentialName);
        }

        internal void CreateSession(PlayerProxy proxy)
        {
            int sessID = m_IDGenerator.Range(100000000, 999999999);
            Console.Message($"Player {proxy.Player.Number} Session ID: {sessID}");
            Table.SetSessionNumber(proxy.Client.Number, sessID);
            proxy.Client.Send(MudMessage.Create((int)NetworkOperation.SetSession, BitConverter.GetBytes(sessID)));
        }

        public void ClearSession(PlayerProxy proxy)
        {
            int sessionID = Table.GetSession(proxy.Client.Number);
            Table.RemoveSession(sessionID);
            Table.RemoveCredentials(proxy.Client.Number);
        }

        private bool AuthUser(int playerNumber, PlayerCredential credential)
        {
            GameClient client = m_RTServer.Server.GetClient(playerNumber);
            if (!Table.HasCredentials(credential.ID) || AllowPlayerReconnect)
            {
                Console.Message($"Player {credential.UserName} authed");
                client.ChangeClientName(credential.UserName);
                Table.SetCredentials(playerNumber, credential);
                m_Game.AuthPlayer(playerNumber, credential);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Load a persistent data tied to a connected player
        /// </summary>
        /// <typeparam name="T">data type (explicit)</typeparam>
        /// <param name="playerNumber">Player index (in server)</param>
        /// <param name="key">unique data identifier</param>
        /// <param name="data">output data</param>
        /// <returns>false on failure, true otherwise</returns>
        public bool TryGetPlayerData<T>(int playerNumber, string key, out T data)
        {
            data = default(T);

            if (Table.TryGetCredentials(playerNumber, out PlayerCredential credential))
            {
                if (Store.TryRead(GetDataPath(key, credential), out data))
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Update persistent data tied to a connected/disconnecting player
        /// </summary>
        /// <typeparam name="T">data type (inferred)</typeparam>
        /// <param name="playerNumber">Player index (in server)</param>
        /// <param name="key">unique data identifier</param>
        /// <param name="data">data object to serialize</param>
        /// <returns>false on failure, true otherwise</returns>
        public bool UpdatePlayerData<T>(int playerNumber, string key, T data)
        {
            if (Table.TryGetCredentials(playerNumber, out PlayerCredential credential))
            {
                Store.Write(GetDataPath(key, credential), data, true);
                return true;
            }
            return false;
        }

        private static string GetDataPath(string key, PlayerCredential credential)
        {
            return $"{DataSep}.{credential.ID}.{key}";
        }
    }
}
