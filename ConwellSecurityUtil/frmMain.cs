using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SEIMM.FileTrans
{
    public partial class frmMain : Form
    {
        public frmMain()
        {
            InitializeComponent();
            this.textBox_Plain.Text = "Data Source=192.168.20.229;Initial Catalog=SEIMM;Persist Security Info=True;User ID=SBLADMIN;Password=SBLADMIN#01";
        }

        private void button_Close_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void button_Encrypt_Click(object sender, EventArgs e)
        {
            textBox_Cryptogram.Text = CFW.Common.SecurityUtil.EncryptString(this.textBox_Plain.Text);
        }

        private void button_Decrypt_Click(object sender, EventArgs e)
        {
            textBox_Plain.Text = CFW.Common.SecurityUtil.DecryptString(this.textBox_Cryptogram.Text);

        }
    }
}
