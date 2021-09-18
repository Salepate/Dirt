using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;

using Console = Dirt.Log.Console;
using Type = System.Type;

namespace Dirt.Simulation.Builder
{
    public class ComponentInjector
    {
        private static readonly Type[] SupportedTypes = new Type[]
        {
            typeof(string),
            typeof(int),
            typeof(float),
            typeof(bool)
        };
        public Type ComponentType { get; private set; }

        private Dictionary<string, FieldInfo> m_InjectableFields;

        private CultureInfo m_ParseCultureInfo;
        public ComponentInjector(Type componentType)
        {
            ComponentType = componentType;

            m_ParseCultureInfo = (CultureInfo) System.Threading.Thread.CurrentThread.CurrentCulture.Clone();
            m_ParseCultureInfo.NumberFormat.NumberDecimalSeparator = ".";

            var fields = ComponentType.GetFields().Where(field => SupportedTypes.Contains(field.FieldType) || field.FieldType.IsEnum);
            m_InjectableFields = fields.ToDictionary(field => field.Name);
        }


        public void Inject(object obj, Dictionary<string, string> compParams)
        {
            foreach (KeyValuePair<string, string> param in compParams)
            {
                if (m_InjectableFields.TryGetValue(param.Key, out FieldInfo field))
                {
                    InjectField(obj, field, param.Value);
                }
            }
        }

        public void Inject<C>(ref C obj, Dictionary<string, string> compParams)
        {
            foreach (KeyValuePair<string, string> param in compParams)
            {
                if (m_InjectableFields.TryGetValue(param.Key, out FieldInfo field))
                {
                    InjectField<C>(ref obj, field, param.Value);
                }
            }
        }

        private void InjectField<C>(ref C obj, FieldInfo field, string valueStr)
        {
            Type fieldType = field.FieldType;

            if (fieldType == typeof(string))
            {
                field.SetValue(obj, valueStr);
            }
            else if (fieldType == typeof(int))
            {
                field.SetValue(obj, int.Parse(valueStr, m_ParseCultureInfo));
            }
            else if (fieldType == typeof(float))
            {
                field.SetValue(obj, float.Parse(valueStr, m_ParseCultureInfo));
            }
            else if (fieldType == typeof(bool))
            {
                field.SetValue(obj, bool.Parse(valueStr));
            }
            else if (fieldType.IsEnum)
            {
                field.SetValue(obj, Enum.Parse(fieldType, valueStr));
            }
            else
            {
                Console.Message($"Unsupported Injection ({field.FieldType.Name})");
            }
        }

        private void InjectField(object obj, FieldInfo field, string valueStr)
        {
            Type fieldType = field.FieldType;

            if (fieldType == typeof(string))
            {
                field.SetValue(obj, valueStr);
            }
            else if (fieldType == typeof(int))
            {
                field.SetValue(obj, int.Parse(valueStr, m_ParseCultureInfo));
            }
            else if (fieldType == typeof(float))
            {
                field.SetValue(obj, float.Parse(valueStr, m_ParseCultureInfo));
            }
            else if (fieldType == typeof(bool))
            {
                field.SetValue(obj, bool.Parse(valueStr));
            }
            else if (fieldType.IsEnum )
            {
                field.SetValue(obj, Enum.Parse(fieldType, valueStr));
            }
            else
            {
                Console.Message($"Unsupported Injection ({field.FieldType.Name})");
            }
        }
    }
}
