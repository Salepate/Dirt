using Dirt.Game;
using Dirt.Game.Content;
using Dirt.Game.Managers;
using Dirt.Simulation.Actor;
using Dirt.Simulation.Actor.Components;
using Dirt.Simulation.Context;
using Dirt.Simulation.Model;
using Dirt.Simulation.SystemHelper;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using Stopwatch = System.Diagnostics.Stopwatch;
using Type = System.Type;

namespace Dirt.Simulation
{
    public class SystemContainer
    {
        public static readonly double TICK_TO_SECOND = 1.0 / Stopwatch.Frequency;
        public static readonly double TICK_TO_MICRO = TICK_TO_SECOND * 1000000;

        private delegate void EventDelegate(SimulationEvent gameEvent, int eventID);
        private Dictionary<Type, EventDelegate> m_EventListenerMap;
        private Dictionary<IEventReader, LambdaReference[]> m_ReaderReferences;
        private List<ISimulationSystem> m_Systems;
        private List<SystemMetric> m_SystemMetrics;
        private List<bool> m_SystemStatus;
        private IContentProvider m_Content;
        private IManagerProvider m_Managers;
        private Stopwatch m_MetricsWatch;
        private MetricsManager m_Metrics;
        private int m_Frame;

        public SimulationContext Context { get; private set; }

        public SystemContainer(IContentProvider content, IManagerProvider manager)
        {
            Context = new SimulationContext();
            m_Content = content;
            m_Managers = manager;
            m_Frame = 0;
            m_EventListenerMap = new Dictionary<Type, EventDelegate>();
            m_ReaderReferences = new Dictionary<IEventReader, LambdaReference[]>();
            m_Systems = new List<ISimulationSystem>();
            m_SystemMetrics = new List<SystemMetric>();
            m_SystemStatus = new List<bool>();
            m_MetricsWatch = new Stopwatch();
            m_Metrics = manager.GetManager<MetricsManager>();
        }

        public void LoadContext(string contextName)
        {
            JObject context = null;
            context = m_Content.LoadContent(contextName);
            foreach (JProperty prop in context.Properties())
            {
                Context.CreateContext(prop.Name, prop.Value);
            }
        }

        public void AddSystem(ISimulationSystem sys) 
        {
            if (sys is IManagerAccess)
            {
                ((IManagerAccess)sys).SetManagers(m_Managers);
            }
            if ( sys is IContextReader)
            {
                ((IContextReader)sys).SetContext(Context);
            }
            if (sys is IContentReader)
            {
                ((IContentReader)sys).SetContent(m_Content);
            }
            if (sys is IEventReader)
            {
                InjectEventDispatchers(sys as IEventReader);
            }
            m_Systems.Add(sys);

            m_SystemMetrics.Add(new SystemMetric()
            {
                Name = sys.GetType().Name
            });

            m_SystemStatus.Add(true);
        }

        public void InitializeSystems(GameSimulation simulation)
        {
            for(int i = 0; i < m_Systems.Count; ++i)
            {
                m_Systems[i].Initialize(simulation);
            }
        }

        public void SetSystemStatus(int systemIndex, bool enabled)
        {
            m_SystemStatus[systemIndex] = enabled;
        }

        public void UpdateSystems(GameSimulation simulation, float deltaTime)
        {
            long accTicks = 0;
            int sysCount = m_Systems.Count;
            for (int i = 0; i < sysCount; ++i)
            {
                int microTime = 0;

                if ( m_SystemStatus[i] )
                {
                    ISimulationSystem sys = m_Systems[i];
                    m_MetricsWatch.Restart();
                    simulation.Filter.ResetQueries();
                    sys.UpdateActors(simulation, deltaTime);
                    m_MetricsWatch.Stop();
                    accTicks += m_MetricsWatch.ElapsedTicks;
                    microTime = (int) (m_MetricsWatch.ElapsedTicks * TICK_TO_SECOND * 1000000);
                }

                ProcessMetrics(simulation.ID, i, microTime);
            }

            m_Metrics.Record($"sim.{simulation.ID}.total", (int) (accTicks * TICK_TO_SECOND * 1000000));

            DispatchQueuedEvents(simulation);

            simulation.Filter.ResetQueries();
            ActorList<Destroy> destroyedActors = simulation.Filter.GetActors<Destroy>();
            // reverse traversal to avoid index invalidation
            for (int i = destroyedActors.Count - 1; i >= 0 ; --i)
            {
                simulation.Builder.DestroyActor(destroyedActors.GetActor(i));
            }

            DispatchQueuedEvents(simulation);

            m_Frame++;
        }

        private void UpdateMetrics(int microtime, ref SystemMetric metric)
        {
            metric.AveragedCost += microtime;

            if (metric.AveragedMax < microtime)
            {
                metric.AveragedMax = microtime;
            }

            if (metric.AveragedMin > microtime)
            {
                metric.AveragedMin = microtime;
            }
        }

        public void ClearSimulation(GameSimulation simulation)
        {
            for(int i = 0; i < m_Systems.Count; ++i)
            {
                ISimulationSystem sys = m_Systems[i];

                if (sys is IDisposable disposable)
                    disposable.Dispose();

                if (sys is IEventReader reader)
                    RemoveEventDispatchers(reader);
            }


            m_SystemMetrics.Clear();
            m_Systems.Clear();

            for(int i = simulation.Filter.Actors.Count - 1; i >= 0; --i)
            {
                simulation.Builder.DestroyActor(simulation.Filter.Actors[i]);
            }

            simulation.Filter.Actors.Clear();

            DispatchQueuedEvents(simulation);
        }

        private void DispatchQueuedEvents(GameSimulation simulation)
        {
            while (simulation.Events.Count > 0)
            {
                var gameEvent = simulation.Events.Dequeue();
                if (m_EventListenerMap.TryGetValue(gameEvent.GetType(), out EventDelegate listeners))
                {
                    listeners(gameEvent, gameEvent.Event);
                }
            }
        }


        //TODO: Move in helper
        // Reflection Part 
        public void InjectEventDispatchers(IEventReader eventReader)
        {
            List<LambdaReference> lambdaRefs = new List<LambdaReference>();

            var methods = eventReader.GetType().GetMethods(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            for(int i = 0; i < methods.Length; ++i)
            {
                var attr = methods[i].GetCustomAttribute<SimulationListenerAttribute>();
                if ( attr != null )
                {
                    var copy = methods[i];

                    void lambdaDelegate(SimulationEvent gameEvent, int eventID)
                    {
                        if ( attr.EventID == eventID )
                            copy.Invoke(eventReader, new object[] { gameEvent });
                    }

                    if (!m_EventListenerMap.ContainsKey(attr.EventType))
                        m_EventListenerMap.Add(attr.EventType, lambdaDelegate);
                    else
                        m_EventListenerMap[attr.EventType] += lambdaDelegate;

                    lambdaRefs.Add(new LambdaReference() { EventType = attr.EventType, Lambda = lambdaDelegate });
                }
            }
            m_ReaderReferences.Add(eventReader, lambdaRefs.ToArray());
        }

        public void RemoveEventDispatchers(IEventReader eventReader)
        {
            if ( m_ReaderReferences.TryGetValue(eventReader, out LambdaReference[] refs))
            {
                for(int i = 0; i < refs.Length; ++i)
                {
                    m_EventListenerMap[refs[i].EventType] -= refs[i].Lambda;

                    if ( m_EventListenerMap[refs[i].EventType] == null )
                    {
                        m_EventListenerMap.Remove(refs[i].EventType);
                    }
                }
                m_ReaderReferences.Remove(eventReader);
            }
        }

        [Conditional(MetricsManager.CONDITIONAL_METRICS)]
        private void ProcessMetrics(int simID, int systemIndex, int microTime)
        {
            SystemMetric metric = m_SystemMetrics[systemIndex];

            if (m_Frame % 60 == 0)
            {
                metric.AveragedCost /= 60; // send true metric
                m_Metrics.Record($"{simID}.{metric.Name}", metric);

                metric = new SystemMetric()
                {
                    AveragedCost = 0,
                    AveragedMax = 0,
                    AveragedMin = float.MaxValue,
                    Name = metric.Name
                };
            }
            else
            {
                UpdateMetrics(microTime, ref metric);
            }

            m_SystemMetrics[systemIndex] = metric;
        }

        private class LambdaReference
        {
            public EventDelegate Lambda;
            public Type EventType;
        }
        private class EventReaderReferences : Dictionary<IEventReader, LambdaReference[]>
        {
        }
    }
}