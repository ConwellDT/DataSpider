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

namespace DataSpider.PC02.PT
{
    /// <summary>
    /// 중간파일 DB 저장
    /// </summary>
    public class PC02S01 : PC00B01
    {
        private DateTime dtLastUpdateProgDateTime = DateTime.MinValue;
        private int serverCode = 0;

        private string errorFilePath = string.Empty;
        private string lastErrorFileName = string.Empty;
        private string programName = string.Empty;

        public PC02S01()
        {
        }

        //public PC02S01(PC02F01 pOwner, string strPlantCode, string strDeviceId, string strDeviceNm, string strStationCd, string strGetPath, string strSetPath, string strBackupPath, string strProcId, int nCurNo, bool bAutoRun) : base(pOwner, strPlantCode, strDeviceId, strDeviceNm, strStationCd, strGetPath, strSetPath, strBackupPath, strProcId, nCurNo, bAutoRun)
        //{
        //}

        public PC02S01(IPC00F00 pOwner, string equipType, string equipName, string connectionInfo, string extraInfo, int nCurNo, bool bAutoRun = false) : base(pOwner, equipType, equipName, connectionInfo, extraInfo, nCurNo, bAutoRun)
        {
            Init();

            if (m_AutoRun == true)
            {
                m_Thd = new Thread(new ThreadStart(ThreadJob));
                m_Thd.Start();
            }
        }

        private void Init()
        {
            serverCode = m_sqlBiz.GetServerId(Environment.MachineName);
            programName = $"{System.Windows.Forms.Application.ProductName}{(serverCode == 0 ? "P" : "S")}";
            errorFilePath = $@"{m_ConnectionInfo}\DataFile_Error";
            if (!Directory.Exists(m_ConnectionInfo))
            {
                Directory.CreateDirectory(m_ConnectionInfo);
            }

            if (!Directory.Exists(errorFilePath))
            {
                Directory.CreateDirectory(errorFilePath);
            }
        }

        private void CheckErrorFile()
        {

            string errorFileDirectoryName = string.Empty;
            DirectoryInfo di = new DirectoryInfo(errorFilePath);
            try
            {
                List<FileInfo> listFileInfo = di.GetFiles("*.ttv", SearchOption.AllDirectories).ToList();
                if (listFileInfo.Count > 0)
                {
                    listFileInfo.Sort((x, y) => x.LastWriteTime.CompareTo(y.LastWriteTime));
                    errorFileDirectoryName = listFileInfo[0].DirectoryName.Substring(listFileInfo[0].DirectoryName.LastIndexOf(@"\") + 1);
                    if (!errorFileDirectoryName.Equals(lastErrorFileName))
                    {
                        listViewMsg.UpdateMsg($"New Error File : {errorFileDirectoryName}", true, false);
                        lastErrorFileName = errorFileDirectoryName;
                        m_sqlBiz.WriteSTCommon("ERROR_STATUS", programName, errorFileDirectoryName);
                    }
                }
                else
                {
                    if (!errorFileDirectoryName.Equals(lastErrorFileName))
                    {
                        lastErrorFileName = errorFileDirectoryName;
                        m_sqlBiz.WriteSTCommon("ERROR_STATUS", programName, errorFileDirectoryName);
                    }
                }

            }
            catch (Exception ex)
            {
                m_Owner.listViewMsg(m_Name, ex.ToString(), true, m_nCurNo, 6, true, PC00D01.MSGTERR);
                listViewMsg.UpdateMsg($"Exception in ThreadJob - ({ex})", false, true, true, PC00D01.MSGTERR);
                UpdateEquipmentProgDateTime(IF_STATUS.InternalError);
            }
        }

        private void ThreadJob()
        {
            string errCode = string.Empty;
            string errText = string.Empty;

            DirectoryInfo di = new DirectoryInfo(m_ConnectionInfo);
            string lineData = string.Empty;
            bool result = false;

            listViewMsg.UpdateStatus(true);
            listViewMsg.UpdateMsg("Thread started");

            while (!bTerminal)
            {
                try
                {
                    CheckErrorFile();

                    if (m_nCurNo == 0)
                        UpdateEquipmentProgDateTime(IF_STATUS.Normal);

                    // 파일 찾기
                    List<FileInfo> listFileInfo = di.GetFiles("*.ttv").ToList();
                    if (listFileInfo.Count < 1)
                    {
                        listViewMsg.UpdateMsg("No Files", true, false);
                        Thread.Sleep(1000);
                        continue;
                    }
                    listFileInfo.Sort((x, y) => x.LastWriteTime.CompareTo(y.LastWriteTime));
                    listViewMsg.UpdateMsg($"{listFileInfo.Count} Files", true, false);

                    foreach (FileInfo fi in listFileInfo)
                    {
                        //using (StreamReader sr = fi.OpenText())
                        using (StreamReader sr = new StreamReader(fi.FullName, dataEncoding))
                        {
                            m_Owner.listViewMsg(m_Name, $"Open File : {fi.Name}", false, m_nCurNo, 6, true, PC00D01.MSGTINF);
                            while (!sr.EndOfStream)
                            {
                                lineData = sr.ReadLine();
                                listViewMsg.UpdateMsg($"Data : {lineData}", false, true, true, PC00D01.MSGTINF);
                                string[] data = lineData.Split(',');
                                if (data.Length < 5)
                                    continue;
                                // 20210401, SHS, 값에 대해서는 공백 여부 체크 제외
                                //if (string.IsNullOrWhiteSpace(data[0]) || string.IsNullOrWhiteSpace(data[1]) || string.IsNullOrWhiteSpace(data[2]))
                                if (string.IsNullOrWhiteSpace(data[0]) || string.IsNullOrWhiteSpace(data[1]))
                                    continue;
                                // DB 저장
                                errCode = string.Empty;
                                errText = string.Empty;
                                //m_Owner.listViewMsg(m_Name, lineData, false, m_nCurNo, 6, true, PC00D01.MSGTINF);
                                // 20210416, SHS, 값이 ',' 가 포함된 경우 ',' 로 splite 되므로 첫번째 값만 값으로 처리되는 것 보완
                                // ',' 로 split 후 3번째 부터 마지막까지 데이터는 다시 ',' 로 join 하여 처리
                                //result = m_sqlBiz.InsertResult(data[0], data[1], data[2], ref errCode, ref errText);
                                string value = string.Join(",", data, 4, 1).Replace("'", "''");
                                //listViewMsg.UpdateMsg($"Value : |{value}|", false, true, true, PC00D01.MSGTINF);

                                result = m_sqlBiz.InsertResult(data[0], data[1], value, data[2], data[3], "", ref errCode, ref errText);
                                if (!result)
                                {
                                    //listViewMsg.UpdateMsg($"Failed to insert DB : {errText}", false, true, true, PC00D01.MSGTERR);
                                    break;
                                }
                            }
                        }

                        if (result)
                        {
                            listViewMsg.UpdateMsg(string.Format(PC00D01.SucceededDBStored, fi.Name), false);
                            // DB 저장 된 ttv 파일은 삭제
                            fi.Delete();
                            // ttv 파일 처리 실패시 해당 파일을 DataFile_Done 폴더로 이동 처리
                            //fi.MoveTo($@"{di.FullName}\DataFile_Done\{fi.Name}");                            
                        }
                        else
                        {
                            m_Owner.listViewMsg(m_Name, string.Format(PC00D01.FailedDBStore, $"{errText} - {fi.Name}"), true, m_nCurNo, 6, true, PC00D01.MSGTERR);
                            listViewMsg.UpdateMsg(string.Format(PC00D01.FailedDBStore, $"{errText} - {fi.Name}"), false, true, true, PC00D01.MSGTERR);
                            // ttv 파일 처리 실패시 DataFile_Error 폴더로 이동 처리
                            fi.MoveTo($@"{di.FullName}\DataFile_Error\{fi.Name}");
                            UpdateEquipmentProgDateTime(IF_STATUS.InvalidData);
                        }
                    }


                }
                catch (Exception ex)
                {
                    m_Owner.listViewMsg(m_Name, ex.ToString(), true, m_nCurNo, 6, true, PC00D01.MSGTERR);
                    listViewMsg.UpdateMsg($"Exception in ThreadJob - ({ex})", false, true, true, PC00D01.MSGTERR);
                    UpdateEquipmentProgDateTime(IF_STATUS.InternalError);
                }
                finally
                {

                }
                Thread.Sleep(10);
            }
            UpdateEquipmentProgDateTime(IF_STATUS.Unknown);
            listViewMsg.UpdateStatus(false);
            listViewMsg.UpdateMsg("Thread finished");
        }
        protected new bool UpdateEquipmentProgDateTime(IF_STATUS status = IF_STATUS.Normal)
        {
            string errCode = string.Empty;
            string errText = string.Empty;
            DateTime dtNow = DateTime.Now;
            if (dtNow.Subtract(dtLastUpdateProgDateTime).TotalSeconds > UpdateInterval || !lastStatus.Equals(status))
            {
                dtLastUpdateProgDateTime = dtNow;
                lastStatus = status;
                return m_sqlBiz.UpdateEquipmentProgDateTimePC02($"{System.Windows.Forms.Application.ProductName}{(serverCode == 0 ? "P" : "S")}", (int)status, ref errCode, ref errText);
            }
            return true;
        }
    }

}
