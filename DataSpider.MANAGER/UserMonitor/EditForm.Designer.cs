
namespace SEIMM.UserMonitor
{
    partial class EditForm
    {
        /// <summary>
        /// 필수 디자이너 변수입니다.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 사용 중인 모든 리소스를 정리합니다.
        /// </summary>
        /// <param name="disposing">관리되는 리소스를 삭제해야 하면 true이고, 그렇지 않으면 false입니다.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form 디자이너에서 생성한 코드

        /// <summary>
        /// 디자이너 지원에 필요한 메서드입니다. 
        /// 이 메서드의 내용을 코드 편집기로 수정하지 마세요.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.Windows.Forms.Label eQUIP_NMLabel;
            System.Windows.Forms.Label eQUIP_DESCLabel;
            System.Windows.Forms.Label eQUIP_TYPELabel;
            System.Windows.Forms.Label iF_TYPELabel;
            System.Windows.Forms.Label cONNECTION_INFOLabel;
            System.Windows.Forms.Label sTART_STRINGLabel;
            System.Windows.Forms.Label sERVER_NMLabel;
            System.Windows.Forms.Label uSE_FLAGLabel;
            System.Windows.Forms.Label pROG_DATETIMELabel;
            System.Windows.Forms.Label uPDATE_REG_DATELabel;
            System.Windows.Forms.Label uPDATE_REG_IDLabel;
            System.Windows.Forms.Label rEG_DATELabel;
            System.Windows.Forms.Label rEG_IDLabel;
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(EditForm));
            this.sEIMMDataSet = new SEIMM.SEIMMDataSet();
            this.mA_EQUIPMENT_CD_TestBindingSource = new System.Windows.Forms.BindingSource(this.components);
            this.mA_EQUIPMENT_CD_TestTableAdapter = new SEIMM.SEIMMDataSetTableAdapters.MA_EQUIPMENT_CD_TestTableAdapter();
            this.tableAdapterManager = new SEIMM.SEIMMDataSetTableAdapters.TableAdapterManager();
            this.mA_EQUIPMENT_CD_TestBindingNavigator = new System.Windows.Forms.BindingNavigator(this.components);
            this.bindingNavigatorAddNewItem = new System.Windows.Forms.ToolStripButton();
            this.bindingNavigatorCountItem = new System.Windows.Forms.ToolStripLabel();
            this.bindingNavigatorDeleteItem = new System.Windows.Forms.ToolStripButton();
            this.bindingNavigatorMoveFirstItem = new System.Windows.Forms.ToolStripButton();
            this.bindingNavigatorMovePreviousItem = new System.Windows.Forms.ToolStripButton();
            this.bindingNavigatorSeparator = new System.Windows.Forms.ToolStripSeparator();
            this.bindingNavigatorPositionItem = new System.Windows.Forms.ToolStripTextBox();
            this.bindingNavigatorSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.bindingNavigatorMoveNextItem = new System.Windows.Forms.ToolStripButton();
            this.bindingNavigatorMoveLastItem = new System.Windows.Forms.ToolStripButton();
            this.bindingNavigatorSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.mA_EQUIPMENT_CD_TestBindingNavigatorSaveItem = new System.Windows.Forms.ToolStripButton();
            this.eQUIP_NMTextBox = new System.Windows.Forms.TextBox();
            this.eQUIP_DESCTextBox = new System.Windows.Forms.TextBox();
            this.eQUIP_TYPETextBox = new System.Windows.Forms.TextBox();
            this.iF_TYPETextBox = new System.Windows.Forms.TextBox();
            this.cONNECTION_INFOTextBox = new System.Windows.Forms.TextBox();
            this.sTART_STRINGTextBox = new System.Windows.Forms.TextBox();
            this.sERVER_NMTextBox = new System.Windows.Forms.TextBox();
            this.uSE_FLAGTextBox = new System.Windows.Forms.TextBox();
            this.pROG_DATETIMEDateTimePicker = new System.Windows.Forms.DateTimePicker();
            this.uPDATE_REG_DATEDateTimePicker = new System.Windows.Forms.DateTimePicker();
            this.uPDATE_REG_IDTextBox = new System.Windows.Forms.TextBox();
            this.rEG_DATEDateTimePicker = new System.Windows.Forms.DateTimePicker();
            this.rEG_IDTextBox = new System.Windows.Forms.TextBox();
            eQUIP_NMLabel = new System.Windows.Forms.Label();
            eQUIP_DESCLabel = new System.Windows.Forms.Label();
            eQUIP_TYPELabel = new System.Windows.Forms.Label();
            iF_TYPELabel = new System.Windows.Forms.Label();
            cONNECTION_INFOLabel = new System.Windows.Forms.Label();
            sTART_STRINGLabel = new System.Windows.Forms.Label();
            sERVER_NMLabel = new System.Windows.Forms.Label();
            uSE_FLAGLabel = new System.Windows.Forms.Label();
            pROG_DATETIMELabel = new System.Windows.Forms.Label();
            uPDATE_REG_DATELabel = new System.Windows.Forms.Label();
            uPDATE_REG_IDLabel = new System.Windows.Forms.Label();
            rEG_DATELabel = new System.Windows.Forms.Label();
            rEG_IDLabel = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.sEIMMDataSet)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.mA_EQUIPMENT_CD_TestBindingSource)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.mA_EQUIPMENT_CD_TestBindingNavigator)).BeginInit();
            this.mA_EQUIPMENT_CD_TestBindingNavigator.SuspendLayout();
            this.SuspendLayout();
            // 
            // eQUIP_NMLabel
            // 
            eQUIP_NMLabel.AutoSize = true;
            eQUIP_NMLabel.Location = new System.Drawing.Point(83, 51);
            eQUIP_NMLabel.Name = "eQUIP_NMLabel";
            eQUIP_NMLabel.Size = new System.Drawing.Size(69, 12);
            eQUIP_NMLabel.TabIndex = 2;
            eQUIP_NMLabel.Text = "EQUIP NM:";
            // 
            // eQUIP_DESCLabel
            // 
            eQUIP_DESCLabel.AutoSize = true;
            eQUIP_DESCLabel.Location = new System.Drawing.Point(83, 78);
            eQUIP_DESCLabel.Name = "eQUIP_DESCLabel";
            eQUIP_DESCLabel.Size = new System.Drawing.Size(82, 12);
            eQUIP_DESCLabel.TabIndex = 4;
            eQUIP_DESCLabel.Text = "EQUIP DESC:";
            // 
            // eQUIP_TYPELabel
            // 
            eQUIP_TYPELabel.AutoSize = true;
            eQUIP_TYPELabel.Location = new System.Drawing.Point(83, 105);
            eQUIP_TYPELabel.Name = "eQUIP_TYPELabel";
            eQUIP_TYPELabel.Size = new System.Drawing.Size(81, 12);
            eQUIP_TYPELabel.TabIndex = 6;
            eQUIP_TYPELabel.Text = "EQUIP TYPE:";
            // 
            // iF_TYPELabel
            // 
            iF_TYPELabel.AutoSize = true;
            iF_TYPELabel.Location = new System.Drawing.Point(83, 132);
            iF_TYPELabel.Name = "iF_TYPELabel";
            iF_TYPELabel.Size = new System.Drawing.Size(55, 12);
            iF_TYPELabel.TabIndex = 8;
            iF_TYPELabel.Text = "IF TYPE:";
            // 
            // cONNECTION_INFOLabel
            // 
            cONNECTION_INFOLabel.AutoSize = true;
            cONNECTION_INFOLabel.Location = new System.Drawing.Point(83, 159);
            cONNECTION_INFOLabel.Name = "cONNECTION_INFOLabel";
            cONNECTION_INFOLabel.Size = new System.Drawing.Size(123, 12);
            cONNECTION_INFOLabel.TabIndex = 10;
            cONNECTION_INFOLabel.Text = "CONNECTION INFO:";
            // 
            // sTART_STRINGLabel
            // 
            sTART_STRINGLabel.AutoSize = true;
            sTART_STRINGLabel.Location = new System.Drawing.Point(83, 186);
            sTART_STRINGLabel.Name = "sTART_STRINGLabel";
            sTART_STRINGLabel.Size = new System.Drawing.Size(98, 12);
            sTART_STRINGLabel.TabIndex = 12;
            sTART_STRINGLabel.Text = "START STRING:";
            // 
            // sERVER_NMLabel
            // 
            sERVER_NMLabel.AutoSize = true;
            sERVER_NMLabel.Location = new System.Drawing.Point(83, 213);
            sERVER_NMLabel.Name = "sERVER_NMLabel";
            sERVER_NMLabel.Size = new System.Drawing.Size(81, 12);
            sERVER_NMLabel.TabIndex = 14;
            sERVER_NMLabel.Text = "SERVER NM:";
            // 
            // uSE_FLAGLabel
            // 
            uSE_FLAGLabel.AutoSize = true;
            uSE_FLAGLabel.Location = new System.Drawing.Point(83, 240);
            uSE_FLAGLabel.Name = "uSE_FLAGLabel";
            uSE_FLAGLabel.Size = new System.Drawing.Size(68, 12);
            uSE_FLAGLabel.TabIndex = 16;
            uSE_FLAGLabel.Text = "USE FLAG:";
            // 
            // pROG_DATETIMELabel
            // 
            pROG_DATETIMELabel.AutoSize = true;
            pROG_DATETIMELabel.Location = new System.Drawing.Point(83, 268);
            pROG_DATETIMELabel.Name = "pROG_DATETIMELabel";
            pROG_DATETIMELabel.Size = new System.Drawing.Size(109, 12);
            pROG_DATETIMELabel.TabIndex = 18;
            pROG_DATETIMELabel.Text = "PROG DATETIME:";
            // 
            // uPDATE_REG_DATELabel
            // 
            uPDATE_REG_DATELabel.AutoSize = true;
            uPDATE_REG_DATELabel.Location = new System.Drawing.Point(83, 295);
            uPDATE_REG_DATELabel.Name = "uPDATE_REG_DATELabel";
            uPDATE_REG_DATELabel.Size = new System.Drawing.Size(122, 12);
            uPDATE_REG_DATELabel.TabIndex = 20;
            uPDATE_REG_DATELabel.Text = "UPDATE REG DATE:";
            // 
            // uPDATE_REG_IDLabel
            // 
            uPDATE_REG_IDLabel.AutoSize = true;
            uPDATE_REG_IDLabel.Location = new System.Drawing.Point(83, 321);
            uPDATE_REG_IDLabel.Name = "uPDATE_REG_IDLabel";
            uPDATE_REG_IDLabel.Size = new System.Drawing.Size(101, 12);
            uPDATE_REG_IDLabel.TabIndex = 22;
            uPDATE_REG_IDLabel.Text = "UPDATE REG ID:";
            // 
            // rEG_DATELabel
            // 
            rEG_DATELabel.AutoSize = true;
            rEG_DATELabel.Location = new System.Drawing.Point(83, 349);
            rEG_DATELabel.Name = "rEG_DATELabel";
            rEG_DATELabel.Size = new System.Drawing.Size(70, 12);
            rEG_DATELabel.TabIndex = 24;
            rEG_DATELabel.Text = "REG DATE:";
            // 
            // rEG_IDLabel
            // 
            rEG_IDLabel.AutoSize = true;
            rEG_IDLabel.Location = new System.Drawing.Point(83, 375);
            rEG_IDLabel.Name = "rEG_IDLabel";
            rEG_IDLabel.Size = new System.Drawing.Size(49, 12);
            rEG_IDLabel.TabIndex = 26;
            rEG_IDLabel.Text = "REG ID:";
            // 
            // sEIMMDataSet
            // 
            this.sEIMMDataSet.DataSetName = "SEIMMDataSet";
            this.sEIMMDataSet.SchemaSerializationMode = System.Data.SchemaSerializationMode.IncludeSchema;
            // 
            // mA_EQUIPMENT_CD_TestBindingSource
            // 
            this.mA_EQUIPMENT_CD_TestBindingSource.DataMember = "MA_EQUIPMENT_CD_Test";
            this.mA_EQUIPMENT_CD_TestBindingSource.DataSource = this.sEIMMDataSet;
            // 
            // mA_EQUIPMENT_CD_TestTableAdapter
            // 
            this.mA_EQUIPMENT_CD_TestTableAdapter.ClearBeforeFill = true;
            // 
            // tableAdapterManager
            // 
            this.tableAdapterManager.BackupDataSetBeforeUpdate = false;
            this.tableAdapterManager.HI_MEASURE_RESULTTableAdapter = null;
                 this.tableAdapterManager.MA_COMMON_CDTableAdapter = null;
            this.tableAdapterManager.MA_EQUIPMENT_CD_TestTableAdapter = this.mA_EQUIPMENT_CD_TestTableAdapter;
            this.tableAdapterManager.MA_EQUIPMENT_CDTableAdapter = null;
             this.tableAdapterManager.MA_TAG_CDTableAdapter = null;
            this.tableAdapterManager.UpdateOrder = SEIMM.SEIMMDataSetTableAdapters.TableAdapterManager.UpdateOrderOption.InsertUpdateDelete;
            // 
            // mA_EQUIPMENT_CD_TestBindingNavigator
            // 
            this.mA_EQUIPMENT_CD_TestBindingNavigator.AddNewItem = this.bindingNavigatorAddNewItem;
            this.mA_EQUIPMENT_CD_TestBindingNavigator.BindingSource = this.mA_EQUIPMENT_CD_TestBindingSource;
            this.mA_EQUIPMENT_CD_TestBindingNavigator.CountItem = this.bindingNavigatorCountItem;
            this.mA_EQUIPMENT_CD_TestBindingNavigator.DeleteItem = this.bindingNavigatorDeleteItem;
            this.mA_EQUIPMENT_CD_TestBindingNavigator.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
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
            this.mA_EQUIPMENT_CD_TestBindingNavigatorSaveItem});
            this.mA_EQUIPMENT_CD_TestBindingNavigator.Location = new System.Drawing.Point(0, 0);
            this.mA_EQUIPMENT_CD_TestBindingNavigator.MoveFirstItem = this.bindingNavigatorMoveFirstItem;
            this.mA_EQUIPMENT_CD_TestBindingNavigator.MoveLastItem = this.bindingNavigatorMoveLastItem;
            this.mA_EQUIPMENT_CD_TestBindingNavigator.MoveNextItem = this.bindingNavigatorMoveNextItem;
            this.mA_EQUIPMENT_CD_TestBindingNavigator.MovePreviousItem = this.bindingNavigatorMovePreviousItem;
            this.mA_EQUIPMENT_CD_TestBindingNavigator.Name = "mA_EQUIPMENT_CD_TestBindingNavigator";
            this.mA_EQUIPMENT_CD_TestBindingNavigator.PositionItem = this.bindingNavigatorPositionItem;
            this.mA_EQUIPMENT_CD_TestBindingNavigator.Size = new System.Drawing.Size(852, 25);
            this.mA_EQUIPMENT_CD_TestBindingNavigator.TabIndex = 0;
            this.mA_EQUIPMENT_CD_TestBindingNavigator.Text = "bindingNavigator1";
            this.mA_EQUIPMENT_CD_TestBindingNavigator.RefreshItems += new System.EventHandler(this.mA_EQUIPMENT_CD_TestBindingNavigator_RefreshItems);
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
            // bindingNavigatorCountItem
            // 
            this.bindingNavigatorCountItem.Name = "bindingNavigatorCountItem";
            this.bindingNavigatorCountItem.Size = new System.Drawing.Size(27, 22);
            this.bindingNavigatorCountItem.Text = "/{0}";
            this.bindingNavigatorCountItem.ToolTipText = "전체 항목 수";
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
            // bindingNavigatorSeparator1
            // 
            this.bindingNavigatorSeparator1.Name = "bindingNavigatorSeparator1";
            this.bindingNavigatorSeparator1.Size = new System.Drawing.Size(6, 25);
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
            // bindingNavigatorSeparator2
            // 
            this.bindingNavigatorSeparator2.Name = "bindingNavigatorSeparator2";
            this.bindingNavigatorSeparator2.Size = new System.Drawing.Size(6, 25);
            // 
            // mA_EQUIPMENT_CD_TestBindingNavigatorSaveItem
            // 
            this.mA_EQUIPMENT_CD_TestBindingNavigatorSaveItem.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.mA_EQUIPMENT_CD_TestBindingNavigatorSaveItem.Image = ((System.Drawing.Image)(resources.GetObject("mA_EQUIPMENT_CD_TestBindingNavigatorSaveItem.Image")));
            this.mA_EQUIPMENT_CD_TestBindingNavigatorSaveItem.Name = "mA_EQUIPMENT_CD_TestBindingNavigatorSaveItem";
            this.mA_EQUIPMENT_CD_TestBindingNavigatorSaveItem.Size = new System.Drawing.Size(23, 22);
            this.mA_EQUIPMENT_CD_TestBindingNavigatorSaveItem.Text = "데이터 저장";
            this.mA_EQUIPMENT_CD_TestBindingNavigatorSaveItem.Click += new System.EventHandler(this.mA_EQUIPMENT_CD_TestBindingNavigatorSaveItem_Click);
            // 
            // eQUIP_NMTextBox
            // 
            this.eQUIP_NMTextBox.DataBindings.Add(new System.Windows.Forms.Binding("Text", this.mA_EQUIPMENT_CD_TestBindingSource, "EQUIP_NM", true));
            this.eQUIP_NMTextBox.Location = new System.Drawing.Point(212, 48);
            this.eQUIP_NMTextBox.Name = "eQUIP_NMTextBox";
            this.eQUIP_NMTextBox.Size = new System.Drawing.Size(200, 21);
            this.eQUIP_NMTextBox.TabIndex = 3;
            // 
            // eQUIP_DESCTextBox
            // 
            this.eQUIP_DESCTextBox.DataBindings.Add(new System.Windows.Forms.Binding("Text", this.mA_EQUIPMENT_CD_TestBindingSource, "EQUIP_DESC", true));
            this.eQUIP_DESCTextBox.Location = new System.Drawing.Point(212, 75);
            this.eQUIP_DESCTextBox.Name = "eQUIP_DESCTextBox";
            this.eQUIP_DESCTextBox.Size = new System.Drawing.Size(200, 21);
            this.eQUIP_DESCTextBox.TabIndex = 5;
            // 
            // eQUIP_TYPETextBox
            // 
            this.eQUIP_TYPETextBox.DataBindings.Add(new System.Windows.Forms.Binding("Text", this.mA_EQUIPMENT_CD_TestBindingSource, "EQUIP_TYPE", true));
            this.eQUIP_TYPETextBox.Location = new System.Drawing.Point(212, 102);
            this.eQUIP_TYPETextBox.Name = "eQUIP_TYPETextBox";
            this.eQUIP_TYPETextBox.Size = new System.Drawing.Size(200, 21);
            this.eQUIP_TYPETextBox.TabIndex = 7;
            // 
            // iF_TYPETextBox
            // 
            this.iF_TYPETextBox.DataBindings.Add(new System.Windows.Forms.Binding("Text", this.mA_EQUIPMENT_CD_TestBindingSource, "IF_TYPE", true));
            this.iF_TYPETextBox.Location = new System.Drawing.Point(212, 129);
            this.iF_TYPETextBox.Name = "iF_TYPETextBox";
            this.iF_TYPETextBox.Size = new System.Drawing.Size(200, 21);
            this.iF_TYPETextBox.TabIndex = 9;
            // 
            // cONNECTION_INFOTextBox
            // 
            this.cONNECTION_INFOTextBox.DataBindings.Add(new System.Windows.Forms.Binding("Text", this.mA_EQUIPMENT_CD_TestBindingSource, "CONNECTION_INFO", true));
            this.cONNECTION_INFOTextBox.Location = new System.Drawing.Point(212, 156);
            this.cONNECTION_INFOTextBox.Name = "cONNECTION_INFOTextBox";
            this.cONNECTION_INFOTextBox.Size = new System.Drawing.Size(200, 21);
            this.cONNECTION_INFOTextBox.TabIndex = 11;
            // 
            // sTART_STRINGTextBox
            // 
            this.sTART_STRINGTextBox.DataBindings.Add(new System.Windows.Forms.Binding("Text", this.mA_EQUIPMENT_CD_TestBindingSource, "START_STRING", true));
            this.sTART_STRINGTextBox.Location = new System.Drawing.Point(212, 183);
            this.sTART_STRINGTextBox.Name = "sTART_STRINGTextBox";
            this.sTART_STRINGTextBox.Size = new System.Drawing.Size(200, 21);
            this.sTART_STRINGTextBox.TabIndex = 13;
            // 
            // sERVER_NMTextBox
            // 
            this.sERVER_NMTextBox.DataBindings.Add(new System.Windows.Forms.Binding("Text", this.mA_EQUIPMENT_CD_TestBindingSource, "SERVER_NM", true));
            this.sERVER_NMTextBox.Location = new System.Drawing.Point(212, 210);
            this.sERVER_NMTextBox.Name = "sERVER_NMTextBox";
            this.sERVER_NMTextBox.Size = new System.Drawing.Size(200, 21);
            this.sERVER_NMTextBox.TabIndex = 15;
            // 
            // uSE_FLAGTextBox
            // 
            this.uSE_FLAGTextBox.DataBindings.Add(new System.Windows.Forms.Binding("Text", this.mA_EQUIPMENT_CD_TestBindingSource, "USE_FLAG", true));
            this.uSE_FLAGTextBox.Location = new System.Drawing.Point(212, 237);
            this.uSE_FLAGTextBox.Name = "uSE_FLAGTextBox";
            this.uSE_FLAGTextBox.Size = new System.Drawing.Size(200, 21);
            this.uSE_FLAGTextBox.TabIndex = 17;
            // 
            // pROG_DATETIMEDateTimePicker
            // 
            this.pROG_DATETIMEDateTimePicker.DataBindings.Add(new System.Windows.Forms.Binding("Value", this.mA_EQUIPMENT_CD_TestBindingSource, "PROG_DATETIME", true));
            this.pROG_DATETIMEDateTimePicker.Location = new System.Drawing.Point(212, 264);
            this.pROG_DATETIMEDateTimePicker.Name = "pROG_DATETIMEDateTimePicker";
            this.pROG_DATETIMEDateTimePicker.Size = new System.Drawing.Size(200, 21);
            this.pROG_DATETIMEDateTimePicker.TabIndex = 19;
            // 
            // uPDATE_REG_DATEDateTimePicker
            // 
            this.uPDATE_REG_DATEDateTimePicker.DataBindings.Add(new System.Windows.Forms.Binding("Value", this.mA_EQUIPMENT_CD_TestBindingSource, "UPDATE_REG_DATE", true));
            this.uPDATE_REG_DATEDateTimePicker.Location = new System.Drawing.Point(212, 291);
            this.uPDATE_REG_DATEDateTimePicker.Name = "uPDATE_REG_DATEDateTimePicker";
            this.uPDATE_REG_DATEDateTimePicker.Size = new System.Drawing.Size(200, 21);
            this.uPDATE_REG_DATEDateTimePicker.TabIndex = 21;
            // 
            // uPDATE_REG_IDTextBox
            // 
            this.uPDATE_REG_IDTextBox.DataBindings.Add(new System.Windows.Forms.Binding("Text", this.mA_EQUIPMENT_CD_TestBindingSource, "UPDATE_REG_ID", true));
            this.uPDATE_REG_IDTextBox.Location = new System.Drawing.Point(212, 318);
            this.uPDATE_REG_IDTextBox.Name = "uPDATE_REG_IDTextBox";
            this.uPDATE_REG_IDTextBox.Size = new System.Drawing.Size(200, 21);
            this.uPDATE_REG_IDTextBox.TabIndex = 23;
            // 
            // rEG_DATEDateTimePicker
            // 
            this.rEG_DATEDateTimePicker.DataBindings.Add(new System.Windows.Forms.Binding("Value", this.mA_EQUIPMENT_CD_TestBindingSource, "REG_DATE", true));
            this.rEG_DATEDateTimePicker.Location = new System.Drawing.Point(212, 345);
            this.rEG_DATEDateTimePicker.Name = "rEG_DATEDateTimePicker";
            this.rEG_DATEDateTimePicker.Size = new System.Drawing.Size(200, 21);
            this.rEG_DATEDateTimePicker.TabIndex = 25;
            // 
            // rEG_IDTextBox
            // 
            this.rEG_IDTextBox.DataBindings.Add(new System.Windows.Forms.Binding("Text", this.mA_EQUIPMENT_CD_TestBindingSource, "REG_ID", true));
            this.rEG_IDTextBox.Location = new System.Drawing.Point(212, 372);
            this.rEG_IDTextBox.Name = "rEG_IDTextBox";
            this.rEG_IDTextBox.Size = new System.Drawing.Size(200, 21);
            this.rEG_IDTextBox.TabIndex = 27;
            // 
            // EditForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 12F);
            this.ClientSize = new System.Drawing.Size(852, 458);
            this.Controls.Add(eQUIP_NMLabel);
            this.Controls.Add(this.eQUIP_NMTextBox);
            this.Controls.Add(eQUIP_DESCLabel);
            this.Controls.Add(this.eQUIP_DESCTextBox);
            this.Controls.Add(eQUIP_TYPELabel);
            this.Controls.Add(this.eQUIP_TYPETextBox);
            this.Controls.Add(iF_TYPELabel);
            this.Controls.Add(this.iF_TYPETextBox);
            this.Controls.Add(cONNECTION_INFOLabel);
            this.Controls.Add(this.cONNECTION_INFOTextBox);
            this.Controls.Add(sTART_STRINGLabel);
            this.Controls.Add(this.sTART_STRINGTextBox);
            this.Controls.Add(sERVER_NMLabel);
            this.Controls.Add(this.sERVER_NMTextBox);
            this.Controls.Add(uSE_FLAGLabel);
            this.Controls.Add(this.uSE_FLAGTextBox);
            this.Controls.Add(pROG_DATETIMELabel);
            this.Controls.Add(this.pROG_DATETIMEDateTimePicker);
            this.Controls.Add(uPDATE_REG_DATELabel);
            this.Controls.Add(this.uPDATE_REG_DATEDateTimePicker);
            this.Controls.Add(uPDATE_REG_IDLabel);
            this.Controls.Add(this.uPDATE_REG_IDTextBox);
            this.Controls.Add(rEG_DATELabel);
            this.Controls.Add(this.rEG_DATEDateTimePicker);
            this.Controls.Add(rEG_IDLabel);
            this.Controls.Add(this.rEG_IDTextBox);
            this.Controls.Add(this.mA_EQUIPMENT_CD_TestBindingNavigator);
            this.Name = "EditForm";
            this.Load += new System.EventHandler(this.EditForm_Load);
            ((System.ComponentModel.ISupportInitialize)(this.sEIMMDataSet)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.mA_EQUIPMENT_CD_TestBindingSource)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.mA_EQUIPMENT_CD_TestBindingNavigator)).EndInit();
            this.mA_EQUIPMENT_CD_TestBindingNavigator.ResumeLayout(false);
            this.mA_EQUIPMENT_CD_TestBindingNavigator.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private SEIMMDataSet sEIMMDataSet;
        private System.Windows.Forms.BindingSource mA_EQUIPMENT_CD_TestBindingSource;
        private SEIMMDataSetTableAdapters.MA_EQUIPMENT_CD_TestTableAdapter mA_EQUIPMENT_CD_TestTableAdapter;
        private SEIMMDataSetTableAdapters.TableAdapterManager tableAdapterManager;
        private System.Windows.Forms.BindingNavigator mA_EQUIPMENT_CD_TestBindingNavigator;
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
        private System.Windows.Forms.ToolStripButton mA_EQUIPMENT_CD_TestBindingNavigatorSaveItem;
        private System.Windows.Forms.TextBox eQUIP_NMTextBox;
        private System.Windows.Forms.TextBox eQUIP_DESCTextBox;
        private System.Windows.Forms.TextBox eQUIP_TYPETextBox;
        private System.Windows.Forms.TextBox iF_TYPETextBox;
        private System.Windows.Forms.TextBox cONNECTION_INFOTextBox;
        private System.Windows.Forms.TextBox sTART_STRINGTextBox;
        private System.Windows.Forms.TextBox sERVER_NMTextBox;
        private System.Windows.Forms.TextBox uSE_FLAGTextBox;
        private System.Windows.Forms.DateTimePicker pROG_DATETIMEDateTimePicker;
        private System.Windows.Forms.DateTimePicker uPDATE_REG_DATEDateTimePicker;
        private System.Windows.Forms.TextBox uPDATE_REG_IDTextBox;
        private System.Windows.Forms.DateTimePicker rEG_DATEDateTimePicker;
        private System.Windows.Forms.TextBox rEG_IDTextBox;
    }
}
