using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NLog;
using NLog.Targets;


namespace DataSpider.PC00.PT
{

    public abstract class PC00B01
    {
        protected int UpdateInterval = 10;
        protected IF_STATUS lastStatus = IF_STATUS.Stop;

        protected DataRow drEquipment = null;
        protected string m_Name;
        protected string m_Type;
        protected string m_ConnectionInfo;
        protected string m_ExtraInfo;
        protected bool m_AutoRun;

        public bool bTerminal = false;
        protected PC00Z01 m_sqlBiz = new PC00Z01();

        protected int m_nCurNo = 0;

        public Thread m_Thd = null;

        protected IPC00F00 m_Owner = null;
        protected FormListViewMsg listViewMsg = null;
        protected FileLog fileLog = null;

        private DateTime dtLastUpdateProgDateTime = DateTime.MinValue;
        protected Encoding dataEncoding = Encoding.UTF8;

        public PC00B01()
        {
        }
        public PC00B01(IPC00F00 pOwner, string equipType, string equipName, string connectionInfo, string extraInfo, int nCurNo, bool bAutoRun)
        {
            m_Owner = pOwner;
            m_Name = equipName.Trim();
            m_Type = equipType.Trim();
            m_ConnectionInfo = connectionInfo.Trim();
            m_ExtraInfo = extraInfo.Trim();
            m_nCurNo = nCurNo;
            m_AutoRun = bAutoRun;


            listViewMsg = new FormListViewMsg(m_Owner, m_Name, m_nCurNo, m_Type);
            //fileLog = new FileLog($"{m_Name}_{m_Type}");
            fileLog = new FileLog(!string.IsNullOrWhiteSpace(m_Type) ? $"{m_Type}_{m_Name}" : m_Name);
            fileLog.SetDbLogger(m_Name);

            //if (m_AutoRun == true)
            //{
            //    m_Thd = new Thread(ThreadJob);
            //    m_Thd.Start();
            //}
            SetDateTimeForamt();
        }
        private void SetDateTimeForamt()
        {
            string strErrCode = string.Empty, strErrText = string.Empty;
            DataTable dt = m_sqlBiz.GetCommonCode("TIMEFORMAT", ref strErrCode, ref strErrText);
            if (dt != null && dt.Rows.Count > 0)
            {
                PC00U01.DateTimeFomatSetting = dt.Rows[0]["CODE_VALUE"].ToString();
            }
        }
        protected void EnQueue(int Type, string Data)
        {
            QueueMsg msg = new QueueMsg();
            msg.m_EqName = m_Name;
            msg.m_MsgType = Type;
            msg.m_Data = Data;

            fileLog.WriteData(Data, $"EnqueData({Type})", "EnQueue");

            PC00U01.WriteQueue(msg);
        }
        /// <summary>
        /// 이전 처리 후 60초가 경과했으면 업데이트
        /// </summary>
        /// <returns></returns>
        protected bool UpdateEquipmentProgDateTime(IF_STATUS status = IF_STATUS.Normal)
        {
            string errCode = string.Empty;
            string errText = string.Empty;
            DateTime dtNow = DateTime.Now;
            if (dtNow.Subtract(dtLastUpdateProgDateTime).TotalSeconds > UpdateInterval || !lastStatus.Equals(status))
            {
                dtLastUpdateProgDateTime = dtNow;
                lastStatus = status;
                return m_sqlBiz.UpdateEquipmentProgDateTime(m_Name, (int)status, ref errCode, ref errText);
            }
            return true;
        }
    }
}
