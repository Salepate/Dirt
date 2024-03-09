using Dirt.Network;
using Mud;
using Mud.Server.Stream;
using System.IO;

namespace Dirt.GameServer.Simulation.Helpers
{
    public static class StreamGroupExtension
    {
        public static void BroadcastEvent<T>(this StreamGroup group, T gameEvent) where T : NetworkEvent
        {
            byte[] eventBuffer;

            using (MemoryStream st = new MemoryStream())
            {
                GameInstance.Serializer.Serialize(st, gameEvent);
                eventBuffer = st.ToArray();
            }

            MudMessage message = MudMessage.Create((int)NetworkOperation.GameEvent, eventBuffer);
            for(int i = 0; i < group.Clients.Count; ++i)
            {
                group.Clients[i].Send(message);
            }
        }
    }
}
