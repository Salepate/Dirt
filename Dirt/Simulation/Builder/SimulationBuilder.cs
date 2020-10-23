using Dirt.Game.Content;
using Dirt.Simulation.Model;
using Dirt.Simulation.SystemHelper;
using Dirt.Simulation.Utility;
using System.Collections.Generic;

namespace Dirt.Simulation.Builder
{
    public class SimulationBuilder
    {
        private Dictionary<string, System.Type> m_ValidSystems;

        public SimulationBuilder()
        {
            m_ValidSystems = new Dictionary<string, System.Type>();

        }

        public ISimulationSystem[] CreateSystems(string archetypeName, IContentProvider contentProvider, bool isServer, out string contextName)
        {
            string archetype = $"sim.{archetypeName}";
            SimulationArchetype newArchetype = contentProvider.LoadContent<SimulationArchetype>(archetype);
            string[] systems = CreateSystemNames(newArchetype, contentProvider, isServer);
            contextName = newArchetype.Context;
            return CreateSystems(systems);
        }

        public ISimulationSystem[] CreateSystems(string[] systemNames)
        {
            List<ISimulationSystem> systems = new List<ISimulationSystem>();
            for (int i = 0; i < systemNames.Length; ++i)
            {
                System.Type sysType = m_ValidSystems[systemNames[i]];
                ISimulationSystem sys = (ISimulationSystem)System.Activator.CreateInstance(sysType);
                systems.Add(sys);
            }
            return systems.ToArray();
        }

        private string[] CreateSystemNames(SimulationArchetype simulation, IContentProvider contentProvider, bool isServer)
        {
            List<string> sys = new List<string>();

            for (int i = 0; i < simulation.Packs.Length; ++i)
            {
                SystemPack pack = contentProvider.LoadContent<SystemPack>($"systempack.{simulation.Packs[i]}");
                for (int j = 0; j < pack.Shared.Length; ++j)
                {
                    sys.Add(pack.Shared[j]);
                }
                string[] exclusiveRules = isServer ? pack.Server : pack.Client;
                for (int j = 0; j < exclusiveRules.Length; ++j)
                {
                    sys.Add(exclusiveRules[j]);
                }
            }
            return sys.ToArray();
        }

        public void LoadAssemblies(AssemblyCollection collection)
        {
            m_ValidSystems = AssemblyReflection.BuildTypeMap<ISimulationSystem>(collection.Assemblies);
        }
    }
}
