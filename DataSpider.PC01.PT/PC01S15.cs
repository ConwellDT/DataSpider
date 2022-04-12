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
// WildcardPattern
using System.Management.Automation;
using System.Web.Script.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Xml.Serialization;

namespace DataSpider.PC01.PT
{

    public class CData
    {
        public enum JSON_TYPE { NONE, DATETIME, KEY, SEARCH };
        [XmlAttribute]
        public string NAME { get; set; } = "JSONKEY";
        [XmlAttribute]
        public string MeasureType { get; set; } = "";


    }
    [Serializable]
    [XmlRoot(ElementName = "CONFIG ELEMENT")]
    public class cfgJSON : CData
    {
        [XmlAttribute]
        public JSON_TYPE DATATYPE { get; set; } = JSON_TYPE.NONE;
        [XmlAttribute]
        public string PATH { get; set; } = "data[0]._timestamp";
        [XmlAttribute]
        public string KEYVALUE { get; set; } = "";
        [XmlElement(IsNullable = false)]
        public string KEY_STR { get; set; } = "";
        public string VALUE_STR { get; set; } = "";

    }

    [Serializable]
    public class cfgData
    {
        [XmlElement(IsNullable = false)]
        public string TimeStampPath { get; set; } = "_timestamp";

        public string MeasureType { get; set; } = "";
        public List<cfgJSON> cfgList { get; set; } = new List<cfgJSON>();
        public cfgData(string sName) : this()
        {
            MeasureType = sName;
        }
        public cfgData()
        {
        }
    }

    public class cfgSave
    {
        [XmlIgnore]
        public Dictionary<string, cfgData> cfgDictionary { get; set; } = new Dictionary<string, cfgData>();

        public string FileName { get; set; } = "";
        public string FilePath { get; set; } = ".\\";
        public static string Serialize(string fileName, Dictionary<string, cfgData> result)
        {
            string sReturn = string.Empty;
            try
            {
                var xmlPath = fileName;
                XmlSerializer xmlSer = new XmlSerializer(typeof(List<cfgData>));
                FileStream fs = new FileStream(xmlPath, FileMode.Create, FileAccess.Write);

                List<cfgData> pList = new List<cfgData>();
                if (result == null)
                    result = new Dictionary<string, cfgData>();
                Dictionary<string, cfgData> pDic = result;
                foreach (string skey in pDic.Keys)
                {
                    pList.Add(pDic[skey]);
                }
                xmlSer.Serialize(fs, pList);

                fs.Close();
            }
            catch (Exception ex)
            {
                sReturn = ex.Message;  //
            }
            return sReturn;
        }
        public Dictionary<string, cfgData> Deserial<T>(string fileName, Dictionary<string, cfgData> paramObj) where T : new()
        {
            var result = new Dictionary<string, cfgData>();

            if (File.Exists(fileName) == false)
                cfgSave.Serialize(fileName, result);

            XmlSerializer xmlSer = new XmlSerializer(typeof(T));
            FileStream fs = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.None);

            var pList = (T)xmlSer.Deserialize(fs);
            Dictionary<string, cfgData> pDic = result as Dictionary<string, cfgData>;

            foreach (cfgData pObj in pList as List<cfgData>)
            {
                pDic[pObj.MeasureType] = pObj;
            }

            fs.Close();

            return pDic;
        }

        public cfgSave()
        {
        }
        public cfgSave(string cfgFile) : this()
        {
            try
            {
                FileName = System.IO.Path.GetFileName(cfgFile);
                FilePath = System.IO.Path.GetDirectoryName(cfgFile);
            }
            catch
            { }
        }

        public void xmlLoadData(string sFilePath = null)
        {
            try
            {
                cfgDictionary = Deserial<List<cfgData>>(sFilePath, cfgDictionary);

            }
            catch (Exception ex)
            {
                //MessageBox.Show(ex.Message);
            }
        }


    }


    public class FIT_SC5P_DATA
    {
        private StringBuilder sbData = null;
        public string TIMESTAMP { get; set; }

        public FIT_SC5P_DATA()
        {
            sbData = new StringBuilder();
        }
        public FIT_SC5P_DATA(DateTime pTime) : this()
        {
            TIMESTAMP = pTime == null ? DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") : pTime.ToString();
        }

        public void SetTimeStampData(DateTime pTime)
        {
            TIMESTAMP = pTime == null ? DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") : pTime.ToString();
        }
        public bool Add(string name, string value)
        {
            sbData.AppendLine($"{name}, {TIMESTAMP}, {value}");
            return true;
        }

        public string GetStringData()
        {
            return sbData.ToString();
        }
        public string ClearStringData()
        {
            sbData.Clear();
            return sbData.ToString();
        }
        public static DateTime TimeStampToDateTime(long value)
        {
            DateTime dt = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            dt = dt.AddSeconds(value).ToLocalTime();
            return dt;
        }

        public static string GetStrDataFromJObj(Object pObj)
        {
            string sReturn = string.Empty;
            if (pObj == null)
                return sReturn;
            try
            {
                string sType = pObj.GetType().Name;

                switch (sType)
                {
                    case "JArray":
                        JArray pArray = pObj as JArray;
                        sReturn = string.Join(",", pArray.ToList());
                        break;
                    case "JObject":
                        JObject pObject = pObj as JObject;
                        sReturn = pObject.ToString();
                        break;
                    case "JValue":
                        JValue pValue = pObj as JValue;
                        sReturn = pValue.ToString();
                        break;
                    default:
                        sReturn = pObj.ToString();
                        break;

                }

            }
            catch (Exception ex)
            {
                sReturn = string.Empty;
            }

            return sReturn;
        }
    }



    /// <summary>
    /// File I/F, Filter Integrity Tester : Sartocheck 5 plus
    /// </summary>
    public class PC01S15 : PC00B03
    {

        cfgSave cfg;
        public PC01S15()
        {
        }
        public PC01S15(IPC00F00 pOwner, string equipType, string equipName, string connectionInfo, string extraInfo, int nCurNo, bool bAutoRun = false) : base(pOwner, equipType, equipName, connectionInfo, extraInfo, nCurNo, bAutoRun)
        {
        }
        public PC01S15(IPC00F00 pOwner, DataRow dr, int nCurNo, bool bAutoRun = false) : this(pOwner, dr["EQUIP_TYPE_NM"].ToString(), dr["EQUIP_NM"].ToString(), dr["CONNECTION_INFO"].ToString(), dr["EXTRA_INFO"].ToString(), nCurNo, bAutoRun)
        {
            drEquipment = dr;

            string cfgFile = @".\CFG\" + m_Name + ".cfg";

            cfg = new cfgSave(cfgFile);
            cfg.xmlLoadData(cfgFile);
        }


        /// <summary>
        /// FIT_SC4P 장비의 경우 테스트 켄슬 등의 문제 발생 시, 차트데이터 (5초 인터벌의 데이터) 가 테스트 시간에 따라 가변적 등의 문제로 ttv 형태로 발췌하여 EnQueue 처리 필요
        /// </summary>
        /// <param name="sData"></param>
        /// <returns></returns>
        public override string GetFileData(string sData)
        {
            List<string> listData = sData.Split(new string[] { Environment.NewLine, "\n" }, StringSplitOptions.None).ToList();
            // DateTime

            var details = JToken.Parse(sData);

            FIT_SC5P_DATA data = new FIT_SC5P_DATA();
            JToken pValue = null;

            List<JToken> jList = new List<JToken>(); 
            try
            {
                var pjObj = details["data"];
             
                int msgType = 1;
                string strQueueData = "";

                if (pjObj == null)
                {
                    var pDataChecker = details.SelectToken("WorkFlowType");
                    if (pDataChecker != null)
                    {
                        var p = JToken.FromObject(details);
                        jList.Add(p);            
                    }
                    else
                        return string.Empty;
                }
                else
                    jList = pjObj.ToList();


                cfgData CFGDATA = null;

                foreach (JToken pTemp in jList)
                {
                    var pjTokenObj = JToken.Parse(pTemp.ToString()); // pjObj1.ToObject<JObject>();

                    string strMeasureType = (string)pjTokenObj.SelectToken("WorkFlowType");
                    if (!cfg.cfgDictionary.TryGetValue(strMeasureType, out CFGDATA))
                    {
                        CFGDATA = new cfgData(strMeasureType);
                        cfg.cfgDictionary[strMeasureType] = CFGDATA;
                    }

                    string sTimeStr = (string)pjTokenObj.SelectToken(CFGDATA.TimeStampPath);
                    Int64 nTimeOffset = 0;
                    if (!string.IsNullOrEmpty(sTimeStr))
                        nTimeOffset = Convert.ToInt64(sTimeStr);
                    DateTime pTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc).AddSeconds(nTimeOffset).ToLocalTime();
                    data.SetTimeStampData(pTime);


                    string sValueTemp = string.Empty;
                    string sBreakstr = string.Empty;
                    foreach (cfgJSON cfgObj in CFGDATA.cfgList)
                    {
                        try
                        {
                            pValue = pjTokenObj.SelectToken(cfgObj.PATH);
                            if (pValue == null)
                                continue;
                            if (cfgObj.DATATYPE == CData.JSON_TYPE.KEY)
                            {
                                var pArraySet = pValue.ToList().Find(p => (p["key"] == null ? "" : p["key"].ToString()) == cfgObj.KEYVALUE);// ("")
                                if (pArraySet == null)
                                    continue;
                                pValue = pArraySet["value"];
                                sValueTemp = (string)pValue.ToString();
                            }
                            if (cfgObj.DATATYPE == CData.JSON_TYPE.SEARCH)
                            {
                                if (string.IsNullOrEmpty(cfgObj.KEY_STR) || string.IsNullOrEmpty(cfgObj.VALUE_STR))
                                    continue;
                                var pArraySet = pValue.ToList().Find(p => (p[cfgObj.KEY_STR] == null ? "" : p[cfgObj.KEY_STR].ToString()) == cfgObj.KEYVALUE);// ("")
                                if (pArraySet == null)
                                    continue;
                                pValue = pArraySet[cfgObj.VALUE_STR];
                                sValueTemp = (string)pValue.ToString();
                            }
                            else if (cfgObj.DATATYPE == CData.JSON_TYPE.DATETIME)
                            {
                                sTimeStr = (string)pValue.ToString();
                                nTimeOffset = 0;
                                if (!string.IsNullOrEmpty(sTimeStr))
                                {
                                    nTimeOffset = Convert.ToInt64(sTimeStr);                                                //1640015936                
                                    pTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc).AddSeconds(nTimeOffset); // 1640015936);
                                    sValueTemp = pTime.ToLocalTime().ToString();

                                }
                            }
                            else
                            {
                                //sValueTemp = pValue.ToString();
                                sValueTemp = FIT_SC5P_DATA.GetStrDataFromJObj(pValue);
                            }


                            data.Add(cfgObj.NAME, sValueTemp);
                        }
                        catch (Exception ex)
                        {
                            string sMessage = $"{ex.ToString()} cfg : TYPE({cfgObj.MeasureType}), Name({cfgObj.NAME}), Path({cfgObj.PATH}), TYPE({cfgObj.DATATYPE})";
                            UpdateEquipmentProgDateTime(IF_STATUS.InvalidData);
                            listViewMsg.UpdateMsg(sMessage, false, true, true, PC00D01.MSGTERR);
                        }
                    }

                    data.Add("SVRTIME", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                    strQueueData = data.GetStringData();
                    EnQueue(msgType, strQueueData);
                    data.ClearStringData();

                    listViewMsg.UpdateMsg($"{m_Name}({msgType}) Data has been enqueued", true, true);
                    fileLog.WriteData(strQueueData, "EnQ", $"{m_Name}({msgType})");

                } //     var pjObj =    details["data"];

                UpdateEquipmentProgDateTime(IF_STATUS.Normal);
            }
            catch (Exception ex)
            {
                UpdateEquipmentProgDateTime(IF_STATUS.InvalidData);
                listViewMsg.UpdateMsg(ex.ToString(), false, true, true, PC00D01.MSGTERR);
            }
            return data.GetStringData();
        }

        public string GetFileData_old(string sData)
        {
            List<string> listData = sData.Split(new string[] { "\n" }, StringSplitOptions.None).ToList<string>();
            // 테스트가 켄슬되는 경우 결과 데이터 중 차트 데이터 (5초 간격 데이터) 가 전부누락 또는 일부 누락하면서 줄수가 줄어드는 문제
            // 테스트 시간(초) 값 읽기. 차트 데이터 수량 읽기. 테스트 시간 / 5 의 값과 차트 데이터 수량 값이 같지 않으면 모자란 만큼 줄을 추가
            int testTime = 600;// int.Parse(listData[10]);
            int chartDataCount = int.Parse(listData[24]);
            for (int i = 0; i < (testTime - (chartDataCount * 5)) / 5; i++)
            {
                listData.Insert(25, string.Empty);
            }

            // [Ext] 데이터를 | 구분자로 한줄로 취합하여 마지막 줄에 추가
            StringBuilder sbExt = new StringBuilder();
            string extString = listData[165].Substring(50);
            while (!string.IsNullOrWhiteSpace(extString))
            {
                if (sbExt.Length > 0)
                {
                    sbExt.Append("|");
                }
                int offset = extString.IndexOf(' ');
                int length = int.Parse(extString.Substring(0, offset));
                sbExt.Append($"{extString.Substring(offset + 1, length)}");
                extString = extString.Substring(offset + 1 + length + 1);
            }
            listData.Add(sbExt.ToString());
            //sbData.Append(sbExt.ToString());
            return string.Join(Environment.NewLine, listData);
            //return sbData.ToString();
        }


        protected FileInfo GetTargetFile(DirectoryInfo pDirectory = null)
        {
            try
            {
                if (pDirectory == null)
                    return null;

                List<FileInfo> listFileInfo = new List<FileInfo>();

                FileInfo[] fileInfo = null;


                if (!pDirectory.Exists)
                {
                    listViewMsg.UpdateMsg("Folder is not Exist", true, false);
                    return null;
                }
                try
                {
                    try
                    {
                        fileInfo = pDirectory.GetFilesMP(fileSearchPattern);
                    }
                    catch (Exception ex)
                    {
                        listViewMsg.UpdateMsg("Get Files Error", true, false);
                        if (!string.IsNullOrWhiteSpace(winID) && !string.IsNullOrWhiteSpace(winPW))
                        {
                            PC00U01.ExecuteNetUse(filePath, winID, winPW);
                        }
                        UpdateEquipmentProgDateTime(IF_STATUS.Disconnected);
                        listViewMsg.UpdateMsg(ex.ToString(), false, true, true, PC00D01.MSGTERR);
                        return null;
                    }

                    var pFileChecker = fileInfo.ToList().FindAll(x => x.Name.ToUpper().Contains("INFO"));

                    if (pFileChecker.Count < 1)
                        return null;


                    foreach (FileInfo fi in fileInfo)
                    {
                        if (!fi.Name.ToUpper().Contains("INFO"))
                            listFileInfo.Add(fi);
                    }

                }
                catch (Exception ex)
                {
                    listViewMsg.UpdateMsg(ex.ToString(), false, true, true, PC00D01.MSGTERR);
                    return null;
                }

                if (listFileInfo.Count < 1)
                {
                    listViewMsg.UpdateMsg("No Files", true, false);
                    UpdateEquipmentProgDateTime(IF_STATUS.Normal);
                    //UpdateEquipmentProgDateTime(IF_STATUS.NoData);
                    return null;
                }

                listFileInfo.Sort((x, y) => x.LastWriteTime.CompareTo(y.LastWriteTime));

                UpdateEquipmentProgDateTime(IF_STATUS.Normal);
                listViewMsg.UpdateMsg($"Last Enqueued file write time ({dtLastEnQueuedFileWriteTime:yyyy-MM-dd HH:mm:ss.fffffff}). ({listFileInfo.Count}/{fileInfo.Length}) files.", false, true);
                return listFileInfo[0];
            }
            catch (Exception ex)
            {
                listViewMsg.UpdateMsg("Exception in GetTargetFile", true, false);
                UpdateEquipmentProgDateTime(IF_STATUS.InternalError);
                listViewMsg.UpdateMsg(ex.ToString(), false, true, true, PC00D01.MSGTERR);
            }
            return null;
        }
        protected DirectoryInfo GetTargetDirectory()
        {
            try
            {

                DirectoryInfo di = new DirectoryInfo(filePath);
                DirectoryInfo pReturn = null;
                DirectoryInfo[] pInfo;

                FileInfo[] fileInfo = null;

                List<DirectoryInfo> listDitectoryInfo = new List<DirectoryInfo>();

                if (!di.Exists)
                {
                    listViewMsg.UpdateMsg("Folder is not Exist", true, false);
                    return pReturn;
                }
                try
                {
                    pInfo = di.GetDirectories("*", System.IO.SearchOption.AllDirectories);


                    foreach (DirectoryInfo info in pInfo)
                    {
                        if (!(dtLastEnQueuedFileWriteTime.CompareTo(info.LastWriteTime) < 0))
                        {
                            continue;
                        }

                        try
                        {
                            fileInfo = info.GetFilesMP(fileSearchPattern);
                        }
                        catch (Exception ex)
                        {
                            listViewMsg.UpdateMsg("Get Files Error", true, false);
                            if (!string.IsNullOrWhiteSpace(winID) && !string.IsNullOrWhiteSpace(winPW))
                            {
                                PC00U01.ExecuteNetUse(filePath, winID, winPW);
                            }
                            UpdateEquipmentProgDateTime(IF_STATUS.Disconnected);
                            listViewMsg.UpdateMsg(ex.ToString(), false, true, true, PC00D01.MSGTERR);
                            return null;
                        }

                        var pFileChecker = fileInfo.ToList().FindAll(x => x.Name.ToUpper().Contains("INFO"));

                        if (pFileChecker.Count < 1)
                            continue;
                        else
                            listDitectoryInfo.Add(info);
                    }

                    if (listDitectoryInfo.Count < 1)
                    {
                        listViewMsg.UpdateMsg("No Directory", true, false);
                        UpdateEquipmentProgDateTime(IF_STATUS.Normal);
                        //UpdateEquipmentProgDateTime(IF_STATUS.NoData);
                        return null;
                    }

                    listDitectoryInfo.Sort((x, y) => x.LastWriteTime.CompareTo(y.LastWriteTime));

                    UpdateEquipmentProgDateTime(IF_STATUS.Normal);
                    listViewMsg.UpdateMsg($"Last Enqueued DitectoryInfo write time ({dtLastEnQueuedFileWriteTime:yyyy-MM-dd HH:mm:ss.fffffff}). ({listDitectoryInfo.Count}/{listDitectoryInfo.Count}) Directory.", false, true);
                    return listDitectoryInfo[0];

                } //   pInfo = di.GetDirectories("*", System.IO.SearchOption.AllDirectories);

                catch (Exception ex)
                {
                    listViewMsg.UpdateMsg(ex.ToString(), false, true, true, PC00D01.MSGTERR);
                    return null;
                }
            }  // 
            catch (Exception ex)
            {
                listViewMsg.UpdateMsg("Exception in GetTargetFile", true, false);
                UpdateEquipmentProgDateTime(IF_STATUS.InternalError);
                listViewMsg.UpdateMsg(ex.ToString(), false, true, true, PC00D01.MSGTERR);
            }
            return null;
        }

        protected override void ThreadJob()
        {
            Thread.Sleep(1000);
            dtLastEnQueuedFileWriteTime = GetLastEnqueuedFileWriteTime();
            listViewMsg.UpdateStatus(true);
            listViewMsg.UpdateMsg("Thread started");
            int msgType;

            while (!bTerminal)
            {
                try
                {
                    //string data = FileProcess();

                    DirectoryInfo pDir = GetTargetDirectory();

                    FileInfo fi = GetTargetFile(pDir);
                    if (fi == null)
                    {
                        Thread.Sleep(10000);
                        continue;
                    }
                    // 파일이름 패턴으로 MessageType 을 얻도록 설정되어 있으면
                    if (dicMessageType.Count > 0)
                    {
                        msgType = GetMessageType(fi.Name);
                        if (msgType < 0)
                        {
                            listViewMsg.UpdateMsg($"File [{fi.Name}] is not registered file name pattern for MessageType.", true, true, true, PC00D01.MSGTERR);
                            UpdateEquipmentProgDateTime(IF_STATUS.InvalidData);
                            Thread.Sleep(10);
                            continue;
                        }
                    }
                    else
                    {
                        msgType = 1;
                    }

                    string sData = GetFileRawData(fi);
                    dtCurrFileWriteTime = pDir.LastWriteTime;

                    dtLastEnQueuedFileWriteTime = dtCurrFileWriteTime;
                    SetLastEnqueuedFileWriteTime(dtLastEnQueuedFileWriteTime);
                    if (string.IsNullOrWhiteSpace(sData))
                    {
                        listViewMsg.UpdateMsg("Failed to get data from file", true, false);
                        UpdateEquipmentProgDateTime(IF_STATUS.InvalidData);
                        Thread.Sleep(10);
                        continue;
                    }
                    string data = GetFileData(sData);

                    //EnQueue(msgType, data);  = GetFileData로 이동

                    //UpdateEquipmentProgDateTime(IF_STATUS.Normal);
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
    }
}
