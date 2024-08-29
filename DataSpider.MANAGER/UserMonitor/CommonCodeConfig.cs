using System;
using System.Collections.Generic;
using System.Data;
using System.Windows.Forms;

using DataSpider.PC00.PT;

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

            dataGridCommonCode.Rows.Clear();

            if (dtCommonCd.Rows.Count > 0)
            {
                for (int nR = 0; nR < dtCommonCd.Rows.Count; nR++)
                {
                    int rowId = dataGridCommonCode.Rows.Add();

                    DataGridViewRow row = dataGridCommonCode.Rows[rowId];

                    row.Cells["CodeGroup"].Value = dtCommonCd.Rows[nR]["CD_GRP"].ToString();
                    row.Cells["Code"].Value = dtCommonCd.Rows[nR]["CODE"].ToString();
                    row.Cells["CodeName"].Value = dtCommonCd.Rows[nR]["CODE_NM"].ToString();
                    row.Cells["CodeValue"].Value = dtCommonCd.Rows[nR]["CODE_VALUE"].ToString();
                    row.Cells["UpdateRegDate"].Value = dtCommonCd.Rows[nR]["UPDATE_REG_DATE"].ToString();
                    row.Cells["UpdateRegId"].Value = dtCommonCd.Rows[nR]["UPDATE_REG_ID"].ToString();
                    row.Cells["RegDate"].Value = dtCommonCd.Rows[nR]["REG_DATE"].ToString();
                    row.Cells["RegId"].Value = dtCommonCd.Rows[nR]["REG_ID"].ToString();
                }
            }
        }
        private void button_Add_Click(object sender, EventArgs e)
        {
            string strErrCode = string.Empty;
            string strErrText = string.Empty;

            CommonCodenfoEdit dlg = new CommonCodenfoEdit("", "", "", "", "", "", CommonCodenfoEdit.EDIT_MODE_ADD);
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                InitControls();
            }
        }

        private void button_Edit_Click(object sender, EventArgs e)
        {


        }

        private void button_Remove_Click(object sender, EventArgs e)
        {
            string strErrCode = string.Empty;
            string strErrText = string.Empty;

            // Load data into DataGridView
            DataTable dtCommonCd = sqlBiz.GetAllCommonCode(ref strErrCode, ref strErrText);
            if (dtCommonCd != null)
            {
                dataGridCommonCode.DataSource = dtCommonCd;
            }
        }


        private void button_Close_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
