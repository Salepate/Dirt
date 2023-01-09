namespace Mud
{
    public enum MudOperation
    {
        Ping             = 0,
        ClientAuth       = 1,
        ValidateAuth     = 2,
        Disconnect       = 3,
        MultiPackets     = 4,
        CustomOperation0 = 20,
        Reliable        = 253,
        ReliableConfirm = 254,
        Error           = 255
    }
}
