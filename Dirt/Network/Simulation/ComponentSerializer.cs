namespace Dirt.Network.Simulation
{
    [System.Serializable]
    public struct ComponentSerializer
    {
        internal bool UseNetSerializer;
        internal bool IsPosition;
        internal bool AuthoredByOwner;
        public int PoolIndex { get; set; }
        public int ComponentIndex { get; set; }

        public int LastIndexInBuffer { get; set; }
    }
}
