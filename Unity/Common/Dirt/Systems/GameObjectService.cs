using Dirt.Log;
using Framework;
using System.Collections.Generic;
using UnityEngine;

namespace Dirt.Systems
{
    //TODO add pool destroy
    public class GameObjectService : DirtSystem
    {
        public override bool HasUpdate => true;
        public override bool HasFixedUpdate => true;
        public override bool HasLateUpdate => true;
        private List<GameObject> m_Objects;
        private PoolManager m_PoolManager;
        public override void Initialize(DirtMode mode)
        {
            m_PoolManager = new PoolManager(true);

            GameObject pooledObjectRoot = new GameObject("PooledObjects");
            m_Objects = new List<GameObject>();
        }

        public void MakePool(GameObject prefab, Transform parent, int baseCapacity)
        {
            m_PoolManager.InitializePool(prefab, parent, baseCapacity);
        }

        public void ReleasePool(GameObject prefab)
        {
            m_PoolManager.DestroyPool(prefab);
        }

        public GameObject Instantiate(GameObject prefab, Transform parent = null)
        {
            GameObject instance = m_PoolManager.Get(prefab, parent);
            m_Objects.Add(instance);
            return instance;
        }

        public T Instantiate<T>(GameObject prefab, Transform parent = null) where T : Component
        {
            GameObject instance = m_PoolManager.Get(prefab, parent);
            T actor = instance.GetComponent<T>();
            Console.Assert(actor != null, $"Invalid prefab, no {typeof(T).Name} component found");
            return actor;
        }

        public void FreeActor(GameObject obj)
        {
            m_PoolManager.Free(obj);
        }
    }
}