using System;
using System.Configuration;
using System.Data;
using System.IO;
using System.Windows.Forms;

using AppSettings;

using DataSpider.PC00.PT;

namespace DataSpider.UserMonitor
{
    public partial class ConfigurationManagerAppSetting : Form
    {
        private PC00Z01 sqlBiz = new PC00Z01();

        public ConfigurationManagerAppSetting()
        {
            InitializeComponent();
        }

        private void ConfigurationManagerAppSetting_Load(object sender, EventArgs e)
        {
            comboBox_Config.SelectedIndex = 0;
        }
        
        private void LoadAppConfig()
        {
            string configFilePath = textBox_FilePath.Text.Trim();

            if (string.IsNullOrEmpty(configFilePath) || !File.Exists(configFilePath))
            {
                MessageBox.Show("Configuration file not found.");
                return;
            }

            //인스턴스를 생성하고 설정 파일의 경로 지정
            ExeConfigurationFileMap configFileMap = new ExeConfigurationFileMap();
            configFileMap.ExeConfigFilename = configFilePath;

            Configuration config = ConfigurationManager.OpenMappedExeConfiguration(configFileMap, ConfigurationUserLevel.None);

            foreach (KeyValueConfigurationElement kvConf in config.AppSettings.Settings)
            {
                dataGridView_AppConfig.Rows.Add(chk.ValueType,kvConf.Key, kvConf.Value);
            }
        }

        private void button_Refresh_Click(object sender, EventArgs e)
        {
            dataGridView_AppConfig.Rows.Clear();
            LoadAppConfig();
        }


        private void button_Set_Click(object sender, EventArgs e)
        {
            string selKey = string.Empty;
            string selValue = string.Empty;

            if (dataGridView_AppConfig.CurrentRow == null)
            {
                MessageBox.Show("Key/Value is not selected", $"App Config", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // DataSource가 null인 경우 데이터는 Cells를 통해 접근
            if (dataGridView_AppConfig.DataSource == null)
            {
                DataGridViewRow currentRow = dataGridView_AppConfig.CurrentRow;
                if (currentRow != null)
                {
                    selKey = currentRow.Cells["Key"].Value?.ToString() ?? string.Empty;
                    selValue = currentRow.Cells["Value"].Value?.ToString() ?? string.Empty;
                }
            }
            else
            {
                // DataSource가 설정되어 있으면 DataBoundItem을 통해 접근
                DataRowView selRow = dataGridView_AppConfig.CurrentRow.DataBoundItem as DataRowView;
                if (selRow != null)
                {
                    selKey = selRow["Key"].ToString();
                    selValue = selRow["Value"].ToString();
                }
            }

            // 선택된 데이터를 사용하여 편집 창 열기
            ConfiguraionManagerAppSettingEdit dlg = new ConfiguraionManagerAppSettingEdit(selKey, selValue, CommonCodenfoEdit.EDIT_MODE_UPDATE);
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                dataGridView_AppConfig.Rows.Clear();
                LoadAppConfig();
            }
        }

        private void button_Append_Click(object sender, EventArgs e)
        {
            ConfiguraionManagerAppSettingEdit dlg = new ConfiguraionManagerAppSettingEdit("", "", CommonCodenfoEdit.EDIT_MODE_ADD);
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                dataGridView_AppConfig.Rows.Clear();
                LoadAppConfig();
            }
        }

        private void button_Remove_Click(object sender, EventArgs e)
        {
            string strErrCode = string.Empty;
            string strErrText = string.Empty;
            bool isRowSelected = false;

            foreach (DataGridViewRow row in dataGridView_AppConfig.Rows)
            {
                if (Convert.ToBoolean(row.Cells[0].Value))
                {
                    isRowSelected = true;
                    break;
                }
            }

            if (!isRowSelected)
            {
                MessageBox.Show("CheckBox is not selected", $"Common Code Delete", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (MessageBox.Show("Are you sure you want to delete the selected rows?", $"Delete CommonCode", MessageBoxButtons.OKCancel, MessageBoxIcon.Information) == DialogResult.OK)
            {
                for (int i = dataGridView_AppConfig.Rows.Count - 1; i >= 0; i--)
                {
                    DataGridViewRow row = dataGridView_AppConfig.Rows[i];
                    if (Convert.ToBoolean(row.Cells[0].Value))
                    {
                        AppSetting.Remove(row.Cells[1].Value.ToString());
                    }
                }
                MessageBox.Show("Configuration Manager App Setting Removed.", $"AppConfig", MessageBoxButtons.OK, MessageBoxIcon.Information);
                dataGridView_AppConfig.Rows.Clear();
                LoadAppConfig();
            }
        }

        private void comboBox_Config_SelectedIndexChanged(object sender, EventArgs e)
        {
            dataGridView_AppConfig.Rows.Clear();

            string selectedItem = comboBox_Config.SelectedItem.ToString();
            if (selectedItem == "Manager") selectedItem = string.Empty;
            string configPath = GetConfigFilePath(selectedItem);
            textBox_FilePath.Text = configPath;

            LoadAppConfig();
        }

        private string GetConfigFilePath(string name)
        {
            string rootPath = AppDomain.CurrentDomain.BaseDirectory;
            string configFileName = "DataSpider"+name+".exe.Config";
            string fullPath = Path.Combine(rootPath, configFileName);

            if (File.Exists(fullPath))
            {
                return fullPath;
            }
            else
            {
                MessageBox.Show("Config file not found.");
                return string.Empty;
            }
        }

        private void button_Close_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
