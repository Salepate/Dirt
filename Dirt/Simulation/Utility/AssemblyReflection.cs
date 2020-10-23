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
            Assembly[] loadedAsses = AppDomain.CurrentDomain.GetAssemblies();
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