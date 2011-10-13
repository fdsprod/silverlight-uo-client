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

namespace Client.Network.Packets
{
    public class SeedPacket : Packet
    {
        public SeedPacket() :
            base(0x01, 4)
        {
            Stream.Write(new byte[] { 0x02, 0x03, 0x04 }, 0, 3);
        }
    }
}
