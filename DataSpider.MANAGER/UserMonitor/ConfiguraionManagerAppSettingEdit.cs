﻿using System;
using System.Data;
using System.Windows.Forms;

using AppSettings;

using DataSpider.PC00.PT;

namespace DataSpider.UserMonitor
{
    public partial class ConfiguraionManagerAppSettingEdit : Form
    {
        int EditMode = -1;
        public string beforeKey = string.Empty;
        public const int EDIT_MODE_ADD = 0;
        public const int EDIT_MODE_UPDATE = 1;
        public string key = string.Empty;
        public string value = string.Empty;

        private PC00Z01 sqlBiz = new PC00Z01();
        public ConfiguraionManagerAppSettingEdit(string key, string value, int edMode)
        {
            InitializeComponent();


            EditMode = edMode;


            if (string.IsNullOrEmpty(key) == false)
            {
                beforeKey = key;
                textBoxKey.Text = key;
            }

            if (string.IsNullOrEmpty(value) == false)
            {
                textBoxValue.Text = value;
            }
        }

        private void button_OK_Click(object sender, EventArgs e)
        {
            string strErrvalue = string.Empty;
            string strErrText = string.Empty;

            key = textBoxKey.Text;
            value = textBoxValue.Text;

            if (key == string.Empty)
            {
                MessageBox.Show("Key is not exist", $"Key invalid", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            if (value == string.Empty)
            {
                MessageBox.Show("Value is not exist", $"Value invalid", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (EditMode == EDIT_MODE_ADD)
            {
                AppSetting.Append(textBoxKey.Text.Trim(), textBoxValue.Text.Trim());
                MessageBox.Show($"Commonvalue has been saved.", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
                DialogResult = DialogResult.OK;
                this.Close();
            }
            else if (EditMode == EDIT_MODE_UPDATE)
            {
                AppSetting.Set(beforeKey, textBoxKey.Text.Trim(), textBoxValue.Text.Trim());
                MessageBox.Show($"Commonvalue has been saved.", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
                DialogResult = DialogResult.OK;
                this.Close();
            }

        }

        private void button_Cancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            this.Close();
        }
    }
}
