using Newtonsoft.Json.Linq;

namespace Dirt.Game.Content
{
    public interface IContentProvider
    {
        T LoadContent<T>(string contentName);
        object LoadContent(string contentName, System.Type contentType);
        JObject LoadContent(string contentName);
        string LoadContentAsText(string contentName);
        bool HasContent(string contentName);

        void LoadGameContent(string manifestName);
        GameContent GetContentMap();
    }
}
