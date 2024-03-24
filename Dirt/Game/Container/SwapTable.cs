namespace Dirt.Game.Container
{
    public class SwapTable<T>
    {
        public T[] m_Table;
        public int Count { get; private set; }
        public SwapTable(int size)
        {
            m_Table = new T[size];
            Count = 0;
        }

        // overload the [] operator
        public ref T this[int index]
        {
            get
            {
                if (index < 0 || index >= Count)
                    throw new System.Exception("index out of range");
                return ref m_Table[index];
            }
        }

        public void Add(in T value)
        {
            if (Count < m_Table.Length)
            {
                m_Table[Count] = value;
                ++Count;
            }
        }

        public void RemoveAt(int index)
        {
            if (Count > 0 && index >= 0 && index < Count)
            {
                if (index != Count - 1)
                    m_Table[index] = m_Table[Count - 1];
                --Count;
            }
        }
    }
}
