using System;
using System.Collections.Generic;
using System.Text;

namespace Mud
{
    public class MudLargeMessage
    {
        private int m_MultipacketOperation;
        private int m_MultipacketExpected;
        private int m_MultipacketReceived;
        private List<byte> m_MultipacketBuffer;

        public bool Complete { get { return m_MultipacketReceived == m_MultipacketExpected; } }

        public MudLargeMessage(int operationCode, int expectedPackets)
        {
            m_MultipacketOperation = operationCode;
            m_MultipacketExpected = expectedPackets;
            m_MultipacketReceived = 0;
            m_MultipacketBuffer = new List<byte>(MudSocket.BufferSize * expectedPackets);
        }

        public void AddPacket(byte[] buffer)
        {
            m_MultipacketBuffer.AddRange(buffer);
            ++m_MultipacketReceived;
        }

        public MudMessage ToMessage()
        {
            return MudMessage.Create(m_MultipacketOperation, m_MultipacketBuffer.ToArray());
        }
    }
}
