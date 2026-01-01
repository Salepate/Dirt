using System.Collections.Generic;
using UnityEngine;

namespace Dirt.Utility
{
    public static class ListUtility
    {
        public static void DestroyList<T>(this IList<T> list) where T : Component
        {
            for (int i = 0; i < list.Count; ++i)
            {
                GameObject.Destroy(list[i].gameObject);
            }
            list.Clear();
        }
    }
}
