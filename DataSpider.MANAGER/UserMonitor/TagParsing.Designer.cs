
namespace DataSpider.UserMonitor
{
    partial class TagParsing
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
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle3 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            this.tableLayoutConfigtagGroupMain = new System.Windows.Forms.TableLayoutPanel();
            this.label_Title = new System.Windows.Forms.Label();
            this.tableLayoutContents = new System.Windows.Forms.TableLayoutPanel();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.panel1 = new System.Windows.Forms.Panel();
            this.dataGridView_Main = new System.Windows.Forms.DataGridView();
            this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.editToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.addToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.deleteToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
            this.tableLayoutPanel3 = new System.Windows.Forms.TableLayoutPanel();
            this.button_Parse = new System.Windows.Forms.Button();
            this.tableLayoutPanel4 = new System.Windows.Forms.TableLayoutPanel();
            this.panel2 = new System.Windows.Forms.Panel();
            this.label1 = new System.Windows.Forms.Label();
            this.tableLayoutPanel5 = new System.Windows.Forms.TableLayoutPanel();
            this.panel3 = new System.Windows.Forms.Panel();
            this.label2 = new System.Windows.Forms.Label();
            this.comboBoxMsgType = new System.Windows.Forms.ComboBox();
            this.dataGridEquipmentName = new System.Windows.Forms.DataGridView();
            this.EquipmentName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.textBox1 = new System.Windows.Forms.RichTextBox();
            this.tableLayoutConfigtagGroupMain.SuspendLayout();
            this.tableLayoutContents.SuspendLayout();
            this.tableLayoutPanel1.SuspendLayout();
            this.panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView_Main)).BeginInit();
            this.contextMenuStrip1.SuspendLayout();
            this.tableLayoutPanel2.SuspendLayout();
            this.tableLayoutPanel3.SuspendLayout();
            this.tableLayoutPanel4.SuspendLayout();
            this.panel2.SuspendLayout();
            this.tableLayoutPanel5.SuspendLayout();
            this.panel3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridEquipmentName)).BeginInit();
            this.SuspendLayout();
            // 
            // tableLayoutConfigtagGroupMain
            // 
            this.tableLayoutConfigtagGroupMain.ColumnCount = 1;
            this.tableLayoutConfigtagGroupMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutConfigtagGroupMain.Controls.Add(this.label_Title, 0, 0);
            this.tableLayoutConfigtagGroupMain.Controls.Add(this.tableLayoutContents, 0, 1);
            this.tableLayoutConfigtagGroupMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutConfigtagGroupMain.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutConfigtagGroupMain.Name = "tableLayoutConfigtagGroupMain";
            this.tableLayoutConfigtagGroupMain.RowCount = 2;
            this.tableLayoutConfigtagGroupMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 8.253359F));
            this.tableLayoutConfigtagGroupMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 91.74664F));
            this.tableLayoutConfigtagGroupMain.Size = new System.Drawing.Size(1563, 611);
            this.tableLayoutConfigtagGroupMain.TabIndex = 0;
            // 
            // label_Title
            // 
            this.label_Title.BackColor = System.Drawing.SystemColors.ActiveCaption;
            this.label_Title.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label_Title.Font = new System.Drawing.Font("맑은 고딕", 16F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.label_Title.Location = new System.Drawing.Point(0, 0);
            this.label_Title.Margin = new System.Windows.Forms.Padding(0);
            this.label_Title.Name = "label_Title";
            this.label_Title.Size = new System.Drawing.Size(1563, 50);
            this.label_Title.TabIndex = 1;
            this.label_Title.Text = "Tag Parsing";
            this.label_Title.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // tableLayoutContents
            // 
            this.tableLayoutContents.ColumnCount = 2;
            this.tableLayoutContents.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutContents.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 1353F));
            this.tableLayoutContents.Controls.Add(this.tableLayoutPanel1, 1, 0);
            this.tableLayoutContents.Controls.Add(this.dataGridEquipmentName, 0, 0);
            this.tableLayoutContents.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutContents.Location = new System.Drawing.Point(0, 50);
            this.tableLayoutContents.Margin = new System.Windows.Forms.Padding(0);
            this.tableLayoutContents.Name = "tableLayoutContents";
            this.tableLayoutContents.RowCount = 1;
            this.tableLayoutContents.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutContents.Size = new System.Drawing.Size(1563, 561);
            this.tableLayoutContents.TabIndex = 2;
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 1;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Controls.Add(this.panel1, 0, 0);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(213, 3);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 1;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 555F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 555F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 555F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 555F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 555F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(1347, 555);
            this.tableLayoutPanel1.TabIndex = 2;
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.dataGridView_Main);
            this.panel1.Controls.Add(this.tableLayoutPanel2);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel1.Location = new System.Drawing.Point(3, 3);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(1341, 549);
            this.panel1.TabIndex = 0;
            // 
            // dataGridView_Main
            // 
            this.dataGridView_Main.AllowUserToAddRows = false;
            this.dataGridView_Main.AllowUserToDeleteRows = false;
            dataGridViewCellStyle3.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(221)))), ((int)(((byte)(235)))), ((int)(((byte)(247)))));
            this.dataGridView_Main.AlternatingRowsDefaultCellStyle = dataGridViewCellStyle3;
            this.dataGridView_Main.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridView_Main.ContextMenuStrip = this.contextMenuStrip1;
            this.dataGridView_Main.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dataGridView_Main.Location = new System.Drawing.Point(373, 0);
            this.dataGridView_Main.Name = "dataGridView_Main";
            this.dataGridView_Main.ReadOnly = true;
            this.dataGridView_Main.RowHeadersVisible = false;
            this.dataGridView_Main.RowHeadersWidth = 51;
            this.dataGridView_Main.RowTemplate.Height = 23;
            this.dataGridView_Main.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dataGridView_Main.Size = new System.Drawing.Size(968, 549);
            this.dataGridView_Main.TabIndex = 2;
            // 
            // contextMenuStrip1
            // 
            this.contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.editToolStripMenuItem,
            this.addToolStripMenuItem,
            this.deleteToolStripMenuItem});
            this.contextMenuStrip1.Name = "contextMenuStrip1";
            this.contextMenuStrip1.Size = new System.Drawing.Size(109, 70);
            // 
            // editToolStripMenuItem
            // 
            this.editToolStripMenuItem.Name = "editToolStripMenuItem";
            this.editToolStripMenuItem.Size = new System.Drawing.Size(108, 22);
            this.editToolStripMenuItem.Text = "Edit";
            this.editToolStripMenuItem.Click += new System.EventHandler(this.EditToolStripMenuItem_Click);
            // 
            // addToolStripMenuItem
            // 
            this.addToolStripMenuItem.Name = "addToolStripMenuItem";
            this.addToolStripMenuItem.Size = new System.Drawing.Size(108, 22);
            this.addToolStripMenuItem.Text = "Add";
            this.addToolStripMenuItem.Click += new System.EventHandler(this.AddToolStripMenuItem_Click);
            // 
            // deleteToolStripMenuItem
            // 
            this.deleteToolStripMenuItem.Name = "deleteToolStripMenuItem";
            this.deleteToolStripMenuItem.Size = new System.Drawing.Size(108, 22);
            this.deleteToolStripMenuItem.Text = "Delete";
            this.deleteToolStripMenuItem.Click += new System.EventHandler(this.DeleteToolStripMenuItem_Click);
            // 
            // tableLayoutPanel2
            // 
            this.tableLayoutPanel2.ColumnCount = 1;
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel2.Controls.Add(this.tableLayoutPanel3, 0, 2);
            this.tableLayoutPanel2.Controls.Add(this.tableLayoutPanel4, 0, 1);
            this.tableLayoutPanel2.Controls.Add(this.tableLayoutPanel5, 0, 0);
            this.tableLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Left;
            this.tableLayoutPanel2.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel2.Name = "tableLayoutPanel2";
            this.tableLayoutPanel2.RowCount = 3;
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 15F));
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 85F));
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 45F));
            this.tableLayoutPanel2.Size = new System.Drawing.Size(373, 549);
            this.tableLayoutPanel2.TabIndex = 0;
            // 
            // tableLayoutPanel3
            // 
            this.tableLayoutPanel3.ColumnCount = 2;
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 68.19222F));
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 31.80778F));
            this.tableLayoutPanel3.Controls.Add(this.button_Parse, 1, 0);
            this.tableLayoutPanel3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel3.Location = new System.Drawing.Point(3, 506);
            this.tableLayoutPanel3.Name = "tableLayoutPanel3";
            this.tableLayoutPanel3.RowCount = 1;
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel3.Size = new System.Drawing.Size(367, 40);
            this.tableLayoutPanel3.TabIndex = 13;
            // 
            // button_Parse
            // 
            this.button_Parse.Dock = System.Windows.Forms.DockStyle.Fill;
            this.button_Parse.Location = new System.Drawing.Point(253, 3);
            this.button_Parse.Name = "button_Parse";
            this.button_Parse.Size = new System.Drawing.Size(111, 34);
            this.button_Parse.TabIndex = 0;
            this.button_Parse.Text = "파싱";
            this.button_Parse.UseVisualStyleBackColor = true;
            this.button_Parse.Click += new System.EventHandler(this.button_Parse_Click);
            // 
            // tableLayoutPanel4
            // 
            this.tableLayoutPanel4.ColumnCount = 1;
            this.tableLayoutPanel4.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel4.Controls.Add(this.textBox1, 0, 1);
            this.tableLayoutPanel4.Controls.Add(this.panel2, 0, 0);
            this.tableLayoutPanel4.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel4.Location = new System.Drawing.Point(3, 78);
            this.tableLayoutPanel4.Name = "tableLayoutPanel4";
            this.tableLayoutPanel4.RowCount = 2;
            this.tableLayoutPanel4.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 7F));
            this.tableLayoutPanel4.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 93F));
            this.tableLayoutPanel4.Size = new System.Drawing.Size(367, 422);
            this.tableLayoutPanel4.TabIndex = 14;
            // 
            // panel2
            // 
            this.panel2.Controls.Add(this.label1);
            this.panel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel2.Location = new System.Drawing.Point(3, 3);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(361, 23);
            this.panel2.TabIndex = 13;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Dock = System.Windows.Forms.DockStyle.Left;
            this.label1.Font = new System.Drawing.Font("굴림", 12F);
            this.label1.Location = new System.Drawing.Point(0, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(39, 16);
            this.label1.TabIndex = 0;
            this.label1.Text = "포맷";
            // 
            // tableLayoutPanel5
            // 
            this.tableLayoutPanel5.ColumnCount = 2;
            this.tableLayoutPanel5.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 40F));
            this.tableLayoutPanel5.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 60F));
            this.tableLayoutPanel5.Controls.Add(this.panel3, 0, 0);
            this.tableLayoutPanel5.Controls.Add(this.comboBoxMsgType, 0, 1);
            this.tableLayoutPanel5.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel5.Location = new System.Drawing.Point(3, 3);
            this.tableLayoutPanel5.Name = "tableLayoutPanel5";
            this.tableLayoutPanel5.RowCount = 2;
            this.tableLayoutPanel5.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 42.02898F));
            this.tableLayoutPanel5.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 57.97102F));
            this.tableLayoutPanel5.Size = new System.Drawing.Size(367, 69);
            this.tableLayoutPanel5.TabIndex = 15;
            // 
            // panel3
            // 
            this.panel3.Controls.Add(this.label2);
            this.panel3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel3.Location = new System.Drawing.Point(3, 3);
            this.panel3.Name = "panel3";
            this.panel3.Size = new System.Drawing.Size(140, 22);
            this.panel3.TabIndex = 0;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Dock = System.Windows.Forms.DockStyle.Left;
            this.label2.Font = new System.Drawing.Font("굴림", 12F);
            this.label2.Location = new System.Drawing.Point(0, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(114, 16);
            this.label2.TabIndex = 0;
            this.label2.Text = "Message Type";
            // 
            // comboBoxMsgType
            // 
            this.comboBoxMsgType.Dock = System.Windows.Forms.DockStyle.Top;
            this.comboBoxMsgType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxMsgType.Font = new System.Drawing.Font("굴림", 12F);
            this.comboBoxMsgType.FormattingEnabled = true;
            this.comboBoxMsgType.Location = new System.Drawing.Point(3, 31);
            this.comboBoxMsgType.Name = "comboBoxMsgType";
            this.comboBoxMsgType.Size = new System.Drawing.Size(140, 24);
            this.comboBoxMsgType.TabIndex = 1;
            this.comboBoxMsgType.SelectedIndexChanged += new System.EventHandler(this.comboBoxMsgType_SelectedIndexChanged);
            // 
            // dataGridEquipmentName
            // 
            this.dataGridEquipmentName.AllowUserToAddRows = false;
            this.dataGridEquipmentName.AllowUserToDeleteRows = false;
            this.dataGridEquipmentName.AutoSizeRowsMode = System.Windows.Forms.DataGridViewAutoSizeRowsMode.DisplayedCells;
            dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle1.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle1.Font = new System.Drawing.Font("굴림", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            dataGridViewCellStyle1.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle1.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle1.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle1.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dataGridEquipmentName.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle1;
            this.dataGridEquipmentName.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridEquipmentName.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.EquipmentName});
            this.dataGridEquipmentName.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dataGridEquipmentName.Location = new System.Drawing.Point(3, 3);
            this.dataGridEquipmentName.Name = "dataGridEquipmentName";
            this.dataGridEquipmentName.ReadOnly = true;
            this.dataGridEquipmentName.RowHeadersVisible = false;
            this.dataGridEquipmentName.RowTemplate.Height = 23;
            this.dataGridEquipmentName.Size = new System.Drawing.Size(204, 555);
            this.dataGridEquipmentName.TabIndex = 3;
            this.dataGridEquipmentName.CellClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dataGridEquipmentName_CellClick);
            // 
            // EquipmentName
            // 
            this.EquipmentName.HeaderText = "Equipment Name";
            this.EquipmentName.Name = "EquipmentName";
            this.EquipmentName.ReadOnly = true;
            this.EquipmentName.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            this.EquipmentName.Width = 200;
            // 
            // textBox1
            // 
            this.textBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.textBox1.Font = new System.Drawing.Font("굴림", 12F);
            this.textBox1.Location = new System.Drawing.Point(3, 32);
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(361, 387);
            this.textBox1.TabIndex = 3;
            this.textBox1.Text = "";
            // 
            // TagParsing
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1563, 611);
            this.Controls.Add(this.tableLayoutConfigtagGroupMain);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "TagParsing";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Tag Parsing";
            this.Load += new System.EventHandler(this.DateTimeParsing_Load);
            this.tableLayoutConfigtagGroupMain.ResumeLayout(false);
            this.tableLayoutContents.ResumeLayout(false);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.panel1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView_Main)).EndInit();
            this.contextMenuStrip1.ResumeLayout(false);
            this.tableLayoutPanel2.ResumeLayout(false);
            this.tableLayoutPanel3.ResumeLayout(false);
            this.tableLayoutPanel4.ResumeLayout(false);
            this.panel2.ResumeLayout(false);
            this.panel2.PerformLayout();
            this.tableLayoutPanel5.ResumeLayout(false);
            this.panel3.ResumeLayout(false);
            this.panel3.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridEquipmentName)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayoutConfigtagGroupMain;
        private System.Windows.Forms.Label label_Title;
        private System.Windows.Forms.TableLayoutPanel tableLayoutContents;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.DataGridView dataGridEquipmentName;
        private System.Windows.Forms.DataGridViewTextBoxColumn EquipmentName;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel3;
        private System.Windows.Forms.Button button_Parse;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel4;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel5;
        private System.Windows.Forms.Panel panel3;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ComboBox comboBoxMsgType;
        private System.Windows.Forms.DataGridView dataGridView_Main;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
        private System.Windows.Forms.ToolStripMenuItem editToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem addToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem deleteToolStripMenuItem;
        private System.Windows.Forms.RichTextBox textBox1;
    }
}