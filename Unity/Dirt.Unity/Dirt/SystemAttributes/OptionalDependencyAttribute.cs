using System;

namespace Dirt
{
    /// <summary>
    /// Mark a dependency as optional (will not throw an exception if not found).
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class OptionalDependencyAttribute : System.Attribute {}
}