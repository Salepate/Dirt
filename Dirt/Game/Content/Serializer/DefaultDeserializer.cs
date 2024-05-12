using Dirt.Game.Content;
using Newtonsoft.Json;
using System;

namespace Game.Content.Serializer
{
    public class DefaultDeserializer : IContentDeserializer
    {
        private JsonSerializerSettings m_Settings;

        public DefaultDeserializer()
        {
            m_Settings = new JsonSerializerSettings()
            {
                Culture = System.Globalization.CultureInfo.InvariantCulture
            };
        }

        public object? DeserializeContent(string textContent, Type contentType)
        {
            return JsonConvert.DeserializeObject(textContent, contentType, m_Settings);
        }

        public T DeserializeContent<T>(string textContent)
        {
            return JsonConvert.DeserializeObject<T>(textContent, m_Settings);
        }
    }
}
