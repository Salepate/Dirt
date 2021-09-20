using Dirt.Network.Model;
using Dirt.Simulation;
using System;
using System.Collections.Generic;

namespace Dirt.Network.Simulation.Components
{
    [Serializable]
    public struct NetInfo : IComponent, IComponentAssign
    {
        public int ID;
        public int Owner;
        public List<ComponentFieldInfo> Fields;
        [NonSerialized]
        public MessageHeader LastInBuffer;
        [NonSerialized]
        public byte[] LastOutBuffer;
        [NonSerialized]
        public MessageHeader LastState;

        public void Assign()
        {
            ID = -1;
            Fields = new List<ComponentFieldInfo>();
        }
    }
}
