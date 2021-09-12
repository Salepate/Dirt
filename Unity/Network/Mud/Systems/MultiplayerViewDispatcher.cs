using Dirt.Log;
using Dirt.Model;
using Dirt.Network.Simulation.Components;
using Dirt.Simulation;
using Mud.Managers;

namespace Dirt.Systems
{
    public abstract class MultiplayerViewDispatcher : SimulationViewDispatcher
    {
        private ServerProxy m_ServerProxy;

        public override void Initialize(DirtMode mode)
        {
            base.Initialize(mode);

            SimulationSystem simSys = mode.FindSystem<SimulationSystem>();
            if (simSys.HasManager<ServerProxy>())
                m_ServerProxy = simSys.GetManager<ServerProxy>();
            else
                Console.Error($"Server Proxy not found, unable to dispatch network views properly");
        }

        protected override bool SpawnView(ViewDefinition viewDef, GameActor targetActor)
        {
            int netInfoIdx = targetActor.GetComponentIndex<NetInfo>();

            if ( netInfoIdx != -1 && m_ServerProxy != null )
            {
                NetInfo netInfo = (NetInfo) targetActor.Components[netInfoIdx];
                bool isActorOwned = netInfo.Owner == m_ServerProxy.LocalPlayer;
                bool showActor = true;
                switch (viewDef.Display)
                {
                    case ViewDefinition.ViewDisplay.Others:
                        showActor = !isActorOwned;
                        break;
                    case ViewDefinition.ViewDisplay.Local:
                        showActor = isActorOwned;
                        break;
                }

                return showActor;
            }

            return base.SpawnView(viewDef, targetActor);
        }
    }
}
