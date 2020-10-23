using System.Collections.Generic;

namespace Dirt.Simulation.Model
{
    [System.Serializable]
    public class ActorArchetype
    {
        public string[] Components;
        public Dictionary<string, ComponentParameters> Settings;
    }
}
