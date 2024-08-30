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
using System.Runtime.InteropServices.ComTypes;
using static DataSpider.PC01.PT.PC01S14;

namespace DataSpider.PC01.PT
{
    /// <summary>
    /// File I/F
    /// </summary>
    public class PC01S28 : PC00B03
    {
        public class Vi_Cell_V2
        {
            public String TextFile = "";
            public String CsvFile = "";
            public int avgBackgoundIntensity = 0;
            DataTable MasterTable = new DataTable();
            public FileLog fileLog = null;
            public DateTime RunDate;
            public DateTime dtCurrFileWriteTime = DateTime.MinValue;
            IDictionary<string, string> m_DataList = new Dictionary<string, string>();

            public void SetFileLog(FileLog _filelog)
            {
                fileLog = _filelog;
            }

            public void ReadCsv(string fileName, IDictionary<string, string> dicConfig)
            {
                try
                {
                    string[] buff = File.ReadAllLines(fileName, Encoding.UTF8);
                    foreach (string str in buff)
                    {
                        string[] split = str.Split(',');

                        if (split.Length > 1)
                        {
                            dicConfig.Add(split[0].Trim(), split[1].Trim());
                        }
                    }
                }
                catch (Exception ex)
                {
                    fileLog.WriteLog($"Exceptioin Vi_Cell_V2- ReadCsv ({ex})");
                }
            }


            public void BuildTable(IDictionary<string, string> dicConfig)
            {
                try
                {
                    MasterTable = new DataTable();
                    DataColumn columnTagName = MasterTable.Columns.Add("TagName", typeof(string));
                    DataColumn columnLabel = MasterTable.Columns.Add("Label", typeof(string));
                    DataColumn columnViable = MasterTable.Columns.Add("value", typeof(string));

                    foreach (KeyValuePair<string, string> kvp in dicConfig)
                    {
                        DataRow row = MasterTable.NewRow();
                        row.SetField(columnTagName, kvp.Key);
                        row.SetField(columnLabel, kvp.Value);
                        row.SetField(columnViable, "");
                        MasterTable.Rows.Add(row);
                    }
                }
                catch(Exception ex)
                {
                    fileLog.WriteLog($"Exceptioin Vi_Cell_V2- BuildTable ({ex})");
                }
            }

            public string ReadDataFile(FileInfo fileInfo)
            {
                try
                {
                    using (FileStream fs = fileInfo.Open(FileMode.Open, FileAccess.Read, FileShare.None))
                    {
                        if (fs != null)
                        {  // listViewMsg.UpdateMsg($"Open File {fileInfo.Name} ({fileInfo.LastWriteTime:yyyy-MM-dd HH:mm:ss.fffffff})", false, true);

                            StringBuilder sbData = new StringBuilder();
                            byte[] b = new byte[1024];
                            UTF8Encoding temp = new UTF8Encoding(true);
                            int count;
                            while ((count = fs.Read(b, 0, b.Length)) > 0)
                            {
                                sbData.Append(temp.GetString(b, 0, count));
                            }

                            string[] gsplit = sbData.ToString().Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
                            m_DataList.Clear();
                            foreach (string str in gsplit)
                            {
                                char[] splitdata = { ':' };
                                string[] split = str.Split(splitdata, 2);

                                if (split.Length > 1)
                                {

                                    m_DataList.Add(split[0].Trim(), split[1].Trim());
                                }
                            }
                            dtCurrFileWriteTime = fileInfo.LastWriteTime;
                            Debug.WriteLine(dtCurrFileWriteTime.ToString());
                            return sbData.ToString();
                        }
                    }
                }
                catch (Exception ex)
                {
                    fileLog.WriteLog($"Exceptioin Vi_Cell_V2- ReadDataFile ({ex})");
                }
                return null;
            }

            public void UpdateDataTable(IDictionary<string, string> dicConfig)
            {
                string images = string.Empty;
                string backgoundIntensitySum = string.Empty;
                string rowvalue = string.Empty;
                try
                {
                    foreach (DataRow row in MasterTable.Rows) row[2] = "";

                    foreach (DataRow row in MasterTable.Rows)
                    {

                        //row[2] = m_DataList[row[1].ToString()];
                        if (m_DataList.TryGetValue(row[1].ToString(), out rowvalue))
                        {
                            row[2] = rowvalue;
                        }
                        else continue;

                        if (row[1].ToString().StartsWith("Images"))
                        {
                            images = row[2].ToString();
                        }
                        if (row[1].ToString().StartsWith("Background intensity sum"))
                        {
                            backgoundIntensitySum = row[2].ToString();
                        }
                        if (row[1].ToString().StartsWith("RunDate"))
                            RunDate = DateTime.Parse(row[2].ToString());
                    }
                    if (int.TryParse(images, out int nImages))
                    {
                        if (int.TryParse(backgoundIntensitySum, out int nBackgoundIntensitySum))
                        {
                            avgBackgoundIntensity = (int)Math.Round((double)nBackgoundIntensitySum / nImages);
                            DataRow[] row2 = MasterTable.Select("Label = 'AvgBackgoundIntensity'");
                            row2[0][2] = avgBackgoundIntensity.ToString();
                        }
                    }
                }
                catch (Exception ex)
                {
                    fileLog.WriteLog($"Exceptioin Vi_Cell_V2- UpdateDataTable ({ex})");
                }
            }

            public string GetData()
            {
                StringBuilder ssb = new StringBuilder(); ;
                string rowvalue = string.Empty;
                string images = string.Empty;
                string backgoundIntensitySum = string.Empty;
                ssb.AppendLine($"SVRTIME,{RunDate:yyyy-MM-dd HH:mm:ss.fff},{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}");
                foreach (DataRow row in MasterTable.Rows)
                {
                    if (row[1].ToString().Contains("Assay value") && string.IsNullOrEmpty(row[2].ToString()))
                    {
                        // DoNothing
                    }
                    else 
                        ssb.AppendLine($"{row[0]},{RunDate:yyyy-MM-dd HH:mm:ss.fff},{row[2]}");
                }
                return ssb.ToString();
            }
        }

        public Vi_Cell_V2 m_ViCell=new Vi_Cell_V2();
        IDictionary<string, string> m_ViCellList = new Dictionary<string, string>();

        public PC01S28()
        {
        }
        public PC01S28(IPC00F00 pOwner, DataRow dr, string equipType, string equipName, string connectionInfo, string extraInfo, int nCurNo, bool bAutoRun = false) : base(pOwner, dr, equipType, equipName, connectionInfo, extraInfo, nCurNo, bAutoRun)
        {
        }
        public PC01S28(IPC00F00 pOwner, DataRow dr, int nCurNo, bool bAutoRun = false) : this(pOwner, dr, dr["EQUIP_TYPE_NM"].ToString(), dr["EQUIP_NM"].ToString(), dr["CONNECTION_INFO"].ToString(), dr["EXTRA_INFO"].ToString(), nCurNo, bAutoRun)
        {
        }

        protected override void ThreadJob()
        {
            Thread.Sleep(1000);
            dtLastEnQueuedFileWriteTime = GetLastEnqueuedFileWriteTime();
            listViewMsg.UpdateStatus(true);
            listViewMsg.UpdateMsg("Thread started");
            int msgType;
            m_ViCell.SetFileLog(fileLog);
            ReadCfg();
            m_ViCell.BuildTable(m_ViCellList);
            while (!bTerminal)
            {
                try
                {
                    // string data = FileProcess();
                    FileInfo fi = GetTargetFile();
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
                    m_ViCell.ReadDataFile(fi);
                    dtCurrFileWriteTime = m_ViCell.dtCurrFileWriteTime;
                    m_ViCell.UpdateDataTable(m_ViCellList);
                    //     m_ViCell.ReadDataFile(fi.Name,);
                    string sData = m_ViCell.GetData();
                    dtLastEnQueuedFileWriteTime = dtCurrFileWriteTime;
                    SetLastEnqueuedFileWriteTime(dtLastEnQueuedFileWriteTime);
                    if (string.IsNullOrWhiteSpace(sData))
                    {
                        listViewMsg.UpdateMsg("Failed to get data from file", true, false);
                        UpdateEquipmentProgDateTime(IF_STATUS.InvalidData);
                        Thread.Sleep(10);
                        continue;
                    }
                    EnQueue(msgType, sData);
                    listViewMsg.UpdateMsg($"{m_Name}({msgType}) Data has been enqueued", true, true);
                    fileLog.WriteData(sData, "EnQ", $"{m_Name}({msgType})");
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
            GetCodeValueDictionary(dtConfig, "ViCellList", m_ViCellList);
        }
        private FileInfo GetTargetFile()
        {
            try
            {
                List<FileInfo> listFileInfo = new List<FileInfo>();

                DirectoryInfo di = new DirectoryInfo(filePath);
                FileInfo[] fileInfo = null;
                try
                {
                    fileInfo = di.GetFilesMP(fileSearchPattern);
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

                foreach (FileInfo fi in fileInfo)
                {
                    if (dtLastEnQueuedFileWriteTime.CompareTo(fi.LastWriteTime) < 0)
                    {
                        listFileInfo.Add(fi);
                    }
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

    }
}
