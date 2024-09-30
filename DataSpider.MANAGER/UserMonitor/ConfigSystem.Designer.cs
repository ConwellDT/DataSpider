
namespace DataSpider.UserMonitor
{
    partial class ConfigSystem
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
            this.labelDBConnStr = new System.Windows.Forms.Label();
            this.textBox_DBConnString = new System.Windows.Forms.TextBox();
            this.buttonChangeDBConn = new System.Windows.Forms.Button();
            this.buttonReloadDBConn = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // labelDBConnStr
            // 
            this.labelDBConnStr.AutoSize = true;
            this.labelDBConnStr.Location = new System.Drawing.Point(9, 20);
            this.labelDBConnStr.Name = "labelDBConnStr";
            this.labelDBConnStr.Size = new System.Drawing.Size(166, 12);
            this.labelDBConnStr.TabIndex = 0;
            this.labelDBConnStr.Text = "Datebase Connection String:";
            // 
            // textBox_DBConnString
            // 
            this.textBox_DBConnString.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_DBConnString.Font = new System.Drawing.Font("맑은 고딕", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.textBox_DBConnString.Location = new System.Drawing.Point(181, 12);
            this.textBox_DBConnString.Name = "textBox_DBConnString";
            this.textBox_DBConnString.Size = new System.Drawing.Size(535, 29);
            this.textBox_DBConnString.TabIndex = 1;
            // 
            // buttonChangeDBConn
            // 
            this.buttonChangeDBConn.Location = new System.Drawing.Point(722, 11);
            this.buttonChangeDBConn.Name = "buttonChangeDBConn";
            this.buttonChangeDBConn.Size = new System.Drawing.Size(75, 31);
            this.buttonChangeDBConn.TabIndex = 2;
            this.buttonChangeDBConn.Text = "Change";
            this.buttonChangeDBConn.UseVisualStyleBackColor = true;
            this.buttonChangeDBConn.Click += new System.EventHandler(this.buttonChangeDBConn_Click);
            // 
            // buttonReloadDBConn
            // 
            this.buttonReloadDBConn.Location = new System.Drawing.Point(803, 12);
            this.buttonReloadDBConn.Name = "buttonReloadDBConn";
            this.buttonReloadDBConn.Size = new System.Drawing.Size(75, 31);
            this.buttonReloadDBConn.TabIndex = 3;
            this.buttonReloadDBConn.Text = "Reload";
            this.buttonReloadDBConn.UseVisualStyleBackColor = true;
            // 
            // ConfigSystem
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(887, 558);
            this.Controls.Add(this.buttonReloadDBConn);
            this.Controls.Add(this.buttonChangeDBConn);
            this.Controls.Add(this.textBox_DBConnString);
            this.Controls.Add(this.labelDBConnStr);
            this.Name = "ConfigSystem";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "ConfigSystem";
            this.Load += new System.EventHandler(this.ConfigSystem_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label labelDBConnStr;
        private System.Windows.Forms.TextBox textBox_DBConnString;
        private System.Windows.Forms.Button buttonChangeDBConn;
        private System.Windows.Forms.Button buttonReloadDBConn;
    }
}