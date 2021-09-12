using Dirt.Log;
using Dirt.States;
using Framework;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Dirt
{
    public abstract class DirtMode
    {
        // states
        public const int Boot = 0;
        public const int Load = 1;
        public const int Run = 2;

        public List<DirtSystem> Systems { get; private set; }
        public List<DirtSystem> UpdateSystems { get; private set; }
        public List<DirtSystem> FixedUpdateSystems { get; private set; }
        public List<DirtSystem> LateUpdateSystems { get; private set; }
        public Dictionary<string, DirtSystemContent> ContentMap { get; private set; }

        public virtual bool IsService { get { return false; } }

        public List<Scene> LoadedScenes { get; private set; }
        private FSM<DirtMode> m_FSM;

        private DirtStarter m_Starter;

        public T GetService<T>() where T : DirtSystem
        {
            DirtSystem system = m_Starter.Services.Find(s => s.GetType() == typeof(T));
            if (system == null)
            {
                Console.Error($"service {typeof(T).Name} not found");
            }
            return (T) system;
        }

        private bool IsSystem<T>(DirtSystem system)
        {
            if (system.GetType() == typeof(T))
                return true;
            else if (typeof(T).IsAssignableFrom(system.GetType()))
                return true;

            return false;
        }

        private bool IsSystem(System.Type systemType, DirtSystem system)
        {
            if (system.GetType() == systemType)
                return true;
            else if (systemType.IsAssignableFrom(system.GetType()))
                return true;

            return false;
        }

        public bool HasSystem<T>() where T: DirtSystem
        {
            //List<DirtSystem> matchingSystems = new List<DirtSystem>(Systems.Count + m_Starter.Services.Count);
            int idx = m_Starter.Services.FindIndex(IsSystem<T>);
            if (idx == -1)
                idx = Systems.FindIndex(IsSystem<T>);

            return idx != -1;
        }
        public T FindSystem<T>(int systemIndex = 0, bool optional = false) where T: DirtSystem
        {
            List<DirtSystem> matchingSystems = new List<DirtSystem>(Systems.Count + m_Starter.Services.Count);
            matchingSystems.AddRange(m_Starter.Services.FindAll(IsSystem<T>));
            matchingSystems.AddRange(Systems.FindAll(IsSystem<T>));

            bool hasSystem = matchingSystems.Count > systemIndex;
            Console.Assert(optional || hasSystem, $"System {typeof(T).Name} ({systemIndex}) not found ({matchingSystems.Count} referenced)");

            if ( hasSystem )
                return (T)matchingSystems[systemIndex];

            return null;
        }

        public DirtSystem FindSystem(System.Type systemType, bool optional = false)
        {
            List<DirtSystem> matchingSystems = new List<DirtSystem>(Systems.Count + m_Starter.Services.Count);
            matchingSystems.AddRange(m_Starter.Services.FindAll(sys => IsSystem(systemType, sys)));
            matchingSystems.AddRange(Systems.FindAll(sys => IsSystem(systemType, sys)));

            bool hasSystem = matchingSystems.Count > 0;
            Console.Assert(optional || hasSystem, $"System {systemType.Name} not found ({matchingSystems.Count} referenced)");

            if (hasSystem)
                return matchingSystems[0];

            return null;
        }
        public void Initialize(DirtStarter starter)
        {
            ContentMap = new Dictionary<string, DirtSystemContent>();
            Systems = new List<DirtSystem>();
            UpdateSystems = new List<DirtSystem>();
            FixedUpdateSystems = new List<DirtSystem>();
            LateUpdateSystems = new List<DirtSystem>();
            LoadedScenes = new List<Scene>();
            m_Starter = starter;

            m_FSM = new FSM<DirtMode>(this);
            m_FSM.AddState<BootState>(Boot);
            m_FSM.AddState<LoadState>(Load);
            m_FSM.AddState<RunState>(Run);
            m_FSM.Start();
            OnInitialize();
        }

        public void SetSystemsReady()
        {
            if ( IsService )
            {
                Console.Message("Mode registered as service");
                m_Starter.ServiceModes.Add(this);
                m_Starter.Services.AddRange(Systems);
            }
        }

        public void Update()
        {
            m_FSM.Update();
            OnUpdate();
        }

        public void LateUpdate()
        {
            m_FSM.LateUpdate();
            OnLateUpdate();
        }

        public void FixedUpdate()
        {
            m_FSM.FixedUpdate();
            OnFixedUpdate();
        }

        protected void AddSystem(DirtSystem system)
        {
            Systems.Add(system);

            if (system.HasUpdate)
                UpdateSystems.Add(system);
            if (system.HasLateUpdate)
                LateUpdateSystems.Add(system);
            if (system.HasFixedUpdate)
                FixedUpdateSystems.Add(system);
        }

        protected void AddSystem<T>() where T: DirtSystem, new()
        {
            T sys = new T();
            AddSystem(sys);
        }

        public abstract void OnInitialize();
        public virtual void OnUpdate() { }
        public virtual void OnLateUpdate() { }
        public virtual void OnFixedUpdate() { }
        public virtual void OnModeReady() { }
    }
}