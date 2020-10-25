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

        public SyncInfo()
        {
            Fields = new Dictionary<string, FieldSettings>();
            SyncedComponents = new string[0];
        }
    }
}