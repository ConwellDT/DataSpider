using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace CFW.Configuration
{
    public class ConfigManager : IConfigurationHandler
    {
        // Nested Types
        private enum ConfigType { cs, web }

        // Fields
        private static ConfigType            _ConfigType = ConfigType.cs;
        private static ConfigManager         _Default    = null;
        private static IConfigurationHandler Handle      = null;
        public const string                  KEY_SECTION_CONNECTIONSTRING = "CONNECTIONSTRINGS";

        // Properties
        public static ConfigManager Default
        {
            get
            {
                if (_Default == null)   _Default = new ConfigManager();
                return _Default;
            }
        }

        public ConfigManager()
        {
            if (_Default == null)
            {
                _Default = this;
            }
        }

        private static void ValidateCsOrWeb()
        {
            if (HttpContext.Current == null)    _ConfigType = ConfigType.cs;
            else                                _ConfigType = ConfigType.web;
        }

        // Methods
        static ConfigManager()
        {
            ValidateCsOrWeb();
            if (_ConfigType == ConfigType.cs)           Handle = new CSConfigHandler();
            else if (_ConfigType == ConfigType.web)     Handle = new WebConfigHandler();
        }

        public string ReadConfig(string Section, string Key)
        {
            return Handle.ReadConfig(Section, Key);
        }

        public void WriteConfig(string Section, string key, string val)
        {
            Handle.WriteConfig(Section, key, val);
        }
    }
}
