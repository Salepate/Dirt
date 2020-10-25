using Dirt.Game;
using Dirt.Game.Content;
using Dirt.Simulation;
using Dirt.Simulation.Builder;
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

        public IEnumerable<int> ActiveSimulations => m_SimulationMap.Keys;
        public SimulationManager(IContentProvider content)
        {
            m_SimulationMap = new Dictionary<int, SimulationProxy>();
            m_Content = content;
            m_IDGenerator = 0;
        }

        public GameSimulation GetSimulation(int id)
        {
            if ( m_SimulationMap.TryGetValue(id, out SimulationProxy sim) )
            {
                return sim.Simulation;
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

        public void AttachSystems(int id, SystemContainer systems)
        {
            if (m_SimulationMap.TryGetValue(id, out SimulationProxy proxy))
            {
                proxy.AttachSystems(systems);
            }
            else
            {
                Console.Error($"Simulation {id} is already running");
            }
        }

        public void DestroySimulation(int id)
        {
            if (m_SimulationMap.ContainsKey(id))
            {
                m_SimulationMap.Remove(id);
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

        public int CreateSimulation(string archetypeName, SimulationSpan span)
        {
            ActorBuilder builder = new ServerActorBuilder();
            builder.SetGameContent(m_Content);
            builder.InitializePool(1000);

            var gameSim = new GameSimulation(builder, m_IDGenerator)
            {
                Archetype = archetypeName
            };

            SimulationProxy proxy = new SimulationProxy(gameSim, span);

            m_SimulationMap.Add(m_IDGenerator, proxy);
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
            foreach(KeyValuePair<int, SimulationProxy> kvp in m_SimulationMap)
            {
                SimulationProxy proxy = kvp.Value;
                if ( proxy.Systems != null )
                {
                    proxy.Systems.UpdateSystems(proxy.Simulation, deltaTime);
                }
            }
        }
    }
}