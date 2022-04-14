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
        public ServerApp()
        {
            Console.Logger = new BasicLogger();

            m_Server = new RealTimeServer(GetConfig("MaxClient"), GetConfig("ServerPort"), GetConfig("ClientTimeout"));

            string contentPath = GetConfigString("ContentRoot");
            string contentVersion = GetConfigString("ContentVersion");

            string pluginLib = GetConfigString("PluginFile");
            string pluginClass = GetConfigString("PluginClass");

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
            WebService = new WebService("127.0.0.1", GetConfig("WebServerPort"));
            m_Game.RegisterManager(WebService);
            m_Game.RegisterManager(new RealTimeServerManager(m_Server));
            m_Game.InitializePlugin();
            //Web.RegisterHandler(new PlayerMonitorRoute(m_Game));
            //Web.RegisterHandler(new SimulationRoute(m_Game));
            Metrics = m_Game.GetManager<MetricsManager>();
            m_Clock = new GameClock();
        }

        public void Run()
        {
            m_TickPeriod = new TimeSpan(10000 * 1000 / GetConfig("TickRate"));
            bool terminate = false;

            m_Clock.Reset();
            int lastTick = m_Clock.GetTick();

            WebService.Start();
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

        public void ManuelSetup()
        {
            m_TickPeriod = new TimeSpan(10000 * 1000 / GetConfig("TickRate"));
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

        private int GetConfig(string name)
        {
            bool exist = int.TryParse(ConfigurationManager.AppSettings[name], out int val);
            if (!exist)
            {
                Console.Message($"Unable to get {name} configuration value");
            }
            return val;
        }

        private string GetConfigString(string name)
        {
            return ConfigurationManager.AppSettings[name];
        }
    }
}