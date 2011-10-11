using System.Collections.Generic;

namespace Client.Framework
{
    internal class DrawOrderComparer : IComparer<IDrawable>
    {
        public static readonly DrawOrderComparer Default = new DrawOrderComparer();

        static DrawOrderComparer()
        {
        }

        public int Compare(IDrawable x, IDrawable y)
        {
            if (x == null && y == null)
                return 0;
            else if (x == null)
                return 1;
            else if (y == null)
                return -1;
            else if (x.Equals((object)y))
                return 0;
            else if (x.DrawOrder < y.DrawOrder)
                return -1;
            else
                return 1;
        }
    }
}