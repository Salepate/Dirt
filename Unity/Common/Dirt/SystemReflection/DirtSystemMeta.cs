using Framework;
using System.Collections.Generic;
using System.Reflection;

namespace Dirt.Reflection
{
    public class DirtSystemMeta
    {
        public bool HasContent { get; private set; }

        public List<ContentMeta> ContentFields { get; private set; }

        public DirtSystemMeta(System.Type system)
        {
            var baseTypes = AssemblyUtility.GetDeclaringTypesWithInterface(system, typeof(IContentSystem));

            HasContent = baseTypes.Length > 0;
            ContentFields = new List<ContentMeta>();

            if ( HasContent )
            {
                for(int j = 0; j < baseTypes.Length; ++j)
                {
                    var fields = baseTypes[j].GetFields(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy);
                    var props = baseTypes[j].GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy);

                    for(int i = 0; i < fields.Length; ++i)
                    {
                        var fi = fields[i];
                        var contentAttr = fi.GetCustomAttribute<BaseContentAttribute>();

                        if ( contentAttr != null )
                        {
                            ContentFields.Add(new ContentMeta(fi, contentAttr.ContentName));
                        }
                    }

                    for (int i = 0; i < props.Length; ++i)
                    {
                        var pi = props[i];
                        var contentAttr = pi.GetCustomAttribute<BaseContentAttribute>();

                        if (contentAttr != null)
                        {
                            ContentFields.Add(new ContentMeta(pi, contentAttr.ContentName));
                        }
                    }
                }
            }
        }
    }
}