using DataSpider.PC00.PT;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DataSpider.SampleCollector
{
    public partial class MainForm : Form
    {

        [DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);


        private PC00Z01 sqlBiz = new PC00Z01();
        public bool bTerminal = false;
        Thread threadStatus = null;
        int MY_ID = -1;
        string MY_EQUIP_NM = string.Empty;
        bool m_bShow = false;
        bool m_bHide = true;

        public MainForm()
        {
            InitializeComponent();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            MY_EQUIP_NM = ConfigHelper.GetAppSetting("EQUIP_NM");
            this.Text = MY_EQUIP_NM;
            threadStatus = new Thread(StatusThread);
            threadStatus.Start(this);
        }

        private void StatusThread(object This)
        {
            string errCode = string.Empty;
            string errText = string.Empty;
            StringBuilder strQuery = new StringBuilder();
            DateTime ScanTime = DateTime.Now;

            while (!bTerminal)
            {
                try
                {
                    if (MY_ID == -1)
                    {
                        MY_ID = sqlBiz.GetServerId(Environment.MachineName);
                        continue;
                    }


                    if ((DateTime.Now - ScanTime).TotalSeconds > 2)
                    {
                        ScanTime = DateTime.Now;
                        //데이타 조회
                        DataTable dtStatus = sqlBiz.GetTableInfo($" EXEC GetFailoverInfo ", ref errCode, ref errText);

                        DataView dvStatus = new DataView(dtStatus);
                        dvStatus.RowFilter = $" EQUIP_NM ='{MY_EQUIP_NM}' ";

                        foreach (DataRowView dr in dvStatus)
                        {
                            if (1 == (int)dr[$"STOP_REQ{MY_ID}"])
                            {
                                bTerminal = true;
                                strQuery = new StringBuilder();
                                strQuery.Append($" UPDATE MA_FAILOVER_CD SET STOP_REQ{MY_ID}=0 WHERE EQUIP_NM='{MY_EQUIP_NM}'  ");
                                CFW.Data.MsSqlDbAccess.ExecuteNonQuery(strQuery.ToString(), null, CommandType.Text, ref errCode, ref errText);
                            }
                            if (1 == (int)dr[$"HIDE_SHOW"])
                            {
                                m_bShow = true;
                            }
                            else 
                            {
                                m_bHide = true;
                            }

                            if (FAILOVER_MODE.AUTO == (int)dr[$"FAILOVER_MODE"] ) // FAILOVER_MODE=AUTO
                            {
                                strQuery = new StringBuilder();
                                strQuery.Append($" UPDATE MA_FAILOVER_CD SET PROG_DATETIME='{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}',");
                                strQuery.Append($" PROG_STATUS=1 ");
                                strQuery.Append($" WHERE  EQUIP_NM ='{MY_EQUIP_NM}' AND ACTIVE_SERVER={MY_ID}");
                                CFW.Data.MsSqlDbAccess.ExecuteNonQuery(strQuery.ToString(), null, CommandType.Text, ref errCode, ref errText);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                }
                finally
                {
                    threadStatus.Join(1000);
                }
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (bTerminal == true)
            {
                this.Close();
            }

            if( m_bHide )
            {
                this.Hide();
                m_bHide = false;
            }

            if( m_bShow )
            {
                this.ShowInTaskbar = true;
                this.TopMost = true;
                this.Show();
                this.TopMost = false;
                //this.wWindowStyle = ProcessWindowStyle.Minimized,
                this.WindowState = System.Windows.Forms.FormWindowState.Normal;
                var prc = Process.GetProcessesByName(Process.GetCurrentProcess().ProcessName);
                if (prc.Length > 0)
                {
                    SetForegroundWindow(prc[0].MainWindowHandle);
                }
                m_bShow = false;
            }

        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            bTerminal = true;
            Thread.Sleep(1000);
        }


        private void btnGetMyId_Click(object sender, EventArgs e)
        {
            string errCode = string.Empty;
            string errText = string.Empty;
            StringBuilder strQuery = new StringBuilder();
            int MyId = -1;
            strQuery = new StringBuilder();
            strQuery.Append($" SELECT CODE_VALUE FROM MA_COMMON_CD WHERE CD_GRP='SERVER_CODE' AND CODE='{Environment.MachineName}' ");
            DataTable MyIdTbl = sqlBiz.GetTableInfo(strQuery.ToString(), ref errCode, ref errText);

        }

        private void btnDisconnectCnt_Click(object sender, EventArgs e)
        {
            string errCode = string.Empty;
            string errText = string.Empty;
            StringBuilder strQuery = new StringBuilder();
            int MyId = -1;
            strQuery = new StringBuilder();
            strQuery.Append($" SELECT CODE_VALUE FROM MA_COMMON_CD WHERE CODE_GRP='SERVER_CODE' AND CODE='{Environment.MachineName}' ");
            DataTable MyIdTbl = sqlBiz.GetTableInfo(strQuery.ToString(), ref errCode, ref errText);
        }
    }
}
