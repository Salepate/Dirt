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

        // New ECS
        public int[] Components;
        public Type[] ComponentTypes { get { return m_Types; } }
        // Old ECS
        //public IComponent[] Components;
        public int ComponentCount { get; private set; }

        [NonSerialized]
        private Type[] m_Types;

        internal GameActor(int id)
        {
            InternalID = id;
            ComponentCount = 0;
            m_Types = new Type[MaxComponents];
            Components = new int[MaxComponents];
            for (int i = 0; i < MaxComponents; ++i)
                Components[i] = -1;
        }

        internal void SetVersion(int version)
        {
            Version = version;
        }

        public void AssignComponent<C>(int index)
        {
            if (ComponentCount >= MaxComponents)
                throw new Exception($"Component limit exceeded {MaxComponents}");

            int idx = GetFreeSlot();
            Components[idx] = index;
            ComponentTypes[idx] = typeof(C);
            ComponentCount++;
        }

        public void AssignComponent(Type compType, int index)
        {
            if (ComponentCount >= MaxComponents)
                throw new Exception($"Component limit exceeded {MaxComponents}");

            int idx = GetFreeSlot();
            Components[idx] = index;
            ComponentTypes[idx] = compType;
            ComponentCount++;
        }

        public void UnassignComponent<C>()
        {
            int compIndex = GetComponentLocalIndex<C>();
            if (compIndex != -1)
            {
                ComponentTypes[compIndex] = null;
                Components[compIndex] = -1;
                ComponentCount--;
            }
        }

        public void UnassignComponent(Type compType)
        {
            int compIndex = GetComponentLocalIndex(compType);
            if ( compIndex != -1 )
            {
                ComponentTypes[compIndex] = null;
                Components[compIndex] = -1;
                ComponentCount--;
            }
        }

        public int GetComponentIndex(Type compType)
        {
            for (int i = 0; i < ComponentCount; ++i)
            {
                if (ComponentTypes[i] == compType)
                    return Components[i];
            }
            return -1;
        }

        /// <summary>
        /// Returns the index in ComponentArray
        /// </summary>
        /// <typeparam name="C"></typeparam>
        /// <returns></returns>
        public int GetComponentIndex<C>()
        {
            for (int i = 0; i < ComponentCount; ++i)
            {
                if (ComponentTypes[i] == typeof(C))
                    return Components[i];
            }
            return -1;
        }

        public int GetComponentLocalIndex(Type compType)
        {
            for (int i = 0; i < ComponentCount; ++i)
            {
                if (ComponentTypes[i] == compType)
                    return i;
            }
            return -1;
        }

        private int GetComponentLocalIndex<C>()
        {
            for (int i = 0; i < ComponentCount; ++i)
            {
                if (ComponentTypes[i] == typeof(C))
                    return i;
            }
            return -1;
        }

        public void ResetActor()
        {
            for(int i = 0; i < ComponentCount; ++i)
            {
                Components[i] = -1;
                m_Types[i] = null;
            }
            ComponentCount = 0;
        }

        // helpers
        private int GetFreeSlot()
        {
            for (int i = 0; i < ComponentCount; ++i)
            {
                if (Components[i] == -1)
                    return i;
            }
            return ComponentCount;
        }
    }
}
