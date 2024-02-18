using Dirt.Network.Model;
using Dirt.Simulation;
using System;
using System.Collections.Generic;

namespace Dirt.Network.Simulation.Components
{
    [Serializable]
    public struct NetInfo : IComponent, IComponentAssign
    {
        public const int MaxID = 255; // used for commonly updated entities

        public int ID;
        public int Owner;
        public bool ServerControl;
        [DisableSync]
        public float SyncClock;
        [DisableSync]
        public ComponentFieldInfo[] Fields;
        [NonSerialized]
        public MessageHeader LastInBuffer;
        [NonSerialized]
        public byte[] LastOutBuffer;
        [NonSerialized]
        public int LastOutStamp;

        [NonSerialized]
        public MessageHeader LastState;
        [NonSerialized]
        public bool Owned;
        [NonSerialized]
        public int ServerControlTime;

        public void Assign()
        {
            ID = -1;
            Owned = true;
            Fields = new ComponentFieldInfo[0];
            ServerControlTime = 0;
        }
    }
}
