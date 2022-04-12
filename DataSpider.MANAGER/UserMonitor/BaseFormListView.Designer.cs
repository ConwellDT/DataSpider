
namespace SEIMM.UserMonitor
{
    partial class BaseFormListView
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(BaseFormListView));
            System.Windows.Forms.ListViewItem listViewItem1 = new System.Windows.Forms.ListViewItem("");
            this.imageList2 = new System.Windows.Forms.ImageList(this.components);
            this.imageList1 = new System.Windows.Forms.ImageList(this.components);
            this.listView_Data = new System.Windows.Forms.ListView();
            this.SuspendLayout();
            // 
            // imageList2
            // 
            this.imageList2.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList2.ImageStream")));
            this.imageList2.TransparentColor = System.Drawing.Color.Transparent;
            this.imageList2.Images.SetKeyName(0, "analyzer.png");
            this.imageList2.Images.SetKeyName(1, "applications-science-2.png");
            this.imageList2.Images.SetKeyName(2, "bioinformatics.png");
            this.imageList2.Images.SetKeyName(3, "cnc_machine.ico");
            this.imageList2.Images.SetKeyName(4, "cnc_machine.png");
            this.imageList2.Images.SetKeyName(5, "free-icon-ph-meter-1327507.png");
            this.imageList2.Images.SetKeyName(6, "free-icon-ph-meter-Stop.png");
            this.imageList2.Images.SetKeyName(7, "laboratory - 복사본.png");
            this.imageList2.Images.SetKeyName(8, "laboratory.png");
            this.imageList2.Images.SetKeyName(9, "weight-scale -Start.png");
            this.imageList2.Images.SetKeyName(10, "weight-scale.png");
            this.imageList2.Images.SetKeyName(11, "weight-scale_1.png");
            this.imageList2.Images.SetKeyName(12, "weight-scale_stop.png");
            // 
            // imageList1
            // 
            this.imageList1.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList1.ImageStream")));
            this.imageList1.TransparentColor = System.Drawing.Color.Transparent;
            this.imageList1.Images.SetKeyName(0, "analyzer.png");
            this.imageList1.Images.SetKeyName(1, "applications-science-2.png");
            this.imageList1.Images.SetKeyName(2, "bioinformatics.png");
            this.imageList1.Images.SetKeyName(3, "cnc_machine.ico");
            this.imageList1.Images.SetKeyName(4, "cnc_machine.png");
            this.imageList1.Images.SetKeyName(5, "free-icon-ph-meter-1327507.png");
            this.imageList1.Images.SetKeyName(6, "free-icon-ph-meter-Stop.png");
            this.imageList1.Images.SetKeyName(7, "laboratory - 복사본.png");
            this.imageList1.Images.SetKeyName(8, "laboratory.png");
            this.imageList1.Images.SetKeyName(9, "weight-scale -Start.png");
            this.imageList1.Images.SetKeyName(10, "weight-scale.png");
            this.imageList1.Images.SetKeyName(11, "weight-scale_1.png");
            this.imageList1.Images.SetKeyName(12, "weight-scale_stop.png");
            // 
            // listView_Data
            // 
            this.listView_Data.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listView_Data.GridLines = true;
            this.listView_Data.HideSelection = false;
            this.listView_Data.Items.AddRange(new System.Windows.Forms.ListViewItem[] {
            listViewItem1});
            this.listView_Data.Location = new System.Drawing.Point(0, 0);
            this.listView_Data.Name = "listView_Data";
            this.listView_Data.Size = new System.Drawing.Size(902, 589);
            this.listView_Data.TabIndex = 0;
            this.listView_Data.UseCompatibleStateImageBehavior = false;
            this.listView_Data.View = System.Windows.Forms.View.Details;
            // 
            // BaseFormListView
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 12F);
            this.ClientSize = new System.Drawing.Size(902, 589);
            this.Controls.Add(this.listView_Data);
            this.Name = "BaseFormListView";
            this.Text = "BaseFormListView";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.BaseFormListView_FormClosed);
            this.Load += new System.EventHandler(this.BaseFormListView_Load);
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.ImageList imageList2;
        private System.Windows.Forms.ImageList imageList1;
        private System.Windows.Forms.ListView listView_Data;
    }
}
