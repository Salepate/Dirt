using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

namespace Dirt.Simulation.Context
{
    public class SimulationContext
    {
        private Dictionary<Type, object> m_Context;

        public SimulationContext()
        {
            m_Context = new Dictionary<Type, object>();
        }

        public void ClearContext()
        {
            m_Context.Clear();
        }

        public void CreateContext(string contextTypeName, JToken contextContent)
        {
            Type contextType = Type.GetType(contextTypeName);

            if (typeof(IContextItem).IsAssignableFrom(contextType))
            {
                object contextObj = contextContent.ToObject(contextType);
                m_Context.Add(contextType, contextObj);
            }
            else
            {
                throw new Exception($"Invalid Context Type {contextTypeName}");
            }
        }

        public void SetContext(object contextObject)
        {
            Type contextType = contextObject.GetType();

            if (typeof(IContextItem).IsAssignableFrom(contextType))
            {
                if (!m_Context.ContainsKey(contextType))
                {
                    m_Context.Add(contextType, contextObject);
                }
            }
            else
            {
                throw new Exception($"{contextType.FullName} does not implement {nameof(IContextItem)}");
            }
        }

        public T GetContext<T>() where T : IContextItem
        {
            if (!m_Context.TryGetValue(typeof(T), out object subContext))
            {
                return default(T);
            }
            return (T)subContext;
        }
    }
}
