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

        string endpointURL { get; set; }
        int clientRunTime = Timeout.Infinite;
        bool SecurityEnabled { get; set; }
        ApplicationInstance applicationInstance = null;
        ApplicationConfiguration config = null;

        const int ReconnectPeriod = 10;
        public Session session { get; set; }
        public SessionReconnectHandler reconnectHandler;

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
        
        public OpcUaClient(string _endpointURL, string _applicationName)
        {
            endpointURL = _endpointURL;
            //Console.WriteLine("Step 1 - Create application configuration and certificate.");
            config = CreateConfig(_applicationName);

            config.Validate(ApplicationType.Client).GetAwaiter().GetResult();
            if (config.SecurityConfiguration.AutoAcceptUntrustedCertificates)
            {
                //config.CertificateValidator.CertificateValidation += (s, e) => { e.Accept = (e.Error.StatusCode == StatusCodes.BadCertificateUntrusted); };
                config.CertificateValidator.CertificateValidation += CertificateValidator_CertificateValidation;
            }

            applicationInstance = CreateApplicationInstance(_applicationName, ApplicationType.Client, config);

            applicationInstance.CheckApplicationInstanceCertificate(false, 2048).GetAwaiter().GetResult();

            // Console.WriteLine($"Step 2 - Create a session with your server: {selectedEndpoint.EndpointUrl} ");
            session = CreateSession();
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
                if (session != null)
                {
                    session.Close();
                    session.Dispose();
                    session = null;
                }
            }
            catch { }
        }

        public ApplicationInstance CreateApplicationInstance(string _applicationName, ApplicationType _applcationType, ApplicationConfiguration _config)
        {
            ApplicationInstance _applicationInstance = new ApplicationInstance();
            _applicationInstance.ApplicationName = _applicationName;
            _applicationInstance.ApplicationType = _applcationType;
            _applicationInstance.ApplicationConfiguration = _config;
            return _applicationInstance;
        }
        public ApplicationConfiguration CreateConfig(string _applicationName)
        {
            ApplicationConfiguration _config = new ApplicationConfiguration()
            {
                ApplicationName = _applicationName,
                ApplicationUri = Utils.Format(@"urn:{0}:" + _applicationName + "", Dns.GetHostName()),
                ApplicationType = ApplicationType.Client,
                SecurityConfiguration = new SecurityConfiguration
                {
                    ApplicationCertificate = new CertificateIdentifier { 
                        StoreType = @"Directory", 
                        StorePath = @"%CommonApplicationData%\OPC Foundation\CertificateStores\MachineDefault", 
                        SubjectName =_applicationName // Utils.Format(@"CN={0}, DC={1}", _applicationName, Dns.GetHostName()) 
                    },
                    TrustedIssuerCertificates = new CertificateTrustList { 
                        StoreType = @"Directory", 
                        StorePath = @"%CommonApplicationData%\OPC Foundation\CertificateStores\UA Certificate Authorities" 
                    },
                    TrustedPeerCertificates = new CertificateTrustList {
                        StoreType = @"Directory", 
                        StorePath = @"%CommonApplicationData%\OPC Foundation\CertificateStores\UA Applications" 
                    },
                    RejectedCertificateStore = new CertificateTrustList {
                        StoreType = @"Directory", 
                        StorePath = @"%CommonApplicationData%\OPC Foundation\CertificateStores\RejectedCertificates" 
                    },
                    AddAppCertToTrustedStore = true,
                    NonceLength = 32,
                    AutoAcceptUntrustedCertificates = autoAccept,
                    RejectSHA1SignedCertificates = false,
                    MinimumCertificateKeySize = 1024
                },
                TransportConfigurations = new TransportConfigurationCollection(),
                TransportQuotas = new TransportQuotas { OperationTimeout = 15000 },
                ClientConfiguration = new ClientConfiguration { DefaultSessionTimeout = 60000 },
                TraceConfiguration = new TraceConfiguration()
            };
            return _config;
        }

        public Session CreateSession()
        {
            // Console.WriteLine($"Step 2 - Create a session with your server: {selectedEndpoint.EndpointUrl} ");
            var selectedEndpoint = CoreClientUtils.SelectEndpoint(endpointURL, useSecurity: SecurityEnabled, discoverTimeout: 15000);
            var endpointConfiguration = EndpointConfiguration.Create(config);
            var endpoint = new ConfiguredEndpoint(null, selectedEndpoint, endpointConfiguration);

            session = Session.Create(config, endpoint, false, config.ApplicationName, 60000, null, null).GetAwaiter().GetResult();
            // register keep alive handler
            session.KeepAlive += Client_KeepAlive;
            return session;
        }

        public void CreateSubscription(int _publishingInterval)
        {
            //Console.WriteLine("Step 4 - Create a subscription. Set a faster publishing interval if you wish.");
            var _subscription = new Subscription(session.DefaultSubscription) { PublishingInterval = _publishingInterval };
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
            bReturn = session.AddSubscription(currentSubscription);
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
            session = reconnectHandler.Session;
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
                return session.ReadValue(nodeId);
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

                ResponseHeader responseHeader = session.Write(
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

                ResponseHeader responseHeader = session.Write(
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