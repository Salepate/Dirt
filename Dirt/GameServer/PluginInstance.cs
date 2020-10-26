using Dirt.Game;
using Dirt.Game.Model;
using Dirt.Simulation.Builder;

namespace Dirt.GameServer
{
    [System.Serializable]
    public abstract class PluginInstance
    {
        protected IManagerProvider Managers { get; private set; }
        public abstract string PluginName { get; }
        public virtual string DefaultSimulation => "lobby";
        public virtual void PlayerJoined(PlayerProxy player) {}
        public virtual void PlayerLeft(PlayerProxy player) {}
        public void SetManagers(IManagerProvider managers)
        {
            Managers = managers;
        }
    }
}
