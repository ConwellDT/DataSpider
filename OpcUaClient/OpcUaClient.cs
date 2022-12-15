using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Opc.Ua;   // Install-Package OPCFoundation.NetStandard.Opc.Ua
using Opc.Ua.Client;
using Opc.Ua.Configuration;
using System.Reflection;


namespace OpcUaClient
{

    public class OpcUaClient
    {
        /// <summary>
        /// The identifier for the Directory_Applications Object.
        /// </summary>
        private const uint GdsId_Directory_Applications = 586;

        private readonly object m_uaclientLock = new object();


        public Session m_session { get; set; }
        public SessionReconnectHandler m_reconnectHandler;
        private uint m_sessionTImeout = 60*1000;//1200*1000;
        public uint SessionTimeout
        {
            get { return m_sessionTImeout; }
            set {  m_sessionTImeout=value; }
        }


        public const int ReconnectPeriod = 10;

        public string endpointURL { get; set; }
        public string applicationName { get; set; }
        public ApplicationType applicationType { get; set; }
        public string subjectName { get; set; }
        bool SecurityEnabled { get; set; }
        ApplicationInstance applicationInstance = null;
        ApplicationConfiguration config = null;

        public Subscription m_subscription;
        public DateTime LastTimeSessionRenewed { get; set; }
        public DateTime LastTimeOPCServerFoundAlive { get; set; }
        public bool InitialisationCompleted { get; set; }

        bool autoAccept = true;

        public string msg = string.Empty;
        public delegate void UpdateTagDataDelegate(string tagname, string value, string datetime, string status);
        public event UpdateTagDataDelegate UpateTagData = null;

        public delegate void LogMsgDelegate(string msg);
        public LogMsgDelegate LogMsgFunc = null;


        public MonitoredItem m_monitoredItem =null;

        public bool bUseSourceTime = true;


        public UserIdentity useridentity = null;

        public ReferenceDescriptionCollection objectsFolderCollection = null;
        public ReferenceDescriptionCollection methodCollection = null;
        public ReferenceDescriptionCollection variableCollection = null;
        public ReferenceDescriptionCollection objectCollection = null;

        public DateTime m_ServerTimestamp;


        public OpcUaClient()
        {
        }

        public string AssemblyProduct
        {
            get
            {
                object[] attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyProductAttribute), false);
                if (attributes.Length == 0)
                {
                    return "DataSpider";
                }
                return ((AssemblyProductAttribute)attributes[0]).Product;
            }
        }

        public ApplicationConfiguration CreateConfig()
        {
            config = new ApplicationConfiguration()
            {
                ApplicationName = applicationName,
                ApplicationUri = Utils.Format($@"urn:{Dns.GetHostName()}:{applicationName}"),
                ApplicationType = ApplicationType.Client,
                SecurityConfiguration = new SecurityConfiguration
                {
                    ApplicationCertificate = new CertificateIdentifier
                    {

                        StoreType = @"Directory",
                        StorePath = $@"%CommonApplicationData%\{AssemblyProduct}\pki\own",
                        SubjectName = subjectName
                    },
                    TrustedIssuerCertificates = new CertificateTrustList
                    {
                        StoreType = @"Directory",
                        StorePath = $@"%CommonApplicationData%\{AssemblyProduct}\pki\issuer"
                    },
                    TrustedPeerCertificates = new CertificateTrustList
                    {
                        StoreType = @"Directory",
                        StorePath = $@"%CommonApplicationData%\{AssemblyProduct}\pki\trusted"
                    },
                    RejectedCertificateStore = new CertificateTrustList
                    {
                        StoreType = @"Directory",
                        StorePath = $@"%CommonApplicationData%\{AssemblyProduct}\pki\rejected"
                    },
                    AddAppCertToTrustedStore = true,
                    NonceLength = 32,
                    AutoAcceptUntrustedCertificates = autoAccept,
                    RejectSHA1SignedCertificates = false,
                    MinimumCertificateKeySize = 2048,
                    SendCertificateChain = true
                },
                TransportConfigurations = new TransportConfigurationCollection(),
                TransportQuotas = new TransportQuotas { OperationTimeout = 15000 },
                ClientConfiguration = new ClientConfiguration { DefaultSessionTimeout = 60000 },
                TraceConfiguration = new TraceConfiguration()
            };
            config.TransportQuotas = new TransportQuotas
            {
                OperationTimeout = 600000,
                MaxStringLength = 1048576,
                MaxByteStringLength = 1048576,
                MaxArrayLength = 65535,
                MaxMessageSize = 4194304,
                MaxBufferSize = 65535,
                ChannelLifetime = 300000,
                SecurityTokenLifetime = 3600000
            };

            config.Validate(ApplicationType.Client).GetAwaiter().GetResult();
            if (config.SecurityConfiguration.AutoAcceptUntrustedCertificates)
            {
                //config.CertificateValidator.CertificateValidation += (s, e) => { e.Accept = (e.Error.StatusCode == StatusCodes.BadCertificateUntrusted); };
                config.CertificateValidator.CertificateValidation += CertificateValidator_CertificateValidation;
            }
            return config;
        }

        public ApplicationInstance CreateApplicationInstance()
        {
            applicationInstance = new ApplicationInstance();
            applicationInstance.ApplicationName = applicationName;
            applicationInstance.ApplicationType = applicationType;
            applicationInstance.ApplicationConfiguration = config;
            bool haveAppCertificate= applicationInstance.CheckApplicationInstanceCertificate(true, 0, 120).GetAwaiter().GetResult();
            if(!haveAppCertificate)
            {
                LogMsg("Application instance certificate invalid!");
            }
            return applicationInstance;
        }



        public OpcUaClient(string _endpointURL, string _applicationName)
        {
            endpointURL = _endpointURL;
            applicationName = _applicationName;
            applicationType = ApplicationType.Client;
            //Console.WriteLine("Step 1 - Create application configuration and certificate.");
            config = CreateConfig();

            //config.Validate(ApplicationType.Client).GetAwaiter().GetResult();
            //if (config.SecurityConfiguration.AutoAcceptUntrustedCertificates)
            //{
            //    //config.CertificateValidator.CertificateValidation += (s, e) => { e.Accept = (e.Error.StatusCode == StatusCodes.BadCertificateUntrusted); };
            //    config.CertificateValidator.CertificateValidation += CertificateValidator_CertificateValidation;
            //}

            applicationInstance = CreateApplicationInstance();

            // Console.WriteLine($"Step 2 - Create a session with your server: {selectedEndpoint.EndpointUrl} ");
            m_session = CreateSession();
        }

        /// <summary>
        /// Handles a certificate validation error.
        /// </summary>
        private void CertificateValidator_CertificateValidation(CertificateValidator sender, CertificateValidationEventArgs e)
        {

            try
            {
                e.Accept = config.SecurityConfiguration.AutoAcceptUntrustedCertificates;
            }
            catch (Exception ex)
            {
                //MessageBox.Show(ex.ToString());
                //
            }
        }

        public void Close()
        {
            try
            {
                if (m_reconnectHandler != null)
                {
                    LogMsg("--- m_reconnectHandler not null ---");
                    if (m_reconnectHandler.Session.Connected)
                    {
                        m_reconnectHandler.Session.Close();
                        LogMsg("--- m_reconnectHandler.Session.Close ---");
                        m_reconnectHandler.Session.Dispose();
                    }
                    LogMsg("--- m_reconnectHandler.Session.Dispose ---");
                    m_reconnectHandler.Dispose();
                    m_reconnectHandler = null;
                    LogMsg("--- m_reconnectHandler   null ---");
                }
            }
            catch (Exception ex)
            {
                LogMsg(ex.Message);
            }

            try { 

                if (m_subscription != null)
                {
                    LogMsg("--- m_subscription not null ---");
                    m_subscription.StateChanged -= new SubscriptionStateChangedEventHandler(Subscription_StateChanged);
                    m_subscription.PublishStatusChanged -= new EventHandler(Subscription_PublishStatusChanged);
                    m_subscription.FastDataChangeCallback = null;
                    foreach ( MonitoredItem monitoredItem in m_subscription.MonitoredItems)
                    {
                        monitoredItem.Notification -= new MonitoredItemNotificationEventHandler(MonitoredItem_Notification);
                    }
                    if(m_subscription.Created)
                    {
                        m_subscription.Delete(true);
                    }
                    //m_subscription.Dispose();
                    m_subscription = null;
                    LogMsg("--- m_subscription null ---");
                }
            }
            catch (Exception ex)
            {
                LogMsg(ex.Message);
            }
            try
            {
                if (m_session != null)
                {
                    LogMsg("--- m_session not null ---");
                    m_session.KeepAlive -= new KeepAliveEventHandler(StandardClient_KeepAlive);
                    if (m_session.Connected)
                    {
                        m_session.Close();
                        LogMsg("---m_session.Close ---");
                        m_session.Dispose();
                    }
                    m_session = null;
                    LogMsg("--- m_session null ---");
                }
            }
            catch (Exception ex)
            {
                LogMsg(ex.Message);
            }

        }
        //class destructor
        ~OpcUaClient()
        {
            LogMsg("~OpcUaClient begin");
            Close();
            LogMsg("~OpcUaClient end");
        }

        public Session CreateSession()
        {
            // Console.WriteLine($"Step 2 - Create a session with your server: {selectedEndpoint.EndpointUrl} ");

            //Task<bool> haveAppCertificateTask = applicationInstance.CheckApplicationInstanceCertificate(true, 0);
            //bool haveAppCertificate = haveAppCertificateTask.Result;
            //if (haveAppCertificate)
            //    config.ApplicationUri = X509Utils.GetApplicationUriFromCertificate(config.SecurityConfiguration.ApplicationCertificate.Certificate);

            var selectedEndpoint = CoreClientUtils.SelectEndpoint(endpointURL, useSecurity: SecurityEnabled, discoverTimeout: 15000);
            var endpointConfiguration = EndpointConfiguration.Create(config);
            var endpoint = new ConfiguredEndpoint(null, selectedEndpoint, endpointConfiguration);

            m_session = Session.Create(config, endpoint, false, config.ApplicationName, SessionTimeout, useridentity, null).GetAwaiter().GetResult();
            m_session.KeepAliveInterval = 5000;
            m_session.KeepAlive += new KeepAliveEventHandler(StandardClient_KeepAlive);
            // 20221212, SHS, V.2.0.4.0, OPC 이벤트 삭제
            //m_session.Notification += new NotificationEventHandler(Session_Notification);


            //Task<Session> t= Session.Create(config, endpoint, false, config.ApplicationName, SessionTimeout, useridentity, null);
            //t.Start();
            //if (t.Wait(30000))
            //{
            //    m_session = t.Result;
            //    // register keep alive handler
            //    m_session.KeepAliveInterval = 5000;
            //    m_session.KeepAlive += new KeepAliveEventHandler(StandardClient_KeepAlive);
            //    // m_session.Notification += new NotificationEventHandler(Session_Notification);
            //}
            //else m_session.Dispose();


            return m_session;
        }

        public ReferenceDescriptionCollection Browse(out byte[] continuationPoint, NodeId nodeId = null)
        {
            if (null == nodeId)
            {
                nodeId = Opc.Ua.ObjectIds.ObjectsFolder;
            }

            if (null == m_session)
            {
                continuationPoint = new byte[1];
                return new ReferenceDescriptionCollection(1);
            }

            m_session.Browse(null, null, nodeId, 0u, BrowseDirection.Forward, ReferenceTypeIds.HierarchicalReferences,
                true,
                (uint)NodeClass.Variable | (uint)NodeClass.Object | (uint)NodeClass.Method, out continuationPoint,
                out var references);

            return references;
        }

        public ReferenceDescriptionCollection Browse(out byte[] continuationPoint, ExpandedNodeId expandedNodeId)
        {
            var nodeId = ExpandedNodeId.ToNodeId(expandedNodeId, m_session.NamespaceUris);
            return Browse(out continuationPoint, nodeId);
        }




        public void CreateSubscription(int _publishingInterval)
        {
            //Console.WriteLine("Step 4 - Create a subscription. Set a faster publishing interval if you wish.");
            m_subscription = new Subscription(m_session.DefaultSubscription) 
            { 
                PublishingInterval = _publishingInterval
            };

            // 20221212, SHS, V.2.0.4.0, OPC 이벤트 삭제
            //m_subscription.StateChanged += new SubscriptionStateChangedEventHandler(Subscription_StateChanged);
            //m_subscription.PublishStatusChanged += new EventHandler(Subscription_PublishStatusChanged);

            //m_subscription.FastDataChangeCallback = OnDataChange;
        }

        public MonitoredItem AddItem(string _displayName, string _nodeId)
        {
            MonitoredItem mitem = new MonitoredItem(m_subscription.DefaultItem)
            {
                DisplayName = _displayName,
                StartNodeId = _nodeId
            };
            mitem.Notification += new MonitoredItemNotificationEventHandler(MonitoredItem_Notification);
            m_subscription.AddItem(mitem);
            return mitem;
        }
  
        public void AddItem(MonitoredItem mitem)
        {
            mitem.Notification += new MonitoredItemNotificationEventHandler(MonitoredItem_Notification);
            m_subscription.AddItem(mitem);
        }


        /// <summary>
        /// Creates an item from a reference.
        /// </summary>
        public void AddItem(ReferenceDescription reference, string _displayName)
        {
            if (reference == null)
            {
                return;
            }
            // After AddSubscription
            //Node node = m_subscription.Session.NodeCache.Find(reference.NodeId) as Node;
            // BeFore AddSubscription
            Node node = m_session.NodeCache.Find(reference.NodeId) as Node;

            if (node == null)
            {
                return;
            }

            Node parent = null;

            // if the NodeId is of type string and contains '.' do not use relative paths
            if (node.NodeId.IdType != IdType.String || (node.NodeId.Identifier.ToString().IndexOf('.') == -1 && node.NodeId.Identifier.ToString().IndexOf('/') == -1))
            {
                parent = FindParent(node);
            }

            MonitoredItem monitoredItem = new MonitoredItem(m_subscription.DefaultItem);

            //if (parent != null)
            //{
            //    monitoredItem.DisplayName = String.Format("{0}.{1}", parent, node);
            //}
            //else
            //{
            //    monitoredItem.DisplayName = String.Format("{0}", node);
            //}
            monitoredItem.DisplayName = _displayName;

            monitoredItem.StartNodeId = node.NodeId;
            monitoredItem.NodeClass = node.NodeClass;

            if (parent != null)
            {
                List<Node> parents = new List<Node>();
                parents.Add(parent);

                while (parent.NodeClass != NodeClass.ObjectType && parent.NodeClass != NodeClass.VariableType)
                {
                    parent = FindParent(parent);

                    if (parent == null)
                    {
                        break;
                    }

                    parents.Add(parent);
                }

                monitoredItem.StartNodeId = parents[parents.Count - 1].NodeId;

                StringBuilder relativePath = new StringBuilder();

                for (int ii = parents.Count - 2; ii >= 0; ii--)
                {
                    relativePath.AppendFormat(".{0}", parents[ii].BrowseName);
                }

                relativePath.AppendFormat(".{0}", node.BrowseName);

                monitoredItem.RelativePath = relativePath.ToString();
            }

            Session session = m_subscription.Session;

            if (node.NodeClass == NodeClass.Object || node.NodeClass == NodeClass.Variable)
            {
                node.Find(ReferenceTypeIds.HasChild, true);

            }
            monitoredItem.Notification += new MonitoredItemNotificationEventHandler(MonitoredItem_Notification);
            m_subscription.AddItem(monitoredItem);
        }

        /// <summary>
        /// Returns the parent for the node.
        /// </summary>
        private Node FindParent(Node node)
        {
            // After AddSubscription
            //IList<IReference> parents = node.ReferenceTable.Find(ReferenceTypeIds.Aggregates, true, true, m_subscription.Session.TypeTree);
            // Before AddSubscription
            IList<IReference> parents = node.ReferenceTable.Find(ReferenceTypeIds.Aggregates, true, true, m_session.TypeTree);

            if (parents.Count > 0)
            {
                bool followToType = false;

                foreach (IReference parentReference in parents)
                {
                    // After AddSubscription
                    //                    Node parent = m_subscription.Session.NodeCache.Find(parentReference.TargetId) as Node;
                    // Before AddSubscription
                    Node parent = m_session.NodeCache.Find(parentReference.TargetId) as Node;

                    if (followToType)
                    {
                        if (parent.NodeClass == NodeClass.VariableType || parent.NodeClass == NodeClass.ObjectType)
                        {
                            return parent;
                        }
                    }
                    else
                    {
                        return parent;
                    }
                }
            }

            return null;
        }


        public void AddItems(List<MonitoredItem> mitems)
        {
            mitems.ForEach(Item => Item.Notification += new MonitoredItemNotificationEventHandler(MonitoredItem_Notification));
            m_subscription.AddItems(mitems);
        }
        public bool AddSubscription()
        {
            bool bReturn = false;
            bReturn = m_session.AddSubscription(m_subscription);
            if (bReturn == true) m_subscription.Create();
            return bReturn;
        }

        private void StandardClient_KeepAlive(Session sender, KeepAliveEventArgs e)
        {
            try
            {

                if (e.Status!=null)
                {
                    if(ServiceResult.IsNotGood(e.Status))
                    {
                        if (m_reconnectHandler == null)
                        {
                            m_reconnectHandler = new SessionReconnectHandler();
                            m_reconnectHandler.BeginReconnect(sender, ReconnectPeriod * 1000, StandardClient_Server_ReconnectComplete);
                            LogMsg("--- RECONNECTING ---");
                        }
                        LogMsg($"StandardClient_KeepAlive ServerStatus:{e.Status} {e.CurrentTime.ToLocalTime():HH:mm:ss} {sender.OutstandingRequestCount}/{sender.DefunctRequestCount}");
                    }
                }
            }
            catch(Exception ex)
            {
                LogMsg(ex.Message);
            }
        }

        private void StandardClient_Server_ReconnectComplete(object sender, EventArgs e)
        {
            try
            {
                LogMsg("--- ReconnectComplete---");
                // ignore callbacks from discarded objects.
                if (!Object.ReferenceEquals(sender, m_reconnectHandler))
                {
                    LogMsg("--- ignore callbacks from discarded objects. ---");
                    return;
                }
                m_session = m_reconnectHandler.Session;
                m_reconnectHandler.Dispose();
                m_reconnectHandler = null;

                LogMsg("--- RECONNECTED ---");
            }
            catch (Exception ex)
            {
                LogMsg("StandardClient_Server_ReconnectComplete :"+ex.Message);

            }
        }

        public void MonitoredItem_Notification(MonitoredItem item, MonitoredItemNotificationEventArgs e)
        {
            try
            {
                foreach (var value in item.DequeueValues())
                {
                    if (UpateTagData == null)
                    {
                        LogMsg("UpateTagData == null");
                        return;
                    }
                    if (value.Value == null)
                    {
                        LogMsg($"{item.DisplayName} value.Value == null");
                        continue;                           
                    }
                    string updatevalue = string.Empty;
                    if (value.WrappedValue.TypeInfo.BuiltInType == BuiltInType.DateTime)
                    {
                        DateTime datetime = (DateTime)value.WrappedValue.Value;
                        updatevalue = datetime.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ss.fffZ");
                    }
                    else if (value.WrappedValue.TypeInfo.BuiltInType == BuiltInType.String && value.WrappedValue.TypeInfo.ValueRank == 1)
                    {  // RankValue=1 :OneDimension (1): The value is an array with one dimension.
                       // RankValue=0 :OneOrMoreDimensions (0): The value is an array with one or more dimensions

                        StringBuilder buffer = new StringBuilder();
                        System.Collections.IEnumerable enumerable = value.WrappedValue.Value as System.Collections.IEnumerable;
                        if (enumerable != null)
                        {
                            foreach (object element in enumerable)
                            {
                                buffer.Append((string)element);
                            }
                        }
                        updatevalue = buffer.ToString();
                    }
                    else
                    {
                        updatevalue = value.WrappedValue.ToString();
                    }
                    if (bUseSourceTime)
                    {
                        UpateTagData(item.DisplayName, updatevalue, value.SourceTimestamp.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss.fff"), value.StatusCode.ToString());
                    }
                    else
                        UpateTagData(item.DisplayName, updatevalue, value.ServerTimestamp.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss.fff"), value.StatusCode.ToString());
                }
            }
            catch(Exception ex)
            {
                LogMsg("MonitoredItem_Notification:"+ex.Message);
            }
        }


        public void OnDataChange(Subscription subscription, DataChangeNotification notification, IList<string> stringTable)
        {

            try
            {
                foreach (MonitoredItemNotification itemNotification in notification.MonitoredItems)
                {
                    MonitoredItem monitoredItem = subscription.FindItemByClientHandle(itemNotification.ClientHandle);

                    if (monitoredItem == null)
                    {
                        continue;
                    }

                    DataValue value=itemNotification.Value;
                    string updatevalue = string.Empty;
                    if (value.WrappedValue.TypeInfo.BuiltInType == BuiltInType.DateTime)
                    {
                        DateTime datetime = (DateTime)value.WrappedValue.Value;
                        updatevalue = datetime.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ss.fffZ");
                    }
                    else if (value.WrappedValue.TypeInfo.BuiltInType == BuiltInType.String && value.WrappedValue.TypeInfo.ValueRank == 1)
                    {  // RankValue=1 :OneDimension (1): The value is an array with one dimension.
                       // RankValue=0 :OneOrMoreDimensions (0): The value is an array with one or more dimensions

                        StringBuilder buffer = new StringBuilder();
                        System.Collections.IEnumerable enumerable = value.WrappedValue.Value as System.Collections.IEnumerable;
                        if (enumerable != null)
                        {
                            foreach (object element in enumerable)
                            {
                                buffer.Append((string)element);
                            }
                        }
                        updatevalue = buffer.ToString();
                    }
                    else
                    {
                        updatevalue = value.WrappedValue.ToString();
                    }
                    if (bUseSourceTime)
                    {
                        UpateTagData(monitoredItem.DisplayName, updatevalue, value.SourceTimestamp.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss.fff"), value.StatusCode.ToString());
                    }
                    else
                        UpateTagData(monitoredItem.DisplayName, updatevalue, value.ServerTimestamp.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss.fff"), value.StatusCode.ToString());
                }
            }
            catch (Exception ex)
            {
                LogMsg("OnDataChange:"+ex.Message);
            }
        }


        public void Session_Notification(Session sender, NotificationEventArgs e)
        {
            try
            {
                //LogMsg($"Session_Notification");

                foreach (EventFieldList eventFields in e.NotificationMessage.GetEvents(true))
                {
                    MonitoredItem monitoredItem = m_subscription.FindItemByClientHandle(eventFields.ClientHandle);
                    LogMsg($"SN {monitoredItem.DisplayName} - {eventFields.Message}");
                }

                foreach (MonitoredItemNotification change in e.NotificationMessage.GetDataChanges(false))
                {
                    MonitoredItem monitoredItem = m_subscription.FindItemByClientHandle(change.ClientHandle);
                    DataValue dataValue = change.Value;
                    LogMsg($"SN {monitoredItem.DisplayName} - {dataValue.WrappedValue}");
                }
            }
            catch (Exception ex)
            {
                LogMsg("Session_Notification:"+ex.Message);
            }
        }

        /// <summary>
        /// Handles a change to the publish status for the subscription.
        /// </summary>
        public void Subscription_PublishStatusChanged(object subscription, EventArgs e)
        {
            try
            {
                NotificationMessage lastMessage = null;
                if (m_monitoredItem != null)
                {
                    MonitoredItem monitoredItem = m_subscription.FindItemByClientHandle(m_monitoredItem.ClientHandle);
                    if(monitoredItem?.LastValue != null)
                        lastMessage = monitoredItem.LastMessage;
                }
                if (m_subscription != null && m_subscription.PublishingStopped)
                {
                    LogMsg($"PSC m_subscription.PublishingEnabled {m_subscription.PublishingEnabled},  m_subscription.PublishingStopped {m_subscription.PublishingStopped} ! BadNoCommunication?");
                    //LogMsg($"PSC m_session.connected : {m_session.Connected}");
                }

                foreach (MonitoredItem monitoredItem in ((Subscription)subscription).MonitoredItems)
                {
                    if (ServiceResult.IsBad(monitoredItem.Status.Error))
                    {
                        LogMsg($" SSC {monitoredItem.Subscription.DisplayName} -  {m_monitoredItem.DisplayName} - {m_monitoredItem.Status.Error}");
                    }
                }


                if (lastMessage != null)
                {
                 //   LogMsg($"PublishTime:{lastMessage.PublishTime.ToLocalTime():HH:mm:ss} SequenceNumber :{lastMessage.SequenceNumber}");
                }

            }
            catch (Exception ex)
            {
                LogMsg("Subscription_PublishStatusChanged:"+ex.Message);
            }

        }

        /// <summary>
        /// Handles a change to the state of the subscription.
        /// </summary>
        public void Subscription_StateChanged(Subscription subscription, SubscriptionStateChangedEventArgs e)
        {
            try
            {
                LogMsg($"Subscription_StateChanged {e.Status}");

                if ((e.Status & SubscriptionChangeMask.ItemsDeleted) != 0)
                {
                    // 삭제 코드
                }

                foreach (MonitoredItem monitoredItem in subscription.MonitoredItems)
                {
                    if (ServiceResult.IsBad(monitoredItem.Status.Error))
                    {
                        LogMsg($" SSC {subscription.DisplayName} -  {m_monitoredItem.DisplayName} - {m_monitoredItem.Status.Error}");
                    }
                }
            }
            catch (Exception ex)
            {
                LogMsg(ex.Message);
            }
        }






        public DataValue ReadValue(NodeId nodeId)
        {
            try
            {
                return m_session.ReadValue(nodeId);
            }
            catch (Exception ex)
            {
                LogMsg(ex.Message);
            }
            return null;
        }
        public ServiceResult WriteValue(NodeId nodeId, Variant varvalue)
        {
            try
            {
                // build a list of values to write.
                WriteValueCollection nodesToWrite = new WriteValueCollection();
                nodesToWrite.Add(new WriteValue()
                {
                    NodeId = nodeId,
                    AttributeId = Attributes.Value,
                    Value = new DataValue() { WrappedValue = varvalue }
                });
                // read the attributes.
                StatusCodeCollection results = null;
                DiagnosticInfoCollection diagnosticInfos = null;

                ResponseHeader responseHeader = m_session.Write(
                    null,
                    nodesToWrite,
                    out results,
                    out diagnosticInfos);
                ClientBase.ValidateResponse(results, nodesToWrite);
                ClientBase.ValidateDiagnosticInfos(diagnosticInfos, nodesToWrite);
                if (StatusCode.IsBad(results[0])) return StatusCodes.Bad;
                else return StatusCodes.Good;
            }
            catch (Exception ex)
            {
                LogMsg(ex.Message);
            }
            return StatusCodes.Bad;
        }


        public StatusCodeCollection WriteValues(WriteValue[] writeValues)
        {
            try
            {
                // build a list of values to write.
                WriteValueCollection nodesToWrite = new WriteValueCollection();
                foreach (WriteValue wvalue in writeValues)
                {
                    nodesToWrite.Add(wvalue);
                }
                // read the attributes.
                StatusCodeCollection results = null;
                DiagnosticInfoCollection diagnosticInfos = null;

                ResponseHeader responseHeader = m_session.Write(
                    null,
                    nodesToWrite,
                    out results,
                    out diagnosticInfos);
                ClientBase.ValidateResponse(results, nodesToWrite);
                ClientBase.ValidateDiagnosticInfos(diagnosticInfos, nodesToWrite);

                return results;                

            }
            catch (Exception ex)
            {
                LogMsg(ex.Message);
            }
            return null;
        }

        public void LogMsg(string msg)
        {
            if (LogMsgFunc == null) return;
            LogMsgFunc(msg);
        }

    }
}