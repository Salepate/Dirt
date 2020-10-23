using Dirt.Network;
using Dirt.Simulation;
using System;
using System.Collections.Generic;

namespace Dirt.Network.Simulation.Components
{
    [Serializable]
    public class NetInfo : IComponent
    {
        public int ID = -1;
        public int Owner;
        [NonSerialized]
        public MessageHeader LastInBuffer;
        [NonSerialized]
        public byte[] LastOutBuffer;
        [NonSerialized]
        public MessageHeader LastState;

        public bool IsClient;

        public List<ComponentFieldInfo> Fields;

        public NetInfo()
        {
            Fields = new List<ComponentFieldInfo>();
        }
    }
}
