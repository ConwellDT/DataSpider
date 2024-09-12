using System;
using System.Globalization;
using System.Windows.Forms;

using DataSpider.PC00.PT;

using static DevExpress.Drawing.Printing.Internal.DXPageSizeInfo;

namespace DataSpider.UserMonitor
{
    public partial class TagPositionEdit : Form
    {
        public string LineValue { get; set; }
        public string OffsetValue { get; set; }
        public string SizeValue { get; set; }

        private PC00Z01 sqlBiz = new PC00Z01();
        public TagPositionEdit()
        {
            InitializeComponent();
        }
        public TagPositionEdit(string line, string offset, string size)
        {

            InitializeComponent();

            if (string.IsNullOrEmpty(line) == false)
            {
                textBoxLine.Text = line;
            }
            if (string.IsNullOrEmpty(offset) == false)
            {
                textBoxOffset.Text = offset;
            }
            if (string.IsNullOrEmpty(size) == false)
            {
                buttonEdit_Size.Text = size;
            }
        }

        private void button_OK_Click(object sender, EventArgs e)
        {
            LineValue = textBoxLine.Text;
            OffsetValue = textBoxOffset.Text;
            SizeValue = buttonEdit_Size.Text;

            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void button_Cancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void buttonEdit_Size_ButtonClick(object sender, DevExpress.XtraEditors.Controls.ButtonPressedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(buttonEdit_Size.Text))
            {
                TagSizeEdit dlg = new TagSizeEdit();
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    buttonEdit_Size.Text = dlg.SizeEdit;
                }
            }
            else 
            {
                TagSizeEdit dlg = new TagSizeEdit(buttonEdit_Size.Text);
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    buttonEdit_Size.Text = dlg.SizeEdit;
                }
            }
        }
    }
}
