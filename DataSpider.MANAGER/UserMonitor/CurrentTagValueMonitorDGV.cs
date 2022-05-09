using DataSpider.PC00.PT;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using System.Reflection;

namespace DataSpider.UserMonitor
{
    public partial class CurrentTagValueMonitorDGV : LibraryWH.FormCtrl.UserForm
    {
        public delegate bool OnRefreshTreeDataDelegate();
        public event OnRefreshTreeDataDelegate OnRefreshTreeData = null;
        //private System.Windows.Forms.Timer timerRefresh = new System.Windows.Forms.Timer();
        private PC00Z01 sqlBiz = new PC00Z01();
        private string equipType = string.Empty;
        private string equipName = string.Empty;
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
        private DateTime DateTimeFilterHistMin = DateTime.Now.AddDays(-30);// DateTime.MinValue;
        private DateTime DateTimeFilterHistMax = DateTime.Now;// DateTime.MinValue;
        private string DescriptionFilter = "";

        public CurrentTagValueMonitorDGV()
        {
            InitializeComponent();
        }
        private void CurrentTagValueMonitor_Load(object sender, EventArgs e)
        {
            threadDataRefresh = new Thread(new ThreadStart(ThreadJob));
            threadDataRefresh.Start();

            UserLogInChanged();
            dataGridView_Main.RowTemplate.MinimumHeight = 30;
            dataGridView_Main.DoubleBuffered(true);
            dataGridView_Main.CellMouseDoubleClick += DataGridView_Main_CellMouseDoubleClick;
            // None 로 해야 사용자 컬럼 사이즈 조절이 가능함. 
            // 바인딩 후 AutoResizeColumns(DataGridViewAutoSizeColumnsMode.DisplayedCells); 처리
            //            dataGridView_Main.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;
            dataGridView_Main.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.DisplayedCells;
            dataGridView_Main.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.None;
            dataGridView_Main.AllowUserToResizeRows = dataGridView_Main.AllowUserToResizeColumns = true;

            //
            // 2022. 3. 14 : Han, Ilho
            //  To activate dadagridview double buffer
            //
            //Type dgvType = dataGridView_Main.GetType();
            //PropertyInfo pi = dgvType.GetProperty("DoubleBuffered", BindingFlags.Instance | BindingFlags.NonPublic);
            //pi.SetValue(dataGridView_Main, true, null);
            //////////////////////////

            checkBox_AutoRefresh.Checked = ConfigHelper.GetAppSetting("TagAutoRefresh").Trim().ToUpper().Equals("Y");
            checkBox_AutoRefresh.CheckedChanged += checkBox_AutoRefresh_CheckedChanged;

            if (!int.TryParse(ConfigHelper.GetAppSetting("TagAutoRefreshInterval").Trim(), out autoRefreshInterval))
            {
                autoRefreshInterval = 10;
            }
            checkBox_AutoRefresh.Text = "Auto Refresh(" + autoRefreshInterval.ToString() + "s)";
            //timerRefresh.Tick += TimerRefresh_Tick;
            //timerRefresh.Interval = autoRefreshInterval * 1000;
            //if (checkBox_AutoRefresh.Checked)
            //{
            //    timerRefresh.Start();
            //}
            logviewProgram = ConfigHelper.GetAppSetting("LogViewProgram").Trim();
        }

        private void DataGridView_Main_CellMouseDoubleClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.RowIndex < 0 || e.RowIndex >= dataGridView_Main.Rows.Count || nDBModeCurrent==0)
            {
                return;
            }
            TAGValueHistoryPopupDGV form = new TAGValueHistoryPopupDGV(radioButtonCurTag.Checked ? dataGridView_Main.Rows[e.RowIndex].Cells[3].Value.ToString() : dataGridView_Main.Rows[e.RowIndex].Cells[1].Value.ToString());
            form.ShowDialog(this);
        }

        public void UserLogInChanged()
        {
            switch (UserAuthentication.UserLevel)
            {
                case UserLevel.Admin:
                case UserLevel.Manager:
                    foreach (ToolStripItem item in contextMenuStrip1.Items)
                    {
                        item.Visible = true;
                    }
                    contextMenuStrip1.Items["editToolStripMenuItem"].Text = "Edit";
                    break;
                default:
                    contextMenuStrip1.Items["addToolStripMenuItem"].Visible = contextMenuStrip1.Items["deleteToolStripMenuItem"].Visible = false;
                    contextMenuStrip1.Items["editToolStripMenuItem"].Text = "Tag Info";
                    break;
            }
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

        private void GetTagCurrentValues()
        {
            string strErrCode = string.Empty;
            string strErrText = string.Empty;

            dataGridView_Main.DataSource = null;

            DataTable dtProgramStatus = sqlBiz.GetCurrentTagValue(equipType.Trim(), equipName.Trim(), ref strErrCode, ref strErrText);
            if (dtProgramStatus == null || dtProgramStatus.Rows.Count < 1)
            {
                return;
            }

            DataView dvProgramStatus = dtProgramStatus.DefaultView;

            String strFileterStr = String.Empty;

            if (String.IsNullOrEmpty(TagNameFilter) == false)
            {
                strFileterStr = $" [TAG Name] LIKE '%{TagNameFilter}%'";
            }
            else
            {
                String selGrpName = comboBoxTagGroupSel.SelectedValue.ToString();

                if (selGrpName != "All")
                {
                    DataTable dtGropTagNames = sqlBiz.GetTagGroupInfo(selGrpName, ref strErrCode, ref strErrText);
                    
                    if (strErrCode == null || strErrCode == string.Empty)
                    {
                        if (String.IsNullOrEmpty(equipName.Trim()) == false)
                        {
                            for (int nT = 0; nT < dtGropTagNames.Rows.Count; nT++)
                            {
                                if (nT == 0)
                                {
                                    strFileterStr = $"([TAG Name] = '{equipName.Trim()}_{dtGropTagNames.Rows[nT]["TAG_NM"].ToString()}'";
                                }
                                else
                                {
                                    strFileterStr += $" OR [TAG Name] = '{equipName.Trim()}_{dtGropTagNames.Rows[nT]["TAG_NM"].ToString()}'";
                                }
                            }
                        }
                        else
                        {
                            for (int nT = 0; nT < dtGropTagNames.Rows.Count; nT++)
                            {
                                if (nT == 0)
                                {
                                    strFileterStr = $"([TAG Name] LIKE '%{equipName.Trim()}_{dtGropTagNames.Rows[nT]["TAG_NM"].ToString()}%'";
                                }
                                else
                                {
                                    strFileterStr += $" OR [TAG Name] LIKE '%{equipName.Trim()}_{dtGropTagNames.Rows[nT]["TAG_NM"].ToString()}%'";
                                }
                            }
                        }

                        if (String.IsNullOrEmpty(strFileterStr) == false) strFileterStr += ")";
                    }
                }
            }
            // 20220420, SHS, 최근값 조회하는데 시간이 왜 필요 ?
            //if (DateTimeFilterCurMin > DateTime.MinValue && DateTimeFilterCurMax > DateTime.MinValue && DateTimeFilterCurMin < DateTimeFilterCurMax)
            //{
            //    if (String.IsNullOrEmpty(strFileterStr) == false) strFileterStr += " AND ";
            //    if (DateTimeFilterCurMin == DateTimeFilterCurMax)
            //    {
            //        strFileterStr += $"([Measure DateTime] = '{DateTimeFilterCurMin.ToString("yyyy-MM-dd HH:mm:ss.fff")}') ";
            //    }
            //    else
            //    {
            //        strFileterStr += $"([Measure DateTime] > '{DateTimeFilterCurMin.ToString("yyyy-MM-dd HH:mm:ss.fff")}' AND [Measure DateTime] < '{DateTimeFilterCurMax.ToString("yyyy-MM-dd HH:mm:ss.fff")}') ";
            //    }
            //}
            if (String.IsNullOrEmpty(strFileterStr) == false) strFileterStr += " AND ";
            strFileterStr += $"[Description] LIKE '%{DescriptionFilter}%'  ";

            dvProgramStatus.RowFilter = strFileterStr;
            dvProgramStatus.Sort = "Measure DateTime DESC";
            
            int nHoriScrollOffset = dataGridView_Main.HorizontalScrollingOffset;
            int nRowIndex = dataGridView_Main.FirstDisplayedScrollingRowIndex;

            dataGridView_Main.DataSource = dvProgramStatus;

            if (dvProgramStatus.Count > 0)
            {
                dataGridView_Main.HorizontalScrollingOffset = nHoriScrollOffset;

                if (dvProgramStatus.Count > nRowIndex)
                {
                    dataGridView_Main.FirstDisplayedScrollingRowIndex = nRowIndex;
                }
                else
                {
                    dataGridView_Main.FirstDisplayedScrollingRowIndex = 0;
                }
            }
        }

        private void GetTagHistoryValues()
        {
            if (String.IsNullOrEmpty(equipName.Trim()) == true)
            {
                dataGridView_Main.DataSource = null;//.Rows.Clear();
                //MessageBox.Show("Equipment is not selected", "History Data Display", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            string strErrCode = string.Empty;
            string strErrText = string.Empty;

            DataTable dtHistoryData = null;

            String strFileterStr = String.Empty;

            String selGrpName = comboBoxTagGroupSel.SelectedValue.ToString();

            DateTime minDate;
            DateTime maxDate;
            if (DateTimeFilterHistMin > DateTime.MinValue && DateTimeFilterHistMax > DateTime.MinValue && (DateTimeFilterHistMax > DateTimeFilterHistMin))
            {
                minDate = DateTimeFilterHistMin;
                maxDate = DateTimeFilterHistMax;
            }
            else
            {
                minDate = DateTime.Now.AddDays(-30);
                maxDate = DateTime.Now;
            }

            if (selGrpName != "All")
            {
                DataTable dtGropTagNames = sqlBiz.GetTagGroupInfo(selGrpName, ref strErrCode, ref strErrText);

                if (strErrCode == null || strErrCode == string.Empty)
                {
                    for (int nT = 0; nT < dtGropTagNames.Rows.Count; nT++)
                    {
                        String strTagName = equipName.Trim() + "_" + dtGropTagNames.Rows[nT]["TAG_NM"].ToString();


                        DataTable dtTagHistory = sqlBiz.GetAllTagHistoryValue(strTagName, minDate.ToString("yyyy-MM-dd HH:mm:ss.fff"), maxDate.ToString("yyyy-MM-dd HH:mm:ss.fff"), ref strErrCode, ref strErrText);

                        if (dtTagHistory != null && dtTagHistory.Rows.Count > 0)
                        {
                            if (dtHistoryData == null)
                            {
                                dtHistoryData = dtTagHistory.Clone();
                            }
                            else
                            {
                                dtHistoryData.Merge(dtTagHistory);
                            }
                        }

                        if (dtTagHistory != null) dtTagHistory.Dispose();
                    }
                }
            }
            else
            {
                DataTable dtTagValueHistory = sqlBiz.GetTagValueHistoryByEquip(equipName, minDate.ToString("yyyy-MM-dd HH:mm:ss.fff"), maxDate.ToString("yyyy-MM-dd HH:mm:ss.fff"), ref strErrCode, ref strErrText);

                if (strErrCode == null || strErrCode == string.Empty)
                {
                    if (dtTagValueHistory != null && dtTagValueHistory.Rows.Count > 0)
                    {
                        dtHistoryData = dtTagValueHistory;
                    }

                    if (dtTagValueHistory != null) dtTagValueHistory.Dispose();
                }
            }

            if (dtHistoryData != null && dtHistoryData.Rows.Count > 0)
            {
                dtHistoryData.DefaultView.Sort = "MEASURE_DATE DESC";


                int nHoriScrollOffset = dataGridView_Main.HorizontalScrollingOffset;
                int nRowIndex = dataGridView_Main.FirstDisplayedScrollingRowIndex;

                dataGridView_Main.DataSource = dtHistoryData;

                if (dtHistoryData.Rows.Count > 0)
                {
                    dataGridView_Main.HorizontalScrollingOffset = nHoriScrollOffset;

                    if (dtHistoryData.Rows.Count > nRowIndex)
                    {
                        dataGridView_Main.FirstDisplayedScrollingRowIndex = nRowIndex;
                    }
                    else
                    {
                        dataGridView_Main.FirstDisplayedScrollingRowIndex = 0;
                    }
                }
            }
            else
            {
                dataGridView_Main.DataSource = null;
                //String strMsg = $"Equipment : {equipName}, TagGroup : {selGrpName}, Period : {minDate} ~ {maxDate} - No data exist";
                //MessageBox.Show(strMsg, "History Data Display", MessageBoxButtons.OK, MessageBoxIcon.Information);
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
                    //if (string.IsNullOrWhiteSpace(equipType) && string.IsNullOrWhiteSpace(equipName))
                    //{
                    //    return;
                    //}

                    //
                    // 2022. 3. 14 : Han. Ilho
                    //      Add tag group property
                    //

                    if( string.IsNullOrEmpty(equipType) )
                    {
                        DataTable dtEquiptype = sqlBiz.GetCommonCode("EQUIP_TYPE", ref strErrCode, ref strErrText);
                        equipType = dtEquiptype.Rows[0]["CODE_NM"].ToString();
                    }

                    if (equipTypeCur != equipType)
                    {
                        DataTable dtEquiptype = sqlBiz.GetCommonCode("EQUIP_TYPE", ref strErrCode, ref strErrText);
                        DataRow[] drEQCodeSel = dtEquiptype.Select($"CD_GRP = 'EQUIP_TYPE' AND CODE_NM = '{equipType.Trim()}'");

                        if (drEQCodeSel != null && drEQCodeSel.Length > 0)
                        {
                            String strEQTypeCode = drEQCodeSel[0]["CODE"].ToString();
                            DataTable dtGroups = sqlBiz.GetTagGroupByEQType(strEQTypeCode, ref strErrCode, ref strErrText);

                            if (strErrCode == null || strErrCode == string.Empty)
                            {
                                DataRow row = dtGroups.NewRow();

                                row["GROUP_NM"] = "All";
                                row["GROUP_DESC"] = "All Tags";
                                row["GROUP_NM_VALUE"] = "All(All Tags)";
                                row["EQUIP_TYPE"] = equipType;

                                dtGroups.Rows.Add(row);

                                comboBoxTagGroupSel.DataSource = dtGroups;
                                comboBoxTagGroupSel.DisplayMember = "GROUP_NM_VALUE";
                                comboBoxTagGroupSel.ValueMember = "GROUP_NM";


                                if( dtGroups.Rows.Count > 0 )
                                {
                                    comboBoxTagGroupSel.SelectedIndex = (comboBoxTagGroupSel.Items.Count - 1);
                                }
                            }

                            equipTypeCur = equipType;
                        }
                    }
                    /////////////////////////////////
                    if( nDBModeCurrent == 1)
                    {
                        GetTagCurrentValues();
                    }
                    else
                    {
                        GetTagHistoryValues();
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

            if (nodeTag is EqType)
            {
                selectedEquipType = nodeTag.Name;
                selectedEquipName = string.Empty;
            }
            if (nodeTag is Eq)
            {
                selectedEquipType = (e.Node.Parent.Tag as SBL).Name;
                selectedEquipName = nodeTag.Name;
            }
            //if (!equipType.Equals(selectedEquipType) || !equipName.Equals(selectedEquipName))
            {
                //needResizeColumn = true;

                treeSelectedNodeChanged = true;
                equipType = selectedEquipType;
                equipName = selectedEquipName;
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
            if (dataGridView_Main.SelectedRows.Count < 1)
            {
                return;
            }
            string tagName = dataGridView_Main.SelectedRows[0].Cells[3].Value.ToString();
            TAGValueHistoryPopupDGV form = new TAGValueHistoryPopupDGV(tagName);
            form.ShowDialog(this);
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
            TagAddEdit form = new TagAddEdit(equipName);
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

        private void checkBox_AutoRefresh_CheckedChanged(object sender, EventArgs e)
        {
            if ((sender as CheckBox).Checked)
            {
                threadPause = false;
                //button_Refresh.Enabled = false;
                //timerRefresh.Start();
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
            SetRefreshInterval dlg = new SetRefreshInterval();

            dlg.RefreshInterval = autoRefreshInterval;

            if( dlg.ShowDialog() == DialogResult.OK )
            {
                if (dlg.RefreshInterval > 0)
                {
                    autoRefreshInterval = dlg.RefreshInterval;

                    ConfigHelper.SetAppSetting("TagAutoRefreshInterval", autoRefreshInterval.ToString());
                }
            }

            checkBox_AutoRefresh.Text = "Auto Refresh(" + autoRefreshInterval.ToString() + "s)";
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

        public void Find(string find, bool top, bool caseSense)
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

        private void toolStripMenuItemLog_Click(object sender, EventArgs e)
        {
            try
            {
                DataGridViewRow dgvr = dataGridView_Main.CurrentRow;
                DateTime MeasureDate = DateTime.Now;
                DateTime.TryParse(dgvr.Cells[5].Value.ToString(), out MeasureDate);
                string filePath = $@"{Directory.GetCurrentDirectory()}\LOG\{equipType}_{equipName}\LOG_{equipType}_{equipName}_{MeasureDate.ToString("yyyyMMdd")}.TXT";
                Process.Start(logviewProgram, filePath);
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
                DataGridViewRow dgvr = dataGridView_Main.CurrentRow;
                DateTime MeasureDate = DateTime.Now;
                DateTime.TryParse(dgvr.Cells[5].Value.ToString(), out MeasureDate);
                string filePath = $@"{Directory.GetCurrentDirectory()}\LOG\{equipType}_{equipName}\DATA_{equipType}_{equipName}_{MeasureDate.ToString("yyyyMMdd")}.TXT";
                Process.Start(logviewProgram, filePath);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private void buttonFilter_Click(object sender, EventArgs e)
        {
            FilterForm ff = new FilterForm();
            ff.TagNameFilter = TagNameFilter;
            if (nDBModeCurrent == 1)
            {
                ff.DateTimeFilterCurMin = DateTimeFilterCurMin;
                ff.DateTimeFilterCurMax = DateTimeFilterCurMax;
            }
            else
            {
                ff.DateTimeFilterCurMin = DateTimeFilterHistMin;
                ff.DateTimeFilterCurMax = DateTimeFilterHistMax;
            }
            ff.DescriptionFilter = DescriptionFilter;
            if ( ff.ShowDialog() == DialogResult.OK )
            {
                TagNameFilter = ff.TagNameFilter;
                if (nDBModeCurrent == 1)
                {
                    DateTimeFilterCurMin = ff.DateTimeFilterCurMin;
                    DateTimeFilterCurMax = ff.DateTimeFilterCurMax;
                }
                else
                {
                    DateTimeFilterHistMin = ff.DateTimeFilterCurMin;
                    DateTimeFilterHistMax = ff.DateTimeFilterCurMax;
                }
                DescriptionFilter = ff.DescriptionFilter;
                GetProgramStatus();
            }
        }

        private void comboBoxTagGroupSel_SelectedIndexChanged(object sender, EventArgs e)
        {
            GetProgramStatus();
        }

        private void radioButtonCurTag_CheckedChanged(object sender, EventArgs e)
        {
            if(radioButtonCurTag.Checked == true)
            {
                nDBModeCurrent = 1;

                checkBox_AutoRefresh.Checked = true;
            }
            else
            {
                nDBModeCurrent = 0;

                checkBox_AutoRefresh.Checked = false;
            }
        }

        private void radioButtonHistoryTag_CheckedChanged(object sender, EventArgs e)
        {
            // Current
            if (radioButtonCurTag.Checked == true)
            {
                nDBModeCurrent = 1;

                checkBox_AutoRefresh.Checked = true;
                checkBox_AutoRefresh.Visible = button_SetInterval.Visible = true;
            }
            // History
            else
            {
                nDBModeCurrent = 0;

                checkBox_AutoRefresh.Checked = false;
                checkBox_AutoRefresh.Visible = button_SetInterval.Visible = false;
            }
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
