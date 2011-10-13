using System.Collections.Generic;

namespace Client.Network
{
	public static class PacketHandlers
	{
		private static PacketHandler[] _handlers;

		private static PacketHandler[] _extendedHandlersLow;
        private static Dictionary<int, PacketHandler> _extendedHandlersHigh;

		public static PacketHandler[] Handlers
		{
			get{ return _handlers; }
		}

        static PacketHandlers()
        {
            _handlers = new PacketHandler[0x100];
            _extendedHandlersLow = new PacketHandler[0x100];
            _extendedHandlersHigh = new Dictionary<int, PacketHandler>();
        }

        public static void Register(int packetID, int length, bool ingame, OnPacketReceive onReceive)
        {
            _handlers[packetID] = new PacketHandler(packetID, length, onReceive);
        }

        public static PacketHandler GetHandler(int packetID)
        {
            return _handlers[packetID];
        }
    }
}
