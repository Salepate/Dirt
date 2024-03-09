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

                while(contextType != typeof(object))
                {
                    m_Context.Add(contextType, contextObj);
                    contextType = contextType.BaseType;
                }
            }
            else
            {
                throw new Exception($"Invalid Context Type {contextTypeName}");
            }
        }

        public T CreateContext<T>() where T: IContextItem, new()
        {
            T ctx = new T();
            m_Context.Add(typeof(T), ctx);
            return ctx;
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
                else
                {
                    Dirt.Log.Console.Message($"Context override: {contextType.Name}");
                    m_Context[contextType] = contextObject;
                }
            }
            else
            {
                throw new Exception($"{contextType.FullName} does not implement {nameof(IContextItem)}");
            }
        }

        /// <summary>
        /// Return a specific context from simulation
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns>null if the context does not exist</returns>
        public T GetContext<T>() where T : class, IContextItem
        {
            if (!m_Context.TryGetValue(typeof(T), out object subContext))
            {
                return null;
            }
            return (T)subContext;
        }
    }
}
