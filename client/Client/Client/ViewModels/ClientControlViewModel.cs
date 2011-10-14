using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Client.ComponentModel.Design;

namespace Client.ViewModels
{
    public class ClientControlViewModel : ViewModelBase
    {
        public bool IsRunningOutOfBrowser
        {
            get { return Get<bool>(() => IsRunningOutOfBrowser); }
            set { Set<bool>(() => IsRunningOutOfBrowser, value); }
        }

        public bool IsInstalled
        {
            get { return Get<bool>(() => IsInstalled); }
            set { Set<bool>(() => IsInstalled, value); }
        }

        public bool IsBusy
        {
            get { return string.IsNullOrEmpty(BusyMessage); }
        }

        public string BusyMessage
        {
            get { return Get(() => BusyMessage); }
            set 
            {
                if (Set(() => BusyMessage, value))
                    RaisePropertyChanged(() => IsBusy);
            }
        }

        public ICommand InstallCommand
        {
            get;
            private set;
        }

        public ClientControlViewModel()
        {
            IsInstalled = Application.Current.InstallState == InstallState.Installed;
            IsRunningOutOfBrowser = Application.Current.IsRunningOutOfBrowser;
            InstallCommand = CreateCommand(OnInstallClick);

            if (IsRunningOutOfBrowser)
            {
                Application.Current.CheckAndDownloadUpdateCompleted += new CheckAndDownloadUpdateCompletedEventHandler(Current_CheckAndDownloadUpdateCompleted);
                Application.Current.CheckAndDownloadUpdateAsync();
            }
        }

        void Current_CheckAndDownloadUpdateCompleted(object sender, CheckAndDownloadUpdateCompletedEventArgs e)
        {
            if(e.UpdateAvailable)
                MessageBox.Show("The client has been updated.  Please restart.");
        }

        private void OnInstallClick()
        {
            if (Application.Current.InstallState == InstallState.NotInstalled)
            {
                Application.Current.Install();
            }
        }
    }
}
