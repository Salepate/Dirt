namespace Mud
{
    //@TODO Enum operations should be limited to Connect, Disconnect
    //Idle/Passthrough are internal operations
    public enum ClientOperation
    {
        Idle,
        Connect,
        Disconnect,
        PassThrough
    }
}