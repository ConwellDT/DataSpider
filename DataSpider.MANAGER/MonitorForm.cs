﻿using System;
using System.Configuration;
using System.Data;
using System.Drawing;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;

using DataSpider.PC00.PT;
using DataSpider.UserMonitor;

using LibraryWH.FormCtrl;

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
        public static bool showAllEquipmtStatus { get; set; }
        private ToolStripMenuItem clickedToolStripMenuItem;
        private CheckDBStatus dbStatus = null;
        EquipmentMonitor equipMonitor = null;
        CurrentTagValueMonitorDGV currentTagValueMonitor = null;
        PIAlarmMonitor PIMonitor = null;
        SystemLogView systemLogview = null;
        Form_Splash splash = null;
        public bool bTerminal = false;
        public bool isChecked = true; 
        Thread threadStatus = null;
        bool m_bDbPgmStatusEnable = true;
        bool m_bPiPgmStatusEnable = true;

        string strSvrName = Environment.MachineName;
        string strSvrCode = String.Empty;

        private EventFrameMonitor eventFrameMonitor = null;
        private EventFrameAlarmMonitor eventFrameAlarm = null;

        private int MY_ID = -1;

        public MonitorForm()
        {
            InitializeComponent();
            ReadStatusConfig();
            toolToolsInit();
        }

        public void toolToolsInit()
        {
            string errCode = string.Empty;
            string errText = string.Empty;
            string equipmentType = string.Empty;
            string useFlag = string.Empty;

            DataTable equipmentTypes = sqlBiz.GetEquipType(ref errCode, ref errText);

            if (equipmentTypes != null && equipmentTypes.Rows.Count > 0)
            {
                ToolStripMenuItem existingMenuItem = viewToolStripMenuItem;

                // equipment type 항목 추가
                foreach (DataRow row in equipmentTypes.Rows)
                {
                    equipmentType = row["EQUIP_NM_VALUE"].ToString();
                    useFlag = row["USE_FLAG"].ToString();

                    ToolStripMenuItem item = new ToolStripMenuItem(equipmentType);
                    item.CheckOnClick = true;

                    item.Checked = useFlag.Equals("Y", StringComparison.OrdinalIgnoreCase);
                    item.ForeColor = item.Checked ? Color.Black : Color.DarkGray;

                    //이벤트 핸들러 추가
                    item.Click += EquipmentType_Click;
                    existingMenuItem.DropDownItems.Add(item);
                }
                this.Refresh();
            }
        }

        private void EquipmentType_Click(object sender, EventArgs e)
        {
            string errCode = string.Empty;
            string errText = string.Empty;
            ToolStripMenuItem clickedItem = sender as ToolStripMenuItem;
            if (clickedItem != null)
            {
                clickedToolStripMenuItem = clickedItem;
                clickedToolStripMenuItem.ForeColor = clickedToolStripMenuItem.Checked ? Color.Black : Color.DarkGray;
                isChecked = clickedToolStripMenuItem.Checked;

                string equipType = clickedToolStripMenuItem.Text.ToString();
                int index = equipType.IndexOf("(");
                string equipTypeNm = equipType.Substring(0, index).Trim().ToString();

                sqlBiz.UpdateEquipTypeFlag(equipTypeNm, isChecked, ref errCode, ref errText);
            }
            // 20241010, SHS, SetSBL 은 최초 1회만 해야 함
            //SetSBL();
            m_pSBLDataCtrl.InitData(!showAllEquipmtToolStripMenuItem.Checked);

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

            foreach (DataRow dr in dtStatus?.Rows)
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
            string dataSpiderPC02P_ErrorFile = sqlBiz.ReadSTCommon("DataSpiderPC02P", "ERROR_FILE").Trim();
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
            string dataSpiderPC02S_ErrorFile = sqlBiz.ReadSTCommon("DataSpiderPC02S", "ERROR_FILE").Trim();
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
            MY_ID = sqlBiz.GetServerId(Environment.MachineName);
            if (MY_ID == -1)
            {
                MessageBox.Show($"Server Code does not exists in database. The program will terminate", this.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Information);
                Application.Exit();
                return;
            }

            if (ConfigHelper.GetAppSetting("SPLASH").Trim().ToUpper().Equals("Y"))//.Contains("y") )
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
            viewToolStripMenuItem.Visible = userToolStripMenuItem.Visible = false;
            toolsToolStripMenuItem.Visible = userToolStripMenuItem.Visible = false;

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
                this.Invoke((MethodInvoker)delegate { OnDBStatusChanged(name, status); });
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

                showAllEquipmtStatus = showAllEquipmtToolStripMenuItem.Checked == false ? true : false;
               
                // TreeView 왼쪽 표시
                m_pSBLDataCtrl.OnChangeDataEvent += new EquipCtrl.OnChangeDataHandler(pTreeForm.OnChangeTreeData);
                pTreeForm.OnRefreshTreeData += new TreeForm.OnRefreshTreeDataDelegate(m_pSBLDataCtrl.InitData);
                m_pSBLDataCtrl.InitData(showAllEquipmtStatus);
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

                //efMonitor
                eventFrameMonitor = new EventFrameMonitor();
                eventFrameMonitor.Text = "EventFrame Monitor";
                pDispView.AddFormToTab(eventFrameMonitor);
                pTreeForm.treeViewEQStatus.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(eventFrameMonitor.treeView_AfterSelect);
                pDispView.OnTabControlSelectedIndexChanged += new TabFromCtrl.OnTabControlSelectedIndexChangedDelegate(eventFrameMonitor.TabControl_SelectedIndexChanged);
                OnUserLoginChanged += new OnUserLogInChangedDelegate(eventFrameMonitor.UserLogInChanged);

                eventFrameAlarm = new EventFrameAlarmMonitor();
                eventFrameAlarm.Text = "EventFrame Alarm Monitor";
                pDispView.AddFormToTab(eventFrameAlarm);
                pTreeForm.treeViewEQStatus.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(eventFrameAlarm.treeView_AfterSelect);
                pDispView.OnTabControlSelectedIndexChanged += new TabFromCtrl.OnTabControlSelectedIndexChangedDelegate(eventFrameAlarm.TabControl_SelectedIndexChanged);
                OnUserLoginChanged += new OnUserLogInChangedDelegate(eventFrameAlarm.UserLogInChanged);

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
            //showAllEquipmtStatus = showAllEquipmtToolStripMenuItem.Checked == false ? true : false;
            //m_pSBLDataCtrl.InitData(showAllEquipmtStatus);
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
            if (MY_ID == -1) return;

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

            pTreeForm.treeViewEQStatus.AfterSelect -= new System.Windows.Forms.TreeViewEventHandler(eventFrameMonitor.treeView_AfterSelect);
            pDispView.OnTabControlSelectedIndexChanged -= new TabFromCtrl.OnTabControlSelectedIndexChangedDelegate(eventFrameMonitor.TabControl_SelectedIndexChanged);
            OnUserLoginChanged -= new OnUserLogInChangedDelegate(eventFrameMonitor.UserLogInChanged);

            pTreeForm.treeViewEQStatus.AfterSelect -= new System.Windows.Forms.TreeViewEventHandler(PIMonitor.treeView_AfterSelect);
            pDispView.OnTabControlSelectedIndexChanged -= new TabFromCtrl.OnTabControlSelectedIndexChangedDelegate(PIMonitor.TabControl_SelectedIndexChanged);

            pTreeForm.treeViewEQStatus.AfterSelect -= new System.Windows.Forms.TreeViewEventHandler(eventFrameAlarm.treeView_AfterSelect);
            pDispView.OnTabControlSelectedIndexChanged -= new TabFromCtrl.OnTabControlSelectedIndexChangedDelegate(eventFrameAlarm.TabControl_SelectedIndexChanged);

            pTreeForm.threadStop = true;
            equipMonitor.threadStop = true;
            currentTagValueMonitor.threadStop = true;
            PIMonitor.threadStop = true;
            systemLogview.threadStop = true;

            eventFrameMonitor.threadStop = true;
            eventFrameAlarm.threadStop = true;

            dbStatus?.Stop();

            bTerminal = true;

            Thread.Sleep(100);

            if (threadStatus != null && threadStatus.IsAlive)
            {
                threadStatus.Abort();
            }

            pTreeForm.Close();
            equipMonitor.Close();
            currentTagValueMonitor.Close();
            PIMonitor.Close();
            eventFrameMonitor.Close();
            eventFrameAlarm.Close();

            pDispView.Dispose();
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
                    viewToolStripMenuItem.Visible = userToolStripMenuItem.Visible = false;
                    toolsToolStripMenuItem.Visible = userToolStripMenuItem.Visible = false;

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
                        viewToolStripMenuItem.Visible = UserAuthentication.UserLevel.Equals(UserLevel.Admin);
                        toolsToolStripMenuItem.Visible = UserAuthentication.UserLevel.Equals(UserLevel.Admin);
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
            if (UserAuthentication.IsAuthorized)
            {
                if (UserAuthentication.UserLevel == UserLevel.Admin || UserAuthentication.UserLevel == UserLevel.Manager)
                {
                    ConfigTagGroup frm = new ConfigTagGroup();
                    frm.ShowDialog(this);
                    currentTagValueMonitor.UpdatecomboBoxTagGroupSel();
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

        private void commonCodeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (UserAuthentication.IsAuthorized)
            {
                if (UserAuthentication.UserLevel == UserLevel.Admin || UserAuthentication.UserLevel == UserLevel.Manager)
                {
                    CommonCodeConfig frm = new CommonCodeConfig();
                    frm.ShowDialog(this);
                    currentTagValueMonitor.UpdatecomboBoxTagGroupSel();
                }
            }
        }

        private void showAllEquipmtToolStripMenuItem_Click(object sender, EventArgs e)
        {
            showAllEquipmtStatus = showAllEquipmtToolStripMenuItem.Checked == false ? true : false;
            if (!showAllEquipmtToolStripMenuItem.Checked) //false
            {
               
                m_pSBLDataCtrl.InitData(showAllEquipmtStatus);
            }
            else
            {
                if (DialogResult.Yes.Equals(MessageBox.Show("Want to see all the equipmt ?", "Show All Equipmt", MessageBoxButtons.YesNo)))
                {

                    m_pSBLDataCtrl.InitData(showAllEquipmtStatus);
                }
                else
                {
                    showAllEquipmtToolStripMenuItem.Checked = false;
                }

            }

        }

        private void dateTimeParsingToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (UserAuthentication.IsAuthorized)
            {
                if (UserAuthentication.UserLevel == UserLevel.Admin || UserAuthentication.UserLevel == UserLevel.Manager)
                {
                    DateTimeParse frm = new DateTimeParse();
                    frm.ShowDialog(this);
                }
            }
        }

        private void configurationManagerAppSettingToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (UserAuthentication.IsAuthorized)
            {
                if (UserAuthentication.UserLevel == UserLevel.Admin || UserAuthentication.UserLevel == UserLevel.Manager)
                {
                    ConfigurationManagerAppSetting frm = new ConfigurationManagerAppSetting();
                    frm.ShowDialog(this);
                    currentTagValueMonitor.UpdatecomboBoxTagGroupSel();
                }
            }
        }

        private void tagParsingToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (UserAuthentication.IsAuthorized)
            {
                if (UserAuthentication.UserLevel == UserLevel.Admin || UserAuthentication.UserLevel == UserLevel.Manager)
                {
                    TagParsing frm = new TagParsing();
                    frm.ShowDialog(this);
                }
            }
        }
    }
}
