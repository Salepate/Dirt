using Dirt.Network.Simulation.Components;
using Dirt.Simulation;
using Dirt.Simulation.Actor;
using Dirt.Simulation.Components;

namespace Dirt.Network.Simulation
{
    public class ActorStream
    {
        public bool SerializeOwnedFieldOnly {get; set;}
        private GameSimulation m_Simulation;

        public ActorStream()
        {
        }

        public void Initialize(GameSimulation simulation)
        {
            m_Simulation = simulation;
        }

        public void DeserializeActor(GameActor actor, ref NetInfo sync)
        {
            if (sync.LastInBuffer == null)
                return;

            sync.LastInBuffer = null;
            NetworkStream stream = new NetworkStream(sync.LastInBuffer);
            stream.Position += 1; // skip id
            for(int i = 0; i < sync.Serializers.Length; ++i)
            {
                ref ComponentSerializer serial = ref sync.Serializers[i];
                if (serial.UseNetSerializer)
                {
                    GenericArray pool = m_Simulation.Builder.Components.GetPoolByIndex(serial.PoolIndex);
                    INetComponent netComp = (INetComponent)pool.Get(actor.Components[serial.ComponentIndex]);
                    netComp.Deserialize(stream);
                    pool.Set(actor.Components[serial.ComponentIndex], netComp);
                }
                else if (serial.IsPosition)
                {
                    ComponentArray<Position> posPool = (ComponentArray<Position>) m_Simulation.Builder.Components.GetPoolByIndex(serial.PoolIndex);
                    ref Position pos = ref posPool.Components[actor.Components[serial.ComponentIndex]];
                    pos.Origin = stream.ReadFloat3();
                }
            }
        }
        public void SerializeActor(GameActor actor, ref NetInfo sync, int frame)
        {
            if (sync.Fields.Length < 1 || sync.LastOutStamp == frame)
                return;

            NetworkStream stream = new NetworkStream();
            stream.Allocate(2048); // random for now
            SimulationPool pool = m_Simulation.Builder.Components;
            for(int i = 0; i < sync.Serializers.Length; ++i)
            {
                ref ComponentSerializer serial = ref sync.Serializers[i];
                if (serial.UseNetSerializer)
                {
                    INetComponent netComp =  (INetComponent) pool.GetPoolByIndex(serial.PoolIndex).Get(actor.Components[serial.ComponentIndex]);
                    netComp.Serialize(stream);
                }
                else if (serial.IsPosition)
                {
                    ComponentArray<Position> posPool = (ComponentArray<Position>) pool.GetPoolByIndex(serial.PoolIndex);
                    ref Position pos = ref posPool.Components[actor.Components[serial.ComponentIndex]];
                    stream.Write(pos.Origin);
                }
            }

            if (stream.Position > 0)
            {
                sync.LastOutBuffer = stream.Buffer;
                sync.BufferSize = stream.Position;
            }

            sync.LastOutStamp = frame;
        }
    }
}
