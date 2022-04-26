using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Collections;
using System.Threading;
using System.Reflection;
using CFW.Common;
using System.IO;
using System.Text.RegularExpressions;
using DataSpider.PC00.PT;
using System.Diagnostics;
using System.Net.Sockets;
using System.Net;
using System.Data;

namespace DataSpider.PC01.PT
{
    /// <summary>
    /// Turbidity Meter : CAQ4500
    /// Equip_Type : AQ4500
    /// </summary>
    public class PC01S13 : PC00B02
    {
        public PC01S13() : base()
        {
        }

        public PC01S13(PC01F01 pOwner, string equipType, string equipName, string connectionInfo, string extraInfo, int nCurNo, bool bAutoRun = false) : base(pOwner, equipType, equipName, connectionInfo, extraInfo, nCurNo, bAutoRun)
        {
        }

        public PC01S13(PC01F01 pOwner, DataRow dr, int nCurNo, bool bAutoRun = false) : this(pOwner, dr["EQUIP_TYPE_NM"].ToString(), dr["EQUIP_NM"].ToString(), dr["CONNECTION_INFO"].ToString(), dr["EXTRA_INFO"].ToString(), nCurNo, bAutoRun)
        {
            drEquipment = dr;
            ReadConfigInfo();
            ReadConnectionInfoForSocket();
            if (m_AutoRun == true)
            {
                m_Thd = new Thread(ThreadJob);
                m_Thd.Start();
            }
        }
        protected override void ParseMessage(string Msg)
        {
            string[] LineData = Msg.Split(new string[] { System.Environment.NewLine }, StringSplitOptions.None);



            List<string> listData = new List<string>();
            bool Started = false;
            foreach (string ln in LineData)
            {
                if (m_StartStringList.Exists(x => ln.Trim().StartsWith(x)) || CheckStartString(ln.Trim()))

                {
                    Started = true;
                    listData.Clear();
                }
                if (Started)
                {
                    if (string.IsNullOrWhiteSpace(ln))
                        continue;

                    listData.Add(ln);
                    if (listData.Count >= m_MessageLineCount)
                    {
                        EnQueue(MSGTYPE.MEASURE, string.Join(System.Environment.NewLine, listData));
                        Started = false;
                        state.sb.Clear();// = new StringBuilder();// Received data string.  
                    }
                }
            }
        }
    }
}
