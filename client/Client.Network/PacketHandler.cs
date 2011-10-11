using System;

namespace Client.Network
{
    public delegate void OnPacketReceive(INetState state, PacketReader reader);

    public class PacketHandler
    {
        private int m_PacketID;
        private int m_Length;
        private OnPacketReceive m_OnReceive;

        public PacketHandler(int packetID, int length, OnPacketReceive onReceive)
        {
            m_PacketID = packetID;
            m_Length = length;
            m_OnReceive = onReceive;
        }

        public int PacketID
        {
            get
            {
                return m_PacketID;
            }
        }

        public int Length
        {
            get
            {
                return m_Length;
            }
        }

        public OnPacketReceive OnReceive
        {
            get
            {
                return m_OnReceive;
            }
        }
    }
}