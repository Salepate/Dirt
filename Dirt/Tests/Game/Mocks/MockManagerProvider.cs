using Dirt.Game;
using System;
using System.Collections.Generic;

namespace Dirt.Tests.Mocks
{
    public class MockManagerProvider : IManagerProvider
    {
        private Dictionary<Type, IGameManager> m_Managers = new Dictionary<Type, IGameManager>();
        public void AddManager<T>(T mgr) where T : IGameManager => m_Managers.Add(typeof(T), mgr);
        public T GetManager<T>() where T : IGameManager
        {
            if (m_Managers.TryGetValue(typeof(T), out IGameManager mgr))
                return (T)mgr;

            return default(T);
        }
    }
}
