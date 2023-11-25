using Dirt.Log;
using Dirt.Network;
using Dirt.Network.Simulation.Events;
using Mud;
using System.Collections.Generic;
using System.IO;

namespace Dirt.Simulation.Action
{
    public static class NetworkActionHelper
    {
        public static void RequestRemoteAction(this GameSimulation simulation, GameActor sourceActor, int actionIndex)
        {
            simulation.Events.Enqueue(new RemoteActionRequestEvent(sourceActor.ID, actionIndex));
        }
        public static void RequestRemoteAction(this GameSimulation simulation, GameActor sourceActor, int actionIndex, object[] parameters)
        {
            simulation.Events.Enqueue(new RemoteActionRequestEvent(sourceActor.ID, actionIndex, parameters));
        }

        public static MudMessage CreateNetworkMessage(bool client, int sourceActorNetID, int actionIndex, 
            List<ActionParameter> parameterBuffer, 
            MemoryStream stream,
            BinaryWriter wr)
        {
            stream.Flush();
            wr.Seek(0, SeekOrigin.Begin);
            wr.Write(sourceActorNetID);
            wr.Write((byte)actionIndex);
            for (int i = 0; i < parameterBuffer.Count; ++i)
            {
                wr.Write(parameterBuffer[i].intValue);
            }

            int opCode = (int)(client ? NetworkOperation.ActionRequest : NetworkOperation.ActorAction);

            return MudMessage.Create(opCode, stream.ToArray());
        }

        public static void ExtractAction(byte[] buffer, out int netID, out int actionIndex, List<ActionParameter> parameters)
        {
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

                for (int i = 0; i < pCount; ++i)
                {
                    parameters.Add(ActionParameter.FromBytes(buffer, fixedSize + i * 4));
                }
            }
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