using Dirt.Log;
using System.Collections.Generic;
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

        public PrefabPoolManager(GameObject prefab, Transform root, int initialCapacity = 10, bool debug = false)
        {
            m_Reference = prefab;
            m_Debug = debug;
            m_Pool = new Stack<GameObject>(initialCapacity);
            m_Root = root;
            m_BaseScale = prefab.transform.localScale;

            Console.Message($"Creating pool {prefab.name} ({initialCapacity})");
            for (int i = 0; i < initialCapacity; ++i)
            {
                GameObject inst = Instantiate();
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