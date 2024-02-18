using Dirt.Log;
using Dirt.Network.Internal;
using Dirt.Network.Managers;
using Dirt.Network.Model;
using Dirt.Network.Simulation.Components;
using Dirt.Simulation;
using Dirt.Simulation.Actor;
using System.Collections.Generic;
using System.IO;

namespace Dirt.Network.Simulation
{
    using Type = System.Type;
    public class ActorStream
    {
        public bool SerializeOwnedFieldOnly {get; set;}
        private GameSimulation m_Simulation;
        private NetworkSerializer m_Serializer;
        // per actor
        private List<int> m_Fields;
        private List<object> m_Values;
        // simulation span
        private ObjectFieldAccessor[][] m_FieldAccessorTable;

        public ActorStream()
        {
            m_Fields = new List<int>();
            m_Values = new List<object>();
        }

        public void Initialize(GameSimulation simulation, NetworkSerializer serializer)
        {
            m_Simulation = simulation;
            m_Serializer = serializer;
            m_FieldAccessorTable = new ObjectFieldAccessor[simulation.Builder.Components.Pools.Count][];

            foreach(KeyValuePair<Type, GenericArray> pool in simulation.Builder.Components.Pools)
            {
                Console.Assert(m_FieldAccessorTable[pool.Value.Index] == null, "Overwritting accessors");

                if (NetworkSerializer.TryGetSetters(pool.Key, out ObjectFieldAccessor[] accessors))
                {
                    m_FieldAccessorTable[pool.Value.Index] = accessors;
                }
            }
        }


        public void SerializeActor(GameActor actor, ref NetInfo sync, int frame)
        {
            if (sync.Fields.Length < 1 || sync.LastOutStamp == frame)
                return;

            MessageHeader lastState = sync.LastState;
            MessageHeader newState = new MessageHeader();
            SimulationPool pool = m_Simulation.Builder.Components;

            m_Fields.Clear();
            m_Values.Clear();

            for (int i = 0; i < sync.Fields.Length; ++i)
            {
                ref ComponentFieldInfo field = ref sync.Fields[i];
                IComponent component = (IComponent)pool.GetPoolByIndex(field.PoolIndex).Get(actor.Components[field.Component]);
                ObjectFieldAccessor[] accessors = m_FieldAccessorTable[field.PoolIndex];

                if ( (!SerializeOwnedFieldOnly || field.Owner) && accessors != null)
                {
                    bool changed = true;
                    System.Func<IComponent, object> getter = accessors[field.Accessor].Getter;

                    object newValue = getter(component);
                    if (lastState != null)
                    {
                        object oldValue = lastState.FieldValue[i];
                        lastState.FieldValue[i] = newValue;
                        Console.Assert(newValue != null, "Cannot send null values");
                        changed = !newValue.Equals(oldValue);
                    }

                    if (changed)
                    {
                        m_Fields.Add(i);
                        m_Values.Add(getter(component));
                    }
                }
            }

            if (m_Fields.Count > 0)
            {
                newState.FieldIndex = m_Fields.ToArray();
                newState.FieldValue = m_Values.ToArray();

                if (sync.LastState == null)
                {
                    sync.LastState = newState;
                }

                using (MemoryStream ms = new MemoryStream())
                {
                    m_Serializer.Serialize(ms, newState);
                    sync.LastOutBuffer = ms.ToArray();
                }
            }
            sync.LastOutStamp = frame;
        }
    }
}
