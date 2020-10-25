﻿using Dirt.Game;
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
        public ActorStreaming()
        {
            m_IDGenerator = 0;
        }

        protected virtual bool ShouldSerialize(bool isOwner) => true;
        protected virtual bool ShouldDeserialize(bool isOwner) => isOwner;

        public void UpdateActors(List<GameActor> actors, float deltaTime)
        {
            var netActors = actors.GetActors<NetInfo>();

            netActors.ForEach(t =>
            {
                GameActor actor = t.Item1;
                NetInfo netBhv = t.Item2;

                //@TODO Server exclusive
                if (netBhv.ID == -1)
                    netBhv.ID = GetID();

                DeserializeActor(actor, netBhv);


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
                    else
                    {
                        for (int i = 0; i < stateToSerialize.FieldIndex.Length; ++i)
                        {
                            int indexInFullState = GetIndexInState(netBhv.LastState, stateToSerialize.FieldIndex[i]);
                            netBhv.LastState.FieldValue[indexInFullState] = stateToSerialize.FieldValue[i];
                        }
                    }
                }
            });
        }

        public static string ByteArrayToString(byte[] ba)
        {
            return BitConverter.ToString(ba).Replace("-", "");
        }

        private int GetID()
        {
            return m_IDGenerator++;
        }

        private void DeserializeActor(GameActor actor, NetInfo sync)
        {
            if (sync.Fields.Count < 1 || sync.LastInBuffer == null)
                return;

            MessageHeader header = sync.LastInBuffer;

            for (int i = 0; i < header.FieldIndex.Length; ++i)
            {
                ComponentFieldInfo field = sync.Fields[header.FieldIndex[i]];
                if (ShouldDeserialize(field.Owner))
                {
                    Type compType = actor.ComponentTypes[field.Component];
                    if (NetworkSerializer.TryGetSetters(compType, out ObjectFieldAccessor[] accessors))
                    {
                        accessors[field.Accessor].Setter(actor.Components[field.Component], header.FieldValue[i]);
                    }
                }
            }
        }

        private MessageHeader SerializeActor(GameActor actor, NetInfo sync)
        {
            if (sync.Fields.Count < 1)
                return null;

            MessageHeader header = new MessageHeader();

            List<int> fields = new List<int>();
            List<object> values = new List<object>();

            MessageHeader oldState = sync.LastState;

            for (int i = 0; i < sync.Fields.Count; ++i)
            {
                ComponentFieldInfo field = sync.Fields[i];
                IComponent component = actor.Components[field.Component];

                if (ShouldSerialize(field.Owner) && NetworkSerializer.TryGetSetters(component.GetType(), out ObjectFieldAccessor[] accessor))
                {
                    bool changed = true;
                    Func<IComponent, object> getter = accessor[field.Accessor].Getter;
                    object newValue = getter(component);
                    if (oldState != null)
                    {
                        int oldStateIndex = GetIndexInState(oldState, i);
                        object oldValue = oldState.FieldValue[oldStateIndex];

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

        }

        public void SetManagers(IManagerProvider provider)
        {
            m_Serializer = provider.GetManager<NetworkSerializer>();
        }
    }
}
