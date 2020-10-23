using System;

namespace Mud.Server
{
    public class MudServer
    {
        public int MaxClients { get; private set; }
        public int ConnectedClients { get; private set; }
        private bool[] m_Slots;
        MudAddress[] m_Addresses;

        public MudServer(int maxClients)
        {
            MaxClients = maxClients;
            m_Slots = new bool[maxClients];
            m_Addresses = new MudAddress[maxClients];
            ConnectedClients = 1;
        }

        public int FindFreeSlot()
        {
            for(int i = 0; i < MaxClients; ++i)
            {
                if (!m_Slots[i])
                    return i;
            }
            return -1;
        }

        public bool IsSlotFree(int slotIndex)
        {
            return !m_Slots[slotIndex];
        }

        public int FindConnectedClientIndex(MudAddress address)
        {
            for(int i = 0; i < MaxClients; ++i)
            {
                if (m_Slots[i] && m_Addresses[i] == address)
                    return i;
            }
            return -1;
        }

        /// <summary>
        /// Assign a client to a slot
        /// </summary>
        /// <param name="slot"></param>
        /// <param name="address"></param>
        /// <returns>true when successful, false if slot is already occupied</returns>
        public bool SetConnectedClient(int slot, MudAddress address)
        {
            if ( !m_Slots[slot])
            {
                m_Slots[slot] = true;
                m_Addresses[slot] = address;
                ConnectedClients++;
                return true;
            }
            return false;
        }
        public MudAddress GetClientAddress(int slotIndex)
        {
            if (m_Slots[slotIndex])
                return m_Addresses[slotIndex];
            return default;
        }

        public void FreeSlot(int slotIndex)
        {
            if(m_Slots[slotIndex])
            {
                m_Addresses[slotIndex] = default;
                m_Slots[slotIndex] = false;
                --ConnectedClients;
            }
        }
    }
}