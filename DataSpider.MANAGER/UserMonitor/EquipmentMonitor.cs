using DataSpider.PC00.PT;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using System.Reflection;

namespace DataSpider.UserMonitor
{
    public partial class EquipmentMonitor : LibraryWH.FormCtrl.UserForm
    {
        public delegate bool OnRefreshTreeDataDelegate();
        public event OnRefreshTreeDataDelegate OnRefreshTreeData = null;

        private System.Windows.Forms.Timer timerRefresh = new System.Windows.Forms.Timer();
        private PC00Z01 sqlBiz = new PC00Z01();
        private string equipType = string.Empty;
        private int selectedIndex = 0;
        private int autoRefreshInterval = 10;
        private bool formSelected = false;
        private bool needResizeColumn = true;
        private DateTime dtLastRefreshed = DateTime.MinValue;
        private Thread threadDataRefresh = null;
        public bool threadStop = false;
        private bool threadPause = false;
        private bool treeSelectedNodeChanged = true;
        public EquipmentMonitor()
        {
            InitializeComponent();
        }
        private void EquipmentMonitor_Load(object sender, EventArgs e)
        {
            //ImageList dummyImageList = new ImageList
            //{
            //    ImageSize = new Size(1, 30)
            //};
            //listView_Main.SmallImageList = dummyImageList;
            listView_Main.Clear();
            GetProgramStatus();
            threadDataRefresh = new Thread(new ThreadStart(ThreadJob));
            threadDataRefresh.Start();
            UserLogInChanged();

            listView_Main.MouseDoubleClick += listView_Main_MouseDoubleClick;

            checkBox_AutoRefresh.Checked = ConfigHelper.GetAppSetting("EquipmentAutoRefresh").Trim().ToUpper().Equals("Y");
            checkBox_AutoRefresh.CheckedChanged += checkBox_AutoRefresh_CheckedChanged;

            if (!int.TryParse(ConfigHelper.GetAppSetting("EquipmentAutoRefreshInterval").Trim(), out autoRefreshInterval))
            {
                autoRefreshInterval = 10;
            }
            textBox_RefreshInterval.Text = autoRefreshInterval.ToString();
            //textBox_RefreshInterval.TextChanged += textBox_RefreshInterval_TextChanged;

            //timerRefresh.Tick += TimerRefresh_Tick;
            //timerRefresh.Interval = autoRefreshInterval * 1000;
            //if (checkBox_AutoRefresh.Checked)
            //{
            //    timerRefresh.Start();
            //}
            
            //
            // 2022. 3. 14 : Han, Ilho
            //  To activate dadagridview double buffer
            //
            Type dgvType = listView_Main.GetType();
            PropertyInfo pi = dgvType.GetProperty("DoubleBuffered", BindingFlags.Instance | BindingFlags.NonPublic);
            pi.SetValue(listView_Main, true, null);
            ////////////////
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
                    contextMenuStrip1.Items["editToolStripMenuItem"].Text = "Equipment Info";
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
                    DataTable dtProgramStatus = sqlBiz.GetProgramStatus(equipType.Trim(), ref strErrCode, ref strErrText);
                    if (dtProgramStatus == null || dtProgramStatus.Rows.Count < 1 || dtProgramStatus.Columns.Count < 1)
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
                        //lvi.Text = lvi.ImageKey = dr[0].ToString();
                        lvi.Text = lvi.ImageKey = dr[0].ToString();

                        for (int i = 1; i < dtProgramStatus.Columns.Count; i++)
                        {
                            lvi.SubItems.Add(dr[i].ToString());
                        }
                        switch (lvi.Text.ToUpper())
                        {
                            case "NORMAL":
                                lvi.SubItems[0].ForeColor = Color.Black;
                                break;
                            case "UNKNOWN":
                                lvi.SubItems[0].ForeColor = Color.DimGray;
                                break;
                            default:
                                lvi.SubItems[0].ForeColor = Color.Red;
                                break;
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
                }
                catch (Exception ex)
                {

                }
                finally
                {
                    listView_Main.EndUpdate();
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
            SBL nodeTag = (SBL)(e.Node.Tag);
            //string selectedEquipType = string.Empty;
            //if (nodeTag.GetType().Equals(typeof(EqType)))
            //{
            //    selectedEquipType = nodeTag.Name;
            //}
            //if (nodeTag.GetType().Equals(typeof(Eq)))
            //{
            //    selectedEquipType = ((SBL)e.Node.Parent.Tag).Name;
            //}
            string selectedEquipType = nodeTag is EqType ? nodeTag.Name : nodeTag is Eq ? ((SBL)e.Node.Parent.Tag).Name : string.Empty;
            if (!equipType.Equals(selectedEquipType))
            {
                treeSelectedNodeChanged = true;
                //needResizeColumn = true;
                equipType = selectedEquipType;
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
        private void RefreshTreeView()
        {
            if (OnRefreshTreeData != null)
            {
                OnRefreshTreeData();
            }
        }

        private void listView_Main_MouseDoubleClick(object sender, EventArgs e)
        {
            if ((sender as ListView).SelectedItems.Count > 0)
            {
                string equipName = (sender as ListView).SelectedItems[0].SubItems[1].Text;
                if (!string.IsNullOrWhiteSpace(equipName))
                {
                    EquipmentAddEdit form = new EquipmentAddEdit(equipName);
                    DialogResult.OK.Equals(form.ShowDialog(this));
                }
            }
        }

        private void AddToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string equipTypeName = listView_Main.SelectedItems.Count < 1 ? string.IsNullOrWhiteSpace(equipType) ? string.Empty : equipType : listView_Main.SelectedItems[0].SubItems[4].Text;
            EquipmentAddEdit form = new EquipmentAddEdit("", equipTypeName);
            if (DialogResult.OK.Equals(form.ShowDialog(this)))
            {
                RefreshTreeView();
            }
        }
        private void EditEquipmentToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listView_Main.SelectedItems.Count < 1)
            {
                return;
            }
            string equipName = listView_Main.SelectedItems[0].SubItems[1].Text;
            EquipmentAddEdit form = new EquipmentAddEdit(equipName);
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
                string equipName = listView_Main.SelectedItems[0].SubItems[1].Text;
                if (DialogResult.Yes.Equals(MessageBox.Show($"{equipName} 장비를 삭제하시겠습니까 ?", this.Text, MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2)))
                {
                    string strErrCode = string.Empty;
                    string strErrText = string.Empty;
                    if (sqlBiz.DeleteEquipmentInfo(equipName, ref strErrCode, ref strErrText))
                    {
                        MessageBox.Show($"{equipName} 장비가 삭제되었습니다.", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
                        RefreshTreeView();
                    }
                    else
                    {
                        MessageBox.Show($"장비 삭제 중 오류가 발생하였습니다. {strErrCode} - {strErrText}", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"장비 삭제 중 오류가 발생하였습니다. {ex.Message}", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);

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

        private void textBox_RefreshInterval_TextChanged(object sender, EventArgs e)
        {
            if (int.TryParse(textBox_RefreshInterval.Text, out int tempInterval))
            {
                //timerRefresh.Interval = tempInterval * 1000;
            }
        }

        private void button_Refresh_Click(object sender, EventArgs e)
        {
            //selectedIndex = listView_Main.SelectedIndices.Count > 0 ? listView_Main.SelectedIndices[0] : 0;
            GetProgramStatus();
        }

        private void button_SetInterval_Click(object sender, EventArgs e)
        {
            if (int.TryParse(textBox_RefreshInterval.Text, out int tempInterval))
            {
                //timerRefresh.Interval = tempInterval * 1000;
                autoRefreshInterval = tempInterval;
            }
        }
    }
}
