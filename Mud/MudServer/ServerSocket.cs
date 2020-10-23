using System;
using System.Net;
using System.Net.Sockets;

namespace Mud
{
    public class ServerSocket : MudSocket
    {
        private UdpClient m_Socket;
        private IPEndPoint m_ClientEndPoint;

        public ServerSocket(UdpClient socket, MudAddress clientAddress)
        {
            m_Socket = socket;
            m_ClientEndPoint = new IPEndPoint(IPAddress.Parse(clientAddress.IP), clientAddress.Port);
        }

        protected override void SendNetworkMessage(byte[] message, int messageLength)
        {
            m_Socket.Send(message, messageLength, m_ClientEndPoint);
        }
    }
}