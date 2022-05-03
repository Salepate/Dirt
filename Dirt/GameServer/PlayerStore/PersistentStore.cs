﻿using Dirt.Log;
using Newtonsoft.Json;
using System.Configuration;
using System.IO;

namespace Dirt.GameServer.PlayerStore
{
    public class PersistentStore
    {
        private DirectoryInfo m_PersistentDirectory;
        public PersistentStore()
        {
            string persistentFolder = ConfigurationManager.AppSettings["PersistentRoot"];
            m_PersistentDirectory = new DirectoryInfo(persistentFolder);
            if ( !m_PersistentDirectory.Exists )
            {
                Console.Error($"Persistent directory not found: {persistentFolder}");
            }
        }
        public bool Exists(string key)
        {
            string fileName = $"{key}.json";
            FileInfo f = new FileInfo(Path.Combine(m_PersistentDirectory.FullName, fileName));
            return f.Exists;
        }

        public bool Write(string key, object data, bool overwrite = false)
        {
            if (!overwrite && Exists(key))
                return false;
            string fileName = $"{key}.json";
            string filePath = Path.Combine(m_PersistentDirectory.FullName, fileName);
            File.WriteAllText(filePath, JsonConvert.SerializeObject(data));
            return true;
        }

        public bool TryRead<T>(string key, out T data)
        {
            data = default;
            if ( Exists(key))
            {
                string fileName = $"{key}.json";
                string filePath = Path.Combine(m_PersistentDirectory.FullName, fileName);
                data = JsonConvert.DeserializeObject<T>(File.ReadAllText(filePath));
                return true;
            }
            return false;
        }
    }
}
