using System;
using System.Collections.Generic;
using System.Text;

namespace Mud
{
    public struct MudMessage
    {
        public int opCode; // < 255
        public byte[] buffer;

        public static MudMessage Create(int op, byte[] data)
        {
            return new MudMessage()
            {
                opCode = op,
                buffer = data
            };
        }

        public static MudMessage Create(MudOperation op, byte[] data)
        {
            return new MudMessage()
            {
                opCode = (int) op,
                buffer = data
            };
        }

        public static MudMessage Error(string message)
        {
            return new MudMessage()
            {
                opCode = (int)MudOperation.Error,
                buffer = Encoding.ASCII.GetBytes(message)
            };
        }

        public static MudMessage FromRaw(byte[] raw, int count)
        {
            byte[] buffer = new byte[count - 1];
            Array.Copy(raw, 1, buffer, 0, count - 1);
            return new MudMessage()
            {
                opCode = raw[0],
                buffer = buffer
            };
        }
    }
}
