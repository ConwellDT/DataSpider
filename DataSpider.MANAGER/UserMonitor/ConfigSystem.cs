using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DataSpider.UserMonitor
{
    public partial class ConfigSystem : Form
    {
        public ConfigSystem()
        {
            InitializeComponent();
        }

        private void ConfigSystem_Load(object sender, EventArgs e)
        {
            String strConnStr = CFW.Common.SecurityUtil.DecryptString(CFW.Configuration.ConfigManager.Default.ReadConfig("connectionStrings", $"SQL_ConnectionString"));

            textBox_DBConnString.Text = strConnStr;
        }

        private DialogResult InputQuery(String caption, String prompt, ref String value)
        {
            Form form;
            form = new Form();
            form.AutoScaleMode = AutoScaleMode.Font;
            form.Font = SystemFonts.IconTitleFont;

            SizeF dialogUnits;
            dialogUnits = form.AutoScaleDimensions;

            form.FormBorderStyle = FormBorderStyle.FixedDialog;
            form.MinimizeBox = false;
            form.MaximizeBox = false;
            form.Text = caption;

            Size[] sizeCal = new Size[2];

            sizeCal[0] = TextRenderer.MeasureText(value, form.Font);
            sizeCal[1] = TextRenderer.MeasureText(prompt, form.Font);

            Size sizeCalUse = sizeCal[0];
            if (sizeCal[0].Width < sizeCal[1].Width) sizeCalUse = sizeCal[1];

            form.ClientSize =
                new Size(sizeCalUse.Width + 16 + 16, sizeCalUse.Height * 6 + 16 + 16);

            //form.ClientSize = new Size((int)Math.Round((float)180 * dialogUnits.Width / 4),
            //                            (int)Math.Round((float)63 * dialogUnits.Height / 8));

            form.StartPosition = FormStartPosition.CenterScreen;

            System.Windows.Forms.Label lblPrompt;
            lblPrompt = new System.Windows.Forms.Label();
            lblPrompt.Parent = form;
            lblPrompt.AutoSize = true;
            lblPrompt.Left = (int)Math.Round((float)8 * dialogUnits.Width / 4);
            lblPrompt.Top = (int)Math.Round((float)8 * dialogUnits.Height / 8);
            lblPrompt.Text = prompt;

            System.Windows.Forms.TextBox edInput;
            edInput = new System.Windows.Forms.TextBox();
            edInput.Parent = form;
            edInput.Left = lblPrompt.Left;
            edInput.Top = (int)Math.Round((float)19 * dialogUnits.Height / 8);
            //edInput.Width = (int)Math.Round((float)164 * dialogUnits.Width / 4);
            sizeCalUse = TextRenderer.MeasureText(value, form.Font);
            edInput.Width = sizeCalUse.Width;
            edInput.Text = value;
            edInput.SelectAll();


            int buttonTop = edInput.Top + edInput.Height + 8;

            Size buttonSize = new Size(80, 40);

            //if (Math.Abs((dialogUnits.Height / 8) / (dialogUnits.Width / 4) - 1) < float.Epsilon)
            //{
            //    var height = (int)(14 * (dialogUnits.Height / 8) / (dialogUnits.Width / 4));
            //    var width = (int)(50 * (dialogUnits.Height / 8) / (dialogUnits.Width / 4));

            //    buttonSize = new Size(width, height);
            //}

            System.Windows.Forms.Button bbOk = new System.Windows.Forms.Button();
            bbOk.Parent = form;
            bbOk.Text = "OK";
            bbOk.DialogResult = DialogResult.OK;
            form.AcceptButton = bbOk;
            bbOk.Location = new Point((int)Math.Round((float)38 * dialogUnits.Width / 4), buttonTop);
            bbOk.Size = buttonSize;

            System.Windows.Forms.Button bbCancel = new System.Windows.Forms.Button();
            bbCancel.Parent = form;
            bbCancel.Text = "Cancel";
            bbCancel.DialogResult = DialogResult.Cancel;
            form.CancelButton = bbCancel;
            bbCancel.Location = new Point((int)Math.Round((float)92 * dialogUnits.Width / 4), buttonTop);
            bbCancel.Size = buttonSize;

            DialogResult dlgResult = form.ShowDialog();

            return dlgResult;
        }

        private void buttonChangeDBConn_Click(object sender, EventArgs e)
        {
            String strConnStr = CFW.Common.SecurityUtil.DecryptString(CFW.Configuration.ConfigManager.Default.ReadConfig("connectionStrings", $"SQL_ConnectionString"));

            DialogResult dialogResult = MessageBox.Show($"If you change the DB connection, the system is shut down.", "Main DB Connection Change", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (dialogResult == DialogResult.Yes)
            {
                InputQuery("Change DB Connection String", "DB Connection string:", ref strConnStr);
            }
        }
    }
}
