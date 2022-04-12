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
    public partial class PanelForm : UserControl
    {
        public PanelForm()
        {
            InitializeComponent();
            ClearPanelControl();
        }


        public void ClearPanelControl()
        {
            panel1.Controls.Clear();
        }

        public UserForm SetFormToPanel(UserFormInfo pInfo)
        {
            UserForm pForm = null;


            Type t = Type.GetType(pInfo.FormName);
            try
            {
                if (pForm == null)
                {
                    AddFormToPanel(pInfo);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

            return pForm;

        }

        public UserForm AddFormToPanel(UserFormInfo pInfo, string sApp = null)
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

                    panel1.Controls.Add(pForm);

                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            return pForm;
        }

        public void SetFormToPanel(UserForm pForm)
        {
            panel1.Controls.Clear();
            pForm.DisplayIn();
            panel1.Controls.Add(pForm);
        }
    }
}
