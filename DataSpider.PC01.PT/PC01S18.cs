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
    /// Scale (Table - top) : MCA
    /// Equip_Type : MCA
    /// </summary>
    public class PC01S18 : PC00B02
    {
        private Dictionary<string, int> dicItemLine = null;
        public PC01S18() : base()
        {
        }

        public PC01S18(PC01F01 pOwner, string equipType, string equipName, string connectionInfo, string extraInfo, int nCurNo, bool bAutoRun = false) : base(pOwner, equipType, equipName, connectionInfo, extraInfo, nCurNo, bAutoRun)
        {
        }

        public PC01S18(PC01F01 pOwner, DataRow dr, int nCurNo, bool bAutoRun = false) : this(pOwner, dr["EQUIP_TYPE_NM"].ToString(), dr["EQUIP_NM"].ToString(), dr["CONNECTION_INFO"].ToString(), dr["EXTRA_INFO"].ToString(), nCurNo, bAutoRun)
        {
            drEquipment = dr;
            ReadConfigInfo();
            InitItemLineConf();
            ReadConnectionInfoForSocket();
            if (m_AutoRun == true)
            {
                m_Thd = new Thread(ThreadJob);
                m_Thd.Start();
            }
        }

        private void InitItemLineConf()
        {
            dicItemLine = new Dictionary<string, int>();

            if (!string.IsNullOrWhiteSpace(itemLineConf))
            {
                //G=3;N=4;T=5;GC=6
                string[] lineConf = itemLineConf.Split(new string[] { ";" }, StringSplitOptions.RemoveEmptyEntries);
                foreach (string item in lineConf)
                {
                    string[] keyVal = item.Split(new string[] { "=" }, StringSplitOptions.RemoveEmptyEntries);
                    dicItemLine.Add(keyVal[0], int.Parse(keyVal[1]));
                }
            }
        }

        protected override void ParseMessage(string Msg)
        {
            string[] LineData = Msg.Split(new string[] { System.Environment.NewLine }, StringSplitOptions.None);

            List<string> listData = new List<string>();
            bool Started = false;
            //foreach (string ln in LineData)
            //{
            //    if (m_StartStringList.Exists(x => ln.Trim().StartsWith(x)) || CheckStartString(ln.Trim()))
            //    {
            //        Started = true;
            //        listData.Clear();
            //    }
            //    if (Started)
            //    {
            //        // G#    -     1.92 kg 형태의 값 처리하기 위해 공백으로 split 한 후 +, - 부호를 값과 붙여주고 각 항목을 공백을 주어 문자열 처리
            //        if (string.IsNullOrWhiteSpace(ln))
            //            continue;

            //        string[] arrLn = ln.Split(new string[] {" "}, StringSplitOptions.RemoveEmptyEntries);
            //        if (arrLn.Length < 3)
            //        {
            //            listData.Add(ln);
            //        }
            //        else
            //        {
            //            listData.Add(arrLn.Length >= 4 ? $"{arrLn[0]} {arrLn[1]}{arrLn[2]} {arrLn[3]}" : $"{arrLn[0]} {arrLn[1]} {arrLn[2]}");
            //        }
            //        if (listData.Count >= maximumLineCount)
            //        {
            //            EnQueue(MSGTYPE.MEASURE, string.Join(System.Environment.NewLine, listData));
            //            Started = false;
            //            state.sb.Clear();
            //        }
            //    }
            //}
            for (int i = 0; i < LineData.Length; i++)
            {
                // 수신데이터 중 시작문자를 찾으면 시작 플래그 on, 이전에 처리하기 위해 저장해 둔 데이터는 클리어.
                if (m_StartStringList.Exists(x => LineData[i].Trim().StartsWith(x)) || CheckStartString(LineData[i].Trim()))
                {
                    Started = true;
                    listData.Clear();
                }
                // 데이터 시작
                if (Started)
                {
                    // G#    -     1.92 kg 형태의 값 처리하기 위해 공백으로 split 한 후 +, - 부호를 값과 붙여주고 각 항목을 공백을 주어 문자열 처리
                    if (string.IsNullOrWhiteSpace(LineData[i]))
                        continue;

                    string[] arrLn = LineData[i].Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
                    if (arrLn.Length < 3)
                    {
                        listData.Add(LineData[i]);
                    }
                    else
                    {
                        listData.Add(arrLn.Length >= 4 ? $"{arrLn[0]} {arrLn[1]}{arrLn[2]} {arrLn[3]}" : $"{arrLn[0]} {arrLn[1]} {arrLn[2]}");
                    }
                    // 최대 데이터 설정만큼 수신되었으면 수신데이터를 파싱처리로 전달 
                    if (listData.Count >= maximumLineCount && maximumLineCount >= minimumLineCount)
                    {
                        //EnQueue(MSGTYPE.MEASURE, string.Join(System.Environment.NewLine, listData));
                        EnQueue(listData);
                        Started = false;
                        state.sb.Clear();
                    }
                    // 최소 데이터 설정 (사용자, 시간 => 고정으로 생성되는 위치가 고정된 데이터) 
                    else if (listData.Count > minimumLineCount)
                    {
                        EnQueue(listData);
                    }
                }
            }
        }
        // 항목이름에 따라 설정된 줄에 해당 항목을 저장하여 파싱처리 전달
        private void EnQueue(List<string> listData)
        {
            List<string> adjustedlistData = new List<string>();
            adjustedlistData.AddRange(new string[maximumLineCount]);

            // 공정항목 add
            for (int i = 0; i < minimumLineCount; i++)
            {
                adjustedlistData[i] = listData[i];
            }
            // 가변항목 add
            for (int i = minimumLineCount; i < listData.Count; i++)
            {
                string[] arrData = listData[i].Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
                if (dicItemLine.TryGetValue(arrData[0], out int val))
                {
                    adjustedlistData[val-1] = listData[i];
                }
            }
            EnQueue(MSGTYPE.MEASURE, string.Join(Environment.NewLine, adjustedlistData));
        }

        protected new void ThreadJob()
        {
            Thread.Sleep(1000);
            bool connectedflag = false;
            Connect();
            Thread.Sleep(1000);
            listViewMsg.UpdateStatus(true);
            listViewMsg.UpdateMsg("Thread started", true, true, true, PC00D01.MSGTINF);

            while (!bTerminal)
            {
                try
                {
                    //connectDone.WaitOne();
                    if (IsConnected == false || state.workSocket.Connected==false)
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
                            // MCA 의 경우 연결 후 1회 메세지를 수신해야 결과 데이터를 전송해준다. 연결 시 ACK 전송.
                            SendControlMessage(ASCII.ACK);
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
            Disconnect();
            UpdateEquipmentProgDateTime(IF_STATUS.Stop);
            listViewMsg.UpdateStatus(false);
            listViewMsg.UpdateMsg("Thread finished");
        }

    }
}
