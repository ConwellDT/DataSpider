using DataSpider.PC00.PT;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace DataSpider.UserMonitor
{
    public partial class TAGValueHistoryPopupDGV : LibraryWH.FormCtrl.UserForm
    {
        private PC00Z01 sqlBiz = new PC00Z01();
        private string tagName = string.Empty;
        private int selectedIndex = 0;
        private string equipType;
        private string equipName;
        private string logviewProgram = "NotePad";
        public TAGValueHistoryPopupDGV(string _tagName = "")
        {
            InitializeComponent();
            tagName = _tagName;
        }
        private void TAGValueHistoryPopup_Load(object sender, EventArgs e)
        {
            ImageList dummyImageList = new ImageList
            {
                ImageSize = new Size(1, 30)
            };
            listView_Info.SmallImageList = dummyImageList;
            dataGridView1.RowTemplate.MinimumHeight = 30;
            dataGridView1.DoubleBuffered(true);

            DateTime dtNow = DateTime.Now;
            dateTimePicker_Start.Value = dtNow.AddDays(-60);
            dateTimePicker_StartTime.Value = DateTime.Parse("00:00:00");
            dateTimePicker_End.Value = dtNow;
            dateTimePicker_EndTime.Value = DateTime.Parse("23:59:59");

            DisplayInfo();
            DisplayData();
            logviewProgram=ConfigHelper.GetAppSetting("LogViewProgram").Trim();
        }

        private void DisplayInfo()
        {
            string strErrCode = string.Empty;
            string strErrText = string.Empty;
            DataTable dtProgramStatus = sqlBiz.GetTagInfoByTag(tagName.Trim(), ref strErrCode, ref strErrText);
            listView_Info.BeginUpdate();
            listView_Info.Clear();
            foreach (DataColumn dc in dtProgramStatus.Columns)
            {
                listView_Info.Columns.Add(dc.ColumnName);
            }
            foreach (DataRow dr in dtProgramStatus.Rows)
            {
                ListViewItem lvi = new ListViewItem();
                lvi.Text = dr[0].ToString();

                for (int i = 1; i < dtProgramStatus.Columns.Count; i++)
                {
                    lvi.SubItems.Add(dr[i].ToString());
                }
                equipType = dr[0].ToString();
                equipName = dr[1].ToString();
                //lvi.BackColor = listView_Main.Items.Count % 2 == 0 ? Color.FromKnownColor(KnownColor.AliceBlue) : Color.Transparent;
                listView_Info.Items.Add(lvi);
            }
            listView_Info.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);
            listView_Info.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);
            listView_Info.EndUpdate();
        }
        private void DisplayData()
        {
            string strErrCode = string.Empty;
            string strErrText = string.Empty;
            DataTable dtProgramStatus = sqlBiz.GetTagValueHistoryByTag(tagName.Trim(), $"{dateTimePicker_Start.Value:yyyy-MM-dd} {dateTimePicker_StartTime.Value:HH:mm:ss}.000", $"{dateTimePicker_End.Value:yyyy-MM-dd} {dateTimePicker_EndTime.Value:HH:mm:ss}.999", ref strErrCode, ref strErrText);

            dataGridView1.DataSource = dtProgramStatus;

            //listView_Main.BeginUpdate();
            //listView_Main.Clear();
            //foreach (DataColumn dc in dtProgramStatus.Columns)
            //{
            //    listView_Main.Columns.Add(dc.ColumnName);
            //}
            //foreach (DataRow dr in dtProgramStatus.Rows) 
            //{ 
            //    ListViewItem lvi = new ListViewItem();
            //    lvi.Text = dr[0].ToString();
            //    for (int i = 1; i < dtProgramStatus.Columns.Count; i++) 
            //    {
            //        lvi.SubItems.Add(dr[i].ToString()); 
            //    }
            //    //lvi.BackColor = listView_Main.Items.Count % 2 == 0 ? Color.FromKnownColor(KnownColor.AliceBlue) : Color.Transparent;
            //    lvi.BackColor = listView_Main.Items.Count % 2 == 0 ? Color.FromArgb(221, 235, 247) : Color.Transparent;
            //    listView_Main.Items.Add(lvi);
            //}
            //listView_Main.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);
            //listView_Main.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);
            //listView_Main.EndUpdate();
        }
        private void TAGValueHistoryPopup_FormClosed(object sender, FormClosedEventArgs e)
        {
        }

        private void button_Refresh_Click(object sender, EventArgs e)
        {
            DisplayInfo();
            DisplayData();
        }

        private void button_Close_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            selectedIndex = e.RowIndex;
        }

        private void toolStripMenuItemLog_Click(object sender, EventArgs e)
        {
            try
            {
                DataGridViewRow dgvr = dataGridView1.CurrentRow;
                DateTime MeasureDate = DateTime.Now;
                DateTime.TryParse(dgvr.Cells[1].Value.ToString(), out MeasureDate);
                string filePath = $@"{Directory.GetCurrentDirectory()}\LOG\{equipType}_{equipName}\LOG_{equipType}_{equipName}_{MeasureDate.ToString("yyyyMMdd")}.TXT";
                Process.Start(logviewProgram, filePath);
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private void toolStripMenuItemData_Click(object sender, EventArgs e)
        {
            try
            {
                DataGridViewRow dgvr = dataGridView1.CurrentRow;
                DateTime MeasureDate = DateTime.Now;
                DateTime.TryParse(dgvr.Cells[1].Value.ToString(), out MeasureDate);
                string filePath = $@"{Directory.GetCurrentDirectory()}\LOG\{equipType}_{equipName}\DATA_{equipType}_{equipName}_{MeasureDate.ToString("yyyyMMdd")}.TXT";
                Process.Start(logviewProgram, filePath);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private void toolStripMenuItemDataList_Click(object sender, EventArgs e)
        {
            try
            {
                TimeSpan oneDay = new TimeSpan(1, 0, 0, 0);
                DataGridViewRow from_dgvr = dataGridView1.Rows[dataGridView1.RowCount - 1];
                DateTime fromDate = DateTime.Parse(from_dgvr.Cells[1].Value.ToString()) - oneDay;
                DataGridViewRow to_dgvr = dataGridView1.Rows[0];
                DateTime toDate = DateTime.Parse(to_dgvr.Cells[1].Value.ToString()) + oneDay;
                string filePath = $@"{Directory.GetCurrentDirectory()}\LOG\{equipType}_{equipName}";
                string fromFile = $@"DATA_{equipType}_{equipName}_{fromDate.ToString("yyyyMMdd")}.TXT";
                string toFile = $@"DATA_{equipType}_{equipName}_{toDate.ToString("yyyyMMdd")}.TXT";
                string fileSearchPattern = "DATA*.txt";
                string stringSearchPattern = tagName.Replace(equipName + "_", "");
                LogListForm llf = new LogListForm();
                llf.loglist = LogList(filePath,
                    fileSearchPattern,
                    fromFile,
                    toFile,
                    stringSearchPattern);
                llf.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        string LogList(string filePath, string fileSearchPattern, string fromFile, string toFile, string stringSearchPattern)
        {
            List<string> loglist = new List<string>();
            List<FileInfo> listFileInfo = new List<FileInfo>();
            DirectoryInfo di = new DirectoryInfo(filePath);
            FileInfo[] fileInfo = di.GetFiles(fileSearchPattern);
            string retString = string.Empty;

            string stringLine;

            foreach (FileInfo fi in fileInfo)
            {
                if( fi.Name.CompareTo(fromFile)>=0 && fi.Name.CompareTo(toFile)<=0 )
                    listFileInfo.Add(fi);
            }
            listFileInfo.Sort((x, y) => x.Name.CompareTo(y.Name));

            foreach (FileInfo fi in listFileInfo)
            {
                using (StreamReader sr = new StreamReader(fi.FullName))
                {
                    while (sr.Peek() >= 0)
                    {
                        stringLine = sr.ReadLine();
                        if (stringLine.Contains(stringSearchPattern))
                            loglist.Add(stringLine);
                    }
                }
            }

            foreach (string str in loglist)
            {
                retString += str + Environment.NewLine;
            }
            return retString;
        }
    }
}
