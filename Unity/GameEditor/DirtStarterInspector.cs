using Framework;
using System;
using UnityEditor;
using UnityEngine;

namespace Dirt.GameEditor
{
    [CustomEditor(typeof(DirtStarter))]
    public class DirtStarterInspector : Editor
    {
        private System.Type[] m_Modes;
        private GUIContent[] m_ModesGUI;
        private int m_SelectedMode;
        private int m_SelectedServiceMode;
        private bool m_LockFrameRate;
        private int m_TargetFrameRate;

        private void OnEnable()
        {
            var modes = AssemblyUtility.GetSubtypes(typeof(DirtMode), false);

            m_Modes = modes;
            m_ModesGUI = new GUIContent[modes.Length+1];

            m_ModesGUI[0] = new GUIContent("Choose GameMode");
            for(int i = 0; i < modes.Length; ++i)
            {
                m_ModesGUI[i+1] = new GUIContent(modes[i].Name);
            }

            DirtStarter starter = (DirtStarter)target;
            string serviceMode = starter.ServiceMode;
            string chosenMode = starter.InitialMode;
            m_LockFrameRate = starter.LockFramerate;
            m_TargetFrameRate = starter.TargetFramerate;

            m_SelectedMode = Array.FindIndex(m_Modes, m => m.FullName == chosenMode) + 1;
            m_SelectedServiceMode = Array.FindIndex(m_Modes, m => m.FullName == serviceMode) + 1;
        }

        public override void OnInspectorGUI()
        {
            serializedObject.ApplyModifiedProperties();

            DirtStarter starter = (DirtStarter)target;

            EditorGUI.BeginChangeCheck();

            bool lockFrameRate = EditorGUILayout.Toggle(new GUIContent("Lock Framerate"), m_LockFrameRate);
            GUI.enabled = lockFrameRate;
            int targetFps = EditorGUILayout.IntField(new GUIContent("Framerate"), m_TargetFrameRate);
            GUI.enabled = true;
            int serviceIdx = EditorGUILayout.Popup(new GUIContent("Service"), m_SelectedServiceMode, m_ModesGUI);
            int idx = EditorGUILayout.Popup(new GUIContent("Default Game Mode"), m_SelectedMode, m_ModesGUI);

            SerializedProperty errProp = serializedObject.FindProperty("ErrorSceneName");
            string errorScene = EditorGUILayout.TextField(new GUIContent("Error Scene"), errProp.stringValue);

            starter.DebugGame = EditorGUILayout.Toggle("Debug", starter.DebugGame);

            if (EditorGUI.EndChangeCheck())
            {
                string modeName = string.Empty;
                string serviceName = string.Empty;
                if (idx > 0)
                    modeName = m_Modes[idx - 1].FullName;
                if (serviceIdx > 0)
                    serviceName = m_Modes[serviceIdx - 1].FullName;

                serializedObject.FindProperty("InitialMode").stringValue = modeName;
                serializedObject.FindProperty("ServiceMode").stringValue = serviceName;
                serializedObject.FindProperty("LockFramerate").boolValue = lockFrameRate;
                serializedObject.FindProperty("TargetFramerate").intValue = targetFps;
                errProp.stringValue = errorScene;
                serializedObject.ApplyModifiedProperties();
                m_SelectedMode = idx;
                m_SelectedServiceMode = serviceIdx;
                m_LockFrameRate = lockFrameRate;
                m_TargetFrameRate = targetFps;
                EditorUtility.SetDirty(target);
            }
        }
    }
}