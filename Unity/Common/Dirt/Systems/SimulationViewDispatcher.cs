using Dirt.Game.Content;
using Dirt.Log;
using Dirt.Model;
using Dirt.Simulation;
using Dirt.Simulation.Context;
using Dirt.Simulation.SystemHelper;
using Dirt.Simulation.Utility;
using Dirt.Simulation.View;
using Framework;
using System.Collections.Generic;
using UnityEngine;

namespace Dirt.Systems
{
    public abstract class SimulationViewDispatcher : DirtSystem, IContentSystem
    {
        protected virtual bool IsDebug => false;
        public override bool HasUpdate => true;

        protected PoolManager PoolManager;
        protected abstract ViewDefinition[] ViewDefinitions { get; }
        private PrefabService m_Prefabs;
        private List<ISimulationView> m_Views;
        private IContentProvider m_Content;
        private SimulationSystem m_Simulation;
        private DirtMode m_Mode;

        public override void Initialize(DirtMode mode)
        {
            PoolManager = new PoolManager(IsDebug);
            m_Views = new List<ISimulationView>();
            m_Prefabs = mode.FindSystem<PrefabService>();
            m_Content = mode.FindSystem<ContentSystem>().Content;
            m_Mode = mode;

            m_Simulation = mode.FindSystem<SimulationSystem>();
            Dictionary<string, System.Type> compMap = AssemblyReflection.BuildTypeMap<IComponent>(m_Simulation.ValidAssemblies.Assemblies);

            for (int i = 0; i < ViewDefinitions.Length; ++i)
            {
                ViewDefinitions[i].CacheViews(compMap);
            }
        }

        public ISimulationView FindView(int actorID)
        {
            return m_Views.Find(view => view.ActorID == actorID);
        }

        protected virtual bool SpawnView(ViewDefinition viewDef, GameActor targetActor) { return true; }

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
                    string prefabName = GetPrefabName(viewDef, actor);

                    if ( !SpawnView(viewDef, actor))
                    {
                        continue;
                    }

                    if (m_Prefabs.TryGetPrefab(prefabName, out GameObject prefab))
                    {
                        var inst = GetInstance(prefab);
                        var simView = inst.GetComponent<ISimulationView>();
                        SetupView(simView);
                        simView.SetActor(actor, m_Simulation.Simulation.Filter);
                        m_Views.Add(simView);
                    }
                    else
                    {
                        Console.Warning($"Prefab {prefabName} not found or referenced");
                    }
                }

            }
        }

        protected void RemoveActor(GameActor actor)
        {
            var views = m_Views.FindAll(v => v.ActorID == actor.ID);

            for (int i = m_Views.Count - 1; i >= 0; --i)
            {
                if (m_Views[i].ActorID == actor.ID && m_Views[i].NotifyActorDestroyed())
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

        public void SetupView(ISimulationView view)
        {
            if (view is IContextReader)
                ((IContextReader)view).SetContext(m_Simulation.Context);
            if (view is IContentReader)
                ((IContentReader)view).SetContent(m_Content);
            if (view is IDirtAccess)
                ((IDirtAccess)view).SetMode(m_Mode);
            if (view is IEventReader)
                m_Simulation.RegisterEventReader(((IEventReader)view));
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
            m_Views.Remove(view);

            Component comp = (Component)view;

            if (comp != null)
            {
                PoolManager.Free(comp.gameObject);
            }
        }
    }
}