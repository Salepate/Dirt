using Dirt.Game;
using Dirt.Network.Simulation.Components;
using Dirt.Network.Simulation.Events;
using Dirt.Simulation;
using Dirt.Simulation.Actor;
using Dirt.Simulation.Actor.Components;
using Dirt.Simulation.SystemHelper;
using Mud;
using Mud.Managers;
using System.Collections.Generic;
using System.IO;

namespace Dirt.Network.Simulation.Systems
{
    public class ClientActorSynchronization : ISimulationSystem, IEventReader, IManagerAccess
    {
        private HashSet<int> m_ToDestroy;
        private ServerProxy m_Server;

        public ClientActorSynchronization()
        {
            m_ToDestroy = new HashSet<int>();
        }

        [SimulationListener(typeof(ActorNetCullEvent), 0)]
        private void OnActorRemoved(ActorNetCullEvent removeEvent)
        {
            m_ToDestroy.Add(removeEvent.NetID);
        }

        public void Initialize(GameSimulation sim)
        {
        }

        public void UpdateActors(GameSimulation sim, float deltaTime)
        {
            foreach(var t in sim.Filter.GetAll<NetInfo>())
            {
                ref NetInfo netBhv = ref t.Get();

                if (m_ToDestroy.Contains(netBhv.ID))
                {
                    sim.Builder.AddComponent<Destroy>(t.Actor);
                    m_ToDestroy.Remove(netBhv.ID);
                }
                else
                {
                    if (netBhv.Owner == m_Server.LocalPlayer) // has some ownership
                    {
                        if (netBhv.LastOutBuffer != null && netBhv.LastOutBuffer.Length > 0)
                        {
                            using (MemoryStream st = new MemoryStream())
                            {
                                st.WriteByte((byte)netBhv.ID);
                                st.Write(netBhv.LastOutBuffer, 0, netBhv.LastOutBuffer.Length);
                                m_Server.Socket.Send(MudMessage.Create((byte)NetworkOperation.ActorSync, st.ToArray()));
                            }
                            netBhv.LastOutBuffer = null;
                        }
                    }
                }
            }
        }

        public void SetManagers(IManagerProvider provider)
        {
            m_Server = provider.GetManager<ServerProxy>();
        }
    }
}