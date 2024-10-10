using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;

using DataSpider.PC00.PT;

using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace DataSpider.UserMonitor
{
    public partial class CommonCodenfoEdit : Form
    {
        int EditMode = -1;

        public const int EDIT_MODE_ADD = 0;
        public const int EDIT_MODE_UPDATE = 1;
        public string beforecdGrp = string.Empty;
        public string beforecode = string.Empty;
        public string cdGrp = string.Empty;
        public string code = string.Empty;
        public string codeNm = string.Empty;
        public string codeVal = string.Empty;
        public string date = string.Empty;
        public string Id = string.Empty;

        private PC00Z01 sqlBiz = new PC00Z01();
        public CommonCodenfoEdit(string cdGrp, string code, string codeNm, string codeVal, string date, string Id, int edMode)
        {
            InitializeComponent();


            EditMode = edMode;
            if (EditMode == EDIT_MODE_UPDATE)
            {
                textBoxCodeGrp.Enabled = false;
                textBoxCodeGrp.BackColor = Color.White;
                textBoxCode.Enabled = false;
                textBoxCodeGrp.BackColor = Color.White;
            }

            if (string.IsNullOrEmpty(cdGrp) == false)
            {
                beforecdGrp = cdGrp;
                textBoxCodeGrp.Text = cdGrp;
            }

            if (string.IsNullOrEmpty(code) == false)
            {
                beforecode = code;
                textBoxCode.Text = code;
            }

            if (string.IsNullOrEmpty(codeNm) == false)
            {
                textBoxCodeName.Text = codeNm;
            }

            if (string.IsNullOrEmpty(codeVal) == false)
            {
                textBoxCodeValue.Text = codeVal;
            }

            textBoxDate.Text = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            textBoxUserId.Text = $"{UserAuthentication.UserID}";
        }

        private void button_OK_Click(object sender, EventArgs e)
        {
            string strErrCode = string.Empty;
            string strErrText = string.Empty;

            cdGrp = textBoxCodeGrp.Text;
            code = textBoxCode.Text;
            codeNm = textBoxCodeName.Text;
            codeVal = textBoxCodeValue.Text;
            date = textBoxDate.Text;
            Id = textBoxUserId.Text;

            if (cdGrp == string.Empty)
            {
                MessageBox.Show("Code Group is not exist", $"Code Group invalid", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            if (code == string.Empty)
            {
                MessageBox.Show("Code is not exist", $"Code invalid", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            if (codeNm == string.Empty)
            {
                MessageBox.Show("codeNm is not exist", $"codeNm invalid", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            if (codeVal == string.Empty)
            {
                MessageBox.Show("codeVal is not exist", $"codeVal invalid", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            //ADD 추가 유효성 체크
            if (EditMode == EDIT_MODE_ADD)
            {
                DataTable dtCommonCode = sqlBiz.GetAllCommonCode(ref strErrCode, ref strErrText);
                if (strErrCode == null || strErrCode == string.Empty)
                {
                    DataRow[] drCommonCode = dtCommonCode.Select($"CD_GRP = '{cdGrp}' AND CODE = '{code}'");

                    if (drCommonCode != null && drCommonCode.Length > 0)
                    {
                        MessageBox.Show($"Code Group: " + cdGrp + $", Code: " + code + $" already exist", $"Common Code invalid", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return;
                    }
                }

                if (sqlBiz.InsertCommonCode(cdGrp, code, codeNm, codeVal, date, Id, ref strErrCode, ref strErrText))
                {
                    MessageBox.Show($"CommonCode has been saved.", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
                    DialogResult = DialogResult.OK;
                    this.Close();
                }
                else
                {
                    MessageBox.Show($"An error occurred while saving. [{strErrText}]", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else if (EditMode == EDIT_MODE_UPDATE)
            {
                if (sqlBiz.UpdateCommonCode(cdGrp, code, codeNm, codeVal, date, Id, beforecdGrp, beforecode, ref strErrCode, ref strErrText))
                {
                    MessageBox.Show($"CommonCode has been saved.", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
                    DialogResult = DialogResult.OK;
                    this.Close();
                }
                else
                {
                    MessageBox.Show($"An error occurred while saving. [{strErrText}]", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }

        }

        private void button_Cancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            this.Close();
        }
    }
}
