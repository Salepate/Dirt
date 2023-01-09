namespace Mud
{
    /// <summary>
    /// An array that has a limited size and loops over when adding more elements than its capacity.
    /// the first index, will always be oldest entry.
    /// </summary>
    /// <typeparam name="T">contained type of data</typeparam>
    public class CircularBuffer<T>
    {
        public int Count { get; private set; }
        public readonly int Capacity;
        private T[] m_Buffer;
        private int m_Offset;
        private int m_Start = 0;

        /// <param name="bufferSize">buffer maximum size</param>
        public CircularBuffer(int bufferSize)
        {
            m_Buffer = new T[bufferSize];
            Capacity = bufferSize;
            m_Offset = 0;
        }
        /// <summary>
        /// Append an element to the buffer, overwriting if the buffer is full.
        /// </summary>
        /// <param name="value">value to append</param>
        public void Add(T value)
        {
            m_Buffer[m_Offset] = value;

            if (Count < Capacity)
            {
                ++Count;
            }
            else
            {
                m_Start = (m_Start + 1) % Capacity;
            }

            m_Offset = (m_Offset + 1) % Capacity;
        }

        /// <summary>
        /// Reads an element from the buffer
        /// </summary>
        /// <param name="key">index of the element to read</param>
        /// <returns>element at index</returns>
        public T this[int key]
        {
            get 
            {
                if (key < 0 || key >= Count)
                    throw new System.Exception("index out of range");
                return m_Buffer[(m_Start + key) % Capacity];
            }
        }
        /// <summary>
        /// Retrieves an element index using System.Array.IndexOf
        /// </summary>
        /// <param name="value">the element to look for</param>
        /// <returns>index of given element, -1 if element was not found</returns>
        public int IndexOf(T value)
        {
            int base_index = System.Array.IndexOf(m_Buffer, value);

            if (base_index == -1)
                return -1;

            if (base_index < m_Start)
            {
                base_index += Capacity;
            }

            return base_index - m_Start;
        }
    }
}
