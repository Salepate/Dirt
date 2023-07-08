namespace Framework
{
    public struct SmallClock
    {
        public float Clock { get; private set; }
        public float Length { get; private set; }
        public float Progress { get { return Clock / Length; } }
        public bool Done {  get { return Clock >= Length; } }

        public void Reset()
        {
            Clock = 0f;
        }

        public SmallClock(float length)
        {
            Length = length;
            Clock = 0f;
        }

        public void Tick(float dt)
        {
            if (Clock < Length)
            {
                Clock += dt;

                if ( Clock > Length )
                {
                    Clock = Length;
                }
            }
        }
    }
}