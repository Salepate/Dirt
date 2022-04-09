using UnityEditor;
using UnityEngine;

namespace Dirt.GameEditor
{
    public class ProjectWatcherData : ScriptableObject
    {
        public bool IsEnabled;
        public string[] WhiteList;
        public string SourceFolder;
        public string ImportFolder;

        private static ProjectWatcherData m_Settings;

        public static ProjectWatcherData Settings
        {
            get
            {
                if (m_Settings == null)
                {

                    var assets = AssetDatabase.FindAssets($"t:{nameof(ProjectWatcherData)}");
                    if (assets.Length > 0)
                    {
                        string path = AssetDatabase.GUIDToAssetPath(assets[0]);
                        m_Settings = AssetDatabase.LoadAssetAtPath<ProjectWatcherData>(path);
                    }
                }

                if (m_Settings == null)
                {
                    Debug.Log("Creating Watcher data");
                    m_Settings = ScriptableObject.CreateInstance<ProjectWatcherData>();
                    m_Settings.name = "dirtwatcher";
                    AssetDatabase.CreateAsset(m_Settings, $"Assets/{m_Settings.name}.asset");
                }
                return m_Settings;
            }
        }
    }
}