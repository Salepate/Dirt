using Dirt.Log;
using Dirt.Network;
using System.Collections.Generic;

using Type = System.Type;

namespace Dirt.Systems
{
    using GenericListener = System.Action<NetworkEvent>;
    public class NetworkEventDispatcher : DirtSystem
    {
        private Dictionary<Type, GenericListener> m_Listeners;
        private Dictionary<object, GenericListener> m_LambdaMap;
        public override void Initialize(DirtMode mode)
        {
            m_Listeners = new Dictionary<Type, GenericListener>();
            m_LambdaMap = new Dictionary<object, GenericListener>();
        }

        public void Dispatch(NetworkEvent NetworkEvent)
        {
            Type eventType = NetworkEvent.GetType();
            Console.Message($"Game Event {eventType.Name}");
            if (m_Listeners.TryGetValue(eventType, out GenericListener listeners))
            {
                listeners(NetworkEvent);
            }
        }

        public void Listen<EventType>(System.Action<EventType> listener) where EventType : NetworkEvent
        {
            if (!m_Listeners.TryGetValue(typeof(EventType), out GenericListener listeners))
            {
                listeners = GetLambda<EventType>(listener);
            }
            else
            {
                listeners += GetLambda<EventType>(listener);
            }
            m_Listeners[typeof(EventType)] = listeners;
        }

        public void RemoveListener<EventType>(System.Action<EventType> listener) where EventType : NetworkEvent
        {
            if (m_LambdaMap.ContainsKey(listener))
            {
                GenericListener lambda = GetLambda<EventType>(listener);
                if (m_Listeners.TryGetValue(typeof(EventType), out GenericListener listeners))
                {
                    listeners -= lambda;
                    if (listeners == null)
                        m_Listeners.Remove(typeof(EventType));
                    else
                        m_Listeners[typeof(EventType)] = listeners;
                }
                m_LambdaMap.Remove(listener);
            }
        }


        private GenericListener GetLambda<EventType>(System.Action<EventType> listener) where EventType : NetworkEvent
        {
            if (!m_LambdaMap.TryGetValue(listener, out GenericListener lambda))
            {
                lambda = (NetworkEvent) => listener((EventType)NetworkEvent);
                m_LambdaMap.Add(listener, lambda);
            }
            return lambda;
        }
    }
}
