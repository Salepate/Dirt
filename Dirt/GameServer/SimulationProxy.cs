﻿using Dirt.Network;
using Dirt.Simulation;
using Dirt.Simulation.Context;
using Mud.Server.Stream;

namespace Dirt.GameServer
{
    public class SimulationProxy
    {
        public GameSimulation Simulation { get; private set; }
        public SimulationSpan Span { get; private set; }
        public SystemContainer Systems { get; private set; }
        public IContextItem[] BaseContext { get; private set; }
        public StreamGroup Group { get; private set; }
        public bool Terminated { get; private set; }
        public bool Paused { get; internal set; }
        public float PauseTime { get; internal set; }

        public SimulationProxy(GameSimulation simulation, SimulationSpan span, IContextItem[] contextItems)
        {
            Simulation = simulation;
            Span = span;
            Terminated = false;

            if (contextItems == null)
                BaseContext = new IContextItem[0];
            else
                BaseContext = contextItems;
        }

        public void SetGroup(StreamGroup group)
        {
            Group = group;
        }

        public void AttachSystems(SystemContainer systems)
        {
            Systems = systems;
        }

        public void Terminate()
        {
            Terminated = true;
        }
    }
}
