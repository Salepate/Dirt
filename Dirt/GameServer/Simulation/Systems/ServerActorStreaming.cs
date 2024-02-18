using Dirt.Game;
using Dirt.Game.Managers;
using Dirt.Network.Simulation.Components;
using Dirt.Network.Systems;
using Dirt.Simulation;
using System;

namespace Dirt.GameServer.Simulation.Systems
{
    public class ServerActorStreaming : ActorStreaming
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
        public override void SetManagers(IManagerProvider provider)
        {
            base.SetManagers(provider);

            m_Metrics = provider.GetManager<MetricsManager>();
        }
    }
}
