using System;
using System.Globalization;
using System.Data;
using System.Web.UI;

namespace CFW.Web
{
	/// <summary>
	/// PageBase에 대한 설명입니다.
	/// </summary>
	public class PageBase : Page
	{
		#region == 변수 선언 ==
		private string m_FormID = string.Empty;
		private string m_UserRight = string.Empty;
        private UserInfo _UserInfo;
		/// <summary>에러메세지</summary>
		private string m_ErrorMessage = string.Empty;
		/// <summary>정보메세지</summary>
		private string m_InformationMessage = string.Empty;
		/// <summary>팝업창 닫기</summary>
		private bool m_WindowClose = false;
		/// <summary>팝업창 리턴값</summary>
		private string m_ReturnValue = string.Empty;
		/// <summary>FormLoad() 이전에 수행되어야 할 ClientScript</summary>
		private string m_BeforeFormLoadScript = string.Empty;
		/// <summary>FormLoad() 이후에 수행되어야 할 ClientScript</summary>
		private string m_AfterFormLoadScript = string.Empty;

		private string ERROR_TRACE_MODE =  string.Empty;
		private string EVENT_LOG =  string.Empty;
		private string EVENT_LOG_SOURCE  =  string.Empty;
		#endregion

		#region == 프로퍼티 ==
		/// <summary>
		/// Cookie
		/// </summary>
		public CFW.Web.Cookie oCookie = null;
		private static bool m_Popup = false;

		/// <summary>
		/// PageBase생성자 입니다.
		/// </summary>
		public PageBase() {
		}

		/// <summary>
		/// FormID를 설정하거나  가져올 수 있습니다.
		/// </summary>
		public string FormID
		{
			get { return m_FormID; }
			set { m_FormID = value; }
		}
		/// <summary>
		/// UserRight를 설정하거나  가져올 수 있습니다.
		/// </summary>
		public string UserRight
		{
			get { return m_UserRight; }
			set { m_UserRight = value; }
		}
		/// <summary>
		/// 언어 Type을 가져올 수 있습니다.
		/// </summary>
		public string LanguageType
		{
		    get { return this.UserInfo.LanguageType; }
		}

        /// <summary>
        /// UserInfo 설정
        /// </summary>
        public UserInfo UserInfo
        {
            get
            {
                return this._UserInfo;
            }
            set
            {
                this._UserInfo = value;
            }
        }
		#endregion

		#region == OnInit 오버라이드 ==
		/// <summary>
		/// OnInit 오버라이드
		/// </summary>
		/// <param name="e"></param>
		protected override void OnInit(EventArgs e)
		{
            this.UserInfo = UserPrincipal.SetUserInfo(this);
            SetPageHiddenField();

            this.DisplayLanguage(this);

            //국가별 날자 포맷 설정
            #region == CurrentCulture ==

            if (this.UserInfo.LanguageType.ToUpper().Equals("LO-LN"))
            {
                System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("EN-US");
                System.Threading.Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo("EN-US");
            }
            else
            {
                System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo(this.UserInfo.LanguageType);
                System.Threading.Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo(this.UserInfo.LanguageType);
            }

            string strShortPattern = System.Threading.Thread.CurrentThread.CurrentCulture.DateTimeFormat.ShortDatePattern.Trim();
            int iCount = strShortPattern.Replace(" ", "").Length;

            if (iCount < 10)
            {
                string strSeparator = "-";

                if (strShortPattern.IndexOf("/", 0) > 0)
                {
                    strShortPattern = strShortPattern.Replace("/", "-");
                    strSeparator = "/";
                }

                if (strShortPattern.IndexOf(".", 0) > 0)
                {
                    strShortPattern = strShortPattern.Replace(".", "-");
                    strSeparator = ".";
                }

                string[] arrTemp = strShortPattern.Split('-');

                if (arrTemp[0].Length < 2)
                {
                    arrTemp[0] = arrTemp[0].Trim().ToString() + arrTemp[0].Trim().ToString();
                }

                if (arrTemp[1].Trim().Length < 2)
                {
                    arrTemp[1] = arrTemp[1].Trim().ToString() + arrTemp[1].Trim().ToString();
                }

                if (arrTemp[2].Length < 2)
                {
                    arrTemp[2] = arrTemp[2].Trim().ToString() + arrTemp[2].Trim().ToString();
                }

                strShortPattern = arrTemp[0].Trim().ToString() + strSeparator + arrTemp[1].Trim().ToString() + strSeparator + arrTemp[2].Trim().ToString();
            }

            System.Threading.Thread.CurrentThread.CurrentCulture.DateTimeFormat.ShortDatePattern = strShortPattern;

            #endregion == CurrentCulture ==

            base.OnInit(e);			
		}
		#endregion

		#region == OnPreRender 오버라이드 ==
		/// <summary>
		/// OnPreRender 오버라이드
		/// </summary>
		/// <param name="e"></param>
		protected override void OnPreRender(EventArgs e)
		{
            //SetPageHiddenField(); //공통스크립트와 HiddenField설정
            m_Popup = false;
            base.OnPreRender(e);
		}
		#endregion

		#region == SetPageHiddenField ==
		/// <summary>
		/// SetPageHiddenField
		/// </summary>
		private void SetPageHiddenField()
		{
            string AppCommonPath = System.Configuration.ConfigurationSettings.AppSettings["WebCommonPath"];
            string AppLanguageType = System.Configuration.ConfigurationSettings.AppSettings["LanguageType"].ToString().ToUpper();
            string RepLanguageType = AppLanguageType.Replace("-", "_").ToString().ToUpper();

            if (this.UserInfo.LanguageType.Length > 0)
            {
                RepLanguageType = this.UserInfo.LanguageType.Replace("-", "_").ToString().ToUpper();
            }

            base.ClientScript.RegisterHiddenField("WebCommonPath", AppCommonPath);
            base.ClientScript.RegisterHiddenField("LanguageType", AppLanguageType);
            base.ClientScript.RegisterHiddenField("DeLanguageType", RepLanguageType);
            base.ClientScript.RegisterHiddenField("MessageCodeRegEx", @"[a-zA-Z0-9]$");
            base.ClientScript.RegisterStartupScript(base.GetType(), "", "<script language='javascript'  type='text/javascript'>function window.onload(){ fn_WindowOnLoad() }</script>");
            base.ClientScript.RegisterClientScriptBlock(base.GetType(), "", "<script language='javascript' type='text/javascript' src='" + AppCommonPath + "Scripts/Commons.js'></script>");
		}
		#endregion

		#region == 폼 로드(공통 FormLoad() 함수) Script가 실행되기 전/후에 실행할 스크립트 등록==
		/// <summary>
		/// Client 단에서 폼 로드(공통 FormLoad() 함수) Script가 실행되기 전에 실행할 스크립트 등록
		/// </summary>
		/// <example>
		/// <code>
		/// this.RegisterBeforeFormLoadClientScriptBlock("alert('hi KIA');");
		/// </code>
		/// </example>
		/// <param name="beforeFormLoadScript">함수안의 Client Script 로직만 String 형식으로 입력</param>
		protected void RegisterBeforeFormLoadClientScriptBlock(string beforeFormLoadScript)
		{
			this.m_BeforeFormLoadScript = beforeFormLoadScript;
		}
		

		/// <summary>
		/// Client 단에서 폼 로드(공통 FormLoad() 함수) Script 실행 후에 실행할 스크립트 등록
		/// </summary>
		/// <example>
		/// <code>
		///  this.RegisterAfterFormLoadClientScriptBlock("alert('hi KIA');");
		///  </code>
		/// </example>
		/// <param name="afterFormLoadScript">함수안의 Client Script 로직만 String 형식으로 입력</param>
		protected void RegisterAfterFormLoadClientScriptBlock(string afterFormLoadScript)
		{
			this.m_AfterFormLoadScript = afterFormLoadScript;
		}
		#endregion

		#region == Inform ==
		private void Inform(string msg)
		{
			string strMessage = null;
			strMessage = CFW.Common.Messaging.SearchMsg(msg);
			if (strMessage == string.Empty) strMessage = msg;

			this.m_InformationMessage = strMessage;
		}

		private void Inform(string msg, params string[] args)
		{
			string strMessage = null;
			strMessage = CFW.Common.Messaging.SearchMsg(msg, args);

			this.m_InformationMessage = strMessage;
		}
		#endregion

		#region == 메시지를 출력 ==

        /// <summary>
        /// 메시지를 출력합니다.
        /// </summary>
        /// <param name="msgID">출력할 메시지 아이디</param>
        /// <returns>출력 메시지</returns>
		protected string DisplayMessage(string msgID)
		{
			string strReturn = "";

			strReturn = DisplayMessage(msgID, null);

			return strReturn;
		}

		/// <summary>
		/// 메시지를 출력합니다.
		/// </summary>
		/// <param name="msgID">출력할 메시지 아이디</param>
		/// <param name="args">추가 메시지</param>
		/// <returns>출력 메시지</returns>
		protected string DisplayMessage(string msgID, string[] args)
		{
			string strReturn = msgID;
			CFW.Common.clsMsgInfo cMsg = null;
			
			cMsg = CFW.Common.Messaging.SearchMsgStrWEB(msgID, this.UserInfo.LanguageType);
			if(cMsg == null)
			{
				cMsg = new CFW.Common.clsMsgInfo();
				cMsg.MsgLocal = msgID;
				cMsg.MsgType = CFW.Common.MSG.LEVEL_INFO;
			}
			if(args != null)
			{
				if(cMsg.MsgLocal.Trim().Length != 0)
				{
					cMsg.MsgLocal = string.Format(cMsg.MsgLocal, args);
				}
				else
				{
					if(cMsg.MsgText.Trim().Length != 0)
					{
						cMsg.MsgLocal = string.Format(cMsg.MsgText, args);
					}
					else
					{
						cMsg.MsgLocal = msgID;
					}
				}	
			}
			switch(cMsg.MsgType)
			{
				case CFW.Common.MSG.LEVEL_INFO  :
					if(!m_Popup)//팝업페이지가 아니면
					{
						//Bottom Frame에 메시지 출력
						this.m_InformationMessage = cMsg.MsgLocal;
					}
					break;
				case CFW.Common.MSG.LEVEL_ERROR :
					this.m_ErrorMessage = cMsg.MsgLocal;
					break;
				default :
					if(!m_Popup)
					{
						this.m_InformationMessage = cMsg.MsgLocal;
					}
					break;
			}
			if( cMsg.MsgLocal.Length > 0)
				strReturn = cMsg.MsgLocal;			

		return strReturn;
		}

		/// <summary>
		/// 메시지를 출력합니다.
		/// 메시지 타입이 없는 경우 DB정보로 메시지 타입이 결정됩니다.
		/// </summary>
		/// <param name="msgID">출력할 메시지 아이디</param>
		/// <param name="args">추가 메시지</param>
		/// <param name="msgType">메시지타입</param>
		/// <returns>출력 메시지</returns>
		protected string DisplayMessage(string msgID, string[] args, string msgType)
		{
			//19일 이후 삭제 (msgType 사용하지 않음)
			string strReturn = msgID;
			CFW.Common.clsMsgInfo cMsg = null;
			cMsg = CFW.Common.Messaging.SearchMsgStrWEB(msgID, this.UserInfo.LanguageType);	

			if(cMsg == null)
			{
				cMsg = new CFW.Common.clsMsgInfo();
				cMsg.MsgType = msgType;
				cMsg.MsgLocal = msgID;
			}
			if(args != null)
			{
				if(cMsg.MsgLocal.Trim().Length != 0)
				{
					cMsg.MsgLocal = string.Format(cMsg.MsgLocal, args);
				}
				else
				{
					if(cMsg.MsgText.Trim().Length != 0)
					{
						cMsg.MsgLocal = string.Format(cMsg.MsgText, args);
					}
					else
					{
						cMsg.MsgLocal = msgID;
					}
				}	
			}
				//DB메시 타입 우선 (DB에 메시지 타입이 없을 때 사용자가 입력한 메시지 타입 사용)
			if(	cMsg.MsgType.Trim().Length == 0)
			{
				cMsg.MsgType = msgType;
			}
			switch(cMsg.MsgType)
			{
				case CFW.Common.MSG.LEVEL_INFO  :
					if(!m_Popup)
					{
						this.m_InformationMessage = cMsg.MsgLocal;
					}
					break;
				case CFW.Common.MSG.LEVEL_ERROR :
					this.m_ErrorMessage = cMsg.MsgLocal;
					break;
				default :
					if(!m_Popup)
					{
						this.m_InformationMessage = cMsg.MsgLocal;
					}
					break;
			}	
			if( cMsg.MsgLocal.Length > 0)
				strReturn = cMsg.MsgLocal;

			return strReturn;
		}
		#endregion

		#region == 에러메시지 처리 및 화면출력 ==
		/// <summary>
		/// 에러 메시지를 처리하고 화면에 출력합니다.
		/// Web.config의 에러트레이스모드가 on일 경우에만 이벤트로그를 남깁니다.
		/// </summary>
		/// <param name="ex">Exception 정보</param>
		protected void DisplayError(Exception ex)
		{
			ERROR_TRACE_MODE = System.Configuration.ConfigurationSettings.AppSettings["ErrorTraceMode"].Trim();
			EVENT_LOG = System.Configuration.ConfigurationSettings.AppSettings["EventLog"].Trim();
			EVENT_LOG_SOURCE = System.Configuration.ConfigurationSettings.AppSettings["EventLogSource"].Trim();

			CFW.Common.clsErrorInfo cErr = null;

			cErr = CFW.Common.Messaging.GetErrorMessage(ex);
			
			this.m_ErrorMessage = string.Concat(cErr.Msg , "|^|", cErr.ErrorDetail);

			//Web.config의 에러트레이스모드가 on일 경우에만 이벤트로그를 남긴다.
			if (ERROR_TRACE_MODE.Equals("on")) 
				CFW.Common.Logging.WriteEventLog(EVENT_LOG, EVENT_LOG_SOURCE, System.Diagnostics.EventLogEntryType.Error
					, cErr.ErrorDetail);
		}
		
		/// <summary>
		/// 에러 메시지를 처리하고 화면에 출력합니다.
		/// </summary>
		/// <param name="msgID">메시지 아이디</param>
		/// <param name="ex">Exception 정보</param>
		protected void DisplayError(string msgID, Exception ex)
		{
			DisplayError(msgID, ex, null);
		}

		/// <summary>
		/// 에러 메시지를 처리하고 화면에 출력합니다.
		/// Web.config의 에러트레이스모드가 on일 경우에만 이벤트로그를 남깁니다.
		/// </summary>
		/// <param name="msgID">메시지 아이디</param>
		/// <param name="ex">Exception 정보</param>
		/// <param name="args">추가 파라미터</param>
		protected void DisplayError(string msgID, Exception ex, params string[] args)
		{
			ERROR_TRACE_MODE = System.Configuration.ConfigurationSettings.AppSettings["ErrorTraceMode"].Trim();
			EVENT_LOG = System.Configuration.ConfigurationSettings.AppSettings["EventLog"].Trim();
			EVENT_LOG_SOURCE = System.Configuration.ConfigurationSettings.AppSettings["EventLogSource"].Trim();

			CFW.Common.clsErrorInfo cErr = null;

			cErr = CFW.Common.Messaging.GetErrorMessage(ex, msgID, args);
			
			this.m_ErrorMessage = string.Concat(cErr.Msg , "|^|", cErr.ErrorDetail);

			//Web.config의 에러트레이스모드가 on일 경우에만 이벤트로그를 남긴다.
			if (ERROR_TRACE_MODE.Equals("on")) 
				CFW.Common.Logging.WriteEventLog(EVENT_LOG, EVENT_LOG_SOURCE, System.Diagnostics.EventLogEntryType.Error
					, cErr.ErrorDetail);
		}
		#endregion

		#region ==  모달 형식의 PopUp 창인 경우 Page Load 시에 호출 ==
		/// <summary>
		/// 모달 형식의 PopUp 창인 경우 Page Load 시에 호출한다.
		/// </summary>
		protected void InitModalPopUpCache()
		{
			Response.Cache.SetNoServerCaching();
			Response.Cache.SetCacheability(System.Web.HttpCacheability.NoCache);
			Response.Cache.SetNoStore();
			Response.Cache.SetExpires(new DateTime(1900, 1, 1, 0, 0, 0, 0));
			m_Popup = true;
		}

		#endregion

		#region == 설정 된 언어로 UI Display == 
		/// <summary>
		/// 설정 된 언어로 UI Display를 합니다.
		/// </summary>
		/// <param name="control"></param>		
		private void DisplayLanguage(System.Web.UI.Control control)
		{
			string strType = string.Empty;
			foreach (System.Web.UI.Control ctrl in control.Controls)
			{
				strType = ctrl.GetType().Name;
				if (strType.Equals("HtmlInputButton"))
				{
					System.Web.UI.HtmlControls.HtmlInputButton  hbtn = (System.Web.UI.HtmlControls.HtmlInputButton)ctrl;
					
                    if( hbtn.Value.Length > 0)
					{
						hbtn.Value = CFW.Common.Dictionary.SearchStaticDicWeb(hbtn.Value, this.UserInfo.LanguageType);
					}
				}
				else if (strType.Equals("Label"))
				{
					System.Web.UI.WebControls.Label lb = (System.Web.UI.WebControls.Label)ctrl;
					
					if(lb.Text.Length > 0)
					{
						lb.Text = CFW.Common.Dictionary.SearchStaticDicWeb(lb.Text, this.UserInfo.LanguageType);
					}
				}
				else if (strType.Equals("Button"))
				{
					System.Web.UI.WebControls.Button btn = (System.Web.UI.WebControls.Button)ctrl;
					if(btn.Text.Length >0 )
					{
						btn.Text  = CFW.Common.Dictionary.SearchStaticDicWeb( btn.Text, this.UserInfo.LanguageType); 
					}
				}
                else if (strType.Equals("ImageButton"))
                {
                    System.Web.UI.WebControls.ImageButton ibtn = (System.Web.UI.WebControls.ImageButton)ctrl;

                    //권한에 따른 버튼 Evabled 설정 추가 
                    AuthChkBtn(ibtn);
                }
				else if(strType.Equals("ImageTextButton"))
				{
					CFW.Web.ImageTextButton itbtn = (CFW.Web.ImageTextButton)ctrl;
					if (itbtn.Text.Length >0)
					{
						itbtn.Text  = CFW.Common.Dictionary.SearchStaticDicWeb(itbtn.Text, this.UserInfo.LanguageType);
					}
					
					//권한에 따른 버튼 Evabled 설정 추가 
					ChangeBtnRight(itbtn);
				}
				else if(strType.Equals("HyperLink"))
				{
					System.Web.UI.WebControls.HyperLink hLink = (System.Web.UI.WebControls.HyperLink)ctrl;
					if(hLink.Text.Length > 0 )
					{
						hLink.Text = CFW.Common.Dictionary.SearchStaticDicWeb(hLink.Text, this.UserInfo.LanguageType);
					}
				}
                else if (strType.Equals("CheckBox"))
                {
                    System.Web.UI.WebControls.CheckBox chkBox = (System.Web.UI.WebControls.CheckBox)ctrl;
                    if (chkBox.Text.Length > 0)
                    {
                        chkBox.Text = CFW.Common.Dictionary.SearchStaticDicWeb(chkBox.Text, this.UserInfo.LanguageType);
                    }
                }
				else if (ctrl.Controls.Count > 0)
				{
					DisplayLanguage(ctrl);
				}
			}
		}
		#endregion 

        #region == 버튼 권한 설정(사용자 웹 관련) ==
        private void AuthChkBtn(System.Web.UI.WebControls.ImageButton ibtn)
        {
            string ControlID = ibtn.ID;
            switch (ControlID)
            {
                case "btnReg":
                case "btnUpd":
                case "btnDel":
                case "btnCommit":
                case "btnInsert":
                case "btnUpdate":
                case "btnDelete":
                //case "btnCheck":
                //case "btnApproval":
                case "btn_Reg":
                case "btn_Update":
                case "btn_Delete":
                case "btn_Reg1":
                case "btn_Reg2":                
                    {
                        if (this.UserRight == "2") ibtn.Visible = true;
                        else if (this.UserRight == "1") ibtn.Visible = false;
                        else if (this.UserRight == "0") ibtn.Visible = false;
                        else if (this.UserRight == "-1") ibtn.Visible = false;
                    }
                    break;
                case "imgSearch":
                case "btnInquiry":
                case "btnCopy":
                case "btnExcel":
                    {
                        if (this.UserRight == "2") ibtn.Visible = true;
                        else if (this.UserRight == "1") ibtn.Visible = true;
                        else if (this.UserRight == "0") ibtn.Visible = false;
                        else if (this.UserRight == "-1") ibtn.Visible = false;
                    }
                    break;       
                //default:
                //    {
                //        if (this.UserRight == "2") ibtn.Visible = true;
                //        else if (this.UserRight == "1") ibtn.Visible = true;
                //        else if (this.UserRight == "0") ibtn.Visible = true;
                //        else if (this.UserRight == "-1") ibtn.Visible = true;
                //    }
                //    break;
            }

        }
        #endregion

        #region == 버튼 권한 설정 ==
        private void ChangeBtnRight(CFW.Web.ImageTextButton itbtn)
        {
            string ControlID = itbtn.ID;
            switch (ControlID)
            {
                case "itbtnbyVehicle":
                case "itbtnColse":
                case "itbtnExcel":
                case "itbtnInquiry":
                case "itbtnPrint":
                case "itbtnReset":
                case "itbtnRightInquiry":
                case "itbtnLeftInquriy":
                    {
                        if (this.UserRight == "1" || this.UserRight == "2") itbtn.RightEnable = itbtn.Enabled = true;
                        else if (this.UserRight == "0") itbtn.RightEnable = itbtn.Enabled = false;
                        else if (this.UserRight == "-1") itbtn.RightEnable = itbtn.Enabled = true;
                    }
                    break;
                case "itbtnAdd":
                case "itbtnAutoManual":
                case "itbtnChange":
                case "itbtnChangeLastCommit":
                case "itbtnConfirm":
                case "itbtnDelete":
                case "itbtnDisposal":
                case "itbtnInsert":
                case "itbtnMaint":
                case "itbtnMaxSize":
                case "itbtnModify":
                case "itbtnRegistration":
                case "itbtnRelease":
                case "itbtnReSending":
                case "itbtnReturn":
                    {
                        if (this.UserRight == "2") itbtn.RightEnable = itbtn.Enabled = true;
                        else if (this.UserRight == "0" || this.UserRight == "1") itbtn.RightEnable = itbtn.Enabled = false;
                        else if (this.UserRight == "-1") itbtn.RightEnable = itbtn.Enabled = true;
                    }
                    break;
                default:
                    {
                        if (ControlID.StartsWith("itbtnR_"))
                        {
                            if (this.UserRight == "1" || this.UserRight == "2") itbtn.RightEnable = itbtn.Enabled = true;
                            else if (this.UserRight == "0") itbtn.RightEnable = itbtn.Enabled = false;
                            else if (this.UserRight == "-1") itbtn.RightEnable = itbtn.Enabled = true;
                        }
                        else if (ControlID.StartsWith("itbtnA_"))
                        {
                            if (this.UserRight == "2") itbtn.RightEnable = itbtn.Enabled = true;
                            else if (this.UserRight == "0" || this.UserRight == "1") itbtn.RightEnable = itbtn.Enabled = false;
                            else if (this.UserRight == "-1") itbtn.RightEnable = itbtn.Enabled = true;

                        }
                    }
                    break;
            }

        }
        #endregion

		#region == Split(주석) ==
//
//		/// <summary>
//		/// string을 지정한 구분자를 기준으로 Array형태로 반환
//		/// </summary>
//		/// <param name="data">Split할 문자</param>
//		/// <param name="delimiters">구분자</param>
//		/// <returns></returns>
//		public static string[] Split(string data, string delimiters)
//		{
//			System.Collections.ArrayList oAL = new System.Collections.ArrayList();
//			int iStartIdx = 0;
//			int iEndIdx = 0;
//			while (true)
//			{
//				iEndIdx = data.IndexOf(delimiters, iStartIdx);
//				if (iEndIdx == -1)
//				{
//					oAL.Add(data.Substring(iStartIdx, data.Length - iStartIdx));
//					break;
//				}
//				else
//					oAL.Add(data.Substring(iStartIdx, iEndIdx - iStartIdx));
//				iStartIdx = iEndIdx + delimiters.Length;
//			} 
//
//			return (string[])oAL.ToArray(System.Type.GetType("System.String"));
//		}
//
		#endregion Split	
	
		#region == 로그인 페이지로 이동 ==
		protected void GoLogin()
		{
			string strFilename = Request.Path;
            string RedirectPath = "http://" + Request.ServerVariables["SERVER_NAME"] + "/login.aspx";

            if (Request.ApplicationPath != "/")
            {
                RedirectPath = "http://" + Request.ServerVariables["SERVER_NAME"] + Request.ApplicationPath + "/login.aspx";
            }

			strFilename = strFilename.Substring(strFilename.LastIndexOf("/")).ToUpper();

			if(strFilename.ToUpper() != "/LOGIN.ASPX")//로그인 페이지 명으로 수정!
			{
                //Page.ClientScript.RegisterClientScriptBlock(this.GetType(), "GoLoginMsg", "<script>alert('해당 페이지에 대한 권한이 없습니다. 로그인 페이지로 돌아갑니다.');</script>");//메시지 코드로 변경
                Page.ClientScript.RegisterClientScriptBlock(this.GetType(), "GoLoginMove", "<script>window.top.location = '" + RedirectPath + "';</script>");//로그인 페이지 명으로 수정!
			}
		}
		#endregion

		#region == 권한 없음으로 인한 전 페이지로 이동 ==
		protected void BackPage()
		{
			string strFilename = Request.Path;                   
			strFilename = strFilename.Substring(strFilename.LastIndexOf("/")).ToUpper();

			if(strFilename.ToUpper() != "/LOGIN.ASPX")//로그인 페이지 명으로 수정!
			{
                Page.ClientScript.RegisterClientScriptBlock(this.GetType(), "GoBackMessage", "<script>alert('해당 페이지에 대한 권한이 없습니다. 이전 페이지로 돌아갑니다.');</script>");//메시지 코드로 변경
                Page.ClientScript.RegisterClientScriptBlock(this.GetType(), "GoBackPage", "<script>history.back();</script>");//로그인 페이지 명으로 수정!
			}
		}
		#endregion


		// CFW.COMMON.Dictionary의 함수 호출 시 현재 쿠키의 언어를 설정 하기 위하여 PageBase를 통함 
		// 모든 Message, Dictionary는  PageBase를 통하여 호출됨 

	    protected string SearchStaticDic(string dicID)
		{
			string DicReturn="";
			DicReturn = CFW.Common.Dictionary.SearchStaticDicWeb(dicID, this.UserInfo.LanguageType); 

			return DicReturn;
		}

        //DropDownList Binding
        protected void DataBind(DataTable dataTable, System.Web.UI.WebControls.DropDownList dropDownList, string selectedValue, CFW.Common.SelectionMode selectionMode)
        {
            CFW.Web.ControlUtil.DataBind(dataTable, dropDownList, selectedValue, selectionMode, this.UserInfo.LanguageType);
        }

        //그리드에 데이터 Bind
        protected void FillGrid(System.Web.UI.WebControls.GridView grid, object dataSource)
        {
            CFW.Web.GridUtil.FillGrid(grid, dataSource, this.UserInfo.LanguageType);
        }

        // Display Message for information
        protected void DisplayAlertMessage(string msgID)
        {
            ScriptManager.RegisterStartupScript(this, GetType(), "Message", "fn_DisplayMessage('"+ msgID +"', 'A');", true);
        }

        // Display Exception for WEB
        protected void DisplayException(string errMsg)
        {
            errMsg = errMsg.Replace("\r\n", "\\r\\n");
            ScriptManager.RegisterStartupScript(this, GetType(), "Exception", "fn_DisplayException('" + errMsg + "');", true);
        }
	}
}
