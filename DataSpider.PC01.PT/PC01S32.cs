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
    public class PC01S32 : PC00B04
    {
        public abstract class A1_CommState
        {
            public const int IDLE = 0;
            public const int WAITING = 1;
            public const int FRAME_RECVED = 2;
            public const int HAVE_DATA_TO_SEND = 3;
        }
        
        DateTime dtMsrDateTime;
        
        string INF_TID_VAL = string.Empty;
        string INF_OID_VAL = string.Empty;
        int nSeqCounter = 0;
        int ninitErrCount = 0;

        string strErrCode;
        string strErrText;

        public PC01S32() : base()
        {
        }

        public PC01S32(PC01F01 pOwner, string equipType, string equipName, string connectionInfo, string extraInfo, int nCurNo, bool bAutoRun = false) : base(pOwner, equipType, equipName, connectionInfo, extraInfo, nCurNo, bAutoRun)
        {
        }

        public PC01S32(PC01F01 pOwner, DataRow dr, int nCurNo, bool bAutoRun = false) : this(pOwner, dr["EQUIP_TYPE_NM"].ToString(), dr["EQUIP_NM"].ToString(), dr["CONNECTION_INFO"].ToString(), dr["EXTRA_INFO"].ToString(), nCurNo, bAutoRun)
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

            string typeName = string.Empty;
            string msgString = string.Empty;
            byte[] sendMsg = new byte[1];
            dtMsrDateTime = System.DateTime.Now;
                
            for (int nCh = 0; nCh < Msg.Length; nCh++)
            {
                switch (CommStatus)
                {
                    //초기에 체크정보를 보내고 idle 상태로 변경(공통 Thread에서)
                    //sc 수신후 정보가 맞으면 waiting으로 변경 틀리면 idle 유지
                    case A1_CommState.IDLE:
                        if (Msg[nCh] == ASCII.EOT)// Sync SC
                        {
                            string[] sp_sync = Msg.Split((char)ASCII.LF);
                            nSeqCounter = int.Parse(sp_sync[sp_sync.Length-3].ToString().Trim());

                            if (blockCheckSum(Msg))
                            {
                                CommStatus = A1_CommState.WAITING;
                            }
                            else // CheckSum Error
                            {
                                ninitErrCount++;
                                fileLog.WriteData("CSE", "ParseMessage", $"CheckSum Error");

                                if (ninitErrCount == 8)
                                {
                                    byte[] sendMsg1 = new byte[9];

                                    sendMsg1[0] = ASCII.SOH;
                                    sendMsg1[1] = ASCII.LF;
                                    sendMsg1[2] = ASCII.LF;
                                    sendMsg1[3] = ASCII.STX;
                                    sendMsg1[4] = ASCII.LF;
                                    sendMsg1[5] = ASCII.ETX;
                                    sendMsg1[6] = ASCII.LF;
                                    sendMsg1[7] = ASCII.EOT;
                                    sendMsg1[8] = ASCII.LF;

                                    state.workSocket.Send(sendMsg1);

                                    ninitErrCount = 0;
                                }
                            }
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
                            if (Msg[nCh] == ASCII.STX || Msg[nCh] == ASCII.SOH)
                            {
                                msgString = string.Empty;
                                msgString += Msg[nCh];
                            }
                            else if (Msg[nCh] == ASCII.LF)
                            {
                                int iResult;                                                                
                                StringBuilder ssb = new StringBuilder();

                                sendMsg = new byte[1];
                                                               
                                if (blockCheckSum(Msg))
                                {
                                    // 데이터 해석 가능
                                    string[] sp_cr = msgString.Split('_');
                                        
                                    if (sp_cr[0].Equals("01"))  //Result Time
                                    {
                                        if (sp_cr[1].ToString().Trim() != "")
                                            PC00U01.TryParseExact(sp_cr[1], out dtMsrDateTime);
                                            
                                        ssb.AppendLine($"INF_RESULT_TIME, {dtMsrDateTime:yyyy-MM-dd HH:mm:ss}, {dtMsrDateTime} ");                                            
                                    }
                                    if (sp_cr[0].Equals("53"))  //Order ID
                                    {
                                        INF_OID_VAL = sp_cr[1].ToString().Replace("\r\n", "");
                                        ssb.AppendLine($"INF_ORDER, {dtMsrDateTime:yyyy-MM-dd HH:mm:ss}, {sp_cr[1]} ");
                                    }
                                    if (sp_cr[0].Equals("55"))  //Test ID
                                    {
                                        INF_TID_VAL = sp_cr[1].ToString().Replace("\r\n", "");
                                        typeName = GetTypeName(INF_TID_VAL);                                    
                                    }                                       
                                    if (sp_cr[0].Equals("00"))  //Result Data
                                    {   
                                        ssb.AppendLine($"RESULTS_{typeName}_VAL, {dtMsrDateTime:yyyy-MM-dd HH:mm:ss}, {sp_cr[1]} ");
                                        ssb.AppendLine($"RESULTS_{typeName}_UNIT, {dtMsrDateTime:yyyy-MM-dd HH:mm:ss}, {sp_cr[2]}");                                            
                                        ssb.AppendLine($"RESULTS_{typeName}_XFLAG, {dtMsrDateTime:yyyy-MM-dd HH:mm:ss}, {sp_cr[3]}");
                                        ssb.AppendLine($"RESULTS_{typeName}_CFLAG, {dtMsrDateTime:yyyy-MM-dd HH:mm:ss}, {sp_cr[5]}");
                                        ssb.AppendLine($"RESULTS_{typeName}_QFLAG, {dtMsrDateTime:yyyy-MM-dd HH:mm:ss}, {sp_cr[6]}");
                                    }
                                        
                                    EnQueue(MSGTYPE.MEASURE, ssb.ToString());
                                    //sendMsg[0] = ASCII.ACK;
                                    fileLog.WriteData("OK", "ParseMessage", $"Normal ");
                                }
                                else // CheckSum Error
                                {
                                    ninitErrCount++;
                                    fileLog.WriteData("CSE", "ParseMessage", $"CheckSum Error");

                                    if (ninitErrCount == 8)
                                    {
                                        byte[] sendMsg1 = new byte[9];

                                        sendMsg1[0] = ASCII.SOH;
                                        sendMsg1[1] = ASCII.LF;
                                        sendMsg1[2] = ASCII.LF;
                                        sendMsg1[3] = ASCII.STX;
                                        sendMsg1[4] = ASCII.LF;
                                        sendMsg1[5] = ASCII.ETX;
                                        sendMsg1[6] = ASCII.LF;
                                        sendMsg1[7] = ASCII.EOT;
                                        sendMsg1[8] = ASCII.LF;

                                        state.workSocket.Send(sendMsg1);

                                        ninitErrCount = 0;
                                    }
                                }
                                
                                state.sb = new StringBuilder();
                                state.sb.Append(Msg.Substring(nCh + 1));

                                //LF 단위로 처리하기때문에 초기화
                                msgString = string.Empty;
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
        public bool blockCheckSum(string Msg)
        {
            //처음부터 SC의 LF까지의 합의 나눈값이 BCS 값임
            int summation = 0;
            int chkCnt = 0;
            //임시 설정 무조건 ok            
            bool ChkVal = true;

            string[] sp_sync = Msg.Split((char)ASCII.LF);

            foreach (char ch in Msg.ToCharArray())
            {
                if (ch != (char)ASCII.CR)
                {
                    summation += (byte)ch;
                }

                if (ch == (char)ASCII.LF)
                {
                    chkCnt++;
                }

                if (chkCnt == sp_sync.Length - 3)
                    break;
            }

            int calc_val = summation % 1000;

            if (calc_val == int.Parse(sp_sync[sp_sync.Length - 2].ToString().Replace("\r\n","")))
                ChkVal = true;           

            return ChkVal;
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
            DataTable dtTypeList = m_sqlBiz.GetCommonCode("ITEM_TYPE_CBH", strTypeCode, ref strErrCode, ref strErrText);

            if (dtTypeList != null && dtTypeList.Rows.Count > 0)
            {
                strTCode = dtTypeList.Rows[0][2].ToString();
            }

            return strTCode;
        }
    }
}
