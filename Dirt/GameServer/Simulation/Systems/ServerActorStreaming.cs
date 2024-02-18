using Dirt.Game;
using Dirt.Game.Managers;
using Dirt.Network.Simulation.Components;
using Dirt.Network.Systems;
using Dirt.Simulation.SystemHelper;

namespace Dirt.GameServer.Simulation.Systems
{
    public class ServerActorStreaming : ActorStreaming, IManagerAccess
    {
        private MetricsManager m_Metrics;
  
        protected override bool ShouldSerializeActor(ref NetInfo info)
        {
            return info.SyncClock <= 0f && base.ShouldSerializeActor(ref info);
        }

        protected override void DoRecord(string id, int value)
        {
            m_Metrics.Record(id, value);
        }
        public void SetManagers(IManagerProvider provider)
        {
            m_Metrics = provider.GetManager<MetricsManager>();
        }
    }
}
