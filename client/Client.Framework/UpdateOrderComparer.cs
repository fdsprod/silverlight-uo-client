using System.Collections.Generic;

namespace Client.Framework
{
    internal class UpdateOrderComparer : IComparer<IUpdateable>
    {
        public static readonly UpdateOrderComparer Default = new UpdateOrderComparer();

        static UpdateOrderComparer()
        {
        }

        public int Compare(IUpdateable x, IUpdateable y)
        {
            if (x == null && y == null)
                return 0;
            else if (x == null)
                return 1;
            else if (y == null)
                return -1;
            else if (x.Equals((object)y))
                return 0;
            else if (x.UpdateOrder < y.UpdateOrder)
                return -1;
            else
                return 1;
        }
    }
}