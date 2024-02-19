using Dirt.Game.Math;
using System.Runtime.CompilerServices;

namespace Dirt.Network
{
    public class NetworkStream
    {
        public int Position { get; set; }

        public byte[] Buffer { get; private set; }


        public NetworkStream()
        {

        }

        public NetworkStream(byte[] buffer)
        {
            Buffer = buffer;
        }

        public void Allocate(int maximumSize)
        {
            Buffer = new byte[maximumSize];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write(byte[] buffer, int start, int size)
        {
            System.Buffer.BlockCopy(buffer, start, Buffer, Position, size);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write(byte[] buffer)
        {
            System.Buffer.BlockCopy(buffer, 0, Buffer, Position, buffer.Length);
        }

        public void Write(byte value)
        {
            Buffer[Position] = value;
            Position++;
        }
        // do the same for int, float, float3, float2
        // using bitwise operators instead of BitConverter
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write(int value)
        {
            Buffer[Position] = (byte)value;
            Buffer[Position + 1] = (byte)(value >> 8);
            Buffer[Position + 2] = (byte)(value >> 16);
            Buffer[Position + 3] = (byte)(value >> 24);
            Position += 4;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write(float value)
        {
            byte[] bytes = System.BitConverter.GetBytes(value);
            Buffer[Position] = bytes[0];
            Buffer[Position+1] = bytes[1];
            Buffer[Position+2] = bytes[2];
            Buffer[Position+3] = bytes[3];
            Position += 4;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write(bool value)
        {
            Buffer[Position] = (byte) (value ? 1 : 0);
            Position += 1;
        }

        // read
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int ReadByte()
        {
            Position += 1;
            return Buffer[Position - 1];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int ReadInt()
        {
            Position += 4;
            return (Buffer[Position - 4] | Buffer[Position - 3] << 8 | Buffer[Position - 2] << 16 | Buffer[Position - 1] << 24);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float ReadFloat()
        {
            Position += 4;
            return System.BitConverter.ToSingle(Buffer, Position - 4);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool ReadBool()
        {
            Position += 1;
            return Buffer[Position - 1] == 1;
        }

        // complex/math
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write(float3 value)
        {
            Write(value.x);
            Write(value.y);
            Write(value.z);
        }

        public void Write(float2 value)
        {
            Write(value.x);
            Write(value.y);
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float3 ReadFloat3()
        {
            Position += 12;
            return new float3(
                System.BitConverter.ToSingle(Buffer, Position - 12),
                System.BitConverter.ToSingle(Buffer, Position - 8),
                System.BitConverter.ToSingle(Buffer, Position - 4));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float2 ReadFloat2()
        {
            Position += 8;
            return new float2(
                System.BitConverter.ToSingle(Buffer, Position - 8),
                System.BitConverter.ToSingle(Buffer, Position - 4));
        }
    }
}
