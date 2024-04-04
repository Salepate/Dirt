using Dirt.Game.Managers;
using Dirt.GameServer;
using Dirt.GameServer.Managers;
using Dirt.GameServer.PlayerStore;
using Dirt.Log;
using Dirt.ServerApplication.Clock;
using Mud.Server;
using System;
using System.Linq;
using Console = Dirt.Log.Console;

namespace Dirt.ServerApplication
{
    public class ServerApp
    {
        public const int MinimumSleep = 2;
        private readonly int m_OneSecondTicks = (int) new TimeSpan(0, 0, 1).Ticks;
        private readonly int m_MinimumSleepTime;
        public MetricsManager Metrics { get; private set; }
        public WebService WebService { get; private set; }

        private GameClock m_Clock;
        private RealTimeServer m_Server;
        private GameInstance m_Game;

        private int m_PeriodTicks;
        private int m_LastTickStamp;
        private int m_Tickrate; // how many ticks per second
        private float m_FixedDelta;
        public ServerApp(IConsoleLogger logger = null)
        {
            Console.Logger = logger ?? new BasicLogger();
            ServerConfig config = new ServerConfig();

            m_Server = new RealTimeServer(config);
            m_Tickrate = config.GetInt("TickRate");
            m_MinimumSleepTime = config.GetInt("MinSleep");
            int netTickrate = config.GetInt("NetTickRate");
            string contentPath = config.GetString("ContentRoot");
            string contentVersion = config.GetString("ContentVersion");
            string pluginLib = config.GetString("PluginFile");
            string pluginClass = config.GetString("PluginClass");
            m_TickPeriod = new TimeSpan(10000 * 1000 / config.GetInt("TickRate"));
            PluginInstance plugin = null;

            if (m_MinimumSleepTime > 0 && m_MinimumSleepTime < MinimumSleep)
            {
                Console.Warning($"Minimum Thread sleep cannot be less than {MinimumSleep}");
                m_MinimumSleepTime = MinimumSleep;
            }

            if (m_Tickrate <= 0)
            {
                Console.Error("Tickrate cannot be less than 1");
                return;
            }

            if (netTickrate <= 0)
            {
                Console.Warning("Net tickrate not specified, defaulting to regular tickrate");
                netTickrate = m_Tickrate;
            }

            Console.Message("Server Tickrate / Net Tickrate: {0} / {1}", m_Tickrate, netTickrate);
            m_PeriodTicks = (int) new TimeSpan(10000 * 1000 / m_Tickrate).Ticks;
            m_FixedDelta = 1f / m_Tickrate;

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
            
            m_Game = new GameInstance(m_Server.StreamGroups, new RealTimeServerManager(m_Server, netTickrate), contentPath, contentVersion, plugin);
            m_Game.InitializePlugin();
            Metrics = m_Game.GetManager<MetricsManager>();

            m_Game.GetManager<PlayerStoreManager>().AllowPlayerReconnect = config.GetBool("AllowPlayerReconnect");
            m_Clock = new GameClock();
        }

        public void Run()
        {
            bool terminate = false;

            m_Clock.Reset();
            m_Server.SetClientConsumer(m_Game);
            m_Server.Run();

            // dbg
            int cycleFrame = 0;
            int cycleStamp = m_Clock.GetTick();

            while (!terminate)
            {
                int now = m_Clock.GetTick();
                int diff100ns = now - m_LastTickStamp;
                bool procUpdate = diff100ns >= m_PeriodTicks && cycleFrame < m_Tickrate;
                bool procCycle = cycleFrame >= m_Tickrate - 1 && now - cycleStamp >= m_OneSecondTicks;

                if (procUpdate)
                {
                    Update(m_FixedDelta);
                    int toNextGameTickMS = (m_PeriodTicks - (m_Clock.GetTick() - now)) / 10000;
                    if (m_MinimumSleepTime > 0 && toNextGameTickMS > m_MinimumSleepTime)
                        System.Threading.Thread.Sleep(toNextGameTickMS - m_MinimumSleepTime);

                    ++cycleFrame;
                    m_LastTickStamp = now;
                }
                if (procCycle)
                {
                    cycleFrame = 0;
                    cycleStamp = m_Clock.GetTick(); // fetch tick directly to catch up with the executing frame
                }
            }

            m_Server.Stop();
        }

        public void ManualSetup()
        {
            m_Clock.Reset();
            m_Server.SetClientConsumer(m_Game);
            m_Server.Run();
            m_LastTickStamp = m_Clock.GetTick();
        }

        public void ManualStep()
        {
            int now = m_Clock.GetTick();
            int diff100ns = now - m_LastTickStamp;
            int diffMS = diff100ns / 10000;
            if (diffMS >= m_PeriodTicks)
            {
                Update(diffMS / 1000f);
                m_LastTickStamp = now;
            }
        }

        public void ManualStop()
        {
            m_Server.Stop();
        }

        public void Update(float delta)
        {
            m_Server.ProcessMessages(delta);
            m_Game.UpdateInstance(delta);
        }
    }
}