using Dirt.Network;

namespace Dirt.Events
{
    public class SessionIDEvent : NetworkEvent
    {
        public int SessionID { get; private set; }

        public SessionIDEvent(int sessID)
        {
            SessionID = sessID;
        }
    }
}
