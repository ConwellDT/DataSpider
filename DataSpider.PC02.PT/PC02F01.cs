using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Threading;
using System.Reflection;
using CFW.Common;
using System.Configuration;
using DataSpider.PC00.PT;
using System.IO;

namespace DataSpider.PC02.PT
{
    public partial class PC02F01 : Form, IPC00F00
    {
        delegate void listViewMsgCallback(string pstrProcId, string pMsg, bool pbGridView, int pCurNo, int nSubitemNo, bool pbLogView, string pstrType);

        #region 전역 변수 설정
        PC02S01[] thProcess = null;
        public bool bTerminated = false;

        //Dictionary<string, PC01S01> m_dicThread;
        DeviceInfo[] m_clsAryDevInfo;                        // Device Infomation
        DbInfo m_clsDbInfo;                            // DB Infomation
        PgmRegInfo m_clsPgmInfo;                           // Program Registration Infomation
        Logging m_clsLog = new Logging();

        //MsSqlDbAccess m_clsDBCon;                         // DB Connection
        PC00Z01 m_SqlBiz;

        System.Windows.Forms.Timer m_tmOraConUpd;
        System.Windows.Forms.Timer m_tmSerInfoLvUpd;      // Timer (Infomation ListView Update)
        System.Windows.Forms.Timer m_tmRevOrdLvUpd;       // Timer (Receive Order Data ListView Update)
        System.Windows.Forms.Timer m_tmLogLvUpd;          // Timer (Log ListView Update)
        System.Windows.Forms.Timer m_tmCurrTmUpd;         // Timer (Current Time Display Label SetValue)

        Point m_ptFirstMouseClick;                         // Point (Using Window Move Control)
        NotifyIcon m_niIcon;                          // m_niIcon (Using OrderRcv Form Hide)

        private Image m_imgDbRun = PC02.PT.Properties.Resources.dbconnected;
        private Image m_imgDbStop = PC02.PT.Properties.Resources.dbdisconnected;

        bool m_bDBCon = false;                           // Using DB Connection State Display Label Update
        int m_nLogLines = 10;
        string m_strLogFileName = Application.ProductName;//"ALIS";

        DataSet dsMain = null;
        string mstrProcID = string.Empty;
        string m_PgmType = "";
        string m_PgmPara = "";
        private DataTable dtEquipmentType = null;

        private Thread threadUpdateProgramStatus = null;
        private int serverCode = 0;
        private List<string> listDataFilePath = new List<string>();
        private string lastErrorFileName = string.Empty;
        public string ProgramName
        {
            get { return $"{Application.ProductName}{(serverCode == 0 ? "P" : "S")}"; }
        }

        #endregion

        #region PC01F01 생성자
        public PC02F01()
        {
            InitializeComponent();
            PC02F01_Initialize();
        }
        #endregion

        #region PC021F01_Initialize 폼 기본 이벤트 생성 
        private void PC02F01_Initialize()
        {
            // Windows Move
            this.lbTitle.MouseDown += new MouseEventHandler(lbTitle_MouseDown);
            this.lbTitle.MouseMove += new MouseEventHandler(lbTitle_MouseMove);

            // Tray Icon
            this.m_niIcon = new System.Windows.Forms.NotifyIcon();
            this.m_niIcon.BalloonTipText = $"{this.ProductName}";// - DB Uploader "; //PC00D01.NotifyTipText;
            this.m_niIcon.Text = $"{this.ProductName}";// - DB Uploader"; //PC00D01.NotifyText;
            this.m_niIcon.Icon = PC02.PT.Properties.Resources.dbupdate;
            this.m_niIcon.DoubleClick += new EventHandler(m_niIcon_Click);
            this.m_niIcon.ContextMenuStrip = contextMenuStrip_TrayIcon;
            this.m_niIcon.Visible = true;

            // Show Lines Text Input Only Number
            this.tbShowLines.MaxLength = 4;
            this.tbShowLines.KeyPress +=new KeyPressEventHandler(tbShowLines_KeyPress);

            // Oracle DB Connection Status PictureBox
            this.pbDbCon.Paint += new PaintEventHandler(pbDbCon_Paint);

            // Button Click Event
            this.btnChange.Click += BtnChange_click;
            this.btnMinimize.Click += new EventHandler(btnMinimize_Click);
            this.btnExit.Click += new EventHandler(btnExit_Click);

            //Timer Set
            this.m_tmOraConUpd = new System.Windows.Forms.Timer();
            this.m_tmOraConUpd.Interval = PC00D01.ItvDBChk;
            this.m_tmOraConUpd.Tick += new EventHandler(m_tmOraConUpd_Tick);

            this.m_tmLogLvUpd = new System.Windows.Forms.Timer();
            this.m_tmLogLvUpd.Interval = PC00D01.ItvLogLvUpd;
            this.m_tmLogLvUpd.Tick += new EventHandler(m_tmLogLvUpd_Tick);

            this.cbInfo.CheckedChanged += new EventHandler(cbInfo_CheckedChanged);
            this.cbError.CheckedChanged += new EventHandler(cbError_CheckedChanged);
            this.cbDebug.CheckedChanged += new EventHandler(cbDebug_CheckedChanged);

        }
        #endregion

        #region PC01F01_Load 폼 로딩
        void PC01F01_Load(object sender, EventArgs e)
        {
            this.cbInfo.Checked = true;
            this.cbError.Checked = true;
            this.cbDebug.Checked = false;

            try
            {
                // DB 연결 
                this.m_SqlBiz = new PC00Z01();
                serverCode = m_SqlBiz.GetServerId(Environment.MachineName);

                Text = lbTitle.Text = $"{this.ProductName} - DB Uploader";
                //label_Version.Text = $"V.{Assembly.GetExecutingAssembly().GetName().Version}";

                // Refresh DB Connection State
                this.m_bDBCon = true;
                this.pbDbCon.Image = m_imgDbRun;
                this.pbDbCon.Refresh();

                this.m_nLogLines = PC00D01.LogShowLinesDefault;
                this.tbShowLines.Text = m_nLogLines.ToString();

                // DB 정보와 프로그램 정보를 로딩한다. CONFIG FILE
                GetConfigInfo();
                ListViewColumnSet(); // 컬럼 세팅

                this.m_tmOraConUpd.Start();
                this.m_tmLogLvUpd.Start();

                //GetProgramInfo();   // 프로그램 정보를 로딩한다. 
                GetEquipmentTypeInfo();
                if (dtEquipmentType == null || dtEquipmentType.Rows.Count < 1)
                {
                    //equipType, machineName
                    MessageBox.Show($"There is no equipment type info", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    Application.Exit();
                }

                ServerInfoListViewSetValue(); //리스트뷰에 세팅처리.
                CreateProcess();

                m_SqlBiz.WriteSTCommon(ProgramName, "ERROR_FILE", string.Empty);
                threadUpdateProgramStatus = new Thread(new ThreadStart(ThreadUpdateProgramStatus));
                threadUpdateProgramStatus.Start();
            }
            catch (Exception ex)
            {
                this.m_clsLog.LogToFile("LOG", this.m_strLogFileName, PC00D01.MSGTDBG, MethodBase.GetCurrentMethod().Name, ex.ToString());
            } 
        }
        #endregion

        private void GetEquipmentTypeInfo()
        {
            string strErrCode = string.Empty;
            string strErrText = string.Empty;
            dtEquipmentType = this.m_SqlBiz.GetCommonCode("EQUIP_TYPE", ref strErrCode, ref strErrText);
        }

        #region PC01F01_FormClosing 폼 닫기 이벤트 
        // Form Closing Event (TightenOrderRcv_FormClosed)
        void PC01F01_FormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {
                /*
                OrdThreadIsRun.threadRun = false;
                this.m_clsLog.LogToFile("LOG", this.m_strLogFileName, PC01D01.MSGTINF, MethodBase.GetCurrentMethod().Name, PC01D01.MSGP0020 + System.Environment.NewLine);

                if (this.m_clsOraCon != null) { this.m_clsOraCon.DBDisConnect(); }

                if (this.m_tmOraConUpd != null    && this.m_tmOraConUpd.Enabled   ) { this.m_tmOraConUpd.Stop(); }
                if (this.m_tmSerInfoLvUpd != null && this.m_tmSerInfoLvUpd.Enabled) { this.m_tmSerInfoLvUpd.Stop(); }
                if (this.m_tmRevOrdLvUpd != null  && this.m_tmRevOrdLvUpd.Enabled ) { this.m_tmRevOrdLvUpd.Stop(); }
                if (this.m_tmLogLvUpd != null     && this.m_tmLogLvUpd.Enabled    ) { this.m_tmLogLvUpd.Stop(); }
                if (this.m_tmCurrTmUpd != null    && this.m_tmCurrTmUpd.Enabled   ) { this.m_tmCurrTmUpd.Stop(); }
                */

                try
                {
                    for (int i = 0; i < thProcess.Count(); i++)
                    {
                        thProcess[i].bTerminal = true;
                    }
                    Thread.Sleep(1000);
                    for (int i = 0; i < thProcess.Count(); i++)
                    {
                        if (thProcess[i].m_Thd != null && thProcess[i].m_Thd.IsAlive)
                        {
                            thProcess[i].m_Thd.Abort();
                        }
                    }
                }
                catch (Exception ex)
                {
                    this.m_clsLog.LogToFile("LOG", this.m_strLogFileName, PC00D01.MSGTDBG, MethodBase.GetCurrentMethod().Name, ex.ToString());
                }

                try
                {
                    this.m_clsLog.LogToFile("LOG", this.m_strLogFileName, PC00D01.MSGTDBG, MethodBase.GetCurrentMethod().Name, "Program Close");
                    Application.Exit();
                }
                catch (Exception ex)
                {
                    this.m_clsLog.LogToFile("LOG", this.m_strLogFileName, PC00D01.MSGTDBG, MethodBase.GetCurrentMethod().Name, ex.ToString());
                }
            }
            catch (Exception ex)
            {
                this.m_clsLog.LogToFile("LOG", this.m_strLogFileName, PC00D01.MSGTDBG, MethodBase.GetCurrentMethod().Name, ex.ToString());
            }
            finally
            {
                this.m_niIcon.Visible = false;
            }
        }
        #endregion

        #region GetProgramInfo 프로그램 정보를 로딩한다. 

        private void CreateProcess()
        {
            try
            {
                // 장비타입 쿼리, 각 장비타입별로 쓰레드 생성 (DATA 파일 경로는 설정+각 장비타입명으로 처리
                //thProcess = new PC02S01[1];
                //thProcess[0] = new PC02S01(this, "", $"{Application.ProductName}P01", $@"{Environment.CurrentDirectory}\{CFW.Configuration.ConfigManager.Default.ReadConfig("", "DataFilePath")}", $@"{Environment.CurrentDirectory}\{CFW.Configuration.ConfigManager.Default.ReadConfig("", "ErrorDataFilePath")}", 0, true);
                thProcess = new PC02S01[dtEquipmentType.Rows.Count];
                int threadIndex = 0;
                foreach (DataRow dr in dtEquipmentType.Rows)
                {
                    string equipType = dr["CODE_NM"].ToString();
                    thProcess[threadIndex] = new PC02S01(this, equipType, $"{Application.ProductName}_{equipType}", $@"{Environment.CurrentDirectory}\Data\{equipType}", string.Empty, threadIndex++, true);
                    listDataFilePath.Add($@"{Environment.CurrentDirectory}\Data\{equipType}");
                }
            }
            catch (Exception ex)
            {
                this.m_clsLog.LogToFile("LOG", this.m_strLogFileName, PC00D01.MSGTDBG, MethodBase.GetCurrentMethod().Name, ex.ToString());
            }
        }


        #endregion

        #region <Tick Timer>
        void m_tmOraConUpd_Tick(object sender, EventArgs e)
        {
            try
            {
                if (TryDBConnect())
                {
                    /*
                    if (!OrdThreadIsRun.threadRun && !this.m_tmRevOrdLvUpd.Enabled)
                    {
                        this.m_tmRevOrdLvUpd.Start();
                    }

                    if (!OrdThreadIsRun.threadRun)
                    {
                        OrdThreadIsRun.threadRun = true;
                    }

                    if (OrdThreadIsRun.threadRun)
                    {
                        //SocketThreadRun();  // ClientSocket Run
                    }
                    */
                }
                else
                {
                    /*
                    OrdThreadIsRun.threadRun = false;

                    if (this.m_tmRevOrdLvUpd.Enabled)
                    {
                        this.m_tmRevOrdLvUpd.Stop();
                    }

                    this.m_clsLog.LogToFile("LOG", this.m_strLogFileName, PC01D01.MSGTERR, MethodBase.GetCurrentMethod().Name, PC01D01.MSGP0011);
                    */
                }

                //GC.Collect();
                // GC.WaitForPendingFinalizers();
            }
            catch (Exception ex)
            {
                this.m_clsLog.LogToFile("LOG", this.m_strLogFileName, PC00D01.MSGTDBG, MethodBase.GetCurrentMethod().Name, ex.ToString());
            }
        }

        // Timer Tick Evnet (Log ListView Update)
        void m_tmLogLvUpd_Tick(object sender, EventArgs e)
        {
            ReceivedDataLogListViewUpdate();
        }

        // Timer Tick Evnet (Infomation ListView Update)
        void m_tmSerInfoLvUpd_Tick(object sender, EventArgs e)
        {
           // ServerInfoListViewUpdate();
        }        

        // Timer Tick Evnet (Current Time Display Label SetValue)
        void m_tmCurrTmUpd_Tick(object sender, EventArgs e)
        {
            DateTime dt = System.DateTime.Now;
            string CurTime = dt.ToString(PC00D01.DateTimeFormat);
            this.lblRunTime.Text = CurTime;
        }
        #endregion

        #region <Connection Oracle Database>
        private bool TryDBConnect()
        {
            try
            {
                //if (this.m_clsOraCon.DBConnectState())
                //{
                //    return true;
                //}

                if (!PingHelper.PingTest(m_clsDbInfo.strDB_IP))
                {
                    this.m_clsLog.LogToFile("LOG", this.m_strLogFileName, PC00D01.MSGTERR, MethodBase.GetCurrentMethod().Name, PC00D01.MSGP0015);

                    // Refresh DB Connection State
                     this.m_bDBCon = false;
                    this.pbDbCon.Image = m_imgDbStop;
                    this.pbDbCon.Refresh();

                    // ListView Write Log Message
                    string LogTime = DateTime.Now.ToString(PC00D01.DateTimeFormat);
                    OrderLogItem ordLogItem = new OrderLogItem();
                    ordLogItem.strPlantCd = "";
                    ordLogItem.strDeviceId = "SYSTEM";
                    ordLogItem.strLogTime = LogTime;
                    ordLogItem.strMsgType = PC00D01.MSGTERR;
                    ordLogItem.strMessage = PC00D01.MSGP0031;
                    OrderDataLogHelper.SetValue(ordLogItem);

                    return false;
                }

                // this.m_clsLog.LogToFile("LOG", this.m_strLogFileName, PC01D01.MSGTINF, MethodBase.GetCurrentMethod().Name, PC01D01.MSGP0014);

                /*
                if (!this.m_clsOraCon.DBConnect(this.m_clsOraDbInfo.strOraConStr))
                {
                    this.m_clsLog.LogToFile("LOG", this.m_strLogFileName, PC01D01.MSGTINF, MethodBase.GetCurrentMethod().Name, PC01D01.MSGP0013);

                    // Refresh DB Connection State
                    this.m_bDBCon = false;
                    this.pbDbCon.Image = m_imgDbStop;
                    this.pbDbCon.Refresh();

                    // ListView Write Log Message
                    string LogTime = DateTime.Now.ToString(PC01D01.DateTimeFormat);
                    OrderLogItem ordLogItem = new OrderLogItem();
                    ordLogItem.strPlantCd = "";
                    ordLogItem.strDeviceId = "SYSTEM";
                    ordLogItem.strLogTime = LogTime;
                    ordLogItem.strMsgType = PC01D01.MSGTERR;
                    ordLogItem.strMessage = PC01D01.MSGP0031;
                    OrderDataLogHelper.SetValue(ordLogItem);

                    return false;
                }
                */
                //this.m_clsLog.LogToFile("LOG", this.m_strLogFileName, PC01D01.MSGTINF, MethodBase.GetCurrentMethod().Name, PC01D01.MSGP0012);

                // Refresh DB Connection State
                this.m_bDBCon = true;
                this.pbDbCon.Image = m_imgDbRun;
                this.pbDbCon.Refresh();
                
            }
            catch (Exception ex)
            {
                this.m_clsLog.LogToFile("LOG", this.m_strLogFileName, PC00D01.MSGTDBG, MethodBase.GetCurrentMethod().Name, ex.ToString());

                // Refresh DB Connection State
                this.m_bDBCon = false;
                this.pbDbCon.Image = m_imgDbStop;
                this.pbDbCon.Refresh();

                return false;
            }

            return true;
        }
        #endregion

        #region ListViewColumnSet 컬럼을 세팅한다. 서버정버와 로그 
        // ListView Set Columns
        private void ListViewColumnSet()
        {
            ImageList imgList = new ImageList();
            imgList.ImageSize = new Size(1, 18);

            Font f = new Font("Arial", 8.0F, FontStyle.Bold);

            this.LvServerInfo.SuspendLayout();
            this.LvServerInfo.ListView.Columns.Add("PRPCESS ID", 110, HorizontalAlignment.Center);
            //this.LvServerInfo.ListView.Columns.Add("STATION ID", 70, HorizontalAlignment.Center);
            //this.LvServerInfo.ListView.Columns.Add("GET PATH", 250, HorizontalAlignment.Center);
            //this.LvServerInfo.ListView.Columns.Add("SET PATH", 180, HorizontalAlignment.Center);
            this.LvServerInfo.ListView.Columns.Add("CONNECTION INFO", 250, HorizontalAlignment.Center);
            this.LvServerInfo.ListView.Columns.Add("EXTRA INFO", 180, HorizontalAlignment.Center);
            this.LvServerInfo.ListView.Columns.Add("STATUS", 60, HorizontalAlignment.Center);
            this.LvServerInfo.ListView.Columns.Add("DATETIME", 115, HorizontalAlignment.Center);
            this.LvServerInfo.ListView.Columns.Add("MESSAGE", 260, HorizontalAlignment.Left);
            this.LvServerInfo.ResumeLayout();
            this.LvServerInfo.SetRowSize(imgList);
            this.LvServerInfo.SetFont(f);

            this.LvRevLog.BeginUpdate();
            this.LvRevLog.ListView.Columns.Add("PRPCESS ID", 140, HorizontalAlignment.Center);
            this.LvRevLog.ListView.Columns.Add("LOGTIME", 120, HorizontalAlignment.Center);
            this.LvRevLog.ListView.Columns.Add("TYPE", 48, HorizontalAlignment.Center);
            this.LvRevLog.ListView.Columns.Add("MESSAGE", 667, HorizontalAlignment.Left);
            this.LvRevLog.ListView.Scrollable = true;
            this.LvRevLog.EndUpdate();
            this.LvRevLog.SetRowSize(imgList);
            this.LvRevLog.SetFont(f);
           
        }
        #endregion

        #region listViewUpdate 로그 리스트 뷰 업데이트 처리 
        public void listViewUpdate(DataSpider.PC02.PT.Controls.CWListView lvName, string pType, string pProcID, string pDateTime, string pMsg, bool bGridView, int nCurNo, int nSubitemNo, bool bLogView, string pstrType)
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

            try
            {
                if (bGridView)
                {
                    cellDataChange(nCurNo, 4, pDateTime); //시간업데이트 리스트뷰
                    if (nSubitemNo == 3)
                    {
                        cellDataChangeStatus(nCurNo, nSubitemNo, pMsg); //상태 업데이트 
                    }
                    else
                    {
                        cellDataChange(nCurNo, nSubitemNo, pMsg); //메세지 업데이트
                    }

                }

            }
            catch (Exception ex)
            {
                this.m_clsLog.LogToFile("LOG", this.m_strLogFileName, PC00D01.MSGTDBG, MethodBase.GetCurrentMethod().Name, ex.ToString());
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
                this.m_clsLog.LogToFile("LOG", this.m_strLogFileName, PC00D01.MSGTDBG, MethodBase.GetCurrentMethod().Name, ex.ToString());
            }
        }
        #endregion

        #region cellDataChange 메인데이타 그리드 메세지 변경처리 
        private void cellDataChange(int nItemNo, int nSubitemNo, string strMsg)
        {

            //DevExpress.XtraGrid.Columns.GridColumn gc = this.gdViewMain.Columns.ColumnByName(cColumnNm);

            try
            {
                //gdViewMain.SetRowCellValue(nCurNo, gc, strMsg);

                LvServerInfo.ListView.Items[nItemNo].SubItems[nSubitemNo].Text = strMsg;
                
            }
            catch (Exception ex)
            {
                this.m_clsLog.LogToFile("LOG", this.m_strLogFileName, PC00D01.MSGTDBG, MethodBase.GetCurrentMethod().Name, ex.ToString());
            }
        }
        #endregion

        #region cellDataChangeStatus 메인데이타 그리드 상태 메세지와 색깔 변경 
        private void cellDataChangeStatus(int nItemNo, int nSubitemNo, string strMsg)
        {

            try
            {
                LvServerInfo.ListView.Items[nItemNo].SubItems[nSubitemNo].Text = strMsg;

                if (strMsg == PC00D01.OFF)
                {

                    this.LvServerInfo.ListView.Items[nItemNo].UseItemStyleForSubItems = false;
                    this.LvServerInfo.ListView.Items[nItemNo].SubItems[nSubitemNo].ForeColor = Color.Red;
                }
                else
                {
                    this.LvServerInfo.ListView.Items[nItemNo].UseItemStyleForSubItems = false;
                    this.LvServerInfo.ListView.Items[nItemNo].SubItems[nSubitemNo].ForeColor = Color.Green;
                }
            }
            catch (Exception ex)
            {
                this.m_clsLog.LogToFile("LOG", this.m_strLogFileName, PC00D01.MSGTDBG, MethodBase.GetCurrentMethod().Name, ex.ToString());
            }
        }
        #endregion

        
        #region <Update Listview Items>
        // Infomation ListView Data Update
        private void ServerInfoListViewSetValue()
        {
            try
            {
                this.LvServerInfo.BeginUpdate();

                ListViewItem lvi;

                //for (int i = 0; i < this.m_clsAryDevInfo?.Length; i++)
                //{
                //    lvi = new ListViewItem(this.m_clsAryDevInfo[i].strProc_ID);
                //    lvi.SubItems.Add(this.m_clsAryDevInfo[i].strStation_Cd);
                //    lvi.SubItems.Add(this.m_clsAryDevInfo[i].strGetPath);
                //    lvi.SubItems.Add(this.m_clsAryDevInfo[i].strSetPath);
                //    lvi.SubItems.Add(PC00D01.OFF);
                //    lvi.SubItems.Add("");
                //    lvi.SubItems.Add("");
                //    this.LvServerInfo.ListView.Items.Add(lvi);

                //    this.LvServerInfo.ListView.Items[i].UseItemStyleForSubItems = false;
                //    this.LvServerInfo.ListView.Items[i].SubItems[4].ForeColor = Color.Red;
                //}

                //lvi = new ListViewItem($"{Application.ProductName}P01");
                ////lvi.SubItems.Add("");
                //lvi.SubItems.Add(CFW.Configuration.ConfigManager.Default.ReadConfig("", "DataFilePath"));
                //lvi.SubItems.Add("");
                //lvi.SubItems.Add(PC00D01.OFF);
                //lvi.SubItems.Add("");
                //lvi.SubItems.Add("");
                //this.LvServerInfo.ListView.Items.Add(lvi);

                //this.LvServerInfo.ListView.Items[0].UseItemStyleForSubItems = false;
                //this.LvServerInfo.ListView.Items[0].SubItems[3].ForeColor = Color.Red;

                int i = 0;
                foreach (DataRow dr in dtEquipmentType.Rows)
                {
                    lvi = new ListViewItem($"{dr["CODE_NM"]}");
                    //lvi.SubItems.Add("");
                    lvi.SubItems.Add($@"{Environment.CurrentDirectory}\Data\{dr["CODE_NM"]}");
                    lvi.SubItems.Add("");
                    lvi.SubItems.Add(PC00D01.OFF);
                    lvi.SubItems.Add("");
                    lvi.SubItems.Add("");
                    this.LvServerInfo.ListView.Items.Add(lvi);

                    this.LvServerInfo.ListView.Items[i].UseItemStyleForSubItems = false;
                    this.LvServerInfo.ListView.Items[i].SubItems[3].ForeColor = Color.Red;
                    i++;
                }

                this.LvServerInfo.EndUpdate();
            }
            catch (Exception ex)
            {
                this.m_clsLog.LogToFile("LOG", this.m_strLogFileName, PC00D01.MSGTDBG, MethodBase.GetCurrentMethod().Name, ex.ToString());
            }
        }

       
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
                this.m_clsLog.LogToFile("LOG", this.m_strLogFileName, PC00D01.MSGTDBG, MethodBase.GetCurrentMethod().Name, ex.ToString());
            }
        }

        #endregion
        // Infomation ListView Update

        #region <Get Setting Data (App.Config)>
        private bool GetConfigInfo()
        {
            try
            {
                this.m_clsDbInfo = ConfigHelper.GetDbInfo();
                this.m_clsPgmInfo = ConfigHelper.GetPgmRegInfo();
            }
            catch(Exception ex)
            {
                this.m_clsLog.LogToFile("LOG", this.m_strLogFileName, PC00D01.MSGTDBG, MethodBase.GetCurrentMethod().Name, ex.ToString());
                return false;
            }

            return true;
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

        #region <Event Button Click>
        // Minimize Button Click Evnet (btnMinimize_Click)
        void btnMinimize_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;
            //this.Visible = false;
            //this.m_niIcon.Visible = true;
            //this.m_niIcon.ShowBalloonTip(2000);
        }

        private void Terminate()
        {
            if (DialogResult.Yes == MessageBox.Show(PC00D01.MSGP0001, PC00D01.MSGP0002, MessageBoxButtons.YesNo, MessageBoxIcon.Question))
            {
                try
                {
                    bTerminated = true;

                    for (int i = 0; i < thProcess.Count(); i++)
                    {
                        if (thProcess[i] != null)
                            thProcess[i].bTerminal = true;
                    }
                    Thread.Sleep(1000);

                    for (int i = 0; i < thProcess.Count(); i++)
                    {
                        if (thProcess[i] != null)
                        {
                            int count = 0;
                            while (thProcess[i].m_Thd != null && thProcess[i].m_Thd.IsAlive)
                            {
                                thProcess[i].m_Thd.Join(10);
                                if (count++ > 10)
                                {
                                    thProcess[i].m_Thd.Abort();
                                    break;
                                }
                            }
                        }
                    }

                    if (threadUpdateProgramStatus != null)
                    {
                        int count = 0;
                        while (threadUpdateProgramStatus != null && threadUpdateProgramStatus.IsAlive)
                        {
                            threadUpdateProgramStatus.Join(10);
                            if (count++ > 100)
                            {
                                threadUpdateProgramStatus.Abort();
                                break;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    this.m_clsLog.LogToFile("LOG", this.m_strLogFileName, PC00D01.MSGTDBG, MethodBase.GetCurrentMethod().Name, ex.ToString());
                }

                try
                {
                    this.m_clsLog.LogToFile("LOG", this.m_strLogFileName, PC00D01.MSGTDBG, MethodBase.GetCurrentMethod().Name, "Program Close");
                    Application.Exit();
                }
                catch (Exception ex)
                {
                    this.m_clsLog.LogToFile("LOG", this.m_strLogFileName, PC00D01.MSGTDBG, MethodBase.GetCurrentMethod().Name, ex.ToString());
                }
                finally
                {
                    m_niIcon.Visible = false;
                    Application.Exit();
                }
            }
        }

        // Exit Button Click Event (btnExit_Click)
        void btnExit_Click(object sender, EventArgs e)
        {
            ShowForm(false);
        }

        // Icon Tray Button Click Event (m_niIcon_Click)
        void m_niIcon_Click(object sender, EventArgs e)
        {
            //this.m_niIcon.Visible = false;
            ShowForm(true);
        }

        void ShowForm(bool show)
        {
            WindowState = show ? FormWindowState.Normal : FormWindowState.Minimized;
            this.ShowInTaskbar = show;
        }
        #endregion

        #region <Moving Form location>
        // Form Window Move Event (lbTitle_MouseDown)
        void lbTitle_MouseDown(object sender, MouseEventArgs e)
        {
            m_ptFirstMouseClick = e.Location;
        }

        // Form Window Move Event (lbTitle_MouseMove)
        void lbTitle_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                int xDiff = m_ptFirstMouseClick.X - e.Location.X;
                int yDiff = m_ptFirstMouseClick.Y - e.Location.Y;

                int x = this.Location.X - xDiff;
                int y = this.Location.Y - yDiff;

                this.Location = new Point(x, y);
            }
        }
        #endregion

        #region <Drawing Oracle connection status>
        void pbDbCon_Paint(object sender, PaintEventArgs e)
        {
            using (Font myFont = new Font("Arial", 11, FontStyle.Bold))
            {
                string strText = string.Empty;
                Point p;

                if (m_bDBCon)
                {
                    // Connection Success
                    strText = PC00D01.MSGP0008;
                    p = new Point(27, 7);
                }
                else
                {
                    // Connection Fail
                    strText = PC00D01.MSGP0009;
                    p = new Point(19, 7);
                }

                e.Graphics.DrawString(strText, myFont, Brushes.Black, p);
            }
        }
        #endregion

        public void DataProcessing()
        {
            while (!bTerminated)
            {
                try
                {

                }
                catch (Exception ex)
                {

                }
                Thread.Sleep(3000);
            }
        }

        private void TrayIconOpen_Click(object sender, EventArgs e)
        {
            ShowForm(true);
        }

        private void TrayIconExit_Click(object sender, EventArgs e)
        {
            Terminate();
        }

        private void ThreadUpdateProgramStatus()
        {
            string errCode = string.Empty;
            string errText = string.Empty;
            while (!bTerminated)
            {
                try
                {
                    int maxStatus = -1;
                    foreach (PC02S01 prc in thProcess)
                    {
                        if (maxStatus < (int)prc.ThreadStatus)
                        {
                            maxStatus = (int)prc.ThreadStatus;
                        }
                    }

                    m_SqlBiz.UpdateEquipmentProgDateTimeForProgram(ProgramName, maxStatus, ref errCode, ref errText);
                    CheckErrorFile();
                    Thread.Sleep(10 * 1000);
                }
                catch (Exception ex)
                {
                }
                finally
                {
                }
            }
            m_SqlBiz.UpdateEquipmentProgDateTimeForProgram(ProgramName, (int)IF_STATUS.Stop, ref errCode, ref errText);
        }

        //protected new bool UpdateEquipmentProgDateTime(IF_STATUS status = IF_STATUS.Normal)
        //{
        //    string errCode = string.Empty;
        //    string errText = string.Empty;
        //    DateTime dtNow = DateTime.Now;
        //    if (dtNow.Subtract(dtLastUpdateProgDateTime).TotalSeconds > UpdateInterval || !lastStatus.Equals(status))
        //    {
        //        dtLastUpdateProgDateTime = dtNow;
        //        lastStatus = status;
        //        return m_sqlBiz.UpdateEquipmentProgDateTimeForProgram($"{System.Windows.Forms.Application.ProductName}{(serverCode == 0 ? "P" : "S")}", (int)status, ref errCode, ref errText);
        //    }
        //    return true;
        //}

        private void CheckErrorFile()
        {

            string errorFileDirectoryName = string.Empty;
            try
            {
                foreach (string errorFilePath in listDataFilePath)
                {
                    DirectoryInfo di = new DirectoryInfo($@"{errorFilePath}\DataFile_Error");
                    FileInfo[] arrFileInfo = di.GetFiles("*.ttv");
                    if (arrFileInfo.Length > 0)
                    {
                        errorFileDirectoryName = errorFilePath.Substring(errorFilePath.LastIndexOf(@"\") + 1);
                        break;
                    }
                }

                string dataSpiderPC02_ErrorFile = m_SqlBiz.ReadSTCommon(ProgramName, "ERROR_FILE").Trim();
                if (!errorFileDirectoryName.Equals(dataSpiderPC02_ErrorFile))
                {
                    m_SqlBiz.WriteSTCommon(ProgramName, "ERROR_FILE", errorFileDirectoryName);
                }
            }
            catch (Exception ex)
            {
                string errCode = string.Empty;
                string errText = string.Empty;
                m_SqlBiz.UpdateEquipmentProgDateTimeForProgram(ProgramName, (int)IF_STATUS.InternalError, ref errCode, ref errText);
            }
        }
    }

}
