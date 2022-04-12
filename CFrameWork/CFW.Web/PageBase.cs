using System;
using System.Globalization;
using System.Data;
using System.Web.UI;

namespace CFW.Web
{
	/// <summary>
	/// PageBase�� ���� �����Դϴ�.
	/// </summary>
	public class PageBase : Page
	{
		#region == ���� ���� ==
		private string m_FormID = string.Empty;
		private string m_UserRight = string.Empty;
        private UserInfo _UserInfo;
		/// <summary>�����޼���</summary>
		private string m_ErrorMessage = string.Empty;
		/// <summary>�����޼���</summary>
		private string m_InformationMessage = string.Empty;
		/// <summary>�˾�â �ݱ�</summary>
		private bool m_WindowClose = false;
		/// <summary>�˾�â ���ϰ�</summary>
		private string m_ReturnValue = string.Empty;
		/// <summary>FormLoad() ������ ����Ǿ�� �� ClientScript</summary>
		private string m_BeforeFormLoadScript = string.Empty;
		/// <summary>FormLoad() ���Ŀ� ����Ǿ�� �� ClientScript</summary>
		private string m_AfterFormLoadScript = string.Empty;

		private string ERROR_TRACE_MODE =  string.Empty;
		private string EVENT_LOG =  string.Empty;
		private string EVENT_LOG_SOURCE  =  string.Empty;
		#endregion

		#region == ������Ƽ ==
		/// <summary>
		/// Cookie
		/// </summary>
		public CFW.Web.Cookie oCookie = null;
		private static bool m_Popup = false;

		/// <summary>
		/// PageBase������ �Դϴ�.
		/// </summary>
		public PageBase() {
		}

		/// <summary>
		/// FormID�� �����ϰų�  ������ �� �ֽ��ϴ�.
		/// </summary>
		public string FormID
		{
			get { return m_FormID; }
			set { m_FormID = value; }
		}
		/// <summary>
		/// UserRight�� �����ϰų�  ������ �� �ֽ��ϴ�.
		/// </summary>
		public string UserRight
		{
			get { return m_UserRight; }
			set { m_UserRight = value; }
		}
		/// <summary>
		/// ��� Type�� ������ �� �ֽ��ϴ�.
		/// </summary>
		public string LanguageType
		{
		    get { return this.UserInfo.LanguageType; }
		}

        /// <summary>
        /// UserInfo ����
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

		#region == OnInit �������̵� ==
		/// <summary>
		/// OnInit �������̵�
		/// </summary>
		/// <param name="e"></param>
		protected override void OnInit(EventArgs e)
		{
            this.UserInfo = UserPrincipal.SetUserInfo(this);
            SetPageHiddenField();

            this.DisplayLanguage(this);

            //������ ���� ���� ����
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

		#region == OnPreRender �������̵� ==
		/// <summary>
		/// OnPreRender �������̵�
		/// </summary>
		/// <param name="e"></param>
		protected override void OnPreRender(EventArgs e)
		{
            //SetPageHiddenField(); //���뽺ũ��Ʈ�� HiddenField����
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

		#region == �� �ε�(���� FormLoad() �Լ�) Script�� ����Ǳ� ��/�Ŀ� ������ ��ũ��Ʈ ���==
		/// <summary>
		/// Client �ܿ��� �� �ε�(���� FormLoad() �Լ�) Script�� ����Ǳ� ���� ������ ��ũ��Ʈ ���
		/// </summary>
		/// <example>
		/// <code>
		/// this.RegisterBeforeFormLoadClientScriptBlock("alert('hi KIA');");
		/// </code>
		/// </example>
		/// <param name="beforeFormLoadScript">�Լ����� Client Script ������ String �������� �Է�</param>
		protected void RegisterBeforeFormLoadClientScriptBlock(string beforeFormLoadScript)
		{
			this.m_BeforeFormLoadScript = beforeFormLoadScript;
		}
		

		/// <summary>
		/// Client �ܿ��� �� �ε�(���� FormLoad() �Լ�) Script ���� �Ŀ� ������ ��ũ��Ʈ ���
		/// </summary>
		/// <example>
		/// <code>
		///  this.RegisterAfterFormLoadClientScriptBlock("alert('hi KIA');");
		///  </code>
		/// </example>
		/// <param name="afterFormLoadScript">�Լ����� Client Script ������ String �������� �Է�</param>
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

		#region == �޽����� ��� ==

        /// <summary>
        /// �޽����� ����մϴ�.
        /// </summary>
        /// <param name="msgID">����� �޽��� ���̵�</param>
        /// <returns>��� �޽���</returns>
		protected string DisplayMessage(string msgID)
		{
			string strReturn = "";

			strReturn = DisplayMessage(msgID, null);

			return strReturn;
		}

		/// <summary>
		/// �޽����� ����մϴ�.
		/// </summary>
		/// <param name="msgID">����� �޽��� ���̵�</param>
		/// <param name="args">�߰� �޽���</param>
		/// <returns>��� �޽���</returns>
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
					if(!m_Popup)//�˾��������� �ƴϸ�
					{
						//Bottom Frame�� �޽��� ���
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
		/// �޽����� ����մϴ�.
		/// �޽��� Ÿ���� ���� ��� DB������ �޽��� Ÿ���� �����˴ϴ�.
		/// </summary>
		/// <param name="msgID">����� �޽��� ���̵�</param>
		/// <param name="args">�߰� �޽���</param>
		/// <param name="msgType">�޽���Ÿ��</param>
		/// <returns>��� �޽���</returns>
		protected string DisplayMessage(string msgID, string[] args, string msgType)
		{
			//19�� ���� ���� (msgType ������� ����)
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
				//DB�޽� Ÿ�� �켱 (DB�� �޽��� Ÿ���� ���� �� ����ڰ� �Է��� �޽��� Ÿ�� ���)
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

		#region == �����޽��� ó�� �� ȭ����� ==
		/// <summary>
		/// ���� �޽����� ó���ϰ� ȭ�鿡 ����մϴ�.
		/// Web.config�� ����Ʈ���̽���尡 on�� ��쿡�� �̺�Ʈ�α׸� ����ϴ�.
		/// </summary>
		/// <param name="ex">Exception ����</param>
		protected void DisplayError(Exception ex)
		{
			ERROR_TRACE_MODE = System.Configuration.ConfigurationSettings.AppSettings["ErrorTraceMode"].Trim();
			EVENT_LOG = System.Configuration.ConfigurationSettings.AppSettings["EventLog"].Trim();
			EVENT_LOG_SOURCE = System.Configuration.ConfigurationSettings.AppSettings["EventLogSource"].Trim();

			CFW.Common.clsErrorInfo cErr = null;

			cErr = CFW.Common.Messaging.GetErrorMessage(ex);
			
			this.m_ErrorMessage = string.Concat(cErr.Msg , "|^|", cErr.ErrorDetail);

			//Web.config�� ����Ʈ���̽���尡 on�� ��쿡�� �̺�Ʈ�α׸� �����.
			if (ERROR_TRACE_MODE.Equals("on")) 
				CFW.Common.Logging.WriteEventLog(EVENT_LOG, EVENT_LOG_SOURCE, System.Diagnostics.EventLogEntryType.Error
					, cErr.ErrorDetail);
		}
		
		/// <summary>
		/// ���� �޽����� ó���ϰ� ȭ�鿡 ����մϴ�.
		/// </summary>
		/// <param name="msgID">�޽��� ���̵�</param>
		/// <param name="ex">Exception ����</param>
		protected void DisplayError(string msgID, Exception ex)
		{
			DisplayError(msgID, ex, null);
		}

		/// <summary>
		/// ���� �޽����� ó���ϰ� ȭ�鿡 ����մϴ�.
		/// Web.config�� ����Ʈ���̽���尡 on�� ��쿡�� �̺�Ʈ�α׸� ����ϴ�.
		/// </summary>
		/// <param name="msgID">�޽��� ���̵�</param>
		/// <param name="ex">Exception ����</param>
		/// <param name="args">�߰� �Ķ����</param>
		protected void DisplayError(string msgID, Exception ex, params string[] args)
		{
			ERROR_TRACE_MODE = System.Configuration.ConfigurationSettings.AppSettings["ErrorTraceMode"].Trim();
			EVENT_LOG = System.Configuration.ConfigurationSettings.AppSettings["EventLog"].Trim();
			EVENT_LOG_SOURCE = System.Configuration.ConfigurationSettings.AppSettings["EventLogSource"].Trim();

			CFW.Common.clsErrorInfo cErr = null;

			cErr = CFW.Common.Messaging.GetErrorMessage(ex, msgID, args);
			
			this.m_ErrorMessage = string.Concat(cErr.Msg , "|^|", cErr.ErrorDetail);

			//Web.config�� ����Ʈ���̽���尡 on�� ��쿡�� �̺�Ʈ�α׸� �����.
			if (ERROR_TRACE_MODE.Equals("on")) 
				CFW.Common.Logging.WriteEventLog(EVENT_LOG, EVENT_LOG_SOURCE, System.Diagnostics.EventLogEntryType.Error
					, cErr.ErrorDetail);
		}
		#endregion

		#region ==  ��� ������ PopUp â�� ��� Page Load �ÿ� ȣ�� ==
		/// <summary>
		/// ��� ������ PopUp â�� ��� Page Load �ÿ� ȣ���Ѵ�.
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

		#region == ���� �� ���� UI Display == 
		/// <summary>
		/// ���� �� ���� UI Display�� �մϴ�.
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

                    //���ѿ� ���� ��ư Evabled ���� �߰� 
                    AuthChkBtn(ibtn);
                }
				else if(strType.Equals("ImageTextButton"))
				{
					CFW.Web.ImageTextButton itbtn = (CFW.Web.ImageTextButton)ctrl;
					if (itbtn.Text.Length >0)
					{
						itbtn.Text  = CFW.Common.Dictionary.SearchStaticDicWeb(itbtn.Text, this.UserInfo.LanguageType);
					}
					
					//���ѿ� ���� ��ư Evabled ���� �߰� 
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

        #region == ��ư ���� ����(����� �� ����) ==
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

        #region == ��ư ���� ���� ==
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

		#region == Split(�ּ�) ==
//
//		/// <summary>
//		/// string�� ������ �����ڸ� �������� Array���·� ��ȯ
//		/// </summary>
//		/// <param name="data">Split�� ����</param>
//		/// <param name="delimiters">������</param>
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
	
		#region == �α��� �������� �̵� ==
		protected void GoLogin()
		{
			string strFilename = Request.Path;
            string RedirectPath = "http://" + Request.ServerVariables["SERVER_NAME"] + "/login.aspx";

            if (Request.ApplicationPath != "/")
            {
                RedirectPath = "http://" + Request.ServerVariables["SERVER_NAME"] + Request.ApplicationPath + "/login.aspx";
            }

			strFilename = strFilename.Substring(strFilename.LastIndexOf("/")).ToUpper();

			if(strFilename.ToUpper() != "/LOGIN.ASPX")//�α��� ������ ������ ����!
			{
                //Page.ClientScript.RegisterClientScriptBlock(this.GetType(), "GoLoginMsg", "<script>alert('�ش� �������� ���� ������ �����ϴ�. �α��� �������� ���ư��ϴ�.');</script>");//�޽��� �ڵ�� ����
                Page.ClientScript.RegisterClientScriptBlock(this.GetType(), "GoLoginMove", "<script>window.top.location = '" + RedirectPath + "';</script>");//�α��� ������ ������ ����!
			}
		}
		#endregion

		#region == ���� �������� ���� �� �������� �̵� ==
		protected void BackPage()
		{
			string strFilename = Request.Path;                   
			strFilename = strFilename.Substring(strFilename.LastIndexOf("/")).ToUpper();

			if(strFilename.ToUpper() != "/LOGIN.ASPX")//�α��� ������ ������ ����!
			{
                Page.ClientScript.RegisterClientScriptBlock(this.GetType(), "GoBackMessage", "<script>alert('�ش� �������� ���� ������ �����ϴ�. ���� �������� ���ư��ϴ�.');</script>");//�޽��� �ڵ�� ����
                Page.ClientScript.RegisterClientScriptBlock(this.GetType(), "GoBackPage", "<script>history.back();</script>");//�α��� ������ ������ ����!
			}
		}
		#endregion


		// CFW.COMMON.Dictionary�� �Լ� ȣ�� �� ���� ��Ű�� �� ���� �ϱ� ���Ͽ� PageBase�� ���� 
		// ��� Message, Dictionary��  PageBase�� ���Ͽ� ȣ��� 

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

        //�׸��忡 ������ Bind
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
