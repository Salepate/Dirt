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

        public ActionRequestManager(GameInstance game)
        {
            m_Game = game;
            m_Game.CustomMessage += OnCustomMessage;
            m_Players = game.GetManager<PlayerManager>();
            m_Sims = game.GetManager<SimulationManager>();
            m_ParameterBuffer = new List<ActionParameter>();
            m_MemoryStream = new MemoryStream();
            m_BufferWriter = new BinaryWriter(m_MemoryStream);
        }
        public void Update(float deltaTime)
        {
        }

        private void OnCustomMessage(GameClient client, MudMessage mudMessage)
        {
            switch ((NetworkOperation)mudMessage.opCode)
            {
                case NetworkOperation.ActionRequest:
                    if (mudMessage.buffer.Length < 5) //TODO: explicit check
                        return;

                    m_ParameterBuffer.Clear();
                    NetworkActionHelper.ExtractAction(mudMessage.buffer, out int netID, out int actionIndex, m_ParameterBuffer);

                    PlayerProxy player = m_Players.FindPlayer(client.Number);
                    SimulationProxy simProxy = m_Sims.GetSimulationProxy(player.Simulation);
                    GameSimulation sim = simProxy.Simulation;
                    ActorActionContext actionContext = m_Sims.GetSimulationProxy(player.Simulation).Systems.Context.GetContext<ActorActionContext>();
                    GameActor sourceActor = sim.Filter.GetSingle<NetInfo>(n => n.ID == netID && n.Owner == client.Number);
                    bool replicateOwner = false;

                    if (sourceActor == null)
                        return;

                    if (actionContext.TryGetAction(actionIndex, out ActorAction action))
                    {
                        replicateOwner = action.ReplicateSelf;
                    }
                    else
                    {
                        Console.Warning($"Unknown Action Index {actionIndex}");
                        return;
                    }

                    // Validate Action
                    action.FetchGameData(sim, simProxy.Systems.Context, sourceActor, m_ParameterBuffer);
                    ActionExecutionData execData = new ActionExecutionData() { SourceActor = sourceActor, Parameters = m_ParameterBuffer.ToArray() };

                    if ( action.ValidateAction(sim, simProxy.Systems.Context, execData))
                    {
                        // Generate action message
                        MudMessage actionMessage = NetworkActionHelper.CreateNetworkMessage(false, netID, actionIndex, m_ParameterBuffer, m_MemoryStream, m_BufferWriter);
                        ActorList<CullArea> cullAreas = sim.Filter.GetActors<CullArea>();
                        for(int i = 0; i < cullAreas.Count; ++i)
                        {
                            ref CullArea cullArea = ref cullAreas.GetC1(i);
                            if (cullArea.ProximityActors.Contains(netID) && (replicateOwner || cullArea.Client != client.Number))
                            {
                                m_Players.FindPlayer(cullArea.Client).Client.Send(actionMessage, true);
                            }
                        }

                        sim.Events.Enqueue(new ActorActionEvent(sourceActor.ID, actionIndex, execData.Parameters));
                    }
                    break;
            }
        }
    }
}
