using Dirt.Log;
using System.Collections.Generic;

namespace Dirt.Simulation.Actor
{
    using Type = System.Type;

    public class SimulationPool
    {
        public const int DefaultComponentSize = 64; // different comps
        public bool AllowLazy { get; set; }
        public Dictionary<Type, GenericArray> Pools;

        private int m_MaximumActor;
        private int m_ComponentPools;
        private GenericArray[] m_Pools;
        public SimulationPool(int maxActor)
        {
            m_MaximumActor = maxActor;
            Pools = new Dictionary<Type, GenericArray>();
            m_Pools = new GenericArray[DefaultComponentSize];
            m_ComponentPools = 0;
        }

        public void RegisterComponentArray<T>(int size = 0) where T : struct
        {
            Console.Assert(!Pools.ContainsKey(typeof(T)), "Pool declared twice");

            if (size <= 0)
            {
                size = m_MaximumActor;
            }

            if (size > m_MaximumActor)
            {
                Console.Warning($"Pool {typeof(T).Name} size ({size}) exceeds actor limit {m_MaximumActor}");
                size = m_MaximumActor;
            }
            ComponentArray<T> arr = new ComponentArray<T>();
            arr.SetSize(size);
            Pools.Add(typeof(T), arr);
            RegisterPool(arr);
        }

        public void RegisterComponentArray(Type componentType, int size = 0)
        {
            Console.Assert(!Pools.ContainsKey(componentType), "Pool declared twice");

            if (size <= 0)
            {
                size = m_MaximumActor;
            }

            if (size > m_MaximumActor)
            {
                Console.Warning($"Pool {componentType.Name} size ({size}) exceeds actor limit {m_MaximumActor}");
                size = m_MaximumActor;
            }

            var compArrayType = typeof(ComponentArray<>).MakeGenericType(componentType);
            GenericArray genArray = (GenericArray) System.Activator.CreateInstance(compArrayType);
            genArray.SetSize(size);
            Pools.Add(componentType, genArray);
            RegisterPool(genArray);
        }

        public ComponentArray<T> GetPool<T>() where T : struct
        {
            if (AllowLazy && !Pools.ContainsKey(typeof(T)))
            {
                Console.Warning($"Lazy Pool {typeof(T).Name} ({m_MaximumActor})");
                RegisterComponentArray<T>(m_MaximumActor);
            }
            return (ComponentArray<T>)Pools[typeof(T)];
        }

        public GenericArray GetPoolByIndex(int index)
        {
            return m_Pools[index];
        }

        public GenericArray GetPool(System.Type type)
        {
            if (AllowLazy && !Pools.ContainsKey(type))
            {
                Console.Warning($"Lazy Pool {type.Name} ({m_MaximumActor})");
                RegisterComponentArray(type);
            }
            return (GenericArray)Pools[type];
        }

        private void RegisterPool(in GenericArray pool)
        {
            pool.Index = m_ComponentPools;
            m_Pools[m_ComponentPools] = pool;
            if (++m_ComponentPools >= m_Pools.Length)
            {
                Console.Warning($"Maximum components reached {m_Pools.Length}, doubling size");
                GenericArray[] newArr = new GenericArray[m_Pools.Length * 2];
                for (int i = 0; i < m_Pools.Length; ++i)
                {
                    newArr[i] = m_Pools[i];
                }
                m_Pools = newArr;
            }
        }
    }
}
