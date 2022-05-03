namespace Dirt.GameServer.PlayerStore.Model
{
    [System.Serializable]
    public class SimpleID
    {
        public const uint DefaultValue = 10000;
        public uint Value;

        public SimpleID()
        {
            Value = DefaultValue;
        }

        public uint GetUnique()
        {
            return Value++;
        }
    }
}
