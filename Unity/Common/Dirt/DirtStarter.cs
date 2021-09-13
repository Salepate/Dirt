﻿using Dirt.Log;
using Framework;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Dirt
{
    public class DirtStarter : MonoBehaviour
    {
        private static DirtStarter s_Starter;

#if !DIRT_PRODUCTION
        public static bool IsDebug => s_Starter != null ? s_Starter.DebugGame : false;
        public bool DebugGame;
#else
        public static bool IsDebug => false;
#endif
        public bool LockFramerate;
        public int TargetFramerate;
        public string ServiceMode;
        public string InitialMode;
        public string ErrorSceneName;
        
        public static void LoadDefaultGameMode()
        {
            LoadMode(s_Starter.InitialMode);
        }

        public DirtMode Mode { get; private set; }

        //public Dictionary<System.Type, DirtSystem> Services { get; private set; }
        public List<DirtSystem> Services { get; private set; }

        public List<DirtMode> ServiceModes;

        private HashSet<DirtSystemContent> m_ServiceContent;

        private void Awake()
        {
            Console.Logger = new UnityLogger();
            if (LockFramerate)
                Application.targetFrameRate = TargetFramerate;
            //#if DIRT_DEBUG
            //            Console.Dump = true;
            //#endif

            //Services = new Dictionary<System.Type, DirtSystem>();
            m_ServiceContent = new HashSet<DirtSystemContent>();
            Services = new List<DirtSystem>();
            ServiceModes = new List<DirtMode>();

            s_Starter = this;

            string gameMode = ServiceMode;
            if (string.IsNullOrEmpty(gameMode))
                gameMode = InitialMode;

            if ( !string.IsNullOrEmpty(gameMode))
            {
                LoadMode(gameMode);
            }
            else
            {
                this.enabled = false;
            }

            Debug = new DirtDebug();
        }

        public static void LoadMode(string modeName)
        {
            Console.Assert(s_Starter != null, "DirtStarter missing");
            s_Starter.InternalLoadMode(modeName);
        }

        public static void LoadMode<T>() where T: DirtMode
        {
            Console.Assert(!typeof(T).IsAbstract, "Cannot load abstract mode");
            s_Starter.InternalLoadMode(typeof(T).FullName);
        }

        private void InternalLoadMode(string modeName)
        {
            System.Type modeType = AssemblyUtility.GetTypeFromName(modeName);
            Console.Assert(modeType != null, $"Unknown mode {modeName} {(modeName.Split('.').Length <= 1 ? "(namespace is mandatory)" : "")}");
            DirtMode mode = (DirtMode)System.Activator.CreateInstance(modeType);

            if (Mode != null)
            {
                if (!Mode.IsService)
                {
                    Console.Message($"Unloading mode {Mode.GetType().Name}");
                    UnloadMode(Mode);
                }
                else
                {
                    // mark content
                    foreach(KeyValuePair<string, DirtSystemContent> kvp in Mode.ContentMap)
                    {
                        if (!m_ServiceContent.Contains(kvp.Value))
                            m_ServiceContent.Add(kvp.Value);
                    }
                }
            }

            Mode = mode;
            Console.Message($"Loading mode {mode.GetType().Name}");
            Mode.Initialize(this);
        }

        private void UnloadMode(DirtMode mode)
        {
            mode.LoadedScenes.ForEach(scene => SceneManager.UnloadSceneAsync(scene));
            for (int i = mode.Systems.Count - 1; i >= 0; --i)
            {
                mode.Systems[i].Unload();
            }
            foreach(var kvp in mode.ContentMap)
            {
                if ( !m_ServiceContent.Contains(kvp.Value))
                    Resources.UnloadAsset(kvp.Value);
            }
            Resources.UnloadUnusedAssets();
            mode.ContentMap.Clear();
        }

        private void OnDisable()
        {
            if ( Mode != null )
                UnloadMode(Mode);
            for(int i = 0; i <Services.Count; ++i)
            {
                Services[i].Unload();
            }
            Services.Clear();
//#if DIRT_DEBUG
//            Console.SaveDump();
//#endif
        }

        private void Update()
        {
            try
            {
                Mode.Update();
                ServiceModes.ForEach(m => m.Update());
            }
            catch(System.Exception e)
            {
                if ( !string.IsNullOrEmpty(ErrorSceneName))
                {
                    SceneManager.LoadScene(ErrorSceneName, LoadSceneMode.Single);
                    Console.Error(e.StackTrace);
                    throw e;
                }
            }
        }

        private void LateUpdate()
        {
            Mode.LateUpdate();
            ServiceModes.ForEach(m => m.LateUpdate());
        }

        private void FixedUpdate()
        {
            Mode.FixedUpdate();
            ServiceModes.ForEach(m => m.FixedUpdate());
        }


        public static void EndGame(float delay)
        {
            s_Starter.Invoke("Restart", 1f + Mathf.Max(0f, delay));

        }

        private void Restart()
        {
            for (int i = 0; i < Services.Count; ++i)
            {
                Services[i].Unload();
            }
            Services.Clear();

            if ( Mode != null && Mode.IsService )
            {
                UnloadMode(Mode);
            }

            SceneManager.LoadSceneAsync(0, LoadSceneMode.Single);
        }

        public static DirtDebug Debug { get; private set; }

#if !DIRT_PRODUCTION
        private void OnGUI()
        {
            if ( DebugGame )
                Debug.GUICallback?.Invoke();
        }
#endif
    }
}