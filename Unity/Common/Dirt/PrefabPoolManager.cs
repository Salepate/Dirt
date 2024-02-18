using Dirt.Log;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

namespace Framework
{
    public class PrefabPoolManager
    {
        private GameObject m_Reference;
        private Stack<GameObject> m_Pool;
        private Transform m_Root;
        private bool m_Debug;

        private Vector3 m_BaseScale;

        private System.Action<GameObject> m_RuntimeInitializer;
        private bool m_HasInitializer;
        public PrefabPoolManager(GameObject prefab, Transform root, int initialCapacity = 10, bool debug = false)
        {
            m_Reference = prefab;
            m_Debug = debug;
            m_Pool = new Stack<GameObject>(initialCapacity);
            m_Root = root;
            m_BaseScale = prefab.transform.localScale;

            //Console.Message($"Creating pool {prefab.name} ({initialCapacity})");
            for (int i = 0; i < initialCapacity; ++i)
            {
                GameObject inst = Instantiate();
                Free(inst);
            }
        }

        public PrefabPoolManager(GameObject prefab, Transform root)
        {
            m_Reference = prefab;
            m_Debug = false;
            m_Root = root;
            m_BaseScale = prefab.transform.localScale;
        }

        public void InitializePool<T>(int capacity, System.Action<T> initializer) where T : Component
        {
            m_Pool = new Stack<GameObject>(capacity);
            m_HasInitializer = true;
            m_RuntimeInitializer = (obj) => initializer(obj.GetComponent<T>());
            //Console.Message($"Creating pool {m_Reference.name} ({capacity})");
            for (int i = 0; i < capacity; ++i)
            {
                GameObject inst = Instantiate();
                initializer(inst.GetComponent<T>());
                Free(inst);
            }
        }

        public void Free(GameObject obj)
        {
            obj.SetActive(false);
            //obj.transform.localScale = Vector3.zero;

#if UNITY_EDITOR
            obj.transform.SetParent(m_Root);
//                obj.hideFlags = HideFlags.HideInHierarchy;
#endif
            m_Pool.Push(obj);
        }

        public void DestroyPool()
        {
            while(m_Pool.Count > 0)
            {
                Object.Destroy(m_Pool.Pop());
            }
        }

        public GameObject Get()
        {
            GameObject inst;

            if ( m_Pool.Count > 0 )
            {
                inst = m_Pool.Pop();
            }
            else
            {
                inst = Instantiate();
                if ( m_HasInitializer )
                    m_RuntimeInitializer(inst);
            }

            inst.SetActive(true);
#if UNITY_EDITOR
//                inst.hideFlags = HideFlags.None;
#endif
            //inst.transform.localScale = m_BaseScale;
            return inst;
        }

        #region Internal
        private GameObject Instantiate()
        {
            GameObject obj = GameObject.Instantiate(m_Reference, m_Root);
            //if (!m_Debug)
            //    obj.hideFlags = HideFlags.HideAndDontSave;
            return obj;
        }
        #endregion
    }
}