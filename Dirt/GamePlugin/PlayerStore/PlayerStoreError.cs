namespace Dirt.GameServer.PlayerStore
{
    public static class PlayerStoreError
    {
        // Commands
        public const int InvalidCommand         = 0x0e0;
        public const int InvalidParameters      = 0x0e1;
        public const int InvalidSession         = 0x0e2;
        // Auth
        public const int InvalidCredentials     = 0x0f0;
        // Register
        public const int UsernameTooSmall       = 0x100;
        public const int UsernameTooLarge       = 0x101;
        public const int UsernameNotAvailable   = 0x102;
        public const int PasswordTooSmall       = 0x103;
        public const int UsernameMissing        = 0x104;
        public const int PasswordMissing        = 0x105;
        public const int MissingData            = 0x106;
        public const int UnknownPlayer          = 0x107;
    }
}
