
namespace DataSpider
{
    partial class MonitorForm
    {
        /// <summary>
        /// 필수 디자이너 변수입니다.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 사용 중인 모든 리소스를 정리합니다.
        /// </summary>
        /// <param name="disposing">관리되는 리소스를 삭제해야 하면 true이고, 그렇지 않으면 false입니다.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form 디자이너에서 생성한 코드

        /// <summary>
        /// 디자이너 지원에 필요한 메서드입니다. 
        /// 이 메서드의 내용을 코드 편집기로 수정하지 마세요.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MonitorForm));
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.pPanelView = new LibraryWH.FormCtrl.PanelForm();
            this.pDispView = new LibraryWH.FormCtrl.TabFromCtrl();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.파일ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator4 = new System.Windows.Forms.ToolStripSeparator();
            this.종료ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.configCToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.tagGroupToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.commonCodeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.userToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.userInfoToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.viewToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.showAllEquipmtToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator5 = new System.Windows.Forms.ToolStripSeparator();
            this.toolsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.dateTimeParsingToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.도움말ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.sEIMM정보ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStrip2 = new System.Windows.Forms.ToolStrip();
            this.toolStripButton_ExpandAll = new System.Windows.Forms.ToolStripButton();
            this.toolStripButton_CollapseAll = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripButton_Log = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripLabel_User = new System.Windows.Forms.ToolStripLabel();
            this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            this.panel1 = new System.Windows.Forms.Panel();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.toolStripStatusLabel_DB_Status = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStripStatusLabel_PI_Status = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStripStatusLabel_PIPGM_Status = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStripStatusLabel_DBPGM_P_Status = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStripStatusLabel_DBPGM_P_ErrorFile = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStripStatusLabel_DBPGM_S_Status = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStripStatusLabel_DBPGM_S_ErrorFile = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStripStatusLabel4 = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStripStatusLabel_ServerName = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStripStatusLabel_MainDBSourceName = new System.Windows.Forms.ToolStripStatusLabel();
            this.imageList1 = new System.Windows.Forms.ImageList(this.components);
            this.imageList_EquipState = new System.Windows.Forms.ImageList(this.components);
            this.configurationManagerAppSettingToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.menuStrip1.SuspendLayout();
            this.toolStrip2.SuspendLayout();
            this.panel1.SuspendLayout();
            this.statusStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
            this.splitContainer1.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.pPanelView);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.pDispView);
            this.splitContainer1.Size = new System.Drawing.Size(1144, 539);
            this.splitContainer1.SplitterDistance = 250;
            this.splitContainer1.TabIndex = 2;
            // 
            // pPanelView
            // 
            this.pPanelView.BackColor = System.Drawing.SystemColors.ControlLightLight;
            this.pPanelView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pPanelView.Location = new System.Drawing.Point(0, 0);
            this.pPanelView.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.pPanelView.Name = "pPanelView";
            this.pPanelView.Size = new System.Drawing.Size(250, 539);
            this.pPanelView.TabIndex = 1;
            // 
            // pDispView
            // 
            this.pDispView.BackColor = System.Drawing.SystemColors.ControlLightLight;
            this.pDispView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pDispView.Location = new System.Drawing.Point(0, 0);
            this.pDispView.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.pDispView.Name = "pDispView";
            this.pDispView.Size = new System.Drawing.Size(890, 539);
            this.pDispView.TabIndex = 0;
            // 
            // menuStrip1
            // 
            this.menuStrip1.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.파일ToolStripMenuItem,
            this.configCToolStripMenuItem,
            this.userToolStripMenuItem,
            this.viewToolStripMenuItem,
            this.toolsToolStripMenuItem,
            this.도움말ToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Padding = new System.Windows.Forms.Padding(6, 3, 0, 3);
            this.menuStrip1.Size = new System.Drawing.Size(1144, 25);
            this.menuStrip1.TabIndex = 4;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // 파일ToolStripMenuItem
            // 
            this.파일ToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripSeparator4,
            this.종료ToolStripMenuItem});
            this.파일ToolStripMenuItem.Name = "파일ToolStripMenuItem";
            this.파일ToolStripMenuItem.ShortcutKeyDisplayString = "";
            this.파일ToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Alt | System.Windows.Forms.Keys.F)));
            this.파일ToolStripMenuItem.ShowShortcutKeys = false;
            this.파일ToolStripMenuItem.Size = new System.Drawing.Size(51, 19);
            this.파일ToolStripMenuItem.Text = "File(F)";
            // 
            // toolStripSeparator4
            // 
            this.toolStripSeparator4.Name = "toolStripSeparator4";
            this.toolStripSeparator4.Size = new System.Drawing.Size(177, 6);
            // 
            // 종료ToolStripMenuItem
            // 
            this.종료ToolStripMenuItem.Name = "종료ToolStripMenuItem";
            this.종료ToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Alt | System.Windows.Forms.Keys.X)));
            this.종료ToolStripMenuItem.ShowShortcutKeys = false;
            this.종료ToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.종료ToolStripMenuItem.Text = "Exit(X)";
            this.종료ToolStripMenuItem.Click += new System.EventHandler(this.종료ToolStripMenuItem_Click);
            // 
            // configCToolStripMenuItem
            // 
            this.configCToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tagGroupToolStripMenuItem,
            this.commonCodeToolStripMenuItem,
            this.configurationManagerAppSettingToolStripMenuItem});
            this.configCToolStripMenuItem.Name = "configCToolStripMenuItem";
            this.configCToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Alt | System.Windows.Forms.Keys.C)));
            this.configCToolStripMenuItem.Size = new System.Drawing.Size(71, 19);
            this.configCToolStripMenuItem.Text = "Config(C)";
            // 
            // tagGroupToolStripMenuItem
            // 
            this.tagGroupToolStripMenuItem.Name = "tagGroupToolStripMenuItem";
            this.tagGroupToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Alt | System.Windows.Forms.Keys.T)));
            this.tagGroupToolStripMenuItem.ShowShortcutKeys = false;
            this.tagGroupToolStripMenuItem.Size = new System.Drawing.Size(255, 22);
            this.tagGroupToolStripMenuItem.Text = "Tag Group (T)";
            this.tagGroupToolStripMenuItem.Click += new System.EventHandler(this.tagGroupToolStripMenuItem_Click);
            // 
            // commonCodeToolStripMenuItem
            // 
            this.commonCodeToolStripMenuItem.Name = "commonCodeToolStripMenuItem";
            this.commonCodeToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Alt | System.Windows.Forms.Keys.O)));
            this.commonCodeToolStripMenuItem.ShowShortcutKeys = false;
            this.commonCodeToolStripMenuItem.Size = new System.Drawing.Size(255, 22);
            this.commonCodeToolStripMenuItem.Text = "Common Code (O)";
            this.commonCodeToolStripMenuItem.Click += new System.EventHandler(this.commonCodeToolStripMenuItem_Click);
            // 
            // userToolStripMenuItem
            // 
            this.userToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.userInfoToolStripMenuItem});
            this.userToolStripMenuItem.Name = "userToolStripMenuItem";
            this.userToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Alt | System.Windows.Forms.Keys.U)));
            this.userToolStripMenuItem.Size = new System.Drawing.Size(58, 19);
            this.userToolStripMenuItem.Text = "User(U)";
            // 
            // userInfoToolStripMenuItem
            // 
            this.userInfoToolStripMenuItem.Name = "userInfoToolStripMenuItem";
            this.userInfoToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.userInfoToolStripMenuItem.Text = "User info";
            this.userInfoToolStripMenuItem.Click += new System.EventHandler(this.userInfoToolStripMenuItem_Click);
            // 
            // viewToolStripMenuItem
            // 
            this.viewToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.showAllEquipmtToolStripMenuItem,
            this.toolStripSeparator5});
            this.viewToolStripMenuItem.Name = "viewToolStripMenuItem";
            this.viewToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Alt | System.Windows.Forms.Keys.V)));
            this.viewToolStripMenuItem.Size = new System.Drawing.Size(61, 19);
            this.viewToolStripMenuItem.Text = "View(V)";
            // 
            // showAllEquipmtToolStripMenuItem
            // 
            this.showAllEquipmtToolStripMenuItem.CheckOnClick = true;
            this.showAllEquipmtToolStripMenuItem.Name = "showAllEquipmtToolStripMenuItem";
            this.showAllEquipmtToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.showAllEquipmtToolStripMenuItem.Text = "Show All Equipmt";
            this.showAllEquipmtToolStripMenuItem.Click += new System.EventHandler(this.showAllEquipmtToolStripMenuItem_Click);
            // 
            // toolStripSeparator5
            // 
            this.toolStripSeparator5.Name = "toolStripSeparator5";
            this.toolStripSeparator5.Size = new System.Drawing.Size(177, 6);
            // 
            // toolsToolStripMenuItem
            // 
            this.toolsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.dateTimeParsingToolStripMenuItem});
            this.toolsToolStripMenuItem.Name = "toolsToolStripMenuItem";
            this.toolsToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Alt | System.Windows.Forms.Keys.S)));
            this.toolsToolStripMenuItem.Size = new System.Drawing.Size(66, 19);
            this.toolsToolStripMenuItem.Text = "Tools (S)";
            // 
            // dateTimeParsingToolStripMenuItem
            // 
            this.dateTimeParsingToolStripMenuItem.Name = "dateTimeParsingToolStripMenuItem";
            this.dateTimeParsingToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.dateTimeParsingToolStripMenuItem.Text = "DateTime Parsing";
            this.dateTimeParsingToolStripMenuItem.Click += new System.EventHandler(this.dateTimeParsingToolStripMenuItem_Click);
            // 
            // 도움말ToolStripMenuItem
            // 
            this.도움말ToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.sEIMM정보ToolStripMenuItem});
            this.도움말ToolStripMenuItem.Name = "도움말ToolStripMenuItem";
            this.도움말ToolStripMenuItem.ShortcutKeyDisplayString = "";
            this.도움말ToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Alt | System.Windows.Forms.Keys.H)));
            this.도움말ToolStripMenuItem.ShowShortcutKeys = false;
            this.도움말ToolStripMenuItem.Size = new System.Drawing.Size(61, 19);
            this.도움말ToolStripMenuItem.Text = "Help(H)";
            // 
            // sEIMM정보ToolStripMenuItem
            // 
            this.sEIMM정보ToolStripMenuItem.Name = "sEIMM정보ToolStripMenuItem";
            this.sEIMM정보ToolStripMenuItem.ShowShortcutKeys = false;
            this.sEIMM정보ToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.sEIMM정보ToolStripMenuItem.Text = "About DataSpider";
            this.sEIMM정보ToolStripMenuItem.Click += new System.EventHandler(this.SEIMM정보ToolStripMenuItem_Click);
            // 
            // toolStrip2
            // 
            this.toolStrip2.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.toolStrip2.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripButton_ExpandAll,
            this.toolStripButton_CollapseAll,
            this.toolStripSeparator1,
            this.toolStripButton_Log,
            this.toolStripSeparator2,
            this.toolStripLabel_User,
            this.toolStripSeparator3});
            this.toolStrip2.Location = new System.Drawing.Point(0, 25);
            this.toolStrip2.Name = "toolStrip2";
            this.toolStrip2.Size = new System.Drawing.Size(1144, 27);
            this.toolStrip2.TabIndex = 6;
            this.toolStrip2.Text = "toolStrip2";
            // 
            // toolStripButton_ExpandAll
            // 
            this.toolStripButton_ExpandAll.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButton_ExpandAll.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton_ExpandAll.Image")));
            this.toolStripButton_ExpandAll.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton_ExpandAll.Name = "toolStripButton_ExpandAll";
            this.toolStripButton_ExpandAll.Size = new System.Drawing.Size(24, 24);
            this.toolStripButton_ExpandAll.Text = "Expand All";
            this.toolStripButton_ExpandAll.ToolTipText = "Expand All";
            this.toolStripButton_ExpandAll.Click += new System.EventHandler(this.toolStripButton_ExpandAll_Click);
            // 
            // toolStripButton_CollapseAll
            // 
            this.toolStripButton_CollapseAll.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButton_CollapseAll.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton_CollapseAll.Image")));
            this.toolStripButton_CollapseAll.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton_CollapseAll.Name = "toolStripButton_CollapseAll";
            this.toolStripButton_CollapseAll.Size = new System.Drawing.Size(24, 24);
            this.toolStripButton_CollapseAll.Text = "Collapse All";
            this.toolStripButton_CollapseAll.ToolTipText = "Collapse All";
            this.toolStripButton_CollapseAll.Click += new System.EventHandler(this.toolStripButton_CollapseAll_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(6, 27);
            // 
            // toolStripButton_Log
            // 
            this.toolStripButton_Log.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripButton_Log.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton_Log.Image")));
            this.toolStripButton_Log.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton_Log.Name = "toolStripButton_Log";
            this.toolStripButton_Log.Size = new System.Drawing.Size(45, 24);
            this.toolStripButton_Log.Text = "Log In";
            this.toolStripButton_Log.Click += new System.EventHandler(this.toolStripButton_Log_Click);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(6, 27);
            // 
            // toolStripLabel_User
            // 
            this.toolStripLabel_User.Name = "toolStripLabel_User";
            this.toolStripLabel_User.Size = new System.Drawing.Size(30, 24);
            this.toolStripLabel_User.Text = "User";
            this.toolStripLabel_User.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.toolStripLabel_User.Click += new System.EventHandler(this.toolStripLabel_User_Click);
            // 
            // toolStripSeparator3
            // 
            this.toolStripSeparator3.Name = "toolStripSeparator3";
            this.toolStripSeparator3.Size = new System.Drawing.Size(6, 27);
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.splitContainer1);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel1.Location = new System.Drawing.Point(0, 52);
            this.panel1.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(1144, 539);
            this.panel1.TabIndex = 7;
            // 
            // statusStrip1
            // 
            this.statusStrip1.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripStatusLabel_DB_Status,
            this.toolStripStatusLabel_PI_Status,
            this.toolStripStatusLabel_PIPGM_Status,
            this.toolStripStatusLabel_DBPGM_P_Status,
            this.toolStripStatusLabel_DBPGM_P_ErrorFile,
            this.toolStripStatusLabel_DBPGM_S_Status,
            this.toolStripStatusLabel_DBPGM_S_ErrorFile,
            this.toolStripStatusLabel4,
            this.toolStripStatusLabel_ServerName,
            this.toolStripStatusLabel_MainDBSourceName});
            this.statusStrip1.Location = new System.Drawing.Point(0, 591);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.ShowItemToolTips = true;
            this.statusStrip1.Size = new System.Drawing.Size(1144, 31);
            this.statusStrip1.TabIndex = 8;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // toolStripStatusLabel_DB_Status
            // 
            this.toolStripStatusLabel_DB_Status.AutoSize = false;
            this.toolStripStatusLabel_DB_Status.AutoToolTip = true;
            this.toolStripStatusLabel_DB_Status.Image = global::DataSpider.Properties.Resources.UnKnown;
            this.toolStripStatusLabel_DB_Status.Name = "toolStripStatusLabel_DB_Status";
            this.toolStripStatusLabel_DB_Status.Size = new System.Drawing.Size(100, 26);
            this.toolStripStatusLabel_DB_Status.Text = "DB Status";
            // 
            // toolStripStatusLabel_PI_Status
            // 
            this.toolStripStatusLabel_PI_Status.AutoSize = false;
            this.toolStripStatusLabel_PI_Status.AutoToolTip = true;
            this.toolStripStatusLabel_PI_Status.Image = global::DataSpider.Properties.Resources.UnKnown;
            this.toolStripStatusLabel_PI_Status.Name = "toolStripStatusLabel_PI_Status";
            this.toolStripStatusLabel_PI_Status.Size = new System.Drawing.Size(100, 26);
            this.toolStripStatusLabel_PI_Status.Text = "PI Status";
            // 
            // toolStripStatusLabel_PIPGM_Status
            // 
            this.toolStripStatusLabel_PIPGM_Status.AutoSize = false;
            this.toolStripStatusLabel_PIPGM_Status.AutoToolTip = true;
            this.toolStripStatusLabel_PIPGM_Status.Image = global::DataSpider.Properties.Resources.UnKnown;
            this.toolStripStatusLabel_PIPGM_Status.Name = "toolStripStatusLabel_PIPGM_Status";
            this.toolStripStatusLabel_PIPGM_Status.Size = new System.Drawing.Size(110, 26);
            this.toolStripStatusLabel_PIPGM_Status.Text = "PI PGM Status";
            // 
            // toolStripStatusLabel_DBPGM_P_Status
            // 
            this.toolStripStatusLabel_DBPGM_P_Status.AutoSize = false;
            this.toolStripStatusLabel_DBPGM_P_Status.AutoToolTip = true;
            this.toolStripStatusLabel_DBPGM_P_Status.Image = global::DataSpider.Properties.Resources.UnKnown;
            this.toolStripStatusLabel_DBPGM_P_Status.Name = "toolStripStatusLabel_DBPGM_P_Status";
            this.toolStripStatusLabel_DBPGM_P_Status.Size = new System.Drawing.Size(130, 26);
            this.toolStripStatusLabel_DBPGM_P_Status.Text = "DB PGM[P] Status";
            // 
            // toolStripStatusLabel_DBPGM_P_ErrorFile
            // 
            this.toolStripStatusLabel_DBPGM_P_ErrorFile.AutoSize = false;
            this.toolStripStatusLabel_DBPGM_P_ErrorFile.AutoToolTip = true;
            this.toolStripStatusLabel_DBPGM_P_ErrorFile.ForeColor = System.Drawing.Color.Red;
            this.toolStripStatusLabel_DBPGM_P_ErrorFile.Name = "toolStripStatusLabel_DBPGM_P_ErrorFile";
            this.toolStripStatusLabel_DBPGM_P_ErrorFile.Size = new System.Drawing.Size(150, 26);
            this.toolStripStatusLabel_DBPGM_P_ErrorFile.Text = "No ErrorFile";
            this.toolStripStatusLabel_DBPGM_P_ErrorFile.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // toolStripStatusLabel_DBPGM_S_Status
            // 
            this.toolStripStatusLabel_DBPGM_S_Status.AutoSize = false;
            this.toolStripStatusLabel_DBPGM_S_Status.AutoToolTip = true;
            this.toolStripStatusLabel_DBPGM_S_Status.Image = global::DataSpider.Properties.Resources.UnKnown;
            this.toolStripStatusLabel_DBPGM_S_Status.Name = "toolStripStatusLabel_DBPGM_S_Status";
            this.toolStripStatusLabel_DBPGM_S_Status.Size = new System.Drawing.Size(130, 26);
            this.toolStripStatusLabel_DBPGM_S_Status.Text = "DB PGM[S] Status";
            // 
            // toolStripStatusLabel_DBPGM_S_ErrorFile
            // 
            this.toolStripStatusLabel_DBPGM_S_ErrorFile.AutoSize = false;
            this.toolStripStatusLabel_DBPGM_S_ErrorFile.AutoToolTip = true;
            this.toolStripStatusLabel_DBPGM_S_ErrorFile.ForeColor = System.Drawing.Color.Red;
            this.toolStripStatusLabel_DBPGM_S_ErrorFile.Name = "toolStripStatusLabel_DBPGM_S_ErrorFile";
            this.toolStripStatusLabel_DBPGM_S_ErrorFile.Size = new System.Drawing.Size(150, 26);
            this.toolStripStatusLabel_DBPGM_S_ErrorFile.Text = "No ErrorFile";
            this.toolStripStatusLabel_DBPGM_S_ErrorFile.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // toolStripStatusLabel4
            // 
            this.toolStripStatusLabel4.Name = "toolStripStatusLabel4";
            this.toolStripStatusLabel4.Size = new System.Drawing.Size(176, 26);
            this.toolStripStatusLabel4.Spring = true;
            this.toolStripStatusLabel4.Text = "  ";
            this.toolStripStatusLabel4.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // toolStripStatusLabel_ServerName
            // 
            this.toolStripStatusLabel_ServerName.Name = "toolStripStatusLabel_ServerName";
            this.toolStripStatusLabel_ServerName.Size = new System.Drawing.Size(0, 26);
            // 
            // toolStripStatusLabel_MainDBSourceName
            // 
            this.toolStripStatusLabel_MainDBSourceName.DoubleClickEnabled = true;
            this.toolStripStatusLabel_MainDBSourceName.Name = "toolStripStatusLabel_MainDBSourceName";
            this.toolStripStatusLabel_MainDBSourceName.Size = new System.Drawing.Size(83, 26);
            this.toolStripStatusLabel_MainDBSourceName.Text = "                   ";
            // 
            // imageList1
            // 
            this.imageList1.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList1.ImageStream")));
            this.imageList1.TransparentColor = System.Drawing.Color.Transparent;
            this.imageList1.Images.SetKeyName(0, "UnKnown.png");
            this.imageList1.Images.SetKeyName(1, "OnLine.png");
            this.imageList1.Images.SetKeyName(2, "OffLine.png");
            // 
            // imageList_EquipState
            // 
            this.imageList_EquipState.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList_EquipState.ImageStream")));
            this.imageList_EquipState.TransparentColor = System.Drawing.Color.Transparent;
            this.imageList_EquipState.Images.SetKeyName(0, "Normal");
            this.imageList_EquipState.Images.SetKeyName(1, "Stop");
            this.imageList_EquipState.Images.SetKeyName(2, "Disconnected");
            this.imageList_EquipState.Images.SetKeyName(3, "NoData");
            this.imageList_EquipState.Images.SetKeyName(4, "InvalidData");
            this.imageList_EquipState.Images.SetKeyName(5, "InternalError");
            this.imageList_EquipState.Images.SetKeyName(6, "Unknown");
            this.imageList_EquipState.Images.SetKeyName(7, "NetworkError");
            // 
            // configurationManagerAppSettingToolStripMenuItem
            // 
            this.configurationManagerAppSettingToolStripMenuItem.Name = "configurationManagerAppSettingToolStripMenuItem";
            this.configurationManagerAppSettingToolStripMenuItem.Size = new System.Drawing.Size(255, 22);
            this.configurationManagerAppSettingToolStripMenuItem.Text = "ConfigurationManagerAppSetting";
            this.configurationManagerAppSettingToolStripMenuItem.Click += new System.EventHandler(this.configurationManagerAppSettingToolStripMenuItem_Click);
            // 
            // MonitorForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 17F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1144, 622);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.toolStrip2);
            this.Controls.Add(this.menuStrip1);
            this.Controls.Add(this.statusStrip1);
            this.Font = new System.Drawing.Font("맑은 고딕", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "MonitorForm";
            this.Text = "DataSpider";
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MonitorForm_FormClosing);
            this.Load += new System.EventHandler(this.Form1_Load);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.toolStrip2.ResumeLayout(false);
            this.toolStrip2.PerformLayout();
            this.panel1.ResumeLayout(false);
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private LibraryWH.FormCtrl.TabFromCtrl pDispView;
        private LibraryWH.FormCtrl.PanelForm pPanelView;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.ToolStripMenuItem 파일ToolStripMenuItem;
        private System.Windows.Forms.ToolStrip toolStrip2;
        private System.Windows.Forms.ToolStripButton toolStripButton_ExpandAll;
        private System.Windows.Forms.ToolStripButton toolStripButton_CollapseAll;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel_PI_Status;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel4;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel_MainDBSourceName;
        private System.Windows.Forms.ToolStripMenuItem 도움말ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem sEIMM정보ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem 종료ToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripButton toolStripButton_Log;
        private System.Windows.Forms.ToolStripLabel toolStripLabel_User;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
        private System.Windows.Forms.ToolStripMenuItem userToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem userInfoToolStripMenuItem;
        private System.Windows.Forms.ImageList imageList1;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel_DBPGM_S_Status;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel_PIPGM_Status;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator4;
        private System.Windows.Forms.ToolStripMenuItem configCToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem tagGroupToolStripMenuItem;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel_ServerName;
        private System.Windows.Forms.ImageList imageList_EquipState;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel_DBPGM_P_Status;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel_DBPGM_P_ErrorFile;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel_DBPGM_S_ErrorFile;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel_DB_Status;
        private System.Windows.Forms.ToolStripMenuItem commonCodeToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem viewToolStripMenuItem;
        public System.Windows.Forms.MenuStrip menuStrip1;
        public System.Windows.Forms.ToolStripMenuItem showAllEquipmtToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem toolsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem dateTimeParsingToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator5;
        private System.Windows.Forms.ToolStripMenuItem configurationManagerAppSettingToolStripMenuItem;
    }
}

