namespace Mud.Framework
{
    public interface IMessageConsumer
    {
        void OnCustomMessage(byte opCode, byte[] buffer);
        void OnLocalNumber(int number);
    }
}