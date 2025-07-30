using Dirt.Unity.Dirt;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Dirt.GameEditor.Drawers
{
    [CustomPropertyDrawer(typeof(ScenePickerAttribute))]
    public class ScenePickerDrawer : PropertyDrawer
    {
        private bool m_BuildOnly;
        private string m_Pattern;
        private bool m_ContentBuilt;
        private GUIContent[] m_Scenes;

        public ScenePickerDrawer()
        {
            m_ContentBuilt = false;
            m_Pattern = string.Empty;
            m_BuildOnly = true;
        }
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            ScenePickerAttribute pickerAttr = (ScenePickerAttribute)attribute;
            bool buildContent = !m_ContentBuilt;
            if (pickerAttr.Build != m_BuildOnly || pickerAttr.Pattern != m_Pattern)
                buildContent = true;


            if (buildContent)
                BuildContent();


            string currentScene = property.stringValue;
            int index = GetIndexFromName(currentScene);
            int newIndex = EditorGUI.Popup(position, index, m_Scenes);
            if (index != newIndex)
            {
                if (newIndex <= 0)
                    property.stringValue = string.Empty;
                else
                    property.stringValue = m_Scenes[newIndex].text;
            }
        }

        private int GetIndexFromName(string name)
        {
            if (string.IsNullOrEmpty(name))
                return 0;

            for(int i = 1; i < m_Scenes.Length; ++i)
            {
                GUIContent scene = m_Scenes[i];
                if (scene.text.CompareTo(name) == 0)
                    return i;
            }
            return 0;
        }

        private void BuildContent()
        {
            ScenePickerAttribute pickerAttr = (ScenePickerAttribute)attribute;
            m_BuildOnly = pickerAttr.Build;
            m_Pattern = pickerAttr.Pattern;
            if (!pickerAttr.Build)
            {
                Debug.LogWarning("Only Build scenes is supported at the moment");
            }

            if (!string.IsNullOrEmpty(m_Pattern) && m_Pattern.Contains("*"))
            {
                Debug.LogWarning("Wildcard (*) not supported yet");
            }

            EditorBuildSettingsScene[] scenes = EditorBuildSettings.scenes;
            m_Scenes = new GUIContent[scenes.Length+1];

            List<GUIContent> validScenes = new List<GUIContent>();
            validScenes.Add(new GUIContent("No Scene selected"));

            for(int i = 0; i < scenes.Length; ++i)
            {
                SceneAsset sceneInfo = AssetDatabase.LoadAssetAtPath<SceneAsset>(scenes[i].path);
                GUIContent sceneGUI = new GUIContent()
                {
                    text = sceneInfo.name
                };

                if (string.IsNullOrEmpty(m_Pattern) || sceneInfo.name.Contains(m_Pattern))
                    validScenes.Add(sceneGUI);
            }

            m_Scenes = validScenes.ToArray();
            m_ContentBuilt = true;
        }
    }
}