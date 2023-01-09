using System;
using System.Collections.Generic;
using System.Text;

namespace Mud
{
    public struct MudMessage
    {
        public int opCode; // < 255
        public byte[] buffer;
        public bool reliable;
        public byte reliableId;

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
            int message_size = count - 1;
            int start_index = 0;
            bool is_reliable = false;
            byte reliable_number = 0;
            if ( (MudOperation) raw[0] == MudOperation.Reliable )
            {
                message_size -= 2; // remove bytes
                start_index = 2;
                is_reliable = true;
                reliable_number = raw[1];
            }
            byte[] buffer = new byte[message_size];
            Array.Copy(raw, start_index + 1, buffer, 0, message_size);
            return new MudMessage()
            {
                opCode = raw[start_index],
                buffer = buffer,
                reliable = is_reliable,
                reliableId = reliable_number
            };
        }
    }
}
