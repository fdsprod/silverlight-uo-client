using System;
using System.Windows.Controls;
using System.Windows.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Client.Graphics;
using Client.Input;
using Client.Ultima;

namespace Client
{
    public class Scene : IDisposable
    {
        private Camera2D _camera;

        readonly IInputService _inputService;
        readonly ContentManager _contentManager;
        readonly Cube _cube1;
        readonly Cube _cube2;
        readonly Textures _textures;
        private SpriteBatch _spriteBatch;
        private SpriteFont _font;


        bool _updateTexture;
        int _index = 0;

        public ContentManager ContentManager
        {
            get { return _contentManager; }
        }

        public GraphicsDevice GraphicsDevice
        {
            get { return GraphicsDeviceManager.Current.GraphicsDevice; }
        }

        public Scene(Engine engine)
        {
            _camera = new Camera2D(engine);
            _camera.NearClip = -1000;
            _camera.FarClip = 1000;
            
            _contentManager = new ContentManager(null)
            {
                RootDirectory = "Content"
            };

            _inputService = (IInputService)engine.Services.GetService(typeof(IInputService));
            _inputService.MouseMove += new EventHandler<MouseStateEventArgs>(_inputService_MouseMove);
            _inputService.KeyDown += new EventHandler<KeyStateEventArgs>(_inputService_KeyDown);

            _textures = new Textures(engine);
            Texture2D d, n;
            _textures.CreateTexture(_index, out d, out n);

            _cube1 = new Cube(this, 1.0f);
            _cube2 = new Cube(this, 1.0f);

            _cube1.Texture = d;// _contentManager.Load<Texture2D>("Textures\\missing-texture");
            _cube2.Texture = n;// _contentManager.Load<Texture2D>("Textures\\missing-texture");

            _spriteBatch = new SpriteBatch(GraphicsDevice);
            _font = _contentManager.Load<SpriteFont>("Fonts\\DebugFont");
        }

        void _inputService_KeyDown(object sender, KeyStateEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Up)
                _index++;
            if (e.Key == System.Windows.Input.Key.Down)
                _index--;

            _updateTexture = true;

            _index = Math.Max(0, _index);
            _index = Math.Min(500, _index);
        }

        void _inputService_MouseMove(object sender, MouseStateEventArgs e)
        {
            if(_inputService.IsMouseDown(MouseButton.Left))
                _camera.Position += e.PositionDelta;
        }

        public void Update(GameTime gameTime)
        {
            if (_updateTexture)
            {
                _updateTexture = false;
                Texture2D d, n;
                _textures.CreateTexture(_index, out d, out n);
                _cube1.Texture = d;
                _cube2.Texture = n;
            }
        }

        public void Draw(GameTime gameTime)
        {
            DrawScene(gameTime);
            DrawUI(gameTime);
        }

        public void EndDraw()
        {

        }

        private void DrawScene(GameTime gameTime)
        {
            GraphicsDeviceManager.Current.GraphicsDevice.Clear(ClearOptions.Target | ClearOptions.DepthBuffer, new Color(0.2f, 0.2f, 0.2f, 1.0f), 1.0f, 0);

            Matrix world1 = Matrix.CreateScale(300) * Matrix.CreateTranslation(Vector3.Left * 200);
            Matrix world2 = Matrix.CreateScale(300) * Matrix.CreateTranslation(Vector3.Right * 200);
            Matrix view = _camera.Transform;
            Matrix projection = _camera.Projection;

            _cube1.WorldViewProjection = world1 * view * projection;
            _cube1.Draw();

            _cube2.WorldViewProjection = world2 * view * projection;
            _cube2.Draw();

            _spriteBatch.Begin();
            _spriteBatch.DrawString(_font, "Press Up/Down arrows to cycle through the textures", new Vector2(24, 24), Color.Black);
            _spriteBatch.End();
        }

        private void DrawUI(GameTime gameTime)
        {

        }

        public void Dispose()
        {

        }
    }
}