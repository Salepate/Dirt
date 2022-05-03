using Dirt.Game;
using Dirt.Game.Managers;
using Dirt.Game.Metrics;
using Dirt.Game.Model;
using Dirt.GameServer.GameCommand;
using Dirt.GameServer.Managers;
using Dirt.GameServer.PlayerStore;
using Dirt.Network;
using Dirt.Network.Events;
using Dirt.Network.Managers;
using Dirt.Network.Model;
using Dirt.Network.Simulation.Components;
using Dirt.Network.Simulation.Events;
using Dirt.Simulation;
using Dirt.Simulation.Actor;
using Dirt.Simulation.Builder;
using Dirt.Simulation.Context;
using Dirt.Simulation.Model;
using Dirt.Simulation.SystemHelper;
using Dirt.Simulation.Utility;
using Mud;
using Mud.Server;
using Mud.Server.Stream;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using Console = Dirt.Log.Console;

namespace Dirt.GameServer
{
    public class GameInstance : IClientConsumer, IManagerProvider
    {
        private const int DefaultSimulation = 0;
        private const string AssemblyCollection = "assemblies.server";
        private const string DefaultSimulationName = "lobby";
        private const string SettingsContentName = "settings.netserial";

        // Dirt
        private Dictionary<int, StreamGroup> m_Groups;
        private Dictionary<Type, IGameManager> m_Managers;
        public SimulationManager Simulations { get; private set; }
        private StreamGroupManager m_GroupManager;
        public ContentProvider Content { get; private set; }
        private SimulationBuilder m_SimBuilder;

        public AssemblyCollection ValidAssemblies;
        public List<IContextItem> m_SharedContexts;
        private PlayerManager m_Players;
        private PluginInstance m_Plugin;

        public Action<GameClient, MudMessage> CustomMessage;
        public GameInstance(StreamGroupManager groupManager, string contentPath, string contentManifest, PluginInstance plugin)
        {
            Console.Assert(plugin != null, "No plugin specified");
            // Dirt
            string contentManifestPath = $"{contentPath}/{contentManifest}.json";
            Content = new ContentProvider(contentPath);
            Content.LoadGameContent(contentManifest);
            Simulations = new SimulationManager(Content);
            m_Managers = new Dictionary<Type, IGameManager>();
            m_GroupManager = groupManager;
            m_Groups = new Dictionary<int, StreamGroup>();
            m_SimBuilder = new SimulationBuilder();
            m_SharedContexts = new List<IContextItem>();
            NetworkTypes netAsses = Content.LoadContent<NetworkTypes>(SettingsContentName);
            ValidAssemblies = Content.LoadContent<AssemblyCollection>(AssemblyCollection);
            m_SharedContexts.Add(ValidAssemblies);
            //@hack: Load missing assemblies before serializer 
            AssemblyReflection.BuildTypeMap<ISimulationSystem>(ValidAssemblies.Assemblies);
            NetworkSerializer netSerializer = new NetworkSerializer(netAsses);
            m_Players = new PlayerManager(netSerializer);

            m_SimBuilder.LoadAssemblies(ValidAssemblies);
            m_Plugin = plugin;
            RegisterManager(new MetricsManager());
            RegisterManager(netSerializer);
            RegisterManager(m_Players);
            RegisterManager(Simulations);

            int webServicePort = int.Parse(ConfigurationManager.AppSettings["WebServerPort"]);
            RegisterManager(new WebService("127.0.0.1", webServicePort));
            RegisterManager(new PlayerStoreManager(this));
            RegisterManager(new CommandProcessor(this));

            GetManager<WebService>().Start();
            GetManager<CommandProcessor>().RegisterClassCommands<SessionCommands>();
            Console.Message($"{m_Plugin.PluginName} Started");
        }

        public void InitializePlugin()
        {
            m_Plugin.Initialize(m_SharedContexts, this);
        }

        public void UpdateInstance(float dt)
        {
            foreach(var kvp in m_Managers)
            {
                kvp.Value.Update(dt);
            }
        }

        public GamePlayer RegisterPlayer(GameClient client)
        {
            PlayerProxy proxy = m_Players.FindPlayer(client.Number);
            
            if (proxy == null)
            {
                proxy = m_Players.CreatePlayer(client);

                PlayerListEvent playerListEvent = new PlayerListEvent()
                {
                    Players = m_Players.ActivePlayers.ToArray()
                };

                m_Players.SendEvent<PlayerConnectionEvent>(new PlayerConnectionEvent() { Player = proxy.Player });
                m_Players.SendEventTo(proxy.Player, playerListEvent);

                int simID = DefaultSimulation;
                GameSimulation sim = Simulations.GetSimulation(simID);

                if (sim == null)
                {
                    simID = Simulations.CreateSimulation(m_Plugin.DefaultSimulation, SimulationSpan.Persistent);
                    sim = Simulations.GetSimulation(simID);
                }

                MovePlayerToSimulation(proxy, simID);
                m_Plugin.PlayerJoined(proxy);
            }
            return null;
        }

        public void MovePlayerToSimulation(PlayerProxy proxy, int simID)
        {
            // Ensure player can be moved
            GameSimulation newSim = Simulations.GetSimulation(simID);
            GameClient client = proxy.Client;

            if (newSim == null )
            {
                Console.Message($"Cannot move player to simulation {simID}: not found");
                return;
            }

            int oldSimIndex = proxy.Simulation;

            if ( oldSimIndex == simID )
            {
                Console.Warning($"Player already in simulation {simID}, skipping request.");
                return;

            }
            proxy.Simulation = simID;

            // Notify previous simulation
            if ( oldSimIndex != -1 )
            {
                GameSimulation oldSim = Simulations.GetSimulation(oldSimIndex);
                var playerLeaveEvent = new PlayerEvent(proxy.Player.Number, PlayerEvent.LeftSimulation);
                oldSim.Events.Enqueue(playerLeaveEvent);
                m_Groups[oldSimIndex].UnregisterClient(client);

                // destroy sim if empty
                if ( m_Groups[oldSimIndex].Clients.Count == 0 && Simulations.GetSpan(oldSimIndex) == SimulationSpan.Temporary )
                {
                    DestroySimulation(oldSimIndex);
                }
            }

            WarmSimulation(simID);
            m_Groups[simID].RegisterClient(client);
            client.Send(MudMessage.Create((int)NetworkOperation.LoadSimulation, System.Text.Encoding.ASCII.GetBytes(newSim.Archetype)));
            PlayerEvent playerEvent = new PlayerEvent(client.Number, PlayerEvent.JoinedSimulation);
            newSim.Events.Enqueue(playerEvent);
        }

        public void UnregisterPlayer(GameClient client)
        {
            PlayerProxy proxy = m_Players.FindPlayer(client.Number);

            if (proxy != null)
            {
                m_Plugin.PlayerLeft(proxy);
                GamePlayer player = proxy.Player;

                m_Players.RemovePlayer(player);
                m_Groups[proxy.Simulation].UnregisterClient(client);

                if ( m_Groups[proxy.Simulation].Clients.Count == 0 && Simulations.GetSpan(proxy.Simulation) == SimulationSpan.Temporary)
                {
                    DestroySimulation(proxy.Simulation);
                }
                else
                {
                    GameSimulation playerSim = Simulations.GetSimulation(proxy.Simulation);
                    var playerEvent = new PlayerEvent(client.Number, PlayerEvent.LeftSimulation);
                    playerSim.Events.Enqueue(playerEvent);
                }
            }
        }


        public void ProcessMessage(GameClient client, MudMessage message)
        {
            NetworkOperation mudOp = (NetworkOperation)message.opCode;
            switch (mudOp)
            {
                case NetworkOperation.ActorSync:
                    
                    PlayerProxy proxy = m_Players.FindPlayer(client.Number);
                    Console.Assert(proxy != null, $"Unknown Player {client.Number}");
                    GameSimulation sim = Simulations.GetSimulation(proxy.Simulation);

                    int syncID = message.buffer[0];
                    bool isOwner = sim.Filter.GetActorsMatching<NetInfo>(a => a.ID == syncID && proxy.Player.Number == a.Owner).Count == 1;
                    if (isOwner)
                    {
                        byte[] syncBuffer = new byte[message.buffer.Length - 1];
                        Array.Copy(message.buffer, 1, syncBuffer, 0, message.buffer.Length - 1);
                        sim.Events.Enqueue(new ActorSyncEvent(message.buffer[0], syncBuffer));
                    }
                    break;
                default:
                    CustomMessage?.Invoke(client, message);
                    break;
            }
        }

        public bool ProcessEvent(GameClient client, ClientOperation operationEvent)
        {
            switch (operationEvent)
            {
                case ClientOperation.Connect:
                    RegisterPlayer(client);
                    break;
                case ClientOperation.Disconnect:
                    UnregisterPlayer(client);
                    break;
                default:
                    break;
            }

            return true;
        }

        private void DestroySimulation(int simID)
        {
            Console.Message($"Destroying Sim {simID}");
            Simulations.DestroySimulation(simID);
            StreamGroup group = m_Groups[simID];
            m_Groups.Remove(simID);
            m_GroupManager.DestroyGroup(group);
        }
        public void WarmSimulation(int simID)
        {
            SimulationProxy simProxy = Simulations.GetSimulationProxy(simID);
            GameSimulation simulation = simProxy.Simulation;

            // Warm Up sim if not loaded
            if (!m_Groups.TryGetValue(simID, out StreamGroup simGroup))
            {
                simGroup = m_GroupManager.CreateGroup();
                m_Groups.Add(simID, simGroup);
                simProxy.SetGroup(simGroup);
            }

            if (!Simulations.IsReady(simID))
            {
                simulation.Builder.LoadAssemblies(ValidAssemblies);
                ISimulationSystem[] sys = m_SimBuilder.CreateSystems(simulation.Archetype, Content, true, out string contextName);
                SystemContainer container = new SystemContainer(Content, this);
                string contextContentFile = $"context.{contextName}";

                Console.Message($"[Simulation {simID}] Context: {contextContentFile}");

                if (Content.HasContent(contextContentFile))
                {
                    container.LoadContext(contextContentFile);
                }

                m_SharedContexts.ForEach(ctx => container.Context.SetContext(ctx));

                for(int i = 0; i < simProxy.BaseContext.Length; ++i)
                {
                    container.Context.SetContext(simProxy.BaseContext[i]);
                }

                m_Plugin.OnSimulationCreated(simulation, container);

                for (int i = 0; i < sys.Length; ++i)
                    container.AddSystem(sys[i]);

                Simulations.AttachSystems(simID, container);
            }
        }

        public T GetManager<T>() where T : IGameManager
        {
            if ( m_Managers.TryGetValue(typeof(T), out IGameManager manager))
            {
                return (T)manager;
            }
            return default;
        }

        public void RegisterManager<T>(T manager) where T : IGameManager
        {
            m_Managers.Add(typeof(T), manager);
        }
    }
}