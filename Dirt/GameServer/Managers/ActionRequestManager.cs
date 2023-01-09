using Dirt.Game;
using Dirt.GameServer.Simulation.Components;
using Dirt.Network;
using Dirt.Network.Simulation.Components;
using Dirt.Simulation;
using Dirt.Simulation.Action;
using Mud;
using Mud.Server;

namespace Dirt.GameServer.Managers
{
    public class ActionRequestManager : IGameManager
    {
        private GameInstance m_Game;
        private PlayerManager m_Players;
        private SimulationManager m_Sims;

        public ActionRequestManager(GameInstance game)
        {
            m_Game = game;
            m_Game.CustomMessage += OnCustomMessage;
            m_Players = game.GetManager<PlayerManager>();
            m_Sims = game.GetManager<SimulationManager>();
        }
        public void Update(float deltaTime)
        {
        }

        private void OnCustomMessage(GameClient client, MudMessage mudMessage)
        {
            switch ((NetworkOperation)mudMessage.opCode)
            {
                case NetworkOperation.ActionRequest:
                    if (mudMessage.buffer.Length < 5) // @TODO: explicit check
                        return;

                    NetworkActionHelper.ExtractAction(mudMessage.buffer, out int netID, out int actionIndex, out ActionParameter[] actionParams);

                    PlayerProxy player = m_Players.FindPlayer(client.Number);
                    GameSimulation sim = m_Sims.GetSimulation(player.Simulation);
                    var validActors = sim.Filter.GetActorsMatching<NetInfo>(n => n.ID == netID);
                    if (validActors.Count > 0 && validActors[0].Get().Owner == client.Number)
                    {
                        GameActor actor = validActors[0].Actor;
                        var cullAreas = sim.Filter.GetActorsMatching<CullArea>(c => c.ProximityActors.Contains(netID));
                        SimulationProxy simProxy = m_Sims.GetSimulationProxy(sim.ID);
                        MudMessage actionMessage = MudMessage.Create((int)NetworkOperation.ActorAction, mudMessage.buffer);
                        // Send other events
                        foreach (Dirt.Simulation.Actor.ActorTuple<CullArea> cullArea in cullAreas)
                        {
                            int targetClient = cullArea.Get().Client;
                            if (targetClient != client.Number)
                            {
                                m_Players.FindPlayer(targetClient).Client.Send(actionMessage, true);
                            }
                        }
                        sim.Events.Enqueue(new ActorActionEvent(validActors[0].Actor.ID, actionIndex, actionParams));
                    }
                    break;
            }
        }
    }
}
