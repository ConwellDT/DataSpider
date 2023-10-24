using DataSpider.PC00.PT;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace DataSpider.UserMonitor
{
    public partial class CurrentTagValueMonitor : LibraryWH.FormCtrl.UserForm
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
        private bool treeSelectedNodeChanged = false;


        // 20210428, SHS, 리스트뷰 컬럼헤더 클릭 시 소팅 기능
        private ListViewColumnSorter lvwColumnSorter;
        public CurrentTagValueMonitor()
        {
            InitializeComponent();
        }
        private void CurrentTagValueMonitor_Load(object sender, EventArgs e)
        {
            ImageList dummyImageList = new ImageList
            {
                ImageSize = new Size(1, 30)
            };
            lvwColumnSorter = new ListViewColumnSorter();
            listView_Main.ListViewItemSorter = lvwColumnSorter;
            listView_Main.SmallImageList = dummyImageList;
            //GetProgramStatus();
            threadDataRefresh = new Thread(new ThreadStart(ThreadJob));
            threadDataRefresh.Start();
            UserLogInChanged();
            listView_Main.MouseDoubleClick += listView_Main_MouseDoubleClick;

            checkBox_AutoRefresh.Checked = ConfigHelper.GetAppSetting("TagAutoRefresh").Trim().ToUpper().Equals("Y");
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

        private void TimerRefresh_Tick(object sender, EventArgs e)
        {
            if (formSelected)
            {
                selectedIndex = listView_Main.SelectedIndices.Count > 0 ? listView_Main.SelectedIndices[0] : 0;
                GetProgramStatus();
            }
        }

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
                    selectedIndex = listView_Main.SelectedIndices.Count > 0 ? listView_Main.SelectedIndices[0] : 0;
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
                    if (string.IsNullOrWhiteSpace(equipType) && string.IsNullOrWhiteSpace(equipName) && string.IsNullOrWhiteSpace(zoneType))
                    {
                        return;
                    }
                    DataTable dtProgramStatus = sqlBiz.GetCurrentTagValue(equipType.Trim(), equipName.Trim(), zoneType.Trim(), ref strErrCode, ref strErrText);
                    if (dtProgramStatus == null || dtProgramStatus.Rows.Count < 1)
                    {
                        return;
                    }

                    listView_Main.BeginUpdate();
                    if (listView_Main.Columns.Count != dtProgramStatus.Columns.Count)
                    {
                        needResizeColumn = true;
                        listView_Main.Clear();
                        foreach (DataColumn dc in dtProgramStatus.Columns)
                        {
                            listView_Main.Columns.Add(dc.ColumnName);
                        }
                    }
                    listView_Main.Items.Clear();
                    foreach (DataRow dr in dtProgramStatus.Rows)
                    {
                        ListViewItem lvi = new ListViewItem();
                        lvi.Text = dr[0].ToString();
                        for (int i = 1; i < dtProgramStatus.Columns.Count; i++)
                        {
                            lvi.SubItems.Add(dr[i].ToString());
                        }
                        //lvi.BackColor = listView_Main.Items.Count % 2 == 0 ? Color.FromKnownColor(KnownColor.AliceBlue) : Color.Transparent;
                        lvi.BackColor = listView_Main.Items.Count % 2 == 0 ? Color.FromArgb(221, 235, 247) : Color.Transparent;
                        listView_Main.Items.Add(lvi);
                    }
                    if (needResizeColumn)
                    {
                        //listView_Main.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);
                        listView_Main.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);
                        needResizeColumn = false;
                    }
                    if (dtProgramStatus.Rows.Count > 0)
                    {
                        listView_Main.EnsureVisible(selectedIndex);
                        listView_Main.Items[selectedIndex].Selected = true;
                    }
                    listView_Main.EndUpdate();
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
            if (!equipType.Equals(selectedEquipType) || !equipName.Equals(selectedEquipName))
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

        private void listView_Main_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if ((sender as ListView).SelectedItems.Count > 0)
            {
                string tagName = (sender as ListView).SelectedItems[0].SubItems[3].Text;
                TAGValueHistoryPopupDGV form = new TAGValueHistoryPopupDGV(tagName);
                form.ShowDialog(this);
            }
        }

        private void ValueHistoryToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listView_Main.SelectedItems.Count < 1)
            {
                return;
            }
            string tagName = listView_Main.SelectedItems[0].SubItems[3].Text;
            TAGValueHistoryPopupDGV form = new TAGValueHistoryPopupDGV(tagName);
            form.ShowDialog(this);
        }

        private void EditToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listView_Main.SelectedItems.Count < 1)
            {
                return;
            }
            string tagName = listView_Main.SelectedItems[0].SubItems[3].Text;
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
                if (listView_Main.SelectedItems.Count < 1)
                {
                    return;
                }
                string tagName = listView_Main.SelectedItems[0].SubItems[3].Text;
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

        private void listView_Main_ColumnClick(object sender, ColumnClickEventArgs e)
        {
            // Determine if clicked column is already the column that is being sorted.
            if (e.Column == lvwColumnSorter.SortColumn)
            {
                // Reverse the current sort direction for this column.
                if (lvwColumnSorter.Order == SortOrder.Ascending)
                {
                    lvwColumnSorter.Order = SortOrder.Descending;
                }
                else
                {
                    lvwColumnSorter.Order = SortOrder.Ascending;
                }
            }
            else
            {
                // Set the column number that is to be sorted; default to ascending.
                lvwColumnSorter.SortColumn = e.Column;
                lvwColumnSorter.Order = SortOrder.Ascending;
            }

            // Perform the sort with these new sort options.
            this.listView_Main.Sort();
        }

    }

    /// <summary>
    /// This class is an implementation of the 'IComparer' interface.
    /// </summary>
    public class ListViewColumnSorter : IComparer
    {
        /// <summary>
        /// Specifies the column to be sorted
        /// </summary>
        private int ColumnToSort;

        /// <summary>
        /// Specifies the order in which to sort (i.e. 'Ascending').
        /// </summary>
        private SortOrder OrderOfSort;

        /// <summary>
        /// Case insensitive comparer object
        /// </summary>
        private CaseInsensitiveComparer ObjectCompare;

        /// <summary>
        /// Class constructor. Initializes various elements
        /// </summary>
        public ListViewColumnSorter()
        {
            // Initialize the column to '0'
            ColumnToSort = 0;

            // Initialize the sort order to 'none'
            OrderOfSort = SortOrder.None;

            // Initialize the CaseInsensitiveComparer object
            ObjectCompare = new CaseInsensitiveComparer();
        }

        /// <summary>
        /// This method is inherited from the IComparer interface. It compares the two objects passed using a case insensitive comparison.
        /// </summary>
        /// <param name="x">First object to be compared</param>
        /// <param name="y">Second object to be compared</param>
        /// <returns>The result of the comparison. "0" if equal, negative if 'x' is less than 'y' and positive if 'x' is greater than 'y'</returns>
        public int Compare(object x, object y)
        {
            int compareResult;
            ListViewItem listviewX, listviewY;

            // Cast the objects to be compared to ListViewItem objects
            listviewX = (ListViewItem)x;
            listviewY = (ListViewItem)y;

            // Compare the two items
            compareResult = ObjectCompare.Compare(listviewX.SubItems[ColumnToSort].Text, listviewY.SubItems[ColumnToSort].Text);

            // Calculate correct return value based on object comparison
            if (OrderOfSort == SortOrder.Ascending)
            {
                // Ascending sort is selected, return normal result of compare operation
                return compareResult;
            }
            else if (OrderOfSort == SortOrder.Descending)
            {
                // Descending sort is selected, return negative result of compare operation
                return (-compareResult);
            }
            else
            {
                // Return '0' to indicate they are equal
                return 0;
            }
        }

        /// <summary>
        /// Gets or sets the number of the column to which to apply the sorting operation (Defaults to '0').
        /// </summary>
        public int SortColumn
        {
            set
            {
                ColumnToSort = value;
            }
            get
            {
                return ColumnToSort;
            }
        }

        /// <summary>
        /// Gets or sets the order of sorting to apply (for example, 'Ascending' or 'Descending').
        /// </summary>
        public SortOrder Order
        {
            set
            {
                OrderOfSort = value;
            }
            get
            {
                return OrderOfSort;
            }
        }

    }
}
