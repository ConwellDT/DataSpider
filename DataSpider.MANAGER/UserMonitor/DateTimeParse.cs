using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlTypes;
using System.Globalization;
using System.Linq;
using System.Text.Json;
using System.Windows.Forms;

using DataSpider.PC00.PT;

namespace DataSpider.UserMonitor
{
    public partial class DateTimeParse : Form
    {
        private PC00Z01 sqlBiz = new PC00Z01();

        public DateTimeParse()
        {
            InitializeComponent();
        }

        private void CommonCodeConfig_Load(object sender, EventArgs e)
        {
            InitControls();
        }

        private void InitControls()
        {
            string strErrCode = string.Empty;
            string strErrText = string.Empty;

            textBox_Input.Text = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

            DataTable dtCommonCd = sqlBiz.GetEquipmentInfo("","", MonitorForm.showAllEquipmtStatus, ref strErrCode, ref strErrText);

            if (dtCommonCd.Rows.Count > 0)
            {
                for (int nR = 0; nR < dtCommonCd.Rows.Count; nR++)
                {
                    int rowId = dataGridEquipmentName.Rows.Add();

                    DataGridViewRow row = dataGridEquipmentName.Rows[rowId];

                    row.Cells["EquipmentName"].Value = dtCommonCd.Rows[nR]["EQUIP_NM"].ToString();
                }
            }
        }

        private void dataGridEquipmentName_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            string strErrCode = string.Empty;
            string strErrText = string.Empty;
            string equipmentName = string.Empty;
            string result = string.Empty;

            DataGridViewRow currentRow = dataGridEquipmentName.CurrentRow;
            if(currentRow != null)
            {
                equipmentName = currentRow.Cells["EquipmentName"].Value?.ToString() ?? string.Empty;
            }

            DataTable dtEquipment = sqlBiz.GetEquipmentDateTimeInfo( equipmentName, ref strErrCode, ref strErrText);

            if (dtEquipment.Rows.Count > 0)
            {
                string extraInfo = dtEquipment.Rows[0]["EXTRA_INFO"].ToString();

                if (extraInfo != "" && string.IsNullOrWhiteSpace(extraInfo))
                {
                    JsonDocument document = JsonDocument.Parse(extraInfo);
                    result = document.RootElement.GetProperty("TimeFormat").GetString();

                }
                else
                {
                    DataTable dt = sqlBiz.GetCommonCode("TIMEFORMAT", ref strErrCode, ref strErrText);
                    if (dt != null && dt.Rows.Count > 0)
                    {
                         result = dt.Rows[0]["CODE_VALUE"].ToString();
                    }
                }

                textBox_Format.Text = result;
            }
        }

        private void button_Parse_Click(object sender, EventArgs e)
        {
            DateTime dtDateTime;
            if (DateTime.TryParseExact(textBox_Input.Text.Trim(), textBox_Format.Text.Split(',').ToArray(), CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal | DateTimeStyles.AllowInnerWhite, out dtDateTime))
            {
                textBox_CustomOutput.Text = dtDateTime.ToString();
            }
            else
            {
                textBox_CustomOutput.Text = "NG";
            }

            if (DateTime.TryParse(textBox_Input.Text.Trim(), out dtDateTime))
            {
                textBox_BasicOutput.Text = dtDateTime.ToString();
            }
            else
            {
                textBox_BasicOutput.Text = "NG";
            }

            if (DateTime.TryParse(textBox_Input.Text.Trim(), CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal, out dtDateTime))
            {
                textBox_BasicOutput2.Text = dtDateTime.ToString();
            }
            else
            {
                textBox_BasicOutput2.Text = "NG";
            }
        }
    }
}
