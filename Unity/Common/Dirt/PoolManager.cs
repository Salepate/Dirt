using Dirt.Log;
using System.Collections.Generic;
using UnityEngine;

namespace Framework
{
    public class PoolManager
    {
        private Dictionary<GameObject, GameObject> m_Instances;
        private Dictionary<GameObject, PrefabPoolManager> m_Pools;
        private bool m_Debug;

        public PoolManager(bool debug = false)
        {
            m_Debug = debug;
            m_Pools = new Dictionary<GameObject, PrefabPoolManager>();
            m_Instances = new Dictionary<GameObject, GameObject>();
        }

        public void DestroyPools()
        {
            foreach (GameObject inst in m_Instances.Keys)
                Object.Destroy(inst);

            foreach (PrefabPoolManager mgr in m_Pools.Values)
                mgr.DestroyPool();

            m_Instances.Clear();
            m_Pools.Clear();
        }

        public bool HasPool(GameObject prefab)
        {
            return m_Pools.ContainsKey(prefab);
        }

        /// <summary>
        /// destroy a specific pool
        /// note: Active objects are not destroyed
        /// </summary>
        /// <param name="prefab"></param>
        public void DestroyPool(GameObject prefab)
        {
            if ( m_Pools.TryGetValue(prefab, out PrefabPoolManager poolMgr))
            {
                poolMgr.DestroyPool();
                m_Pools.Remove(prefab);
            }
        }

        public PrefabPoolManager InitializePool(GameObject prefab, Transform instanceRoot = null , int initialCapacity = 1)
        {
            if (!m_Pools.ContainsKey(prefab))
            {
                PrefabPoolManager pool = new PrefabPoolManager(prefab, instanceRoot, initialCapacity, m_Debug);
                m_Pools.Add(prefab, pool);
            }

            return m_Pools[prefab];
        }

        public PrefabPoolManager InitializePool<T>(GameObject prefab, Transform instanceRoot, System.Action<T> initializer, int initialCapacity) where T : Component
        {
            if (!m_Pools.ContainsKey(prefab))
            {
                PrefabPoolManager pool = new PrefabPoolManager(prefab, instanceRoot);
                pool.InitializePool(initialCapacity, initializer);
                m_Pools.Add(prefab, pool);
            }

            return m_Pools[prefab];
        }

        public GameObject Get(GameObject prefab, Transform instanceRoot = null)
        {
            if ( !m_Pools.ContainsKey(prefab))
            {
                Console.Warning($"Lazy Pool initialization {prefab.name}");
                InitializePool(prefab, instanceRoot);
            }

            GameObject clone = m_Pools[prefab].Get();

            if (!m_Debug)
                clone.hideFlags = HideFlags.DontSave;

            if (!m_Instances.ContainsKey(clone))
                m_Instances.Add(clone, prefab);

            if (instanceRoot != null)
                clone.transform.SetParent(instanceRoot, false);

            return clone;
        }

        public T Get<T>(GameObject prefab, Transform parent = null) where T : Component
        {
            GameObject g = Get(prefab, parent);
            return g.GetComponent<T>();
        }

        public void Free(GameObject inst)
        {
            m_Pools[m_Instances[inst]].Free(inst);
        }
    }
}