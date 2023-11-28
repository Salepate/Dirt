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
        public float LastPing { get; set; }
        private ClientSocket m_Socket;
        private bool m_Auth;
        private MudAddress m_Address;
        private Random m_AnonID;
        private float m_PingClock;
        private readonly float m_MinimumRTT;
        private bool m_RTTUpdate;
        private float m_RTTWatch; // measure rtt during ping
        private CircularBuffer<float> m_RTTMeasures;
        private float m_LowestRTT;
        private float m_HighestRTT;

        internal GameClient(MudAddress clientAddress, ClientSocket socket, int number, float minRTT = 80f / 1000f)
        {
            m_Auth = false;
            m_Address = clientAddress;
            m_Socket = socket;
            Number = number;
            RequestDisconnection = false;
            m_AnonID = new Random();
            m_RTTMeasures = new CircularBuffer<float>(4);
            RTT = minRTT;
            m_MinimumRTT = minRTT;
            m_LowestRTT = float.MaxValue;
            m_HighestRTT = float.MinValue;
        }

        /// <summary>
        /// Send a message to client (only support byte array)
        /// </summary>
        /// <see cref="MudMessage.Create"/>
        /// <param name="message">mud message</param>
        public void Send(MudMessage message) => Send(message, false);

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
            m_PingClock += deltaTime;

            if ( m_RTTUpdate )
            {
                m_RTTWatch += deltaTime;
            }

            if (m_PingClock >= CLIENT_PING )
            {
                m_PingClock = 0f;
                m_Socket.Send(MudMessage.Create((int)MudOperation.Ping, null));
                m_RTTUpdate = true;
                m_RTTWatch = 0f;
            }
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
                    //Console.Message($"Client {ToString()}: Ping");
                    if ( m_RTTUpdate )
                    {
                        m_RTTMeasures.Add(m_RTTWatch);
                        m_RTTUpdate = false;
                    }
                    UpdateRTT();
                    //m_Socket.Send(MudMessage.Create(MudOperation.Ping, null));
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

        private void UpdateRTT()
        {
            if ( m_RTTMeasures.Count > 0 )
            {
                float sum = 0f;
                for (int i = 0; i < m_RTTMeasures.Count; ++i)
                {
                    sum += m_RTTMeasures[i];
                }

                float newRTT = Math.Max(m_MinimumRTT, sum / m_RTTMeasures.Count);
                RTT = newRTT;

                if (newRTT < m_LowestRTT)
                {
                    m_LowestRTT = newRTT;
                }
                if (newRTT > m_HighestRTT)
                {
                    m_HighestRTT = newRTT;
                }
            }
        }
    }
}
