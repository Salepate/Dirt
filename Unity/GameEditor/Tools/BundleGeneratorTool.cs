using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Dirt.Game;
using Dirt.GameEditor.Imgui;

namespace Dirt.GameEditor.Tools
{
    using VerticalScope = EditorGUILayout.VerticalScope;
    public class BundleHelperTool : EditorWindow
    {
        private GameBundle[] m_Bundles;
        private GUIContent[] m_BundleSelectorPopup;
        private List<Object> m_ToBundle;
        private List<Object> m_InBundle;
        private int m_SelectedBundle;

        private string m_MainToken;
        private string m_NewToken;

        [MenuItem("Dirt/Bundle Tool")]
        private static void ShowBundleTool()
        {
            GetWindow<BundleHelperTool>("Bundle Helper").Show(true);
        }


        private void OnEnable()
        {
            var guids = AssetDatabase.FindAssets("t:GameBundle", new string[] { "Assets/" });
            m_Bundles = new GameBundle[guids.Length];
            m_BundleSelectorPopup = new GUIContent[guids.Length];
            m_ToBundle = new List<Object>();
            m_InBundle = new List<Object>();
            m_SelectedBundle = 0;

            for (int i = 0; i < guids.Length; ++i)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[i]);
                m_Bundles[i] = AssetDatabase.LoadAssetAtPath<GameBundle>(path);
            }

            for (int i = 0; i < m_Bundles.Length; ++i)
            {
                m_BundleSelectorPopup[i] = new GUIContent(m_Bundles[i].name);
            }

            Selection.selectionChanged += ComputeObjectList;
            ComputeObjectList();
        }
        private void OnDisable()
        {
            Selection.selectionChanged -= ComputeObjectList;
        }

        private void OnGUI()
        {
            GUIStyle richText = GUI.skin.label;
            richText.richText = true;
            using (var scope = new VerticalScope(GUI.skin.box))
            {
                int count = 0;
                if (Selection.objects != null)
                {
                    count = Selection.objects.Length;
                }
                GUILayout.Label($"Selection {count} items");
                GUILayout.BeginVertical(GUI.skin.box);
                for (int i = 0; i < count; ++i)
                {
                    string bundleInfo = "No Bundle";
                    string bundleCol = "#ff8888";
                    if (TryGetBundle(Selection.objects[i], out GameBundle bundle, out int objIndex))
                    {
                        bundleInfo = $"{bundle.name}: {bundle.Assets[objIndex].ID}";
                        bundleCol = "#44ff44";
                    }
                    GUILayout.Label($"{Selection.objects[i].name} ({Selection.objects[i].GetType().Name}) [<b><color={bundleCol}>{bundleInfo}</color></b>]", richText);
                }
                GUILayout.EndVertical();
            }
            using (var scope = new VerticalScope(GUI.skin.box))
            {
                if (m_Bundles.Length > 0)
                {
                    m_SelectedBundle = EditorGUILayout.Popup(m_SelectedBundle, m_BundleSelectorPopup, GUILayout.Width(300f));
                }
                else
                {
                    EditorGUILayout.HelpBox("No bundles were detected, you need at least one bundle to use this tool.", MessageType.Warning);
                }
                for (int i = 0; i < m_ToBundle.Count; ++i)
                {
                    string prevName = m_ToBundle[i].name;
                    string nextName = FormatName(m_ToBundle[i]);
                    if (!string.IsNullOrEmpty(m_MainToken))
                    {
                        nextName = nextName.Replace(m_MainToken.ToLower(), m_NewToken);
                    }
                    GUILayout.Label($"{prevName} -> {nextName}");
                }

                if (GUIExtension.Button("Add To Bundle", DirtGUI.ColorSave))
                {
                    AddObjects();
                }

                bool state = GUI.enabled;
                GUI.enabled = m_InBundle.Count > 0;
                if (GUIExtension.Button("Remove from Bundles", DirtGUI.ColorAlert))
                {
                    RemoveObjects();
                }
                GUI.enabled = state;
            }
            using (var scope = new VerticalScope(GUI.skin.box))
            {
                m_MainToken = EditorGUILayout.TextField("Main Token", m_MainToken, GUILayout.Width(400f));
                m_NewToken = EditorGUILayout.TextField("New Name", m_NewToken, GUILayout.Width(400f));
            }
        }

        private void RemoveObjects()
        {
            HashSet<GameBundle> bundles = new HashSet<GameBundle>();
            for(int i = 0; i < m_InBundle.Count; ++i)
            {
                TryGetBundle(m_InBundle[i], out GameBundle bundle, out int objIndex);
                bundle.Assets.RemoveAt(objIndex);
                bundles.Add(bundle);
            }

            foreach(GameBundle bundle in bundles)
            {
                EditorUtility.SetDirty(bundle);
            }

            ComputeObjectList();
        }

        private void AddObjects()
        {
            GameBundle bundle = m_Bundles[m_SelectedBundle];
            for (int i = 0; i < m_ToBundle.Count; ++i)
            {
                string objName = FormatName(m_ToBundle[i]);
                Object obj = m_ToBundle[i];

                if (!string.IsNullOrEmpty(m_MainToken))
                {
                    objName = objName.Replace(m_MainToken.ToLower(), m_NewToken);
                }

                AssetDatabase.TryGetGUIDAndLocalFileIdentifier(obj, out string guid, out long _);

                bundle.Assets.Add(new GameResource()
                {
                    ID = objName,
                    Label = $"[{obj.GetType().Name}] {obj.name}",
                    Object = obj,
                    GUID = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(obj))
                });
            }
            EditorUtility.SetDirty(bundle);
            ComputeObjectList();
        }

        public static string GetAssetPrefix(Object obj)
        {
            string prefix = "unknown";

            if (obj is Texture2D)
            {
                prefix = "texture";
            }
            if (obj is Mesh)
            {
                prefix = "mesh";
            }
            if (obj is AudioClip)
            {
                prefix = "sound";
            }
            if (obj is ScriptableObject)
            {
                prefix = "data";
            }
            return prefix;
        }
        private static string TransformName(string name)
        {
            name = name.Replace('_', '.').Replace("MetallicSmoothness", "detail")
                .Replace(".AlbedoTransparency", "");

            return name;
        }

        public static string FormatName(Object unityObj)
        {
            string newName = TransformName(unityObj.name);
            string prefix = GetAssetPrefix(unityObj);
            return $"{prefix}.{newName.ToLower()}";
        }

        private void ComputeObjectList()
        {
            m_ToBundle.Clear();
            m_InBundle.Clear();
            for (int i = 0; i < Selection.objects.Length; ++i)
            {
                if (!TryGetBundle(Selection.objects[i], out GameBundle bundle, out _))
                {
                    m_ToBundle.Add(Selection.objects[i]);
                }
                else
                {
                    m_InBundle.Add(Selection.objects[i]);
                }
            }
            Repaint();
        }

        private bool TryGetBundle(Object obj, out GameBundle bundle, out int objectIndex)
        {
            bundle = null;
            for (int i = 0; i < m_Bundles.Length; ++i)
            {
                objectIndex = m_Bundles[i].FindObject(obj);
                if (objectIndex != -1)
                {
                    bundle = m_Bundles[i];
                    return true;
                }
            }

            objectIndex = -1;
            return false;
        }
    }
}
