using Dirt;
using Dirt.Log;
using Dirt.Network;
using Dirt.Network.Managers;
using Dirt.Network.Model;
using Dirt.Network.Simulation;
using Dirt.Network.Simulation.Components;
using Dirt.Network.Simulation.Events;
using Dirt.Simulation;
using Dirt.Simulation.Builder;
using Dirt.Simulation.Events;
using Dirt.Systems;
using Mud.Framework;
using Mud.Managers;
using System.IO;

namespace Mud.DirtSystems
{
    public class SimulationMessageSystem : DirtSystem, IMessageConsumer
    {
        private const string SettingsContentName = "settings.netserial";

        private SimulationSystem m_Simulation;
        private NetworkSerializer m_Serializer;
        private ServerProxy m_Proxy;
        private NetworkEventDispatcher m_EventDispatcher;

        public override void Initialize(DirtMode mode)
        {
            mode.FindSystem<MudConnector>().SetConsumer(this);
            m_Simulation = mode.FindSystem<SimulationSystem>();
            NetworkTypes serializableAss = mode.FindSystem<ContentSystem>().Content.LoadContent<NetworkTypes>(SettingsContentName);
            m_Serializer = new NetworkSerializer(serializableAss);
            m_Simulation.RegisterManager(m_Serializer);
            m_EventDispatcher = mode.FindSystem<NetworkEventDispatcher>();
            m_Proxy = m_Simulation.GetManager<ServerProxy>();
        }

        public void OnCustomMessage(byte opCode, byte[] buffer)
        {
            ProcessCustomMessage((NetworkOperation)opCode, buffer);
        }

        public void OnLocalNumber(int number)
        {
            m_Proxy.SetLocalPlayer(number);
        }

        private void ProcessCustomMessage(NetworkOperation netOp, byte[] message)
        {
            switch (netOp)
            {
                case NetworkOperation.LoadSimulation:
                    string sim = System.Text.Encoding.ASCII.GetString(message);
                    Console.Message($"Loading simulation {sim}");
                    m_Simulation.ChangeSimulation(sim);
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
                    //Console.Message("Received Actor");
                    using (MemoryStream st = new MemoryStream(message))
                    {
                        state = (ActorState)m_Serializer.Deserialize(st);
                    }
                    ActorBuilder builder = m_Simulation.Simulation.Builder;
                    GameActor copyActor = builder.CreateActor();
                    for(int i = 0; i < state.Components.Length; ++i)
                    {
                        System.Type compType = state.Components[i].GetType();
                        int compIdx = builder.AddComponent(copyActor, compType);
                        builder.Components.GetPool(compType).Set(compIdx, state.Components[i]);
                    }
                    if ( copyActor.GetComponentIndex<NetInfo>() != -1 )
                    {
                        ref NetInfo netBhv = ref m_Simulation.Simulation.Filter.Get<NetInfo>(copyActor);
                        bool isOwner = netBhv.Owner == m_Proxy.LocalPlayer;
                        for (int i = 0; i < netBhv.Fields.Count; ++i)
                        {
                            ComponentFieldInfo field = netBhv.Fields[i];
                            field.Owner = field.Owner && isOwner;
                            netBhv.Fields[i] = field;
                        }
                    }
                    break;
                case NetworkOperation.ActorSync:
                    byte[] syncBuffer = new byte[message.Length - 1];
                    System.Array.Copy(message, 1, syncBuffer, 0, message.Length - 1);
                    m_Simulation.DispatchEvent(new ActorSyncEvent(message[0], syncBuffer));
                    break;
                case NetworkOperation.ActorRemove:
                    m_Simulation.DispatchEvent(new ActorNetCullEvent(message[0]));
                    break;
                default:
                    break;
            }
        }
    }
}