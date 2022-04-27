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
    public partial class TagGroupInfoEdit : Form
    {
        String strEQTypeCode = string.Empty;
        int EditMode = -1;

        public const int EDIT_MODE_ADD = 0;
        public const int EDIT_MODE_UPDATE = 1;

        public String strGroupName = String.Empty;
        public String strDesc = String.Empty;

        private PC00Z01 sqlBiz = new PC00Z01();
        public TagGroupInfoEdit(String eqType, String strGrpName, String strDesc, int edMode)
        {
            InitializeComponent();

            strEQTypeCode = eqType;
            EditMode = edMode;

            if( String.IsNullOrEmpty(strGrpName) == false )
            {
                textBoxGroupNameInput.Text = strGrpName;
                if (edMode == EDIT_MODE_UPDATE) textBoxGroupNameInput.Enabled = false;
            }

            if (String.IsNullOrEmpty(strDesc) == false)
            {
                textBoxGroupDescInput.Text = strDesc;
            }
        }

        private void button_OK_Click(object sender, EventArgs e)
        {
            string strErrCode = string.Empty;
            string strErrText = string.Empty;

            strGroupName = textBoxGroupNameInput.Text;
            strDesc = textBoxGroupDescInput.Text;

            if( strGroupName == string.Empty)
            {
                MessageBox.Show("Group name is not exist", $"Group name invalid", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            DataTable dtGroups = sqlBiz.GetTagGroupByEQType(strEQTypeCode, ref strErrCode, ref strErrText);

            if (strErrCode == null || strErrCode == string.Empty)
            {
                DataRow[] drGroupSel = dtGroups.Select($"GROUP_NM = '{strGroupName}'");

                if ( EditMode == EDIT_MODE_ADD)
                {
                    if( drGroupSel != null && drGroupSel.Length > 0 )
                    {
                        MessageBox.Show($"EQ type:" + strGroupName + $" already exist", $"Group name invalid", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else
                    {
                        DialogResult = DialogResult.OK;
                        this.Close();
                    }
                }
                else
                {
                    if (drGroupSel != null && drGroupSel.Length > 0)
                    {
                        DialogResult = DialogResult.OK;
                        this.Close();
                    }
                    else
                    {
                        MessageBox.Show($"EQ type:" + strGroupName + $" is not exist", $"Group name invalid", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
            }
            else
            {
                MessageBox.Show(strErrText, $"Group info read fail", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void button_Cancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            this.Close();
        }
    }
}
