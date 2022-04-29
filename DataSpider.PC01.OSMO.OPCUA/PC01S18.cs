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
    public class PC01S18 : PC00B01
    {
        OpcUaClient.OpcUaClient myUaClient=null;
        private DateTime dtNormalTime = DateTime.Now;
        private string m_LastResultDate = string.Empty;
        private string m_ToBeResultDate = string.Empty;
        private string m_LastEventDate = string.Empty;
        private string m_ToBeEventDate = string.Empty;
        private Osmo m_Osmo = new Osmo();

        private DateTime dtDateTime;
        private string dataString = string.Empty;
        private string typeName = "MSR";
        private StringBuilder ssb = new StringBuilder();

        public class Osmo
        {
            const int ARRAY0 = 0;
            const int PROPERTY1 = 1;
            const int ARRAY1 = 2;
            const int PROPERTY2 = 3;

            public string newResultTS;
            public string newResultDS;
            public string newResultRS;
            public string newResultSI;
            public string newResultUI;

            public string newEventTS;
            public string newEventDS;
            public string newEventTY;
            public string newEventUI;


            public Dictionary<string, List<string>> m_OsmoDic = new Dictionary<string, List<string>>();


            /// <summary>
            /// Result jsonString에서 currentTS 보다 큰 가장 작은 TS 찾는다.
            /// 만일 currentTS 보다 큰 가장 작은 TS를 찾지 못하면 newTS는 string.Empty가 된다.
            /// 이 때는 newTS를 currentTS로 설정한다.
            /// </summary>
            /// <param name="jsonString"></param>
            /// <param name="currentTS"></param>
            public bool GetResultTimeStamp(string jsonString, string currentTS) 
            {
                JsonDocument document;
                JsonElement root, jElement;

                newResultTS = currentTS;

                document = JsonDocument.Parse(jsonString);
                root = document.RootElement;
                jElement=root.GetProperty("Results");
                if (jElement.ValueKind != JsonValueKind.Array  || jElement.GetArrayLength() == 0 ) return false;

                newResultTS = DateTime.MaxValue.ToString("yyyy-MM-ddTHH:mm:ss");
                newResultDS = string.Empty;
                newResultRS = string.Empty;
                newResultSI = string.Empty;
                newResultUI = string.Empty;

                foreach (JsonElement jjElement in jElement.EnumerateArray())
                {
                    if (string.Compare(jjElement.GetProperty("TS").GetString(), newResultTS)< 0)
                    {
                        newResultTS = jjElement.GetProperty("TS").GetString();
                        newResultDS = jjElement.GetProperty("DS").GetString();
                        newResultRS = jjElement.GetProperty("RS").ToString();
                        newResultSI = jjElement.GetProperty("SI").GetString();
                        newResultUI = jjElement.GetProperty("UI").GetString();
                    }
                }
                return true;
            }


            /// <summary>
            /// Event jsonString에서 currentTS 보다 큰 가장 작은 TS 찾는다.
            /// 만일 currentTS 보다 큰 가장 작은 TS를 찾지 못하면 newTS는 string.Empty가 된다.
            /// 이 때는 newTS를 currentTS로 설정한다.
            /// </summary>
            /// <param name="jsonString"></param>
            /// <param name="currentTS"></param>
            public bool GetEventTimeStamp(string jsonString, string currentTS)
            {
                JsonDocument document;
                JsonElement root, jElement;

                newResultTS = currentTS;

                document = JsonDocument.Parse(jsonString);
                root = document.RootElement;
                jElement = root.GetProperty("Events");
                if (jElement.ValueKind != JsonValueKind.Array || jElement.GetArrayLength() == 0) return false;

                newEventTS = DateTime.MaxValue.ToString("yyyy-MM-ddTHH:mm:ss");
                newEventDS = string.Empty;
                newEventTY = string.Empty;
                newEventUI = string.Empty;

                foreach (JsonElement jjElement in jElement.EnumerateArray())
                {
                    if (string.Compare(jjElement.GetProperty("TS").GetString(), newEventTS) < 0)
                    {
                        newEventTS = jjElement.GetProperty("TS").GetString();
                        newEventDS = jjElement.GetProperty("DS").GetString();
                        newEventTY = jjElement.GetProperty("TY").ToString();
                        newEventUI = jjElement.GetProperty("UI").GetString();
                    }
                }
                return true;
            }
        }

        public PC01S18() : base()
        {
        }

        public PC01S18(PC01F01 pOwner, string equipType, string equipName, string connectionInfo, string extraInfo, int nCurNo, bool bAutoRun = false) : base(pOwner, equipType, equipName, connectionInfo, extraInfo, nCurNo, bAutoRun)
        {
        }

        public PC01S18(PC01F01 pOwner, DataRow dr, int nCurNo, bool bAutoRun = false) : this(pOwner, dr["EQUIP_TYPE_NM"].ToString(), dr["EQUIP_NM"].ToString(), dr["CONNECTION_INFO"].ToString(), dr["EXTRA_INFO"].ToString(), nCurNo, bAutoRun)
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
                listViewMsg.UpdateMsg($"Exceptioin - PC01S18 ({ex})", false, true, true, PC00D01.MSGTERR);
            }
        }

        private void ThreadJob()
        {

            listViewMsg.UpdateStatus(true);
            listViewMsg.UpdateMsg("Thread started");

            m_LastResultDate = GetLastResultDate();
            listViewMsg.UpdateMsg($"Read From Ini File m_LastResultDate :{m_LastResultDate}", false, true, true, PC00D01.MSGTINF);
            m_LastEventDate = GetLastEventDate();
            listViewMsg.UpdateMsg($"Read From Ini File m_LastEventDate :{m_LastEventDate}", false, true, true, PC00D01.MSGTINF);

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
                        dtNormalTime = DateTime.Now;
                    }
                    else
                    {
                        if (myUaClient.reconnectHandler != null)
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
                            bool bResults = ProcessMethodRemoteResults(m_LastResultDate, out m_ToBeResultDate);
                            bool bEvents = ProcessMethodRemoteEvents(m_LastEventDate, out m_ToBeEventDate);

                            if (bResults == true)
                            {
                                if (m_LastResultDate != m_ToBeResultDate)
                                {
                                    m_LastResultDate = m_ToBeResultDate;
                                    SetLastResultDate(m_LastResultDate);
                                }
                            }

                            if( bEvents == true)
                            {
                                if (m_LastEventDate != m_ToBeEventDate)
                                {
                                    m_LastEventDate = m_ToBeEventDate;
                                    SetLastEventDate(m_LastEventDate);
                                }
                            }

                            if (bResults == true || bEvents == true)
                            {
                                UpdateEquipmentProgDateTime(IF_STATUS.Normal);
                                dtNormalTime = DateTime.Now;
                            }
                            else
                            { 
                                // 메소드 호출/응답 이상일 때
                                if ((DateTime.Now - dtNormalTime).TotalHours < 1)
                                {
                                    //UpdateEquipmentProgDateTime(IF_STATUS.InvalidData);
                                    UpdateEquipmentProgDateTime(IF_STATUS.NetworkError);
                                }
                                else
                                {
                                    myUaClient = null;
                                    listViewMsg.UpdateMsg($" Network Error Time > 1 Hr, Ua Client Reset ", false, true, true, PC00D01.MSGTERR);
                                }
                            }
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
        /// RemoteResults 메소드를 호출하여 startDate이후의 값들을 읽어온다. 
        /// 처리안된 가장 작은 시간의 데이터를 읽어온다.
        /// </summary>
        /// <param name="startDate"></param>
        /// <param name="ToBeProcessedDate"></param>
        /// <returns> false : Method 호출 실패  </returns>
        /// <returns>true : Method 호출/응답 정상일 때</returns>
        private bool ProcessMethodRemoteResults(string startDate, out string newDate)
        {
            newDate = startDate;

            IList<object> outputArguments = null;
            try
            {
                List<object> InputArguments = new List<object>();

                outputArguments = myUaClient.session.Call(new NodeId("ns=3;s=OsmoTECH_XT"),
                                                            new NodeId("ns=3;s=remoteResults"),
                                                            startDate
                                                         );
            }
            catch (Opc.Ua.ServiceResultException ex)
            {
                listViewMsg.UpdateMsg($"ProcessMethodRemoteResults Exception in OPC Call - {ex.Message}", false, true, true, PC00D01.MSGTINF);
                UpdateEquipmentProgDateTime(IF_STATUS.NetworkError);
                return false;
            }
            if (outputArguments != null && outputArguments.Count >= 1)
            {
                if (m_Osmo.GetResultTimeStamp(outputArguments[0].ToString(), startDate) == true)
                {
                    PC00U01.TryParseExact(m_Osmo.newResultTS, out dtDateTime);
                    ssb.Clear();
                    ssb.AppendLine($"{typeName}_SVRTIME, {dtDateTime:yyyy-MM-dd HH:mm:ss.fff}, {DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}");

                    ssb.AppendLine($"{typeName}_DS, {dtDateTime:yyyy-MM-dd HH:mm:ss.fff}, {m_Osmo.newResultDS}");
                    ssb.AppendLine($"{typeName}_RS, {dtDateTime:yyyy-MM-dd HH:mm:ss.fff}, {m_Osmo.newResultRS}");
                    ssb.AppendLine($"{typeName}_SI, {dtDateTime:yyyy-MM-dd HH:mm:ss.fff}, {m_Osmo.newResultSI}");
                    ssb.AppendLine($"{typeName}_UI, {dtDateTime:yyyy-MM-dd HH:mm:ss.fff}, {m_Osmo.newResultUI}");
                    EnQueue(MSGTYPE.MEASURE, ssb.ToString());
                    newDate = m_Osmo.newResultTS;
                }                
            }
            return true;
        }
        /// <summary>
        /// RemoteEvents 메소드를 호출하여 startDate이후의 값들을 읽어온다. 
        /// 처리안된 가장 작은 시간의 데이터를 읽어온다.
        /// </summary>
        /// <param name="startDate"></param>
        /// <param name="ToBeProcessedDate"></param>
        /// <returns> false : Method 호출 실패   </returns>
        /// <returns>true : Method 호출/응답 정상일 때</returns>
        private bool ProcessMethodRemoteEvents(string startDate, out string newDate)
        {
            newDate = startDate;

            IList<object> outputArguments = null;
            try
            {
                List<object> InputArguments = new List<object>();

                outputArguments = myUaClient.session.Call(new NodeId("ns=3;s=OsmoTECH_XT"),
                                                            new NodeId("ns=3;s=remoteEvents"),
                                                            startDate
                                                         );
            }
            catch (Opc.Ua.ServiceResultException ex)
            {
                listViewMsg.UpdateMsg($"ProcessMethodRemoteEvents Exception in OPC Call - {ex.Message}", false, true, true, PC00D01.MSGTINF);
                UpdateEquipmentProgDateTime(IF_STATUS.NetworkError);
                return false;
            }
            if (outputArguments != null && outputArguments.Count >= 1)
            {
                if (m_Osmo.GetEventTimeStamp(outputArguments[0].ToString(), startDate) == true)
                {
                    PC00U01.TryParseExact(m_Osmo.newEventTS, out dtDateTime);
                    ssb.Clear();
                    ssb.AppendLine($"EVT_SVRTIME, {dtDateTime:yyyy-MM-dd HH:mm:ss.fff}, {DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}");
                    ssb.AppendLine($"EVT_DS, {dtDateTime:yyyy-MM-dd HH:mm:ss.fff}, {m_Osmo.newEventDS}");
                    ssb.AppendLine($"EVT_TY, {dtDateTime:yyyy-MM-dd HH:mm:ss.fff}, {m_Osmo.newEventTY}");
                    ssb.AppendLine($"EVT_UI, {dtDateTime:yyyy-MM-dd HH:mm:ss.fff}, {m_Osmo.newEventUI}");
                    EnQueue(MSGTYPE.EVENT, ssb.ToString());
                    newDate = m_Osmo.newEventTS;
                }
            }
            return true;
        }


        private void InitOpcUaClient()
        {
            try
            {
                // OpcUaClient 를 생성하고
                myUaClient = new OpcUaClient.OpcUaClient(m_ConnectionInfo, m_Name);
                // Session/Subscription을 생성한 후
                myUaClient.CreateSubscription(1000);
                listViewMsg.UpdateMsg($"myUaClient.UpateTagData ", false, true, true, PC00D01.MSGTINF);
                // CSV 파일에 있는 TagName, NodeId 리스트를 MonitoredItem으로 등록하고 
                ReadCsvFile();
                myUaClient.UpateTagData += UpdateTagValue;
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
        private string GetLastEventDate()
        {
            string LastEventDate;
            LastEventDate = m_sqlBiz.ReadSTCommon(m_Name, "LastEventDate"); //PC00U01.ReadConfigValue("LastEventDate", m_Name, $@".\CFG\{m_Type}.ini");
            listViewMsg.UpdateMsg($"Read LastEventDate   : {LastEventDate}", false, true);
            listViewMsg.UpdateMsg($"m_LastEventDate :{LastEventDate}  !", false, true, true, PC00D01.MSGTINF);
            return LastEventDate;
        }
        private bool SetLastEventDate(string LastEventDate)
        {
            //if (!PC00U01.WriteConfigValue("LastEventDate", m_Name, $@".\CFG\{m_Type}.ini", $"{LastEventDate}"))
            if (!m_sqlBiz.WriteSTCommon(m_Name, "LastEventDate", $"{LastEventDate}"))
            {
                listViewMsg.UpdateMsg($"Error to write LastEventDate to INI file", false, true);
                return false;
            }
            listViewMsg.UpdateMsg($"Write  LastEventDate : {LastEventDate}", false, true);
            return true;
        }
        private string GetLastResultDate()
        {
            string LastResultDate;
            LastResultDate = m_sqlBiz.ReadSTCommon(m_Name, "LastResultDate"); //PC00U01.ReadConfigValue("LastResultDate", m_Name, $@".\CFG\{m_Type}.ini");
            listViewMsg.UpdateMsg($"Read last result Date  : {LastResultDate}", false, true);
            listViewMsg.UpdateMsg($"m_LastResultDate :{LastResultDate}  !", false, true, true, PC00D01.MSGTINF);
            return LastResultDate;
        }
        private bool SetLastResultDate(string LastResultDate)
        {
            //if (!PC00U01.WriteConfigValue("LastResultDate", m_Name, $@".\CFG\{m_Type}.ini", $"{LastResultDate}"))
            if (!m_sqlBiz.WriteSTCommon(m_Name, "LastResultDate", $"{LastResultDate}"))
            {
                listViewMsg.UpdateMsg($"Error to write LastResultDate to INI file", false, true);
                return false;
            }
            listViewMsg.UpdateMsg($"Write last LastResultDate : {LastResultDate}", false, true);
            return true;
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
