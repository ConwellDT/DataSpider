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
    /// Scale (Table - top) : MSA36201S-000-D0
    /// Floor Scale : CAIS2+IFS4-3000RR-I
    /// Floor Scale : CAIS2+IFS4-600NL-I
    /// Scale(Portable) : CAIS2/IFS4-600II-I
    /// Equip_Type : SC_MSA, SC_CAIS2, FSC_CAIS2
    /// </summary>
    public class PC01S11 : PC00B02
    {
        public PC01S11() : base()
        {
        }

        public PC01S11(PC01F01 pOwner, DataRow dr, string equipType, string equipName, string connectionInfo, string extraInfo, int nCurNo, bool bAutoRun = false) : base(pOwner, dr, equipType, equipName, connectionInfo, extraInfo, nCurNo, bAutoRun)
        {
        }

        public PC01S11(PC01F01 pOwner, DataRow dr, int nCurNo, bool bAutoRun = false) : this(pOwner, dr, dr["EQUIP_TYPE_NM"].ToString(), dr["EQUIP_NM"].ToString(), dr["CONNECTION_INFO"].ToString(), dr["EXTRA_INFO"].ToString(), nCurNo, bAutoRun)
        {
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
                    // G#    -     1.92 kg 형태의 값 처리하기 위해 공백으로 split 한 후 +, - 부호를 값과 붙여주고 각 항목을 공백을 주어 문자열 처리
                    if (string.IsNullOrWhiteSpace(ln))
                        continue;

                    string[] arrLn = ln.Split(new string[] {" "}, StringSplitOptions.RemoveEmptyEntries);
                    if (arrLn.Length < 3)
                    {
                        listData.Add(ln);
                    }
                    else
                    {
                        listData.Add(arrLn.Length >= 4 ? $"{arrLn[0]} {arrLn[1]}{arrLn[2]} {arrLn[3]}" : $"{arrLn[0]} {arrLn[1]} {arrLn[2]}");
                    }
                    if (listData.Count >= m_MessageLineCount)
                    {
                        EnQueue(MSGTYPE.MEASURE, string.Join(System.Environment.NewLine, listData));
                        Started = false;
                        state.sb.Clear();
                    }
                }
            }
        }
    }
}
