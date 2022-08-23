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
using OpcUaClient;
using Opc.Ua;
using Newtonsoft.Json.Linq;
using System.Globalization;
using System.Text.Json;
namespace DataSpider.PC01.PT
{
    /// <summary>
    /// Equip_Type : SOLOVPE OPC UA Server
    /// </summary>
    public class PC01S14 : PC00B01
    {
        OpcUaClient.OpcUaClient myUaClient=null;
        private DateTime dtNormalTime = DateTime.Now;
        private int m_LastEnqueuedDaqID = 0;
        private int m_ToBeProcessedDaqID = 0;
        private DateTime m_LastEnqueuedDate = DateTime.Now;
        private DateTime m_ToBeProcessedDate = DateTime.Now;
        //private SoloVpe m_soloVpe = new SoloVpe();

        private DateTime dtDateTime;
        private string dataString = string.Empty;
        private string typeName = "MSR";
        private StringBuilder ssb = new StringBuilder();
        private string Uid;
        private string Pwd;


        public class SoloVpeTable
        {
            // SoloeVpe Data Table
            public DataTable sdt = null;
            // Raw Data Table (SeriesData)
            public DataTable rdt = null;

            public FileLog fileLog = null;
            public int newDaqID;
            public string newSampleName;
            public string newLDAPUserID;
            public string newRunStart;
            public string newRunEnd;

            public void SetFileLog(FileLog _filelog)
            {
                fileLog = _filelog;
            }
            /// <summary>
            /// jsonString에서 currentDaqID 보다 큰 가장 작은 DaqID를 찾는다.
            /// 만일 currentDaqID 보다 큰 가장 작은 DaqID를 찾지 못하면 newDaqID는 int.MaxValue가 된다.
            /// 이 때는 newDaqID를 currentDaqID로 설정한다.
            /// </summary>
            /// <param name="jsonString"></param>
            /// <param name="currentDaqID"></param>
            public void GetDaqID(string jsonString, int currentDaqID)
            {
                JsonDocument document;
                JsonElement root;
                int DaqID;

                newDaqID = currentDaqID;
                newSampleName = string.Empty;
                newLDAPUserID = string.Empty;
                newRunStart = string.Empty;
                newRunEnd = string.Empty;

                document = JsonDocument.Parse(jsonString);
                root = document.RootElement;
                if (root.ValueKind != JsonValueKind.Array || root.GetArrayLength() == 0) return;

                newDaqID = int.MaxValue;
                foreach (JsonElement jElement in root.EnumerateArray())
                {
                    DaqID = jElement.GetProperty("ID").GetInt32();

                    if (DaqID > currentDaqID && DaqID < newDaqID)
                    {
                        if (jElement.GetProperty("RunEnd").GetString() != null)
                        {
                            newDaqID = DaqID;
                            newSampleName = jElement.GetProperty("SampleName").GetString();
                            newLDAPUserID = jElement.GetProperty("LDAPUserID").GetString();
                            newRunStart = jElement.GetProperty("RunStart").GetString();
                            newRunEnd = jElement.GetProperty("RunEnd").GetString();
                        }
                    }
                }
                if (newDaqID == int.MaxValue) newDaqID = currentDaqID;
            }

            public int GetArrayLength(string jsonString)
            {
                //JArray jArray = JArray.Parse(jsonString);
                //return jArray.Count;
                JsonDocument document = JsonDocument.Parse(jsonString);
                return document.RootElement.GetArrayLength();
            }

            public string MakeDataTable(string jsonString)
            {
                string retValue = string.Empty;
                JsonDocument document;
                JsonElement root;
                DataColumn sdt_column, rdt_column;
                try
                {
                    document = JsonDocument.Parse(jsonString);
                    root = document.RootElement;
                    if (root.ValueKind != JsonValueKind.Array)
                    {
                        sdt = rdt = null;
                        return retValue;
                    }
                    foreach (JsonElement je in root.EnumerateArray())
                    {
                        foreach (JsonProperty jp in je.EnumerateObject())
                        {
                            if (jp.Name == "SeriesData")
                            {
                                JsonElement.ArrayEnumerator ja = jp.Value.EnumerateArray();
                                ja.MoveNext();
                                JsonElement jje = ja.Current;

                                // 2022-08-22 kwc Repeat별로 데이터리하는 문제를 위해 추가
                                rdt_column = new DataColumn();
                                rdt_column.DataType = Type.GetType("System.String");
                                rdt_column.ColumnName = "IDID";
                                rdt.Columns.Add(rdt_column);

                                foreach (JsonProperty jjp in jje.EnumerateObject())
                                {
                                    rdt_column = new DataColumn();
                                    rdt_column.DataType = Type.GetType("System.String");
                                    rdt_column.ColumnName = jjp.Name;
                                    rdt.Columns.Add(rdt_column);
                                }
                            }
                            else
                            {
                                sdt_column = new DataColumn();
                                sdt_column.DataType = Type.GetType("System.String");
                                sdt_column.ColumnName = jp.Name;
                                sdt.Columns.Add(sdt_column);
                            }
                        }
                        return retValue;
                    }
                    return retValue;
                }
                catch (Exception ex)
                {
                    fileLog.WriteLog($"Exception in MakeDataTable : {ex}");
                }
                return retValue;
            }

            public string FillDataTable(string jsonString)
            {
                string retValue = string.Empty;
                JsonDocument document;
                JsonElement root;
                DataRow sdt_row, rdt_row;
                // 2022-08-22 kwc Repeat별로 데이터리하는 문제를 위해 추가
                JsonProperty idid = new JsonProperty();

                try
                {
                    sdt.Clear();
                    rdt.Clear();
                    document = JsonDocument.Parse(jsonString);
                    root = document.RootElement;
                    if (root.ValueKind != JsonValueKind.Array) return retValue;
                    foreach (JsonElement je in root.EnumerateArray())
                    {
                        sdt_row = sdt.NewRow();
                        foreach (JsonProperty jp in je.EnumerateObject())
                        {

                            if (jp.Name == "SeriesData")
                            {
                                foreach (JsonElement jje in jp.Value.EnumerateArray())
                                {
                                    rdt_row = rdt.NewRow();
                                    // 2022-08-22 kwc Repeat별로 데이터리하는 문제를 위해 추가
                                    rdt_row["IDID"] = idid.Value;
                                    foreach (JsonProperty jjp in jje.EnumerateObject())
                                    {
                                        rdt_row[jjp.Name] = jjp.Value;
                                    }
                                    rdt.Rows.Add(rdt_row);
                                }
                            }
                            else
                            {
                                sdt_row[jp.Name] = jp.Value;
                                // 2022-08-22 kwc Repeat별로 데이터리하는 문제를 위해 추가
                                if (jp.Name == "ID") idid = jp;
                            }
                        }
                        sdt.Rows.Add(sdt_row);
                    }
                }
                catch (Exception ex)
                {
                    fileLog.WriteLog($"Exception in FillDataTable : {ex}");
                }
                return retValue;
            }

            public int GetWaveLengthCount()
            {
                DataTable distinctTable = sdt.DefaultView.ToTable(true, "WaveLength");
                return distinctTable.Rows.Count;
            }

            public int GetRepeatCount()
            {
                int nRepeatCount = sdt.Rows.Count / GetWaveLengthCount();
                return nRepeatCount;
            }

            public string GetWaveLength(int nWaveLength)
            {
                DataTable distinctTable = sdt.DefaultView.ToTable(true, "WaveLength");
                return (string)distinctTable.Rows[nWaveLength]["WaveLength"];
            }

            public string GetConfigWaveLengths()
            {
                string retString = string.Empty;
                try
                {
                    DataTable distinctTable = sdt.DefaultView.ToTable(true, "WaveLength", "ExtinctionCoefficient", "DataPoints");
                    foreach (DataRow dr in distinctTable.Rows)
                    {
                        retString += " " + dr["WaveLength"] + " ,";
                    }
                    if (retString.Length > 0)
                        retString = retString.Substring(0, retString.Length - 1);
                }
                catch (Exception ex)
                {
                    fileLog.WriteLog($"Exception in GetConfigWaveLengths : {ex}");
                }
                return retString.Trim();
            }

            public string GetConfigDataPoints()
            {
                string retString = string.Empty;
                try
                {
                    DataTable distinctTable = sdt.DefaultView.ToTable(true, "WaveLength", "ExtinctionCoefficient", "DataPoints");
                    if (distinctTable.Rows.Count > 0)
                        retString = (string)sdt.Rows[0]["Datapoints"];
                }
                catch (Exception ex)
                {
                    fileLog.WriteLog($"Exception in GetConfigDataPoints : {ex}");
                }
                return retString.Trim();
            }

            public string GetConfigECs()
            {
                string retString = string.Empty;
                try
                {
                    DataTable distinctTable = sdt.DefaultView.ToTable(true, "WaveLength", "ExtinctionCoefficient", "DataPoints");
                    foreach (DataRow dr in distinctTable.Rows)
                    {
                        retString += " " + dr["ExtinctionCoefficient"] + " ,";
                    }
                    if (retString.Length > 0)
                        retString = retString.Substring(0, retString.Length - 1);
                }
                catch (Exception ex)
                {
                    fileLog.WriteLog($"Exception in GetConfigECs : {ex}");
                }
                return retString.Trim();
            }

            public string GetSeachPathLengths()
            {
                string retString = string.Empty;
                try
                {
                    for (int nRow = 0; nRow < 3 && nRow < rdt.Rows.Count; nRow++)
                    {
                        retString += " " + rdt.Rows[nRow]["Pathlength"] + " ,";
                    }
                    if (retString.Length > 0)
                        retString = retString.Substring(0, retString.Length - 1);
                }
                catch (Exception ex)
                {
                    fileLog.WriteLog($"Exception in GetSeachPathLengths : {ex}");
                }
                return retString.Trim();
            }
            // 통계는 WaveLength순으로
            public string GetStatValue(int nWaveLength, string PropertyName)
            {
                string retString = string.Empty;
                DataRow[] rows = sdt.Select($"WaveLength ='{GetWaveLength(nWaveLength)}'", "ID DESC");
                retString = (string)rows[0][PropertyName];
                return retString.Trim();
            }

            public string GetRepeatValue(int nWaveLength, int nRepeat, string PropertyName)
            {
                string retString = string.Empty;
                DataRow[] rows = sdt.Select($"", "ID ASC");
                retString = (string)rows[(nWaveLength) * GetRepeatCount() + nRepeat][PropertyName];
                return retString.Trim();
            }
            //public string GetRawValue(int nRepeat, string PropertyName)
            //{
            //    string retString = string.Empty;
            //    int nSeriesData = rdt.Rows.Count / GetRepeatCount();

            //    DataRow[] rows = rdt.Select($"", "ID ASC");
            //    for (int nData = 0; nData < nSeriesData; nData++)
            //    {
            //        retString += " " + (string)rows[nRepeat * nSeriesData + (nSeriesData-nData-1)][PropertyName] + " ;";
            //    }
            //    if (retString.Length > 0)
            //        retString = retString.Substring(0, retString.Length - 1);
            //    return retString.Trim();
            //}

            // 2022-08-22 kwc Repeat별로 데이터리하는 문제를 위해 추가
            //public string GetIDID(int nRepeat)
            //{
            //    string retString = string.Empty;
            //    DataTable distinctTable = rdt.DefaultView.ToTable(true, "IDID");
            //    retString = (string)distinctTable.Rows[nRepeat]["IDID"];
            //    return retString;
            //}

            // 2022-08-23 kwc Repeat, WaveLength를 고려해서 변경
            public string GetIDID(int nRepeat, string WaveLength)
            {
                string retString = string.Empty;
                DataTable distinctTable = rdt.DefaultView.ToTable(true, "IDID", "Wavelength");
                // 20220823, SHS, REPEAT 데이터 소팅이 안된 상태로 처리되어 IDID ASC 추가
                DataRow[] rows = distinctTable.Select($"Wavelength='{WaveLength}'", "IDID ASC");
                retString = (string)rows[nRepeat]["IDID"];

                //DataRow[] rows = sdt.Select($"Wavelength='{WaveLength}'", "ID ASC");
                //if( rows.Length > 0 )
                //    retString = rows[0]["ID"].ToString();

                return retString;
            }


            // 2022-08-22 kwc Repeat별로 데이터리하는 문제를 위해 수정
            // 2022-08-23 kwc Repeat, WaveLength를 고려해서 변경
            public string GetRawValue(int nRepeat, string PropertyName)
            {
                string retString = string.Empty;
                //DataRow[] rows = rdt.Select($"IDID='{GetIDID(nRepeat)}'", "ID ASC");
                string selectString = string.Empty;
                for (int nWaveLength = 0; nWaveLength < GetWaveLengthCount(); nWaveLength++)
                {
                    selectString += $" IDID='{GetIDID(nRepeat, GetWaveLength(nWaveLength))}' OR";
                }
                if (selectString.Length > 2) selectString = selectString.Substring(0, selectString.Length - 2);
                DataRow[] rows = rdt.Select(selectString, "ID ASC");
                for (int nData = 0; nData < rows.Length; nData++)
                {
                    retString += " " + (string)rows[rows.Length - nData - 1][PropertyName] + " ;";
                }
                if (retString.Length > 0)
                    retString = retString.Substring(0, retString.Length - 1);
                return retString.Trim();
            }

            public string GetSdtValue(int nWaveLength, string PropertyName)
            {
                string retString = string.Empty;
                retString = (string)sdt.Rows[nWaveLength][PropertyName];
                return retString.Trim();
            }

        }
        private SoloVpeTable m_soloVpeTable = new SoloVpeTable();
        private string prevOutputArguments = string.Empty;

        public PC01S14() : base()
        {
        }

        public PC01S14(PC01F01 pOwner, string equipType, string equipName, string connectionInfo, string extraInfo, int nCurNo, bool bAutoRun = false) : base(pOwner, equipType, equipName, connectionInfo, extraInfo, nCurNo, bAutoRun)
        {
        }

        public PC01S14(PC01F01 pOwner, DataRow dr, int nCurNo, bool bAutoRun = false) : this(pOwner, dr["EQUIP_TYPE_NM"].ToString(), dr["EQUIP_NM"].ToString(), dr["CONNECTION_INFO"].ToString(), dr["EXTRA_INFO"].ToString(), nCurNo, bAutoRun)
        {
            try
            {

                drEquipment = dr;

                if (m_AutoRun == true)
                {
                    m_Thd = new Thread(ThreadJob);

                    m_Thd.Start();
                }
            }
            catch(Exception ex)
            {
                listViewMsg.UpdateMsg($"Exceptioin - PC01S14 ({ex})", false, true, true, PC00D01.MSGTERR);
            }
        }

        private void ThreadJob()
        {

            listViewMsg.UpdateStatus(true);
            listViewMsg.UpdateMsg("Thread started");

            m_LastEnqueuedDate = GetLastEnqueuedDate();
            listViewMsg.UpdateMsg($"Read m_LastEnqueuedDate :{m_LastEnqueuedDate.ToString("yyyyMMdd")}", false, true, true, PC00D01.MSGTINF);
            m_LastEnqueuedDaqID = GetLastEnqueuedDaqID();
            listViewMsg.UpdateMsg($"Read m_LastEnqueuedDaqID :{m_LastEnqueuedDaqID}", false, true, true, PC00D01.MSGTINF);

            string strErrCode = string.Empty, strErrText = string.Empty;
            DataTable dtConfig = null;
            m_soloVpeTable.SetFileLog(fileLog);
            while (!bTerminal)
            {
                try
                {
                    if (myUaClient == null)
                    {
                        UpdateEquipmentProgDateTime(IF_STATUS.Disconnected);
                        listViewMsg.UpdateMsg($"OPC UA Not Connected. Try to connect.", false, true, true, PC00D01.MSGTERR);
                        for (int i = 0; i < 5; i++)
                        {
                            Thread.Sleep(1000);
                            if (bTerminal)
                            {
                                break;
                            }
                        }
                        if (!bTerminal)
                        {
                            InitOpcUaClient();

                            //m_soloVpe.ReadCfgData(drEquipment);
                            dtNormalTime = DateTime.Now;
                        }
                    }
                    else
                    {
                        if (myUaClient.m_reconnectHandler != null)
                        {
                            UpdateEquipmentProgDateTime(IF_STATUS.Disconnected);
                            if ((DateTime.Now - dtNormalTime).TotalHours >= 1)
                            {
                                myUaClient = null;
                                listViewMsg.UpdateMsg($" Network Error Time >= 1 Hr, Ua Client Reset ", false, true, true, PC00D01.MSGTERR);
                            }
                        }
                        else
                        {
                            if (ProcessMethodDaqStartInfo(m_LastEnqueuedDate, m_LastEnqueuedDaqID, out m_ToBeProcessedDate, out m_ToBeProcessedDaqID))
                            {
                                // 처리할 내용이 있으면 
                                if (m_LastEnqueuedDaqID < m_ToBeProcessedDaqID)
                                {
                                    if (ProcessMethodDaqID(m_ToBeProcessedDaqID))
                                    {
                                        m_LastEnqueuedDaqID = m_ToBeProcessedDaqID;
                                        SetLastEnqueuedDaqID(m_LastEnqueuedDaqID);
                                    }
                                }

                                if (m_LastEnqueuedDate != m_ToBeProcessedDate)
                                {
                                    m_LastEnqueuedDate = m_ToBeProcessedDate;
                                    SetLastEnqueuedDate(m_LastEnqueuedDate);
                                }

                                UpdateEquipmentProgDateTime(IF_STATUS.Normal);
                                dtNormalTime = DateTime.Now;
                            }
                            // 20220818, SHS, ProcessMethodDaqStartInfo (OPC Call) return false (opc call exception 또는 데이터가 없을때) 일때 하는 로직 삭제 
                            //else
                            //{  // 메소드 호출/응답 이상일 때
                            //    if ((DateTime.Now - dtNormalTime).TotalHours < 1)
                            //    {
                            //        //UpdateEquipmentProgDateTime(IF_STATUS.InvalidData);
                            //          UpdateEquipmentProgDateTime(IF_STATUS.NetworkError);
                            //    }
                            //    else
                            //    {
                            //        myUaClient = null;
                            //        listViewMsg.UpdateMsg($" Network Error Time > 1 Hr, Ua Client Reset ", false, true, true, PC00D01.MSGTERR);
                            //    }
                            //}
                            Thread.Sleep(1 * 1000); // 1회 처리 후 10초가 휴식
                        }
                    }
                }
                catch (Exception ex)
                {
                    UpdateEquipmentProgDateTime(IF_STATUS.InternalError);
                    listViewMsg.UpdateMsg($"Exception in ThreadJob - ({ex})", false, true, true, PC00D01.MSGTERR);
                }
                finally
                {
                }
                Thread.Sleep(1000);
            }
            UpdateEquipmentProgDateTime(IF_STATUS.Stop);
            listViewMsg.UpdateStatus(false);
            listViewMsg.UpdateMsg("Thread finished");
        }

        /// <summary>
        /// DaqStartInfo 메소드를 호출하여 startDate 날짜의 DaqID 목록을 읽는다.
        /// LastEnqueuedDaqID 와 비교해서 처리대상 DaqID를 찾는다.
        /// 검색대상이 없고 날짜가 오늘이 아니면 날짜를 증가 시킨다.
        /// </summary>
        /// <param name="startDate"></param>
        /// <param name="LastEnqueuedDaqID"></param>
        /// <param name="ToBeProcessedDate"></param>
        /// <param name="ToBeProcessedDaqID"></param>
        /// <returns> false : Method 호출 실패 또는 응답이 이상 </returns>
        /// <returns>true : Method 호출/응답 정상일 때</returns>
        private bool ProcessMethodDaqStartInfo(DateTime startDate, int LastEnqueuedDaqID, out DateTime newDate, out int newDaqID)
        {
            listViewMsg.UpdateMsg($"ProcessMethodDaqStartInfo - Call Daq/GetDaqStartInfo, Date : {startDate:MM/dd/yyyy}, LastEnqueuedDaqID : {LastEnqueuedDaqID}", false, true, true, PC00D01.MSGTINF);

            DateTime endDate = startDate;// + new TimeSpan(1, 0, 0, 0);
            DateTime toDay= new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day);

            newDate = startDate;
            newDaqID = LastEnqueuedDaqID;

            IList<object> outputArguments = null;
            try
            {
                outputArguments = myUaClient.m_session.Call(new NodeId("ns=2;s=Daq"),
                                                                        new NodeId("ns=2;s=Daq/GetDaqStartInfo"),
                                                                        startDate.ToString("MM/dd/yyyy"),
                                                                        endDate.ToString("MM/dd/yyyy")
                                                                        );
            }
            catch (Opc.Ua.ServiceResultException ex)
            {
                listViewMsg.UpdateMsg($"Exception in OPC Call - {ex}", false, true, true, PC00D01.MSGTERR);
                // 20220818, SHS, OPC Call Exception 시 OPC 접속종료 처리
                listViewMsg.UpdateMsg($"OPC Client null", false, true, true, PC00D01.MSGTINF);
                myUaClient = null;
                return false;
            }
            if (outputArguments != null && outputArguments.Count >= 1)
            {

                // 20220818, SHS, OPC Call 수신 데이터 로그
                if (!outputArguments[0].ToString().Equals(prevOutputArguments))
                {
                    fileLog.WriteData(outputArguments[0].ToString(), "Daq/GetDaqStartInfo call", "GetDaqStartInfo");
                    prevOutputArguments = outputArguments[0].ToString();
                }
                else
                {
                    listViewMsg.UpdateMsg($"There is no DAQ added.", false, true, true, PC00D01.MSGTINF);
                }
                // JSON 데이터를 분석하여 LastEnqueuedDaqID보다 큰 가장 작은 DaqID 를 찾는다.
                //m_soloVpe.GetDaqID(outputArguments[0].ToString(), LastEnqueuedDaqID);
                m_soloVpeTable.GetDaqID(outputArguments[0].ToString(), LastEnqueuedDaqID);
                //newDaqID = m_soloVpe.newDaqID;
                newDaqID = m_soloVpeTable.newDaqID;

                // LastEnqueuedDaqID보다 보다 큰 DaqID가 없다면
                //          날짜가 오늘 이전이면 날짜를 변경해야 함.
                if (newDaqID == LastEnqueuedDaqID)
                {
                    if (startDate < toDay)
                    {
                        newDate = endDate +new TimeSpan(1, 0, 0, 0);
                    }
                }

                listViewMsg.UpdateMsg($"Daq/GetDaqStartInfo call - newDaqID : {newDaqID}, newDate : {newDate}", false, true, true, PC00D01.MSGTINF);

                return true;
            }
            else
            {
                listViewMsg.UpdateMsg($"Daq/GetDaqStartInfo call - Result : null or count 0", false, true, true, PC00D01.MSGTINF);
            }
            return false;
        }

        private bool ProcessMethodDaqID(int ToBeProcessedDaqID)
        {
            IList<object> outputArguments = null;
            try
            {
                listViewMsg.UpdateMsg($"ProcessMethodDaqID - Call Daq/GetCycleData, ToBeProcessedDaqID : {ToBeProcessedDaqID}", false, true, true, PC00D01.MSGTINF);
                //
                outputArguments = myUaClient.m_session.Call(new NodeId("ns=2;s=Daq"),
                                                                        new NodeId("ns=2;s=Daq/GetCycleData"),
                                                                        ToBeProcessedDaqID
                                                                        );

                // 20220818, SHS, 수신데이터 로그 추가, null, count < 1, [] 형태로 수신되는 경우 스킵처리 추가
                if (outputArguments == null || outputArguments.Count < 1)
                {
                    listViewMsg.UpdateMsg($"Call Daq/GetCycleData - ToBeProcessedDaqID : {ToBeProcessedDaqID}, Result : null or count 0 - Processing Skip!", false, true, true, PC00D01.MSGTINF);
                    return true;
                }
                else
                {
                    listViewMsg.UpdateMsg($"Call Daq/GetCycleData - ToBeProcessedDaqID : {ToBeProcessedDaqID}", false, true, true, PC00D01.MSGTINF);
                    fileLog.WriteData(outputArguments[0].ToString(), $"Call Daq/GetCycleData - ToBeProcessedDaqID : {ToBeProcessedDaqID}", "GetCycleData");
                    if (outputArguments[0].ToString().Length < 10)
                    {
                        listViewMsg.UpdateMsg($"Invalid Result Data (Data length < 10) - DaqID:{ToBeProcessedDaqID} - Processing Skip!", false, true, true, PC00D01.MSGTINF);
                        return true;
                    }
                }
            }
            catch (Opc.Ua.ServiceResultException ex)
            {
                listViewMsg.UpdateMsg($"Exception in ProcessMethodDaqID OPC Call - {ex}", false, true, true, PC00D01.MSGTERR);
                // josn 데이터 사이즈 초과 시
                if (ex.Message.Contains("Could not encode outgoing message"))
                {
                    listViewMsg.UpdateMsg($"Exception in OPC Call - DaqID:{ToBeProcessedDaqID} - Processing Skip!", false, true, true, PC00D01.MSGTERR);
                    return true;
                }
                // 20220818, SHS, OPC Call Exception 시 OPC 접속종료 처리
                listViewMsg.UpdateMsg($"OPC Client null", false, true, true, PC00D01.MSGTINF);
                myUaClient = null;
                return false;
            }
            if (outputArguments != null && outputArguments.Count >= 1)
            {
                try
                {
                    //PC00U01.TryParseExact(m_soloVpe.newRunStart, out dtDateTime);
                    PC00U01.TryParseExact(m_soloVpeTable.newRunStart, out dtDateTime);
                    dtDateTime = dtDateTime.ToLocalTime();
                    ssb.Clear();
                    ssb.AppendLine($"SVRTIME, {dtDateTime:yyyy-MM-dd HH:mm:ss.fff}, {DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}");

                    //ssb.AppendLine($"{typeName}_DAQID, {dtDateTime:yyyy-MM-dd HH:mm:ss.fff}, {m_soloVpe.newDaqID}");
                    //ssb.AppendLine($"{typeName}_SAMPLENAME, {dtDateTime:yyyy-MM-dd HH:mm:ss.fff}, {m_soloVpe.newSampleName}");
                    //ssb.AppendLine($"{typeName}_USERID, {dtDateTime:yyyy-MM-dd HH:mm:ss.fff}, {m_soloVpe.newLDAPUserID}");
                    //ssb.AppendLine($"{typeName}_RUNSTART, {dtDateTime:yyyy-MM-dd HH:mm:ss.fff}, {m_soloVpe.newRunStart}");
                    //ssb.AppendLine($"{typeName}_RUNEND, {dtDateTime:yyyy-MM-dd HH:mm:ss.fff}, {m_soloVpe.newRunEnd}");

                    ssb.AppendLine($"{typeName}_DAQID, {dtDateTime:yyyy-MM-dd HH:mm:ss.fff}, {m_soloVpeTable.newDaqID}");
                    ssb.AppendLine($"{typeName}_SAMPLENAME, {dtDateTime:yyyy-MM-dd HH:mm:ss.fff}, {m_soloVpeTable.newSampleName}");
                    ssb.AppendLine($"{typeName}_USERID, {dtDateTime:yyyy-MM-dd HH:mm:ss.fff}, {m_soloVpeTable.newLDAPUserID}");
                    ssb.AppendLine($"{typeName}_RUNSTART, {dtDateTime:yyyy-MM-dd HH:mm:ss.fff}, {m_soloVpeTable.newRunStart}");
                    ssb.AppendLine($"{typeName}_RUNEND, {dtDateTime:yyyy-MM-dd HH:mm:ss.fff}, {m_soloVpeTable.newRunEnd}");

                    // 20220818, SHS, table 은 무조건 생성하는것으로 수정
                    //if (m_soloVpeTable.sdt == null)
                    {
                        m_soloVpeTable.sdt = new DataTable();
                        m_soloVpeTable.rdt = new DataTable();
                        m_soloVpeTable.MakeDataTable(outputArguments[0].ToString());
                    }

                    if (m_soloVpeTable.sdt != null)
                    {
                        // Json Data를 DataTable에 저장
                        m_soloVpeTable.FillDataTable(outputArguments[0].ToString());

                        // Configuration
                        ssb.AppendLine($"{typeName}_CONF_WAVELENGTH, {dtDateTime:yyyy-MM-dd HH:mm:ss.fff}, {m_soloVpeTable.GetConfigWaveLengths()}");
                        ssb.AppendLine($"{typeName}_CONF_DATA_POINTS, {dtDateTime:yyyy-MM-dd HH:mm:ss.fff}, {m_soloVpeTable.GetConfigDataPoints()}");
                        ssb.AppendLine($"{typeName}_CONF_SEARCH_PATHLENGTHS, {dtDateTime:yyyy-MM-dd HH:mm:ss.fff}, {m_soloVpeTable.GetSeachPathLengths()}");
                        ssb.AppendLine($"{typeName}_CONF_EXTINCTION_COEFFICIENT, {dtDateTime:yyyy-MM-dd HH:mm:ss.fff}, {m_soloVpeTable.GetConfigECs()}");

                        // Stat
                        for (int nWaveLength = 0; nWaveLength < m_soloVpeTable.GetWaveLengthCount(); nWaveLength++)
                        {
                            ssb.AppendLine($"{typeName}_STAT_WAVELENGTH_{nWaveLength + 1}, {dtDateTime:yyyy-MM-dd HH:mm:ss.fff},  {m_soloVpeTable.GetStatValue(nWaveLength, "Wavelength")}");
                            ssb.AppendLine($"{typeName}_STAT_AVG_SLOPE_{nWaveLength + 1}, {dtDateTime:yyyy-MM-dd HH:mm:ss.fff}, {m_soloVpeTable.GetStatValue(nWaveLength, "MovingAverageConcentration")}");
                            ssb.AppendLine($"{typeName}_STAT_STD_DEV_SLOPE_{nWaveLength + 1} , {dtDateTime:yyyy-MM-dd HH:mm:ss.fff},  {m_soloVpeTable.GetStatValue(nWaveLength, "StdDevConcentration")}");
                            ssb.AppendLine($"{typeName}_STAT_MAX_SLOPE_{nWaveLength + 1} , {dtDateTime:yyyy-MM-dd HH:mm:ss.fff},  {m_soloVpeTable.GetStatValue(nWaveLength, "MaximumConcentration")}");
                            ssb.AppendLine($"{typeName}_STAT_MIN_SLOPE_{nWaveLength + 1}, {dtDateTime:yyyy-MM-dd HH:mm:ss.fff},  {m_soloVpeTable.GetStatValue(nWaveLength, "MinimumConcentration")}");
                            ssb.AppendLine($"{typeName}_STAT_RANGE_SLOPE_{nWaveLength + 1}, {dtDateTime:yyyy-MM-dd HH:mm:ss.fff},  {m_soloVpeTable.GetStatValue(nWaveLength, "RangeConcentration")}");
                            ssb.AppendLine($"{typeName}_STAT_SLOPE_PERCENT_REL_DIFF_{nWaveLength + 1} , {dtDateTime:yyyy-MM-dd HH:mm:ss.fff},  {m_soloVpeTable.GetStatValue(nWaveLength, "PercentRelDiffConcentration")}");
                            ssb.AppendLine($"{typeName}_STAT_SLOPE_PERCENT_RSD_{nWaveLength + 1} , {dtDateTime:yyyy-MM-dd HH:mm:ss.fff},  {m_soloVpeTable.GetStatValue(nWaveLength, "PercentRSDConcentration")}");
                            ssb.AppendLine($"{typeName}_STAT_EXTINCTION_COEFFICIENT_{nWaveLength + 1}, {dtDateTime:yyyy-MM-dd HH:mm:ss.fff},  {m_soloVpeTable.GetStatValue(nWaveLength, "ExtinctionCoefficient")}");
                        }
                        // Repeat
                        for (int nRepeat = 0; nRepeat < m_soloVpeTable.GetRepeatCount(); nRepeat++)
                        {
                            for (int nWaveLength = 0; nWaveLength < m_soloVpeTable.GetWaveLengthCount(); nWaveLength++)
                            {
                                ssb.AppendLine($"{typeName}_WAVELENGTH_{nWaveLength * m_soloVpeTable.GetRepeatCount() + nRepeat + 1} , {dtDateTime:yyyy-MM-dd HH:mm:ss.fff},  {m_soloVpeTable.GetRepeatValue(nWaveLength, nRepeat, "Wavelength")}");
                                ssb.AppendLine($"{typeName}_CONC_{nWaveLength * m_soloVpeTable.GetRepeatCount() + nRepeat + 1} , {dtDateTime:yyyy-MM-dd HH:mm:ss.fff},  {m_soloVpeTable.GetRepeatValue(nWaveLength, nRepeat, "BestConcentration")}");
                                ssb.AppendLine($"{typeName}_SLOPE_{nWaveLength * m_soloVpeTable.GetRepeatCount() + nRepeat + 1} , {dtDateTime:yyyy-MM-dd HH:mm:ss.fff},  {m_soloVpeTable.GetRepeatValue(nWaveLength, nRepeat, "BestSlope")}");
                                ssb.AppendLine($"{typeName}_R2_{nWaveLength * m_soloVpeTable.GetRepeatCount() + nRepeat + 1} , {dtDateTime:yyyy-MM-dd HH:mm:ss.fff}, {m_soloVpeTable.GetRepeatValue(nWaveLength, nRepeat, "BestRSquared")}");
                                ssb.AppendLine($"{typeName}_THRESHOLD_PATHLENGTH_{nWaveLength * m_soloVpeTable.GetRepeatCount() + nRepeat + 1} , {dtDateTime:yyyy-MM-dd HH:mm:ss.fff},  {m_soloVpeTable.GetRepeatValue(nWaveLength, nRepeat, "ThresholdPL")}");
                                ssb.AppendLine($"{typeName}_PATHLENGTH_STEP_{nWaveLength * m_soloVpeTable.GetRepeatCount() + nRepeat + 1} , {dtDateTime:yyyy-MM-dd HH:mm:ss.fff},  {m_soloVpeTable.GetRepeatValue(nWaveLength, nRepeat, "StepPL")}");
                                ssb.AppendLine($"{typeName}_EXTINCTION_COEFFICIENT_{nWaveLength * m_soloVpeTable.GetRepeatCount() + nRepeat + 1} , {dtDateTime:yyyy-MM-dd HH:mm:ss.fff},  {m_soloVpeTable.GetRepeatValue(nWaveLength, nRepeat, "ExtinctionCoefficient")}");
                                ssb.AppendLine($"{typeName}_COLLECT_TIME_{nWaveLength * m_soloVpeTable.GetRepeatCount() + nRepeat + 1}, {dtDateTime:yyyy-MM-dd HH:mm:ss.fff},  {m_soloVpeTable.GetRepeatValue(nWaveLength, nRepeat, "CompletionDateTime")}");
                            }
                        }
                        // Raw Data
                        for (int nRepeat = 0; nRepeat < m_soloVpeTable.GetRepeatCount(); nRepeat++)
                        {
                            ssb.AppendLine($"{typeName}_REPEAT{nRepeat + 1}_PATHLENGTH, {dtDateTime:yyyy-MM-dd HH:mm:ss.fff},  {m_soloVpeTable.GetRawValue(nRepeat, "Pathlength")}");
                            ssb.AppendLine($"{typeName}_REPEAT{nRepeat + 1}_WAVELENGTH, {dtDateTime:yyyy-MM-dd HH:mm:ss.fff},  {m_soloVpeTable.GetRawValue(nRepeat, "Wavelength")}");
                            ssb.AppendLine($"{typeName}_REPEAT{nRepeat + 1}_ABS , {dtDateTime:yyyy-MM-dd HH:mm:ss.fff},  {m_soloVpeTable.GetRawValue(nRepeat, "Absorbance")}");
                        }
                    }

                    //foreach (KeyValuePair<string, List<string>> kvp in m_soloVpe.m_soloVpeDic)
                    //{
                    //    dataString = m_soloVpe.GetValue(outputArguments[0].ToString(), kvp.Value);
                    //    ssb.AppendLine($"{typeName}_{kvp.Key.ToUpper()}, {dtDateTime:yyyy-MM-dd HH:mm:ss.fff}, {dataString}");
                    //}
                    EnQueue(MSGTYPE.MEASURE, ssb.ToString());
                }
                catch(Exception ex)
                {
                    listViewMsg.UpdateMsg($"Exception in ProcessMethodDaqID. DaqID : {m_soloVpeTable.newDaqID} Jason DataProcessing - {ex}", false, true, true, PC00D01.MSGTINF);
                }
            }
            return true;
        }

        private void InitOpcUaClient()
        {
            try
            {
                // OpcUaClient 를 생성하고
                myUaClient = new OpcUaClient.OpcUaClient()
                {
                    endpointURL = m_ConnectionInfo,
                    applicationName = m_Name,
                    applicationType = ApplicationType.Client,
                    subjectName = Utils.Format($@"CN={m_Name}, DC={0}", Dns.GetHostName())
                };

                if (!string.IsNullOrEmpty(Uid) && !string.IsNullOrEmpty(Pwd))
                    myUaClient.useridentity = new UserIdentity(Uid, Pwd);

                myUaClient.CreateConfig();
                myUaClient.CreateApplicationInstance();
                myUaClient.CreateSession();

                // Session/Subscription을 생성한 후
                myUaClient.CreateSubscription(1000);
                listViewMsg.UpdateMsg($"myUaClient.CreateSubscription ", false, true, true, PC00D01.MSGTINF);
                // CSV 파일에 있는 TagName, NodeId 리스트를 MonitoredItem으로 등록하고 
                ReadConfigInfo();
                myUaClient.UpateTagData += UpdateTagValue;
                // 20220818, SHS, OPC Client Log 출력용 등록
                myUaClient.LogMsgFunc += LogMsg;

                listViewMsg.UpdateMsg($"myUaClient.UpateTagData ", false, true, true, PC00D01.MSGTINF);
                // currentSubscription에 대한 서비스를 등록한다.
                bool bReturn = myUaClient.AddSubscription();
                if (bReturn == false) myUaClient = null;
                listViewMsg.UpdateMsg($"{bReturn}= myUaClient.AddSubscription", false, true, true, PC00D01.MSGTINF);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
                listViewMsg.UpdateMsg($"Exceptioin - InitOpcUaClient ({ex})", false, true, true, PC00D01.MSGTERR);
                myUaClient = null;
            }
        }
        // 20220818, SHS, OPC Client Log 출력 처리 함수
        public void LogMsg(string Msg)
        {
            listViewMsg.UpdateMsg($" OPC UA Client - {Msg}", false, true, true, PC00D01.MSGTINF);
        }

        private DateTime GetLastEnqueuedDate()
        {
            DateTime LastEnqueuedDate=DateTime.MinValue;
            string strLastEnqueuedDate = m_sqlBiz.ReadSTCommon(m_Name, "LastEnqueuedDate"); //PC00U01.ReadConfigValue("LastEnqueuedDate", m_Name, $@".\CFG\{m_Type}.ini");
            DateTime.TryParseExact(strLastEnqueuedDate,"yyyyMMdd", CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal | DateTimeStyles.AllowInnerWhite, out LastEnqueuedDate);
            if (LastEnqueuedDate == DateTime.MinValue)
                LastEnqueuedDate = DateTime.Now.Subtract(TimeSpan.FromDays(1));
            listViewMsg.UpdateMsg($"Read last enqueued Date  : {LastEnqueuedDate.ToString("yyyyMMdd")}", false, true);
            listViewMsg.UpdateMsg($"m_LastEnqueuedDate :{LastEnqueuedDate.ToString("yyyyMMdd") } , {strLastEnqueuedDate}  !", false, true, true, PC00D01.MSGTINF);
            return LastEnqueuedDate;
        }
        private bool SetLastEnqueuedDate(DateTime LastEnqueuedDate)
        {
            //if (!PC00U01.WriteConfigValue("LastEnqueuedDate", m_Name, $@".\CFG\{m_Type}.ini", $"{LastEnqueuedDate.ToString("yyyyMMdd")}"))
            if (!m_sqlBiz.WriteSTCommon(m_Name, "LastEnqueuedDate", $"{LastEnqueuedDate.ToString("yyyyMMdd")}"))
            {
                listViewMsg.UpdateMsg($"Error to write LastEnqueuedDate to INI file", false, true);
                return false;
            }
            listViewMsg.UpdateMsg($"Write last LastEnqueuedDate : {LastEnqueuedDate.ToString("yyyyMMdd")}", false, true);
            return true;
        }

        private int GetLastEnqueuedDaqID()
        {
            int LastEnqueuedDaqID;
            string strLastEnqueuedDaqID = m_sqlBiz.ReadSTCommon(m_Name, "LastEnqueuedDaqID"); //PC00U01.ReadConfigValue("LastEnqueuedDaqID", m_Name, $@".\CFG\{m_Type}.ini");
            int.TryParse(strLastEnqueuedDaqID, out LastEnqueuedDaqID);
            listViewMsg.UpdateMsg($"Read last enqueued DaqID  : {LastEnqueuedDaqID}", false, true);
            listViewMsg.UpdateMsg($"m_LastEnqueuedDaqID :{LastEnqueuedDaqID} , {strLastEnqueuedDaqID}  !", false, true, true, PC00D01.MSGTINF);
            return LastEnqueuedDaqID;
        }
        private bool SetLastEnqueuedDaqID(int LastEnqueuedDaqID)
        {
            //if (!PC00U01.WriteConfigValue("LastEnqueuedDaqID", m_Name, $@".\CFG\{m_Type}.ini", $"{LastEnqueuedDaqID}"))
            if (!m_sqlBiz.WriteSTCommon(m_Name, "LastEnqueuedDaqID", $"{LastEnqueuedDaqID}"))
            {
                listViewMsg.UpdateMsg($"Error to write LastEnqueuedDaqID to INI file", false, true);
                return false;
            }
            listViewMsg.UpdateMsg($"Write last LastEnqueuedDaqID : {LastEnqueuedDaqID}", false, true);
            return true;
        }
        private void ReadConfigInfo()
        {
            try
            {
                string configInfo = drEquipment["CONFIG_INFO"]?.ToString();
                if (string.IsNullOrWhiteSpace(configInfo))
                {
                    m_Owner.listViewMsg(m_Name, $"Read Config Info", false, m_nCurNo, 6, true, PC00D01.MSGTINF);
                    return;
                }
                string[] arrConfigInfo = drEquipment["CONFIG_INFO"]?.ToString().Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
                foreach (string lineData in arrConfigInfo)
                {
                    listViewMsg.UpdateMsg($"Data : {lineData}", false, true, true, PC00D01.MSGTINF);
                    string[] data = lineData.Split(',');
                    if (data.Length < 2)
                        continue;
                    if (string.IsNullOrWhiteSpace(data[0]) || string.IsNullOrWhiteSpace(data[1]))
                        continue;
                    myUaClient.AddItem(data[0], data[1]);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
                listViewMsg.UpdateMsg($"Exceptioin - ReadConfigInfo ({ex})", false, true, true, PC00D01.MSGTERR);
            }
        }



        void ReadCsvFile()
        {
            string section = m_Name;
            string configFile = @".\CFG\" + m_Name + ".csv";
            string lineData = string.Empty;
            string errCode = string.Empty;
            string errText = string.Empty;


            try
            {
                using (StreamReader sr = new StreamReader(configFile))
                {
                    m_Owner.listViewMsg(m_Name, $"Open File : {configFile}", false, m_nCurNo, 6, true, PC00D01.MSGTINF);
                    while (!sr.EndOfStream)
                    {
                        lineData = sr.ReadLine();
                        listViewMsg.UpdateMsg($"Data : {lineData}", false, true, true, PC00D01.MSGTINF);
                        string[] data = lineData.Split(',');
                        if (data.Length < 2)
                            continue;
                        if (string.IsNullOrWhiteSpace(data[0]) || string.IsNullOrWhiteSpace(data[1]) )
                            continue;
                        myUaClient.AddItem(data[0], data[1]);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
                listViewMsg.UpdateMsg($"Exceptioin - ReadCsvFile ({ex})", false, true, true, PC00D01.MSGTERR);
            }
        }

        public void UpdateTagValue( string tagname, string value, string datetime, string status)
        {
            //EnQueue(MSGTYPE.MEASURE,$"{tagname},{datetime},{value},{status}");
            EnQueue(MSGTYPE.MEASURE,$"{tagname},{datetime},{value}");
            listViewMsg.UpdateMsg($"{tagname},{datetime},{value},{status}",false, true, true, PC00D01.MSGTINF);
        }



    }
}
