﻿using System;
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


namespace DataSpider.PC01.PT
{
    /// <summary>
    /// Equip_Type : OSMO_TECH OPC UA Server
    /// </summary>
    public class PC01S19 : PC00B01
    {
        OpcUaClient.OpcUaClient myUaClient=null;
        private DateTime dtNormalTime = DateTime.Now;
        Dictionary<string, string> m_OpcItemList = new Dictionary<string, string>();
        private string Uid;
        private string Pwd;

        public PC01S19() : base()
        {
        }

        public PC01S19(PC01F01 pOwner, DataRow dr, string equipType, string equipName, string connectionInfo, string extraInfo, int nCurNo, bool bAutoRun = false) : base(pOwner, dr, equipType, equipName, connectionInfo, extraInfo, nCurNo, bAutoRun)
        {
        }

        public PC01S19(PC01F01 pOwner, DataRow dr, int nCurNo, bool bAutoRun = false) : this(pOwner, dr, dr["EQUIP_TYPE_NM"].ToString(), dr["EQUIP_NM"].ToString(), dr["CONNECTION_INFO"].ToString(), dr["EXTRA_INFO"].ToString(), nCurNo, bAutoRun)
        {
            try
            {

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
                        if (myUaClient.m_reconnectHandler != null)
                        {
                            UpdateEquipmentProgDateTime(IF_STATUS.Disconnected);
                            // 20221212, SHS, V.2.0.4.0, OPC 초기화 부분 삭제
                            //if ((DateTime.Now - dtNormalTime).TotalHours >= 1 )
                            //{
                            //    myUaClient = null;
                            //    listViewMsg.UpdateMsg($" Network Error Time >= 1 Hr, Ua Client Reset ", false, true, true, PC00D01.MSGTERR);
                            //}
                        }
                        else
                        {
                            UpdateEquipmentProgDateTime(IF_STATUS.Normal);
                            dtNormalTime = DateTime.Now;
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
                myUaClient.CreateSubscription(1000);
                listViewMsg.UpdateMsg($"myUaClient.CreateSubscription ", false, true, true, PC00D01.MSGTINF);
                // CSV 파일에 있는 TagName, NodeId 리스트를 MonitoredItem으로 등록하고 
                ReadConfigInfo();
                myUaClient.UpateTagData += UpdateTagValue;
                myUaClient.LogMsgFunc += LogMsg; 

                listViewMsg.UpdateMsg($"myUaClient.UpateTagData ", false, true, true, PC00D01.MSGTINF);
                // currentSubscription에 대한 서비스를 등록한다.
                bool bReturn = myUaClient.AddSubscription();
                if (bReturn == false)
                {
                    // 20240322, SHS, opcClient = null 처리 전에 opcClient?.Close() 추가
                    myUaClient?.Close();
                    myUaClient = null;
                }
                listViewMsg.UpdateMsg($"{bReturn}= myUaClient.AddSubscription", false, true, true, PC00D01.MSGTINF);
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
            if (tagname == "MSR_SVRTIME")
            {
                DateTime svrtime;
                List<string> listData = new List<string>();

                PC00U01.TryParseExact(value, out svrtime);  // 측정시간
                foreach (KeyValuePair<string, string> kvp in m_OpcItemList)
                {
                    try
                    {
                        string strValue =myUaClient.ReadValue(kvp.Value).Value?.ToString();
                        if (kvp.Key == "MSR_SVRTIME")
                        {
                            //EnQueue(MSGTYPE.MEASURE, $" {kvp.Key},{svrtime.ToString("yyyy-MM-dd HH:mm:ss")}, {DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}");
                            string currentTime = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss}";
                            listData.Add($"{kvp.Key}, {svrtime:yyyy-MM-dd HH:mm:ss}, {currentTime}");
                            listViewMsg.UpdateMsg($" {kvp.Key}, {svrtime:yyyy-MM-dd HH:mm:ss}, {strValue}, {currentTime}", false, true, true, PC00D01.MSGTINF);
                        }
                        else
                        {
                            //EnQueue(MSGTYPE.MEASURE, $"{kvp.Key}, {svrtime:yyyy-MM-dd HH:mm:ss}, {strValue}");
                            listData.Add($"{kvp.Key}, {svrtime:yyyy-MM-dd HH:mm:ss}, {strValue}");
                        }
                        listViewMsg.UpdateMsg($" {kvp.Key}, {svrtime:yyyy-MM-dd HH:mm:ss}, {strValue} ", false, true, true, PC00D01.MSGTINF);
                    }
                    catch (Exception ex)
                    {
                        listViewMsg.UpdateMsg($" UpdateTagValue - {kvp.Key },{kvp.Value} - {ex}", false, true, true, PC00D01.MSGTERR);
                    }
                }
                if (listData.Count > 0) 
                {
                    EnQueue(MSGTYPE.MEASURE, string.Join(System.Environment.NewLine, listData));
                }
            }
        }
    }
}
