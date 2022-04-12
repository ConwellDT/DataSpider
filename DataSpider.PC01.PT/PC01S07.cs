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
    /// Equip_Type : PCM_S470K, PCM_PC220
    /// 공백줄 제거, 여러 줄로 표시되는 항목을 한줄로 편집 처리
    /// PCM_S470K : 소켓 수신 데이터 UTF7 인코딩 처리 (기본 UTF8)
    ///           : DC pH 중 BUFFER 가 2 세트만 오는 경우 (3 세트 기본) 처리
    /// </summary>
    public class PC01S07 : PC00B02
    {
        public PC01S07() : base()
        {
        }

        public PC01S07(PC01F01 pOwner, string equipType, string equipName, string connectionInfo, string extraInfo, int nCurNo, bool bAutoRun = false) : base(pOwner, equipType, equipName, connectionInfo, extraInfo, nCurNo, bAutoRun)
        {
        }

        public PC01S07(PC01F01 pOwner, DataRow dr, int nCurNo, bool bAutoRun = false) : this(pOwner, dr["EQUIP_TYPE_NM"].ToString(), dr["EQUIP_NM"].ToString(), dr["CONNECTION_INFO"].ToString(), dr["EXTRA_INFO"].ToString(), nCurNo, bAutoRun)
        {
            drEquipment = dr;
            if (m_Type.Equals("PCM_S470-K"))
            {
                dataEncoding = Encoding.UTF7;
            }
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
            string[] LineData = Msg.Split(new string[] { System.Environment.NewLine }, StringSplitOptions.None);
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
            string sMsgTemp = string.Join(System.Environment.NewLine, listData);

            if (listData.Count > 0)
            {
                // 메세지 타입을 판별한 문자열을 찾고 그 타입의 데이터가 다 수신되었는지 확인
                int keyIndex = m_KeyStringList.FindIndex(s => sMsgTemp.Contains(s) && !string.IsNullOrWhiteSpace(s));

                // 20210419, SHS, PCM_S470K DC pH 중 BUFFER 가 2 개 오는 경우 처리
                if (m_Type.Equals("PCM_S470-K") && keyIndex == 2 && listData.Count >= m_LineLengthList[keyIndex] - 3 && !sMsgTemp.Contains("Buffer 3"))
                {
                    //listData.Insert(14, " Endpoint       ".TrimEnd());
                    //listData.Insert(14, " Temp. 3     ".TrimEnd());
                    //listData.Insert(14, "Buffer 3        ".TrimEnd());
                    /* Format 변경이 발생할 수 있음 buffer2  다음으로 수정 2021/8/5
                    listData.Insert(14, string.Empty);
                    listData.Insert(14, string.Empty);
                    listData.Insert(14, string.Empty);
                    */
                    int nBuffer3Position = listData.FindIndex(p => p.Contains("Buffer 2")) + 3;
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
                    EnQueue(nType, string.Join(System.Environment.NewLine, listData));
                    nType = MSGTYPE.UNKNOWN;
                    Started = false;
                    state.sb.Clear();// = new StringBuilder();// Received data string.  
                }
            }
        }

        // 20210409, shs, 수정전 백업
        //protected override void ParseMessage(string Msg)
        //{
        //    string[] LineData = Msg.Split(new string[] { System.Environment.NewLine }, StringSplitOptions.None);

        //    List<string> listData = new List<string>();
        //    bool Started = false;
        //    List<string> forEnque = new List<string>();
        //    string lineString = string.Empty;
        //    int nType = MSGTYPE.UNKNOWN;
        //    foreach (string ln in LineData)// (int lineDataindex = 0; lineDataindex < LineData.Length; lineDataindex++)
        //    {
        //        //if (ln.StartsWith("\u001b"))// "m_StartStringList[0]))  // <ESC>CS3 이라는 가정하에 프로그램 함.
        //        if (CheckStartString(ln))// "m_StartStringList[0]))  // <ESC>CS3 이라는 가정하에 프로그램 함.
        //        {
        //            Started = true;
        //            // 시작이면 그 이전 데이터는 버려야 함
        //            listData.Clear();
        //        }
        //        if (Started)
        //        {
        //            // 20210407, SHS, Ignore line
        //            if (m_IgnoreStringList.Exists(s => ln.Contains(s) && !string.IsNullOrWhiteSpace(s)))
        //            {
        //                continue;
        //            }
        //            listData.Add(ln);
        //            //for (int i = 0; i < m_KeyStringList.Count; i++)
        //            //{
        //            //    if (Msg.Contains(m_KeyStringList[i]) == true && listData.Count >= m_LineLengthList[i])
        //            //    {
        //            //        nType = i + 1;
        //            //        break;
        //            //    }
        //            //}
        //            int keyIndex = m_KeyStringList.FindIndex(s => Msg.Contains(s) && !string.IsNullOrWhiteSpace(s));
        //            if (keyIndex > -1 && listData.Count >= m_LineLengthList[keyIndex])
        //            {
        //                nType = keyIndex + 1;
        //                break;
        //            }
        //            // 메세지 타입이 있으면 
        //            if (nType != MSGTYPE.UNKNOWN)
        //            {
        //                foreach (string oneline in listData)
        //                {
        //                    //if (oneline.Length < 12)
        //                    //    continue;
        //                    if (string.IsNullOrWhiteSpace(oneline)) // 현재 라인이 공백줄이면 
        //                    {
        //                        if (string.IsNullOrWhiteSpace(lineString)) // 이전 라인이 없으면 다음 라인 읽기
        //                        {
        //                            continue;
        //                        }
        //                        else // 이전 라인이 있었으면 해당 라인 저장
        //                        {
        //                            forEnque.Add(lineString);
        //                            lineString = string.Empty;
        //                        }
        //                    }
        //                    else // 현재 라인이 공백이 아니면
        //                    {
        //                        if (string.IsNullOrWhiteSpace(oneline.Length > 11 ? oneline.Substring(0, 12) : oneline)) // 왼쪽이 없으면 더해주기
        //                        {
        //                            lineString += oneline.Trim();
        //                        }
        //                        else // 현재 라인이 새 항목이면
        //                        {
        //                            if (!string.IsNullOrWhiteSpace(lineString)) // 이전 라인 항목이 있었으면 저장
        //                            {
        //                                forEnque.Add(lineString);
        //                            }
        //                            lineString = oneline;
        //                        }
        //                    }
        //                }
        //                if (!string.IsNullOrWhiteSpace(lineString))
        //                {
        //                    forEnque.Add(lineString);
        //                }
        //                EnQueue(nType, string.Join(System.Environment.NewLine, forEnque));
        //                nType = MSGTYPE.UNKNOWN;
        //                Started = false;
        //                state.sb = new StringBuilder();// Received data string.  
        //            }
        //            //break;
        //        }
        //    }

        //}

        //protected override void ParseMessage(string Msg)
        //{
        //    string[] LineData = Msg.Split(new string[] { System.Environment.NewLine }, StringSplitOptions.None);

        //    //List<string> listData = new List<string>();
        //    bool Started = false;
        //    List<string> forEnque = new List<string>();
        //    string lineString = string.Empty;
        //    int nType = MSGTYPE.UNKNOWN;
        //    foreach (string ln in LineData)
        //    {
        //        //if (ln.StartsWith("\u001b"))// "m_StartStringList[0]))  // <ESC>CS3 이라는 가정하에 프로그램 함.
        //        if (ln.StartsWith("\u001b"))// "m_StartStringList[0]))  // <ESC>CS3 이라는 가정하에 프로그램 함.
        //        {
        //            Started = true;
        //            //listData = new List<string>();
        //        }
        //        if (Started)
        //        {
        //            //listData.Add(ln);
        //            // 에러의 종류가 여러가지가 있을 수 있음.
        //            // 확인바람.
        //            // 메시지 포맷이 구체적으로 파악되지 않음.
        //            if (Msg.Contains("Error") == true && LineData.Length >= 17)
        //            {
        //                nType = MSGTYPE.ERR;
        //            }
        //            else if (Msg.Contains("DC Cond.") == true && LineData.Length >= 22 )
        //            {
        //                nType = MSGTYPE.COND_ADJ;
        //            }
        //            else if (Msg.Contains("DM pH") == true && LineData.Length >= 19)
        //            {
        //                nType = MSGTYPE.MSR_PH;
        //            }
        //            else if (Msg.Contains("DM Cond.") == true && LineData.Length >= 21)
        //            {
        //                nType = MSGTYPE.MSR_COND;
        //            }
        //            else 
        //            {
        //                // do Nothing
        //            }
        //            if (nType != MSGTYPE.UNKNOWN)
        //            {
        //                foreach (string oneline in LineData)
        //                {
        //                    //if (oneline.Length < 12)
        //                    //    continue;
        //                    if (string.IsNullOrWhiteSpace(oneline)) // 현재 라인이 공백줄이면 
        //                    {
        //                        if (string.IsNullOrWhiteSpace(lineString)) // 이전 라인이 없으면 다음 라인 읽기
        //                        {
        //                            continue;
        //                        }
        //                        else // 이전 라인이 있었으면 해당 라인 저장
        //                        {
        //                            forEnque.Add(lineString);
        //                            lineString = string.Empty;
        //                        }
        //                    }
        //                    else // 현재 라인이 공백이 아니면
        //                    {
        //                        if (string.IsNullOrWhiteSpace(oneline.Length > 11 ? oneline.Substring(0, 12) : oneline)) // 왼쪽이 없으면 더해주기
        //                        {
        //                            lineString += oneline.Trim();
        //                        }
        //                        else // 현재 라인이 새 항목이면
        //                        {
        //                            if (!string.IsNullOrWhiteSpace(lineString)) // 이전 라인 항목이 있었으면 저장
        //                            {
        //                                forEnque.Add(lineString);
        //                            }
        //                            lineString = oneline;
        //                        }
        //                    }
        //                }
        //                if (!string.IsNullOrWhiteSpace(lineString))
        //                {
        //                    forEnque.Add(lineString);
        //                }
        //                EnQueue(nType, string.Join(System.Environment.NewLine, forEnque));
        //                nType = MSGTYPE.UNKNOWN;
        //                Started = false;
        //                state.sb = new StringBuilder();// Received data string.  
        //            }
        //            break;
        //        }
        //    }

        //}
    }
}
