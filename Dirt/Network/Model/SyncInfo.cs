using System.Collections.Generic;

namespace Dirt.Network.Model
{
    [System.Serializable]
    public class SyncInfo
    {
        [System.Serializable]
        public class FieldSettings : Dictionary<string, string> { }

        public string[] SyncedComponents;
        public Dictionary<string, FieldSettings> Fields;

        public Dictionary<string, bool> OwnerAuthority;
        public SyncInfo()
        {
            Fields = new Dictionary<string, FieldSettings>();
            OwnerAuthority = new Dictionary<string, bool>();
            SyncedComponents = new string[0];
        }
    }
}