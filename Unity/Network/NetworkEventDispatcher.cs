using Dirt.Log;
using Dirt.Network;
using System.Collections.Generic;

using Type = System.Type;

namespace Dirt.Systems
{
    using GenericListener = System.Action<NetworkEvent>;
    public class NetworkEventDispatcher : DirtSystem
    {
        private Dictionary<Type, OrderedListener> m_Listeners;
        private Dictionary<object, GenericListener> m_LambdaMap;
        public override void Initialize(DirtMode mode)
        {
            m_Listeners = new Dictionary<Type, OrderedListener>();
            m_LambdaMap = new Dictionary<object, GenericListener>();
        }

        public void Dispatch(NetworkEvent NetworkEvent)
        {
            Type eventType = NetworkEvent.GetType();
            //Console.Message($"Game Event {eventType.Name}");
            if (m_Listeners.TryGetValue(eventType, out OrderedListener listeners))
            {
                listeners.Dispatch(NetworkEvent);
            }
        }

        public void Listen<EventType>(System.Action<EventType> listener, int slot = -1) where EventType : NetworkEvent
        {
            if (!m_Listeners.TryGetValue(typeof(EventType), out OrderedListener listeners))
            {
                listeners = new OrderedListener();
                m_Listeners[typeof(EventType)] = listeners;
            }

            listeners.Add(GetLambda(listener), slot);
        }

        public void RemoveListener<EventType>(System.Action<EventType> listener) where EventType : NetworkEvent
        {
            if (m_LambdaMap.ContainsKey(listener))
            {
                GenericListener lambda = GetLambda<EventType>(listener);
                if (m_Listeners.TryGetValue(typeof(EventType), out OrderedListener listeners))
                {
                    listeners.Remove(lambda);

                    if (listeners.Count == 0)
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

        private class OrderedListener
        {
            private List<GenericListener> m_Listeners;

            public OrderedListener()
            {
                m_Listeners = new List<GenericListener>();
            }

            public int Count => m_Listeners.Count;

            public void Add(GenericListener listener, int slot)
            {
                if ( slot == -1 )
                {
                    m_Listeners.Add(listener);
                }
                else
                {
                    m_Listeners.Insert(slot, listener);
                }
            }

            public void Remove(GenericListener listener)
            {
                int idx = m_Listeners.IndexOf(listener);
                if ( idx != -1)
                {
                    m_Listeners.RemoveAt(idx);
                }
            }

            public void Dispatch(NetworkEvent networkEvent)
            {
                for(int i = 0; i < m_Listeners.Count && !networkEvent.Consumed; ++i)
                {
                    m_Listeners[i](networkEvent);
                }
            }
        }
    }
}
