using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Graphics;

namespace Client
{
    public partial class ClientControl
    {
        private ClientGame _game;

        public ClientControl()
        {
            InitializeComponent();

            _game = new ClientGame(DrawingSurface);
            _game.Run();
        }

        private void OnDraw(object sender, DrawEventArgs e)
        {
            _game.Tick(e.DeltaTime, e.TotalTime);
            e.InvalidateSurface();
        }

        private void OnLoad(object sender, RoutedEventArgs e)
        {
            if(GraphicsDeviceManager.Current.RenderMode != RenderMode.Hardware)
                MessageBox.Show("Please activate enableGPUAcceleration=true on your Silverlight plugin page.", "Warning", MessageBoxButton.OK);
        }
    }
}