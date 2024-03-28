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

        [GameCommand("RegisterPlayer", 3, true)]
        public static bool RegisterPlayer(CommandContext ctx, CommandParameters parameters)
        {
            PlayerStoreManager storeMgr = ctx.Instance.GetManager<PlayerStoreManager>();
            string userName = parameters.PopString();
            string userPass = parameters.PopString();
            string userCode = parameters.PopString();

            uint id;
            if (storeMgr.TryGetFreeID(userName, out id) && (!storeMgr.UseRegistrationCode || storeMgr.VerifyCode(userCode)))
            {
                bool valid = storeMgr.RegisterUser(userName, storeMgr.GetHash(userPass), id);
                if (valid)
                {
                    if (storeMgr.UseRegistrationCode)
                        storeMgr.ConsumeCode(userCode);
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

            bool validUser = pStore.TryGetUniqueID(playerTag, out uint id);

            if (!validUser)
                return false;

            bool isLoggedIn = pStore.Table.TryGetPlayerNumber(id, out int previousPlayer);
            bool auth = (!isLoggedIn || pStore.AllowPlayerReconnect) && pStore.DryAuth(ctx.PlayerNumber, playerTag, hashedPass);
            if ( auth )
            {
                if ( isLoggedIn )
                {
                    // kick previous owner
                    PlayerProxy oldPlayer = ctx.Instance.GetManager<PlayerManager>().FindPlayer(previousPlayer);
                    ctx.Instance.UnregisterPlayer(oldPlayer.Client);
                    oldPlayer.Client.ForceDisconnect();
                }

                if (pStore.AttemptUserAuth(ctx.PlayerNumber, playerTag, hashedPass) )
                {
                    ctx.Player.Player.Name = ctx.Player.Client.ID;
                    PlayerManager playerMgr = ctx.Instance.GetManager<PlayerManager>();
                    playerMgr.SendEvent(new PlayerRenameEvent(ctx.PlayerNumber, ctx.Player.Player.Name));
                }
                else
                {
                    auth = false;
                }
            }

            return auth;
        }
    }
}
