using DataSpider.PC00.PT;
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
    public partial class CommonCodeConfig : Form
    {
        private PC00Z01 sqlBiz = new PC00Z01();

        public CommonCodeConfig()
        {
            InitializeComponent();
        }

        private void CommonCodeConfig_Load(object sender, EventArgs e)
        {
            InitControls();
        }

        private void InitControls()
        {
            string strErrCode = string.Empty;
            string strErrText = string.Empty;

            DataTable dtCommonCd = sqlBiz.GetAllCommonCode(ref strErrCode, ref strErrText);

        }

        private void button_Add_Click(object sender, EventArgs e)
        {

        }

        private void button_Edit_Click(object sender, EventArgs e)
        {

           
        }

        private void button_Remove_Click(object sender, EventArgs e)
        {

           
        }

        private void button_Close_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
