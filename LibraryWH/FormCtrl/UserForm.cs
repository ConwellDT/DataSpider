using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LibraryWH.FormCtrl
{
    public partial class UserForm : Form
    {
        public UserForm()
        {
            InitializeComponent();
        }

        public void DisplayIn(UserFormInfo pInfo = null)
        {
            TopLevel = false;
            if( pInfo != null )
                this.Text = pInfo.Description;

            FormBorderStyle = FormBorderStyle.None;
            Dock = DockStyle.Fill;
            Show();

        }

    }
}
