using DataSpider.PC00.PT;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DataSpider.FailoverManager
{
    public partial class MainForm : Form
    {
//        EventLog eventLog = null;
        FileLog m_Logger = null;
        private PC00Z01 sqlBiz = new PC00Z01();
        public bool m_bTermial = false;
        Thread threadFailover = null;
        int MY_ID = -1;

        Dictionary<string, Process> m_ProcessList = new Dictionary<string, Process>();

        StringBuilder strQuery = new StringBuilder();
        string errCode = string.Empty;
        string errText = string.Empty;

        public MainForm()
        {
            InitializeComponent();

            m_Logger = new FileLog("FailoverManager");
            m_Logger.SetDbLogger("FailoverManager");
        }

        private void button1_Click(object sender, EventArgs e)
        {

        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;
            threadFailover = new Thread(FailoverThread);
            threadFailover.Start();
        }

        private void FailoverThread()
        {
            m_Logger.WriteLog($"FailoverThread Start  MY_ID={MY_ID} ");
            string errCode = string.Empty;
            string errText = string.Empty;
            StringBuilder strQuery = new StringBuilder();
            DateTime ScanTime = DateTime.Now;
            DateTime MinuteScanTime = DateTime.Now;
            string CurDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");


            m_Logger.WriteLog(" EXECUTE DSFM_Initialization ");
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

                    //if ((DateTime.Now - MinuteScanTime).TotalSeconds > 10)
                    //{
                    //    // 일정한 주기로 ST_...._CD 테이블에 Failover Manager 상태를 기록
                    //    strQuery = new StringBuilder();
                    //    strQuery.Append($" EXECUTE CheckEquipmentStatus ");
                    //    CFW.Data.MsSqlDbAccess.ExecuteNonQuery(strQuery.ToString(), null, CommandType.Text, ref errCode, ref errText);
                    //    MinuteScanTime = DateTime.Now;
                    //}
                    if ((DateTime.Now - ScanTime).TotalSeconds > 2)
                    {
                        ScanTime = DateTime.Now;
                        //데이타 조회
                        DataTable dtStatus = sqlBiz.GetTableInfo($" EXEC GetFailoverInfo ", ref errCode, ref errText);
                        if (dtStatus == null) continue;
                        if (dtStatus.Rows.Count != m_ProcessList.Count)
                        {
                            m_Logger.WriteLog(" ProcessList Changed! ");
                            Dictionary<string, Process> m_ProcessListOld = m_ProcessList;
                            Process prc = null;
                            m_ProcessList.Clear();
                            foreach (DataRow dr in dtStatus.Rows)
                            {
                                m_ProcessList.Add((string)dr["EQUIP_NM"], null);
                                if (m_ProcessListOld.TryGetValue((string)dr["EQUIP_NM"], out prc))
                                {
                                    m_ProcessList[(string)dr["EQUIP_NM"]] = m_ProcessListOld[(string)dr["EQUIP_NM"]];
                                }
                            }
                        }
                        textBox1.Text = "";
                        foreach (DataRow dr in dtStatus.Rows)
                        {
                            // 프로그램이 종료되었으면 null
                            if (IsProcessTerminated(m_ProcessList[(string)dr["EQUIP_NM"]])) m_ProcessList[(string)dr["EQUIP_NM"]] = null;

                            if (FAILOVER_MODE.AUTO == (int)dr["FAILOVER_MODE"])  // FAILOVER_MODE=AUTO
                            {
                                if (MY_ID == (int)dr["ACTIVE_SERVER"])
                                {
                                    // 99 <== 이 경우는 잘 동작함.    99 !=  이 경우는 문제가 있음.
                                    if (99 == (int)dr["PROG_STATUS"] )
                                    {
                                        //Execute Program
                                        if (IsProcessTerminated(m_ProcessList[(string)dr["EQUIP_NM"]]))
                                        {
                                            try
                                            {
                                                string[] FilePath = ((string)dr["FILE_PATH"]).Split(' ');
                                                Process process = Process.Start(new ProcessStartInfo(FilePath[0], FilePath[1])
                                                {
                                                    WindowStyle = ProcessWindowStyle.Normal,
                                                    CreateNoWindow = false,
                                                    UseShellExecute = true
                                                });
                                                m_ProcessList[(string)dr["EQUIP_NM"]] = process;
                                                m_Logger.WriteLog($" {(string)dr["EQUIP_NM"]} Collector Executed");
                                            }
                                            catch (Exception ex)
                                            {
                                                m_Logger.WriteLog($" {ex.ToString()} ");
                                            }
                                        }
                                    }
                                }
                                else
                                { // ActiveServer != MY_ID
                                    if (99 != (int)dr["PROG_STATUS"] )
                                    {
                                        textBox1.Text += string.Format($"{(string)dr["EQUIP_NM"]} : 0=={(int)dr[$"STOP_REQ{MY_ID}"]}  && {IsProcessTerminated(m_ProcessList[(string)dr["EQUIP_NM"]])}");
                                        if (IsProcessTerminated(m_ProcessList[(string)dr["EQUIP_NM"]]))
                                        {
                                            if (0 < (int)dr[$"STOP_REQ{MY_ID}"])
                                            {
                                                strQuery = new StringBuilder();
                                                strQuery.Append($" UPDATE MA_FAILOVER_CD SET STOP_REQ{MY_ID}=0 WHERE EQUIP_NM='{dr["EQUIP_NM"]}'  ");
                                                CFW.Data.MsSqlDbAccess.ExecuteNonQuery(strQuery.ToString(), null, CommandType.Text, ref errCode, ref errText);
                                            }
                                        }
                                        else
                                        {// IsProcessTerminated(m_ProcessList[(string)dr["EQUIP_NM"]])==false
                                            if (0 == (int)dr[$"STOP_REQ{MY_ID}"])
                                            {
                                                //Stop Req=1
                                                //Stop_Req_time=Now
                                                strQuery = new StringBuilder();
                                                strQuery.Append($" UPDATE MA_FAILOVER_CD SET STOP_REQ{MY_ID}=1, STOP_REQ_TIME{MY_ID}=GETDATE() WHERE EQUIP_NM='{dr["EQUIP_NM"]}'  ");
                                                CFW.Data.MsSqlDbAccess.ExecuteNonQuery(strQuery.ToString(), null, CommandType.Text, ref errCode, ref errText);
                                                m_Logger.WriteLog($" {(string)dr["EQUIP_NM"]} {strQuery.ToString()} ");
                                            }
                                            else
                                            { // STOP_REQ == 1
                                                if ((DateTime.Now - (DateTime)dr[$"STOP_REQ_TIME{MY_ID}"]).Seconds > (int)dr["STOP_WAIT"])
                                                {
                                                    // Kill DSC
                                                    // 기억된 process가 있으면 기억된 process로 kill
                                                    if (IsProcessTerminated(m_ProcessList[(string)dr["EQUIP_NM"]]) == false)
                                                    {
                                                        try
                                                        {
                                                            Process prc = m_ProcessList[(string)dr["EQUIP_NM"]];
                                                            prc.Kill();
                                                            prc.WaitForExit();
                                                            m_Logger.WriteLog($" {(string)dr["EQUIP_NM"]} Collector Killed");
                                                            m_ProcessList[(string)dr["EQUIP_NM"]] = null;
                                                            // STOP_REQ=0
                                                            strQuery = new StringBuilder();
                                                            strQuery.Append($" UPDATE MA_FAILOVER_CD SET STOP_REQ{MY_ID}=0 WHERE EQUIP_NM='{dr["EQUIP_NM"]}'  ");
                                                            CFW.Data.MsSqlDbAccess.ExecuteNonQuery(strQuery.ToString(), null, CommandType.Text, ref errCode, ref errText);
                                                        }
                                                        catch (Exception ex)
                                                        {
                                                            m_Logger.WriteLog($" {ex.ToString()} ");
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                            else
                            {//FAILOVER_MODE.MANL == (int)dr["FAILOVER_MODE"]
                                if (1 == (int)dr[$"RUN_REQ{MY_ID}"])
                                {
                                    //Execute Program
                                    if (IsProcessTerminated(m_ProcessList[(string)dr["EQUIP_NM"]])==true )
                                    {
                                        try
                                        {
                                            string[] FilePath = ((string)dr["FILE_PATH"]).Split(' ');
                                            Process process = Process.Start(new ProcessStartInfo(FilePath[0], FilePath[1])
                                            {
                                                WindowStyle = ProcessWindowStyle.Normal,
                                                CreateNoWindow = false,
                                                UseShellExecute = true
                                            });
                                            m_ProcessList[(string)dr["EQUIP_NM"]] = process;
                                            m_Logger.WriteLog($" {(string)dr["EQUIP_NM"]} Collector Executed");
                                        }
                                        catch (Exception ex)
                                        {
                                            m_Logger.WriteLog($" {ex.ToString()} ");
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
                                            m_Logger.WriteLog($" {(string)dr["EQUIP_NM"]} Collector Killed");
                                        }
                                        catch (Exception ex)
                                        {
                                            m_Logger.WriteLog($" {ex.ToString()} ");
                                        }
                                    }
                                    m_ProcessList[(string)dr["EQUIP_NM"]] = null;
                                    strQuery = new StringBuilder();
                                    strQuery.Append($" UPDATE MA_FAILOVER_CD SET PROG_STATUS=99, STOP_REQ{MY_ID}=0  WHERE EQUIP_NM='{dr["EQUIP_NM"]}'  ");
                                    CFW.Data.MsSqlDbAccess.ExecuteNonQuery(strQuery.ToString(), null, CommandType.Text, ref errCode, ref errText);
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    m_Logger.WriteLog($" {ex.ToString()} ");
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
                    m_Logger.WriteLog($" {kvp.Key} STOP_REQ : {strQuery.ToString()} ");
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
                    m_Logger.WriteLog($" {kvp.Key}  Collector Killed ");
                }
            }
            m_Logger.WriteLog("FailoverThread End");
        }

        public bool IsProcessTerminated(Process prc)
        {
            bool bReturn = false;
            if (prc == null || prc.HasExited == true) bReturn = true;
            return bReturn;
        }


        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            m_bTermial = true;
            Thread.Sleep(1000);
            m_Logger.WriteLog($"MainForm_FormClosing");
        }

        private void btn1_mode_Click(object sender, EventArgs e)
        {
            strQuery = new StringBuilder();
            strQuery.Append($" UPDATE MA_FAILOVER_CD SET FAILOVER_MODE=(FAILOVER_MODE+1)%2 WHERE EQUIP_NM='P4S-EQ-0001' ");
            CFW.Data.MsSqlDbAccess.ExecuteNonQuery(strQuery.ToString(), null, CommandType.Text, ref errCode, ref errText);
        }

        private void btn1_show_Click(object sender, EventArgs e)
        {
            strQuery = new StringBuilder();
            strQuery.Append($" UPDATE MA_FAILOVER_CD SET HIDE_SHOW=1 WHERE EQUIP_NM='P4S-EQ-0001' ");
            CFW.Data.MsSqlDbAccess.ExecuteNonQuery(strQuery.ToString(), null, CommandType.Text, ref errCode, ref errText);

        }

        private void btn1_hide_Click(object sender, EventArgs e)
        {
            strQuery = new StringBuilder();
            strQuery.Append($" UPDATE MA_FAILOVER_CD SET HIDE_SHOW=0 WHERE EQUIP_NM='P4S-EQ-0001' ");
            CFW.Data.MsSqlDbAccess.ExecuteNonQuery(strQuery.ToString(), null, CommandType.Text, ref errCode, ref errText);

        }

        private void btn1_run_req_Click(object sender, EventArgs e)
        {
            strQuery = new StringBuilder();
            strQuery.Append($" UPDATE MA_FAILOVER_CD SET RUN_REQ0=1 WHERE EQUIP_NM='P4S-EQ-0001' AND FAILOVER_MODE=0 ");
            CFW.Data.MsSqlDbAccess.ExecuteNonQuery(strQuery.ToString(), null, CommandType.Text, ref errCode, ref errText);

        }
        private void btn1_run_req1_Click(object sender, EventArgs e)
        {
            strQuery = new StringBuilder();
            strQuery.Append($" UPDATE MA_FAILOVER_CD SET RUN_REQ1=1 WHERE EQUIP_NM='P4S-EQ-0001' AND FAILOVER_MODE=0 ");
            CFW.Data.MsSqlDbAccess.ExecuteNonQuery(strQuery.ToString(), null, CommandType.Text, ref errCode, ref errText);

        }

        private void btn1_stop_req_Click(object sender, EventArgs e)
        {
            strQuery = new StringBuilder();
            strQuery.Append($" UPDATE MA_FAILOVER_CD SET STOP_REQ0=1 WHERE EQUIP_NM='P4S-EQ-0001' AND FAILOVER_MODE=0 ");
            CFW.Data.MsSqlDbAccess.ExecuteNonQuery(strQuery.ToString(), null, CommandType.Text, ref errCode, ref errText);
        }
        private void btn1_stop_req1_Click(object sender, EventArgs e)
        {
            strQuery = new StringBuilder();
            strQuery.Append($" UPDATE MA_FAILOVER_CD SET STOP_REQ1=1 WHERE EQUIP_NM='P4S-EQ-0001' AND FAILOVER_MODE=0 ");
            CFW.Data.MsSqlDbAccess.ExecuteNonQuery(strQuery.ToString(), null, CommandType.Text, ref errCode, ref errText);

        }

        private void btn2_mode_Click(object sender, EventArgs e)
        {
            strQuery = new StringBuilder();
            strQuery.Append($" UPDATE MA_FAILOVER_CD SET FAILOVER_MODE=(FAILOVER_MODE+1)%2 WHERE EQUIP_NM='P4S-EQ-0002' ");
            CFW.Data.MsSqlDbAccess.ExecuteNonQuery(strQuery.ToString(), null, CommandType.Text, ref errCode, ref errText);
        }

        private void btn2_show_Click(object sender, EventArgs e)
        {
            strQuery = new StringBuilder();
            strQuery.Append($" UPDATE MA_FAILOVER_CD SET HIDE_SHOW=1 WHERE EQUIP_NM='P4S-EQ-0002' ");
            CFW.Data.MsSqlDbAccess.ExecuteNonQuery(strQuery.ToString(), null, CommandType.Text, ref errCode, ref errText);

        }

        private void btn2_hide_Click(object sender, EventArgs e)
        {
            strQuery = new StringBuilder();
            strQuery.Append($" UPDATE MA_FAILOVER_CD SET HIDE_SHOW=0 WHERE EQUIP_NM='P4S-EQ-0002' ");
            CFW.Data.MsSqlDbAccess.ExecuteNonQuery(strQuery.ToString(), null, CommandType.Text, ref errCode, ref errText);


        }

        private void btn2_run_req_Click(object sender, EventArgs e)
        {
            strQuery = new StringBuilder();
            strQuery.Append($" UPDATE MA_FAILOVER_CD SET RUN_REQ=1 WHERE EQUIP_NM='P4S-EQ-0002' AND FAILOVER_MODE=0 ");
            CFW.Data.MsSqlDbAccess.ExecuteNonQuery(strQuery.ToString(), null, CommandType.Text, ref errCode, ref errText);

        }

        private void btn2_stop_req_Click(object sender, EventArgs e)
        {
            strQuery = new StringBuilder();
            strQuery.Append($" UPDATE MA_FAILOVER_CD SET STOP_REQ0=1 WHERE EQUIP_NM='P4S-EQ-0002' AND FAILOVER_MODE=0 ");
            CFW.Data.MsSqlDbAccess.ExecuteNonQuery(strQuery.ToString(), null, CommandType.Text, ref errCode, ref errText);

        }
        private void btn2_stop_req1_Click(object sender, EventArgs e)
        {
            strQuery = new StringBuilder();
            strQuery.Append($" UPDATE MA_FAILOVER_CD SET STOP_REQ1=1 WHERE EQUIP_NM='P4S-EQ-0002' AND FAILOVER_MODE=0 ");
            CFW.Data.MsSqlDbAccess.ExecuteNonQuery(strQuery.ToString(), null, CommandType.Text, ref errCode, ref errText);
        }

        private void btn1_active_server_Click(object sender, EventArgs e)
        {
            strQuery = new StringBuilder();
            strQuery.Append($" UPDATE MA_FAILOVER_CD SET ACTIVE_SERVER=(ACTIVE_SERVER+1)%2 WHERE EQUIP_NM='P4S-EQ-0001' ");
            CFW.Data.MsSqlDbAccess.ExecuteNonQuery(strQuery.ToString(), null, CommandType.Text, ref errCode, ref errText);

        }

        private void btn2_active_server_Click(object sender, EventArgs e)
        {
            strQuery = new StringBuilder();
            strQuery.Append($" UPDATE MA_FAILOVER_CD SET ACTIVE_SERVER=(ACTIVE_SERVER+1)%2 WHERE EQUIP_NM='P4S-EQ-0002' ");
            CFW.Data.MsSqlDbAccess.ExecuteNonQuery(strQuery.ToString(), null, CommandType.Text, ref errCode, ref errText);

        }

        private void 열기ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Show();
            this.WindowState = FormWindowState.Normal;
            notifyIcon1.Visible = false;

        }

        private void 종료ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            m_bTermial = true;
            Thread.Sleep(1000);
            m_Logger.WriteLog($"Application.Exit");
            Application.Exit();
        }

        private void MainForm_Resize(object sender, EventArgs e)
        {
            if (FormWindowState.Minimized == this.WindowState)
            {
                notifyIcon1.Visible = true; // tray icon 표시
                this.Hide();
                this.ShowInTaskbar = false; // 작업 표시줄 표시
            }
            else if (FormWindowState.Normal == this.WindowState)
            {
                notifyIcon1.Visible = false;
                this.ShowInTaskbar = true; // 작업 표시줄 표시
            }

        }

        private void notifyIcon1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            this.Show();
            this.WindowState = FormWindowState.Normal;
            notifyIcon1.Visible = false;
        }
    }
}
