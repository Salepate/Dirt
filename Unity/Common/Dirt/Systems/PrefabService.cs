using Arena.Contents;
using Dirt.Log;
using System.Collections.Generic;
using UnityEngine;

namespace Dirt.Systems
{
    public class PrefabService : DirtSystem, IContentSystem
    {
        public const string PrefabDatabaseAsset = "gameprefabs";

        private Dictionary<string, GameObject> m_PrefabMap;
        private Dictionary<System.Type, List<string>> m_PrefabFilters;
        private static readonly string[] s_EmptyArray = new string[0];

        [BaseContent(PrefabDatabaseAsset)]
        private PrefabDatabase m_PrefabDB = null;

        public PrefabService()
        {
            m_PrefabMap = new Dictionary<string, GameObject>();
            m_PrefabFilters = new Dictionary<System.Type, List<string>>();
        }

        public override void InitializeContent()
        {
            CachePrefabs(m_PrefabDB.Prefabs);
        }

        public bool TryGetPrefab(string name, out GameObject prefab)
        {
            return m_PrefabMap.TryGetValue(name, out prefab);
        }

        public bool TryGetPrefab<T>(string name, out T comp) where T : Component
        {
            if (m_PrefabMap.TryGetValue(name, out GameObject prefab))
            {
                comp = prefab.GetComponent<T>();
                return true;

            }
            else
            {
                comp = null;
                return false;
            }
        }

        private void CachePrefabs(GameObject[] prefabs)
        {
            m_PrefabFilters.Clear();

            int addedPrefabs = 0;

            for (int i = 0; i < prefabs.Length; ++i)
            {
                GameObject prefab = prefabs[i];
                if (m_PrefabMap.ContainsKey(prefab.name))
                {
                    Console.Warning($"a prefab named {prefab.name} has already been detected, skipping this one");
                }
                else
                {
                    m_PrefabMap.Add(prefab.name, prefab);
                    ++addedPrefabs;
                }
            }
        }
    }
}
