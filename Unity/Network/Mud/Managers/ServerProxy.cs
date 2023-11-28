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

        private float m_PingClock;
        public const float PING_DELAY = 15f;
        public ServerProxy(MudConnector connector)
        {
            m_Connector = connector;
            m_PingClock = 0f;
        }

        public void SetLocalPlayer(int number)
        {
            Console.Message($"Local Player Number: {number}");
            LocalPlayer = number;
        }

        public void Update(float deltaTime)
        {
            //if ( m_Connector.Connected )
            //{
            //    m_PingClock += deltaTime;
            //    if (m_PingClock >= PING_DELAY )
            //    {
            //        Send(MudMessage.Create(MudOperation.Ping, null));
            //    }
            //}
        }

        public void Send(MudMessage message)
        {
            m_Connector.Socket.Send(message);
            m_PingClock = 0f;
        }
    }
}
