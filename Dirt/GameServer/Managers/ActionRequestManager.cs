using Dirt.Game;
using Dirt.GameServer.Simulation.Components;
using Dirt.Log;
using Dirt.Network;
using Dirt.Network.Simulation.Components;
using Dirt.Simulation;
using Dirt.Simulation.Action;
using Dirt.Simulation.Actor;
using Mud;
using Mud.Server;
using System.Collections.Generic;
using System.IO;

namespace Dirt.GameServer.Managers
{
    public class ActionRequestManager : IGameManager
    {
        private GameInstance m_Game;
        private PlayerManager m_Players;
        private SimulationManager m_Sims;

        private MemoryStream m_MemoryStream;
        private BinaryWriter m_BufferWriter;
        private List<ActionParameter> m_ParameterBuffer;
        private List<ActionParameter> m_NetRequestParameterBuffer;

        public ActionRequestManager(GameInstance game)
        {
            m_Game = game;
            m_Game.CustomMessage += OnCustomMessage;
            m_Players = game.GetManager<PlayerManager>();
            m_Sims = game.GetManager<SimulationManager>();
            m_ParameterBuffer = new List<ActionParameter>();
            m_NetRequestParameterBuffer = new List<ActionParameter>();
            m_MemoryStream = new MemoryStream();
            m_BufferWriter = new BinaryWriter(m_MemoryStream);
        }
        public void Update(float deltaTime)
        {
        }

        /// <summary>
        /// use to replicate server local actions (like monster attacking)
        /// </summary>
        /// <param name="sourceActor"></param>
        /// <param name="sim"></param>
        /// <param name="actionIndex"></param>
        public void RequestRemoteAction(GameActor sourceActor, GameSimulation sim, int actionIndex, params ActionParameter[] parameters)
        {
            SimulationProxy proxy = m_Sims.GetSimulationProxy(sim.ID);
            ActorActionContext actionContext = proxy.Systems.Context.GetContext<ActorActionContext>();
            bool replicateOwner, replicateOthers;

            if (actionContext.TryGetAction(actionIndex, out ActorAction action))
            {
                replicateOwner = action.Replication != ReplicationType.Others;
                replicateOthers = action.Replication != ReplicationType.Self;
            }
            else
            {
                Console.Warning($"Unknown Action Index {actionIndex}");
                return;
            }

            // Validate Action
            m_ParameterBuffer.Clear();
            for (int i = 0; i < parameters.Length; ++i)
                m_ParameterBuffer.Add(parameters[i]);
            
            action.FetchGameData(sim, proxy.Systems.Context, sourceActor, m_ParameterBuffer);
            ActionExecutionData execData = new ActionExecutionData() { SourceActor = sourceActor, Parameters = m_ParameterBuffer.ToArray() };

            if (action.ValidateAction(sim, proxy.Systems.Context, execData))
            {
                ref NetInfo netInfo = ref sim.Filter.Get<NetInfo>(sourceActor);
                int netID = netInfo.ID;
                int playerNumber = netInfo.Owner;

                // Generate action message
                MudMessage actionMessage = NetworkActionHelper.CreateNetworkMessage(false, netID, actionIndex, m_ParameterBuffer, m_MemoryStream, m_BufferWriter);
                ActorList<CullArea> cullAreas = sim.Filter.GetActors<CullArea>();
                for (int i = 0; i < cullAreas.Count; ++i)
                {
                    ref CullArea cullArea = ref cullAreas.GetC1(i);
                    bool isOwner = cullArea.Client == playerNumber;
                    if (cullArea.ProximityActors.Contains(netID) && (replicateOwner && isOwner || replicateOthers && !isOwner))
                    {
                        m_Players.FindPlayer(cullArea.Client).Client.Send(actionMessage, true);
                    }
                }
                sim.Events.Enqueue(new ActorActionEvent(sourceActor.ID, actionIndex, execData.Parameters));
            }
        }

        private void OnCustomMessage(GameClient client, MudMessage mudMessage)
        {
            switch ((NetworkOperation)mudMessage.opCode)
            {
                case NetworkOperation.ActionRequest:
                    if (mudMessage.buffer.Length < 5) //TODO: explicit check
                        return;

                    m_NetRequestParameterBuffer.Clear();
                    NetworkActionHelper.ExtractAction(mudMessage.buffer, out int netID, out int actionIndex, m_NetRequestParameterBuffer);
                    PlayerProxy player = m_Players.FindPlayer(client.Number);
                    SimulationProxy simProxy = m_Sims.GetSimulationProxy(player.Simulation);
                    GameSimulation sim = simProxy.Simulation;
                    GameActor sourceActor = sim.Filter.GetSingle<NetInfo>(n => n.ID == netID && n.Owner == client.Number);
                    RequestRemoteAction(sourceActor, sim, actionIndex, m_NetRequestParameterBuffer.ToArray());
                    break;
            }
        }
    }
}
