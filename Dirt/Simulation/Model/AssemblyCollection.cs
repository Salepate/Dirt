namespace Dirt.Simulation.Model
{
    [System.Serializable]
    public class AssemblyCollection
    {
        public string[] Assemblies;

        public AssemblyCollection()
        {
            Assemblies = new string[0];
        }
    }
}