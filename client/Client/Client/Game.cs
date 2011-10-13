using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Graphics;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Client.Diagnostics;
using Microsoft.Xna.Framework.Input;

namespace Client
{
    public class Game
    {
        private readonly TimeSpan _maximumElapsedTime = TimeSpan.FromMilliseconds(500.0);

        private int _updatesSinceRunningSlowly1 = int.MaxValue;
        private int _updatesSinceRunningSlowly2 = int.MaxValue;

        private bool _drawRunningSlowly;
        private bool _doneFirstUpdate;
        private bool _forceElapsedTimeToZero;
        private bool _suppressDraw;

        private TimeSpan _totalGameTime;
        private TimeSpan _targetElapsedTime;
        private TimeSpan _accumulatedElapsedGameTime;
        private TimeSpan _lastFrameElapsedGameTime;

        private GameServiceContainer _gameServices;
        private ContentManager _content;
        private GameTime _gameTime;
        private DrawingSurface _drawingSurface;

        public DrawingSurface DrawingSurface
        {
            get { return _drawingSurface; }
        }

        public GameServiceContainer Services
        {
            get { return _gameServices; }
        }

        public TimeSpan TargetElapsedTime
        {
            get { return _targetElapsedTime; }
            set
            {
                Asserter.AssertIsNotLessThan("value", value, TimeSpan.Zero);
                _targetElapsedTime = value;
            }
        }
        
        public GraphicsDevice GraphicsDevice
        {
            get { return GraphicsDeviceManager.Current.GraphicsDevice; }
        }

        public ContentManager Content
        {
            get { return _content; }
            set
            {
                Asserter.AssertIsNotNull(value, "value");
                _content = value;
            }
        }

        public Game(DrawingSurface drawingSurface)
        {
            Asserter.AssertIsNotNull(drawingSurface, "drawingSurface");

            _drawingSurface = drawingSurface;
            _drawingSurface.SizeChanged += OnDrawingSurfaceSizeChanged;
            _content = new ContentManager((IServiceProvider)_gameServices);
            _totalGameTime = TimeSpan.Zero;
            _accumulatedElapsedGameTime = TimeSpan.Zero;
            _lastFrameElapsedGameTime = TimeSpan.Zero;
            _targetElapsedTime = TimeSpan.FromTicks(166667L);
            _gameTime = new GameTime();
            _gameServices = new GameServiceContainer();

            Keyboard.RootControl = (Control)drawingSurface.Parent;
        }

        ~Game()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize((object)this);
        }

        public void ResetElapsedTime()
        {
            _forceElapsedTimeToZero = true;
            _drawRunningSlowly = false;
            _updatesSinceRunningSlowly1 = int.MaxValue;
            _updatesSinceRunningSlowly2 = int.MaxValue;
        }

        public void Run()
        {
            try
            {
                Tracer.Verbose("Initializing Engine...");
                Initialize();

                _gameTime.ElapsedGameTime = TimeSpan.Zero;
                _gameTime.TotalGameTime = _totalGameTime;
                _gameTime.IsRunningSlowly = false;

                Update(_gameTime);

                _doneFirstUpdate = true;
            }
            //TODO: Add catch here.
            finally
            {                    

            }
        }

        public void Tick(TimeSpan elapsedTime, TimeSpan totalGameTime)
        {
            bool suppressDraw = true;

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

            while (speed > 0L)
            {
                --speed;
                try
                {
                    _gameTime.ElapsedGameTime = targetElapsedTime;
                    _gameTime.TotalGameTime = _totalGameTime;
                    _gameTime.IsRunningSlowly = _drawRunningSlowly;
                    Update(_gameTime);
                    suppressDraw = suppressDraw & _suppressDraw;
                    _suppressDraw = false;
                }
                finally
                {
                    _lastFrameElapsedGameTime += targetElapsedTime;
                    _totalGameTime = totalGameTime;
                }
            }

            if (!suppressDraw)
                DrawFrame();
        }

        protected virtual void Update(GameTime gameTime)
        {
            _doneFirstUpdate = true;
        }

        protected virtual bool BeginDraw()
        {
            return true;
        }

        protected virtual void Draw(GameTime gameTime)
        {
        }

        protected virtual void EndDraw() { }

        protected virtual void Initialize()
        {
            Tracer.Verbose("Loading Content...");
            LoadContent();
        }

        protected virtual void OnDrawingSurfaceSizeChanged(object sender, SizeChangedEventArgs e)
        {

        }

        protected virtual void LoadContent()
        {
        }

        protected virtual void UnloadContent()
        {
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                lock (this)
                {
                    Tracer.Verbose("Unloading Content...");
                    UnloadContent();

                    if (_drawingSurface != null)
                        _drawingSurface.SizeChanged -= OnDrawingSurfaceSizeChanged;
                }
            }
        }

        private void DrawFrame()
        {
            try
            {
                if (_doneFirstUpdate && (BeginDraw()))
                {
                    _gameTime.TotalGameTime = _totalGameTime;
                    _gameTime.ElapsedGameTime = _lastFrameElapsedGameTime;
                    _gameTime.IsRunningSlowly = _drawRunningSlowly;

                    Draw(_gameTime);
                    EndDraw();
                }
            }
            finally
            {
                _lastFrameElapsedGameTime = TimeSpan.Zero;
            }
        }
    }
}