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
		/// Cookie �������Դϴ�.
		/// </summary>
		public Cookie()	{}
		
		#region �������

		private string m_LanguageType = string.Empty;
		private string m_UserName = string.Empty;
		private string m_UserId = string.Empty;
		private string m_UserDept = string.Empty;
		private string m_UserGup = string.Empty;

		#endregion �������

		#region Property

		/// <summary>
		/// ��� Type
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
		/// ����� ��
		/// </summary>
		public string UserName
		{
			get
			{
				return this.m_UserName;
			}
		}

		/// <summary>
		/// ����� ID
		/// </summary>
		public string UserId
		{
			get
			{
				return this.m_UserId;
			}
		}


		/// <summary>
		/// �μ� ����
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

		#region ��Ű���� �о�ɴϴ�.

		#region == �ּ� ==
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
//			{// �ѱ��� ���� ���� ��� Decodestring ó��
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
		/// ��Ű���� �о�ɴϴ�.
		/// </summary>
		/// <param name="page">�ش� ������</param>
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
			{// �ѱ��� ���� ���� ��� Decodestring ó��

				this.m_LanguageType = DecodeString(oMESCookie["LanguageType"]);
				this.m_UserId = DecodeString(oMESCookie["UserId"]);
				this.m_UserName = DecodeString(oMESCookie["UserName"]);
				this.m_UserDept = DecodeString(oMESCookie["UserDept"]);
				this.m_UserGup = DecodeString(oMESCookie["UserGup"]);
			}
		}

		#endregion

		#region ��Ű�� �����մϴ�.
		/// <summary>
		/// ��Ű�� �����մϴ�.
		/// </summary>
		/// <param name="keys">key�� Array</param>
		/// <param name="values">value �� Array</param>
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
				//�ѱ��� ���� ���� ��� Encodeing
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
