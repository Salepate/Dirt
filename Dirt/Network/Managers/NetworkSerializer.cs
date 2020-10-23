using Dirt.Game;
using Dirt.Network.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Type = System.Type;

namespace Dirt.Network.Managers
{
    public class NetworkSerializer : IGameManager
    {
        private NetSerializer.Serializer m_Serializer;

        public NetworkSerializer(NetworkTypes assemblies)
        {
            Assembly[] loadedAsses = AppDomain.CurrentDomain.GetAssemblies();
            IEnumerable<Assembly> gameAssemblies = loadedAsses.Where(ass => assemblies.Assemblies.Contains(ass.FullName));

            IEnumerable<Type> serializableTypes = gameAssemblies.SelectMany(ass =>
            {
                IEnumerable<Type> eventTypes = CollectAbstractTypes<NetworkEvent>(ass);
                IEnumerable<Type> gameTypes = ass.GetTypes().Where(t => assemblies.Types.Contains(t.FullName));
                return gameTypes.Concat(eventTypes);
            });

            serializableTypes = serializableTypes.Concat(new Type[] { typeof(MessageHeader) });
            m_Serializer = new NetSerializer.Serializer(serializableTypes.Where(t => t != null));
        }

        public void RegisterTypes(Type[] types)
        {
            m_Serializer.AddTypes(types);
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
    }
}
