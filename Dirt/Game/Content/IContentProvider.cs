using Newtonsoft.Json.Linq;

namespace Dirt.Game.Content
{
    public interface IContentProvider
    {
        T LoadContent<T>(string contentName);
        object LoadContent(string contentName, System.Type contentType);
        JObject LoadContent(string contentName);
        bool HasContent(string contentName);
        GameContent GetContentMap();
    }
}
