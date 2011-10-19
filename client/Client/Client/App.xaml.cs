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
        const long KB = 1024;
        const long MB = KB * 1024;
        const long GB = MB * 1024;

        private static Grid _root;
        private static ClientControl _clientControl;

        public App()
        {
            Startup += Application_Startup;
            Exit += Application_Exit;
            UnhandledException += Application_UnhandledException;

            InitializeComponent();
        }

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            var store = IsolatedStorageFile.GetUserStoreForApplication();

            if (store.Quota < GB)
            {
                if (!store.IncreaseQuotaTo(GB))
                {
                    MessageBox.Show("Silverlight UO Client cannot continue until more storage is made available.");
                    return;
                }
            }
            else if (store.AvailableFreeSpace < 200 * MB)
            {
                if (!store.IncreaseQuotaTo(store.Quota + GB))
                {
                    MessageBox.Show("Silverlight UO Client may have trouble running due to limitted available storage.");
                }
            }
            
            Application.Current.Host.Settings.EnableFrameRateCounter = true;
            Application.Current.Host.Settings.MaxFrameRate = int.MaxValue;

            new DebugTraceListener { TraceLevel = TraceLevels.Verbose };
            new DebugLogTraceListener(Path.Combine(Paths.LogsDirectory, "debug.txt"));

            _root = new Grid();
            _clientControl = new ClientControl();
            _root.Children.Add(_clientControl);

            RootVisual = _root;
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

            _root.Children.Clear();
            _root.Children.Add(control);
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