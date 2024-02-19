using Dirt.Network.Model;
using Dirt.Simulation;
using System;
using System.Collections.Generic;

namespace Dirt.Network.Simulation.Components
{
    [Serializable]
    public struct NetInfo : IComponent, IComponentAssign, INetComponent
    {
        public const int MaxID = 255; // used for commonly updated entities

        public int ID;
        public int Owner;
        public bool ServerControl;
        [DisableSync]
        public float SyncClock;
        [DisableSync]
        public ComponentFieldInfo[] Fields;
        public ComponentSerializer[] Serializers;
        public string[] Synced; // list of synced components

        [NonSerialized]
        public MessageHeader LastMessageBuffer;
        [NonSerialized]
        public byte[] LastInBuffer;
        [NonSerialized]
        public byte[] LastOutBuffer;
        [NonSerialized]
        public int BufferSize;
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

        public void Serialize(NetworkStream stream, bool client)
        {
            if (!client)
            {
                stream.Write(ServerControl);
            }
        }

        public void Deserialize(NetworkStream stream, bool client, bool author)
        {
            if (client)
            {
                ServerControl = stream.ReadBool();
            }
        }
    }
}
