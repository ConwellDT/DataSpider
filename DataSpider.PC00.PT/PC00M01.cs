using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using OSIsoft.AF;
using OSIsoft.AF.Asset;
using OSIsoft.AF.PI;
using OSIsoft.AF.Time;

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

        public PC00M01(IPC00F00 owner, string equipType, string equipName, string connectionInfo, string extraInfo, int nCurNo, bool bAutoRun = false) : base(owner, equipType, equipName, connectionInfo, extraInfo, nCurNo, bAutoRun)
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
            DataTable dtCurrTagInfo = m_sqlBiz.GetTagInfoForDSC(m_Type, string.Empty, true, ref errCode, ref errText);
            if (string.IsNullOrWhiteSpace(errText))
            {
                if (dtCurrTagInfo.Rows.Count < 1)
                {
                    listViewMsg.UpdateMsg($"There are no TAGs for Equipment Type : {m_Type}, Server Name : {Environment.MachineName}");
                    return false;
                }
                if (PC00U01.TryParseExact(dtCurrTagInfo.Select("", "UPDATE_REG_DATE DESC")[0]["UPDATE_REG_DATE"]?.ToString(), out DateTime dtTemp))
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

            dataProcess = new EquipmentDataProcess(m_ConnectionInfo, dtTagInfo, listViewMsg, dataEncoding);
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
        public bool Available
        {
            get { return !string.IsNullOrWhiteSpace(ReplaceTag) || Line > 0 && Offset > 0 && (Size > 0 || Size == -1 || Size == -2 || Size == -3 || Size == -4); }
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
                ReplaceTag = posInfo.Trim().Substring(1);
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
        public TTVTAG(string tagName, string equipName, int msgType, string piTagName, string ttvTagName, string lastMeasuredValue, string lastMeasuredDateTime) : base(tagName, equipName, msgType, piTagName, lastMeasuredValue, lastMeasuredDateTime)
        {
            TTVTagName = ttvTagName;
        }
        public override bool DataProcess(string[] data, out string errMessage)
        {
            IsValueUpdated = false;
            errMessage = string.Empty;
            //data.ToList().Find(x => x.Split(',')[0].Equals(TTVTagName))
            foreach (string line in data)
            {
                string[] items = line.Split(',');
                if (items != null && items.Length > 2)
                {
                    if (items[0].Trim().Equals(TTVTagName))
                    {
                        //tagValue.Value = items[2].Trim();
                        tagValue.Value = string.Join(",", items, 2, items.Length - 2).Replace("'", "''").Trim();
                        if (!PC00U01.TryParseExact($"{items[1].Trim()}", out dtTimeStamp))
                        {
                            errMessage = $"Parsing TimeStamp failed ({items[1]})";
                            return false;
                        }
                        Debug.WriteLine(TTV);
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
        }
        public string TTV
        {
            get { return $"{TagName},{TimeStamp},{Value}"; }
        }

        // 20220331, SHS, 통합처리구조 변경
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
        public string Remark { get; set; }
        public string LastMeasureDateTime { get; set; } = string.Empty;
        public string LastMeasureValue { get; set; } = string.Empty;
        // ---

        public bool IsValueUpdated { get; set; }

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
        public TAG(string tagName, string equipName, int msgType, string piTagName, string lastMeasuredValue, string lastMeasuredDateTime)
        {
            TagName = tagName;
            MsgType = msgType;
            EquipName = equipName;
            PITagName = piTagName;
            LastMeasureValue = lastMeasuredValue;
            LastMeasureDateTime = lastMeasuredDateTime;
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
            IsValueUpdated = false;

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

            //IsValueUpdated = true;
            // 20220404, SHS, 이전 데이터와 값과 시간이 모두 같으면 업데이트로 판단하지 않음 (중복데이터 제거)
            if (!(TimeStamp.Equals(LastMeasureDateTime) && Value.Equals(LastMeasureValue)))
            {
                // server time replace tag 인데 측정시간이 같으면 제외
                if (!(tagValue.ReplaceTag.Equals(ReplaceTagDef.SERVER_TIME_TAG) && TimeStamp.Equals(LastMeasureDateTime)))
                {
                    LastMeasureDateTime = TimeStamp;
                    LastMeasureValue = Value;
                    IsValueUpdated = true;
                }
            }
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
        static PISystem _PIStstem;
        static PIServer _PIserver;
        private PIInfo m_clsPIInfo;

        public EquipmentDataProcess(string filePath, FormListViewMsg _listViewMsg, Encoding _dataEncoding = null)
        {
            FilePath = filePath;
            listViewMsg = _listViewMsg;
            dataEncoding = _dataEncoding == null ? Encoding.UTF8 : _dataEncoding;

            try
            {
                m_clsPIInfo = ConfigHelper.GetPIInfo();
                _PIserver = PIServer.FindPIServer(_PIStstem, m_clsPIInfo.strPI_Server);
                _PIserver.Connect();
            }
            catch (Exception ex)
            {
                listViewMsg.UpdateMsg($"PI Server Connection - {ex}", false, true, true, PC00D01.MSGTERR);
            }
        }
        public EquipmentDataProcess(string filePath, DataTable dtTag, FormListViewMsg listViewMsg, Encoding dataEncoding = null) : this(filePath, listViewMsg, dataEncoding)
        {
            UpdateTag(dtTag);
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
        public bool Add(string tagName, string equipName, string msgType, string dataPosition, string datePosition, string timePosition, string piTagName, string opcItemName, string lastMeasuredValue, string lastMeasuredDateTime)
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
                TAG tag = new TAG(tagName, equipName, msgTypeNo, piTagName, lastMeasuredValue, lastMeasuredDateTime);
                if (!tag.UpdateFormat(dataPosition, datePosition, timePosition, out string errMessage))
                {
                    listViewMsg.UpdateMsg($"An error occurred while update TAG format ({tag.TagName}) - {errMessage}", false, true, true, PC00D01.MSGTERR);
                    return false;
                }
                DicTAGList[tagKey].Add(tag);
            }
            else
            {
                TTVTAG tag = new TTVTAG(tagName, equipName, msgTypeNo, piTagName, opcItemName, lastMeasuredValue, lastMeasuredDateTime);
                DicTAGList[tagKey].Add(tag);
            }

            return true;
        }
        public bool Add(DataTable dtTag)
        {
            foreach (DataRow dr in dtTag.Rows)
            {
                //Add(dr["TAG_NM"].ToString().Trim(), dr["EQUIP_NM"].ToString().Trim(), dr["MSG_TYPE"].ToString().Trim(), dr["DATA_POSITION"].ToString().Trim(), dr["DATE_POSITION"].ToString().Trim(), dr["TIME_POSITION"].ToString().Trim(), dr["OPCITEM_NM"].ToString().Trim());
                Add(dr["TAG_NM"].ToString().Trim(), dr["EQUIP_NM"].ToString().Trim(), dr["MSG_TYPE"].ToString().Trim(), dr["DATA_POSITION"].ToString().Trim(), dr["DATE_POSITION"].ToString().Trim(), dr["TIME_POSITION"].ToString().Trim(), dr["PI_TAG_NM"].ToString().Trim(), dr["OPCITEM_NM"].ToString().Trim(), dr["LAST_UPDATED_VALUE"].ToString().Trim(), dr["LAST_MEASURED_DATETIME"].ToString().Trim());
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
            /*
            List<TAG> listresult = listTAG.FindAll(x => x.IsValueUpdated);
            // 저장할 데이터가 있는 경우에만
            if (listresult.Count > 0)
            {
                string fullFileName;
                while (true)
                {
                    fullFileName = $@"{FilePath}\{DateTime.Now:yyyyMMddHHmmssfff}.ttv";
                    if (File.Exists(fullFileName))
                        continue;
                    //File.AppendAllLines(fullFileName, listTAG.FindAll(x => x.IsValueUpdated).Select(x => x.TTV));
                    File.AppendAllLines(fullFileName, listresult.Select(x => x.TTV), dataEncoding);
                    string fileContent=File.ReadAllText(fullFileName);
                    listViewMsg.UpdateMsg($"File saved. ({fullFileName}) {fileContent}", false, true, true, PC00D01.MSGTINF);
                    break;
                }
            }
            */
            List<TAG> listUpdated = listTAG.FindAll(x => x.IsValueUpdated);
            // 저장할 데이터가 있는 경우에만
            if (listUpdated.Count > 0)
            {
                // 초기화
                DateTime dtNow = DateTime.Now;
                listUpdated.ForEach(x => x.PIIFDateTime = dtNow);
                listUpdated.ForEach(x => x.Remark = string.Empty);
                listUpdated.ForEach(x => x.PIIFFlag = "N");
                listUpdated.ForEach(x => x.IsDBInserted = false);
                //
                SavePI(listUpdated);
                SaveDBHistory(listUpdated);

                List<TAG> listNotInserted = listUpdated.FindAll(x => !x.IsDBInserted);
                if (listNotInserted.Count > 0)
                {
                    SaveFile(listNotInserted);
                }
            }
            return true;
        }
        private bool SavePI(List<TAG> listResult)
        {
            try
            {
                string errText;
                bool result = false;

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
                PIPoint point = PIPoint.FindPIPoint(_PIserver, pointName);

                AFTime aTime = new AFTime(mTime.ToUniversalTime());

                AFValue value = new AFValue(pointValue, aTime);

                //AFValue value = new AFValue(pointValue, aTime, null, AFValueStatus.Bad);

                //2021.05.20 변경요청 [김현지 프로]
                //point.UpdateValue(value, OSIsoft.AF.Data.AFUpdateOption.Replace);
                //point.UpdateValue(value, OSIsoft.AF.Data.AFUpdateOption.Insert);
                // 버퍼 미사용 옵션
                point.UpdateValue(value, OSIsoft.AF.Data.AFUpdateOption.Insert, OSIsoft.AF.Data.AFBufferOption.DoNotBuffer);

            }
            catch (Exception ex)
            {
                _strErrText = ex.ToString().Replace("\\", "").Replace("\r\n", "").Replace("'", ""); ;
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
