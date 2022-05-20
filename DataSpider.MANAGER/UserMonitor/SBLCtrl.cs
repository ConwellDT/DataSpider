using LibraryWH.ControlElement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataSpider.UserMonitor
{

    class TreeNode_SBL : TreeNodeEx
    {
        public SBL Obj { get; set; }

        public TreeNode_SBL(SBL pObj) : base()
        {
            Obj = pObj;

            Name = Obj.Name;

            this.Tag = Obj;
            if (pObj is EqType)
            {
                this.Text = $"{Obj.Name} ({Obj.Description}) [0/{(Obj as EqType).EqLists.Count}]";
            }
            else
            {
                this.Text = $"{Obj.Name} [{Obj.State}]";
            }

            Obj.OnChangeStateEvent += new SBL.OnChangeStateHandler(OnChangeStateFunc);
            ChangeImgKey(Obj);
        }
        private void OnChangeStateFunc(SBL pObj, EventArgs args)
        {
            ChangeImgKey(pObj);
        }

        protected void ChangeImgKey(SBL pObj)
        {
            //StringBuilder imgKeyStr = new StringBuilder();
            //imgKeyStr.Clear();
            //imgKeyStr.Append($"{pObj.Image}_{ pObj.State.ToString()}");
            //this.ImageKey = imgKeyStr.ToString();
            //this.SelectedImageKey = imgKeyStr.ToString();

            this.ImageKey = SelectedImageKey = pObj.State.ToString();
        }
    }

    class ListItem_SBL : ListViewItemEx
    {
        public SBL Obj { get; set; }

        public ListItem_SBL(SBL pObj, string[] items) : base(items)
        {
            Obj = pObj;

            this.Tag = Obj;
            this.Text = Obj.Name;

            Obj.OnChangeStateEvent += new SBL.OnChangeStateHandler(OnChangeStateFunc);
            ChangeImgKey(Obj);
        }
        private void OnChangeStateFunc(SBL pObj, EventArgs args)
        {
            ChangeImgKey(pObj);
        }

        protected void ChangeImgKey(SBL pObj)
        {

            StringBuilder imgKeyStr = new StringBuilder();
            imgKeyStr.Clear();
            imgKeyStr.Append($"{ pObj.Image}_{ pObj.State.ToString()}");
            this.ImageKey = imgKeyStr.ToString();

        }
    }

}
