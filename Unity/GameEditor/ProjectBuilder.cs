using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Dirt.GameEditor
{
    public static class ProjectBuilder
    {
        private static DirtBuild m_Settings = null;
        public static readonly string DefaultSettingsPath = $"Assets/Resources/{DirtBuild.FileName}.asset";

        private static string GetBuildName(bool isDebug = false)
        {
            string label = isDebug ? "debug" : "release";
            return $"{Settings.ProjectName}.{label}.{PlayerSettings.bundleVersion}.{Settings.BuildNumber}";
        }

        internal static DirtBuild Settings
        {
            get
            {
                if (m_Settings != null)
                    return m_Settings;

                m_Settings = RawLoad();

                if (m_Settings == null)
                {
                    m_Settings = ScriptableObject.CreateInstance<DirtBuild>();
                    AssetDatabase.CreateFolder("Assets", "Resources");
                    AssetDatabase.CreateAsset(m_Settings, DefaultSettingsPath);
                    AssetDatabase.SaveAssets();
                }

                return m_Settings;
            }
        }

        internal static DirtBuild RawLoad()
        {
            var assets = AssetDatabase.FindAssets("t:DirtBuild");
            if (assets.Length > 0)
            {
                string path = AssetDatabase.GUIDToAssetPath(assets[0]);
                m_Settings = AssetDatabase.LoadAssetAtPath<DirtBuild>(path);
                return m_Settings;
            }
            return null;
        }

        [MenuItem("Dirt/Build Windows")]
        internal static void BuildGameWindows()
        {
            BuildGame(BuildTarget.StandaloneWindows64, true, Settings.BinaryName);
        }

        [MenuItem("Dirt/Build Android")]
        internal static void BuildGameAndroid()
        {
            BuildGame(BuildTarget.Android, false, GetBuildName());
        }

        [MenuItem("Dirt/Build Android Devel")]
        internal static void BuildGameAndroidDevel()
        {
            BuildGame(BuildTarget.Android, false, GetBuildName(true), BuildOptions.Development | BuildOptions.ConnectWithProfiler | BuildOptions.AllowDebugging); // | BuildOptions.EnableDeepProfilingSupport);
        }
        internal static void BuildGame(BuildTarget target, bool createArchive, string binaryName, BuildOptions options = BuildOptions.None)
        {
            var settings = ProjectBuilder.Settings;
            int buildVersion = settings.BuildNumber;

            try
            {
                Debug.Log($"Building Release (build {buildVersion})");
                EditorBuildSettingsScene[] scenes = EditorBuildSettings.scenes;
                DirectoryInfo buildDir = new DirectoryInfo("Builds");
                if (buildDir.Exists)
                {
                    Debug.Log("Cleaning Build folder");
                    buildDir.Delete(true);
                }

                string extension = "exe";
                if (target == BuildTarget.Android)
                    extension = "apk";

                var buildReport = BuildPipeline.BuildPlayer(scenes, Path.Combine("Builds/", $"{binaryName}.{extension}"), target, options);

                if (buildReport.summary.result == UnityEditor.Build.Reporting.BuildResult.Succeeded)
                {
                    if ( createArchive)
                    {
                        DirectoryInfo projectContent = new DirectoryInfo(Settings.ContentPath);
                        string targetDir = Path.Combine("Builds/", $"{Settings.BinaryName}_Data", "Content");
                        CopyDirectory(projectContent, targetDir, true);
                        //ZipFile.CreateFromDirectory(buildDir.FullName, $"{GetBuildName((options & BuildOptions.Development) != 0)}.zip");
                    }
                    settings.BuildNumber++;
                    EditorUtility.SetDirty(settings);
                    AssetDatabase.SaveAssets();
                    Debug.Log($"Build succeeded");
                }
                else
                {
                    Debug.LogWarning($"Something went wrong during build ({buildReport.summary.totalErrors} error)");
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Error During Build");
                Debug.LogError(e);
            }
        }

        internal static SerializedObject GetSerializedSettings()
        {
            return new SerializedObject(Settings);
        }

        private static void CopyDirectory(DirectoryInfo dir, string path, bool recursive)
        {
            DirectoryInfo targetDir = new DirectoryInfo(path);

            if (!targetDir.Exists)
            {
                targetDir.Create();
            }

            FileInfo[] rootFiles = dir.GetFiles();

            for (int i = 0; i < rootFiles.Length; ++i)
            {
                string tmp = Path.Combine(path, rootFiles[i].Name);
                rootFiles[i].CopyTo(tmp);
            }

            if (recursive)
            {
                foreach (DirectoryInfo subDir in dir.GetDirectories())
                {
                    string subPath = Path.Combine(path, subDir.Name);
                    CopyDirectory(subDir, subPath, true);
                }
            }
        }
    }

    internal static class ProjectBuilderUI
    {
        private static string FolderField(string label, string value)
        {
            GUILayout.BeginHorizontal();
            EditorGUI.BeginChangeCheck();
            string newText = EditorGUILayout.TextField(label, value);
            if (EditorGUI.EndChangeCheck())
            {
                value = newText;
            }
            if (GUILayout.Button("Browse", GUILayout.Width(120f)))
            {
                string newFolder = EditorUtility.OpenFolderPanel("Select Folder", value ?? Application.dataPath, "");
                if (!string.IsNullOrEmpty(newFolder))
                {
                    value = newFolder;

                }
            }
            GUILayout.EndHorizontal();
            return value;
        }

        [SettingsProvider]
        public static SettingsProvider CreateSettingsProvider()
        {
            var rawSettings = ProjectBuilder.RawLoad();
            if (rawSettings == null)
                rawSettings = ScriptableObject.CreateInstance<DirtBuild>();
            string projName = rawSettings.ProjectName;
            string binaryName = rawSettings.BinaryName;

            if ( string.IsNullOrEmpty(rawSettings.ProjectName))
                projName = "New Project";
            if (string.IsNullOrEmpty(rawSettings.BinaryName))
                binaryName = "NewProject";

            var provider = new SettingsProvider(Path.Combine("Project/Dirt/", projName), SettingsScope.Project)
            {
                label = binaryName,
                guiHandler = (searchContext) =>
                {
                    var settings = ProjectBuilder.Settings;
                    EditorGUI.BeginChangeCheck();
                    settings.ProjectName = EditorGUILayout.TextField("Project Name", settings.ProjectName);
                    settings.BinaryName = EditorGUILayout.TextField("Binary Name", settings.BinaryName);
                    settings.ContentPath = FolderField("Content Folder", settings.ContentPath);
                    settings.BuildNumber = EditorGUILayout.IntField("Build Number", settings.BuildNumber);
                    if (EditorGUI.EndChangeCheck())
                    {
                        EditorUtility.SetDirty(settings);
                    }
                },
                keywords = new HashSet<string>(new string[] { binaryName, projName }),
                deactivateHandler = () =>
                {
                    AssetDatabase.SaveAssets();
                },
            };
            return provider;
        }
    }
}
