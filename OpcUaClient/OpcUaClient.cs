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


namespace OpcUaClient
{

    public class OpcUaClient
    {
        public Session m_session { get; set; }
        public SessionReconnectHandler reconnectHandler;
        public const int ReconnectPeriod = 10;

        public ServiceMessageContext m_context;



        public string endpointURL { get; set; }
        public string applicationName { get; set; }
        public ApplicationType applicationType { get; set; }
        public string subjectName { get; set; }
        int clientRunTime = Timeout.Infinite;
        bool SecurityEnabled { get; set; }
        ApplicationInstance applicationInstance = null;
        ApplicationConfiguration config = null;

        Subscription currentSubscription;
        public DateTime LastTimeSessionRenewed { get; set; }
        public DateTime LastTimeOPCServerFoundAlive { get; set; }
        public bool ClassDisposing { get; set; }
        public bool InitialisationCompleted { get; set; }

        bool autoAccept = true;

        public string msg = string.Empty;
        public delegate void UpdateTagDataDelegate(string tagname, string value, string datetime, string status);
        public event UpdateTagDataDelegate UpateTagData = null;

        public delegate void LogMsgDelegate(string msg);
        public LogMsgDelegate LogMsgFunc = null;


        public bool bUseSourceTime = true;


        public UserIdentity useridentity = null;

        public ReferenceDescriptionCollection objectsFolderCollection = null;
        public ReferenceDescriptionCollection methodCollection = null;
        public ReferenceDescriptionCollection variableCollection = null;
        public ReferenceDescriptionCollection objectCollection = null;

        public OpcUaClient()
        {
        }

        public ApplicationConfiguration CreateConfig()
        {
            config = new ApplicationConfiguration()
            {
                ApplicationName = applicationName,
                ApplicationUri = Utils.Format(@"urn:{0}:" + applicationName + "", Dns.GetHostName()),
                ApplicationType = ApplicationType.Client,
                SecurityConfiguration = new SecurityConfiguration
                {
                    ApplicationCertificate = new CertificateIdentifier
                    {
                        StoreType = @"Directory",
                        StorePath = @"%CommonApplicationData%\OPC Foundation\CertificateStores\MachineDefault",
                        SubjectName = subjectName
                    },
                    TrustedIssuerCertificates = new CertificateTrustList
                    {
                        StoreType = @"Directory",
                        StorePath = @"%CommonApplicationData%\OPC Foundation\CertificateStores\UA Certificate Authorities"
                    },
                    TrustedPeerCertificates = new CertificateTrustList
                    {
                        StoreType = @"Directory",
                        StorePath = @"%CommonApplicationData%\OPC Foundation\CertificateStores\UA Applications"
                    },
                    RejectedCertificateStore = new CertificateTrustList
                    {
                        StoreType = @"Directory",
                        StorePath = @"%CommonApplicationData%\OPC Foundation\CertificateStores\RejectedCertificates"
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
            applicationInstance.CheckApplicationInstanceCertificate(false, 2048).GetAwaiter().GetResult();
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
            catch (Exception exception)
            {
                //MessageBox.Show(exception.ToString());
                //
            }
        }
        //class destructor
        ~OpcUaClient()
        {
            ClassDisposing = true;
            try
            {
                if (m_session != null)
                {
                    m_session.Close();
                    m_session.Dispose();
                    m_session = null;
                }
            }
            catch { }
        }

        public Session CreateSession()
        {
            // Console.WriteLine($"Step 2 - Create a session with your server: {selectedEndpoint.EndpointUrl} ");
            Task<bool> haveAppCertificateTask = applicationInstance.CheckApplicationInstanceCertificate(false, 0);
            bool haveAppCertificate = haveAppCertificateTask.Result;
            if (haveAppCertificate)
                config.ApplicationUri = X509Utils.GetApplicationUriFromCertificate(config.SecurityConfiguration.ApplicationCertificate.Certificate);

            var selectedEndpoint = CoreClientUtils.SelectEndpoint(endpointURL, useSecurity: SecurityEnabled, discoverTimeout: 15000);
            var endpointConfiguration = EndpointConfiguration.Create(config);
            var endpoint = new ConfiguredEndpoint(null, selectedEndpoint, endpointConfiguration);

            m_session = Session.Create(config, endpoint, false, config.ApplicationName, 60000, useridentity, null).GetAwaiter().GetResult();
            // register keep alive handler
            m_session.KeepAlive += Client_KeepAlive;
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
            var _subscription = new Subscription(m_session.DefaultSubscription) { PublishingInterval = _publishingInterval };
            currentSubscription = _subscription;
        }

        public void AddItem(string _displayName, string _nodeId)
        {
            MonitoredItem mitem = new MonitoredItem(currentSubscription.DefaultItem) { DisplayName = _displayName, StartNodeId = _nodeId };
            mitem.Notification += OnNotification;
            currentSubscription.AddItem(mitem);

        }
        public void AddItem(MonitoredItem mitem)
        {
            mitem.Notification += OnNotification;
            currentSubscription.AddItem(mitem);
        }
        public void AddItems(List<MonitoredItem> mitems)
        {
            mitems.ForEach(Item => Item.Notification += OnNotification);
            currentSubscription.AddItems(mitems);
        }
        public bool AddSubscription()
        {
            bool bReturn = false;
            bReturn = m_session.AddSubscription(currentSubscription);
            if (bReturn == true) currentSubscription.Create();
            return bReturn;
        }

        private void Client_KeepAlive(Session sender, KeepAliveEventArgs e)
        {
            if (e.Status != null && ServiceResult.IsNotGood(e.Status))
            {
                LogMsg($"Client_KeepAlive {e.Status} {sender.OutstandingRequestCount}/{sender.DefunctRequestCount}");

                if (reconnectHandler == null)
                {
                    LogMsg("--- RECONNECTING ---");
                    reconnectHandler = new SessionReconnectHandler();
                    reconnectHandler.BeginReconnect(sender, ReconnectPeriod * 1000, Client_ReconnectComplete);
                }
            }
        }

        private void Client_ReconnectComplete(object sender, EventArgs e)
        {
            // ignore callbacks from discarded objects.
            if (!Object.ReferenceEquals(sender, reconnectHandler))
            {
                LogMsg("--- ignore callbacks from discarded objects. ---");
                return;
            }
            m_session = reconnectHandler.Session;
            reconnectHandler.Dispose();
            reconnectHandler = null;
            LogMsg("--- RECONNECTED ---");
        }
        public void OnNotification(MonitoredItem item, MonitoredItemNotificationEventArgs e)
        {
            foreach (var value in item.DequeueValues())
            {
                if (UpateTagData == null) return;
                if (value.Value == null) continue;
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

        public DataValue ReadValue(NodeId nodeId)
        {
            try
            {
                return m_session.ReadValue(nodeId);
            }
            catch (Exception exception)
            {
                //
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
            catch (Exception exception)
            {
                //HandleException("WriteValue", exception);
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
            catch (Exception exception)
            {
                //HandleException("WriteValue", exception);
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