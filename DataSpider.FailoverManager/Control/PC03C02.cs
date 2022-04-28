using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace DataSpider.PC03.PT.Controls
{
    public partial class CWListView : CWUserControl
    {
        private ImageList image = new ImageList();
        private TListView tListView = new TListView();

        public CWListView()
        {
            InitializeComponent();

            this.Controls.Add(tListView);
            DefaultSetting();
        }

        private void DefaultSetting()
        {
            ImageList imgList = new ImageList();
            imgList.ImageSize = new Size(1, 25);

            this.tListView.Dock = DockStyle.Fill;
            this.tListView.Font = new Font("Arial", 9.75F, FontStyle.Bold);
            this.tListView.SmallImageList = imgList;
            this.tListView.View = View.Details;
            this.tListView.HeaderStyle = ColumnHeaderStyle.Nonclickable;
            this.tListView.HideSelection = false;
            this.tListView.MultiSelect = false;
            this.tListView.GridLines = true;
            this.tListView.FullRowSelect = false;
            this.tListView.Activation = ItemActivation.OneClick;
        }

        public void SetRowSize(ImageList image)
        {
            this.tListView.SmallImageList = image;
        }

        public void SetFont(Font f)
        {
            this.tListView.Font = f;
        }

        public bool Scrollable
        {
            set { this.tListView.Scrollable = value; }
        }

        public void BeginUpdate()
        {
            this.tListView.BeginUpdate();
        }

        public void EndUpdate()
        {
            this.tListView.EndUpdate();
        }

        public ListView ListView
        {
            get { return this.tListView; }
        }
    }

    

}
