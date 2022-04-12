
namespace DataSpider
{
    partial class TAGValueHsitoryForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(TAGValueHsitoryForm));
            this.comboBoxTagGroupSel = new System.Windows.Forms.ComboBox();
            this.tableLayoutHistoryTagValueMenu = new System.Windows.Forms.TableLayoutPanel();
            this.panelTagHistoryValue = new System.Windows.Forms.Panel();
            this.dataGridView_Main = new System.Windows.Forms.DataGridView();
            this.panelHistoryTagValueViewTopPanel = new System.Windows.Forms.Panel();
            this.labelHistoryTagValueView = new System.Windows.Forms.Label();
            this.toolStripMenuItemData = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItemLog = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.deleteToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.addToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.editToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.valueHistoryToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.imageList1 = new System.Windows.Forms.ImageList(this.components);
            this.imageList2 = new System.Windows.Forms.ImageList(this.components);
            this.tableLayoutHistoryTagValueMenu.SuspendLayout();
            this.panelTagHistoryValue.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView_Main)).BeginInit();
            this.panelHistoryTagValueViewTopPanel.SuspendLayout();
            this.contextMenuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // comboBoxTagGroupSel
            // 
            this.comboBoxTagGroupSel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.comboBoxTagGroupSel.FormattingEnabled = true;
            this.comboBoxTagGroupSel.Location = new System.Drawing.Point(665, 11);
            this.comboBoxTagGroupSel.Margin = new System.Windows.Forms.Padding(3, 11, 3, 3);
            this.comboBoxTagGroupSel.MaxDropDownItems = 24;
            this.comboBoxTagGroupSel.Name = "comboBoxTagGroupSel";
            this.comboBoxTagGroupSel.Size = new System.Drawing.Size(244, 20);
            this.comboBoxTagGroupSel.TabIndex = 8;
            this.comboBoxTagGroupSel.Text = "Tag Group (All)";
            // 
            // tableLayoutHistoryTagValueMenu
            // 
            this.tableLayoutHistoryTagValueMenu.ColumnCount = 3;
            this.tableLayoutHistoryTagValueMenu.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutHistoryTagValueMenu.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 250F));
            this.tableLayoutHistoryTagValueMenu.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 100F));
            this.tableLayoutHistoryTagValueMenu.Controls.Add(this.panelTagHistoryValue, 0, 1);
            this.tableLayoutHistoryTagValueMenu.Controls.Add(this.panelHistoryTagValueViewTopPanel, 0, 0);
            this.tableLayoutHistoryTagValueMenu.Controls.Add(this.comboBoxTagGroupSel, 1, 0);
            this.tableLayoutHistoryTagValueMenu.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutHistoryTagValueMenu.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutHistoryTagValueMenu.Name = "tableLayoutHistoryTagValueMenu";
            this.tableLayoutHistoryTagValueMenu.RowCount = 2;
            this.tableLayoutHistoryTagValueMenu.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 40F));
            this.tableLayoutHistoryTagValueMenu.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutHistoryTagValueMenu.Size = new System.Drawing.Size(1012, 516);
            this.tableLayoutHistoryTagValueMenu.TabIndex = 2;
            // 
            // panelTagHistoryValue
            // 
            this.tableLayoutHistoryTagValueMenu.SetColumnSpan(this.panelTagHistoryValue, 3);
            this.panelTagHistoryValue.Controls.Add(this.dataGridView_Main);
            this.panelTagHistoryValue.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelTagHistoryValue.Location = new System.Drawing.Point(3, 43);
            this.panelTagHistoryValue.Name = "panelTagHistoryValue";
            this.panelTagHistoryValue.Size = new System.Drawing.Size(1006, 470);
            this.panelTagHistoryValue.TabIndex = 0;
            // 
            // dataGridView_Main
            // 
            this.dataGridView_Main.AllowUserToAddRows = false;
            this.dataGridView_Main.AllowUserToDeleteRows = false;
            dataGridViewCellStyle1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(221)))), ((int)(((byte)(235)))), ((int)(((byte)(247)))));
            this.dataGridView_Main.AlternatingRowsDefaultCellStyle = dataGridViewCellStyle1;
            this.dataGridView_Main.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.AllCells;
            this.dataGridView_Main.AutoSizeRowsMode = System.Windows.Forms.DataGridViewAutoSizeRowsMode.DisplayedCellsExceptHeaders;
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
            this.dataGridView_Main.Size = new System.Drawing.Size(1006, 470);
            this.dataGridView_Main.TabIndex = 1;
            // 
            // panelHistoryTagValueViewTopPanel
            // 
            this.panelHistoryTagValueViewTopPanel.Controls.Add(this.labelHistoryTagValueView);
            this.panelHistoryTagValueViewTopPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelHistoryTagValueViewTopPanel.Location = new System.Drawing.Point(3, 3);
            this.panelHistoryTagValueViewTopPanel.Name = "panelHistoryTagValueViewTopPanel";
            this.panelHistoryTagValueViewTopPanel.Size = new System.Drawing.Size(656, 34);
            this.panelHistoryTagValueViewTopPanel.TabIndex = 3;
            // 
            // labelHistoryTagValueView
            // 
            this.labelHistoryTagValueView.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.labelHistoryTagValueView.Font = new System.Drawing.Font("맑은 고딕", 16F);
            this.labelHistoryTagValueView.Location = new System.Drawing.Point(0, 0);
            this.labelHistoryTagValueView.Name = "labelHistoryTagValueView";
            this.labelHistoryTagValueView.Size = new System.Drawing.Size(656, 34);
            this.labelHistoryTagValueView.TabIndex = 3;
            this.labelHistoryTagValueView.Text = "TAG History Value View";
            this.labelHistoryTagValueView.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // toolStripMenuItemData
            // 
            this.toolStripMenuItemData.Name = "toolStripMenuItemData";
            this.toolStripMenuItemData.Size = new System.Drawing.Size(146, 22);
            this.toolStripMenuItemData.Text = "Data";
            // 
            // toolStripMenuItemLog
            // 
            this.toolStripMenuItemLog.Name = "toolStripMenuItemLog";
            this.toolStripMenuItemLog.Size = new System.Drawing.Size(146, 22);
            this.toolStripMenuItemLog.Text = "Log";
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(143, 6);
            // 
            // deleteToolStripMenuItem
            // 
            this.deleteToolStripMenuItem.Name = "deleteToolStripMenuItem";
            this.deleteToolStripMenuItem.Size = new System.Drawing.Size(146, 22);
            this.deleteToolStripMenuItem.Text = "Delete";
            // 
            // addToolStripMenuItem
            // 
            this.addToolStripMenuItem.Name = "addToolStripMenuItem";
            this.addToolStripMenuItem.Size = new System.Drawing.Size(146, 22);
            this.addToolStripMenuItem.Text = "Add";
            // 
            // editToolStripMenuItem
            // 
            this.editToolStripMenuItem.Name = "editToolStripMenuItem";
            this.editToolStripMenuItem.Size = new System.Drawing.Size(146, 22);
            this.editToolStripMenuItem.Text = "Edit";
            // 
            // valueHistoryToolStripMenuItem
            // 
            this.valueHistoryToolStripMenuItem.Name = "valueHistoryToolStripMenuItem";
            this.valueHistoryToolStripMenuItem.Size = new System.Drawing.Size(146, 22);
            this.valueHistoryToolStripMenuItem.Text = "Value History";
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
            // TAGValueHsitoryForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1012, 516);
            this.Controls.Add(this.tableLayoutHistoryTagValueMenu);
            this.Name = "TAGValueHsitoryForm";
            this.Text = "TAGValueHsitoryForm";
            this.tableLayoutHistoryTagValueMenu.ResumeLayout(false);
            this.panelTagHistoryValue.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView_Main)).EndInit();
            this.panelHistoryTagValueViewTopPanel.ResumeLayout(false);
            this.contextMenuStrip1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ComboBox comboBoxTagGroupSel;
        private System.Windows.Forms.TableLayoutPanel tableLayoutHistoryTagValueMenu;
        private System.Windows.Forms.Panel panelTagHistoryValue;
        private System.Windows.Forms.DataGridView dataGridView_Main;
        private System.Windows.Forms.Panel panelHistoryTagValueViewTopPanel;
        private System.Windows.Forms.Label labelHistoryTagValueView;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemData;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemLog;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripMenuItem deleteToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem addToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem editToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem valueHistoryToolStripMenuItem;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
        private System.Windows.Forms.ImageList imageList1;
        private System.Windows.Forms.ImageList imageList2;
    }
}