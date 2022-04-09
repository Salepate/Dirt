using Dirt.Game.Content;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Dirt.GameEditor
{

    public class DirtContentEditor : EditorWindow
    {
        private const string ManifestName = "gamecontent.json";
        private const string ContentPathKey = "dirtcontent_path";
        private string m_ContentPath;
        private DirectoryInfo m_ContentDir;
        private Vector2 m_ListScroll;

        private Dictionary<string, bool> m_ContentFiles;

        [MenuItem("Dirt/Content")]
        private static void ShowEditor()
        {
            DirtContentEditor ed = EditorWindow.GetWindow<DirtContentEditor>("Content Editor");
            ed.Show();
        }


        private void OnEnable()
        {
            m_ContentDir = null;
            m_ContentFiles = new Dictionary<string, bool>();
            string path = EditorPrefs.GetString(ContentPathKey, null);
            UpdateContentPath(path);
        }

        private void OnGUI()
        {
            DrawSettings();
            if (m_ContentDir != null && m_ContentDir.Exists)
                DrawContentEditor();
        }


        private void DrawContentEditor()
        {
            GUILayout.BeginHorizontal();
            GUILayout.BeginVertical(GUI.skin.box, GUILayout.Width(250f));

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Select All"))
                SetAllContentStatus(true);
            if (GUILayout.Button("Unselect All"))
                SetAllContentStatus(false);
            GUILayout.EndHorizontal();
            m_ListScroll = GUILayout.BeginScrollView(m_ListScroll);
            var contentFiles = m_ContentDir.GetFiles("*.json");
            for(int i = 0; i < contentFiles.Length; ++i)
            {
                string contentFile = contentFiles[i].Name;
                if ( string.Compare(contentFile, ManifestName) != 0 )
                {
                    
                    GUILayout.BeginHorizontal(GUI.skin.box);
                    GUILayout.Label(contentFile, GUILayout.Width(180f));
                    bool isSelected = false;
                    if (!m_ContentFiles.TryGetValue(contentFile, out isSelected))
                        m_ContentFiles[contentFile] = false;

                    EditorGUI.BeginChangeCheck();
                    isSelected = GUILayout.Toggle(isSelected, string.Empty);
                    if ( EditorGUI.EndChangeCheck())
                    {
                        m_ContentFiles[contentFile] = isSelected;
                    }
                    GUILayout.EndHorizontal();
                }
            }
            GUILayout.EndScrollView();
            GUILayout.EndVertical();

            GUILayout.BeginVertical(GUI.skin.box);
            if ( GUILayout.Button("Save Content") )
            {
                SaveContent();
            }

            GUILayout.EndVertical();
            GUILayout.EndHorizontal();
        }


        private void SaveContent()
        {
            GameContent newContent = new GameContent()
            {
                FileMap = m_ContentFiles.Where(kvp => kvp.Value).OrderBy(k => k.Key).ToDictionary(p => p.Key.Replace(".json", ""), p => p.Key)
            };

            File.WriteAllText(Path.Combine(m_ContentPath, ManifestName),
                JsonConvert.SerializeObject(newContent, Formatting.Indented));
        }

        private void SetAllContentStatus(bool selected)
        {
            foreach(var k in m_ContentFiles.Keys.ToList())
            {
                m_ContentFiles[k] = selected;
            }
        }

        private void DrawSettings()
        {
            GUILayout.BeginHorizontal();
            GUI.enabled = false;
            m_ContentPath = EditorGUILayout.TextField(m_ContentPath);
            GUI.enabled = true;
            if (GUILayout.Button("Browse"))
            {
                string newPath = EditorUtility.OpenFolderPanel("Content Folder", Application.dataPath, "Content");
                UpdateContentPath(newPath);
            }
            GUILayout.EndHorizontal();
        }

        private void UpdateContentPath(string path)
        {
            if (string.IsNullOrEmpty(path))
                return;

            DirectoryInfo dir = new DirectoryInfo(path);

            if ( dir.Exists)
            {
                m_ContentPath = path;
                m_ContentDir = dir;
                EditorPrefs.SetString(ContentPathKey, path);

                string manifestPath = Path.Combine(path, ManifestName);
                GameContent content = JsonConvert.DeserializeObject<GameContent>(File.ReadAllText(manifestPath));
                ParseFromContent(content);
            }
        }

        private void ParseFromContent(GameContent content)
        {
            m_ContentFiles.Clear();
            foreach(var kvp in content.FileMap)
            {
                m_ContentFiles.Add(kvp.Value, true);
            }
        }
    }
}