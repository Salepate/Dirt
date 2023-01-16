using Dirt.Game;
using Dirt.Game.Content;
using Dirt.Game.Managers;
using Dirt.Log;
using Dirt.Simulation;
using Dirt.Simulation.Builder;
using Dirt.Simulation.Context;
using Dirt.Simulation.Events;
using Dirt.Simulation.Model;
using Dirt.Simulation.SystemHelper;
using System.Collections.Generic;
using UnityEngine;

namespace Dirt.Systems
{
    public class SimulationSystem : DirtSystem, IManagerProvider
    {
        private const string AssemblyCollection = "assemblies.client";
        private const string ClientSimulationName = "clientsim.default";

        public GameSimulation Simulation { get; private set; }
        private SystemContainer m_Systems;

        private Dictionary<System.Type, IGameManager> m_Managers;
        private SimulationBuilder m_SimBuilder;
        public AssemblyCollection ValidAssemblies { get; private set; }

        public IContextItem[] SharedContext => m_SharedContext.ToArray();

        private List<IContextItem> m_SharedContext;
        public override bool HasFixedUpdate => true;
        public override bool HasUpdate => true;

        private IContentProvider m_Content;

        public SimulationContext Context { get { return m_Systems.Context; } }


        public bool HasManager<T>() where T : IGameManager
        {
            return m_Managers.ContainsKey(typeof(T));
        }

        public T GetManager<T>() where T : IGameManager
        {
            System.Type t = typeof(T);
            Console.Assert(m_Managers.ContainsKey(t), $"Unknown Manager {t.Name}");
            return (T)m_Managers[t];
        }

        public override void Initialize(DirtMode mode)
        {
            m_Managers = new Dictionary<System.Type, IGameManager>();
            m_SimBuilder = new SimulationBuilder();
            m_SharedContext = new List<IContextItem>();
            m_Content = mode.FindSystem<ContentSystem>().Content;
            ValidAssemblies = m_Content.LoadContent<AssemblyCollection>(AssemblyCollection);

            AddSharedContext(ValidAssemblies);
            m_SimBuilder.LoadAssemblies(ValidAssemblies);
            RegisterManager<IContentProvider>(m_Content);
            RegisterManager(new MetricsManager());
            Simulation = new GameSimulation();
            Simulation.Builder.LoadAssemblies(ValidAssemblies);
            Simulation.Builder.SetGameContent(m_Content);
            m_Systems = new SystemContainer(m_Content, this);
        }

        public override void Unload()
        {
            DispatchEvent(new LocalSimulationEvent(Simulation.Archetype, LocalSimulationEvent.SimulationDestroyed));
            m_Systems.ClearSimulation(Simulation);
        }

        public void RegisterEventReader(IEventReader eventReader)
        {
            m_Systems.InjectEventDispatchers(eventReader);
        }

        public void RemoveEventReader(IEventReader eventReader)
        {
            m_Systems.RemoveEventDispatchers(eventReader);
        }

        public void DispatchEvent(SimulationEvent simEvent)
        {
            Simulation.Events.Enqueue(simEvent);
        }

        public void ChangeSimulation(string archetypeName)
        {
            ISimulationSystem[] systems = m_SimBuilder.CreateSystems(archetypeName, m_Content, false, out string context);
            string contextName = $"context.{context ?? "default"}";

            m_Systems.Context.ClearContext();
            DispatchEvent(new LocalSimulationEvent(Simulation.Archetype, LocalSimulationEvent.SimulationDestroyed));
            m_Systems.ClearSimulation(Simulation);
            Simulation.Archetype = archetypeName;
            if (m_Content.HasContent(contextName))
            {
                m_Systems.LoadContext(contextName);
            }

            m_SharedContext.ForEach(ctx =>
            {
                m_Systems.Context.SetContext(ctx);
            });

            for (int i = 0; i < systems.Length; ++i)
            {
                m_Systems.AddSystem(systems[i]);
            }

            m_Systems.InitializeSystems(Simulation);

            DispatchEvent(new LocalSimulationEvent(archetypeName, LocalSimulationEvent.SimulationLoaded));
        }

        public override void Update()
        {
            foreach(var mgr in m_Managers)
            {
                mgr.Value.Update(Time.deltaTime);
            }
        }
        public override void FixedUpdate()
        {
            m_Systems.UpdateSystems(Simulation, Time.fixedDeltaTime);
        }

        public void RegisterManager<T>(T manager) where T : IGameManager
        {
            m_Managers.Add(typeof(T), manager);
        }

        public void AddSharedContext<T>(T context) where T: IContextItem
        {
            m_SharedContext.Add(context);

            if (m_Systems != null && Context != null )
            {
                Context.SetContext(context);
            }
        }
    }
}