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
    /// NOVA LIS
    /// 
    /// </summary>
    public class PC01S31 : PC00B02
    {
        public abstract class A1_CommState
        {
            public const int IDLE = 0;
            public const int WAITING = 1;
            public const int FRAME_RECVED = 2;
            public const int HAVE_DATA_TO_SEND = 3;
        }
        int nFrameNumber = 0;
        DateTime dtDateTime;
        DateTime dtMsrDateTime;
        string _SENDER_NAME_VAL = string.Empty;
        string _RECVID_VAL = string.Empty;
        string _INSTRUCTION_VAL = string.Empty;
        string _PROCESSID_VAL = string.Empty;
        string _VERSION_VAL = string.Empty;
        string _LAPID_VAL = string.Empty;
        string _EXPIRES_VAL = string.Empty;
        string _SID_VAL = string.Empty;
        string _ISID_VAL = string.Empty;
        string _UTID_VAL = string.Empty;
        string _PRIORITY_VAL = string.Empty;
        string _ACTIONCD_VAL = string.Empty;
        string _RDATETIME_VAL = string.Empty;
        string _SD_VAL = string.Empty;
        string _COMMENT_VAL = string.Empty;
        string _TCODE_VAL = string.Empty;
        string _MESSAGEDATETIME_VAL = string.Empty;

        string strErrCode;
        string strErrText;

        public PC01S31() : base()
        {
        }

        public PC01S31(PC01F01 pOwner, string equipType, string equipName, string connectionInfo, string extraInfo, int nCurNo, bool bAutoRun = false) : base(pOwner, equipType, equipName, connectionInfo, extraInfo, nCurNo, bAutoRun)
        {
        }

        public PC01S31(PC01F01 pOwner, DataRow dr, int nCurNo, bool bAutoRun = false) : this(pOwner, dr["EQUIP_TYPE_NM"].ToString(), dr["EQUIP_NM"].ToString(), dr["CONNECTION_INFO"].ToString(), dr["EXTRA_INFO"].ToString(), nCurNo, bAutoRun)
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
        /// <summary>
        /// Receiving Device
        /// </summary>
        /// <param name="Msg"></param>
        protected override void ParseMessage(string Msg)
        {
            // listData에 저장용 메시지를 저장함. 
            List<string> listData = new List<string>();
            DateTime dt = new DateTime();

            string msgString = string.Empty;
            byte[] sendMsg = new byte[1];

            for (int nCh = 0; nCh < Msg.Length; nCh++)
            {
                switch (CommStatus)
                {
                    case A1_CommState.IDLE:
                        if (Msg[nCh] == ASCII.ENQ)// Send Ack and Reset FrameNumber to 1
                        {
                            sendMsg = new byte[1];
                            sendMsg[0] = ASCII.ACK;
                            state.workSocket.Send(sendMsg);
                            state.sb = new StringBuilder();
                            state.sb.Append(Msg.Substring(nCh + 1));
                            nFrameNumber = 1;
                            CommStatus = A1_CommState.WAITING;
                        }
                        break;
                    case A1_CommState.WAITING:
                        // TimeOut Check는 별도의 thread에서
                        if (Msg[nCh] == ASCII.EOT)
                        {
                            state.sb = new StringBuilder();
                            state.sb.Append(Msg.Substring(nCh + 1));
                            CommStatus = A1_CommState.IDLE;
                        }
                        else
                        {
                            msgString += Msg[nCh];
                            if (Msg[nCh] == ASCII.STX)
                            {
                                msgString = string.Empty;
                                msgString += Msg[nCh];
                            }
                            else if (Msg[nCh] == ASCII.LF)
                            {
                                int iResult;
                                string TestName = string.Empty;
                                string typeName = string.Empty;
                                StringBuilder ssb = new StringBuilder();

                                sendMsg = new byte[1];

                                //임시 처리 제외
                                if (int.TryParse(msgString.Substring(1, 1), out iResult) == true && iResult == nFrameNumber)
                                {
                                   if (AstmCalcedCheckSum(msgString) == AstmRecvedCheckSum(msgString))
                                    {
                                        // 데이터 해석 가능
                                        string[] sp_cr = msgString.Split((char)ASCII.CR);
                                        string[] sp_vert = sp_cr[0].Substring(1).Split('|');
                                        if (sp_vert[0].EndsWith("H"))
                                        {
                                            _LAPID_VAL = string.Empty;
                                            _EXPIRES_VAL = string.Empty;
                                            _SID_VAL = string.Empty;
                                            _ISID_VAL = string.Empty;
                                            _SD_VAL = string.Empty;
                                            _COMMENT_VAL = string.Empty;
                                            _TCODE_VAL = string.Empty;
                                            dtMsrDateTime = DateTime.MinValue;

                                            typeName = "MHR";                                            
                                            _SENDER_NAME_VAL = sp_vert[4];
                                            _RECVID_VAL = sp_vert[9];
                                            _INSTRUCTION_VAL = sp_vert[10];
                                            _PROCESSID_VAL = sp_vert[11];
                                            _VERSION_VAL = sp_vert[12];
                                            _MESSAGEDATETIME_VAL = sp_vert[13];
                                        }

                                        if (sp_vert[0].EndsWith("P"))
                                        {
                                            typeName = "SIR";
                                            _LAPID_VAL=sp_vert[3];
                                            if (sp_vert.Length >= 8)
                                                _EXPIRES_VAL = sp_vert[7];
                                            else
                                                _EXPIRES_VAL = " ";
                                        }

                                        if (sp_vert[0].EndsWith("O"))
                                        {
                                            typeName = "TOR";
                                            _SID_VAL = sp_vert[2];
                                            _ISID_VAL = sp_vert[3];
                                            _UTID_VAL = sp_vert[4];
                                            _PRIORITY_VAL = sp_vert[5];
                                            _ACTIONCD_VAL = sp_vert[11];
                                            _SD_VAL = sp_vert[15];
                                            _RDATETIME_VAL = sp_vert[22];
                                        }

                                        //23.09.20 항목에 대한 정의 필요.
                                        if (sp_vert[0].EndsWith("R"))
                                        {
                                            typeName = "RR";
                                            string[] utid = sp_vert[2].Split('^');
                                            if (dtMsrDateTime == DateTime.MinValue)
                                            {
                                                //PC00U01.TryParseExact(sp_vert[11], out dtMsrDateTime);
                                                // KWC 202301123
                                                PC00U01.TryParseExact(sp_vert[12], out dtMsrDateTime);
                                            }

                                            TestName = GetTypeName(utid[3]);

                                            ssb.AppendLine($"{typeName}_{TestName}_VAL, {dtMsrDateTime:yyyy-MM-dd HH:mm:ss}, {sp_vert[3]} ");
                                            ssb.AppendLine($"{typeName}_{TestName}_UNIT, {dtMsrDateTime:yyyy-MM-dd HH:mm:ss}, {sp_vert[4]}");                                            
                                            ssb.AppendLine($"{typeName}_{TestName}_AFLAG, {dtMsrDateTime:yyyy-MM-dd HH:mm:ss}, {sp_vert[6]}");
                                            ssb.AppendLine($"{typeName}_{TestName}_FLAG, {dtMsrDateTime:yyyy-MM-dd HH:mm:ss}, {sp_vert[8]}");
                                            ssb.AppendLine($"{typeName}_{TestName}_OID, {dtMsrDateTime:yyyy-MM-dd HH:mm:ss}, {sp_vert[10]}");
                                            
                                        }
                                        if (sp_vert[0].EndsWith("C"))
                                        {
                                            typeName = "CR";

                                            if(string.IsNullOrEmpty(_COMMENT_VAL))
                                                _COMMENT_VAL += sp_vert[3];
                                            else
                                                _COMMENT_VAL += " ; " + sp_vert[3];
                                        }
                                        // R이 들어 오지 않은 경우 측정시각을 알수 없으므로 저장하지 않음.
                                        if (sp_vert[0].EndsWith("L") && dtMsrDateTime != DateTime.MinValue)
                                        {
                                            typeName = "HMR";
                                            
                                            ssb.AppendLine($"SVRTIME, {dtMsrDateTime:yyyy-MM-dd HH:mm:ss}, {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
                                            ssb.AppendLine($"{typeName}_SENDER_NAME_VAL, {dtMsrDateTime:yyyy-MM-dd HH:mm:ss}, {_SENDER_NAME_VAL}");
                                            ssb.AppendLine($"{typeName}_RECVID_VAL, {dtMsrDateTime:yyyy-MM-dd HH:mm:ss}, {_RECVID_VAL}");
                                            ssb.AppendLine($"{typeName}_INSTRUCTION_VAL, {dtMsrDateTime:yyyy-MM-dd HH:mm:ss}, {_INSTRUCTION_VAL}");
                                            ssb.AppendLine($"{typeName}_PROCESSID_VAL, {dtMsrDateTime:yyyy-MM-dd HH:mm:ss}, {_PROCESSID_VAL}");
                                            ssb.AppendLine($"{typeName}_VERSION_VAL, {dtMsrDateTime:yyyy-MM-dd HH:mm:ss}, {_VERSION_VAL}");
                                            ssb.AppendLine($"{typeName}_MESSAGEDATETIME_VAL, {dtMsrDateTime:yyyy-MM-dd HH:mm:ss}, {_MESSAGEDATETIME_VAL}");

                                            typeName = "SIR";
                                            ssb.AppendLine($"{typeName}_LAPID_VAL, {dtMsrDateTime:yyyy-MM-dd HH:mm:ss},  {_LAPID_VAL}");                                           
                                            ssb.AppendLine($"{typeName}_EXPIRES_VAL, {dtMsrDateTime:yyyy-MM-dd HH:mm:ss},  {_EXPIRES_VAL}");

                                            typeName = "TOR";
                                            ssb.AppendLine($"{typeName}_SID_VAL, {dtMsrDateTime:yyyy-MM-dd HH:mm:ss}, {_SID_VAL}");
                                            ssb.AppendLine($"{typeName}_ISID_VAL, {dtMsrDateTime:yyyy-MM-dd HH:mm:ss}, {_ISID_VAL}");
                                            ssb.AppendLine($"{typeName}_UTID_VAL, {dtMsrDateTime:yyyy-MM-dd HH:mm:ss}, {_UTID_VAL}");
                                            ssb.AppendLine($"{typeName}_PRIORITY_VAL, {dtMsrDateTime:yyyy-MM-dd HH:mm:ss}, {_PRIORITY_VAL}");
                                            ssb.AppendLine($"{typeName}_ACTIONCD_VAL, {dtMsrDateTime:yyyy-MM-dd HH:mm:ss}, {_ACTIONCD_VAL}");
                                            ssb.AppendLine($"{typeName}_SD_VAL, {dtMsrDateTime:yyyy-MM-dd HH:mm:ss}, {_SD_VAL}");
                                            ssb.AppendLine($"{typeName}_RDATETIME_VAL, {dtMsrDateTime:yyyy-MM-dd HH:mm:ss}, {_RDATETIME_VAL}");

                                            if (!string.IsNullOrEmpty(_COMMENT_VAL))
                                            {
                                                typeName = "CR";
                                                ssb.AppendLine($"{typeName}_COMMENT_VAL, {dtMsrDateTime:yyyy-MM-dd HH:mm:ss}, {_COMMENT_VAL}");
                                            }

                                            typeName = "MTR";
                                            ssb.AppendLine($"{typeName}_TCODE_VAL, {dtMsrDateTime:yyyy-MM-dd HH:mm:ss}, {sp_vert[2]}");
                                        }

                                        EnQueue(MSGTYPE.MEASURE, ssb.ToString());
                                        sendMsg[0] = ASCII.ACK;
                                        fileLog.WriteData("ACK", "ParseMessage", $"Normal ");
                                        nFrameNumber = (nFrameNumber + 1) % 8;
                                    }
                                    else // CheckSum Error => Send NAK;
                                    {
                                        sendMsg[0] = ASCII.NAK;
                                        fileLog.WriteData("NAK", "ParseMessage", $"CheckSum Error");
                                    }
                                }
                                else
                                { // Bad Frame Error
                                    sendMsg[0] = ASCII.NAK;
                                    fileLog.WriteData("NAK", "ParseMessage", $"Bad Frame Error");
                                }
                                fileLog.WriteData(BitConverter.ToString(sendMsg, 0, sendMsg.Length), "ParseMessage", $"Send BYTE({sendMsg.Length})");
                                state.workSocket.Send(sendMsg);
                                state.sb = new StringBuilder();
                                state.sb.Append(Msg.Substring(nCh + 1));
                            }
                            else { }
                        }
                        break;
                    default:
                        CommStatus = A1_CommState.IDLE;
                        break;
                }
            }
        }
        public string CheckSum(string Msg, int nLength)
        {
            byte summation = 0;

            for (int nCh = 0; nCh < nLength; nCh++)
            {
                summation += (byte)Msg[nCh];
            }
            return summation.ToString("X02");
        }
        // 수신 메시지를 이용해서 계산한  CheckSum
        string AstmCalcedCheckSum(string Msg)
        {
            byte byteCheckSum = 0;
            bool bCheckSum = false;
            foreach (char ch in Msg.ToCharArray())
            {
                switch ((int)ch)
                {
                    case ASCII.STX:
                        byteCheckSum = 0;
                        bCheckSum = true;
                        break;
                    case ASCII.ETX:
                    case ASCII.ETB:
                        byteCheckSum += (byte)ch;
                        bCheckSum = false;
                        break;
                    default:
                        if (bCheckSum == true)
                            byteCheckSum += (byte)ch;
                        break;
                }
            }
            return byteCheckSum.ToString("X02");
        }
        //수신된 CheckSum
        string AstmRecvedCheckSum(string Msg)
        {
            string csString = string.Empty;
            int state = 0;
            foreach (char ch in Msg.ToCharArray())
            {
                switch (state)
                {
                    case 1:
                    case 2:
                        csString += ch;
                        state++;
                        break;
                    default:
                        break;
                }
                switch ((int)ch)
                {
                    case ASCII.ETX:
                    case ASCII.ETB:
                        state = 1;
                        break;
                    default:
                        break;
                }
            }
            return csString.PadLeft(2, '0');
        }

        private string GetTypeName(string strTypeCode)
        {
            string strTCode = "";
            DataTable dtTypeList = m_sqlBiz.GetCommonCode("ITEM_TYPE_CB", strTypeCode, ref strErrCode, ref strErrText);

            if (dtTypeList != null && dtTypeList.Rows.Count > 0)
            {
                strTCode = dtTypeList.Rows[0][2].ToString();
            }

            return strTCode;
        }
    }
}
