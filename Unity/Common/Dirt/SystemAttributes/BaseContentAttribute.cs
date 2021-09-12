using System;

namespace Dirt
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
    public class BaseContentAttribute : System.Attribute {
        public string ContentName { get; private set; }

        public BaseContentAttribute(string content)
        {
            ContentName = content;
        }
    }
}