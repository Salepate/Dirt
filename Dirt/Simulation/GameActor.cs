using System;

namespace Dirt.Simulation
{
    [Serializable]
    public class GameActor
    {
        public const int MaxComponents = 12;
        public int ID => InternalID << 8 | Version;
        [NonSerialized]
        public int InternalID = 0;
        [NonSerialized]
        public int Version = 0;

        public System.Type[] ComponentTypes { get { return m_Types; } }
        public IComponent[] Components;

        public int ComponentCount { get; private set; }

        [NonSerialized]
        private Type[] m_Types;

        internal GameActor(int id)
        {
            InternalID = id;
            ComponentCount = 0;
            Components = new IComponent[MaxComponents];
            m_Types = new Type[MaxComponents];
        }

        internal void SetVersion(int version)
        {
            Version = version;
        }

        public void AddComponent<T>(T comp) where T: IComponent
        {
            if (ComponentCount >= MaxComponents)
                throw new Exception($"Component limit exceeded {MaxComponents}");

            int idx = GetFreeSlot();
            Components[idx] = comp;
            ComponentTypes[idx] = typeof(T);
            ComponentCount++;
        }

        public void AddComponent(IComponent comp)
        {
            if (ComponentCount >= MaxComponents)
                throw new System.Exception($"Component limit exceeded {MaxComponents}");

            int idx = GetFreeSlot();
            Components[idx] = comp;
            ComponentTypes[idx] = comp.GetType();
            ComponentCount++;
        }

        public int GetComponentIndex<T>() where T:IComponent
        {
            for (int i = 0; i < ComponentCount; ++i)
            {
                if (ComponentTypes[i] == typeof(T))
                    return i;
            }
            return -1;
        }
        public T GetComponent<T>() where T: IComponent
        {
            for(int i = 0; i < ComponentCount; ++i)
            {
                if (ComponentTypes[i] == typeof(T))
                    return (T)Components[i];
            }
            throw new System.Exception($"Component is missing {typeof(T).Name}");
        }

        public void SetComponent(IComponent component, int idx)
        {
            Components[idx] = component;
            m_Types[idx] = component.GetType();
        }

        public void CacheActor()
        {
            int compCount = 0;

            if (m_Types == null)
                m_Types = new Type[MaxComponents];

            for(int i = 0; i < MaxComponents; ++i)
            {
                if ( Components[i] != null )
                {
                    ++compCount;
                    m_Types[i] = Components[i].GetType();
                }
            }
            ComponentCount = compCount;
        }

        public void RemoveComponent<T>() where T: IComponent
        {
            int idx = GetComponentIndex<T>();
            if ( idx != -1 )
            {
                Components[idx] = null;
                m_Types[idx] = null;
            }
        }
        public void ResetActor()
        {
            for(int i = 0; i < ComponentCount; ++i)
            {
                Components[i] = null;
                m_Types[i] = null;
            }
            ComponentCount = 0;
        }

        // helpers
        private int GetFreeSlot()
        {
            for(int i = 0; i < ComponentCount; ++i)
            {
                if (Components[i] == null)
                    return i;
            }
            return ComponentCount;
        }
    }
}
