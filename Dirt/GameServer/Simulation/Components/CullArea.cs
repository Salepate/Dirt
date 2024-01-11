using Dirt.Simulation;
using System.Collections.Generic;

namespace Dirt.GameServer.Simulation.Components
{
    [System.Serializable]
    public struct CullArea : IComponent, IComponentAssign
    {
        public int Client;
        public float Radius;
        public float Threshold;
        public List<int> ProximityActors;
        public List<int> LocalIDs;

        public void Assign()
        {
            ProximityActors = new List<int>(16);
            LocalIDs = new List<int>(16);
        }
    }
}
