using System;

namespace Dirt.Network.Model
{
    [Serializable]
    public struct ComponentFieldInfo
    {
        public int Component;
        public int Accessor;
        public bool Owner;
    }
}