using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DataSpider.UserMonitor
{
    public partial class FilterForm : Form
    {
        private DateTime dtSelDateTimeMin = DateTime.MinValue;
        private DateTime dtSelDateTimeMax = DateTime.MinValue;

        public string TagNameFilter
        {
            get { return textBoxTagNameFilter.Text; }           
            set { textBoxTagNameFilter.Text = value; }
        }

        public DateTime DateTimeFilterCurMin
        {
            get { return dtSelDateTimeMin; }
            set 
            { 
                dtSelDateTimeMin = value;
                if (dtSelDateTimeMin != DateTime.MinValue)
                {
                    dateTimePickerDateMin.Value = dtSelDateTimeMin;
                    dateTimePickerTimeMin.Value = dtSelDateTimeMin;
                }
                else
                {
                    dateTimePickerDateMin.Value = DateTime.Now;
                    dateTimePickerTimeMin.Value = DateTime.Now;
                }

                textBoxDateTimeSelMin.Text = dtSelDateTimeMin.ToString("yyyy-MM-dd HH:mm:ss.ff");
            }
        }

        public DateTime DateTimeFilterCurMax
        {
            get { return dtSelDateTimeMax; }
            set
            {
                dtSelDateTimeMax = value;
                if (dtSelDateTimeMax != DateTime.MinValue)
                {
                    dateTimePickerDateMax.Value = dtSelDateTimeMax;
                    dateTimePickerTimeMax.Value = dtSelDateTimeMax;
                }
                else
                {
                    dateTimePickerDateMax.Value = DateTime.Now;
                    dateTimePickerTimeMax.Value = DateTime.Now;
                }

                textBoxDateTimeSelMax.Text = dtSelDateTimeMax.ToString("yyyy-MM-dd HH:mm:ss.ff");
            }
        }

        public string DescriptionFilter
        {
            get { return textBoxDescriptionFilter.Text; }
            set { textBoxDescriptionFilter.Text = value; }
        }

        public FilterForm()
        {
            InitializeComponent();
        }

        private void buttonInitialize_Click(object sender, EventArgs e)
        {
            TagNameFilter = "";
            DescriptionFilter = "";
            dtSelDateTimeMin = DateTime.MinValue;
            dtSelDateTimeMax = DateTime.MinValue;
        }

        private void FilterForm_Load(object sender, EventArgs e)
        {
            textBoxDateTimeSelMin.Text = dtSelDateTimeMin.ToString("yyyy-MM-dd HH:mm:ss.ff");
            dateTimePickerTimeMin.ShowUpDown = true;
            dateTimePickerTimeMax.ShowUpDown = true;
        }

        private void dateTimePickerDateMin_ValueChanged(object sender, EventArgs e)
        {
            int nH = 0, nM = 0, nS = 0;

            nH = dtSelDateTimeMin.Hour;
            nM = dtSelDateTimeMin.Minute;
            nS = dtSelDateTimeMin.Second;

            dtSelDateTimeMin = new DateTime(dateTimePickerDateMin.Value.Year, dateTimePickerDateMin.Value.Month, dateTimePickerDateMin.Value.Day,
                                     nH, nM, nS);

            textBoxDateTimeSelMin.Text = dtSelDateTimeMin.ToString("yyyy-MM-dd HH:mm:ss.ff");
        }

        private void dateTimePickerTimeMin_ValueChanged(object sender, EventArgs e)
        {
            int nY = 0, nM = 0, nD = 0;

            nY = dtSelDateTimeMin.Year;
            nM = dtSelDateTimeMin.Month;
            nD = dtSelDateTimeMin.Day;

            dtSelDateTimeMin = new DateTime( nY, nM, nD, 
                                          dateTimePickerTimeMin.Value.Hour, dateTimePickerTimeMin.Value.Minute, dateTimePickerTimeMin.Value.Second);

            textBoxDateTimeSelMin.Text = dtSelDateTimeMin.ToString("yyyy-MM-dd HH:mm:ss.ff");
        }

        private void dateTimePickerDateMax_ValueChanged(object sender, EventArgs e)
        {
            int nH = 0, nM = 0, nS = 0;

            nH = dtSelDateTimeMax.Hour;
            nM = dtSelDateTimeMax.Minute;
            nS = dtSelDateTimeMax.Second;

            dtSelDateTimeMax = new DateTime(dateTimePickerDateMax.Value.Year, dateTimePickerDateMax.Value.Month, dateTimePickerDateMax.Value.Day,
                                     nH, nM, nS);

            textBoxDateTimeSelMax.Text = dtSelDateTimeMax.ToString("yyyy-MM-dd HH:mm:ss.ff");
        }

        private void dateTimePickerTimeMax_ValueChanged(object sender, EventArgs e)
        {
            int nY = 0, nM = 0, nD = 0;

            nY = dtSelDateTimeMax.Year;
            nM = dtSelDateTimeMax.Month;
            nD = dtSelDateTimeMax.Day;

            dtSelDateTimeMax = new DateTime(nY, nM, nD,
                                          dateTimePickerTimeMax.Value.Hour, dateTimePickerTimeMax.Value.Minute, dateTimePickerTimeMax.Value.Second);

            textBoxDateTimeSelMax.Text = dtSelDateTimeMax.ToString("yyyy-MM-dd HH:mm:ss.ff");
        }

        private void textBoxDateTimeSelMin_TextChanged(object sender, EventArgs e)
        {
            DateTime dtTemp;

            if( DateTime.TryParse(textBoxDateTimeSelMin.Text, out dtTemp) == true )
            {
                if (dtTemp > dateTimePickerDateMin.MinDate)
                {
                    dateTimePickerDateMin.Value = dtTemp;
                    dateTimePickerTimeMin.Value = dtTemp;
                }
            }
        }

        private void textBoxDateTimeSelMax_TextChanged(object sender, EventArgs e)
        {
            DateTime dtTemp;

            if (DateTime.TryParse(textBoxDateTimeSelMax.Text, out dtTemp) == true)
            {
                if (dtTemp > dateTimePickerDateMax.MinDate)
                {
                    dateTimePickerDateMax.Value = dtTemp;
                    dateTimePickerTimeMax.Value = dtTemp;
                }
            }
        }
    }
}
