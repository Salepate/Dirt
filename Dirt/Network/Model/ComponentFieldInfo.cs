using System;

namespace Dirt.Network.Model
{
    /// <summary>
    /// Component serialization settings
    /// </summary>
    [Serializable]
    public struct ComponentFieldInfo
    {
        public int PoolIndex; 
        public int Component; // local index
        public int Accessor; 
        public bool Owner; // grants owner permission to write
        public string Debug;
    }
}