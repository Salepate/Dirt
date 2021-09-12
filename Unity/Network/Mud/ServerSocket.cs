using System.Net;
using System.Net.Sockets;

namespace Mud.Framework
{
    public class ServerSocket : MudSocket
    {
        private UdpClient m_Socket;
        private IPEndPoint m_Endpoint;
        public ServerSocket(string address, int port)
        {
            m_Socket = new UdpClient();
            m_Endpoint = new IPEndPoint(IPAddress.Parse(address), port);
            m_Socket.Connect(m_Endpoint);
        }
        protected override void SendNetworkMessage(byte[] message, int messageLength)
        {
            m_Socket.Send(message, messageLength);
        }

        public byte[] Receive()
        {
            return m_Socket.Receive(ref m_Endpoint);
        }

        public int Timeout { get { return m_Socket.Client.ReceiveTimeout; } set { m_Socket.Client.ReceiveTimeout = value; } }
    }
}