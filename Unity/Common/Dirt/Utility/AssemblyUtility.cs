using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Framework
{
    public static class AssemblyUtility
    {
        private static Assembly[] s_AppAssemblies;

        static AssemblyUtility()
        {
            s_AppAssemblies = System.AppDomain.CurrentDomain.GetAssemblies();
        }

        public static System.Type GetTypeFromName(string typeName)
        {
            for(int i = 0; i < s_AppAssemblies.Length; ++i)
            {
                System.Type t = s_AppAssemblies[i].GetType(typeName);
                if (t != null)
                    return t;
            }
            return null;
        }

        public static System.Type[] GetSubtypes(System.Type baseClass, bool allowAbstracts = true)
        {
            List<System.Type> result = new List<System.Type>();
            var assesTypes = s_AppAssemblies.SelectMany(ass => ass.GetTypes());
            var filteredTypes = assesTypes.Where(t => baseClass.IsAssignableFrom(t) && t != baseClass && (allowAbstracts || !t.IsAbstract));
            result.AddRange(filteredTypes);
            return result.ToArray();
        }

        public static System.Type[] GetTypesWithInterface(System.Type baseClass, bool allowAbstracts = true)
        {
            List<System.Type> result = new List<System.Type>();
            var assesTypes = s_AppAssemblies.SelectMany(ass => ass.GetTypes());
            var filteredTypes = assesTypes.Where(t => t.GetInterface(baseClass.Name) != null && (allowAbstracts || !t.IsAbstract));
            result.AddRange(filteredTypes);
            return result.ToArray();
        }

        public static System.Type[] GetDeclaringTypesWithInterface(System.Type subType, System.Type typeInterface)
        {
            List<System.Type> result = new List<System.Type>();

            if ( !subType.IsValueType )
            {
                while(subType != typeof(object))
                {
                    if (subType.GetInterface(typeInterface.Name) != null)
                        result.Add(subType);

                    subType = subType.BaseType;
                }
            }
            return result.ToArray();
        }
    }
}