using Dirt.Game.Model;
using Dirt.GameServer.GameCommand;
using Dirt.GameServer.Managers;
using Dirt.Network.Events;

namespace Dirt.GameServer.PlayerStore
{
    public class SessionCommands 
    {
        private SessionCommands()
        {

        }

        [GameCommand(commandName: "GetSelf", paramCount: 0, postCommand: false)]
        public static GamePlayer GetSelf(CommandContext ctx, CommandParameters parameters)
        {
            return ctx.Player.Player;
        }

        [GameCommand(commandName: "GetTagNumber", paramCount: 0, postCommand: false)]
        public static uint GetTagNumber(CommandContext ctx, CommandParameters parameters)
        {
            PlayerStoreManager storeMgr = ctx.Instance.GetManager<PlayerStoreManager>();
            if ( storeMgr.TryGetPlayerTagNumber(ctx.PlayerNumber, out uint tagNumber))
            {
                return tagNumber;
            }
            return 0;
        }

        [GameCommand("RegisterPlayer", 2, true)]
        public static bool RegisterPlayer(CommandContext ctx, CommandParameters parameters)
        {
            PlayerStoreManager storeMgr = ctx.Instance.GetManager<PlayerStoreManager>();
            string userName = parameters.PopString();
            string userPass = parameters.PopString();
            uint id;
            if (storeMgr.TryGetFreeID(userName, out id))
            {
                bool valid = storeMgr.RegisterUser(userName, storeMgr.GetHash(userPass), id);
                if ( valid )
                {
                    string playerTag = $"{userName}#{id}";
                    return storeMgr.AttemptUserAuth(ctx.PlayerNumber, playerTag, storeMgr.GetHash(userPass));
                }
            }
            return false;
        }

        [GameCommand("AuthPlayer", 2, true)]
        public static bool AuthPlayer(CommandContext ctx, CommandParameters parameters)
        {
            string playerTag = parameters.PopString();
            string playerPass = parameters.PopString();
            PlayerStoreManager pStore = ctx.Instance.GetManager<PlayerStoreManager>();
            var hashedPass = pStore.GetHash(playerPass);
            bool auth = pStore.AttemptUserAuth(ctx.PlayerNumber, playerTag, hashedPass);

            if ( auth )
            {
                ctx.Player.Player.Name = ctx.Player.Client.ID;
                PlayerManager playerMgr = ctx.Instance.GetManager<PlayerManager>();
                playerMgr.SendEvent(new PlayerRenameEvent(ctx.PlayerNumber, ctx.Player.Player.Name));
            }

            return auth;
        }
    }
}
