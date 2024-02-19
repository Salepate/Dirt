using Dirt.Game.Math;
using Dirt.Network.Simulation.Components;
using Dirt.Simulation;
using Dirt.Simulation.Actor;
using Dirt.Simulation.Components;
using Mud;
using System.ComponentModel;

namespace Dirt.Network.Simulation
{
    public class ActorStream
    {
        private bool m_Client;
        public bool SerializeOwnedFieldOnly {get; set;}
        private GameSimulation m_Simulation;

        public ActorStream()
        {
        }

        public void Initialize(GameSimulation simulation, bool client)
        {
            m_Simulation = simulation;
            m_Client = client;
        }

        public void DeserializeActor(GameActor actor, ref NetInfo sync)
        {
            if (sync.LastInBuffer == null || sync.Serializers == null || sync.Serializers.Length == 0)
                return;

            NetworkStream stream = new NetworkStream(sync.LastInBuffer);
            sync.LastInBuffer = null;
            stream.Position += 1; // skip id

            while(stream.Position < stream.Buffer.Length)
            {
                int compIndex = stream.ReadByte();
                if (compIndex <= sync.Serializers.Length)
                {
                    ref ComponentSerializer serial = ref sync.Serializers[compIndex];
                    bool authority;
                    bool ignore;

                    if (m_Client) 
                    {
                        // Always deserialize all data, but use authority to ignore some data
                        authority = serial.AuthoredByOwner && sync.Owned;
                        ignore = false;
                    }
                    else
                    {
                        ignore = !serial.AuthoredByOwner;
                        authority = !serial.AuthoredByOwner;
                    }

                    if (ignore)
                        continue;

                    if (serial.UseNetSerializer)
                    {
                        GenericArray pool = m_Simulation.Builder.Components.GetPoolByIndex(serial.PoolIndex);
                        INetComponent netComp = (INetComponent)pool.Get(actor.Components[serial.ComponentIndex]);
                        netComp.Deserialize(stream, m_Client, authority);
                        pool.Set(actor.Components[serial.ComponentIndex], netComp);
                    }
                    else if (serial.IsPosition)
                    {
                        ComponentArray<Position> posPool = (ComponentArray<Position>)m_Simulation.Builder.Components.GetPoolByIndex(serial.PoolIndex);
                        ref Position pos = ref posPool.Components[actor.Components[serial.ComponentIndex]];

                        float3 p = stream.ReadFloat3();
                        if (!authority)
                            pos.Origin = p;
                    }
                }
            }
        }
        public void SerializeActor(GameActor actor, ref NetInfo sync, int frame)
        {
            if (sync.Serializers.Length < 1 || sync.LastOutStamp == frame)
                return;

            NetworkStream stream = new NetworkStream();
            stream.Allocate(128); // random for now
            stream.Write((byte)NetworkOperation.ActorSync);
            stream.Write((byte)sync.ID);

            bool change = false;

            SimulationPool pool = m_Simulation.Builder.Components;
            for(int i = 0; i < sync.Serializers.Length; ++i)
            {
                ref ComponentSerializer serial = ref sync.Serializers[i];

                if (m_Client && (!serial.AuthoredByOwner || !sync.Owned))
                    continue;


                stream.Write((byte)i);
                int prevPos = serial.LastIndexInBuffer;
                serial.LastIndexInBuffer = stream.Position;

                if (serial.UseNetSerializer)
                {
                    INetComponent netComp =  (INetComponent) pool.GetPoolByIndex(serial.PoolIndex).Get(actor.Components[serial.ComponentIndex]);
                    netComp.Serialize(stream, m_Client);
                }
                else if (serial.IsPosition)
                {
                    ComponentArray<Position> posPool = (ComponentArray<Position>) pool.GetPoolByIndex(serial.PoolIndex);
                    ref Position pos = ref posPool.Components[actor.Components[serial.ComponentIndex]];
                    stream.Write(pos.Origin);
                }

                change |= prevPos != serial.LastIndexInBuffer;
                int size = stream.Position - prevPos;
                for(int j = prevPos; !change && j < prevPos + size; ++j)
                {
                    if (sync.LastOutBuffer == null || sync.LastOutBuffer.Length <= j || sync.LastOutBuffer[j] != stream.Buffer[j])
                    {
                        change = true;
                    }
                }
            }

            if (stream.Position > 0)
            {
                sync.LastOutBuffer = stream.Buffer;
                sync.BufferSize = stream.Position;
            }

            if (change)
            {
                sync.LastOutStamp = frame;
            }
        }
    }
}
