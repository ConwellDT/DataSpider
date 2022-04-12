using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CFW.Data
{
    /// <summary>
    /// MsSqlDbDef 대한 요약 설명입니다.
    /// </summary>
    public abstract class MsSqlDbDef
    {
        /// <summary>
        /// MsSqlDbDef 생성자
        /// </summary>
        public MsSqlDbDef() { }

        /// <summary>
        /// ConnectionString
        /// </summary>
        public static string ConnectionString = CFW.Common.SecurityUtil.DecryptString(CFW.Configuration.ConfigManager.Default.ReadConfig("connectionStrings", "SQL_ConnectionString"));

    }
}
