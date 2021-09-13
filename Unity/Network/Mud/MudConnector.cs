using Dirt;
using Dirt.Log;
using Dirt.Systems;
using Mud.Framework;
using Mud.Managers;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;

namespace Mud.DirtSystems
{
    public class MudConnector : DirtSystem
    {
        public System.Action<bool> AuthAction;
        public const int DefaultPort = 11000;
        public ServerSocket Socket { get; private set; }
        public override bool HasUpdate => true;

        public bool Connected { get; private set; }

        public int PlayerNumber { get; private set; }
        private Queue<MudMessage> m_Messages;
        private object _QueueLock = new object();
        private MudLargeMessage m_LargeMessage;
        private IMessageConsumer m_MessageConsumer;
        private bool m_Authed;
        private Thread m_SocketThread;

        public string PlayerName { get; private set; }
        public void SetConsumer(IMessageConsumer consumer)
        {
            m_MessageConsumer = consumer;
        }

        public override void Initialize(DirtMode mode)
        {
            m_Messages = new Queue<MudMessage>();

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
                m_SocketThread = new Thread(new ThreadStart(ReceiveThread));
                m_SocketThread.IsBackground = true;
                m_SocketThread.Start();

                if (string.IsNullOrEmpty(userName) || userName.Length < 3)
                {
                    userName = $"User{Random.Range(10000, 99999)}";
                }

                PlayerName = userName;

                Socket.Send(MudMessage.Create(MudOperation.ClientAuth, Encoding.ASCII.GetBytes(userName)));
                m_Authed = true;
            }
        }
        private void TerminateSocket()
        {
            m_SocketThread.Abort();
            Socket = null;
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
            if (m_SocketThread != null)
            {
                if (!m_SocketThread.IsAlive)
                {
                    m_SocketThread = null;
                }

            }
            if (m_Messages.Count > 0)
            {
                while (m_Messages.Count > 0)
                {
                    MudMessage msg = m_Messages.Dequeue();
                    ProcessMessage(msg.opCode, msg.buffer);
                }
            }
        }

        private void ProcessMessage(int operation, byte[] buffer)
        {
            switch ((MudOperation)operation)
            {
                case MudOperation.Ping:
                    break;
                case MudOperation.ClientAuth:
                    break;
                case MudOperation.ValidateAuth:
                    //RecordValidHost(m_Login.HostText.text);
                    int id = (int)buffer[0];
                    PlayerNumber = id;
                    Connected = true;
                    m_MessageConsumer?.OnLocalNumber(id);
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
                    Console.Error(Encoding.ASCII.GetString(buffer));
                    break;
                default:
                    m_MessageConsumer?.OnCustomMessage((byte)operation, buffer);
                    break;
            }
        }

        private void ReceiveThread()
        {
            Socket.Timeout = 5000;
            bool connected = false;
            try
            {
                while (true)
                {
                    byte[] data = Socket.Receive();

                    if (!connected)
                    {
                        Socket.Timeout = 0;
                        connected = true;
                    }

                    lock (_QueueLock)
                    {
                        m_Messages.Enqueue(MudMessage.FromRaw(data, data.Length));
                    }
                }
            }
            catch (SocketException socket)
            {
                Console.Warning($"Unable to reach host");
                Console.Warning(socket.Message);
            }

            m_Authed = false;
        }
    }
}