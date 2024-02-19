using Dirt.Game.Math;
using Dirt.Network.Simulation.Components;
using Dirt.Simulation;
using Dirt.Simulation.Actor;
using Dirt.Simulation.Components;
using System.ComponentModel;
using System.Diagnostics.SymbolStore;

namespace Dirt.Network.Simulation
{
    /// <summary>
    /// Handle Actor synchronization (not state)
    /// Transform an actor set of components into a byte array suited for network transport.
    /// Detects changed components and reduce sent data to the minimum.
    /// </summary>
    public class ActorStream
    {
        private bool m_Client;
        public bool SerializeOwnedFieldOnly {get; set;}
        private GameSimulation m_Simulation;

        public ActorStream()
        {
        }

        /// <summary>
        /// Setup the stream
        /// </summary>
        public void Initialize(GameSimulation simulation, bool client)
        {
            m_Simulation = simulation;
            m_Client = client;
        }

        /// <summary>
        /// Deserialize an actor using NetInfo.LastInBuffer
        /// </summary>
        /// <param name="actor">actor to serialize</param>
        /// <param name="sync">target behaviour to deserialize from</param>
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

        /// <summary>
        /// Serialize components and shove it into NetInfo.LastOutBuffer
        /// Deserialize an actor using NetInfo.LastInBuffer
        /// </summary>
        /// <param name="actor">actor to serialize</param>
        /// <param name="sync">target behaviour to serialize into</param>
        public void SerializeActor(GameActor actor, ref NetInfo sync, int frame)
        {
            if (sync.Serializers.Length < 1 || sync.LastOutStamp == frame)
                return;

            
            NetworkStream stream = new NetworkStream(sync.LastOutBuffer);
            stream.Write((byte)NetworkOperation.ActorSync);
            stream.Write((byte)sync.ID);

            int delta = stream.Position; // ignore delta to emit change
            bool writeAll = sync.Serializers[0].LastIndexInBuffer == 0;

            SimulationPool pool = m_Simulation.Builder.Components;
            for(int i = 0; i < sync.Serializers.Length; ++i)
            {
                ref ComponentSerializer serial = ref sync.Serializers[i];
                if (m_Client && (!serial.AuthoredByOwner || !sync.Owned))
                {
                    serial.LastIndexInBuffer = -1; // hijack to 
                    continue;
                }

                int bufferIndex = stream.Position;
                stream.Write((byte)i);

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

                if (writeAll)
                {
                    serial.LastIndexInBuffer = bufferIndex;
                }
                else
                {
                    bool diff = CheckDiff(sync.LastSerializedState, serial.LastIndexInBuffer, stream.Buffer, bufferIndex, stream.Position - bufferIndex);
                    if (!diff) 
                    {
                        // rewind in out buffer
                        stream.Position = bufferIndex;
                    }
                    else
                    {
                        System.Buffer.BlockCopy(stream.Buffer, bufferIndex, sync.LastSerializedState, serial.LastIndexInBuffer, stream.Position - bufferIndex);
                    }
                }
            }

            if (writeAll)
            {
                System.Buffer.BlockCopy(stream.Buffer, 0, sync.LastSerializedState, 0, stream.Position);
            }

            if (stream.Position - delta > 0)
            {
                sync.BufferSize = stream.Position;
                sync.LastOutStamp = frame;
            }
        }

        private bool CheckDiff(byte[] src, int srcStart, byte[] dst, int dstStart, int length)
        {
            for(int i = length - 1; i >= 0; --i)
            {
                if (src[srcStart + i] != dst[dstStart + i])
                    return true;
            }
            return false;
        }
    }
}
