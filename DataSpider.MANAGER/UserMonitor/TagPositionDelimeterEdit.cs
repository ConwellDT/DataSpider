using System;
using System.Windows.Forms;

using DataSpider.PC00.PT;

namespace DataSpider.UserMonitor
{
    public partial class TagPositionDelimeterEdit : Form
    {
        public string LineValue { get; set; }
        public string DelimeterVale { get; set; }

        public string ItemIndexValue { get; set; }

        public string OffsetValue { get; set; }
        public string SizeValue { get; set; }

        private PC00Z01 sqlBiz = new PC00Z01();
        public TagPositionDelimeterEdit()
        {
            InitializeComponent();
        }
        public TagPositionDelimeterEdit(string line, string delimeter, string itemindex, string offset, string size)
        {

            InitializeComponent();

            if (string.IsNullOrEmpty(line) == false)
            {
                textBoxLine.Text = line;
            }
            if (string.IsNullOrEmpty(delimeter) == false)
            {
                textBoxDelimeter.Text = delimeter;
            }
            if (string.IsNullOrEmpty(itemindex) == false)
            {
                textBoxItemIndex.Text = itemindex;
            }
            if (string.IsNullOrEmpty(offset) == false)
            {
                textBoxOffset.Text = offset;
            }
            if (string.IsNullOrEmpty(size) == false)
            {
                textBoxSize.Text = size;
            }
        }

        private void button_OK_Click(object sender, EventArgs e)
        {
            LineValue = textBoxLine.Text;
            DelimeterVale = textBoxDelimeter.Text;
            ItemIndexValue = textBoxItemIndex.Text; 
            OffsetValue = textBoxOffset.Text;
            SizeValue = textBoxSize.Text;

            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void button_Cancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void buttonTimePosition_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(textBoxSize.Text))
            {
                TagSizeEdit dlg = new TagSizeEdit();
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    textBoxSize.Text = dlg.SizeEdit;
                }
            }
            else
            {
                TagSizeEdit dlg = new TagSizeEdit(textBoxSize.Text);
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    textBoxSize.Text = dlg.SizeEdit;
                }
            }
        }
    }
}
