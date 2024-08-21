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
using System.Text.Json;
using System.Globalization;


namespace DataSpider.PC01.PT
{
    /// <summary>
    /// Equip_Type : OSMO_TECH XT OPC UA Server
    /// </summary>
    public class PC01S35 : PC00B01
    {
        OpcUaClient.OpcUaClient myUaClient=null;
        Dictionary<string, string> dicOpcItem = new Dictionary<string, string>();
        private string Uid;
        private string Pwd;
        private DateTime dtLastEnqueuedResult = DateTime.Now;
        private DateTime dtLastEnqueuedEvent = DateTime.Now;
        private int noEventDataCount = 60;
        private int noResultDataCount = 60;

        public PC01S35() : base()
        {            
        }

        public PC01S35(PC01F01 pOwner, string equipType, string equipName, string connectionInfo, string extraInfo, int nCurNo, bool bAutoRun = false) : base(pOwner, equipType, equipName, connectionInfo, extraInfo, nCurNo, bAutoRun)
        {
        }

        public PC01S35(PC01F01 pOwner, DataRow dr, int nCurNo, bool bAutoRun = false) : this(pOwner, dr["EQUIP_TYPE_NM"].ToString(), dr["EQUIP_NM"].ToString(), dr["CONNECTION_INFO"].ToString(), dr["EXTRA_INFO"].ToString(), nCurNo, bAutoRun)
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
                listViewMsg.UpdateMsg($"Exceptioin - {this.GetType().Name} ({ex})", false, true, true, PC00D01.MSGTERR);
            }
        }

        // remoteResults 를 call 하여 처리
        private bool ProcessMethodRemoteResults()
        {
            IList<object> outputArguments = null;
            try
            {
                List<object> InputArguments = new List<object>();

                listViewMsg.UpdateMsg($"ProcessMethodRemoteResults. OPC Call remoteResults - Result Requestd : {dtLastEnqueuedResult:yyyy-MM-dd HH:mm:ss}", false, true, true, PC00D01.MSGTINF);

                outputArguments = myUaClient.m_session.Call(new NodeId("ns=3;s=OsmoTECH_XT"),
                                                            new NodeId("ns=3;s=remoteResults"),
                                                            $"{dtLastEnqueuedResult:yyyy-MM-dd HH:mm:ss}"
                                                         );
            }
            catch (Opc.Ua.ServiceResultException ex)
            {
                listViewMsg.UpdateMsg($"ProcessMethodRemoteResults Exception in OPC Call - {ex.Message}", false, true, true, PC00D01.MSGTERR);
                UpdateEquipmentProgDateTime(IF_STATUS.NetworkError);
                return false;
            }
            try
            {
                if (outputArguments != null && outputArguments.Count > 0)
                {
                    fileLog.WriteData(outputArguments[0].ToString(), "ResultData", "remoteResults");

                    JsonDocument jDoc = JsonDocument.Parse(outputArguments[0].ToString());
                    JsonElement jElement = jDoc.RootElement.GetProperty("Results");
                    if (jElement.ValueKind != JsonValueKind.Array)
                    {
                        listViewMsg.UpdateMsg($"ProcessMethodRemoteResults Invalid Json Data", false, true, true, PC00D01.MSGTERR);
                        return false;
                    }
                    if (jElement.GetArrayLength() < 1)
                    {
                        if (noResultDataCount++ > 60)
                        {
                            noResultDataCount = 0;
                            listViewMsg.UpdateMsg($"No result data since {dtLastEnqueuedResult:yyyy-MM-dd HH:mm:ss}", false, true, true, PC00D01.MSGTINF);
                        }
                        return false;
                    }
                    noResultDataCount = 0;

                    StringBuilder sbResult = new StringBuilder();
                    string TS = string.Empty;

                    // TS 로 오름차순으로 소팅하여 오래된 데이터 부터 처리
                    foreach (JsonElement element in jElement.EnumerateArray().OrderBy(e => e.GetProperty("TS").ToString()))
                    {
                        sbResult.Clear();

                        TS = element.GetProperty("TS").GetDateTime().ToString("yyyy-MM-dd HH:mm:ss");
                        sbResult.AppendLine($"MSR_SVRTIME, {TS}, {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
                        sbResult.AppendLine($"MSR_DS, {TS}, {element.GetProperty("DS")}");
                        sbResult.AppendLine($"MSR_RS, {TS}, {element.GetProperty("RS")}");
                        sbResult.AppendLine($"MSR_SI, {TS}, {element.GetProperty("SI")}");
                        sbResult.AppendLine($"MSR_UI, {TS}, {element.GetProperty("UI")}");

                        foreach (KeyValuePair<string, string> kvp in dicOpcItem)
                        {
                            try
                            {
                                string strValue = myUaClient.ReadValue(kvp.Value).Value?.ToString();
                                sbResult.AppendLine($"{kvp.Key}, {TS}, {strValue}");
                                listViewMsg.UpdateMsg($" {kvp.Key}, {TS}, {strValue} ", false, true, true, PC00D01.MSGTINF);
                            }
                            catch (Exception ex)
                            {
                                listViewMsg.UpdateMsg($" Read Tag Value - {kvp.Key}, {kvp.Value} - {ex}", false, true, true, PC00D01.MSGTERR);
                            }
                        }

                        EnQueue(MSGTYPE.MEASURE, sbResult.ToString());
                        dtLastEnqueuedResult = element.GetProperty("TS").GetDateTime();
                        SetLastEnqueuedDateTime(dtLastEnqueuedResult, "Result");
                    }
                }
            }
            catch (Exception ex)
            {
                listViewMsg.UpdateMsg($"ProcessMethodRemoteResults Exception in Parsing json data - {ex.Message}", false, true, true, PC00D01.MSGTERR);
                UpdateEquipmentProgDateTime(IF_STATUS.InvalidData);
                return false;
            }
            return true;
        }

        // remoteEvents 를 call 하여 처리
        private bool ProcessMethodRemoteEvents()
        {
            IList<object> outputArguments = null;
            try
            {
                List<object> InputArguments = new List<object>();

                listViewMsg.UpdateMsg($"ProcessMethodRemoteResults. OPC Call remoteResults - Result Requestd : {dtLastEnqueuedEvent:yyyy-MM-dd HH:mm:ss}", false, true, true, PC00D01.MSGTINF);

                outputArguments = myUaClient.m_session.Call(new NodeId("ns=3;s=OsmoTECH_XT"),
                                                            new NodeId("ns=3;s=remoteEvents"),
                                                            $"{dtLastEnqueuedEvent:yyyy-MM-dd HH:mm:ss}"
                                                         );
            }
            catch (Opc.Ua.ServiceResultException ex)
            {
                listViewMsg.UpdateMsg($"ProcessMethodRemoteEvents Exception in OPC Call - {ex.Message}", false, true, true, PC00D01.MSGTERR);
                UpdateEquipmentProgDateTime(IF_STATUS.NetworkError);
                return false;
            }
            try
            {
                if (outputArguments != null && outputArguments.Count > 0)
                {
                    fileLog.WriteData(outputArguments[0].ToString(), "EventData", "remoteEvents");

                    JsonDocument jDoc = JsonDocument.Parse(outputArguments[0].ToString());
                    JsonElement jElement = jDoc.RootElement.GetProperty("Events");
                    if (jElement.ValueKind != JsonValueKind.Array)
                    {
                        listViewMsg.UpdateMsg($"ProcessMethodRemoteEvents Invalid Json Data", false, true, true, PC00D01.MSGTERR);
                        return false;
                    }
                    if (jElement.GetArrayLength() < 1)
                    {
                        if (noEventDataCount++ > 60)
                        {
                            noEventDataCount = 0;
                            listViewMsg.UpdateMsg($"No event data since {dtLastEnqueuedEvent:yyyy-MM-dd HH:mm:ss}", false, true, true, PC00D01.MSGTINF);
                        }
                        return false;
                    }
                    noEventDataCount = 0;

                    StringBuilder sbResult = new StringBuilder();
                    string TS = string.Empty;
                    string DS = string.Empty;

                    string timeStamp = string.Empty;
                    string calVal = string.Empty;
                    string calStep = string.Empty;
                    string temp = string.Empty;

                    // TS 로 오름차순으로 소팅하여 오래된 데이터 부터 처리
                    foreach (JsonElement element in jElement.EnumerateArray().OrderBy(e => e.GetProperty("TS").ToString()))
                    {
                        // TYPE 이 Calibration 이 아니면 다음에 조회대상에서 제외하기 위해 그 시간만 업데이트
                        if (!element.GetProperty("TY").ToString().Equals("Calibration"))
                        {
                            dtLastEnqueuedEvent = element.GetProperty("TS").GetDateTime();
                            SetLastEnqueuedDateTime(dtLastEnqueuedEvent, "Event");
                            continue;
                        }

                        DS = element.GetProperty("DS").ToString();

                        // DS 맨 앞글자가 숫자이면 정상 Calibration Step (0 #1: 12 / 6003.0 / -0.022 / 12.0 / 12.0 / 2 / 101)
                        // 첫번째 : 의 왼쪽이 CAL_STEP, 오른쪽이 CAL_VAL 처리
                        if (Regex.IsMatch(DS, "^\\d+"))
                        {
                            temp = DS.Substring(0, DS.IndexOf("/"));    //0 #1: 12 
                            calStep = temp.Substring(0, temp.IndexOf(":")).Trim();
                            calVal = temp.Substring(temp.IndexOf(":") + 1).Trim();
                        }
                        else
                        {
                            if (DS.StartsWith("Successful"))
                            {
                                calStep = "Successful";
                            }
                            //if (ds.StartsWith("Calibrator out of range"))
                            else
                            {
                                calStep = DS;
                            }
                        }
                        if (string.IsNullOrWhiteSpace(calStep))
                        {
                            dtLastEnqueuedEvent = element.GetProperty("TS").GetDateTime();
                            SetLastEnqueuedDateTime(dtLastEnqueuedEvent, "Event");
                            continue;
                        }

                        sbResult.Clear();

                        TS = element.GetProperty("TS").GetDateTime().ToString("yyyy-MM-dd HH:mm:ss");
                        sbResult.AppendLine($"CAL_SVRTIME, {TS}, {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
                        sbResult.AppendLine($"CAL_STEP, {TS}, {calStep}");
                        sbResult.AppendLine($"CAL_VAL, {TS}, {calVal}");
                        sbResult.AppendLine($"CAL_UI, {TS}, {element.GetProperty("UI")}");

                        EnQueue(MSGTYPE.CALIBRATION, sbResult.ToString());
                        dtLastEnqueuedEvent = element.GetProperty("TS").GetDateTime();
                        SetLastEnqueuedDateTime(dtLastEnqueuedEvent, "Event");
                    }
                }
            }
            catch (Exception ex)
            {
                listViewMsg.UpdateMsg($"ProcessMethodRemoteEvents Exception in Parsing json data - {ex.Message}", false, true, true, PC00D01.MSGTERR);
                UpdateEquipmentProgDateTime(IF_STATUS.InvalidData);
                return false;
            }
            return true;
        }

        private void ThreadJob()
        {
            listViewMsg.UpdateStatus(true);
            listViewMsg.UpdateMsg("Thread started");

            //GetLastEnqueuedDateTime(out dtLastEnqueuedEvent, "Event");
            GetLastEnqueuedDateTime(out dtLastEnqueuedResult, "Result");

            while (!bTerminal)
            {
                try
                {
                    if (myUaClient == null)
                    {
                        UpdateEquipmentProgDateTime(IF_STATUS.Disconnected);
                        listViewMsg.UpdateMsg($"OPC UA Not Connected. Try to connect.", false, true, true, PC00D01.MSGTERR);
                        Thread.Sleep(5000);
                        InitOpcUaClient();
                    }
                    else
                    {
                        if (myUaClient.m_reconnectHandler != null)
                        {
                            UpdateEquipmentProgDateTime(IF_STATUS.Disconnected);
                        }
                        else
                        {
                            UpdateEquipmentProgDateTime(IF_STATUS.Normal);

                            ProcessMethodRemoteResults();
                            //ProcessMethodRemoteEvents();
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

        private bool GetLastEnqueuedDateTime(out DateTime lastEnqueuedDateTime, string postfix = "")
        {
            string codeName = string.IsNullOrWhiteSpace(postfix) ? "LastEnqueuedDateTime" : $"LastEnqueuedDateTime_{postfix}";
            string strLastEnqueuedDate = m_sqlBiz.ReadSTCommon(m_Name, codeName);
            DateTime.TryParseExact(strLastEnqueuedDate, "yyyyMMddHHmmss", CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal | DateTimeStyles.AllowInnerWhite, out lastEnqueuedDateTime);
            if (lastEnqueuedDateTime == DateTime.MinValue)
            {
                lastEnqueuedDateTime = DateTime.Now.Subtract(TimeSpan.FromDays(1));
                //SetLastEnqueuedDateTime(lastEnqueuedDateTime);
            }
            listViewMsg.UpdateMsg($"Read {codeName} : {lastEnqueuedDateTime:yyyyMMddHHmmss}", false, true, true, PC00D01.MSGTINF);
            return true;
        }

        private bool SetLastEnqueuedDateTime(DateTime lastEnqueuedDateTime, string postfix = "")
        {
            string codeName = string.IsNullOrWhiteSpace(postfix) ? "LastEnqueuedDateTime" : $"LastEnqueuedDateTime_{postfix}";
            if (!m_sqlBiz.WriteSTCommon(m_Name, codeName, $"{lastEnqueuedDateTime:yyyyMMddHHmmss}"))
            {
                listViewMsg.UpdateMsg($"Error to write {codeName} to DB", false, true, true);
                return false;
            }
            listViewMsg.UpdateMsg($"Write {codeName} : {lastEnqueuedDateTime:yyyyMMddHHmmss}", false, true, true, PC00D01.MSGTINF);
            return true;
        }

        public void LogMsg(string Msg)
        {
            listViewMsg.UpdateMsg($" OPC UA Client - {Msg}", false, true, true, PC00D01.MSGTINF);
        }

        void InitOpcUaClient()
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
                //myUaClient.CreateSubscription(1000);
                //listViewMsg.UpdateMsg($"myUaClient.CreateSubscription ", false, true, true, PC00D01.MSGTINF);
                // CSV 파일에 있는 TagName, NodeId 리스트를 MonitoredItem으로 등록하고 
                ReadConfigInfo();
                //myUaClient.UpateTagData += UpdateTagValue;
                myUaClient.LogMsgFunc += LogMsg; 

                //listViewMsg.UpdateMsg($"myUaClient.UpateTagData ", false, true, true, PC00D01.MSGTINF);

                // currentSubscription에 대한 서비스를 등록한다.
                //bool bReturn = myUaClient.AddSubscription();
                //if (bReturn == false)
                //{
                //    // 20240322, SHS, opcClient = null 처리 전에 opcClient?.Close() 추가
                //    myUaClient?.Close();
                //    myUaClient = null;
                //}
                //listViewMsg.UpdateMsg($"{bReturn}= myUaClient.AddSubscription", false, true, true, PC00D01.MSGTINF);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
                listViewMsg.UpdateMsg($"Exceptioin - InitOpcUaClient ({ex})", false, true, true, PC00D01.MSGTERR);
                // 20240322, SHS, opcClient = null 처리 전에 opcClient?.Close() 추가
                myUaClient?.Close();
                myUaClient = null;
            }
        }

        //void ReadCsvFile()
        //{
        //    string section = m_Name;
        //    string configFile = @".\CFG\" + m_Name + ".csv";
        //    string lineData = string.Empty;
        //    string errCode = string.Empty;
        //    string errText = string.Empty;

        //    try
        //    {
        //        using (StreamReader sr = new StreamReader(configFile))
        //        {
        //            m_Owner.listViewMsg(m_Name, $"Open File : {configFile}", false, m_nCurNo, 6, true, PC00D01.MSGTINF);
        //            while (!sr.EndOfStream)
        //            {
        //                lineData = sr.ReadLine();
        //                listViewMsg.UpdateMsg($"Data : {lineData}", false, true, true, PC00D01.MSGTINF);
        //                string[] data = lineData.Split(',');
        //                if (data.Length < 2)
        //                    continue;
        //                if (string.IsNullOrWhiteSpace(data[0]) || string.IsNullOrWhiteSpace(data[1]) )
        //                    continue;
        //                myUaClient.AddItem(data[0], data[1]);
        //                m_OpcItemList[data[0]] = data[1];
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        Debug.WriteLine(ex.ToString());
        //        listViewMsg.UpdateMsg($"Exceptioin - ReadCsvFile ({ex})", false, true, true, PC00D01.MSGTERR);
        //    }
        //}
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
                    //myUaClient.AddItem(data[0].Trim(), data[1].Trim());
                    dicOpcItem.TryAdd(data[0].Trim(), data[1].Trim());
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
                listViewMsg.UpdateMsg($"Exceptioin - ReadConfigInfo ({ex})", false, true, true, PC00D01.MSGTERR);
            }
        }

        //public void UpdateTagValue( string tagname, string value, string datetime, string status)
        //{
        //    if (tagname == "MSR_SVRTIME")
        //    {
        //        DateTime svrtime;
        //        List<string> listData = new List<string>();

        //        PC00U01.TryParseExact(value, out svrtime);  // 측정시간
        //        foreach (KeyValuePair<string, string> kvp in m_OpcItemList)
        //        {
        //            try
        //            {
        //                string strValue =myUaClient.ReadValue(kvp.Value).Value?.ToString();
        //                if (kvp.Key == "MSR_SVRTIME")
        //                {
        //                    //EnQueue(MSGTYPE.MEASURE, $" {kvp.Key},{svrtime.ToString("yyyy-MM-dd HH:mm:ss")}, {DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}");
        //                    listData.Add($"{kvp.Key}, {svrtime:yyyy-MM-dd HH:mm:ss}, {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
        //                    listViewMsg.UpdateMsg($" {kvp.Key}, {svrtime:yyyy-MM-dd HH:mm:ss}, {strValue}, {DateTime.Now:yyyy-MM-dd HH:mm:ss} ", false, true, true, PC00D01.MSGTINF);
        //                }
        //                else
        //                {
        //                    //EnQueue(MSGTYPE.MEASURE, $"{kvp.Key}, {svrtime:yyyy-MM-dd HH:mm:ss}, {strValue}");
        //                    listData.Add($"{kvp.Key}, {svrtime:yyyy-MM-dd HH:mm:ss}, {strValue}");
        //                }
        //                listViewMsg.UpdateMsg($" {kvp.Key}, {svrtime:yyyy-MM-dd HH:mm:ss}, {strValue} ", false, true, true, PC00D01.MSGTINF);
        //            }
        //            catch (Exception ex)
        //            {
        //                listViewMsg.UpdateMsg($" UpdateTagValue - {kvp.Key },{kvp.Value} - {ex}", false, true, true, PC00D01.MSGTERR);
        //            }
        //        }
        //        if (listData.Count > 0) 
        //        {
        //            EnQueue(MSGTYPE.MEASURE, string.Join(System.Environment.NewLine, listData));
        //        }
        //    }
        //}
    }
}
