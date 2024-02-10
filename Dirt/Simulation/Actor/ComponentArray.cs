using Dirt.Simulation.Exceptions;
using System;

namespace Dirt.Simulation.Actor
{
    public interface GenericArray 
    {
        System.Type ComponentType { get; }
        void SetSize(int size);
        int Allocate(int actorIndex);
        void Free(int actorIndex);
        object Get(int idx);
        void Set(int idx, object newValue);
    }
    public class ComponentArray<T> : GenericArray where T: struct
    {
        public int Allocated { get; private set; }
        public int NextIndex { get; private set; }
        public Type ComponentType => typeof(T);

        public int[] Actors;
        public T[] Components;
        public T Fallback;

        private delegate void ComponentAllocator(int idx);
        private ComponentAllocator m_Allocator;

        public ComponentArray()
        {
            Fallback = default;

            if ( typeof(T).GetInterface(nameof(IComponentAssign)) != null )
            {
                m_Allocator = AssignAllocate;
            }
            else
            {
                m_Allocator = DefaultAllocate;
            }
        }

        public void SetSize(int size)
        {
            NextIndex = 0;
            Allocated = 0;
            Components = new T[size];
            Actors = new int[size];
            for (int i = 0; i < size; ++i)
            {
                Actors[i] = -1;
            }
        }

        public int Allocate(int actorIndex)
        {
            if (NextIndex >= Actors.Length)
                throw new ComponentLimitException(typeof(T), Actors.Length);

            int idx = NextIndex++;
            while (NextIndex < Actors.Length && Actors[NextIndex] != -1)
                ++NextIndex;

            Actors[idx] = actorIndex;
            m_Allocator(idx);
            ++Allocated;
            return idx;
        }

        public void Free(int idx)
        {
            if ( Actors[idx] != -1 )
            {
                --Allocated;
                Actors[idx] = -1;

                if (idx < NextIndex)
                {
                    NextIndex = idx;
                }
            }
        }

        private void DefaultAllocate(int idx)
        {
            Components[idx] = new T();
        }
        private void AssignAllocate(int idx)
        {
            IComponentAssign assignBox = (IComponentAssign)(new T());
            assignBox.Assign();
            Components[idx] = (T)assignBox;
        }
        public object Get(int idx) => Components[idx];
        public void Set(int idx, object newValue)
        {
            T castObj = (T)newValue;
            Components[idx] = castObj;
        }
    }
}
