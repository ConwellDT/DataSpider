using System;
using System.Windows.Forms;

using DataSpider.PC00.PT;

namespace DataSpider.UserMonitor
{
    public partial class TagPositionEdit : Form
    {
        public string DelimeterUse { get; set; }
        public string LineValue { get; set; }
        public string DelimeterVale { get; set; }
        public string ItemIndexValue { get; set; }
        public string OffsetValue { get; set; }
        public string SizeValue { get; set; }

        private PC00Z01 sqlBiz = new PC00Z01();

        public TagPositionEdit(string use, string line, string delimeter, string itemindex, string offset, string size)
        {

            InitializeComponent();

            comboBox_DelimeterUse.Text = use;

            if (use.Equals("Y"))
            {
                textBoxDelimeter.Enabled = true;
                textBoxItemIndex.Enabled = true;

                if (string.IsNullOrWhiteSpace(line) == false)
                {
                    textBoxLine.Text = line.Trim();
                }
                if (string.IsNullOrWhiteSpace(offset) == false)
                {
                    textBoxOffset.Text = offset.Trim();
                }
                if (string.IsNullOrWhiteSpace(size) == false)
                {
                    textBoxSize.Text = size.Trim();
                }

                textBoxDelimeter.Text = delimeter;

                if (string.IsNullOrWhiteSpace(itemindex) == false)
                {
                    textBoxItemIndex.Text = itemindex.Trim();
                }
            }
            else
            {
                textBoxDelimeter.Enabled = false;
                textBoxItemIndex.Enabled = false;

                textBoxDelimeter.Text = string.Empty;
                textBoxItemIndex.Text = string.Empty;

                if (string.IsNullOrEmpty(line) == false)
                {
                    textBoxLine.Text = line.Trim();
                }
                if (string.IsNullOrEmpty(delimeter) == false)
                {
                    textBoxOffset.Text = delimeter.Trim();
                }
                if (string.IsNullOrEmpty(itemindex) == false)
                {
                    textBoxSize.Text = itemindex.Trim();
                }
            }
        }

        private void button_OK_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(comboBox_DelimeterUse.Text))
            {
                MessageBox.Show($"Delimeter Use를 선택하세요.", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (string.IsNullOrWhiteSpace(textBoxLine.Text))
            {
                MessageBox.Show($"Line을 입력하세요.", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (string.IsNullOrWhiteSpace(textBoxOffset.Text))
            {
                MessageBox.Show($"Offset을 입력하세요.", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (string.IsNullOrWhiteSpace(textBoxSize.Text))
            {
                MessageBox.Show($"Size를 입력하세요.", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            if (comboBox_DelimeterUse.SelectedItem.ToString().Equals("Y"))
            {
                if (string.IsNullOrWhiteSpace(textBoxItemIndex.Text))
                {
                    MessageBox.Show($"Item Index를 입력하세요.", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                DelimeterUse = comboBox_DelimeterUse.Text.Trim();
                LineValue = textBoxLine.Text.Trim();
                DelimeterVale = textBoxDelimeter.Text;
                ItemIndexValue = textBoxItemIndex.Text.Trim();
                OffsetValue = textBoxOffset.Text.Trim();
                SizeValue = textBoxSize.Text.Trim();
            }
            else
            {
                DelimeterUse = comboBox_DelimeterUse.Text;
                LineValue = textBoxLine.Text;
                OffsetValue = textBoxOffset.Text;
                SizeValue = textBoxSize.Text;
            }

            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void button_Cancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void button_Click(object sender, EventArgs e)
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

        private void comboBox_DelimeterUse_SelectedIndexChanged(object sender, EventArgs e)
        {
            textBoxDelimeter.Text = string.Empty;
            textBoxItemIndex.Text = string.Empty;

            if (comboBox_DelimeterUse.SelectedItem.ToString().Equals("Y"))
            {
                textBoxDelimeter.Enabled = true;
                textBoxItemIndex.Enabled = true;
            }
            else
            {
                textBoxDelimeter.Enabled = false;
                textBoxItemIndex.Enabled = false;
            }
        }
    }
}
