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
    public partial class SystemLogView : LibraryWH.FormCtrl.UserForm
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
        public SystemLogView()
        {
            InitializeComponent();
        }

        private void SystemLogView_Load(object sender, EventArgs e)
        {
            InitControls();

            listView_Main.Clear();
            GetProgramStatus();
            threadDataRefresh = new Thread(new ThreadStart(ThreadJob));
            threadDataRefresh.Start();

            checkBox_AutoRefresh.Checked = ConfigHelper.GetAppSetting("LovViewAutoRefresh").Trim().ToUpper().Equals("Y");
            checkBox_AutoRefresh.CheckedChanged += checkBox_AutoRefresh_CheckedChanged;

            if (!int.TryParse(ConfigHelper.GetAppSetting("LovViewAutoRefreshInterval").Trim(), out autoRefreshInterval))
            {
                autoRefreshInterval = 10;
            }
            textBox_RefreshInterval.Text = autoRefreshInterval.ToString();

            listView_Main.DoubleBuffered(true);
        }

        private void InitControls()
        {
            DateTime dtNow = DateTime.Now;

            dateTimePicker_Start.Value = dtNow;
            dateTimePicker_StartTime.Value = DateTime.Parse("00:00:00");
            dateTimePicker_End.Value = dtNow;
            dateTimePicker_EndTime.Value = DateTime.Parse("23:59:59");

            comboBox_Level.Items.AddRange(new object[] { "All", "Trace", "Debug", "Info", "Warn", "Error", "Fatal", "Off" });
            comboBox_Level.SelectedIndex = 0;

            string strErrCode = string.Empty; string strErrText = string.Empty;

            DataTable dtEquipment = sqlBiz.GetEquipmentInfoForCombo(ref strErrCode, ref strErrText);
            if (dtEquipment != null && dtEquipment.Rows.Count > 0)
            {
                //DataRow row = dtEquipment.NewRow();
                //row["EQUIP_NM_DESC"] = "All";
                //row["EQUIP_NM"] = "All";
                //dtEquipment.Rows.InsertAt(row, 0);

                //row = dtEquipment.NewRow();
                //row["EQUIP_NM_DESC"] = "DataSpiderPC02";
                //row["EQUIP_NM"] = "DataSpiderPC02";
                //dtEquipment.Rows.Add(row);

                //row = dtEquipment.NewRow();
                //row["EQUIP_NM_DESC"] = "DataSpiderPC03";
                //row["EQUIP_NM"] = "DataSpiderPC03";
                //dtEquipment.Rows.Add(row);

                //row = dtEquipment.NewRow();
                //row["EQUIP_NM_DESC"] = "FailoverManager";
                //row["EQUIP_NM"] = "FailoverManager";
                //dtEquipment.Rows.Add(row);

                comboBox_Equipment.DataSource = dtEquipment;
                comboBox_Equipment.DisplayMember = "EQUIP_NM_DESC";
                comboBox_Equipment.ValueMember = "EQUIP_NM";
                comboBox_Equipment.SelectedIndex = 0;
            }
            
        }

        public void TabControl_SelectedIndexChanged(LibraryWH.FormCtrl.UserForm ctrl)
        {
            formSelected = this.Name.Equals(ctrl.Name);
            if (formSelected)
            {
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

                    if (!formSelected)
                    {
                        return;
                    }
                    string strErrCode = string.Empty;
                    string strErrText = string.Empty;

                    SetSelectedIndex();

                    listView_Main.Items.Clear();

                    DataTable dtProgramStatus = sqlBiz.GetSystemLog($"{dateTimePicker_Start.Value:yyyy-MM-dd} {dateTimePicker_StartTime.Value:HH:mm:ss}", $"{dateTimePicker_End.Value:yyyy-MM-dd} {dateTimePicker_EndTime.Value:HH:mm:ss}", comboBox_Equipment.SelectedValue.ToString(), comboBox_Level.SelectedItem.ToString(), ref strErrCode, ref strErrText);

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

        private void SetSelectedIndex()
        {
            selectedIndex = listView_Main.SelectedIndices.Count > 0 ? listView_Main.SelectedIndices[0] : 0;
        }
    }
}
