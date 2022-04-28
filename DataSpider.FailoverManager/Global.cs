using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataSpider.PC00.PT;

namespace DataSpider.FailoverManager
{
    public class MyClsLog : FileLog
    {
        MainForm m_Owner = null;

        public MyClsLog(string fileName = "") : base(fileName)
        {
        }
        public void SetOwner(MainForm This)
        {
            m_Owner = This;
        }

        public void LogToFile(string FileType, string FileName, string p_strStat, string p_strExplain, string p_strLogMsg)
        {
            base.WriteLog(p_strLogMsg, p_strStat, logFileName);
        }


        public new void WriteLog(string msg, string msgType = PC00D01.MSGTINF, [System.Runtime.CompilerServices.CallerMemberName] string callerName = "")
        {
            m_Owner.listViewMsg(logFileName, msg, false, 1, 1, true, msgType);
            base.WriteLog(msg, msgType, callerName);
        }

    }

    public  class Global
    {
    }
}
