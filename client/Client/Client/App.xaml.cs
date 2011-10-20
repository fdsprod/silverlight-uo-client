using System;
using System.IO;
using System.IO.IsolatedStorage;
using System.Windows;
using Client.Diagnostics;
using Client.IO;
using System.Windows.Controls;
using System.Text;
using System.Reflection;

namespace Client
{
    public partial class App
    {
        private static ContentHost _gameHost;
        private static GameHost _clientControl;

        public App()
        {
            Startup += Application_Startup;
            Exit += Application_Exit;
            UnhandledException += Application_UnhandledException;

            InitializeComponent();
        }

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            Application.Current.CheckAndDownloadUpdateCompleted += new CheckAndDownloadUpdateCompletedEventHandler(Current_CheckAndDownloadUpdateCompleted);
            Application.Current.CheckAndDownloadUpdateAsync();

            Application.Current.Host.Settings.EnableFrameRateCounter = true;
            Application.Current.Host.Settings.MaxFrameRate = int.MaxValue;

            new DebugTraceListener { TraceLevel = TraceLevels.Verbose };
            new DebugLogTraceListener(Path.Combine(Paths.LogsDirectory, "debug.txt"));

            _gameHost = new ContentHost();
            _clientControl = new GameHost();
            _gameHost.LayoutRoot.Children.Add(_clientControl);

            RootVisual = _gameHost;
        }

        void Current_CheckAndDownloadUpdateCompleted(object sender, CheckAndDownloadUpdateCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                MessageBox.Show(e.Error.ToString());


            }
        }

        private static void Application_Exit(object sender, EventArgs e)
        {
            Tracer.Info("Exiting...\n\n");
        }

        private static void Application_UnhandledException(object sender, ApplicationUnhandledExceptionEventArgs e)
        {
            string errorMsg = e.ExceptionObject.Message + e.ExceptionObject.StackTrace;
            errorMsg = errorMsg.Replace('"', '\'').Replace("\r\n", @"\n");

            Tracer.Error(errorMsg);
            e.Handled = true;

            var control = new ExceptionControl();
            var viewModel = new ExceptionViewModel();

            viewModel.Exception = e.ExceptionObject.ToString();
            control.DataContext = viewModel;

            _gameHost.LayoutRoot.Children.Clear();
            _gameHost.LayoutRoot.Children.Add(control);
        }

        private static string GenerateCrashReport(Exception e)
        {
            try
            {
                StringBuilder sb = new StringBuilder(); 


                sb.AppendLine("Server Crash Report");
                sb.AppendLine("===================");
                sb.AppendLine();
                //sb.AppendFormat("Silverlight UO Client Version {0}.{1}, Build {2}.{3}\n", version.Major, version.Minor, version.Build, version.Revision);
                sb.AppendFormat("Operating System: {0}\n", Environment.OSVersion);
                sb.AppendFormat("Silverlight Framework: {0}\n", Environment.Version);
                sb.AppendFormat("Time: {0}\n", DateTime.Now);
                sb.AppendFormat("HasElevatedPermissions: {0}", Application.Current.HasElevatedPermissions);
                sb.AppendFormat("IsRunningOutOfBrowser: {0}", Application.Current.IsRunningOutOfBrowser);
                //sb.AppendFormat("IsRunningOutOfBrowser: {0}",);                
                sb.AppendLine("Exception:");
                sb.AppendLine(e.ToString());
                sb.AppendLine();

                return sb.ToString();
            }
            catch
            {
                return e.ToString();
            }
        }
    }
}