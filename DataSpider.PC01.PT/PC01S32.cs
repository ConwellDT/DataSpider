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
using System.Windows.Forms;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json.Linq;

namespace DataSpider.PC01.PT
{
    /// <summary>
    /// CEDEX BIO HT
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

        public abstract class BLOCK_CODE
        {
            public const string IDLE_OR_SYNC_BLOCK = "00";
            public const string CALIBRATION_RESULT = "02";
            public const string CONTROL_RESULT = "03";
            public const string PATIENT_RESULT = "04";
            public const string SAMPLE_RESULT = "04";   // Extended Host Interface?

            public const string CALIBRATION_RESULT_WITH_LOT_INFO = "05";
            public const string CONTROL_RESULT_WITH_LOT_INFO = "06";
            public const string PATIENT_RESULT_WITH_LOT_INFO = "07";
            public const string SAMPLE_RESULT_WITH_LOT_INFO = "07"; // Extended Host Interface?

            public const string RESULT_REQUEST_RESPONSE = "08";
            public const string RESULT_REQUEST = "09";
            public const string ORDER_ENTRY = "10";
            public const string ORDER_DELETTION = "11";

            public const string MCS_REQUEST = "60";     // MCS: Multi-Configuration Service
            public const string SLOT_CONFIGURATION = "61";
            public const string SAMPLE_TUBE_INFORMATION = "62";
            public const string SERVICE_REQUEST_RESPONSE = "69";



            public const string SYSTEM_STATUS_REQUEST = "90";
            public const string SYSTEM_STATUS_RESPONSE = "91";

            public const string PROTOCOL_VERSION_REQUEST = "92";
            public const string PROTOCOL_VERSION_DATA = "93";

            public const string DATE_AND_TIME_REQUEST = "94";
            public const string DATE_AND_TIME_RESPONSE = "95";

            public const string SERIAL_NUMBER_REQUEST = "96";
            public const string SERIAL_NUMBER_RESPONSE = "97";

            public const string CONTROL_MESSAGE = "99";
        }

        public abstract class LINE_CODE
        {
            public const string RESULT_DATA = "00";
            public const string RESULT_TIME = "01";
            public const string CONTROL_ID = "02";

            public const string RESULT_TYPE_SELECTION = "10";

            public const string ACCESS_DATE_TIME = "15";

            public const string SERVICE_SELECTION = "40";

            public const string PAITIENT_ID = "50";

            public const string ORDER_ID = "53";

            public const string TEST_ID = "55";
            public const string SAMPLE_NAME = "56";

            public const string PROTOCOL_VERSION = "98";
            public const string GENERAL_ERROR_CODE = "99";
        }

        public abstract class RESULT_TYPE_SELECTION
        {

            public const string UNSPECIFIC_RESULT_REQUEST = "01";
            public const string RESERVED = "02";
            public const string PATIENT_RESULT_03 = "03";
            public const string PATIENT_RESULT_04 = "04";
            public const string PATIENT_RESULT_05 = "05";
            public const string CONTROL_RESULT_BY_CONTROL_ID = "06";
            public const string SINGLE_PATIENT_RESULT = "07";
            public const string SINGLE_CALIBRATION_RESULT = "08";
            public const string CONTROL_RESULT_BY_TEST_ID = "09";
            public const string SINGLE_AVAILABLE_RESULT_WITH_INFO = "11";
            public const string SINGLE_PATIENT_RESULT_WITH_INFO = "17";
            public const string SINGLE_SAMPLE_RESULT_WITH_INFO = "17";
            public const string SINGLE_CALIBRATION_RESULT_WITH_INFO = "18";
            public const string SINGLE_CONTROL_RESULT_WITH_INFO = "19";

        }

        public abstract class SERVICE_SELECTION { 
            //BLOCK 60 
            public const string SLOT_CONFIGURATION = "0"; // RESPONSE: BLOCK 61
            public const string ONBOARD_SAMPLE_TURBES_WITHOUT_ORDERS = "1"; //RESPONSE: BLOCK 62
            public const string CAL_CS_STATUS_PER_TEST = "2"; // RESPONSE: BLOCK 63;
            public const string RESEREVED_SEVICE_SELECTION = "3"; //
            public const string LIST_OF_ALL_KNOWN_SAMPLE_TUBES = "4"; //RESPONSE : BLOCK 62
        }


        Dictionary<string, string> m_dicConfigInfo = new Dictionary<string, string>();
        string m_InstrumentCode = "14";
        string m_InstrumentIdentifier = "DATASPIDER      ";
        DateTime dtMsrDateTime;
        
        string INF_TID_VAL = string.Empty;
        string INF_OID_VAL = string.Empty;
        int ninitErrCount = 0;

        string strErrCode;
        string strErrText;

        private System.Threading.Timer m_WatchDogTimer;
        private DateTime m_lastWriteTime=DateTime.MinValue;
        private int m_SequenceCounter = -1;
        private int m_ProtocolVersion = -1;
        private int m_SystemStatus = -1;
        private int m_DateAndTime = -1;
        private int m_SerialNumber= -1;



        public PC01S32() : base()
        {
        }

        public PC01S32(PC01F01 pOwner, DataRow dr, string equipType, string equipName, string connectionInfo, string extraInfo, int nCurNo, bool bAutoRun = false) : base(pOwner, dr, equipType, equipName, connectionInfo, extraInfo, nCurNo, bAutoRun)
        {
        }

        public PC01S32(PC01F01 pOwner, DataRow dr, int nCurNo, bool bAutoRun = false) : this(pOwner, dr, dr["EQUIP_TYPE_NM"].ToString(), dr["EQUIP_NM"].ToString(), dr["CONNECTION_INFO"].ToString(), dr["EXTRA_INFO"].ToString(), nCurNo, bAutoRun)
        {
            ReadConfigInfo();
            ReadConnectionInfoForSocket();
            ReadConfigForCedexBioHt();

            if (m_AutoRun == true)
            {
                m_Thd = new Thread(ThreadJob);
                m_Thd.Start();
            }

            // 10초 후 부터 WatchDog Timer 가동
            m_WatchDogTimer = new System.Threading.Timer(WatchDogTimerCallBack, null, 10000, 1000);

        }
        /// <summary>
        /// MA_EQUIPMENT_CD 테이블 CONFIG_INFO 컬럼값 읽기
        /// </summary>
        protected void ReadConfigForCedexBioHt()
        {
            try
            {
                if (drEquipment == null)
                {
                    return;
                }
                string result = string.Empty;
                string[] arrConfigInfo = drEquipment["CONFIG_INFO"]?.ToString().Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
                int index = -1;
                string infoName = string.Empty;
                foreach (string info in arrConfigInfo)
                {
                    index = info.IndexOf("=");
                    if (index < 0)
                    {
                        continue;
                    }
                    m_dicConfigInfo.TryAdd(info.Substring(0, index).Trim(), info.Substring(index + 1).Trim());
                }

                string property = string.Empty;


                property = "INSTRUMENT_CODE";
                string InstrumentCode = m_dicConfigInfo.TryGetValue(property);
                if (!string.IsNullOrWhiteSpace(InstrumentCode))
                    m_InstrumentCode = InstrumentCode;
                listViewMsg.UpdateMsg($"{property} : {m_InstrumentCode}");
                property = "INSTRUMENT_IDENTIFIER";
                string InstrumentIdentifier = m_dicConfigInfo.TryGetValue(property);
                if (!string.IsNullOrWhiteSpace(InstrumentIdentifier))
                    m_InstrumentIdentifier = string.Format("{0:16}", InstrumentIdentifier);
                listViewMsg.UpdateMsg($"{property} : {m_InstrumentIdentifier}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
                listViewMsg.UpdateMsg($"Exceptioin - ReadConfigForCedexBioHt ({ex})", false, true, true, PC00D01.MSGTERR);
            }
        }

        private void WatchDogTimerCallBack(object obj)
        {
            try
            {
                if (!bTerminal)
                {
                    TimeSpan ts = DateTime.Now - m_lastWriteTime;
                    if (CommStatus == A1_CommState.WAITING)
                    {
                        if (ts.TotalSeconds > 180 && IsConnected == true)
                        {
                            // TimeOut
                            fileLog.WriteData($"{ts.TotalSeconds} TimeOut", "WatchDogTimerCallBack", "TimeOut Error");
                            m_SequenceCounter = -1;
                            m_ProtocolVersion = -1;
                            m_SystemStatus = -1;
                            m_SerialNumber = -1;
                            CommStatus = A1_CommState.IDLE;
                        }

                    }
                    else if (CommStatus == A1_CommState.IDLE)
                    {
                        if (ts.TotalSeconds > 60 && IsConnected == true)
                        {
                            if (m_SequenceCounter == -1)
                                SendSyncBlockMsg();
                            //else if (m_ProtocolVersion == -1)
                            //    SendProtocolVersionRequestMsg();
                            //else if (m_SystemStatus == -1)
                            //    SendSystemStatusRequestMsg();
                            //else if (m_DateAndTime == -1)
                            //    SendDateAndTimeRequestMsg();
                            //else if (m_SerialNumber == -1)
                            //    SendSerialNumberRequestMsg();
                            else
                                SendResultRequestMsg();
                            m_lastWriteTime = DateTime.Now;
                        }
                    }
                    else
                    { }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
                listViewMsg.UpdateMsg($"Exceptioin - WatchDogTimerCallBack ({ex})", false, true, true, PC00D01.MSGTERR);
            }
        }
        private void SendMcsRequestMsg()
        {
            string SendBlock = string.Empty;
            string BlockCode = BLOCK_CODE.MCS_REQUEST;
            SendBlock = string.Empty;
            SendBlock += $"{(char)ASCII.SOH}{(char)ASCII.LF}";
            SendBlock += $"{m_InstrumentCode} {m_InstrumentIdentifier} {BlockCode}{(char)ASCII.LF}";
            SendBlock += $"{(char)ASCII.STX}{(char)ASCII.LF}";
            SendBlock += $"{LINE_CODE.SERVICE_SELECTION} {SERVICE_SELECTION.ONBOARD_SAMPLE_TURBES_WITHOUT_ORDERS}";
            SendBlock += $"{(char)ASCII.ETX}{(char)ASCII.LF}";
            SendBlock += $"{m_SequenceCounter}{(char)ASCII.LF}";
            SendBlock += $"{BuildBlockCheckSum(SendBlock)}{(char)ASCII.LF}";
            SendBlock += $"{(char)ASCII.EOT}{(char)ASCII.LF}";

            state.workSocket.Send(Encoding.UTF8.GetBytes(SendBlock));
            fileLog.WriteData($"{SendBlock}", "SendMcsRequestMsg", "String");
            fileLog.WriteData($"{BitConverter.ToString(Encoding.UTF8.GetBytes(SendBlock), 0, SendBlock.Length)}", "SendMcsRequestMsg", "Byte Array");

        }

        private void SendResultRequestMsg()
        {
            string SendBlock = string.Empty;
            string BlockCode = BLOCK_CODE.RESULT_REQUEST;
            SendBlock = string.Empty;
            SendBlock += $"{(char)ASCII.SOH}{(char)ASCII.LF}";
            SendBlock += $"{m_InstrumentCode} {m_InstrumentIdentifier} {BlockCode}{(char)ASCII.LF}";
            SendBlock += $"{(char)ASCII.STX}{(char)ASCII.LF}";
            //SendBlock += $"{LINE_CODE.RESULT_TYPE_SELECTION} {RESULT_TYPE_SELECTION.SINGLE_SAMPLE_RESULT_WITH_INFO}";
            SendBlock += $"{LINE_CODE.RESULT_TYPE_SELECTION} {RESULT_TYPE_SELECTION.UNSPECIFIC_RESULT_REQUEST}";
            SendBlock += $"{(char)ASCII.ETX}{(char)ASCII.LF}";
            SendBlock += $"{m_SequenceCounter}{(char)ASCII.LF}";
            SendBlock += $"{BuildBlockCheckSum(SendBlock)}{(char)ASCII.LF}";
            SendBlock += $"{(char)ASCII.EOT}{(char)ASCII.LF}";

            state.workSocket.Send(Encoding.UTF8.GetBytes(SendBlock));
            fileLog.WriteData($"{SendBlock}", "SendResultRequestMsg", "String" );
            fileLog.WriteData($"{BitConverter.ToString(Encoding.UTF8.GetBytes(SendBlock), 0, SendBlock.Length)}", "SendResultRequestMsg", "Byte Array");

        }
        private void SendSystemStatusRequestMsg()
        {
            string SendBlock = string.Empty;
            string BlockCode = BLOCK_CODE.SYSTEM_STATUS_REQUEST;
            SendBlock = string.Empty;
            SendBlock += $"{(char)ASCII.SOH}{(char)ASCII.LF}";
            SendBlock += $"{m_InstrumentCode} {m_InstrumentIdentifier} {BlockCode}{(char)ASCII.LF}";
            SendBlock += $"{(char)ASCII.STX}{(char)ASCII.LF}";
            SendBlock += $"{(char)ASCII.ETX}{(char)ASCII.LF}";
            SendBlock += $"{m_SequenceCounter}{(char)ASCII.LF}";
            SendBlock += $"{BuildBlockCheckSum(SendBlock)}{(char)ASCII.LF}";
            SendBlock += $"{(char)ASCII.EOT}{(char)ASCII.LF}";

            state.workSocket.Send(Encoding.UTF8.GetBytes(SendBlock));
            fileLog.WriteData($"{SendBlock}", "SendSystemStatusRequestMsg", "String");
            fileLog.WriteData($"{BitConverter.ToString(Encoding.UTF8.GetBytes(SendBlock), 0, SendBlock.Length)}", "SendSystemStatusRequestMsg", "Byte Array");

        }

        private void SendProtocolVersionRequestMsg()
        {
            string SendBlock = string.Empty;
            string BlockCode = BLOCK_CODE.PROTOCOL_VERSION_REQUEST;
            SendBlock = string.Empty;
            SendBlock += $"{(char)ASCII.SOH}{(char)ASCII.LF}";
            SendBlock += $"{m_InstrumentCode} {m_InstrumentIdentifier} {BlockCode}{(char)ASCII.LF}";
            SendBlock += $"{(char)ASCII.STX}{(char)ASCII.LF}";
            SendBlock += $"{(char)ASCII.ETX}{(char)ASCII.LF}";
            SendBlock += $"{m_SequenceCounter}{(char)ASCII.LF}";
            SendBlock += $"{BuildBlockCheckSum(SendBlock)}{(char)ASCII.LF}";
            SendBlock += $"{(char)ASCII.EOT}{(char)ASCII.LF}";

            state.workSocket.Send(Encoding.UTF8.GetBytes(SendBlock));
            fileLog.WriteData($"{SendBlock}", "SendProtocolVersionRequestMsg", "String");
            fileLog.WriteData($"{BitConverter.ToString(Encoding.UTF8.GetBytes(SendBlock), 0, SendBlock.Length)}", "SendProtocolVersionRequestMsg", "Byte Array");

        }

        private void SendDateAndTimeRequestMsg()
        {
            string SendBlock = string.Empty;
            string BlockCode = BLOCK_CODE.DATE_AND_TIME_REQUEST;
            SendBlock = string.Empty;
            SendBlock += $"{(char)ASCII.SOH}{(char)ASCII.LF}";
            SendBlock += $"{m_InstrumentCode} {m_InstrumentIdentifier} {BlockCode}{(char)ASCII.LF}";
            SendBlock += $"{(char)ASCII.STX}{(char)ASCII.LF}";
            SendBlock += $"{(char)ASCII.ETX}{(char)ASCII.LF}";
            SendBlock += $"{m_SequenceCounter}{(char)ASCII.LF}";
            SendBlock += $"{BuildBlockCheckSum(SendBlock)}{(char)ASCII.LF}";
            SendBlock += $"{(char)ASCII.EOT}{(char)ASCII.LF}";

            state.workSocket.Send(Encoding.UTF8.GetBytes(SendBlock));
            fileLog.WriteData($"{SendBlock}", "SendDateAndTimeRequestMsg", "String");
            fileLog.WriteData($"{BitConverter.ToString(Encoding.UTF8.GetBytes(SendBlock), 0, SendBlock.Length)}", "SendDateAndTimeRequestMsg", "Byte Array");

        }

        private void SendSerialNumberRequestMsg()
        {
            string SendBlock = string.Empty;
            string BlockCode = BLOCK_CODE.SERIAL_NUMBER_REQUEST;
            SendBlock = string.Empty;
            SendBlock += $"{(char)ASCII.SOH}{(char)ASCII.LF}";
            SendBlock += $"{m_InstrumentCode} {m_InstrumentIdentifier} {BlockCode}{(char)ASCII.LF}";
            SendBlock += $"{(char)ASCII.STX}{(char)ASCII.LF}";
            SendBlock += $"{(char)ASCII.ETX}{(char)ASCII.LF}";
            SendBlock += $"{m_SequenceCounter}{(char)ASCII.LF}";
            SendBlock += $"{BuildBlockCheckSum(SendBlock)}{(char)ASCII.LF}";
            SendBlock += $"{(char)ASCII.EOT}{(char)ASCII.LF}";

            state.workSocket.Send(Encoding.UTF8.GetBytes(SendBlock));
            fileLog.WriteData($"{SendBlock}", "SendSerialNumberRequestMsg", "String");
            fileLog.WriteData($"{BitConverter.ToString(Encoding.UTF8.GetBytes(SendBlock), 0, SendBlock.Length)}", "SendSerialNumberRequestMsg", "Byte Array");

        }

        private void SendSyncBlockMsg()
        {
            string SyncBlock = string.Empty;
            string BlockCode = BLOCK_CODE.IDLE_OR_SYNC_BLOCK;
            SyncBlock = string.Empty;
            SyncBlock += $"{(char)ASCII.SOH}{(char)ASCII.LF}";
            SyncBlock += $"{m_InstrumentCode} {m_InstrumentIdentifier} {BlockCode}{(char)ASCII.LF}";
            SyncBlock += $"{(char)ASCII.STX}{(char)ASCII.LF}";
            SyncBlock += $"{(char)ASCII.ETX}{(char)ASCII.LF}";
            SyncBlock += $"{(char)ASCII.EOT}{(char)ASCII.LF}";
            state.workSocket.Send(Encoding.UTF8.GetBytes(SyncBlock));
            fileLog.WriteData($"{SyncBlock}", "SendSyncBlockMsg", "String");
            fileLog.WriteData($"{BitConverter.ToString(Encoding.UTF8.GetBytes(SyncBlock), 0, SyncBlock.Length)}", "SendSyncBlockMsg", "Byte Array");
        }
        public string BuildBlockCheckSum(string msg)
        {
            int ibcs = 0;
            string bcs = string.Empty;
            foreach (byte b in msg)
            {
                ibcs += b;
            }
            bcs = string.Format("{0}", ibcs % 1000);
            return bcs;
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

            try
            {
                for (int nCh = 0; nCh < Msg.Length; nCh++)
                {
                    switch (CommStatus)
                    {
                        //초기에 체크정보를 보내고 idle 상태로 변경(공통 Thread에서)
                        //sc 수신후 정보가 맞으면 waiting으로 변경 틀리면 idle 유지
                        case A1_CommState.IDLE:
                            if (Msg[nCh] == ASCII.SOH)
                            {
                                msgString = string.Empty;
                                msgString += Msg[nCh];
                                CommStatus = A1_CommState.WAITING;
                            }
                            break;
                        case A1_CommState.WAITING:
                            msgString += Msg[nCh];
                            if (msgString.Contains($"{(char)ASCII.EOT}{(char)ASCII.LF}"))
                            {
                                if (BlockCheckSum(msgString))
                                {
                                   m_SequenceCounter = (GetSequenceCounter(msgString) + 1) % 2;

                                    StringBuilder ssb = new StringBuilder();
                                    string[] splitstring = msgString.Split((char)ASCII.LF);

                                    string instrumentCode = splitstring[1].Substring(0, 2);
                                    string instrumentIdentifier = splitstring[1].Substring(3, 16);
                                    string BlockCode = splitstring[1].Substring(20, 2);
                                    if (BlockCode == BLOCK_CODE.IDLE_OR_SYNC_BLOCK)
                                    {
                                        fileLog.WriteData("IDLE_OR_SYNC_BLOCK", "ParseMessage", $"IDLE_OR_SYNC_BLOCK Recved");
                                        // Do Nothing
                                    }
                                    else if (BlockCode == BLOCK_CODE.PROTOCOL_VERSION_DATA)
                                    {
                                        fileLog.WriteData("PROTOCOL_VERSION_DATA", "ParseMessage", $"PROTOCOL_VERSION_DATA Recved");
                                        m_ProtocolVersion = 1;
                                    }
                                    else if (BlockCode == BLOCK_CODE.SYSTEM_STATUS_RESPONSE)
                                    {
                                        fileLog.WriteData("SYSTEM_STATUS_RESPONSE", "ParseMessage", $"SYSTEM_STATUS_RESPONSE Recved");
                                        m_SystemStatus = 1;
                                    }
                                    else if (BlockCode == BLOCK_CODE.DATE_AND_TIME_RESPONSE)
                                    {
                                        fileLog.WriteData("DATE_AND_TIME_RESPONSE", "ParseMessage", $"DATE_AND_TIME_RESPONSE Recved");
                                        m_DateAndTime = 1;
                                    }
                                    else if (BlockCode == BLOCK_CODE.SERIAL_NUMBER_RESPONSE)
                                    {
                                        fileLog.WriteData("SERIAL_NUMBER_RESPONSE", "ParseMessage", $"SERIAL_NUMBER_RESPONSE Recved");
                                        m_SerialNumber = 1;
                                    }

                                    //                                  else if( BlockCode == BLOCK_CODE.SLOT_CONFIGURATION)
                                    else if ( BlockCode == BLOCK_CODE.SAMPLE_TUBE_INFORMATION)
                                    {
                                        fileLog.WriteData("SAMPLE_TUBE_INFORMATION", "ParseMessage", $"SAMPLE_TUBE_INFORMATION Recved");

                                    }
                                    else if( BlockCode == BLOCK_CODE.SERVICE_REQUEST_RESPONSE)
                                    {
                                        fileLog.WriteData("SERVICE_REQUEST_RESPONSE", "ParseMessage", $"SERVICE_REQUEST_RESPONSE Recved");
                                        // BLOCK 69, LINE 96, SERVICE 61 이면 30초 SLEEP 후 다음 
                                        Thread.Sleep(30 * 1000);
                                        SendMcsRequestMsg();
                                    }                                   
                                    else if (BlockCode == BLOCK_CODE.SAMPLE_RESULT_WITH_LOT_INFO)
                                    {
                                        typeName = string.Empty;
                                        for (int i = 3; i < splitstring.Length; i++)
                                        {
                                            if (splitstring[i].Length > 1)
                                            {
                                                switch (BlockCode)
                                                {
                                                    case LINE_CODE.ORDER_ID:
                                                        INF_OID_VAL = splitstring[i].Substring(3, 15);
                                                        //OrderDate = splitstring[i].Substring(19, 10);
                                                        //SampleType = splitstring[i].Substring(30, 3);
                                                        ssb.AppendLine($"INF_ORDER, {dtMsrDateTime:yyyy-MM-dd HH:mm:ss}, {INF_OID_VAL} ");
                                                        break;
                                                    case LINE_CODE.TEST_ID:
                                                        INF_TID_VAL = splitstring[i].Substring(3, 3);
                                                        typeName = GetTypeName(INF_TID_VAL);
                                                        break;
                                                    case LINE_CODE.ACCESS_DATE_TIME:
                                                        string TimeString = splitstring[i].Substring(3, 8);
                                                        string DateString = splitstring[i].Substring(12, 10);
                                                        PC00U01.TryParseExact($"{DateString} {TimeString}", out dtMsrDateTime);
                                                        ssb.AppendLine($"INF_RESULT_TIME, {dtMsrDateTime:yyyy-MM-dd HH:mm:ss}, {dtMsrDateTime} ");
                                                        break;
                                                    case LINE_CODE.RESULT_DATA:
                                                        ssb.AppendLine($"RESULTS_{typeName}_VAL, {dtMsrDateTime:yyyy-MM-dd HH:mm:ss}, {splitstring[i].Substring(3, 13)} ");
                                                        ssb.AppendLine($"RESULTS_{typeName}_UNIT, {dtMsrDateTime:yyyy-MM-dd HH:mm:ss}, {splitstring[i].Substring(17, 6)}");
                                                        ssb.AppendLine($"RESULTS_{typeName}_XFLAG, {dtMsrDateTime:yyyy-MM-dd HH:mm:ss}, {splitstring[i].Substring(24, 3)}");
                                                        //ssb.AppendLine($"RESULTS_{typeName}_SFLAG, {dtMsrDateTime:yyyy-MM-dd HH:mm:ss}, {splitstring[i].Substring(28, 3)}");
                                                        ssb.AppendLine($"RESULTS_{typeName}_CFLAG, {dtMsrDateTime:yyyy-MM-dd HH:mm:ss}, {splitstring[i].Substring(32, 3)}");
                                                        ssb.AppendLine($"RESULTS_{typeName}_QFLAG, {dtMsrDateTime:yyyy-MM-dd HH:mm:ss}, {splitstring[i].Substring(36, 3)}");
                                                        //RangeValueToFlag = splitstring[i].Substring(40, 13);
                                                        //RangeLimit = splitstring[i].Substring(54, 13);

                                                        break;
                                                    default:
                                                        break;
                                                }
                                            }
                                        }
                                        EnQueue(MSGTYPE.MEASURE, ssb.ToString());

                                        // SendResultRequestMsg();
                                    }
                                    else
                                    {
                                        fileLog.WriteData("ETC BLOCK", "ParseMessage", $"BLOCK_CODE : {BlockCode} Recved");
                                        SendSyncBlockMsg();
                                    }
                                    fileLog.WriteData("OK", "ParseMessage", $"Normal ");
                                    ninitErrCount = 0;
                                    state.sb = new StringBuilder();
                                    state.sb.Append(Msg.Substring(nCh + 1));
                                }
                                else
                                {
                                    fileLog.WriteData("CSE", "ParseMessage", $"BlockCheckSum Error");
                                    ninitErrCount++;
                                    if (ninitErrCount > 8)
                                    {
                                        m_SequenceCounter = -1;
                                        ninitErrCount = 0;
                                    }
                                }
                            }


                            //if (Msg[nCh] == ASCII.EOT)
                            //{
                            //    state.sb = new StringBuilder();
                            //    state.sb.Append(Msg.Substring(nCh + 1));
                            //    CommStatus = A1_CommState.IDLE;
                            //}
                            //else
                            //{
                            //    msgString += Msg[nCh];
                            //    if (Msg[nCh] == ASCII.STX || Msg[nCh] == ASCII.SOH)
                            //    {
                            //        msgString = string.Empty;
                            //        msgString += Msg[nCh];
                            //    }
                            //    else if (Msg[nCh] == ASCII.LF)
                            //    {
                            //        int iResult;                                                                
                            //        StringBuilder ssb = new StringBuilder();

                            //        sendMsg = new byte[1];

                            //        if (blockCheckSum(Msg))
                            //        {
                            //            // 데이터 해석 가능
                            //            string[] sp_cr = msgString.Split('_');

                            //            if (sp_cr[0].Equals("01"))  //Result Time
                            //            {
                            //                if (sp_cr[1].ToString().Trim() != "")
                            //                    PC00U01.TryParseExact(sp_cr[1], out dtMsrDateTime);

                            //                ssb.AppendLine($"INF_RESULT_TIME, {dtMsrDateTime:yyyy-MM-dd HH:mm:ss}, {dtMsrDateTime} ");                                            
                            //            }
                            //            if (sp_cr[0].Equals("53"))  //Order ID
                            //            {
                            //                INF_OID_VAL = sp_cr[1].ToString().Replace("\r\n", "");
                            //                ssb.AppendLine($"INF_ORDER, {dtMsrDateTime:yyyy-MM-dd HH:mm:ss}, {sp_cr[1]} ");
                            //            }
                            //            if (sp_cr[0].Equals("55"))  //Test ID
                            //            {
                            //                INF_TID_VAL = sp_cr[1].ToString().Replace("\r\n", "");
                            //                typeName = GetTypeName(INF_TID_VAL);                                    
                            //            }                                       
                            //            if (sp_cr[0].Equals("00"))  //Result Data
                            //            {   
                            //                ssb.AppendLine($"RESULTS_{typeName}_VAL, {dtMsrDateTime:yyyy-MM-dd HH:mm:ss}, {sp_cr[1]} ");
                            //                ssb.AppendLine($"RESULTS_{typeName}_UNIT, {dtMsrDateTime:yyyy-MM-dd HH:mm:ss}, {sp_cr[2]}");                                            
                            //                ssb.AppendLine($"RESULTS_{typeName}_XFLAG, {dtMsrDateTime:yyyy-MM-dd HH:mm:ss}, {sp_cr[3]}");
                            //                ssb.AppendLine($"RESULTS_{typeName}_CFLAG, {dtMsrDateTime:yyyy-MM-dd HH:mm:ss}, {sp_cr[5]}");
                            //                ssb.AppendLine($"RESULTS_{typeName}_QFLAG, {dtMsrDateTime:yyyy-MM-dd HH:mm:ss}, {sp_cr[6]}");
                            //            }

                            //            EnQueue(MSGTYPE.MEASURE, ssb.ToString());
                            //            //sendMsg[0] = ASCII.ACK;
                            //            fileLog.WriteData("OK", "ParseMessage", $"Normal ");
                            //        }
                            //        else // CheckSum Error
                            //        {
                            //            ninitErrCount++;
                            //            fileLog.WriteData("CSE", "ParseMessage", $"CheckSum Error");

                            //            if (ninitErrCount == 8)
                            //            {
                            //                SendSyncBlockMsg();

                            //                ninitErrCount = 0;
                            //            }
                            //        }

                            //        state.sb = new StringBuilder();
                            //        state.sb.Append(Msg.Substring(nCh + 1));

                            //        //LF 단위로 처리하기때문에 초기화
                            //        msgString = string.Empty;
                            //    }
                            //    else { }
                            //}
                            break;
                        default:
                            CommStatus = A1_CommState.IDLE;
                            break;
                    }
                }
            }
            catch(Exception ex)
            {
                Debug.WriteLine(ex.ToString());
                listViewMsg.UpdateMsg($"Exceptioin - ParseMessage ({ex})", false, true, true, PC00D01.MSGTERR);
            }
        }
        private int GetSequenceCounter(string msg)
        {
            int isc = 0;

            string[] splitstring = msg.Split((char)ASCII.LF);
            if (splitstring.Length > 3)
            {
                int.TryParse(splitstring[splitstring.Length - 4], out isc);
            }
            return isc;
        }

        private bool BlockCheckSum(string msg)
        {
            string bcs = string.Empty;
            int ibcs = 0, ibcsRecved = 0;

            for (int i = 0; i < msg.Length - 6; i++)
            {
                ibcs += msg[i];
            }
            ibcs = ibcs % 1000;
            string[] splitstring = msg.Split((char)ASCII.LF);
            if (!int.TryParse(splitstring[splitstring.Length - 3], out ibcsRecved)) return false;
            if (ibcs != ibcsRecved) return false;
            return true;
        }

        //public bool blockCheckSum(string Msg)
        //{
        //    //처음부터 SC의 LF까지의 합의 나눈값이 BCS 값임
        //    int summation = 0;
        //    int chkCnt = 0;
        //    //임시 설정 무조건 ok            
        //    bool ChkVal = true;

        //    string[] sp_sync = Msg.Split((char)ASCII.LF);

        //    foreach (char ch in Msg.ToCharArray())
        //    {
        //        if (ch != (char)ASCII.CR)
        //        {
        //            summation += (byte)ch;
        //        }

        //        if (ch == (char)ASCII.LF)
        //        {
        //            chkCnt++;
        //        }

        //        if (chkCnt == sp_sync.Length - 3)
        //            break;
        //    }

        //    int calc_val = summation % 1000;

        //    if (calc_val == int.Parse(sp_sync[sp_sync.Length - 2].ToString().Replace("\r\n","")))
        //        ChkVal = true;           

        //    return ChkVal;
        //}
        
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
