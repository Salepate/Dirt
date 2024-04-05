using System.Reflection;

namespace Dirt.Reflection
{
    public class ContentMeta
    {
        public string ContentName { get; private set; }

        private FieldInfo m_Field;
        private MethodInfo m_Setter;
        public ContentMeta(FieldInfo contentField, string contentName)
        {
            ContentName = contentName;
            m_Field = contentField;
        }

        public ContentMeta(PropertyInfo contentProp, string contentName)
        {
            ContentName = contentName;
            //m_Field = contentProp.
            m_Setter = contentProp.GetSetMethod();
        }

        public void Inject(object targetObject, object content)
        {
            if ( m_Field != null )
            {
                m_Field.SetValue(targetObject, content);
            }
            else
            {
                m_Setter.Invoke(targetObject, new object[] { content });
            }
        }
    }
}