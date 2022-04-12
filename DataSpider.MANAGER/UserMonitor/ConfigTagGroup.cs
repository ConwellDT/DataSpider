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
    public partial class ConfigTagGroup : Form
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

        public ConfigTagGroup()
        {
            InitializeComponent();
        }

        private void ConfigTagGroup_Load(object sender, EventArgs e)
        {
            InitControls();
        }

        private void labelTitle_MouseDown(object sender, MouseEventArgs e)
        {
            ReleaseCapture();
            SendMessage(this.Handle, WM_NCLBUTTONDOWN, HT_CAPTION, 0);
        }

        private void InitControls()
        {
            string strErrCode = string.Empty;
            string strErrText = string.Empty;

            DataTable dtEquiptype = sqlBiz.GetCommonCode("EQUIP_TYPE", ref strErrCode, ref strErrText);
            if (strErrCode == null || strErrCode == string.Empty)
            {
                comboBox_EquipType.DataSource = dtEquiptype;
                comboBox_EquipType.DisplayMember = "CODE_NM_VALUE";
                comboBox_EquipType.ValueMember = "CODE";
                comboBox_EquipType.SelectedIndex = 0;
            }
            else
            {
                MessageBox.Show(strErrText, $"Equipment type read fail", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void ShowGroupInfoSelected()
        {
            string strErrCode = string.Empty;
            string strErrText = string.Empty;

            if (comboBoxGroupSel.Items.Count > 0)
            {
                if (comboBoxGroupSel.SelectedIndex < 0) comboBoxGroupSel.SelectedIndex = 0;

                DataRowView selRow = (DataRowView)comboBoxGroupSel.SelectedItem;
                string selGrpName = selRow["GROUP_NM"].ToString();
                string selGrpDesc = selRow["GROUP_DESC"].ToString();

                labelGroupDesc.Text = selGrpDesc;

                DataTable dtEquiptype = sqlBiz.GetCommonCode("EQUIP_TYPE", ref strErrCode, ref strErrText);

                string strSelEQTypeCD = dtEquiptype.Rows[comboBox_EquipType.SelectedIndex]["CODE"].ToString();
                string strSelEQTypeNM = dtEquiptype.Rows[comboBox_EquipType.SelectedIndex]["CODE_NM"].ToString();

                DataTable dtGroups = sqlBiz.GetTagGroupByEQType(strSelEQTypeCD, ref strErrCode, ref strErrText);

                DataRow[] drGroupSel = dtGroups.Select($"GROUP_NM = '{selGrpName}'");

                DataTable dtGropTagNames = sqlBiz.GetTagGroupInfo(selGrpName, ref strErrCode, ref strErrText);

                if (strErrCode == null || strErrCode == string.Empty)
                {
                    for (int nT = 0; nT < checkedLBoxTagList.Items.Count; nT++)
                    {
                        DataRow[] drTagName = dtGropTagNames.Select($"TAG_NM = '{checkedLBoxTagList.Items[nT].ToString()}'");

                        if (drTagName != null && drTagName.Length > 0)
                            checkedLBoxTagList.SetItemCheckState(nT, CheckState.Checked);
                        else
                            checkedLBoxTagList.SetItemCheckState(nT, CheckState.Unchecked);
                    }
                }
                else
                {
                    MessageBox.Show("Group list is empty", $"Group info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            else
            {
                MessageBox.Show("No group created yet", $"Group info", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void ShowEQTypeSelected()
        {
            string strErrCode = string.Empty;
            string strErrText = string.Empty;

            if (checkedLBoxTagList.Items.Count > 0) checkedLBoxTagList.Items.Clear();
            //if(comboBoxGroupSel.Items.Count > 0 ) comboBoxGroupSel.Items.Clear();

            DataTable dtEquiptype = sqlBiz.GetCommonCode("EQUIP_TYPE", ref strErrCode, ref strErrText);
            if (strErrCode == null || strErrCode == string.Empty)
            {
                if (dtEquiptype.Rows.Count > 0)
                {
                    string strSelEQTypeCD = dtEquiptype.Rows[comboBox_EquipType.SelectedIndex]["CODE"].ToString();
                    string strSelEQTypeNM = dtEquiptype.Rows[comboBox_EquipType.SelectedIndex]["CODE_NM"].ToString();

                    DataTable dtEquipment = sqlBiz.GetEquipmentInfo(strSelEQTypeNM, "", true, ref strErrCode, ref strErrText);

                    if (strErrCode == null || strErrCode == string.Empty)
                    {
                        if (dtEquipment.Rows.Count > 0)
                        {
                            DataRow drow = dtEquipment.Rows[0];

                            DataTable dtTagList = sqlBiz.GetTagInfoByEquip(drow["EQUIP_NM"].ToString(), ref strErrCode, ref strErrText);

                            foreach (DataRow rowVal in dtTagList.Rows)
                            {
                                String strTagName = rowVal["Tag Name"].ToString();
                                int nPosEQName = strTagName.IndexOf("_", 0);
                                strTagName = strTagName.Substring(nPosEQName + 1, strTagName.Length - nPosEQName - 1);

                                checkedLBoxTagList.Items.Add(strTagName);
                            }

                            DataTable dtGroups = sqlBiz.GetTagGroupByEQType(strSelEQTypeCD, ref strErrCode, ref strErrText);

                            if (strErrCode == null || strErrCode == string.Empty)
                            {
                                comboBoxGroupSel.DataSource = dtGroups;

                                comboBoxGroupSel.DisplayMember = "GROUP_NM";

                                ShowGroupInfoSelected();
                            }
                            else
                            {
                                MessageBox.Show(strErrText, $"Group info read fail", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            }
                        }
                        else
                        {
                            MessageBox.Show("No equipment info exist", $"Group info read fail", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                    }
                    else
                    {
                        MessageBox.Show("Equipment info read fail", $"Group info read fail", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
                else
                {
                    MessageBox.Show("Equipment type is not selected", $"Group info read fail", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            else
            {
                MessageBox.Show(strErrText, $"Equipment type read fail", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void comboBox_EquipType_SelectedIndexChanged(object sender, EventArgs e)
        {
            ShowEQTypeSelected();
        }

        private void comboBoxGroupSel_SelectedIndexChanged(object sender, EventArgs e)
        {
            ShowGroupInfoSelected();
        }

        private void labelBTAdd_Click(object sender, EventArgs e)
        {
            string strErrCode = string.Empty;
            string strErrText = string.Empty;

            if (comboBox_EquipType.SelectedIndex < 0 || comboBox_EquipType.Items.Count < 1)
            {
                MessageBox.Show("Equipment type is not selected", $"Group Add", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            DataTable dtEquiptype = sqlBiz.GetCommonCode("EQUIP_TYPE", ref strErrCode, ref strErrText);
            string strSelEQTypeCD = dtEquiptype.Rows[comboBox_EquipType.SelectedIndex]["CODE"].ToString();

            TagGroupInfoEdit dlg = new TagGroupInfoEdit(strSelEQTypeCD, "", "", TagGroupInfoEdit.EDIT_MODE_ADD);
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                String strGrpName = dlg.strGroupName;
                String strGrpDesc = dlg.strDesc;

                List<string> checkedTag = new List<string>();

                for (int nC = 0; nC < checkedLBoxTagList.CheckedItems.Count; nC++)
                {
                    checkedTag.Add(checkedLBoxTagList.CheckedItems[nC].ToString());
                }

                if (checkedTag.Count < 1)
                {
                    MessageBox.Show("No tags are selected", $"Group Add", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                if (sqlBiz.InsertTagGroupInfo(strGrpName, checkedTag, ref strErrCode, ref strErrText) == true)
                {
                    if (sqlBiz.InsertTagGroup(strGrpName, strSelEQTypeCD, strGrpDesc, ref strErrCode, ref strErrText) != true)
                    {
                        MessageBox.Show(strErrText, $"Group add fail", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return;
                    }

                    ShowEQTypeSelected();
                }
                else
                {
                    MessageBox.Show(strErrText, $"Group add fail", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }
            }
        }

        private void labelBTEdit_Click(object sender, EventArgs e)
        {
            string strErrCode = string.Empty;
            string strErrText = string.Empty;

            if (comboBox_EquipType.SelectedIndex < 0 || comboBox_EquipType.Items.Count < 1)
            {
                MessageBox.Show("Equipment type is not selected", $"Group Edit", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            DataTable dtEquiptype = sqlBiz.GetCommonCode("EQUIP_TYPE", ref strErrCode, ref strErrText);
            string strSelEQTypeCD = dtEquiptype.Rows[comboBox_EquipType.SelectedIndex]["CODE"].ToString();

            DataRowView selRow = (DataRowView)comboBoxGroupSel.SelectedItem;
            string selGrpName = selRow["GROUP_NM"].ToString();
            string selGrpDesc = selRow["GROUP_DESC"].ToString();

            TagGroupInfoEdit dlg = new TagGroupInfoEdit(strSelEQTypeCD, selGrpName, selGrpDesc, TagGroupInfoEdit.EDIT_MODE_UPDATE);
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                String strGrpName = dlg.strGroupName;
                String strGrpDesc = dlg.strDesc;

                List<string> checkedTag = new List<string>();

                List<string> addedTag = new List<string>();
                List<string> removedTag = new List<string>();

                for (int nC = 0; nC < checkedLBoxTagList.CheckedItems.Count; nC++)
                {
                    checkedTag.Add(checkedLBoxTagList.CheckedItems[nC].ToString());
                }

                DataTable dtGroups = sqlBiz.GetTagGroupByEQType(strSelEQTypeCD, ref strErrCode, ref strErrText);

                if (comboBoxGroupSel.Items.Count > 0)
                {
                    DataRow[] drGroupSel = dtGroups.Select($"GROUP_NM = '{selGrpName}'");

                    DataTable dtGropTagNames = sqlBiz.GetTagGroupInfo(selGrpName, ref strErrCode, ref strErrText);

                    if (strErrCode == null || strErrCode == string.Empty)
                    {
                        for (int nT = 0; nT < checkedLBoxTagList.Items.Count; nT++)
                        {
                            DataRow[] drTagName = dtGropTagNames.Select($"TAG_NM = '{checkedLBoxTagList.Items[nT].ToString()}'");

                            if (checkedLBoxTagList.GetItemCheckState(nT) == CheckState.Checked)
                            {
                                if (drTagName != null && drTagName.Length > 0)
                                {
                                }
                                else
                                {
                                    addedTag.Add(checkedLBoxTagList.Items[nT].ToString());
                                }
                            }
                            else
                            {
                                if (drTagName != null && drTagName.Length > 0)
                                {
                                    removedTag.Add(checkedLBoxTagList.Items[nT].ToString());
                                }
                                else
                                {
                                }
                            }
                        }
                    }
                    else
                    {
                        MessageBox.Show("Group list is empty", $"Group info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
                else
                {
                    MessageBox.Show("No group exist yet", $"Group info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                if (sqlBiz.InsertTagGroupInfo(strGrpName, addedTag, ref strErrCode, ref strErrText) != true)
                {
                    MessageBox.Show(strErrText, $"Group edit fail (add)", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }

                if (sqlBiz.DeleteTagGroupInfo(strGrpName, removedTag, ref strErrCode, ref strErrText) != true)
                {
                    MessageBox.Show(strErrText, $"Group edit fail (removce)", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }

                if (sqlBiz.UpdateTagGroup(strGrpName, strSelEQTypeCD, strGrpDesc, ref strErrCode, ref strErrText) != true)
                {
                    MessageBox.Show(strErrText, $"Group edit fail", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }

                dtGroups = sqlBiz.GetTagGroupByEQType(strSelEQTypeCD, ref strErrCode, ref strErrText);

                if (strErrCode == null || strErrCode == string.Empty)
                {
                    comboBoxGroupSel.DataSource = dtGroups;

                    comboBoxGroupSel.DisplayMember = "GROUP_NM";

                    ShowGroupInfoSelected();
                }
                else
                {
                    MessageBox.Show(strErrText, $"Group info read fail", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }

        private void labelBTRemove_Click(object sender, EventArgs e)
        {
            string strErrCode = string.Empty;
            string strErrText = string.Empty;

            if (comboBox_EquipType.SelectedIndex < 0 || comboBox_EquipType.Items.Count < 1)
            {
                MessageBox.Show("Equipment type is not selected", $"Group Delete", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            DataTable dtEquiptype = sqlBiz.GetCommonCode("EQUIP_TYPE", ref strErrCode, ref strErrText);
            string strSelEQTypeCD = dtEquiptype.Rows[comboBox_EquipType.SelectedIndex]["CODE"].ToString();

            DataRowView selRow = (DataRowView)comboBoxGroupSel.SelectedItem;
            string selGrpName = selRow["GROUP_NM"].ToString();
            string selGrpDesc = selRow["GROUP_DESC"].ToString();

            if (String.IsNullOrEmpty(selGrpName) == true)
            {
                MessageBox.Show("Group is not selected", $"Group Delete", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (MessageBox.Show($"Remove Group (" + selGrpName + ") ?", $"Delete Group", MessageBoxButtons.OKCancel, MessageBoxIcon.Information) == DialogResult.OK)
            {
                if (sqlBiz.DeleteTagGroupInfo(selGrpName, null, ref strErrCode, ref strErrText) != true)
                {
                    MessageBox.Show(strErrText, $"Group remove fail (remove tags)", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }
                else
                {
                    if (sqlBiz.DeleteTagGroup(selGrpName, strSelEQTypeCD, ref strErrCode, ref strErrText) != true)
                    {
                        MessageBox.Show(strErrText, $"Group remove fail (remove group)", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return;
                    }
                }
            }

            ShowEQTypeSelected();
        }

        private void labelBTSelectAll_Click(object sender, EventArgs e)
        {
            for (int nL = 0; nL < checkedLBoxTagList.Items.Count; nL++)
            {
                checkedLBoxTagList.SetItemCheckState(nL, CheckState.Checked);
            }
        }

        private void labelBTDeselectAll_Click(object sender, EventArgs e)
        {
            for (int nL = 0; nL < checkedLBoxTagList.Items.Count; nL++)
            {
                checkedLBoxTagList.SetItemCheckState(nL, CheckState.Unchecked);
            }
        }

        private void labelBTExit_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
