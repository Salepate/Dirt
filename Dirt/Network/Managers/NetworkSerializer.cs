using Dirt.Game;
using Dirt.Log;
using Dirt.Network.Internal;
using Dirt.Network.Model;
using Dirt.Network.Simulation;
using Dirt.Simulation;
using Dirt.Simulation.Actor;
using Dirt.Simulation.Components;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Dirt.Network.Managers
{
    using Console = Dirt.Log.Console;
    public class NetworkSerializer : IGameManager
    {
        private NetSerializer.Serializer m_Serializer;
        private static MethodInfo s_CreateComponentMethodInfo;
        private static Dictionary<Type, ObjectFieldAccessor[]> s_ComponentSetters;
        private static Dictionary<Type, ComponentSerializer> s_Serializers;
        static NetworkSerializer()
        {
            s_CreateComponentMethodInfo = typeof(NetworkSerializer).GetMethod("CreateComponentMeta", BindingFlags.NonPublic | BindingFlags.Instance);
            s_ComponentSetters = new Dictionary<Type, ObjectFieldAccessor[]>();
            s_Serializers = new Dictionary<Type, ComponentSerializer>();
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
                    if (typeof(INetComponent).IsAssignableFrom(comp) || comp == typeof(Position))
                    {
                        CreateComponentSerializer(comp);
                    }
                }

                return gameTypes.Concat(eventTypes).Concat(compTypes);
            });

            serializableTypes = serializableTypes.Concat(new Type[] { typeof(MessageHeader), typeof(ActorState) }.Where(t => t != null));
            var validTypes = serializableTypes.OrderBy(t => t.FullName).ToList();
            m_Serializer = new NetSerializer.Serializer(validTypes);
        }

        internal static bool TryGetSerializer(Type compType, out ComponentSerializer serializer)
        {
            return s_Serializers.TryGetValue(compType, out serializer);
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

        private void CreateComponentSerializer(Type compType)
        {
            if (s_Serializers.ContainsKey(compType))
            {
                Console.Warning($"Serializer already has {compType.Name}");
                return;
            }
            // Raw
            ComponentSerializer serializer = new ComponentSerializer();
            if (typeof(INetComponent).IsAssignableFrom(compType))
            {
                serializer.UseNetSerializer = true;
            }
            else
            {
                if (compType == typeof(Position))
                {
                    serializer.IsPosition = true;
                }
            }

            s_Serializers.Add(compType, serializer);
        }
    }
}
