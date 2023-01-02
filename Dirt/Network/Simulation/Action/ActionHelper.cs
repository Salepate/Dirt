using Dirt.Log;
using Dirt.Network.Simulation.Events;

namespace Dirt.Simulation.Action
{
    public static partial class NetworkActionHelper
    {
        public static void RequestRemoteAction(this GameSimulation simulation, GameActor sourceActor, int actionIndex)
        {
            simulation.Events.Enqueue(new ActorActionEvent(sourceActor.ID, actionIndex));
            simulation.Events.Enqueue(new RemoteActionRequestEvent(sourceActor.ID, actionIndex));
        }
        public static void ExtractAction(byte[] buffer, out int netID, out int actionIndex, out ActionParameter[] parameters)
        {
            parameters = null;
            netID = System.BitConverter.ToInt32(buffer, 0);
            const int fixedSize = 5;
            actionIndex = (int)buffer[4];

            if (buffer.Length > fixedSize)
            {
                int rest = buffer.Length - fixedSize;
                int pCount = rest / 4;

                if (rest - pCount * 4 != 0)
                {
                    Console.Error($"Invalid byte array size for action {actionIndex}: size was {buffer.Length}");
                    return;
                }

                parameters = new ActionParameter[pCount];
                for (int i = 0; i < pCount; ++i)
                {
                    parameters[i] = ActionParameter.FromBytes(buffer, fixedSize + i * 4);
                }
            }
        }
    }
}