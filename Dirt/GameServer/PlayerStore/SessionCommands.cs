﻿using Dirt.Game.Model;
using Dirt.GameServer.GameCommand;

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

        [GameCommand("RegisterPlayer", 2, true)]
        public static bool RegisterPlayer(CommandContext ctx, CommandParameters parameters)
        {
            PlayerStoreManager storeMgr = ctx.Instance.GetManager<PlayerStoreManager>();
            string userName = parameters.PopString();
            string userPass = parameters.PopString();
            uint id;
            if (storeMgr.TryGetFreeID(userName, out id))
            {
                return storeMgr.RegisterUser(userName, storeMgr.GetHash(userPass), id);
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
            return pStore.AttemptUserAuth(ctx.PlayerNumber, playerTag, hashedPass);
        }
    }
}