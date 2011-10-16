using System;
using System.Windows.Controls;
using System.Windows.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Client.Graphics;
using Client.Input;

namespace Client
{
    public class Scene : IDisposable
    {
        private Camera2D _camera;

        readonly IInputService _inputService;
        readonly ContentManager _contentManager;
        readonly Cube _cube;
        
        float _rotationAngle;

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

            _cube = new Cube(this, 1.0f);
            _cube.Texture = _contentManager.Load<Texture2D>("Textures\\missing-texture");
        }

        void _inputService_MouseMove(object sender, MouseStateEventArgs e)
        {
            if(_inputService.IsMouseDown(MouseButton.Left))
                _camera.Position += e.PositionDelta;
        }

        public void Update(GameTime gameTime)
        {
        }

        public void Draw(GameTime gameTime)
        {
            GraphicsDeviceManager.Current.GraphicsDevice.Clear(ClearOptions.Target | ClearOptions.DepthBuffer, new Color(0.2f, 0.2f, 0.2f, 1.0f), 1.0f, 0);
        }

        public void Dispose()
        {

        }
    }
}