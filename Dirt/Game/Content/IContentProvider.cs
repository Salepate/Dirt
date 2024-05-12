using Newtonsoft.Json.Linq;

namespace Dirt.Game.Content
{
    public interface IContentProvider : IGameManager
    {
        T LoadContent<T>(string contentName);
        object LoadContent(string contentName, System.Type contentType);
        JObject LoadAsJObject(string contentName);
        string LoadAsText(string contentName);
        bool HasContent(string contentName);

        void LoadManifest(string manifestName);
        GameContent GetContentMap();
    }
}
