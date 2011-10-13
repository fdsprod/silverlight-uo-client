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

namespace Client.Ultima
{
    public static class PixelConverter
    {
        public static uint ARGB1555toARGB8888(ushort color16)
        {
            uint a = color16 & (uint)0x8000;
            uint r = color16 & (uint)0x7C00;
            uint g = color16 & (uint)0x03E0;
            uint b = color16 & (uint)0x1F;
            uint rgb = (r << 9) | (g << 6) | (b << 3);

            return (a * 0x1FE00) | rgb | ((rgb >> 5) & 0x070707);
        }
    }
}
