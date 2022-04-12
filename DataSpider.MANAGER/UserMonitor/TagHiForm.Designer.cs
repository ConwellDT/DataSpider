
namespace SEIMM.UserMonitor
{
    partial class TagHiForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(TagHiForm));
            this.sEIMMDataSet = new SEIMM.SEIMMDataSet();
            this.hI_MEASURE_RESULTBindingSource = new System.Windows.Forms.BindingSource(this.components);
            this.hI_MEASURE_RESULTTableAdapter = new SEIMM.SEIMMDataSetTableAdapters.HI_MEASURE_RESULTTableAdapter();
            this.tableAdapterManager = new SEIMM.SEIMMDataSetTableAdapters.TableAdapterManager();
            this.hI_MEASURE_RESULTBindingNavigator = new System.Windows.Forms.BindingNavigator(this.components);
            this.bindingNavigatorSeparator = new System.Windows.Forms.ToolStripSeparator();
            this.bindingNavigatorPositionItem = new System.Windows.Forms.ToolStripTextBox();
            this.bindingNavigatorCountItem = new System.Windows.Forms.ToolStripLabel();
            this.bindingNavigatorSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.bindingNavigatorSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.hI_MEASURE_RESULTDataGridView = new System.Windows.Forms.DataGridView();
            this.dataGridViewTextBoxColumn1 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewTextBoxColumn2 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewTextBoxColumn3 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewTextBoxColumn4 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewTextBoxColumn5 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewTextBoxColumn6 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewTextBoxColumn7 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.bindingNavigatorAddNewItem = new System.Windows.Forms.ToolStripButton();
            this.bindingNavigatorDeleteItem = new System.Windows.Forms.ToolStripButton();
            this.bindingNavigatorMoveFirstItem = new System.Windows.Forms.ToolStripButton();
            this.bindingNavigatorMovePreviousItem = new System.Windows.Forms.ToolStripButton();
            this.bindingNavigatorMoveNextItem = new System.Windows.Forms.ToolStripButton();
            this.bindingNavigatorMoveLastItem = new System.Windows.Forms.ToolStripButton();
            this.hI_MEASURE_RESULTBindingNavigatorSaveItem = new System.Windows.Forms.ToolStripButton();
            ((System.ComponentModel.ISupportInitialize)(this.sEIMMDataSet)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.hI_MEASURE_RESULTBindingSource)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.hI_MEASURE_RESULTBindingNavigator)).BeginInit();
            this.hI_MEASURE_RESULTBindingNavigator.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.hI_MEASURE_RESULTDataGridView)).BeginInit();
            this.SuspendLayout();
            // 
            // sEIMMDataSet
            // 
            this.sEIMMDataSet.DataSetName = "SEIMMDataSet";
            this.sEIMMDataSet.SchemaSerializationMode = System.Data.SchemaSerializationMode.IncludeSchema;
            // 
            // hI_MEASURE_RESULTBindingSource
            // 
            this.hI_MEASURE_RESULTBindingSource.DataMember = "HI_MEASURE_RESULT";
            this.hI_MEASURE_RESULTBindingSource.DataSource = this.sEIMMDataSet;
            // 
            // hI_MEASURE_RESULTTableAdapter
            // 
            this.hI_MEASURE_RESULTTableAdapter.ClearBeforeFill = true;
            // 
            // tableAdapterManager
            // 
            this.tableAdapterManager.BackupDataSetBeforeUpdate = false;
            this.tableAdapterManager.HI_MEASURE_RESULTTableAdapter = this.hI_MEASURE_RESULTTableAdapter;
            this.tableAdapterManager.MA_COMMON_CDTableAdapter = null;
            this.tableAdapterManager.MA_EQUIPMENT_CD_TestTableAdapter = null;
            this.tableAdapterManager.MA_EQUIPMENT_CDTableAdapter = null;
            this.tableAdapterManager.MA_TAG_CDTableAdapter = null;
            this.tableAdapterManager.UpdateOrder = SEIMM.SEIMMDataSetTableAdapters.TableAdapterManager.UpdateOrderOption.InsertUpdateDelete;
            // 
            // hI_MEASURE_RESULTBindingNavigator
            // 
            this.hI_MEASURE_RESULTBindingNavigator.AddNewItem = this.bindingNavigatorAddNewItem;
            this.hI_MEASURE_RESULTBindingNavigator.BindingSource = this.hI_MEASURE_RESULTBindingSource;
            this.hI_MEASURE_RESULTBindingNavigator.CountItem = this.bindingNavigatorCountItem;
            this.hI_MEASURE_RESULTBindingNavigator.DeleteItem = this.bindingNavigatorDeleteItem;
            this.hI_MEASURE_RESULTBindingNavigator.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.bindingNavigatorMoveFirstItem,
            this.bindingNavigatorMovePreviousItem,
            this.bindingNavigatorSeparator,
            this.bindingNavigatorPositionItem,
            this.bindingNavigatorCountItem,
            this.bindingNavigatorSeparator1,
            this.bindingNavigatorMoveNextItem,
            this.bindingNavigatorMoveLastItem,
            this.bindingNavigatorSeparator2,
            this.bindingNavigatorAddNewItem,
            this.bindingNavigatorDeleteItem,
            this.hI_MEASURE_RESULTBindingNavigatorSaveItem});
            this.hI_MEASURE_RESULTBindingNavigator.Location = new System.Drawing.Point(0, 0);
            this.hI_MEASURE_RESULTBindingNavigator.MoveFirstItem = this.bindingNavigatorMoveFirstItem;
            this.hI_MEASURE_RESULTBindingNavigator.MoveLastItem = this.bindingNavigatorMoveLastItem;
            this.hI_MEASURE_RESULTBindingNavigator.MoveNextItem = this.bindingNavigatorMoveNextItem;
            this.hI_MEASURE_RESULTBindingNavigator.MovePreviousItem = this.bindingNavigatorMovePreviousItem;
            this.hI_MEASURE_RESULTBindingNavigator.Name = "hI_MEASURE_RESULTBindingNavigator";
            this.hI_MEASURE_RESULTBindingNavigator.PositionItem = this.bindingNavigatorPositionItem;
            this.hI_MEASURE_RESULTBindingNavigator.Size = new System.Drawing.Size(800, 25);
            this.hI_MEASURE_RESULTBindingNavigator.TabIndex = 0;
            this.hI_MEASURE_RESULTBindingNavigator.Text = "bindingNavigator1";
            // 
            // bindingNavigatorSeparator
            // 
            this.bindingNavigatorSeparator.Name = "bindingNavigatorSeparator";
            this.bindingNavigatorSeparator.Size = new System.Drawing.Size(6, 25);
            // 
            // bindingNavigatorPositionItem
            // 
            this.bindingNavigatorPositionItem.AccessibleName = "위치";
            this.bindingNavigatorPositionItem.AutoSize = false;
            this.bindingNavigatorPositionItem.Font = new System.Drawing.Font("맑은 고딕", 9F);
            this.bindingNavigatorPositionItem.Name = "bindingNavigatorPositionItem";
            this.bindingNavigatorPositionItem.Size = new System.Drawing.Size(50, 23);
            this.bindingNavigatorPositionItem.Text = "0";
            this.bindingNavigatorPositionItem.ToolTipText = "현재 위치";
            // 
            // bindingNavigatorCountItem
            // 
            this.bindingNavigatorCountItem.Name = "bindingNavigatorCountItem";
            this.bindingNavigatorCountItem.Size = new System.Drawing.Size(27, 22);
            this.bindingNavigatorCountItem.Text = "/{0}";
            this.bindingNavigatorCountItem.ToolTipText = "전체 항목 수";
            // 
            // bindingNavigatorSeparator1
            // 
            this.bindingNavigatorSeparator1.Name = "bindingNavigatorSeparator";
            this.bindingNavigatorSeparator1.Size = new System.Drawing.Size(6, 25);
            // 
            // bindingNavigatorSeparator2
            // 
            this.bindingNavigatorSeparator2.Name = "bindingNavigatorSeparator";
            this.bindingNavigatorSeparator2.Size = new System.Drawing.Size(6, 25);
            // 
            // hI_MEASURE_RESULTDataGridView
            // 
            this.hI_MEASURE_RESULTDataGridView.AutoGenerateColumns = false;
            this.hI_MEASURE_RESULTDataGridView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.hI_MEASURE_RESULTDataGridView.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.dataGridViewTextBoxColumn1,
            this.dataGridViewTextBoxColumn2,
            this.dataGridViewTextBoxColumn3,
            this.dataGridViewTextBoxColumn4,
            this.dataGridViewTextBoxColumn5,
            this.dataGridViewTextBoxColumn6,
            this.dataGridViewTextBoxColumn7});
            this.hI_MEASURE_RESULTDataGridView.DataSource = this.hI_MEASURE_RESULTBindingSource;
            this.hI_MEASURE_RESULTDataGridView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.hI_MEASURE_RESULTDataGridView.Location = new System.Drawing.Point(0, 25);
            this.hI_MEASURE_RESULTDataGridView.Name = "hI_MEASURE_RESULTDataGridView";
            this.hI_MEASURE_RESULTDataGridView.RowTemplate.Height = 23;
            this.hI_MEASURE_RESULTDataGridView.Size = new System.Drawing.Size(800, 425);
            this.hI_MEASURE_RESULTDataGridView.TabIndex = 1;
            // 
            // dataGridViewTextBoxColumn1
            // 
            this.dataGridViewTextBoxColumn1.DataPropertyName = "HI_SEQ";
            this.dataGridViewTextBoxColumn1.HeaderText = "HI_SEQ";
            this.dataGridViewTextBoxColumn1.Name = "dataGridViewTextBoxColumn1";
            this.dataGridViewTextBoxColumn1.ReadOnly = true;
            // 
            // dataGridViewTextBoxColumn2
            // 
            this.dataGridViewTextBoxColumn2.DataPropertyName = "TAG_NM";
            this.dataGridViewTextBoxColumn2.HeaderText = "TAG_NM";
            this.dataGridViewTextBoxColumn2.Name = "dataGridViewTextBoxColumn2";
            // 
            // dataGridViewTextBoxColumn3
            // 
            this.dataGridViewTextBoxColumn3.DataPropertyName = "MEASURE_VALUE";
            this.dataGridViewTextBoxColumn3.HeaderText = "MEASURE_VALUE";
            this.dataGridViewTextBoxColumn3.Name = "dataGridViewTextBoxColumn3";
            // 
            // dataGridViewTextBoxColumn4
            // 
            this.dataGridViewTextBoxColumn4.DataPropertyName = "MEASURE_DATE";
            this.dataGridViewTextBoxColumn4.HeaderText = "MEASURE_DATE";
            this.dataGridViewTextBoxColumn4.Name = "dataGridViewTextBoxColumn4";
            // 
            // dataGridViewTextBoxColumn5
            // 
            this.dataGridViewTextBoxColumn5.DataPropertyName = "IF_FLAG";
            this.dataGridViewTextBoxColumn5.HeaderText = "IF_FLAG";
            this.dataGridViewTextBoxColumn5.Name = "dataGridViewTextBoxColumn5";
            // 
            // dataGridViewTextBoxColumn6
            // 
            this.dataGridViewTextBoxColumn6.DataPropertyName = "REG_DATE";
            this.dataGridViewTextBoxColumn6.HeaderText = "REG_DATE";
            this.dataGridViewTextBoxColumn6.Name = "dataGridViewTextBoxColumn6";
            // 
            // dataGridViewTextBoxColumn7
            // 
            this.dataGridViewTextBoxColumn7.DataPropertyName = "REG_ID";
            this.dataGridViewTextBoxColumn7.HeaderText = "REG_ID";
            this.dataGridViewTextBoxColumn7.Name = "dataGridViewTextBoxColumn7";
            // 
            // bindingNavigatorAddNewItem
            // 
            this.bindingNavigatorAddNewItem.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.bindingNavigatorAddNewItem.Image = ((System.Drawing.Image)(resources.GetObject("bindingNavigatorAddNewItem.Image")));
            this.bindingNavigatorAddNewItem.Name = "bindingNavigatorAddNewItem";
            this.bindingNavigatorAddNewItem.RightToLeftAutoMirrorImage = true;
            this.bindingNavigatorAddNewItem.Size = new System.Drawing.Size(23, 22);
            this.bindingNavigatorAddNewItem.Text = "새로 추가";
            // 
            // bindingNavigatorDeleteItem
            // 
            this.bindingNavigatorDeleteItem.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.bindingNavigatorDeleteItem.Image = ((System.Drawing.Image)(resources.GetObject("bindingNavigatorDeleteItem.Image")));
            this.bindingNavigatorDeleteItem.Name = "bindingNavigatorDeleteItem";
            this.bindingNavigatorDeleteItem.RightToLeftAutoMirrorImage = true;
            this.bindingNavigatorDeleteItem.Size = new System.Drawing.Size(23, 22);
            this.bindingNavigatorDeleteItem.Text = "삭제";
            // 
            // bindingNavigatorMoveFirstItem
            // 
            this.bindingNavigatorMoveFirstItem.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.bindingNavigatorMoveFirstItem.Image = ((System.Drawing.Image)(resources.GetObject("bindingNavigatorMoveFirstItem.Image")));
            this.bindingNavigatorMoveFirstItem.Name = "bindingNavigatorMoveFirstItem";
            this.bindingNavigatorMoveFirstItem.RightToLeftAutoMirrorImage = true;
            this.bindingNavigatorMoveFirstItem.Size = new System.Drawing.Size(23, 22);
            this.bindingNavigatorMoveFirstItem.Text = "처음으로 이동";
            // 
            // bindingNavigatorMovePreviousItem
            // 
            this.bindingNavigatorMovePreviousItem.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.bindingNavigatorMovePreviousItem.Image = ((System.Drawing.Image)(resources.GetObject("bindingNavigatorMovePreviousItem.Image")));
            this.bindingNavigatorMovePreviousItem.Name = "bindingNavigatorMovePreviousItem";
            this.bindingNavigatorMovePreviousItem.RightToLeftAutoMirrorImage = true;
            this.bindingNavigatorMovePreviousItem.Size = new System.Drawing.Size(23, 22);
            this.bindingNavigatorMovePreviousItem.Text = "이전으로 이동";
            // 
            // bindingNavigatorMoveNextItem
            // 
            this.bindingNavigatorMoveNextItem.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.bindingNavigatorMoveNextItem.Image = ((System.Drawing.Image)(resources.GetObject("bindingNavigatorMoveNextItem.Image")));
            this.bindingNavigatorMoveNextItem.Name = "bindingNavigatorMoveNextItem";
            this.bindingNavigatorMoveNextItem.RightToLeftAutoMirrorImage = true;
            this.bindingNavigatorMoveNextItem.Size = new System.Drawing.Size(23, 22);
            this.bindingNavigatorMoveNextItem.Text = "다음으로 이동";
            // 
            // bindingNavigatorMoveLastItem
            // 
            this.bindingNavigatorMoveLastItem.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.bindingNavigatorMoveLastItem.Image = ((System.Drawing.Image)(resources.GetObject("bindingNavigatorMoveLastItem.Image")));
            this.bindingNavigatorMoveLastItem.Name = "bindingNavigatorMoveLastItem";
            this.bindingNavigatorMoveLastItem.RightToLeftAutoMirrorImage = true;
            this.bindingNavigatorMoveLastItem.Size = new System.Drawing.Size(23, 22);
            this.bindingNavigatorMoveLastItem.Text = "마지막으로 이동";
            // 
            // hI_MEASURE_RESULTBindingNavigatorSaveItem
            // 
            this.hI_MEASURE_RESULTBindingNavigatorSaveItem.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.hI_MEASURE_RESULTBindingNavigatorSaveItem.Image = ((System.Drawing.Image)(resources.GetObject("hI_MEASURE_RESULTBindingNavigatorSaveItem.Image")));
            this.hI_MEASURE_RESULTBindingNavigatorSaveItem.Name = "hI_MEASURE_RESULTBindingNavigatorSaveItem";
            this.hI_MEASURE_RESULTBindingNavigatorSaveItem.Size = new System.Drawing.Size(23, 22);
            this.hI_MEASURE_RESULTBindingNavigatorSaveItem.Text = "데이터 저장";
            this.hI_MEASURE_RESULTBindingNavigatorSaveItem.Click += new System.EventHandler(this.hI_MEASURE_RESULTBindingNavigatorSaveItem_Click);
            // 
            // TagHiForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.hI_MEASURE_RESULTDataGridView);
            this.Controls.Add(this.hI_MEASURE_RESULTBindingNavigator);
            this.Name = "TagHiForm";
            this.Text = "TagHiForm";
            this.Load += new System.EventHandler(this.TagHiForm_Load);
            ((System.ComponentModel.ISupportInitialize)(this.sEIMMDataSet)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.hI_MEASURE_RESULTBindingSource)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.hI_MEASURE_RESULTBindingNavigator)).EndInit();
            this.hI_MEASURE_RESULTBindingNavigator.ResumeLayout(false);
            this.hI_MEASURE_RESULTBindingNavigator.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.hI_MEASURE_RESULTDataGridView)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private SEIMMDataSet sEIMMDataSet;
        private System.Windows.Forms.BindingSource hI_MEASURE_RESULTBindingSource;
        private SEIMMDataSetTableAdapters.HI_MEASURE_RESULTTableAdapter hI_MEASURE_RESULTTableAdapter;
        private SEIMMDataSetTableAdapters.TableAdapterManager tableAdapterManager;
        private System.Windows.Forms.BindingNavigator hI_MEASURE_RESULTBindingNavigator;
        private System.Windows.Forms.ToolStripButton bindingNavigatorAddNewItem;
        private System.Windows.Forms.ToolStripLabel bindingNavigatorCountItem;
        private System.Windows.Forms.ToolStripButton bindingNavigatorDeleteItem;
        private System.Windows.Forms.ToolStripButton bindingNavigatorMoveFirstItem;
        private System.Windows.Forms.ToolStripButton bindingNavigatorMovePreviousItem;
        private System.Windows.Forms.ToolStripSeparator bindingNavigatorSeparator;
        private System.Windows.Forms.ToolStripTextBox bindingNavigatorPositionItem;
        private System.Windows.Forms.ToolStripSeparator bindingNavigatorSeparator1;
        private System.Windows.Forms.ToolStripButton bindingNavigatorMoveNextItem;
        private System.Windows.Forms.ToolStripButton bindingNavigatorMoveLastItem;
        private System.Windows.Forms.ToolStripSeparator bindingNavigatorSeparator2;
        private System.Windows.Forms.ToolStripButton hI_MEASURE_RESULTBindingNavigatorSaveItem;
        private System.Windows.Forms.DataGridView hI_MEASURE_RESULTDataGridView;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn1;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn2;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn3;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn4;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn5;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn6;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn7;
    }
}