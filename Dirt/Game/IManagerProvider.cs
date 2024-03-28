namespace Dirt.Game
{
    public interface IManagerProvider
    {
        T GetManager<T>() where T : IGameManager;
    }
}
