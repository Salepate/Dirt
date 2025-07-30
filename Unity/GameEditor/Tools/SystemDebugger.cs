using System;
using UnityEditor;
using UnityEngine;

namespace Dirt.GameEditor.Tools
{
    public class SystemDebugger : EditorWindow
    {
        [MenuItem("Dirt/System Debugger")]
        private static void ShowTool()
        {
            EditorWindow.GetWindow<SystemDebugger>("Dirt System Debugger").Show(true);
        }


        private DirtStarter m_Dirt;
        private GUIContent[] m_CurrentSystems;
        private SystemInfo[] m_Infos;
        private Vector2 m_ScrollInfo;
        private void OnEnable()
        {
            m_Dirt = null;
            m_CurrentSystems = null;
            m_Infos = null;
        }

        private void OnGUI()
        {
            if (m_Dirt == null)
            {
                if (GUILayout.Button("Find Dirt"))
                {
                    m_Dirt = GameObject.FindObjectOfType<DirtStarter>();
                }
            }
            if (m_Dirt != null)
            {
                if (GUILayout.Button("Debug Systems"))
                {
                    DebugSystems();
                }


                m_ScrollInfo = EditorGUILayout.BeginScrollView(m_ScrollInfo);
                GUILayout.BeginVertical(GUI.skin.box);



                for(int i = 0; m_CurrentSystems != null && i < m_CurrentSystems.Length; ++i)
                {
                    GUILayout.BeginVertical(GUI.skin.box);
                    GUILayout.Label(m_CurrentSystems[i]);
                    GUILayout.BeginHorizontal();
                    DrawSystem(i);
                    GUILayout.EndHorizontal();
                    GUILayout.EndVertical();

                }
                GUILayout.EndVertical();
                EditorGUILayout.EndScrollView();
            }
        }

        private void DrawSystem(int systemIndex)
        {
            ref SystemInfo info = ref m_Infos[systemIndex];
            string updateState = info.IsUpdating ? "Yes" : "No";
            int serviceOffset = m_Dirt.Services.Count;


            if (GUILayout.Button("Update: " + updateState))
            {
                info.IsUpdating = !info.IsUpdating;
                DirtMode mode = systemIndex < serviceOffset ? m_Dirt.ServiceModes[0] : m_Dirt.Mode;
                if (systemIndex >= serviceOffset)
                    systemIndex -= serviceOffset;

                DirtSystem system = mode.Systems[systemIndex];

                if (info.IsUpdating && !mode.UpdateSystems.Contains(system))
                    mode.UpdateSystems.Add(system);
                else if (!info.IsUpdating)
                    mode.UpdateSystems.Remove(system);
            }
        }

        private void DebugSystems()
        {
            DirtMode currentMode = m_Dirt.Mode;

            m_CurrentSystems = new GUIContent[m_Dirt.Services.Count + currentMode.Systems.Count];
            m_Infos = new SystemInfo[m_CurrentSystems.Length];
            for (int i = 0; i < m_Dirt.Services.Count; ++i)
            {
                m_CurrentSystems[i] = new GUIContent(m_Dirt.Services[i].GetType().Name);
                m_Infos[i] = new SystemInfo()
                {
                    IsService = true,
                    IsUpdating = m_Dirt.Services[i].HasUpdate
                };
            }
            for (int i = 0; i < currentMode.Systems.Count; ++i)
            {
                m_CurrentSystems[m_Dirt.Services.Count + i] = new GUIContent(currentMode.Systems[i].GetType().Name);
                m_Infos[m_Dirt.Services.Count + i] = new SystemInfo()
                {
                    IsService = true,
                    IsUpdating = currentMode.Systems[i].HasUpdate
                };
            }
        }

        //
        private struct SystemInfo
        {
            public bool IsService;
            public bool IsUpdating;
        }
    }
}