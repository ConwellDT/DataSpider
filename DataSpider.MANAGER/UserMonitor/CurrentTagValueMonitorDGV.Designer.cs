
namespace DataSpider.UserMonitor
{
    partial class CurrentTagValueMonitorDGV
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(CurrentTagValueMonitorDGV));
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle3 = new System.Windows.Forms.DataGridViewCellStyle();
            this.imageList2 = new System.Windows.Forms.ImageList(this.components);
            this.imageList1 = new System.Windows.Forms.ImageList(this.components);
            this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.valueHistoryToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.editToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.addToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.deleteToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripMenuItemLog = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItemData = new System.Windows.Forms.ToolStripMenuItem();
            this.panel8 = new System.Windows.Forms.Panel();
            this.button_Find = new System.Windows.Forms.Button();
            this.panel7 = new System.Windows.Forms.Panel();
            this.button_SetInterval = new System.Windows.Forms.Button();
            this.panel4 = new System.Windows.Forms.Panel();
            this.label2 = new System.Windows.Forms.Label();
            this.panel3 = new System.Windows.Forms.Panel();
            this.button_Refresh = new System.Windows.Forms.Button();
            this.panel2 = new System.Windows.Forms.Panel();
            this.checkBox_AutoRefresh = new System.Windows.Forms.CheckBox();
            this.tableLayoutTagValueMonitorMenu = new System.Windows.Forms.TableLayoutPanel();
            this.buttonFilter = new System.Windows.Forms.Button();
            this.panel1 = new System.Windows.Forms.Panel();
            this.dataGridView_Main = new System.Windows.Forms.DataGridView();
            this.comboBoxTagGroupSel = new System.Windows.Forms.ComboBox();
            this.groupBoxCurOrHistory = new System.Windows.Forms.GroupBox();
            this.radioButtonHistoryTag = new System.Windows.Forms.RadioButton();
            this.radioButtonCurTag = new System.Windows.Forms.RadioButton();
            this.contextMenuStrip1.SuspendLayout();
            this.panel8.SuspendLayout();
            this.panel7.SuspendLayout();
            this.panel4.SuspendLayout();
            this.panel3.SuspendLayout();
            this.panel2.SuspendLayout();
            this.tableLayoutTagValueMonitorMenu.SuspendLayout();
            this.panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView_Main)).BeginInit();
            this.groupBoxCurOrHistory.SuspendLayout();
            this.SuspendLayout();
            // 
            // imageList2
            // 
            this.imageList2.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList2.ImageStream")));
            this.imageList2.TransparentColor = System.Drawing.Color.Transparent;
            this.imageList2.Images.SetKeyName(0, "analyzer.png");
            this.imageList2.Images.SetKeyName(1, "applications-science-2.png");
            this.imageList2.Images.SetKeyName(2, "bioinformatics.png");
            this.imageList2.Images.SetKeyName(3, "cnc_machine.ico");
            this.imageList2.Images.SetKeyName(4, "cnc_machine.png");
            this.imageList2.Images.SetKeyName(5, "free-icon-ph-meter-1327507.png");
            this.imageList2.Images.SetKeyName(6, "free-icon-ph-meter-Stop.png");
            this.imageList2.Images.SetKeyName(7, "laboratory - 복사본.png");
            this.imageList2.Images.SetKeyName(8, "laboratory.png");
            this.imageList2.Images.SetKeyName(9, "weight-scale -Start.png");
            this.imageList2.Images.SetKeyName(10, "weight-scale.png");
            this.imageList2.Images.SetKeyName(11, "weight-scale_1.png");
            this.imageList2.Images.SetKeyName(12, "weight-scale_stop.png");
            // 
            // imageList1
            // 
            this.imageList1.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList1.ImageStream")));
            this.imageList1.TransparentColor = System.Drawing.Color.Transparent;
            this.imageList1.Images.SetKeyName(0, "analyzer.png");
            this.imageList1.Images.SetKeyName(1, "applications-science-2.png");
            this.imageList1.Images.SetKeyName(2, "bioinformatics.png");
            this.imageList1.Images.SetKeyName(3, "cnc_machine.ico");
            this.imageList1.Images.SetKeyName(4, "cnc_machine.png");
            this.imageList1.Images.SetKeyName(5, "free-icon-ph-meter-1327507.png");
            this.imageList1.Images.SetKeyName(6, "free-icon-ph-meter-Stop.png");
            this.imageList1.Images.SetKeyName(7, "laboratory - 복사본.png");
            this.imageList1.Images.SetKeyName(8, "laboratory.png");
            this.imageList1.Images.SetKeyName(9, "weight-scale -Start.png");
            this.imageList1.Images.SetKeyName(10, "weight-scale.png");
            this.imageList1.Images.SetKeyName(11, "weight-scale_1.png");
            this.imageList1.Images.SetKeyName(12, "weight-scale_stop.png");
            // 
            // contextMenuStrip1
            // 
            this.contextMenuStrip1.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.valueHistoryToolStripMenuItem,
            this.editToolStripMenuItem,
            this.addToolStripMenuItem,
            this.deleteToolStripMenuItem,
            this.toolStripSeparator1,
            this.toolStripMenuItemLog,
            this.toolStripMenuItemData});
            this.contextMenuStrip1.Name = "contextMenuStrip1";
            this.contextMenuStrip1.Size = new System.Drawing.Size(147, 142);
            // 
            // valueHistoryToolStripMenuItem
            // 
            this.valueHistoryToolStripMenuItem.Name = "valueHistoryToolStripMenuItem";
            this.valueHistoryToolStripMenuItem.Size = new System.Drawing.Size(146, 22);
            this.valueHistoryToolStripMenuItem.Text = "Value History";
            this.valueHistoryToolStripMenuItem.Click += new System.EventHandler(this.ValueHistoryToolStripMenuItem_Click);
            // 
            // editToolStripMenuItem
            // 
            this.editToolStripMenuItem.Name = "editToolStripMenuItem";
            this.editToolStripMenuItem.Size = new System.Drawing.Size(146, 22);
            this.editToolStripMenuItem.Text = "Edit";
            this.editToolStripMenuItem.Click += new System.EventHandler(this.EditToolStripMenuItem_Click);
            // 
            // addToolStripMenuItem
            // 
            this.addToolStripMenuItem.Name = "addToolStripMenuItem";
            this.addToolStripMenuItem.Size = new System.Drawing.Size(146, 22);
            this.addToolStripMenuItem.Text = "Add";
            this.addToolStripMenuItem.Click += new System.EventHandler(this.AddToolStripMenuItem_Click);
            // 
            // deleteToolStripMenuItem
            // 
            this.deleteToolStripMenuItem.Name = "deleteToolStripMenuItem";
            this.deleteToolStripMenuItem.Size = new System.Drawing.Size(146, 22);
            this.deleteToolStripMenuItem.Text = "Delete";
            this.deleteToolStripMenuItem.Click += new System.EventHandler(this.DeleteToolStripMenuItem_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(143, 6);
            // 
            // toolStripMenuItemLog
            // 
            this.toolStripMenuItemLog.Name = "toolStripMenuItemLog";
            this.toolStripMenuItemLog.Size = new System.Drawing.Size(146, 22);
            this.toolStripMenuItemLog.Text = "Log";
            this.toolStripMenuItemLog.Click += new System.EventHandler(this.toolStripMenuItemLog_Click);
            // 
            // toolStripMenuItemData
            // 
            this.toolStripMenuItemData.Name = "toolStripMenuItemData";
            this.toolStripMenuItemData.Size = new System.Drawing.Size(146, 22);
            this.toolStripMenuItemData.Text = "Data";
            this.toolStripMenuItemData.Click += new System.EventHandler(this.toolStripMenuItemData_Click);
            // 
            // panel8
            // 
            this.panel8.Controls.Add(this.button_Find);
            this.panel8.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel8.Location = new System.Drawing.Point(617, 3);
            this.panel8.Name = "panel8";
            this.panel8.Size = new System.Drawing.Size(94, 34);
            this.panel8.TabIndex = 7;
            // 
            // button_Find
            // 
            this.button_Find.Dock = System.Windows.Forms.DockStyle.Fill;
            this.button_Find.Location = new System.Drawing.Point(0, 0);
            this.button_Find.Name = "button_Find";
            this.button_Find.Size = new System.Drawing.Size(94, 34);
            this.button_Find.TabIndex = 2;
            this.button_Find.Text = "Find (Ctrl+F)";
            this.button_Find.UseVisualStyleBackColor = true;
            this.button_Find.Click += new System.EventHandler(this.button_Find_Click);
            // 
            // panel7
            // 
            this.panel7.Controls.Add(this.button_SetInterval);
            this.panel7.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel7.Location = new System.Drawing.Point(847, 3);
            this.panel7.Name = "panel7";
            this.panel7.Size = new System.Drawing.Size(94, 34);
            this.panel7.TabIndex = 6;
            // 
            // button_SetInterval
            // 
            this.button_SetInterval.Dock = System.Windows.Forms.DockStyle.Fill;
            this.button_SetInterval.Location = new System.Drawing.Point(0, 0);
            this.button_SetInterval.Name = "button_SetInterval";
            this.button_SetInterval.Size = new System.Drawing.Size(94, 34);
            this.button_SetInterval.TabIndex = 1;
            this.button_SetInterval.Text = "Set Interval";
            this.button_SetInterval.UseVisualStyleBackColor = true;
            this.button_SetInterval.Click += new System.EventHandler(this.button_SetInterval_Click);
            // 
            // panel4
            // 
            this.panel4.Controls.Add(this.label2);
            this.panel4.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel4.Location = new System.Drawing.Point(3, 3);
            this.panel4.Name = "panel4";
            this.panel4.Size = new System.Drawing.Size(93, 34);
            this.panel4.TabIndex = 3;
            // 
            // label2
            // 
            this.label2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.label2.Font = new System.Drawing.Font("맑은 고딕", 16F);
            this.label2.Location = new System.Drawing.Point(0, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(93, 34);
            this.label2.TabIndex = 3;
            this.label2.Text = "TAG Value Monitor";
            this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // panel3
            // 
            this.panel3.Controls.Add(this.button_Refresh);
            this.panel3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel3.Location = new System.Drawing.Point(1097, 3);
            this.panel3.Name = "panel3";
            this.panel3.Size = new System.Drawing.Size(64, 34);
            this.panel3.TabIndex = 2;
            // 
            // button_Refresh
            // 
            this.button_Refresh.Dock = System.Windows.Forms.DockStyle.Fill;
            this.button_Refresh.Location = new System.Drawing.Point(0, 0);
            this.button_Refresh.Name = "button_Refresh";
            this.button_Refresh.Size = new System.Drawing.Size(64, 34);
            this.button_Refresh.TabIndex = 0;
            this.button_Refresh.Text = "Refresh";
            this.button_Refresh.UseVisualStyleBackColor = true;
            this.button_Refresh.Click += new System.EventHandler(this.button_Refresh_Click);
            // 
            // panel2
            // 
            this.panel2.Controls.Add(this.checkBox_AutoRefresh);
            this.panel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel2.Location = new System.Drawing.Point(947, 3);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(134, 34);
            this.panel2.TabIndex = 1;
            // 
            // checkBox_AutoRefresh
            // 
            this.checkBox_AutoRefresh.Checked = true;
            this.checkBox_AutoRefresh.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBox_AutoRefresh.Dock = System.Windows.Forms.DockStyle.Fill;
            this.checkBox_AutoRefresh.Location = new System.Drawing.Point(0, 0);
            this.checkBox_AutoRefresh.Name = "checkBox_AutoRefresh";
            this.checkBox_AutoRefresh.Size = new System.Drawing.Size(134, 34);
            this.checkBox_AutoRefresh.TabIndex = 0;
            this.checkBox_AutoRefresh.Text = "Auto Refresh (0s)";
            this.checkBox_AutoRefresh.UseVisualStyleBackColor = true;
            // 
            // tableLayoutTagValueMonitorMenu
            // 
            this.tableLayoutTagValueMonitorMenu.ColumnCount = 14;
            this.tableLayoutTagValueMonitorMenu.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutTagValueMonitorMenu.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 10F));
            this.tableLayoutTagValueMonitorMenu.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 135F));
            this.tableLayoutTagValueMonitorMenu.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 10F));
            this.tableLayoutTagValueMonitorMenu.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 350F));
            this.tableLayoutTagValueMonitorMenu.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 10F));
            this.tableLayoutTagValueMonitorMenu.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 100F));
            this.tableLayoutTagValueMonitorMenu.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 10F));
            this.tableLayoutTagValueMonitorMenu.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 60F));
            this.tableLayoutTagValueMonitorMenu.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 60F));
            this.tableLayoutTagValueMonitorMenu.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 100F));
            this.tableLayoutTagValueMonitorMenu.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 140F));
            this.tableLayoutTagValueMonitorMenu.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 10F));
            this.tableLayoutTagValueMonitorMenu.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 70F));
            this.tableLayoutTagValueMonitorMenu.Controls.Add(this.buttonFilter, 8, 0);
            this.tableLayoutTagValueMonitorMenu.Controls.Add(this.panel1, 0, 1);
            this.tableLayoutTagValueMonitorMenu.Controls.Add(this.panel2, 11, 0);
            this.tableLayoutTagValueMonitorMenu.Controls.Add(this.panel3, 13, 0);
            this.tableLayoutTagValueMonitorMenu.Controls.Add(this.panel4, 0, 0);
            this.tableLayoutTagValueMonitorMenu.Controls.Add(this.panel7, 10, 0);
            this.tableLayoutTagValueMonitorMenu.Controls.Add(this.panel8, 6, 0);
            this.tableLayoutTagValueMonitorMenu.Controls.Add(this.comboBoxTagGroupSel, 4, 0);
            this.tableLayoutTagValueMonitorMenu.Controls.Add(this.groupBoxCurOrHistory, 2, 0);
            this.tableLayoutTagValueMonitorMenu.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutTagValueMonitorMenu.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutTagValueMonitorMenu.Name = "tableLayoutTagValueMonitorMenu";
            this.tableLayoutTagValueMonitorMenu.RowCount = 2;
            this.tableLayoutTagValueMonitorMenu.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 40F));
            this.tableLayoutTagValueMonitorMenu.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutTagValueMonitorMenu.Size = new System.Drawing.Size(1164, 834);
            this.tableLayoutTagValueMonitorMenu.TabIndex = 1;
            // 
            // buttonFilter
            // 
            this.buttonFilter.Dock = System.Windows.Forms.DockStyle.Fill;
            this.buttonFilter.Location = new System.Drawing.Point(727, 3);
            this.buttonFilter.Name = "buttonFilter";
            this.buttonFilter.Size = new System.Drawing.Size(54, 34);
            this.buttonFilter.TabIndex = 3;
            this.buttonFilter.Text = "Filter";
            this.buttonFilter.UseVisualStyleBackColor = true;
            this.buttonFilter.Click += new System.EventHandler(this.buttonFilter_Click);
            // 
            // panel1
            // 
            this.tableLayoutTagValueMonitorMenu.SetColumnSpan(this.panel1, 14);
            this.panel1.Controls.Add(this.dataGridView_Main);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel1.Location = new System.Drawing.Point(3, 43);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(1158, 788);
            this.panel1.TabIndex = 0;
            // 
            // dataGridView_Main
            // 
            this.dataGridView_Main.AllowUserToAddRows = false;
            this.dataGridView_Main.AllowUserToDeleteRows = false;
            dataGridViewCellStyle3.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(221)))), ((int)(((byte)(235)))), ((int)(((byte)(247)))));
            this.dataGridView_Main.AlternatingRowsDefaultCellStyle = dataGridViewCellStyle3;
            this.dataGridView_Main.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.DisplayedCells;
            this.dataGridView_Main.AutoSizeRowsMode = System.Windows.Forms.DataGridViewAutoSizeRowsMode.DisplayedCells;
            this.dataGridView_Main.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridView_Main.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dataGridView_Main.Location = new System.Drawing.Point(0, 0);
            this.dataGridView_Main.MultiSelect = false;
            this.dataGridView_Main.Name = "dataGridView_Main";
            this.dataGridView_Main.ReadOnly = true;
            this.dataGridView_Main.RowHeadersVisible = false;
            this.dataGridView_Main.RowHeadersWidth = 51;
            this.dataGridView_Main.RowTemplate.Height = 23;
            this.dataGridView_Main.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dataGridView_Main.Size = new System.Drawing.Size(1158, 788);
            this.dataGridView_Main.TabIndex = 1;
            this.dataGridView_Main.CellMouseClick += new System.Windows.Forms.DataGridViewCellMouseEventHandler(this.dataGridView_Main_CellMouseClick);
            // 
            // comboBoxTagGroupSel
            // 
            this.comboBoxTagGroupSel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.comboBoxTagGroupSel.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxTagGroupSel.FormattingEnabled = true;
            this.comboBoxTagGroupSel.Location = new System.Drawing.Point(257, 8);
            this.comboBoxTagGroupSel.Margin = new System.Windows.Forms.Padding(3, 8, 3, 3);
            this.comboBoxTagGroupSel.MaxDropDownItems = 24;
            this.comboBoxTagGroupSel.Name = "comboBoxTagGroupSel";
            this.comboBoxTagGroupSel.Size = new System.Drawing.Size(344, 25);
            this.comboBoxTagGroupSel.TabIndex = 8;
            this.comboBoxTagGroupSel.SelectedIndexChanged += new System.EventHandler(this.comboBoxTagGroupSel_SelectedIndexChanged);
            // 
            // groupBoxCurOrHistory
            // 
            this.groupBoxCurOrHistory.Controls.Add(this.radioButtonHistoryTag);
            this.groupBoxCurOrHistory.Controls.Add(this.radioButtonCurTag);
            this.groupBoxCurOrHistory.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBoxCurOrHistory.Location = new System.Drawing.Point(109, 0);
            this.groupBoxCurOrHistory.Margin = new System.Windows.Forms.Padding(0, 0, 0, 6);
            this.groupBoxCurOrHistory.Name = "groupBoxCurOrHistory";
            this.groupBoxCurOrHistory.Size = new System.Drawing.Size(135, 34);
            this.groupBoxCurOrHistory.TabIndex = 9;
            this.groupBoxCurOrHistory.TabStop = false;
            // 
            // radioButtonHistoryTag
            // 
            this.radioButtonHistoryTag.AutoSize = true;
            this.radioButtonHistoryTag.Location = new System.Drawing.Point(53, 10);
            this.radioButtonHistoryTag.Name = "radioButtonHistoryTag";
            this.radioButtonHistoryTag.Size = new System.Drawing.Size(78, 21);
            this.radioButtonHistoryTag.TabIndex = 1;
            this.radioButtonHistoryTag.Text = "HISTORY";
            this.radioButtonHistoryTag.UseVisualStyleBackColor = true;
            this.radioButtonHistoryTag.CheckedChanged += new System.EventHandler(this.radioButtonHistoryTag_CheckedChanged);
            // 
            // radioButtonCurTag
            // 
            this.radioButtonCurTag.AutoSize = true;
            this.radioButtonCurTag.Checked = true;
            this.radioButtonCurTag.Location = new System.Drawing.Point(4, 10);
            this.radioButtonCurTag.Name = "radioButtonCurTag";
            this.radioButtonCurTag.Size = new System.Drawing.Size(51, 21);
            this.radioButtonCurTag.TabIndex = 0;
            this.radioButtonCurTag.TabStop = true;
            this.radioButtonCurTag.Text = "CUR";
            this.radioButtonCurTag.UseVisualStyleBackColor = true;
            // 
            // CurrentTagValueMonitorDGV
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 17F);
            this.ClientSize = new System.Drawing.Size(1164, 834);
            this.ContextMenuStrip = this.contextMenuStrip1;
            this.Controls.Add(this.tableLayoutTagValueMonitorMenu);
            this.Font = new System.Drawing.Font("맑은 고딕", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.Name = "CurrentTagValueMonitorDGV";
            this.Text = "Monitor";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.EquipmentMonitor_FormClosed);
            this.Load += new System.EventHandler(this.CurrentTagValueMonitor_Load);
            this.contextMenuStrip1.ResumeLayout(false);
            this.panel8.ResumeLayout(false);
            this.panel7.ResumeLayout(false);
            this.panel4.ResumeLayout(false);
            this.panel3.ResumeLayout(false);
            this.panel2.ResumeLayout(false);
            this.tableLayoutTagValueMonitorMenu.ResumeLayout(false);
            this.panel1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView_Main)).EndInit();
            this.groupBoxCurOrHistory.ResumeLayout(false);
            this.groupBoxCurOrHistory.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.ImageList imageList2;
        private System.Windows.Forms.ImageList imageList1;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
        private System.Windows.Forms.ToolStripMenuItem valueHistoryToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem editToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem addToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem deleteToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemLog;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemData;
        private System.Windows.Forms.Panel panel8;
        private System.Windows.Forms.Button button_Find;
        private System.Windows.Forms.Panel panel7;
        private System.Windows.Forms.Button button_SetInterval;
        private System.Windows.Forms.Panel panel4;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Panel panel3;
        private System.Windows.Forms.Button button_Refresh;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.CheckBox checkBox_AutoRefresh;
        private System.Windows.Forms.TableLayoutPanel tableLayoutTagValueMonitorMenu;
        private System.Windows.Forms.Button buttonFilter;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.DataGridView dataGridView_Main;
        private System.Windows.Forms.ComboBox comboBoxTagGroupSel;
        private System.Windows.Forms.GroupBox groupBoxCurOrHistory;
        private System.Windows.Forms.RadioButton radioButtonHistoryTag;
        private System.Windows.Forms.RadioButton radioButtonCurTag;
    }
}
