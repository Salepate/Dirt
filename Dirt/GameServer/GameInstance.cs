﻿using Dirt.Game;
using Dirt.Game.Content;
using Dirt.Game.Managers;
using Dirt.Game.Math;
using Dirt.Game.Model;
using Dirt.GameServer.GameCommand;
using Dirt.GameServer.Managers;
using Dirt.GameServer.PlayerStore;
using Dirt.GameServer.PlayerStore.Model;
using Dirt.Network;
using Dirt.Network.Events;
using Dirt.Network.Managers;
using Dirt.Network.Model;
using Dirt.Network.Simulation.Components;
using Dirt.Network.Simulation.Events;
using Dirt.Simulation;
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

namespace Dirt.GameServer
{
    using Console = Log.Console;

    public class GameInstance : IClientConsumer, IManagerProvider
    {
        public const string AssemblyCollection = "assemblies.server";
        private const int DefaultSimulation = 0;
        private const string DefaultSimulationName = "lobby";
        private const string SettingsContentName = "settings.netserial";

        /// <summary>
        /// This should only be accessed from the main thread
        /// </summary>
        public static NetworkSerializer Serializer;
        // Dirt
        private Dictionary<int, StreamGroup> m_Groups;
        private Dictionary<Type, IGameManager> m_Managers;
        public PluginInstance Plugin => m_Plugin;
        public SimulationManager Simulations { get; private set; }
        private StreamGroupManager m_GroupManager;
        public ContentProvider Content { get; private set; }
        private SimulationBuilder m_SimBuilder;

        public AssemblyCollection ValidAssemblies;
        public List<IContextItem> m_SharedContexts;
        private PlayerManager m_Players;
        private PluginInstance m_Plugin;

        public Action<GameClient, MudMessage> CustomMessage;
        public GameInstance(StreamGroupManager groupManager, RealTimeServerManager realTimeManager, string contentPath, string contentManifest, PluginInstance plugin)
        {
            Console.Assert(plugin != null, "No plugin specified");
            int webServicePort = int.Parse(ConfigurationManager.AppSettings["WebServerPort"]);
            // Dirt
            Content = new ContentProvider(contentPath);
            Content.LoadGameContent(contentManifest);
            Simulations = new SimulationManager(Content);
            Simulations.NotifySimulationDestroyed += OnSimulationDestroyed;
            m_Managers = new Dictionary<Type, IGameManager>();
            m_GroupManager = groupManager;
            m_Groups = new Dictionary<int, StreamGroup>();
            m_SimBuilder = new SimulationBuilder();
            m_SharedContexts = new List<IContextItem>();
            ValidAssemblies = Content.LoadContent<AssemblyCollection>(AssemblyCollection);
            m_SharedContexts.Add(ValidAssemblies);
            NetworkTypes netAsses = Content.LoadContent<NetworkTypes>(SettingsContentName);
            //@hack: Load missing assemblies before serializer 
            AssemblyReflection.BuildTypeMap<ISimulationSystem>(ValidAssemblies.Assemblies);
            Serializer = new NetworkSerializer(netAsses);
            m_Players = new PlayerManager(Serializer);

            m_SimBuilder.LoadAssemblies(ValidAssemblies);
            m_Plugin = plugin;

            RegisterManager<IContentProvider>(Content);
            RegisterManager(realTimeManager);
            RegisterManager(new MetricsManager());
            RegisterManager(Serializer);
            RegisterManager(m_Players);
            RegisterManager(Simulations);
            RegisterManager(new ActionRequestManager(this));
            RegisterManager(new WebService("127.0.0.1", webServicePort));
            PlayerStoreManager playerStore = new PlayerStoreManager(this);
            if (bool.TryParse(ConfigurationManager.AppSettings["UseRegistrationCode"], out bool useRegistrationCode))
            {
                playerStore.UseRegistrationCode = useRegistrationCode;
            }

            RegisterManager(playerStore);
            RegisterManager(new CommandProcessor(this));
            GetManager<CommandProcessor>().RegisterClassCommands<SessionCommands>();
            GetManager<WebService>().Start();
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

                m_Players.SendEvent(new PlayerConnectionEvent() { Player = proxy.Player });
                m_Players.SendEventTo(playerListEvent, proxy.Player);

                int simID = DefaultSimulation;
                GameSimulation sim = Simulations.GetSimulation(simID);

                if (sim == null)
                {
                    simID = Simulations.CreateSimulation(m_Plugin.DefaultSimulation, SimulationSpan.Persistent);
                    sim = Simulations.GetSimulation(simID);
                }

                GetManager<PlayerStoreManager>().CreateSession(proxy);
                MovePlayerToSimulation(proxy, simID);
                m_Plugin.PlayerJoined(proxy);
            }


            return null;
        }

        public void MovePlayerToSimulation(GameClient client, int simID, bool force = false)
        {
            PlayerProxy proxy = m_Players.FindPlayer(client.Number);
            if ( proxy == null)
            {
                Console.Error($"Invalid Player {client.ID}");
            }
            else
            {
                MovePlayerToSimulation(proxy, simID, force);
            }
        }
        public void MovePlayerToSimulation(PlayerProxy proxy, int simID, bool force = false)
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

            if ( oldSimIndex == simID && !force )
            {
                Console.Warning($"Player already in simulation {simID}, skipping request.");
                return;

            }
            proxy.Simulation = simID;
            proxy.Group = m_Groups[simID];

            // Notify previous simulation
            if ( oldSimIndex != -1 )
            {
                GameSimulation oldSim = Simulations.GetSimulation(oldSimIndex);
                var playerLeaveEvent = new PlayerEvent(proxy.Player.Number, PlayerEvent.LeftSimulation);
                oldSim.Events.Enqueue(playerLeaveEvent);
                m_Groups[oldSimIndex].UnregisterClient(client);

                // destroy sim if empty
                if ( oldSimIndex != simID && m_Groups[oldSimIndex].Clients.Count == 0 && Simulations.GetSpan(oldSimIndex) == SimulationSpan.Temporary )
                {
                    Simulations.Terminate(oldSimIndex);
                }
            }

            WarmSimulation(simID);
            m_Groups[simID].RegisterClient(client);
            client.Send(MudMessage.Create((int)NetworkOperation.LoadSimulation, System.Text.Encoding.ASCII.GetBytes(newSim.ArchetypeName)));
            PlayerEvent playerEvent = new PlayerEvent(client.Number, PlayerEvent.JoinedSimulation);
            newSim.Events.Enqueue(playerEvent);
            proxy.AwaitingLoad = true;
        }

        internal void AuthPlayer(int playerNumber, PlayerCredential credential)
        {
            PlayerProxy proxy = m_Players.FindPlayer(playerNumber);
            if ( proxy != null)
            {
                m_Plugin.PlayerAuthed(proxy, credential);
            }
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
                GetManager<PlayerStoreManager>().ClearSession(proxy);

                if ( m_Groups[proxy.Simulation].Clients.Count == 0 && Simulations.GetSpan(proxy.Simulation) == SimulationSpan.Temporary)
                {
                    Simulations.Terminate(proxy.Simulation);
                }
                else
                {
                    GameSimulation playerSim = Simulations.GetSimulation(proxy.Simulation);
                    PlayerEvent playerEvent = new PlayerEvent(client.Number, PlayerEvent.LeftSimulation);
                    playerSim.Events.Enqueue(playerEvent);
                }
            }
        }


        public void ProcessMessage(GameClient client, MudMessage message)
        {

            PlayerProxy proxy;
            GameSimulation sim;
            NetworkOperation mudOp = (NetworkOperation)message.opCode;
            switch (mudOp)
            {
                case NetworkOperation.ActorSync:
                    if(message.buffer.Length > 0)
                    {
                        int syncID = message.buffer[0];
                        SyncPlayerActor(client.Number, syncID, message.buffer);
                    }
                    break;
                case NetworkOperation.ClientReady:
                    proxy = m_Players.FindPlayer(client.Number);
                    if (proxy.AwaitingLoad)
                    {
                        Console.Assert(proxy != null, $"Unknown Player {client.Number}");
                        sim = Simulations.GetSimulation(proxy.Simulation);
                        sim.Events.Enqueue(new PlayerEvent(client.Number, PlayerEvent.LoadedSimulation));
                        proxy.AwaitingLoad = false;
                    }
                    break;
                default:
                    CustomMessage?.Invoke(client, message);
                    break;
            }
        }

        private void SyncPlayerActor(int number, int netID, byte[] buffer)
        {
            PlayerProxy proxy = m_Players.FindPlayer(number);
            Console.Assert(proxy != null, $"Unknown Player {number}");
            GameSimulation sim = Simulations.GetSimulation(proxy.Simulation);
            GameActor targetActor = sim.Filter.GetSingle<NetInfo>(a => a.ID == netID && proxy.Player.Number == a.Owner);
            if (targetActor != null)
            {
                ref NetInfo netInfo = ref sim.Filter.Get<NetInfo>(targetActor);
                netInfo.LastInBuffer[0] = buffer[0]; // netid
                netInfo.LastInBufferSize = mathop.max(1, netInfo.LastInBufferSize);
                if (netInfo.LastInBufferSize + buffer.Length - 1 < netInfo.LastInBuffer.Length)
                {
                    System.Buffer.BlockCopy(buffer, 1, netInfo.LastInBuffer, netInfo.LastInBufferSize, buffer.Length - 1);
                    netInfo.LastInBufferSize += buffer.Length - 1;
                }
                else
                {
                    Console.Error($"Buffer overflow for Net Actor {netID} in simulation {sim.ID}");
                }
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

        // SimulationManager.NotifySimulationDestroyed
        private void OnSimulationDestroyed(int simID)
        {
            StreamGroup group = m_Groups[simID];
            m_Groups.Remove(simID);
            m_GroupManager.DestroyGroup(group);
            m_Plugin.OnSimulationDestroyed(simID);
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
                if ( simulation.Archetype.LimitComponents )
                {
                    simulation.Builder.SetAllowedComponents(simulation.Archetype.LimitedComponents);
                }

                simulation.Builder.LoadAssemblies(ValidAssemblies);
                simulation.Resize(simulation.Archetype.MaximumActors, simulation.Archetype.MaximumQueries);

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
            Console.Warning($"Trying to get unregistered IGameManager: {typeof(T).Name}");
            return default;
        }

        public void RegisterManager<T>(T manager) where T : IGameManager
        {
            m_Managers.Add(typeof(T), manager);
        }
    }
}