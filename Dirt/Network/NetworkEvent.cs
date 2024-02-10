namespace Dirt.Network
{   
    [System.Serializable]
    public abstract class NetworkEvent 
    {
        public bool Consumed { get; private set; }

        public NetworkEvent()
        {
            Consumed = false;
        }

        public void Consume()
        {
            Consumed = true;
        }
    }
}
