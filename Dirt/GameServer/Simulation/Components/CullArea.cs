using Dirt.Simulation;
using System.Collections.Generic;

namespace Dirt.GameServer.Simulation.Components
{
    [System.Serializable]
    public class CullArea : IComponent
    {
        public int Client;
        public float Radius;
        public float Threshold;
        public List<int> ProximityActors = new List<int>();
    }
}
