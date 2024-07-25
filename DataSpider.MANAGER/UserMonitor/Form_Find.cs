using LibraryWH.FormCtrl;
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
    public partial class Form_Find : LibraryWH.FormCtrl.UserForm
    {
        private UserForm owner = null;
        private bool fin = false;
        public Form_Find()
        {
            InitializeComponent();
        }
        public Form_Find(UserForm _owner)
        {
            InitializeComponent();
            owner = _owner;
        }

        private void button_Next_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(textBox_Find.Text))
            {
                owner.Find(textBox_Find.Text, radioButton_Top.Checked, checkBox_CaseSense.Checked);
            }
        }

        private void Form_Find_Load(object sender, EventArgs e)
        {
            //this.ActiveControl = textBox_Find;
            //textBox_Find.Focus();
            //textBox_Find.SelectAll();//.Focus();
        }

        private void Form_Find_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!fin)
            {
                e.Cancel = true;
                this.Hide();
            }
        }

        private void button_Cancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {

            Keys key = keyData & ~(Keys.Shift | Keys.Control | Keys.Alt);
            switch (key)
            {
                case Keys.F3:
                    button_Next_Click(null, null);
                    break;
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }

        private void Form_Find_Activated(object sender, EventArgs e)
        {
            this.ActiveControl = textBox_Find;
            textBox_Find.Focus();
            textBox_Find.SelectAll();//.Focus();
        }
    }
}
