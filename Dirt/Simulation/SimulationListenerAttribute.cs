namespace Dirt.Simulation
{
    [System.AttributeUsage(System.AttributeTargets.Method)]
    public class SimulationListenerAttribute : System.Attribute
    {
        public System.Type EventType { get; private set; }
        public int EventID { get; private set; }
        public SimulationListenerAttribute(System.Type eventType, int eventID)
        {
            EventType = eventType;
            EventID = eventID;
        }
    }
}