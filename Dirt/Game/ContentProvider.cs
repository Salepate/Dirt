using Dirt.Game.Content;
using Game.Content.Serializer;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;


namespace Dirt.Game
{
    using Dirt.Log;

    public class ContentProvider : IContentProvider
    {
        public string LoadedManifestName { get; private set; }
        public Dictionary<string, string> ContentMap => m_RawMap.FileMap;

        // Content Data
        private GameContent m_RawMap;
        private Dictionary<string, object> m_ContentBufferMap;
        private Dictionary<string, string> m_ContentMap;
        private DirectoryInfo m_ContentDirectory;
        // Serialization data
        private DefaultDeserializer m_JsonDeserializer;
        private Dictionary<string, IContentDeserializer> m_AddDeserializers;
        public ContentProvider(string contentPath)
        {
            LoadedManifestName = string.Empty;

            m_RawMap = new GameContent();
            m_JsonDeserializer = new DefaultDeserializer();
            m_AddDeserializers = new Dictionary<string, IContentDeserializer>();

            m_ContentDirectory = new DirectoryInfo(contentPath);
            m_ContentMap = new Dictionary<string, string>();
            m_ContentBufferMap = new Dictionary<string, object>();

            if (!m_ContentDirectory.Exists)
            {
                Console.Message($"Invalid Content Directory {m_ContentDirectory.FullName}");
            }
        }

        /// <summary>
        /// Main initialization routine. Loads up a manifest and makes its content ready for load ops
        /// </summary>
        /// <param name="contentManifest"></param>
        public void LoadManifest(string contentManifest)
        {
            LoadedManifestName = contentManifest;
            string manifestPath = Path.Combine(m_ContentDirectory.FullName, $"{contentManifest}.json");

            try
            {
                GameContent content = m_JsonDeserializer.DeserializeContent<GameContent>(File.ReadAllText(manifestPath));
                SetGameContent(content);
            }
            catch (Exception e)
            {
                Console.Error($"Failed to load content manifest {contentManifest}\n{e.ToString()}");
            }
        }

        /// <summary>
        /// Legacy method to get the raw content map
        /// </summary>
        /// <returns></returns>
        public GameContent GetContentMap() => m_RawMap;

        /// <summary>
        /// Get content directory absolute path
        /// </summary>
        /// <returns></returns>
        public string GetContentDirectory() => m_ContentDirectory.FullName;


        /// <summary>
        /// Provide a custom deserializer for a specific extension
        /// </summary>
        /// <param name="extension"></param>
        /// <param name="deserializer"></param>
        public void AddDeserializer(string extension, IContentDeserializer deserializer)
        {
            m_AddDeserializers.Add(extension, deserializer);
        }

        /// <summary>
        /// Provide a single deserializer for multiple extensions
        /// </summary>
        /// <param name="extensions"></param>
        /// <param name="deserializer"></param>
        public void AddDeserializer(string[] extensions, IContentDeserializer deserializer)
        {
            for(int i = 0; i < extensions.Length; ++i)
            {
                m_AddDeserializers.Add(extensions[i], deserializer);
            }
        }

        /// <summary>
        /// Will ensure all content are reloaded again in memory next time
        /// </summary>
        public void ClearBuffer()
        {
            m_ContentBufferMap.Clear();
        }

        /// <summary>
        /// Check if a content is present based on the loaded manifest
        /// </summary>
        /// <param name="contentName"></param>
        /// <returns></returns>
        public bool HasContent(string contentName)
        {
            return m_ContentMap.ContainsKey(contentName);
        }

        public object LoadContent(string contentName, Type contentType)
        {
            object? res = default;

            if (!m_ContentBufferMap.TryGetValue(contentName, out res))
            {
                if (m_ContentMap.TryGetValue(contentName, out string assetPath))
                {
                    IContentDeserializer deserializer = GetDeserializerFromExtension(assetPath);
                    string filePath = Path.Combine(m_ContentDirectory.FullName, assetPath);

                    try
                    {
                        res = deserializer.DeserializeContent(File.ReadAllText(filePath), contentType);
                    }
                    catch(System.Exception e)
                    {
                        Console.Error($"Failed to deserialize {contentName}\n{e.ToString()}");
                    }

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
                    IContentDeserializer deserializer = GetDeserializerFromExtension(assetPath);
                    string filePath = Path.Combine(m_ContentDirectory.FullName, assetPath);

                    try
                    {
                        res = deserializer.DeserializeContent<T>(File.ReadAllText(filePath));
                    }
                    catch (System.Exception e)
                    {
                        Console.Error($"Failed to deserialize {contentName}\n{e.ToString()}");
                    }

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

        public string LoadAsText(string contentName)
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

        public JObject LoadAsJObject(string contentName)
        {
            JObject res = null;

            if (!m_ContentBufferMap.TryGetValue(contentName, out object bufferValue))
            {
                if (m_ContentMap.TryGetValue(contentName, out string assetPath))
                {
                    res = ReadJObject(Path.Combine(m_ContentDirectory.FullName, assetPath));
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

        private JObject ReadJObject(string assetPath)
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


        private void SetGameContent(GameContent content)
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

        private IContentDeserializer GetDeserializerFromExtension(string path)
        {
            string extension = Path.GetExtension(path);
            if (m_AddDeserializers.TryGetValue(extension, out IContentDeserializer deserializer))
            {
                return deserializer;
            }

            return m_JsonDeserializer;
        }

        public void Update(float deltaTime)
        {
        }
    }
}
