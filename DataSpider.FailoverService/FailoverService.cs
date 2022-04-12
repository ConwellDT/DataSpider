using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DataSpider.PC00.PT;

namespace DataSpider.FailoverService
{
    public partial class FailoverService : ServiceBase
    {
        EventLog eventLog = null;
        private PC00Z01 sqlBiz = new PC00Z01();
        public bool m_bTermial = false;
        Thread threadFailover = null;
        int MY_ID = -1;

        Dictionary<string, Process> m_ProcessList = new Dictionary<string, Process>();

        StringBuilder strQuery = new StringBuilder();
        string errCode = string.Empty;
        string errText = string.Empty;

        ServiceStatus serviceStatus = new ServiceStatus();

        public enum ServiceState
        {
            SERVICE_STOPPED = 0x00000001,
            SERVICE_START_PENDING = 0x00000002,
            SERVICE_STOP_PENDING = 0x00000003,
            SERVICE_RUNNING = 0x00000004,
            SERVICE_CONTINUE_PENDING = 0x00000005,
            SERVICE_PAUSE_PENDING = 0x00000006,
            SERVICE_PAUSED = 0x00000007,
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct ServiceStatus
        {
            public int dwServiceType;
            public ServiceState dwCurrentState;
            public int dwControlsAccepted;
            public int dwWin32ExitCode;
            public int dwServiceSpecificExitCode;
            public int dwCheckPoint;
            public int dwWaitHint;
        };

        [DllImport("advapi32.dll", SetLastError = true)]
        private static extern bool SetServiceStatus(System.IntPtr handle, ref ServiceStatus serviceStatus);

        public FailoverService()
        {
            InitializeComponent();
            if (!EventLog.SourceExists("DataSpider"))
            {
                EventLog.CreateEventSource( "DataSpider", "FailoverLog");
            }
            eventLog = new EventLog("FailoverLog", ".", "DataSpider");
        }

        protected override void OnStart(string[] args)
        {
            // Update the service state to Start Pending.
            serviceStatus.dwCurrentState = ServiceState.SERVICE_START_PENDING;
            serviceStatus.dwWaitHint = 100000;
            SetServiceStatus(this.ServiceHandle, ref serviceStatus);

            //
            eventLog.WriteEntry("OnStart");

            //MY_ID = int.Parse(ConfigHelper.GetAppSetting("MY_ID"));
            threadFailover = new Thread(FailoverThread);
            threadFailover.Start();

            // Update the service state to Running.
            serviceStatus.dwCurrentState = ServiceState.SERVICE_RUNNING;
            SetServiceStatus(this.ServiceHandle, ref serviceStatus);
        }

        protected override void OnStop()
        {
            // Update the service state to Stop Pending.
            serviceStatus.dwCurrentState = ServiceState.SERVICE_STOP_PENDING;
            serviceStatus.dwWaitHint = 100000;
            SetServiceStatus(this.ServiceHandle, ref serviceStatus);

            //
            eventLog.WriteEntry("OnStop");
            m_bTermial = true;
            Thread.Sleep(3000);

            // Update the service state to Stopped.
            serviceStatus.dwCurrentState = ServiceState.SERVICE_STOPPED;
            SetServiceStatus(this.ServiceHandle, ref serviceStatus);
        }


        private void FailoverThread()
        {
            eventLog.WriteEntry($"FailoverThread Start  MY_ID={MY_ID} ");
            string errCode = string.Empty;
            string errText = string.Empty;
            StringBuilder strQuery = new StringBuilder();
            DateTime ScanTime = DateTime.Now;
            DateTime MinuteScanTime = DateTime.Now;
            string CurDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");


            eventLog.WriteEntry(" EXECUTE DSFM_Initialization ");
            strQuery = new StringBuilder();
            strQuery.Append($" EXECUTE DSFM_Initialization ");
            CFW.Data.MsSqlDbAccess.ExecuteNonQuery(strQuery.ToString(), null, CommandType.Text, ref errCode, ref errText);

            while (!m_bTermial)
            {
                try
                {
                    if (MY_ID == -1)
                    {
                        MY_ID = sqlBiz.GetServerId(Environment.MachineName);
                        continue;
                    }

                    if ((DateTime.Now - MinuteScanTime).TotalSeconds > 10)
                    {
                        // 일정한 주기로 ST_...._CD 테이블에 Failover Manager 상태를 기록
                        strQuery = new StringBuilder();
                        strQuery.Append($" EXECUTE CheckEquipmentStatus ");
                        CFW.Data.MsSqlDbAccess.ExecuteNonQuery(strQuery.ToString(), null, CommandType.Text, ref errCode, ref errText);
                        MinuteScanTime = DateTime.Now;
                    }
                    if ((DateTime.Now - ScanTime).TotalSeconds > 2)
                    {
                        ScanTime = DateTime.Now;
                        //데이타 조회
                        DataTable dtStatus = sqlBiz.GetTableInfo($" EXEC GetFailoverInfo ", ref errCode, ref errText);
                        if (dtStatus == null) continue;
                        if (dtStatus.Rows.Count != m_ProcessList.Count)
                        {
                            eventLog.WriteEntry(" ProcessList Changed! ");
                            Dictionary<string, Process> m_ProcessListOld = m_ProcessList;
                            Process prc = null;
                            m_ProcessList.Clear();
                            foreach (DataRow dr in dtStatus.Rows)
                            {
                                m_ProcessList.Add((string)dr["EQUIP_NM"], null);                                   
                                if(m_ProcessListOld.TryGetValue((string)dr["EQUIP_NM"], out prc))
                                {
                                    m_ProcessList[(string)dr["EQUIP_NM"]] = m_ProcessListOld[(string)dr["EQUIP_NM"]];
                                }
                            }
                        }

                        foreach (DataRow dr in dtStatus.Rows)
                        {

                            if (FAILOVER_MODE.AUTO == (int)dr["FAILOVER_MODE"])  // FAILOVER_MODE=AUTO
                            {
                                if (MY_ID == (int)dr["ACTIVE_SERVER"])
                                {
                                    if (99 == (int)dr["PROG_STATUS"])
                                    {
                                        //Execute Program
                                        if (m_ProcessList[(string)dr["EQUIP_NM"]] == null ||
                                            m_ProcessList[(string)dr["EQUIP_NM"]].HasExited == true)
                                        {
                                            try
                                            {

                                                Process process = Process.Start(new ProcessStartInfo((string)dr["FILE_PATH"])
                                                {
                                                    WindowStyle = ProcessWindowStyle.Normal,
                                                    CreateNoWindow = false,
                                                    UseShellExecute = true
                                                });
                                                m_ProcessList[(string)dr["EQUIP_NM"]] = process;
                                                eventLog.WriteEntry($" {(string)dr["EQUIP_NM"]} Collector Executed");
                                            }
                                            catch(Exception ex)
                                            {
                                                eventLog.WriteEntry($" {ex.ToString()} ");
                                            }
                                        }
                                    }
                                }
                                else
                                { // ActiveServer != MY_ID
                                    if (99 != (int)dr["PROG_STATUS"])
                                    {
                                        if (0 == (int)dr[$"STOP_REQ{MY_ID}"])
                                        {
                                            //Stop Req=1
                                            //Stop_Req_time=Now
                                            strQuery = new StringBuilder();
                                            strQuery.Append($" UPDATE MA_FAILOVER_CD SET STOP_REQ{MY_ID}=1, STOP_REQ_TIME{MY_ID}=GETDATE() WHERE EQUIP_NM='{dr["EQUIP_NM"]}'  ");
                                            CFW.Data.MsSqlDbAccess.ExecuteNonQuery(strQuery.ToString(), null, CommandType.Text, ref errCode, ref errText);
                                            eventLog.WriteEntry($" {(string)dr["EQUIP_NM"]} {strQuery.ToString()} ");
                                        }
                                        else
                                        { // STOP_REQ == 1
                                            if ((DateTime.Now - (DateTime)dr[$"STOP_REQ_TIME{MY_ID}"]).Seconds > (int)dr["STOP_WAIT"])
                                            {
                                                // Kill DSC
                                                try
                                                {
                                                    // 기억된 process가 있으면 기억된 process로 kill
                                                    m_ProcessList.TryGetValue((string)dr["EQUIP_NM"], out Process prc);
                                                    if (prc != null)
                                                    {
                                                        prc.Kill();
                                                        prc.WaitForExit();
                                                        m_ProcessList[(string)dr["EQUIP_NM"]] = null;
                                                        eventLog.WriteEntry($" {(string)dr["EQUIP_NM"]} Collector Killed");
                                                    }
                                                }
                                                catch (Exception ex)
                                                {
                                                    eventLog.WriteEntry($" {ex.ToString()} ");
                                                }

                                                // STOP_REQ=0
                                                strQuery = new StringBuilder();
                                                strQuery.Append($" UPDATE MA_FAILOVER_CD SET STOP_REQ{MY_ID}=0 WHERE EQUIP_NM='{dr["EQUIP_NM"]}'  ");
                                                CFW.Data.MsSqlDbAccess.ExecuteNonQuery(strQuery.ToString(), null, CommandType.Text, ref errCode, ref errText);
                                            }
                                        }
                                    }
                                    else
                                    {
                                        m_ProcessList[(string)dr["EQUIP_NM"]] = null;
                                    }
                                }
                            }
                            else
                            {
                                if (1 == (int)dr[$"RUN_REQ{MY_ID}"])
                                {
                                    //Execute Program
                                    if (m_ProcessList[(string)dr["EQUIP_NM"]] == null ||
                                        m_ProcessList[(string)dr["EQUIP_NM"]].HasExited == true)
                                    {
                                        try
                                        {
                                            Process process = Process.Start(new ProcessStartInfo((string)dr["FILE_PATH"])
                                            {
                                                WindowStyle = ProcessWindowStyle.Normal,
                                                CreateNoWindow = false,
                                                UseShellExecute = true
                                            });
                                            m_ProcessList[(string)dr["EQUIP_NM"]] = process;
                                            eventLog.WriteEntry($" {(string)dr["EQUIP_NM"]} Collector Executed");
                                        }
                                        catch(Exception ex)
                                        {
                                            eventLog.WriteEntry($" {ex.ToString()} ");
                                        }
                                    }
                                    strQuery = new StringBuilder();
                                    strQuery.Append($" UPDATE MA_FAILOVER_CD SET RUN_REQ{MY_ID}=0  WHERE EQUIP_NM='{dr["EQUIP_NM"]}'  ");
                                    CFW.Data.MsSqlDbAccess.ExecuteNonQuery(strQuery.ToString(), null, CommandType.Text, ref errCode, ref errText);
                                }
                                if (1 == (int)dr[$"STOP_REQ{MY_ID}"])
                                {
                                    m_ProcessList.TryGetValue((string)dr["EQUIP_NM"], out Process prc);
                                    if (prc != null)
                                    {
                                        try
                                        {
                                            prc.Kill();
                                            prc.WaitForExit();
                                        }
                                        catch(Exception ex)
                                        {
                                            eventLog.WriteEntry($" {ex.ToString()} ");
                                        }
                                    }
                                    m_ProcessList[(string)dr["EQUIP_NM"]] = null;
                                    strQuery = new StringBuilder();
                                    strQuery.Append($" UPDATE MA_FAILOVER_CD SET STOP_REQ{MY_ID}=0  WHERE EQUIP_NM='{dr["EQUIP_NM"]}'  ");
                                    CFW.Data.MsSqlDbAccess.ExecuteNonQuery(strQuery.ToString(), null, CommandType.Text, ref errCode, ref errText);
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    eventLog.WriteEntry($" {ex.ToString()} ");
                }
                finally
                {
                    threadFailover.Join(500);
                }
            }
            foreach (KeyValuePair<string, Process> kvp in m_ProcessList)
            {
                Process prc = kvp.Value;
                if (prc != null)
                {
                    strQuery = new StringBuilder();
                    strQuery.Append($" UPDATE MA_FAILOVER_CD SET STOP_REQ{MY_ID}=1, STOP_REQ_TIME{MY_ID}=GETDATE() WHERE EQUIP_NM='{kvp.Key}'  ");
                    CFW.Data.MsSqlDbAccess.ExecuteNonQuery(strQuery.ToString(), null, CommandType.Text, ref errCode, ref errText);
                    eventLog.WriteEntry($" {kvp.Key} STOP_REQ : {strQuery.ToString()} ");
                }
            }
            Thread.Sleep(1000);
            foreach (KeyValuePair<string, Process> kvp in m_ProcessList)
            {
                Process prc = kvp.Value;
                if (prc != null)
                {
                    prc.Kill();
                    prc.WaitForExit();
                    eventLog.WriteEntry($" {kvp.Key}  Collector Killed ");
                }
            }
            eventLog.WriteEntry("FailoverThread End");
        }

 
    }
}
