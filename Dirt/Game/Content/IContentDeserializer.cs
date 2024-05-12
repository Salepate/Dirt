namespace Dirt.Game.Content
{
    public interface IContentDeserializer
    {
        object? DeserializeContent(string textContent, System.Type contentType);
        T DeserializeContent<T>(string textContent);
    }
}