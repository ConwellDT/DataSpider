using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Text;
using System.Text.Json;
using System.Windows.Forms;

using OSIsoft.AF;
using OSIsoft.AF.PI;

/// <summary>
/// 큐 처리 구현
/// 데이터 파일 저장 : 1개 처리 기준 데이터(string 배열)를 설정대로 파싱하여 태그명, 시간, 값 형태 파일(yyyyMMddHHmmssfff.ttv)로 저장
/// </summary>
namespace DataSpider.PC00.PT
{
    public class PC00M02 : PC00B01
    {
        private EquipmentDataProcess2 dataProcess = null;
        private DataGridView dataGridView_Main;
        private DataGridView dataGridView_Attributes;
        private DataTable dtTagInfo = null;
        private DateTime dtLastTagUpdated = DateTime.MinValue;
        private DateTime dtCheckTagUpdate = DateTime.MinValue;
        private bool isTagUpdated = false;
        private int intervalTagUpdate = 60;

        // 20220908, SHS, SERVERTIME 중복체크 (시간만 비교할지 값도 비교할지) 옵션 기능 추가
        private bool checkServerTimeDup = false;

        // 20220908, SHS, SERVERTIME 중복체크 (시간만 비교할지 값도 비교할지) 옵션 기능 추가
        public PC00M02()
        {
        }
        public void ProcessData(DataGridView dataGridView, DataGridView dataGridView_EventFrame, DataGridView dataGridView_Attributes)
        {
            GetTagInfo();
            Job(dataGridView, dataGridView_EventFrame, dataGridView_Attributes);
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
            DataTable dtCurrTagInfo = m_sqlBiz.GetTagInfoForDSC(m_Name, string.Empty, true, ref errCode, ref errText);
            if (string.IsNullOrWhiteSpace(errText))
            {
                if (dtCurrTagInfo.Rows.Count < 1)
                {
                    return false;
                }
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
                return false;
            }
        }
        private void CheckTagUpdated()
        {
            DateTime dtNow = DateTime.Now;
            if (dtCheckTagUpdate.AddSeconds(intervalTagUpdate).CompareTo(dtNow) < 0)
            {
                dtCheckTagUpdate = dtNow;
                if (GetTagInfo() && isTagUpdated)
                {
                    dataProcess.UpdateTag(dtTagInfo);
                }
            }
        }
        private void Job(DataGridView dataGridView, DataGridView dataGridView_EventFrame, DataGridView dataGridView_Attributes)
        {
            dataProcess = new EquipmentDataProcess2(m_ConnectionInfo, drEquipment, dtTagInfo, listViewMsg, dataEncoding, checkServerTimeDup);
            try
            {
                CheckTagUpdated();

                QueueMsg msg = PC00U01.ReadQueue();
                if (msg == null)
                {
                    return;
                }
                if (dataProcess.DataProcess(msg, dataGridView, dataGridView_EventFrame, dataGridView_Attributes))
                {
                }
                else
                {
                }
            }
            catch (Exception ex)
            {
            }
        }
    }

    public class Data2
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
                switch (ReplaceTag)
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
                Value = string.Empty;
                return true;
            }

            Value = PC00U01.ExtractString(line, Offset, Size);
            return true;
        }
    }

    public class TTVTAG2 : TAG2
    {
        public string TTVTagName { get; set; } = string.Empty;

        public TTVTAG2(string tagName, string equipName, int msgType, string piTagName, string ttvTagName) : base(tagName, equipName, msgType, piTagName)
        {
            TTVTagName = ttvTagName;
        }
        public TTVTAG2(string tagName, string equipName, int msgType, string piTagName, string ttvTagName, string efAttributeName, string lastMeasuredValue, string lastMeasuredDateTime) : base(tagName, equipName, msgType, piTagName, efAttributeName, lastMeasuredValue, lastMeasuredDateTime)
        {
            TTVTagName = ttvTagName;
        }
        public override bool DataProcess(string[] data, out string errMessage)
        {
            IsDuplicated = IsValueUpdated = false;
            errMessage = string.Empty;

            foreach (string line in data)
            {
                string[] items = line.Split(',');
                if (items != null && items.Length > 2)
                {
                    if (items[0].Trim().Equals(TTVTagName))
                    {
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

                        if (dtTimeStamp.CompareTo(DateTime.Parse("1753-01-01 00:00:00.000")) < 0)
                        {
                            errMessage = $"Invalid TimeStamp. TimeStamp is before 1753-01-01 00:00:00.000 ({TimeStamp})";
                            return false;
                        }

                        if (!(TimeStamp.Equals(LastMeasureDateTime) && Value.Equals(LastMeasureValue)))
                        {
                            LastMeasureDateTime = TimeStamp;
                            LastMeasureValue = Value;
                            IsDuplicated = true;
                        }

                        IsValueUpdated = true;
                        break;
                    }
                }
            }
            return true;
        }
    }
    public class TAG2
    {
        public bool checkServerTimeDup = false;

        protected Data2 tagValue = new Data2();
        private Data2 dateValue = new Data2();
        private Data2 timeValue = new Data2();
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
        public string Remark { get; set; } = string.Empty;
        public string LastMeasureDateTime { get; set; } = string.Empty;
        public string LastMeasureValue { get; set; } = string.Empty;

        public string EFAttributeName { get; set; } = string.Empty;

        public bool IsValueUpdated { get; set; }

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
            return timeValue.UpdateDataPosition(posInfo, out errMessage);
        }

        public TAG2(string tagName, string equipName, int msgType, string piTagName)
        {
            TagName = tagName;
            MsgType = msgType;
            EquipName = equipName;
            PITagName = piTagName;
        }
        public TAG2(string tagName, string equipName, int msgType, string piTagName, string efAttributeName, string lastMeasuredValue, string lastMeasuredDateTime)
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
            if (!UpdateDataPosition(dataPosition, out errMessage))
                return false;
            if (!UpdateDatePosition(datePosition, out errMessage))
                return false;
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

            if (dtTimeStamp.CompareTo(DateTime.Parse("1753-01-01 00:00:00.000")) < 0)
            {
                errMessage = $"Invalid TimeStamp. TimeStamp is before 1753-01-01 00:00:00.000 ({TimeStamp})";
                return false;
            }

            if (!(TimeStamp.Equals(LastMeasureDateTime) && Value.Equals(LastMeasureValue)))
            {
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
            }
            IsValueUpdated = true;
            return true;
        }
    }
    public class EquipmentDataProcess2
    {
        public string FilePath { get; set; } = string.Empty;
        public Dictionary<string, List<TAG2>> DicTAGList { get; private set; } = new Dictionary<string, List<TAG2>>();
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

        // 20220908, SHS, SERVERTIME 중복체크 (시간만 비교할지 값도 비교할지) 옵션 기능 추가
        public bool CheckServerTimeDup { get; set; } = false;
        public EquipmentDataProcess2(string filePath, FormListViewMsg _listViewMsg, Encoding _dataEncoding = null)
        {
            FilePath = filePath;
            listViewMsg = _listViewMsg;
            dataEncoding = _dataEncoding == null ? Encoding.UTF8 : _dataEncoding;
        }
        // 20220908, SHS, SERVERTIME 중복체크 (시간만 비교할지 값도 비교할지) 옵션 기능 추가
        public EquipmentDataProcess2(string filePath, DataRow dr, DataTable dtTag, FormListViewMsg listViewMsg, Encoding dataEncoding = null, bool checkServerTimeDup = false) : this(filePath, listViewMsg, dataEncoding)
        {
            // 20220908, SHS, SERVERTIME 중복체크 (시간만 비교할지 값도 비교할지) 옵션 기능 추가
            CheckServerTimeDup = checkServerTimeDup;
            UpdateTag(dtTag);
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
        public bool Add(string tagName, string equipName, string msgType, string dataPosition, string datePosition, string timePosition, string piTagName, string opcItemName)
        {
            if (!int.TryParse(msgType, out int msgTypeNo))
            {
                msgTypeNo = 1;
            }
            string tagKey = $"{equipName}_{msgTypeNo}";

            if (!DicTAGList.ContainsKey(tagKey))
            {
                DicTAGList.Add(tagKey, new List<TAG2>());
            }
            if (string.IsNullOrWhiteSpace(opcItemName))
            {
                TAG2 tag = new TAG2(tagName, equipName, msgTypeNo, piTagName);
                if (!tag.UpdateFormat(dataPosition, datePosition, timePosition, out string errMessage))
                {
                    return false;
                }
                DicTAGList[tagKey].Add(tag);
            }
            else
            {
                TTVTAG2 tag = new TTVTAG2(tagName, equipName, msgTypeNo, piTagName, opcItemName);
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
                DicTAGList.Add(tagKey, new List<TAG2>());
            }
            if (string.IsNullOrWhiteSpace(opcItemName))
            {
                TAG2 tag = new TAG2(tagName, equipName, msgTypeNo, piTagName, efAttributeName, lastMeasuredValue, lastMeasuredDateTime);
                // 20220908, SHS, SERVERTIME 중복체크 (시간만 비교할지 값도 비교할지) 옵션 기능 추가
                tag.checkServerTimeDup = CheckServerTimeDup;
                if (!tag.UpdateFormat(dataPosition, datePosition, timePosition, out string errMessage))
                {
                    return false;
                }
                DicTAGList[tagKey].Add(tag);
            }
            else
            {
                TTVTAG2 tag = new TTVTAG2(tagName, equipName, msgTypeNo, piTagName, opcItemName, efAttributeName, lastMeasuredValue, lastMeasuredDateTime);
                DicTAGList[tagKey].Add(tag);
            }

            return true;
        }
        public bool Add(DataTable dtTag)
        {
            foreach (DataRow dr in dtTag.Rows)
            {
                DateTime.TryParse(dr["LAST_MEASURED_DATETIME"].ToString().Trim(), out DateTime dt);
                Add(dr["TAG_NM"].ToString().Trim(), dr["EQUIP_NM"].ToString().Trim(), dr["MSG_TYPE"].ToString().Trim(), dr["DATA_POSITION"].ToString().Trim(), dr["DATE_POSITION"].ToString().Trim(), dr["TIME_POSITION"].ToString().Trim(), dr["PI_TAG_NM"].ToString().Trim(), dr["OPCITEM_NM"].ToString().Trim(), dr["EF_ATTRIBUTE_NM"].ToString().Trim(), dr["LAST_UPDATED_VALUE"].ToString().Trim(), dt.ToString("yyyy-MM-dd HH:mm:ss.fff"));
            }
            return true;
        }
        public bool DataProcess(QueueMsg msg, DataGridView dataGridView, DataGridView dataGridView_EventFrame, DataGridView dataGridView_Attributes)
        {
            if (msg == null) return false;
            else return DataProcess(msg.m_EqName, msg.m_MsgType, msg.m_Data.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.None), dataGridView, dataGridView_EventFrame, dataGridView_Attributes);
        }

        public bool DataProcess(string equipName, int msgType, string[] data, DataGridView dataGridView, DataGridView dataGridView_EventFrame, DataGridView dataGridView_Attributes)
        {
            if (!DicTAGList.TryGetValue($"{equipName}_{msgType}", out List<TAG2> listTAG))
            {
                return false;
            }
            dtServerTime = DateTime.Now;
            foreach (TAG2 tag in listTAG)
            {
                tag.ServerTime = dtServerTime;
                if (!tag.DataProcess(data, out string errMessage))
                {
                }
            }

            List<TAG2> listUpdated = listTAG.FindAll(x => x.IsValueUpdated);
            // 저장할 데이터가 있는 경우에만
            if (listUpdated.Count > 0)
            {
                DateTime dtNow = DateTime.Now;
                listUpdated.ForEach(tag =>
                {
                    tag.PIIFDateTime = dtNow;
                    tag.Remark = string.Empty;
                    tag.PIIFFlag = "N"; // N -> Y, E -> F, Z
                    tag.IsDBInserted = false;
                }); ;

                // DataGridView에 업데이트된 태그 값을 설정
                foreach (var tag in listUpdated)
                {
                    foreach (DataGridViewRow row in dataGridView.Rows)
                    {
                        if (row.Cells["TAG Name"].Value.ToString() == tag.TagName) // 태그 이름 비교
                        {
                            row.Cells["Last Value"].Value = tag.Value; // 태그 값 업데이트
                            row.Cells["Measure DateTime"].Value = tag.TimeStamp; // PIIF 시간 업데이트
                            row.Cells["Update DateTime"].Value = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                            break; // 일치하는 태그가 업데이트되면 루프 종료
                        }
                    }
                }
            }

            /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            // EventFrame
            /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

            List<TAG2> listUpdatedEF = listUpdated.FindAll(x => !string.IsNullOrWhiteSpace(x.EFAttributeName));
            if (listUpdatedEF.Count > 0)
            {
                string serverTime = dtServerTime.ToString("yyyy-MM-dd HH:mm:ss.fff");
                string eventFrameName = $"{equipName}_{dtServerTime:yyyyMMddHHmmssfff}";
                string measureTime = listUpdatedEF[0].dtTimeStamp.ToString("yyyy-MM-dd HH:mm:ss.fff");

                EventFrameData efData = new EventFrameData()
                {
                    Name = eventFrameName,
                    StartTime = measureTime,
                    EndTime = measureTime,
                    EquipmentName = equipName,
                    MessageType = msgType,
                    ServerTime = serverTime,
                    TemplateName = $"{equipTypeName}_{msgType:00}"
                };

                ///
                // 값이 처리된 태그와 EFAttributeName 이 설정된 태그만 처리
                listUpdatedEF.ForEach(tag => efData.Attributes.Add(new EventFrameAttributeData() { Name = tag.EFAttributeName, Value = tag.Value }));

                foreach (DataGridViewRow row in dataGridView_EventFrame.Rows)
                {
                    row.Cells["Measure DateTime"].Value = measureTime;
                    row.Cells["EventFrame Name"].Value = eventFrameName;
                    row.Cells["Server DateTime"].Value = serverTime;

                    string attributesJson = JsonSerializer.Serialize(efData.Attributes);
                    row.Cells["Attributes"].Value = attributesJson;

                    row.Cells["Update DateTime"].Value = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

                    string equipmentType = row.Cells["Equipment Type"].Value?.ToString().Trim();
                    row.Cells["EventFrame Template"].Value = !string.IsNullOrEmpty(equipmentType)
                        ? $"{equipmentType}_{msgType:00}" 
                        : $"{msgType:00}"; 

                    break;
                }

                foreach (var tag in listUpdatedEF)
                {
                    foreach (DataGridViewRow row in dataGridView_Attributes.Rows)
                    {
                        if (row.Cells["AttributeName"].Value.ToString() == tag.EFAttributeName)
                        {
                            row.Cells["AttributeValue"].Value = tag.Value;
                            break;
                        }
                    }
                }
            }
            return true;
        }
    }
}
