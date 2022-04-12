using SEIMM.PC00.PT;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace SEIMM.UserMonitor
{
    public partial class BaseFormListView : LibraryWH.FormCtrl.UserForm
    {
        protected Timer timerRefresh = null;
        private PC00Z01 sqlBiz = new PC00Z01();
        private string equipType = string.Empty;

        public BaseFormListView()
        {
            InitializeComponent();
        }
        private void BaseFormListView_Load(object sender, EventArgs e)
        {
            timerRefresh = new Timer();
            timerRefresh.Tick += TimerRefresh_Tick;
            timerRefresh.Interval = 5000;
            timerRefresh.Start();
        }

        private void TimerRefresh_Tick(object sender, EventArgs e)
        {
            GetProgramStatus();
        }

        private void GetProgramStatus()
        {
            string strErrCode = string.Empty;
            string strErrText = string.Empty;
            DataTable dtProgramStatus = sqlBiz.GetProgramStatus(equipType.Trim(), ref strErrCode, ref strErrText);
            //listView_EquipStatus.Columns.Clear();
            listView_Data.BeginUpdate();
            listView_Data.Clear();
            foreach (DataColumn dc in dtProgramStatus.Columns)
            {
                listView_Data.Columns.Add(dc.ColumnName);
            }
            foreach (DataRow dr in dtProgramStatus.Rows) 
            { 
                ListViewItem lvi = new ListViewItem();
                lvi.Text = dr[0].ToString();
                for (int i = 1; i < dtProgramStatus.Columns.Count; i++) 
                {
                    lvi.SubItems.Add(dr[i].ToString()); 
                }
                switch (lvi.Text)
                {
                    case "START":
                        lvi.SubItems[0].ForeColor = Color.Black;
                        break;
                    default:
                        lvi.SubItems[0].ForeColor = Color.Red;
                        break;
                }
                listView_Data.Items.Add(lvi);
            }
            listView_Data.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);
            listView_Data.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);
            listView_Data.EndUpdate();
        }
        public void treeView1_AfterSelect(object sender, TreeViewEventArgs e)
        {
            SBL nodeTag = (SBL)(e.Node.Tag);
            equipType = string.Empty;
            if (nodeTag.GetType().Equals(typeof(EqType)))
            {
                equipType = nodeTag.Name;
            }
            if (nodeTag.GetType().Equals(typeof(Eq)))
            {
                equipType = ((SBL)e.Node.Parent.Tag).Name;
            }
            GetProgramStatus();
        }
        private void BaseFormListView_FormClosed(object sender, FormClosedEventArgs e)
        {
            timerRefresh.Stop();
        }
    }
}
