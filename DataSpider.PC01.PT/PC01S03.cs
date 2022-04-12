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
using System.Globalization;

namespace DataSpider.PC01.PT
{
    /// <summary>
    /// OsmoMeter
    /// </summary>
    public class PC01S03 : PC00B02
    {

        public PC01S03() : base()
        {
        }

        public PC01S03(PC01F01 pOwner, string equipType, string equipName, string connectionInfo, string extraInfo, int nCurNo, bool bAutoRun = false) : base(pOwner, equipType, equipName, connectionInfo, extraInfo, nCurNo, bAutoRun)
        {
        }

        public PC01S03(PC01F01 pOwner, DataRow dr, int nCurNo, bool bAutoRun = false) : this(pOwner, dr["EQUIP_TYPE_NM"].ToString(), dr["EQUIP_NM"].ToString(), dr["CONNECTION_INFO"].ToString(), dr["EXTRA_INFO"].ToString(), nCurNo, bAutoRun)
        {
            drEquipment = dr;
            ReadConfig();
            ReadConnectionInfoForSocket();
            if (m_AutoRun == true)
            {
                m_Thd = new Thread(ThreadJob);
                m_Thd.Start();
            }
        }
        protected override void ParseMessage(string Msg)
        {
            Debug.WriteLine(Msg);
            fileLog.WriteData(Msg, "ParseMessage", "ParseMessage Start");

            string[] LineData = Msg.Split(new string[] { System.Environment.NewLine }, StringSplitOptions.None);

            // listData에 저장용 메시지를 저장함. 
            List<string> listData = new List<string>();
            bool bCalibrationValue = false;
            bool bMeasuredValue = false;
            DateTime dt = new DateTime();
            int nLineCount = 0;

            foreach (string ln in LineData)
            {
                nLineCount++;

                if (string.IsNullOrWhiteSpace(ln)) continue;
                
                if (m_StatusStringList.Exists(x => ln.Trim().StartsWith(x)))// x.StartsWith(ln)))
                {
                    listData.Add(ln.Trim());
                    EnQueue(MSGTYPE.EVENT, string.Join(System.Environment.NewLine, listData));
                    listData.Clear();// = new List<string>();
                    if (LineData.Length > nLineCount)
                    {
                        string[] newLineData = new string[LineData.Length - nLineCount];
                        System.Array.Copy(LineData, nLineCount, newLineData, 0, LineData.Length - nLineCount);
                        state.sb = new StringBuilder();// Received data string.  
                        state.sb.Append(string.Join(System.Environment.NewLine, newLineData));
                    }
                    else
                        state.sb = new StringBuilder();// Received data string.  
                }
                else if (m_StartStringList.Exists(x => ln.Trim().StartsWith(x)))// x.StartsWith(ln)))
                {
                    fileLog.WriteData(ln, "ParseMessage", "Calibration ON");
                    bCalibrationValue = true;
                    listData.Clear();// = new List<string>();
                }
                // 20210408, SHS, OM 의 경우 날짜 형식이 dd/MM/yyyy HH;mm:ss 인데 이것은 그대로 파싱하면 MM/dd/yyyy 형태로 처리되는 문제
                // OM 처리 프로세스에서 날짜인 데이터의 경우 yyyy-MM-dd 형태로 변형처리하기
                //else if (bCalibrationValue==false && PC00U01.TryParseExact(ln.Trim(), out dt) == true)
                //else if (bCalibrationValue == false && DateTime.TryParseExact(ln.Trim(), "dd/MM/yyyy HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out dt) == true)
                //else if (bCalibrationValue == false && PC00U01.TryParseExact(ln.Trim(), out dt) == true)
                // 20210423, SHS, 측정데이터인지 판단하기 위해 시간인지 파싱하는 부분에서는 포맷 상관없이 시간인지만 판단
                //else if (bCalibrationValue == false && DateTime.TryParseExact(ln.Trim(), new string[] { "dd/MM/yyyy HH:mm:ss", "MM/dd/yyyy hh:mm:ss tt" }, CultureInfo.InvariantCulture, DateTimeStyles.None, out dt) == true)
                else if (bCalibrationValue == false && DateTime.TryParseExact(ln.Trim(), new string[] { m_ExtraInfo.Trim() }, CultureInfo.InvariantCulture, DateTimeStyles.None, out dt) == true)
                {
                    fileLog.WriteData(ln, "ParseMessage", "Measure ON");
                    bMeasuredValue = true;
                    listData.Clear(); //listData = new List<string>();
                }
                else 
                {
                    fileLog.WriteData(ln, "ParseMessage", "else");
                    fileLog.WriteData(bCalibrationValue.ToString(), "ParseMessage", "bCalibrationValue");
                }

                if (bCalibrationValue)
                {
                    //if (DateTime.TryParseExact(ln.Trim(), new string[] { "dd/MM/yyyy HH:mm:ss", "MM/dd/yyyy hh:mm:ss tt" }, CultureInfo.InvariantCulture, DateTimeStyles.None, out dt))
                    ////if (PC00U01.TryParseExact(ln.Trim(), out dt))
                    //{
                    //    listData.Add(dt.ToString("yyyy-MM-dd HH:mm:ss"));
                    //}
                    //else
                    //{
                    //    listData.Add(ln);
                    //}
                    fileLog.WriteData(ln, "ParseMessage", "Start Calibration");
                    if (!string.IsNullOrWhiteSpace(m_ExtraInfo) && DateTime.TryParseExact(ln.Trim(), new string[] { m_ExtraInfo.Trim() }, CultureInfo.InvariantCulture, DateTimeStyles.None, out dt))
                    {
                        listData.Add(dt.ToString("yyyy-MM-dd HH:mm:ss"));
                    }
                    else
                    {
                        if (PC00U01.TryParseExact(ln.Trim(), out dt))
                        {
                            listData.Add(dt.ToString("yyyy-MM-dd HH:mm:ss"));
                        }
                        else
                        {
                            listData.Add(ln);
                        }
                    }
                    if (listData.Count >= 4)
                    {
                        // 20211213 kwc Value자리에 들어오는 문자열 처리를 위해 수정
                        //if (ln.Contains("Out of Range"))
                        if (m_EventStringList.Exists(x => ln.Contains(x)))
                            EnQueue(MSGTYPE.EVENT, ln);
                        else
                            EnQueue(MSGTYPE.CALIBRATION, string.Join(System.Environment.NewLine, listData));
                        listData.Clear(); //listData = new List<string>();
                        bCalibrationValue = false;
                        if (LineData.Length > nLineCount)
                        {
                            string[] newLineData = new string[LineData.Length - nLineCount];
                            System.Array.Copy(LineData, nLineCount, newLineData, 0, LineData.Length - nLineCount);
                            state.sb = new StringBuilder();// Received data string.  
                            state.sb.Append(string.Join(System.Environment.NewLine, newLineData));
                        }
                        else
                            state.sb = new StringBuilder();// Received data string.                      
                    }
                }
                if (bMeasuredValue)
                {
                    fileLog.WriteData(ln, "ParseMessage", "Start Measure");
                    //if (PC00U01.TryParseExact(ln.Trim(), out dt))
                    //if (DateTime.TryParseExact(ln.Trim(), new string[] { "dd/MM/yyyy HH:mm:ss", "MM/dd/yyyy hh:mm:ss tt" }, CultureInfo.InvariantCulture, DateTimeStyles.None, out dt))
                    if (DateTime.TryParseExact(ln.Trim(), new string[] { m_ExtraInfo.Trim() }, CultureInfo.InvariantCulture, DateTimeStyles.None, out dt))
                    {
                            listData.Add(dt.ToString("yyyy-MM-dd HH:mm:ss"));
                    }
                    else
                    {
                        if (PC00U01.TryParseExact(ln.Trim(), out dt))
                        {
                            listData.Add(dt.ToString("yyyy-MM-dd HH:mm:ss"));
                        }
                        else
                        {
                            listData.Add(ln);
                        }
                    }
                    if ( listData.Count==2  && ln.Contains("SN") == false )
                    {
                        bMeasuredValue = false;
                        listData.Clear(); //listData = new List<string>();
                    }
                    else if ( listData.Count >= 3 )
                    {
                        // 20211123 kwc Value자리에 들어오는 문자열 처리를 위해 수정
                        //if (ln.Contains("Out of Range"))
                        if ( m_EventStringList.Exists(x=>ln.Contains(x)))
                            EnQueue(MSGTYPE.EVENT, ln);
                        else
                            EnQueue(MSGTYPE.MEASURE, string.Join(System.Environment.NewLine, listData));
                        listData.Clear(); //listData = new List<string>();
                        bMeasuredValue = false;
                        if (LineData.Length > nLineCount)
                        {
                            string[] newLineData = new string[LineData.Length - nLineCount];
                            System.Array.Copy(LineData, nLineCount, newLineData, 0, LineData.Length - nLineCount);
                            state.sb = new StringBuilder();// Received data string.  
                            state.sb.Append(string.Join(System.Environment.NewLine, newLineData));
                        }
                        else
                            state.sb = new StringBuilder();// Received data string.  
                    }
                }
            }
        }
    }
}
