
using System;
using System.IO;
using System.Windows;
namespace Client.IO
{
    public static class Paths
    {
        public static string StorageFolder
        {
            get
            {
                if(!Application.Current.IsRunningOutOfBrowser)
                    return string.Empty;

                return Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                    "Silverlight UO Client");
            }
        }

        public static string Logs
        {
            get
            {
                if (!Application.Current.IsRunningOutOfBrowser)
                    return string.Empty; 
                
                return Path.Combine(StorageFolder, "logs");
            }
        }

        public static string ConfigFile
        {
            get
            {
                if (!Application.Current.IsRunningOutOfBrowser)
                    return string.Empty;
                
                return Path.Combine(StorageFolder, "config.xml");
            }
        }
    }
}
