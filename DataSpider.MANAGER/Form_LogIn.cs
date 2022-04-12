using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using DataSpider.PC00.PT;

namespace DataSpider
{
    public partial class Form_LogIn : Form
    {
        public Form_LogIn()
        {
            InitializeComponent();
        }

        private void button_OK_Click(object sender, EventArgs e)
        {
            if (UserAuthentication.LogIn(textBox_ID.Text, textBox_PW.Text))
            {
                DialogResult = DialogResult.OK;
                this.Close();
            }
            else
            {
                MessageBox.Show("Failed to log in", this.Text, MessageBoxButtons.OK);
            }
        }

        private void button_Cancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void panel_Free_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (UserAuthentication.FreePass())
            {
                DialogResult = DialogResult.OK;
                this.Close();
            }
        }
    }
}
