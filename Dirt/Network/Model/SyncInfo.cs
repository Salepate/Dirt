using System.Collections.Generic;

namespace Dirt.Network.Model
{
    [System.Serializable]
    public class SyncInfo
    {
        public string[] SyncedComponents;

        public Dictionary<string, bool> OwnerAuthority;
        public SyncInfo()
        {
            OwnerAuthority = new Dictionary<string, bool>();
            SyncedComponents = new string[0];
        }
    }
}