
namespace DataSpider.UserMonitor
{
    partial class SystemLogView
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
            System.Windows.Forms.ListViewItem listViewItem1 = new System.Windows.Forms.ListViewItem("", "(없음)");
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SystemLogView));
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.panel1 = new System.Windows.Forms.Panel();
            this.listView_Main = new System.Windows.Forms.ListView();
            this.panel6 = new System.Windows.Forms.Panel();
            this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
            this.panel8 = new System.Windows.Forms.Panel();
            this.label2 = new System.Windows.Forms.Label();
            this.panel9 = new System.Windows.Forms.Panel();
            this.label1 = new System.Windows.Forms.Label();
            this.panel10 = new System.Windows.Forms.Panel();
            this.textBox_RefreshInterval = new System.Windows.Forms.TextBox();
            this.panel11 = new System.Windows.Forms.Panel();
            this.button_SetInterval = new System.Windows.Forms.Button();
            this.panel12 = new System.Windows.Forms.Panel();
            this.checkBox_AutoRefresh = new System.Windows.Forms.CheckBox();
            this.panel13 = new System.Windows.Forms.Panel();
            this.tableLayoutPanel3 = new System.Windows.Forms.TableLayoutPanel();
            this.panel14 = new System.Windows.Forms.Panel();
            this.comboBox_Equipment = new System.Windows.Forms.ComboBox();
            this.panel2 = new System.Windows.Forms.Panel();
            this.button_Refresh = new System.Windows.Forms.Button();
            this.panel3 = new System.Windows.Forms.Panel();
            this.dateTimePicker_Start = new System.Windows.Forms.DateTimePicker();
            this.panel4 = new System.Windows.Forms.Panel();
            this.comboBox_Level = new System.Windows.Forms.ComboBox();
            this.panel5 = new System.Windows.Forms.Panel();
            this.dateTimePicker_End = new System.Windows.Forms.DateTimePicker();
            this.panel7 = new System.Windows.Forms.Panel();
            this.label3 = new System.Windows.Forms.Label();
            this.panel15 = new System.Windows.Forms.Panel();
            this.label4 = new System.Windows.Forms.Label();
            this.panel16 = new System.Windows.Forms.Panel();
            this.label5 = new System.Windows.Forms.Label();
            this.panel17 = new System.Windows.Forms.Panel();
            this.dateTimePicker_StartTime = new System.Windows.Forms.DateTimePicker();
            this.panel18 = new System.Windows.Forms.Panel();
            this.dateTimePicker_EndTime = new System.Windows.Forms.DateTimePicker();
            this.tableLayoutPanel1.SuspendLayout();
            this.panel1.SuspendLayout();
            this.panel6.SuspendLayout();
            this.tableLayoutPanel2.SuspendLayout();
            this.panel8.SuspendLayout();
            this.panel9.SuspendLayout();
            this.panel10.SuspendLayout();
            this.panel11.SuspendLayout();
            this.panel12.SuspendLayout();
            this.panel13.SuspendLayout();
            this.tableLayoutPanel3.SuspendLayout();
            this.panel14.SuspendLayout();
            this.panel2.SuspendLayout();
            this.panel3.SuspendLayout();
            this.panel4.SuspendLayout();
            this.panel5.SuspendLayout();
            this.panel7.SuspendLayout();
            this.panel15.SuspendLayout();
            this.panel16.SuspendLayout();
            this.panel17.SuspendLayout();
            this.panel18.SuspendLayout();
            this.SuspendLayout();
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 1;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Controls.Add(this.panel1, 0, 2);
            this.tableLayoutPanel1.Controls.Add(this.panel6, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.panel13, 0, 1);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 3;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 50F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 40F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(1333, 834);
            this.tableLayoutPanel1.TabIndex = 1;
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.listView_Main);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel1.Location = new System.Drawing.Point(3, 93);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(1327, 738);
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
            this.listView_Main.Size = new System.Drawing.Size(1327, 738);
            this.listView_Main.TabIndex = 1;
            this.listView_Main.UseCompatibleStateImageBehavior = false;
            this.listView_Main.View = System.Windows.Forms.View.Details;
            // 
            // panel6
            // 
            this.panel6.Controls.Add(this.tableLayoutPanel2);
            this.panel6.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel6.Location = new System.Drawing.Point(3, 3);
            this.panel6.Name = "panel6";
            this.panel6.Size = new System.Drawing.Size(1327, 44);
            this.panel6.TabIndex = 5;
            // 
            // tableLayoutPanel2
            // 
            this.tableLayoutPanel2.ColumnCount = 5;
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 150F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 100F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 100F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 120F));
            this.tableLayoutPanel2.Controls.Add(this.panel8, 0, 0);
            this.tableLayoutPanel2.Controls.Add(this.panel9, 1, 0);
            this.tableLayoutPanel2.Controls.Add(this.panel10, 2, 0);
            this.tableLayoutPanel2.Controls.Add(this.panel11, 3, 0);
            this.tableLayoutPanel2.Controls.Add(this.panel12, 4, 0);
            this.tableLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel2.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel2.Name = "tableLayoutPanel2";
            this.tableLayoutPanel2.RowCount = 1;
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel2.Size = new System.Drawing.Size(1327, 44);
            this.tableLayoutPanel2.TabIndex = 0;
            // 
            // panel8
            // 
            this.panel8.Controls.Add(this.label2);
            this.panel8.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel8.Location = new System.Drawing.Point(3, 3);
            this.panel8.Name = "panel8";
            this.panel8.Size = new System.Drawing.Size(851, 38);
            this.panel8.TabIndex = 0;
            // 
            // label2
            // 
            this.label2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label2.Font = new System.Drawing.Font("맑은 고딕", 16F);
            this.label2.Location = new System.Drawing.Point(0, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(851, 38);
            this.label2.TabIndex = 5;
            this.label2.Text = "System Log";
            this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // panel9
            // 
            this.panel9.Controls.Add(this.label1);
            this.panel9.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel9.Location = new System.Drawing.Point(860, 3);
            this.panel9.Name = "panel9";
            this.panel9.Size = new System.Drawing.Size(144, 38);
            this.panel9.TabIndex = 1;
            // 
            // label1
            // 
            this.label1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label1.Location = new System.Drawing.Point(0, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(144, 38);
            this.label1.TabIndex = 4;
            this.label1.Text = "Refresh Interval (sec)";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // panel10
            // 
            this.panel10.Controls.Add(this.textBox_RefreshInterval);
            this.panel10.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel10.Location = new System.Drawing.Point(1010, 3);
            this.panel10.Name = "panel10";
            this.panel10.Size = new System.Drawing.Size(94, 38);
            this.panel10.TabIndex = 2;
            // 
            // textBox_RefreshInterval
            // 
            this.textBox_RefreshInterval.Location = new System.Drawing.Point(0, 6);
            this.textBox_RefreshInterval.Name = "textBox_RefreshInterval";
            this.textBox_RefreshInterval.Size = new System.Drawing.Size(94, 25);
            this.textBox_RefreshInterval.TabIndex = 2;
            this.textBox_RefreshInterval.Text = "10";
            this.textBox_RefreshInterval.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // panel11
            // 
            this.panel11.Controls.Add(this.button_SetInterval);
            this.panel11.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel11.Location = new System.Drawing.Point(1110, 3);
            this.panel11.Name = "panel11";
            this.panel11.Size = new System.Drawing.Size(94, 38);
            this.panel11.TabIndex = 3;
            // 
            // button_SetInterval
            // 
            this.button_SetInterval.Dock = System.Windows.Forms.DockStyle.Fill;
            this.button_SetInterval.Location = new System.Drawing.Point(0, 0);
            this.button_SetInterval.Name = "button_SetInterval";
            this.button_SetInterval.Size = new System.Drawing.Size(94, 38);
            this.button_SetInterval.TabIndex = 3;
            this.button_SetInterval.Text = "Set Interval";
            this.button_SetInterval.UseVisualStyleBackColor = true;
            // 
            // panel12
            // 
            this.panel12.Controls.Add(this.checkBox_AutoRefresh);
            this.panel12.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel12.Location = new System.Drawing.Point(1210, 3);
            this.panel12.Name = "panel12";
            this.panel12.Size = new System.Drawing.Size(114, 38);
            this.panel12.TabIndex = 4;
            // 
            // checkBox_AutoRefresh
            // 
            this.checkBox_AutoRefresh.Checked = true;
            this.checkBox_AutoRefresh.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBox_AutoRefresh.Dock = System.Windows.Forms.DockStyle.Fill;
            this.checkBox_AutoRefresh.Location = new System.Drawing.Point(0, 0);
            this.checkBox_AutoRefresh.Name = "checkBox_AutoRefresh";
            this.checkBox_AutoRefresh.Size = new System.Drawing.Size(114, 38);
            this.checkBox_AutoRefresh.TabIndex = 2;
            this.checkBox_AutoRefresh.Text = "Auto Refresh";
            this.checkBox_AutoRefresh.UseVisualStyleBackColor = true;
            // 
            // panel13
            // 
            this.panel13.Controls.Add(this.tableLayoutPanel3);
            this.panel13.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel13.Location = new System.Drawing.Point(3, 53);
            this.panel13.Name = "panel13";
            this.panel13.Size = new System.Drawing.Size(1327, 34);
            this.panel13.TabIndex = 7;
            // 
            // tableLayoutPanel3
            // 
            this.tableLayoutPanel3.ColumnCount = 10;
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 200F));
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 130F));
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 200F));
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 130F));
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 80F));
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 250F));
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 60F));
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 80F));
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel3.Controls.Add(this.panel14, 6, 0);
            this.tableLayoutPanel3.Controls.Add(this.panel2, 9, 0);
            this.tableLayoutPanel3.Controls.Add(this.panel3, 0, 0);
            this.tableLayoutPanel3.Controls.Add(this.panel4, 8, 0);
            this.tableLayoutPanel3.Controls.Add(this.panel5, 3, 0);
            this.tableLayoutPanel3.Controls.Add(this.panel7, 2, 0);
            this.tableLayoutPanel3.Controls.Add(this.panel15, 5, 0);
            this.tableLayoutPanel3.Controls.Add(this.panel16, 7, 0);
            this.tableLayoutPanel3.Controls.Add(this.panel17);
            this.tableLayoutPanel3.Controls.Add(this.panel18, 4, 0);
            this.tableLayoutPanel3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel3.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel3.Name = "tableLayoutPanel3";
            this.tableLayoutPanel3.RowCount = 1;
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel3.Size = new System.Drawing.Size(1327, 34);
            this.tableLayoutPanel3.TabIndex = 0;
            // 
            // panel14
            // 
            this.panel14.Controls.Add(this.comboBox_Equipment);
            this.panel14.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel14.Location = new System.Drawing.Point(763, 3);
            this.panel14.Name = "panel14";
            this.panel14.Size = new System.Drawing.Size(244, 28);
            this.panel14.TabIndex = 0;
            // 
            // comboBox_Equipment
            // 
            this.comboBox_Equipment.Dock = System.Windows.Forms.DockStyle.Fill;
            this.comboBox_Equipment.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBox_Equipment.FormattingEnabled = true;
            this.comboBox_Equipment.Location = new System.Drawing.Point(0, 0);
            this.comboBox_Equipment.Name = "comboBox_Equipment";
            this.comboBox_Equipment.Size = new System.Drawing.Size(244, 25);
            this.comboBox_Equipment.TabIndex = 0;
            // 
            // panel2
            // 
            this.panel2.Controls.Add(this.button_Refresh);
            this.panel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel2.Location = new System.Drawing.Point(1153, 3);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(171, 28);
            this.panel2.TabIndex = 1;
            // 
            // button_Refresh
            // 
            this.button_Refresh.Dock = System.Windows.Forms.DockStyle.Fill;
            this.button_Refresh.Location = new System.Drawing.Point(0, 0);
            this.button_Refresh.Name = "button_Refresh";
            this.button_Refresh.Size = new System.Drawing.Size(171, 28);
            this.button_Refresh.TabIndex = 3;
            this.button_Refresh.Text = "Inquire";
            this.button_Refresh.UseVisualStyleBackColor = true;
            this.button_Refresh.Click += new System.EventHandler(this.button_Refresh_Click);
            // 
            // panel3
            // 
            this.panel3.Controls.Add(this.dateTimePicker_Start);
            this.panel3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel3.Location = new System.Drawing.Point(3, 3);
            this.panel3.Name = "panel3";
            this.panel3.Size = new System.Drawing.Size(194, 28);
            this.panel3.TabIndex = 2;
            // 
            // dateTimePicker_Start
            // 
            this.dateTimePicker_Start.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dateTimePicker_Start.Location = new System.Drawing.Point(0, 0);
            this.dateTimePicker_Start.Name = "dateTimePicker_Start";
            this.dateTimePicker_Start.Size = new System.Drawing.Size(194, 25);
            this.dateTimePicker_Start.TabIndex = 1;
            // 
            // panel4
            // 
            this.panel4.Controls.Add(this.comboBox_Level);
            this.panel4.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel4.Location = new System.Drawing.Point(1073, 3);
            this.panel4.Name = "panel4";
            this.panel4.Size = new System.Drawing.Size(74, 28);
            this.panel4.TabIndex = 3;
            // 
            // comboBox_Level
            // 
            this.comboBox_Level.Dock = System.Windows.Forms.DockStyle.Fill;
            this.comboBox_Level.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBox_Level.FormattingEnabled = true;
            this.comboBox_Level.Location = new System.Drawing.Point(0, 0);
            this.comboBox_Level.Name = "comboBox_Level";
            this.comboBox_Level.Size = new System.Drawing.Size(74, 25);
            this.comboBox_Level.TabIndex = 0;
            // 
            // panel5
            // 
            this.panel5.Controls.Add(this.dateTimePicker_End);
            this.panel5.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel5.Location = new System.Drawing.Point(353, 3);
            this.panel5.Name = "panel5";
            this.panel5.Size = new System.Drawing.Size(194, 28);
            this.panel5.TabIndex = 4;
            // 
            // dateTimePicker_End
            // 
            this.dateTimePicker_End.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dateTimePicker_End.Location = new System.Drawing.Point(0, 0);
            this.dateTimePicker_End.Name = "dateTimePicker_End";
            this.dateTimePicker_End.Size = new System.Drawing.Size(194, 25);
            this.dateTimePicker_End.TabIndex = 0;
            // 
            // panel7
            // 
            this.panel7.Controls.Add(this.label3);
            this.panel7.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel7.Location = new System.Drawing.Point(333, 3);
            this.panel7.Name = "panel7";
            this.panel7.Size = new System.Drawing.Size(14, 28);
            this.panel7.TabIndex = 5;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label3.Location = new System.Drawing.Point(0, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(17, 17);
            this.label3.TabIndex = 0;
            this.label3.Text = "~";
            // 
            // panel15
            // 
            this.panel15.Controls.Add(this.label4);
            this.panel15.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel15.Location = new System.Drawing.Point(683, 3);
            this.panel15.Name = "panel15";
            this.panel15.Size = new System.Drawing.Size(74, 28);
            this.panel15.TabIndex = 6;
            // 
            // label4
            // 
            this.label4.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label4.Location = new System.Drawing.Point(0, 0);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(74, 28);
            this.label4.TabIndex = 5;
            this.label4.Text = "Equipment";
            this.label4.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // panel16
            // 
            this.panel16.Controls.Add(this.label5);
            this.panel16.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel16.Location = new System.Drawing.Point(1013, 3);
            this.panel16.Name = "panel16";
            this.panel16.Size = new System.Drawing.Size(54, 28);
            this.panel16.TabIndex = 7;
            // 
            // label5
            // 
            this.label5.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label5.Location = new System.Drawing.Point(0, 0);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(54, 28);
            this.label5.TabIndex = 6;
            this.label5.Text = "Level";
            this.label5.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // panel17
            // 
            this.panel17.Controls.Add(this.dateTimePicker_StartTime);
            this.panel17.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel17.Location = new System.Drawing.Point(203, 3);
            this.panel17.Name = "panel17";
            this.panel17.Size = new System.Drawing.Size(124, 28);
            this.panel17.TabIndex = 8;
            // 
            // dateTimePicker_StartTime
            // 
            this.dateTimePicker_StartTime.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dateTimePicker_StartTime.Format = System.Windows.Forms.DateTimePickerFormat.Time;
            this.dateTimePicker_StartTime.Location = new System.Drawing.Point(0, 0);
            this.dateTimePicker_StartTime.Name = "dateTimePicker_StartTime";
            this.dateTimePicker_StartTime.ShowUpDown = true;
            this.dateTimePicker_StartTime.Size = new System.Drawing.Size(124, 25);
            this.dateTimePicker_StartTime.TabIndex = 2;
            // 
            // panel18
            // 
            this.panel18.Controls.Add(this.dateTimePicker_EndTime);
            this.panel18.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel18.Location = new System.Drawing.Point(553, 3);
            this.panel18.Name = "panel18";
            this.panel18.Size = new System.Drawing.Size(124, 28);
            this.panel18.TabIndex = 9;
            // 
            // dateTimePicker_EndTime
            // 
            this.dateTimePicker_EndTime.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dateTimePicker_EndTime.Format = System.Windows.Forms.DateTimePickerFormat.Time;
            this.dateTimePicker_EndTime.Location = new System.Drawing.Point(0, 0);
            this.dateTimePicker_EndTime.Name = "dateTimePicker_EndTime";
            this.dateTimePicker_EndTime.ShowUpDown = true;
            this.dateTimePicker_EndTime.Size = new System.Drawing.Size(124, 25);
            this.dateTimePicker_EndTime.TabIndex = 3;
            // 
            // SystemLogView
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 17F);
            this.ClientSize = new System.Drawing.Size(1333, 834);
            this.Controls.Add(this.tableLayoutPanel1);
            this.Font = new System.Drawing.Font("맑은 고딕", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.Name = "SystemLogView";
            this.Text = "Monitor";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.PIAlarmMonitor_FormClosed);
            this.Load += new System.EventHandler(this.SystemLogView_Load);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.panel1.ResumeLayout(false);
            this.panel6.ResumeLayout(false);
            this.tableLayoutPanel2.ResumeLayout(false);
            this.panel8.ResumeLayout(false);
            this.panel9.ResumeLayout(false);
            this.panel10.ResumeLayout(false);
            this.panel10.PerformLayout();
            this.panel11.ResumeLayout(false);
            this.panel12.ResumeLayout(false);
            this.panel13.ResumeLayout(false);
            this.tableLayoutPanel3.ResumeLayout(false);
            this.panel14.ResumeLayout(false);
            this.panel2.ResumeLayout(false);
            this.panel3.ResumeLayout(false);
            this.panel4.ResumeLayout(false);
            this.panel5.ResumeLayout(false);
            this.panel7.ResumeLayout(false);
            this.panel7.PerformLayout();
            this.panel15.ResumeLayout(false);
            this.panel16.ResumeLayout(false);
            this.panel17.ResumeLayout(false);
            this.panel18.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.ListView listView_Main;
        private System.Windows.Forms.Panel panel6;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
        private System.Windows.Forms.Panel panel8;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Panel panel9;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Panel panel10;
        private System.Windows.Forms.TextBox textBox_RefreshInterval;
        private System.Windows.Forms.Panel panel11;
        private System.Windows.Forms.Button button_SetInterval;
        private System.Windows.Forms.Panel panel12;
        private System.Windows.Forms.CheckBox checkBox_AutoRefresh;
        private System.Windows.Forms.Panel panel13;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel3;
        private System.Windows.Forms.Panel panel14;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.Button button_Refresh;
        private System.Windows.Forms.Panel panel3;
        private System.Windows.Forms.Panel panel4;
        private System.Windows.Forms.ComboBox comboBox_Equipment;
        private System.Windows.Forms.DateTimePicker dateTimePicker_Start;
        private System.Windows.Forms.ComboBox comboBox_Level;
        private System.Windows.Forms.Panel panel5;
        private System.Windows.Forms.DateTimePicker dateTimePicker_End;
        private System.Windows.Forms.Panel panel7;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Panel panel15;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Panel panel16;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Panel panel17;
        private System.Windows.Forms.DateTimePicker dateTimePicker_StartTime;
        private System.Windows.Forms.Panel panel18;
        private System.Windows.Forms.DateTimePicker dateTimePicker_EndTime;
    }
}
