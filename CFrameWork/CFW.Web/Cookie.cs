using System;
using System.Text; 

namespace CFW.Web
{
	/// <summary>
	/// Summary description for Cookie.
	/// </summary>
	public class Cookie
	{
		/// <summary>
		/// Cookie 생성자입니다.
		/// </summary>
		public Cookie()	{}
		
		#region 멤버변수

		private string m_LanguageType = string.Empty;
		private string m_UserName = string.Empty;
		private string m_UserId = string.Empty;
		private string m_UserDept = string.Empty;
		private string m_UserGup = string.Empty;

		#endregion 멤버변수

		#region Property

		/// <summary>
		/// 언어 Type
		/// </summary>
		public string LanguageType
		{
			get
			{
				return this.m_LanguageType;
			}
			set
			{
				if(value != null && value.Length > 0)
					this.m_LanguageType = value;
			}
		}

		/// <summary>
		/// 사용자 명
		/// </summary>
		public string UserName
		{
			get
			{
				return this.m_UserName;
			}
		}

		/// <summary>
		/// 사용자 ID
		/// </summary>
		public string UserId
		{
			get
			{
				return this.m_UserId;
			}
		}


		/// <summary>
		/// 부서 정보
		/// </summary>
		public string UserDept
		{
			get
			{
				return this.m_UserDept;
			}
		}

		/// <summary>
		/// User Group
		/// </summary>
		public string UserGup
		{
			get
			{
				return this.m_UserGup;
			}
		}

		#endregion Property

		#region 쿠키값을 읽어옵니다.

		#region == 주석 ==
//		public void GetCookies(System.Web.UI.Page page)
//		{
//			System.Web.HttpCookie oMESCookie = null;
//			oMESCookie = page.Request.Cookies["KRRI"];
//
//			if(oMESCookie == null)
//			{
//				this.m_LanguageType = CFW.Common.Configuration.ReadConfigValue(CFW.Common.SECTION.CULTURE,CFW.Common.KEY.CUL_DEFALT,CFW.Common.Configuration.ReadConfigValue("ConfigFilePath"));
//				this.m_UserId = "";
//				this.m_UserName = "";
//				//this.m_UserPwd = "";
//				this.m_UserPln = "";
//				this.m_UserDept = "";
//				this.m_UserGup = "";
//				this.m_UserWKTeam = "";
//				this.m_UserSysCD = "";
//			}
//			else
//			{// 한글이 들어가는 값은 모두 Decodestring 처리
//				this.m_LanguageType = DecodeString(oMESCookie["LanguageType"]);
//				this.m_UserId = DecodeString(oMESCookie["UserId"]);
//				this.m_UserName = DecodeString(oMESCookie["UserName"]);
//				//this.m_UserPwd = oMESCookie["UserPwd"];
//				this.m_UserPln = DecodeString(oMESCookie["UserPln"]);
//				this.m_UserDept = DecodeString(oMESCookie["UserDept"]);
//				this.m_UserGup = DecodeString(oMESCookie["UserGup"]);
//				this.m_UserWKTeam = DecodeString(oMESCookie["UserWKTeam"]);
//				this.m_UserSysCD = DecodeString(oMESCookie["UserSysCD"]);
//			}
//		}
		#endregion

		/// <summary>
		/// 쿠키값을 읽어옵니다.
		/// </summary>
		/// <param name="page">해당 페이지</param>
		public void GetCookies(System.Web.UI.Page page)
		{
			System.Web.HttpCookie oMESCookie = null;
			oMESCookie = page.Request.Cookies["KRRI"];

			if(oMESCookie == null)
			{
				this.m_LanguageType = CFW.Configuration.ConfigManager.Default.ReadConfig("Language","LanguageType");
				this.m_UserId = "";
				this.m_UserName = "";
				this.m_UserDept = "";
				this.m_UserGup = "";
			}
			else
			{// 한글이 들어가는 값은 모두 Decodestring 처리

				this.m_LanguageType = DecodeString(oMESCookie["LanguageType"]);
				this.m_UserId = DecodeString(oMESCookie["UserId"]);
				this.m_UserName = DecodeString(oMESCookie["UserName"]);
				this.m_UserDept = DecodeString(oMESCookie["UserDept"]);
				this.m_UserGup = DecodeString(oMESCookie["UserGup"]);
			}
		}

		#endregion

		#region 쿠키를 생성합니다.
		/// <summary>
		/// 쿠키를 생성합니다.
		/// </summary>
		/// <param name="keys">key값 Array</param>
		/// <param name="values">value 값 Array</param>
		/// <param name="page">System.Web.UI.Page</param>
		/// <example>
		/// this.oCookie.MakeCookies(arrCookieKey, arrCookieValue, this);
		/// </example>
		public void MakeCookies(string[] keys, string[] values, System.Web.UI.Page page)
		{
			int iKeyCount = keys.Length;

			System.Web.HttpCookie oMESCookie = new System.Web.HttpCookie("KRRI");
			for(int i=0; i<iKeyCount; i++)
			{
				//한글이 들어가는 것은 모두 Encodeing
				oMESCookie.Values.Add(keys[i], EncodeString(values[i]));
			}
			page.Response.Cookies.Set(oMESCookie);
		}

		#endregion

		#region == Decode/Encode
        		
		private string DecodeString(string text)
		{
			if(text !=null)
			{              
				if(text.Trim().Length>0)
				{
					char[] data=text.ToCharArray();
					Base64Decoder myDecoder=new Base64Decoder(data);
					StringBuilder sb=new StringBuilder();
					byte[] temp=myDecoder.GetDecoded();
					sb.Append(System.Text.UTF8Encoding.UTF8.GetChars(temp));
					return sb.ToString();
				}
				else return "";
			}
			else return "";
		}


		private string EncodeString(string text)
		{  
			if(text !=null)
			{
				if(text.Trim().Length>0)
				{
					byte[] data=System.Text.UnicodeEncoding.UTF8.GetBytes(text);
					Base64Encoder myEncoder=new Base64Encoder(data);
					StringBuilder sb=new StringBuilder();
					sb.Append(myEncoder.GetEncoded());
					return sb.ToString();
				}
				else return "";
			}
			else return "";
		} 
		#endregion
	}
}
