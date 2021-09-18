using Dirt.Simulation;
using System.Collections.Generic;

namespace Dirt.GameServer.Simulation.Components
{
    [System.Serializable]
    public struct GlobalSyncInfo : IComponentAssign
    {
        public int Client;
        public List<int> SynchronizedActors;

        public void Assign()
        {
            SynchronizedActors = new List<int>();
        }
    }
}
