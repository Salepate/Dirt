using Dirt.Game;
using Dirt.Log;
using Mud.DirtSystems;
using Mud.Framework;

namespace Mud.Managers
{
    public class ServerProxy : IGameManager
    {
        private MudConnector m_Connector;
        public int LocalPlayer { get; private set; }
        public ServerSocket Socket { get { return m_Connector.Socket; } }
        public ServerProxy(MudConnector connector)
        {
            m_Connector = connector;
        }

        public void SetLocalPlayer(int number)
        {
            Console.Message($"Local Player Number: {number}");
            LocalPlayer = number;
        }

        public void Update(float deltaTime)
        {
        }
    }
}
