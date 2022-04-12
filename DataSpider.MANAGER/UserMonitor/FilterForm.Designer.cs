
namespace DataSpider.UserMonitor
{
    partial class FilterForm
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
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.dateTimePickerTimeMax = new System.Windows.Forms.DateTimePicker();
            this.dateTimePickerDateMax = new System.Windows.Forms.DateTimePicker();
            this.textBoxDateTimeSelMax = new System.Windows.Forms.TextBox();
            this.labelTimeFilterMax = new System.Windows.Forms.Label();
            this.textBoxDateTimeSelMin = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.textBoxTagNameFilter = new System.Windows.Forms.TextBox();
            this.labelTimeFilterMin = new System.Windows.Forms.Label();
            this.dateTimePickerDateMin = new System.Windows.Forms.DateTimePicker();
            this.dateTimePickerTimeMin = new System.Windows.Forms.DateTimePicker();
            this.labelDesc = new System.Windows.Forms.Label();
            this.textBoxDescriptionFilter = new System.Windows.Forms.TextBox();
            this.buttonInitialize = new System.Windows.Forms.Button();
            this.buttonApply = new System.Windows.Forms.Button();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.tableLayoutPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 2;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 120F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Controls.Add(this.dateTimePickerTimeMax, 1, 6);
            this.tableLayoutPanel1.Controls.Add(this.dateTimePickerDateMax, 1, 5);
            this.tableLayoutPanel1.Controls.Add(this.textBoxDateTimeSelMax, 1, 4);
            this.tableLayoutPanel1.Controls.Add(this.labelTimeFilterMax, 0, 4);
            this.tableLayoutPanel1.Controls.Add(this.textBoxDateTimeSelMin, 1, 1);
            this.tableLayoutPanel1.Controls.Add(this.label3, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.textBoxTagNameFilter, 1, 0);
            this.tableLayoutPanel1.Controls.Add(this.labelTimeFilterMin, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.dateTimePickerDateMin, 1, 2);
            this.tableLayoutPanel1.Controls.Add(this.dateTimePickerTimeMin, 1, 3);
            this.tableLayoutPanel1.Controls.Add(this.labelDesc, 0, 7);
            this.tableLayoutPanel1.Controls.Add(this.textBoxDescriptionFilter, 1, 7);
            this.tableLayoutPanel1.Controls.Add(this.buttonInitialize, 1, 8);
            this.tableLayoutPanel1.Controls.Add(this.buttonApply, 1, 9);
            this.tableLayoutPanel1.Controls.Add(this.buttonCancel, 1, 10);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 11;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 32F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 32F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 32F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 32F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 33F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 31F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 32F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(319, 305);
            this.tableLayoutPanel1.TabIndex = 2;
            // 
            // dateTimePickerTimeMax
            // 
            this.dateTimePickerTimeMax.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dateTimePickerTimeMax.Format = System.Windows.Forms.DateTimePickerFormat.Time;
            this.dateTimePickerTimeMax.Location = new System.Drawing.Point(123, 156);
            this.dateTimePickerTimeMax.Margin = new System.Windows.Forms.Padding(3, 0, 3, 0);
            this.dateTimePickerTimeMax.Name = "dateTimePickerTimeMax";
            this.dateTimePickerTimeMax.Size = new System.Drawing.Size(193, 21);
            this.dateTimePickerTimeMax.TabIndex = 20;
            this.dateTimePickerTimeMax.ValueChanged += new System.EventHandler(this.dateTimePickerTimeMax_ValueChanged);
            // 
            // dateTimePickerDateMax
            // 
            this.dateTimePickerDateMax.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dateTimePickerDateMax.Location = new System.Drawing.Point(123, 136);
            this.dateTimePickerDateMax.Margin = new System.Windows.Forms.Padding(3, 0, 3, 0);
            this.dateTimePickerDateMax.Name = "dateTimePickerDateMax";
            this.dateTimePickerDateMax.Size = new System.Drawing.Size(193, 21);
            this.dateTimePickerDateMax.TabIndex = 19;
            this.dateTimePickerDateMax.ValueChanged += new System.EventHandler(this.dateTimePickerDateMax_ValueChanged);
            // 
            // textBoxDateTimeSelMax
            // 
            this.textBoxDateTimeSelMax.Dock = System.Windows.Forms.DockStyle.Fill;
            this.textBoxDateTimeSelMax.Location = new System.Drawing.Point(123, 110);
            this.textBoxDateTimeSelMax.Margin = new System.Windows.Forms.Padding(3, 6, 3, 3);
            this.textBoxDateTimeSelMax.Name = "textBoxDateTimeSelMax";
            this.textBoxDateTimeSelMax.Size = new System.Drawing.Size(193, 21);
            this.textBoxDateTimeSelMax.TabIndex = 18;
            this.textBoxDateTimeSelMax.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.textBoxDateTimeSelMax.TextChanged += new System.EventHandler(this.textBoxDateTimeSelMax_TextChanged);
            // 
            // labelTimeFilterMax
            // 
            this.labelTimeFilterMax.AutoSize = true;
            this.labelTimeFilterMax.Dock = System.Windows.Forms.DockStyle.Right;
            this.labelTimeFilterMax.Location = new System.Drawing.Point(19, 104);
            this.labelTimeFilterMax.Name = "labelTimeFilterMax";
            this.labelTimeFilterMax.Size = new System.Drawing.Size(98, 32);
            this.labelTimeFilterMax.TabIndex = 17;
            this.labelTimeFilterMax.Text = "Date/Time Max:";
            this.labelTimeFilterMax.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // textBoxDateTimeSelMin
            // 
            this.textBoxDateTimeSelMin.Dock = System.Windows.Forms.DockStyle.Fill;
            this.textBoxDateTimeSelMin.Location = new System.Drawing.Point(123, 38);
            this.textBoxDateTimeSelMin.Margin = new System.Windows.Forms.Padding(3, 6, 3, 3);
            this.textBoxDateTimeSelMin.Name = "textBoxDateTimeSelMin";
            this.textBoxDateTimeSelMin.Size = new System.Drawing.Size(193, 21);
            this.textBoxDateTimeSelMin.TabIndex = 13;
            this.textBoxDateTimeSelMin.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.textBoxDateTimeSelMin.TextChanged += new System.EventHandler(this.textBoxDateTimeSelMin_TextChanged);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Dock = System.Windows.Forms.DockStyle.Right;
            this.label3.Location = new System.Drawing.Point(48, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(69, 32);
            this.label3.TabIndex = 8;
            this.label3.Text = "Tag Name:";
            this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // textBoxTagNameFilter
            // 
            this.textBoxTagNameFilter.Dock = System.Windows.Forms.DockStyle.Fill;
            this.textBoxTagNameFilter.Location = new System.Drawing.Point(123, 6);
            this.textBoxTagNameFilter.Margin = new System.Windows.Forms.Padding(3, 6, 3, 3);
            this.textBoxTagNameFilter.Name = "textBoxTagNameFilter";
            this.textBoxTagNameFilter.Size = new System.Drawing.Size(193, 21);
            this.textBoxTagNameFilter.TabIndex = 9;
            // 
            // labelTimeFilterMin
            // 
            this.labelTimeFilterMin.AutoSize = true;
            this.labelTimeFilterMin.Dock = System.Windows.Forms.DockStyle.Right;
            this.labelTimeFilterMin.Location = new System.Drawing.Point(23, 32);
            this.labelTimeFilterMin.Name = "labelTimeFilterMin";
            this.labelTimeFilterMin.Size = new System.Drawing.Size(94, 32);
            this.labelTimeFilterMin.TabIndex = 10;
            this.labelTimeFilterMin.Text = "Date/Time Min:";
            this.labelTimeFilterMin.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // dateTimePickerDateMin
            // 
            this.dateTimePickerDateMin.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dateTimePickerDateMin.Location = new System.Drawing.Point(123, 64);
            this.dateTimePickerDateMin.Margin = new System.Windows.Forms.Padding(3, 0, 3, 0);
            this.dateTimePickerDateMin.Name = "dateTimePickerDateMin";
            this.dateTimePickerDateMin.Size = new System.Drawing.Size(193, 21);
            this.dateTimePickerDateMin.TabIndex = 15;
            this.dateTimePickerDateMin.ValueChanged += new System.EventHandler(this.dateTimePickerDateMin_ValueChanged);
            // 
            // dateTimePickerTimeMin
            // 
            this.dateTimePickerTimeMin.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dateTimePickerTimeMin.Format = System.Windows.Forms.DateTimePickerFormat.Time;
            this.dateTimePickerTimeMin.Location = new System.Drawing.Point(123, 84);
            this.dateTimePickerTimeMin.Margin = new System.Windows.Forms.Padding(3, 0, 3, 0);
            this.dateTimePickerTimeMin.Name = "dateTimePickerTimeMin";
            this.dateTimePickerTimeMin.Size = new System.Drawing.Size(193, 21);
            this.dateTimePickerTimeMin.TabIndex = 16;
            this.dateTimePickerTimeMin.ValueChanged += new System.EventHandler(this.dateTimePickerTimeMin_ValueChanged);
            // 
            // labelDesc
            // 
            this.labelDesc.AutoSize = true;
            this.labelDesc.Dock = System.Windows.Forms.DockStyle.Right;
            this.labelDesc.Location = new System.Drawing.Point(45, 176);
            this.labelDesc.Name = "labelDesc";
            this.labelDesc.Size = new System.Drawing.Size(72, 32);
            this.labelDesc.TabIndex = 10;
            this.labelDesc.Text = "Description:";
            this.labelDesc.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // textBoxDescriptionFilter
            // 
            this.textBoxDescriptionFilter.Dock = System.Windows.Forms.DockStyle.Fill;
            this.textBoxDescriptionFilter.Location = new System.Drawing.Point(123, 182);
            this.textBoxDescriptionFilter.Margin = new System.Windows.Forms.Padding(3, 6, 3, 3);
            this.textBoxDescriptionFilter.Name = "textBoxDescriptionFilter";
            this.textBoxDescriptionFilter.Size = new System.Drawing.Size(193, 21);
            this.textBoxDescriptionFilter.TabIndex = 9;
            // 
            // buttonInitialize
            // 
            this.buttonInitialize.Dock = System.Windows.Forms.DockStyle.Fill;
            this.buttonInitialize.Location = new System.Drawing.Point(123, 210);
            this.buttonInitialize.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.buttonInitialize.Name = "buttonInitialize";
            this.buttonInitialize.Size = new System.Drawing.Size(193, 29);
            this.buttonInitialize.TabIndex = 11;
            this.buttonInitialize.Text = "초기화";
            this.buttonInitialize.UseVisualStyleBackColor = true;
            this.buttonInitialize.Click += new System.EventHandler(this.buttonInitialize_Click);
            // 
            // buttonApply
            // 
            this.buttonApply.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.buttonApply.Dock = System.Windows.Forms.DockStyle.Fill;
            this.buttonApply.Location = new System.Drawing.Point(123, 243);
            this.buttonApply.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.buttonApply.Name = "buttonApply";
            this.buttonApply.Size = new System.Drawing.Size(193, 27);
            this.buttonApply.TabIndex = 11;
            this.buttonApply.Text = "적용";
            this.buttonApply.UseVisualStyleBackColor = true;
            // 
            // buttonCancel
            // 
            this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonCancel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.buttonCancel.Location = new System.Drawing.Point(123, 274);
            this.buttonCancel.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(193, 29);
            this.buttonCancel.TabIndex = 12;
            this.buttonCancel.Text = "취소";
            this.buttonCancel.UseVisualStyleBackColor = true;
            // 
            // FilterForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(319, 305);
            this.Controls.Add(this.tableLayoutPanel1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FilterForm";
            this.ShowIcon = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Search Filter";
            this.TopMost = true;
            this.Load += new System.EventHandler(this.FilterForm_Load);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.Label labelDesc;
        private System.Windows.Forms.Label labelTimeFilterMin;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox textBoxTagNameFilter;
        private System.Windows.Forms.TextBox textBoxDescriptionFilter;
        private System.Windows.Forms.Button buttonInitialize;
        private System.Windows.Forms.Button buttonApply;
        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.TextBox textBoxDateTimeSelMin;
        private System.Windows.Forms.DateTimePicker dateTimePickerDateMin;
        private System.Windows.Forms.DateTimePicker dateTimePickerTimeMin;
        private System.Windows.Forms.DateTimePicker dateTimePickerTimeMax;
        private System.Windows.Forms.DateTimePicker dateTimePickerDateMax;
        private System.Windows.Forms.TextBox textBoxDateTimeSelMax;
        private System.Windows.Forms.Label labelTimeFilterMax;
    }
}