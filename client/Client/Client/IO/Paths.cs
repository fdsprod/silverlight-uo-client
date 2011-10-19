
using System;
using System.IO;
using System.IO.IsolatedStorage;
namespace Client.IO
{
    public static class Paths
    {
        public static string LogsDirectory
        {
            get
            {
                return "logs";
            }
        }

        public static string ConfigFile
        {
            get
            {
                return "config.xml";
            }
        }
    }
}
