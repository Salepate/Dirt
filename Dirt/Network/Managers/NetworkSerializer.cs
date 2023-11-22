using Dirt.Game;
using Dirt.Network.Internal;
using Dirt.Network.Model;
using Dirt.Network.Simulation;
using Dirt.Simulation;
using Dirt.Simulation.Actor;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Dirt.Network.Managers
{
    public class NetworkSerializer : IGameManager
    {
        private NetSerializer.Serializer m_Serializer;
        private static MethodInfo s_CreateComponentMethodInfo;
        private static Dictionary<Type, ObjectFieldAccessor[]> s_ComponentSetters;

        static NetworkSerializer()
        {
            s_CreateComponentMethodInfo = typeof(NetworkSerializer).GetMethod("CreateComponentMeta", BindingFlags.NonPublic | BindingFlags.Instance);
            s_ComponentSetters = new Dictionary<Type, ObjectFieldAccessor[]>();
        }

        public NetworkSerializer(NetworkTypes assemblies)
        {
            List<Assembly> loadedAsses = AppDomain.CurrentDomain.GetAssemblies().ToList();


            IEnumerable<Assembly> gameAssemblies = loadedAsses.Where(ass => assemblies.Assemblies.Contains(ass.FullName));
            IEnumerable<Type> serializableTypes = gameAssemblies.SelectMany(ass =>
            {
                IEnumerable<Type> eventTypes = CollectAbstractTypes<NetworkEvent>(ass);
                IEnumerable<Type> gameTypes = ass.GetTypes().Where(t => assemblies.Types.Contains(t.FullName));
                IEnumerable<Type> compTypes = CollectInterfaceTypes<IComponent>(ass);

                foreach (Type comp in compTypes)
                {
                    CreateComponentSetters(comp);
                }

                return gameTypes.Concat(eventTypes).Concat(compTypes);
            });

            serializableTypes = serializableTypes.Concat(new Type[] { typeof(MessageHeader), typeof(ActorState) }.Where(t => t != null));
            var validTypes = serializableTypes.OrderBy(t => t.FullName).ToList();
            m_Serializer = new NetSerializer.Serializer(validTypes);
        }

        internal static bool TryGetSetters(Type compType, out ObjectFieldAccessor[] setters)
        {
            return s_ComponentSetters.TryGetValue(compType, out setters);
        }

        public void Serialize(MemoryStream st, object obj)
        {
            m_Serializer.Serialize(st, obj);
        }

        public object Deserialize(MemoryStream stream)
        {
            return m_Serializer.Deserialize(stream);
        }

        public void Update(float deltaTime)
        {
        }

        private IEnumerable<Type> CollectAbstractTypes<C>(Assembly ass)
        {
            Type abstractType = typeof(C);
            return ass.GetTypes().Where(t => t.IsSubclassOf(abstractType) && t.GetCustomAttribute<SerializableAttribute>() != null);
        }
        private IEnumerable<Type> CollectInterfaceTypes<C>(Assembly ass)
        {
            return ass.GetTypes().Where(t => typeof(C).IsAssignableFrom(t) && t.GetCustomAttribute<SerializableAttribute>() != null);
        }

        private void CreateComponentMeta<T>() where T : struct, IComponent
        {
            Type compType = typeof(T);

            if (s_ComponentSetters.ContainsKey(compType))
                return;

            FieldInfo[] pubFields = compType.GetFields(BindingFlags.FlattenHierarchy | BindingFlags.Public | BindingFlags.Instance);
            List<ObjectFieldAccessor> accessors = new List<ObjectFieldAccessor>();
            for (int i = 0; i < pubFields.Length; ++i)
            {
                if (!pubFields[i].IsNotSerialized && pubFields[i].GetCustomAttribute<DisableSyncAttribute>() == null)
                {
                    FastInvoke.SetterAction<T> specializedSetter = FastInvoke.BuildUntypedSetter<T>(pubFields[i]);
                    Func<T, object> specializedGetter = FastInvoke.BuildUntypedGetter<T>(pubFields[i]);

                    Action<GenericArray, int, object> setterAction = (arr, idx, newValue) =>
                    {
                        ComponentArray<T> compArr = (ComponentArray<T>)arr;
                        specializedSetter(ref compArr.Components[idx], newValue);
                    };

                    Func<IComponent, object> genericGetter = (comp) =>
                    {
                        return specializedGetter((T)comp);
                    };

                    accessors.Add(new ObjectFieldAccessor()
                    {
                        Name = pubFields[i].Name,
                        Getter = genericGetter,
                        Setter = setterAction
                    });
                }
            }

            s_ComponentSetters.Add(compType, accessors.ToArray());
        }

        private void CreateComponentSetters(Type compType)
        {
            s_CreateComponentMethodInfo.MakeGenericMethod(compType).Invoke(this, null);
        }
    }
}
