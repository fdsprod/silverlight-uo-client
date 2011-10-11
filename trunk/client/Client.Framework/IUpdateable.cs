using System;

namespace Client.Framework
{
    public interface IUpdateable
    {
        bool Enabled { get; }

        int UpdateOrder { get; }

        event EventHandler<EventArgs> EnabledChanged;
        event EventHandler<EventArgs> UpdateOrderChanged;

        void Update(GameTime gameTime);
    }
}