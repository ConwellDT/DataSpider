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

namespace DataSpider.PC01.PT
{
    public partial class PC01F01 : Form, IPC00F00
    {
        delegate void listViewMsgCallback(string pstrProcId, string pMsg, bool pbGridView, int pCurNo, int nSubitemNo, bool pbLogView, string pstrType);

        #region 전역 변수 설정
        PC00M01 thDataProcess = null;
        PC00B01[] thProcess = null;
        public bool bTerminated = false;

        //Dictionary<string, PC01S01> m_dicThread;
        DeviceInfo[]    m_clsAryDevInfo;                        // Device Infomation
        DbInfo          m_clsDbInfo;                            // DB Infomation
        PgmRegInfo      m_clsPgmInfo;                           // Program Registration Infomation
        Logging         m_clsLog = new Logging();

        //MsSqlDbAccess m_clsDBCon;                         // DB Connection
        PC00Z01       m_SqlBiz;
        string strErrCode = string.Empty;
        string strErrText = string.Empty;

        System.Windows.Forms.Timer m_tmOraConUpd;
        System.Windows.Forms.Timer m_tmSerInfoLvUpd;      // Timer (Infomation ListView Update)
        System.Windows.Forms.Timer m_tmRevOrdLvUpd;       // Timer (Receive Order Data ListView Update)
        System.Windows.Forms.Timer m_tmLogLvUpd;          // Timer (Log ListView Update)
        System.Windows.Forms.Timer m_tmCurrTmUpd;         // Timer (Current Time Display Label SetValue)

        Point       m_ptFirstMouseClick;                         // Point (Using Window Move Control)
        NotifyIcon  m_niIcon;                          // m_niIcon (Using OrderRcv Form Hide)

        private Image m_imgDbRun = PC01.PT.Properties.Resources.dbconnected;
        private Image m_imgDbStop = PC01.PT.Properties.Resources.dbdisconnected;

        bool    m_bDBCon = false;                           // Using DB Connection State Display Label Update
        int     m_nLogLines = 10;
        string m_strLogFileName = Application.ProductName;// "ALIS";

        DataSet dsMain = null;
        string mstrProcID = string.Empty;
        string m_PgmType = "";
        string m_PgmPara = "";

        private string equipName = string.Empty;
        private string equipType = string.Empty;
        private DataTable dtEquipment = null;

        private Thread thCheckRequest = null;

        private string serverCode = "0";

        private int m_bPrevHideShow = -1;
        #endregion

        #region PC01F01 생성자
        public PC01F01()
        {
            InitializeComponent();
            PC01F01_Initialize();
        }
        //public PC01F01(string _equipType) : this()
        //{
        //    equipType = _equipType;
        //    this.m_niIcon.Text = this.m_niIcon.BalloonTipText = $"{this.ProductName} - {equipType}"; //PC00D01.NotifyTipText;
        //}
        public PC01F01(string _equipName) : this()
        {
            equipName = _equipName;
            this.m_niIcon.Text = this.m_niIcon.BalloonTipText = $"{this.ProductName} - {equipName}"; //PC00D01.NotifyTipText;
        }
        #endregion

        #region PC01F01_Initialize 폼 기본 이벤트 생성 
        private void PC01F01_Initialize()
        {
            // Windows Move
            this.lbTitle.MouseDown += new MouseEventHandler(lbTitle_MouseDown);
            this.lbTitle.MouseMove += new MouseEventHandler(lbTitle_MouseMove);

            // Tray Icon
            this.m_niIcon = new System.Windows.Forms.NotifyIcon();
            //this.m_niIcon.BalloonTipText = $"{this.ProductName} - {equipType}"; //PC00D01.NotifyTipText;
            //this.m_niIcon.Text = $"{this.ProductName} - {equipType}"; //PC00D01.NotifyText;
            this.m_niIcon.Icon = PC01.PT.Properties.Resources.icon2;
            this.m_niIcon.Click += new EventHandler(m_niIcon_Click);

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

                GetEquipmentInfo();
                bool Flag = false;
                #if DEBUG
                Flag = true;
                #endif
                if (!Flag && ( dtEquipment == null || dtEquipment.Rows.Count < 1 ) )
                {
                    //equipType, machineName
                    //MessageBox.Show($"There is no assigned equipment(Type : {equipType}) to this computer({Environment.MachineName})", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    MessageBox.Show($"There is no assigned equipment(Equip Name : {equipName})", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    Application.Exit();
                }
                //Text = lbTitle.Text = $"{this.ProductName} - {equipType} [{dtEquipment.Rows[0]["EQUIP_TYPE_DESC"]}]";
                Text = lbTitle.Text = $"{this.ProductName} - {equipName} [{dtEquipment.Rows[0]["EQUIP_DESC"]} - {dtEquipment.Rows[0]["EQUIP_TYPE_NM"]}]";
                //label_Version.Text = $"V.{Assembly.GetExecutingAssembly().GetName().Version}";
                ServerInfoListViewSetValue(); //리스트뷰에 세팅처리.
                this.m_clsLog.LogToFile("LOG", this.m_strLogFileName, PC00D01.MSGTDBG, MethodBase.GetCurrentMethod().Name, "1");
                equipType = dtEquipment.Rows[0]["EQUIP_TYPE_NM"].ToString();
                CreateDataProcss();
                this.m_clsLog.LogToFile("LOG", this.m_strLogFileName, PC00D01.MSGTDBG, MethodBase.GetCurrentMethod().Name, "2");
                CreateProcess(equipType);
                this.m_clsLog.LogToFile("LOG", this.m_strLogFileName, PC00D01.MSGTDBG, MethodBase.GetCurrentMethod().Name, "3");

                serverCode = GetServerCode();

                thCheckRequest = new Thread(ThreadCheckRequest);
                thCheckRequest.Start();
            }
            catch (Exception ex)
            {
                this.m_clsLog.LogToFile("LOG", this.m_strLogFileName, PC00D01.MSGTDBG, MethodBase.GetCurrentMethod().Name, ex.ToString());
            } 
        }
        private void ThreadCheckRequest()
        {
            DataTable dtRequest = null;

            Thread.Sleep(3000);
            //ShowHideForm(false);

            while (!bTerminated)
            {
                try
                {
                    dtRequest = m_SqlBiz.GetRequest(equipName, serverCode, ref strErrCode, ref strErrText);

                    if( dtRequest == null )
                    {
                        //this.m_clsLog.LogToFile("LOG", this.m_strLogFileName, PC00D01.MSGTERR, MethodBase.GetCurrentMethod().Name, $"GetRequest Return NULL -{equipName}:{strErrText}");
                        //Terminate();
                        return;
                    }
                    if (dtRequest.Rows.Count < 1)
                    {
                        //this.m_clsLog.LogToFile("LOG", this.m_strLogFileName, PC00D01.MSGTERR, MethodBase.GetCurrentMethod().Name, $"GetRequest Return No Data - {equipName}:{strErrText}");
                        //Terminate();
                        return;
                    }
                    if (dtRequest.Rows.Count > 0)
                    {
                        if (dtRequest.Rows[0][$"STOP_REQ{serverCode}"].ToString().Equals("1"))
                        {
                            Terminate();
                            return;
                        }

                        int bHideShow = (int)dtRequest.Rows[0]["HIDE_SHOW"];
                        if (bHideShow != m_bPrevHideShow)
                        {
                            if (bHideShow == 0) ShowHideForm(false);
                            else                ShowHideForm(true);
                            m_bPrevHideShow = bHideShow;
                        }
                    }
                }
                catch (Exception ex)
                {
                    this.m_clsLog.LogToFile("LOG", this.m_strLogFileName, PC00D01.MSGTERR, MethodBase.GetCurrentMethod().Name, $"{equipName}:"+ex.ToString());
                }
                Thread.Sleep(5000);
            }
        }
        #endregion

        private void CreateProcess(string equipType)
        {
            try
            {
                int i = 0;
                switch (equipType)
                {
                    // Beckman coulter, Automatic Cell Counter (Vi-Cell) : Vi-CELL XR 2.0.4, File
                    case "ACC_Vi-Cell":
                    // HACH, Turbidity meter : TL2300, File
                    case "TBTM_TL2300":
                        thProcess = new PC01S01[dtEquipment.Rows.Count];
                        foreach (DataRow dr in dtEquipment.Rows)
                        {
                            thProcess[i++] = new PC01S01(this, dr, i, true);
                        }
                        break;
                    // Mettler Toledo, Scale : IND570+PUA579-E1500, TCP/IP
                    case "SC_IND570":
                    // A&D, Floor Scale : PF-SBL-K3000, AD-4410
                    case "FSC_PFSBL":
                        thProcess = new PC01S02[dtEquipment.Rows.Count];
                        foreach (DataRow dr in dtEquipment.Rows)
                        {
                            thProcess[i++] = new PC01S02(this, dr, i, true);
                        }
                        break;
                    // Advanced Instruments Inc, Osmometer : 3320, TCP /IP
                    case "OM_3320":
                        thProcess = new PC01S03[dtEquipment.Rows.Count];
                        foreach (DataRow dr in dtEquipment.Rows)
                        {
                            thProcess[i++] = new PC01S03(this, dr, i, true);
                        }
                        break;
                    // Roche, Automatic Cell Counter (CEDEX) : CEDEX HiRes 2.4.0, DB
                    case "ACC_CEDEX":
                    // Roche, CEDEX Bio : CEDEX Bio, DB
                    case "CEDEX_BIO":
                        thProcess = new PC01S04[dtEquipment.Rows.Count];
                        foreach (DataRow dr in dtEquipment.Rows)
                        {
                            thProcess[i++] = new PC01S04(this, dr, i, true);
                        }
                        break;
                    // Roche, Automatic Cell Counter (CEDEX) : CEDEX HiRes 2.5.1, DB
                    case "CEDEX_HIRES":
                        thProcess = new PC01S17[dtEquipment.Rows.Count];
                        foreach (DataRow dr in dtEquipment.Rows)
                        {
                            thProcess[i++] = new PC01S17(this, dr, i, true);
                        }
                        break;
                    // Siemens, Blood GAS Analyzer : RAPIDLab 348EX, LIS2
                    case "BGA_RAPIDLab":
                        thProcess = new PC01S05[dtEquipment.Rows.Count];
                        foreach (DataRow dr in dtEquipment.Rows)
                        {
                            thProcess[i++] = new PC01S05(this, dr, i, true);
                        }
                        break;
                    // pHOx Analyzer : BioProfile pHOx
                    case "PHOXA_BPPHOX":
                        thProcess = new PC01S06[dtEquipment.Rows.Count];
                        foreach (DataRow dr in dtEquipment.Rows)
                        {
                            thProcess[i++] = new PC01S06(this, dr, i, true);
                        }
                        break;
                    // pH/Conductivity meter: PC220
                    case "PCM_PC220":
                    // pH/Conductivity meter : S470-K
                    case "PCM_S470-K":
                        thProcess = new PC01S07[dtEquipment.Rows.Count];
                        foreach (DataRow dr in dtEquipment.Rows)
                        {
                            thProcess[i++] = new PC01S07(this, dr, i, true);
                        }
                        break;
                    //MTBA_BPFLEX2 :OPC UA INTERFACE
                    case "MTBA_BPFLEX2":
                        thProcess = new PC01S08[dtEquipment.Rows.Count];
                        foreach (DataRow dr in dtEquipment.Rows)
                        {
                            thProcess[i++] = new PC01S08(this, dr, i, true);
                        }
                        break;
                    // Sartorius, Filter Integrity Tester : Sartocheck 4 plus, File
                    case "FIT_SC4P":
                        thProcess = new PC01S09[dtEquipment.Rows.Count];
                        foreach (DataRow dr in dtEquipment.Rows)
                        {
                            thProcess[i++] = new PC01S09(this, dr, i, true);
                        }
                        break;
                    // Floor Scale : CAIS2+IFS4-3000RR-I
                    // Floor Scale : CAIS2+IFS4-600NL-I
                    //case "FSC_CAIS2": => SC_CAIS2 로 합침
                    // Scale (Table - top) : MSA36201S-000-D0
                    case "SC_MSA":
                    // Scale(Portable) : CAIS2/IFS4-600II-I
                    case "SC_CAIS2":
                        thProcess = new PC01S11[dtEquipment.Rows.Count];
                        foreach (DataRow dr in dtEquipment.Rows)
                        {
                            thProcess[i++] = new PC01S11(this, dr, i, true);
                        }
                        break;
                    //// pH / Conductivity meter: PC220
                    //case "PCM_PC220":
                    //    thProcess = new PC01S10[dtEquipment.Rows.Count];
                    //    foreach (DataRow dr in dtEquipment.Rows)
                    //    {
                    //        thProcess[i++] = new PC01S10(this, dr, i, true);
                    //    }
                    //    break;
                    //case "MEAN_FLEX2":
                    //    thProcess = new PC01S12[dtEquipment.Rows.Count];
                    //    foreach (DataRow dr in dtEquipment.Rows)
                    //    {
                    //        thProcess[i++] = new PC01S12(this, dr, i, true);
                    //    }
                    //    break;
                    // PCM_S470-K 분리 하다가 중단
                    //// pH/Conductivity meter : S470-K
                    //case "PCM_S470-K":
                    //    thProcess = new PC01S13[dtEquipment.Rows.Count];
                    //    foreach (DataRow dr in dtEquipment.Rows)
                    //    {
                    //        thProcess[i++] = new PC01S13(this, dr, i, true);
                    //    }
                    //    break;
                    case "Turbidity_AQ4500":
                        thProcess = new PC01S13[dtEquipment.Rows.Count];
                        foreach (DataRow dr in dtEquipment.Rows)
                        {
                            thProcess[i++] = new PC01S13(this, dr, i, true);
                        }
                        break;
                    //SOLOVPE :OPC UA INTERFACE
                    case "SOLOVPE":
                        thProcess = new PC01S14[dtEquipment.Rows.Count];
                        foreach (DataRow dr in dtEquipment.Rows)
                        {
                            thProcess[i++] = new PC01S14(this, dr, i, true);
                        }
                        break;
                    //SARTOCHECK5 : FILE INTERFACE
                    case "SARTOCHECK5":
                        thProcess = new PC01S15[dtEquipment.Rows.Count];
                        foreach (DataRow dr in dtEquipment.Rows)
                        {
                            thProcess[i++] = new PC01S15(this, dr, i, true);
                        }
                        break;
                    //FIT_PF4 UPGRADE 
                    case "FIT_PF4":
                        thProcess = new PC01S16[dtEquipment.Rows.Count];
                        foreach (DataRow dr in dtEquipment.Rows)
                        {
                            thProcess[i++] = new PC01S16(this, dr, i, true);
                        }
                        break;
                    //case "SC_MCA":
                    //    thProcess = new PC01S21[dtEquipment.Rows.Count];
                    //    foreach (DataRow dr in dtEquipment.Rows)
                    //    {
                    //        thProcess[i++] = new PC01S21(this, dr, i, true);
                    //    }
                    //    break;
                    //Satorius Bubis MCA6202S-2300-0 : MCA 
                    case "SC_MCA":
                        thProcess = new PC01S18[dtEquipment.Rows.Count];
                        foreach (DataRow dr in dtEquipment.Rows)
                        {
                            thProcess[i++] = new PC01S18(this, dr, i, true);
                        }
                        break;
                    default:
                        listViewMsg("Equipment Interface", $"Invalid Equipment Type : {equipType}", false, 1, 6, true, PC00D01.MSGTERR);
                        //listViewMsg(string pstrProcID, string pMsg, bool pbGridView, int pnCurNo, int pnSubItemNo, bool pbLogView, string pstrType)
                        this.m_clsLog.LogToFile("LOG", this.m_strLogFileName, PC00D01.MSGTERR, MethodBase.GetCurrentMethod().Name, $"Invalid Equipment Type : {equipType}");
                        break;
                }
            }
            catch (Exception ex)
            {
                this.m_clsLog.LogToFile("LOG", this.m_strLogFileName, PC00D01.MSGTDBG, MethodBase.GetCurrentMethod().Name, ex.ToString());
            }
        }

        #region PC01F01_FormClosing 폼 닫기 이벤트 
        // Form Closing Event (TightenOrderRcv_FormClosed)
        void PC01F01_FormClosing(object sender, FormClosingEventArgs e)
        {
            //try
            //{
            //    /*
            //    OrdThreadIsRun.threadRun = false;
            //    this.m_clsLog.LogToFile("LOG", this.m_strLogFileName, PC01D01.MSGTINF, MethodBase.GetCurrentMethod().Name, PC01D01.MSGP0020 + System.Environment.NewLine);

            //    if (this.m_clsOraCon != null) { this.m_clsOraCon.DBDisConnect(); }

            //    if (this.m_tmOraConUpd != null    && this.m_tmOraConUpd.Enabled   ) { this.m_tmOraConUpd.Stop(); }
            //    if (this.m_tmSerInfoLvUpd != null && this.m_tmSerInfoLvUpd.Enabled) { this.m_tmSerInfoLvUpd.Stop(); }
            //    if (this.m_tmRevOrdLvUpd != null  && this.m_tmRevOrdLvUpd.Enabled ) { this.m_tmRevOrdLvUpd.Stop(); }
            //    if (this.m_tmLogLvUpd != null     && this.m_tmLogLvUpd.Enabled    ) { this.m_tmLogLvUpd.Stop(); }
            //    if (this.m_tmCurrTmUpd != null    && this.m_tmCurrTmUpd.Enabled   ) { this.m_tmCurrTmUpd.Stop(); }
            //    */

            //    try
            //    {
            //        thDataProcess.bTerminal = true;

            //        for (int i = 0; i < thProcess.Count(); i++)
            //        {
            //            thProcess[i].bTerminal = true;
            //        }

            //        if (thDataProcess.m_Thd != null && !thDataProcess.m_Thd.Join(1000))
            //        {
            //            thDataProcess.m_Thd.Abort();
            //        }

            //        for (int i = 0; i < thProcess.Count(); i++)
            //        {
            //            if (thProcess[i].m_Thd != null && !thProcess[i].m_Thd.Join(1000))
            //            {
            //                thProcess[i].m_Thd.Abort();
            //            }
            //        }
            //    }
            //    catch (Exception ex)
            //    {
            //        this.m_clsLog.LogToFile("LOG", this.m_strLogFileName, PC00D01.MSGTDBG, MethodBase.GetCurrentMethod().Name, ex.ToString());
            //    }

            //    try
            //    {
            //        this.m_clsLog.LogToFile("LOG", this.m_strLogFileName, PC00D01.MSGTDBG, MethodBase.GetCurrentMethod().Name, "Program Close");
            //        Application.Exit();
            //    }
            //    catch (Exception ex)
            //    {
            //        this.m_clsLog.LogToFile("LOG", this.m_strLogFileName, PC00D01.MSGTDBG, MethodBase.GetCurrentMethod().Name, ex.ToString());
            //    }
            //}
            //catch (Exception ex)
            //{
            //    this.m_clsLog.LogToFile("LOG", this.m_strLogFileName, PC00D01.MSGTDBG, MethodBase.GetCurrentMethod().Name, ex.ToString());
            //}
        }
        #endregion
        void Terminate()
        {
            try
            {
                m_SqlBiz.ExecuteNonQuery($"Update MA_FAILOVER_CD SET PROG_STATUS=99 WHERE EQUIP_NM = {equipName} and FAILOVER_MODE=0", ref strErrCode, ref strErrText);
                bTerminated = true;
                thDataProcess.bTerminal = true;

                for (int i = 0; i < thProcess.Count(); i++)
                {
                    thProcess[i].bTerminal = true;
                }

                if (thDataProcess.m_Thd != null && !thDataProcess.m_Thd.Join(1000))
                {
                    thDataProcess.m_Thd.Abort();
                }

                for (int i = 0; i < thProcess.Count(); i++)
                {
                    if (thProcess[i].m_Thd != null && !thProcess[i].m_Thd.Join(1000))
                    {
                        thProcess[i].m_Thd.Abort();
                    }
                }
            }
            catch (Exception ex)
            {
                this.m_clsLog.LogToFile("LOG", this.m_strLogFileName, PC00D01.MSGTERR, MethodBase.GetCurrentMethod().Name, ex.ToString());
            }

            try
            {
                this.m_clsLog.LogToFile("LOG", this.m_strLogFileName, PC00D01.MSGTDBG, MethodBase.GetCurrentMethod().Name, "Program Close");
                Application.Exit();
            }
            catch (Exception ex)
            {
                this.m_clsLog.LogToFile("LOG", this.m_strLogFileName, PC00D01.MSGTERR, MethodBase.GetCurrentMethod().Name, ex.ToString());
            }
        }
        private string GetServerCode()
        {
            string result = string.Empty;
            try
            {

                DataTable dtResult = m_SqlBiz.GetCommonCode("SERVER_CODE", Environment.MachineName, ref strErrCode, ref strErrText);
                if (dtResult.Rows.Count > 0)
                {
                    result = dtResult.Rows[0]["CODE_VALUE"].ToString();
                }
            }
            catch (Exception ex)
            {
                this.m_clsLog.LogToFile("LOG", this.m_strLogFileName, PC00D01.MSGTERR, MethodBase.GetCurrentMethod().Name, ex.ToString());
            }
            return result;
        }
        private void GetEquipmentInfo()
        {
            string strErrCode = string.Empty;
            string strErrText = string.Empty;
            //string machineName = Environment.MachineName;
            //string[] args = Environment.GetCommandLineArgs();
            //if (args.Length > 2 && args[2].Trim().ToUpper().Equals("IGNORESERVERNAME"))
            //{
            //    machineName = string.Empty;
            //}
            //dtEquipment = this.m_SqlBiz.GetEquipmentInfo(equipType, machineName, true, ref strErrCode, ref strErrText);
            dtEquipment = this.m_SqlBiz.GetEquipmentInfoForDSC(equipName, true, ref strErrCode, ref strErrText);
        }

        #region CreateProcess 각 설비별로 PLC 데이타 수집 클래스 생성
        private void CreateDataProcss()
        {
            try
            {
                //string connectionInfo = $@"{Environment.CurrentDirectory}\Data";
                string connectionInfo = $@"{Environment.CurrentDirectory}\Data\{equipType}";
                if (!Directory.Exists(connectionInfo))
                {
                    Directory.CreateDirectory(connectionInfo);
                }
                // 20210427, SHS, 장비타입별로 동작하는 데이터 파싱 및 TTV 데이터 파일 생성 프로세스에 TTV 파일 저장 경로를 기존 한개로 설정
                // 장비타입별로 TTV 파일 저장 폴더를 분리 처리. Data 폴더 하위에 장비타입명 폴더에 저장
                //thDataProcess = new PC00M01(this, equipType, "DataProcess", connectionInfo, "", 0, true);
                thDataProcess = new PC00M01(this, equipName, "DataProcess", connectionInfo, "", 0, true);
                //thDataProcess = new PC00M01(this, equipType, "DataProcess", $@"{Environment.CurrentDirectory}\Data\{equipType}", "", 0, true);
            }
            catch (Exception ex)
            {
                this.m_clsLog.LogToFile("LOG", this.m_strLogFileName, PC00D01.MSGTDBG, MethodBase.GetCurrentMethod().Name, ex.ToString());
            }
        }

        private void ShowHideForm(bool show)
        {
            if (this.InvokeRequired)
            {
                this.Invoke((MethodInvoker)delegate { ShowHideForm(show); });
            }
            else
            {
                this.ShowInTaskbar = show;
                if (show)
                {
                    //this.Show();
                    this.Activate();
                    this.Opacity = 100;
                }
                else
                {
                    this.Opacity = 0;

//                    this.Hide();
                }
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
        public void listViewUpdate(DataSpider.PC01.PT.Controls.CWListView lvName, string pType, string pProcID, string pDateTime, string pMsg, bool bGridView, int nCurNo, int nSubitemNo, bool bLogView, string pstrType)
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

                lvi = new ListViewItem("DataProcess");
                lvi.SubItems.Add("");
                //lvi.SubItems.Add("");
                lvi.SubItems.Add("");
                lvi.SubItems.Add(PC00D01.OFF);
                lvi.SubItems.Add("");
                lvi.SubItems.Add("");
                this.LvServerInfo.ListView.Items.Add(lvi);
                this.LvServerInfo.ListView.Items[LvServerInfo.ListView.Items.Count - 1].UseItemStyleForSubItems = false;
                this.LvServerInfo.ListView.Items[LvServerInfo.ListView.Items.Count - 1].SubItems[3].ForeColor = Color.Red;

                foreach (DataRow dr in dtEquipment.Rows)
                {
                    lvi = new ListViewItem(dr["EQUIP_NM"].ToString());
                    //lvi.SubItems.Add("");
                    lvi.SubItems.Add(dr["CONNECTION_INFO"].ToString());
                    lvi.SubItems.Add(dr["EXTRA_INFO"].ToString());
                    lvi.SubItems.Add(PC00D01.OFF);
                    lvi.SubItems.Add("");
                    lvi.SubItems.Add("");
                    this.LvServerInfo.ListView.Items.Add(lvi);
                    this.LvServerInfo.ListView.Items[LvServerInfo.ListView.Items.Count - 1].UseItemStyleForSubItems = false;
                    this.LvServerInfo.ListView.Items[LvServerInfo.ListView.Items.Count - 1].SubItems[3].ForeColor = Color.Red;
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

        // Exit Button Click Event (btnExit_Click)
        void btnExit_Click(object sender, EventArgs e)
        {
            DialogResult dialogResult = MessageBox.Show(PC00D01.MSGP0001, PC00D01.MSGP0002, MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (dialogResult == DialogResult.Yes)
            {
                //this.Close();
                Terminate();
            }
        }

        // Icon Tray Button Click Event (m_niIcon_Click)
        void m_niIcon_Click(object sender, EventArgs e)
        {
            this.Visible = true;
            this.m_niIcon.Visible = false;
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

    }

}
