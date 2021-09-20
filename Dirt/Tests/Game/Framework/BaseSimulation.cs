using Dirt.Game;
using Dirt.Game.Content;
using Dirt.Simulation;
using Dirt.Simulation.Builder;
using Dirt.Simulation.Model;
using Dirt.Tests.Mocks;

namespace Dirt.Tests.Framework
{
    public class BaseSimulation : Framework.DirtTest
    {
        private IContentProvider m_Content;
        private IManagerProvider m_Managers;
        protected SystemContainer Container { get; private set; }
        protected GameSimulation Simulation { get; private set; }
        protected ActorBuilder Builder => Simulation.Builder;

        public override void Initialize()
        {
            base.Initialize();
            m_Content = new MockContentProvider();
            m_Managers = new MockManagerProvider();
            Container = new SystemContainer(m_Content, m_Managers);
            Simulation = new GameSimulation();
            Builder.LoadAssemblies(new AssemblyCollection() { Assemblies = new string[] {
                "Dirt.Simulation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null",
                "Dirt.Tests, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null"} 
            });
            Container.InitializeSystems(Simulation);
        }
    }
}
