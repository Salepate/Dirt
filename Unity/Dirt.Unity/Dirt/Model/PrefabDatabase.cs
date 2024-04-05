using Dirt;
using UnityEngine;

namespace Arena.Contents
{
    [CreateAssetMenu(fileName = "PrefabSystem", menuName = "Dirt/Prefab Database")]
    public class PrefabDatabase : DirtSystemContent
    {
        public GameObject[] Prefabs;
    }
}