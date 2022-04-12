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
    public partial class UserAddEdit : Form
    {
        private PC00Z01 sqlBiz = new PC00Z01();
        private readonly string UserID = string.Empty;
        private readonly string EquipTypeName = string.Empty;
        private readonly bool UserManagementMode = false;
        private bool AddMode { get { return string.IsNullOrWhiteSpace(UserID); } }

        public UserAddEdit(string userID = "", bool isUserManagement = false)
        {
            InitializeComponent();
            UserID = userID;
            UserManagementMode = isUserManagement;
        }

        private void UserAddEdit_Load(object sender, EventArgs e)
        {
            InitControls();
        }

        private void InitControls()
        {
            string strErrCode = string.Empty;
            string strErrText = string.Empty;
            DataTable dtUserLevel = sqlBiz.GetCommonCode("USER_LEVEL", ref strErrCode, ref strErrText);
            comboBox_UserLevel.DataSource = dtUserLevel;
            comboBox_UserLevel.DisplayMember = "CODE_NM_VALUE";
            comboBox_UserLevel.ValueMember = "CODE";

            if (AddMode)
            {
                this.Text = label_Title.Text = "Add User";
                foreach (Control ctrl in this.Controls)
                {
                    if (ctrl is TextBox)
                    {
                        ctrl.Text = string.Empty;
                    }
                }
                comboBox_UserLevel.SelectedIndex = 0;
                textBox_UserID.Enabled = true;
            }
            else
            {
                this.Text = "Edit User Info";
                label_Title.Text = $"Edit User Info [ {UserID} ]";
                textBox_UserID.Text = UserID;
                if (UserManagementMode)
                {
                    textBox_Password.Enabled = textBox_PasswordConfirm.Enabled = false;
                    button_InitializePassword.Visible = true;
                }
                else
                {
                    comboBox_UserLevel.Enabled = false;
                }

                DataTable dtUser = sqlBiz.GetUserInfo(UserID, ref strErrCode, ref strErrText);
                if (dtUser != null && dtUser.Rows.Count > 0)
                {
                    comboBox_UserLevel.SelectedValue = int.Parse(dtUser.Rows[0]["USER_LEVEL"].ToString());
                    textBox_Password.Text = UserAuthentication.Decrypt(dtUser.Rows[0]["PASSWORD"].ToString());
                    textBox_PasswordConfirm.Text = textBox_Password.Text;
                    textBox_UserName.Text = dtUser.Rows[0]["USER_NAME"].ToString();
                    textBox_Department.Text = dtUser.Rows[0]["DEPARTMENT"].ToString();
                    textBox_Description.Text = dtUser.Rows[0]["DESCRIPTION"].ToString();
                    if (comboBox_UserLevel.SelectedValue == null)
                    {
                        comboBox_UserLevel.SelectedIndex = 0;
                    }
                    textBox_UserID.Enabled = false;
                }
                else
                {
                    MessageBox.Show($"Error to read user info", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
                    this.Close();
                }
            }
        }

        private void button_Save_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(textBox_UserID.Text))
            {
                MessageBox.Show($"Enter User ID.", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            if (string.IsNullOrWhiteSpace(textBox_Password.Text))
            {
                MessageBox.Show($"Enter Password.", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            if (string.IsNullOrWhiteSpace(textBox_UserName.Text))
            {
                MessageBox.Show($"Enter User Name.", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            if (!textBox_Password.Text.Equals(textBox_PasswordConfirm.Text))
            {
                MessageBox.Show($"Password and Password Confirm are different.", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            string message = AddMode ? $"를 추가하시겠습니까?" : "를 저장하시겠습니까?";
            if (DialogResult.Yes.Equals(MessageBox.Show($"{textBox_UserID.Text} {message}", this.Text, MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2)))
            {
                string strErrText = string.Empty;
                if (UserAuthentication.InsertUpdateUserInfo(AddMode, textBox_UserID.Text.Trim(), textBox_Password.Text.Trim(), textBox_UserName.Text.Trim(), (UserLevel)int.Parse(comboBox_UserLevel.SelectedValue.ToString()), 
                    textBox_Department.Text, textBox_Description.Text, ref strErrText))
                {
                    MessageBox.Show($"User info has been saved.", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
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
            if (UserAuthentication.UserLevel.Equals(UserLevel.UnAuthorized) || DialogResult.Yes.Equals(MessageBox.Show($"Do you want to exit without saving ?", this.Text, MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2)))
            {
                DialogResult = DialogResult.Cancel;
                this.Close();
            }
        }

        private void button_InitializePassword_Click(object sender, EventArgs e)
        {
            string strErrText = string.Empty;
            if (UserAuthentication.InitializePassword(textBox_UserID.Text, ref strErrText))
            {
                MessageBox.Show($"Password has been initialized with the UserID.", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                MessageBox.Show($"Failed to initialize password. - {strErrText}", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
