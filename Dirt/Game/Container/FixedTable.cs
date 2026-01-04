using System;

namespace Game.Container
{
    [System.Serializable]
    public class FixedTable<T> where T : struct
    {
        public int Allocated { get; private set; }
        public int NextIndex { get; private set; }
        public int Index { get; set; }
        public Type ComponentType => typeof(T);

        public bool[] Slots;
        public T[] Components;

        public delegate void ComponentAllocator(T[] table, int idx);
        private ComponentAllocator m_Allocator;

        public FixedTable(ComponentAllocator allocator = null)
        {
            if (allocator != null)
            {
                m_Allocator = allocator;
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
            Slots = new bool[size];
            for (int i = 0; i < size; ++i)
            {
                Slots[i] = false;
            }
        }

        public void Resize(int newSize)
        {
            if (newSize < Slots.Length)
                throw new System.Exception("Cannot reduce fixed table size");
            else if (newSize > Slots.Length)
            {
                var oldComps = Components;
                var oldSlots = Slots;
                Components = new T[newSize];
                Slots = new bool[newSize];
                Buffer.BlockCopy(oldSlots, 0, Slots, 0, oldSlots.Length);
                Buffer.BlockCopy(oldComps, 0, Components, 0, oldSlots.Length);
            }
        }

        public int Allocate()
        {
            if (NextIndex >= Slots.Length)
                throw new System.Exception($"Out of bound exception {NextIndex} > {Slots.Length}");

            int idx = NextIndex++;
            while (NextIndex < Slots.Length && Slots[NextIndex])
                ++NextIndex;

            m_Allocator(Components, idx);
            ++Allocated;
            return idx;
        }

        public void Free(int idx)
        {
            if (Slots[idx])
            {
                --Allocated;
                Slots[idx] = false;

                if (idx < NextIndex)
                {
                    NextIndex = idx;
                }
            }
        }

        private void DefaultAllocate(T[] table, int idx)
        {
            table[idx] = new T();
        }

        public T Get(int idx) => Components[idx];
        public void Set(int idx, T newValue)
        {
            Components[idx] = newValue;
        }
    }
}
