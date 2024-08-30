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
using SEIMM.PC00.PT;
using System.Diagnostics;
using System.Net.Sockets;
using System.Net;
using System.Data;

namespace SEIMM.PC01.PT
{
    /// <summary>
    /// Scale (Table - top) : MCA
    /// </summary>
    public class PC01S21 : PC00B02
    {
        public PC01S21() : base()
        {
        }

        public PC01S21(PC01F01 pOwner, DataRow dr, string equipType, string equipName, string connectionInfo, string extraInfo, int nCurNo, bool bAutoRun = false) : base(pOwner, dr, equipType, equipName, connectionInfo, extraInfo, nCurNo, bAutoRun)
        {
        }

        public PC01S21(PC01F01 pOwner, DataRow dr, int nCurNo, bool bAutoRun = false) : this(pOwner, dr, dr["EQUIP_TYPE_NM"].ToString(), dr["EQUIP_NM"].ToString(), dr["CONNECTION_INFO"].ToString(), dr["EXTRA_INFO"].ToString(), nCurNo, bAutoRun)
        {
            ReadConfig();
            ReadConnectionInfoForSocket();
            if (m_AutoRun == true)
            {
                m_Thd = new Thread(ThreadJob2);
                m_Thd.Start();
            }
        }
        protected async void ThreadJob2()
        {
            Thread.Sleep(1000);
            bool connectedflag = false;
            Connect();
            Thread.Sleep(1000);
            listViewMsg.UpdateStatus(true);
            listViewMsg.UpdateMsg("Thread started", true, true, true, PC00D01.MSGTINF);
            byte[] sendMsg = new byte[1];
            while (!bTerminal)
            {
                try
                {
                    //connectDone.WaitOne();
                    if (IsConnected == false || state.workSocket.Connected == false)
                    {
                        connectedflag = false;
                        UpdateEquipmentProgDateTime(IF_STATUS.Disconnected);
                        listViewMsg.UpdateMsg("Disconnected. Try to connect", true, true, true, PC00D01.MSGTERR);
                        Connect();
                        //Thread.Sleep(1000);
                    }
                    else
                    {
                        if (!connectedflag)
                        {
                            sendMsg = new byte[1];
                            sendMsg[0] = ASCII.ACK;
                            state.workSocket.Send(sendMsg);
                            listViewMsg.UpdateMsg("Connected", true, true, true, PC00D01.MSGTINF);
                            connectedflag = true;
                        }
                        else
                        {
                            if (DateTime.Now.Subtract(dtLastStatusUpdate).TotalSeconds > 10)
                            {
                                listViewMsg.UpdateMsg("Connected", true, false, false, PC00D01.MSGTINF);
                            }
                        }
                        dtLastStatusUpdate = DateTime.Now;


                        if ((CommStatus != 0) && (DateTime.Now.Subtract(state.dtLastReceived).TotalSeconds > 30))
                        {
                            CommStatus = 0;
                        }
                        UpdateEquipmentProgDateTime(IF_STATUS.Normal);
                    }
                }
                catch (Exception ex)
                {
                    UpdateEquipmentProgDateTime(IF_STATUS.InternalError);
                    listViewMsg.UpdateMsg($"Exception in ThreadJob - ({ex})", false, true, true, PC00D01.MSGTERR);
                }
                finally
                {
                }
                Thread.Sleep(1000);
            }
            UpdateEquipmentProgDateTime(IF_STATUS.Stop);
            Disconnect();
            listViewMsg.UpdateStatus(false);
            listViewMsg.UpdateMsg("Thread finished");
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

                    string[] arrLn = ln.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
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
