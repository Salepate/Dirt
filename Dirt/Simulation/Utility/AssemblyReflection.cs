using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;

namespace Dirt.Simulation.Utility
{
    public static class AssemblyReflection
    { 
        public static Dictionary<string, Type> BuildTypeMap<I>(string[] assemblies)
        {
            Dictionary<string, Type> map = new Dictionary<string, Type>();
            IEnumerable<Assembly> loadedAsses = AppDomain.CurrentDomain.GetAssemblies();
            IEnumerable<string> loadedAssNames = loadedAsses.Select(ass => ass.FullName);
            IEnumerable<string> missingAssemblies = assemblies.Where(assName => !loadedAssNames.Contains(assName));


            loadedAsses = loadedAsses.Concat(missingAssemblies.Select(assName => AppDomain.CurrentDomain.Load(assName)));

            for(int i = 0; i < missingAssemblies.Count(); ++i)
            {
                Log.Console.Message($"Loading Assembly {missingAssemblies.ElementAt(i)}");
            }

            IEnumerable<Assembly> gameAssemblies = loadedAsses.Where(ass => assemblies.Contains(ass.FullName));
            List<Type> systemTypes = gameAssemblies.SelectMany(ass =>
            {
                return ass.GetTypes().Where(t => typeof(I).IsAssignableFrom(t) && typeof(I) != t);
            }).ToList();

            systemTypes.ForEach(t =>
            {
                map.Add(t.Name, t);
            });

            return map;
        }
    }
}