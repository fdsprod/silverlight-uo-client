using System;
using System.Windows.Controls;
using System.Windows.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Client
{
    public class Scene : IDisposable
    {
        readonly DrawingSurface _drawingSurface;
        readonly ContentManager _contentManager;
        readonly Cube _cube;

        float _aspectRatio;
        float _rotationAngle;

        int _index;

        KeyboardState _previousState;

        public ContentManager ContentManager
        {
            get { return _contentManager; }
        }

        public GraphicsDevice GraphicsDevice
        {
            get { return GraphicsDeviceManager.Current.GraphicsDevice; }
        }

        public Scene(DrawingSurface drawingSurface)
        {
            _drawingSurface = drawingSurface;

            // Register for size changed to update the aspect ratio
            _drawingSurface.SizeChanged += _drawingSurface_SizeChanged;

            // Get a content manager to access content pipeline
            _contentManager = new ContentManager(null)
            {
                RootDirectory = "Content"
            };

            // Initializing variables
            _cube = new Cube(this, 1.0f);
            _cube.Texture = _contentManager.Load<Texture2D>("Textures\\missing-texture");
        }

        public void Update(GameTime gameTime)
        {
            int i = _index;
            //KeyboardState currentState = Keyboard.GetState();

            //if (_previousState.IsKeyUp(System.Windows.Input.Key.Down) &&
            //    currentState.IsKeyDown(System.Windows.Input.Key.Down))
            //{
            //    _index--;
            //}
            //if (_previousState.IsKeyUp(System.Windows.Input.Key.Up) &&
            //    currentState.IsKeyDown(System.Windows.Input.Key.Up))
            //{
            //    _index++;
            //}

            //_index = Math.Min(0x1000, _index);
            //_index = Math.Max(0, _index);

            //if (_index != i)
            //{
            //    _cube.Texture = Textures.CreateTexture(_index);
            //}

            //_previousState = currentState;
        }

        private void _drawingSurface_SizeChanged(object sender, System.Windows.SizeChangedEventArgs e)
        {
            _aspectRatio = (float)(_drawingSurface.ActualWidth / _drawingSurface.ActualHeight);
        }

        public void Draw(GameTime gameTime)
        {
            GraphicsDeviceManager.Current.GraphicsDevice.Clear(ClearOptions.Target | ClearOptions.DepthBuffer, new Color(0.2f, 0.2f, 0.2f, 1.0f), 1.0f, 0);

            Matrix world = Matrix.CreateRotationX(_rotationAngle) * Matrix.CreateRotationY(_rotationAngle);
            Matrix view = Matrix.CreateLookAt(new Vector3(0, 0, -5.0f), Vector3.Zero, Vector3.UnitY);
            Matrix projection = Matrix.CreatePerspectiveFieldOfView(0.85f, _aspectRatio, 0.01f, 1000.0f);

            _cube.World = world;
            _cube.WorldViewProjection = world * view * projection;

            _cube.Draw();

            _rotationAngle += (float)gameTime.ElapsedGameTime.TotalSeconds;
        }

        public void Dispose()
        {
            _drawingSurface.SizeChanged -= _drawingSurface_SizeChanged;
        }
    }
}