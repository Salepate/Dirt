using Dirt.Game;
using Dirt.Network.Internal;
using Dirt.Network.Managers;
using Dirt.Network.Model;
using Dirt.Network.Simulation.Components;
using Dirt.Simulation;
using Dirt.Simulation.Actor;
using Dirt.Simulation.SystemHelper;
using System;
using System.Collections.Generic;
using System.IO;

namespace Dirt.Network.Systems
{
    using Console = Dirt.Log.Console;
    public class ActorStreaming : ISimulationSystem, IManagerAccess
    {
        private int m_IDGenerator;
        private NetworkSerializer m_Serializer;
        private ActorFilter Filter => m_Simulation.Filter;

        private GameSimulation m_Simulation;
        public ActorStreaming()
        {
            m_IDGenerator = 0;
        }

        protected virtual bool ShouldSerialize(bool isOwner) => true;
        protected virtual bool ShouldDeserialize(bool serverAuthor, bool isOwner) => isOwner;
        protected virtual bool ShouldDeserialize(ref NetInfo info) => !info.ServerControl && info.Owner != -1;

        public void UpdateActors(GameSimulation sim, float deltaTime)
        {
            ActorList<NetInfo> netActors = Filter.GetActors<NetInfo>();
            for(int i = 0; i < netActors.Count; ++i)
            {
                GameActor actor = netActors.GetActor(i);
                ref NetInfo netBhv = ref netActors.GetC1(i);

                //@TODO Server exclusive
                if (netBhv.ID == -1)
                    netBhv.ID = GetID();

                //@TODO Add timed state deserial/serial to control net throughput

                if (ShouldDeserialize(ref netBhv))
                {
                    DeserializeActor(actor, ref netBhv);
                }

                if (ShouldSerialize(netBhv.Owned))
                {
                    MessageHeader stateToSerialize = SerializeActor(actor, netBhv);

                    if (stateToSerialize != null && stateToSerialize.FieldIndex != null)
                    {
                        byte[] message = null;

                        using (MemoryStream messageStream = new MemoryStream())
                        {
                            m_Serializer.Serialize(messageStream, stateToSerialize);
                            message = messageStream.ToArray();
                        }

                        netBhv.LastOutBuffer = message;
                        if (netBhv.LastState == null) // first buffer
                        {
                            netBhv.LastState = stateToSerialize;
                        }
                    }
                }
            }
        }

        public static string ByteArrayToString(byte[] ba)
        {
            return BitConverter.ToString(ba).Replace("-", "");
        }

        private int GetID()
        {
            return m_IDGenerator++;
        }

        private void DeserializeActor(GameActor actor, ref NetInfo sync)
        {
            if (sync.Fields.Count < 1 || sync.LastInBuffer == null)
                return;

            MessageHeader header = sync.LastInBuffer;
            sync.LastInBuffer = null;
            SimulationPool pool = m_Simulation.Builder.Components;

            for (int i = 0; i < header.FieldIndex.Length; ++i)
            {
                ComponentFieldInfo field = sync.Fields[header.FieldIndex[i]];
                if (ShouldDeserialize(sync.ServerControl, field.Owner))
                {
                    Type compType = actor.ComponentTypes[field.Component];
                    if (NetworkSerializer.TryGetSetters(compType, out ObjectFieldAccessor[] accessors))
                    {
                        int compIdx = actor.Components[field.Component];
                        GenericArray genArr = pool.GetPool(compType);
                        accessors[field.Accessor].Setter(genArr, compIdx, header.FieldValue[i]);
                    }
                }
            }
        }

        private MessageHeader SerializeActor(GameActor actor, NetInfo sync)
        {
            if (sync.Fields.Count < 1)
                return null;

            MessageHeader header = new MessageHeader();
            SimulationPool pool = m_Simulation.Builder.Components;
            List<int> fields = new List<int>();
            List<object> values = new List<object>();
            MessageHeader oldState = sync.LastState;

            for (int i = 0; i < sync.Fields.Count; ++i)
            {
                ComponentFieldInfo field = sync.Fields[i];
                Type compType = actor.ComponentTypes[field.Component];
                IComponent component = (IComponent) pool.GetPool(compType).Get(actor.Components[field.Component]);

                if (ShouldSerialize(field.Owner) && NetworkSerializer.TryGetSetters(compType, out ObjectFieldAccessor[] accessor))
                {
                    bool changed = true;
                    Func<IComponent, object> getter = accessor[field.Accessor].Getter;
                    object newValue = getter(component);
                    if (oldState != null)
                    {
                        int oldStateIndex = GetIndexInState(oldState, i);
                        object oldValue = oldState.FieldValue[oldStateIndex];
                        oldState.FieldValue[oldStateIndex] = newValue;
                        Console.Assert(newValue != null, "Cannot send null values");

                        changed = !newValue.Equals(oldValue);
                    }

                    if (changed)
                    {
                        fields.Add(i);
                        values.Add(newValue);
                    }
                }
            }

            if (fields.Count > 0)
            {
                header.FieldIndex = fields.ToArray();
                header.FieldValue = values.ToArray();
            }
            return header;
        }

        private int GetIndexInState(MessageHeader state, int fieldIndex)
        {
            for (int i = 0; i < state.FieldIndex.Length; ++i)
                if (state.FieldIndex[i] == fieldIndex)
                    return i;
            return -1;
        }

        public void Initialize(GameSimulation sim)
        {
            m_Simulation = sim;
        }

        public void SetManagers(IManagerProvider provider)
        {
            m_Serializer = provider.GetManager<NetworkSerializer>();
        }
    }
}
