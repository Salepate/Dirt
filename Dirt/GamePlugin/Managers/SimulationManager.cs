using Dirt.Game;
using Dirt.Game.Content;
using Dirt.GameServer.Simulation.Events;
using Dirt.Simulation;
using Dirt.Simulation.Builder;
using Dirt.Simulation.Context;
using Dirt.Simulation.Model;
using System;
using System.Collections.Generic;

using Console = Dirt.Log.Console;

namespace Dirt.GameServer.Managers
{
    public class SimulationManager : IGameManager
    {
        private Dictionary<int, SimulationProxy> m_SimulationMap;
        private int m_IDGenerator;
        private IContentProvider m_Content;
        private List<SimulationProxy> m_ActiveSimulations;
        private List<int> m_Terminated;
        public IEnumerable<int> ActiveSimulations => m_SimulationMap.Keys;
        public Action<int> NotifySimulationDestroyed;
        private float m_Time;
        public SimulationManager(IContentProvider content)
        {
            m_SimulationMap = new Dictionary<int, SimulationProxy>();
            m_Content = content;
            m_IDGenerator = 0;
            m_ActiveSimulations = new List<SimulationProxy>();
            m_Terminated = new List<int>();
            m_Time = 0f;
        }

        public GameSimulation GetSimulation(int id)
        {
            if (m_SimulationMap.TryGetValue(id, out SimulationProxy sim))
            {
                return sim.Simulation;
            }

            return null;
        }

        public SimulationProxy GetSimulationProxy(int id)
        {
            if (m_SimulationMap.TryGetValue(id, out SimulationProxy sim))
            {
                return sim;
            }

            return null;
        }

        public bool IsReady(int id)
        {
            if (m_SimulationMap.TryGetValue(id, out SimulationProxy sim))
            {
                return sim.Systems != null;
            }

            return false;
        }

        public void PauseSimulation(int id)
        {
            if ( m_SimulationMap.TryGetValue(id, out SimulationProxy sim)  )
            {
                if ( !sim.Paused )
                {
                    sim.Paused = true;
                    sim.PauseTime = m_Time;
                }
                else
                {
                    Console.Warning($"Cannot pause Simulation {id}: Already paused");
                }
            }
            else
            {
                Console.Error($"Cannot pause Simulation {id}: Not found");
            }
        }

        public void ResumeSimulation(int id)
        {
            if (m_SimulationMap.TryGetValue(id, out SimulationProxy sim))
            {
                if (!sim.Paused)
                {
                    Console.Warning($"Cannot resume Simulation {id}: Not paused");
                }
                else
                {
                    sim.Paused = false;
                    sim.Simulation.Events.Enqueue(new SimulationPausedEvent(m_Time - sim.PauseTime));
                }
            }
            else
            {
                Console.Error($"Cannot pause Simulation {id}: Not found");
            }
        }

        public void AttachSystems(int id, SystemContainer systems)
        {
            if (m_SimulationMap.TryGetValue(id, out SimulationProxy proxy))
            {
                systems.InitializeSystems(proxy.Simulation);
                proxy.AttachSystems(systems);
            }
            else
            {
                Console.Error($"Simulation {id} is already running");
            }
        }

        public SimulationSpan GetSpan(int id)
        {
            if( !m_SimulationMap.TryGetValue(id, out SimulationProxy proxy) )
            {
                Console.Warning($"Uknown Simulation {id}");
            }
            return proxy.Span;
        }

        public int CreateSimulation(string archetypeName, SimulationSpan span, IContextItem[] initialContext = null)
        {
            ActorBuilder builder = new ServerActorBuilder();
            SimulationArchetype archetype = m_Content.LoadContent<SimulationArchetype>($"sim.{archetypeName}");
            builder.SetGameContent(m_Content);

            var gameSim = new GameSimulation(builder, m_IDGenerator, archetype.MaximumQueries)
            {
                ArchetypeName = archetypeName,
                Archetype = archetype
            };

            SimulationProxy proxy = new SimulationProxy(gameSim, span, initialContext);

            m_SimulationMap.Add(m_IDGenerator, proxy);
            m_ActiveSimulations.Add(proxy);
            return m_IDGenerator++;
        }

        public int FindSimulation(Predicate<GameSimulation> predicate)
        {
            foreach(var simEntry in m_SimulationMap)
            {
                if (predicate(simEntry.Value.Simulation))
                    return simEntry.Key;
            }
            return -1;
        }

        public void Update(float deltaTime)
        {
            m_Time += deltaTime;
            int simCount = m_ActiveSimulations.Count;
            for (int i = 0; i < simCount; ++i)
            {
                SimulationProxy simProxy = m_ActiveSimulations[i];
                if (simProxy.Terminated)
                {
                    m_Terminated.Add(i);
                }
                else if (!simProxy.Paused)
                {
                    m_ActiveSimulations[i].Systems.UpdateSystems(m_ActiveSimulations[i].Simulation, deltaTime);
                }
            }

            if ( m_Terminated.Count > 0 )
            {
                DestroyTerminatedSimulations();
            }
        }

        private void DestroyTerminatedSimulations()
        {
            for (int i = m_Terminated.Count - 1; i >= 0; --i)
            {
                int activeIndex = m_Terminated[i];
                SimulationProxy proxy = m_ActiveSimulations[activeIndex];
                m_ActiveSimulations.RemoveAt(activeIndex);
                m_SimulationMap.Remove(proxy.Simulation.ID);
                NotifySimulationDestroyed?.Invoke(proxy.Simulation.ID);
                proxy.Systems.ClearSimulation(proxy.Simulation);
                Console.Message($"Destroyed simulation {proxy.Simulation.ID}");
            }
            m_Terminated.Clear();
        }

        internal void Terminate(int id)
        {
            SimulationProxy proxy = GetSimulationProxy(id);
            proxy.Terminate();
        }
    }
}