using Dirt.Simulation;

namespace Dirt.GameServer.Simulation.Events
{
    public class SimulationPausedEvent : SimulationEvent
    {
        public float PauseTime { get; private set; }
        public SimulationPausedEvent(float pauseTime)
        {
            PauseTime = pauseTime;
        }
    }
}
