using Dirt.Game;

namespace Dirt.Tests.Mocks
{
    public class MockManagerProvider : IManagerProvider
    {
        public T GetManager<T>() where T : IGameManager
        {
            return default(T);
        }
    }
}
