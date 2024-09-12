using System.Windows.Forms;

using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace DataSpider.UserMonitor
{
    public partial class TagSizeEdit : Form
    {
        public string SizeEdit { get; set; }

        public TagSizeEdit()
        {
            InitializeComponent();
        }

        public TagSizeEdit(string size)
        {
            InitializeComponent();
            SetRadioButtonChecked(int.Parse(size));
        }

        private void SetRadioButtonChecked(int size)
        {
            // Assuming you have RadioButton controls named radioButtonSmall, radioButtonMedium, and radioButtonLarge
            switch (size)
            {
                case -1:
                    radioButton1.Checked = true;
                    richTextBoxScript.Text = "끝까지";
                    break;
                case -2:
                    radioButton2.Checked = true;
                    richTextBoxScript.Text = "공백이 나올때까지, 공백을 찾아야 하는데 일단 앞뒤 공백제거 후 해야 함\r\n";
                    break;
                case -3:
                    radioButton3.Checked = true;
                    richTextBoxScript.Text = "뒤에서 부터 offset 부터 끝까지";
                    break;
                case -4:
                    radioButton4.Checked = true;
                    richTextBoxScript.Text = "정해진 줄, 정해진 위치부터 숫자만 추출";
                    break;
                case -5:
                    radioButton5.Checked = true;
                    richTextBoxScript.Text = "정해진 줄, 정해진 위치부터 숫자에 해당하는 문자열 추출";
                    break;
                default:
                    radioButton6.Checked = true;
                    textBoxDefault.Visible = true;
                    textBoxDefault.Text = size.ToString();
                    richTextBoxScript.Text = "정해진 사이즈 길이만큼";
                    break;
            }
        }

        private void radioButton_CheckedChanged(object sender, System.EventArgs e)
        {
            richTextBoxScript.Text = string.Empty;
            textBoxDefault.Visible = false;
            // 선택된 라디오 버튼을 찾기
            RadioButton selectedRadioButton = sender as RadioButton;

            if (selectedRadioButton == null || !selectedRadioButton.Checked) return;
            switch (selectedRadioButton.Name)
            {
                case "radioButton1":
                    richTextBoxScript.Text = "끝까지";
                    break;
                case "radioButton2":
                    richTextBoxScript.Text = "공백이 나올때까지, 공백을 찾아야 하는데 일단 앞뒤 공백제거 후 해야 함\r\n";
                    break;
                case "radioButton3":
                    richTextBoxScript.Text = "뒤에서 부터 offset 부터 끝까지";
                    break;
                case "radioButton4":
                    richTextBoxScript.Text = "정해진 줄, 정해진 위치부터 숫자만 추출";
                    break;
                case "radioButton5":
                    richTextBoxScript.Text = "정해진 줄, 정해진 위치부터 숫자에 해당하는 문자열 추출";
                    break;
                default:
                    textBoxDefault.Visible = true;
                    richTextBoxScript.Text = "정해진 사이즈 길이만큼";
                    break;
            }

        }

        private void button_OK_Click(object sender, System.EventArgs e)
        {
            // Check if at least one radio button is checked
            if (!radioButton1.Checked && !radioButton2.Checked && !radioButton3.Checked && !radioButton4.Checked && !radioButton5.Checked && !radioButton6.Checked)
            {
                MessageBox.Show("하나의 버튼을 클릭하세요", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // Determine which radio button is checked
            if (radioButton1.Checked)
            {
                SizeEdit = "-1";
            }
            else if (radioButton2.Checked)
            {
                SizeEdit = "-2";
            }
            else if (radioButton3.Checked)
            {
                SizeEdit = "-3";
            }
            else if (radioButton4.Checked)
            {
                SizeEdit = "-4";
            }
            else if (radioButton5.Checked)
            {
                SizeEdit = "-5";
            }
            else if (radioButton6.Checked)
            {
                // Use the value from textBoxDefault if radioButton6 is checked
                SizeEdit = textBoxDefault.Text;
                if (string.IsNullOrWhiteSpace(SizeEdit) || !int.TryParse(SizeEdit, out _))
                {
                    MessageBox.Show("숫자를 입력하세요", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }
            }

            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void button_Cancel_Click(object sender, System.EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            this.Close();
        }
    }
}
