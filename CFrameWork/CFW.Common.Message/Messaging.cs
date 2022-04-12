using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Data;
using System.Collections;
using System.EnterpriseServices;
using System.Xml;
using System.Web;
//using System.Data.SqlClient;
using Oracle.DataAccess.Client;


namespace CFW.Common
{
	/// <summary>
	/// Messaging�� ���� ��� �����Դϴ�.
	/// 2007.08.29 by ������ ������ �żҵ�� cs �������� �״�� �����Ͽ� ����� �͹̳ο��� 
	/// ������ ������ �żҵ带 ����ϰ� 
	/// WEB �뵵�� ������ �żҵ带 �ű� �����Ͽ� �����  ( SearchMsgStrWEB , LoadStaticMsgList, ReadMsgFromDBWeb,ReadMsgFromXmlWeb  ) 
	/// </summary>
	/// 
	[Synchronization]
	public class Messaging
	{

		#region -- Member Variables.

		public static string UNHANDLED_MSG_ID = "FW0000";
		public static Hashtable htMsgList = null;
		private static ArrayList alKeys = null;
		private static ArrayList alValues = null;
		private static Mutex mutMsgLog = new Mutex();
        public static CFW.Data.OracleComDB m_ComDB = new CFW.Data.OracleComDB();
		public static DataTable dt = null;
        public static string m_ComConfigPath = "";//CFW.Config.ComConfigPath;
		private static string m_strLocal = "";
        private static string m_strDefaultLang = CFW.Configuration.ConfigManager.Default.ReadConfig("Language", "LanguageType");
        //private static string m_strDefaultLang = CFW.Configuration.ConfigManager.Default.ReadConfig("CULTURE", "CULTURE_DEFALT");
        //private static string m_strWebDefaultLang = CFW.Configuration.ConfigManager.Default.ReadConfig("Language","LanguageType");
        				
		/// <summary>
		/// �÷� ������ �����Դϴ�.
		/// </summary>
		public static string COLUMN_DELIMITER = System.String.Concat((char)28, (char)29);

		/// <summary>
		/// �޽��� �ڵ� ���Խ� �����Դϴ�.
		/// </summary>
		public const string MESSAGE_CODE_REGEX = @"[a-zA-Z]{2}\d{1}[a-zA-Z0-9]{3}$";

		#endregion

		/// <summary>
		/// Messaging �������Դϴ�.
		/// </summary>
		public Messaging(){ }

        #region GetErrorMessage : Error �޽��� and Error ����==  �̻����..���� �ʿ�� ������ ���� �ҽ�

        /// <summary>
		/// ���� �޽��� �����ɴϴ�.
		/// </summary>
		/// <param name="ex">Exception</param>
		/// <example>
		/// Throw New Exception( SearchMsg(sMsgID, languageType) );
		/// </example>
		/// <remarks>
		/// �Ϲ� �޽����� ����ϱ� ���� Exception�� �߻���Ű�� �ʽ��ϴ�.
		/// </remarks>
		/// <returns>���� �޽���</returns>
		public static clsErrorInfo GetErrorMessage(Exception ex)
		{
			clsErrorInfo cErr = null;

			GetErrorMessage(ex, ref cErr);

			return cErr;
		}

		/// <summary>
		/// ���� �޽��� (����) �����ɴϴ�.
		/// </summary>
		/// <param name="ex">Exception</param>
		/// <param name="msgID">�޽���ID</param>
		/// <example>
		/// catch(Exception ex)
		///	{
		///		sErrMsg = CFW.Common.Messaging.GetErrorMessage(ex, "TO1100");
		///	    this.textBox1.Text = sErrMsg;
		/// }
		/// </example>
		/// <returns>���� �޽���(����)</returns>
		public static clsErrorInfo GetErrorMessage(Exception ex, string msgID)
		{
			clsErrorInfo cErr = null;
			cErr = GetErrorMessage(ex, msgID, null);
			return cErr;
		}


		/// <summary>
		/// �޽��� ID�� �ش��ϴ� �޽����� Error�� ���� �� ������ ������.
		/// </summary>
		/// <param name="ex">Exception</param>
		/// <param name="msgID">�޽���ID</param>
		/// <param name="args">�߰��޽���</param>
		/// <example>
		/// catch(Exception ex)
		///	{
		///		sErrMsg = CFW.Common.Messaging.GetErrorMessage(ex, "TO1100" ,new string[]{"PgmID2045"});
		///	    this.textBox1.Text = sErrMsg;
		/// }
		/// </example>
		/// <returns>���� �޽���(������)</returns>
		public static clsErrorInfo GetErrorMessage(Exception ex, string msgID, string[] args)
		{
			clsErrorInfo cErr = null;

			string strMsgID = msgID;
			string strMsg = "";
			string strErrorDetail = "";

			strMsg = SearchMsg(strMsgID, args);
			strErrorDetail = GetErrorDetail(ex);

			cErr = new clsErrorInfo();
			cErr.MsgID = strMsgID;
			cErr.Msg = strMsg;
			cErr.ErrorDetail = strErrorDetail;
			
			return cErr;
		}


		private static void GetErrorMessage(Exception ex, ref clsErrorInfo errInfo)
		{
			string strMsgID = UNHANDLED_MSG_ID; //Unhandled Exception �߻�
			string strMsg = "";
			string strErrorMsg = ex.Message;
			string strErrorDetail = "";

			System.Text.RegularExpressions.Regex re = new System.Text.RegularExpressions.Regex(MESSAGE_CODE_REGEX);
			
			if( (strErrorMsg.Length == 6) && (re.IsMatch(strErrorMsg)) )
			{
				strMsgID = strErrorMsg;
				strMsg = SearchMsg(strMsgID);
			}
			else
			{
				strMsg = strErrorMsg;
				strErrorDetail = GetErrorDetail(ex);
			}

			if (ex is Oracle.DataAccess.Client.OracleException) //����Ŭ ����
			{
				Oracle.DataAccess.Client.OracleException oraEx = (Oracle.DataAccess.Client.OracleException)ex;
				
				if( oraEx.Message.Substring(0,3) == "ORA" )
				{
					string strSysErrorID = oraEx.Message.Substring(4,5);

					if( strSysErrorID == "20001" ) //����� ���� �޽���
					{
						strMsgID = strErrorMsg;
						strMsg = SearchMsg(strMsgID);
					}
				}
			}

			errInfo = new clsErrorInfo();
			errInfo.MsgID = strMsgID;
			errInfo.Msg = strMsg;
			errInfo.ErrorDetail = strErrorDetail;
		}


		private static string GetErrorDetail(Exception exception)
		{
			string tmp = string.Empty;
			System.Text.StringBuilder msg = null;
			System.Diagnostics.StackTrace st1 = null;
			System.Diagnostics.StackTrace st2 = null;
			System.Diagnostics.StackFrame sf = null;
			try
			{
				//Stack�� Exception�� �ϳ��� msg�� ��´�.
				msg = new System.Text.StringBuilder();

				//msg�� ���� Ʈ���̵̽� �ð��� �㵵�� �Ѵ�.
				msg.Append("[DateTime] : "+DateTime.Now.ToString() +"");

				//StackƮ���̽��ϴºκ�
				st1 = new System.Diagnostics.StackTrace(1, true);
				msg.Append("[CallStackTrace]");
				for(int i =0; i< st1.FrameCount; i++ )
				{
					sf = st1.GetFrame(i);
					tmp = sf.GetMethod().DeclaringType.FullName + "." + sf.GetMethod().Name;
					if(tmp.IndexOf("System") != 0 && tmp.IndexOf("Microsoft") != 0 )
					{
						msg.Append(tmp);
						msg.Append(" : ("+sf.GetFileLineNumber()+")");
						msg.Append("");
					}
				}

				//Exception�� Ʈ���̽��ϴºκ�
				st2 = new System.Diagnostics.StackTrace(exception, true);
				msg.Append("; [ErrStackTrace]");
				for(int i =0; i< st2.FrameCount; i++ )
				{
					sf = st2.GetFrame(i);
					tmp = sf.GetMethod().DeclaringType.FullName+"."+sf.GetMethod().Name;
					if(tmp.IndexOf("System") != 0 && tmp.IndexOf("Microsoft") != 0 )
					{
						msg.Append(tmp);
						msg.Append(" : ("+sf.GetFileLineNumber()+")");
						msg.Append("");
					}
				}

				//���� Sql���� Exception�̸� �߰��׸��� �־��ֵ��� �Ѵ�.
				if (exception is Oracle.DataAccess.Client.OracleException)
				{
					Oracle.DataAccess.Client.OracleException oraEx = (Oracle.DataAccess.Client.OracleException)exception;
					msg.Append("; [Oracle StackTrace] : ").Append(oraEx.StackTrace);
					msg.Append("[Oracle ExceptionType] : ").Append(oraEx.Errors.GetType());
					msg.Append("[Oracle Message] : ").Append(oraEx.Message);
					msg.Append("[Oracle Number] : ").Append(oraEx.Number);
					msg.Append("[Oracle Source] : ").Append(oraEx.Source);
					msg.Append("[Oracle TargetSite] : ").Append(oraEx.TargetSite.ToString());
					msg.Append("[Oracle HelpLink] : ").Append(oraEx.HelpLink);
					msg.Append("[Oracle Procedure] : ").Append(oraEx.Procedure);
				}
				else
				{
					msg.Append("; [Exception DetailMsg] : ");
					msg.Append("{" + exception.Message + "}");
				}
			}
			catch(Exception ex)
			{
				//�̳��� throw�ϸ� �ȵȴ�, �̳� ��ü�� Exception�� ó���ϴ� ���̱⶧����...
			}
			return msg.ToString();
		}


		#endregion GetErrorMessage

		#region -- SearchMsg (WEB) �̻����..���� �ʿ�� ������ ���� �ҽ�

		/// <summary>
		/// �ε� �� Hashtable���� �޽��� ���̵� Ű������ �˻��� �޽�������ü�� �����մϴ�
		///  2007.10.05 by ������  
		/// </summary>
		/// <param name="msgID">�޽��� ID</param>
		/// <returns>�˻��� �޽��� ����ü</returns>
		public static clsMsgInfo SearchMsgStrWEB(string msgID, string langtype)
		{

			clsMsgInfo msgInfo = null;
			clsMsgInfo msgInfoNew = new clsMsgInfo();


			//htMsgList = (Hashtable) HttpContext.Current.Application["Message"];

			if(msgID != null)
			{
                //�α��ν� ������ DB�� üũ�� �ش� ���� ���� 
				if(htMsgList == null)
				{
					// �޽��� hashtable�� ���� ��� �ٽ� hashtable�� �ε� ��Ŵ 
					CFW.Common.Messaging.LoadStaticMsgList(langtype); 
				}

				if(htMsgList != null)
				{
					msgInfo = (clsMsgInfo)htMsgList[msgID];		
					if( msgInfo != null )
					{				
						msgInfoNew.Dbflg = msgInfo.Dbflg;
						msgInfoNew.MsgGrp = msgInfo.MsgGrp;
						//msgInfoNew.MsgLocal = msgInfo.MsgLocal;

						if(langtype.Equals("ko-kr"))
						{
							msgInfoNew.MsgLocal = msgInfo.MsgTextKo;
						}
						else if(langtype.Equals("en-us"))
						{
							msgInfoNew.MsgLocal = msgInfo.MsgTextEn;
						}
						else if(langtype.Equals("zh-cn"))
						{
							msgInfoNew.MsgLocal = msgInfo.MsgTextCh;
						}
                        else
                            msgInfoNew.MsgLocal = msgInfo.MsgTextLo;

						//msgInfoNew.MsgText = msgInfo.MsgText;
						msgInfoNew.MsgType = msgInfo.MsgType;

						// �ش� ����� �޽����� ���� ��� 1. ���� �޽���, 2. ID ������ ������ �� 

						if(msgInfoNew.MsgLocal.Trim().Length == 0)
						{
							if(msgInfoNew.MsgText.Trim().Length == 0)
							{
								msgInfoNew.MsgLocal = msgID;
							}
							else msgInfoNew.MsgLocal = msgInfo.MsgText;
						}
					}
					else
					{
						msgInfoNew.MsgLocal = msgID;
						msgInfoNew.MsgText = msgID;					
					}
				}
				else
				{
					//msgInfo = new clsMsgInfo();
					msgInfoNew.MsgText = msgID;
					msgInfoNew.MsgLocal = msgID;
				}
			}
			return msgInfoNew;
		}

		/// <summary>
		/// �ε� �� Hashtable���� �޽��� ���̵� Ű������ �˻��� �޽����� string ���·� �����մϴ�
		/// </summary>
		/// <param name="msgID">�޽��� ID</param>
		/// <returns>�˻��� �޽��� ����ü</returns>
		public static string SearchMsgWEB(string msgID, string langtype)
		{

			clsMsgInfo msgInfo = null;
			clsMsgInfo msgInfoNew = new clsMsgInfo();


			if(msgID != null)
			{
                //�α��ν� ������ DB�� üũ�� �ش� ���� ���� 
				if(htMsgList == null)
				{
					// �޽��� hashtable�� ���� ��� �ٽ� hashtable�� �ε� ��Ŵ 
					CFW.Common.Messaging.LoadStaticMsgList(langtype); 
				}

				if(htMsgList != null)
				{
					msgInfo = (clsMsgInfo)htMsgList[msgID];		
					if( msgInfo != null )
					{				
						msgInfoNew.Dbflg = msgInfo.Dbflg;
						msgInfoNew.MsgGrp = msgInfo.MsgGrp;
						//msgInfoNew.MsgLocal = msgInfo.MsgLocal;

						if(langtype.Equals("ko-kr"))
						{
							msgInfoNew.MsgLocal = msgInfo.MsgTextKo;
						}
						else if(langtype.Equals("en-us"))
						{
							msgInfoNew.MsgLocal = msgInfo.MsgTextEn;
						}
						else if(langtype.Equals("zh-cn"))
						{
							msgInfoNew.MsgLocal = msgInfo.MsgTextCh;
						}
                        else
                            msgInfoNew.MsgLocal = msgInfo.MsgTextLo;

						//msgInfoNew.MsgText = msgInfo.MsgText;
						msgInfoNew.MsgType = msgInfo.MsgType;

						// �ش� ����� �޽����� ���� ��� 1. ���� �޽���, 2. ID ������ ������ �� 

						if(msgInfoNew.MsgLocal.Trim().Length == 0)
						{
							if(msgInfoNew.MsgText.Trim().Length == 0)
							{
								msgInfoNew.MsgLocal = msgID;
							}
							else msgInfoNew.MsgLocal = msgInfo.MsgText;
						}
					}
					else
					{
						msgInfoNew.MsgLocal = msgID;
						msgInfoNew.MsgText = msgID;					
					}
				}
				else
				{
					//msgInfo = new clsMsgInfo();
					msgInfoNew.MsgText = msgID;
					msgInfoNew.MsgLocal = msgID;
				}
			}
			return msgInfoNew.MsgLocal; 
		}

		/// <summary>
		/// �ε� �� Hashtable���� �޽��� ���̵� Ű������ �˻��� �޽�������ü�� �����մϴ�
		/// </summary>
		/// <param name="msgID">�޽��� ID</param>
		/// <param name="args">�߰� �޽���</param>
		/// <param name="langtype">�������</param>
		/// <returns>�˻��� �޽��� ����ü</returns>
		public static clsMsgInfo SearchMsgStrWEB(string msgID, string[] args, string langtype)
		{
			clsMsgInfo msgInfo = null;
			StringBuilder sbTempText = null;
			msgInfo = SearchMsgStrWEB(msgID,langtype);

			if( msgInfo == null )
			{
				for(int i = 0; i< args.Length; i++)
				{
					sbTempText.Append("[");
					sbTempText.Append(args[i].ToString());
					sbTempText.Append("]");
				}
				msgInfo = new clsMsgInfo();
				if(sbTempText.Length>0)
				{
					msgInfo.MsgText = sbTempText.ToString();
					msgInfo.MsgLocal = sbTempText.ToString();
				}
				else
				{
					msgInfo.MsgText = msgID;
					msgInfo.MsgLocal = msgID;
				}
			}
			else
			{
				msgInfo.MsgText = string.Format(msgInfo.MsgText , args );
				msgInfo.MsgLocal = string.Format(msgInfo.MsgLocal, args ) ;
			}
			return msgInfo;
		}
		

		#endregion

        #region -- SearchMsg  ( CS ) �̻����..���� �ʿ�� ������ ���� �ҽ�
        /// <summary>
		/// �ε� �� Hashtable���� �޽��� ���̵� Ű������ �˻��� �޽�������ü�� �����մϴ�
		/// ��ȸ�� �� ���� ���� ���� 
		/// </summary>
		/// <param name="msgID">�޽��� ID</param>
		/// <returns>�˻��� �޽��� ����ü</returns>
		public static clsMsgInfo SearchMsgStr(string msgID)
		{

			clsMsgInfo msgInfo = null;
			clsMsgInfo msgInfoNew = new clsMsgInfo();
			if(htMsgList != null)
			{
				msgInfo = (clsMsgInfo)htMsgList[msgID];		
				if( msgInfo != null )
				{				
					msgInfoNew.Dbflg = msgInfo.Dbflg;
					msgInfoNew.MsgGrp = msgInfo.MsgGrp;
					msgInfoNew.MsgLocal = msgInfo.MsgLocal;

			        msgInfoNew.MsgText = msgInfo.MsgText;
					msgInfoNew.MsgType = msgInfo.MsgType;

					if(msgInfoNew.MsgLocal.Trim().Length == 0)
					{
						if(msgInfoNew.MsgText.Trim().Length == 0)
						{
							msgInfoNew.MsgLocal = msgID;
						}
						else msgInfoNew.MsgLocal = msgInfo.MsgText;
					}
				}
				else
				{
					msgInfoNew.MsgLocal = msgID;
					msgInfoNew.MsgText = msgID;					
				}
			}
			else
			{
				//msgInfo = new clsMsgInfo();
				msgInfoNew.MsgText = msgID;
				msgInfoNew.MsgLocal = msgID;
			}
			return msgInfoNew;
		}

		/// <summary>
		/// �ε� �� Hashtable���� �޽��� ���̵� Ű������ �˻��� �޽�������ü�� �����մϴ�
		/// /// ��ȸ�� �� ���� ���� ���� 
		/// </summary>
		/// <param name="msgID">�޽��� ID</param>
		/// <param name="args">�߰� �޽���</param>
		/// <returns>�˻��� �޽��� ����ü</returns>
		public static clsMsgInfo SearchMsgStr(string msgID, string[] args)
		{
			clsMsgInfo msgInfo = null;
			StringBuilder sbTempText = null;
			msgInfo = SearchMsgStr(msgID);

			if( msgInfo == null )
			{
				for(int i = 0; i< args.Length; i++)
				{
					sbTempText.Append("[");
					sbTempText.Append(args[i].ToString());
					sbTempText.Append("]");
				}
				msgInfo = new clsMsgInfo();
				if(sbTempText.Length>0)
				{
					msgInfo.MsgText = sbTempText.ToString();
					msgInfo.MsgLocal = sbTempText.ToString();
				}
				else
				{
					msgInfo.MsgText = msgID;
					msgInfo.MsgLocal = msgID;
				}
			}
			else
			{
				msgInfo.MsgText = string.Format(msgInfo.MsgText , args );
				msgInfo.MsgLocal = string.Format(msgInfo.MsgLocal, args ) ;
			}
			return msgInfo;
		}
		#endregion

        #region -- SearchMsgStr (CS) : Searching and returning message string �̻����..���� �ʿ�� ������ ���� �ҽ�


        /// <summary>
		/// �ε� �� Hashtable���� �޽��� ���̵� Ű������ �˻��� �޽������ڿ��� �����մϴ�. 
		/// </summary>
		/// <param name="msgID">�޽��� ID</param>
		/// <param name="args">�߰� �޽���</param>
		/// <returns>�˻��� �޽���</returns>
		public static string SearchMsg(string msgID, string[] args)
		{
			string strMsg = "";
			strMsg = SearchMsg(msgID);
			if( args != null && args.Length > 0)
				strMsg = string.Format(strMsg, args);
			return strMsg;
		}


		/// <summary>
		/// �ε� �� Hashtable���� �޽��� ���̵� Ű������ �˻��� �޽������ڿ��� �����մϴ�. 
		/// </summary>
		/// <param name="msgID">�޽��� ID</param>
		/// <returns>�˻��� �޽���</returns>
		public static string SearchMsg(string msgID)
		{
			string strMessage = "";
			clsMsgInfo msgInfo = null;
			System.Text.RegularExpressions.Regex re = new System.Text.RegularExpressions.Regex(MESSAGE_CODE_REGEX);
			
			if( (msgID.Length == 6) && (re.IsMatch(msgID)) )
			{
				if(htMsgList != null)
				{
					msgInfo = (clsMsgInfo)htMsgList[msgID];	
					if( msgInfo != null)
					{
						if(msgInfo.MsgLocal.Trim().Length == 0)
						{
							if(msgInfo.MsgText.Trim().Length == 0)
							{
								strMessage = msgID;
							}
							else strMessage = msgInfo.MsgText;
						}
						else strMessage = msgInfo.MsgLocal;
					}
					else
						strMessage = msgID;
				}
				else
				{
					strMessage = msgID;
				}
			}
			return strMessage;
		}
		
       	#endregion


        #region -- Load Message List (CS)  �̻��
        /// <summary>
		/// �޽��� ���̺��� �޽����� �о���ų� DB ���� ���� �� XML �޽��� ���Ͽ��� �޽����� �о�ɴϴ�.
		/// </summary>
		/// <param name="languageType">���Ÿ��</param>
		/// <param name="sErrCode">Error Code</param>
		/// <param name="sErrText">Error Text</param>
		/// <remarks>CS</remarks>
		/// <returns>���� ����</returns>
        //public static bool LoadMsgList(string languageType, ref string sErrCode, ref string sErrText)
        //{
        //    DataSet ds = null;
        //    string strFullPath = null;
        //    if(languageType.Trim().Length == 0)
        //    {
        //        languageType = CFW.Configuration.ConfigManager.Default.ReadConfig("Language", "LanguageType");		
        //    }
        //    ds = m_ComDB.GetMsgList(languageType, ref  sErrCode, ref  sErrText);

        //    m_strLocal = languageType.Replace("-","_").ToUpper();
        //    strFullPath = CFW.Configuration.ConfigManager.Default.ReadConfig("Message", "MessageFilePath");			
			
        //    //DB���� ���� �� XML ���Ͽ��� �о�´�.
        //    if((ds == null) || (ds.Tables.Count == 0) || (ds.Tables[0].Rows.Count == 0))
        //    {
        //        ReadMsgFromXml();				
        //    }
        //    else //DB ���� ���� �� DB���� �о�´�.
        //    {
        //        CFW.Common.Util.MakeXML(strFullPath,ds);
        //        ReadMsgFromDB(ds);
        //    }

        //    if(htMsgList == null) // DB �� XML ��� �о���� ���� �� False ����
        //    {
        //        return false;
        //    }
        //    return true;
        //}

        ///// <summary>
        ///// �޽��� ���̺��� �޽����� �о���ų� DB ���� ���� �� XML �޽��� ���Ͽ��� �޽����� �о�ɴϴ�.
        ///// </summary>
        ///// <param name="languageType">���Ÿ��</param>
        //public static void LoadMsgList(string languageType)
        //{
        //    string sErrCode = "";
        //    string sErrText = "";
        //    try
        //    {
        //        LoadMsgList(languageType, ref sErrCode, ref sErrText);
        //    }
        //    catch(Exception ex)
        //    {
        //        throw new Exception(sErrText);
        //    }
        //}
		#endregion

        #region -- ȯ�漳�����Ͽ� ������ �⺻ ���� �о�ɴϴ�.
        /// <summary>
		/// ȯ�漳�����Ͽ� ������ �⺻ ���� �о�ɴϴ�.
		/// </summary>
		/// <returns>�⺻��� ������</returns>
		public static string getDefaultLanguage()
		{
            string reLanguage = CFW.Configuration.ConfigManager.Default.ReadConfig("Language", "LanguageType");
			return reLanguage;
		}
		#endregion

		#region  -- Load Message List (WEB/CS ����) 
		/// <summary>
		/// �޽��� ���̺��� �޽����� �о���ų� DB ���� ���� �� XML �޽��� ���Ͽ��� �޽����� �о�ɴϴ�.
		///  2007.09.12 by ������  Web �α��� �� �ѹ��� Dictionary �ε���  
		/// </summary>
		/// <remarks>Web</remarks>
		/// <param name="languageType"></param>
		public static void LoadStaticMsgList(string languageType)
		{
			DataSet ds          = null;
			string  strFullPath = null;

			if(languageType.Trim().Length == 0)     languageType = getDefaultLanguage();

            //string strErrCode = string.Empty;
            //string strErrText = string.Empty;
			//ds = m_ComDB.GetMsgListWeb(languageType,ref  strErrCode, ref  strErrText);
            
            if (HttpContext.Current == null)    ds = CFW.Data.OracleComDB.GetMsgList("C");          // CS�� Message
            else                                ds = CFW.Data.OracleComDB.GetMsgList("W");          // Web�� Message            

			//ds = (DataSet) HttpContext.Current.Application["Message"];
			m_strLocal = languageType.Replace("-","_").ToUpper();
            strFullPath = CFW.Configuration.ConfigManager.Default.ReadConfig("Message", "MessageFilePath");
			
			//DB���� ���� �� XML ���Ͽ��� �о�´�.
			if((ds == null) || (ds.Tables.Count == 0) || (ds.Tables[0].Rows.Count == 0))
			{
				ReadMsgFromXml();				
			}
			else //DB ���� ���� �� DB���� �о�´�.
			{
                CFW.Common.Util.MakeXML(strFullPath, ds);
				ReadMsgFromDB(ds);
			}
		}

        #region   == �̻��
		/// <summary>
		/// �޽��� ���̺��� �޽����� �о���ų� DB ���� ���� �� XML �޽��� ���Ͽ��� �޽����� �о�ɴϴ�.
		///  2008.08.29 by ������  Web �α��� �� �ѹ��� Dictionary �ε���  (HMI �䱸 ���� �α��ν� ������ DB ���� ) 
		/// </summary>
		/// <remarks>Web</remarks>
		/// <param name="languageType"></param>
        //public static void LoadStaticMsgList(string languageType, string dbType)
        //{
        //    DataSet ds          = null;
        //    string  strFullPath = null;

        //    if(languageType.Trim().Length == 0)     languageType = getDefaultLanguage();
	
        //    string strErrCode = string.Empty;
        //    string strErrText = string.Empty;
        //    ds = m_ComDB.GetMsgListWeb(languageType, dbType, ref  strErrCode, ref  strErrText);

        //    //ds = (DataSet) HttpContext.Current.Application["Message"];
        //    m_strLocal = languageType.Replace("-","_").ToUpper();
        //    strFullPath = CFW.Configuration.ConfigManager.Default.ReadConfig("Config","ConfigFilePath");			
			
        //    //DB���� ���� �� XML ���Ͽ��� �о�´�.
        //    if((ds == null) || (ds.Tables.Count == 0) || (ds.Tables[0].Rows.Count == 0))
        //    {
        //        ReadMsgFromXml();				
        //    }
        //    else //DB ���� ���� �� DB���� �о�´�.
        //    {
        //        ReadMsgFromDB(ds);
        //    }
        //}
        #endregion

		// DB���� �޽��� �о�� Hashtable�� ��´�. 
		private static void ReadMsgFromDB(DataSet ds)
		{
			htMsgList   = new Hashtable();
			alKeys      = new ArrayList();
			alValues    = new ArrayList();

			clsMsgInfo msgInfo;
			htMsgList.Clear();
			string strLocal = "MSG_TEXT_"+ m_strLocal;
			string strMsgText= "";
			
            //if(m_strWebDefaultLang == null) strMsgText = "MSG_TEXT_" +m_strDefaultLang.ToUpper().Replace("-","_");
            //else                            strMsgText = "MSG_TEXT_" +m_strWebDefaultLang.ToUpper().Replace("-","_");
            strMsgText = "MSG_TEXT_" + m_strDefaultLang.ToUpper().Replace("-", "_");

			for(int i = 0; i < ds.Tables[0].Rows.Count; i++)
			{
				msgInfo = new clsMsgInfo();
				alKeys.Add(ds.Tables[0].Rows[i]["MSG_ID"].ToString().Trim());
                msgInfo.MsgGrp = ""; // ds.Tables[0].Rows[i]["MSG_GRP"].ToString().Trim();
				msgInfo.MsgText    = ds.Tables[0].Rows[i][strMsgText].ToString().Trim();
                msgInfo.Dbflg = ""; // ds.Tables[0].Rows[i]["DB_LOG_FLG"].ToString().Trim();
				msgInfo.MsgLocal   = ds.Tables[0].Rows[i][strLocal].ToString().Trim();
                msgInfo.MsgType = ""; // ds.Tables[0].Rows[i]["MSG_TYPE"].ToString().Trim();
				//2007.08.29 by ������  ��ü��� �޼����� �޸𸮿� �ε��Ŵ
                msgInfo.MsgTextKo = ds.Tables[0].Rows[i]["MSG_TEXT_KO_KR"].ToString().Trim();
				msgInfo.MsgTextEn = ds.Tables[0].Rows[i]["MSG_TEXT_EN_US"].ToString().Trim();
                msgInfo.MsgTextLo = ds.Tables[0].Rows[i]["MSG_TEXT_LO_LN"].ToString().Trim();

				alValues.Add(msgInfo);	
			}

			if(alKeys.Count>0)
			{
					for(int idx=0; idx<alKeys.Count; idx++)
					{
						if(!htMsgList.Contains(alKeys[idx])) 
						{
							htMsgList.Add( (string)alKeys[idx], (clsMsgInfo)alValues[idx]);
						}
					}

				   // HttpContext.Current.Application["Message"] = htMsgList; 
			}
			else
			{
				htMsgList = new Hashtable();
				htMsgList.Clear();
			}
		}

		// Message.XML���� �޽��� �о�� Hashtable�� ��´�.	
		private static bool ReadMsgFromXml()
		{
			string strFullPath = null;
            strFullPath = CFW.Configuration.ConfigManager.Default.ReadConfig("Message", "MessageFilePath");

			DataSet dsData = new DataSet();
			dsData.ReadXml(strFullPath);		
			ReadMsgFromDB(dsData);
			
			return true;
		}
	    #endregion

        #region == �����ҽ� ������� ����.. ���� ��������.

		// DB���� �޽��� �о�� Hashtable�� ��´�.
		// 2007.10.08 by ������ Cs�� web���� �������� ����ϰ� �����Ƿ� ������ WEB �żҵ带 �űԷ� ������  
        //private static void ReadMsgFromDBWeb(DataSet ds)
        //{
        //    htMsgList = new Hashtable();
        //    alKeys = new ArrayList();
        //    alValues = new ArrayList();
        //    clsMsgInfo msgInfo;
        //    htMsgList.Clear();
        //    string strLocal = "MSG_TEXT_"+ m_strLocal;
        //    string strMsgText= "";
			
        //    if(m_strWebDefaultLang == null)
        //    {
        //        strMsgText = "MSG_TEXT_" +m_strDefaultLang.ToUpper().Replace("-","_");
        //    }
        //    else
        //    {
        //        strMsgText =  "MSG_TEXT_" +m_strWebDefaultLang.ToUpper().Replace("-","_");
        //    }

        //    for(int i = 0; i < ds.Tables[0].Rows.Count; i++)
        //    {
        //        msgInfo = new clsMsgInfo();
        //        alKeys.Add(ds.Tables[0].Rows[i]["MSG_ID"].ToString().Trim());
        //        msgInfo.MsgGrp = ""; // ds.Tables[0].Rows[i]["MSG_GRP"].ToString().Trim();
        //        msgInfo.MsgText    = ds.Tables[0].Rows[i][strMsgText].ToString().Trim();
        //        msgInfo.Dbflg = ""; // ds.Tables[0].Rows[i]["DB_LOG_FLG"].ToString().Trim();
        //        msgInfo.MsgLocal   = ds.Tables[0].Rows[i][strLocal].ToString().Trim();
        //        msgInfo.MsgType = ""; // ds.Tables[0].Rows[i]["MSG_TYPE"].ToString().Trim();
        //        //2007.08.29 by ������  ��ü��� �޼����� �޸𸮿� �ε��Ŵ
        //        msgInfo.MsgTextKo = ds.Tables[0].Rows[i]["MSG_TEXT_KO_KR"].ToString().Trim();
        //        msgInfo.MsgTextEn = ds.Tables[0].Rows[i]["MSG_TEXT_EN_US"].ToString().Trim();

        //        alValues.Add(msgInfo);	
        //    }
        //    if(alKeys.Count>0)
        //    {
        //            for(int idx=0; idx<alKeys.Count; idx++)
        //            {
        //                if(!htMsgList.Contains(alKeys[idx])) 
        //                {
        //                    htMsgList.Add( (string)alKeys[idx], (clsMsgInfo)alValues[idx]);
        //                }
        //            }

        //           // HttpContext.Current.Application["Message"] = htMsgList; 
        //    }
        //    else
        //    {
        //        htMsgList = new Hashtable();
        //        htMsgList.Clear();
        //    }
        //}

        //// Message.XML���� �޽��� �о�� Hashtable�� ��´�.	
        //// Cs�� web���� �������� ����ϰ� �����Ƿ� ������ WEB �żҵ带 �űԷ� ������  
        //private static bool ReadMsgFromXmlWeb()
        //{
        //    string strFullPath = null;
        //    strFullPath = CFW.Configuration.ConfigManager.Default.ReadConfig("Message", "MessageFilePath");

        //    //strFullPath = CFW.Common.Configuration.ReadConfigValue("MessageFilePath");

        //    //if(strFullPath == null)
        //    //{
        //    //    strFullPath = CFW.Common.Configuration.ReadConfigValue("FILEPATH","MESSAGE",m_ComConfigPath);
        //    //}

        //    DataSet dsData = new DataSet();
        //    dsData.ReadXml(strFullPath);		
        //    ReadMsgFromDBWeb(dsData);
			
        //    return true;
        //}


        //// DB���� �޽��� �о�� Hashtable�� ��´�.
        //private static void ReadMsgFromDB(DataSet ds)
        //{

        //    htMsgList = new Hashtable();
        //    alKeys = new ArrayList();
        //    alValues = new ArrayList();
        //    clsMsgInfo msgInfo;
        //    htMsgList.Clear();
        //    string strLocal = "MSG_TEXT_"+ m_strLocal;
        //    string strMsgText= "";
			
        //    if(m_strWebDefaultLang == null)
        //    {
        //        strMsgText = "MSG_TEXT_" +m_strDefaultLang.ToUpper().Replace("-","_");
        //    }
        //    else
        //    {
        //        strMsgText =  "MSG_TEXT_" +m_strWebDefaultLang.ToUpper().Replace("-","_");
        //    }
        //    for(int i = 0; i < ds.Tables[0].Rows.Count; i++)
        //    {
        //        msgInfo = new clsMsgInfo();
        //        alKeys.Add(ds.Tables[0].Rows[i]["MSG_ID"].ToString().Trim());
        //        msgInfo.MsgGrp     = ds.Tables[0].Rows[i]["MSG_GRP"].ToString().Trim();
        //        msgInfo.MsgText    = ds.Tables[0].Rows[i][strMsgText].ToString().Trim();
        //        msgInfo.Dbflg      = ds.Tables[0].Rows[i]["DB_LOG_FLG"].ToString().Trim();
        //        msgInfo.MsgLocal   = ds.Tables[0].Rows[i][strLocal].ToString().Trim();
        //        msgInfo.MsgType	= ds.Tables[0].Rows[i]["MSG_TYPE"].ToString().Trim();
        //        alValues.Add(msgInfo);	

        //    }
        //    if(alKeys.Count>0)
        //    {
        //        for(int idx=0; idx<alKeys.Count; idx++)
        //        {
        //            if(!htMsgList.Contains(alKeys[idx])) 
        //            {
        //                htMsgList.Add( (string)alKeys[idx], (clsMsgInfo)alValues[idx]);
        //            }
        //        }
							
        //    }
        //    else
        //    {
        //        htMsgList = new Hashtable();
        //        htMsgList.Clear();
        //    }
        //}
	
        //// Message.XML���� �޽��� �о�� Hashtable�� ��´�.	
        //private static bool ReadMsgFromXml()
        //{
        //    string strFullPath = null;
        //    strFullPath = CFW.Configuration.ConfigManager.Default.ReadConfig("Message","MessageFilePath");
        //    if(strFullPath == null)
        //    {
        //        strFullPath = CFW.Configuration.ConfigManager.Default.ReadConfig("FILEPATH", "MESSAGE");
        //    }
        //    DataSet dsData = new DataSet();
        //    dsData.ReadXml(strFullPath);		
        //    ReadMsgFromDB(dsData);
			
        //    return true;
        //}
        #endregion
	}

	#region -- class clsMsgInfo : Message Structure
	/// <summary>
	/// clsMsgInfo Structure - �޽��� ���� ����ü
	/// C_DICTIONARY_DF ���̺� ������ ���� MSG_TEXT_KO_KR, MSG_TEXT_EN_US
	/// </summary>
	public class clsMsgInfo
	{
		#region -- Member
		/// <summary>
		/// �޽��� �׷�
		/// </summary>
		public string MsgGrp       = "";			
		/// <summary>
		/// �޽���(�⺻ - Logging ���)
		/// </summary>
		public string MsgText      = "";			
		/// <summary>
		/// DB ���忩��
		/// </summary>
		public string Dbflg      	= "";			
		/// <summary>
		/// �޽��� ����
		/// </summary>
		public string MsgType   	= "";			
		/// <summary>
		/// ����� ���� Language Text
		/// </summary>
		public string MsgLocal     = "";		
		/// <summary>
		/// �ѱ��� Language Text
		/// </summary>
		 public string MsgTextKo     = "";	 
		/// <summary>
		/// ���� Language Text
		/// </summary>
		public string MsgTextEn     = "";	
		/// <summary>
		/// �߱��� Language Text
		/// </summary>
		public string MsgTextCh   = "";
        /// <summary>
        /// Local Language Text
        /// </summary>
        public string MsgTextLo = "";	

		#endregion
	}
	#endregion

	#region -- class clsMsgType 

	/// <summary>
	/// �޽��� Ÿ��
	/// </summary>
	public class MSG
	{
		/// <summary>
		/// Information
		/// </summary>
		public const string			LEVEL_NONE		= "";
		/// <summary>
		/// Information
		/// </summary>
		public const string			LEVEL_INFO		= "I";
		/// <summary>
		/// Alarm, warnning
		/// </summary>
		public const string			LEVEL_ALARM		= "A";
		/// <summary>
		/// error
		/// </summary>
		public const string			LEVEL_ERROR		= "E";
		/// <summary>
		/// confirm
		/// </summary>
		public const string			LEVEL_CONFIRM	= "C";
	}

	#endregion

	#region class clsErrorInfo : ErrorInfo Structure

	/// <summary>
	/// ���� ����
	/// </summary>
	public class clsErrorInfo
	{
		/// <summary>
		/// �޽��� ���̵�
		/// </summary>
		public string MsgID	= "";
		/// <summary>
		/// �޽���
		/// </summary>
		public string Msg	= "";
		/// <summary>
		/// ���� �� ����
		/// </summary>
		public string ErrorDetail	= "";
	}

	#endregion

	#region -- class LanguageType 
	/// <summary>
	/// ��� Ÿ��
	/// </summary>
	public class LanguageType
	{
		public const string �Ұ����ƾ�_�Ұ�����                  = "BG-BG"; 	
		public const string īŻ�δϾƾ�_īŻ�δϾ�              = "CA-ES";		
		public const string �߱���_ȫ��Ư��������                = "ZH-HK";		
		public const string �߱���_��ī��Ư��������              = "ZH-MO";		
		public const string �߱���_�߱�                          = "ZH-CN";		
		public const string �߱���_��ü                          = "ZH-CHS";	
		public const string �߱���_�̰�����                      = "ZH-SG";		
		public const string �߱���_�븸                          = "ZH-TW";		
		public const string �߱���_��ü                          = "ZH-CHT";	
		public const string ũ�ξ�Ƽ�ƾ�_ũ�ξ�Ƽ��              = "HR-HR";		
		public const string ü�ھ�_ü��                          = "CS-CZ";		
		public const string ����ũ��_����ũ                      = "DA-DK";		
		public const string ������_�����                      = "DIV-MV";	
		public const string �״������_���⿡                    = "NL-BE";		
		public const string �״������_�״�����                  = "NL-NL";		
		public const string ����_����                            = "EN-GB";		
		public const string ����_�̱�                            = "EN-US";		
		public const string ����_���ٺ��                        = "EN-ZW";		
		public const string ������Ͼƾ�_������Ͼ�              = "ET-EE";		
		public const string ��ν���_�������                    = "FO-FO";		
		public const string �丣�þƾ�_�̶�                      = "FA-IR";		
		public const string ��������_������                      = "FR-FR";		
		public const string �����þƾ�_�����þ�                  = "GL-ES";		
		public const string �׷����߾�_�׷�����                  = "KA-GE";		
		public const string ���Ͼ�_����Ʈ����                    = "DE-AT";		
		public const string ���Ͼ�_����                          = "DE-DE";		
		public const string ���Ͼ�_�����ٽ�Ÿ��                  = "DE-LI";		
		public const string ���Ͼ�_����θ�ũ                    = "DE-LU";		
		public const string ���Ͼ�_������                        = "DE-CH";		
		public const string �׸�����_�׸���                      = "EL-GR";		
		public const string ���ڶ�Ʈ��_�ε�                      = "GU-IN";		
		public const string ���긮��_�̽���                    = "HE-IL";		
		public const string �����_�ε�                          = "HI-IN";		
		public const string �밡����_�밡��                      = "HU-HU";		
		public const string ���̽������_���̽�����              = "IS-IS";		
		public const string �ε��׽þƾ�_�ε��׽þ�              = "ID-ID";		
		public const string ��Ż���ƾ�_��Ż����                  = "IT-IT";		
		public const string ��Ż���ƾ�_������                    = "IT-CH";		
		public const string �Ϻ���_�Ϻ�                          = "JA-JP";		
		public const string ī���پ�_�ε�                        = "KN-IN";		
		public const string ī�����_ī���彺ź                  = "KK-KZ";		
		public const string ��ĭ��_�ε�                          = "KOK-IN";	
		public const string �ѱ���_�ѱ�                          = "KO-KR";		
		public const string Ű���⽺��_Ű���⽺��ź              = "KY-KG";		
		public const string ��Ʈ��ƾ�_��Ʈ���                  = "LV-LV";		
		public const string �����ƴϾƾ�_�����ƴϾ�              = "LT-LT";		
		public const string ���ɵ��Ͼƾ�_���ɵ��Ͼ�              = "MK-MK";		
		public const string �����̾�_��糪��                    = "MS-BN";		
		public const string �����̾�_�����̽þ�                  = "MS-MY";		
		public const string ����Ƽ��_�ε�                        = "MR-IN";		
		public const string �����_����                          = "MN-MN";		
		public const string �븣���̾�_����_�븣����             = "NB-NO";		
		public const string �븣���̾�_�ϳ븣��ũ_�븣����       = "NN-NO";		
		public const string �������_������                      = "PL-PL";		
		public const string ����������_�����                    = "PT-BR";		
		public const string ����������_��������                  = "PT-PT";		
		public const string �����_�ε�                          = "PA-IN";		
		public const string �縶�Ͼƾ�_�縶�Ͼ�                  = "RO-RO";		
		public const string ���þƾ�_���þ�                      = "RU-RU";		
		public const string �꽺ũ��Ʈ��_�ε�                    = "SA-IN";		
		public const string ������ƾ�_Ű���ڸ�_�������         = "SR-SP-CYRL";	
		public const string ������ƾ�_��ƾ����_�������         = "SR-SP-LATN";	
		public const string ���ι�Ű�ƾ�_���ι�Ű��              = "SK-SK";		
		public const string �ַκ��Ͼƾ�_�ַκ��Ͼ�              = "SL-SI";		
		public const string ��������_�ɶ���                      = "SV-FI";		
		public const string ��������_������                      = "SV-SE";		
		public const string �ø��ƾ�_�ø���                      = "SYR-SY";	
		public const string Ÿ�о�_�ε�							 = "TA-IN";		
		public const string �ڷ籸��_�ε�						 = "TE-IN";		
		public const string �±���_�±�							 = "TH-TH";		
		public const string ��Ű��_��Ű							 = "TR-TR";		
		public const string ��ũ���̳���_��ũ���̳�				 = "UK-UA";		
		public const string �츣�ξ�_��Ű��ź					 = "UR-PK";		
		public const string ���ũ��_Ű���ڸ�_���Ű��ź	 = "UZ-UZ-CYRL";	
		public const string ���ũ��_��ƾ����_���Ű��ź	 = "UZ-UZ-LATN";	
		public const string ��Ʈ����_��Ʈ��						 = "VI-VN";	
	}
	#endregion
}
