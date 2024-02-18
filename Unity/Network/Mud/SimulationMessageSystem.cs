using Dirt;
using Dirt.Events;
using Dirt.Log;
using Dirt.Network;
using Dirt.Network.Managers;
using Dirt.Network.Model;
using Dirt.Network.Simulation;
using Dirt.Network.Simulation.Components;
using Dirt.Network.Simulation.Events;
using Dirt.Network.Systems;
using Dirt.Simulation;
using Dirt.Simulation.Action;
using Dirt.Simulation.Actor.Components;
using Dirt.Simulation.Builder;
using Dirt.Systems;
using Mud.Framework;
using Mud.Managers;
using System.IO;

namespace Mud.DirtSystems
{
    using BitConverter = System.BitConverter;
    // @TODO: Rename to NetworkMessageSystem
    public class SimulationMessageSystem : DirtSystem, IMessageConsumer
    {
        private const string SettingsContentName = "settings.netserial";

        private SimulationSystem m_Simulation;
        private NetworkSerializer m_Serializer;
        private ServerProxy m_Proxy;
        private NetworkEventDispatcher m_EventDispatcher;

        // translation table for net id to local id
        private int[] m_TranslationTable;

        public override void Initialize(DirtMode mode)
        {
            mode.FindSystem<MudConnector>().SetConsumer(this);
            m_Simulation = mode.FindSystem<SimulationSystem>();
            NetworkTypes serializableAss = mode.FindSystem<ContentSystem>().Content.LoadContent<NetworkTypes>(SettingsContentName);
            m_Serializer = new NetworkSerializer(serializableAss);
            m_Simulation.RegisterManager(m_Serializer);
            m_EventDispatcher = mode.FindSystem<NetworkEventDispatcher>();
            m_Proxy = m_Simulation.GetManager<ServerProxy>();
            m_TranslationTable = new int[NetInfo.MaxID+1];

        }

        private void ResetTranslationTable()
        {
            for(int i = 0; i < m_TranslationTable.Length; ++i)
            {
                m_TranslationTable[i] = 0;
            }
        }

        public bool OnCustomMessage(byte opCode, byte[] buffer) => ProcessCustomMessage((NetworkOperation)opCode, buffer);

        public void OnLocalNumber(int number)
        {
            m_Proxy.SetLocalPlayer(number);
        }

        private bool ProcessCustomMessage(NetworkOperation netOp, byte[] message)
        {
            int actorID;

            switch (netOp)
            {
                case NetworkOperation.LoadSimulation:
                    string sim = System.Text.Encoding.ASCII.GetString(message);
                    Console.Message($"Load simulation {sim}");
                    m_Simulation.ChangeSimulation(sim);
                    m_Proxy.Send(MudMessage.Create((byte)NetworkOperation.ClientReady, null));
                    ResetTranslationTable();
                    break;
                case NetworkOperation.SetSession:
                    int sessionID = BitConverter.ToInt32(message, 0);
                    ClientCommandProcessor.SessionID = sessionID.ToString();
                    m_EventDispatcher.Dispatch(new SessionIDEvent(sessionID));
                    break;
                case NetworkOperation.GameEvent:
                    NetworkEvent netEvent;
                    using (MemoryStream st = new MemoryStream(message))
                    {
                        netEvent = (NetworkEvent)m_Serializer.Deserialize(st);
                    }
                    m_EventDispatcher.Dispatch(netEvent);
                    break;
                case NetworkOperation.ActorState:
                    ActorState state;
                    using (MemoryStream st = new MemoryStream(message))
                    {
                        state = (ActorState)m_Serializer.Deserialize(st);
                    }
                    ActorBuilder builder = m_Simulation.Simulation.Builder;
                    GameActor newActor = builder.CreateActor();
                    for(int i = 0; i < state.Components.Length; ++i)
                    {
                        System.Type compType = state.Components[i].GetType();
                        int compIdx = builder.AddComponent(newActor, compType);
                        builder.Components.GetPool(compType).Set(compIdx, state.Components[i]);
                    }
                    if ( newActor.GetComponentIndex<NetInfo>() != -1 )
                    {
                        ref NetInfo netBhv = ref m_Simulation.Simulation.Filter.Get<NetInfo>(newActor);
                        m_TranslationTable[netBhv.ID] = newActor.ID;

                        bool isOwner = netBhv.Owner == m_Proxy.LocalPlayer;
                        netBhv.Owned = isOwner;
                        for (int i = 0; i < netBhv.Fields.Length; ++i)
                        {
                            ComponentFieldInfo field = netBhv.Fields[i];
                            field.Owner = field.Owner && isOwner;
                            netBhv.Fields[i] = field;
                        }
                    }
                    break;
                case NetworkOperation.ActorSync:
                    SyncActor(message[0], message);
                    break;
                case NetworkOperation.ActorRemove:
                    int reason = ActorStreaming.Destroyed;
                    if (message.Length > 1)
                        reason = message[message.Length - 1];
                    RemoveActor(message[0], reason);
                    break;
                case NetworkOperation.ActorAction:
                    NetworkActionHelper.ExtractAction(message, out int netID, out int actionIndex, out ActionParameter[] parameters);
                    actorID = m_TranslationTable[netID];
                    if (actorID > 0)
                    { 
                        ActorActionEvent actionEvent = new ActorActionEvent(actorID, actionIndex, parameters);
                        m_Simulation.DispatchEvent(actionEvent);
                    }
                    else
                    {
                        Console.Warning($"Actor (NetID {netID}) not found");
                    }
                    break;
                default:
                    return false;
            }
            return true;
        }

        private void RemoveActor(int netID, int reason)
        {
            int actorID = m_TranslationTable[netID];
            if (m_Simulation.Simulation.Filter.TryGetActor(actorID, out GameActor actor))
            {
                ref Destroy destroy = ref m_Simulation.Simulation.Builder.AddComponent<Destroy>(actor);
                destroy.Reason = reason;
            }
        }

        private void SyncActor(int netID, byte[] syncBuffer)
        {
            //actorID = m_TranslationTable[message[0]];
            int actorID = m_TranslationTable[netID];
            if (actorID > 0)
            {
                ref NetInfo netInfo = ref m_Simulation.Simulation.Filter.Get<NetInfo>(actorID);
                using (MemoryStream ms = new MemoryStream(syncBuffer))
                {
                    ms.Position = 1; // skip the net id
                    MessageHeader state = (MessageHeader)m_Serializer.Deserialize(ms);
                    netInfo.LastInBuffer = state;
                }
            }
        }
    }
}