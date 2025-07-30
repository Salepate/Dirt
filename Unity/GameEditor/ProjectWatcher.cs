using Dirt.GameEditor.Imgui;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Unity.EditorCoroutines.Editor;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace Dirt.GameEditor
{
    public static class DirtProjectWatcher
    {
        public const string WatcherFilename = "dirtwatcher";
        public static readonly string[] s_WhiteList =
        {
        };

        private static EditorWaitForSeconds m_CheckInterval = new EditorWaitForSeconds(5f);
        private static EditorWaitForSeconds m_CheckLongInterval = new EditorWaitForSeconds(8f);

        // Coroutine Cache
        private static Dictionary<string, long> s_ProjectWatcher;
        private static List<FileInfo> s_AcceptedLibs = new List<FileInfo>();


        [InitializeOnLoadMethod()]
        private static void HookRefreshEvent()
        {
            EditorCoroutineUtility.StartCoroutineOwnerless(CheckRoutine());
        }

        private static IEnumerator CheckRoutine()
        {
            var watcherData = ProjectWatcherData.Settings;

            while (true)
            {
                if (InternalEditorUtility.isApplicationActive && watcherData.IsEnabled)
                {
                    if (s_ProjectWatcher == null)
                        s_ProjectWatcher = LoadWatcher(WatcherFilename);

                    bool hasUpdated = CheckAndUpdateLibraries(s_ProjectWatcher);

                    if (hasUpdated)
                    {
                        UpdateWatcher(WatcherFilename, s_ProjectWatcher);
                        AssetDatabase.Refresh();
                    }
                    yield return m_CheckInterval;
                }
                else
                {
                    yield return m_CheckLongInterval;
                }
            }
        }

        private static bool CheckAndUpdateLibraries(Dictionary<string, long> watcher)
        {
            DirectoryInfo source = new DirectoryInfo(ProjectWatcherData.Settings.SourceFolder);
            // get files
            var ignoredlibs = source.GetFiles("*", SearchOption.TopDirectoryOnly);
            var allLibs = source.GetFiles("*", SearchOption.AllDirectories);
            allLibs = allLibs.Where(lib => ProjectWatcherData.Settings.WhiteList.Any(lib.Name.Contains)).ToArray();
            s_AcceptedLibs.Clear();
            s_AcceptedLibs.AddRange(allLibs);

            int updatedLibCount = 0;


            s_AcceptedLibs.ForEach(lib =>
            {
                string hash = GetHash(lib.Directory.Name, lib.Name);
                long ts = lib.LastWriteTime.Ticks;
                if (!watcher.ContainsKey(hash) || watcher[hash] < ts)
                {
                    UpdateLibrary(lib);
                    ++updatedLibCount;
                    watcher[hash] = ts;
                }
            });

            if (updatedLibCount > 0)
            {
                Debug.Log($"Updated {updatedLibCount} libraries");
                return true;
            }

            return false;
        }

        private static void UpdateLibrary(FileInfo sourceFile)
        {
            Debug.Log($"updating {sourceFile.Name}...");

            string targetPath = Path.Combine(ProjectWatcherData.Settings.ImportFolder, sourceFile.Name);
            FileInfo targetFile = new FileInfo(targetPath);
            DirectoryInfo targetFolder = targetFile.Directory;
            if (!targetFolder.Exists)
            {
                targetFolder.Create();
            }
            File.Copy(sourceFile.FullName, targetFile.FullName, true);
        }

        private static Dictionary<string, long> LoadWatcher(string name)
        {
            DirectoryInfo watcherPath = InitializeWatcherPath();

            FileInfo projectWatcher = new FileInfo(Path.Combine(watcherPath.FullName, $"{name}.watcher"));
            Dictionary<string, long> timeStamps = null;

            if (projectWatcher.Exists)
            {
                try
                {
                    timeStamps = JsonConvert.DeserializeObject<Dictionary<string, long>>(File.ReadAllText(projectWatcher.FullName));
                }
                catch (Exception e)
                {
                    Debug.LogError($"Could not deserialize watcher {projectWatcher.Name}\n{e.ToString()}\n, creating new one");
                }
            }

            if (timeStamps == null)
            {
                timeStamps = new Dictionary<string, long>();
            }

            return timeStamps;
        }

        private static void UpdateWatcher(string name, Dictionary<string, long> data)
        {
            DirectoryInfo watcherPath = InitializeWatcherPath();
            string path = Path.Combine(watcherPath.FullName, $"{name}.watcher");
            File.WriteAllText(path, JsonConvert.SerializeObject(data));
        }

        private static DirectoryInfo InitializeWatcherPath()
        {
            DirectoryInfo watcherPath = new DirectoryInfo(Path.Combine(Application.dataPath, @"..\Watchers"));
            if (!watcherPath.Exists)
            {
                watcherPath.Create();
            }

            return watcherPath;
        }

        private static string GetHash(string dir, string file)
        {
            return $"{dir}/{file}";
        }
    }


    internal static class ProjectWatcherUI
    {
        private static string FolderField(string label, string value)
        {
            GUILayout.BeginHorizontal();
            EditorGUI.BeginChangeCheck();
            string newText = EditorGUILayout.TextField(label, value);
            if ( EditorGUI.EndChangeCheck())
            {
                value = newText;
            }
            if ( GUIExtension.Button("Browse", DirtGUI.ColorNew, GUILayout.Width(120f))) {
                string newFolder = EditorUtility.OpenFolderPanel("Select Folder", value ?? Application.dataPath, "");
                if (!string.IsNullOrEmpty(newFolder))
                    value = newFolder;
            }
            GUILayout.EndHorizontal();
            return value;
        }
        [SettingsProvider]
        public static SettingsProvider CreateSettingsProvider()
        {
            var provider = new SettingsProvider("Project/Dirt/Watcher", SettingsScope.Project)
            {
                label = "Watcher",
                guiHandler = (searchContext) =>
                {
                    var settings = ProjectWatcherData.Settings;

                    EditorGUI.BeginChangeCheck();
                    GUIExtension.DrawView(settings.WhiteList, (index, elem) => {
                        GUILayout.BeginHorizontal();

                        settings.WhiteList[index] = EditorGUILayout.TextField(settings.WhiteList[index]);
                        if (GUIExtension.Button("Browse", DirtGUI.ColorNew, GUILayout.Width(120f)))
                        {
                            string file = EditorUtility.OpenFilePanel("Choose file", "", "dll,pdb");
                            FileInfo fileInfo = new FileInfo(file);
                            if (fileInfo.Exists)
                            {
                                settings.WhiteList[index] = fileInfo.Name;
                            }
                        }
                        GUILayout.EndHorizontal();
                    }, (index, elem) =>
                    {
                        ArrayUtility.RemoveAt(ref settings.WhiteList, index);
                    }, (index) => false, -1, GUIExtension.ListViewFlags.Manageable);
                    //EditorGUILayout.PropertyField(whitelist, true);
                    if ( GUILayout.Button("Add Entry"))
                    {
                        ArrayUtility.Insert(ref settings.WhiteList, settings.WhiteList.Length, "MyPlugin.dll");
                    }


                    EditorGUI.BeginChangeCheck();
                    settings.IsEnabled = EditorGUILayout.Toggle("Enable", settings.IsEnabled);
                    settings.SourceFolder = FolderField("Source Folder", settings.SourceFolder);
                    settings.ImportFolder = FolderField("Unity Import Folder", settings.ImportFolder);
                    if (EditorGUI.EndChangeCheck())
                    {
                        EditorUtility.SetDirty(settings);
                    }
                },
                keywords = new HashSet<string>(new string[] { "Watcher" }),
                deactivateHandler = () =>
                {
                    AssetDatabase.SaveAssets();
                },
            };
            return provider;
        }
    }
}