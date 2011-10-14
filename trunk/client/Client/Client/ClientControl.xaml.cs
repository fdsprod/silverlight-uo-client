using System.Windows;
using System.Windows.Controls;
using System.Windows.Graphics;

using Client.Diagnostics;

namespace Client
{
    public partial class ClientControl
    {
        private ClientGame _game;

        public ClientControl()
        {
            InitializeComponent();
        }

        private void OnDraw(object sender, DrawEventArgs e)
        {
            _game.Tick(e.DeltaTime, e.TotalTime);
            e.InvalidateSurface();
        }

        private void OnLoad(object sender, RoutedEventArgs e)
        {
            if (GraphicsDeviceManager.Current.RenderMode != RenderMode.Hardware)
            {
                MessageBox.Show("Please activate enableGPUAcceleration=true on your Silverlight plugin page.", "Warning", MessageBoxButton.OK);
                return;
            }

            Tracer.Verbose("Checking for updates...");

            Application.Current.CheckAndDownloadUpdateCompleted += Current_CheckAndDownloadUpdateCompleted;
            Application.Current.CheckAndDownloadUpdateAsync();

            _game = new ClientGame(DrawingSurface);
            _game.Run();
        }

        void Current_CheckAndDownloadUpdateCompleted(object sender, CheckAndDownloadUpdateCompletedEventArgs e)
        {
            CheckingForUpdatesPanel.Visibility = Visibility.Collapsed;

            if (e.Error != null)
            {
                Tracer.Error(e.Error);
                MessageBox.Show("An error occurred while updating.");
            }

            if (e.UpdateAvailable)
            {
                Tracer.Verbose("Update available.");
                UpdatedPanel.Visibility = Visibility.Visible;
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            UpdatedPanel.Visibility = Visibility.Collapsed;
        }
    }
}