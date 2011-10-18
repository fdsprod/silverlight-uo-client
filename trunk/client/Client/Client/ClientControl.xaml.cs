using System.Windows;
using System.Windows.Controls;
using System.Windows.Graphics;

using Client.Configuration;
using Client.Diagnostics;
using Client.Graphics;
using Client.Input;

namespace Client
{
    public partial class ClientControl
    {
        private ClientEngine _game;

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

            IConfigurationService configurationService = new ConfigurationService();
            //IRenderer renderer = new Renderer();

            Tracer.TraceLevel = configurationService.GetValue<TraceLevels>(ConfigSections.Debug, ConfigKeys.DebugLogLevel);
            Tracer.Info("Checking for updates...");

            Application.Current.CheckAndDownloadUpdateCompleted += Current_CheckAndDownloadUpdateCompleted;
            Application.Current.CheckAndDownloadUpdateAsync();

            _game = new ClientEngine(DrawingSurface);
            
            IInputService inputService = new InputService(_game);

            _game.Services.AddService(typeof(IConfigurationService), configurationService);
            _game.Services.AddService(typeof(IInputService), inputService);

            _game.Run();
        }

        void Current_CheckAndDownloadUpdateCompleted(object sender, CheckAndDownloadUpdateCompletedEventArgs e)
        {
            //CheckingForUpdatesPanel.Visibility = Visibility.Collapsed;

            if (e.Error != null)
            {
                Tracer.Error(e.Error);
                MessageBox.Show("An error occurred while updating.");
            }

            if (e.UpdateAvailable)
            {
                Tracer.Info("Update available.");
                //UpdatedPanel.Visibility = Visibility.Visible;
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            //UpdatedPanel.Visibility = Visibility.Collapsed;
        }
    }
}