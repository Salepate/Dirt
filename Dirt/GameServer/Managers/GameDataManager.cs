using Dirt.Game;
using Newtonsoft.Json;
using System.IO;

namespace Dirt.GameServer.Managers
{
    public class GameDataManager : IGameManager
    {
        private const string PlayerSave = "players";

        private DirectoryInfo m_DataRoot;
        private DirectoryInfo m_PlayerSavePath;

        public GameDataManager(string dataPath)
        {
            m_DataRoot = new DirectoryInfo(dataPath);
            if (!m_DataRoot.Exists)
            {
                Log.Console.Message($"Creating game data folder {m_DataRoot.FullName}");
                m_DataRoot.Create();
                m_PlayerSavePath = m_DataRoot.CreateSubdirectory(PlayerSave);
            }
            else
            {
                m_PlayerSavePath = new DirectoryInfo(Path.Combine(dataPath, PlayerSave));
                if (!m_PlayerSavePath.Exists)
                {
                    Log.Console.Message($"Player folder missing, recreating");
                    m_PlayerSavePath.Create();
                }
            }
        }

        public bool TryLoadPlayerData<T>(string uid, out T data)
        {
            data = default;
            FileInfo file = new FileInfo(Path.Combine(m_PlayerSavePath.FullName, $"{uid}.json"));

            if (!file.Exists)
            {
                return false;
            }

            try
            {
                string fileData = File.ReadAllText(file.FullName);
                data = JsonConvert.DeserializeObject<T>(fileData);
            }

            catch (System.Exception e)
            {
                Log.Console.Error($"Unable to read player {uid} save");
                Log.Console.Message(e.Message);
            }

            return true;
        }

        public void SavePlayerData<T>(string uid, in T data)
        {
            try
            {
                string dataSerialized = JsonConvert.SerializeObject(data);
                File.WriteAllText(Path.Combine(m_PlayerSavePath.FullName, $"{uid}.json"), dataSerialized);
            }
            catch (System.Exception e)
            {
                Log.Console.Error($"Unable to save player {uid} data");
                Log.Console.Message(e.Message);
            }

        }

        public void Update(float deltaTime)
        {
        }
    }
}
