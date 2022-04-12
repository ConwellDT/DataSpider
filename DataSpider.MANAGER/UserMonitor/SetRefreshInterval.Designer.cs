
namespace DataSpider.UserMonitor
{
    partial class SetRefreshInterval
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
            this.labelRefreshInterval = new System.Windows.Forms.Label();
            this.panelRefreshIntervalBack = new System.Windows.Forms.Panel();
            this.textBox_RefreshInterval = new System.Windows.Forms.TextBox();
            this.button_SetInterval = new System.Windows.Forms.Button();
            this.button_Cancel = new System.Windows.Forms.Button();
            this.panelRefreshIntervalBack.SuspendLayout();
            this.SuspendLayout();
            // 
            // labelRefreshInterval
            // 
            this.labelRefreshInterval.Location = new System.Drawing.Point(3, 6);
            this.labelRefreshInterval.Name = "labelRefreshInterval";
            this.labelRefreshInterval.Size = new System.Drawing.Size(132, 37);
            this.labelRefreshInterval.TabIndex = 3;
            this.labelRefreshInterval.Text = "Refresh Interval (sec)";
            this.labelRefreshInterval.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // panelRefreshIntervalBack
            // 
            this.panelRefreshIntervalBack.BackColor = System.Drawing.Color.White;
            this.panelRefreshIntervalBack.Controls.Add(this.textBox_RefreshInterval);
            this.panelRefreshIntervalBack.ForeColor = System.Drawing.Color.White;
            this.panelRefreshIntervalBack.Location = new System.Drawing.Point(141, 8);
            this.panelRefreshIntervalBack.Name = "panelRefreshIntervalBack";
            this.panelRefreshIntervalBack.Size = new System.Drawing.Size(54, 34);
            this.panelRefreshIntervalBack.TabIndex = 5;
            // 
            // textBox_RefreshInterval
            // 
            this.textBox_RefreshInterval.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.textBox_RefreshInterval.Location = new System.Drawing.Point(0, 9);
            this.textBox_RefreshInterval.Margin = new System.Windows.Forms.Padding(0, 8, 0, 0);
            this.textBox_RefreshInterval.Name = "textBox_RefreshInterval";
            this.textBox_RefreshInterval.Size = new System.Drawing.Size(54, 14);
            this.textBox_RefreshInterval.TabIndex = 0;
            this.textBox_RefreshInterval.Text = "10";
            this.textBox_RefreshInterval.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // button_SetInterval
            // 
            this.button_SetInterval.Location = new System.Drawing.Point(201, 8);
            this.button_SetInterval.Name = "button_SetInterval";
            this.button_SetInterval.Size = new System.Drawing.Size(94, 34);
            this.button_SetInterval.TabIndex = 6;
            this.button_SetInterval.Text = "Set Interval";
            this.button_SetInterval.UseVisualStyleBackColor = true;
            this.button_SetInterval.Click += new System.EventHandler(this.button_SetInterval_Click);
            // 
            // button_Cancel
            // 
            this.button_Cancel.Location = new System.Drawing.Point(301, 8);
            this.button_Cancel.Name = "button_Cancel";
            this.button_Cancel.Size = new System.Drawing.Size(94, 34);
            this.button_Cancel.TabIndex = 7;
            this.button_Cancel.Text = "Cancel";
            this.button_Cancel.UseVisualStyleBackColor = true;
            this.button_Cancel.Click += new System.EventHandler(this.button_Cancel_Click);
            // 
            // SetRefreshInterval
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(400, 51);
            this.Controls.Add(this.button_Cancel);
            this.Controls.Add(this.button_SetInterval);
            this.Controls.Add(this.panelRefreshIntervalBack);
            this.Controls.Add(this.labelRefreshInterval);
            this.Name = "SetRefreshInterval";
            this.Text = "Set Tag Value Refresh Interval";
            this.Load += new System.EventHandler(this.SetRefreshInterval_Load);
            this.panelRefreshIntervalBack.ResumeLayout(false);
            this.panelRefreshIntervalBack.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label labelRefreshInterval;
        private System.Windows.Forms.Panel panelRefreshIntervalBack;
        private System.Windows.Forms.TextBox textBox_RefreshInterval;
        private System.Windows.Forms.Button button_SetInterval;
        private System.Windows.Forms.Button button_Cancel;
    }
}