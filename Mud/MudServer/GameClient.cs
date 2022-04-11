using System;
using System.Collections.Generic;
using System.Text;

using Console = Dirt.Log.Console;

namespace Mud.Server
{
    public class GameClient
    {
        public string ID { get; private set; }
        public int Number { get; private set; }
        public bool RequestDisconnection { get; private set; }

        public float LastPing { get; set; }

        private ServerSocket m_Socket;
        private bool m_Auth;
        private MudAddress m_Address;
        // large packet progress
        private int m_MultipacketOperation;
        private int m_MultipacketExpected;
        private int m_MultipacketReceived;
        private List<byte> m_MultipacketBuffer;

        private Random m_AnonID;

        public GameClient(MudAddress clientAddress, ServerSocket socket, int number)
        {
            m_Auth = false;
            m_Address = clientAddress;
            m_Socket = socket;
            Number = number;
            RequestDisconnection = false;
            m_AnonID = new Random();
        }

        public void Send(MudMessage message)
        {
            if (message.buffer.Length <= MudSocket.BufferSize)
                m_Socket.Send(message);
            else
                SendLargeMessage(message.opCode, message.buffer);
        }

        public void ForceDisconnect()
        {
            RequestDisconnection = true;
        }

        public void ChangeClientName(string newName)
        {
            Console.Message($"Client {Number} name changed to {newName}");
            ID = newName;
        }

        public void SendLargeMessage(int op, byte[] message)
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
                    Console.Message($"Client {ToString()}: Ping");
                    m_Socket.Send(MudMessage.Create(MudOperation.Ping, null));
                    return ClientOperation.Idle;
                case MudOperation.ClientAuth:
                    if ( !m_Auth )
                    {
                        // string userAuth = Encoding.ASCII.GetString(buffer);
                        int ranId = m_AnonID.Next(1, 9999);
                        string userAuth = $"player{ranId.ToString("D4")}";
                        ID = userAuth;
                        m_Auth = true;
                        Console.Message($"Client {ToString()}: Authed as {ID} (Player {Number})");
                        m_Socket.Send(MudMessage.Create(MudOperation.ValidateAuth, new byte[] { (byte) Number }));
                        return ClientOperation.Connect;

                        //if ( !string.IsNullOrEmpty(userAuth) && userAuth.Length > 3 )
                        //{
                        //} 
                        //else
                        //{
                        //    m_Socket.Send(MudMessage.Error($"Invalid Auth"));
                        //}
                    }
                    return ClientOperation.Idle;
                case MudOperation.MultiPackets:
                    throw new Exception("Client multipacket not supported");
                //case MudOperation.MultiPackets:
                //    {
                //        if (buffer.Length == 2 && m_MultipacketOperation == -1)
                //        {
                //            m_MultipacketOperation = buffer[0];
                //            m_MultipacketExpected = buffer[1];
                //            m_MultipacketBuffer = new List<byte>(MudSocket.BufferSize * m_MultipacketExpected);
                //            m_MultipacketReceived = 0;
                //        } 
                //        else if (m_MultipacketReceived < m_MultipacketExpected) {
                //            ++m_MultipacketReceived;
                //            m_MultipacketBuffer.AddRange(buffer);
                //            if ( m_MultipacketReceived == m_MultipacketExpected)
                //            {
                //                int multiOp = m_MultipacketOperation;
                //                Console.Message("Large Message Op: " + multiOp);
                //                m_MultipacketOperation = -1;
                //                OnMessage((MudOperation)multiOp, m_MultipacketBuffer.ToArray());
                //            }
                //        }
                //        return ClientEvent.Idle;
                //    }
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
                        //Console.Message($"Ignored Operation {op.ToString()}: Client is not authed ({m_Address.IP})");
                        m_Socket.Send(MudMessage.Error("Client is not authed"));
                    } else
                    {
                        return ClientOperation.PassThrough; // custom message
                        //Console.Message($"Client {m_Address.IP} [{op.ToString()}]: {Encoding.ASCII.GetString(buffer)}");
                    }
                    return ClientOperation.Idle;
            }
        }
    }
}
