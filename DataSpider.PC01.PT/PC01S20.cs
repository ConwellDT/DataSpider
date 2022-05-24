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
using System.Globalization;
using Opc.Ua;


namespace DataSpider.PC01.PT
{
    /// <summary>
    /// Equip_Type : ViCELL_BLU OPC UA Server
    /// </summary>
    public class PC01S20 : PC00B01
    {
        OpcUaClient.OpcUaClient myUaClient=null;
        private DateTime dtNormalTime = DateTime.Now;
        Dictionary<string, string> m_OpcItemList = new Dictionary<string, string>();
        ReferenceDescription viCellBlue;
        ReferenceDescriptionCollection viCellBlueRefs;
        ReferenceDescription _browsedMethods;
        ReferenceDescriptionCollection _methodCollection;
        NodeId _parentMethodNode;
        string m_ViCellStatus = String.Empty;
        string m_PrevViCellStatus = String.Empty;

        private DateTime m_LastEnqueuedDate = DateTime.Now;
        private DateTime m_PrevLastEnqueuedDate = DateTime.Now;
        private DateTime m_LastAccessTime = DateTime.Now;
        private TimeSpan m_accessTimeSpan = TimeSpan.FromSeconds(60);
        private string Uid;
        private string Pwd;
        public PC01S20() : base()
        {
        }

        public PC01S20(PC01F01 pOwner, string equipType, string equipName, string connectionInfo, string extraInfo, int nCurNo, bool bAutoRun = false) : base(pOwner, equipType, equipName, connectionInfo, extraInfo, nCurNo, bAutoRun)
        {
        }

        public PC01S20(PC01F01 pOwner, DataRow dr, int nCurNo, bool bAutoRun = false) : this(pOwner, dr["EQUIP_TYPE_NM"].ToString(), dr["EQUIP_NM"].ToString(), dr["CONNECTION_INFO"].ToString(), dr["EXTRA_INFO"].ToString(), nCurNo, bAutoRun)
        {
            try
            {
                drEquipment = dr;
                string[] extraInfo = dr["EXTRA_INFO"].ToString().Split(',');
                Uid = extraInfo[0].Trim();
                Pwd = extraInfo[1].Trim();
                if (m_AutoRun == true)
                {
                    m_Thd = new Thread(ThreadJob);
                    m_Thd.Start();
                }
            }
            catch(Exception ex)
            {
                listViewMsg.UpdateMsg($"Exceptioin - PC01S20 ({ex})", false, true, true, PC00D01.MSGTERR);
            }
        }

        private void ThreadJob()
        {
            listViewMsg.UpdateStatus(true);
            listViewMsg.UpdateMsg("Thread started");

            m_LastEnqueuedDate = GetLastEnqueuedDate();
            listViewMsg.UpdateMsg($"Read m_LastEnqueuedDate :{m_LastEnqueuedDate.ToString("yyyyMMddHHmmss")}", false, true, true, PC00D01.MSGTINF);


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
                            if ((DateTime.Now - dtNormalTime).TotalHours >= 1 )
                            {
                                myUaClient = null;
                                listViewMsg.UpdateMsg($" Network Error Time >= 1 Hr, Ua Client Reset ", false, true, true, PC00D01.MSGTERR);
                            }
                        }
                        else
                        {
                            UpdateEquipmentProgDateTime(IF_STATUS.Normal);
                            dtNormalTime = DateTime.Now;
                            m_ViCellStatus = m_OpcItemList["ViCellStatus"];
                            if (m_ViCellStatus == "0")
                            {
                                if (m_ViCellStatus != m_PrevViCellStatus || m_LastAccessTime < DateTime.Now.Subtract(m_accessTimeSpan))
                                {
                                    if (RequestLock())
                                    {
                                        GetSampleResult();
                                        ReleaseLock();
                                        m_LastAccessTime = DateTime.Now;
                                        if (m_LastEnqueuedDate != m_PrevLastEnqueuedDate)
                                        {
                                            m_PrevLastEnqueuedDate = m_LastEnqueuedDate;
                                            m_PrevViCellStatus = m_ViCellStatus;
                                            m_accessTimeSpan = TimeSpan.FromSeconds(60);
                                        }
                                    }
                                }
                            }
                            else
                            {
                                m_PrevViCellStatus = m_ViCellStatus;
                                m_accessTimeSpan = TimeSpan.FromSeconds(1);
                            }
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


        private void GetSampleResult()
        {
            try
            {
                var callResult = new ViCellBlu.VcbResultGetSampleResults { ErrorLevel = ViCellBlu.ErrorLevelEnum.Warning, MethodResult = ViCellBlu.MethodResultEnum.Failure };

                var method = _methodCollection.First(n => n.DisplayName.ToString().Equals("GetSampleResults"));
                var methodNodeGetSampleResults = ExpandedNodeId.ToNodeId(method.NodeId, myUaClient.m_session.NamespaceUris);

                var sampleResults = new List<ViCellBlu.SampleResult>();

                var requestHeader = new RequestHeader();
                var callMethodRequest = new CallMethodRequest
                {
                    ObjectId = _parentMethodNode,
                    MethodId = methodNodeGetSampleResults,
                    InputArguments = new VariantCollection
                            {
                                new Variant(string.Empty),                                  // User name string
                                new Variant(m_LastEnqueuedDate.AddSeconds(10)),             // start date
                                new Variant(DateTime.Now),                                  // end date
                                new Variant(1),                                             // filter ON: 0 = sample set, 1 = sample
                                new Variant(string.Empty),                                  //new Variant("Insect"), // Cell type or QC name
                                new Variant(string.Empty),                                  // Search string (sample or sample set name)
                                new Variant(string.Empty)                                   // Search tag string
                            }
                };

                var methodsToCall = new CallMethodRequestCollection { callMethodRequest };
                var responseHeader = myUaClient.m_session.Call(requestHeader, methodsToCall, out var results, out var diagnosticInfos);
                ClientBase.ValidateResponse(results, methodsToCall);
                ClientBase.ValidateDiagnosticInfos(diagnosticInfos, methodsToCall);

                if (ServiceResult.IsGood(responseHeader.ServiceResult))
                {
                    foreach (var result in results)
                    {
                        foreach (var outputArgument in result.OutputArguments)
                        {
                            callResult = DecodeRawSampleResults(outputArgument.Value, myUaClient.m_session.MessageContext);
                            if (callResult.MethodResult == ViCellBlu.MethodResultEnum.Success)
                            {
                                sampleResults = callResult.SampleResults;
                                sampleResults.Reverse();
                                foreach (var sampleResult in sampleResults)
                                {
                                    if (sampleResult.SampleDataUuid == Uuid.Empty) continue;

                                    DateTime svrtime;
                                    svrtime = sampleResult.AnalysisDateTime.ToLocalTime(); // 측정시간
                                                                                           //sampleResult.ReanalysisDateTime
                                                                                           // 재분석 시간의 처리방안.
                                    EnQueue(MSGTYPE.MEASURE, $" SVRTIME, {svrtime:yyyy-MM-dd HH:mm:ss}, {DateTime.Now:yyyy-MM-dd HH:mm:ss} ");
                                    listViewMsg.UpdateMsg($" SVRTIME, {svrtime:yyyy-MM-dd HH:mm:ss}, {DateTime.Now:yyyy-MM-dd HH:mm:ss} ", false, true, true, PC00D01.MSGTINF);

                                    PropertyInfo[] properties = sampleResult.GetType().GetProperties();
                                    foreach (PropertyInfo property in properties)
                                    {
                                        switch (property.Name)
                                        {
                                            case "Position":
                                                ViCellBlu.SamplePosition samplePosition = (ViCellBlu.SamplePosition)property.GetValue(sampleResult);
                                                PropertyInfo[] positionproperties = samplePosition.GetType().GetProperties();
                                                foreach (PropertyInfo positionproperty in positionproperties)
                                                {
                                                    switch (positionproperty.Name)
                                                    {
                                                        case "Row":
                                                        case "Column":
                                                            EnQueue(MSGTYPE.MEASURE, $" {positionproperty.Name},{svrtime:yyyy-MM-dd HH:mm:ss}, {positionproperty.GetValue(samplePosition)} ");
                                                            listViewMsg.UpdateMsg($" {positionproperty.Name}, {svrtime:yyyy-MM-dd HH:mm:ss}, {positionproperty.GetValue(samplePosition)} ", false, true, true, PC00D01.MSGTINF);
                                                            break;
                                                        default:
                                                            break;
                                                    }
                                                }
                                                break;
                                            default:
                                                if (property.PropertyType == typeof(DateTime))
                                                {
                                                    EnQueue(MSGTYPE.MEASURE, $" {property.Name}, {svrtime:yyyy-MM-dd HH:mm:ss}, {property.GetValue(sampleResult):yyyy-MM-dd HH:mm:ss} ");
                                                    listViewMsg.UpdateMsg($" {property.Name}, {svrtime:yyyy-MM-dd HH:mm:ss}, {property.GetValue(sampleResult):yyyy-MM-dd HH:mm:ss} ", false, true, true, PC00D01.MSGTINF);
                                                }
                                                else
                                                {
                                                    EnQueue(MSGTYPE.MEASURE, $" {property.Name}, {svrtime:yyyy-MM-dd HH:mm:ss}, {property.GetValue(sampleResult)} ");
                                                    listViewMsg.UpdateMsg($" {property.Name}, {svrtime:yyyy-MM-dd HH:mm:ss}, {property.GetValue(sampleResult)} ", false, true, true, PC00D01.MSGTINF);
                                                }
                                                break;
                                        }
                                    }
                                    m_LastEnqueuedDate = sampleResult.AnalysisDateTime.ToLocalTime();
                                    SetLastEnqueuedDate(m_LastEnqueuedDate);
                                }
                            }
                        }
                    }

                }
            }
            catch(Exception ex)
            {
                listViewMsg.UpdateMsg($"Exception in GetSampleResult - ({ex})", false, true, true, PC00D01.MSGTERR);
            }
        }

        public ViCellBlu.VcbResultGetSampleResults DecodeRawSampleResults(object rawResult, IServiceMessageContext messageContext)
        {
            var callResult = new ViCellBlu.VcbResultGetSampleResults { ErrorLevel = ViCellBlu.ErrorLevelEnum.Warning, MethodResult = ViCellBlu.MethodResultEnum.Failure };
            callResult.ErrorLevel = ViCellBlu.ErrorLevelEnum.Warning;
            callResult.MethodResult = ViCellBlu.MethodResultEnum.Failure;
            callResult.ResponseDescription = "Decoding raw result ...";
            try
            {
                var val = (Opc.Ua.ExtensionObject)rawResult;
                var myData = (byte[])val.Body;
                callResult.Decode(new Opc.Ua.BinaryDecoder(myData, 0, myData.Count(), messageContext));
            }
            catch (Exception ex)
            {
                callResult.ErrorLevel = ViCellBlu.ErrorLevelEnum.RequiresUserInteraction;
                callResult.MethodResult = ViCellBlu.MethodResultEnum.Failure;
                callResult.ResponseDescription = "DecodeRaw-Exception: " + ex.ToString();
            }
            return callResult;
        }


        private bool RequestLock()
        {
            bool bReturn = false;
            var callResult = new ViCellBlu.VcbResultRequestLock { ErrorLevel = ViCellBlu.ErrorLevelEnum.Warning, MethodResult = ViCellBlu.MethodResultEnum.Failure };

            var methodRequestLock = _methodCollection.First(n => n.DisplayName.ToString().Equals("RequestLock"));
            var methodNodeRequestLock = ExpandedNodeId.ToNodeId(methodRequestLock.NodeId, myUaClient.m_session.NamespaceUris);

            var requestHeader = new RequestHeader();
            var callMethodRequest = new CallMethodRequest
            {
                ObjectId = _parentMethodNode,
                MethodId = methodNodeRequestLock
            };
            var methodsToCall = new CallMethodRequestCollection { callMethodRequest };
            var responseHeader = myUaClient.m_session.Call(requestHeader, methodsToCall, out var results, out var diagnosticInfos);

            ClientBase.ValidateResponse(results, methodsToCall);
            ClientBase.ValidateDiagnosticInfos(diagnosticInfos, methodsToCall);

            if (ServiceResult.IsGood(responseHeader.ServiceResult))
            {
                foreach (var result in results)
                {
                    foreach (var outputArgument in result.OutputArguments)
                    {
                        callResult.ErrorLevel = ViCellBlu.ErrorLevelEnum.Warning;
                        callResult.MethodResult = ViCellBlu.MethodResultEnum.Failure;
                        callResult.ResponseDescription = "Decoding raw result ...";
                        try
                        {
                            var val = (Opc.Ua.ExtensionObject)outputArgument.Value;
                            var myData = (byte[])val.Body;
                            callResult.Decode(new Opc.Ua.BinaryDecoder(myData, 0, myData.Count(), myUaClient.m_session.MessageContext));
                        }
                        catch (Exception ex)
                        {
                            callResult.ErrorLevel = ViCellBlu.ErrorLevelEnum.RequiresUserInteraction;
                            callResult.MethodResult = ViCellBlu.MethodResultEnum.Failure;
                            callResult.ResponseDescription = "DecodeRaw-Exception: " + ex.ToString();
                        }
                        if (callResult.LockState == ViCellBlu.LockStateEnum.Locked) bReturn = true;
                    }
                }
            }
            foreach (var diagnosticInfo in diagnosticInfos) { }
            return bReturn;
        }

        private bool ReleaseLock()
        {
            bool bReturn = false;
            var callResult = new ViCellBlu.VcbResultReleaseLock { ErrorLevel = ViCellBlu.ErrorLevelEnum.Warning, MethodResult = ViCellBlu.MethodResultEnum.Failure };

            var methodReleaseLock = _methodCollection.First(n => n.DisplayName.ToString().Equals("ReleaseLock"));
            var methodNodeReleaseLock = ExpandedNodeId.ToNodeId(methodReleaseLock.NodeId, myUaClient.m_session.NamespaceUris);

            var requestHeader = new RequestHeader();
            var callMethodRequest = new CallMethodRequest
            {
                ObjectId = _parentMethodNode,
                MethodId = methodNodeReleaseLock
            };
            var methodsToCall = new CallMethodRequestCollection { callMethodRequest };
            var responseHeader = myUaClient.m_session.Call(requestHeader, methodsToCall, out var results, out var diagnosticInfos);
            ClientBase.ValidateResponse(results, methodsToCall);
            ClientBase.ValidateDiagnosticInfos(diagnosticInfos, methodsToCall);

            if (ServiceResult.IsGood(responseHeader.ServiceResult))
            {
                foreach (var result in results)
                {
                    foreach (var outputArgument in result.OutputArguments)
                    {
                        callResult.ErrorLevel = ViCellBlu.ErrorLevelEnum.Warning;
                        callResult.MethodResult = ViCellBlu.MethodResultEnum.Failure;
                        callResult.ResponseDescription = "Decoding raw result ...";
                        try
                        {
                            var val = (Opc.Ua.ExtensionObject)outputArgument.Value;
                            var myData = (byte[])val.Body;
                            callResult.Decode(new Opc.Ua.BinaryDecoder(myData, 0, myData.Count(), myUaClient.m_session.MessageContext));
                        }
                        catch (Exception ex)
                        {
                            callResult.ErrorLevel = ViCellBlu.ErrorLevelEnum.RequiresUserInteraction;
                            callResult.MethodResult = ViCellBlu.MethodResultEnum.Failure;
                            callResult.ResponseDescription = "DecodeRaw-Exception: " + ex.ToString();
                        }
                        if (callResult.LockState == ViCellBlu.LockStateEnum.Locked) bReturn = true;
                    }
                }
            }
            foreach (var diagnosticInfo in diagnosticInfos) { }
            return bReturn;
        }



        private DateTime GetLastEnqueuedDate()
        {
            DateTime LastEnqueuedDate=DateTime.MinValue;
            string strLastEnqueuedDate = m_sqlBiz.ReadSTCommon(m_Name, "LastEnqueuedDate"); //PC00U01.ReadConfigValue("LastEnqueuedDate", m_Name, $@".\CFG\{m_Type}.ini");
            DateTime.TryParseExact(strLastEnqueuedDate, "yyyyMMddHHmmss", CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal | DateTimeStyles.AllowInnerWhite, out LastEnqueuedDate);
            if (LastEnqueuedDate == DateTime.MinValue)
            {
                LastEnqueuedDate = DateTime.Now.Subtract(TimeSpan.FromDays(1));
                SetLastEnqueuedDate(LastEnqueuedDate);
            }
            listViewMsg.UpdateMsg($"Read last enqueued Date  : {LastEnqueuedDate.ToString("yyyyMMddHHmmss")}", false, true);
            listViewMsg.UpdateMsg($"m_LastEnqueuedDate :{LastEnqueuedDate.ToString("yyyyMMddHHmmss")} , {strLastEnqueuedDate}  !", false, true, true, PC00D01.MSGTINF);
            return LastEnqueuedDate;
        }
        private bool SetLastEnqueuedDate(DateTime LastEnqueuedDate)
        {
            //if (!PC00U01.WriteConfigValue("LastEnqueuedDate", m_Name, $@".\CFG\{m_Type}.ini", $"{LastEnqueuedDate.ToString("yyyyMMdd")}"))
            if (!m_sqlBiz.WriteSTCommon(m_Name, "LastEnqueuedDate", $"{LastEnqueuedDate.ToString("yyyyMMddHHmmss")}"))
            {
                listViewMsg.UpdateMsg($"Error to write LastEnqueuedDate to INI file", false, true);
                return false;
            }
            listViewMsg.UpdateMsg($"Write last LastEnqueuedDate : {LastEnqueuedDate.ToString("yyyyMMddHHmmss")}", false, true);
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
                //                myUaClient = new OpcUaClient.OpcUaClient(m_ConnectionInfo, m_Name);
                myUaClient = new OpcUaClient.OpcUaClient()
                {
                    endpointURL = m_ConnectionInfo,
                    //applicationName = "ViCELLBLU:Server",
                    applicationName = m_Name,
                    useridentity = new UserIdentity(Uid, Pwd),
                    applicationType = ApplicationType.Client,
                    subjectName = Utils.Format(@"CN=Vi-Cell BLU Client, C=US, S=Colorado, O=Beckman Coulter, DC={0}", Dns.GetHostName())
                };
                myUaClient.CreateConfig();
                myUaClient.CreateApplicationInstance();
                myUaClient.CreateSession();

                // Session/Subscription을 생성한 후
                myUaClient.CreateSubscription(1000);
                listViewMsg.UpdateMsg($"myUaClient.CreateSubscription ", false, true, true, PC00D01.MSGTINF);
                // CSV 파일에 있는 TagName, NodeId 리스트를 MonitoredItem으로 등록하고 
                ReadConfigInfo();
                myUaClient.UpateTagData += UpdateTagValue;
                myUaClient.LogMsgFunc += LogMsg; 

                listViewMsg.UpdateMsg($"myUaClient.UpateTagData ", false, true, true, PC00D01.MSGTINF);
                // currentSubscription에 대한 서비스를 등록한다.
                bool bReturn = myUaClient.AddSubscription();
                if (bReturn == false) myUaClient = null;
                listViewMsg.UpdateMsg($"{bReturn}= myUaClient.AddSubscription", false, true, true, PC00D01.MSGTINF);
                myUaClient.objectsFolderCollection = myUaClient.Browse(out _);

                viCellBlue = myUaClient.objectsFolderCollection.First(n => n.BrowseName.Name.Equals("ViCellBluStateObject"));
                viCellBlueRefs = myUaClient.Browse(out _, viCellBlue.NodeId);

                _browsedMethods = viCellBlueRefs.First(n => n.BrowseName.Name.Equals("Methods"));
                _methodCollection = myUaClient.Browse(out _, _browsedMethods.NodeId);
                _parentMethodNode = ExpandedNodeId.ToNodeId(_browsedMethods.NodeId, myUaClient.m_session.NamespaceUris);


            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
                listViewMsg.UpdateMsg($"Exceptioin - InitOpcUaClient ({ex})", false, true, true, PC00D01.MSGTERR);
                myUaClient = null;
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
                        m_OpcItemList[data[0]] = data[1];
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
                listViewMsg.UpdateMsg($"Exceptioin - ReadCsvFile ({ex})", false, true, true, PC00D01.MSGTERR);
            }
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
                    myUaClient.AddItem(data[0].Trim(), data[1].Trim());
                    m_OpcItemList.Add(data[0].Trim(), data[1].Trim());
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
                listViewMsg.UpdateMsg($"Exceptioin - ReadConfigInfo ({ex})", false, true, true, PC00D01.MSGTERR);
            }
        }

        public void UpdateTagValue( string tagname, string value, string datetime, string status)
        {
            m_OpcItemList[tagname] = value.Trim();
        }
    }
}
