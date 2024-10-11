using System;
using System.Data;
using System.Windows.Forms;

using DataSpider.PC00.PT;

namespace DataSpider.UserMonitor
{
    public partial class TagParsing : Form
    {
        public delegate bool OnRefreshTreeDataDelegate();
        public event OnRefreshTreeDataDelegate OnRefreshTreeData = null;
        private PC00Z01 sqlBiz = new PC00Z01();
        private PC00M02 dataProcess;
        private string equipType;
        private string equipmentName;
        private string zoneType;

        public TagParsing()
        {
            InitializeComponent();
            dataProcess = new PC00M02(dataGridView_Main);
        }

        private void DateTimeParsing_Load(object sender, EventArgs e)
        {
            InitControls();
        }

        private void InitControls()
        {
            string strErrCode = string.Empty;
            string strErrText = string.Empty;

            DataTable dtCommonCd = sqlBiz.GetEquipmentInfo("", "", MonitorForm.showAllEquipmtStatus, ref strErrCode, ref strErrText);

            if (dtCommonCd.Rows.Count > 0)
            {
                for (int nR = 0; nR < dtCommonCd.Rows.Count; nR++)
                {
                    int rowId = dataGridEquipmentName.Rows.Add();

                    DataGridViewRow row = dataGridEquipmentName.Rows[rowId];

                    row.Cells["EquipmentName"].Value = dtCommonCd.Rows[nR]["EQUIP_NM"].ToString();
                }
            }
            EquipmentMsgType();
        }

        private void dataGridEquipmentName_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            EquipmentMsgType();
        }

        private void EquipmentMsgType()
        {
            string strErrCode = string.Empty;
            string strErrText = string.Empty;

            comboBoxMsgType.Items.Clear();

            DataGridViewRow currentRow = dataGridEquipmentName.CurrentRow;
            if (currentRow == null) return;

            equipmentName = currentRow.Cells["EquipmentName"].Value?.ToString() ?? string.Empty;
            DataTable dtMesType = sqlBiz.GetEquipmentMsgType(equipmentName, MonitorForm.showAllEquipmtStatus, ref strErrCode, ref strErrText);

            if (dtMesType == null || dtMesType.Rows.Count < 0) return;

            foreach (DataRow row in dtMesType.Rows)
            {
                comboBoxMsgType.Items.Add(row["MsgType"].ToString());
            }

            equipType = dtMesType.Rows[0]["EquipType"].ToString();
            zoneType = dtMesType.Rows[0]["ZoneType"].ToString();

            GetTagCurrentValues(equipType, equipmentName, zoneType, comboBoxMsgType.Text.Equals("") ? "%" : comboBoxMsgType.Text);
        }

        private void GetTagCurrentValues(string equipType, string equipName, string zoneType, string msgType)
        {
            string strErrCode = string.Empty;
            string strErrText = string.Empty;

            int nHoriScrollOffset = dataGridView_Main.HorizontalScrollingOffset;
            int nRowIndex = dataGridView_Main.FirstDisplayedScrollingRowIndex;

            dataGridView_Main.RowTemplate.MinimumHeight = 30;
            dataGridView_Main.DoubleBuffered(true);
            dataGridView_Main.DataSource = null;

            DataTable dtProgramStatus = sqlBiz.GetMsgTypeTagValue(equipType.Trim(), equipName.Trim(), zoneType, msgType, ref strErrCode, ref strErrText);
            if (dtProgramStatus == null || dtProgramStatus.Rows.Count < 1)
            {
                return;
            }

            DataView dvProgramStatus = dtProgramStatus.DefaultView;

            dataGridView_Main.DataSource = dvProgramStatus;

            if (dvProgramStatus.Count > 0)
            {
                dataGridView_Main.HorizontalScrollingOffset = nHoriScrollOffset > 0 ? nHoriScrollOffset : 0;

                if (dvProgramStatus.Count > nRowIndex)
                {
                    dataGridView_Main.FirstDisplayedScrollingRowIndex = nRowIndex > 0 ? nRowIndex : 0;
                }
                else
                {
                    dataGridView_Main.FirstDisplayedScrollingRowIndex = 0;
                }
            }
        }

        private void comboBoxMsgType_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBoxMsgType.SelectedItem != null) 
            {
                GetTagCurrentValues(equipType, equipmentName, zoneType, comboBoxMsgType.Text.Equals("") ? "%" : comboBoxMsgType.Text);
            }
        }

        private void EditToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (dataGridView_Main.SelectedRows.Count < 1)
            {
                return;
            }
            string tagName = dataGridView_Main.SelectedRows[0].Cells[3].Value.ToString();
            TagAddEdit form = new TagAddEdit(string.Empty, tagName);
            if (DialogResult.OK.Equals(form.ShowDialog(this)))
            {
                RefreshTreeView();
            }
        }

        private void AddToolStripMenuItem_Click(object sender, EventArgs e)
        {
            TagAddEdit form = new TagAddEdit(equipmentName);
            if (DialogResult.OK.Equals(form.ShowDialog(this)))
            {
                RefreshTreeView();
            }
        }

        private void DeleteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                if (dataGridView_Main.SelectedRows.Count < 1)
                {
                    return;
                }
                string tagName = dataGridView_Main.SelectedRows[0].Cells[3].Value.ToString();
                if (DialogResult.Yes.Equals(MessageBox.Show($"{tagName} 태그를 삭제하시겠습니까 ?", this.Text, MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2)))
                {
                    string strErrCode = string.Empty;
                    string strErrText = string.Empty;
                    if (sqlBiz.DeleteTagInfo(tagName, ref strErrCode, ref strErrText))
                    {
                        MessageBox.Show($"{tagName} 태그가 삭제되었습니다.", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
                        RefreshTreeView();
                    }
                    else
                    {
                        MessageBox.Show($"태그 삭제 중 오류가 발생하였습니다. {strErrCode} - {strErrText}", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"태그 삭제 중 오류가 발생하였습니다. {ex.Message}", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);

            }
        }

        private void button_Parse_Click(object sender, EventArgs e)
        {
            string strErrCode = string.Empty;
            string strErrText = string.Empty;

            if (string.IsNullOrWhiteSpace(comboBoxMsgType.Text))
            {
                MessageBox.Show("Message Type is not exist", $"Message Type invalid", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            string queueMessage = textBox1.Text;
            if (string.IsNullOrWhiteSpace(queueMessage))
            {
                MessageBox.Show("Queue message type is empty", "Queue Message Type Invalid", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            QueueMsg msg = new QueueMsg
            {
                m_EqName = equipmentName,
                m_MsgType = Convert.ToInt32(comboBoxMsgType.Text),
                m_Data = queueMessage
            };
            PC00U01.WriteQueue(msg);
            dataProcess.ProcessData(dataGridView_Main);
        }

        private void RefreshTreeView()
        {
            if (OnRefreshTreeData != null)
            {
                OnRefreshTreeData();
            }
        }
    }
}
