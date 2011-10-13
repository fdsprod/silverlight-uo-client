using System;

namespace Client.Network
{
    public delegate void OnPacketReceive(NetState state, PacketReader reader);

    public class PacketHandler
    {
        private int _packetId;
        private int _length;
        private OnPacketReceive _onReceive;

        public PacketHandler(int packetId, int length, OnPacketReceive onReceive)
        {
            _packetId = packetId;
            _length = length;
            _onReceive = onReceive;
        }

        public int PacketID
        {
            get
            {
                return _packetId;
            }
        }

        public int Length
        {
            get
            {
                return _length;
            }
        }

        public OnPacketReceive OnReceive
        {
            get
            {
                return _onReceive;
            }
        }
    }
}