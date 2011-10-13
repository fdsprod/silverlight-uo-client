using System.Windows;
using System.Windows.Controls;
using System.Windows.Graphics;
using System.Diagnostics;
using Microsoft.Xna.Framework.Graphics;
using Client.Ultima;

namespace Client
{
    public partial class ClientControl
    {
        ClientGame game;

        public ClientControl()
        {
            InitializeComponent();

            game = new ClientGame(myDrawingSurface);
            game.Run();
        }

        private void OnDraw(object sender, DrawEventArgs e)
        {
            game.Tick(e.DeltaTime, e.TotalTime);
            e.InvalidateSurface();
        }

        private void OnLoad(object sender, RoutedEventArgs e)
        {
            // Check if GPU is on
            if (GraphicsDeviceManager.Current.RenderMode != RenderMode.Hardware)
            {
                MessageBox.Show("Please activate enableGPUAcceleration=true on your Silverlight plugin page.", "Warning", MessageBoxButton.OK);
            }
        }
    }
}