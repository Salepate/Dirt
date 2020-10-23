using Dirt.Simulation;

namespace Dirt.Network.Simulation.Events
{
    public class PlayerEvent : SimulationEvent
    {
        public const int JoinedSimulation = 0;
        public const int LeftSimulation = 1;

        public int Number { get; private set; }
        public PlayerEvent(int playerNumber, int playerEvent)
        {
            Number = playerNumber;
            Event = playerEvent;
        }
    }
}