using System;
using System.Windows.Controls;
using System.Windows.Graphics;
using Client.Framework;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Client.Network;

namespace Client
{
    public class Scene : IDisposable
    {
        readonly DrawingSurface _drawingSurface;
        readonly ContentManager contentManager;
        readonly Cube cube;

        float aspectRatio;
        float rotationAngle;
        
        public ContentManager ContentManager
        {
            get
            {
                return contentManager;
            }
        }

        public GraphicsDevice GraphicsDevice
        {
            get
            {
                return GraphicsDeviceManager.Current.GraphicsDevice;
            }
        }

        public Scene(DrawingSurface drawingSurface)
        {
            _drawingSurface = drawingSurface;

            // Register for size changed to update the aspect ratio
            _drawingSurface.SizeChanged += _drawingSurface_SizeChanged;

            // Get a content manager to access content pipeline
            contentManager = new ContentManager(null)
            {
                RootDirectory = "Content"
            };

            // Initializing variables
            cube = new Cube(this, 1.0f);
                        
            NetState nc = new NetState();
            nc.Connect("127.0.0.1", 2593);
        }
        
        private void _drawingSurface_SizeChanged(object sender, System.Windows.SizeChangedEventArgs e)
        {
            aspectRatio = (float)(_drawingSurface.ActualWidth / _drawingSurface.ActualHeight);
        }

        public void Draw(GameTime gameTime)
        {
            // Clear surface
            GraphicsDeviceManager.Current.GraphicsDevice.Clear(ClearOptions.Target | ClearOptions.DepthBuffer, new Color(0.2f, 0.2f, 0.2f, 1.0f), 1.0f, 0);

            // Compute matrices
            Matrix world = Matrix.CreateRotationX(rotationAngle) * Matrix.CreateRotationY(rotationAngle);
            Matrix view = Matrix.CreateLookAt(new Vector3(0, 0, -5.0f), Vector3.Zero, Vector3.UnitY);
            Matrix projection = Matrix.CreatePerspectiveFieldOfView(0.85f, aspectRatio, 0.01f, 1000.0f);

            // Update per frame parameter values
            cube.World = world;
            cube.WorldViewProjection = world * view * projection;

            // Drawing the cube
            cube.Draw();

            // Animate rotation
            rotationAngle += (float)gameTime.ElapsedGameTime.TotalSeconds;
        }

        public void Dispose()
        {
            _drawingSurface.SizeChanged -= _drawingSurface_SizeChanged;
        }
    }
}