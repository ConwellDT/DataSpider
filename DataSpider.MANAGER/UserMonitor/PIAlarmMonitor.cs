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
    public partial class PIAlarmMonitor : LibraryWH.FormCtrl.UserForm
    {
        public delegate bool OnRefreshTreeDataDelegate();
        public event OnRefreshTreeDataDelegate OnRefreshTreeData = null;

        private System.Windows.Forms.Timer timerRefresh = new System.Windows.Forms.Timer();
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
        private bool treeSelectedNodeChanged = true;
        public PIAlarmMonitor()
        {
            InitializeComponent();
        }
        private void PIAlarmMonitor_Load(object sender, EventArgs e)
        {
            listView_Main.Clear();
            GetProgramStatus();
            threadDataRefresh = new Thread(new ThreadStart(ThreadJob));
            threadDataRefresh.Start();
            
            checkBox_AutoRefresh.Checked = ConfigHelper.GetAppSetting("PIAlarmAutoRefresh").Trim().ToUpper().Equals("Y");
            checkBox_AutoRefresh.CheckedChanged += checkBox_AutoRefresh_CheckedChanged;

            if (!int.TryParse(ConfigHelper.GetAppSetting("PIAlarmAutoRefreshInterval").Trim(), out autoRefreshInterval))
            {
                autoRefreshInterval = 10;
            }
            textBox_RefreshInterval.Text = autoRefreshInterval.ToString();

            UserLogInChanged();
            listView_Main.DoubleBuffered(true);
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
                    
                    if (!formSelected)
                    {
                        return;
                    }
                    string strErrCode = string.Empty;
                    string strErrText = string.Empty;

                    SetSelectedIndex();

                    listView_Main.Items.Clear();

                    DataTable dtProgramStatus = sqlBiz.GetPIAlarmStatus(equipType.Trim(), equipName.Trim(), ref strErrCode, ref strErrText);

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
                        //lvi.Text = lvi.ImageKey = dr[0].ToString();
                        lvi.Text = dr[0].ToString();

                        for (int i = 1; i < dtProgramStatus.Columns.Count; i++)
                        {
                            lvi.SubItems.Add(dr[i].ToString());
                        }
                        //switch (lvi.Text.ToUpper())
                        //{
                        //    case "NORMAL":
                        //        lvi.SubItems[0].ForeColor = Color.Black;
                        //        break;
                        //    case "UNKNOWN":
                        //        lvi.SubItems[0].ForeColor = Color.DimGray;
                        //        break;
                        //    default:
                        //        lvi.SubItems[0].ForeColor = Color.Red;
                        //        break;
                        //}
                                                
                        lvi.BackColor = listView_Main.Items.Count % 2 == 0 ? Color.FromArgb(221, 235, 247) : Color.Transparent;

                        listView_Main.Items.Add(lvi);
                    }
                    if (needResizeColumn)
                    {                        
                        listView_Main.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);
                        needResizeColumn = false;
                    }
                    if (dtProgramStatus.Rows.Count > 0)
                    {
                        listView_Main.EnsureVisible(selectedIndex);
                        listView_Main.Items[selectedIndex].Selected = true;
                    }

                    //줄간격 조정
                    ImageList imglist = new ImageList();
                    imglist.ImageSize = new Size(1, 30);
                    listView_Main.SmallImageList = imglist;
                }
                catch (Exception ex)
                {

                }
                finally
                {
                    listView_Main.EndUpdate();
                    
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
        private void PIAlarmMonitor_FormClosed(object sender, FormClosedEventArgs e)
        {
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

        private void checkBox_AutoRefresh_CheckedChanged(object sender, EventArgs e)
        {
            if ((sender as CheckBox).Checked)
            {
                threadPause = false;
                //button_Refresh.Enabled = false;                
            }
            else
            {
                threadPause = true;
                //button_Refresh.Enabled = true;                
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
            GetProgramStatus();
        }

        private void button_SetInterval_Click(object sender, EventArgs e)
        {
            if (int.TryParse(textBox_RefreshInterval.Text, out int tempInterval))
            {
                autoRefreshInterval = tempInterval;
            }
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
                    break;
                default:
                    foreach (ToolStripItem item in contextMenuStrip1.Items)
                    {
                        item.Visible = false;
                    }
                    break;
            }
        }
        private void resetIFFlagToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listView_Main.SelectedItems.Count > 0 && int.TryParse(listView_Main.SelectedItems[0].Text, out int hiSeq))
            {
                if (DialogResult.Yes.Equals(MessageBox.Show($"Do you want to reset PI I/F flag [{listView_Main.SelectedItems[0].SubItems[1].Text}] ? It will try to save the PI again.", "PIAlarm", MessageBoxButtons.YesNo)))
                {
                    sqlBiz.RestPIIFFlag(hiSeq);
                    GetProgramStatus();
                }
            }
        }

        private void removePIAlarmToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listView_Main.SelectedItems.Count > 0 && int.TryParse(listView_Main.SelectedItems[0].Text, out int hiSeq))
            {
                if (DialogResult.Yes.Equals(MessageBox.Show($"Do you want to remove PI I/F alarm [{listView_Main.SelectedItems[0].SubItems[1].Text}] ?", "PIAlarm", MessageBoxButtons.YesNo)))
                {
                    sqlBiz.RemovePIAlarm(hiSeq);
                    GetProgramStatus();
                }
            }
        }
        private void SetSelectedIndex()
        {
            selectedIndex = listView_Main.SelectedIndices.Count > 0 ? listView_Main.SelectedIndices[0] : 0;
        }
    }
}
