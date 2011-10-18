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
using System.Collections.Generic;

namespace Client
{
    public sealed class World 
    {
        private ClientEngine _engine;

        private Dictionary<Serial, Mobile> _mobiles;
        private Dictionary<Serial, Item> _items;

        public Dictionary<Serial, Mobile> Mobiles
        {
            get { return _mobiles; }
        }

        public Dictionary<Serial, Item> Items
        {
            get { return _items; }
        }

        public World(ClientEngine engine)
        {
            _engine = engine;

            _mobiles = new Dictionary<Serial, Mobile>();
            _items = new Dictionary<Serial, Item>();
        }
    }
}
