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
    /// Equip_Type : PCM_S47_MULTI
    /// PCM_S470K에서 파생                  
    ///           : Ref.Temp. 가 있는 (CONDMSR) 경우- 
    ///           : Cal.Mode: 가 있는 (PHCAL)  경우 (3 세트 기본) 처리
    ///           
    /// 
    /// 
    ///           DC pH 중 BUFFER 가 2 세트만 오는 경우 (3 세트 기본) 처리
    /// </summary>
    public class PC01S27 : PC00B02
    {
        public PC01S27() : base()
        {
        }

        public PC01S27(PC01F01 pOwner, string equipType, string equipName, string connectionInfo, string extraInfo, int nCurNo, bool bAutoRun = false) : base(pOwner, equipType, equipName, connectionInfo, extraInfo, nCurNo, bAutoRun)
        {
        }

        public PC01S27(PC01F01 pOwner, DataRow dr, int nCurNo, bool bAutoRun = false) : this(pOwner, dr["EQUIP_TYPE_NM"].ToString(), dr["EQUIP_NM"].ToString(), dr["CONNECTION_INFO"].ToString(), dr["EXTRA_INFO"].ToString(), nCurNo, bAutoRun)
        {
            drEquipment = dr;
            if (m_Type.Equals("PCM_S47_MULTI"))
            {
                dataEncoding = Encoding.GetEncoding("IBM437");
            }
            ReadConfigInfo();
            ReadConnectionInfoForSocket();
            if (m_AutoRun == true)
            {
                m_Thd = new Thread(ThreadJob);
                m_Thd.Start();
            }
        }
        // 2022-07-25 kwc
        // 장비 OFF -> ON 시 NULL 문자 두개가 개행문자 없이 수신되어 측정데이터 시작 문자열앞에 붙어서 시작문자열 검출이 안되는 문제로 인해 NULL 문자 제거 처리
        // AUTO BAUDRATE OFF 인 경우 NULL 문자 두개가 수신되지만 개행문자가 
        string RemoveNull(string Msg)
        {
            string returnString = string.Empty;
            foreach (char ch in Msg)
            {
                if (ch != 0) returnString += ch;
            }
            return returnString;
        }


        protected override void ParseMessage(string Msg)
        {
            // 2023-02-01  kwc    S7_MULTI는  "\n\r"을 PrinterNewLine 로 사용함.
            string PrinterNewLine = "\n\r";
            // 2022-07-25 kwc
            string[] LineData1 = Msg.Split(new string[] { PrinterNewLine }, StringSplitOptions.None);

            foreach (string ln in LineData1)
            {
                if (m_IgnoreStringList.Exists(s => ln.Contains(s) && !string.IsNullOrWhiteSpace(s)))
                {
                    state.sb = state.sb.Replace(ln + PrinterNewLine, "");
                }
            }
            Msg = state.sb.ToString();
            Msg = RemoveNull(Msg);

            string[] LineData = Msg.Split(new string[] { PrinterNewLine }, StringSplitOptions.None);
            List<string> listData = new List<string>();
            bool Started = false;
            //List<string> forEnque = new List<string>();
            string lineString = string.Empty;
            int nType = MSGTYPE.UNKNOWN;

            // 현재까지 수신한 데이터 정리 (무시문자열 라인 제거, 길이가 길어 두줄 이상으로 나뉜 항목 한줄로 통합처리)
            foreach (string ln in LineData)
            {
                //if (CheckStartString(ln))
                if (m_StartStringList.Exists(x => ln.Trim().StartsWith(x)) || CheckStartString(ln.Trim()))
                {
                    Started = true;
                    // 시작이면 그 이전 데이터는 버려야 함
                    listData.Clear();
                    lineString = string.Empty;
                }

                if (Started)
                {
                    // 20210407, SHS, Ignore line, 수신한 라인에 무시문자열이 있으면 해당 라인 처리 제외
                    if (m_IgnoreStringList.Exists(s => ln.Contains(s) && !string.IsNullOrWhiteSpace(s)))
                    {
                        continue;
                    }
                    if (string.IsNullOrWhiteSpace(ln)) // 현재 라인이 공백줄이면 
                    {
                        if (string.IsNullOrWhiteSpace(lineString)) // 이전 라인이 없으면 다음 라인 읽기
                        {
                            continue;
                        }
                        else // 이전 라인이 있었으면 해당 라인 저장
                        {
                            listData.Add(lineString.TrimEnd());
                            lineString = string.Empty;
                        }

                    }
                    else // 현재 라인이 공백이 아니면
                    {
                        if (string.IsNullOrWhiteSpace(ln.Length > 11 ? ln.Substring(0, 11) : ln)) // 왼쪽이 없으면 더해주기
                        {
                            // 왼쪽 공백만 없앰
                            lineString += ln.TrimStart();
                        }
                        else // 현재 라인이 새 항목이면
                        {
                            if (!string.IsNullOrWhiteSpace(lineString)) // 이전 라인 항목이 있었으면 저장
                            {
                                listData.Add(lineString.TrimEnd());
                            }
                            lineString = ln;
                        }
                    }
                }
            }
            string sMsgTemp = string.Join(PrinterNewLine, listData);

            if (listData.Count > 0)
            {
                // 메세지 타입을 판별한 문자열을 찾고 그 타입의 데이터가 다 수신되었는지 확인
                int keyIndex = m_KeyStringList.FindIndex(s => sMsgTemp.Contains(s) && !string.IsNullOrWhiteSpace(s));

                // 20210419, SHS, PCM_S470K DC pH 중 BUFFER 가 2 개 오는 경우 처리
                if (m_Type.Equals("PCM_S47_MULTI") && keyIndex == 1 && listData.Count >= m_LineLengthList[keyIndex] - 6 && !sMsgTemp.Contains("B2("))
                {
                    //listData.Insert(14, " Endpoint       ".TrimEnd());
                    //listData.Insert(14, " Temp. 3     ".TrimEnd());
                    //listData.Insert(14, "Buffer 3        ".TrimEnd());
                    /* Format 변경이 발생할 수 있음 buffer2  다음으로 수정 2021/8/5
                    listData.Insert(14, string.Empty);
                    listData.Insert(14, string.Empty);
                    listData.Insert(14, string.Empty);
                    */
                    int nBuffer3Position = listData.FindIndex(p => p.Contains("B1(")) + 3;
                    listData.Insert(nBuffer3Position, string.Empty);
                    listData.Insert(nBuffer3Position, string.Empty);
                    listData.Insert(nBuffer3Position, string.Empty);
                    listData.Insert(nBuffer3Position, string.Empty);
                    listData.Insert(nBuffer3Position, string.Empty);
                    listData.Insert(nBuffer3Position, string.Empty);
                }
                else if (m_Type.Equals("PCM_S47_MULTI") && keyIndex == 1 && listData.Count >= m_LineLengthList[keyIndex] - 3 && !sMsgTemp.Contains("B3("))
                {
                    //listData.Insert(14, " Endpoint       ".TrimEnd());
                    //listData.Insert(14, " Temp. 3     ".TrimEnd());
                    //listData.Insert(14, "Buffer 3        ".TrimEnd());
                    /* Format 변경이 발생할 수 있음 buffer2  다음으로 수정 2021/8/5
                    listData.Insert(14, string.Empty);
                    listData.Insert(14, string.Empty);
                    listData.Insert(14, string.Empty);
                    */
                    int nBuffer3Position = listData.FindIndex(p => p.Contains("B2(")) + 3;
                    listData.Insert(nBuffer3Position, string.Empty);
                    listData.Insert(nBuffer3Position, string.Empty);
                    listData.Insert(nBuffer3Position, string.Empty);
                }

                if (keyIndex > -1 && listData.Count >= m_LineLengthList[keyIndex])
                {
                    nType = keyIndex + 1;
                }
                if (nType != MSGTYPE.UNKNOWN)
                {
                    EnQueue(nType, string.Join(PrinterNewLine, listData));
                    nType = MSGTYPE.UNKNOWN;
                    Started = false;
                    state.sb.Clear();// = new StringBuilder();// Received data string.  
                }
            }
        }

    }
}
