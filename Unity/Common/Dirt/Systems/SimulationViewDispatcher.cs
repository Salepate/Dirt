using Dirt.Game.Content;
using Dirt.Log;
using Dirt.Model;
using Dirt.Simulation;
using Dirt.Simulation.Actor;
using Dirt.Simulation.Context;
using Dirt.Simulation.SystemHelper;
using Dirt.Simulation.Utility;
using Dirt.Simulation.View;
using Framework;
using System.Collections.Generic;
using UnityEngine;

namespace Dirt.Systems
{
    using ViewLoader = ViewDefinition.ViewLoader;
    public abstract class SimulationViewDispatcher : DirtSystem, IContentSystem
    {
        protected virtual bool IsDebug => false;
        public override bool HasUpdate => true;
        /// <summary>
        /// set to true if you want to delay actor view instantiation
        /// the dispatcher will queue all requests and spawn them once set to false again
        /// </summary>
        public bool QueueRequests { get; set; }

        protected PoolManager PoolManager;
        protected abstract ViewDefinition[] ViewDefinitions { get; }
        private PrefabService m_Prefabs;
        private List<ISimulationView> m_Views;
        private IContentProvider m_Content;
        private SimulationSystem m_Simulation;
        private DirtMode m_Mode;
        private List<ViewBinding> m_QueuedActors;
        public override void Initialize(DirtMode mode)
        {
            PoolManager = new PoolManager(IsDebug);
            m_Views = new List<ISimulationView>();
            m_Prefabs = mode.FindSystem<PrefabService>();
            m_Content = mode.FindSystem<ContentSystem>().Content;
            m_QueuedActors = new List<ViewBinding>();
            m_Mode = mode;

            m_Simulation = mode.FindSystem<SimulationSystem>();
            Dictionary<string, System.Type> compMap = AssemblyReflection.BuildTypeMap<IComponent>(m_Simulation.ValidAssemblies.Assemblies);

            GameObject root = new GameObject("Views");
            for (int i = 0; i < ViewDefinitions.Length; ++i)
            {
                ViewDefinition def = ViewDefinitions[i];
                def.CacheViews(compMap);
                if ( def.Loader == ViewDefinition.ViewLoader.Fixed)
                {
                    if ( !m_Prefabs.TryGetPrefab(def.Prefab, out GameObject pfb))
                    {
                        Console.Error($"Prefab {def.Prefab} not found");
                        continue;
                    }

                    def.CachedPrefab = pfb;
                    
                    if (def.InitialPoolSize > 0)
                    {
                        PoolManager.InitializePool(pfb, root.transform, def.InitialPoolSize);

                    }
                    else
                    {
                        Console.Warning($"Prefab {pfb.name} was not initialized, pool size not specified");
                    }
                }
            }

            QueueRequests = false;
        }

        public override void Unload()
        {
            for(int i = m_Views.Count -1; i >= 0; --i)
            {
                FreeView(i);
            }
        }

        public ISimulationView FindView(int actorID)
        {
            return m_Views.Find(view => view.ActorID == actorID);
        }

        protected virtual bool ValidateView(GameActor targetActor, ViewDefinition viewDef) { return true; }

        protected void AddActor(GameActor actor)
        {
            List<string> comps = new List<string>();

            for(int i = 0; i < actor.Components.Length; ++i)
            {
                if (actor.Components[i] != -1)
                {

                    comps.Add(actor.ComponentTypes[i].Name);
                }
            }

            List<ViewDefinition> views = GetValidViews(comps.ToArray());
            if (views.Count > 0)
            {
                for (int i = 0; i < views.Count; ++i)
                {
                    ViewDefinition viewDef = views[i];

                    if (!ValidateView(actor, viewDef))
                    {
                        continue;
                    }

                    if ( QueueRequests )
                    {
                        m_QueuedActors.Add(ViewBinding.Create(actor, viewDef));
                    }
                    else
                    {
                        SpawnView(actor, viewDef);
                    }
                }
            }
        }

        protected virtual GameObject SpawnCustomView(ViewDefinition viewDef, GameActor actor, ActorFilter filter) { return null; }
        protected virtual void DestroyCustomView(ISimulationView view) { }

        protected void RemoveActor(GameActor actor, int reason = 0)
        {
            var views = m_Views.FindAll(v => v.ActorID == actor.ID);

            for (int i = m_Views.Count - 1; i >= 0; --i)
            {
                if (m_Views[i].ActorID == actor.ID && m_Views[i].NotifyActorDestroyed(reason))
                    FreeView(i);
            }
        }

        protected virtual GameObject GetInstance(GameObject prefab)
        {
            var inst = PoolManager.Get(prefab);
            return inst;
        }

        public override void Update()
        {
            if (!QueueRequests && m_QueuedActors.Count > 0 )
            {
                Console.Message($"{m_QueuedActors.Count} queued view detected, spawning...");
                for(int i = 0; i < m_QueuedActors.Count; ++i)
                {
                    // check actor still valid
                    if ( m_Simulation.Simulation.Filter.TryGetActor(m_QueuedActors[i].ActorID, out GameActor actor))
                    {
                        SpawnView(actor, m_QueuedActors[i].View);
                    }
                }
                m_QueuedActors.Clear();
            }
            UpdateViews();
        }

        protected void UpdateViews()
        {
            for (int i = 0; i < m_Views.Count;)
            {
                m_Views[i].UpdateView(Time.deltaTime);

                if (m_Views[i].Destroy)
                {
                    FreeView(i);
                }
                else
                {
                    ++i;
                }
            }
        }

        public void SetupView(ISimulationView view, GameActor actor)
        {
            if (view is IContextReader)
                ((IContextReader)view).SetContext(m_Simulation.Context);
            if (view is IContentReader)
                ((IContentReader)view).SetContent(m_Content);
            if (view is IDirtAccess)
                ((IDirtAccess)view).SetMode(m_Mode);
            if (view is IEventReader)
                m_Simulation.RegisterEventReader(((IEventReader)view));

            view.SetActor(actor, m_Simulation.Simulation.Filter);

            m_Views.Add(view);
        }


        private List<ViewDefinition> GetValidViews(string[] compList)
        {
            List<ViewDefinition> res = new List<ViewDefinition>();
            for (int i = 0; i < ViewDefinitions.Length; ++i)
            {
                ViewDefinition viewDef = ViewDefinitions[i];
                int dups = CountDuplicates(compList, viewDef.Components);
                if (CountDuplicates(compList, viewDef.Components) == viewDef.Components.Length)
                    res.Add(viewDef);

            }
            return res;
        }

        private int CountDuplicates(string[] l1, string[] l2)
        {
            int res = 0;
            for (int i = 0; i < l1.Length; ++i)
            {

                for (int j = 0; j < l2.Length; ++j)
                {
                    if (l1[i] == l2[j])
                    {
                        res++;
                        break;
                    }

                }
            }
            return res;
        }

        private void SpawnView(GameActor actor, ViewDefinition viewDef)
        {
            GameObject inst = null;

            if (viewDef.Loader == ViewLoader.Fixed)
            {
                inst = GetInstance(viewDef.CachedPrefab);
            }
            else if (viewDef.Loader == ViewLoader.Generic)
            {
                string prefabName = GetPrefabName(viewDef, actor);

                if (m_Prefabs.TryGetPrefab(prefabName, out GameObject prefab))
                {
                    inst = GetInstance(prefab);
                }
                else
                {
                    Console.Warning($"Prefab {prefabName} not found or referenced");
                }
            }
            else
            {

                inst = SpawnCustomView(viewDef, actor, m_Simulation.Simulation.Filter);
                if (inst == null)
                    throw new System.Exception("Custom Spawner did not return a valid GameObject");
            }

            ISimulationView simView = inst.GetComponent<ISimulationView>();
            SetupView(simView, actor);
        }

        private string GetPrefabName(ViewDefinition view, GameActor actor)
        {
            switch (view.Loader)
            {
                default:
                case ViewDefinition.ViewLoader.Fixed: return view.Prefab;
                case ViewDefinition.ViewLoader.Generic: return GetGenericPrefab(view, actor);
            }
        }

        private string GetGenericPrefab(ViewDefinition viewDef, GameActor actor)
        {
            for (int i = 0; i < actor.ComponentTypes.Length; ++i)
            {
                if (actor.ComponentTypes[i] == viewDef.CachedType)
                {
                    string prefabSuffix = (string)viewDef.CachedField.GetValue(actor.Components[i]);
                    return $"{viewDef.Prefix}{prefabSuffix}";
                }
            }
            throw new System.Exception($"Actor has no {viewDef.Component} component");
        }

        private void FreeView(int idx)
        {
            ISimulationView view = m_Views[idx];
            view.CleanView();
            m_Views.Remove(view);

            if ( view.UseCustomLoader )
            {
                DestroyCustomView(view);
            }
            else
            {
                Component comp = (Component)view;

                if (comp != null)
                {
                    PoolManager.Free(comp.gameObject);
                }
            }

            if (view is IEventReader)
            {
                m_Simulation.RemoveEventReader((IEventReader)view);
            }
        }


        private struct ViewBinding
        {
            public int ActorID;
            public ViewDefinition View;
            public static ViewBinding Create(GameActor actor, ViewDefinition viewDef)
                => new ViewBinding() { ActorID = actor.ID, View = viewDef };
            public static ViewBinding Create(int actorID, ViewDefinition viewDef)
                => new ViewBinding() { ActorID = actorID, View = viewDef };
        }
    }
}