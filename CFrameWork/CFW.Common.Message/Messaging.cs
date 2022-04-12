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
	/// Messaging에 대한 요약 설명입니다.
	/// 2007.08.29 by 한주희 기존의 매소드는 cs 버젼으로 그대로 유지하여 데몬과 터미널에서 
	/// 기존과 동일한 매소드를 사용하고 
	/// WEB 용도의 별도의 매소드를 신규 개발하여 사용함  ( SearchMsgStrWEB , LoadStaticMsgList, ReadMsgFromDBWeb,ReadMsgFromXmlWeb  ) 
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
		/// 컬럼 구분자 정의입니다.
		/// </summary>
		public static string COLUMN_DELIMITER = System.String.Concat((char)28, (char)29);

		/// <summary>
		/// 메시지 코드 정규식 정의입니다.
		/// </summary>
		public const string MESSAGE_CODE_REGEX = @"[a-zA-Z]{2}\d{1}[a-zA-Z0-9]{3}$";

		#endregion

		/// <summary>
		/// Messaging 생성자입니다.
		/// </summary>
		public Messaging(){ }

        #region GetErrorMessage : Error 메시지 and Error 정보==  미사용중..향후 필요시 재정리 참조 소스

        /// <summary>
		/// 에러 메시지 가져옵니다.
		/// </summary>
		/// <param name="ex">Exception</param>
		/// <example>
		/// Throw New Exception( SearchMsg(sMsgID, languageType) );
		/// </example>
		/// <remarks>
		/// 일반 메시지를 출력하기 위해 Exception은 발생시키지 않습니다.
		/// </remarks>
		/// <returns>에러 메시지</returns>
		public static clsErrorInfo GetErrorMessage(Exception ex)
		{
			clsErrorInfo cErr = null;

			GetErrorMessage(ex, ref cErr);

			return cErr;
		}

		/// <summary>
		/// 에러 메시지 (정보) 가져옵니다.
		/// </summary>
		/// <param name="ex">Exception</param>
		/// <param name="msgID">메시지ID</param>
		/// <example>
		/// catch(Exception ex)
		///	{
		///		sErrMsg = CFW.Common.Messaging.GetErrorMessage(ex, "TO1100");
		///	    this.textBox1.Text = sErrMsg;
		/// }
		/// </example>
		/// <returns>에러 메시지(정보)</returns>
		public static clsErrorInfo GetErrorMessage(Exception ex, string msgID)
		{
			clsErrorInfo cErr = null;
			cErr = GetErrorMessage(ex, msgID, null);
			return cErr;
		}


		/// <summary>
		/// 메시지 ID에 해당하는 메시지와 Error에 대한 상세 정보를 가져옴.
		/// </summary>
		/// <param name="ex">Exception</param>
		/// <param name="msgID">메시지ID</param>
		/// <param name="args">추가메시지</param>
		/// <example>
		/// catch(Exception ex)
		///	{
		///		sErrMsg = CFW.Common.Messaging.GetErrorMessage(ex, "TO1100" ,new string[]{"PgmID2045"});
		///	    this.textBox1.Text = sErrMsg;
		/// }
		/// </example>
		/// <returns>에러 메시지(상세정보)</returns>
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
			string strMsgID = UNHANDLED_MSG_ID; //Unhandled Exception 발생
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

			if (ex is Oracle.DataAccess.Client.OracleException) //오라클 에러
			{
				Oracle.DataAccess.Client.OracleException oraEx = (Oracle.DataAccess.Client.OracleException)ex;
				
				if( oraEx.Message.Substring(0,3) == "ORA" )
				{
					string strSysErrorID = oraEx.Message.Substring(4,5);

					if( strSysErrorID == "20001" ) //사용자 정의 메시지
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
				//Stack과 Exception을 하나의 msg에 담는다.
				msg = new System.Text.StringBuilder();

				//msg에 현재 트래이싱된 시간을 담도록 한다.
				msg.Append("[DateTime] : "+DateTime.Now.ToString() +"");

				//Stack트래이싱하는부분
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

				//Exception을 트래이싱하는부분
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

				//만약 Sql관련 Exception이면 추가항목을 넣어주도록 한다.
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
				//이넘은 throw하면 안된다, 이넘 자체가 Exception을 처리하는 넘이기때문에...
			}
			return msg.ToString();
		}


		#endregion GetErrorMessage

		#region -- SearchMsg (WEB) 미사용중..향후 필요시 재정리 참조 소스

		/// <summary>
		/// 로드 된 Hashtable에서 메시지 아이디를 키값으로 검색한 메시지구조체를 리턴합니다
		///  2007.10.05 by 한주희  
		/// </summary>
		/// <param name="msgID">메시지 ID</param>
		/// <returns>검색된 메시지 구조체</returns>
		public static clsMsgInfo SearchMsgStrWEB(string msgID, string langtype)
		{

			clsMsgInfo msgInfo = null;
			clsMsgInfo msgInfoNew = new clsMsgInfo();


			//htMsgList = (Hashtable) HttpContext.Current.Application["Message"];

			if(msgID != null)
			{
                //로그인시 접속할 DB를 체크함 해당 로직 삭제 
				if(htMsgList == null)
				{
					// 메시지 hashtable아 없는 경우 다시 hashtable에 로드 시킴 
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

						// 해당 언어의 메시지가 없는 경우 1. 영어 메시지, 2. ID 순으로 나오게 함 

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
		/// 로드 된 Hashtable에서 메시지 아이디를 키값으로 검색한 메시지를 string 형태로 리턴합니다
		/// </summary>
		/// <param name="msgID">메시지 ID</param>
		/// <returns>검색된 메시지 구조체</returns>
		public static string SearchMsgWEB(string msgID, string langtype)
		{

			clsMsgInfo msgInfo = null;
			clsMsgInfo msgInfoNew = new clsMsgInfo();


			if(msgID != null)
			{
                //로그인시 접속할 DB를 체크함 해당 로직 삭제 
				if(htMsgList == null)
				{
					// 메시지 hashtable아 없는 경우 다시 hashtable에 로드 시킴 
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

						// 해당 언어의 메시지가 없는 경우 1. 영어 메시지, 2. ID 순으로 나오게 함 

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
		/// 로드 된 Hashtable에서 메시지 아이디를 키값으로 검색한 메시지구조체를 리턴합니다
		/// </summary>
		/// <param name="msgID">메시지 ID</param>
		/// <param name="args">추가 메시지</param>
		/// <param name="langtype">언어종류</param>
		/// <returns>검색된 메시지 구조체</returns>
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

        #region -- SearchMsg  ( CS ) 미사용중..향후 필요시 재정리 참조 소스
        /// <summary>
		/// 로드 된 Hashtable에서 메시지 아이디를 키값으로 검색한 메시지구조체를 리턴합니다
		/// 조회될 언어를 지정 하지 않음 
		/// </summary>
		/// <param name="msgID">메시지 ID</param>
		/// <returns>검색된 메시지 구조체</returns>
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
		/// 로드 된 Hashtable에서 메시지 아이디를 키값으로 검색한 메시지구조체를 리턴합니다
		/// /// 조회될 언어를 지정 하지 않음 
		/// </summary>
		/// <param name="msgID">메시지 ID</param>
		/// <param name="args">추가 메시지</param>
		/// <returns>검색된 메시지 구조체</returns>
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

        #region -- SearchMsgStr (CS) : Searching and returning message string 미사용중..향후 필요시 재정리 참조 소스


        /// <summary>
		/// 로드 된 Hashtable에서 메시지 아이디를 키값으로 검색한 메시지문자열을 리턴합니다. 
		/// </summary>
		/// <param name="msgID">메시지 ID</param>
		/// <param name="args">추가 메시지</param>
		/// <returns>검색된 메시지</returns>
		public static string SearchMsg(string msgID, string[] args)
		{
			string strMsg = "";
			strMsg = SearchMsg(msgID);
			if( args != null && args.Length > 0)
				strMsg = string.Format(strMsg, args);
			return strMsg;
		}


		/// <summary>
		/// 로드 된 Hashtable에서 메시지 아이디를 키값으로 검색한 메시지문자열을 리턴합니다. 
		/// </summary>
		/// <param name="msgID">메시지 ID</param>
		/// <returns>검색된 메시지</returns>
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


        #region -- Load Message List (CS)  미사용
        /// <summary>
		/// 메시지 테이블에서 메시지를 읽어오거나 DB 연결 실패 시 XML 메시지 파일에서 메시지를 읽어옵니다.
		/// </summary>
		/// <param name="languageType">언어타입</param>
		/// <param name="sErrCode">Error Code</param>
		/// <param name="sErrText">Error Text</param>
		/// <remarks>CS</remarks>
		/// <returns>성공 여부</returns>
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
			
        //    //DB연결 실패 시 XML 파일에서 읽어온다.
        //    if((ds == null) || (ds.Tables.Count == 0) || (ds.Tables[0].Rows.Count == 0))
        //    {
        //        ReadMsgFromXml();				
        //    }
        //    else //DB 연결 성공 시 DB에서 읽어온다.
        //    {
        //        CFW.Common.Util.MakeXML(strFullPath,ds);
        //        ReadMsgFromDB(ds);
        //    }

        //    if(htMsgList == null) // DB 및 XML 모두 읽어오기 실패 시 False 리턴
        //    {
        //        return false;
        //    }
        //    return true;
        //}

        ///// <summary>
        ///// 메시지 테이블에서 메시지를 읽어오거나 DB 연결 실패 시 XML 메시지 파일에서 메시지를 읽어옵니다.
        ///// </summary>
        ///// <param name="languageType">언어타입</param>
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

        #region -- 환경설정파일에 설정된 기본 언어값을 읽어옵니다.
        /// <summary>
		/// 환경설정파일에 설정된 기본 언어값을 읽어옵니다.
		/// </summary>
		/// <returns>기본언어 설정값</returns>
		public static string getDefaultLanguage()
		{
            string reLanguage = CFW.Configuration.ConfigManager.Default.ReadConfig("Language", "LanguageType");
			return reLanguage;
		}
		#endregion

		#region  -- Load Message List (WEB/CS 통합) 
		/// <summary>
		/// 메시지 테이블에서 메시지를 읽어오거나 DB 연결 실패 시 XML 메시지 파일에서 메시지를 읽어옵니다.
		///  2007.09.12 by 한주희  Web 로그인 시 한번만 Dictionary 로드함  
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
            
            if (HttpContext.Current == null)    ds = CFW.Data.OracleComDB.GetMsgList("C");          // CS용 Message
            else                                ds = CFW.Data.OracleComDB.GetMsgList("W");          // Web용 Message            

			//ds = (DataSet) HttpContext.Current.Application["Message"];
			m_strLocal = languageType.Replace("-","_").ToUpper();
            strFullPath = CFW.Configuration.ConfigManager.Default.ReadConfig("Message", "MessageFilePath");
			
			//DB연결 실패 시 XML 파일에서 읽어온다.
			if((ds == null) || (ds.Tables.Count == 0) || (ds.Tables[0].Rows.Count == 0))
			{
				ReadMsgFromXml();				
			}
			else //DB 연결 성공 시 DB에서 읽어온다.
			{
                CFW.Common.Util.MakeXML(strFullPath, ds);
				ReadMsgFromDB(ds);
			}
		}

        #region   == 미사용
		/// <summary>
		/// 메시지 테이블에서 메시지를 읽어오거나 DB 연결 실패 시 XML 메시지 파일에서 메시지를 읽어옵니다.
		///  2008.08.29 by 한주희  Web 로그인 시 한번만 Dictionary 로드함  (HMI 요구 사항 로그인시 접속할 DB 지정 ) 
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
			
        //    //DB연결 실패 시 XML 파일에서 읽어온다.
        //    if((ds == null) || (ds.Tables.Count == 0) || (ds.Tables[0].Rows.Count == 0))
        //    {
        //        ReadMsgFromXml();				
        //    }
        //    else //DB 연결 성공 시 DB에서 읽어온다.
        //    {
        //        ReadMsgFromDB(ds);
        //    }
        //}
        #endregion

		// DB에서 메시지 읽어와 Hashtable에 담는다. 
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
				//2007.08.29 by 한주희  전체언어 메세지를 메모리에 로드시킴
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

		// Message.XML에서 메시지 읽어와 Hashtable에 담는다.	
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

        #region == 기존소스 사용하지 않음.. 별도 재정리됨.

		// DB에서 메시지 읽어와 Hashtable에 담는다.
		// 2007.10.08 by 한주희 Cs와 web에서 공통으로 사용하고 있으므로 별도의 WEB 매소드를 신규로 개발함  
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
        //        //2007.08.29 by 한주희  전체언어 메세지를 메모리에 로드시킴
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

        //// Message.XML에서 메시지 읽어와 Hashtable에 담는다.	
        //// Cs와 web에서 공통으로 사용하고 있으므로 별도의 WEB 매소드를 신규로 개발함  
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


        //// DB에서 메시지 읽어와 Hashtable에 담는다.
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
	
        //// Message.XML에서 메시지 읽어와 Hashtable에 담는다.	
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
	/// clsMsgInfo Structure - 메시지 정보 구조체
	/// C_DICTIONARY_DF 테이블 구조에 따라 MSG_TEXT_KO_KR, MSG_TEXT_EN_US
	/// </summary>
	public class clsMsgInfo
	{
		#region -- Member
		/// <summary>
		/// 메시지 그룹
		/// </summary>
		public string MsgGrp       = "";			
		/// <summary>
		/// 메시지(기본 - Logging 사용)
		/// </summary>
		public string MsgText      = "";			
		/// <summary>
		/// DB 저장여부
		/// </summary>
		public string Dbflg      	= "";			
		/// <summary>
		/// 메시지 유형
		/// </summary>
		public string MsgType   	= "";			
		/// <summary>
		/// 사용자 정의 Language Text
		/// </summary>
		public string MsgLocal     = "";		
		/// <summary>
		/// 한국어 Language Text
		/// </summary>
		 public string MsgTextKo     = "";	 
		/// <summary>
		/// 영어 Language Text
		/// </summary>
		public string MsgTextEn     = "";	
		/// <summary>
		/// 중국어 Language Text
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
	/// 메시지 타입
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
	/// 에러 정보
	/// </summary>
	public class clsErrorInfo
	{
		/// <summary>
		/// 메시지 아이디
		/// </summary>
		public string MsgID	= "";
		/// <summary>
		/// 메시지
		/// </summary>
		public string Msg	= "";
		/// <summary>
		/// 에러 상세 정보
		/// </summary>
		public string ErrorDetail	= "";
	}

	#endregion

	#region -- class LanguageType 
	/// <summary>
	/// 언어 타입
	/// </summary>
	public class LanguageType
	{
		public const string 불가리아어_불가리아                  = "BG-BG"; 	
		public const string 카탈로니아어_카탈로니아              = "CA-ES";		
		public const string 중국어_홍콩특별행정구                = "ZH-HK";		
		public const string 중국어_마카오특별행정구              = "ZH-MO";		
		public const string 중국어_중국                          = "ZH-CN";		
		public const string 중국어_간체                          = "ZH-CHS";	
		public const string 중국어_싱가포르                      = "ZH-SG";		
		public const string 중국어_대만                          = "ZH-TW";		
		public const string 중국어_번체                          = "ZH-CHT";	
		public const string 크로아티아어_크로아티아              = "HR-HR";		
		public const string 체코어_체코                          = "CS-CZ";		
		public const string 덴마크어_덴마크                      = "DA-DK";		
		public const string 디베히어_몰디브                      = "DIV-MV";	
		public const string 네덜란드어_벨기에                    = "NL-BE";		
		public const string 네덜란드어_네덜란드                  = "NL-NL";		
		public const string 영어_영국                            = "EN-GB";		
		public const string 영어_미국                            = "EN-US";		
		public const string 영어_짐바브웨                        = "EN-ZW";		
		public const string 에스토니아어_에스토니아              = "ET-EE";		
		public const string 페로스어_페로제도                    = "FO-FO";		
		public const string 페르시아어_이란                      = "FA-IR";		
		public const string 프랑스어_프랑스                      = "FR-FR";		
		public const string 갈리시아어_갈리시아                  = "GL-ES";		
		public const string 그루지야어_그루지야                  = "KA-GE";		
		public const string 독일어_오스트리아                    = "DE-AT";		
		public const string 독일어_독일                          = "DE-DE";		
		public const string 독일어_리히텐슈타인                  = "DE-LI";		
		public const string 독일어_룩셈부르크                    = "DE-LU";		
		public const string 독일어_스위스                        = "DE-CH";		
		public const string 그리스어_그리스                      = "EL-GR";		
		public const string 구자라트어_인도                      = "GU-IN";		
		public const string 히브리어_이스라엘                    = "HE-IL";		
		public const string 힌디어_인도                          = "HI-IN";		
		public const string 헝가리어_헝가리                      = "HU-HU";		
		public const string 아이슬란드어_아이슬란드              = "IS-IS";		
		public const string 인도네시아어_인도네시아              = "ID-ID";		
		public const string 이탈리아어_이탈리아                  = "IT-IT";		
		public const string 이탈리아어_스위스                    = "IT-CH";		
		public const string 일본어_일본                          = "JA-JP";		
		public const string 카나다어_인도                        = "KN-IN";		
		public const string 카자흐어_카자흐스탄                  = "KK-KZ";		
		public const string 콘칸어_인도                          = "KOK-IN";	
		public const string 한국어_한국                          = "KO-KR";		
		public const string 키르기스어_키르기스스탄              = "KY-KG";		
		public const string 라트비아어_라트비아                  = "LV-LV";		
		public const string 리투아니아어_리투아니아              = "LT-LT";		
		public const string 마케도니아어_마케도니아              = "MK-MK";		
		public const string 말레이어_브루나이                    = "MS-BN";		
		public const string 말레이어_말레이시아                  = "MS-MY";		
		public const string 마라티어_인도                        = "MR-IN";		
		public const string 몽골어_몽골                          = "MN-MN";		
		public const string 노르웨이어_복말_노르웨이             = "NB-NO";		
		public const string 노르웨이어_니노르스크_노르웨이       = "NN-NO";		
		public const string 폴란드어_폴란드                      = "PL-PL";		
		public const string 포르투갈어_브라질                    = "PT-BR";		
		public const string 포르투갈어_포르투갈                  = "PT-PT";		
		public const string 펀잡어_인도                          = "PA-IN";		
		public const string 루마니아어_루마니아                  = "RO-RO";		
		public const string 러시아어_러시아                      = "RU-RU";		
		public const string 산스크리트어_인도                    = "SA-IN";		
		public const string 세르비아어_키릴자모_세르비아         = "SR-SP-CYRL";	
		public const string 세르비아어_라틴문자_세르비아         = "SR-SP-LATN";	
		public const string 슬로바키아어_슬로바키아              = "SK-SK";		
		public const string 솔로베니아어_솔로베니아              = "SL-SI";		
		public const string 스웨덴어_핀란드                      = "SV-FI";		
		public const string 스웨덴어_스웨덴                      = "SV-SE";		
		public const string 시리아어_시리아                      = "SYR-SY";	
		public const string 타밀어_인도							 = "TA-IN";		
		public const string 텔루구어_인도						 = "TE-IN";		
		public const string 태국어_태국							 = "TH-TH";		
		public const string 터키어_터키							 = "TR-TR";		
		public const string 우크라이나어_우크라이나				 = "UK-UA";		
		public const string 우르두어_파키스탄					 = "UR-PK";		
		public const string 우즈베크어_키릴자모_우즈베키스탄	 = "UZ-UZ-CYRL";	
		public const string 우즈베크어_라틴문자_우즈베키스탄	 = "UZ-UZ-LATN";	
		public const string 베트남어_베트남						 = "VI-VN";	
	}
	#endregion
}
