using System;
using System.Windows.Graphics;
using Microsoft.Xna.Framework.Graphics;

namespace Client.Framework.Graphics
{
    public class DrawableGameComponent : GameComponent, IDrawable
    {
        private bool _visible = true;
        private bool _initialized;
        private int _drawOrder;

        public bool Visible
        {
            get { return _visible; }
            set
            {
                if (_visible != value)
                {
                    _visible = value;
                    OnVisibleChanged((object)this, EventArgs.Empty);
                }
            }
        }

        public int DrawOrder
        {
            get { return _drawOrder; }
            set
            {
                if (_drawOrder != value)
                {
                    _drawOrder = value;
                    OnDrawOrderChanged((object)this, EventArgs.Empty);
                }
            }
        }

        public GraphicsDevice GraphicsDevice
        {
            get { return GraphicsDeviceManager.Current.GraphicsDevice; }
        }

        public event EventHandler<EventArgs> VisibleChanged;
        public event EventHandler<EventArgs> DrawOrderChanged;

        public DrawableGameComponent(Game game)
            : base(game)
        {
        }

        public override void Initialize()
        {
            base.Initialize();

            if (!_initialized)
            {
                LoadContent();
            }

            _initialized = true;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                UnloadContent();
            }

            base.Dispose(disposing);
        }

        private void DeviceResetting(object sender, EventArgs e)
        {
        }

        private void DeviceReset(object sender, EventArgs e)
        {
        }

        private void DeviceCreated(object sender, EventArgs e)
        {
            LoadContent();
        }

        private void DeviceDisposing(object sender, EventArgs e)
        {
            UnloadContent();
        }

        public virtual void Draw(GameTime gameTime)
        {
        }

        protected virtual void LoadContent()
        {
        }

        protected virtual void UnloadContent()
        {
        }

        protected virtual void OnDrawOrderChanged(object sender, EventArgs args)
        {
            if (DrawOrderChanged != null)
                DrawOrderChanged((object)this, args);
        }

        protected virtual void OnVisibleChanged(object sender, EventArgs args)
        {
            if (VisibleChanged != null)
                VisibleChanged((object)this, args);
        }
    }
}