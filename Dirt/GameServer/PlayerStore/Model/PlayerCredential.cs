namespace Dirt.GameServer.PlayerStore.Model
{
    [System.Serializable]
    public class PlayerCredential
    {
        public uint ID;
        public uint UserNumber; 
        public string UserName;
        public string PasswordHash;
    }
}
