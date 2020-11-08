using Dirt.Game;
using Dirt.Game.Model;
using Dirt.Simulation.Builder;
using Dirt.Simulation.Context;
using System.Collections.Generic;

namespace Dirt.GameServer
{
    [System.Serializable]
    public abstract class PluginInstance
    {
        public abstract string PluginName { get; }
        public virtual string DefaultSimulation => "lobby";
        public virtual void Initialize(List<IContextItem> sharedContext, GameInstance game) {}
        public virtual void PlayerJoined(PlayerProxy player) {}
        public virtual void PlayerLeft(PlayerProxy player) {}
    }
}
