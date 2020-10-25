using Dirt.Simulation;
using System.Collections.Generic;

namespace Dirt.GameServer.Simulation.Components
{
    [System.Serializable]
    public class GlobalSyncInfo : IComponent
    {
        public int Client;
        public List<int> SynchronizedActors = new List<int>();
    }
}
