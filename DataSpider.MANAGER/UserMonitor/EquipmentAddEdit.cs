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
    public partial class EquipmentAddEdit : Form
    {
        private PC00Z01 sqlBiz = new PC00Z01();
        private readonly string EquipName = string.Empty;
        private readonly string EquipTypeName = string.Empty;
        //
        // 2022. 2. 15 : Han, Ilho
        //  Add copy functionality
        //  --> Changeed all parts include this flag, not add comment for all
        //
        private readonly bool EditModeCopy = false;
        //
        private bool AddMode { get { return string.IsNullOrWhiteSpace(EquipName); } }

        public EquipmentAddEdit(string equipName = "", string equipTypeName = "", bool bEditModeCopy = false)
        {
            InitializeComponent();
            EquipName = equipName;
            EquipTypeName = equipTypeName;
            EditModeCopy = bEditModeCopy;
        }
        public EquipmentAddEdit(SBL node, bool bEditModeCopy = false)
        {
            InitializeComponent();
            if (node is Eq)
            {
                EquipName = node.Name;
            }
            // add
            else
            {
                EquipTypeName = node.Name;
            }
            EditModeCopy = bEditModeCopy;
        }

        private void EquipmentAddEdit_Load(object sender, EventArgs e)
        {
            InitControls();
        }

        private class ComboBoxItem
        {
            public int Value { get; set; }
            public string Text { get; set; }
        }

        private void InitControls()
        {
            string strErrCode = string.Empty;
            string strErrText = string.Empty;

            DataTable dtEquiptype = sqlBiz.GetCommonCode("EQUIP_TYPE", ref strErrCode, ref strErrText);
            comboBox_EquipType.DataSource = dtEquiptype;
            comboBox_EquipType.DisplayMember = "CODE_NM_VALUE";
            comboBox_EquipType.ValueMember = "CODE";

            DataTable dtInterfacetype = sqlBiz.GetCommonCode("IF_TYPE", ref strErrCode, ref strErrText);
            comboBox_InterfaceType.DataSource = dtInterfacetype;
            comboBox_InterfaceType.DisplayMember = "CODE_NM_VALUE";
            comboBox_InterfaceType.ValueMember = "CODE";

            DataTable dtServerName = sqlBiz.GetCommonCode("SERVER_CODE", ref strErrCode, ref strErrText);
            comboBox_ServerName.DataSource = dtServerName;
            comboBox_ServerName.DisplayMember = "CODE_NM_VALUE";
            comboBox_ServerName.ValueMember = "CODE_VALUE";

            DataTable dtZoneType = sqlBiz.GetCommonCode("ZONE_TYPE", ref strErrCode, ref strErrText);
            comboBox_ZoneType.DataSource = dtZoneType;
            comboBox_ZoneType.DisplayMember = "CODE_NM_VALUE";
            comboBox_ZoneType.ValueMember = "CODE";

            var failoverModeItems = new List<ComboBoxItem>
            {
                new ComboBoxItem { Value = 0, Text = "Manual" },
                new ComboBoxItem { Value = 1, Text = "Auto" }
            };

            var failoverItems = new List<ComboBoxItem>
            {
                new ComboBoxItem { Value = 0, Text = "Y" },
                new ComboBoxItem { Value = 1, Text = "N" }
            };

            comboBox_FailoverMode.DataSource = failoverModeItems;

            // ValueMember와 DisplayMember 설정
            comboBox_FailoverMode.ValueMember = "Value";
            comboBox_FailoverMode.DisplayMember = "Text";

            comboBox_Failover.DataSource = failoverItems;

            // ValueMember와 DisplayMember 설정
            comboBox_Failover.ValueMember = "Value";
            comboBox_Failover.DisplayMember = "Text";

            dataGridTagInfo.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;
            dataGridTagInfo.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.None;
            dataGridTagInfo.AllowUserToResizeRows = dataGridTagInfo.AllowUserToResizeColumns = true;
            //
            // 2022. 2. 16 : Han, Ilho
            //      Add tag editor UI
            //
            dataGridTagInfo.ColumnCount = 9;

            dataGridTagInfo.Columns[0].Name = "Message Type"; dataGridTagInfo.Columns[0].Width = 80;
            dataGridTagInfo.Columns[1].Name = "Tag Name"; dataGridTagInfo.Columns[1].Width = 120;
            dataGridTagInfo.Columns[2].Name = "Description"; dataGridTagInfo.Columns[2].Width = 400;
            dataGridTagInfo.Columns[3].Name = "Item Name"; dataGridTagInfo.Columns[3].Width = 300;
            dataGridTagInfo.Columns[4].Name = "PI Tag Name"; dataGridTagInfo.Columns[4].Width = 120;
            dataGridTagInfo.Columns[5].Name = "EventFrame Attribute Name"; dataGridTagInfo.Columns[5].Width = 120;
            dataGridTagInfo.Columns[6].Name = "Value Position"; dataGridTagInfo.Columns[6].Width = 80;
            dataGridTagInfo.Columns[7].Name = "Date Position"; dataGridTagInfo.Columns[7].Width = 80;
            dataGridTagInfo.Columns[8].Name = "Time Position"; dataGridTagInfo.Columns[8].Width = 80;

            dataGridTagInfo.EditMode = DataGridViewEditMode.EditOnKeystrokeOrF2;
            dataGridTagInfo.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;

            for (int nC = 0; nC < dataGridTagInfo.Columns.Count; nC++)
            {
                dataGridTagInfo.Columns[nC].SortMode = DataGridViewColumnSortMode.NotSortable;
            }
            //---------------------------------

            if (AddMode)
            {
                this.Text = label_Title.Text = "Add Equipment";
                if (!string.IsNullOrWhiteSpace(EquipTypeName))
                {
                    comboBox_EquipType.SelectedValue = dtEquiptype.Select($"CODE_NM = '{EquipTypeName}'")[0]["CODE"].ToString();
                    //comboBox_InterfaceType.SelectedValue = dtInterfacetype.Select($"CODE_NM = '{EquipTypeName}'")[0]["CODE"].ToString();
                    comboBox_InterfaceType.SelectedIndex = 0;
                    comboBox_ZoneType.SelectedIndex = 0;
                    comboBox_FailoverMode.SelectedIndex = 0;
                    comboBox_Failover.SelectedIndex = 0;
                }
                foreach (Control ctrl in this.Controls)
                {
                    if (ctrl is TextBox)
                    {
                        ctrl.Text = string.Empty;
                    }
                }
                comboBox_UseFlag.SelectedIndex = 0;
                textBox_EquipName.Enabled = true;
                comboBox_PiPointSave.SelectedIndex = 0;
                comboBox_EventFrameSave.SelectedIndex = 0;

                textBox_FailWait.Text = "60";
                textBox_DisconnectSet.Text = "0";
                textBox_DefaultServer.Text = "0";
            }
            else
            {
                if (UserAuthentication.UserLevel.Equals(UserLevel.UnAuthorized))
                {
                    this.Text = "Equipmemt Info";
                    label_Title.Text = $"Equipmemt [ {EquipName} ]";
                    button_Save.Visible = false;
                    button_Cancel.Text = "Close";
                }
                else
                {
                    if (EditModeCopy == false)
                    {
                        this.Text = "Edit Equipmemt";
                        label_Title.Text = $"Edit Equipmemt [ {EquipName} ]";
                    }
                    else
                    {
                        this.Text = "Copy Equipmemt";
                        label_Title.Text = $"Copy Equipmemt [ {EquipName} ]";

                        comboBox_Failover.SelectedValue = 0;
                        comboBox_FailoverMode.SelectedValue = 0;
                        comboBox_UseFlag.SelectedIndex = 0;
                        textBox_EquipName.Enabled = true;
                        comboBox_PiPointSave.SelectedIndex = 0;
                        comboBox_EventFrameSave.SelectedIndex = 0;
                    }
                }
                DataTable dtEquipment = sqlBiz.GetEquipmentInfo("", "", true, ref strErrCode, ref strErrText);

                //
                // 2022. 2. 16 : Han, Ilho
                //      : Get Tag list with Equip Name
                //
                DataTable dtTagList = sqlBiz.GetTagInfoByEquip(EquipName, ref strErrCode, ref strErrText);
                //----------------
                DataRow drEquipment = dtEquipment.Select($"EQUIP_NM = '{EquipName}'")[0];

                comboBox_EquipType.SelectedValue = int.Parse(drEquipment["EQUIP_TYPE"].ToString());
                comboBox_InterfaceType.SelectedValue = int.Parse(drEquipment["IF_TYPE"].ToString());
                comboBox_ZoneType.SelectedValue = int.Parse(drEquipment["ZONE_TYPE"].ToString());
                comboBox_FailoverMode.SelectedValue = int.Parse(drEquipment["FAILOVER_MODE"].ToString());
                comboBox_Failover.SelectedValue = int.Parse(drEquipment["FAILOVER"].ToString());


                dataGridTagInfo.Rows.Clear();
                textBox_ConnectionInfo.Text = drEquipment["CONNECTION_INFO"].ToString();

                textBox_ConfigInfo.Text = drEquipment["CONFIG_INFO"].ToString();
                if (EditModeCopy == false)
                {
                    textBox_EquipName.Text = drEquipment["EQUIP_NM"].ToString();

                    for (int nR = 0; nR < dtTagList.Rows.Count; nR++)
                    {
                        int rowId = dataGridTagInfo.Rows.Add();

                        DataGridViewRow row = dataGridTagInfo.Rows[rowId];

                        DataTable dtTag = sqlBiz.GetTagInfo(string.Empty, string.Empty, dtTagList.Rows[nR][3].ToString(), false, ref strErrCode, ref strErrText);

                        if (row != null && dtTag != null && row.Cells.Count > 0 && dtTag.Rows.Count > 0 && dtTag.Columns.Count > 0)
                        {
                            if (dtTag.Rows[0]["MSG_TYPE"] != null) row.Cells["Message Type"].Value = dtTag.Rows[0]["MSG_TYPE"].ToString();
                            if (dtTag.Rows[0]["TAG_NM"] != null) row.Cells["Tag Name"].Value = dtTag.Rows[0]["TAG_NM"].ToString();
                            if (dtTag.Rows[0]["TAG_DESC"] != null) row.Cells["Description"].Value = dtTag.Rows[0]["TAG_DESC"].ToString();
                            if (dtTag.Rows[0]["OPCITEM_NM"] != null) row.Cells["Item Name"].Value = dtTag.Rows[0]["OPCITEM_NM"].ToString();
                            if (dtTag.Rows[0]["PI_TAG_NM"] != null) row.Cells["PI Tag Name"].Value = dtTag.Rows[0]["PI_TAG_NM"].ToString();
                            if (dtTag.Rows[0]["EF_ATTRIBUTE_NM"] != null) row.Cells["EventFrame Attribute Name"].Value = dtTag.Rows[0]["EF_ATTRIBUTE_NM"].ToString();
                            if (dtTag.Rows[0]["DATA_POSITION"] != null) row.Cells["Value Position"].Value = dtTag.Rows[0]["DATA_POSITION"].ToString();
                            if (dtTag.Rows[0]["DATE_POSITION"] != null) row.Cells["Date Position"].Value = dtTag.Rows[0]["DATE_POSITION"].ToString();
                            if (dtTag.Rows[0]["TIME_POSITION"] != null) row.Cells["Time Position"].Value = dtTag.Rows[0]["TIME_POSITION"].ToString();
                        }
                    }
                }
                else
                {
                    //int[] nSPos = { -1, -1, -1 };

                    //String strConStr = drEquipment["CONNECTION_INFO"].ToString();

                    //nSPos[0] = strConStr.IndexOf("Data Source", 0);
                    //if (nSPos[0] < 0) nSPos[0] = strConStr.IndexOf("data source", 0);
                    //if (nSPos[0] < 0) nSPos[0] = strConStr.IndexOf("Data source", 0);
                    //if (nSPos[0] < 0) nSPos[0] = strConStr.IndexOf("data Source", 0);

                    //if (nSPos[0] >= 0)
                    //{
                    //    nSPos[1] = strConStr.IndexOf("=", nSPos[0] + 1);
                    //    nSPos[2] = strConStr.IndexOf(";", nSPos[1] + 1);

                    //    textBox_ConnectionInfo.Text = strConStr.Substring(nSPos[0], nSPos[1] - nSPos[0]) + "= XXX.XXX.XXX.XXX" + strConStr.Substring(nSPos[2], strConStr.Length - nSPos[2]);
                    //}
                    //else
                    //{
                    //    nSPos[0] = strConStr.IndexOf(@"\\", 0);

                    //    if (nSPos[0] >= 0)
                    //    {
                    //        nSPos[1] = strConStr.IndexOf(@"\", nSPos[0] + 2);

                    //        if (nSPos[1] > 0)
                    //        {
                    //            textBox_ConnectionInfo.Text = @"\\" + "XXX.XXX.XXX.XXX" + strConStr.Substring(nSPos[1], strConStr.Length - nSPos[1]);
                    //        }
                    //        else
                    //        {
                    //            textBox_ConnectionInfo.Text = @"\\XXX.XXX.XXX.XXX\";
                    //        }
                    //    }
                    //    else
                    //    {
                    //        textBox_ConnectionInfo.Text = String.Empty;
                    //    }
                    //}

                    textBox_EquipName.Text = String.Empty;

                    for (int nR = 0; nR < dtTagList.Rows.Count; nR++)
                    {
                        int rowId = dataGridTagInfo.Rows.Add();

                        DataGridViewRow row = dataGridTagInfo.Rows[rowId];

                        DataTable dtTag = sqlBiz.GetTagInfo(string.Empty, string.Empty, dtTagList.Rows[nR][3].ToString(), false, ref strErrCode, ref strErrText);

                        if (row != null && dtTag != null && row.Cells.Count > 0 && dtTag.Rows.Count > 0 && dtTag.Columns.Count > 0)
                        {
                            if (dtTag.Rows[0]["MSG_TYPE"] != null) row.Cells["Message Type"].Value = dtTag.Rows[0]["MSG_TYPE"].ToString();
                            if (dtTag.Rows[0]["TAG_NM"] != null)
                            {
                                String strTag = dtTag.Rows[0]["TAG_NM"].ToString();
                                int nPos = strTag.IndexOf("_", 0);
                                if (nPos > 0) row.Cells["Tag Name"].Value = strTag.Substring(nPos, strTag.Length - nPos);
                                else row.Cells["Tag Name"].Value = strTag;
                            }
                            if (dtTag.Rows[0]["TAG_DESC"] != null) row.Cells["Description"].Value = dtTag.Rows[0]["TAG_DESC"].ToString();
                            if (dtTag.Rows[0]["OPCITEM_NM"] != null) row.Cells["Item Name"].Value = dtTag.Rows[0]["OPCITEM_NM"].ToString();
                            if (dtTag.Rows[0]["PI_TAG_NM"] != null)
                            {
                                String strTag = dtTag.Rows[0]["PI_TAG_NM"].ToString();
                                int nPos = strTag.IndexOf("_", 0);
                                if (nPos > 0) row.Cells["PI Tag Name"].Value = strTag.Substring(nPos, strTag.Length - nPos);
                                else row.Cells["PI Tag Name"].Value = strTag;
                            }
                            if (dtTag.Rows[0]["EF_ATTRIBUTE_NM"] != null) row.Cells["EventFrame Attribute Name"].Value = dtTag.Rows[0]["EF_ATTRIBUTE_NM"].ToString();
                            if (dtTag.Rows[0]["DATA_POSITION"] != null) row.Cells["Value Position"].Value = dtTag.Rows[0]["DATA_POSITION"].ToString();
                            if (dtTag.Rows[0]["DATE_POSITION"] != null) row.Cells["Date Position"].Value = dtTag.Rows[0]["DATE_POSITION"].ToString();
                            if (dtTag.Rows[0]["TIME_POSITION"] != null) row.Cells["Time Position"].Value = dtTag.Rows[0]["TIME_POSITION"].ToString();
                        }
                    }
                }
                for (int nR = 0; nR < dataGridTagInfo.Rows.Count; nR++)
                {
                    for (int nC = 0; nC < dataGridTagInfo.Columns.Count; nC++)
                    {
                        dataGridTagInfo.Rows[nR].Cells[nC].ReadOnly = false;
                    }
                }

                textBox_Description.Text = drEquipment["EQUIP_DESC"].ToString();
                textBox_ExtraInfo.Text = drEquipment["EXTRA_INFO"].ToString();
                comboBox_ServerName.SelectedValue = drEquipment["SERVER_NM"].ToString();
                if (comboBox_ServerName.SelectedValue == null)
                {
                    comboBox_ServerName.SelectedIndex = 0;
                }

                comboBox_UseFlag.Text = drEquipment["USE_FLAG"].ToString().ToUpper();
                comboBox_PiPointSave.Text = drEquipment["UPDATE_PIPOINT_FLAG"].ToString().ToUpper();
                comboBox_EventFrameSave.Text = drEquipment["UPDATE_EVENTFRAME_FLAG"].ToString().ToUpper();

                //20240828 dayeong
                textBox_FailWait.Text = drEquipment["FAIL_WAIT"].ToString();
                textBox_DisconnectSet.Text = drEquipment["DISCONNECT_SET"].ToString();
                textBox_DefaultServer.Text = drEquipment["DEFAULT_SERVER"].ToString();

                if (EditModeCopy == false)
                {
                    textBox_EquipName.Enabled = false;
                }
            }
            dataGridTagInfo.Columns[1].ReadOnly = true;
            dataGridTagInfo.AutoResizeColumns(DataGridViewAutoSizeColumnsMode.DisplayedCells);
        }

        private void button_Save_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(textBox_EquipName.Text))
            {
                MessageBox.Show($"Enter Equipment Name.", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (dataGridTagInfo.Rows.Count <= 1)
            {
                if (DialogResult.No.Equals(MessageBox.Show($"{textBox_EquipName.Text} has no TAG info, save anyway?", this.Text, MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2)))
                {
                    MessageBox.Show($"Enter Equipment Name.", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }
            }

            string message = AddMode ? $"를 추가하시겠습니까?" : "를 저장하시겠습니까?";
            if (DialogResult.Yes.Equals(MessageBox.Show($"{textBox_EquipName.Text} {message}", this.Text, MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2)))
            {
                string strErrCode = string.Empty;
                string strErrText = string.Empty;

                DataRow[] drSelectEquip = null;

                if (AddMode == true || EditModeCopy == true) // Check EQ Exist
                {
                    DataTable dtEquipment = sqlBiz.GetEquipmentInfo("", "", true, ref strErrCode, ref strErrText);

                    //drSelectEquip = dtEquipment.Select($"EQUIP_NM = '{textBox_EquipName.Text.Trim()}' AND EQUIP_TYPE = '{comboBox_EquipType.SelectedValue.ToString()}'");
                    drSelectEquip = dtEquipment.Select($"EQUIP_NM = '{textBox_EquipName.Text.Trim()}'");
                    if (drSelectEquip != null && drSelectEquip.Length > 0)
                    {
                        MessageBox.Show($"Equipment Name ({textBox_EquipName.Text}) already exists", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return;
                    }
                }

                bool EQSaveMode = false;

                if (AddMode == true || EditModeCopy == true) EQSaveMode = true;

                if (sqlBiz.InsertUpdateEquipmentInfo(EQSaveMode, textBox_EquipName.Text.Trim(), textBox_Description.Text.Trim(), comboBox_EquipType.SelectedValue.ToString(),
                    comboBox_InterfaceType.SelectedValue.ToString(), textBox_ConnectionInfo.Text, textBox_ExtraInfo.Text, comboBox_ServerName.SelectedValue.ToString(),
                    comboBox_UseFlag.Text, textBox_ConfigInfo.Text, comboBox_ZoneType.SelectedValue.ToString(), comboBox_PiPointSave.Text,
                    comboBox_EventFrameSave.Text, Convert.ToInt32(textBox_FailWait.Text), Convert.ToInt32(comboBox_FailoverMode.SelectedValue.ToString()), Convert.ToInt32(comboBox_Failover.SelectedValue.ToString()), Convert.ToInt32(textBox_DisconnectSet.Text), ref strErrCode, ref strErrText))
                {
                    // Save Trag Info
                    int nTagSaveFailCount = 0;
                    bool TagAdd = true;

                    List<String> tagNameList = new List<string>();

                    for (int nR = 0; nR < dataGridTagInfo.Rows.Count; nR++)
                    {
                        DataGridViewRow row = dataGridTagInfo.Rows[nR];

                        if (row.Cells["Tag Name"].Value != null)
                        {
                            tagNameList.Add(row.Cells["Tag Name"].Value.ToString().Trim());

                            DataTable dtTag = sqlBiz.GetTagInfo(string.Empty, string.Empty, row.Cells["Tag Name"].Value.ToString().Trim(), false, ref strErrCode, ref strErrText);

                            if (dtTag != null && dtTag.Rows.Count > 0) TagAdd = false;

                            if (sqlBiz.InsertUpdateTagInfo(TagAdd,
                                                            row.Cells["Tag Name"].Value?.ToString().Trim(),
                                                            row.Cells["Message Type"].Value?.ToString().Trim(),
                                                            textBox_EquipName.Text.Trim(),
                                                            row.Cells["Description"].Value?.ToString().Trim(),
                                                            row.Cells["PI Tag Name"].Value?.ToString().Trim(),
                                                            row.Cells["EventFrame Attribute Name"].Value?.ToString().Trim(),
                                                            row.Cells["Value Position"].Value?.ToString().Trim(),
                                                            row.Cells["Date Position"].Value?.ToString().Trim(),
                                                            row.Cells["Time Position"].Value?.ToString().Trim(),
                                                            row.Cells["Item Name"].Value?.ToString().Trim(),
                                                            ref strErrCode, ref strErrText) == false)
                            {
                                MessageBox.Show($"Tag {row.Cells["Tag Name"].Value.ToString().Trim()}  저장 중 {strErrText} 오류가 발생하였습니다.", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);

                                nTagSaveFailCount++;
                            }
                        }

                        row.Dispose();
                    }

                    DataTable dtTagInDB = sqlBiz.GetTagInfoByEquip(textBox_EquipName.Text.Trim(), ref strErrCode, ref strErrText);

                    for (int nR = 0; nR < dtTagInDB.Rows.Count; nR++)
                    {
                        DataRow rowInDB = dtTagInDB.Rows[nR];

                        String strTagFound = tagNameList.Find(s => s == rowInDB["Tag Name"].ToString().Trim());

                        if (strTagFound == null)
                        {
                            if (sqlBiz.DeleteTagInfo(rowInDB["Tag Name"].ToString().Trim(), ref strErrCode, ref strErrText) == false)
                            {
                                MessageBox.Show($"Tag {rowInDB["Tag Name"].ToString().Trim()} 삭제 중 {strErrText} 오류가 발생하였습니다.", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
                            }
                        }
                    }

                    if (nTagSaveFailCount > 0)
                    {
                        MessageBox.Show($"Tag 저장 중 {nTagSaveFailCount.ToString()}번의 오류가 발생하였습니다.", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }

                    //nTagSaveFailCount = 0;

                    //DataTable dtEquiptype = sqlBiz.GetCommonCode("EQUIP_TYPE", ref strErrCode, ref strErrText);

                    //if (dtEquiptype != null && dtEquiptype.Rows.Count > 0)
                    //{
                    //    DataRow[] drEqiptype = dtEquiptype.Select($"CODE = '{comboBox_EquipType.SelectedValue.ToString()}'");

                    //    if (drEqiptype != null && drEqiptype.Length > 0)
                    //    {
                    //        String strIniName = @".\CFG\" + drEqiptype[0]["CODE_NM"].ToString() + ".ini";

                    //        try
                    //        {
                    //            String [] strSectionNames = PC00U01.GetSectionNames(strIniName);

                    //            if( strSectionNames.Length > 0)
                    //            {
                    //                String[] strKeys = PC00U01.GetEntryNames( strSectionNames[0], strIniName);

                    //                foreach( String strKey in strKeys )
                    //                {
                    //                    String strVal = PC00U01.ReadConfigValue(strKey, strSectionNames[0], strIniName);

                    //                    if( PC00U01.WriteConfigValue(strKey, textBox_EquipName.Text.Trim(), strIniName, strVal) == false )
                    //                    {
                    //                        MessageBox.Show($"Key {strKey}  저장 중 오류가 발생하였습니다.", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);

                    //                        nTagSaveFailCount++;
                    //                    }
                    //                }
                    //            }
                    //        }
                    //        catch
                    //        {
                    //            MessageBox.Show($"Config Ini file  저장 중 오류가 발생하였습니다.", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);

                    //            nTagSaveFailCount++;
                    //        }
                    //    }
                    //}

                    //if (nTagSaveFailCount > 0)
                    //{
                    //    MessageBox.Show($"Config Ini file 저장 중 {nTagSaveFailCount.ToString()}번의 오류가 발생하였습니다.", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    //}

                    MessageBox.Show($"Equipment info has been saved.", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
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

        private void textBox_EquipName_TextChanged(object sender, EventArgs e)
        {
            if (EditModeCopy == true)
            {
                for (int nR = 0; nR < dataGridTagInfo.Rows.Count; nR++)
                {
                    DataGridViewRow row = dataGridTagInfo.Rows[nR];

                    if (row != null && row.Cells.Count > 0)
                    {
                        if (row.Cells["Tag Name"].Value != null)
                        {
                            String strTag = row.Cells["Tag Name"].Value.ToString();
                            int nPos = strTag.IndexOf("_", 0);
                            if (nPos >= 0) row.Cells["Tag Name"].Value = textBox_EquipName.Text + strTag.Substring(nPos, strTag.Length - nPos);
                            else row.Cells["Tag Name"].Value = textBox_EquipName.Text;
                        }
                        if (row.Cells["PI Tag Name"].Value != null)
                        {
                            String strTag = row.Cells["PI Tag Name"].Value.ToString();
                            int nPos = strTag.IndexOf("_", 0);
                            if (nPos >= 0) row.Cells["PI Tag Name"].Value = textBox_EquipName.Text + strTag.Substring(nPos, strTag.Length - nPos);
                            else row.Cells["PI Tag Name"].Value = textBox_EquipName.Text;
                        }
                    }
                }
            }
        }
    }
}
