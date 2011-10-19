using System;
using System.Windows.Controls;
using System.Windows.Graphics;
using System.Windows.Input;

using Client.Diagnostics;
using Client.Graphics;
using Client.Graphics.Shaders;
using Client.Input;
using Client.Ultima;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Client
{
    public class Engine
    {
        public readonly TimeSpan TargetElapsedTime = TimeSpan.FromTicks(TimeSpan.TicksPerSecond / 60);
        public readonly TimeSpan MaxElapsedTime = TimeSpan.FromTicks(TimeSpan.TicksPerSecond / 10);

        private readonly GameServiceContainer _gameServices;
        private readonly DrawState _drawState;
        private readonly UpdateState _updateState;
        private readonly DrawingSurface _drawingSurface;

        private int _updatesSinceRunningSlowly1 = int.MaxValue;
        private int _updatesSinceRunningSlowly2 = int.MaxValue;

        private bool _drawRunningSlowly;
        private bool _doneFirstUpdate;
        private bool _forceElapsedTimeToZero;
        private bool _suppressDraw;
        private bool _shouldStop;

        private TimeSpan _totalGameTime;
        private TimeSpan _targetElapsedTime;
        private TimeSpan _accumulatedElapsedGameTime;
        private TimeSpan _lastFrameElapsedGameTime;

        private Camera2D _camera;
        private IInputService _inputService;
        private TextureFactory _textureFactory;
        private DeferredRenderer _renderer;
        private DiffuseShader _shader;
        private ContentManager _content;

        public DrawingSurface DrawingSurface
        {
            get { return _drawingSurface; }
        }

        public GameServiceContainer Services
        {
            get { return _gameServices; }
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

        public Control RootControl
        {
            get;
            private set;
        }

        public Engine(DrawingSurface drawingSurface)
        {
            RootControl = (Control)drawingSurface.Parent;

            Asserter.AssertIsNotNull(RootControl, "RootControl");
            Asserter.AssertIsNotNull(drawingSurface, "drawingSurface");

            _drawingSurface = drawingSurface;
            _content = new ContentManager(_gameServices);
            _content.RootDirectory = "Content";
            _totalGameTime = TimeSpan.Zero;
            _accumulatedElapsedGameTime = TimeSpan.Zero;
            _lastFrameElapsedGameTime = TimeSpan.Zero;
            _targetElapsedTime = TimeSpan.FromTicks(166667L);
            _drawState = new DrawState();
            _updateState = new UpdateState();
            _gameServices = new GameServiceContainer();
        }

        ~Engine()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public void ResetElapsedTime()
        {
            Tracer.Info("ResetElapsedTime was called");

            _forceElapsedTimeToZero = true;
            _drawRunningSlowly = false;
            _updatesSinceRunningSlowly1 = int.MaxValue;
            _updatesSinceRunningSlowly2 = int.MaxValue;
        }

        public void Run()
        {
            try
            {
                Tracer.Info("Initializing Engine...");
                Initialize();

                _updateState.ElapsedGameTime = TimeSpan.Zero;
                _updateState.TotalGameTime = _totalGameTime;
                _updateState.IsRunningSlowly = false;

                Update(_updateState);

                _doneFirstUpdate = true;
            }
            catch (Exception e)
            {
                Tracer.Error(e);
                throw;
            }
        }

        public void Tick(TimeSpan elapsedTime, TimeSpan totalGameTime)
        {
            if (_shouldStop)
                return;

            //if (_drawState.TotalGameTime > TimeSpan.FromSeconds(2))
            //    throw new Exception();

            bool suppressDraw = true;

            if (elapsedTime < TimeSpan.Zero)
                elapsedTime = TimeSpan.Zero;

            if (_forceElapsedTimeToZero)
            {
                elapsedTime = TimeSpan.Zero;
                _forceElapsedTimeToZero = false;
            }

            if (elapsedTime > MaxElapsedTime)
                elapsedTime = MaxElapsedTime;

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
                    _updateState.ElapsedGameTime = targetElapsedTime;
                    _updateState.TotalGameTime = _totalGameTime;
                    _updateState.IsRunningSlowly = _drawRunningSlowly;
                    Update(_updateState);
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

        private void Initialize()
        {
            _camera = new Camera2D(this);
            _camera.NearClip = -1000;
            _camera.FarClip = 1000;

            _inputService = (IInputService)Services.GetService(typeof(IInputService));
            _inputService.MouseMove += _inputService_MouseMove;
            _inputService.KeyDown += _inputService_KeyDown;

            _textureFactory = new TextureFactory(this);

            _shader = new DiffuseShader(this);
            _renderer = new DeferredRenderer(this);

            Tracer.Info("Loading Content...");
            LoadContent();
        }

        int _x = 187;
        int _y = 203;

        void _inputService_KeyDown(object sender, KeyStateEventArgs e)
        {
            if (e.Key == Key.Up)
                _y--;
            if (e.Key == Key.Down)
                _y++;
            if (e.Key == Key.Left)
                _x--;
            if (e.Key == Key.Right)
                _x++;

            _y = Math.Max(0, _y);
            _y = Math.Min(400, _y);

            _x = Math.Max(0, _x);
            _x = Math.Min(700, _x);
        }

        void _inputService_MouseMove(object sender, MouseStateEventArgs e)
        {
            if (_inputService.IsMouseDown(MouseButton.Left))
                _camera.Position += e.PositionDelta;
        }

        private void Update(UpdateState state)
        {

        }

        private bool BeginDraw()
        {
            return true;
        }

        private void Draw(DrawState state)
        {
            state.GraphicsDevice.Clear(ClearOptions.Target | ClearOptions.DepthBuffer, new Color(0.2f, 0.2f, 0.2f, 1.0f), 1.0f, 0);

            state.PushView(_camera.View);
            state.PushProjection(_camera.Projection);

            _shader.Bind(state);

            DrawBlock(state, _x, _y);

            state.PopView();
            state.PopProjection();
        }

        private void DrawBlock(DrawState state, int bx, int by)
        {
            Tile[] tiles = new Maps(this).Felucca.Tiles.GetLandBlock(bx, by);

            const int size = 8 * 44;
            const int sizeOver2 = size / 2;

            for (int y = 0; y < 8; y++)
            {
                for (int x = 0; x < 8; x++)
                {
                    Tile tile = tiles[(y * 8) + x];

                    Vector2 v1 = new Vector2(-sizeOver2 + x * 44, -sizeOver2 + y * 44 + tile._z);
                    Vector2 v2 = new Vector2(v1.X + 44, v1.Y + 44);

                    state.GraphicsDevice.Textures[0] = _textureFactory.CreateLand(tile._id);
                    _renderer.DrawQuad(v1, v2);
                }
            }

        }

        private void EndDraw()
        {

        }

        private void LoadContent()
        {

        }

        private void UnloadContent()
        {

        }

        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                lock (this)
                {
                    Tracer.Info("Unloading Content...");
                    UnloadContent();
                }
            }
        }

        private void DrawFrame()
        {
            try
            {
                if (_doneFirstUpdate && (BeginDraw()))
                {
                    _drawState.TotalGameTime = _totalGameTime;
                    _drawState.ElapsedGameTime = _lastFrameElapsedGameTime;
                    _drawState.IsRunningSlowly = _drawRunningSlowly;
                    _drawState.GraphicsDevice = GraphicsDevice;
                    _drawState.Camera = _camera;
                    _drawState.Reset();

                    Draw(_drawState);
                    EndDraw();
                }
            }
            finally
            {
                _lastFrameElapsedGameTime = TimeSpan.Zero;
            }
        }

        internal void Stop()
        {
            _shouldStop = true;
        }
    }
}