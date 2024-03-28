using System;

namespace Mud.Server
{
    using Console = Dirt.Log.Console;
    /// <summary>
    /// Holds connected client network information
    /// </summary>
    public class GameClient
    {
        public const float CLIENT_PING = 15f;
        public string ID { get; private set; }
        public int Number { get; private set; }
        public bool RequestDisconnection { get; private set; }
        public float RTT { get; private set; }
        public float LastMessageStamp { get; set; }
        public float LastPingStamp { get; set; }
        public int PingMS { get; set; }
        private ClientSocket m_Socket;
        private bool m_Auth;
        private MudAddress m_Address;
        private Random m_AnonID;
        private byte[] m_PingBuffer;
        internal GameClient(MudAddress clientAddress, ClientSocket socket, int number, float minRTT = 80f / 1000f)
        {
            m_Auth = false;
            m_Address = clientAddress;
            m_Socket = socket;
            Number = number;
            RequestDisconnection = false;
            m_AnonID = new Random();
            RTT = minRTT;
            m_PingBuffer = new byte[1];
        }

        /// <summary>
        /// Send a message to client (only support byte array)
        /// </summary>
        /// <see cref="MudMessage.Create"/>
        /// <param name="message">mud message</param>
        public void Send(MudMessage message) => Send(message, false);

        public void SendRaw(byte[] buffer, int bufferSize) => m_Socket.Send(buffer, bufferSize, false);

        /// <summary>
        /// Send a message to client (only support byte array)
        /// </summary>
        /// <see cref="MudMessage.Create"/>
        /// <param name="message">mud message</param>
        /// <param name="reliable">if true, require the client to confirm message</param>
        public void Send(MudMessage message, bool reliable)
        {
            if (message.buffer.Length <= MudSocket.BufferSize)
            {
                m_Socket.Send(message, reliable);
            }
            else
            {
                SendLargeMessage(message.opCode, message.buffer);
            }
        }

        /// <summary>
        /// If invoked, player will get disconnected
        /// </summary>
        public void ForceDisconnect()
        {
            RequestDisconnection = true;
        }

        public void ChangeClientName(string newName)
        {
            ID = newName;
        }

        internal void UpdateSocket(float deltaTime)
        {
            m_Socket.SendReliables(deltaTime, RTT);
        }

        internal void SendLargeMessage(int op, byte[] message)
        {
            int packetCount = message.Length / MudSocket.BufferSize;
            int rest = message.Length - packetCount * MudSocket.BufferSize;

            if (rest > 0)
                ++packetCount;

            if ( packetCount > byte.MaxValue )
            {
                throw new Exception($"Message too large (${message.Length} bytes)");
            }

            m_Socket.Send(MudMessage.Create(MudOperation.MultiPackets, new byte[] { (byte)op, (byte)packetCount }));

            byte[] temp = new byte[MudSocket.BufferSize];
            for (int i = 0; i < packetCount; ++i)
            {
                int length = (i < packetCount - 1 || rest == 0) ? MudSocket.BufferSize : rest;
                Array.Copy(message, i * MudSocket.BufferSize, temp, 0, length);

                m_Socket.Send(MudMessage.Create(MudOperation.MultiPackets, temp));
            }
        }

        public override string ToString()
        {
            return $"{(m_Auth ? ID : "Anon")} ({m_Address.IP})";
        }

        public ClientOperation OnMessage(MudOperation op, byte[] buffer)
        {
            switch (op)
            {
                case MudOperation.Ping:
                    PingMS = (int)((LastMessageStamp - LastPingStamp) * 500);
                    return ClientOperation.Idle;
                case MudOperation.ClientAuth:
                    if ( !m_Auth )
                    {
                        int ranId = m_AnonID.Next(1, 9999);
                        string userAuth = $"player{ranId.ToString("D4")}";
                        ID = userAuth;
                        m_Auth = true;
                        Console.Message($"Client {ToString()}: Authed as {ID} (Player {Number})");
                        m_Socket.Send(MudMessage.Create(MudOperation.ValidateAuth, new byte[] { (byte) Number }));
                        return ClientOperation.Connect;
                    }
                    return ClientOperation.Idle;
                case MudOperation.MultiPackets:
                    throw new Exception("Client multipacket not supported");
                case MudOperation.ReliableConfirm:
                    m_Socket.ConfirmReliableMessage(buffer[0]);
                    return ClientOperation.Idle;
                case MudOperation.Disconnect:
                    {
                        Console.Message($"Client {ToString()}: Disconnect");
                        RequestDisconnection = true;
                        if ( m_Auth )
                        {
                            return ClientOperation.Disconnect;
                        }
                        return ClientOperation.Idle;
                    }
                default:
                    if ( !m_Auth )
                    {
                        m_Socket.Send(MudMessage.Error("Client is not authed"));
                    } else
                    {
                        return ClientOperation.PassThrough; // custom message
                    }
                    return ClientOperation.Idle;
            }
        }

        internal void SendPing(float m_Clock)
        {
            LastPingStamp = m_Clock;
            m_PingBuffer[0] = (byte)PingMS;
            m_Socket.Send(MudMessage.Create(MudOperation.Ping, m_PingBuffer));
        }
    }
}
