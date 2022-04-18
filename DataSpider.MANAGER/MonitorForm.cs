using LibraryWH.FormCtrl;
using DataSpider.PC00.PT;
using DataSpider.UserMonitor;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DataSpider
{
    public partial class MonitorForm : UserForm
    {
        EquipCtrl m_pSBLDataCtrl;
        TreeForm pTreeForm;
        bool m_bSprash = false;       
        SysUser pUser = new SysUser();

        private PC00Z01 sqlBiz = new PC00Z01();
        public delegate void OnUserLogInChangedDelegate();
        public event OnUserLogInChangedDelegate OnUserLoginChanged = null;

        private CheckDBStatus[] dbStatus = null;
        EquipmentMonitor equipMonitor = null;
        CurrentTagValueMonitorDGV currentTagValueMonitor = null;
        PIAlarmMonitor PIMonitor = null;
        Form_Splash splash = null;
        public bool bTerminal = false;
        Thread threadStatus = null;
        bool m_bDb1StatusEnable = true;
        bool m_bDb2StatusEnable = true;
        bool m_bDb3StatusEnable = true;
        bool m_bDbPgmStatusEnable = true;
        bool m_bPiPgmStatusEnable = true;

        string strSvrName = Environment.MachineName;
        string strSvrCode = String.Empty;

        public MonitorForm()
        {
            InitializeComponent();
            ReadStatusConfig();
        }

        public string AssemblyTitle
        {
            get
            {
                object[] attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyTitleAttribute), false);
                if (attributes.Length > 0)
                {
                    AssemblyTitleAttribute titleAttribute = (AssemblyTitleAttribute)attributes[0];
                    if (titleAttribute.Title != "")
                    {
                        return titleAttribute.Title;
                    }
                }
                return System.IO.Path.GetFileNameWithoutExtension(Assembly.GetExecutingAssembly().CodeBase);
            }
        }

        public string AssemblyVersion
        {
            get
            {
                return Assembly.GetExecutingAssembly().GetName().Version.ToString();
            }
        }
        private void SplashThread()
        {
            splash = new Form_Splash();
            splash.ShowDialog();

        }

        private void StatusThread()
        {
            string errCode = string.Empty;
            string errText = string.Empty;
            bool result = true;

            string CurDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

            while (!bTerminal)
            {
                try
                {
                    
                    //데이타 조회
                    DataTable dtStatus = sqlBiz.GetProgramStatus2(ref errCode, ref errText);

                    foreach (DataRow dr in dtStatus.Rows)
                    {
                        int nStatus = Convert.ToInt16(dr[8].ToString());
                        string strEqName = dr[1].ToString();
                        
                        if (!string.IsNullOrWhiteSpace(strEqName))
                        {
                            switch (strEqName)
                            {
                                case "DataSpiderPC02":
                                    
                                    if (this.InvokeRequired)
                                    {
                                        this.Invoke(new MethodInvoker(delegate ()
                                        {
                                            toolStripStatusLabel_DBPGM_Status.Image = imageList1.Images[nStatus];
                                        }));
                                    }
                                    else
                                    {
                                        toolStripStatusLabel_DBPGM_Status.Image = imageList1.Images[nStatus];
                                    }

                                    break;
                                case "DataSpiderPC03":
                                    if (this.InvokeRequired)
                                    {
                                        this.Invoke(new MethodInvoker(delegate ()
                                        {
                                            toolStripStatusLabel_PIPGM_Status.Image = imageList1.Images[nStatus];
                                        }));
                                    }
                                    else
                                    {
                                        toolStripStatusLabel_PIPGM_Status.Image = imageList1.Images[nStatus];
                                    }
                                    
                                    break;
                                default:
                                    break;
                            }
                        }
                    }                    
                }
                catch (Exception ex)
                {   
                }
                finally
                {
                    threadStatus.Join(5000);
                }
            }
        }


        private void Form1_Load(object sender, EventArgs e)
        {
            
            if (ConfigHelper.GetAppSetting("SPLASH").Contains("y") )
            {
                Thread threadSplash = new Thread(SplashThread);
                threadSplash.Start();
            }
            string[] args = Environment.GetCommandLineArgs();

            //bool bRunModeVirtualMachine = false;

            //for (int nA = 0; nA < args.Length; nA++)
            //{
            //    switch (args[nA].Trim().ToUpper())
            //    {
            //        case "-S":
            //            if (args.Length > (nA + 1)) strSvrName = args[nA + 1];
            //            break;
            //    }
            //}

            //threadStatus = new Thread(StatusThread);
            //threadStatus.Start();

            Text = $"{AssemblyTitle} V.{AssemblyVersion}";
            adminToolStripMenuItem.Visible = userToolStripMenuItem.Visible = false;

            SetSBL();

            //if (bRunModeVirtualMachine == true)
            //{
            //    //MessageBox.Show("View Mode. Do not run programs automatically.");
            //}
            //else
            //{
            //    ExecutePrograms();
            //    //Thread.Sleep(60 * 1000);
            //}
            string strErrCode = string.Empty;
            string strErrText = string.Empty;

            int MY_ID = sqlBiz.GetServerId(Environment.MachineName);
            if (MY_ID == -1)
            {
                MessageBox.Show(strErrText, $"Server Code is not exist in database", MessageBoxButtons.OK, MessageBoxIcon.Information);
                Application.Exit();
            }
            strSvrCode = (MY_ID == 0) ? "P" : "S";
            toolStripStatusLabel_ServerName.Text = $"Server = {strSvrName}({strSvrCode})";

            ////////////////////////////////////////////////////

            dbStatus = new CheckDBStatus[1];
            dbStatus[0] = new CheckDBStatus($"DB", 60 * 1000);
            dbStatus[0].DBConnectionString = CFW.Common.SecurityUtil.DecryptString(CFW.Configuration.ConfigManager.Default.ReadConfig("connectionStrings", $"SQL_ConnectionString"));
            dbStatus[0].OnDBStatusChanged += OnDBStatusChanged;
            dbStatus[0].Start();
            splash?.Stop();
        }

        private void OnDBStatusChanged(string name, int status)
        {
            if (this.InvokeRequired)
            {
                this.Invoke((MethodInvoker) delegate { OnDBStatusChanged(name, status); });
            }
            else
            {
                switch (name)
                {
                    case "DB1":
                        toolStripStatusLabel_DB1_Status.Image = imageList1.Images[status];
                        break;
                    case "DB2":
                        toolStripStatusLabel_DB2_Status.Image = imageList1.Images[status];
                        break;
                    case "DB3":
                        toolStripStatusLabel_DB3_Status.Image = imageList1.Images[status];
                        break;
                    default:
                        break;
                }

                //
                // 2022. 2. 15 : Han,Ilho
                //  Add DB Source Name to prevent wrong DB use
                //
                if (dbStatus.Length > 0)
                {
                    int[] nSPos = { -1, -1 };

                    nSPos[0] = dbStatus[0].DBConnectionString.IndexOf("Data Source", 0);

                    if( nSPos[0] >= 0 )
                    {
                        nSPos[1] = dbStatus[0].DBConnectionString.IndexOf(";", nSPos[0] + 1);

                        toolStripStatusLabel_MainDBSourceName.Text = dbStatus[0].DBConnectionString.Substring( nSPos[0], nSPos[1] - nSPos[0] );
                    }
                    else
                    {
                        toolStripStatusLabel_MainDBSourceName.Text = "No MSSQL DB";
                    }
                }
                else
                {
                    toolStripStatusLabel_MainDBSourceName.Text = "No Main DB";
                }
                //---------------
            }
        }

        

        private void SetSBL()
        {

            try
            {
                //foreach ( AuthofForm pAuth in pUser.권한들)
                //{
                //    if (pAuth.Read || pAuth.Write && pAuth.FormName == "SEIMM.UserMonitor.EditForm")
                //    {
                //        UserFormInfo pInfo = new UserFormInfo(pAuth.FormName, "testst");
                //        pDispView.AddFormToTab(pInfo);
                //    }
                //    if (!pAuth.Write && pAuth.FormName == "SEIMM.MonitorForm") ;
                //    {

                //        this.초기화ToolStripMenuItem.Enabled = false;
                //    }
                //}

                //SBL Data Control
                m_pSBLDataCtrl = new EquipCtrl();
                pTreeForm = new TreeForm(this);
                pPanelView.SetFormToPanel(pTreeForm);

                // TreeView 왼쪽 표시
                m_pSBLDataCtrl.OnChangeDataEvent += new EquipCtrl.OnChangeDataHandler(pTreeForm.OnChangeTreeData);
                pTreeForm.OnRefreshTreeData += new TreeForm.OnRefreshTreeDataDelegate(m_pSBLDataCtrl.InitData);
                m_pSBLDataCtrl.InitData();
                // 오른쪽. TabControl 
                //ListForm pListForm = new ListForm();
                //pDispView.AddFormToTab(pListForm);
                //m_pSBLDataCtrl.OnChangeDataEvent += new EquipCtrl.OnChangeDataHandler(pListForm.OnChangeListData);
                //pTreeForm.treeView1.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(pListForm.treeView1_AfterSelect);

                //GridForm pGridForm = new GridForm();
                //pDispView.AddFormToTab(pGridForm);
                //pTreeForm.treeView1.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(pGridForm.treeView1_AfterSelect);

                equipMonitor = new EquipmentMonitor();
                equipMonitor.Text = "Equipment Monitor";
                pDispView.AddFormToTab(equipMonitor);
                pTreeForm.treeViewEQStatus.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(equipMonitor.treeView_AfterSelect);
                OnUserLoginChanged += new OnUserLogInChangedDelegate(pTreeForm.UserLogInChanged);
                equipMonitor.OnRefreshTreeData += new EquipmentMonitor.OnRefreshTreeDataDelegate(m_pSBLDataCtrl.InitData);
                pDispView.OnTabControlSelectedIndexChanged += new TabFromCtrl.OnTabControlSelectedIndexChangedDelegate(equipMonitor.TabControl_SelectedIndexChanged);
                OnUserLoginChanged += new OnUserLogInChangedDelegate(equipMonitor.UserLogInChanged);

                currentTagValueMonitor = new CurrentTagValueMonitorDGV();
                currentTagValueMonitor.Text = "TAG Value Monitor";
                pDispView.AddFormToTab(currentTagValueMonitor);
                pTreeForm.treeViewEQStatus.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(currentTagValueMonitor.treeView_AfterSelect);
                pDispView.OnTabControlSelectedIndexChanged += new TabFromCtrl.OnTabControlSelectedIndexChangedDelegate(currentTagValueMonitor.TabControl_SelectedIndexChanged);
                OnUserLoginChanged += new OnUserLogInChangedDelegate(currentTagValueMonitor.UserLogInChanged);

                PIMonitor = new PIAlarmMonitor();
                PIMonitor.Text = "PI Alarm Monitor";
                pDispView.AddFormToTab(PIMonitor);
                pTreeForm.treeViewEQStatus.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(PIMonitor.treeView_AfterSelect);
                pDispView.OnTabControlSelectedIndexChanged += new TabFromCtrl.OnTabControlSelectedIndexChangedDelegate(PIMonitor.TabControl_SelectedIndexChanged);

                pDispView.SelectTabIndex(0);
                equipMonitor.TabControl_SelectedIndexChanged(equipMonitor);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void PDispView_TabIndexChanged(object sender, EventArgs e)
        {
            int selected = (sender as TabControl).SelectedIndex;

        }

        private void 초기화ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            m_pSBLDataCtrl.InitData();
        }

        private void 큰ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //this.큰ToolStripMenuItem.Checked 
            //pListForm.SetViewStyle(View.LargeIcon);
        }

        private void 자세히ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //pListForm.listView1.View = View.Details;
        }

        private void MonitorForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            DialogResult dialogResult = MessageBox.Show(PC00D01.MSGP0001, PC00D01.MSGP0002, MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (!dialogResult.Equals(DialogResult.Yes))
            {
                e.Cancel = true;
                return;
            }


            m_pSBLDataCtrl.OnChangeDataEvent -= new EquipCtrl.OnChangeDataHandler(pTreeForm.OnChangeTreeData);
            pTreeForm.OnRefreshTreeData -= new TreeForm.OnRefreshTreeDataDelegate(m_pSBLDataCtrl.InitData);

            pTreeForm.treeViewEQStatus.AfterSelect -= new System.Windows.Forms.TreeViewEventHandler(equipMonitor.treeView_AfterSelect);
            OnUserLoginChanged -= new OnUserLogInChangedDelegate(pTreeForm.UserLogInChanged);
            equipMonitor.OnRefreshTreeData -= new EquipmentMonitor.OnRefreshTreeDataDelegate(m_pSBLDataCtrl.InitData);
            pDispView.OnTabControlSelectedIndexChanged -= new TabFromCtrl.OnTabControlSelectedIndexChangedDelegate(equipMonitor.TabControl_SelectedIndexChanged);

            OnUserLoginChanged -= new OnUserLogInChangedDelegate(equipMonitor.UserLogInChanged);
            pTreeForm.treeViewEQStatus.AfterSelect -= new System.Windows.Forms.TreeViewEventHandler(currentTagValueMonitor.treeView_AfterSelect);
            pDispView.OnTabControlSelectedIndexChanged -= new TabFromCtrl.OnTabControlSelectedIndexChangedDelegate(currentTagValueMonitor.TabControl_SelectedIndexChanged);
            OnUserLoginChanged -= new OnUserLogInChangedDelegate(currentTagValueMonitor.UserLogInChanged);

            pTreeForm.treeViewEQStatus.AfterSelect -= new System.Windows.Forms.TreeViewEventHandler(PIMonitor.treeView_AfterSelect);
            pDispView.OnTabControlSelectedIndexChanged -= new TabFromCtrl.OnTabControlSelectedIndexChangedDelegate(PIMonitor.TabControl_SelectedIndexChanged);

            pTreeForm.threadStop = true;
            equipMonitor.threadStop = true;
            currentTagValueMonitor.threadStop = true;
            PIMonitor.threadStop = true;

            for (int i = 0; i < dbStatus.Length; i++)
            {
                dbStatus[i].Stop();
            }

            bTerminal = true;

            Thread.Sleep(100);

            if (threadStatus != null && threadStatus.IsAlive)
            {
                threadStatus.Abort();
            }

            pDispView.Dispose();

            pTreeForm.Close();
            equipMonitor.Close();
            currentTagValueMonitor.Close();
            PIMonitor.Close();
        }

        private void SEIMM정보ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AboutBox_Conwell form = new AboutBox_Conwell();
            form.ShowDialog(this);
        }

        private void toolStripButton_ExpandAll_Click(object sender, EventArgs e)
        {
            pTreeForm.treeViewEQStatus.ExpandAll();
        }

        private void toolStripButton_CollapseAll_Click(object sender, EventArgs e)
        {
            pTreeForm.treeViewEQStatus.CollapseAll();
        }

        private void toolStripButton_Log_Click(object sender, EventArgs e)
        {
            if (UserAuthentication.IsAuthorized)
            {
                if (DialogResult.Yes.Equals(MessageBox.Show("Do you want to log out ?", "Log Out", MessageBoxButtons.YesNo)))
                {
                    UserAuthentication.LogOut();
                    toolStripLabel_User.Text = "User";// "Not logged in";
                    toolStripButton_Log.Text = "Log In";
                    OnUserLoginChanged?.Invoke();
                    adminToolStripMenuItem.Visible = userToolStripMenuItem.Visible = false;
                }
            }
            else
            {
                Form_LogIn frm = new Form_LogIn();
                if (DialogResult.OK.Equals(frm.ShowDialog(this)))
                {
                    if (UserAuthentication.IsAuthorized)
                    {
                        toolStripLabel_User.Text = $"{UserAuthentication.UserID} ({UserAuthentication.UserName}) - {UserAuthentication.UserLevel}";
                        toolStripButton_Log.Text = "Log Out";
                        OnUserLoginChanged?.Invoke();
                        userToolStripMenuItem.Visible = true;
                        adminToolStripMenuItem.Visible = UserAuthentication.UserLevel.Equals(UserLevel.Admin);
                    }
                }
            }
        }

        private void 종료ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void userInfoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (UserAuthentication.IsAuthorized)
            {
                UserAddEdit frm = new UserAddEdit(UserAuthentication.UserID);
                frm.ShowDialog(this);
            }
        }

        private void toolStripLabel_User_Click(object sender, EventArgs e)
        {
            if (UserAuthentication.IsAuthorized)
            {
                UserAddEdit frm = new UserAddEdit(UserAuthentication.UserID);
                frm.ShowDialog(this);
            }
        }

        private void ReadStatusConfig()
        {
            toolStripStatusLabel_DB1_Status.Visible = true;
            toolStripStatusLabel_DB2_Status.Visible = false;
            toolStripStatusLabel_DB3_Status.Visible = false;

            m_bDbPgmStatusEnable = ConfigHelper.GetAppSetting("DbPgmStatusEnable").Trim().ToUpper().Equals("Y");
            if (m_bDbPgmStatusEnable) toolStripStatusLabel_DBPGM_Status.Visible = true;
            else toolStripStatusLabel_DBPGM_Status.Visible = false;

            m_bPiPgmStatusEnable = ConfigHelper.GetAppSetting("PiPgmStatusEnable").Trim().ToUpper().Equals("Y");
            if (m_bPiPgmStatusEnable) toolStripStatusLabel_PIPGM_Status.Visible = true;
            else toolStripStatusLabel_PIPGM_Status.Visible = false;
        }

        private void tagGroupToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (UserAuthentication.IsAuthorized )
            {
                if (UserAuthentication.UserLevel == UserLevel.Admin || UserAuthentication.UserLevel == UserLevel.Manager)
                {
                    ConfigTagGroup frm = new ConfigTagGroup();
                    frm.ShowDialog(this);
                }
            }
        }

        private void systemConfigCToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (UserAuthentication.IsAuthorized)
            {
                if (UserAuthentication.UserLevel == UserLevel.Admin || UserAuthentication.UserLevel == UserLevel.Manager)
                {
                    ConfigSystem dlg = new ConfigSystem();

                    dlg.ShowDialog();
                }
            }
        }
    }
}
