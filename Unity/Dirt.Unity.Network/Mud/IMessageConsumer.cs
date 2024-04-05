namespace Mud.Framework
{
    public interface IMessageConsumer
    {
        // Return true if message was processed
        bool OnCustomMessage(byte opCode, byte[] buffer);
        // On player local number assigned
        void OnLocalNumber(int number);
    }
}