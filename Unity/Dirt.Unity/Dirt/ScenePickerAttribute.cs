using System;

namespace Dirt.Unity.Dirt
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
    public class ScenePickerAttribute : UnityEngine.PropertyAttribute
    {
        /// <summary>
        /// Include scene from player build only
        /// </summary>
        public bool Build;
        /// <summary>
        /// Supports *
        /// </summary>
        public string Pattern = string.Empty;

        public ScenePickerAttribute(string pattern, bool buildOnly = true)
        {
            Build = buildOnly;
            Pattern = pattern;
        }
    }
}
