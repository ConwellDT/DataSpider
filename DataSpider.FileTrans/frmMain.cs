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
                        this.textBox1.Text = "Data Source=192.168.20.229;Initial Catalog=DataSpider;Persist Security Info=True;User ID=SBLADMIN;Password=SBLADMIN#01";
            //this.textBox1.Text = "Data Source=192.168.0.148;Initial Catalog=DataSpider;Persist Security Info=True;User ID=SBLADMIN;Password=SBLADMIN#01";
        }

        private void 암호화ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //string str = CFW.Common.SecurityUtil.EncryptString("Data Source=192.168.20.229;Initial Catalog=SEIMM;Persist Security Info=True;User ID=SBLADMIN;Password=SBLADMIN#01");
            string str = CFW.Common.SecurityUtil.EncryptString(this.textBox1.Text);
            //MessageBox.Show(str);
            this.textBox2.Text = str;

        }

        private void 설정ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string str = CFW.Common.SecurityUtil.DecryptString(this.textBox2.Text);
            //MessageBox.Show(str);
            this.textBox1.Text = str;
        }
    }
}
