using Dirt.Game;
using Dirt.Game.Content;
using Dirt.Game.Managers;
using Dirt.Simulation.Actor;
using Dirt.Simulation.Actor.Components;
using Dirt.Simulation.Context;
using Dirt.Simulation.Model;
using Dirt.Simulation.SystemHelper;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using Stopwatch = System.Diagnostics.Stopwatch;
using Type = System.Type;

namespace Dirt.Simulation
{
    public class SystemContainer
    {
        private delegate void EventDelegate(SimulationEvent gameEvent, int eventID);
        private Dictionary<Type, EventDelegate> m_EventListenerMap;
        private Dictionary<IEventReader, LambdaReference[]> m_ReaderReferences;
        public List<ISimulationSystem> Systems { get; private set; }

        private List<SystemMetric> m_MetricsOverTime;
        private List<bool> m_SystemStatus;

        private IContentProvider m_Content;
        private IManagerProvider m_Managers;
        private Stopwatch m_SystemStopWatch;
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
            Systems = new List<ISimulationSystem>();
            m_MetricsOverTime = new List<SystemMetric>();
            m_SystemStatus = new List<bool>();
            m_SystemStopWatch = new Stopwatch();
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
            Systems.Add(sys);
            m_MetricsOverTime.Add(new SystemMetric());
            m_SystemStatus.Add(true);
        }

        public void InitializeSystems(GameSimulation simulation)
        {
            Systems.ForEach(sys =>
            {
                sys.Initialize(simulation);
            });
        }

        public void SetSystemStatus(int systemIndex, bool enabled)
        {
            m_SystemStatus[systemIndex] = enabled;
        }

        public void UpdateSystems(GameSimulation simulation, float deltaTime)
        {
            float tickToSec = 1f / Stopwatch.Frequency;
            long totalTicks = 0;
            int sysCount = Systems.Count;

            for(int i = 0; i < sysCount; ++i)
            {
                int microTime = 0;

                if ( m_SystemStatus[i] )
                {
                    ISimulationSystem sys = Systems[i];
                    SystemMetric metric = m_MetricsOverTime[i];
                    m_SystemStopWatch.Reset();
                    m_SystemStopWatch.Start();
                    sys.UpdateActors(simulation, deltaTime);
                    m_SystemStopWatch.Stop();
                    totalTicks += m_SystemStopWatch.ElapsedMilliseconds;
                    microTime = (int) (m_SystemStopWatch.ElapsedTicks * tickToSec * 1000000);
                }

                ProcessMetrics(simulation.ID, i, microTime);
            }

            DispatchQueuedEvents(simulation);

            // actor clean up
            List<ActorTuple<Destroy>> destroyedActors = simulation.Filter.GetAll<Destroy>();
            for (int i = 0; i < destroyedActors.Count; ++i)
            {
                //Console.Message($"Destroying Actor {destroyedActors[i].Item1.ID}");
                simulation.Builder.DestroyActor(destroyedActors[i].Actor);
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
            Systems.ForEach(sys =>
            {
                if (sys is ISystemDispose)
                    ((ISystemDispose)sys).DisposeSystem();

                if (sys is IEventReader)
                    RemoveEventDispatchers((IEventReader)sys);
            });


            m_MetricsOverTime.Clear();
            Systems.Clear();

            for(int i = simulation.World.Actors.Count - 1; i >= 0; --i)
            {
                simulation.Builder.DestroyActor(simulation.World.Actors[i]);
            }

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
            SystemMetric metric = m_MetricsOverTime[systemIndex];

            if (m_Frame % 60 == 0)
            {
                metric.AveragedCost /= 60; // send true metric
                m_Metrics.Record($"{simID}.{Systems[systemIndex].GetType().Name}", metric);

                metric = new SystemMetric()
                {
                    AveragedCost = 0,
                    AveragedMax = 0,
                    AveragedMin = float.MaxValue
                };
            }
            else
            {
                UpdateMetrics(microTime, ref metric);
            }

            m_MetricsOverTime[systemIndex] = metric;
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