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
using System.Data;
using System.Security.Cryptography;
using System.Text.Json;
using System.Management.Automation;
using Newtonsoft.Json.Linq;
using NLog.Fluent;
using System.Runtime.InteropServices.ComTypes;

namespace DataSpider.PC01.PT
{
    /// <summary>
    /// File I/F
    /// </summary>
    public class PC01S36 : PC00B03
    {
        private Dictionary<string, int> dicMsgType = new Dictionary<string, int>();
        private Dictionary<string, string> dicData = new Dictionary<string, string>();
        private string measureDateTimeName = "_TIMESTAMP";

        public PC01S36()
        {
        }

        public PC01S36(IPC00F00 pOwner, DataRow dr, string equipType, string equipName, string connectionInfo, string extraInfo, int nCurNo, bool bAutoRun = false) : base(pOwner, dr, equipType, equipName, connectionInfo, extraInfo, nCurNo, bAutoRun)
        {
        }

        public PC01S36(IPC00F00 pOwner, DataRow dr, int nCurNo, bool bAutoRun = false) : this(pOwner, dr, dr["EQUIP_TYPE_NM"].ToString(), dr["EQUIP_NM"].ToString(), dr["CONNECTION_INFO"].ToString(), dr["EXTRA_INFO"].ToString(), nCurNo, bAutoRun)
        {
        }

        protected override void ThreadJob()
        {
            GetConfigInfo();
            Thread.Sleep(1000);
            dtLastEnQueuedFileWriteTime = GetLastEnqueuedFileWriteTime();
            listViewMsg.UpdateStatus(true);
            listViewMsg.UpdateMsg("Thread started", true, true, true);

            while (!bTerminal)
            {
                try
                {
                    FileInfo fi = GetTargetFile();
                    if (fi == null)
                    {
                        Thread.Sleep(1000);
                        continue;
                    }

                    string sRawData = GetFileRawData(fi);
                    fileLog.WriteData(sRawData, "GetFileRawData", "RawData");
                    dtLastEnQueuedFileWriteTime = dtCurrFileWriteTime;
                    SetLastEnqueuedFileWriteTime(dtLastEnQueuedFileWriteTime);
                    if (string.IsNullOrWhiteSpace(sRawData))
                    {
                        listViewMsg.UpdateMsg("Failed to get data from file", true, false, true);
                        UpdateEquipmentProgDateTime(IF_STATUS.InvalidData);
                        Thread.Sleep(10);
                        continue;
                    }

                    ProcessSC5P(sRawData);
                }
                catch (Exception ex)
                {
                    UpdateEquipmentProgDateTime(IF_STATUS.InternalError);
                    listViewMsg.UpdateMsg(ex.ToString(), false, true, true, PC00D01.MSGTERR);
                }
                finally
                {
                }
                Thread.Sleep(10);
            }
            UpdateEquipmentProgDateTime(IF_STATUS.Stop);
            listViewMsg.UpdateStatus(false);
            listViewMsg.UpdateMsg("Thread finished");
        }

        private bool ProcessSC5P(string rawData)
        {
            dicData.Clear();
            JToken jtData = JToken.Parse(rawData);

            JToken jtFirst = jtData.AsJEnumerable().First();
            if (jtFirst is JProperty) 
            {
                if ((jtFirst as JProperty).Name == "payload")
                {
                    listViewMsg.UpdateMsg($"Info file. Skip parsing. ", false, true, true, PC00D01.MSGTINF);
                    return false;
                }
            }

            bool result = Extract_SC5P(jtData);

            // WorkFlowType (MessageType)
            if (!dicData.TryGetValue("WORKFLOWTYPE", out string workFlowType))
            {
                listViewMsg.UpdateMsg($"There is no WorkFlowType Data. Failed to process file. ", false, true, true, PC00D01.MSGTERR);
                return false;
            }
            if (!dicMsgType.TryGetValue(workFlowType, out int msgType))
            {
                listViewMsg.UpdateMsg($"Not defined WorkFlowType : {workFlowType}. Failed to process file", false, true, true, PC00D01.MSGTERR);
                return false;
            }

            // Measure DateTime
            //if (!dicData.TryGetValue("_TIMESTAMP", out string rawMeasureDateTime))
            if (!dicData.TryGetValue(measureDateTimeName, out string rawMeasureDateTime))
            {
                listViewMsg.UpdateMsg($"There is no _timestamp Data. Failed to process file. ", false, true, true, PC00D01.MSGTERR);
                return false;
            }

            DateTime dtMeasureDateTime;

            // rawMeasureDateTime 이 long 타입 숫자이면 EpochSeconds 로 판단하여 변환
            if (long.TryParse(rawMeasureDateTime, out long longMeasureDateTime))
            { 
                if (!PC00U01.TryParseEpochSeconds(longMeasureDateTime, out dtMeasureDateTime))
                {
                    listViewMsg.UpdateMsg($"Failed to Parse _timestamp to DateTime : {rawMeasureDateTime}. Failed to process file. ", false, true, true, PC00D01.MSGTERR);
                    return false;
                }
            }
            // 문자열 시간 파싱, MA_COMMON_CD TIMEFORMAT 에 "yyyyMMddTHHmmss zzz" 포맷 추가 필요 (20240614T112620+0900 형태)
            else
            {
                if (!PC00U01.TryParseExact(rawMeasureDateTime, out dtMeasureDateTime))
                {
                    listViewMsg.UpdateMsg($"Failed to Parse _timestamp to DateTime : {rawMeasureDateTime}. Failed to process file. ", false, true, true, PC00D01.MSGTERR);
                    return false;
                }
            }

            string measureDateTime = dtMeasureDateTime.ToString("yyyy-MM-dd HH:mm:ss.fff");

            StringBuilder ssb = new StringBuilder(); ;

            ssb.AppendLine($"SVRTIME, {measureDateTime}, {DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}");

            foreach (KeyValuePair<string, string> kvp in dicData)
            {
                ssb.AppendLine($"{kvp.Key}, {measureDateTime}, {kvp.Value}");
            }

            EnQueue(msgType, ssb.ToString());

            listViewMsg.UpdateMsg($"{m_Name}({msgType}) Data has been enqueued", true, true, true);
            fileLog.WriteData(ssb.ToString(), "EnQ", $"{m_Name}({msgType})");

            return true;
        }

        private void AddData(string key, string val)
        {
            if (string.IsNullOrWhiteSpace(key)) return;

            key = key.ToUpper();
            if (!dicData.TryAdd(key, val))
            {
                listViewMsg.UpdateMsg($"{key} is duplicated property name. Skip this one.", false, true, true, PC00D01.MSGTERR);
            }
        }
        private bool Extract_SC5P(JToken json)
        {
            try
            {
                foreach (JToken item in json.AsJEnumerable())
                {
                    if (!item.HasValues)
                    {
                        if (item.Parent is JProperty)
                        {
                            JProperty jp = item.Parent as JProperty;
                            AddData(jp.Path, jp.Value.ToString());
                        }
                        if (item.Parent is JArray)
                        {
                            JArray ja = item.Parent as JArray;
                            AddData(ja.Path, ja.ToString().Replace("\r\n", string.Empty).Replace("[", string.Empty).Replace("]", string.Empty));
                        }
                        return true;
                    }

                    if (item is JArray)
                    {
                        if (item.Parent is JProperty)
                        {
                            JProperty jp = item.Parent as JProperty;
                            AddData(jp.Path, jp.Value.ToString().Replace("\r\n", string.Empty));
                        }
                    }

                    if (item is JProperty && (item as JProperty).Name.ToUpper().Equals("PROTOCOLDATA"))
                    {
                        foreach (JToken pd in item.AsJEnumerable())
                        {
                            foreach (JToken pdv in pd.AsJEnumerable())
                            {
                                AddData($"{(item as JProperty).Path}.{pdv["key"]}", pdv["value"].ToString());
                            }
                        }
                    }

                    Extract_SC5P(item);
                }
            }
            catch (Exception ex)
            {
                listViewMsg.UpdateMsg($"Exception in Extract_SC5P - {ex}", false, true, true, PC00D01.MSGTERR);
                return false;
            }

            return true;
        }

        //private bool Extract_SC5P(JToken json)
        //{
        //    try
        //    {
        //        foreach (JToken item in json.AsJEnumerable())
        //        {
        //            if (!item.HasValues)
        //            {
        //                if (item.Parent is JProperty)
        //                {
        //                    JProperty jp = item.Parent as JProperty;
        //                    if (!dicData.TryAdd(jp.Path, jp.Value.ToString()))
        //                    {
        //                        listViewMsg.UpdateMsg($"{jp.Path} is duplicated property name. Skip this one.", false, true, true, PC00D01.MSGTERR);
        //                    }
        //                }
        //                return true;
        //            }

        //            if (item is JArray)
        //            {
        //                JProperty jp = item.Parent as JProperty;
        //                if (!dicData.TryAdd(jp.Path, jp.Value.ToString()))
        //                {
        //                    listViewMsg.UpdateMsg($"{jp.Path} is duplicated property name. Skip this one.", false, true, true, PC00D01.MSGTERR);
        //                }
        //                Extract_SC5P(item);
        //                return true;
        //            }

        //            if (item is JProperty && (item as JProperty).Name.ToUpper().Equals("PROTOCOLDATA"))
        //            {
        //                foreach (JToken pd in item.AsJEnumerable())
        //                {
        //                    foreach (JToken pdv in pd.AsJEnumerable())
        //                    {
        //                        if (!string.IsNullOrWhiteSpace((item as JProperty).Path))
        //                        {
        //                            string jsonPath = $"{(item as JProperty).Path}.{pdv["key"]}";
        //                            if (!dicData.TryAdd($"{jsonPath}", pdv["value"].ToString()))
        //                            {
        //                                listViewMsg.UpdateMsg($"{jsonPath} is duplicated property name. Skip this one.", false, true, true, PC00D01.MSGTERR);
        //                            }
        //                        }
        //                    }

        //                }
        //                Extract_SC5P(item);
        //                continue;
        //                //return true;
        //            }

        //            Extract_SC5P(item);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        listViewMsg.UpdateMsg($"Exception in Extract_SC5P - {ex}", false, true, true, PC00D01.MSGTERR);
        //        return false;
        //    }

        //    return true;
        //}

        private void GetConfigInfo()
        {
            GetMsgType();
            GetMeasureDateTimeName();
        }

        private void GetMsgType()
        {
            try
            {
                string configInfo = drEquipment["CONFIG_INFO"]?.ToString();
                if (string.IsNullOrEmpty(configInfo))
                {
                    listViewMsg.UpdateMsg($"No Config Info.", false, true, true, PC00D01.MSGTERR);
                    return;
                }
                JsonDocument document = JsonDocument.Parse(configInfo);
                JsonElement root = document.RootElement;
                if (root.TryGetProperty("WORK_FLOW_TYPE", out JsonElement eWorkFlowType))
                {
                    foreach (var e in eWorkFlowType.EnumerateArray())
                    {
                        string key = e.GetProperty("NAME").ToString().ToUpper();
                        string val = e.GetProperty("MSG_TYPE").ToString();
                        if (!dicMsgType.ContainsKey(key))
                        {
                            if (int.TryParse(val, out int value))
                            {
                                dicMsgType.Add(key, value);
                                listViewMsg.UpdateMsg($"MSG_TYPE - {key} = {value}", false, true, true, PC00D01.MSGTINF);
                            }
                        }
                    }
                }
                else
                {
                    listViewMsg.UpdateMsg($"WORK_FLOW_TYPE is not defined (CONFIG_INFO).", false, true, true, PC00D01.MSGTERR);
                }
            }
            catch (Exception ex)
            {
                listViewMsg.UpdateMsg($"Exception in ReadMessageType - {ex}", false, true, true, PC00D01.MSGTERR);
            }
        }
        private void GetMeasureDateTimeName()
        {

            try
            {
                string configInfo = drEquipment["CONFIG_INFO"]?.ToString();
                if (string.IsNullOrEmpty(configInfo))
                {
                    listViewMsg.UpdateMsg($"No Config Info.", false, true, true, PC00D01.MSGTERR);
                    return;
                }
                JsonDocument document = JsonDocument.Parse(configInfo);
                JsonElement root = document.RootElement;

                if (root.TryGetProperty("MEASURE_DATETIME_NAME", out JsonElement mdtnElement))
                {
                    measureDateTimeName = mdtnElement.ToString().ToUpper();
                    listViewMsg.UpdateMsg($"MEASURE_DATETIME_NAME = {measureDateTimeName}", false, true, true, PC00D01.MSGTINF);
                }
                else
                {
                    listViewMsg.UpdateMsg($"MEASURE_DATETIME_NAME is not defined (CONFIG_INFO). Use default MEASURE_DATETIME_NAME ({measureDateTimeName})", false, true, true, PC00D01.MSGTERR);
                }
            }
            catch (Exception ex)
            {
                listViewMsg.UpdateMsg($"Exception in GetMeasureDateTimeName - {ex}", false, true, true, PC00D01.MSGTERR);
            }
        }
    }
}
