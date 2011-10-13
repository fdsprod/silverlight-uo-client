
using System;
using System.IO;
namespace Client.IO
{
    public static class Paths
    {
        public static string StorageFolder
        {
            get
            {
                return Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                    "Silverlight UO Client");
            }
        }

        public static string Logs
        {
            get { return Path.Combine(StorageFolder, "logs"); }
        }

        public static string ConfigFile
        {
            get { return Path.Combine(StorageFolder, "config.xml"); }
        }
    }
}
