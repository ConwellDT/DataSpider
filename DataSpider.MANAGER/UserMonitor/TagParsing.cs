using System;
using System.Data;
using System.Text.Json;
using System.Windows.Forms;

using DataSpider.PC00.PT;

namespace DataSpider.UserMonitor
{
    public partial class TagParsing : Form
    {
        public delegate bool OnRefreshTreeDataDelegate();
        public event OnRefreshTreeDataDelegate OnRefreshTreeData = null;
        private PC00Z01 sqlBiz = new PC00Z01();
        private PC00M02 dataProcess = new PC00M02();
        private string equipType;
        private string equipmentName;
        private string zoneType;

        public TagParsing()
        {
            InitializeComponent();
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

            InitializeDataGridView(dataGridView_Main);
            InitializeDataGridView(dataGridView_EventFrame);

            DataTable dtProgramStatus = sqlBiz.GetMsgTypeTagValue(equipType.Trim(), equipName.Trim(), zoneType, msgType, ref strErrCode, ref strErrText);
            if (dtProgramStatus == null || dtProgramStatus.Rows.Count < 1)
            {

                return;
            }
            dataGridView_Main.DataSource = dtProgramStatus.DefaultView;

            DataTable dtProgramStatus2 = sqlBiz.GetCurrentMsgTypeEventFrameData(equipType.Trim(), equipName.Trim(), zoneType.Trim(), msgType, ref strErrCode, ref strErrText);
            if (dtProgramStatus2 == null || dtProgramStatus2.Rows.Count < 1)
            {
                dataGridView_Attributes.Rows.Clear();
                return;
            }
            dataGridView_EventFrame.DataSource = dtProgramStatus2.DefaultView;

            dataGridView_Attributes.Rows.Clear();
            AttributesGrid(dataGridView_EventFrame);

            RestoreScrollPosition(dataGridView_Main);
            RestoreScrollPosition(dataGridView_EventFrame);
        }

        private void InitializeDataGridView(DataGridView gridView)
        {
            gridView.RowTemplate.MinimumHeight = 30;
            gridView.DoubleBuffered(true);
            gridView.DataSource = null;
        }

        private void AttributesGrid(DataGridView eventFrameGrid)
        {

            DataView dv = eventFrameGrid.DataSource as DataView;
            if (dv == null) return;

            string attributes = dv[eventFrameGrid.SelectedCells[0].RowIndex]["Attributes"].ToString();
            using (JsonDocument document = JsonDocument.Parse(attributes))
            {
                JsonElement root = document.RootElement;
                if (root.ValueKind != JsonValueKind.Array || root.GetArrayLength() == 0) return;

                foreach (JsonElement jElement in root.EnumerateArray())
                {
                    int row = dataGridView_Attributes.Rows.Add();
                    dataGridView_Attributes.Rows[row].Cells["AttributeName"].Value = jElement.GetProperty("Name").GetString();
                    dataGridView_Attributes.Rows[row].Cells["AttributeValue"].Value = jElement.GetProperty("Value").GetString();
                }
            }
        }

        private void RestoreScrollPosition(DataGridView gridView)
        {
            if (gridView.DataSource != null)
            {
                int nHoriScrollOffset = gridView.HorizontalScrollingOffset;
                int nRowIndex = gridView.FirstDisplayedScrollingRowIndex;

                gridView.HorizontalScrollingOffset = Math.Max(nHoriScrollOffset, 0);

                if (gridView.Rows.Count > 0)
                {
                    int maxIndex = gridView.Rows.Count - 1;
                    gridView.FirstDisplayedScrollingRowIndex = Clamp(nRowIndex, 0, maxIndex);
                }
            }
        }

        // Custom clamp method
        private int Clamp(int value, int min, int max)
        {
            return Math.Max(min, Math.Min(value, max));
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
            dataProcess.ProcessData(dataGridView_Main, dataGridView_EventFrame ,dataGridView_Attributes);
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
