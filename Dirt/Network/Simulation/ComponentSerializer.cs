namespace Dirt.Network.Simulation
{
    [System.Serializable]
    internal struct ComponentSerializer
    {
        internal bool UseNetSerializer;
        internal bool IsPosition;

        public int PoolIndex { get; set; }
        public int ComponentIndex { get; set; }
    }
}
