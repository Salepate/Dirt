using Dirt.Log;
using System.Collections.Generic;
using UnityEngine;

namespace Dirt.Model
{
    [System.Serializable]
    public class ViewDefinition
    {
        public enum ViewDisplay
        {
            Everyone,
            Others,
            Local
        }

        public string Prefab;
        [Tooltip("Applicable if prefab is fixed")]
        public int InitialPoolSize;
        public string[] Components;
        public ViewLoader Loader;
        public ViewDisplay Display;

        // Reflection
        public string Prefix;
        public string Component;
        public string Field;

        public GameObject CachedPrefab { get; set; }
        public System.Type CachedType { get; private set; }
        public System.Reflection.FieldInfo CachedField { get; private set; }

        public void CacheViews(Dictionary<string, System.Type> compMap)
        {
            if (Loader == ViewLoader.Generic)
            {
                Console.Assert(compMap.ContainsKey(Component), $"{Component} is not a valid component");
                System.Type compType = compMap[Component];
                System.Reflection.FieldInfo targetField = compType.GetField(Field);
                Console.Assert(targetField != null, $"Component does not have field {Field}");

                CachedType = compType;
                CachedField = targetField;
            }
        }

        public enum ViewLoader
        {
            Fixed,  // use prefab name directly
            Generic, // compose prefab name from component field
            Custom // use custom loader
        };
    }
}