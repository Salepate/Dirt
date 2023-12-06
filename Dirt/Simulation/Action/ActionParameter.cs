using System;
using System.Runtime.InteropServices;

namespace Dirt.Simulation.Action
{

    [StructLayout(LayoutKind.Explicit)]
    public struct ActionParameter
    {
        [FieldOffset(0)]
        public float floatValue;
        [FieldOffset(0)]
        public int intValue;


        public static implicit operator ActionParameter(int value) { return new ActionParameter() { intValue = value }; }
        public static implicit operator ActionParameter(float value) { return new ActionParameter() { floatValue = value }; }

        public static ActionParameter FromBytes(byte[] buffer, int start)
        {
            return new ActionParameter()
            {
                intValue = BitConverter.ToInt32(buffer, start)
            };
        }

        public static ActionParameter Create(float value)
        {
            return new ActionParameter()
            {
                floatValue = value
            };
        }

        public static ActionParameter Create(int value)
        {
            return new ActionParameter()
            {
                intValue = value
            };
        }
    }
}
