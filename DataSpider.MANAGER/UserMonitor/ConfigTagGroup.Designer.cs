
namespace DataSpider.UserMonitor
{
    partial class ConfigTagGroup
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
            this.comboBoxGroupSel = new System.Windows.Forms.ComboBox();
            this.comboBox_EquipType = new System.Windows.Forms.ComboBox();
            this.checkedLBoxTagList = new System.Windows.Forms.CheckedListBox();
            this.labelGroupDesc = new System.Windows.Forms.Label();
            this.labelBTAdd = new System.Windows.Forms.Label();
            this.labelBTEdit = new System.Windows.Forms.Label();
            this.labelBTRemove = new System.Windows.Forms.Label();
            this.labelBTSelectAll = new System.Windows.Forms.Label();
            this.labelBTDeselectAll = new System.Windows.Forms.Label();
            this.labelTitle = new System.Windows.Forms.Label();
            this.labelBTExit = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // comboBoxGroupSel
            // 
            this.comboBoxGroupSel.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxGroupSel.Font = new System.Drawing.Font("맑은 고딕", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.comboBoxGroupSel.FormattingEnabled = true;
            this.comboBoxGroupSel.Location = new System.Drawing.Point(154, 124);
            this.comboBoxGroupSel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 3);
            this.comboBoxGroupSel.Name = "comboBoxGroupSel";
            this.comboBoxGroupSel.Size = new System.Drawing.Size(563, 29);
            this.comboBoxGroupSel.TabIndex = 3;
            this.comboBoxGroupSel.SelectedIndexChanged += new System.EventHandler(this.comboBoxGroupSel_SelectedIndexChanged);
            // 
            // comboBox_EquipType
            // 
            this.comboBox_EquipType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBox_EquipType.Font = new System.Drawing.Font("맑은 고딕", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.comboBox_EquipType.FormattingEnabled = true;
            this.comboBox_EquipType.Location = new System.Drawing.Point(154, 76);
            this.comboBox_EquipType.Margin = new System.Windows.Forms.Padding(3, 6, 3, 3);
            this.comboBox_EquipType.Name = "comboBox_EquipType";
            this.comboBox_EquipType.Size = new System.Drawing.Size(563, 29);
            this.comboBox_EquipType.TabIndex = 2;
            this.comboBox_EquipType.SelectedIndexChanged += new System.EventHandler(this.comboBox_EquipType_SelectedIndexChanged);
            // 
            // checkedLBoxTagList
            // 
            this.checkedLBoxTagList.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.checkedLBoxTagList.Font = new System.Drawing.Font("맑은 고딕", 12F);
            this.checkedLBoxTagList.FormattingEnabled = true;
            this.checkedLBoxTagList.Location = new System.Drawing.Point(22, 197);
            this.checkedLBoxTagList.Margin = new System.Windows.Forms.Padding(0);
            this.checkedLBoxTagList.Name = "checkedLBoxTagList";
            this.checkedLBoxTagList.Size = new System.Drawing.Size(539, 336);
            this.checkedLBoxTagList.TabIndex = 0;
            // 
            // labelGroupDesc
            // 
            this.labelGroupDesc.BackColor = System.Drawing.Color.Transparent;
            this.labelGroupDesc.Font = new System.Drawing.Font("맑은 고딕", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.labelGroupDesc.ForeColor = System.Drawing.Color.White;
            this.labelGroupDesc.Location = new System.Drawing.Point(154, 156);
            this.labelGroupDesc.Name = "labelGroupDesc";
            this.labelGroupDesc.Size = new System.Drawing.Size(563, 26);
            this.labelGroupDesc.TabIndex = 9;
            this.labelGroupDesc.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // labelBTAdd
            // 
            this.labelBTAdd.BackColor = System.Drawing.Color.Transparent;
            this.labelBTAdd.Font = new System.Drawing.Font("Tahoma", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.labelBTAdd.ForeColor = System.Drawing.Color.White;
            this.labelBTAdd.Location = new System.Drawing.Point(581, 197);
            this.labelBTAdd.Margin = new System.Windows.Forms.Padding(0);
            this.labelBTAdd.Name = "labelBTAdd";
            this.labelBTAdd.Size = new System.Drawing.Size(138, 27);
            this.labelBTAdd.TabIndex = 308;
            this.labelBTAdd.Text = "Add";
            this.labelBTAdd.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.labelBTAdd.Click += new System.EventHandler(this.labelBTAdd_Click);
            // 
            // labelBTEdit
            // 
            this.labelBTEdit.BackColor = System.Drawing.Color.Transparent;
            this.labelBTEdit.Font = new System.Drawing.Font("Tahoma", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.labelBTEdit.ForeColor = System.Drawing.Color.White;
            this.labelBTEdit.Location = new System.Drawing.Point(581, 240);
            this.labelBTEdit.Margin = new System.Windows.Forms.Padding(0);
            this.labelBTEdit.Name = "labelBTEdit";
            this.labelBTEdit.Size = new System.Drawing.Size(138, 27);
            this.labelBTEdit.TabIndex = 309;
            this.labelBTEdit.Text = "Edit";
            this.labelBTEdit.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.labelBTEdit.Click += new System.EventHandler(this.labelBTEdit_Click);
            // 
            // labelBTRemove
            // 
            this.labelBTRemove.BackColor = System.Drawing.Color.Transparent;
            this.labelBTRemove.Font = new System.Drawing.Font("Tahoma", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.labelBTRemove.ForeColor = System.Drawing.Color.White;
            this.labelBTRemove.Location = new System.Drawing.Point(581, 284);
            this.labelBTRemove.Margin = new System.Windows.Forms.Padding(0);
            this.labelBTRemove.Name = "labelBTRemove";
            this.labelBTRemove.Size = new System.Drawing.Size(138, 27);
            this.labelBTRemove.TabIndex = 310;
            this.labelBTRemove.Text = "Remove";
            this.labelBTRemove.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.labelBTRemove.Click += new System.EventHandler(this.labelBTRemove_Click);
            // 
            // labelBTSelectAll
            // 
            this.labelBTSelectAll.BackColor = System.Drawing.Color.Transparent;
            this.labelBTSelectAll.Font = new System.Drawing.Font("Tahoma", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.labelBTSelectAll.ForeColor = System.Drawing.Color.White;
            this.labelBTSelectAll.Location = new System.Drawing.Point(582, 467);
            this.labelBTSelectAll.Margin = new System.Windows.Forms.Padding(0);
            this.labelBTSelectAll.Name = "labelBTSelectAll";
            this.labelBTSelectAll.Size = new System.Drawing.Size(137, 27);
            this.labelBTSelectAll.TabIndex = 311;
            this.labelBTSelectAll.Text = "Select All";
            this.labelBTSelectAll.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.labelBTSelectAll.Click += new System.EventHandler(this.labelBTSelectAll_Click);
            // 
            // labelBTDeselectAll
            // 
            this.labelBTDeselectAll.BackColor = System.Drawing.Color.Transparent;
            this.labelBTDeselectAll.Font = new System.Drawing.Font("Tahoma", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.labelBTDeselectAll.ForeColor = System.Drawing.Color.White;
            this.labelBTDeselectAll.Location = new System.Drawing.Point(583, 512);
            this.labelBTDeselectAll.Margin = new System.Windows.Forms.Padding(0);
            this.labelBTDeselectAll.Name = "labelBTDeselectAll";
            this.labelBTDeselectAll.Size = new System.Drawing.Size(137, 27);
            this.labelBTDeselectAll.TabIndex = 312;
            this.labelBTDeselectAll.Text = "Deselect All";
            this.labelBTDeselectAll.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.labelBTDeselectAll.Click += new System.EventHandler(this.labelBTDeselectAll_Click);
            // 
            // labelTitle
            // 
            this.labelTitle.BackColor = System.Drawing.Color.Transparent;
            this.labelTitle.Font = new System.Drawing.Font("Tahoma", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.labelTitle.ForeColor = System.Drawing.Color.White;
            this.labelTitle.Location = new System.Drawing.Point(18, 9);
            this.labelTitle.Margin = new System.Windows.Forms.Padding(0);
            this.labelTitle.Name = "labelTitle";
            this.labelTitle.Size = new System.Drawing.Size(659, 48);
            this.labelTitle.TabIndex = 313;
            this.labelTitle.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.labelTitle.MouseDown += new System.Windows.Forms.MouseEventHandler(this.labelTitle_MouseDown);
            // 
            // labelBTExit
            // 
            this.labelBTExit.BackColor = System.Drawing.Color.Transparent;
            this.labelBTExit.Font = new System.Drawing.Font("Tahoma", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.labelBTExit.ForeColor = System.Drawing.Color.White;
            this.labelBTExit.Location = new System.Drawing.Point(677, 9);
            this.labelBTExit.Margin = new System.Windows.Forms.Padding(0);
            this.labelBTExit.Name = "labelBTExit";
            this.labelBTExit.Size = new System.Drawing.Size(51, 48);
            this.labelBTExit.TabIndex = 314;
            this.labelBTExit.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.labelBTExit.Click += new System.EventHandler(this.labelBTExit_Click);
            // 
            // ConfigTagGroup
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackgroundImage = global::DataSpider.Properties.Resources.TagGroupConfig;
            this.ClientSize = new System.Drawing.Size(737, 565);
            this.Controls.Add(this.labelBTExit);
            this.Controls.Add(this.labelTitle);
            this.Controls.Add(this.labelBTDeselectAll);
            this.Controls.Add(this.labelBTSelectAll);
            this.Controls.Add(this.labelBTRemove);
            this.Controls.Add(this.labelBTEdit);
            this.Controls.Add(this.labelBTAdd);
            this.Controls.Add(this.labelGroupDesc);
            this.Controls.Add(this.comboBoxGroupSel);
            this.Controls.Add(this.checkedLBoxTagList);
            this.Controls.Add(this.comboBox_EquipType);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "ConfigTagGroup";
            this.Text = "ConfigTagGroup";
            this.Load += new System.EventHandler(this.ConfigTagGroup_Load);
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.ComboBox comboBox_EquipType;
        private System.Windows.Forms.CheckedListBox checkedLBoxTagList;
        private System.Windows.Forms.ComboBox comboBoxGroupSel;
        private System.Windows.Forms.Label labelGroupDesc;
        private System.Windows.Forms.Label labelBTAdd;
        private System.Windows.Forms.Label labelBTEdit;
        private System.Windows.Forms.Label labelBTRemove;
        private System.Windows.Forms.Label labelBTSelectAll;
        private System.Windows.Forms.Label labelBTDeselectAll;
        private System.Windows.Forms.Label labelTitle;
        private System.Windows.Forms.Label labelBTExit;
    }
}