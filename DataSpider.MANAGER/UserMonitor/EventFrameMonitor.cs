﻿using System;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Windows.Forms;

using DataSpider.PC00.PT;

namespace DataSpider.UserMonitor
{
    public partial class EventFrameMonitor : LibraryWH.FormCtrl.UserForm
    {
        public delegate bool OnRefreshTreeDataDelegate();
        public event OnRefreshTreeDataDelegate OnRefreshTreeData = null;
        //private System.Windows.Forms.Timer timerRefresh = new System.Windows.Forms.Timer();
        private PC00Z01 sqlBiz = new PC00Z01();
        private string equipType = string.Empty;
        private string equipName = string.Empty;
        private string zoneType = string.Empty;
        private int selectedIndex = 0;
        private int autoRefreshInterval = 10;
        private bool formSelected = false;
        private bool needResizeColumn = true;
        private DateTime dtLastRefreshed = DateTime.MinValue;
        private Thread threadDataRefresh = null;
        public bool threadStop = false;
        private bool threadPause = false;
        private ManualResetEvent refreshDone = new ManualResetEvent(false);
        private bool treeSelectedNodeChanged = true;
        //
        // 2022. 3. 14 : Han. Ilho
        //      Add tag group property : To check if EQ Type is changed
        private string equipTypeCur = string.Empty;
        private int nDBModeCurrent = 1;
        //////////////////////////////
        private Form_Find finder = null;
        private int lastFoundIndex = 0;
        private string logviewProgram = "NotePad";
        private string TagNameFilter = "";
        private DateTime DateTimeFilterCurMin = DateTime.MinValue;
        private DateTime DateTimeFilterCurMax = DateTime.MinValue;
        private DateTime DateTimeFilterHistMin = DateTime.Now.AddDays(-1);// DateTime.MinValue;
        private DateTime DateTimeFilterHistMax = DateTime.Now;// DateTime.MinValue;
        private string DescriptionFilter = "";
        private bool autoRefresheChecked = true;

        public EventFrameMonitor()
        {
            InitializeComponent();
        }
        private void Form_Load(object sender, EventArgs e)
        {
            //buttonFilter.Visible = false;
            threadDataRefresh = new Thread(new ThreadStart(ThreadJob));
            threadDataRefresh.Start();

            UserLogInChanged();
            dataGridView_Main.RowTemplate.MinimumHeight = 30;
            dataGridView_Main.DoubleBuffered(true);
            //dataGridView_Main.CellMouseDoubleClick += DataGridView_Main_CellMouseDoubleClick;
            dataGridView_Main.CellMouseClick += DataGridView_Main_CellMouseClick;
            // None 로 해야 사용자 컬럼 사이즈 조절이 가능함. 
            // 바인딩 후 AutoResizeColumns(DataGridViewAutoSizeColumnsMode.DisplayedCells); 처리
            //            dataGridView_Main.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;
            dataGridView_Main.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;
            dataGridView_Main.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.None;
            dataGridView_Main.AllowUserToResizeRows = dataGridView_Main.AllowUserToResizeColumns = true;

            dataGridView_Attributes.DoubleBuffered(true);
            dataGridView_Attributes.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dataGridView_Attributes.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.None;
            dataGridView_Attributes.AllowUserToResizeRows = dataGridView_Attributes.AllowUserToResizeColumns = true;

            dateTimePicker_StartTime.Value = DateTime.Parse("00:00:00");
            dateTimePicker_EndTime.Value = DateTime.Parse("23:59:59");
            dateTimePicker_Start.Value = dateTimePicker_End.Value = DateTime.Now;

            label_MeasureTime.Visible = panel_FromTo.Visible = dateTimePicker_Start.Visible = dateTimePicker_StartTime.Visible = dateTimePicker_End.Visible = dateTimePicker_EndTime.Visible = button_InquireEFDataHistory.Visible = false;

            //
            // 2022. 3. 14 : Han, Ilho
            //  To activate dadagridview double buffer
            //
            //Type dgvType = dataGridView_Main.GetType();
            //PropertyInfo pi = dgvType.GetProperty("DoubleBuffered", BindingFlags.Instance | BindingFlags.NonPublic);
            //pi.SetValue(dataGridView_Main, true, null);
            //////////////////////////

            autoRefresheChecked = checkBox_AutoRefresh.Checked = ConfigHelper.GetAppSetting("TagAutoRefresh").Trim().ToUpper().Equals("Y");
            checkBox_AutoRefresh.CheckedChanged += checkBox_AutoRefresh_CheckedChanged;

            if (!int.TryParse(ConfigHelper.GetAppSetting("TagAutoRefreshInterval").Trim(), out autoRefreshInterval))
            {
                autoRefreshInterval = 10;
            }
            textBox_RefreshInterval.Text = autoRefreshInterval.ToString();
            //timerRefresh.Tick += TimerRefresh_Tick;
            //timerRefresh.Interval = autoRefreshInterval * 1000;
            //if (checkBox_AutoRefresh.Checked)
            //{
            //    timerRefresh.Start();
            //}
            logviewProgram = ConfigHelper.GetAppSetting("LogViewProgram").Trim();
        }

        private void DataGridView_Main_CellMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.RowIndex < 0 || e.RowIndex >= dataGridView_Main.Rows.Count)
            {
                return;
            }

            dataGridView_Attributes.Rows.Clear();
            label_SelectedEventFrameName.Text = "";

            DataView dv = dataGridView_Main.DataSource as DataView;
            label_SelectedEventFrameName.Text = dv[e.RowIndex]["EventFrame Name"].ToString();
            string attributes = dv[e.RowIndex]["Attributes"].ToString();

            JsonDocument document = JsonDocument.Parse(attributes);
            JsonElement root = document.RootElement;
            if (root.ValueKind != JsonValueKind.Array || root.GetArrayLength() == 0) return;

            //DataTable dt = dataGridView_Attributes.Columns[].DataSource as DataTable;

            foreach (JsonElement jElement in root.EnumerateArray())
            {
                //dt.Rows.Add(jElement.GetProperty("Name").GetString(), jElement.GetProperty("Value").GetString());
                int row = dataGridView_Attributes.Rows.Add();
                dataGridView_Attributes.Rows[row].Cells["AttributeName"].Value = jElement.GetProperty("Name").GetString();
                dataGridView_Attributes.Rows[row].Cells["AttributeValue"].Value = jElement.GetProperty("Value").GetString();
            }
        }

        public void UserLogInChanged()
        {
            //switch (UserAuthentication.UserLevel)
            //{
            //    case UserLevel.Admin:
            //    case UserLevel.Manager:
            //        foreach (ToolStripItem item in contextMenuStrip1.Items)
            //        {
            //            item.Visible = true;
            //        }
            //        contextMenuStrip1.Items["editToolStripMenuItem"].Text = "Edit";
            //        break;
            //    default:
            //        contextMenuStrip1.Items["addToolStripMenuItem"].Visible = contextMenuStrip1.Items["deleteToolStripMenuItem"].Visible = false;
            //        contextMenuStrip1.Items["editToolStripMenuItem"].Text = "Tag Info";
            //        break;
            //}
        }

        public void TabControl_SelectedIndexChanged(LibraryWH.FormCtrl.UserForm ctrl)
        {
            formSelected = this.Name.Equals(ctrl.Name);
            if (formSelected)
            {
                if (treeSelectedNodeChanged)
                {
                    GetProgramStatus();
                    treeSelectedNodeChanged = false;
                }
            }
        }

        private void RefreshTreeView()
        {
            if (OnRefreshTreeData != null)
            {
                OnRefreshTreeData();
            }
        }

        //private void TimerRefresh_Tick(object sender, EventArgs e)
        //{
        //    if (formSelected)
        //    {
        //        selectedIndex = listView_Main.SelectedIndices.Count > 0 ? listView_Main.SelectedIndices[0] : 0;
        //        GetProgramStatus();
        //    }
        //}

        private void ThreadJob()
        {
            Thread.Sleep(1000);

            while (!threadStop)
            {
                if (threadPause)
                {
                    Thread.Sleep(1000);
                    continue;
                }
                if (dtLastRefreshed.AddSeconds(autoRefreshInterval).CompareTo(DateTime.Now) <= 0)
                {
                    GetProgramStatus();
                }
                Thread.Sleep(1000);
            }
        }

        private void GetCurrentValues()
        {
            string strErrCode = string.Empty;
            string strErrText = string.Empty;

            int nHoriScrollOffset = dataGridView_Main.HorizontalScrollingOffset;
            int nRowIndex = dataGridView_Main.FirstDisplayedScrollingRowIndex;

            dataGridView_Main.DataSource = null;

            DataTable dtProgramStatus = sqlBiz.GetCurrentEventFrameData(equipType.Trim(), equipName.Trim(), zoneType.Trim(), ref strErrCode, ref strErrText);
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
                dataGridView_Main.Rows[0].Selected = false;
                dataGridView_Main.Rows[selectedIndex].Selected = true;
            }
        }

        private void GetHistoryValues()
        {
            string strErrCode = string.Empty;
            string strErrText = string.Empty;

            int nHoriScrollOffset = dataGridView_Main.HorizontalScrollingOffset;
            int nRowIndex = dataGridView_Main.FirstDisplayedScrollingRowIndex;

            dataGridView_Main.DataSource = null;

            DataTable dtProgramStatus = sqlBiz.GetEventFrameDataHistory($"{dateTimePicker_Start.Value:yyyy-MM-dd} {dateTimePicker_StartTime.Value:HH:mm:ss}.000", $"{dateTimePicker_End.Value:yyyy-MM-dd} {dateTimePicker_EndTime.Value:HH:mm:ss}.000",  equipType.Trim(), equipName.Trim(), zoneType.Trim(), ref strErrCode, ref strErrText);
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
                dataGridView_Main.Rows[0].Selected = false;
                dataGridView_Main.Rows[selectedIndex].Selected = true;
            }
        }

        private void GetProgramStatus()
        {
            // 20210428, SHS, 값 업데이트 쓰레드 처리위해
            if (this.InvokeRequired)
            {
                this.Invoke((MethodInvoker)delegate { GetProgramStatus(); });
            }
            else
            {
                try
                {
                    //selectedIndex = dataGridView_Main.SelectedRows.Count > 0 ? dataGridView_Main.SelectedRows[0].Index : 0;
                    //if (checkBox_AutoRefresh.Checked)
                    //{
                    //    timerRefresh.Stop();
                    //}
                    if (!formSelected)
                    {
                        return;
                    }
                    string strErrCode = string.Empty;
                    string strErrText = string.Empty;

                    //if( string.IsNullOrEmpty(equipType) )
                    //{
                    //    DataTable dtEquiptype = sqlBiz.GetCommonCode("EQUIP_TYPE", ref strErrCode, ref strErrText);
                    //    equipType = dtEquiptype.Rows[0]["CODE_NM"].ToString();
                    //}
                    //if (equipTypeCur != equipType)
                    //{
                    //    UpdatecomboBoxTagGroupSel();
                    //    equipTypeCur = equipType;
                    //}
                    /////////////////////////////////
                    if( nDBModeCurrent == 1)
                    {
                        GetCurrentValues();
                    }
                    else
                    {
                        GetHistoryValues();
                    }
                    dataGridView_Main.AutoResizeColumns(DataGridViewAutoSizeColumnsMode.DisplayedCells);
                }
                catch (Exception ex)
                {

                }
                finally
                {
                    //if (checkBox_AutoRefresh.Checked)
                    //{
                    //    timerRefresh.Start();
                    //}
                    dtLastRefreshed = DateTime.Now;
                }
            }
        }

        public void treeView_AfterSelect(object sender, TreeViewEventArgs e)
        {
            SBL nodeTag = e.Node.Tag as SBL;
            string selectedEquipType = string.Empty;
            string selectedEquipName = string.Empty;
            string selectedZoneType = string.Empty;

            if (nodeTag is Zone)
            {
                selectedZoneType = (nodeTag as Zone).TypeCode;
            }
            if (nodeTag is EqType)
            {
                selectedEquipType = nodeTag.Name;
                selectedEquipName = string.Empty;
                selectedZoneType = nodeTag.GetData("ZONE_TYPE");
            }
            if (nodeTag is Eq)
            {
                selectedEquipType = (e.Node.Parent.Tag as SBL).Name;
                selectedEquipName = nodeTag.Name;
                selectedZoneType = nodeTag.GetData("ZONE_TYPE");
            }
            //if (!equipType.Equals(selectedEquipType) || !equipName.Equals(selectedEquipName))
            {
                //needResizeColumn = true;

                treeSelectedNodeChanged = true;
                equipType = selectedEquipType;
                equipName = selectedEquipName;
                zoneType = selectedZoneType;
                selectedIndex = 0;
                GetProgramStatus();
            }
        }
        private void EquipmentMonitor_FormClosed(object sender, FormClosedEventArgs e)
        {
            //timerRefresh.Stop();
            threadStop = true;
            int count = 0;
            if (threadDataRefresh != null)
            {
                while (threadDataRefresh.IsAlive)
                {
                    if (count++ > 200)
                    {
                        threadDataRefresh.Abort();
                        break;
                    }
                    Thread.Sleep(10);
                }
            }
            threadDataRefresh = null;
        }

        private void ValueHistoryToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void EditToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void AddToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void DeleteToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void checkBox_AutoRefresh_CheckedChanged(object sender, EventArgs e)
        {
            if ((sender as CheckBox).Checked)
            {
                threadPause = false;
                //button_Refresh.Enabled = false;
                //timerRefresh.Start();
                autoRefresheChecked = true;
            }
            else
            {
                threadPause = true;
                //button_Refresh.Enabled = true;
                //timerRefresh.Stop();
            }
        }

        private void button_SetInterval_Click(object sender, EventArgs e)
        {
            if (int.TryParse(textBox_RefreshInterval.Text, out int tempInterval))
            {
                //timerRefresh.Interval = tempInterval * 1000;
                autoRefreshInterval = tempInterval;
            }
        }


        private void button_Refresh_Click(object sender, EventArgs e)
        {
            //selectedIndex = listView_Main.SelectedIndices.Count > 0 ? listView_Main.SelectedIndices[0] : 0;
            GetProgramStatus();
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {

            Keys key = keyData & ~(Keys.Shift | Keys.Control | Keys.Alt);
            switch (key)
            {
                case Keys.F:
                    if ((keyData & Keys.Control) != 0)
                    {
                        if (finder == null)
                        {
                            finder = new Form_Find(this);
                        }
                        finder.Show();

                    }
                    break;
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }

        public override void Find(string find, bool top, bool caseSense)
        {
            bool found = false;
            if (!caseSense)
            {
                find = find.ToUpper();
            }
            //int selectdIndex = dataGridView_Main.SelectedRows[0].Index;
            if (top)
            {
                int startIndex = lastFoundIndex.Equals(selectedIndex) && selectedIndex > 0 ? selectedIndex - 1 : selectedIndex;
                for (int i = startIndex; i >= 0; i--)
                {
                    foreach (DataGridViewCell dgvc in dataGridView_Main.Rows[i].Cells)
                    {
                        string gridString = !caseSense ? dgvc.Value.ToString() : dgvc.Value.ToString().ToUpper();
                        if (gridString.Contains(find))
                        {
                            dataGridView_Main.Rows[i].Selected = true;
                            lastFoundIndex = selectedIndex = dataGridView_Main.FirstDisplayedScrollingRowIndex = i;
                            found = true;
                            break;
                        }
                    }
                    if (found)
                    {
                        break;
                    }
                }
            }
            else
            {
                int startIndex = lastFoundIndex.Equals(selectedIndex) && selectedIndex + 1 >= dataGridView_Main.Rows.Count ? selectedIndex : selectedIndex + 1;
                for (int i = startIndex; i < dataGridView_Main.Rows.Count; i++)
                {
                    foreach (DataGridViewCell dgvc in dataGridView_Main.Rows[i].Cells)
                    {
                        string gridString = !caseSense ? dgvc.Value.ToString() : dgvc.Value.ToString().ToUpper();
                        if (gridString.Contains(find))
                        {
                            dataGridView_Main.Rows[i].Selected = true;
                            lastFoundIndex = selectedIndex = dataGridView_Main.FirstDisplayedScrollingRowIndex = i;
                            found = true;
                            break;
                        }
                    }
                    if (found)
                    {
                        break;
                    }
                }
            }
        }

        private void dataGridView_Main_CellMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            selectedIndex = e.RowIndex;
        }

        private void button_Find_Click(object sender, EventArgs e)
        {
            if (finder == null)
            {
                finder = new Form_Find(this);
            }
            finder.Show();
        }

        private void ShowFile(string logData)
        {
            try
            {
                DataGridViewRow dgvr = dataGridView_Main.CurrentRow;
                if (dgvr != null)
                {

                    if (!DateTime.TryParse(dgvr.Cells[7].Value.ToString(), out DateTime dtReg))
                    {
                        dtReg = DateTime.Now;
                    }
                    string filePath = $@"{Directory.GetCurrentDirectory()}\LOG\{equipType}_{equipName}\{logData}_{equipType}_{equipName}_{dtReg:yyyyMMdd}.TXT";
                    // 20221212, SHS, V.2.0.4.0, 오픈 대상 파일 유무 확인 메서드 오류 수정
                    //if (Directory.Exists(filePath))
                    if (File.Exists(filePath))
                    {
                            Process.Start(logviewProgram, filePath);
                    }
                    else
                    {
                        MessageBox.Show("Log file not exists.", "System Log", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private void toolStripMenuItemLog_Click(object sender, EventArgs e)
        {
            try
            {
                ShowFile("LOG");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }

        }

        private void toolStripMenuItemData_Click(object sender, EventArgs e)
        {
            try
            {
                ShowFile("DATA");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }

        }

        private void radioButtonHistoryTag_CheckedChanged(object sender, EventArgs e)
        {
            // Current
            if (radioButtonCurTag.Checked == true)
            {
                nDBModeCurrent = 1;

                checkBox_AutoRefresh.Checked = autoRefresheChecked;
                button_Refresh.Visible = checkBox_AutoRefresh.Visible = button_SetInterval.Visible = textBox_RefreshInterval.Visible = label_RefreshInterval.Visible = true;
                label_MeasureTime.Visible = panel_FromTo.Visible = dateTimePicker_Start.Visible = dateTimePicker_StartTime.Visible = dateTimePicker_End.Visible = dateTimePicker_EndTime.Visible = button_InquireEFDataHistory.Visible = false;
            }
            // History
            else
            {
                nDBModeCurrent = 0;

                checkBox_AutoRefresh.Checked = false;
                button_Refresh.Visible = checkBox_AutoRefresh.Visible = button_SetInterval.Visible = textBox_RefreshInterval.Visible = label_RefreshInterval.Visible = false;
                label_MeasureTime.Visible = panel_FromTo.Visible = dateTimePicker_Start.Visible = dateTimePicker_StartTime.Visible = dateTimePicker_End.Visible = dateTimePicker_EndTime.Visible = button_InquireEFDataHistory.Visible = true;
            }
            GetProgramStatus();
        }

        private void button_InquireEFDataHistory_Click(object sender, EventArgs e)
        {
            GetProgramStatus();
        }

        //private void radioButtonHistoryTag_MouseClick(object sender, MouseEventArgs e)
        //{
        //    nDBModeCurrent = 0;

        //    checkBox_AutoRefresh.Checked = false;

        //    GetProgramStatus();
        //}

        //private void listView_Main_ColumnClick(object sender, ColumnClickEventArgs e)
        //{
        //    // Determine if clicked column is already the column that is being sorted.
        //    if (e.Column == lvwColumnSorter.SortColumn)
        //    {
        //        // Reverse the current sort direction for this column.
        //        if (lvwColumnSorter.Order == SortOrder.Ascending)
        //        {
        //            lvwColumnSorter.Order = SortOrder.Descending;
        //        }
        //        else
        //        {
        //            lvwColumnSorter.Order = SortOrder.Ascending;
        //        }
        //    }
        //    else
        //    {
        //        // Set the column number that is to be sorted; default to ascending.
        //        lvwColumnSorter.SortColumn = e.Column;
        //        lvwColumnSorter.Order = SortOrder.Ascending;
        //    }

        //    // Perform the sort with these new sort options.
        //    this.listView_Main.Sort();
        //}

    }

    /// <summary>
    /// This class is an implementation of the 'IComparer' interface.
    /// </summary>
    //public class ListViewColumnSorter : IComparer
    //{
    //    /// <summary>
    //    /// Specifies the column to be sorted
    //    /// </summary>
    //    private int ColumnToSort;

    //    /// <summary>
    //    /// Specifies the order in which to sort (i.e. 'Ascending').
    //    /// </summary>
    //    private SortOrder OrderOfSort;

    //    /// <summary>
    //    /// Case insensitive comparer object
    //    /// </summary>
    //    private CaseInsensitiveComparer ObjectCompare;

    //    /// <summary>
    //    /// Class constructor. Initializes various elements
    //    /// </summary>
    //    public ListViewColumnSorter()
    //    {
    //        // Initialize the column to '0'
    //        ColumnToSort = 0;

    //        // Initialize the sort order to 'none'
    //        OrderOfSort = SortOrder.None;

    //        // Initialize the CaseInsensitiveComparer object
    //        ObjectCompare = new CaseInsensitiveComparer();
    //    }

    //    /// <summary>
    //    /// This method is inherited from the IComparer interface. It compares the two objects passed using a case insensitive comparison.
    //    /// </summary>
    //    /// <param name="x">First object to be compared</param>
    //    /// <param name="y">Second object to be compared</param>
    //    /// <returns>The result of the comparison. "0" if equal, negative if 'x' is less than 'y' and positive if 'x' is greater than 'y'</returns>
    //    public int Compare(object x, object y)
    //    {
    //        int compareResult;
    //        ListViewItem listviewX, listviewY;

    //        // Cast the objects to be compared to ListViewItem objects
    //        listviewX = (ListViewItem)x;
    //        listviewY = (ListViewItem)y;

    //        // Compare the two items
    //        compareResult = ObjectCompare.Compare(listviewX.SubItems[ColumnToSort].Text, listviewY.SubItems[ColumnToSort].Text);

    //        // Calculate correct return value based on object comparison
    //        if (OrderOfSort == SortOrder.Ascending)
    //        {
    //            // Ascending sort is selected, return normal result of compare operation
    //            return compareResult;
    //        }
    //        else if (OrderOfSort == SortOrder.Descending)
    //        {
    //            // Descending sort is selected, return negative result of compare operation
    //            return (-compareResult);
    //        }
    //        else
    //        {
    //            // Return '0' to indicate they are equal
    //            return 0;
    //        }
    //    }

    //    /// <summary>
    //    /// Gets or sets the number of the column to which to apply the sorting operation (Defaults to '0').
    //    /// </summary>
    //    public int SortColumn
    //    {
    //        set
    //        {
    //            ColumnToSort = value;
    //        }
    //        get
    //        {
    //            return ColumnToSort;
    //        }
    //    }

    //    /// <summary>
    //    /// Gets or sets the order of sorting to apply (for example, 'Ascending' or 'Descending').
    //    /// </summary>
    //    public SortOrder Order
    //    {
    //        set
    //        {
    //            OrderOfSort = value;
    //        }
    //        get
    //        {
    //            return OrderOfSort;
    //        }
    //    }

    //}
}
