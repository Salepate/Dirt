using System;

namespace Mud
{
    public abstract class MudSocket
    {
        public const int BufferSize = 1024;
        private byte[] m_Buffer;


        public MudSocket()
        {
            m_Buffer = new byte[BufferSize+1];
        }

        public void Send(MudMessage message, bool reliable = false)
        {
            m_Buffer[0] = (byte)message.opCode;
            int messageLength = 1;
            if (message.buffer != null && message.buffer.Length > 0)
            {
                Array.Copy(message.buffer, 0, m_Buffer, 1, message.buffer.Length);
                messageLength += message.buffer.Length;
            }

            if ( !reliable )
            {
                SendNetworkMessage(m_Buffer, messageLength);
            }
            else
            {
                SendNetworkMessageReliable(m_Buffer, messageLength);
            }
        }

        protected abstract void SendNetworkMessage(byte[] message, int messageLength);

        protected virtual void SendNetworkMessageReliable(byte[] message, int messageLength) { SendNetworkMessage(message, messageLength);  }
    }
}
