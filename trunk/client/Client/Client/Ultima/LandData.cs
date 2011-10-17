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
    public struct LandData
    {
        private readonly string m_Name;
        private readonly TileFlag m_Flags;

        public LandData(string name, TileFlag flags)
        {
            m_Name = name;
            m_Flags = flags;
        }

        public string Name
        {
            get { return m_Name; }
        }

        public TileFlag Flags
        {
            get { return m_Flags; }
        }
    }
}
