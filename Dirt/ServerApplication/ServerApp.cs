using Dirt.Game.Managers;
using Dirt.GameServer;
using Dirt.GameServer.Managers;
using Dirt.Log;
using Dirt.ServerApplication.Clock;
using Mud.Server;
using System;
using System.Configuration;
using System.Linq;
using Console = Dirt.Log.Console;

namespace Dirt.ServerApplication
{
    public class ServerApp
    {
        public MetricsManager Metrics { get; private set; }
        public WebService WebService { get; private set; }

        private GameClock m_Clock;
        private RealTimeServer m_Server;
        private GameInstance m_Game;

        private TimeSpan m_TickPeriod;
        private int m_LastTick;
        public ServerApp(IConsoleLogger logger = null)
        {
            Console.Logger = logger ?? new BasicLogger();
            ServerConfig config = new ServerConfig();

            m_Server = new RealTimeServer(config);
            int netTickrate = config.GetInt("NetTickRate");
            if ( netTickrate <= 0 )
            {
                netTickrate = config.GetInt("TickRate");
                Console.Warning($"Net tickrate not specified, defaulting to regular tickrate ({netTickrate}/s)");
            }
            else
            {
                Console.Message($"Net Tickrate set to {netTickrate}/s");
            }

            string contentPath = config.GetString("ContentRoot");
            string contentVersion = config.GetString("ContentVersion");

            string pluginLib = config.GetString("PluginFile");
            string pluginClass = config.GetString("PluginClass");
            m_TickPeriod = new TimeSpan(10000 * 1000 / config.GetInt("TickRate"));

            PluginInstance plugin = null;

            try
            {
                var pluginAssembly = System.AppDomain.CurrentDomain.Load(pluginLib);
                Type pluginType = pluginAssembly.GetTypes().Where(t => t.FullName == pluginClass).FirstOrDefault();
                if ( pluginType != null )
                {
                    plugin = (PluginInstance) System.Activator.CreateInstance(pluginType);
                }
                else
                {
                    Console.Error($"Plugin Class {pluginClass} not found in {pluginAssembly.FullName}");
                    plugin = new DummyPlugin();
                }

            }
            catch (Exception e)
            {
                Console.Error($"Unable to load {pluginLib}");
                Console.Error(e.Message);
            }

            m_Game = new GameInstance(m_Server.StreamGroups, contentPath, contentVersion, plugin);
            m_Game.RegisterManager(new RealTimeServerManager(m_Server, netTickrate));
            m_Game.InitializePlugin();
            Metrics = m_Game.GetManager<MetricsManager>();
            m_Clock = new GameClock();
        }

        public void Run()
        {
            bool terminate = false;

            m_Clock.Reset();
            int lastTick = m_Clock.GetTick();

            m_Server.SetClientConsumer(m_Game);
            m_Server.Run();

            while (!terminate)
            {
                int now = m_Clock.GetTick();
                TimeSpan diff = new TimeSpan(now - lastTick);
                if (diff >= m_TickPeriod)
                {
                    Update((float)diff.TotalMilliseconds);
                    lastTick += (int)diff.Ticks;
                }
            }

            m_Server.Stop();
        }

        public void ManualSetup()
        {
            m_Clock.Reset();
            m_Server.SetClientConsumer(m_Game);
            m_Server.Run();
            m_LastTick = m_Clock.GetTick();
        }

        public void ManualStep()
        {
            int now = m_Clock.GetTick();
            TimeSpan diff = new TimeSpan(now - m_LastTick);
            if (diff >= m_TickPeriod)
            {
                Update((float)diff.TotalMilliseconds);
                m_LastTick += (int)diff.Ticks;
            }
        }

        public void ManualStop()
        {
            m_Server.Stop();
        }

        public void Update(float delta)
        {
            float deltaInSeconds = delta / 1000f;
            m_Server.ProcessMessages(deltaInSeconds);
            m_Game.UpdateInstance(deltaInSeconds);
        }
    }
}