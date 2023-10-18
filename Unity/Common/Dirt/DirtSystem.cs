using Dirt.Log;
using Framework;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using Array = System.Array;

namespace Dirt
{
    public abstract class DirtSystem
    {
        public bool HasScenes {  get { return Scenes.Count > 0; } }
        // update properties
        public virtual bool HasUpdate => false;
        public virtual bool HasLateUpdate => false;
        public virtual bool HasFixedUpdate => false;
        // data
        public List<string> Scenes { get; private set; }
        // init
        public virtual void InitializeContent() { }
        public virtual void SetupScenes() { }
        public virtual void OnScenesReady(SceneContent content) { }
        public virtual void Initialize(DirtMode mode) { }
        public virtual void Unload() { }
        // update cycles
        public virtual void Update() { }
        public virtual void LateUpdate() { }
        public virtual void FixedUpdate() { }

        public DirtSystem()
        {
            Scenes = new List<string>();
        }

        // protected api
        protected void LoadScene(string sceneName)
        {
            Scenes.Add(sceneName);
        }

        public class SceneContent
        {
            protected GameObject[] RootObjects { get; private set; }

            internal SceneContent(GameObject[] rootObjects)
            {
                RootObjects = rootObjects;
            }


            public T[] GetRootComponents<T>() where T: Component
            {
                List<T> res = new List<T>();
                for (int i = 0; i < RootObjects.Length; ++i)
                {
                    T comp = RootObjects[i].GetComponent<T>();
                    if (comp != null)
                        res.Add(comp);
                }
                return res.ToArray();
            }

            public T GetRootComponent<T>() where T: Component
            {
                for (int i = 0; i < RootObjects.Length; ++i)
                {
                    T comp = RootObjects[i].GetComponent<T>();
                    if (comp != null)
                        return comp;
                }
                Console.Error($"Could not find component {typeof(T).Name}");
                return default;
            }

            public T GetRootComponent<T>(string gameObjectName) where T : Component
            {
                for (int i = 0; i < RootObjects.Length; ++i)
                {
                    if ( RootObjects[i].name == gameObjectName)
                    {
                        T comp = RootObjects[i].GetComponent<T>();

                        if (comp != null)
                            return comp;
                        else
                            Console.Error($"Could not find component {nameof(T)} on Gameobject {gameObjectName}");
                    }
                }

                Console.Error($"Could not game object {gameObjectName}");

                return null;
            }
        }
    }
}