using System.Windows;
using System.Windows.Controls;
using System.Windows.Graphics;
using System.Diagnostics;

namespace Client
{
    public partial class ClientControl
    {
        private readonly ClientGame _game;

        public ClientControl()
        {
            InitializeComponent();

            _game = new ClientGame(myDrawingSurface);
            _game.Run();
        }

        private void OnDraw(object sender, DrawEventArgs e)
        {
            _game.Tick(e.DeltaTime, e.TotalTime);
            e.InvalidateSurface();

            Debug.WriteLine(GraphicsDeviceManager.Current.GraphicsDevice.PresentationParameters.BackBufferWidth);
        }

        private void OnLoad(object sender, RoutedEventArgs e)
        {
            if (GraphicsDeviceManager.Current.RenderMode != RenderMode.Hardware)
            {
                MessageBox.Show("Please activate enableGPUAcceleration=true on your Silverlight plugin page.", "Warning", MessageBoxButton.OK);
            }
        }
    }
}