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

        private CheckDBStatus dbStatus = null;
        EquipmentMonitor equipMonitor = null;
        CurrentTagValueMonitorDGV currentTagValueMonitor = null;
        PIAlarmMonitor PIMonitor = null;
        SystemLogView systemLogview = null;
        Form_Splash splash = null;
        public bool bTerminal = false;
        Thread threadStatus = null;
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

        private void GetProgramStatus()
        {
            string errCode = string.Empty;
            string errText = string.Empty;
            DataTable dtStatus = sqlBiz.GetProgramStatus2(ref errCode, ref errText);

            foreach (DataRow dr in dtStatus.Rows)
            {
                int nStatus = Convert.ToInt16(dr[2].ToString());
                string strEqName = dr[1].ToString();

                if (!string.IsNullOrWhiteSpace(strEqName))
                {
                    switch (strEqName)
                    {
                        case "DataSpiderPC02P":

                            if (this.InvokeRequired)
                            {
                                this.Invoke(new MethodInvoker(delegate ()
                                {
                                    toolStripStatusLabel_DBPGM_P_Status.Image = imageList_EquipState.Images[dr[0].ToString()];
                                    toolStripStatusLabel_DBPGM_P_Status.ToolTipText = dr[0].ToString();
                                }));
                            }
                            else
                            {
                                toolStripStatusLabel_DBPGM_P_Status.Image = imageList_EquipState.Images[dr[0].ToString()];
                                toolStripStatusLabel_DBPGM_P_Status.ToolTipText = dr[0].ToString();
                            }
                            break;
                        case "DataSpiderPC02S":

                            if (this.InvokeRequired)
                            {
                                this.Invoke(new MethodInvoker(delegate ()
                                {
                                    toolStripStatusLabel_DBPGM_S_Status.Image = imageList_EquipState.Images[dr[0].ToString()];
                                    toolStripStatusLabel_DBPGM_S_Status.ToolTipText = dr[0].ToString();
                                }));
                            }
                            else
                            {
                                toolStripStatusLabel_DBPGM_S_Status.Image = imageList_EquipState.Images[dr[0].ToString()];
                                toolStripStatusLabel_DBPGM_S_Status.ToolTipText = dr[0].ToString();
                            }
                            break;
                        case "DataSpiderPC03":
                            if (this.InvokeRequired)
                            {
                                this.Invoke(new MethodInvoker(delegate ()
                                {
                                    toolStripStatusLabel_PIPGM_Status.Image = imageList_EquipState.Images[dr[0].ToString()];
                                    toolStripStatusLabel_PIPGM_Status.ToolTipText = dr[0].ToString();
                                }));
                            }
                            else
                            {
                                toolStripStatusLabel_PIPGM_Status.Image = imageList_EquipState.Images[dr[0].ToString()];
                                toolStripStatusLabel_PIPGM_Status.ToolTipText = dr[0].ToString();
                            }
                            break;
                        case "PIConnection":
                            if (this.InvokeRequired)
                            {
                                this.Invoke(new MethodInvoker(delegate ()
                                {
                                    toolStripStatusLabel_PI_Status.Image = imageList_EquipState.Images[dr[0].ToString()];
                                    toolStripStatusLabel_PI_Status.ToolTipText = dr[0].ToString();
                                }));
                            }
                            else
                            {
                                toolStripStatusLabel_PI_Status.Image = imageList_EquipState.Images[dr[0].ToString()];
                                toolStripStatusLabel_PI_Status.ToolTipText = dr[0].ToString();
                            }
                            break;
                        default:
                            break;
                    }
                }
            }
        }

        private void GetPC02ErrorFileStatus()
        {
            string dataSpiderPC02P_ErrorFile = sqlBiz.ReadSTCommon("ERROR_STATUS", "DataSpiderPC02P").Trim();
            if (this.InvokeRequired)
            {
                this.Invoke(new MethodInvoker(delegate ()
                {
                    toolStripStatusLabel_DBPGM_P_ErrorFile.Text = string.IsNullOrWhiteSpace(dataSpiderPC02P_ErrorFile) ? "No ErrorFile" : dataSpiderPC02P_ErrorFile;
                    toolStripStatusLabel_DBPGM_P_ErrorFile.ForeColor = string.IsNullOrWhiteSpace(dataSpiderPC02P_ErrorFile) ? Color.Black : Color.Red;
                    toolStripStatusLabel_DBPGM_P_ErrorFile.Visible = true;
                }));
            }
            else
            {
                toolStripStatusLabel_DBPGM_P_ErrorFile.Text = string.IsNullOrWhiteSpace(dataSpiderPC02P_ErrorFile) ? "No ErrorFile" : dataSpiderPC02P_ErrorFile;
                toolStripStatusLabel_DBPGM_P_ErrorFile.ForeColor = string.IsNullOrWhiteSpace(dataSpiderPC02P_ErrorFile) ? Color.Black : Color.Red;
                toolStripStatusLabel_DBPGM_P_ErrorFile.Visible = true;
            }
            string dataSpiderPC02S_ErrorFile = sqlBiz.ReadSTCommon("ERROR_STATUS", "DataSpiderPC02S").Trim();
            if (this.InvokeRequired)
            {
                this.Invoke(new MethodInvoker(delegate ()
                {
                    toolStripStatusLabel_DBPGM_S_ErrorFile.Text = string.IsNullOrWhiteSpace(dataSpiderPC02S_ErrorFile) ? "No ErrorFile" : dataSpiderPC02S_ErrorFile;
                    toolStripStatusLabel_DBPGM_S_ErrorFile.ForeColor = string.IsNullOrWhiteSpace(dataSpiderPC02S_ErrorFile) ? Color.Black : Color.Red;
                    toolStripStatusLabel_DBPGM_S_ErrorFile.Visible = true;
                }));
            }
            else
            {
                toolStripStatusLabel_DBPGM_S_ErrorFile.Text = string.IsNullOrWhiteSpace(dataSpiderPC02S_ErrorFile) ? "No ErrorFile" : dataSpiderPC02S_ErrorFile;
                toolStripStatusLabel_DBPGM_S_ErrorFile.ForeColor = string.IsNullOrWhiteSpace(dataSpiderPC02S_ErrorFile) ? Color.Black : Color.Red;
                toolStripStatusLabel_DBPGM_S_ErrorFile.Visible = true;
            }
        }
        private void StatusThread()
        {
            while (!bTerminal)
            {
                try
                {
                    GetProgramStatus();
                    GetPC02ErrorFileStatus();
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

            threadStatus = new Thread(StatusThread);
            threadStatus.Start();

            Text = $"{AssemblyTitle} V.{AssemblyVersion}";
            configCToolStripMenuItem.Visible = userToolStripMenuItem.Visible = false;

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
                MessageBox.Show($"Server Code does not exist in database", this.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Information);
                Application.Exit();
            }
            strSvrCode = (MY_ID == 0) ? "P" : "S";
            toolStripStatusLabel_ServerName.Text = $"Server = {strSvrName}({strSvrCode})";

            dbStatus = new CheckDBStatus($"DB", 60 * 1000);
            dbStatus.DBConnectionString = CFW.Common.SecurityUtil.DecryptString(CFW.Configuration.ConfigManager.Default.ReadConfig("connectionStrings", $"SQL_ConnectionString"));

            int[] nSPos = { -1, -1 };
            toolStripStatusLabel_MainDBSourceName.Text = "No MSSQL DB";
            nSPos[0] = dbStatus.DBConnectionString.IndexOf("Data Source", 0);
            if (nSPos[0] >= 0)
            {
                nSPos[1] = dbStatus.DBConnectionString.IndexOf(";", nSPos[0] + 1);
                toolStripStatusLabel_MainDBSourceName.Text = dbStatus.DBConnectionString.Substring(nSPos[0], nSPos[1] - nSPos[0]);
            }

            dbStatus.OnDBStatusChanged += OnDBStatusChanged;
            dbStatus.Start();
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
                toolStripStatusLabel_DB_Status.Image = imageList1.Images[status];
                toolStripStatusLabel_DB_Status.ToolTipText = status.Equals(1) ? "Connected" : "Disconnected";
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
                OnUserLoginChanged += new OnUserLogInChangedDelegate(PIMonitor.UserLogInChanged);

                systemLogview = new SystemLogView();
                systemLogview.Text = "System Log";
                pDispView.AddFormToTab(systemLogview);
                //pTreeForm.treeViewEQStatus.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(systemLogview.treeView_AfterSelect);
                pDispView.OnTabControlSelectedIndexChanged += new TabFromCtrl.OnTabControlSelectedIndexChangedDelegate(systemLogview.TabControl_SelectedIndexChanged);
                //OnUserLoginChanged += new OnUserLogInChangedDelegate(systemLogview.UserLogInChanged);


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
            systemLogview.threadStop = true;

            dbStatus?.Stop();

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
                    configCToolStripMenuItem.Visible = userToolStripMenuItem.Visible = false;
                     
                }
            }
            else
            {
                Form_LogIn frm = new Form_LogIn();
                if (DialogResult.OK.Equals(frm.ShowDialog(this)))
                {
                    if (UserAuthentication.IsAuthorized)
                    {
                        toolStripLabel_User.Text = $"{UserAuthentication.UserID}";// ({UserAuthentication.UserName})";// - {UserAuthentication.UserLevel}";
                        toolStripButton_Log.Text = "Log Out";
                        OnUserLoginChanged?.Invoke();
                        userToolStripMenuItem.Visible = true;
                        configCToolStripMenuItem.Visible = UserAuthentication.UserLevel.Equals(UserLevel.Admin);
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
            toolStripStatusLabel_DB_Status.Visible = true;
            toolStripStatusLabel_PI_Status.Visible = true;
            toolStripStatusLabel_DBPGM_S_Status.Visible = !ConfigHelper.GetAppSetting("DbPgmStatusEnable").Trim().ToUpper().Equals("N");
            toolStripStatusLabel_PIPGM_Status.Visible = !ConfigHelper.GetAppSetting("PiPgmStatusEnable").Trim().ToUpper().Equals("N");
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
