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
    public partial class SetRefreshInterval : Form
    {
        public int RefreshInterval
        {
            get 
            {
                if (int.TryParse(textBox_RefreshInterval.Text, out int tempInterval))
                {
                    return tempInterval;
                }
                return 0; 
            }
            set 
            {
                textBox_RefreshInterval.Text = value.ToString();
            }
        }

        public SetRefreshInterval()
        {
            InitializeComponent();
        }

        private void SetRefreshInterval_Load(object sender, EventArgs e)
        {

        }

        private void button_SetInterval_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void button_Cancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }
    }
}
