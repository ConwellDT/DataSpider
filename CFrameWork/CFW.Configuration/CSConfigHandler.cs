using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Runtime.InteropServices;
using System.Configuration;

namespace CFW.Configuration
{
    class CSConfigHandler : IConfigurationHandler
    {
        // Fields
        //private const string KEY_ConfigFilePath = "ConfigFilePath";
        private const int INIT_SIZE = 800;

        // Properties
        public static string ConfigFilePath
        {
            get
            {
                string path = string.Empty;
                try
                {
                    path = ConfigurationSettings.AppSettings["ConfigFilePath"];
                }
                catch (Exception)
                {
                    throw new ApplicationException(string.Format("환경정보(app.config) AppSettings에서 key {0} 를 확인할 수 없습니다", "ConfigFilePath"));
                }
                return path;
            }
        }

        #region CS exe.config 읽고 쓰기

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
            string val = "";

            if(ConfigFilePath != "" && ConfigFilePath != null)  val = ReadConfigValue(Section, Key, ConfigFilePath);
            else                                                val = System.Configuration.ConfigurationManager.AppSettings[Key];

            return val;
        }

        public void WriteConfig(string Section, string key, string val)
        {
            string data = val;
            System.Configuration.Configuration cfg = ConfigurationManager.OpenExeConfiguration(AppDomain.CurrentDomain.BaseDirectory + AppDomain.CurrentDomain.FriendlyName);

            if (ConfigFilePath != "" && ConfigFilePath != null)
            {
                WriteConfigValue(Section, key, data, ConfigFilePath);
            }
            else
            {
                AppSettingsSection appSection = cfg.AppSettings;
                KeyValueConfigurationCollection settings = appSection.Settings;

                if (((cfg != null) && (appSection != null)) && (settings != null))
                {
                    if (this.ContainKeys(settings, key)) appSection.Settings[key].Value = data;
                    else appSection.Settings.Add(key, data);

                    cfg.Save(ConfigurationSaveMode.Full, false);
                }

                //try
                //{
                //    cfg.AppSettings.Settings[key].Value = data;
                //}
                //catch(Exception)
                //{
                //    cfg.AppSettings.Settings.Add(key, data);
                //}
                //cfg.Save();         
            }
        }
        #endregion


        [DllImport("kernel32.dll")]
        public static extern int GetPrivateProfileString(string section, string key, string def, StringBuilder retVal, int size, string filePath);

        public static string ReadConfigValue(string Section, string Key, string cfgPath)
        {
            StringBuilder   temp = new StringBuilder(INIT_SIZE);
            string          path = Path.GetFullPath(cfgPath);
            string          strReturn = string.Empty;

            try
            {
                int i = GetPrivateProfileString(Section, Key, "", temp, INIT_SIZE, path);
                
                strReturn = temp.ToString();
            }
            catch (Exception)
            {
                throw new ApplicationException(string.Format("{0} 파일에서 {1} 섹션 {2} 키 정보를 읽을 수 없습니다.", cfgPath, Section, Key));
            }
            return strReturn;
        }


        [DllImport("kernel32.dll")]
        public static extern long WritePrivateProfileString(string section, string key, string val, string filePath);

        //public void WriteConfig(string Section, string key, string val)
        //{
        //    WriteConfigValue(Section, key, val, ConfigFilePath);
        //}

        public static void WriteConfigValue(string Section, string Key, string Value, string cfgPath)
        {
            StringBuilder   temp = new StringBuilder(INIT_SIZE);
            string          path = Path.GetFullPath(cfgPath);

            if (File.Exists(path))
            {
                File.SetAttributes(path, FileAttributes.Normal);
            }
            else
            {
                File.CreateText(path).Close();
            }

            WritePrivateProfileString(Section, Key, Value, path);
        }

        // Methods ==> OracleDBDef.cs 로 이동함.
        //public static string GetSQLStorageDBConnectionString()
        //{
        //    StringBuilder   temp      = new StringBuilder(INIT_SIZE);
        //    string          path      = Path.GetFullPath(ConfigFilePath);
        //    string          Section   = string.Empty;
        //    string          Key       = string.Empty;
        //    string          strReturn = string.Empty;

        //    try
        //    {
        //        Section = "SQLStorage";
        //        Key     = "OracleSQLStorage_ConnectionString";

        //        int i = GetPrivateProfileString(Section, Key, "", temp, INIT_SIZE, path);
        //        strReturn = temp.ToString();
        //    }
        //    catch (Exception)
        //    {
        //        throw new ApplicationException(string.Format("{0} 파일에서 {1} 섹션 {2} 키 정보를 읽을 수 없습니다.", ConfigFilePath, Section, Key));
        //    }
        //    return strReturn;
        //}

    }
}
