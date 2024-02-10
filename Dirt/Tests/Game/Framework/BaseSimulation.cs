using Dirt.Game;
using Dirt.Game.Content;
using Dirt.Game.Managers;
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

            if ( MetricsManager.Recording )
            {
               ((MockManagerProvider) m_Managers).AddManager(new MetricsManager());
            }

            Container = new SystemContainer(m_Content, m_Managers);
            Simulation = new GameSimulation(0, 1000, 10);
            Builder.LoadAssemblies(new AssemblyCollection() { Assemblies = new string[] {
                "Dirt.Simulation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null",
                "Dirt.Tests, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null"} 
            });
            Simulation.Resize(10000, 100);
            Container.InitializeSystems(Simulation);
        }
    }
}
