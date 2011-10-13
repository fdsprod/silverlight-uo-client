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
    public class LoginPacket : Packet
    {
        public LoginPacket(string username, string password)
            : base(0x80, 62)
        {
            Stream.WriteAsciiFixed(username, 30);
            Stream.WriteAsciiFixed(password, 30);
            Stream.Write((byte)0x5D);
        }
    }
}
