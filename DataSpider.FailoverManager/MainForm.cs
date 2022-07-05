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
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DataSpider.FailoverManager
{
    public partial class MainForm : Form
    {
        delegate void listViewMsgCallback(string pstrProcId, string pMsg, bool pbGridView, int pCurNo, int nSubitemNo, bool pbLogView, string pstrType);

        //        EventLog eventLog = null;
        MyClsLog m_Logger = null;
        private PC00Z01 sqlBiz = new PC00Z01();
        public bool m_bTermial = false;
        Thread threadFailover = null;
        int MY_ID = -1;

        int m_nLogLines = 10;
        string m_strLogFileName = Application.ProductName;//"ALIS";


        Dictionary<string, Process> m_ProcessList = new Dictionary<string, Process>();

        StringBuilder strQuery = new StringBuilder();
        string errCode = string.Empty;
        string errText = string.Empty;
        System.Windows.Forms.Timer m_tmLogLvUpd;          // Timer (Log ListView Update)

        public MainForm()
        {
            m_Logger = new MyClsLog(Application.ProductName);
            m_Logger.SetOwner(this);
            m_Logger.SetDbLogger(Application.ProductName);

            InitializeComponent();


            // Show Lines Text Input Only Number
            this.tbShowLines.MaxLength = 4;
            this.tbShowLines.KeyPress += new KeyPressEventHandler(tbShowLines_KeyPress);

            this.m_tmLogLvUpd = new System.Windows.Forms.Timer();
            this.m_tmLogLvUpd.Interval = PC00D01.ItvLogLvUpd;
            this.m_tmLogLvUpd.Tick += new EventHandler(m_tmLogLvUpd_Tick);


            this.cbInfo.CheckedChanged += new EventHandler(cbInfo_CheckedChanged);
            this.cbError.CheckedChanged += new EventHandler(cbError_CheckedChanged);
            this.cbDebug.CheckedChanged += new EventHandler(cbDebug_CheckedChanged);


        }

        // Timer Tick Evnet (Log ListView Update)
        void m_tmLogLvUpd_Tick(object sender, EventArgs e)
        {
           ReceivedDataLogListViewUpdate();
        }


        private void MainForm_Load(object sender, EventArgs e)
        {
            this.cbInfo.Checked = true;
            this.cbError.Checked = true;
            this.cbDebug.Checked = false;


            this.m_nLogLines = PC00D01.LogShowLinesDefault;
            this.tbShowLines.Text = m_nLogLines.ToString();


            ListViewColumnSet(); // 컬럼 세팅

            this.m_tmLogLvUpd.Start();



            this.WindowState = FormWindowState.Minimized;
            threadFailover = new Thread(FailoverThread);
            threadFailover.Start();
        }

        private void FailoverThread()
        {
            string errCode = string.Empty;
            string errText = string.Empty;
            StringBuilder strQuery = new StringBuilder();
            DateTime ScanTime = DateTime.Now;
            DateTime MinuteScanTime = DateTime.Now;
            string CurDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

            try
            {
                strQuery = new StringBuilder();
                strQuery.Append($" EXECUTE DSFM_Initialization ");
                m_Logger.WriteLog(strQuery.ToString());
                sqlBiz.ExecuteNonQuery(strQuery.ToString(), ref errCode, ref errText);
            }
            catch (Exception ex)
            {
                m_Logger.WriteLog($" {ex.ToString()} ");
            }

            while (!m_bTermial)
            {
                try
                {
                    if (MY_ID == -1)
                    {
                        MY_ID = sqlBiz.GetServerId(Environment.MachineName);
                        m_Logger.WriteLog($"FailoverThread MY_ID={MY_ID} ");
                        if (MY_ID == -1)
                        {
                            Thread.Sleep(5000);
                        }
                        continue;
                    }

                    //if ((DateTime.Now - MinuteScanTime).TotalSeconds > 10)
                    //{
                    //    // 일정한 주기로 ST_...._CD 테이블에 Failover Manager 상태를 기록
                    //    strQuery = new StringBuilder();
                    //    strQuery.Append($" EXECUTE CheckEquipmentStatus ");
                    //    sqlBiz.ExecuteNonQuery(strQuery.ToString(),  ref errCode, ref errText);
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
                        //textBox1.Text = "";
                        foreach (DataRow dr in dtStatus.Rows)
                        {
                            // 프로그램이 종료되었으면 null
                            //if (IsProcessTerminated(m_ProcessList[(string)dr["EQUIP_NM"]]))
                            //    m_ProcessList[(string)dr["EQUIP_NM"]] = null;

                            if (FAILOVER_MODE.AUTO == (int)dr["FAILOVER_MODE"])  // FAILOVER_MODE=AUTO
                            {
                                if (MY_ID == (int)dr["ACTIVE_SERVER"])
                                {
                                    // 99 <== 이 경우는 잘 동작함.    99 !=  이 경우는 문제가 있음.
                                    if (99 == (int)dr["PROG_STATUS"])
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
                                        //textBox1.Text += string.Format($"{(string)dr["EQUIP_NM"]} : 0=={(int)dr[$"STOP_REQ{MY_ID}"]}  && {IsProcessTerminated(m_ProcessList[(string)dr["EQUIP_NM"]])}");
                                        if (IsProcessTerminated(m_ProcessList[(string)dr["EQUIP_NM"]]))
                                        {
                                            if (0 < (int)dr[$"STOP_REQ{MY_ID}"])
                                            {
                                                strQuery = new StringBuilder();
                                                strQuery.Append($" UPDATE MA_FAILOVER_CD SET STOP_REQ{MY_ID}=0 WHERE EQUIP_NM='{dr["EQUIP_NM"]}'  ");
                                                sqlBiz.ExecuteNonQuery(strQuery.ToString(),  ref errCode, ref errText);
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
                                                sqlBiz.ExecuteNonQuery(strQuery.ToString(), ref errCode, ref errText);
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
                                                            sqlBiz.ExecuteNonQuery(strQuery.ToString(),  ref errCode, ref errText);
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
                                    m_Logger.WriteLog($" {(string)dr["EQUIP_NM"]} Collector RUN_REQ Has Detected!");
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
                                    else
                                    {
                                        m_Logger.WriteLog($" {(string)dr["EQUIP_NM"]} Collector still Running");
                                        if(m_ProcessList[(string)dr["EQUIP_NM"]]==null)
                                            m_Logger.WriteLog($" process == null ");
                                        else
                                        {
                                            if (m_ProcessList[(string)dr["EQUIP_NM"]].HasExited == false)
                                                m_Logger.WriteLog($" HasExited == false ");
                                            else
                                                m_Logger.WriteLog($" HasExited == true ");
                                        }                                            
                                    }
                                    strQuery = new StringBuilder();
                                    strQuery.Append($" UPDATE MA_FAILOVER_CD SET RUN_REQ{MY_ID}=0  WHERE EQUIP_NM='{dr["EQUIP_NM"]}'  ");
                                    sqlBiz.ExecuteNonQuery(strQuery.ToString(), ref errCode, ref errText);
                                }
                                if (1 == (int)dr[$"STOP_REQ{MY_ID}"])
                                {
                                    m_Logger.WriteLog($" {(string)dr["EQUIP_NM"]} Collector STOP_REQ Has Detected!");
                                    m_ProcessList.TryGetValue((string)dr["EQUIP_NM"], out Process prc);
                                    if (prc != null && IsProcessTerminated(prc) == false)
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
                                    m_Logger.WriteLog($" {(string)dr["EQUIP_NM"]} Collector Has Exited!");
                                    m_ProcessList[(string)dr["EQUIP_NM"]] = null;
                                    strQuery = new StringBuilder();
                                    strQuery.Append($" UPDATE MA_FAILOVER_CD SET PROG_STATUS=99, STOP_REQ{MY_ID}=0  WHERE EQUIP_NM='{dr["EQUIP_NM"]}'  ");
                                    sqlBiz.ExecuteNonQuery(strQuery.ToString(), ref errCode, ref errText);
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
                    Thread.Sleep(50);
                }
            }
            try
            {
                foreach (KeyValuePair<string, Process> kvp in m_ProcessList)
                {
                    Process prc = kvp.Value;
                    if (prc != null)
                    {
                        strQuery = new StringBuilder();
                        strQuery.Append($" UPDATE MA_FAILOVER_CD SET STOP_REQ{MY_ID}=1, STOP_REQ_TIME{MY_ID}=GETDATE() WHERE EQUIP_NM='{kvp.Key}'  ");
                        sqlBiz.ExecuteNonQuery(strQuery.ToString(), ref errCode, ref errText);
                        m_Logger.WriteLog($" {kvp.Key} STOP_REQ : {strQuery.ToString()} ");
                    }
                }
                Thread.Sleep(1000);
                foreach (KeyValuePair<string, Process> kvp in m_ProcessList)
                {
                    Process prc = kvp.Value;
                    if( prc!=null && IsProcessTerminated(prc) == false)                    
                    {
                        try
                        {
                            prc.Kill();
                            prc.WaitForExit();
                        }
                        catch (Exception ex)
                        {
                            m_Logger.WriteLog($" {ex.ToString()} ");
                        }
                        m_Logger.WriteLog($" {kvp.Key}  Collector Killed ");
                    }
                }
                m_Logger.WriteLog("FailoverThread End");
            }
            catch (Exception ex)
            {
                m_Logger.WriteLog($" {ex.ToString()} ");
            }
            finally
            {
            }
        }

        public bool IsProcessTerminated(Process prc)
        {
            bool bReturn = false;
            if (prc == null || prc.HasExited == true) bReturn = true;
            return bReturn;
        }

        private void MainForm_Resize(object sender, EventArgs e)
        {
            if (FormWindowState.Minimized == this.WindowState)
            {
                notifyIcon1.Visible = true; // tray icon 표시
                this.Hide();
                this.ShowInTaskbar = false; // 작업 표시줄 표시
            }
            else //if (FormWindowState.Normal == this.WindowState)
            {
                //notifyIcon1.Visible = false;
                this.Show();
                this.ShowInTaskbar = true; // 작업 표시줄 표시
            }

        }

        private void notifyIcon1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            this.Show();
            this.WindowState = FormWindowState.Normal;
            //notifyIcon1.Visible = false;
        }

        #region <Update Listview Items>
        // Infomation ListView Data Update

        // Log ListView Update
        private void ReceivedDataLogListViewUpdate()
        {
            try
            {
                OrderLogItem[] ordLogItemArray = OrderDataLogHelper.GetValue();

                if (ordLogItemArray != null && ordLogItemArray.Length > 0)
                {
                    this.LvRevLog.BeginUpdate();

                    for (int i = 0; i < ordLogItemArray.Length; i++)
                    {
                        switch (ordLogItemArray[i].strMsgType)
                        {
                            case PC00D01.MSGTINF:
                                if (!this.cbInfo.Checked) continue;
                                break;

                            case PC00D01.MSGTERR:

                                if (!this.cbError.Checked) continue;
                                break;

                            case PC00D01.MSGTDBG:
                                if (!this.cbDebug.Checked) continue;
                                break;
                        }

                        string strMsg = string.Empty;

                        ListViewItem lvi = new ListViewItem(ordLogItemArray[i].strDeviceId);
                        lvi.SubItems.Add(ordLogItemArray[i].strLogTime);
                        lvi.SubItems.Add(ordLogItemArray[i].strMsgType);

                        switch (ordLogItemArray[i].strMsgType)
                        {
                            case PC00D01.MSGTSEN:
                                lvi.SubItems.Add(ordLogItemArray[i].strMessage.Trim());
                                break;

                            case PC00D01.MSGTREV:
                                lvi.SubItems.Add(ordLogItemArray[i].strMessage.Trim());
                                break;

                            case PC00D01.MSGTINF:
                                lvi.SubItems.Add(ordLogItemArray[i].strMessage);
                                break;

                            case PC00D01.MSGTERR:
                                lvi.SubItems.Add(ordLogItemArray[i].strMessage);
                                lvi.ForeColor = Color.Red;
                                break;

                            default:
                                lvi.SubItems.Add(ordLogItemArray[i].strMessage);
                                break;
                        }

                        this.LvRevLog.ListView.Items.Insert(0, lvi);
                    }

                    for (int i = LvRevLog.ListView.Items.Count; i > m_nLogLines; i--)
                    {
                        this.LvRevLog.ListView.Items.RemoveAt(i - 1);
                    }

                    this.LvRevLog.EndUpdate();
                }
            }
            catch (Exception ex)
            {
                m_Logger.LogToFile("LOG", this.m_strLogFileName, PC00D01.MSGTDBG, MethodBase.GetCurrentMethod().Name, ex.ToString());
            }
        }

        #endregion


        #region listViewMsg  크로스 스레드 방지용. 리스트뷰에 로그데이터를 처리한다. 
        public void listViewMsg(string pstrProcID, string pMsg, bool pbGridView, int pnCurNo, int pnSubItemNo, bool pbLogView, string pstrType)
        {
            try
            {
                if (this.InvokeRequired)
                {
                    listViewMsgCallback d = new listViewMsgCallback(listViewMsg);
                    this.Invoke(d, new object[] { pstrProcID, pMsg, pbGridView, pnCurNo, pnSubItemNo, pbLogView, pstrType });
                }
                else
                {
                    listViewUpdate(LvRevLog, DEFINE.INFO, pstrProcID, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), pMsg, pbGridView, pnCurNo, pnSubItemNo, pbLogView, pstrType);
                }

            }
            catch (Exception ex)
            {
                m_Logger.LogToFile("LOG", this.m_strLogFileName, PC00D01.MSGTDBG, MethodBase.GetCurrentMethod().Name, ex.ToString());
            }
        }
        #endregion

        #region listViewUpdate 로그 리스트 뷰 업데이트 처리 
        public void listViewUpdate(DataSpider.PC03.PT.Controls.CWListView lvName, string pType, string pProcID, string pDateTime, string pMsg, bool bGridView, int nCurNo, int nSubitemNo, bool bLogView, string pstrType)
        {
            if (bLogView)
            {
                // ListView Write Log Message
                string LogTime = DateTime.Now.ToString(PC00D01.DateTimeFormat);
                OrderLogItem ordLogItem = new OrderLogItem();
                ordLogItem.strPlantCd = "";
                ordLogItem.strDeviceId = pProcID;
                ordLogItem.strLogTime = pDateTime;
                ordLogItem.strMsgType = pstrType;
                ordLogItem.strMessage = pMsg;
                OrderDataLogHelper.SetValue(ordLogItem);
            }

        }
        #endregion


        #region ListViewColumnSet 컬럼을 세팅한다. 서버정버와 로그 
        // ListView Set Columns
        private void ListViewColumnSet()
        {
            ImageList imgList = new ImageList();
            imgList.ImageSize = new Size(1, 18);

            Font f = new Font("Arial", 8.0F, FontStyle.Bold);

            this.LvRevLog.BeginUpdate();
            this.LvRevLog.ListView.Columns.Add("PRPCESS ID", 140, HorizontalAlignment.Center);
            this.LvRevLog.ListView.Columns.Add("LOGTIME", 120, HorizontalAlignment.Center);
            this.LvRevLog.ListView.Columns.Add("TYPE", 48, HorizontalAlignment.Center);
            this.LvRevLog.ListView.Columns.Add("MESSAGE", 667, HorizontalAlignment.Left);
            this.LvRevLog.ListView.Columns[0].Width = 0;
            this.LvRevLog.ListView.Scrollable = true;
            this.LvRevLog.EndUpdate();
            this.LvRevLog.SetRowSize(imgList);
            this.LvRevLog.SetFont(f);

        }
        #endregion


        #region <Filter Items in listview (LvRevLog)>
        // LogListView에 표시되는 Line 숫자 입력 텍스트박스에 숫자값만 입력가능하도록 셋팅
        void tbShowLines_KeyPress(object sender, KeyPressEventArgs e)
        {
            int nNum = 0;

            if (int.TryParse(e.KeyChar.ToString(), out nNum))
            {
                e.Handled = false;
                return;
            }

            if (e.KeyChar.ToString() == "\b")
            {
                e.Handled = false;
                return;
            }

            e.Handled = true;
        }

        private void BtnChange_click(object sender, EventArgs e)
        {
            int nLines = PC00D01.LogShowLinesDefault;

            int.TryParse(this.tbShowLines.Text, out nLines);
            this.m_nLogLines = nLines;
        }

        void cbDebug_CheckedChanged(object sender, EventArgs e)
        {
            OrderDataLogHelper.isCheckedDebug = this.cbDebug.Checked;
        }

        void cbError_CheckedChanged(object sender, EventArgs e)
        {
            OrderDataLogHelper.isCheckedError = this.cbError.Checked;
        }

        void cbInfo_CheckedChanged(object sender, EventArgs e)
        {
            OrderDataLogHelper.isCheckedInfo = this.cbInfo.Checked;
        }

        #endregion

        private void MainForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            m_Logger.WriteLog($"MainForm_FormClosed!");
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            DialogResult dialogResult = MessageBox.Show(PC00D01.MSGP0001, PC00D01.MSGP0002, MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (!dialogResult.Equals(DialogResult.Yes))
            {
                e.Cancel = true;
                return;
            }
            m_bTermial = true;
            Thread.Sleep(10000);
            notifyIcon1.Visible = false;
            m_Logger.WriteLog($"MainForm_FormClosing");
        }


        private void 열기ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Show();
            this.WindowState = FormWindowState.Normal;
            notifyIcon1.Visible = false;

        }

        private void 종료ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DialogResult dialogResult = MessageBox.Show(PC00D01.MSGP0001, PC00D01.MSGP0002, MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (!dialogResult.Equals(DialogResult.Yes))
            {
                return;
            }
            m_bTermial = true;
            Thread.Sleep(10000);
            notifyIcon1.Visible = false;
            m_Logger.WriteLog($"Application.Exit");
            Application.Exit();
        }
    }
}
