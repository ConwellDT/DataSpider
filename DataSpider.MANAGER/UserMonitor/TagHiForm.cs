using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SEIMM.UserMonitor
{
    public partial class TagHiForm : Form
    {
        public string EqTag { get; set; }
        public TagHiForm()
        {
            InitializeComponent();
        }

        private void hI_MEASURE_RESULTBindingNavigatorSaveItem_Click(object sender, EventArgs e)
        {
            this.Validate();
            this.hI_MEASURE_RESULTBindingSource.EndEdit();
            this.tableAdapterManager.UpdateAll(this.sEIMMDataSet);

        }

        private void TagHiForm_Load(object sender, EventArgs e)
        {
            // TODO: 이 코드는 데이터를 'sEIMMDataSet.HI_MEASURE_RESULT' 테이블에 로드합니다. 필요 시 이 코드를 이동하거나 제거할 수 있습니다.
            //this.hI_MEASURE_RESULTTableAdapter.Fill(this.sEIMMDataSet.HI_MEASURE_RESULT);
            ChangeGridInformation(EqTag);

        }

        private void ChangeGridInformation(string EqTagj)
        {

            try
            {

                string sSQL = string.Empty;
                string sFilter = EqTagj;
                string sOrder = "";

                sSQL += $"SELECT [HI_SEQ]";
                sSQL += $"      ,[TAG_NM]";
                sSQL += $"      ,[MEASURE_VALUE]";
                sSQL += $"      ,[MEASURE_DATE]";
                sSQL += $"      ,[IF_FLAG]";
                sSQL += $"      ,[REG_DATE]";
                sSQL += $"      ,[REG_ID]";
                sSQL += $"  FROM [SEIMM].[dbo].[HI_MEASURE_RESULT]";


                if (!string.IsNullOrEmpty(sFilter))
                    sSQL += $"{" "} where TAG_NM =  '{sFilter}'";

                if (!string.IsNullOrEmpty(sOrder))
                    sSQL += $"{" "} Order by [REG_DATE] desc";


                SqlCommand pCommand = new SqlCommand(sSQL, hI_MEASURE_RESULTTableAdapter.Connection);
                pCommand.CommandType = global::System.Data.CommandType.Text;
                this.hI_MEASURE_RESULTTableAdapter.Adapter.SelectCommand = pCommand;

                sEIMMDataSet.HI_MEASURE_RESULT.Clear();
                this.hI_MEASURE_RESULTTableAdapter.Adapter.Fill(sEIMMDataSet.HI_MEASURE_RESULT);




            }
            catch (Exception e)
            {

                MessageBox.Show(e.Message);
                //Log.ExceptionLog(e);
                // //throw;
            }
            //GetEquipmentType();
        }
    }
}
