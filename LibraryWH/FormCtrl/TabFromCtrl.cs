
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LibraryWH.FormCtrl
{
    public partial class TabFromCtrl : UserControl
    {
        public delegate void OnTabControlSelectedIndexChangedDelegate(UserForm userForm);
        public event OnTabControlSelectedIndexChangedDelegate OnTabControlSelectedIndexChanged = null;
        public TabFromCtrl()
        {
            InitializeComponent();
            ClearTabControl();
            tabControl1.SelectedIndexChanged += TabControl1_SelectedIndexChanged;
        }

        private void TabControl1_SelectedIndexChanged(object sender, EventArgs e)
        {
            OnTabControlSelectedIndexChanged?.Invoke(tabControl1.SelectedTab.Controls[0] as UserForm);
        }

        public void ClearTabControl()
        {
            tabControl1.TabPages.Clear();
            img_Tabs.Images.Clear();
        }

        public UserForm SetFormToTab(UserFormInfo pInfo)
        {
            UserForm pForm = null;

            Type t = Type.GetType(pInfo.FormName);
            try
            {
                foreach (TabPage pPage in tabControl1.TabPages)
                {
                    if (pPage.Controls[0].GetType() == t)
                    {
                        tabControl1.SelectedTab = pPage;
                        pForm = pPage.Controls[0] as UserForm;
                        break;
                    }
                }

                if (pForm == null)
                {
                    AddFormToTab(pInfo);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

            return pForm;
        }
        public UserForm AddFormToTab(UserForm p)
        {
            UserForm pForm = null;
            
            try
            {
                foreach (TabPage pPage in tabControl1.TabPages)
                {
                    if (pPage.Controls[0].GetType() == p.GetType())
                    {
                        //tabControl1.SelectedTab = pPage;
                        pForm = pPage.Controls[0] as UserForm;
                        break;
                    }
                }

                //Type t = Type.GetType(pInfo.FormName);
                //pForm = t.Assembly.CreateInstance(t.FullName) as UserForm;
                if (pForm == null)
                {
                    pForm = p;
                    pForm.DisplayIn();

                    TabPage tbp = new TabPage();

                    tabControl1.TabPages.Add(tbp);
                    tabControl1.ImageList.Images.Add(pForm.Name, pForm.Icon);

                    tbp.Text = pForm.Text;
                    tbp.Controls.Add(pForm);
                    tbp.ImageKey = pForm.Name;

                    //tabControl1.SelectedTab = tbp;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            return pForm;
        }

        // 20210428, SHS, 탭을 선택하기 위해
        public void SelectTabIndex(int index)
        {
            if (tabControl1.TabCount >= index)
            {
                tabControl1.SelectedIndex = index;
            }
        }
        public UserForm AddFormToTab(UserFormInfo pInfo, string sApp = null)
        {
            UserForm pForm = null;

            try
            {
                //Type t = Type.GetType(pInfo.FormName);
                //pForm = t.Assembly.CreateInstance(t.FullName) as UserForm;
                if (pForm == null)
                {
                    if (string.IsNullOrEmpty(sApp))
                        sApp = $"{Application.StartupPath}\\{Application.ProductName}.exe";

                    System.Runtime.Remoting.ObjectHandle oh = Activator.CreateInstanceFrom(sApp, pInfo.FormName);

                    pForm = (UserForm)oh.Unwrap();
                    pForm.DisplayIn(pInfo);

                    //pForm.DisplayIn(pInfo);

                    if (string.IsNullOrEmpty(pInfo.Name))
                        pInfo.Name = pForm.Name;

                    TabPage tbp = new TabPage();

                    tabControl1.TabPages.Add(tbp);
                    tabControl1.ImageList.Images.Add(pInfo.FormName, pForm.Icon);

                    tbp.Text = pForm.Text;
                    tbp.Controls.Add(pForm);
                    tbp.ImageKey = pInfo.FormName;

                    tabControl1.SelectedTab = tbp;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            return pForm;
        }

        internal void SetLayOut()
        {
            //abControl1.RightToLeft = RightToLeft.Yes;
            tabControl1.RightToLeftLayout = true;
        }

    }
}
