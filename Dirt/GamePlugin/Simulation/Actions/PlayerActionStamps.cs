namespace Dirt.GameServer.Simulation.Actions
{
    /// <summary>
    /// track performed actions to allow multiple action in a same frame, but not the same action twice
    /// </summary>
    internal struct PlayerActionStamps
    {
        public ulong[] Stamps;
        public PlayerActionStamps(int actionCount)
        {
            Stamps = new ulong[actionCount];
        }

        public void Stamp(int action, ulong stamp)
        {
            Stamps[action] = stamp;
        }

        public bool HasStamp(int action, ulong stamp)
        {
            return Stamps[action] >= stamp;
        }
    }
}
