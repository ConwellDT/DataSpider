
namespace DataSpider.FailoverManager
{
    partial class MainForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.notifyIcon1 = new System.Windows.Forms.NotifyIcon(this.components);
            this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.열기ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.종료ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.panel1 = new System.Windows.Forms.Panel();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.panel2 = new System.Windows.Forms.Panel();
            this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
            this.panel5 = new System.Windows.Forms.Panel();
            this.label_Version = new System.Windows.Forms.Label();
            this.lbTitle = new System.Windows.Forms.Label();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.panel3 = new System.Windows.Forms.Panel();
            this.tableLayoutPanel3 = new System.Windows.Forms.TableLayoutPanel();
            this.panel6 = new System.Windows.Forms.Panel();
            this.tableLayoutPanel4 = new System.Windows.Forms.TableLayoutPanel();
            this.panel10 = new System.Windows.Forms.Panel();
            this.label4 = new System.Windows.Forms.Label();
            this.panel11 = new System.Windows.Forms.Panel();
            this.LvRevLog = new DataSpider.PC03.PT.Controls.CWListView();
            this.panel14 = new System.Windows.Forms.Panel();
            this.tbShowLines = new System.Windows.Forms.TextBox();
            this.lbShowLines = new System.Windows.Forms.Label();
            this.cbInfo = new System.Windows.Forms.CheckBox();
            this.cbError = new System.Windows.Forms.CheckBox();
            this.btnChange = new System.Windows.Forms.Button();
            this.cbDebug = new System.Windows.Forms.CheckBox();
            this.panel4 = new System.Windows.Forms.Panel();
            this.lblRunTime = new System.Windows.Forms.Label();
            this.pbDbCon = new System.Windows.Forms.PictureBox();
            this.contextMenuStrip1.SuspendLayout();
            this.panel1.SuspendLayout();
            this.tableLayoutPanel1.SuspendLayout();
            this.panel2.SuspendLayout();
            this.tableLayoutPanel2.SuspendLayout();
            this.panel5.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.panel3.SuspendLayout();
            this.tableLayoutPanel3.SuspendLayout();
            this.panel6.SuspendLayout();
            this.tableLayoutPanel4.SuspendLayout();
            this.panel10.SuspendLayout();
            this.panel11.SuspendLayout();
            this.panel14.SuspendLayout();
            this.panel4.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pbDbCon)).BeginInit();
            this.SuspendLayout();
            // 
            // notifyIcon1
            // 
            this.notifyIcon1.ContextMenuStrip = this.contextMenuStrip1;
            this.notifyIcon1.Icon = ((System.Drawing.Icon)(resources.GetObject("notifyIcon1.Icon")));
            this.notifyIcon1.Text = "DataSpider.FailoverManager";
            this.notifyIcon1.Visible = true;
            this.notifyIcon1.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.notifyIcon1_MouseDoubleClick);
            // 
            // contextMenuStrip1
            // 
            this.contextMenuStrip1.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.열기ToolStripMenuItem,
            this.toolStripSeparator1,
            this.종료ToolStripMenuItem});
            this.contextMenuStrip1.Name = "contextMenuStrip1";
            this.contextMenuStrip1.Size = new System.Drawing.Size(109, 58);
            // 
            // 열기ToolStripMenuItem
            // 
            this.열기ToolStripMenuItem.Name = "열기ToolStripMenuItem";
            this.열기ToolStripMenuItem.Size = new System.Drawing.Size(108, 24);
            this.열기ToolStripMenuItem.Text = "열기";
            this.열기ToolStripMenuItem.Click += new System.EventHandler(this.열기ToolStripMenuItem_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(105, 6);
            // 
            // 종료ToolStripMenuItem
            // 
            this.종료ToolStripMenuItem.Name = "종료ToolStripMenuItem";
            this.종료ToolStripMenuItem.Size = new System.Drawing.Size(108, 24);
            this.종료ToolStripMenuItem.Text = "종료";
            this.종료ToolStripMenuItem.Click += new System.EventHandler(this.종료ToolStripMenuItem_Click);
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.tableLayoutPanel1);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(990, 660);
            this.panel1.TabIndex = 3;
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 1;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Controls.Add(this.panel2, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.panel3, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.panel4, 0, 2);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Margin = new System.Windows.Forms.Padding(3, 1, 3, 1);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 3;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 35F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 45F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(990, 660);
            this.tableLayoutPanel1.TabIndex = 0;
            // 
            // panel2
            // 
            this.panel2.Controls.Add(this.tableLayoutPanel2);
            this.panel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel2.Location = new System.Drawing.Point(3, 3);
            this.panel2.Margin = new System.Windows.Forms.Padding(3, 3, 3, 1);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(984, 31);
            this.panel2.TabIndex = 0;
            // 
            // tableLayoutPanel2
            // 
            this.tableLayoutPanel2.ColumnCount = 2;
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 73F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel2.Controls.Add(this.panel5, 1, 0);
            this.tableLayoutPanel2.Controls.Add(this.pictureBox1, 0, 0);
            this.tableLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel2.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel2.Name = "tableLayoutPanel2";
            this.tableLayoutPanel2.RowCount = 1;
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel2.Size = new System.Drawing.Size(984, 31);
            this.tableLayoutPanel2.TabIndex = 0;
            // 
            // panel5
            // 
            this.panel5.Controls.Add(this.label_Version);
            this.panel5.Controls.Add(this.lbTitle);
            this.panel5.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel5.Location = new System.Drawing.Point(76, 3);
            this.panel5.Margin = new System.Windows.Forms.Padding(3, 3, 3, 1);
            this.panel5.Name = "panel5";
            this.panel5.Size = new System.Drawing.Size(905, 27);
            this.panel5.TabIndex = 0;
            // 
            // label_Version
            // 
            this.label_Version.AutoSize = true;
            this.label_Version.BackColor = System.Drawing.Color.RoyalBlue;
            this.label_Version.Font = new System.Drawing.Font("맑은 고딕", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label_Version.ForeColor = System.Drawing.Color.White;
            this.label_Version.Location = new System.Drawing.Point(783, 8);
            this.label_Version.Name = "label_Version";
            this.label_Version.Size = new System.Drawing.Size(0, 20);
            this.label_Version.TabIndex = 4;
            // 
            // lbTitle
            // 
            this.lbTitle.BackColor = System.Drawing.Color.RoyalBlue;
            this.lbTitle.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lbTitle.Font = new System.Drawing.Font("Arial", 15F, System.Drawing.FontStyle.Bold);
            this.lbTitle.ForeColor = System.Drawing.Color.White;
            this.lbTitle.Location = new System.Drawing.Point(0, 0);
            this.lbTitle.Margin = new System.Windows.Forms.Padding(3, 3, 3, 0);
            this.lbTitle.Name = "lbTitle";
            this.lbTitle.Size = new System.Drawing.Size(905, 27);
            this.lbTitle.TabIndex = 0;
            this.lbTitle.Text = "DataSpider FailoverManager";
            this.lbTitle.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // pictureBox1
            // 
            this.pictureBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pictureBox1.Image = ((System.Drawing.Image)(resources.GetObject("pictureBox1.Image")));
            this.pictureBox1.Location = new System.Drawing.Point(3, 3);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(67, 25);
            this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pictureBox1.TabIndex = 1;
            this.pictureBox1.TabStop = false;
            // 
            // panel3
            // 
            this.panel3.Controls.Add(this.tableLayoutPanel3);
            this.panel3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel3.Location = new System.Drawing.Point(3, 36);
            this.panel3.Margin = new System.Windows.Forms.Padding(3, 1, 3, 1);
            this.panel3.Name = "panel3";
            this.panel3.Size = new System.Drawing.Size(984, 578);
            this.panel3.TabIndex = 0;
            // 
            // tableLayoutPanel3
            // 
            this.tableLayoutPanel3.ColumnCount = 1;
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 700F));
            this.tableLayoutPanel3.Controls.Add(this.panel6, 0, 0);
            this.tableLayoutPanel3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel3.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel3.Margin = new System.Windows.Forms.Padding(3, 1, 3, 1);
            this.tableLayoutPanel3.Name = "tableLayoutPanel3";
            this.tableLayoutPanel3.RowCount = 1;
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 541F));
            this.tableLayoutPanel3.Size = new System.Drawing.Size(984, 578);
            this.tableLayoutPanel3.TabIndex = 0;
            // 
            // panel6
            // 
            this.panel6.Controls.Add(this.tableLayoutPanel4);
            this.panel6.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel6.Location = new System.Drawing.Point(0, 1);
            this.panel6.Margin = new System.Windows.Forms.Padding(0, 1, 1, 1);
            this.panel6.Name = "panel6";
            this.panel6.Size = new System.Drawing.Size(983, 576);
            this.panel6.TabIndex = 0;
            // 
            // tableLayoutPanel4
            // 
            this.tableLayoutPanel4.ColumnCount = 1;
            this.tableLayoutPanel4.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel4.Controls.Add(this.panel10, 0, 0);
            this.tableLayoutPanel4.Controls.Add(this.panel11, 0, 2);
            this.tableLayoutPanel4.Controls.Add(this.panel14, 0, 1);
            this.tableLayoutPanel4.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel4.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel4.Margin = new System.Windows.Forms.Padding(1);
            this.tableLayoutPanel4.Name = "tableLayoutPanel4";
            this.tableLayoutPanel4.RowCount = 3;
            this.tableLayoutPanel4.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel4.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 24F));
            this.tableLayoutPanel4.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel4.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel4.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel4.Size = new System.Drawing.Size(983, 576);
            this.tableLayoutPanel4.TabIndex = 1;
            // 
            // panel10
            // 
            this.panel10.BackColor = System.Drawing.SystemColors.Control;
            this.panel10.Controls.Add(this.label4);
            this.panel10.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel10.Location = new System.Drawing.Point(3, 1);
            this.panel10.Margin = new System.Windows.Forms.Padding(3, 1, 1, 1);
            this.panel10.Name = "panel10";
            this.panel10.Size = new System.Drawing.Size(979, 18);
            this.panel10.TabIndex = 0;
            // 
            // label4
            // 
            this.label4.BackColor = System.Drawing.Color.RoyalBlue;
            this.label4.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label4.Font = new System.Drawing.Font("Arial", 9.5F, System.Drawing.FontStyle.Bold);
            this.label4.ForeColor = System.Drawing.Color.White;
            this.label4.Location = new System.Drawing.Point(0, 0);
            this.label4.Margin = new System.Windows.Forms.Padding(3, 0, 1, 0);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(979, 18);
            this.label4.TabIndex = 0;
            this.label4.Text = "Process Log";
            // 
            // panel11
            // 
            this.panel11.Controls.Add(this.LvRevLog);
            this.panel11.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel11.Location = new System.Drawing.Point(3, 45);
            this.panel11.Margin = new System.Windows.Forms.Padding(3, 1, 1, 1);
            this.panel11.Name = "panel11";
            this.panel11.Size = new System.Drawing.Size(979, 530);
            this.panel11.TabIndex = 0;
            // 
            // LvRevLog
            // 
            this.LvRevLog.AutoScroll = true;
            this.LvRevLog.Dock = System.Windows.Forms.DockStyle.Fill;
            this.LvRevLog.Font = new System.Drawing.Font("굴림", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.LvRevLog.Location = new System.Drawing.Point(0, 0);
            this.LvRevLog.Margin = new System.Windows.Forms.Padding(3, 3, 1, 1);
            this.LvRevLog.Name = "LvRevLog";
            this.LvRevLog.Size = new System.Drawing.Size(979, 530);
            this.LvRevLog.TabIndex = 0;
            // 
            // panel14
            // 
            this.panel14.Controls.Add(this.tbShowLines);
            this.panel14.Controls.Add(this.lbShowLines);
            this.panel14.Controls.Add(this.cbInfo);
            this.panel14.Controls.Add(this.cbError);
            this.panel14.Controls.Add(this.btnChange);
            this.panel14.Controls.Add(this.cbDebug);
            this.panel14.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel14.Location = new System.Drawing.Point(3, 20);
            this.panel14.Margin = new System.Windows.Forms.Padding(3, 0, 1, 0);
            this.panel14.Name = "panel14";
            this.panel14.Size = new System.Drawing.Size(979, 24);
            this.panel14.TabIndex = 0;
            // 
            // tbShowLines
            // 
            this.tbShowLines.Font = new System.Drawing.Font("굴림", 8F);
            this.tbShowLines.Location = new System.Drawing.Point(854, 3);
            this.tbShowLines.MaxLength = 4;
            this.tbShowLines.Name = "tbShowLines";
            this.tbShowLines.Size = new System.Drawing.Size(50, 23);
            this.tbShowLines.TabIndex = 4;
            this.tbShowLines.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // lbShowLines
            // 
            this.lbShowLines.AutoSize = true;
            this.lbShowLines.Font = new System.Drawing.Font("굴림", 8F);
            this.lbShowLines.Location = new System.Drawing.Point(759, 6);
            this.lbShowLines.Name = "lbShowLines";
            this.lbShowLines.Size = new System.Drawing.Size(94, 14);
            this.lbShowLines.TabIndex = 3;
            this.lbShowLines.Text = "Show Lines :";
            // 
            // cbInfo
            // 
            this.cbInfo.AutoSize = true;
            this.cbInfo.Font = new System.Drawing.Font("굴림", 8F);
            this.cbInfo.Location = new System.Drawing.Point(0, 5);
            this.cbInfo.Name = "cbInfo";
            this.cbInfo.Size = new System.Drawing.Size(53, 18);
            this.cbInfo.TabIndex = 0;
            this.cbInfo.Text = "Info";
            this.cbInfo.UseVisualStyleBackColor = true;
            // 
            // cbError
            // 
            this.cbError.AutoSize = true;
            this.cbError.Font = new System.Drawing.Font("굴림", 8F);
            this.cbError.Location = new System.Drawing.Point(83, 5);
            this.cbError.Name = "cbError";
            this.cbError.Size = new System.Drawing.Size(59, 18);
            this.cbError.TabIndex = 1;
            this.cbError.Text = "Error";
            this.cbError.UseVisualStyleBackColor = true;
            // 
            // btnChange
            // 
            this.btnChange.Font = new System.Drawing.Font("굴림", 8F);
            this.btnChange.Location = new System.Drawing.Point(912, 2);
            this.btnChange.Name = "btnChange";
            this.btnChange.Size = new System.Drawing.Size(70, 21);
            this.btnChange.TabIndex = 5;
            this.btnChange.Text = "Change";
            this.btnChange.UseVisualStyleBackColor = true;
            // 
            // cbDebug
            // 
            this.cbDebug.AutoSize = true;
            this.cbDebug.Font = new System.Drawing.Font("굴림", 8F);
            this.cbDebug.Location = new System.Drawing.Point(162, 5);
            this.cbDebug.Name = "cbDebug";
            this.cbDebug.Size = new System.Drawing.Size(71, 18);
            this.cbDebug.TabIndex = 2;
            this.cbDebug.Text = "Debug";
            this.cbDebug.UseVisualStyleBackColor = true;
            // 
            // panel4
            // 
            this.panel4.BackColor = System.Drawing.Color.RoyalBlue;
            this.panel4.Controls.Add(this.lblRunTime);
            this.panel4.Controls.Add(this.pbDbCon);
            this.panel4.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel4.Location = new System.Drawing.Point(6, 616);
            this.panel4.Margin = new System.Windows.Forms.Padding(6, 1, 6, 6);
            this.panel4.Name = "panel4";
            this.panel4.Size = new System.Drawing.Size(978, 38);
            this.panel4.TabIndex = 0;
            // 
            // lblRunTime
            // 
            this.lblRunTime.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.lblRunTime.BackColor = System.Drawing.Color.RoyalBlue;
            this.lblRunTime.Font = new System.Drawing.Font("굴림", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.lblRunTime.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.lblRunTime.Location = new System.Drawing.Point(733, 6);
            this.lblRunTime.Name = "lblRunTime";
            this.lblRunTime.Size = new System.Drawing.Size(235, 24);
            this.lblRunTime.TabIndex = 68;
            this.lblRunTime.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // pbDbCon
            // 
            this.pbDbCon.Location = new System.Drawing.Point(3, 3);
            this.pbDbCon.Name = "pbDbCon";
            this.pbDbCon.Size = new System.Drawing.Size(161, 31);
            this.pbDbCon.TabIndex = 70;
            this.pbDbCon.TabStop = false;
            // 
            // MainForm
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.ClientSize = new System.Drawing.Size(990, 660);
            this.Controls.Add(this.panel1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.Name = "MainForm";
            this.ShowInTaskbar = false;
            this.Text = "DataSpider FailoverManager";
            this.WindowState = System.Windows.Forms.FormWindowState.Minimized;
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MainForm_FormClosing);
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.contextMenuStrip1.ResumeLayout(false);
            this.panel1.ResumeLayout(false);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.panel2.ResumeLayout(false);
            this.tableLayoutPanel2.ResumeLayout(false);
            this.panel5.ResumeLayout(false);
            this.panel5.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.panel3.ResumeLayout(false);
            this.tableLayoutPanel3.ResumeLayout(false);
            this.panel6.ResumeLayout(false);
            this.tableLayoutPanel4.ResumeLayout(false);
            this.panel10.ResumeLayout(false);
            this.panel11.ResumeLayout(false);
            this.panel14.ResumeLayout(false);
            this.panel14.PerformLayout();
            this.panel4.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pbDbCon)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.NotifyIcon notifyIcon1;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
        private System.Windows.Forms.ToolStripMenuItem 열기ToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripMenuItem 종료ToolStripMenuItem;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
        private System.Windows.Forms.Panel panel5;
        private System.Windows.Forms.Label label_Version;
        private System.Windows.Forms.Label lbTitle;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.Panel panel3;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel3;
        private System.Windows.Forms.Panel panel6;
        private System.Windows.Forms.Panel panel4;
        private System.Windows.Forms.Label lblRunTime;
        private System.Windows.Forms.PictureBox pbDbCon;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel4;
        private System.Windows.Forms.Panel panel10;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Panel panel11;
        private PC03.PT.Controls.CWListView LvRevLog;
        private System.Windows.Forms.Panel panel14;
        private System.Windows.Forms.TextBox tbShowLines;
        private System.Windows.Forms.Label lbShowLines;
        private System.Windows.Forms.CheckBox cbInfo;
        private System.Windows.Forms.CheckBox cbError;
        private System.Windows.Forms.Button btnChange;
        private System.Windows.Forms.CheckBox cbDebug;
    }
}

