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
using System.Runtime.InteropServices;

namespace DataSpider.UserMonitor
{
    public partial class TagAddEdit : Form
    {
        #region Using Win32 DLL
        public const int WM_NCLBUTTONDOWN = 0xA1;
        public const int HT_CAPTION = 0x2;
        [DllImportAttribute("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);
        [DllImportAttribute("user32.dll")]
        public static extern bool ReleaseCapture();

        [DllImportAttribute("user32.dll")]
        private static extern IntPtr FindWindow(string lpClassName, string lpWindowName);
        #endregion

        private PC00Z01 sqlBiz = new PC00Z01();
        private readonly string TagName = string.Empty;
        private readonly string EquipName = string.Empty;
        private bool AddMode { get { return string.IsNullOrWhiteSpace(TagName); } }

        public TagAddEdit(string equipName, string tagName = "")
        {
            InitializeComponent();
            TagName = tagName;
            EquipName = equipName;
        }
        private void TagAddEdit_Load(object sender, EventArgs e)
        {
            InitControls();
        }

        private void InitControls()
        {
            string strErrCode = string.Empty;
            string strErrText = string.Empty;
            DataTable dtEquipment = sqlBiz.GetEquipmentInfoForCombo(ref strErrCode, ref strErrText);
            comboBox_Equipment.DataSource = dtEquipment;
            comboBox_Equipment.DisplayMember = "EQUIP_NM_DESC";
            comboBox_Equipment.ValueMember = "EQUIP_NM";

            if (AddMode)
            {
                this.Text = label_Title.Text = "Add Tag";
                foreach (Control ctrl in this.Controls)
                {
                    if (ctrl is TextBox)
                    {
                        ctrl.Text = string.Empty;
                    }
                }
                textBox_TagName.Enabled = true;

                if (!string.IsNullOrWhiteSpace(EquipName))
                {
                    if (dtEquipment.Select($"EQUIP_NM = '{EquipName}'").Length > 0)
                    {
                        comboBox_Equipment.SelectedValue = EquipName;
                    }
                }
            }
            else
            {
                DataTable dtTag = sqlBiz.GetTagInfo(string.Empty, string.Empty, TagName, false, ref strErrCode, ref strErrText);
                if (UserAuthentication.UserLevel.Equals(UserLevel.UnAuthorized))
                {
                    this.Text = "Tag Info";
                    label_Title.Text = $"Tag [ {TagName} - {dtTag.Rows[0]["EQUIP_NM"]} ]";
                }
                else
                {
                    this.Text = "Edit Tag";
                    label_Title.Text = $"Edit Tag [ {TagName} - {dtTag.Rows[0]["EQUIP_NM"]} ]";
                }
                comboBox_Equipment.SelectedValue = dtTag.Rows[0]["EQUIP_NM"].ToString();
                textBox_TagName.Text = dtTag.Rows[0]["TAG_NM"].ToString();
                textBox_MessageType.Text = dtTag.Rows[0]["MSG_TYPE"].ToString();
                textBox_Description.Text = dtTag.Rows[0]["TAG_DESC"].ToString();
                textBox_PITagName.Text = dtTag.Rows[0]["PI_TAG_NM"].ToString();
                textBox_ValuePosition.Text = dtTag.Rows[0]["DATA_POSITION"].ToString();
                textBox_DatePosition.Text = dtTag.Rows[0]["DATE_POSITION"].ToString();
                textBox_TimePosition.Text = dtTag.Rows[0]["TIME_POSITION"].ToString();
                textBox_ItemName.Text = dtTag.Rows[0]["OPCITEM_NM"].ToString();
                textBox_TagName.Enabled = false;
            }
        }

        private void button_Save_Click(object sender, EventArgs e)
        {  
        }

        private bool CheckItem()
        {
            if (string.IsNullOrWhiteSpace(textBox_TagName.Text))
            {
                MessageBox.Show($"Tag Name 을 입력하세요.", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
                return false;
            }
            if (string.IsNullOrWhiteSpace(textBox_MessageType.Text))
            {
                MessageBox.Show($"Message Type 을 입력하세요.", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
                return false;
            }
            if (string.IsNullOrWhiteSpace(textBox_ItemName.Text) && (string.IsNullOrWhiteSpace(textBox_ValuePosition.Text) || string.IsNullOrWhiteSpace(textBox_DatePosition.Text)))
            {
                MessageBox.Show($"Value, Date Position 또는 Item Name 을 입력하세요.", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
                return false;
            }
            if (string.IsNullOrWhiteSpace(textBox_ItemName.Text))
            {
                if (!PC00U01.CheckPosInfo(textBox_ValuePosition.Text.Trim(), out string errMessage))
                {
                    MessageBox.Show($"Value Position 설정 오류 입니다. [ {errMessage} ]", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }
                if (!PC00U01.CheckPosInfo(textBox_DatePosition.Text.Trim(), out errMessage))
                {
                    MessageBox.Show($"Date Position 설정 오류 입니다. [ {errMessage} ]", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }
                if (!string.IsNullOrWhiteSpace(textBox_TimePosition.Text) && !PC00U01.CheckPosInfo(textBox_TimePosition.Text.Trim(), out errMessage))
                {
                    MessageBox.Show($"Time Position 설정 오류 입니다. [ {errMessage} ]", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }
            }
            return true;
        }

        private void button_Cancel_Click(object sender, EventArgs e)
        { 
        }

        private void labelBTSave_Click(object sender, EventArgs e)
        {
            if (!CheckItem())
            {
                return;
            }
            string message = AddMode ? $"를 추가하시겠습니까?" : "를 저장하시겠습니까?";
            if (DialogResult.Yes.Equals(MessageBox.Show($"{textBox_TagName.Text} {message}", this.Text, MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2)))
            {
                string strErrCode = string.Empty;
                string strErrText = string.Empty;
                if (sqlBiz.InsertUpdateTagInfo(AddMode, textBox_TagName.Text.Trim(), textBox_MessageType.Text.Trim(), comboBox_Equipment.SelectedValue.ToString(), textBox_Description.Text.Trim(),
                    textBox_PITagName.Text.Trim(), textBox_ValuePosition.Text.Trim(), textBox_DatePosition.Text.Trim(), textBox_TimePosition.Text.Trim(), textBox_ItemName.Text.Trim(), ref strErrCode, ref strErrText))
                {
                    MessageBox.Show($"저장 되었습니다.", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
                    DialogResult = DialogResult.OK;
                    this.Close();
                }
                else
                {
                    MessageBox.Show($"저장 중 오류가 발생하였습니다. [{strErrText}]", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void labelBTCancel_Click(object sender, EventArgs e)
        {
            if (UserAuthentication.UserLevel.Equals(UserLevel.UnAuthorized) || DialogResult.Yes.Equals(MessageBox.Show($"Do you want to exit without saving ?", this.Text, MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2)))
            {
                DialogResult = DialogResult.Cancel;
                this.Close();
            }
        }

        private void label_Title_MouseDown(object sender, MouseEventArgs e)
        {
            ReleaseCapture();
            SendMessage(this.Handle, WM_NCLBUTTONDOWN, HT_CAPTION, 0);
        }
    }
}
