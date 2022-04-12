using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI;

namespace CFW.Web
{
    public class UserPrincipal
    {
        public static UserInfo SetUserInfo(Page page)
        {
            UserInfo info = new UserInfo();

            if (HttpContext.Current.Session["LanguageType"] == null)
            {
                info.LanguageType = "";
                info.UserId = "";
                info.UserName = "";
                info.UserPlant = "";
                info.UserGrp = "";
                info.UserGrade = "";

                return info;
            }
            
            info.LanguageType = HttpContext.Current.Session["LanguageType"].ToString();
            info.UserId = HttpContext.Current.Session["UserId"].ToString();
            info.UserName = HttpContext.Current.Session["UserName"].ToString();
            info.UserPlant = HttpContext.Current.Session["UserPlant"].ToString();
            info.UserGrp = HttpContext.Current.Session["UserGrp"].ToString();
            info.UserGrade = HttpContext.Current.Session["UserGrade"].ToString();

            return info;
        }
    }
}
