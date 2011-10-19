using System;
using System.IO;
using System.IO.IsolatedStorage;
using System.Windows;
using Client.Diagnostics;
using Client.IO;

namespace Client
{
    public partial class App
    {
        const long KB = 1024;
        const long MB = KB * 1024;
        const long GB = MB * 1024;

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

            const long twoGB = GB * 4;

            if (store.AvailableFreeSpace < twoGB)
            {
                if (!store.IncreaseQuotaTo(twoGB))
                {

                }
            }

            new DebugTraceListener { TraceLevel = TraceLevels.Verbose };
            new DebugLogTraceListener(Path.Combine(Paths.Logs, "debug.txt"));

            RootVisual = new ClientControl();
        }

        private static void Application_Exit(object sender, EventArgs e)
        {
            Tracer.Info("Exiting...\n\n");
        }

        private static void Application_UnhandledException(object sender, ApplicationUnhandledExceptionEventArgs e)
        {
            if (!System.Diagnostics.Debugger.IsAttached)
            {
                e.Handled = true;
                Deployment.Current.Dispatcher.BeginInvoke(() => ReportErrorToDOM(e));
            }

            string errorMsg = e.ExceptionObject.Message + e.ExceptionObject.StackTrace;
            errorMsg = errorMsg.Replace('"', '\'').Replace("\r\n", @"\n");

            MessageBox.Show(errorMsg);
            Tracer.Error(errorMsg);
        }

        private static void ReportErrorToDOM(ApplicationUnhandledExceptionEventArgs e)
        {
            try
            {
                string errorMsg = e.ExceptionObject.Message + e.ExceptionObject.StackTrace;
                errorMsg = errorMsg.Replace('"', '\'').Replace("\r\n", @"\n");

                System.Windows.Browser.HtmlPage.Window.Eval("throw new Error(\"Unhandled Error in Silverlight Application " + errorMsg + "\");");
            }
            catch (Exception ex)
            {
                Tracer.Error(ex);
            }
        }
    }
}