using System.Windows.Controls;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Client.Input;
using Client.Graphics;
using Client.Ultima;
using Client.Graphics.Shaders;
using System;
using System.Diagnostics;


namespace Client
{
    public class ClientEngine : Engine
    {
        private Camera2D _camera;

        private IInputService _inputService;
        private TextureFactory _textureFactory;
        private SpriteBatch _spriteBatch;
        private SpriteFont _font;
        private DeferredRenderer _renderer;
        private DiffuseShader _shader;

        int _index = 0;

        public ClientEngine(DrawingSurface surface)
            : base(surface)
        {

        }

        protected override void Initialize()
        {
            base.Initialize();

            _camera = new Camera2D(this);
            _camera.NearClip = -1000;
            _camera.FarClip = 1000;
            
            _inputService = (IInputService)Services.GetService(typeof(IInputService));
            _inputService.MouseMove += _inputService_MouseMove;
            _inputService.KeyDown += _inputService_KeyDown;

            _textureFactory = new TextureFactory(this);

            _spriteBatch = new SpriteBatch(GraphicsDevice);
            _font = Content.Load<SpriteFont>("Fonts\\DebugFont");

            _shader = new DiffuseShader(this);
            _renderer = new DeferredRenderer(this);
        }

        void _inputService_KeyDown(object sender, KeyStateEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Up)
                _y--;
            if (e.Key == System.Windows.Input.Key.Down)
                _y++;
            if (e.Key == System.Windows.Input.Key.Left)
                _x--;
            if (e.Key == System.Windows.Input.Key.Right)
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

        protected override void Update(UpdateState state)
        {
            base.Update(state);
        }

        protected override void Draw(DrawState state)
        {
            base.Draw(state);

            DrawScene(state);
            //DrawUI(state);
        }

        int _x = 187;
        int _y = 203;

        private void DrawScene(DrawState state)
        {
            state.GraphicsDevice.Clear(ClearOptions.Target | ClearOptions.DepthBuffer, new Color(0.2f, 0.2f, 0.2f, 1.0f), 1.0f, 0);
            
            _shader.WorldViewProjection = _camera.View * _camera.Projection;
            _shader.Apply();



            DrawBlock(state, _x, _y);
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

        private void DrawUI(DrawState state)
        {

        }
    }
}