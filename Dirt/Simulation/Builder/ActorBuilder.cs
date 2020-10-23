using System.Collections.Generic;

using Type = System.Type;
using Console = Dirt.Log.Console;
using Dirt.Simulation.Model;
using Dirt.Simulation.Utility;

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

        public ActorBuilder(int actorPoolSize)
        {
            m_Injectors = new Dictionary<Type, ComponentInjector>();
            m_Actors = new GameActor[actorPoolSize];
            m_LastFreeIndex = 0;
            m_ValidComponents = new Dictionary<string, Type>();
            for (int i = 0; i < actorPoolSize; ++i)
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

        public void CreateActor(IComponent[] baseComponents)
        {
            GameActor actor = GetActor();
            for (int i = 0; i < baseComponents.Length; ++i)
            {
                if ( baseComponents[i] != null )
                    actor.AddComponent(baseComponents[i]);
            }
            ActorCreateAction?.Invoke(actor);
        }

        public GameActor BuildActor(ActorArchetype archetype)
        {
            GameActor actor = GetActor();
            InternalBuild(actor, archetype);
            ActorCreateAction?.Invoke(actor);
            return actor;
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
