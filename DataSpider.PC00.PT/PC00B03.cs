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
using OSIsoft.AF.Data;

namespace DataSpider.PC00.PT
{
    /// <summary>
    /// Base Class For File 
    /// </summary>
    public class PC00B03 : PC00B01
    {
        protected DateTime dtLastEnQueuedFileWriteTime = DateTime.MinValue;
        protected DateTime dtCurrFileWriteTime = DateTime.MinValue;
        protected string filePath = string.Empty;
        protected string fileSearchPattern = "*.*";
        protected Dictionary<int, WildcardPattern> dicMessageType = new Dictionary<int, WildcardPattern>();

        protected string winID = string.Empty;
        protected string winPW = string.Empty;

        public PC00B03()
        {
        }
        public PC00B03(IPC00F00 pOwner, string equipType, string equipName, string connectionInfo, string extraInfo, int nCurNo, bool bAutoRun = false) : base(pOwner, equipType, equipName, connectionInfo, extraInfo, nCurNo, bAutoRun)
        {
            string[] conn = m_ConnectionInfo.Split(',');
            if (conn.Length < 1 || string.IsNullOrWhiteSpace(conn[0]))
            {
                listViewMsg.UpdateStatus(false);
                listViewMsg.UpdateMsg("Invalid Connection_Info", true, true, true, PC00D01.MSGTERR);
                return;
            }
            filePath = conn[0].Trim();
            if (conn.Length > 1 && !string.IsNullOrWhiteSpace(conn[1]))
            {
                fileSearchPattern = conn[1].Trim();
            }
            // 네트워크 공유 폴더의 경우 3, 4 번째에 각각 ID, PW 설정
            if (conn.Length == 4)
            {
                winID = conn[2].Trim();
                winPW = conn[3].Trim();
                PC00U01.ExecuteNetUse(filePath, winID, winPW);
            }

            // 1 = *dif*.*, 2 = *wti*.*, 3 = *aif*.*
            string[] extraInfos = m_ExtraInfo.Split(',');
            // 1 = *dif*.*
            foreach (string msgTypeInfo in extraInfos)
            {
                string[] items = msgTypeInfo.Trim().Split('=');
                if (items.Length < 2)
                {
                    continue;
                }
                if (!int.TryParse(items[0], out int msgType))
                {
                    continue;
                }
                if (!dicMessageType.ContainsKey(msgType))
                {
                    dicMessageType.Add(msgType, new WildcardPattern(items[1].Trim(), WildcardOptions.IgnoreCase));
                }
            }
            if (m_AutoRun == true)
            {
                m_Thd = new Thread(ThreadJob);
                m_Thd.Start();
            }
        }
        protected virtual void ThreadJob()
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
                    string sData = GetFileRawData(fi);
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
                    EnQueue(msgType, data);
                    listViewMsg.UpdateMsg($"{m_Name}({msgType}) Data has been enqueued", true, true);
                    fileLog.WriteData(data, "EnQ", $"{m_Name}({msgType})");
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
        protected int GetMessageType(string fileName)
        {
            foreach(KeyValuePair<int, WildcardPattern> kvp in dicMessageType)
            {
                if (kvp.Value.IsMatch(fileName))
                {
                    return kvp.Key;
                }
            }

            return -1;
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
        /// <summary>
        /// 파일에서 읽은 데이터에 대한 처리를 파생클래스에서 재정의 하여 사용. 데이터에 대한 처리가 필요 없는 경우 재정의 필요 없음
        /// </summary>
        /// <param name="sbData"></param>
        /// <returns></returns>
        public virtual string GetFileData(string sbData)
        {
            UpdateEquipmentProgDateTime(IF_STATUS.Normal);
            // ACC_Vi-Cell, avgBackgoundIntensity 구하기 위한 로직 (해당 값 사용안함)
            if (m_Type.Equals("ACC_Vi-Cell"))
            {
                sbData = ACC_Vi_Cell(sbData);
            }
            return sbData;
        }

        private string ACC_Vi_Cell(string data)
        {
            List<string> listString = data.Split(new string[] { Environment.NewLine }, StringSplitOptions.None).ToList<string>();

            string images = string.Empty;
            string backgoundIntensitySum = string.Empty;
            int avgBackgoundIntensity = 0;

            foreach (string line in listString)
            {
                if (line.StartsWith("Images"))
                {
                    images = line.Split(':')[1].Trim();
                }
                if (line.StartsWith("Background intensity sum"))
                {
                    backgoundIntensitySum = line.Split(':')[1].Trim();
                }
            }

            if (int.TryParse(images, out int nImages))
            {
                if (int.TryParse(backgoundIntensitySum, out int nBackgoundIntensitySum))
                {
                    avgBackgoundIntensity =  (int)Math.Round((double)nBackgoundIntensitySum / nImages);
                }
            }
            string sMsgTemp = string.Join(System.Environment.NewLine, listString);
            if (!sMsgTemp.Contains("Assay value"))
            {
                int pos = listString.FindIndex(p => p.Contains("Decluster degree"))+1;
                listString.Insert(pos,  string.Empty);
            }
            listString.Insert(49, avgBackgoundIntensity.ToString());

            return string.Join(Environment.NewLine, listString);
        }

        public virtual string GetFileRawData(FileInfo fileInfo)
        {
            try
            {
                using (FileStream fs = fileInfo.Open(FileMode.Open, FileAccess.Read, FileShare.None))
                {
                    if (fs != null)
                    {
                        listViewMsg.UpdateMsg($"Open File {fileInfo.Name} ({fileInfo.LastWriteTime:yyyy-MM-dd HH:mm:ss.fffffff})", false, true);

                        StringBuilder sbData = new StringBuilder();
                        byte[] b = new byte[1024];
                        UTF8Encoding temp = new UTF8Encoding(true);
                        int count;
                        while ((count = fs.Read(b, 0, b.Length)) > 0)
                        {
                            sbData.Append(temp.GetString(b, 0, count));
                        }
                        dtCurrFileWriteTime = fileInfo.LastWriteTime;
                        Debug.WriteLine(dtCurrFileWriteTime.ToString());
                        return sbData.ToString();
                    }
                }
            }
            catch (Exception ex)
            {
                listViewMsg.UpdateMsg(ex.ToString(), false, true, true, PC00D01.MSGTERR);
            }
            return null;
        }

        private string FileProcess()
        {
            List<FileInfo> listFileInfo = new List<FileInfo>();

            DirectoryInfo di = new DirectoryInfo(filePath);
            FileInfo[] fileInfo = di.GetFiles(fileSearchPattern);

            foreach (FileInfo fi in fileInfo)
            {
                if (dtLastEnQueuedFileWriteTime.CompareTo(fi.LastWriteTime) < 0)
                {
                    listFileInfo.Add(fi);
                }
            }

            listFileInfo.Sort((x, y) => x.LastWriteTime.CompareTo(y.LastWriteTime));
            
            if (listFileInfo.Count > 0)
            {
                using (FileStream fs = listFileInfo[0].Open(FileMode.Open, FileAccess.Read, FileShare.None))
                {
                    if (fs != null)
                    {
                        listViewMsg.UpdateMsg($"Last Enqueued file write time ({dtLastEnQueuedFileWriteTime:yyyy-MM-dd HH:mm:ss.fffffff}). ({listFileInfo.Count}/{fileInfo.Length}) files.", false, true);
                        listViewMsg.UpdateMsg($"Open File {listFileInfo[0].Name} ({listFileInfo[0].LastWriteTime:yyyy-MM-dd HH:mm:ss.fffffff})", false, true);

                        StringBuilder sbData = new StringBuilder();
                        byte[] b = new byte[1024];
                        UTF8Encoding temp = new UTF8Encoding(true);
                        int count;
                        while ((count = fs.Read(b, 0, b.Length)) > 0)
                        {
                            sbData.Append(temp.GetString(b, 0, count));
                        }
                        dtCurrFileWriteTime = listFileInfo[0].LastWriteTime;
                        Debug.WriteLine(dtCurrFileWriteTime.ToString());
                        return sbData.ToString();
                    }
                }
            }
            return string.Empty;
        }

        protected DateTime GetLastEnqueuedFileWriteTime()
        {
            string dtString = m_sqlBiz.ReadSTCommon(m_Name, "LastEnqueudFileWriteTime");//  PC00U01.ReadConfigValue("LastEnqueudFileWriteTime", m_Name, $@".\CFG\{m_Type}.ini");
            PC00U01.TryParseExact(dtString, out DateTime dt);
            listViewMsg.UpdateMsg($"Read last enqueued file write time : {dt:yyyy-MM-dd HH:mm:ss.fffffff})", false, true);
            return dt;
        }

        protected bool SetLastEnqueuedFileWriteTime(DateTime dt)
        {
            //if (!PC00U01.WriteConfigValue("LastEnqueudFileWriteTime", m_Name, $@".\CFG\{m_Type}.ini", $"{dt:yyyy-MM-dd HH:mm:ss.fffffff}"))
            if (!m_sqlBiz.WriteSTCommon(m_Name, "LastEnqueudFileWriteTime", $"{dt:yyyy-MM-dd HH:mm:ss.fffffff}"))
            {
                listViewMsg.UpdateMsg($"Error to write last enqueued file write time to INI file", false, true);
                return false;
            }
            listViewMsg.UpdateMsg($"Write last enqueued file write time : {dt:yyyy-MM-dd HH:mm:ss.fffffff})", false, true);
            return true;
        }
    }
}
