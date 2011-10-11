using System;
using System.Threading;

namespace Client.Framework
{
    public class GameComponent : IGameComponent, IUpdateable, IDisposable
    {
        private bool enabled = true;
        private int updateOrder;
        private Game game;
        private EventHandler<EventArgs> enabledChanged;
        private EventHandler<EventArgs> updateOrderChanged;
        private EventHandler<EventArgs> disposed;

        public bool Enabled
        {
            get
            {
                return enabled;
            }
            set
            {
                if (enabled != value)
                {
                    enabled = value;
                    OnEnabledChanged((object)this, EventArgs.Empty);
                }
            }
        }

        public int UpdateOrder
        {
            get
            {
                return updateOrder;
            }
            set
            {
                if (updateOrder != value)
                {
                    updateOrder = value;
                    OnUpdateOrderChanged((object)this, EventArgs.Empty);
                }
            }
        }

        public Game Game
        {
            get
            {
                return game;
            }
        }

        public event EventHandler<EventArgs> EnabledChanged
        {
            add
            {
                EventHandler<EventArgs> eventHandler = enabledChanged;
                EventHandler<EventArgs> comparand;
                do
                {
                    comparand = eventHandler;
                    eventHandler = Interlocked.CompareExchange<EventHandler<EventArgs>>(ref enabledChanged, comparand + value, comparand);
                }
                while (eventHandler != comparand);
            }
            remove
            {
                EventHandler<EventArgs> eventHandler = enabledChanged;
                EventHandler<EventArgs> comparand;
                do
                {
                    comparand = eventHandler;
                    eventHandler = Interlocked.CompareExchange<EventHandler<EventArgs>>(ref enabledChanged, comparand - value, comparand);
                }
                while (eventHandler != comparand);
            }
        }

        public event EventHandler<EventArgs> UpdateOrderChanged
        {
            add
            {
                EventHandler<EventArgs> eventHandler = updateOrderChanged;
                EventHandler<EventArgs> comparand;
                do
                {
                    comparand = eventHandler;
                    eventHandler = Interlocked.CompareExchange<EventHandler<EventArgs>>(ref updateOrderChanged, comparand + value, comparand);
                }
                while (eventHandler != comparand);
            }
            remove
            {
                EventHandler<EventArgs> eventHandler = updateOrderChanged;
                EventHandler<EventArgs> comparand;
                do
                {
                    comparand = eventHandler;
                    eventHandler = Interlocked.CompareExchange<EventHandler<EventArgs>>(ref updateOrderChanged, comparand - value, comparand);
                }
                while (eventHandler != comparand);
            }
        }

        public event EventHandler<EventArgs> Disposed
        {
            add
            {
                EventHandler<EventArgs> eventHandler = disposed;
                EventHandler<EventArgs> comparand;
                do
                {
                    comparand = eventHandler;
                    eventHandler = Interlocked.CompareExchange<EventHandler<EventArgs>>(ref disposed, comparand + value, comparand);
                }
                while (eventHandler != comparand);
            }
            remove
            {
                EventHandler<EventArgs> eventHandler = disposed;
                EventHandler<EventArgs> comparand;
                do
                {
                    comparand = eventHandler;
                    eventHandler = Interlocked.CompareExchange<EventHandler<EventArgs>>(ref disposed, comparand - value, comparand);
                }
                while (eventHandler != comparand);
            }
        }

        public GameComponent(Game game)
        {
            game = game;
        }

        ~GameComponent()
        {
            Dispose(false);
        }

        public virtual void Initialize()
        {
        }

        public virtual void Update(GameTime gameTime)
        {
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize((object)this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                lock (this)
                {
                    if (Game != null)
                        Game.Components.Remove((IGameComponent)this);
                    if (disposed != null)
                        disposed((object)this, EventArgs.Empty);
                }
            }
        }

        protected virtual void OnUpdateOrderChanged(object sender, EventArgs args)
        {
            if (updateOrderChanged != null)
                updateOrderChanged((object)this, args);
        }

        protected virtual void OnEnabledChanged(object sender, EventArgs args)
        {
            if (enabledChanged != null)
                enabledChanged((object)this, args);
        }
    }
}