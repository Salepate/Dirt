using Dirt.Log;
using Dirt.Reflection;
using Framework;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.SceneManagement;
using static Dirt.DirtSystem;

namespace Dirt.States
{
    public class LoadState : State<DirtMode>
    {
        public int m_SystemIndex;
        private int m_SceneIndex;

        private List<GameObject> m_SystemObjects;


        private bool AllSystemsLoaded {  get { return m_SystemIndex >= controller.Systems.Count; } }

        private bool m_Waiting;

        public override void OnEnter()
        {
            m_SystemObjects = new List<GameObject>();
            m_SystemIndex = 0;
            m_SceneIndex = 0;
            m_Waiting = false;

            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        public override void OnLeave()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            controller.SetSystemsReady();
        }


        private DirtSystemContent GetContent(string contentName)
        {
            Dictionary<string, DirtSystemContent> contentMap = controller.ContentMap;

            if (!contentMap.TryGetValue(contentName, out DirtSystemContent content))
            {
                content = Resources.Load<DirtSystemContent>(contentName);
                if (content != null)
                {
                    Console.Message($"Loaded {contentName} content");
                    contentMap.Add(contentName, content);
                }
                else
                {
                    Console.Warning($"Unable to load content {contentName}, file not found");
                }
            }

            return content;
        }

        public void InitializeSystem(DirtSystem system)
        {
            Console.Message($"Loading System {system.GetType().Name}");
            var contentMap = controller.ContentMap;
            System.Type systemType = system.GetType();


            DirtSystemMeta meta = new DirtSystemMeta(system.GetType());

            if ( meta.HasContent )
            {
                meta.ContentFields.ForEach(cf =>
                {
                    cf.Inject(system, GetContent(cf.ContentName));
                });

                system.InitializeContent();
            }
            system.SetupScenes();

            if ( system.HasScenes )
            {
                m_Waiting = true;
                m_SceneIndex = 0;
                LoadScene();
            }
            else
            {
                controller.InjectDependencies(system);
                system.Initialize(controller);
                OnSystemReady();
            }
        }
        public override void Update()
        {
            if ( AllSystemsLoaded )
            {
                Console.Message($"{controller.GetType().Name} systems loaded ({controller.Systems.Count})");
                SetState(DirtMode.Run);
            }
            else
            {
                if ( !m_Waiting)
                {
                    DirtSystem system = controller.Systems[m_SystemIndex];
                    InitializeSystem(system);
                }
            }
        }

        private void LoadScene()
        {
            DirtSystem system = controller.Systems[m_SystemIndex];
            string sceneName = system.Scenes[m_SceneIndex];
            Console.Message($"Loading Scene {sceneName} for system {system.GetType().Name}");
            SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            DirtSystem system = controller.Systems[m_SystemIndex];
            m_SceneIndex++;

            m_SystemObjects.AddRange(scene.GetRootGameObjects());

            controller.LoadedScenes.Add(scene);

            if (m_SceneIndex >= system.Scenes.Count)
            {
                SceneContent sceneContent = new SceneContent(m_SystemObjects.ToArray());
                system.OnScenesReady(sceneContent);
                m_SystemObjects.Clear();
                OnScenesLoaded();
            }
            else
            {
                LoadScene();
            }
        }

        private void OnScenesLoaded()
        {
            DirtSystem system = controller.Systems[m_SystemIndex];
            Console.Message($"{system.GetType().Name} scenes loaded");
            controller.InjectDependencies(system);
            system.Initialize(controller);
            OnSystemReady();
        }

        private void OnSystemReady()
        {
            m_Waiting = false;
            m_SystemIndex++;
        }
    }
}