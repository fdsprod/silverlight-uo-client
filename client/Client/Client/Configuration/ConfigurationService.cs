using System.IO;
using Client.IO;

namespace Client.Configuration
{
    public sealed class ConfigurationService : IConfigurationService
    {
        private readonly ConfigFile _configFile;
        private readonly Engine _engine;

        public ConfigurationService(Engine engine)
        {
            string file = Paths.ConfigFile;

            _engine = engine;
            _configFile = new ConfigFile(file);

            if (!File.Exists(file))
                RestoreDefaults();
        }

        public void RestoreDefaults()
        {
            SetValue(ConfigSections.Graphics, ConfigKeys.Width, 1024);
            SetValue(ConfigSections.Graphics, ConfigKeys.Height, 768);
        }

        public T GetValue<T>(string section, string key)
        {
            return _configFile.GetValue<T>(section, key);
        }

        public T GetValue<T>(string section, string key, T defaultValue)
        {
            return _configFile.GetValue<T>(section, key, defaultValue);
        }

        public void SetValue<T>(string section, string key, T value)
        {
            _configFile.SetValue(section, key, value);
        }
    }
}
