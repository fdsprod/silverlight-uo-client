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
    [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential, Pack = 1)]
    public struct HuedTile
    {
        internal short m_ID;
        internal short m_Hue;
        internal sbyte m_Z;

        public int ID
        {
            get
            {
                return m_ID;
            }
        }

        public int Hue
        {
            get
            {
                return m_Hue;
            }
        }

        public int Z
        {
            get
            {
                return m_Z;
            }
            set
            {
                m_Z = (sbyte)value;
            }
        }

        public HuedTile(short id, short hue, sbyte z)
        {
            m_ID = id;
            m_Hue = hue;
            m_Z = z;
        }

        public void Set(short id, short hue, sbyte z)
        {
            m_ID = id;
            m_Hue = hue;
            m_Z = z;
        }
    }
}
