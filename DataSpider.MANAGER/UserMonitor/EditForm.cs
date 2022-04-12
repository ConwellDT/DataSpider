using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace SEIMM.UserMonitor
{
    public partial class EditForm : LibraryWH.FormCtrl.UserForm
    {
        public EditForm()
        {
            InitializeComponent();
        }


        private void EditForm_Load(object sender, EventArgs e)
        {
            // TODO: 이 코드는 데이터를 'sEIMMDataSet.MA_EQUIPMENT_CD_Test' 테이블에 로드합니다. 필요 시 이 코드를 이동하거나 제거할 수 있습니다.
            //this.mA_EQUIPMENT_CD_TestTableAdapter.Fill(this.sEIMMDataSet.MA_EQUIPMENT_CD_Test);



        }

        private void mA_EQUIPMENT_CD_TestBindingNavigatorSaveItem_Click(object sender, EventArgs e)
        {
            this.Validate();
            this.mA_EQUIPMENT_CD_TestBindingSource.EndEdit();
            this.tableAdapterManager.UpdateAll(this.sEIMMDataSet);

        }

        private void mA_EQUIPMENT_CD_TestBindingNavigator_RefreshItems(object sender, EventArgs e)
        {

        }
    }

}
