using System;
using System.Text;
using System.Web;
using System.Web.UI;

namespace CFW.Web
{
	/// <summary>
	/// Summary description for ExcelExport.
	/// </summary>
	public class ExcelExport
	{
		/// <summary>
		/// ExcelExport 생성자입니다.
		/// </summary>
		public ExcelExport()	{}
		
		#region 멤버변수

		private HttpResponse httpResponse = null;
		private string m_UserName = string.Empty;
		private string m_UserId = string.Empty;
		private string m_UserPln = string.Empty;
		private string m_UserDept = string.Empty;
		private string m_UserGup = string.Empty;
		private string m_UserWKTeam = string.Empty;
		private string m_UserSysCD = string.Empty;
		private string m_UserDbCon = string.Empty;

		#endregion 멤버변수

		#region Excel Export
		public void BeginExcelExport(HttpResponse Response, System.Web.UI.WebControls.GridView gridView, string fileName)
		{
			Response.Clear();

			Response.AddHeader("content-disposition", "attachment;filename="+fileName);

			Response.Charset = "";

			Response.ContentType = "application/vnd.xls";

			System.IO.StringWriter stringWrite = new System.IO.StringWriter();

			System.Web.UI.HtmlTextWriter htmlWrite = new HtmlTextWriter(stringWrite);

			gridView.RenderControl(htmlWrite);

			Response.Write(stringWrite.ToString());

			Response.End();

		}

		#endregion Excel Export

	}
}
