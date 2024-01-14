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
using System.Diagnostics;
using System.Globalization;
using System.IO;

namespace Dirt.Network.Systems
{
    using Console = Dirt.Log.Console;

    /// <summary>
    /// Transform actors into memory streams suited for network transmission.
    /// </summary>
    public class ActorStreaming : ISimulationSystem, IManagerAccess
    {
        private int m_IDGenerator;
        private NetworkSerializer m_Serializer;
        private ActorFilter Filter => m_Simulation.Filter;

        private GameSimulation m_Simulation;
        private Stopwatch m_Watch;
        public ActorStreaming()
        {
            m_IDGenerator = 0;
            m_Watch = new Stopwatch();
            

        }

        protected virtual void DoRecord(string id, int value) { }

        protected virtual bool ShouldSerializeActor(ref NetInfo info) => true;
        protected virtual bool ShouldSerializeField(bool isOwner) => true;
        protected virtual bool ShouldDeserialize(bool serverAuthor, bool isOwner) => isOwner;
        protected virtual bool ShouldDeserialize(ref NetInfo info) => !info.ServerControl && info.Owner != -1;

        public void UpdateActors(GameSimulation sim, float deltaTime)
        {
            m_Watch.Restart();
            ActorList<NetInfo> netActors = Filter.GetActors<NetInfo>();
            long ticks = m_Watch.Elapsed.Ticks;
            long serialTicks = 0;
            long deserialTicks = 0;
            DoRecord("ActorStreaming.Filter", (int) (ticks * SystemContainer.TICK_TO_MICRO));
            for (int i = 0; i < netActors.Count; ++i)
            {
                GameActor actor = netActors.GetActor(i);
                ref NetInfo netBhv = ref netActors.GetC1(i);

                // Only performed server side (assuming negative id are never serialized)
                if (netBhv.ID == -1)
                    netBhv.ID = GetID();

                //TODO: Add timed state deserial/serial to control net throughput (use net tickrate)


                if (ShouldDeserialize(ref netBhv))
                {
                    DeserializeActor(actor, ref netBhv);
                }
                deserialTicks += m_Watch.Elapsed.Ticks - ticks;
                ticks = m_Watch.Elapsed.Ticks;

                if (ShouldSerializeActor(ref netBhv))
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
                serialTicks += m_Watch.Elapsed.Ticks - ticks;
                ticks = m_Watch.Elapsed.Ticks;
            }
            DoRecord("ActorStreaming.DeserializeAll", (int) (deserialTicks * SystemContainer.TICK_TO_MICRO));
            DoRecord("ActorStreaming.SerializeAll", (int) (serialTicks * SystemContainer.TICK_TO_MICRO));
            DoRecord("ActorStreaming.Actors", netActors.Count);
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
                int fieldIndex = header.FieldIndex[i];
                if ( fieldIndex < 0 || fieldIndex >= sync.Fields.Count)
                {
                    Console.Error($"Actor {actor} Failed to deserialize field {header.FieldIndex[i]}");
                    for(int j = 0; j < actor.ComponentCount; ++j)
                    {
                        if (actor.ComponentTypes[j] != null)
                            Console.Message(actor.ComponentTypes[j].Name);
                    }
                    continue;
                }
                ComponentFieldInfo field = sync.Fields[fieldIndex];
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

        private MessageHeader SerializeActor(GameActor actor, in NetInfo sync)
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

                if (ShouldSerializeField(field.Owner) && NetworkSerializer.TryGetSetters(compType, out ObjectFieldAccessor[] accessor))
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

        public virtual void SetManagers(IManagerProvider provider)
        {
            m_Serializer = provider.GetManager<NetworkSerializer>();
        }
    }
}
