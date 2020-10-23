using System.Collections.Generic;
using Type = System.Type;

namespace Dirt.Game.Metrics
{
    public class MetricsManager : IGameManager
    {
        public const string CONDITIONAL_METRICS = "GAME_METRICS";

        public System.Action<string, int> MetricEvent;
        public delegate void MetricObjectEventDelegate(string hash, MetricObject obj);

        private Dictionary<string, int> m_Metrics;
        private Dictionary<string, MetricObject> m_Objects;
        private Dictionary<Type, MetricObjectEventDelegate> m_Listeners;
        private Dictionary<object, MetricObjectEventDelegate> m_LambdaMap;

        public MetricsManager()
        {
            m_Metrics = new Dictionary<string, int>();
            m_Objects = new Dictionary<string, MetricObject>();
            m_Listeners = new Dictionary<Type, MetricObjectEventDelegate>();
            m_LambdaMap = new Dictionary<object, MetricObjectEventDelegate>();
        }
        
        public void ListenMetricObjectEvent<T>(System.Action<string, T> listener) where T: MetricObject
        {
            if (!m_Listeners.TryGetValue(typeof(T), out MetricObjectEventDelegate del))
            {
                MetricObjectEventDelegate lambda = (string hash, MetricObject obj) => { listener(hash, (T)obj); };
                m_Listeners[typeof(T)] = lambda;
                m_LambdaMap.Add(listener, lambda);
            }
        }

        public void RemoveMetricObjectEventListener<T>(System.Action<string, T> listener) where T: MetricObject
        {
            if ( m_LambdaMap.TryGetValue(listener, out MetricObjectEventDelegate del))
            {
                Type t = typeof(T);
                m_LambdaMap.Remove(listener);
                m_Listeners[t] -= del;
                if (m_Listeners[t] == null)
                    m_Listeners.Remove(t);
            }
        }

        public void Record(string hash, MetricObject data)
        {
            m_Objects[hash] = data;
            if (m_Listeners.TryGetValue(data.GetType(), out MetricObjectEventDelegate lambda))
            {
                lambda(hash, data);
            }
        }

        public void Record<T>(string hash, T data) where T: MetricObject
        {
            m_Objects[hash] = data;
            if ( m_Listeners.TryGetValue(typeof(T), out MetricObjectEventDelegate lambda) )
            {
                lambda(hash, data);
            }
        }

        public void Record(string hash, int value, bool deltaOnly = true)
        {
            if ( !m_Metrics.TryGetValue(hash, out int oldValue) || oldValue != value || !deltaOnly)
            {
                MetricEvent?.Invoke(hash, value);
            }
            m_Metrics[hash] = value;
        }

        public void Update(float deltaTime)
        {
        }
    }
}
