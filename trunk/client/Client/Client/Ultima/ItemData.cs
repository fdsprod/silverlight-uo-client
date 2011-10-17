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
    public struct ItemData
    {
        internal string m_Name;
        internal TileFlag m_Flags;
        internal byte m_Weight;
        internal byte m_Quality;
        internal byte m_Quantity;
        internal byte m_Value;
        internal byte m_Height;
        internal short m_Animation;

        public ItemData(string name, TileFlag flags, int weight, int quality, int quantity, int value, int height, int anim)
        {
            m_Name = name;
            m_Flags = flags;
            m_Weight = (byte)weight;
            m_Quality = (byte)quality;
            m_Quantity = (byte)quantity;
            m_Value = (byte)value;
            m_Height = (byte)height;
            m_Animation = (short)anim;
        }

        public string Name
        {
            get { return m_Name; }
        }

        public int Animation
        {
            get { return m_Animation; }
        }

        public TileFlag Flags
        {
            get { return m_Flags; }
        }

        public bool Background
        {
            get { return ((m_Flags & TileFlag.Background) != 0); }
        }

        public bool Bridge
        {
            get { return ((m_Flags & TileFlag.Bridge) != 0); }
        }

        public bool Impassable
        {
            get { return ((m_Flags & TileFlag.Impassable) != 0); }
        }

        public bool Surface
        {
            get { return ((m_Flags & TileFlag.Surface) != 0); }
        }

        public int Weight
        {
            get { return m_Weight; }
        }

        public int Quality
        {
            get { return m_Quality; }
        }

        public int Quantity
        {
            get { return m_Quantity; }
        }

        public int Value
        {
            get { return m_Value; }
        }

        public int Height
        {
            get { return m_Height; }
        }

        public int CalcHeight
        {
            get
            {
                if ((m_Flags & TileFlag.Bridge) != 0)
                    return m_Height / 2;

                return m_Height;
            }
        }
    }
}
