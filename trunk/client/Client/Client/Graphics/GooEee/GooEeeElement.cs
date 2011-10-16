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
using Microsoft.Xna.Framework;

namespace Client.Graphics.GooEee
{
    public abstract class GooEeeElement
    {
        private Vector2 _position;
        private Vector2 _size;
        private GooEeeElement _parent;

        public Vector2 Position
        {
            get { return _position; }
            set { _position = value; }
        }

        public Vector2 Size
        {
            get { return _size; }
            set { _size = value; }
        }

        public GooEeeElement Parent
        {
            get { return _parent; }
            set { _parent = value; }
        }

        protected Vector2 AbsolutePosition
        {
            get
            {
                Vector2 position = _position;

                if (_parent != null)
                    position += _parent.AbsolutePosition;

                return position;
            }
        }

        public GooEeeElement()
        {

        }
    }
}
