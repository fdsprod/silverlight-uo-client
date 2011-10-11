using System;

namespace Client.Framework
{
    public interface IDrawable
    {
        bool Visible { get; }

        int DrawOrder { get; }

        event EventHandler<EventArgs> VisibleChanged;
        event EventHandler<EventArgs> DrawOrderChanged;

        void Draw(GameTime gameTime);
    }
}