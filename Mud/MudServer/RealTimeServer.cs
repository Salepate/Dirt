using Mud.Server.Stream;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;

using Console = Dirt.Log.Console;
using GameClientOperation = System.Tuple<Mud.Server.GameClient, Mud.ClientOperation>;
using NetworkMessage = System.Tuple<Mud.MudAddress, Mud.MudMessage>;

namespace Mud.Server
{
    public class RealTimeServer
    {
        public Queue<GameClientOperation> ClientOperations;
        public StreamGroupManager StreamGroups;

        private MudServer m_Server;
        private GameClient[] m_Clients;
        private int m_ServerPort;
        private object m_QueueLock = new object();
        private Queue<NetworkMessage> m_MessageQueue;
        private UdpClient m_Socket;
        private Thread m_ReceiveThread;
        private IClientConsumer m_ClientConsumer;
        private float m_Clock;
        private const float PING_CYCLE = 120f;
        private const float PING_TIMEOUT = 30f;
        public const int SIO_UDP_CONNRESET = -1744830452;
        public void SetClientConsumer(IClientConsumer clientConsumer)
        {
            m_ClientConsumer = clientConsumer;
        }
        public RealTimeServer(int maxClient, int port)
        {
            ClientOperations = new Queue<GameClientOperation>();
            StreamGroups = new StreamGroupManager();

            m_Server = new MudServer(maxClient);
            m_Clients = new GameClient[maxClient];
            m_ServerPort = port;
            m_MessageQueue = new Queue<NetworkMessage>();
            m_ClientConsumer = null;
            m_Clock = 0f;
        }

        public GameClient GetClient(int number)
        {
            int idx = number - 1;

            if (idx < 0 || idx >= m_Clients.Length)
                return null;

            return m_Clients[idx];
        }

        public void Run()
        {
            Console.Message($"Mud Server starting on port {m_ServerPort}");
            m_Socket = new UdpClient(m_ServerPort);
            m_Socket.Client.IOControl(SIO_UDP_CONNRESET, new byte[] { 0, 0, 0, 0 }, null); // handle icmp
            // Start Receive Thread
            m_ReceiveThread = new Thread(new ThreadStart(ReceiveThread));
            m_ReceiveThread.IsBackground = true;
            m_ReceiveThread.Start();
        }

        public void Stop()
        {
            Console.Message("Server shutdown");

            if ( m_ReceiveThread.IsAlive )
            {
                m_ReceiveThread.Abort();
            }
        }

        public void ProcessMessages(float delta) {
            m_Clock += delta;
            if (m_Clock >= PING_CYCLE)
                m_Clock -= PING_CYCLE;
            //if (m_MessageQueue.Count <= 0)
            //    return;

            Queue<NetworkMessage> messageToProcess = new Queue<NetworkMessage>();

            if ( Monitor.TryEnter(m_QueueLock))
            {
                try
                {
                    while(m_MessageQueue.Count > 0)
                    {
                        messageToProcess.Enqueue(m_MessageQueue.Dequeue());
                    }
                }
                finally
                {
                    Monitor.Exit(m_QueueLock);
                }
            }

            while(messageToProcess.Count > 0 )
            {
                var netMsg = messageToProcess.Dequeue();
                MudAddress clientAddr = netMsg.Item1;
                MudMessage msg = netMsg.Item2;

                int clientIdx = m_Server.FindConnectedClientIndex(clientAddr);

                if (clientIdx == -1) // new client
                {
                    int newSlot = m_Server.FindFreeSlot();
                    if (newSlot >= 0 && m_Server.SetConnectedClient(newSlot, clientAddr))
                    {
                        ServerSocket clientSocket = new ServerSocket(m_Socket, clientAddr);
                        m_Clients[newSlot] = new GameClient(clientAddr, clientSocket, newSlot + 1);
                        clientIdx = newSlot;
                        Console.Message($"Client connected: {m_Clients[newSlot]}");
                    }
                    else
                    {
                        Console.Warning($"Client Rejected {clientAddr.IP} (no more slots)");
                    }
                }

                if (clientIdx != -1)
                {
                    m_Clients[clientIdx].LastPing = m_Clock;

                    ClientOperation clientEvent = m_Clients[clientIdx].OnMessage((MudOperation)msg.opCode, msg.buffer);
                    if ( clientEvent == ClientOperation.Connect || clientEvent == ClientOperation.Disconnect )
                    {
                        ClientOperations.Enqueue(Tuple.Create(m_Clients[clientIdx], clientEvent));
                    }

                    if ( clientEvent == ClientOperation.PassThrough)
                    {
                        m_ClientConsumer.ProcessMessage(m_Clients[clientIdx], msg);
                    }

                    if (m_Clients[clientIdx].RequestDisconnection)
                    {
                        m_Server.FreeSlot(clientIdx);
                        m_Clients[clientIdx] = null;
                    }
                }
            }

            for (int i = 0; i < m_Clients.Length; ++i)
            {
                if (m_Clients[i] == null)
                    continue;

                GameClient client = m_Clients[i];
                float dt;

                if (client.LastPing > m_Clock)
                {
                    dt = (PING_CYCLE - client.LastPing) + m_Clock;
                }
                else
                {
                    dt = m_Clock - client.LastPing;
                }

                if (dt >= PING_TIMEOUT) // timedout
                {
                    Console.Message($"Client {m_Clients[i].ID} timed out ({PING_TIMEOUT}s)");
                    m_Clients[i].ForceDisconnect();
                    m_Server.FreeSlot(i);
                    ClientOperations.Enqueue(Tuple.Create(m_Clients[i], ClientOperation.Disconnect));
                    m_Clients[i] = null;
                }
            }

            if (ClientOperations.Count > 0 && m_ClientConsumer != null)
            {
                do
                {
                    var op = ClientOperations.Dequeue();
                    m_ClientConsumer.ProcessEvent(op.Item1, op.Item2);

                } while (ClientOperations.Count > 0);
            }
        }

   
        private void ReceiveThread()
        {
            byte[] messageBuffer;

            try
            {
                while (true)
                {
                    IPEndPoint endPoint = new IPEndPoint(IPAddress.Any, 0);
                    messageBuffer = m_Socket.Receive(ref endPoint);

                    if (!TryGetAddress(endPoint, out MudAddress clientAddr))
                    {
                        continue;
                    }

                    MudMessage msg = MudMessage.FromRaw(messageBuffer, messageBuffer.Length);

                    lock (m_QueueLock)
                    {
                        m_MessageQueue.Enqueue(Tuple.Create(clientAddr, msg));
                    }
                }
            }
            catch (Exception e)
            {
                Console.Error($"{e.ToString()}");
            }
            Console.Message($"Socket Closed");
        }

        private bool TryGetAddress(EndPoint ep, out MudAddress address)
        {
            address = default;
            if (ep.AddressFamily == AddressFamily.InterNetwork)
            {
                IPEndPoint ipEp = (IPEndPoint)ep;
                address.IP = ipEp.Address.ToString();
                address.Port = ipEp.Port;
                return true;
            }
            return false;
        }
    }
}
