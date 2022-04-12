using LibraryWH.FormCtrl;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataSpider.UserMonitor
{
    class SysUser
    {
        List<AuthofForm> m_pFormList = new List<AuthofForm>();
        public List<AuthofForm>  권한들
        {
            get { return m_pFormList; }
        }
    }

    public class AuthofForm
    {
        public string FormName { get; set; }
        public bool Read { get; set; }
        public bool Write { get; set; }
    }

}
