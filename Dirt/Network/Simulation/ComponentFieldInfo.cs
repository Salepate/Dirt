using System;

namespace Dirt.Network.Simulation
{
    [Serializable]
    public struct ComponentFieldInfo
    {
        public int Component;
        public int Accessor;
        public bool Owner;
    }
}