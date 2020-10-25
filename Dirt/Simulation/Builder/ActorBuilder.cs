using System.Collections.Generic;

using Type = System.Type;
using Console = Dirt.Log.Console;
using Dirt.Simulation.Model;
using Dirt.Simulation.Utility;
using Dirt.Game.Content;

namespace Dirt.Simulation.Builder
{
    public class ActorBuilder
    {
        public System.Action<GameActor> ActorCreateAction;
        public System.Action<GameActor> ActorDestroyAction;

        private GameActor[] m_Actors;

        private int m_LastFreeIndex;

        private Dictionary<Type, ComponentInjector> m_Injectors;

        private Dictionary<string, Type> m_ValidComponents;

        protected IContentProvider Content;

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
            m_Actors = new GameActor[0];
            m_LastFreeIndex = 0;
            m_ValidComponents = new Dictionary<string, Type>();
        }

        public void SetGameContent(IContentProvider content)
        {
            Content = content;
        }

        public void InitializePool(int poolSize)
        {
            m_Actors = new GameActor[poolSize];
            for (int i = 0; i < poolSize; ++i)
                m_Actors[i] = new GameActor(i);
        }

        public void LoadAssemblies(AssemblyCollection collection)
        {
            m_ValidComponents = AssemblyReflection.BuildTypeMap<IComponent>(collection.Assemblies);
        }

        public GameActor CreateActor()
        {
            GameActor actor = GetActor();
            ActorCreateAction?.Invoke(actor);
            return actor;
        }

        public GameActor CreateActor(IComponent[] baseComponents)
        {
            GameActor actor = GetActor();
            for (int i = 0; i < baseComponents.Length; ++i)
            {
                if ( baseComponents[i] != null )
                    actor.AddComponent(baseComponents[i]);
            }
            ActorCreateAction?.Invoke(actor);
            return actor;
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
            int freeSlot = GetEmptyIndex();
            if (freeSlot != -1)
            {
                ActorDestroyAction?.Invoke(actor);
                actor.ResetActor();
                m_Actors[freeSlot] = actor;
                m_LastFreeIndex = freeSlot;
            }
            else
            {
                throw new System.Exception("Unable to destroy game actor");
            }
        }

        public void RegisterComponent(Type compType)
        {
            m_Injectors.Add(compType, new ComponentInjector(compType));
        }

        // internal
        private GameActor GetActor()
        {
            if (m_LastFreeIndex >= m_Actors.Length || m_LastFreeIndex == -1)
                throw new System.Exception($"Actor Limit exceeded {m_LastFreeIndex}");

            GameActor actor = m_Actors[m_LastFreeIndex];
            m_Actors[m_LastFreeIndex] = null;
            m_LastFreeIndex = GetNextFreeIndex(m_LastFreeIndex + 1);
            actor.SetVersion((actor.Version + 1) % 256);
            return actor;
        }

        private int GetEmptyIndex()
        {
            for (int i = m_LastFreeIndex - 1; i >= 0; --i)
            {
                if (m_Actors[i] == null)
                    return i;
            }

            return -1;
        }

        private int GetNextFreeIndex(int from)
        {
            for(int i = from; i < m_Actors.Length; ++i)
            {
                if (m_Actors[i] != null)
                    return i;
            }

            return m_Actors.Length;
        }

        private void InternalBuild(GameActor actor, ActorArchetype archetype)
        {
            for(int i = 0; i < archetype.Components.Length; ++i)
            {
                string compName = archetype.Components[i];
                m_ValidComponents.TryGetValue(compName, out Type compType);
                if ( compType != null )
                {
                    IComponent comp = (IComponent) System.Activator.CreateInstance(compType);

                    if ( archetype.Settings != null && archetype.Settings.TryGetValue(compType.Name, out ComponentParameters parameters))
                    {
                        InjectParameters(comp, parameters);
                    }

                    actor.AddComponent(comp);
                }
                else
                {
                    Console.Message($"Invalid Component {compName}");
                }
            }
        }

        private void InjectParameters(IComponent component, ComponentParameters parameters)
        {
            Type compType = component.GetType();

            if ( !m_Injectors.TryGetValue(compType, out ComponentInjector injector))
            {
                RegisterComponent(compType);
                injector = m_Injectors[compType];
                Console.Warning($"Lazy Component Registration {compType.Name}");
            }
            injector.Inject(component, parameters);
        }
    }
}
