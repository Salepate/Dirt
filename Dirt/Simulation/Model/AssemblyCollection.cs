using Dirt.Simulation.Context;

namespace Dirt.Simulation.Model
{
    [System.Serializable]
    public class AssemblyCollection : IContextItem
    {
        public string[] Assemblies;

        public AssemblyCollection()
        {
            Assemblies = new string[0];
        }
    }
}