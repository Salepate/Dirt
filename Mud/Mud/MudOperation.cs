﻿namespace Mud
{
    public enum MudOperation
    {
        Ping             = 0,
        ClientAuth       = 1,
        ValidateAuth     = 2,
        Disconnect       = 3,
        MultiPackets     = 4,
        CustomOperation0 = 20,
        Error           = 255
    }
}
