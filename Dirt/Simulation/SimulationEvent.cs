namespace Dirt.Simulation
{
    public abstract class SimulationEvent
    {
        public int Event { get; protected set; }
        public bool Consumed { get; private set; }

        public SimulationEvent()
        {
            Consumed = false;
        }

        public void Consume()
        {
            Consumed = true;
        }
   
    }
}
