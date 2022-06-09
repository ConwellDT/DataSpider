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
        private string Uid;
        private string Pwd;
        private bool m_bReconnected = false;

        private UInt32 m_dwCurrentSessionCount = 0;
        private UInt32 m_dwCurrentSubscriptionCount = 0;
        private UInt32 m_dwCummulatedSubscriptionCount = 0;

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
            int nLogCounter = 0;
            bool bCurrentSessionCountChanged = false;
            bool bCurrentSubscriptionCountChanged = false;
            bool bCummulatedSubscriptionCountChanged = false;

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
                        Thread.Sleep(1000);
                        dtNormalTime = DateTime.Now;
                        m_bReconnected = true;
                    }
                    else
                    {
                        if (myUaClient.m_reconnectHandler != null)
                        {
                            m_bReconnected = true;
                            //                            listViewMsg.UpdateMsg($" Network Error , IF_STATUS.Disconnected ", false, true, true, PC00D01.MSGTERR);
                            UpdateEquipmentProgDateTime(IF_STATUS.Disconnected);


                            if ((DateTime.Now - dtNormalTime).TotalMilliseconds >= myUaClient.m_session.SessionTimeout)
                            {
                                LogMsg("--- SessionTimeout---");

                                if (myUaClient.m_reconnectHandler.Session.Connected)
                                {
                                    //    //myUaClient.m_session = myUaClient.m_reconnectHandler.Session;
                                    myUaClient.m_reconnectHandler.Session.Close(5000);
                                    //    myUaClient.m_reconnectHandler.Session.Dispose();
                                    //    myUaClient.m_reconnectHandler.Dispose();
                                    //    myUaClient.m_reconnectHandler = null;
                                    LogMsg("--- Session.Close---");
                                }
                                dtNormalTime = DateTime.Now;
                            }

                            //if ((DateTime.Now - dtNormalTime).TotalMilliseconds >= myUaClient.m_session.SessionTimeout)
                            //{
                            //    listViewMsg.UpdateMsg($" Network Error , UaClient.Close() ", false, true, true, PC00D01.MSGTERR);
                            //    myUaClient.Close();
                            //    myUaClient = null;
                            //    listViewMsg.UpdateMsg($" Network Error , Ua Client Reset ", false, true, true, PC00D01.MSGTERR);
                            //}
                        }
                        else
                        {
                            UpdateEquipmentProgDateTime(IF_STATUS.Normal);
                            dtNormalTime = DateTime.Now;
                            bCurrentSessionCountChanged = CurrentSessionCountChanged();
                            bCurrentSubscriptionCountChanged = CurrentSubscriptionCountChanged();
                            bCummulatedSubscriptionCountChanged = CummulatedSubscriptionCountChanged();
                            //if (bCurrentSessionCountChanged || 
                            //    bCurrentSubscriptionCountChanged || 
                            //    bCummulatedSubscriptionCountChanged ||
                            if( myUaClient.m_subscription.CurrentPublishingEnabled == false || 
                                myUaClient.m_subscription.PublishingEnabled==false)
                            {
                                LogMsg($"---m_subscription.Delete/Create 1   CurrentPublishingEnabled ={myUaClient.m_subscription.CurrentPublishingEnabled}-PublishingEnabled ={myUaClient.m_subscription.PublishingEnabled}---");
                                //myUaClient.m_subscription.Delete(true);
                                //myUaClient.m_subscription.Create();
                                //myUaClient.m_subscription.PublishingEnabled = true;
                                myUaClient.m_subscription.SetPublishingMode(true);
                                LogMsg($"---m_subscription.Delete/Create 2   CurrentPublishingEnabled ={myUaClient.m_subscription.CurrentPublishingEnabled}-PublishingEnabled ={myUaClient.m_subscription.PublishingEnabled}---");
                                Thread.Sleep(1000);
                            }

                            m_ViCellStatus = m_OpcItemList["ViCellStatus"];
                            if (m_ViCellStatus == "0")
                            {
                                if ( bReadSampleResult() )  // reconnect, reidle
                                {
                                    if (RequestLock())
                                    {
                                        listViewMsg.UpdateMsg("RequestLock", false, true, true, PC00D01.MSGTINF);
                                        if ( GetSampleResult() ) // ReturnSuccess==true
                                        {
                                            if (m_bReconnected == true) m_bReconnected = false;
                                            m_PrevViCellStatus = m_ViCellStatus;
                                        }                                        
                                    }
                                }
                            }
                            else
                            {
                                m_PrevViCellStatus = m_ViCellStatus;
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
            myUaClient.Close();
            myUaClient = null;
            UpdateEquipmentProgDateTime(IF_STATUS.Stop);
            listViewMsg.UpdateStatus(false);
            listViewMsg.UpdateMsg("Thread finished");
        }

        public bool CurrentSessionCountChanged()
        {
            bool bReturn = false;
            UInt32 dwCurrentSessionCount = 0;
            try

            {
                if (string.IsNullOrEmpty(m_OpcItemList["CurrentSessionCount"])) return false;
                dwCurrentSessionCount = UInt32.Parse(m_OpcItemList["CurrentSessionCount"]);

                if (dwCurrentSessionCount != m_dwCurrentSessionCount)
                {
                    bReturn = true;
                    m_dwCurrentSessionCount = dwCurrentSessionCount;
                }
            }
            catch(Exception ex)
            {
                LogMsg($"CurrentSessionCountChanged -{ex.Message}");
            }
            return bReturn;
        }

        public bool CurrentSubscriptionCountChanged()
        {
            bool bReturn = false;
            UInt32 dwCurrentSubscriptionCount = 0;
            try
            {

                if (string.IsNullOrEmpty(m_OpcItemList["CurrentSubscriptionCount"])) return false;
                dwCurrentSubscriptionCount = UInt32.Parse(m_OpcItemList["CurrentSubscriptionCount"]);
                if (dwCurrentSubscriptionCount != m_dwCurrentSubscriptionCount)
                {
                    bReturn = true;
                    m_dwCurrentSubscriptionCount = dwCurrentSubscriptionCount;
                }
            }
            catch (Exception ex)
            {
                LogMsg($"CurrentSubscriptionCountChanged -{ex.Message}");
            }
            return bReturn;
        }
        public bool CummulatedSubscriptionCountChanged()
        {
            bool bReturn = false;
            UInt32 dwCummulatedSubscriptionCount = 0;

            try
            {
                if (string.IsNullOrEmpty(m_OpcItemList["CumulatedSubscriptionCount"])) return false;
                dwCummulatedSubscriptionCount = UInt32.Parse(m_OpcItemList["CumulatedSubscriptionCount"]);
                if (dwCummulatedSubscriptionCount != m_dwCummulatedSubscriptionCount)
                {
                    bReturn = true;
                    m_dwCummulatedSubscriptionCount = dwCummulatedSubscriptionCount;
                }
            }
            catch (Exception ex)
            {
                LogMsg($"CurrentSubscriptionCountChanged -{ex.Message}");
            }
            return bReturn;
        }


        private bool bReadSampleResult()
        {
            bool bReturn = false;
            if ( m_bReconnected == true ) bReturn = true;
            if ( m_ViCellStatus != m_PrevViCellStatus ) bReturn = true;
            return bReturn;
        }

        private bool GetSampleResult()
        {
            bool bReturnSuccess = false;
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
                                new Variant(DateTime.Now.AddHours(1)),                                  // end date
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
                ReleaseLock();
                listViewMsg.UpdateMsg("ReleaseLock", false, true, true, PC00D01.MSGTINF);
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
                                sampleResults.Sort(delegate (ViCellBlu.SampleResult x, ViCellBlu.SampleResult y)
                                {
                                    if (x.ReanalysisDateTime != DateTime.MinValue && y.ReanalysisDateTime != DateTime.MinValue)
                                    {
                                        if (x.ReanalysisDateTime == DateTime.MinValue && y.ReanalysisDateTime == DateTime.MinValue) return 0;
                                        else if (x.ReanalysisDateTime == DateTime.MinValue) return -1;
                                        else if (y.ReanalysisDateTime == DateTime.MinValue) return 1;
                                        else return x.ReanalysisDateTime.CompareTo(y.AnalysisDateTime);
                                    }
                                    else if (x.ReanalysisDateTime != DateTime.MinValue)
                                    {
                                        return x.ReanalysisDateTime.CompareTo(y.AnalysisDateTime);
                                    }
                                    else if (y.ReanalysisDateTime != DateTime.MinValue)
                                    {
                                        return x.AnalysisDateTime.CompareTo(y.ReanalysisDateTime);
                                    }
                                    else 
                                    {
                                        if (x.AnalysisDateTime == DateTime.MinValue && y.AnalysisDateTime == DateTime.MinValue) return 0;
                                        else if (x.AnalysisDateTime == DateTime.MinValue) return -1;
                                        else if (y.AnalysisDateTime == DateTime.MinValue) return 1;
                                        else return x.AnalysisDateTime.CompareTo(y.AnalysisDateTime);
                                    }
                                });
                                
                                foreach (var sampleResult in sampleResults)
                                {
                                    if (sampleResult.SampleDataUuid == Uuid.Empty) continue;

                                    DateTime svrtime;
                                    if(sampleResult.ReanalysisDateTime != DateTime.MinValue )
                                        svrtime = sampleResult.ReanalysisDateTime.ToLocalTime(); //  재분석 시간을 측정시간으로
                                    else
                                        svrtime = sampleResult.AnalysisDateTime.ToLocalTime(); // 측정시간
                                                                                          
                                    if (svrtime.CompareTo(m_LastEnqueuedDate) <= 0) continue;
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
                                                    DateTime dateTime = (DateTime)(property.GetValue(sampleResult));
                                                    EnQueue(MSGTYPE.MEASURE, $" {property.Name}, {svrtime:yyyy-MM-dd HH:mm:ss}, {dateTime.ToLocalTime():yyyy-MM-dd HH:mm:ss} ");
                                                    listViewMsg.UpdateMsg($" {property.Name}, {svrtime:yyyy-MM-dd HH:mm:ss}, {dateTime.ToLocalTime():yyyy-MM-dd HH:mm:ss} ", false, true, true, PC00D01.MSGTINF);
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

                    bReturnSuccess = true;
                }
            }
            catch(Exception ex)
            {
                listViewMsg.UpdateMsg($"Exception in GetSampleResult - ({ex})", false, true, true, PC00D01.MSGTERR);
            }
            return bReturnSuccess;
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
            listViewMsg.UpdateMsg($"Read last enqueued Date  : {LastEnqueuedDate.ToString("yyyyMMddHHmmss")}", false, true, true, PC00D01.MSGTINF);
            listViewMsg.UpdateMsg($"m_LastEnqueuedDate :{LastEnqueuedDate.ToString("yyyyMMddHHmmss")} , {strLastEnqueuedDate}  !", false, true, true, PC00D01.MSGTINF);
            return LastEnqueuedDate;
        }
        private bool SetLastEnqueuedDate(DateTime LastEnqueuedDate)
        {
            //if (!PC00U01.WriteConfigValue("LastEnqueuedDate", m_Name, $@".\CFG\{m_Type}.ini", $"{LastEnqueuedDate.ToString("yyyyMMdd")}"))
            if (!m_sqlBiz.WriteSTCommon(m_Name, "LastEnqueuedDate", $"{LastEnqueuedDate.ToString("yyyyMMddHHmmss")}"))
            {
                listViewMsg.UpdateMsg($"Error to write LastEnqueuedDate to INI file", false, true, true);
                return false;
            }
            listViewMsg.UpdateMsg($"Write last LastEnqueuedDate : {LastEnqueuedDate.ToString("yyyyMMddHHmmss")}", false, true,true, PC00D01.MSGTINF);
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


                myUaClient.objectsFolderCollection = myUaClient.Browse(out _);

                viCellBlue = myUaClient.objectsFolderCollection.First(n => n.BrowseName.Name.Equals("ViCellBluStateObject"));
                viCellBlueRefs = myUaClient.Browse(out _, viCellBlue.NodeId);

                _browsedMethods = viCellBlueRefs.First(n => n.BrowseName.Name.Equals("Methods"));
                _methodCollection = myUaClient.Browse(out _, _browsedMethods.NodeId);
                _parentMethodNode = ExpandedNodeId.ToNodeId(_browsedMethods.NodeId, myUaClient.m_session.NamespaceUris);


                // Session/Subscription을 생성한 후
                myUaClient.CreateSubscription(1000);
                listViewMsg.UpdateMsg($"myUaClient.CreateSubscription ", false, true, true, PC00D01.MSGTINF);
                // CSV 파일에 있는 TagName, NodeId 리스트를 MonitoredItem으로 등록하고 
                myUaClient.UpateTagData += UpdateTagValue;
                myUaClient.LogMsgFunc += LogMsg;

                ReadConfigInfo();

                listViewMsg.UpdateMsg($"myUaClient.UpateTagData ", false, true, true, PC00D01.MSGTINF);
                // currentSubscription에 대한 서비스를 등록한다.
                bool bReturn = myUaClient.AddSubscription();
                listViewMsg.UpdateMsg($"{bReturn}= myUaClient.AddSubscription", false, true, true, PC00D01.MSGTINF);
                if (bReturn == false)
                {
                    myUaClient.Close();
                    myUaClient = null;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
                listViewMsg.UpdateMsg($"Exceptioin - InitOpcUaClient ({ex})", false, true, true, PC00D01.MSGTERR);
                myUaClient.Close();
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
                m_OpcItemList.Clear();
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
                    if (data.Length < 2) continue;
                    if (string.IsNullOrWhiteSpace(data[0]) || string.IsNullOrWhiteSpace(data[1])) continue;
                    data[0] = data[0].Trim();
                    data[1] = data[1].Trim();
                    Opc.Ua.Client.MonitoredItem monitoreditem = myUaClient.AddItem(data[0], data[1]);
                    m_OpcItemList.Add(data[0], data[1]);

                }
                foreach(Opc.Ua.Client.MonitoredItem monitoreditem in  myUaClient.m_subscription.MonitoredItems )
                {                    
                   if (  monitoreditem.DisplayName == "ViCellStatus") myUaClient.m_monitoredItem = monitoreditem;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
                listViewMsg.UpdateMsg($"Exceptioin - ReadConfigInfo ({ex})", false, true, true, PC00D01.MSGTERR);
            }
        }

        public void UpdateTagValue(string tagname, string value, string datetime, string status)
        {
            m_OpcItemList[tagname] = value.Trim();
          //  if (tagname == "ViCellStatus")
            {
                listViewMsg.UpdateMsg($"{tagname} - {value}", false, true, true, PC00D01.MSGTINF);
            }
        }
    }
}
