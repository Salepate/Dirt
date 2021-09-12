namespace Framework
{
    public struct SmallClock
    {
        private float m_Clock;

        public float Length { get; private set; }
        public float Progress { get { return m_Clock / Length; } }
        public bool Done {  get { return m_Clock >= Length; } }

        public void Reset()
        {
            m_Clock = 0f;
        }

        public SmallClock(float length)
        {
            Length = length;
            m_Clock = 0f;
        }

        public void Tick(float dt)
        {
            if (m_Clock < Length)
            {
                m_Clock += dt;

                if ( m_Clock > Length )
                {
                    m_Clock = Length;
                }
            }
        }
    }
}