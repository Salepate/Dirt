using Dirt.Log;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;

namespace Mud
{
    public class ClientSocket : MudSocket
    {
        private UdpClient m_Socket;
        private IPEndPoint m_ClientEndPoint;

        private MemoryStream m_MessageCopy;
        private BinaryWriter m_Writer;

        private List<MessageReference> m_ReliableMessages;
        private readonly byte m_AckRotation;
        private byte m_CurrentAck;
        public ClientSocket(UdpClient socket, MudAddress clientAddress, byte ackRotation)
        {
            m_Socket = socket;
            m_ClientEndPoint = new IPEndPoint(IPAddress.Parse(clientAddress.IP), clientAddress.Port);
            m_MessageCopy = new MemoryStream();
            m_Writer = new BinaryWriter(m_MessageCopy);
            m_CurrentAck = 0;
            m_AckRotation = ackRotation;

            m_ReliableMessages = new List<MessageReference>();
        }

        protected override void SendNetworkMessage(byte[] message, int messageLength)
        {
            m_Socket.Send(message, messageLength, m_ClientEndPoint);
        }

        protected override void SendNetworkMessageReliable(byte[] message, int messageLength)
        {
            ClearStream();
            m_Writer.Write((byte)MudOperation.Reliable);
            m_Writer.Write(m_CurrentAck);
            m_Writer.Write(message, 0, messageLength);
            byte[] reliable_message = m_MessageCopy.ToArray();

            m_ReliableMessages.Add(MessageReference.Create(reliable_message, m_CurrentAck));

            if (m_CurrentAck == m_AckRotation)
            {
                m_CurrentAck = 0;
            }
            else
            { 
                m_CurrentAck++;
            }
            m_Socket.Send(reliable_message, reliable_message.Length, m_ClientEndPoint);
        }

        internal void SendReliables(float deltaTime, float rtt)
        {
            int retries = 0;
            for(int i = 0; i < m_ReliableMessages.Count; ++i)
            {
                MessageReference message = m_ReliableMessages[i];
                message.Clock += deltaTime;
                if ( !message.Received && message.Clock > rtt)
                {
                    message.Clock -= rtt;
                    m_Socket.Send(message.Message, message.Message.Length, m_ClientEndPoint);
                    retries++;
                }
                if (message.Message[1] != message.AckID )
                {
                    Console.Warning($"Ack Inconsistency: {message.Message[1]} != {message.AckID}");
                }
                m_ReliableMessages[i] = message;
            }

            if (retries > 0 )
            {
                Console.Warning($"Resending {retries} messages");
            }
        }

        internal void ConfirmReliableMessage(byte ackID)
        {
            for(int i = 0; i < m_ReliableMessages.Count; ++i)
            {
                MessageReference msg = m_ReliableMessages[i];
                if (msg.AckID == ackID)
                {
                    Console.Message($"Client Confirm {ackID}");
                    msg.Received = true;
                    m_ReliableMessages[i] = msg;
                    break;
                }
            }

            for(int i = 0; i < m_ReliableMessages.Count;)
            {
                MessageReference msg = m_ReliableMessages[i];
                if ( !msg.Received )
                {
                    break;
                }
                m_ReliableMessages.RemoveAt(0);
            }
        }

        private void ClearStream()
        {
            System.Array buffer = m_MessageCopy.GetBuffer();
            System.Array.Clear(buffer, 0, buffer.Length);
            m_MessageCopy.Position = 0;
            m_MessageCopy.SetLength(0);
        }


        internal struct MessageReference
        {
            public byte AckID;
            public byte[] Message;
            public bool Received;
            public float Clock;

            public static MessageReference Create(byte[] message, byte ackId)
            {
                return new MessageReference()
                {
                    AckID = ackId,
                    Message = message,
                    Received = false,
                    Clock = 0f
                };
            }
        }
    }
}