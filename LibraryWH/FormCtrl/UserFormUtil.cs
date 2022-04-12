using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibraryWH.FormCtrl
{

    public class UserFormInfo
    {
        string _form;
        string _desc;
        string _name;
        public UserFormInfo(string form, string desc)
        {
            _form = form;
            _desc = desc;
        }

        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }

        public string FormName
        {
            get { return _form; }
            set { _form = value; }
        }

        public string Description
        {
            get { return _desc; }
            set { _desc = value; }
        }
    }
}
