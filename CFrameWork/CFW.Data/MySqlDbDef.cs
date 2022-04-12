using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CFW.Data
{
    /// <summary>
    /// MySqlDbDef 대한 요약 설명입니다.
    /// </summary>
    public abstract class MySqlDbDef
    {
        /// <summary>
        /// MySqlDbDef 생성자
        /// </summary>
        public MySqlDbDef() { }

        /// <summary>
        /// ConnectionString
        /// </summary>
        //public static string ConnectionString = CFW.Common.SecurityUtil.DecryptString(CFW.Configuration.ConfigManager.Default.ReadConfig("connectionStrings", "MySQL_ConnectionString"));


        //public static string tt = CFW.Common.SecurityUtil.EncryptString("127.0.0.1");

        public static string ConnectionString = CFW.Common.SecurityUtil.DecryptString(System.Configuration.ConfigurationSettings.AppSettings["MySQL_ConnectionString"]);

    }

}
