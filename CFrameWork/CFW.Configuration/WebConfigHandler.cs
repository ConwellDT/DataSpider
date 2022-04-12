using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Configuration;
using System.Configuration;

namespace CFW.Configuration
{
    class WebConfigHandler : IConfigurationHandler
    {
        // Methods
        private bool ContainKeys(KeyValueConfigurationCollection col, string key)
        {
            string[] Keys = col.AllKeys;
            foreach (string s in Keys)
            {
                if (string.Compare(s, key) == 0)
                {
                    return true;
                }
            }
            return false;
        }

        public string ReadConfig(string Section, string Key)
        {
            string val = string.Empty;
            
            val = WebConfigurationManager.AppSettings[Key];            
            if (val == null)    return string.Empty;
           
            return val;
        }

        public void WriteConfig(string Section, string key, string val)
        {
            string data = val;
            System.Configuration.Configuration cfg = WebConfigurationManager.OpenWebConfiguration("~");

            AppSettingsSection              appSection = cfg.AppSettings;
            KeyValueConfigurationCollection settings   = appSection.Settings;

            if (((cfg != null) && (appSection != null)) && (settings != null))
            {
                if (this.ContainKeys(settings, key))    appSection.Settings[key].Value = data;
                else                                    appSection.Settings.Add(key, data);

                cfg.Save(ConfigurationSaveMode.Full, false);
            }
        }
    }
}
