using Dirt.Network.Simulation.Components;
using Dirt.Simulation;
using Dirt.Simulation.Action;
using Mud;
using Mud.DirtSystems;
using System.Collections.Generic;
using System.IO;

namespace Dirt.Network.Simulation.Action
{
    public abstract class NetworkActorAction
    {
        private GameSimulation m_Simulation;
        private MudConnector m_Connector;
        // Network data
        private MemoryStream m_BufferStream;
        private BinaryWriter m_BufferWriter;

        public NetworkActorAction()
        {
            m_BufferStream = new MemoryStream();
            m_BufferWriter = new BinaryWriter(m_BufferStream);
        }


        public void SetNetworkData(MudConnector connector)
        {
            m_Connector = connector;
        }

        public void SetSimulation(GameSimulation simulation)
        {
            m_Simulation = simulation;
        }

        protected void SendActorAction(GameActor actor, int actorAction, List<ActionParameter> actionParameters)
        {
            int netID = m_Simulation.Filter.Get<NetInfo>(actor).ID;
            m_BufferStream.Flush();
            m_BufferWriter.Seek(0, SeekOrigin.Begin);
            m_BufferWriter.Write(netID);
            m_BufferWriter.Write((byte)actorAction);
            for (int i = 0; i < actionParameters.Count; ++i)
            {
                m_BufferWriter.Write(actionParameters[i].intValue);
            }
            MudMessage mudMessage = MudMessage.Create((int)NetworkOperation.ActionRequest, m_BufferStream.ToArray());
            m_Connector.Socket.Send(mudMessage);
        }
    }
}
