﻿using System;
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

            if (dtCommonCd.Rows.Count > 0)
            {
                for (int nR = 0; nR < dtCommonCd.Rows.Count; nR++)
                {
                    int rowId = dataGridCommonCode.Rows.Add();

                    DataGridViewRow row = dataGridCommonCode.Rows[rowId];

                    row.Cells["Selected"].Value = "false";
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
            CommonCodenfoEdit dlg = new CommonCodenfoEdit("", "", "", "", "", "", CommonCodenfoEdit.EDIT_MODE_ADD);
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                dataGridCommonCode.Rows.Clear();
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
            bool isRowSelected = false;

            foreach (DataGridViewRow row in dataGridCommonCode.Rows)
            {
                if (Convert.ToBoolean(row.Cells[0].Value))
                {
                    isRowSelected = true;
                    break;
                }
            }

            if (!isRowSelected)
            {
                MessageBox.Show("CheckBox is not selected", $"Common Code Delete", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (MessageBox.Show("Are you sure you want to delete the selected rows?", $"Delete CommonCode", MessageBoxButtons.OKCancel, MessageBoxIcon.Information) == DialogResult.OK)
            {
                for (int i = dataGridCommonCode.Rows.Count - 1; i >= 0; i--)
                {
                    DataGridViewRow row = dataGridCommonCode.Rows[i];
                    if (Convert.ToBoolean(row.Cells[0].Value))
                    {
                        if(sqlBiz.DeleteCommonCode(row.Cells[1].Value.ToString(), row.Cells[2].Value.ToString(), ref strErrCode, ref strErrText) != true)
                        {
                            MessageBox.Show(strErrText, $"Common Code remove fail", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            return;
                        }
                    }
                }
                MessageBox.Show("Common Code Removed.", $"Common Code", MessageBoxButtons.OK, MessageBoxIcon.Information);
                dataGridCommonCode.Rows.Clear();
                InitControls();
            }
        }

        private void button_Close_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
