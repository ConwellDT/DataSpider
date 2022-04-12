using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CFW.Configuration
{
    interface IConfigurationHandler
    {
        // Methods
        string ReadConfig(string Section, string Key);
        void WriteConfig(string Section, string key, string val);
    }
}
