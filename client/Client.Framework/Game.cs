using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Graphics;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Client.Framework.Graphics;

namespace Client.Framework
{
    public class Game
    {
        private readonly TimeSpan _maximumElapsedTime = TimeSpan.FromMilliseconds(500.0);
        private GameTime _gameTime = new GameTime();
        private int _updatesSinceRunningSlowly1 = int.MaxValue;
        private int _updatesSinceRunningSlowly2 = int.MaxValue;
        private List<IUpdateable> _updateableComponents = new List<IUpdateable>();
        private List<IUpdateable> _currentlyUpdatingComponents = new List<IUpdateable>();
        private List<IDrawable> _drawableComponents = new List<IDrawable>();
        private List<IDrawable> _currentlyDrawingComponents = new List<IDrawable>();
        private List<IGameComponent> _notYetInitialized = new List<IGameComponent>();
        private GameServiceContainer _gameServices = new GameServiceContainer();

        private bool _isActive;
        private bool _exitRequested;
        private bool _isMouseVisible;
        private bool _inRun;
        private bool _endRunRequired;
        private bool _drawRunningSlowly;
        private bool _doneFirstUpdate;
        private bool _doneFirstDraw;
        private bool _forceElapsedTimeToZero;
        private bool _suppressDraw;
        private TimeSpan _inactiveSleepTime;
        private TimeSpan _totalGameTime;
        private TimeSpan _targetElapsedTime;
        private TimeSpan _accumulatedElapsedGameTime;
        private TimeSpan _lastFrameElapsedGameTime;
        private GameComponentCollection _gameComponents;
        private ContentManager _content;
        private EventHandler<EventArgs> _activated;
        private EventHandler<EventArgs> _deactivated;
        private EventHandler<EventArgs> _exiting;
        private EventHandler<EventArgs> _disposed;
        private DrawingSurface _drawingSurface;

        public GameComponentCollection Components
        {
            get { return _gameComponents; }
        }

        public DrawingSurface DrawingSurface
        {
            get { return _drawingSurface; }
        }

        public GameServiceContainer Services
        {
            get { return _gameServices; }
        }

        public TimeSpan InactiveSleepTime
        {
            get { return _inactiveSleepTime; }
            set
            {
                if (value < TimeSpan.Zero)
                    throw new ArgumentOutOfRangeException("value", "InactiveSleepTime cannot be Zero.");
                else
                    _inactiveSleepTime = value;
            }
        }

        //public bool IsMouseVisible
        //{
        //    get
        //    {
        //        return isMouseVisible;
        //    }
        //    set
        //    {
        //        isMouseVisible = value;
        //        if (Window != null)
        //            Window.IsMouseVisible = value;
        //    }
        //}

        public TimeSpan TargetElapsedTime
        {
            get
            {
                return _targetElapsedTime;
            }
            set
            {
                if (value <= TimeSpan.Zero)
                    throw new ArgumentOutOfRangeException("value", "TargetElapsedTime cannot be Zero.");
                else
                    _targetElapsedTime = value;
            }
        }
        
        public GraphicsDevice GraphicsDevice
        {
            get { return GraphicsDeviceManager.Current.GraphicsDevice; }
        }

        public ContentManager Content
        {
            get
            {
                return _content;
            }
            set
            {
                if (value == null)
                    throw new ArgumentNullException();
                else
                    _content = value;
            }
        }

        internal bool IsActiveIgnoringGuide
        {
            get
            {
                return _isActive;
            }
        }

        private bool ShouldExit
        {
            get
            {
                return _exitRequested;
            }
        }

        public event EventHandler<EventArgs> Activated
        {
            add
            {
                EventHandler<EventArgs> eventHandler = _activated;
                EventHandler<EventArgs> comparand;
                do
                {
                    comparand = eventHandler;
                    eventHandler = Interlocked.CompareExchange<EventHandler<EventArgs>>(ref _activated, comparand + value, comparand);
                }
                while (eventHandler != comparand);
            }
            remove
            {
                EventHandler<EventArgs> eventHandler = _activated;
                EventHandler<EventArgs> comparand;
                do
                {
                    comparand = eventHandler;
                    eventHandler = Interlocked.CompareExchange<EventHandler<EventArgs>>(ref _activated, comparand - value, comparand);
                }
                while (eventHandler != comparand);
            }
        }

        public event EventHandler<EventArgs> Deactivated
        {
            add
            {
                EventHandler<EventArgs> eventHandler = _deactivated;
                EventHandler<EventArgs> comparand;
                do
                {
                    comparand = eventHandler;
                    eventHandler = Interlocked.CompareExchange<EventHandler<EventArgs>>(ref _deactivated, comparand + value, comparand);
                }
                while (eventHandler != comparand);
            }
            remove
            {
                EventHandler<EventArgs> eventHandler = _deactivated;
                EventHandler<EventArgs> comparand;
                do
                {
                    comparand = eventHandler;
                    eventHandler = Interlocked.CompareExchange<EventHandler<EventArgs>>(ref _deactivated, comparand - value, comparand);
                }
                while (eventHandler != comparand);
            }
        }

        public event EventHandler<EventArgs> Exiting
        {
            add
            {
                EventHandler<EventArgs> eventHandler = _exiting;
                EventHandler<EventArgs> comparand;
                do
                {
                    comparand = eventHandler;
                    eventHandler = Interlocked.CompareExchange<EventHandler<EventArgs>>(ref _exiting, comparand + value, comparand);
                }
                while (eventHandler != comparand);
            }
            remove
            {
                EventHandler<EventArgs> eventHandler = _exiting;
                EventHandler<EventArgs> comparand;
                do
                {
                    comparand = eventHandler;
                    eventHandler = Interlocked.CompareExchange<EventHandler<EventArgs>>(ref _exiting, comparand - value, comparand);
                }
                while (eventHandler != comparand);
            }
        }

        public event EventHandler<EventArgs> Disposed
        {
            add
            {
                EventHandler<EventArgs> eventHandler = _disposed;
                EventHandler<EventArgs> comparand;
                do
                {
                    comparand = eventHandler;
                    eventHandler = Interlocked.CompareExchange<EventHandler<EventArgs>>(ref _disposed, comparand + value, comparand);
                }
                while (eventHandler != comparand);
            }
            remove
            {
                EventHandler<EventArgs> eventHandler = _disposed;
                EventHandler<EventArgs> comparand;
                do
                {
                    comparand = eventHandler;
                    eventHandler = Interlocked.CompareExchange<EventHandler<EventArgs>>(ref _disposed, comparand - value, comparand);
                }
                while (eventHandler != comparand);
            }
        }

        public Game(DrawingSurface drawingSurface)
        {
            _drawingSurface = drawingSurface;
            _gameComponents = new GameComponentCollection();
            _gameComponents.ComponentAdded += new EventHandler<GameComponentCollectionEventArgs>(GameComponentAdded);
            _gameComponents.ComponentRemoved += new EventHandler<GameComponentCollectionEventArgs>(GameComponentRemoved);
            _content = new ContentManager((IServiceProvider)_gameServices);
            _totalGameTime = TimeSpan.Zero;
            _accumulatedElapsedGameTime = TimeSpan.Zero;
            _lastFrameElapsedGameTime = TimeSpan.Zero;
            _targetElapsedTime = TimeSpan.FromTicks(166667L);
            _inactiveSleepTime = TimeSpan.FromMilliseconds(20.0);

            Initialize();
        }

        ~Game()
        {
            Dispose(false);
        }

        public void Run()
        {
            RunGame(true);
        }

        internal void StartGameLoop()
        {
            RunGame(false);
        }

        private void RunGame(bool useBlockingRun)
        {
            try
            {
                Initialize();
                _inRun = true;
                _gameTime.ElapsedGameTime = TimeSpan.Zero;
                _gameTime.TotalGameTime = _totalGameTime;
                _gameTime.IsRunningSlowly = false;
                Update(_gameTime);
                _doneFirstUpdate = true;
            }
            finally
            {
                if (!_endRunRequired)
                    _inRun = false;
            }
        }

        public void Tick(TimeSpan elapsedTime, TimeSpan totalGameTime)
        {
            if (!ShouldExit)
            {
                bool draw = true;

                if (elapsedTime < TimeSpan.Zero)
                    elapsedTime = TimeSpan.Zero;
                if (_forceElapsedTimeToZero)
                {
                    elapsedTime = TimeSpan.Zero;
                    _forceElapsedTimeToZero = false;
                }
                if (elapsedTime > _maximumElapsedTime)
                    elapsedTime = _maximumElapsedTime;

                if (Math.Abs(elapsedTime.Ticks - _targetElapsedTime.Ticks) < _targetElapsedTime.Ticks >> 6)
                    elapsedTime = _targetElapsedTime;

                _accumulatedElapsedGameTime += elapsedTime;
                long speed = _accumulatedElapsedGameTime.Ticks / _targetElapsedTime.Ticks;
                _accumulatedElapsedGameTime = TimeSpan.FromTicks(_accumulatedElapsedGameTime.Ticks % _targetElapsedTime.Ticks);
                _lastFrameElapsedGameTime = TimeSpan.Zero;

                if (speed == 0L)
                {
                    return;
                }
                else
                {
                    TimeSpan targetElapsedTime = _targetElapsedTime;
                    if (speed > 1L)
                    {
                        _updatesSinceRunningSlowly2 = _updatesSinceRunningSlowly1;
                        _updatesSinceRunningSlowly1 = 0;
                    }
                    else
                    {
                        if (_updatesSinceRunningSlowly1 < int.MaxValue)
                            ++_updatesSinceRunningSlowly1;
                        if (_updatesSinceRunningSlowly2 < int.MaxValue)
                            ++_updatesSinceRunningSlowly2;
                    }
                    _drawRunningSlowly = _updatesSinceRunningSlowly2 < 20;
                    while (speed > 0L && !ShouldExit)
                    {
                        --speed;
                        try
                        {
                            _gameTime.ElapsedGameTime = targetElapsedTime;
                            _gameTime.TotalGameTime = _totalGameTime;
                            _gameTime.IsRunningSlowly = _drawRunningSlowly;
                            Update(_gameTime);
                            draw = draw & _suppressDraw;
                            _suppressDraw = false;
                        }
                        finally
                        {
                            _lastFrameElapsedGameTime += targetElapsedTime;
                            _totalGameTime = totalGameTime;
                        }
                    }
                }

                if (!draw)
                    DrawFrame();
            }
        }

        public void SuppressDraw()
        {
            _suppressDraw = true;
        }

        protected virtual void Update(GameTime gameTime)
        {
            for (int index = 0; index < _updateableComponents.Count; ++index)
                _currentlyUpdatingComponents.Add(_updateableComponents[index]);

            for (int index = 0; index < _currentlyUpdatingComponents.Count; ++index)
            {
                IUpdateable updateable = _currentlyUpdatingComponents[index];
                if (updateable.Enabled)
                    updateable.Update(gameTime);
            }

            _currentlyUpdatingComponents.Clear();
            _doneFirstUpdate = true;
        }

        protected virtual bool BeginDraw()
        {
            return true;
        }

        protected virtual void Draw(GameTime gameTime)
        {
            for (int index = 0; index < _drawableComponents.Count; ++index)
                _currentlyDrawingComponents.Add(_drawableComponents[index]);
            for (int index = 0; index < _currentlyDrawingComponents.Count; ++index)
            {
                IDrawable drawable = _currentlyDrawingComponents[index];
                if (drawable.Visible)
                    drawable.Draw(gameTime);
            }
            _currentlyDrawingComponents.Clear();
        }

        protected virtual void EndDraw()
        {
        }

        private void Paint(object sender, EventArgs e)
        {
            if (_doneFirstDraw)
                DrawFrame();
        }

        protected virtual void Initialize()
        {
            while (_notYetInitialized.Count != 0)
            {
                _notYetInitialized[0].Initialize();
                _notYetInitialized.RemoveAt(0);
            }

            LoadContent();
        }

        public void ResetElapsedTime()
        {
            _forceElapsedTimeToZero = true;
            _drawRunningSlowly = false;
            _updatesSinceRunningSlowly1 = int.MaxValue;
            _updatesSinceRunningSlowly2 = int.MaxValue;
        }

        private void DrawFrame()
        {
            try
            {
                if (!ShouldExit && _doneFirstUpdate && (BeginDraw()))
                {
                    _gameTime.TotalGameTime = _totalGameTime;
                    _gameTime.ElapsedGameTime = _lastFrameElapsedGameTime;
                    _gameTime.IsRunningSlowly = _drawRunningSlowly;
                    Draw(_gameTime);
                    EndDraw();
                    _doneFirstDraw = true;
                }
            }
            finally
            {
                _lastFrameElapsedGameTime = TimeSpan.Zero;
            }
        }

        private void GameComponentRemoved(object sender, GameComponentCollectionEventArgs e)
        {
            if (!_inRun)
                _notYetInitialized.Remove(e.GameComponent);
            IUpdateable updateable = e.GameComponent as IUpdateable;
            if (updateable != null)
            {
                _updateableComponents.Remove(updateable);
                updateable.UpdateOrderChanged -= new EventHandler<EventArgs>(UpdateableUpdateOrderChanged);
            }
            IDrawable drawable = e.GameComponent as IDrawable;
            if (drawable != null)
            {
                _drawableComponents.Remove(drawable);
                drawable.DrawOrderChanged -= new EventHandler<EventArgs>(DrawableDrawOrderChanged);
            }
        }

        private void GameComponentAdded(object sender, GameComponentCollectionEventArgs e)
        {
            if (_inRun)
                e.GameComponent.Initialize();
            else
                _notYetInitialized.Add(e.GameComponent);
            IUpdateable updateable = e.GameComponent as IUpdateable;
            if (updateable != null)
            {
                int num = _updateableComponents.BinarySearch(updateable, (IComparer<IUpdateable>)UpdateOrderComparer.Default);
                if (num < 0)
                {
                    int index = ~num;
                    while (index < _updateableComponents.Count && _updateableComponents[index].UpdateOrder == updateable.UpdateOrder)
                        ++index;
                    _updateableComponents.Insert(index, updateable);
                    updateable.UpdateOrderChanged += new EventHandler<EventArgs>(UpdateableUpdateOrderChanged);
                }
            }
            IDrawable drawable = e.GameComponent as IDrawable;
            if (drawable != null)
            {
                int num = _drawableComponents.BinarySearch(drawable, (IComparer<IDrawable>)DrawOrderComparer.Default);
                if (num < 0)
                {
                    int index = ~num;
                    while (index < _drawableComponents.Count && _drawableComponents[index].DrawOrder == drawable.DrawOrder)
                        ++index;
                    _drawableComponents.Insert(index, drawable);
                    drawable.DrawOrderChanged += new EventHandler<EventArgs>(DrawableDrawOrderChanged);
                }
            }
        }

        private void DrawableDrawOrderChanged(object sender, EventArgs e)
        {
            IDrawable drawable = sender as IDrawable;
            _drawableComponents.Remove(drawable);
            int num = _drawableComponents.BinarySearch(drawable, (IComparer<IDrawable>)DrawOrderComparer.Default);
            if (num < 0)
            {
                int index = ~num;
                while (index < _drawableComponents.Count && _drawableComponents[index].DrawOrder == drawable.DrawOrder)
                    ++index;
                _drawableComponents.Insert(index, drawable);
            }
        }

        private void UpdateableUpdateOrderChanged(object sender, EventArgs e)
        {
            IUpdateable updateable = sender as IUpdateable;
            _updateableComponents.Remove(updateable);
            int num = _updateableComponents.BinarySearch(updateable, (IComparer<IUpdateable>)UpdateOrderComparer.Default);
            if (num < 0)
            {
                int index = ~num;
                while (index < _updateableComponents.Count && _updateableComponents[index].UpdateOrder == updateable.UpdateOrder)
                    ++index;
                _updateableComponents.Insert(index, updateable);
            }
        }

        protected virtual void OnActivated(object sender, EventArgs args)
        {
            if (_activated != null)
                _activated((object)this, args);
        }

        protected virtual void OnDeactivated(object sender, EventArgs args)
        {
            if (_deactivated != null)
                _deactivated((object)this, args);
        }

        protected virtual void OnExiting(object sender, EventArgs args)
        {
            if (_exiting != null)
                _exiting((object)null, args);
        }

        protected virtual void LoadContent()
        {
        }

        protected virtual void UnloadContent()
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
                    IGameComponent[] local_0 = new IGameComponent[_gameComponents.Count];
                    _gameComponents.CopyTo(local_0, 0);
                    for (int local_1 = 0; local_1 < local_0.Length; ++local_1)
                    {
                        IDisposable local_2 = local_0[local_1] as IDisposable;
                        if (local_2 != null)
                            local_2.Dispose();
                    }

                    if (_disposed != null)
                        _disposed((object)this, EventArgs.Empty);
                }
            }
        }
    }
}