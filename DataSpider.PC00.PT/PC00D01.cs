using OSIsoft.AF.Asset;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Text.Json;

namespace DataSpider.PC00.PT
{
    #region DEFINE Program Value (Message, FlagType, TimerInterval, Etc...)

    public enum IF_STATUS
    {
        Stop = 0,
        Normal = 1,
        Disconnected = 2,
        NetworkError = 3,
        NoData = 4,
        InvalidData = 8,
        InternalError = 16,
        Unknown = 99
    }

    public abstract class PC00D01
    {
        public const string MoveComplete = "{0} File Copied";
        public const string FileCopyComplete = "{0} File Copy Complete";

        public const string SucceededDBStored = "Succeeded to store file info. [{0}]";
        public const string FailedDBStore = "Failed to store file info. [{0}]";
        public const string NoFiles = "There are no files to process [{0}]";

        public const string SucceededtoPI = "The data transfer to the PI server was successful.[{0}/{1}]";
        public const string FailedtoPI = "Data transfer to PI server failed.[{0}/{1}]";
        public const string SucceededtoEF = "EventFrame [{0}] transfer to the PI AF DB was successful.";
        public const string FailedtoEF = "EventFrame [{0}] transfer to the PI AF DB was failed. [{1}]";

        public const string MSGTINF = "INFO";
        public const string MSGTERR = "ERROR";
        public const string MSGTREV = "RECV";
        public const string MSGTSEN = "SEND";
        public const string MSGTDBG = "DEBUG";

        public const string ON = "ON";
        public const string OFF = "OFF";

        public const string MSGP0001 = "Exit program, Are you sure?";
        public const string MSGP0002 = "Alert";
        public const string MSGP0003 = "Connected";
        public const string MSGP0004 = "Disconnected";
        public const string MSGP0007 = "Received Order Data ListView Update";
        public const string MSGP0008 = "DB Connected";
        public const string MSGP0009 = "DB Disconnected";
        public const string MSGP0015 = "DB Ping Test Fail";
        public const string MSGP0031 = "DB Connection Fail, Please check network connection!!";
        public const string MSGP0033 = "Equipment Inteface Program (Equipment Type : {0}) is Already Running !!";
        public const string MSGP0036 = "Are you sure?";
        public const string MSGP0046 = "Failed to retrieve data";
        public const string MSGP0047 = "Socket Ping Test Success";
        public const string MSGP0048 = "Socket Ping Test failed";


        public const string DateTimeFormat = "yyyy-MM-dd HH':'mm':'ss";
        public const string NotifyTipText = "PLC QUALITY COLLECT";
        public const string NotifyText = "PLC QUALITY COLLECT";

        public const int LogShowLinesDefault = 300;     // Log ListView Default Show Lines

        public const int ItvCurrTmUpd = 1000;           // Current Time Display Label Update Millisecond
        public const int ItvSerInfoLvUpd = 1000;        // Information ListView Update Millisecond
        public const int ItvRevOrdLvUpd = 60000;        // Order Receive List ListView Update Millisecond
        public const int ItvDBChk = 50000;               // Oracle DB Check Update Millisecond
        public const int ItvLogLvUpd = 1100;            // Log ListView Update Millisecond

        public const int nBufferSize = 3000;            // Socket BufferSize 

    }
    #endregion

    #region Process Infomation
    public class DeviceInfo
    {
        public string strPLANT_CD { get; set; }       // PLANT CD
        public string strProc_ID { get; set; }       // Process ID
        public string strProc_NM { get; set; }       // Process Name
        public string strProc_Init_Run_Flag { get; set; }       // Run Flag
        public int nProc_HeartBit_Check_Sec { get; set; }       // Heart Bit Check Seconds
        public int nProc_Pooling_Sec { get; set; }       // Pooling Seconds
        public string strDevice_ID { get; set; }       // Device ID
        public string strDevice_Name { get; set; }       // Device Name       
        public string strDevice_IP { get; set; }       // SFCS IP
        public int nDevice_Port { get; set; }       // SFCS PORT
        public string strOpc_IP { get; set; }       // OPC IP
        public int nOpc_Port { get; set; }       // OPC PORT
        public int nDevice_SocketConnectionWaitingSec { get; set; }       // 소켓 연결을 대기하는 시간
        public int nDevice_SocketReconnectSec { get; set; }       // Socket Reconnect Waiting time
        public int nDevice_SocketReceivingWaitingSec { get; set; }       // 소켓이 수신받는 것을 대기하는 시간
        public int nProc_DB_Reconnect_Sec { get; set; }       // DB 재연결 시간

        public string strStation_Cd { get; set; }       // Station Code

        public string strGetPath { get; set; }  // Image Get Path
        public string strSetPath { get; set; }  // Image Set Path
        // 20201006, DQK, Image BackupPath
        public string strBackupPath { get; set; }
        // 20201027, DQK, Vision File Format 
        public string strVisionFileFormat { get; set; }

    }
    #endregion

    #region TighteningOrder Data (Receive, Send)
    public class OrderItem
    {
        public DateTime ExecTime { get; set; }

        public string strPJI_NO { get; set; }
        public string strTRIM_IN_NO { get; set; }
        public string strMODEL_PREF { get; set; }
        public string strMODEL_BASE { get; set; }
        public string strMODEL_SUF { get; set; }
        public string strENGINE_TYPE { get; set; }
        public string strDST { get; set; }
        public string strIN_COLOR { get; set; }
        public string strOUT_COLOR { get; set; }
        public string strPBS_OUT { get; set; }
        public string strCAR_SERIES { get; set; }
        public string strCHASSIS_NO { get; set; }
        public string strTRIM_WI { get; set; }
        public string strREG_ID { get; set; }

    }
    #endregion

    #region TigteningOrder Data Log
    public class OrderLogItem
    {
        public string strPlantCd { get; set; }
        public string strDeviceId { get; set; }
        public string strLogTime { get; set; }
        public string strMsgType { get; set; }
        //public string strFlag       { get; set; }
        public string strMessage { get; set; }
        public bool bIsRead { get; set; }

    }
    #endregion

    #region Database Infomation
    public class DbInfo
    {
        public string strConStr { get; set; }    // DB ConnectionString
        public string strDB_IP { get; set; }    // DB IP

    }
    #endregion

    #region Log Infomation
    public class LogInfo
    {
        public string mLog_Dir { get; set; }   // Log File Directory
        public int mLogSaveDays { get; set; }
        public string mCfg_Dir { get; set; }

    }
    #endregion

    #region Program Regist Info
    public class PgmRegInfo
    {

        public string strEQUIP_TYPE { get; set; }

    }
    #endregion

    public abstract class DEFINE
    {
        public const string OK = "OK";
        public const string NG = "NG";

        //데이터 타입
        public const string INFO = "[INFO]";
        public const string ERROR = "[ERROR]";


        public const string ON = "ON";
        public const string OFF = "OFF";


    }

    #region PI Infomation
    public class PIInfo
    {
        public string strPI_Server { get; set; }    // PI SERVER NAME
        public string strPI_DB { get; set; }    // PI DB NAME

        public string strPI_USER { get; set; }    // PI USER

        public string strPI_PWD { get; set; }    // PI PASSWORD

        public string AF_DB { get; set; }
        public string AF_USER {  get; set; }
        public string AF_PWD { get; set; }
        public string AF_DOMAIN { get; set; }


    }

    public class EventFrameAttributeData
    {
        public string Name { get; set; }
        public string Value { get; set; }
    }
    public class EventFrameData
    {
        public string Name { get; set; }
        public string StartTime { get; set; }
        public string EndTime { get; set; }

        public List<EventFrameAttributeData> Attributes { get; set; } = new List<EventFrameAttributeData>();

        //public string TemplateName { get; set; }
        public string IFTime { get; set; }
        /// <summary>
        /// PI EventFrame I/F Flag
        /// 초기 : N |
        /// EventFrame 저장 Disable : D
        /// EventFrame 저장 성공 : Y |
        /// EventFrame 저장 실패 : E |
        /// EventFrame 저장 실패 후 재시도 10회 실패 : F |
        /// EventFrame 저장 실패 내용 삭제 : Z |
        /// N, E 에 대해서만 PI 저장 시도
        /// </summary>
        public string IFFlag { get; set; }
        public string IFRemark { get; set; }

        public string EquipmentName { get; set; }
        public int MessageType { get; set; }

        public string ServerTime { get; set; }

        public string GetSerializedAttributes()
        {
            return JsonSerializer.Serialize(Attributes);
        }
    }


    #endregion

    public abstract class MSGTYPE
    {
        public const int UNKNOWN = 0;
        public const int MEASURE = 1;
        public const int CALIBRATION = 2;
        public const int EVENT = 3;
        public const int SMP_NEW_DATA = 4;
        public const int SMP_EDIT_DATA = 5;
        public const int QC_NEW_DATA = 6;
        public const int QC_DEL = 7;
        public const int CAL_NEW_DATA = 8;

        public const int MSR_PH = 10;
        public const int MSR_COND = 11;
        public const int COND_ADJ = 12;
        public const int ERR = 13;


    }

    public class ReplaceTagDef
    {
        public const string SERVER_TIME_TAG = "SERVER_TIME";
    }

    public interface IPC00F00
    {
        void listViewMsg(string pstrProcID, string pMsg, bool pbGridView, int pnCurNo, int pnSubItemNo, bool pbLogView, string pstrType);
    }

    public abstract class ASCII
    {
        public const int
        NUL = 0x00,
        SOH = 0X01,
        STX = 0X02,
        ETX = 0X03,
        EOT = 0x04,
        ENQ = 0x05,
        ACK = 0x06,
        BEL = 0x07,
        BS = 0x08,
        HT = 0x09,
        LF = 0x0a,
        VT = 0x0b,
        FF = 0x0c,
        CR = 0x0d,
        SO = 0x0e,
        SI = 0x0f,
        DLE = 0x10,
        DC1 = 0x11,
        DC2 = 0x12,
        DC3 = 0x13,
        DC4 = 0x14,
        NAK = 0x15,
        SYN = 0x16,     //SYNCHRONOUS IDLE
        ETB = 0x17,     //END OF TRANS. BLOCK
        CAN = 0x18,
        EM = 0x19,
        SUB = 0x1a,
        ESC = 0x1b,
        FS = 0x1c,
        GS = 0x1d,
        RS = 0x1e,
        US = 0x1f;
    }
    public enum UserLevel
    {
        UnAuthorized = 0,
        Manager = 1,   // 장비, 태그 추가/수정/삭제 가능
        Admin = 2     // Operator + 사용자 관리
    }


    public abstract class FAILOVER_MODE
    {
        public const int AUTO = 1;
        public const int MANL = 0;
    }
}