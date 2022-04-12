
namespace DataSpider.UserMonitor
{
    partial class PIAlarmMonitor
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(PIAlarmMonitor));
            System.Windows.Forms.ListViewItem listViewItem1 = new System.Windows.Forms.ListViewItem("", "(없음)");
            this.imageList2 = new System.Windows.Forms.ImageList(this.components);
            this.imageList1 = new System.Windows.Forms.ImageList(this.components);
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.panel1 = new System.Windows.Forms.Panel();
            this.listView_Main = new System.Windows.Forms.ListView();
            this.panel2 = new System.Windows.Forms.Panel();
            this.button_Refresh = new System.Windows.Forms.Button();
            this.panel3 = new System.Windows.Forms.Panel();
            this.checkBox_AutoRefresh = new System.Windows.Forms.CheckBox();
            this.panel4 = new System.Windows.Forms.Panel();
            this.textBox_RefreshInterval = new System.Windows.Forms.TextBox();
            this.panel5 = new System.Windows.Forms.Panel();
            this.label1 = new System.Windows.Forms.Label();
            this.panel6 = new System.Windows.Forms.Panel();
            this.label2 = new System.Windows.Forms.Label();
            this.panel7 = new System.Windows.Forms.Panel();
            this.button_SetInterval = new System.Windows.Forms.Button();
            this.tableLayoutPanel1.SuspendLayout();
            this.panel1.SuspendLayout();
            this.panel2.SuspendLayout();
            this.panel3.SuspendLayout();
            this.panel4.SuspendLayout();
            this.panel5.SuspendLayout();
            this.panel6.SuspendLayout();
            this.panel7.SuspendLayout();
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
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 6;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 150F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 100F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 100F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 150F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 150F));
            this.tableLayoutPanel1.Controls.Add(this.panel1, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.panel2, 5, 0);
            this.tableLayoutPanel1.Controls.Add(this.panel3, 4, 0);
            this.tableLayoutPanel1.Controls.Add(this.panel4, 2, 0);
            this.tableLayoutPanel1.Controls.Add(this.panel5, 1, 0);
            this.tableLayoutPanel1.Controls.Add(this.panel6, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.panel7, 3, 0);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 2;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 40F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(902, 834);
            this.tableLayoutPanel1.TabIndex = 1;
            // 
            // panel1
            // 
            this.tableLayoutPanel1.SetColumnSpan(this.panel1, 6);
            this.panel1.Controls.Add(this.listView_Main);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel1.Location = new System.Drawing.Point(3, 43);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(896, 788);
            this.panel1.TabIndex = 0;
            // 
            // listView_Main
            // 
            this.listView_Main.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listView_Main.Font = new System.Drawing.Font("맑은 고딕", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.listView_Main.FullRowSelect = true;
            this.listView_Main.GridLines = true;
            this.listView_Main.HideSelection = false;
            listViewItem1.StateImageIndex = 0;
            this.listView_Main.Items.AddRange(new System.Windows.Forms.ListViewItem[] {
            listViewItem1});
            this.listView_Main.Location = new System.Drawing.Point(0, 0);
            this.listView_Main.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.listView_Main.MultiSelect = false;
            this.listView_Main.Name = "listView_Main";
            this.listView_Main.Size = new System.Drawing.Size(896, 788);
            this.listView_Main.TabIndex = 1;
            this.listView_Main.UseCompatibleStateImageBehavior = false;
            this.listView_Main.View = System.Windows.Forms.View.Details;
            // 
            // panel2
            // 
            this.panel2.Controls.Add(this.button_Refresh);
            this.panel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel2.Location = new System.Drawing.Point(755, 3);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(144, 34);
            this.panel2.TabIndex = 1;
            // 
            // button_Refresh
            // 
            this.button_Refresh.Dock = System.Windows.Forms.DockStyle.Fill;
            this.button_Refresh.Location = new System.Drawing.Point(0, 0);
            this.button_Refresh.Name = "button_Refresh";
            this.button_Refresh.Size = new System.Drawing.Size(144, 34);
            this.button_Refresh.TabIndex = 1;
            this.button_Refresh.Text = "Refresh";
            this.button_Refresh.UseVisualStyleBackColor = true;
            this.button_Refresh.Click += new System.EventHandler(this.button_Refresh_Click);
            // 
            // panel3
            // 
            this.panel3.Controls.Add(this.checkBox_AutoRefresh);
            this.panel3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel3.Location = new System.Drawing.Point(605, 3);
            this.panel3.Name = "panel3";
            this.panel3.Size = new System.Drawing.Size(144, 34);
            this.panel3.TabIndex = 2;
            // 
            // checkBox_AutoRefresh
            // 
            this.checkBox_AutoRefresh.Checked = true;
            this.checkBox_AutoRefresh.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBox_AutoRefresh.Location = new System.Drawing.Point(0, 0);
            this.checkBox_AutoRefresh.Name = "checkBox_AutoRefresh";
            this.checkBox_AutoRefresh.Size = new System.Drawing.Size(144, 34);
            this.checkBox_AutoRefresh.TabIndex = 1;
            this.checkBox_AutoRefresh.Text = "Auto Refresh";
            this.checkBox_AutoRefresh.UseVisualStyleBackColor = true;
            // 
            // panel4
            // 
            this.panel4.Controls.Add(this.textBox_RefreshInterval);
            this.panel4.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel4.Location = new System.Drawing.Point(405, 3);
            this.panel4.Name = "panel4";
            this.panel4.Size = new System.Drawing.Size(94, 34);
            this.panel4.TabIndex = 3;
            // 
            // textBox_RefreshInterval
            // 
            this.textBox_RefreshInterval.Location = new System.Drawing.Point(0, 5);
            this.textBox_RefreshInterval.Name = "textBox_RefreshInterval";
            this.textBox_RefreshInterval.Size = new System.Drawing.Size(94, 29);
            this.textBox_RefreshInterval.TabIndex = 1;
            this.textBox_RefreshInterval.Text = "10";
            // 
            // panel5
            // 
            this.panel5.Controls.Add(this.label1);
            this.panel5.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel5.Location = new System.Drawing.Point(255, 3);
            this.panel5.Name = "panel5";
            this.panel5.Size = new System.Drawing.Size(144, 34);
            this.panel5.TabIndex = 4;
            // 
            // label1
            // 
            this.label1.Location = new System.Drawing.Point(0, -1);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(144, 37);
            this.label1.TabIndex = 3;
            this.label1.Text = "Refresh Interval (sec)";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // panel6
            // 
            this.panel6.Controls.Add(this.label2);
            this.panel6.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel6.Location = new System.Drawing.Point(3, 3);
            this.panel6.Name = "panel6";
            this.panel6.Size = new System.Drawing.Size(246, 34);
            this.panel6.TabIndex = 5;
            // 
            // label2
            // 
            this.label2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.label2.Font = new System.Drawing.Font("맑은 고딕", 16F);
            this.label2.Location = new System.Drawing.Point(0, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(246, 34);
            this.label2.TabIndex = 4;
            this.label2.Text = "PI Alarm Monitor";
            this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // panel7
            // 
            this.panel7.Controls.Add(this.button_SetInterval);
            this.panel7.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel7.Location = new System.Drawing.Point(505, 3);
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
            this.button_SetInterval.TabIndex = 2;
            this.button_SetInterval.Text = "Set Interval";
            this.button_SetInterval.UseVisualStyleBackColor = true;
            this.button_SetInterval.Click += new System.EventHandler(this.button_SetInterval_Click);
            // 
            // PIAlarmMonitor
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 21F);
            this.ClientSize = new System.Drawing.Size(902, 834);
            this.Controls.Add(this.tableLayoutPanel1);
            this.Font = new System.Drawing.Font("맑은 고딕", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.Name = "PIAlarmMonitor";
            this.Text = "Monitor";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.PIAlarmMonitor_FormClosed);
            this.Load += new System.EventHandler(this.PIAlarmMonitor_Load);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.panel1.ResumeLayout(false);
            this.panel2.ResumeLayout(false);
            this.panel3.ResumeLayout(false);
            this.panel4.ResumeLayout(false);
            this.panel4.PerformLayout();
            this.panel5.ResumeLayout(false);
            this.panel6.ResumeLayout(false);
            this.panel7.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.ImageList imageList2;
        private System.Windows.Forms.ImageList imageList1;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.ListView listView_Main;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.Panel panel3;
        private System.Windows.Forms.Panel panel4;
        private System.Windows.Forms.Panel panel5;
        private System.Windows.Forms.Panel panel6;
        private System.Windows.Forms.Button button_Refresh;
        private System.Windows.Forms.CheckBox checkBox_AutoRefresh;
        private System.Windows.Forms.TextBox textBox_RefreshInterval;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Panel panel7;
        private System.Windows.Forms.Button button_SetInterval;
    }
}
