using Dirt.Game.Content;
using Newtonsoft.Json.Linq;
using System;

namespace Dirt.Tests.Mocks
{
    internal class MockContentProvider : IContentProvider
    {
        public GameContent GetContentMap()
        {
            throw new NotImplementedException();
        }

        public bool HasContent(string contentName)
        {
            throw new NotImplementedException();
        }

        public T LoadContent<T>(string contentName)
        {
            return default(T);
        }

        public JObject LoadContent(string contentName)
        {
            return new JObject();
        }

        public object LoadContent(string contentName, Type contentType)
        {
            throw new NotImplementedException();
        }

        public string LoadContentAsText(string contentName)
        {
            throw new NotImplementedException();
        }

        public void LoadGameContent(string manifestName)
        {
        }

        public void Update(float deltaTime)
        {
        }
    }
}
