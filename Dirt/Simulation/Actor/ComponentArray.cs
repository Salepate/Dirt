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
        public delegate T AssignComponentDelegate(T comp);

        public AssignComponentDelegate AssignComponent;
        public int NextIndex { get; private set; }

        public Type ComponentType => typeof(T);

        public int[] Actors;
        public T[] Components;
        public T Fallback;

        public ComponentArray()
        {
            Fallback = default;

            if ( typeof(T).GetInterface(nameof(IComponentAssign)) != null )
            {
                AssignComponent = AssignableComponent;
            }
        }

        public void SetSize(int size)
        {
            NextIndex = 0;
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
            Components[idx] = new T();
            if (AssignComponent != null)
            {
                Components[idx] = AssignComponent(new T());
            }
            else
            {
                Components[idx] = new T();
            }
            return idx;
        }

        public void Free(int idx)
        {
            if ( Actors[idx] != -1 )
            {
                Actors[idx] = -1;

                if (idx < NextIndex)
                {
                    NextIndex = idx;
                }
            }
        }

        private T AssignableComponent(T comp)
        {
            IComponentAssign assignComp = (IComponentAssign)comp;
            assignComp.Assign();
            return (T) assignComp;
        }

        public object Get(int idx) => Components[idx];

        public void Set(int idx, object newValue)
        {
            T castObj = (T)newValue;
            Components[idx] = castObj;
        }
    }
}
