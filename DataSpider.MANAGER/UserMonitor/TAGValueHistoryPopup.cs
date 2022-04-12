using DataSpider.PC00.PT;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace DataSpider.UserMonitor
{
    public partial class TAGValueHistoryPopup : LibraryWH.FormCtrl.UserForm
    {
        private PC00Z01 sqlBiz = new PC00Z01();
        private string tagName = string.Empty;
        private int periodDays = 30;

        public TAGValueHistoryPopup(string _tagName = "", int _periodDays = 0)
        {
            InitializeComponent();
            tagName = _tagName;
            periodDays = _periodDays;
        }
        private void TAGValueHistoryPopup_Load(object sender, EventArgs e)
        {
            ImageList dummyImageList = new ImageList
            {
                ImageSize = new Size(1, 30)
            };
            listView_Main.SmallImageList = dummyImageList;
            listView_Info.SmallImageList = dummyImageList;
            DisplayInfo();
            DisplayData();
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
                //lvi.BackColor = listView_Main.Items.Count % 2 == 0 ? Color.FromKnownColor(KnownColor.AliceBlue) : Color.Transparent;
                lvi.BackColor = listView_Main.Items.Count % 2 == 0 ? Color.FromArgb(221, 235, 247) : Color.Transparent;
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
            DataTable dtProgramStatus = sqlBiz.GetTagValueHistory(tagName.Trim(), periodDays, ref strErrCode, ref strErrText);
            listView_Main.BeginUpdate();
            listView_Main.Clear();
            foreach (DataColumn dc in dtProgramStatus.Columns)
            {
                listView_Main.Columns.Add(dc.ColumnName);
            }
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
            listView_Main.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);
            listView_Main.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);
            listView_Main.EndUpdate();
        }
        private void TAGValueHistoryPopup_FormClosed(object sender, FormClosedEventArgs e)
        {
        }
    }
}
