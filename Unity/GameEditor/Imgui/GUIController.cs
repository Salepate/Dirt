using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEditor;
using UnityEngine;

namespace Dirt.GameEditor.Imgui
{
    public class GUIController
    {
        private static GUISkin m_Skin;

        static GUIController()
        {
            if (m_Skin ==null)
            {
                m_Skin = AssetDatabase.LoadAssetAtPath<GUISkin>("Assets/Externals/Dirt/EditorSkin.guiskin");
            }
        }

        // ctor
        public GUIController()
        {
            m_Folders = new Dictionary<int, bool>();
            m_Tabs = new Dictionary<GUIContent[], int>();
        }

        //---------------------------------------------------------------------
        //- Tab System
        //---------------------------------------------------------------------
        private Dictionary<GUIContent[], int> m_Tabs;

        public delegate bool TabDelegate();

        /// <summary>
        /// Create Tabs
        /// </summary>
        /// <param name="tabTitles">tab names</param>
        /// <param name="tabCallbacks">tab gui functions</param>
        /// <returns></returns>
        public GUIContent[] MakeTabs(string[] tabTitles)
        {
            GUIContent[] copy = new GUIContent[tabTitles.Length];
            for (int i = 0; i < copy.Length; ++i)
            {
                copy[i] = new GUIContent(tabTitles[i]);
            }
            return copy;
        }

        public TabDelegate ShowTabs(GUIContent[] content, TabDelegate[] tabCallback, out int indexChanged)
        {
            if (!m_Tabs.ContainsKey(content))
            {
                m_Tabs.Add(content, 0);
            }
            int current = m_Tabs[content];

            int selected = GUILayout.Toolbar(current, content);
            indexChanged = -1;
            if (selected != current)
            {
                m_Tabs[content] = selected;
                indexChanged = selected;
            }

            return tabCallback[selected];
        }

        public TabDelegate ShowTabs(GUIContent[] content, TabDelegate[] tabCallback)
        {
            return ShowTabs(content, tabCallback, out _);
        }

        //---------------------------------------------------------------------
        //- Folder System
        //---------------------------------------------------------------------
        private Dictionary<int, bool> m_Folders;

        public bool Folder(string label, string strHash, bool defaultUnfold = true)
        {
            int hash = strHash.GetHashCode();
            if (!m_Folders.ContainsKey(hash))
            {
                m_Folders[hash] = defaultUnfold;
            }

            EditorGUILayout.BeginVertical("box");
            m_Folders[hash] = EditorGUILayout.Foldout(m_Folders[hash], label);

            return m_Folders[hash];
        }

        public bool Folder(string foldName, bool defaultUnfold = true)
        {
            return Folder(foldName, foldName, defaultUnfold);
        }

        public void EndFolder()
        {
            EditorGUILayout.EndVertical();
        }

        public void ResetFolders()
        {
            m_Folders.Clear();
        }

        public void ForceFolderUnfold(string strHash, bool unfold)
        {
            int hash = strHash.GetHashCode();
            m_Folders[hash] = unfold;
        }

        public bool IsFolded(string folderName)
        {
            bool res = false;
            m_Folders.TryGetValue(folderName.GetHashCode(), out res);
            return res;
        }
    }
}