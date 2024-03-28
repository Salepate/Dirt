namespace Dirt.GameServer.GameCommand
{
    public struct CommandContext
    {
        public GameInstance Instance { get; private set; }
        public PlayerProxy Player { get; private set; }

        public int PlayerNumber => Player.Client.Number;

        public static CommandContext Create(GameInstance game, PlayerProxy player) =>
            new CommandContext()
            {
                Instance = game,
                Player = player
            };
    }
}
