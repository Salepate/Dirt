using Dirt.Game.Metrics;
using System.Collections.Generic;

using Type = System.Type;

namespace Dirt.Game.Managers
{
    public class MetricsManager : IGameManager
    {
        public const string CONDITIONAL_METRICS = "GAME_METRICS";

        public System.Action<string, int> MetricEvent;
        public delegate void MetricObjectEventDelegate(string hash, MetricObject obj);

        public Dictionary<string, int> IntegerMetrics { get; private set; }
        public Dictionary<string, MetricObject> ObjectMetrics { get; private set; }

        private Dictionary<Type, MetricObjectEventDelegate> m_Listeners;
        private Dictionary<object, MetricObjectEventDelegate> m_LambdaMap;

        public MetricsManager()
        {
            IntegerMetrics = new Dictionary<string, int>();
            ObjectMetrics = new Dictionary<string, MetricObject>();
            m_Listeners = new Dictionary<Type, MetricObjectEventDelegate>();
            m_LambdaMap = new Dictionary<object, MetricObjectEventDelegate>();
        }

        public void ListenMetricObjectEvent<T>(System.Action<string, T> listener) where T : MetricObject
        {
            if (!m_Listeners.TryGetValue(typeof(T), out MetricObjectEventDelegate del))
            {
                MetricObjectEventDelegate lambda = (string hash, MetricObject obj) => { listener(hash, (T)obj); };
                m_Listeners[typeof(T)] = lambda;
                m_LambdaMap.Add(listener, lambda);
            }
        }

        public void RemoveMetricObjectEventListener<T>(System.Action<string, T> listener) where T : MetricObject
        {
            if (m_LambdaMap.TryGetValue(listener, out MetricObjectEventDelegate del))
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
            ObjectMetrics[hash] = data;
            if (m_Listeners.TryGetValue(data.GetType(), out MetricObjectEventDelegate lambda))
            {
                lambda(hash, data);
            }
        }

        public void Record<T>(string hash, T data) where T : MetricObject
        {
            ObjectMetrics[hash] = data;
            if (m_Listeners.TryGetValue(typeof(T), out MetricObjectEventDelegate lambda))
            {
                lambda(hash, data);
            }
        }

        public void Record(string hash, int value, bool deltaOnly = true)
        {
            if (!IntegerMetrics.TryGetValue(hash, out int oldValue) || oldValue != value || !deltaOnly)
            {
                MetricEvent?.Invoke(hash, value);
            }
            IntegerMetrics[hash] = value;
        }

        public void Update(float deltaTime)
        {
        }
    }
}
