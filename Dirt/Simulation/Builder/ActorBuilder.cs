using System.Collections.Generic;

using Type = System.Type;
using Console = Dirt.Log.Console;
using Dirt.Simulation.Model;
using Dirt.Simulation.Utility;
using Dirt.Game.Content;
using Dirt.Simulation.Actor;
using Dirt.Simulation.Exceptions;

namespace Dirt.Simulation.Builder
{
    public class ActorBuilder
    {
        public System.Action<GameActor> ActorCreateAction;
        public System.Action<GameActor> ActorDestroyAction;

        private GameActor[] m_AvailableActor;
        private GameActor[] m_ActorCollection;

        private int m_LastFreeIndex;

        private Dictionary<Type, ComponentInjector> m_Injectors;

        private Dictionary<string, Type> m_ValidComponents;

        protected IContentProvider Content;

        public SimulationPool Components { get; private set; }

        public ActorBuilder(int actorPoolSize)
        {
            m_Injectors = new Dictionary<Type, ComponentInjector>();
            m_LastFreeIndex = 0;
            m_ValidComponents = new Dictionary<string, Type>();
            InitializePool(actorPoolSize);
        }

        public ActorBuilder()
        {
            m_Injectors = new Dictionary<Type, ComponentInjector>();
            m_AvailableActor = new GameActor[0];
            m_LastFreeIndex = 0;
            m_ValidComponents = new Dictionary<string, Type>();
        }

        public void SetGameContent(IContentProvider content)
        {
            Content = content;
        }

        public void InitializePool(int poolSize)
        {
            m_AvailableActor = new GameActor[poolSize];
            m_ActorCollection = new GameActor[poolSize];
            for (int i = 0; i < poolSize; ++i)
            {
                m_AvailableActor[i] = new GameActor(i);
                m_ActorCollection[i] = m_AvailableActor[i];
            }

            Components = new SimulationPool(poolSize);
        }

        public GameActor GetActorByID(int actorID)
        {
            int internalID = actorID >> 8;
            int version = actorID & 0xFF;

            if (m_AvailableActor[internalID] == null && m_ActorCollection[internalID].Version == version)
            {
                return m_ActorCollection[internalID];
            }
            return null;
        }
        public void LoadAssemblies(AssemblyCollection collection)
        {
            m_ValidComponents = AssemblyReflection.BuildTypeMap<IComponent>(collection.Assemblies);
            foreach (KeyValuePair<string, Type> kvp in m_ValidComponents)
            {
                Console.Message($"Register Comp Pool {kvp.Key}");
                RegisterComponent(kvp.Value);
            }
        }

        public GameActor CreateActor()
        {
            GameActor actor = GetActor();
            ActorCreateAction?.Invoke(actor);
            return actor;
        }

        public int AddComponent(GameActor actor, Type compType)
        {
            if (compType != null)
            {
                int idx = actor.GetComponentIndex(compType);

                if (idx == -1)
                {
                    GenericArray pool = Components.GetPool(compType);
                    idx = pool.Allocate(actor.InternalID);
                    actor.AssignComponent(compType, idx);
                }
                return idx;
            }
            return -1;
        }

        public void RemoveComponent(GameActor actor, Type compType)
        {
            int compIdx = actor.GetComponentIndex(compType);
            if (compIdx == -1)
                throw new ComponentNotFoundException(compType);
            actor.UnassignComponent(compType);
            Components.GetPool(compType).Free(compIdx);
        }
        public ref C AddComponent<C>(GameActor actor) where C : struct
        {
            ComponentArray<C> pool = Components.GetPool<C>();
            int idx = actor.GetComponentIndex<C>();
            if (idx == -1)
            {
                idx = pool.Allocate(actor.InternalID);
                actor.AssignComponent<C>(idx);
            }
            return ref pool.Components[idx];
        }

        public void RemoveComponent<C>(GameActor actor) where C : struct
        {
            int compIdx = actor.GetComponentIndex<C>();
            if (compIdx == -1)
                throw new ComponentNotFoundException(typeof(C));
            actor.UnassignComponent<C>();
            Components.GetPool<C>().Free(compIdx);
        }

        public virtual GameActor BuildActor(ActorArchetype archetype)
        {
            GameActor actor = GetActor();
            InternalBuild(actor, archetype);
            ActorCreateAction?.Invoke(actor);
            return actor;
        }

        public virtual GameActor BuildActor(string archetype)
        {
            Console.Assert(Content != null, "Content Provider not set");
            return BuildActor(Content.LoadContent<ActorArchetype>($"actor.{archetype}"));
        }

        public void DestroyActor(GameActor actor)
        {
            int actorSlot = actor.InternalID;
            if (m_AvailableActor[actorSlot] == null)
            {
                ActorDestroyAction?.Invoke(actor);
                for (int i = 0; i < actor.Components.Length; ++i)
                {
                    if (actor.Components[i] != -1)
                    {
                        Components.GetPool(actor.ComponentTypes[i]).Free(actor.Components[i]);
                    }
                }
                actor.ResetActor();
                m_AvailableActor[actorSlot] = actor;

                if (actorSlot < m_LastFreeIndex)
                    m_LastFreeIndex = actorSlot;
            }
            else
            {
                throw new System.Exception("Unable to destroy game actor");
            }
        }

        public void RegisterComponent(Type compType)
        {
            m_Injectors.Add(compType, new ComponentInjector(compType));
            Components.RegisterComponentArray(compType);
        }

        public Type GetComponentType(string compName)
        {
            m_ValidComponents.TryGetValue(compName, out Type compType);
            return compType;
        }

        // internal
        private GameActor GetActor()
        {
            if (m_LastFreeIndex >= m_AvailableActor.Length || m_LastFreeIndex == -1)
                throw new System.Exception($"Actor Limit exceeded {m_LastFreeIndex}");

            GameActor actor = m_AvailableActor[m_LastFreeIndex];
            m_AvailableActor[m_LastFreeIndex] = null;
            m_LastFreeIndex = GetNextFreeIndex(m_LastFreeIndex + 1);
            actor.SetVersion((actor.Version + 1) % 256);
            return actor;
        }

        private int GetEmptyIndex()
        {
            for (int i = m_LastFreeIndex - 1; i >= 0; --i)
            {
                if (m_AvailableActor[i] == null)
                    return i;
            }

            return -1;
        }

        private int GetNextFreeIndex(int from)
        {
            for (int i = from; i < m_AvailableActor.Length; ++i)
            {
                if (m_AvailableActor[i] != null)
                    return i;
            }

            return m_AvailableActor.Length;
        }

        private void InternalBuild(GameActor actor, ActorArchetype archetype)
        {
            for (int i = 0; i < archetype.Components.Length; ++i)
            {
                string compName = archetype.Components[i];
                m_ValidComponents.TryGetValue(compName, out Type compType);
                if (compType != null)
                {
                    int idx = AddComponent(actor, compType);

                    if (archetype.Settings != null && archetype.Settings.TryGetValue(compType.Name, out ComponentParameters parameters))
                    {
                        InjectParameters(Components.GetPool(compType), idx, parameters);
                    }
                }
                else
                {
                    Console.Message($"Invalid Component {compName}");
                }
            }
        }

        private void InjectParameters(GenericArray comp, int compIndex, ComponentParameters parameters)
        {
            if (!m_Injectors.TryGetValue(comp.ComponentType, out ComponentInjector injector))
            {
                RegisterComponent(comp.ComponentType);
                injector = m_Injectors[comp.ComponentType];
                Console.Warning($"Lazy Component Registration {comp.ComponentType.Name}");
            }

            object genObj = comp.Get(compIndex);
            injector.Inject(genObj, parameters);
            comp.Set(compIndex, genObj);
        }
    }
}
