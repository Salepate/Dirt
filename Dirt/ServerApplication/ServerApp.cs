using Dirt.Game.Managers;
using Dirt.GameServer;
using Dirt.GameServer.Managers;
using Dirt.Log;
using Dirt.ServerApplication.Clock;
using Mud.Server;
using System;
using System.Configuration;
using Console = Dirt.Log.Console;

namespace Space
{
    public class ServerApp
    {
        public MetricsManager Metrics { get; private set; }
        public WebService Web { get; private set; }

        private GameClock m_Clock;
        private RealTimeServer m_Server;
        private GameInstance m_Game;

        private TimeSpan m_TickPeriod;
        private int m_LastTick;
        public ServerApp()
        {
            Console.Logger = new BasicLogger();

            m_Server = new RealTimeServer(GetConfig("MaxClient"), GetConfig("ServerPort"));

            string contentPath = GetConfigString("ContentRoot");
            string contentVersion = GetConfigString("ContentVersion");

            m_Game = new GameInstance(m_Server.StreamGroups, contentPath, contentVersion);

            Web = new WebService("127.0.0.1", GetConfig("WebServerPort"));
            //Web.RegisterHandler(new PlayerMonitorRoute(m_Game));
            //Web.RegisterHandler(new SimulationRoute(m_Game));

            Web.Start();
            Metrics = m_Game.GetManager<MetricsManager>();

            m_Clock = new GameClock();
        }

        public void Run()
        {
            m_TickPeriod = new TimeSpan(10000 * 1000 / GetConfig("TickRate"));
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
            float deltaMs = delta / 1000f;
            m_Server.ProcessMessages();
            m_Game.UpdateInstance(deltaMs);
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