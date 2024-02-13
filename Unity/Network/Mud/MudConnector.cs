using Dirt;
using Dirt.Log;
using Dirt.Systems;
using Mud.Framework;
using Mud.Managers;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;

namespace Mud.DirtSystems
{
    public class MudConnector : DirtSystem
    {
        public const int ReliableMessageBuffer = 20;

        public System.Action<bool> AuthAction;
        public System.Action DisconnectAction;
        public const int DefaultPort = 11000;
        public ServerSocket Socket { get; private set; }
        public override bool HasUpdate => true;
        public int PingMS { get; private set; }
        public bool Connected { get; private set; }
        public int PlayerNumber { get; private set; }
        private Queue<MudMessage> m_Messages;
        private MudLargeMessage m_LargeMessage;
        private List<IMessageConsumer> m_Consumers;
        private bool m_Authed;

        private CircularBuffer<byte> m_ReliableBuffer;

        public string PlayerName { get; private set; }
        public void SetConsumer(IMessageConsumer consumer)
        {
            m_Consumers.Add(consumer);
        }

        public override void Initialize(DirtMode mode)
        {
            m_Consumers = new List<IMessageConsumer>(2);
            m_Messages = new Queue<MudMessage>();
            m_ReliableBuffer = new CircularBuffer<byte>(ReliableMessageBuffer);

            if ( mode.HasSystem<SimulationSystem>())
            {
                mode.FindSystem<SimulationSystem>().RegisterManager(new ServerProxy(this));
            }
            Connected = false;
        }

        public void StartSocket(string address, int port = DefaultPort, string userName = null)
        {
            if (!m_Authed)
            {
                Socket = new ServerSocket(address, port);
                PlayerName = userName;
                Socket.Send(MudMessage.Create(MudOperation.ClientAuth, null));
                m_Authed = true;
            }
        }
        private void TerminateSocket()
        {
            Connected = false;
            m_Authed = false;
            Socket = null;
            DisconnectAction?.Invoke();
        }

        public override void Unload()
        {
            if (Socket != null)
            {
                Socket.Send(MudMessage.Create(MudOperation.Disconnect, null));
            }
        }

        public override void Update()
        {
            if ( m_Authed )
            {
                ReceiveMessages();
                if ( !m_Authed )
                {
                    DisconnectAction?.Invoke();
                }
                if (m_Messages.Count > 0)
                {
                    while (m_Messages.Count > 0)
                    {
                        bool process = true;

                        MudMessage msg = m_Messages.Dequeue();

                        if ( msg.reliable )
                        {
                            process = m_ReliableBuffer.IndexOf(msg.reliableId) == -1;
                            if ( process )
                            {
                                m_ReliableBuffer.Add(msg.reliableId);
                            }
                            Socket.Confirm(msg.reliableId);
                        }

                        if ( process )
                        {
                            ProcessMessage(msg.opCode, msg.buffer);
                        }
                    }
                }
            }
        }

        private void SendPong()
        {
            Socket.Send(MudMessage.Create(MudOperation.Ping, null));
        }

        private void ProcessMessage(int operation, byte[] buffer)
        {
            switch ((MudOperation)operation)
            {
                case MudOperation.Ping:
                    if ( buffer != null)
                    {
                        PingMS = buffer[0];
                    }
                    SendPong();
                    break;
                case MudOperation.ClientAuth:
                    break;
                case MudOperation.ValidateAuth:
                    //RecordValidHost(m_Login.HostText.text);
                    int id = (int)buffer[0];
                    PlayerNumber = id;
                    Connected = true;
                    for (int i = 0; i < m_Consumers.Count; ++i)
                        m_Consumers[i].OnLocalNumber(id);
                    AuthAction?.Invoke(true);
                    break;
                case MudOperation.Disconnect:
                    m_Authed = false;
                    Connected = false;
                    TerminateSocket();
                    break;
                case MudOperation.MultiPackets:
                    if (m_LargeMessage == null || m_LargeMessage.Complete)
                    {
                        m_LargeMessage = new MudLargeMessage(buffer[0], buffer[1]);
                    }
                    else
                    {
                        m_LargeMessage.AddPacket(buffer);
                        if (m_LargeMessage.Complete)
                        {
                            m_Messages.Enqueue(m_LargeMessage.ToMessage());
                        }
                    }
                    break;
                case MudOperation.Error:
                    if (m_Authed)
                    {
                        m_Authed = false;
                        DisconnectAction?.Invoke();
                    }
                    Console.Error(Encoding.ASCII.GetString(buffer));
                    break;
                default:
                    for(int i = 0; i < m_Consumers.Count; ++i)
                    {
                        if (m_Consumers[i].OnCustomMessage((byte)operation, buffer))
                            break;
                    }
                    break;
            }
        }

        private void ReceiveMessages()
        {
            Socket.Timeout = 5000;
            try
            {
                while (Socket.HasData)
                {
                    byte[] data = Socket.Receive();
                    m_Messages.Enqueue(MudMessage.FromRaw(data, data.Length));
                }
            }
            catch (SocketException socket)
            {
                Console.Warning($"Unable to reach host");
                Console.Warning(socket.Message);
                m_Messages.Clear();
                m_Authed = false;
            }
        }
    }
}