namespace Mud.Framework
{
    public interface IMessageConsumer
    {
        // Return true if message was processed
        bool OnCustomMessage(byte opCode, byte[] buffer);
        void OnLocalNumber(int number);
    }
}