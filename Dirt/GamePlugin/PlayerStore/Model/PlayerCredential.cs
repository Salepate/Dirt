namespace Dirt.GameServer.PlayerStore.Model
{
    [System.Serializable]
    public class PlayerCredential
    {
        public uint ID;
        public uint UserNumber; 
        public string UserName;
        public string PasswordHash;
        public bool Admin; // only editable through data

        public string Tag { get; internal set; }

        public PlayerCredential()
        {
            ID = 0;
            UserNumber = 0;
            UserName = string.Empty;
            PasswordHash = string.Empty;
            Admin = false;
            Tag = string.Empty;
        }
    }
}
