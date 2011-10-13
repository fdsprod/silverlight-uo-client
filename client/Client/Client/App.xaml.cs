using System;
using System.IO;
using System.Windows;
using Client.Diagnostics;

namespace Client
{
    public partial class App
    {
        public App()
        {
            Startup += Application_Startup;
            Exit += Application_Exit;
            UnhandledException += Application_UnhandledException;

            InitializeComponent();
        }

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            string myDocumentsDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            string loggingFolder = Path.Combine(myDocumentsDirectory, "Silverlight UO Client\\logs");
            string debugLogPath = Path.Combine(loggingFolder, "debug.log");

            new DebugTraceListener();
            new DebugLogTraceListener(debugLogPath);

            RootVisual = new ClientControl();
        }

        private static void Application_Exit(object sender, EventArgs e)
        {
        }

        private static void Application_UnhandledException(object sender, ApplicationUnhandledExceptionEventArgs e)
        {
            if (!System.Diagnostics.Debugger.IsAttached)
            {
                e.Handled = true;
                Deployment.Current.Dispatcher.BeginInvoke(() => ReportErrorToDOM(e));
            }
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