using Dirt.Game.Content;
using Dirt.Log;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;

using Type = System.Type;

namespace Dirt.Game
{
    public class ContentProvider : IContentProvider
    {
        public string LoadedManifestName { get; private set; }
        private GameContent m_RawMap;
        private Dictionary<string, object> m_ContentBufferMap;
        private Dictionary<string, string> m_ContentMap;

        private DirectoryInfo m_ContentDirectory;
        public ContentProvider(string contentPath)
        {
            LoadedManifestName = string.Empty;

            m_ContentDirectory = new DirectoryInfo(contentPath);
            m_ContentMap = new Dictionary<string, string>();
            m_ContentBufferMap = new Dictionary<string, object>();

            if (!m_ContentDirectory.Exists)
            {
                Log.Console.Message($"Invalid Content Directory {m_ContentDirectory.FullName}");
            }
        }

        public bool HasContent(string contentName)
        {
            return m_ContentMap.ContainsKey(contentName);
        }

        public JObject LoadContent(string contentName)
        {
            JObject res = null;

            if (!m_ContentBufferMap.TryGetValue(contentName, out object bufferValue))
            {
                if (m_ContentMap.TryGetValue(contentName, out string assetPath))
                {
                    res = DeserializeContent(Path.Combine(m_ContentDirectory.FullName, assetPath));
                    if (res != null)
                    {
                        m_ContentBufferMap.Add(contentName, res);
                    }
                }
                else
                {
                    Log.Console.Message($"Unknown asset {contentName}");
                }
            }
            else
            {
                res = (JObject)bufferValue;
            }


            return res;
        }

        public void LoadGameContent(string contentManifest)
        {
            LoadedManifestName = contentManifest;
            string manifestPath = Path.Combine(m_ContentDirectory.FullName, $"{contentManifest}.json");
            SetContent(DeserializeContent<GameContent>(manifestPath));
        }

        public object LoadContent(string contentName, Type contentType)
        {
            object res = default;

            if (!m_ContentBufferMap.TryGetValue(contentName, out res))
            {
                if (m_ContentMap.TryGetValue(contentName, out string assetPath))
                {
                    res = DeserializeContent(Path.Combine(m_ContentDirectory.FullName, assetPath), contentType);
                    if (res != null)
                    {
                        m_ContentBufferMap.Add(contentName, res);
                    }
                }
                else
                {
                    Log.Console.Warning($"Unknown asset {contentName}");
                }
            }
            return res;
        }

        public string LoadContentAsText(string contentName)
        {
            string res = string.Empty;
            if (m_ContentBufferMap.TryGetValue(contentName, out object bufferValue))
            {
                res = (string)bufferValue;
            }
            else
            {
                if (m_ContentMap.TryGetValue(contentName, out string assetPath))
                {
                    res = File.ReadAllText(Path.Combine(m_ContentDirectory.FullName, assetPath));
                    if (res != null)
                    {
                        m_ContentBufferMap.Add(contentName, res);
                    }
                }
                else
                {
                    Log.Console.Warning($"Unknown asset {contentName}");
                }
            }

            return res;
        }

        public T LoadContent<T>(string contentName)
        {
            T res = default;

            if (m_ContentBufferMap.TryGetValue(contentName, out object bufferValue))
            {
                res = (T)bufferValue;
            }
            else
            {
                if (m_ContentMap.TryGetValue(contentName, out string assetPath))
                {
                    res = DeserializeContent<T>(Path.Combine(m_ContentDirectory.FullName, assetPath));
                    if (res != null)
                    {
                        m_ContentBufferMap.Add(contentName, res);
                    }
                }
                else
                {
                    Log.Console.Warning($"Unknown asset {contentName}");
                }
            }


            return res;
        }

        public static JObject DeserializeContent(string assetPath)
        {
            try
            {
                JObject res = JObject.Parse(File.ReadAllText(assetPath));
                return res;

            }
            catch (System.Exception e)
            {
                Log.Console.Error($"Unable to read {assetPath}\n{e.ToString()}");
            }
            return default;

        }

        public static object DeserializeContent(string assetPath, Type contentType)
        {
            try
            {
                return JsonConvert.DeserializeObject(File.ReadAllText(assetPath), contentType);

            }
            catch (System.Exception e)
            {
                Log.Console.Error($"Unable to read {assetPath}\n{e.ToString()}");
            }
            return default;

        }

        public static T DeserializeContent<T>(string assetPath)
        {
            try
            {
                T res = JsonConvert.DeserializeObject<T>(File.ReadAllText(assetPath));
                return res;

            }
            catch (System.Exception e)
            {
                Log.Console.Error($"Unable to read {assetPath}\n{e.ToString()}");
            }
            return default;

        }

        private void SetContent(GameContent content)
        {
            m_ContentMap.Clear();
            m_ContentBufferMap.Clear();
            m_RawMap = content;

            foreach (var contentEntry in content.FileMap)
            {
                string contentPath = Path.Combine(m_ContentDirectory.FullName, contentEntry.Value);
                FileInfo contentFile = new FileInfo(contentPath);
                if (contentFile.Exists)
                {
                    m_ContentMap.Add(contentEntry.Key, contentEntry.Value);
                }
                else
                {
                    Log.Console.Warning($"Missing asset file {contentEntry.Value} (for {contentEntry.Key})");
                }
            }
        }

        public GameContent GetContentMap()
        {
            return m_RawMap;
        }
    }
}
