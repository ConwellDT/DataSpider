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
using System.Xml;
using Opc.Ua;

namespace DataSpider.PC01.PT
{
    /// <summary>
    /// Equip_Type : OPC UA Server
    /// </summary>
    public class PC01S16 : PC00B01
    {
        OpcUaClient.OpcUaClient myUaClient=null;

        IDictionary<string, string> m_BubblePoint = new Dictionary<string, string>();
        IDictionary<string, string> m_FlowCheck = new Dictionary<string, string>();
        IDictionary<string, string> m_ForwardFlow = new Dictionary<string, string>();
        IDictionary<string, string> m_SelfTest = new Dictionary<string, string>();
        IDictionary<string, string> m_WaterIntrusion = new Dictionary<string, string>();
        IDictionary<string, string> m_PressureDecay = new Dictionary<string, string>();

        private DateTime m_LastWriteTime = DateTime.MinValue;
        private UInt32 m_LastEnqueuedRecord = 0;
        private UInt32 m_CurrentRecord = 0;
        private UInt32 m_FromRecord = 0;
        private UInt32 m_ToRecord = 0;
        private DateTime dtDateTime;
        private string dataString = string.Empty;
        private string typeName = "MSR";
        private StringBuilder ssb = new StringBuilder();
        private DateTime dtNormalTime=DateTime.Now;
        private string Uid;
        private string Pwd;

        public PC01S16() : base()
        {
        }

        public PC01S16(PC01F01 pOwner, string equipType, string equipName, string connectionInfo, string extraInfo, int nCurNo, bool bAutoRun = false) : base(pOwner, equipType, equipName, connectionInfo, extraInfo, nCurNo, bAutoRun)
        {
        }

        public PC01S16(PC01F01 pOwner, DataRow dr, int nCurNo, bool bAutoRun = false) : this(pOwner, dr["EQUIP_TYPE_NM"].ToString(), dr["EQUIP_NM"].ToString(), dr["CONNECTION_INFO"].ToString(), dr["EXTRA_INFO"].ToString(), nCurNo, bAutoRun)
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
                listViewMsg.UpdateMsg($"Exceptioin - PC01S08 ({ex})", false, true, true, PC00D01.MSGTERR);
            }
        }

        private void ThreadJob()
        {
            listViewMsg.UpdateStatus(true);
            listViewMsg.UpdateMsg("Thread started");
            UpdateEquipmentProgDateTime(IF_STATUS.Normal);
            m_LastEnqueuedRecord = GetLastEnqueuedRecord();
            m_FromRecord = GetFromRecord();
            m_ToRecord = GetToRecord();
            m_LastWriteTime = GetConfigLastWriteTime();
            listViewMsg.UpdateMsg($"Read From Ini File m_LastEnqueuedRecord :{m_LastEnqueuedRecord}", false, true, true, PC00D01.MSGTINF);
            listViewMsg.UpdateMsg($"Read From Ini File m_FromRecord :{m_FromRecord}", false, true, true, PC00D01.MSGTINF);
            listViewMsg.UpdateMsg($"Read From Ini File m_ToRecord :{m_ToRecord}", false, true, true, PC00D01.MSGTINF);
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

                        // Read Configuration
                        ReadCfg();
                    }
                    else
                    {
                        // 20221212, SHS, V.2.0.4.0, ReconnectHandler 동작 확인 부분 추가
                        if (myUaClient.m_reconnectHandler != null)
                        {
                            UpdateEquipmentProgDateTime(IF_STATUS.Disconnected);
                        }
                        else
                        {
                            DataValue LastTestCompleted = myUaClient.ReadValue(new NodeId("ns=2;s=LastTestCompleted"));
                            // TESTTEST
                            //DataValue LastTestCompleted = myUaClient.ReadValue(new NodeId("ns=2;s=Channel1.Device1.LastTestCompleted"));
                            //                        listViewMsg.UpdateMsg($"LastTestCompleted :{LastTestCompleted.Value.ToString() }  !", false, true, true, PC00D01.MSGTINF);
                            // 최근 검사완료 번호를 못받음
                            if (LastTestCompleted == null)
                            {
                                // 20221212, SHS, V.2.0.4.0, OPC 초기화 부분 삭제
                                listViewMsg.UpdateMsg($"LastTestCompleted is null", false, true, true, PC00D01.MSGTERR);
                                //if ((DateTime.Now - dtNormalTime).TotalHours < 1)
                                //{
                                //    //UpdateEquipmentProgDateTime(IF_STATUS.InvalidData);
                                //    UpdateEquipmentProgDateTime(IF_STATUS.NetworkError);
                                //}
                                //else
                                //{
                                //    myUaClient = null;
                                //    listViewMsg.UpdateMsg($" Network Error Time > 1 Hr, Ua Client Reset ", false, true, true, PC00D01.MSGTERR);
                                //}

                                // 20221212, SHS, V.2.0.4.0, OPC 초기화 부분 삭제 하고 LastTestCompleted = null 이면 Disconnected 상태 업데이트
                                UpdateEquipmentProgDateTime(IF_STATUS.Disconnected);

                                Thread.Sleep(10 * 1000);
                                continue;
                            }
                            // 20221212, SHS, V.2.0.4.0, LastTestCompleted 값 로그
                            listViewMsg.UpdateMsg($"LastTestCompleted : {LastTestCompleted}", false, true, true, PC00D01.MSGTINF);

                            dtNormalTime = DateTime.Now;
                            // 최근 처리한 데이터 이후 데이터 수신
                            // 현재 값과 이전 값 만 가지고 처리
                            //      이전 값                현재 값
                            m_CurrentRecord = (UInt32)LastTestCompleted.Value;
                            if (m_LastEnqueuedRecord != m_CurrentRecord)
                            {
                                listViewMsg.UpdateMsg($"Current m_LastEnqueuedRecord :{m_LastEnqueuedRecord}", false, true, true, PC00D01.MSGTINF);
                                listViewMsg.UpdateMsg($"Received LastTestCompleted :{m_CurrentRecord}", false, true, true, PC00D01.MSGTINF);

                                if (ProcessMethod(m_CurrentRecord) == true)
                                {
                                    listViewMsg.UpdateMsg($" ProcessMethod({m_CurrentRecord})-OK ", false, true, true, PC00D01.MSGTINF);
                                    m_LastEnqueuedRecord = m_CurrentRecord;
                                    SetLastEnqueuedRecord(m_LastEnqueuedRecord);
                                }
                                else
                                {
                                    listViewMsg.UpdateMsg($" ProcessMethod({m_CurrentRecord})- NG ", false, true, true, PC00D01.MSGTINF);
                                }
                            }
                            else
                            {// m_LastEnqueuedRecord == m_CurrentRecord  => Nothing To Do => FromRecord/ToRecord 처리
                                if (m_FromRecord + m_ToRecord > 0)
                                {
                                    listViewMsg.UpdateMsg($"From Record  :{m_FromRecord}", false, true, true, PC00D01.MSGTINF);
                                    listViewMsg.UpdateMsg($"To Record  :{m_ToRecord}", false, true, true, PC00D01.MSGTINF);

                                    if (ProcessMethod(m_FromRecord) == true)
                                    {
                                        listViewMsg.UpdateMsg($" ProcessMethod({m_FromRecord})-OK ", false, true, true, PC00D01.MSGTINF);
                                        m_FromRecord++;
                                        if (m_FromRecord > m_ToRecord)
                                        {
                                            m_FromRecord = m_ToRecord = 0;
                                            SetToRecord(m_ToRecord);
                                        }
                                        SetFromRecord(m_FromRecord);
                                    }
                                    else
                                    {
                                        listViewMsg.UpdateMsg($" ProcessMethod({m_FromRecord})- NG ", false, true, true, PC00D01.MSGTINF);
                                    }
                                }
                                else
                                {// FromRecord/ToRecord 처리할 것도 없음. 
                                    if (m_LastWriteTime != GetConfigLastWriteTime())
                                    {
                                        m_FromRecord = GetFromRecord();
                                        m_ToRecord = GetToRecord();
                                        m_LastWriteTime = GetConfigLastWriteTime();
                                    }
                                    Thread.Sleep(10 * 1000);

                                }
                                UpdateEquipmentProgDateTime(IF_STATUS.Normal);
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

        private bool ProcessMethod(UInt32 ToBeProcessedRecord)
        {
            IList<object> outputArguments=null;

            try
            {
                outputArguments = myUaClient.m_session.Call(new NodeId("ns=2;s=Automation"),
                                                                        new NodeId("ns=2;s=RequestTestRecord"),
                                                                        ToBeProcessedRecord
                                                                        );
            }
            catch (Opc.Ua.ServiceResultException ex)
            {
                listViewMsg.UpdateMsg($"Exception in OPC Call - {ex.Message}", false, true, true, PC00D01.MSGTINF);
                UpdateEquipmentProgDateTime(IF_STATUS.NetworkError);
                if (ex.Message.Contains("BadNoData")) return true;
                return false;
            }
            if (outputArguments != null && outputArguments.Count >= 1)
            {
                fileLog.WriteData(outputArguments[0].ToString(), "RequestTestRecord", "ThreadJob");

                string TestName = GetData(outputArguments[0].ToString(), "/TestResult/@TestName");

                switch (TestName)
                {
                    case "Bubble Point":
                        dataString = GetData(outputArguments[0].ToString(), m_BubblePoint["DateTime"]);
                        PC00U01.TryParseExact(dataString, out dtDateTime);
                        ssb.Clear();
                        ssb.AppendLine($"{typeName}_SVRTIME, {dtDateTime:yyyy-MM-dd HH:mm:ss}, {DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}");
                        foreach (KeyValuePair<string, string> kvp in m_BubblePoint)
                        {
                            dataString = GetData(outputArguments[0].ToString(), kvp.Value);
                            ssb.AppendLine($"{typeName}_{kvp.Key.ToUpper()}, {dtDateTime:yyyy-MM-dd HH:mm:ss}, {dataString}");
                        }
                        EnQueue(MSGTYPE.MEASURE, ssb.ToString());
                        break;
                    case "Flow Check":
                        dataString = GetData(outputArguments[0].ToString(), m_FlowCheck["DateTime"]);
                        PC00U01.TryParseExact(dataString, out dtDateTime);
                        ssb.Clear();
                        ssb.AppendLine($"{typeName}_SVRTIME, {dtDateTime:yyyy-MM-dd HH:mm:ss}, {DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}");
                        foreach (KeyValuePair<string, string> kvp in m_FlowCheck)
                        {
                            dataString = GetData(outputArguments[0].ToString(), kvp.Value);
                            ssb.AppendLine($"{typeName}_{kvp.Key.ToUpper()}, {dtDateTime:yyyy-MM-dd HH:mm:ss}, {dataString}");
                        }
                        EnQueue(MSGTYPE.MEASURE, ssb.ToString());
                        break;
                    case "Forward Flow":
                        dataString = GetData(outputArguments[0].ToString(), m_ForwardFlow["DateTime"]);
                        PC00U01.TryParseExact(dataString, out dtDateTime);
                        ssb.Clear();
                        ssb.AppendLine($"{typeName}_SVRTIME, {dtDateTime:yyyy-MM-dd HH:mm:ss}, {DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}");
                        foreach (KeyValuePair<string, string> kvp in m_ForwardFlow)
                        {
                            dataString = GetData(outputArguments[0].ToString(), kvp.Value);
                            ssb.AppendLine($"{typeName}_{kvp.Key.ToUpper()}, {dtDateTime:yyyy-MM-dd HH:mm:ss}, {dataString}");
                        }
                        EnQueue(MSGTYPE.MEASURE, ssb.ToString());
                        break;
                    case "Self Test":
                        dataString = GetData(outputArguments[0].ToString(), m_SelfTest["DateTime"]);
                        PC00U01.TryParseExact(dataString, out dtDateTime);
                        ssb.Clear();
                        ssb.AppendLine($"{typeName}_SVRTIME, {dtDateTime:yyyy-MM-dd HH:mm:ss}, {DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}");
                        foreach (KeyValuePair<string, string> kvp in m_SelfTest)
                        {
                            dataString = GetData(outputArguments[0].ToString(), kvp.Value);
                            ssb.AppendLine($"{typeName}_{kvp.Key.ToUpper()}, {dtDateTime:yyyy-MM-dd HH:mm:ss}, {dataString}");
                        }
                        EnQueue(MSGTYPE.MEASURE, ssb.ToString());
                        break;
                    case "Water Intrusion":
                        dataString = GetData(outputArguments[0].ToString(), m_WaterIntrusion["DateTime"]);
                        PC00U01.TryParseExact(dataString, out dtDateTime);
                        ssb.Clear();
                        ssb.AppendLine($"{typeName}_SVRTIME, {dtDateTime:yyyy-MM-dd HH:mm:ss}, {DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}");
                        foreach (KeyValuePair<string, string> kvp in m_WaterIntrusion)
                        {
                            dataString = GetData(outputArguments[0].ToString(), kvp.Value);
                            ssb.AppendLine($"{typeName}_{kvp.Key.ToUpper()}, {dtDateTime:yyyy-MM-dd HH:mm:ss}, {dataString}");
                        }
                        EnQueue(MSGTYPE.MEASURE, ssb.ToString());
                        break;
                    case "Pressure Decay":
                        dataString = GetData(outputArguments[0].ToString(), m_PressureDecay["DateTime"]);
                        PC00U01.TryParseExact(dataString, out dtDateTime);
                        ssb.Clear();
                        ssb.AppendLine($"{typeName}_SVRTIME, {dtDateTime:yyyy-MM-dd HH:mm:ss}, {DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}");
                        foreach (KeyValuePair<string, string> kvp in m_PressureDecay)
                        {
                            dataString = GetData(outputArguments[0].ToString(), kvp.Value);
                            ssb.AppendLine($"{typeName}_{kvp.Key.ToUpper()}, {dtDateTime:yyyy-MM-dd HH:mm:ss}, {dataString}");
                        }
                        EnQueue(MSGTYPE.MEASURE, ssb.ToString());
                        break;
                    default:
                        listViewMsg.UpdateMsg($"Unknown Test Name !", false, true, true, PC00D01.MSGTERR);
                        break;
                }
            }
            return true;
        }

        private DateTime GetConfigLastWriteTime()
        {
            // 20221212, SHS, V.2.0.4.0, 수동 레코드 설정 부분, 디렉토리 경로 별도 변수 할당
            //string path = $@".\CFG\{m_Type}_MANL.ini";
            string dirPath = $@".\CFG";
            string path = dirPath + $@"\{m_Type}_MANL.ini";
            DateTime dtReturn = DateTime.MinValue;
            try
            {
                if (!File.Exists(path))
                {
                    // 20221212, SHS, V.2.0.4.0, 디렉토리가 없으면 생성
                    if (!Directory.Exists(dirPath))  Directory.CreateDirectory(dirPath);

                    File.Create(path);
                    SetFromRecord(m_FromRecord);
                    SetToRecord(m_ToRecord);
                }
                dtReturn = File.GetLastWriteTime(path);
            }
            catch (Exception e)
            {
                listViewMsg.UpdateMsg($"GetConfigLastWriteTime Error : {e.ToString()}", false, true);               
            }
            return dtReturn;
        }
        private UInt32 GetLastEnqueuedRecord()
        {
            UInt32 retVal =0;
            string dtString = m_sqlBiz.ReadSTCommon(m_Name, "LastEnqueuedRecord"); //PC00U01.ReadConfigValue("LastEnqueuedRecord", m_Name, $@".\CFG\{m_Type}.ini");
            UInt32.TryParse(dtString, out retVal);
            listViewMsg.UpdateMsg($"Read last enqueued Record  : {retVal.ToString()}", false, true);

            listViewMsg.UpdateMsg($"m_LastEnqueuedRecord :{m_LastEnqueuedRecord.ToString() } , {dtString}  !", false, true, true, PC00D01.MSGTINF);

            return retVal;
        }
        private bool SetLastEnqueuedRecord(UInt32 lastEnqueuedRecord)
        {
            //if (!PC00U01.WriteConfigValue("LastEnqueuedRecord", m_Name, $@".\CFG\{m_Type}.ini", $"{lastEnqueuedRecord.ToString()}"))
            if (!m_sqlBiz.WriteSTCommon(m_Name, "LastEnqueuedRecord", $"{lastEnqueuedRecord.ToString()}"))
            {
                listViewMsg.UpdateMsg($"Error to write LastEnqueuedRecord to INI file", false, true);
                return false;
            }
            listViewMsg.UpdateMsg($"Write last LastEnqueuedRecord : {lastEnqueuedRecord.ToString()}", false, true);
            return true;
        }


        private UInt32 GetFromRecord()
        {
            UInt32 retVal = 0;
            string dtString = m_sqlBiz.ReadSTCommon(m_Name, "FromRecord"); //PC00U01.ReadConfigValue("FromRecord", m_Name, $@".\CFG\{m_Type}_MANL.ini");
            UInt32.TryParse(dtString, out retVal);
            listViewMsg.UpdateMsg($"Read FromRecord : {retVal.ToString()}", false, true);
            listViewMsg.UpdateMsg($"FromRecord :{m_FromRecord.ToString() } , {dtString}  !", false, true, true, PC00D01.MSGTINF);

            return retVal;
        }
        private bool SetFromRecord(UInt32 fromRecord)
        {
            //if (!PC00U01.WriteConfigValue("FromRecord", m_Name, $@".\CFG\{m_Type}_MANL.ini", $"{fromRecord.ToString()}"))
            if (!m_sqlBiz.WriteSTCommon(m_Name, "FromRecord", $"{fromRecord.ToString()}"))
            {
                listViewMsg.UpdateMsg($"Error to write FromRecord to INI file", false, true);
                return false;
            }
            listViewMsg.UpdateMsg($"Write last fromRecord : {fromRecord.ToString()}", false, true);
            return true;
        }

        private UInt32 GetToRecord()
        {
            UInt32 retVal = 0;
            string dtString = m_sqlBiz.ReadSTCommon(m_Name, "ToRecord"); //PC00U01.ReadConfigValue("ToRecord", m_Name, $@".\CFG\{m_Type}_MANL.ini");
            UInt32.TryParse(dtString, out retVal);
            listViewMsg.UpdateMsg($"Read ToRecord : {retVal.ToString()}", false, true);
            listViewMsg.UpdateMsg($"ToRecord :{m_FromRecord.ToString() } , {dtString}  !", false, true, true, PC00D01.MSGTINF);

            return retVal;
        }
        private bool SetToRecord(UInt32 toRecord)
        {
            //if (!PC00U01.WriteConfigValue("ToRecord", m_Name, $@".\CFG\{m_Type}_MANL.ini", $"{toRecord.ToString()}"))
            if (!m_sqlBiz.WriteSTCommon(m_Name, "ToRecord", $"{toRecord.ToString()}"))
            {
                listViewMsg.UpdateMsg($"Error to write ToRecord to INI file", false, true);
                return false;
            }
            listViewMsg.UpdateMsg($"Write last ToRecord : {toRecord.ToString()}", false, true);
            return true;
        }

        string GetData(string xmlData, string xPath)
        {
            string retString = string.Empty;
            XmlDocument xmldoc = new XmlDocument();
            xmldoc.LoadXml(xmlData);
            //XmlNode xmlNode = xmldoc.SelectSingleNode(xPath);
            XmlNodeList nodeList = xmldoc.SelectNodes(xPath);
            
            // 2022-10-13, V.2.0.2.0
            // SHS, 멀티라인 데이터의 경우 " ;" 로 구분 -> 3공장처럼 " " 로 구분 처리. 마지막 ; 지우는 로직도 3공장처럼 삭제
            //foreach (XmlNode xmlNode in nodeList)
            //{
            //    retString += xmlNode.Value + " ;";
            //}
            //if(retString.Length>0)
            //    retString= retString.Substring(0, retString.Length - 1);
            foreach (XmlNode xmlNode in nodeList)
            {
                retString += xmlNode.Value + " ";
            }

            //retString = xmlNode?.Value;
            retString = retString.Replace((char)ASCII.CR, ' ');
            retString=retString.Replace((char)ASCII.LF, ' ');
            return retString.Trim();
        }
        void ReadCfgData()
        {
            string path;
            string line;
            path = $@".\Cfg\{m_Type}_BubblePoint.csv";
            m_BubblePoint.Clear();
            using (StreamReader file = new StreamReader(path))
            {
                while ((line = file.ReadLine()) != null)
                {
                    string[] spline = line.Split(',');
                    if (spline.Length > 1)
                    {
                        m_BubblePoint.Add(spline[0].Trim(), spline[1].Trim());
                    }
                }
            }
            path = $@".\Cfg\{m_Type}_FlowCheck.csv";
            m_FlowCheck.Clear();
            using (StreamReader file = new StreamReader(path))
            {
                while ((line = file.ReadLine()) != null)
                {
                    string[] spline = line.Split(',');
                    if (spline.Length > 1)
                    {
                        m_FlowCheck.Add(spline[0].Trim(), spline[1].Trim());
                    }
                }
            }
            path = $@".\Cfg\{m_Type}_ForwardFlow.csv";
            m_ForwardFlow.Clear();
            using (StreamReader file = new StreamReader(path))
            {
                while ((line = file.ReadLine()) != null)
                {
                    string[] spline = line.Split(',');
                    if (spline.Length > 1)
                    {
                        m_ForwardFlow.Add(spline[0].Trim(), spline[1].Trim());
                    }
                }
            }
            path = $@".\Cfg\{m_Type}_SelfTest.csv";
            m_SelfTest.Clear();
            using (StreamReader file = new StreamReader(path))
            {
                while ((line = file.ReadLine()) != null)
                {
                    string[] spline = line.Split(',');
                    if (spline.Length > 1)
                    {
                        m_SelfTest.Add(spline[0].Trim(), spline[1].Trim());
                    }
                }
            }
            path = $@".\Cfg\{m_Type}_WaterIntrusion.csv";
            m_WaterIntrusion.Clear();
            using (StreamReader file = new StreamReader(path))
            {
                while ((line = file.ReadLine()) != null)
                {
                    string[] spline = line.Split(',');
                    if (spline.Length > 1)
                    {
                        m_WaterIntrusion.Add(spline[0].Trim(), spline[1].Trim());
                    }
                }
            }
            path = $@".\Cfg\{m_Type}_PressureDecay.csv";
            m_PressureDecay.Clear();
            using (StreamReader file = new StreamReader(path))
            {
                while ((line = file.ReadLine()) != null)
                {
                    string[] spline = line.Split(',');
                    if (spline.Length > 1)
                    {
                        m_PressureDecay.Add(spline[0].Trim(), spline[1].Trim());
                    }
                }
            }
        }

        private void GetCodeValueDictionary(DataTable dtConfig, string codeName, IDictionary<string, string> dicConfig)
        {
            string[] arrCodeValue = dtConfig.Select($"CODE_NM = '{codeName}'")?[0]["CODE_VALUE"].ToString().Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
            dicConfig.Clear();

            foreach (string line in arrCodeValue)
            {
                string[] spline = line.Split(',');
                if (spline.Length > 1)
                {
                    dicConfig.Add(spline[0].Trim(), spline[1].Trim());
                }
            }
        }

        void ReadCfg()
        {
            string strErrCode = string.Empty; string strErrText = string.Empty;
            DataTable dtConfig = m_sqlBiz.GetCommonCode($"{m_Type}_CONFIG", ref strErrCode, ref strErrText);

            if (dtConfig == null || dtConfig.Rows.Count < 1)
            {
                return;
            }
            GetCodeValueDictionary(dtConfig, "BubblePoint", m_BubblePoint);
            GetCodeValueDictionary(dtConfig, "FlowCheck", m_FlowCheck);
            GetCodeValueDictionary(dtConfig, "ForwardFlow", m_ForwardFlow);
            GetCodeValueDictionary(dtConfig, "SelfTest", m_SelfTest);
            GetCodeValueDictionary(dtConfig, "WaterIntrusion", m_WaterIntrusion);
            GetCodeValueDictionary(dtConfig, "PressureDecay", m_PressureDecay);
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

                // 20221212, SHS, V.2.0.4.0, OPC CLIENT 생성 조건 문 위치 이동 / 생성 확인 후 진행
                if (myUaClient != null)
                {

                    if (!string.IsNullOrEmpty(Uid) && !string.IsNullOrEmpty(Pwd))
                        myUaClient.useridentity = new UserIdentity(Uid, Pwd);

                    myUaClient.CreateConfig();
                    myUaClient.CreateApplicationInstance();
                    myUaClient.CreateSession();

                    // 20221212, SHS, V.2.0.4.0, OPC CLIENT LOG 추가
                    myUaClient.LogMsgFunc += LogMsg;
                    UpdateEquipmentProgDateTime(IF_STATUS.Normal);  // 연결 성공 여부는 CreateSession 에서 exception 이 발생하지 않아야 하는 걸로 보임
                    listViewMsg.UpdateMsg($"OpcUaClient Created !", false, true, true, PC00D01.MSGTINF);
                }
                else
                {
                    UpdateEquipmentProgDateTime(IF_STATUS.Disconnected);
                    listViewMsg.UpdateMsg($"OpcUaClient Create Error !", false, true, true, PC00D01.MSGTINF);
                }


            }
            catch (Exception ex)
            {
                UpdateEquipmentProgDateTime(IF_STATUS.Disconnected);
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
            EnQueue(MSGTYPE.MEASURE,$"{tagname},{datetime},{value},{status}");
            listViewMsg.UpdateMsg($"{tagname},{datetime},{value},{status}",false, true, true, PC00D01.MSGTINF);
        }
    }
}
