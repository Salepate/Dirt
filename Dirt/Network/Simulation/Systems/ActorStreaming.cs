using Dirt.Game;
using Dirt.Network.Internal;
using Dirt.Network.Managers;
using Dirt.Network.Model;
using Dirt.Network.Simulation;
using Dirt.Network.Simulation.Components;
using Dirt.Simulation;
using Dirt.Simulation.Actor;
using Dirt.Simulation.Components;
using Dirt.Simulation.SystemHelper;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices.ComTypes;

namespace Dirt.Network.Systems
{
    using Console = Dirt.Log.Console;

    /// <summary>
    /// Transform actors into memory streams suited for network transmission.
    /// </summary>
    public class ActorStreaming : ISimulationSystem
    {
        public const int Destroyed = 0;
        public const int Culled = 255;

        protected bool ClientStream;
        private ActorStream m_Stream;
        private ActorFilter Filter => m_Simulation.Filter;
        private GameSimulation m_Simulation;
        private Stopwatch m_Watch;
        private int m_Frame;

        protected virtual void DoRecord(string id, int value) { }

        protected virtual bool ShouldSerializeActor(ref NetInfo info) => false;
        protected virtual bool ShouldSerializeField(bool isOwner) => true;
        protected virtual bool ShouldDeserialize(bool serverAuthor, bool isOwner) => isOwner;
        protected virtual bool ShouldDeserialize(ref NetInfo info) => !info.ServerControl && info.Owner != -1;

        public void UpdateActors(GameSimulation sim, float deltaTime)
        {
            m_Frame++;
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
                if (ShouldDeserialize(ref netBhv))
                {
                    m_Stream.DeserializeActor(actor, ref netBhv);
                }
                deserialTicks += m_Watch.Elapsed.Ticks - ticks;
                ticks = m_Watch.Elapsed.Ticks;

                if (ShouldSerializeActor(ref netBhv))
                {
                    m_Stream.SerializeActor(actor, ref netBhv, m_Frame);
                }
                serialTicks += m_Watch.Elapsed.Ticks - ticks;
                ticks = m_Watch.Elapsed.Ticks;
            }
            DoRecord("ActorStreaming.DeserializeAll", (int) (deserialTicks * SystemContainer.TICK_TO_MICRO));
            DoRecord("ActorStreaming.SerializeAll", (int) (serialTicks * SystemContainer.TICK_TO_MICRO));
            DoRecord("ActorStreaming.Actors", netActors.Count);
        }

        public virtual void Initialize(GameSimulation sim)
        {
            m_Watch = new Stopwatch();
            m_Frame = 0;
            m_Simulation = sim;
            m_Stream = new ActorStream();
            m_Stream.Initialize(m_Simulation, ClientStream);
        }
    }
}
