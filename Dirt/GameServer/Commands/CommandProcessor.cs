using Dirt.Game;
using Dirt.GameServer.Managers;
using Dirt.GameServer.PlayerStore;
using Dirt.Log;
using Dirt.Network;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;

namespace Dirt.GameServer.GameCommand
{
    public class CommandProcessor : IGameManager
    {
        private Dictionary<string, CommandData> m_Commands;
        private GameInstance m_Game;
        private PlayerStoreManager m_Store;
        private PlayerManager m_Players;

        // cache
        private object[] m_CallArgs;
        public CommandProcessor(GameInstance game)
        {
            game.GetManager<WebService>().HandleRoute("command", OnCommandRequest);
            m_Store = game.GetManager<PlayerStoreManager>();
            m_Players = game.GetManager<PlayerManager>();

            m_Commands = new Dictionary<string, CommandData>();
            m_Game = game;
            m_CallArgs = new object[2];
        }

        public void Update(float deltaTime)
        {
        }

        private string OnCommandRequest(HttpListenerRequest req)
        {
            string cmdName = req.Headers.Get(ClientCommandProcessor.CommandNameParameter);
            string sessIDStr = req.Headers.Get(ClientCommandProcessor.SessionIDParameter);
            PlayerProxy playerProxy;
            int sessID;
            int playerNumber;

            if (string.IsNullOrEmpty(sessIDStr) || 
                !int.TryParse(sessIDStr, out sessID) || 
                !m_Store.Table.TryGetPlayerNumber(sessID, out playerNumber))
            {
                return StoreResponse.RespondWithError(PlayerStoreError.InvalidSession, "Invalid session number");
            }

            if ((playerProxy = m_Players.FindPlayer(playerNumber)) == null)
            {
                return StoreResponse.RespondWithError(PlayerStoreError.InvalidSession, "Unknown Player");
            }

            if (cmdName == null)
            {
                return StoreResponse.RespondWithError(PlayerStoreError.InvalidParameters, "Missing command parameter");
            }

            if ( m_Commands.TryGetValue(cmdName, out CommandData cmdData))
            {
                CommandParameters cmdParams = new CommandParameters();

                bool hasError = false;

                MethodInfo method = cmdData.CommandMethod;
                int pCount = cmdData.Attribute.Parameters;
                for(int i = 0; i < pCount; ++i)
                {
                    string paramStr = req.Headers.Get($"param{i + 1}");
                    if ( paramStr == null )
                    {
                        hasError = true;
                        break;
                    }
                    cmdParams.Push(paramStr);
                }

                if (hasError)
                {
                    return StoreResponse.RespondWithError(PlayerStoreError.InvalidParameters, "Invalid command parameters");
                }
                else
                {
                    m_CallArgs[0] = CommandContext.Create(m_Game, playerProxy);
                    m_CallArgs[1] = cmdParams;
                    object res = method.Invoke(null, m_CallArgs);
                    if (cmdData.Attribute.IsPost)
                        return ((bool)res == false ? 0 : 1).ToString();
                    else
                        return JsonConvert.SerializeObject(res);
                }
            }
            else
            {
                return StoreResponse.RespondWithError(PlayerStoreError.InvalidCommand, "Command unknown");
            }
        }

        public void RegisterClassCommands<T>()
        {
            IEnumerable<MethodInfo> availableComms = CollectCommands<T, GameCommandAttribute>();
            foreach(MethodInfo cmd in availableComms)
            {
                GameCommandAttribute cmdAttr = cmd.GetCustomAttribute<GameCommandAttribute>();
                CommandData data = new CommandData()
                {
                    Attribute = cmdAttr,
                    CommandMethod = cmd
                };

                if ( m_Commands.ContainsKey(cmdAttr.Name))
                {
                    Console.Error($"Command name duplicate {cmdAttr.Name}");
                    continue;
                }    
                m_Commands.Add(cmdAttr.Name, data);
            }
        }

        private static IEnumerable<MethodInfo> CollectCommands<Class, WithAttr>() where WithAttr : System.Attribute
        {
            System.Type classType = typeof(Class);
            MethodInfo[] availableCommands = classType.GetMethods(BindingFlags.Public | BindingFlags.Static);
            return availableCommands.Where(c => c.GetCustomAttribute<WithAttr>() != null);
        }
    }
}
