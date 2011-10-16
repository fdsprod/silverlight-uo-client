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

namespace Client.Input
{
    public sealed class KeyStateEventArgs : RoutedEventArgs
    {
        public bool Handled { get; set; }
        public Key Key { get; private set; }
        public int PlatformKeyCode { get; private set; }

        public KeyStateEventArgs(Key key, int platformKeyCode)
        {
            Key = key;
            PlatformKeyCode = platformKeyCode;
        }
    }
}
