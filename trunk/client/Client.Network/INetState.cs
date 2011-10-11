using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Net.Sockets;

namespace Client.Network
{
    public interface INetState
    {
        IPAddress Address { get; }
        ByteQueue Buffer { get; }
        TimeSpan ConnectedFor { get; }
        DateTime ConnectedOn { get; }
        Socket Socket { get; }
        bool Running { get; }
        bool CompressionEnabled { get; set; }
        bool SentFirstPacket { get; set; }

        void Dispose();

        bool Flush();
        bool CheckAlive();
        bool Send(Packet p);
        bool Send(byte[] buffer, int index, int length);

        PacketHandler GetHandler(int packetID);
    }
}
