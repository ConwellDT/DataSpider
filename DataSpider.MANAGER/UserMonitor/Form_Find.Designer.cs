
namespace DataSpider.UserMonitor
{
    partial class Form_Find
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
            this.button_Next = new System.Windows.Forms.Button();
            this.button_Cancel = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.textBox_Find = new System.Windows.Forms.TextBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.radioButton_Bottom = new System.Windows.Forms.RadioButton();
            this.radioButton_Top = new System.Windows.Forms.RadioButton();
            this.checkBox_CaseSense = new System.Windows.Forms.CheckBox();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // button_Next
            // 
            this.button_Next.Location = new System.Drawing.Point(434, 14);
            this.button_Next.Name = "button_Next";
            this.button_Next.Size = new System.Drawing.Size(106, 29);
            this.button_Next.TabIndex = 0;
            this.button_Next.Text = "다음 찾기(&F)";
            this.button_Next.UseVisualStyleBackColor = true;
            this.button_Next.Click += new System.EventHandler(this.button_Next_Click);
            // 
            // button_Cancel
            // 
            this.button_Cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.button_Cancel.Location = new System.Drawing.Point(434, 53);
            this.button_Cancel.Name = "button_Cancel";
            this.button_Cancel.Size = new System.Drawing.Size(106, 29);
            this.button_Cancel.TabIndex = 1;
            this.button_Cancel.Text = "취소";
            this.button_Cancel.UseVisualStyleBackColor = true;
            this.button_Cancel.Click += new System.EventHandler(this.button_Cancel_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 22);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(69, 12);
            this.label1.TabIndex = 2;
            this.label1.Text = "찾을 내용 : ";
            // 
            // textBox_Find
            // 
            this.textBox_Find.Location = new System.Drawing.Point(87, 19);
            this.textBox_Find.Name = "textBox_Find";
            this.textBox_Find.Size = new System.Drawing.Size(341, 21);
            this.textBox_Find.TabIndex = 3;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.radioButton_Bottom);
            this.groupBox1.Controls.Add(this.radioButton_Top);
            this.groupBox1.Location = new System.Drawing.Point(236, 52);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(192, 50);
            this.groupBox1.TabIndex = 5;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "방향";
            // 
            // radioButton_Bottom
            // 
            this.radioButton_Bottom.AutoSize = true;
            this.radioButton_Bottom.Checked = true;
            this.radioButton_Bottom.Location = new System.Drawing.Point(93, 20);
            this.radioButton_Bottom.Name = "radioButton_Bottom";
            this.radioButton_Bottom.Size = new System.Drawing.Size(77, 16);
            this.radioButton_Bottom.TabIndex = 6;
            this.radioButton_Bottom.TabStop = true;
            this.radioButton_Bottom.Text = "아래로(&D)";
            this.radioButton_Bottom.UseVisualStyleBackColor = true;
            // 
            // radioButton_Top
            // 
            this.radioButton_Top.AutoSize = true;
            this.radioButton_Top.Location = new System.Drawing.Point(20, 20);
            this.radioButton_Top.Name = "radioButton_Top";
            this.radioButton_Top.Size = new System.Drawing.Size(65, 16);
            this.radioButton_Top.TabIndex = 5;
            this.radioButton_Top.Text = "위로(&U)";
            this.radioButton_Top.UseVisualStyleBackColor = true;
            // 
            // checkBox_CaseSense
            // 
            this.checkBox_CaseSense.AutoSize = true;
            this.checkBox_CaseSense.Location = new System.Drawing.Point(13, 85);
            this.checkBox_CaseSense.Name = "checkBox_CaseSense";
            this.checkBox_CaseSense.Size = new System.Drawing.Size(125, 16);
            this.checkBox_CaseSense.TabIndex = 6;
            this.checkBox_CaseSense.Text = "대/소문자 구분(&C)";
            this.checkBox_CaseSense.UseVisualStyleBackColor = true;
            // 
            // Form_Find
            // 
            this.AcceptButton = this.button_Next;
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.button_Cancel;
            this.ClientSize = new System.Drawing.Size(552, 114);
            this.Controls.Add(this.checkBox_CaseSense);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.textBox_Find);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.button_Cancel);
            this.Controls.Add(this.button_Next);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Name = "Form_Find";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Find";
            this.TopMost = true;
            this.Activated += new System.EventHandler(this.Form_Find_Activated);
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form_Find_FormClosing);
            this.Load += new System.EventHandler(this.Form_Find_Load);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button button_Next;
        private System.Windows.Forms.Button button_Cancel;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox textBox_Find;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.RadioButton radioButton_Bottom;
        private System.Windows.Forms.RadioButton radioButton_Top;
        private System.Windows.Forms.CheckBox checkBox_CaseSense;
    }
}