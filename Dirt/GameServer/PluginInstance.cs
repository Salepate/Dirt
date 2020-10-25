using Dirt.Game;
using Dirt.Game.Model;

namespace Dirt.GameServer
{
    [System.Serializable]
    public abstract class PluginInstance
    {
        protected IManagerProvider Managers { get; private set; }
        public abstract string PluginName { get; }

        public virtual string GetDefaultSimulation => "lobby";

        public virtual void PlayerJoined(GamePlayer player) {}

        public virtual void PlayerLeft(GamePlayer player) {}

        public void SetManagers(IManagerProvider managers)
        {
            Managers = managers;
        }
    }
}
