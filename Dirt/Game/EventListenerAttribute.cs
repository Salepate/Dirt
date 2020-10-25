namespace Dirt.Game
{
    [System.AttributeUsage(System.AttributeTargets.Method)]
    public class EventListenerAttribute : System.Attribute
    {
        public System.Type EventType { get; private set; }
        public int EventID { get; private set; }
        public EventListenerAttribute(System.Type eventType, int eventID)
        {
            EventType = eventType;
            EventID = eventID;
        }
    }
}
