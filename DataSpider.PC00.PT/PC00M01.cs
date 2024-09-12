using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlTypes;
using System.Diagnostics;
using System.Diagnostics.Eventing.Reader;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using OSIsoft.AF;
using OSIsoft.AF.Asset;
using OSIsoft.AF.EventFrame;
using OSIsoft.AF.PI;
using OSIsoft.AF.Search;
using OSIsoft.AF.Time;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

/// <summary>
/// 큐 처리 구현
/// 데이터 파일 저장 : 1개 처리 기준 데이터(string 배열)를 설정대로 파싱하여 태그명, 시간, 값 형태 파일(yyyyMMddHHmmssfff.ttv)로 저장
/// </summary>
namespace DataSpider.PC00.PT
{
    public class PC00M01 : PC00B01
    {
        private EquipmentDataProcess dataProcess = null;
        private DataTable dtTagInfo = null;
        private DateTime dtLastTagUpdated = DateTime.MinValue;
        private DateTime dtCheckTagUpdate = DateTime.MinValue;
        private bool isTagUpdated = false;
        private int intervalTagUpdate = 60;

        // 20220908, SHS, SERVERTIME 중복체크 (시간만 비교할지 값도 비교할지) 옵션 기능 추가
        private bool checkServerTimeDup = false;
        //

        // 20220908, SHS, SERVERTIME 중복체크 (시간만 비교할지 값도 비교할지) 옵션 기능 추가
        //public PC00M01(IPC00F00 owner, string equipType, string equipName, string connectionInfo, string extraInfo, int nCurNo, bool bAutoRun = false) : base(owner, equipType, equipName, connectionInfo, extraInfo, nCurNo, bAutoRun)
        public PC00M01(IPC00F00 owner, DataRow dr, string equipType, string equipName, string connectionInfo, string extraInfo, int nCurNo, bool bAutoRun = false, bool bCheckServerTimeDup = false) : base(owner, dr, equipType, equipName, connectionInfo, extraInfo, nCurNo, bAutoRun)
        {
            //if (GetTagInfo())
            //{
            //    if (m_AutoRun == true)
            //    {
            //        m_Thd = new Thread(ThreadJob);
            //        m_Thd.Start();
            //    }
            //}
            //else
            //{
            //    bTerminal = false;
            //}

            //while (!GetTagInfo() && !bTerminal)
            //{
            //    Thread.Sleep(intervalTagUpdate * 1000);
            //}

            // 20220908, SHS, SERVERTIME 중복체크 (시간만 비교할지 값도 비교할지) 옵션 기능 추가
            checkServerTimeDup = bCheckServerTimeDup;
            //

            GetTagInfo();
            if (m_AutoRun && !bTerminal)
            {
                m_Thd = new Thread(ThreadJob);
                m_Thd.Start();
            }
        }
        public bool GetTagInfo()
        {
            string errCode = string.Empty;
            string errText = string.Empty;
            string machineName = Environment.MachineName;
            string[] args = Environment.GetCommandLineArgs();
            if (args.Length > 2 && args[2].Trim().ToUpper().Equals("TEST"))
            {
                machineName = string.Empty;
            }
            isTagUpdated = false;
            //DataTable dtCurrTagInfo = m_sqlBiz.GetTagInfo(m_Type, machineName, string.Empty, true, ref errCode, ref errText) ;
            DataTable dtCurrTagInfo = m_sqlBiz.GetTagInfoForDSC(m_Name, string.Empty, true, ref errCode, ref errText);
            if (string.IsNullOrWhiteSpace(errText))
            {
                if (dtCurrTagInfo.Rows.Count < 1)
                {
                    listViewMsg.UpdateMsg($"There are no TAGs for Equipment Type : {m_Name}, Server Name : {Environment.MachineName}");
                    return false;
                }
                //if (PC00U01.TryParseExact(dtCurrTagInfo.Select("", "UPDATE_REG_DATE DESC")[0]["UPDATE_REG_DATE"]?.ToString(), out DateTime dtTemp))
                if (DateTime.TryParse(dtCurrTagInfo.Select("", "UPDATE_REG_DATE DESC")[0]["UPDATE_REG_DATE"]?.ToString(), out DateTime dtTemp))
                {
                    isTagUpdated = dtLastTagUpdated.CompareTo(dtTemp) < 0;
                    dtLastTagUpdated = dtTemp;
                }
                if (isTagUpdated || dtTagInfo == null)
                {
                    dtTagInfo = dtCurrTagInfo;
                }
                return true;
            }
            else
            {
                listViewMsg.UpdateMsg($"Error to get TAGs info. Equipment Type : {m_Type}, Server Name : {Environment.MachineName} - {errText}");
                return false;
            }
        }
        private void CheckTagUpdated()
        {
            DateTime dtNow = DateTime.Now;
            if (dtCheckTagUpdate.AddSeconds(intervalTagUpdate).CompareTo(dtNow) < 0)
            {
                listViewMsg.UpdateMsg("Check Tag Updated.", false, true);
                dtCheckTagUpdate = dtNow;
                if (GetTagInfo() && isTagUpdated)
                {
                    dataProcess.UpdateTag(dtTagInfo);
                    listViewMsg.UpdateMsg("Tag Updated.", false, true);
                }
            }
        }
        private void ThreadJob()
        {
            listViewMsg.UpdateStatus(true);
            listViewMsg.UpdateMsg("Thread started");

            //dataProcess = new EquipmentDataProcess(m_ConnectionInfo, dtTagInfo, listViewMsg, dataEncoding);
            dataProcess = new EquipmentDataProcess(m_ConnectionInfo, drEquipment, dtTagInfo, listViewMsg, dataEncoding, checkServerTimeDup);
            while (!bTerminal || PC00U01.QueueCount > 0)
            {
                try
                {
                    if (dtTagInfo == null)
                    {
                        if (!GetTagInfo())
                        {
                            //listViewMsg.UpdateMsg("Loading Tag Info Error or There is no Tags", true, true, true, PC00D01.MSGTERR);
                            Thread.Sleep(intervalTagUpdate * 1000);
                            continue;
                        }
                    }

                    CheckTagUpdated();
                    
                    QueueMsg msg = PC00U01.ReadQueue();
                    if (msg == null)
                    {
                        listViewMsg.UpdateMsg("Queue is empty", true, false);
                        Thread.Sleep(1000);
                        continue;
                    }
                    if (dataProcess.DataProcess(msg))
                    {
                        listViewMsg.UpdateMsg($"{msg.m_EqName}({msg.m_MsgType}) Data has been processed", true, true);
                        fileLog.WriteData(msg.m_Data, "OK", $"{msg.m_EqName}({msg.m_MsgType})");
                    }
                    else
                    {
                        listViewMsg.UpdateMsg($"{msg.m_EqName}({msg.m_MsgType}) Data processing failed", true, true, false, PC00D01.MSGTERR);
                        fileLog.WriteData(msg.m_Data, "NG", $"{msg.m_EqName}({msg.m_MsgType})");
                    }
                }
                catch (Exception ex)
                {
                    listViewMsg.UpdateMsg(ex.ToString(), false, true, true, PC00D01.MSGTERR);
                }
                finally
                {
                }
                Thread.Sleep(10);
            }
            listViewMsg.UpdateStatus(false);
            listViewMsg.UpdateMsg("Thread finished");
        }
    }

    public class Data
    { 
        public int Line { get; private set; } = -1;
        public int Offset { get; private set; } = -1;
        public int Size { get; private set; } = -1;
        public string Value { get; set; } = string.Empty;
        public string ReplaceTag { get; private set; } = string.Empty;
        public string Delimeter { get; private set; } = string.Empty;
        public int ItemIndex { get; private set; } = -1;

        // 20230111 kwc -5 추가
        public bool Available
        {
            get { return !string.IsNullOrWhiteSpace(ReplaceTag) || Line > 0 && Offset > 0 && (Size > 0 || Size == -1 || Size == -2 || Size == -3 || Size == -4 || Size == -5); }
        }
        public DateTime ServerTime = DateTime.MinValue;

        public string DataPosition
        {
            get 
            { 
                if (!string.IsNullOrWhiteSpace(ReplaceTag))
                {
                    return ReplaceTag;
                }
                //if (!string.IsNullOrWhiteSpace(Delimeter))
                if (ItemIndex > 0)
                {
                    return $"{Line},'{Delimeter}',{ItemIndex},{Offset},{Size}";
                }
                return $"{Line},{Offset},{Size}"; 
            }
        }
        public bool UpdateDataPosition(string posInfo, out string errMessage)
        {
            errMessage = string.Empty;

            if (string.IsNullOrWhiteSpace(posInfo))
            {
                errMessage = "Format information is empty";
                return false;
            }

            if (!PC00U01.CheckPosInfo(posInfo, out errMessage))
                return false;

            if (posInfo.Trim().StartsWith("#"))
            {
                // 20240912, SHS, ReplaceTag 는 #으로 시작. # 을 제거하지 않고 처리. # 만 설정 시 공백으로 VALUE 처리 하게 됨
                ReplaceTag = posInfo.Trim();//.Substring(1);
                return true;
            }

            string[] info = posInfo.Split(',');
            // line, offset, size
            // line, delimeter, itemIndex, offset, size
            if (info.Length == 3)
            {
                Line = int.Parse(info[0]);
                Offset = int.Parse(info[1]);
                Size = int.Parse(info[2]);
            }
            else
            {
                Line = int.Parse(info[0]);
                Delimeter = info[1];
                ItemIndex = int.Parse(info[2]);
                Offset = int.Parse(info[3]);
                Size = int.Parse(info[4]);
            }

            return true;
        }

        public bool DataProcess(string[] data, out string errMessage)
        {
            errMessage = string.Empty;
            Value = string.Empty;

            if (!Available)
            {
                errMessage = $"TAG's Value format is not valid (Line : {Line}, Offset : {Offset}, Size : {Size}, Replace TAG : {ReplaceTag})";
                return false;
            }

            if (!string.IsNullOrWhiteSpace(ReplaceTag))
            {
                switch(ReplaceTag)
                {
                    case ReplaceTagDef.SERVER_TIME_TAG:
                        Value = ServerTime.ToString("yyyy-MM-dd HH:mm:ss");// DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                        break;
                    default:
                        Value = ReplaceTag.Length > 1 ? ReplaceTag.Substring(1) : string.Empty;
                        break;
                }
                return true;
            }

            if (data.Length < Line)
            {
                errMessage = $"Data Line count less than Line (Line : {Line}, Offset : {Offset}, Size : {Size}, Replace TAG : {ReplaceTag})";
                return false;
            }

            string line = data[Line - 1];
            if (string.IsNullOrWhiteSpace(line))
            {
                errMessage = $"Line string is empty (Line : {Line}, Offset : {Offset}, Size : {Size}, Replace TAG : {ReplaceTag})";
                return false;
            }

            // Delimiter 설정되어 있으면 Delimiter 로 split, 설정된 item index 항목을 line 으로 치환
            //if (!string.IsNullOrWhiteSpace(Delimeter))
            if (ItemIndex > 0)
            {
                string[] items = line.Split(Delimeter.ToCharArray()[0]);
                if (items.Length >= ItemIndex - 1)
                {
                    line = items[ItemIndex - 1];
                }
                else
                {
                    errMessage = $"Split data count less than ItemIndex (Line Data : {line}, Delimeter : '{Delimeter}', ItemIndex : {ItemIndex}, Offset : {Offset}, Size : {Size})";
                    return false;
                }
            }

            if (line.Length < Offset)
            {
                //errMessage = $"Line length is less than Offset (Line Data : {line}, Line : {Line}, Offset : {Offset}, Size : {Size})";
                Value = string.Empty;
                return true;
            }

            //Value = Size == -1 ? line.Substring(Offset - 1).Trim() : line.Substring(Offset - 1, line.Length < Size ? line.Length : Size).Trim();
            Value = PC00U01.ExtractString(line, Offset, Size);
            return true;
        }
    }

    public class TTVTAG : TAG
    {
        public string TTVTagName { get; set; } = string.Empty;
        //public TTVTAG(string tagName, string equipName, int msgType, string ttvTagName) : base(tagName, equipName, msgType)
        public TTVTAG(string tagName, string equipName, int msgType, string piTagName, string ttvTagName) : base(tagName, equipName, msgType, piTagName)
        {
            TTVTagName = ttvTagName;
        }
        public TTVTAG(string tagName, string equipName, int msgType, string piTagName, string ttvTagName, string efAttributeName, string lastMeasuredValue, string lastMeasuredDateTime) : base(tagName, equipName, msgType, piTagName, efAttributeName,lastMeasuredValue, lastMeasuredDateTime)
        {
            TTVTagName = ttvTagName;
        }
        public override bool DataProcess(string[] data, out string errMessage)
        {
            IsDuplicated = IsValueUpdated = false;
            errMessage = string.Empty;
            //data.ToList().Find(x => x.Split(',')[0].Equals(TTVTagName))
            foreach (string line in data)
            {
                string[] items = line.Split(',');
                if (items != null && items.Length > 2)
                {
                    if (items[0].Trim().Equals(TTVTagName))
                    {
                        // 20240702, SHS, P5, TTVTagName(OPCTIME_NM) 이 SVR_TIME 이면 장비인터페이스에서 부여한 Value (SVRTIME) 를 공통에서 부여한 현재시간으로 대체 (PI Tag, EventFrame 모두 동일한 SVRTIME 을 사용하기 위해)
                        //tagValue.Value = string.Join(",", items, 2, items.Length - 2).Replace("'", "''").Trim();
                        if (TTVTagName.EndsWith("SVRTIME"))
                        {
                            tagValue.Value = tagValue.ServerTime.ToString("yyyy-MM-dd HH:mm:ss");
                        }
                        else
                        {
                            tagValue.Value = string.Join(",", items, 2, items.Length - 2).Replace("'", "''").Trim();
                        }

                        if (!PC00U01.TryParseExact($"{items[1].Trim()}", out dtTimeStamp))
                        {
                            errMessage = $"Parsing TimeStamp failed ({items[1]})";
                            return false;
                        }
                        Debug.WriteLine(TTV);
                        
                        // 20230106, SHS, TimeStamp 값이 MSSQL datetime 범위에 벗어나면 (DB 저장시 Error 발생) PI/DataSpider 저장 처리 안하도록 수정
                        if (dtTimeStamp.CompareTo(DateTime.Parse("1753-01-01 00:00:00.000")) < 0)
                        {
                            errMessage = $"Invalid TimeStamp. TimeStamp is before 1753-01-01 00:00:00.000 ({TimeStamp})";
                            return false;
                        }

                        //IsValueUpdated = true;
                        // 20220908, SHS, TTV 형태 데이터 처리시에도 중복데이터 제외 기능 누락되었던것 추가
                        // 별도의 ServerTime TAG 설정이 없이 데이터에 있는 태그명 값 시간으로 처리되므로 ServerTime 태그 상관없음
                        //
                        if (!(TimeStamp.Equals(LastMeasureDateTime) && Value.Equals(LastMeasureValue)))
                        {
                            LastMeasureDateTime = TimeStamp;
                            LastMeasureValue = Value;
                            IsDuplicated = true;
                        }
                        //
                        IsValueUpdated = true;
                        break;
                    }
                }
            }
            return true;
        }
    }
    public class TAG
    {
        //
        public bool checkServerTimeDup = false;
        //

        protected Data tagValue = new Data();
        private Data dateValue = new Data();
        private Data timeValue = new Data();
        public DateTime dtTimeStamp = DateTime.MinValue;
        private bool useTimeValue = true;
        public DateTime ServerTime 
        {
            set
            {
                tagValue.ServerTime = value;
                dateValue.ServerTime = value;
                timeValue.ServerTime = value;
            }
        }

        public string TimeStamp { get { return dtTimeStamp.ToString("yyyy-MM-dd HH:mm:ss.fff"); } }

        public string Value
        {
            get { return tagValue.Value; }
            // 20240703, SHS, P5, EVENT FRAME 저장 정보 TAG 를 EQUIPMENTDATAPROCESS 에서 처리하기 위해 SET 가능하도록 수정
            set { tagValue.Value = value; }
        }
        public string TTV
        {
            get { return $"{TagName},{TimeStamp},{Value}"; }
        }

        // 20220331, SHS, 통합처리구조 변경
        // InsertResult 프로시져에서 I/F Flag 가 D 인 경우에도 hi_measure_result_BK 테이블 저장하도록 수정 필요
        /// <summary>
        /// PI Point I/F Flag
        ///  초기 : N |
        ///  PI 저장 Disable : D
        ///  PI 저장 성공 : Y |
        ///  PI 저장 실패 : E |
        ///  PI 저장 실패 후 재시도 10회 실패 : F |
        ///  PI 저장 실패 내용 삭제 : Z |
        ///  N, E 에 대해서만 PI 저장 시도
        /// </summary>
        public string PIIFFlag { get; set; } = "N";
        public DateTime PIIFDateTime { get; set; } = DateTime.MinValue;
        public string PIIFTimeStamp { get { return PIIFDateTime.ToString("yyyy-MM-dd HH:mm:ss.fff"); } }
        public string TTFTV
        {
            get { return $"{TagName},{TimeStamp},{PIIFFlag},{PIIFTimeStamp},{Value}"; }
        }
        public bool IsDBInserted { get; set; } = false;
        public string PITagName { get; set; }
        // PI I/F Error Text
        public string Remark { get; set; } = string.Empty;
        public string LastMeasureDateTime { get; set; } = string.Empty;
        public string LastMeasureValue { get; set; } = string.Empty;
        // ---

        /// <summary>
        /// 저장할 EventFrame Attribute Name 
        /// </summary>
        public string EFAttributeName { get; set; } = string.Empty;
        // InsertResult 프로시져에서 I/F Flag 가 D 인 경우에도 hi_measure_result_BK 테이블 저장하도록 수정 필요

        /// <summary>
        /// 태그 값이 정상적으로 파싱 처리된 경우
        /// </summary>
        public bool IsValueUpdated { get; set; }

        /// <summary>
        /// 최근 처리 데이터와 시간 & 값이 동일한 경우
        /// </summary>
        public bool IsDuplicated { get; set; }

        public string TagName { get; set; }
        public string EquipName { get; set; }
        public int MsgType { get; set; } = 1;
        public string DataPosition
        {
            get { return tagValue.DataPosition; }
        }
        private bool UpdateDataPosition(string posInfo, out string errMessage)
        {
            return tagValue.UpdateDataPosition(posInfo, out errMessage);
        }
        public string DatePosition
        {
            get { return dateValue.DataPosition; }
        }
        private bool UpdateDatePosition(string posInfo, out string errMessage)
        {
            return dateValue.UpdateDataPosition(posInfo, out errMessage);
        }
        public string TimePosition
        {
            get { return timeValue.DataPosition; }
        }
        private bool UpdateTimePosition(string posInfo, out string errMessage)
        {
            //if (string.IsNullOrWhiteSpace(posInfo))
            //{
            //    errMessage = string.Empty;
            //    return true;
            //}
            return timeValue.UpdateDataPosition(posInfo, out errMessage);
        }

        //public TAG(string tagName, string equipName, int msgType)
        public TAG(string tagName, string equipName, int msgType, string piTagName)
        {
            TagName = tagName;
            MsgType = msgType;
            EquipName = equipName;
            PITagName = piTagName;
        }
        public TAG(string tagName, string equipName, int msgType, string piTagName, string efAttributeName, string lastMeasuredValue, string lastMeasuredDateTime)
        {
            TagName = tagName;
            MsgType = msgType;
            EquipName = equipName;
            PITagName = piTagName;
            LastMeasureValue = lastMeasuredValue;
            LastMeasureDateTime = lastMeasuredDateTime;

            EFAttributeName = efAttributeName;
        }
        public bool UpdateFormat(string dataPosition, string datePosition, string timePosition, out string errMessage)
        {
            //errMessage = string.Empty;
            if (!UpdateDataPosition(dataPosition, out errMessage))
                return false;
            if (!UpdateDatePosition(datePosition, out errMessage))
                return false;
            // datePosition 에 시간까지 설정한 경우 시간설정은 하지 않음
            if (!string.IsNullOrWhiteSpace(timePosition))
            {
                if (!UpdateTimePosition(timePosition, out errMessage))
                    return false;
            }
            else
            { 
                useTimeValue = false;
            }
            return true;
        }
        public virtual bool DataProcess(string[] data, out string errMessage)
        {
            IsDuplicated = IsValueUpdated = false;

            if (!tagValue.DataProcess(data, out errMessage))
                return false;

            if (dateValue.Available && (timeValue.Available || !useTimeValue))
            {
                if (!dateValue.DataProcess(data, out errMessage))
                    return false;
                if (useTimeValue)
                {
                    if (!timeValue.DataProcess(data, out errMessage))
                        return false;
                }
                if (!PC00U01.TryParseExact($"{dateValue.Value} {timeValue.Value}", out dtTimeStamp))
                {
                    errMessage = $"Parsing TimeStamp failed ({dateValue.Value} {timeValue.Value})";
                    return false;
                }
            }
            else
            {
                dtTimeStamp = DateTime.Now;
            }

            Debug.WriteLine(TTV);

            // 20230106, SHS, TimeStamp 값이 MSSQL datetime 범위에 벗어나면 (DB 저장시 Error 발생) PI/DataSpider 저장 처리 안하도록 수정
            if (dtTimeStamp.CompareTo(DateTime.Parse("1753-01-01 00:00:00.000")) < 0)
            {
                errMessage = $"Invalid TimeStamp. TimeStamp is before 1753-01-01 00:00:00.000 ({TimeStamp})";
                return false;
            }

            //IsValueUpdated = true;
            // 20220404, SHS, 이전 데이터와 값과 시간이 모두 같으면 업데이트로 판단하지 않음 (중복데이터 제거)
            if (!(TimeStamp.Equals(LastMeasureDateTime) && Value.Equals(LastMeasureValue)))
            {
                // server time replace tag 인데 측정시간이 같으면 제외
                // 20220907, SHS, ServerTime 의 경우 측정시간이 같으면 중복판단하여 제외하던 기능 삭제, SLB 신성종 프로, 김현지 프로 확인, 20220913 적용
                // P3 와 통일, RePrinted DateTime 의 경우 중복데이터지만 시간이 현재 시간이라 처리되고 있어서 ServerTime 도 동일하게 처리되어야 하는것으로 판단
                //if (!(tagValue.ReplaceTag.Equals(ReplaceTagDef.SERVER_TIME_TAG) && TimeStamp.Equals(LastMeasureDateTime)))
                //{
                // 20220908, SHS, SERVERTIME 중복체크 (시간만 비교할지 값도 비교할지) 옵션 기능 추가
                if (checkServerTimeDup)
                {
                    // 타임스탬프가 같고 이번 태그가 서버타임 태그이면 업데이트 하지 않음
                    if (!(tagValue.ReplaceTag.Equals(ReplaceTagDef.SERVER_TIME_TAG) && TimeStamp.Equals(LastMeasureDateTime)))
                    {
                        LastMeasureDateTime = TimeStamp;
                        LastMeasureValue = Value;
                        IsDuplicated = true;
                    }
                }
                else
                {
                    LastMeasureDateTime = TimeStamp;
                    LastMeasureValue = Value;
                    IsDuplicated = true;
                }
                //}
            }
            IsValueUpdated = true;
            //
            return true;
        }
    }
    public class EquipmentDataProcess
    {
        public string FilePath { get; set; } = string.Empty;
        public Dictionary<string, List<TAG>> DicTAGList { get; private set; } = new Dictionary<string, List<TAG>>();
        private FormListViewMsg listViewMsg = null;
        private DataTable dtTAG = null;
        private DateTime dtServerTime = DateTime.MinValue;
        private Encoding dataEncoding = Encoding.UTF8;

        protected PC00Z01 m_sqlBiz = new PC00Z01();
        static PISystem _PISystem = null;
        static PIServer _PIServer = null;
        private PIInfo m_clsPIInfo;

        private DataRow drEquipment = null;
        private bool updateEventFrame = true;
        private bool updatePIPoint = true;
        private Dictionary<int, string> dicEFNamePrefix = new Dictionary<int, string>();
        private Dictionary<int, string> dicEFTemplateName = new Dictionary<int, string>();
        private static AFDatabase _AFDatabase = null;
        private static object objLock = new object();
        private string equipTypeName = string.Empty;

        //
        // 20220908, SHS, SERVERTIME 중복체크 (시간만 비교할지 값도 비교할지) 옵션 기능 추가
        public bool CheckServerTimeDup { get; set; } = false;
        //
        public EquipmentDataProcess(string filePath, FormListViewMsg _listViewMsg, Encoding _dataEncoding = null)
        {
            FilePath = filePath;
            listViewMsg = _listViewMsg;
            dataEncoding = _dataEncoding == null ? Encoding.UTF8 : _dataEncoding;

            try
            {
                m_clsPIInfo = ConfigHelper.GetPIInfo();
                //_PIServer = PIServer.FindPIServer(_PISystem, m_clsPIInfo.strPI_Server);
                //_PIServer?.Connect();
                if (!CheckPIConnection(out string errText))
                {
                    listViewMsg.UpdateMsg($"PI Server Connection Error (PI Server : {m_clsPIInfo.strPI_Server}, Error : {errText})", false, true, true, PC00D01.MSGTERR);
                }
                if (!CheckAFDatabase(out string errString))
                {
                    listViewMsg.UpdateMsg($"GetAFDatabase ({errString})", false, true, true, PC00D01.MSGTERR);
                }
            }
            catch (Exception ex)
            {
                listViewMsg.UpdateMsg($"PI Server Connection ({m_clsPIInfo.strPI_Server})- {ex}", false, true, true, PC00D01.MSGTERR);
            }
        }
        // 20220908, SHS, SERVERTIME 중복체크 (시간만 비교할지 값도 비교할지) 옵션 기능 추가
        //public EquipmentDataProcess(string filePath, DataTable dtTag, FormListViewMsg listViewMsg, Encoding dataEncoding = null) : this(filePath, listViewMsg, dataEncoding)
        public EquipmentDataProcess(string filePath, DataRow dr, DataTable dtTag, FormListViewMsg listViewMsg, Encoding dataEncoding = null, bool checkServerTimeDup = false) : this(filePath, listViewMsg, dataEncoding)
        {
            // 20220908, SHS, SERVERTIME 중복체크 (시간만 비교할지 값도 비교할지) 옵션 기능 추가
            CheckServerTimeDup = checkServerTimeDup;
            UpdateTag(dtTag);

            drEquipment = dr;
            updateEventFrame = drEquipment["UPDATE_EVENTFRAME_FLAG"].ToString().ToUpper().Equals("Y");
            updatePIPoint = drEquipment["UPDATE_PIPOINT_FLAG"].ToString().ToUpper().Equals("Y");
            equipTypeName = drEquipment["EQUIP_TYPE_NM"].ToString();
            //LoadEventFrameInfo(drEquipment["EXTRA_INFO"]?.ToString());
        }

        private bool LoadEventFrameInfo(string extraInfo)
        {
            dicEFNamePrefix.Clear();
            dicEFTemplateName.Clear();

            if (string.IsNullOrWhiteSpace(extraInfo)) return false;

            try
            {
                JsonDocument jDoc = JsonDocument.Parse(extraInfo);
                if (!jDoc.RootElement.TryGetProperty("EventFrame", out JsonElement jEle))
                {
                    return true;
                }
                foreach (JsonElement item in jEle.EnumerateArray())
                {
                    // Type 이 숫자로 파싱이 안되면 오류이므로 프로그램 종료 시킴
                    if (!int.TryParse(item.GetProperty("Type").GetString(), out int msgType))
                    {
                        continue;
                    }
                    if (!dicEFNamePrefix.ContainsKey(msgType))
                    {
                        dicEFNamePrefix.Add(msgType, item.GetProperty("EventFrameNamePrefix").GetString().Trim());
                        dicEFTemplateName.Add(msgType, item.GetProperty("EventFrameTemplateName").GetString().Trim());
                    }
                }
            }
            catch (Exception ex)
            {
                listViewMsg.UpdateMsg($"Exception in LoadEventFrameInfo (EXTRA_INFO) - {ex.Message}", true, true, true, PC00D01.MSGTERR);
                return false;
            }
            return true;
        }

        public bool UpdateTag(DataTable dtTag)
        {
            if (dtTag != null && dtTag.Rows.Count > 0)
            {
                DicTAGList.Clear();
                dtTAG = dtTag;
                return Add(dtTag);
            }
            return false;
        }
        //public bool Add(string tagName, string equipName, string msgType, string dataPosition, string datePosition, string timePosition, string opcItemName)
        public bool Add(string tagName, string equipName, string msgType, string dataPosition, string datePosition, string timePosition, string piTagName, string opcItemName)
        {
            if (!int.TryParse(msgType, out int msgTypeNo))
            {
                msgTypeNo = 1;
            }
            string tagKey = $"{equipName}_{msgTypeNo}";

            if (!DicTAGList.ContainsKey(tagKey))
            {
                DicTAGList.Add(tagKey, new List<TAG>());
            }
            if (string.IsNullOrWhiteSpace(opcItemName))
            {
                //TAG tag = new TAG(tagName, equipName, msgTypeNo);
                TAG tag = new TAG(tagName, equipName, msgTypeNo, piTagName);
                if (!tag.UpdateFormat(dataPosition, datePosition, timePosition, out string errMessage))
                {
                    listViewMsg.UpdateMsg($"An error occurred while update TAG format ({tag.TagName}) - {errMessage}", false, true, true, PC00D01.MSGTERR);
                    return false;
                }
                DicTAGList[tagKey].Add(tag);
            }
            else
            {
                //TTVTAG tag = new TTVTAG(tagName, equipName, msgTypeNo, opcItemName);
                TTVTAG tag = new TTVTAG(tagName, equipName, msgTypeNo, piTagName, opcItemName);
                DicTAGList[tagKey].Add(tag);
            }

            return true;
        }
        public bool Add(string tagName, string equipName, string msgType, string dataPosition, string datePosition, string timePosition, string piTagName, string opcItemName, string efAttributeName, string lastMeasuredValue, string lastMeasuredDateTime)
        {
            if (!int.TryParse(msgType, out int msgTypeNo))
            {
                msgTypeNo = 1;
            }
            string tagKey = $"{equipName}_{msgTypeNo}";

            if (!DicTAGList.ContainsKey(tagKey))
            {
                DicTAGList.Add(tagKey, new List<TAG>());
            }
            if (string.IsNullOrWhiteSpace(opcItemName))
            {
                TAG tag = new TAG(tagName, equipName, msgTypeNo, piTagName, efAttributeName, lastMeasuredValue, lastMeasuredDateTime);
                // 20220908, SHS, SERVERTIME 중복체크 (시간만 비교할지 값도 비교할지) 옵션 기능 추가
                tag.checkServerTimeDup = CheckServerTimeDup;
                //
                if (!tag.UpdateFormat(dataPosition, datePosition, timePosition, out string errMessage))
                {
                    listViewMsg.UpdateMsg($"An error occurred while update TAG format ({tag.TagName}) - {errMessage}", false, true, true, PC00D01.MSGTERR);
                    return false;
                }
                DicTAGList[tagKey].Add(tag);
            }
            else
            {
                TTVTAG tag = new TTVTAG(tagName, equipName, msgTypeNo, piTagName, opcItemName, efAttributeName, lastMeasuredValue, lastMeasuredDateTime);
                DicTAGList[tagKey].Add(tag);
            }

            return true;
        }
        public bool Add(DataTable dtTag)
        {
            foreach (DataRow dr in dtTag.Rows)
            {
                //Add(dr["TAG_NM"].ToString().Trim(), dr["EQUIP_NM"].ToString().Trim(), dr["MSG_TYPE"].ToString().Trim(), dr["DATA_POSITION"].ToString().Trim(), dr["DATE_POSITION"].ToString().Trim(), dr["TIME_POSITION"].ToString().Trim(), dr["OPCITEM_NM"].ToString().Trim());
                // 20220908, SHS, TAG 설정 정보 UPDATE 시 DB 에 저장된 최근값의 측정시간을 yyyy-MM-dd HH:mm:ss 형식 문자열로 변환 처리 (중복데이터 체크 측정시간 비교 위해)
                //Add(dr["TAG_NM"].ToString().Trim(), dr["EQUIP_NM"].ToString().Trim(), dr["MSG_TYPE"].ToString().Trim(), dr["DATA_POSITION"].ToString().Trim(), dr["DATE_POSITION"].ToString().Trim(), dr["TIME_POSITION"].ToString().Trim(), dr["PI_TAG_NM"].ToString().Trim(), dr["OPCITEM_NM"].ToString().Trim(), dr["LAST_UPDATED_VALUE"].ToString().Trim(), dr["LAST_MEASURED_DATETIME"].ToString().Trim());
                DateTime.TryParse(dr["LAST_MEASURED_DATETIME"].ToString().Trim(), out DateTime dt);
                Add(dr["TAG_NM"].ToString().Trim(), dr["EQUIP_NM"].ToString().Trim(), dr["MSG_TYPE"].ToString().Trim(), dr["DATA_POSITION"].ToString().Trim(), dr["DATE_POSITION"].ToString().Trim(), dr["TIME_POSITION"].ToString().Trim(), dr["PI_TAG_NM"].ToString().Trim(), dr["OPCITEM_NM"].ToString().Trim(), dr["EF_ATTRIBUTE_NM"].ToString().Trim(), dr["LAST_UPDATED_VALUE"].ToString().Trim(), dt.ToString("yyyy-MM-dd HH:mm:ss.fff"));
            }
            return true;
        }
        public bool DataProcess(QueueMsg msg)
        {
            //return DataProcess(msg.m_EqName, msg.m_MsgType, msg.m_Data.Split(new string[] { "\n" }, StringSplitOptions.None));
            return DataProcess(msg.m_EqName, msg.m_MsgType, msg.m_Data.Split(new string[] { Environment.NewLine }, StringSplitOptions.None));
        }

        public bool DataProcess(string equipName, int msgType, string[] data)
        {
            if (!DicTAGList.TryGetValue($"{equipName}_{msgType}", out List<TAG> listTAG))
            {
                listViewMsg.UpdateMsg($"There are no TAGs for Equipment : {equipName}, MessageType : {msgType}", false, true, true, PC00D01.MSGTERR);
                return false;
            }
            dtServerTime = DateTime.Now;
            foreach (TAG tag in listTAG)
            {
                tag.ServerTime = dtServerTime;
                if (!tag.DataProcess(data, out string errMessage))
                {
                    listViewMsg.UpdateMsg($"An error occurred while processing data for TAG ({tag.TagName}) - {errMessage}", false, true, true, PC00D01.MSGTERR);
                }
            }

            /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            // PI Point 
            /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            ///
            // 값이 처리된 태그와 중복되지 않은 태그만 처리
            // 20240701, SHS, P5 에서 PI TAG 도 중복체크 하지 않고 수신하는 그대로 저장
            //List<TAG> listUpdated = listTAG.FindAll(x => x.IsValueUpdated && !x.IsDuplicated);
            List<TAG> listUpdated = listTAG.FindAll(x => x.IsValueUpdated);
            // 저장할 데이터가 있는 경우에만
            if (listUpdated.Count > 0)
            {
                // 초기화
                DateTime dtNow = DateTime.Now;
                listUpdated.ForEach(tag =>
                {
                    tag.PIIFDateTime = dtNow;
                    tag.Remark = string.Empty;
                    tag.PIIFFlag = "N"; // N -> Y, E -> F, Z
                    tag.IsDBInserted = false;
                });

                SavePI(listUpdated);
                SaveDBHistory(listUpdated);

                List<TAG> listNotInserted = listUpdated.FindAll(x => !x.IsDBInserted);
                if (listNotInserted.Count > 0)
                {
                    SaveFile(listNotInserted);
                }
            }


            /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            // EventFrame
            /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            ///
            // 값이 처리된 태그와 EFAttributeName 이 설정된 태그만 처리
            List<TAG> listUpdatedEF = listTAG.FindAll(x => x.IsValueUpdated && !string.IsNullOrWhiteSpace(x.EFAttributeName));
            if (listUpdatedEF.Count > 0)
            {
                // AF I/F Time
                string afIFTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
                // SVRTIME
                string serverTime = dtServerTime.ToString("yyyy-MM-dd HH:mm:ss.fff");
                string eventFrameName = $"{equipName}_{dtServerTime:yyyyMMddHHmmssfff}";
                //string eventFrameTemplateName = $"{equipTypeName}_{msgType:00}_Template_A";                
                string measureTime = listUpdatedEF[0].dtTimeStamp.ToString("yyyy-MM-dd HH:mm:ss.fff");

                // EventFrame 저장
                //var efResult = SaveEventFrame(equipName, eventFrameName, eventFrameTemplateName, measureTime, measureTime, listUpdatedEF, listTAG.FindAll(x => !string.IsNullOrWhiteSpace(x.EFAttributeName)));
                List<string> listAttributeNames = listTAG.Where(x => !string.IsNullOrWhiteSpace(x.EFAttributeName)).Select(x => x.EFAttributeName).ToList();

                EventFrameData efData = new EventFrameData()
                {
                    Name = eventFrameName,
                    StartTime = measureTime,
                    EndTime = measureTime,
                    IFTime = afIFTime,
                    EquipmentName = equipName,
                    MessageType = msgType,
                    ServerTime = serverTime, 
                    IFFlag = updateEventFrame ? "E" : "D", 
                    TemplateName = $"{equipTypeName}_{msgType:00}"
                };

                listUpdatedEF.ForEach(tag => efData.Attributes.Add(new EventFrameAttributeData() { Name = tag.EFAttributeName, Value = tag.Value }));

                // updateEventFrame is disabled
                if (!updateEventFrame)
                {
                    listViewMsg.UpdateMsg($"EventFrame Save is disabled.", false, true, true, PC00D01.MSGTINF);
                    efData.IFRemark = "EventFrame Save is disabled.";
                }
                else
                {
                    AFElementTemplate efTemplate = GetEventFrameTemplate(equipTypeName, msgType, listAttributeNames, out string errMessage);
                    if (efTemplate == null)
                    {
                        efData.IFRemark = errMessage.Replace("'", " ");
                    }
                    else // (efTemplate != null)
                    {
                        // AF DB EventFrame 저장
                        var efResult = SaveEventFrame(eventFrameName, efTemplate, measureTime, measureTime, efData.Attributes);

                        efData.IFFlag = efResult.afIFFlag;
                        efData.IFRemark = efResult.afIFRemark.Replace("'", " ");
                        //efData.TemplateName = efTemplate.Name;

                        // EventFrame 저장 성공일때만 EventFrame 정보 TAG 저장, 실패 시 PC03 에서 저장 시 처리
                        if (efResult.afIFFlag.Equals("Y"))
                        {
                            // EventFrameName 저장 TAG 는 MSGTYPE 0, 태그명 장비명_EVENTID 로 태그가 있어야 함
                            // MessageType 저장 TAG 는 MSGTYPE 0, 태그명 장비명_MSGTYPE 로 태그가 있어야 함
                            if (DicTAGList.TryGetValue($"{equipName}_0", out List<TAG> listMsgtype0TAGs))
                            {
                                // 20240819, SHS, EventFrameName 저장 TAG NAme : 장비명_EVENTFRAMENAME -> 장비명_EVENTID
                                TAG tag = listMsgtype0TAGs.Find(x => x.TagName.Equals($"{equipName}_EVENTID"));
                                if (tag != null)
                                {
                                    tag.PIIFDateTime = DateTime.Now;
                                    tag.PIIFFlag = "N"; // N -> Y, E -> F, Z
                                    tag.IsDBInserted = false;
                                    tag.dtTimeStamp = listUpdatedEF[0].dtTimeStamp;
                                    tag.Value = eventFrameName;

                                    // 최근값 업데이트 
                                    if (!(tag.TimeStamp.Equals(tag.LastMeasureDateTime) && tag.Value.Equals(tag.LastMeasureValue)))
                                    {
                                        tag.LastMeasureDateTime = tag.TimeStamp;
                                        tag.LastMeasureValue = tag.Value;
                                    }
                                    SavePI(new List<TAG> { tag }, true);
                                    SaveDBHistory(new List<TAG> { tag });
                                    if (!tag.IsDBInserted)
                                    {
                                        SaveFile(new List<TAG> { tag });
                                    }
                                }
                                else
                                {
                                    listViewMsg.UpdateMsg($"There are no TAGs for EventFrameName : {equipName}, MessageType : 0", false, true, true, PC00D01.MSGTERR);
                                }

                                tag = listMsgtype0TAGs.Find(x => x.TagName.Equals($"{equipName}_MSGTYPE"));
                                if (tag != null)
                                {
                                    tag.PIIFDateTime = DateTime.Now;
                                    tag.Remark = string.Empty;
                                    tag.PIIFFlag = "N"; // N -> Y, E -> F, Z
                                    tag.IsDBInserted = false;
                                    tag.dtTimeStamp = listUpdatedEF[0].dtTimeStamp;
                                    tag.Value = msgType.ToString(); ;

                                    // 최근값 업데이트 
                                    if (!(tag.TimeStamp.Equals(tag.LastMeasureDateTime) && tag.Value.Equals(tag.LastMeasureValue)))
                                    {
                                        tag.LastMeasureDateTime = tag.TimeStamp;
                                        tag.LastMeasureValue = tag.Value;
                                    }
                                    SavePI(new List<TAG> { tag }, true);
                                    SaveDBHistory(new List<TAG> { tag });
                                    if (!tag.IsDBInserted)
                                    {
                                        SaveFile(new List<TAG> { tag });
                                    }
                                }
                                else
                                {
                                    listViewMsg.UpdateMsg($"There are no TAGs for MSGTYPE : {equipName}, MessageType : 0", false, true, true, PC00D01.MSGTERR);
                                }
                            }
                            else
                            {
                                listViewMsg.UpdateMsg($"There are no TAGs for EventFrameName, MSGTYPE : {equipName}, MessageType : 0", false, true, true, PC00D01.MSGTERR);
                            }
                        }
                    }
                }
                //string jsonAttributes = JsonSerializer.Serialize(efData.Attributes);

                // listUpdated 내용을 EventFrame 형식대로 DB 저장 (조회조건에 필요하거나 조회시 표시되어야 할 내용을 컬럼으로)
                bool dbResult = SaveDBEFHistory(equipName, msgType, eventFrameName, measureTime, measureTime, efData.GetSerializedAttributes(), serverTime, efData.TemplateName, efData.IFTime, efData.IFFlag, efData.IFRemark);
                if (!dbResult)
                {
                    //SaveFileEFHistory(equipName, msgType, dtAFIF.ToString("yyyy-MM-dd HH:mm:ss.fff"), measureTime, measureTime, eventFrameName, jsonAttributes, dtAFIF.ToString("yyyy-MM-dd HH:mm:ss.fff"), efResult.afIFFlag, efResult.afIFRemark);
                    SaveFileEFHistory(efData);
                }
            }

            return true;
        }
        private (string afIFFlag, string afIFRemark) SaveEventFrame(string eventFrameName, AFElementTemplate eventFrameTemplate, string startTime, string endTime, List<EventFrameAttributeData> listAttributes)
        {
            try
            {
                //// updateEventFrame is disabled
                //if (!updateEventFrame)
                //{
                //    listViewMsg.UpdateMsg($"EventFrame Save is disabled.", false, true, true, PC00D01.MSGTINF);
                //    return ("D", "EventFrame Save is disabled.");
                //}

                // AF 연결 
                if (!CheckAFDatabase(out string errString))
                {
                    listViewMsg.UpdateMsg($"{errString}", false, true, true, PC00D01.MSGTERR);
                    return ("E", errString);
                }

                // EF 만들기
                AFEventFrame ef = new AFEventFrame(_AFDatabase, eventFrameName, eventFrameTemplate);
                ef.SetStartTime(startTime);
                ef.SetEndTime(endTime);

                listAttributes.ForEach(attrib => ef.Attributes[attrib.Name]?.SetValue(new AFValue(attrib.Value)));
                ef.CheckIn();
            }
            catch (Exception ex)
            {
                listViewMsg.UpdateMsg($"Exception in SaveEventFrame - ({ex})", false, true, true, PC00D01.MSGTERR);
                return ("E", $"Exception in SaveEventFrame - ({ex})");
            }
            return ("Y", string.Empty);
        }

        private bool SaveFileEFHistory(EventFrameData efData)// string equipName, int msgType, string equipIFTime, string measureStartTime, string measureEndTime, string eventFrameName, string jsonAttributes, string afIFTime, string afIFFlag, string afIFRemark)
        {
            string fullFileName;
            try
            {
                while (true)
                {
                    fullFileName = $@"{FilePath}\{efData.Name}.eff";
                    if (File.Exists(fullFileName))
                        continue;
                    File.AppendAllText(fullFileName, JsonSerializer.Serialize(efData), dataEncoding);
                    string fileContent = File.ReadAllText(fullFileName);
                    listViewMsg.UpdateMsg($"EventFrame ({efData.Name}) File saved. ({fullFileName}) {fileContent}", false, true, true, PC00D01.MSGTINF);
                    break;
                }
            }
            catch (Exception ex)
            {
                listViewMsg.UpdateMsg($"Exception in SaveFileEFHistory. EventFrame ({efData.Name}) - ({ex})", false, true, true, PC00D01.MSGTERR);
                return false;
            }

            return true;
        }


        // HI_EVENTFRAME_RESULT TABLE
        // (ID), EQUIP_NM, MSG_TYPE, EQUIP_IF_TIME, MEASURE_START_TIME, MEASURE_END_TIME, EVENTFRAME_NAME, EVENTFRAME_DATA, AF_IF_TIME, AF_IF_FLAG, AF_IF_REMARK, (REG_ID), (REG_TIME)
        private bool SaveDBEFHistory(string equipName, int msgType, string eventFrameName, string measureStartTime, string measureEndTime, string jsonAttributes, string serverTime, string templateName, string afIFTime, string afIFFlag, string afIFRemark)
        {
            bool result = false;
            string errCode = string.Empty;
            string errText = string.Empty;

            try
            {
                if (result = m_sqlBiz.InsertEFResult(equipName, msgType, measureStartTime, measureEndTime, eventFrameName, jsonAttributes, serverTime, templateName, afIFTime, afIFFlag, afIFRemark, ref errCode, ref errText))
                {
                    listViewMsg.UpdateMsg($"DB inserted. EventFrame ({eventFrameName})", false, true, true, PC00D01.MSGTINF);
                }
                else
                {
                    listViewMsg.UpdateMsg($"DB insert failed. EventFrame ({eventFrameName}) - {errText}", false, true, true, PC00D01.MSGTERR);
                }
            }
            catch (Exception ex)
            {
                listViewMsg.UpdateMsg($"Exception in SaveDBHistoryEF. EventFrame ({eventFrameName}) - ({ex})", false, true, true, PC00D01.MSGTERR);
                return false;
            }
            return result;
        }

        private bool CheckAFDatabase(out string errText)
        {
            errText = string.Empty;
            lock (objLock)
            {
                // AF 연결 
                if (_AFDatabase == null || !_AFDatabase.PISystem.ConnectionInfo.IsConnected)
                {
                    _AFDatabase = GetAFDatabase(m_clsPIInfo.AF_SERVER, m_clsPIInfo.AF_DB, m_clsPIInfo.AF_USER, m_clsPIInfo.AF_PWD, m_clsPIInfo.AF_DOMAIN, out string errString);
                    if (_AFDatabase == null)
                    {
                        listViewMsg.UpdateMsg($"AF not connected.", false, true, true, PC00D01.MSGTINF);
                        errText = $"AF not connected. {(string.IsNullOrWhiteSpace(errString) ? "" : errString)}";
                        return false;
                    }
                }
                if (!_AFDatabase.PISystem.ConnectionInfo.IsConnected)
                {
                    listViewMsg.UpdateMsg($"PISystem not connected.", false, true, true, PC00D01.MSGTINF);
                    errText = "PISystem not connected.";
                    return false;
                }
            }
            return true;
        }


        public AFDatabase GetAFDatabase(string serverName, string databaseName, string user, string pwd, string domain, out string errString)
        {
            try
            {
                errString = string.Empty;

                PISystems systems = new PISystems();
                PISystem assetServer;

                if (!string.IsNullOrEmpty(serverName))
                    assetServer = systems[serverName];
                else
                    assetServer = systems.DefaultPISystem;

                NetworkCredential credential = new NetworkCredential(user, pwd, domain);

                assetServer.Connect(credential);
                if (assetServer.ConnectionInfo.IsConnected)
                {
                    if (!string.IsNullOrEmpty(databaseName))
                        return assetServer.Databases[databaseName];
                    else
                        return assetServer.Databases.DefaultDatabase;
                }
            }
            catch (Exception ex)
            {
                errString = ex.Message;
            }
            return null;
        }

        private AFElementTemplate GetEventFrameTemplate(string templateName)
        {
            AFElementTemplate result = _AFDatabase?.ElementTemplates?.Where(x => x.InstanceType.Equals(typeof(AFEventFrame)) && x.Name.Equals(templateName))?.FirstOrDefault();
            //return afDB?.ElementTemplates?.Where(x => x.InstanceType.Equals(typeof(AFEventFrame)) && x.Name.Equals(templateName)).First();
            return result;
        }

        //private AFElementTemplate GetEventFrameTemplate(string equipTypeName, int msgType, List<string> listAttributeNames, out string errMessage)
        //{
        //    try
        //    {
        //        errMessage = string.Empty;
        //        AFElementTemplate efTemp = null;

        //        //if (afDB == null) throw new ArgumentNullException(nameof(afDB));
        //        string efTemplateName = m_sqlBiz.ReadSTCommon(equipTypeName, "EventFrameTemplateName");

        //        // 최초 afDB 로드 후 다른 인스턴스에서 변경된 내용은 refresh 해야 한다
        //        // AF 연결 
        //        if (!CheckAFDatabase(out string errString))
        //        {
        //            listViewMsg.UpdateMsg($"{errString}", false, true, true, PC00D01.MSGTERR);
        //            errMessage = errString;
        //            return null;
        //        }
        //        _AFDatabase.Refresh();

        //        if (!string.IsNullOrWhiteSpace(efTemplateName))
        //        {
        //            // 설정된 템플릿 조회
        //            efTemp = GetEventFrameTemplate(efTemplateName);

        //            if (efTemp != null)
        //            {
        //                List<string> omittedAttributeNames = efTemp.AttributeTemplates.GetOmittedAttributes(listAttributeNames);
        //                // Template 의 Attribute 와 TAG Attribute 가 완전 동일하면 사용 
        //                if (omittedAttributeNames.Count == 0 && efTemp.AttributeTemplates.Count == listAttributeNames.Count)
        //                {
        //                    return efTemp;
        //                }
        //                listViewMsg.UpdateMsg($"EventFrameTemplate({efTemplateName}) is not matches. Omitted attribute names : {string.Join(", ", omittedAttributeNames)}.", false, true, true, PC00D01.MSGTINF);
        //            }
        //        }

        //        // 기본 템플릿 이름으로 시작하는 템플릿 모두 조회
        //        //AFElementTemplates eventFrameTemplate = afDB.ElementTemplates?.Where(x => x.InstanceType.Equals(typeof(AFEventFrame)) && x.Name.StartsWith(efTemplateBaseName)).OrderBy(x => x.Name);
        //        IEnumerable<AFElementTemplate> eventFrameTemplates = from temp in _AFDatabase.ElementTemplates
        //                                                             where temp.InstanceType.Equals(typeof(AFEventFrame)) && temp.Name.StartsWith($"{equipTypeName}_{msgType:00}") && !temp.Name.Equals(efTemplateName)
        //                                                             orderby temp.Name descending
        //                                                             select temp;

        //        efTemp = eventFrameTemplates.FirstOrDefault(x => x.AttributeTemplates.GetOmittedAttributes(listAttributeNames).Count == 0);

        //        // 저장가능한 템플릿이 없으면 새로 생성
        //        if (efTemp == null)
        //        {
        //            string eventFrameTemplateName = $"{equipTypeName}_{msgType:00}_{DateTime.Now:yyyyMMddHHmmssfff}";
        //            listViewMsg.UpdateMsg($"There are no matched EventFrameTemplates. Create new EventFrameTemplate Name : {eventFrameTemplateName}, AttributeTemplates : {string.Join(", ", listAttributeNames)}.", false, true, true, PC00D01.MSGTINF);
        //            efTemp = CreateEventFrameTemplate(_AFDatabase, eventFrameTemplateName, listAttributeNames);
        //        }
        //        // db 저장된 템플릿을 사용하지 않았으면 사용한 EventFrameTemplate Name DB 업데이트
        //        m_sqlBiz.WriteSTCommon(equipTypeName, "EventFrameTemplateName", efTemp.Name);
        //        return efTemp;
        //        //return efTemp ?? CreateEventFrameTemplate(afDB, $"{efTemplateBaseName}_{DateTime.Now:yyyyMMddHHmmssfff}", listAttributeNames);
        //    }
        //    catch (Exception ex)
        //    {
        //        errMessage = $"Exception in GetEventFrameTemplate - ({ex})";
        //        listViewMsg.UpdateMsg($"Exception in GetEventFrameTemplate - ({ex})", false, true, true, PC00D01.MSGTERR);
        //        return null;
        //    }
        //}
        /// <summary>
        /// 20240820. SHS. EventFrame Template 가져오기. 
        /// 장비타입_MSGTYPE 이름의 템플릿 고정
        /// AF DB 에 해당 템플릿이 없는 경우에만 TAG 정보 기반으로 템플릿 생성
        /// </summary>
        /// <param name="equipTypeName"></param>
        /// <param name="msgType"></param>
        /// <param name="listAttributeNames"></param>
        /// <param name="errMessage"></param>
        /// <returns></returns>
        private AFElementTemplate GetEventFrameTemplate(string equipTypeName, int msgType, List<string> listAttributeNames, out string errMessage)
        {
            try
            {
                errMessage = string.Empty;

                // 최초 afDB 로드 후 다른 인스턴스에서 변경된 내용은 refresh 해야 한다
                // AF 연결 
                if (!CheckAFDatabase(out string errString))
                {
                    listViewMsg.UpdateMsg($"{errString}", false, true, true, PC00D01.MSGTERR);
                    errMessage = errString;
                    return null;
                }

                AFElementTemplate efTemplate = null;
                string efTemplateName = $"{equipTypeName}_{msgType:00}";

                _AFDatabase.Refresh();
                // 템플릿 조회
                efTemplate = GetEventFrameTemplate(efTemplateName);
                if (efTemplate != null) 
                {
                    listViewMsg.UpdateMsg($"GetEventFrameTemplate({efTemplateName}) success.", false, true, true, PC00D01.MSGTINF);
                }
                else
                {
                    // 없을때만 새로 생성
                    listViewMsg.UpdateMsg($"GetEventFrameTemplate({efTemplateName}) failed.", false, true, true, PC00D01.MSGTERR);
                    listViewMsg.UpdateMsg($"Create a new EventFrameTemplate({efTemplateName}), AttributeTemplates : {string.Join(", ", listAttributeNames)}.", false, true, true, PC00D01.MSGTINF);
                    efTemplate = CreateEventFrameTemplate(_AFDatabase, efTemplateName, listAttributeNames);
                }
                return efTemplate;
            }
            catch (Exception ex)
            {
                errMessage = $"Exception in GetEventFrameTemplate - ({ex})";
                listViewMsg.UpdateMsg($"Exception in GetEventFrameTemplate - ({ex})", false, true, true, PC00D01.MSGTERR);
                return null;
            }
        }

        private AFElementTemplate CreateEventFrameTemplate(AFDatabase database, string eventFrameTemplateName, List<string> listAttributeNames)
        {
            AFElementTemplate eventFrameTemplate = null;

            if (database == null) throw new ArgumentNullException(nameof(database));

            eventFrameTemplate = database.ElementTemplates.Add(eventFrameTemplateName);
            eventFrameTemplate.InstanceType = typeof(AFEventFrame);
            eventFrameTemplate.Description = $"{eventFrameTemplateName}";

            listViewMsg.UpdateMsg($"CreateEventFrameTemplate ({eventFrameTemplateName}).", false, true, true, PC00D01.MSGTINF);

            AFAttributeTemplate afAttrib;

            foreach (var attribName in listAttributeNames)
            {
                afAttrib = eventFrameTemplate.AttributeTemplates.Add(attribName);
                afAttrib.Type = typeof(string);
            }

            eventFrameTemplate.CheckIn();

            return eventFrameTemplate;
        }

        private bool CheckPIConnection(out string errText)
        {
            errText = string.Empty;

            if (string.IsNullOrWhiteSpace(m_clsPIInfo.strPI_Server))
            {
                errText = "No PI Server info.";
                return false;
            }

            if (_PIServer == null)
            {
                _PIServer = PIServer.FindPIServer(_PISystem, m_clsPIInfo.strPI_Server);
                if (_PIServer == null)
                {
                    errText = "FindPIServer Returned Null.";
                    return false;
                }
            }
            if (!_PIServer.ConnectionInfo.IsConnected)
            {
                try
                {
                    _PIServer.Connect();
                }
                catch (Exception ex)
                {
                    listViewMsg.UpdateMsg($"PI Server Connection ({m_clsPIInfo.strPI_Server})- {ex}", false, true, true, PC00D01.MSGTERR);
                    errText = $"Exception PI Server Connection ({m_clsPIInfo.strPI_Server})";
                    return false;
                }
                if (!_PIServer.ConnectionInfo.IsConnected)
                {
                    errText = "PI Not Connected.";
                    return false;
                }
            }
            return true;
        }

        // 20240703, SHS, P5, EventFrame 정보 TAG 는 PIPoint 업데이트 Disabled 상태에도 처리해야 함. PIPoint 업데이트 Disabled 를 무시하는 파라미터 추가 (기본은 무시안함)
        //private bool SavePI(List<TAG> listResult)
        private bool SavePI(List<TAG> listResult, bool ignoreDisabled = false)
        {
            try
            {
                string errText;
                bool result = false;

                // UpdatePIPoint is disabled
                if (!updatePIPoint && !ignoreDisabled)
                {
                    // PIPoint 업데이트 Disabled 상태
                    listResult.ForEach(tag =>
                    {
                        tag.PIIFFlag = "D";
                        tag.Remark = "PIPoint Save is disabled.";
                    });
                    listViewMsg.UpdateMsg($"PIPoint Save is disabled.", false, true, true, PC00D01.MSGTINF);
                    return false;
                }

                if (!CheckPIConnection(out errText))
                {
                    listResult.ForEach(tag =>
                    {
                        tag.PIIFFlag = "E";
                        tag.Remark = errText;
                    });
                    listViewMsg.UpdateMsg(string.Format(PC00D01.FailedtoPI, $"{errText}", ""), false, true, true, PC00D01.MSGTERR);
                    return false;
                }

                foreach (TAG tag in listResult)
                {
                    errText = string.Empty;
                    tag.PIIFFlag = "E";
                    result = false;

                    if (string.IsNullOrWhiteSpace(tag.PITagName))
                    {
                        errText = "Missing PI TAG Name";
                    }
                    else
                    {
                        result = SetPIValue(tag.PITagName, tag.Value, tag.dtTimeStamp, ref errText);
                    }

                    if (result)
                    {
                        tag.PIIFFlag = "Y";
                        listViewMsg.UpdateMsg(string.Format(PC00D01.SucceededtoPI, $"{tag.TagName} - {tag.PITagName}", tag.Value), false, true, true, PC00D01.MSGTINF);
                    }
                    else
                    {
                        tag.PIIFFlag = "E";
                        tag.Remark = errText;
                        listViewMsg.UpdateMsg(string.Format(PC00D01.FailedtoPI, $"{errText} - {tag.TagName} - {tag.PITagName}", tag.Value), false, true, true, PC00D01.MSGTERR);
                    }
                }
            }
            catch (Exception ex)
            {
                listViewMsg.UpdateMsg($"Exception in SavePI - ({ex})", false, true, true, PC00D01.MSGTERR);
                return false;
            }
            return true;
        }
        public bool SetPIValue(string pointName, object pointValue, DateTime mTime, ref string _strErrText)
        {
            try
            {
                PIPoint point = PIPoint.FindPIPoint(_PIServer, pointName);

                AFTime aTime = new AFTime(mTime.ToUniversalTime());

                AFValue value = new AFValue(pointValue, aTime);

                //AFValue value = new AFValue(pointValue, aTime, null, AFValueStatus.Bad);

                //2021.05.20 변경요청 [김현지 프로]
                //point.UpdateValue(value, OSIsoft.AF.Data.AFUpdateOption.Replace);
                //point.UpdateValue(value, OSIsoft.AF.Data.AFUpdateOption.Insert);
                // 버퍼 미사용 옵션
                point.UpdateValue(value, OSIsoft.AF.Data.AFUpdateOption.Insert, OSIsoft.AF.Data.AFBufferOption.Buffer);

            }
            catch (Exception ex)
            {
                _strErrText = ex.ToString().Replace("\\", " ").Replace("\r\n", " ").Replace("'", " ");
                return false;
            }
            return true; ;
        }
        private bool SaveDBHistory(List<TAG> listResult)
        {
            string errCode = string.Empty;
            string errText = string.Empty;

            try
            {
                foreach (TAG tag in listResult)
                {
                    errCode = string.Empty;
                    errText = string.Empty;

                    if (tag.IsDBInserted = m_sqlBiz.InsertResult(tag.TagName, tag.TimeStamp, tag.Value, tag.PIIFFlag, tag.PIIFTimeStamp, tag.Remark, ref errCode, ref errText))
                    {
                        listViewMsg.UpdateMsg($"DB inserted. ({tag.TTFTV})", false, true, true, PC00D01.MSGTINF);
                    }
                    else
                    {
                        listViewMsg.UpdateMsg($"DB insert failed. ({tag.TTFTV}) - {errText}", false, true, true, PC00D01.MSGTERR);
                    }
                }
            }
            catch (Exception ex)
            {
                listViewMsg.UpdateMsg($"Exception in SaveDBHistory - ({ex})", false, true, true, PC00D01.MSGTERR);
                return false;
            }
            return true;
        }
        private bool SaveFile(List<TAG> listResult)
        {
            string fullFileName;
            try
            {
                while (true)
                {
                    fullFileName = $@"{FilePath}\{DateTime.Now:yyyyMMddHHmmssfff}.ttv";
                    if (File.Exists(fullFileName))
                        continue;
                    File.AppendAllLines(fullFileName, listResult.Select(x => x.TTFTV), dataEncoding);
                    string fileContent = File.ReadAllText(fullFileName);
                    listViewMsg.UpdateMsg($"File saved. ({fullFileName}) {fileContent}", false, true, true, PC00D01.MSGTINF);
                    break;
                }
            }
            catch (Exception ex)
            {
                listViewMsg.UpdateMsg($"Exception in SaveFile - ({ex})", false, true, true, PC00D01.MSGTERR);
                return false;
            }

            return true;
        }
    }
}
