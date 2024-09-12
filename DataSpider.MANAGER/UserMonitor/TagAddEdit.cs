using System;
using System.Data;
using System.Windows.Forms;

using DataSpider.PC00.PT;

using DevExpress.Export.Xl;
using DevExpress.XtraEditors;

using static DevExpress.Drawing.Printing.Internal.DXPageSizeInfo;

namespace DataSpider.UserMonitor
{
    public partial class TagAddEdit : Form
    {
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
                    button_Save.Visible = false;
                    button_Cancel.Text = "Close";
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
                textBoxValuePosition.Text = dtTag.Rows[0]["DATA_POSITION"].ToString();
                textBoxDatePosition.Text = dtTag.Rows[0]["DATE_POSITION"].ToString();
                textBoxTimePosition.Text = dtTag.Rows[0]["TIME_POSITION"].ToString();
                textBox_ItemName.Text = dtTag.Rows[0]["OPCITEM_NM"].ToString();
                textBox_EventFrameAttributeName.Text = dtTag.Rows[0]["EF_ATTRIBUTE_NM"].ToString();

                textBox_TagName.Enabled = false;
            }
        }

        private void button_Save_Click(object sender, EventArgs e)
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
                    textBox_PITagName.Text.Trim(), textBox_EventFrameAttributeName.Text.Trim(), textBoxValuePosition.Text.Trim(), textBoxDatePosition.Text.Trim(), textBoxTimePosition.Text.Trim(), textBox_ItemName.Text.Trim(), ref strErrCode, ref strErrText))
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
            if (string.IsNullOrWhiteSpace(textBox_ItemName.Text) && (string.IsNullOrWhiteSpace(textBoxValuePosition.Text) || string.IsNullOrWhiteSpace(textBoxDatePosition.Text)))
            {
                MessageBox.Show($"Value, Date Position 또는 Item Name 을 입력하세요.", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
                return false;
            }
            if (string.IsNullOrWhiteSpace(textBox_ItemName.Text))
            {
                if (!PC00U01.CheckPosInfo(textBoxValuePosition.Text.Trim(), out string errMessage))
                {
                    MessageBox.Show($"Value Position 설정 오류 입니다. [ {errMessage} ]", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }
                if (!PC00U01.CheckPosInfo(textBoxDatePosition.Text.Trim(), out errMessage))
                {
                    MessageBox.Show($"Date Position 설정 오류 입니다. [ {errMessage} ]", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }
                if (!string.IsNullOrWhiteSpace(textBoxTimePosition.Text) && !PC00U01.CheckPosInfo(textBoxTimePosition.Text.Trim(), out errMessage))
                {
                    MessageBox.Show($"Time Position 설정 오류 입니다. [ {errMessage} ]", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }
            }
            return true;
        }

        private void button_Cancel_Click(object sender, EventArgs e)
        {
            if (UserAuthentication.UserLevel.Equals(UserLevel.UnAuthorized) || DialogResult.Yes.Equals(MessageBox.Show($"Do you want to exit without saving ?", this.Text, MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2)))
            {
                DialogResult = DialogResult.Cancel;
                this.Close();
            }
        }

        private void ShowTagPositionEditDialog(TextBox textcontrol)
        {
            TagPositionEdit dlg = CreateTagPositionEditFromText(textcontrol.Text);

            if (dlg.ShowDialog() == DialogResult.OK)
            {
                textcontrol.Text = FormatTagPositionEditValues(dlg);
            }
        }

        private TagPositionEdit CreateTagPositionEditFromText(string text)
        {
            string use = string.Empty;
            string[] useValues = textBoxValuePosition.Text.Split(',');
            if (!string.IsNullOrEmpty(text))
            {
                use = useValues.Length == 5 ? "Y" : "N";
            }
            var values = ParseValues(text, 5);
            return new TagPositionEdit(use, values[0], values[1], values[2], values[3], values[4]);
        }

        private string[] ParseValues(string text, int expectedLength)
        {
            var values = text.Split(',');
            var result = new string[expectedLength];
            for (int i = 0; i < expectedLength; i++)
            {
                result[i] = i < values.Length ? values[i].Trim() : string.Empty;
            }
            return result;
        }

        private string FormatTagPositionEditValues(TagPositionEdit dlg)
        {
            if (dlg.DelimeterUse.Equals("Y"))
            {
                // Return the formatted string with all values
                return $"{dlg.LineValue},{dlg.DelimeterVale},{dlg.ItemIndexValue},{dlg.OffsetValue},{dlg.SizeValue}";
            }
            else
            {
                // Return the formatted string without Delimeter and ItemIndex
                return $"{dlg.LineValue},{dlg.OffsetValue},{dlg.SizeValue}";
            }
        }

        private void button_ValuePosition_ButtonClick(object sender, EventArgs e)
        {

            ShowTagPositionEditDialog(textBoxValuePosition);
        }

        private void buttonDatePosition_Click(object sender, EventArgs e)
        {
            ShowTagPositionEditDialog(textBoxDatePosition);
        }

        private void buttonTimePosition_Click(object sender, EventArgs e)
        {
            ShowTagPositionEditDialog(textBoxTimePosition);
        }
    }
}
