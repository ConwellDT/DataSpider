using System;
using System.IO;
using Oracle.DataAccess.Client;
using System.Text;
using System.Collections;
using System.Threading;
using System.Diagnostics;

namespace CFW.Common
{
	/// <summary>
	/// TODO : static 함수 제거
	/// </summary>
	public class Logging
	{
		#region -- 변수 선언(Demon)
		private string		m_strType;
		private string		m_strMsgID;
		private long		m_lgStime;
		private string		m_strPgmID;
		private string		m_strFileID;
		private string		m_strMethod;
		private string		m_strMsg;
		private string		m_strStat;
		

		private string		m_strPath;
		private DateTime	m_dtNEW;
		private DateTime	m_dtOLD;
		#endregion

		#region -- 변수 선언(Terminal)
		private static string	m_FilePath	= "";
		private static string	m_sProgramID = "";
		private static string	m_sUserID	= "";

		private string			m_BefMsgID = "";
		private string			m_BefMsgStr= "";
		private DateTime		m_dBefLogTm;
		

		private static Mutex mutMsgLog = new Mutex();


		private static Hashtable	l_htDelTime = new Hashtable();
		private Hashtable			m_htDelTime = Hashtable.Synchronized(l_htDelTime);
		private double				m_LogSaveDays = 30;

		#endregion 

		#region -- 변수 선언(CS)
		private string m_QueuePath = "";
		private string m_QueueSize = "";
		#endregion


		#region -- Logging생성자
		/// <summary>
		/// 생성자입니다.
		/// </summary>
		public Logging()
		{
			m_dtOLD = DateTime.Now;
			//
			// TODO: 여기에 생성자 논리를 추가합니다.
			//
		}
		#endregion

		#region -- Property 

		/// <summary>
		/// 로그 파일 경로를 설정하거나 값을 읽어올 수 있습니다.
		/// </summary>
		/// <param name="filePath">로그 파일 경로</param>
		public void UpdateFilePath(string filePath)      // (IN) LOG File Path
		{
			m_FilePath    = filePath;
		}	

		/// <summary>
		/// 로그 파일 경로를 읽어옵니다.
		/// </summary>
		public string LogFilePath
		{
			get
			{
				string strReturn = "";
				if( m_FilePath != null & m_FilePath.Length > 0)
					strReturn = m_FilePath;
				else
				{
                    strReturn = CFW.Configuration.ConfigManager.Default.ReadConfig("LOG", "LOG_PATH");
				}

				if( strReturn.EndsWith(@"\") == false)
					strReturn += @"\";

				m_FilePath = strReturn;

				return m_FilePath;
			}
		}

		/// <summary>
		/// 사용자 아이디를 설정하거나 읽어올 수 있습니다.
		/// </summary>
		public string UserID
		{
			get
			{
				if(m_sUserID != null && m_sUserID.Length > 0)
					return m_sUserID;
				else
				{
					m_sUserID = System.Net.Dns.GetHostName();
				}
				return m_sUserID;
			}
			set{ m_sUserID = (string)value; }
		}



		/// <summary>
		/// DB로그 큐 사이즈를 읽어옵니다.
		/// </summary>
		public string QueueSize
		{
			get
			{
				if(m_QueueSize != null && m_QueueSize.Length > 0)
					return m_QueueSize;
				else
				{
                    m_QueueSize = CFW.Configuration.ConfigManager.Default.ReadConfig("MSMQ", "DBLOG_QUEUE_SIZE");
				}
				return m_QueueSize;
			}
		}

		/// <summary>
		/// DB로그 큐 이름을 읽어옵니다.
		/// </summary>
        //public string DBLogQueueName
        //{
        //    get
        //    {
        //        if(m_QueuePath != null && m_QueuePath.Length > 0)
        //            return m_QueuePath;
        //        else
        //        {
        //            string strQueueName = CFW.Configuration.ConfigManager.Default.ReadConfig("MSMQ", "DBLOG_QUEUE_NAME");
        //            m_QueuePath = CFW.Msmq.Sender.MakeQueuePath(strQueueName);
        //        }
        //        return m_QueuePath;
        //    }
        //}

		/// <summary>
		/// 로그 저장 일수를 설정하거나 값을 읽어올 수 있습니다.
		/// </summary>
		public int	LogSaveDays		
		{
			get
			{
                string strValue = CFW.Configuration.ConfigManager.Default.ReadConfig("LOG", "LOG_SAVE_DAYS");	
				
				if( strValue != null && strValue.Length > 0)
					m_LogSaveDays = int.Parse(strValue);
				return (int)m_LogSaveDays; 
			} 
			
			set{ m_LogSaveDays = (double)value; }
		}

		#endregion Property


		//Write EventView=======================================

		#region -- 웹 로깅 (이벤트 표시기)

		/// <summary>
		/// 로그를 이벤트 표시기에 씁니다.
		/// </summary>
		/// <param name="eventLog">"응용 프로그램", "보안", "시스템" 이 아닌 다른 위치로 지정할 수 있습니다.</param>
		/// <param name="eventLogSource">이벤트 소스명입니다.</param>
		/// <param name="logType">이벤트 종류입니다.정보/경고/오류 등의 카테고리가 있습니다.</param>
		/// <param name="contents">로그 내용입니다.</param>
		public static void WriteEventLog(string eventLog, string eventLogSource, System.Diagnostics.EventLogEntryType logType, string contents)
		{
			System.Diagnostics.EventLog eLog = null;

			try
			{
				if (!(System.Diagnostics.EventLog.SourceExists(eventLogSource)))
					System.Diagnostics.EventLog.CreateEventSource(eventLogSource, eventLog);

				eLog = new System.Diagnostics.EventLog(eventLog);
				eLog.Source = eventLogSource;
				eLog.WriteEntry(contents, logType);
			}
			finally
			{
				if (eLog != null) eLog.Dispose();
			}
		}

		#endregion 		

		//메시지 조합 ====================================

		#region -- Log/Display 용 메시지 조합

		#region 화면에 Display 될 형식에 맞추어 문자 생성

		/// <summary>
		/// 화면에 Display 될 형식에 맞추어 문자 생성
		/// </summary>
		/// <returns>Display 생성 문자</returns>
		private string MakeDisplayData(string msgID, clsMsgInfo msgInfo, clsProgramLogInfo programLogInfo
			, clsSourceInfo sourceInfo, string[] arg, System.DateTime dt)
		{
			string strType = msgInfo.MsgType;
			System.Text.StringBuilder sb = new StringBuilder();

			//일자
			sb.AppendFormat("{0:yyyy-MM-dd}", dt.Date);
			sb.Append(CFW.Common.Constant.DATA_DELIMITER);
			//시간
			sb.AppendFormat("{0:D2}:{1:D2}:{2:D2}", dt.Hour, dt.Minute, dt.Second);
			sb.Append(CFW.Common.Constant.DATA_DELIMITER);
			//프로그램 ID
			sb.Append(programLogInfo.ProgramID );
			sb.Append(CFW.Common.Constant.DATA_DELIMITER);
			//프로세스 ID
			sb.Append(programLogInfo.ProcessID );
			sb.Append(CFW.Common.Constant.DATA_DELIMITER);
			
			//메시지 Level
			if ( strType != null ) 
			{
				if( msgInfo.MsgType == MSG.LEVEL_INFO  )	sb.Append("     ");
				else if( strType == MSG.LEVEL_INFO  )	sb.Append("INFO ");
				else if( strType == MSG.LEVEL_ALARM )	sb.Append("ALRAM");
				else if( strType == MSG.LEVEL_ERROR )	sb.Append("ERROR");
			}
			sb.Append(CFW.Common.Constant.DATA_DELIMITER);
			
			//Msg ID
			sb.Append(msgID);
			sb.Append(CFW.Common.Constant.DATA_DELIMITER);	

			//Msg Text
			sb.Append(msgInfo.MsgLocal);
			sb.Append(CFW.Common.Constant.DATA_DELIMITER);	

			//Source Info
			sb.Append(sourceInfo.File );
			sb.Append(".");
			sb.Append(sourceInfo.Method);
			sb.Append("(");
			sb.Append(sourceInfo.LineNumber);
			sb.Append(")");
			sb.Append(CFW.Common.Constant.DATA_DELIMITER);

			return sb.ToString();
		}

		#endregion Display 될 형식에 맞추어 문자 생성

		#region 형식에 상관없이 데이터를 구분자를 이용하여 string 생성(DB Log)
		
		/// <summary>
		/// 형식에 상관없이 데이터를 구분자를 이용하여 string 생성 
		/// </summary>
		/// <param name="msgID">메시지 ID</param>
		/// <param name="msgInfo">메시지 정보</param>
		/// <param name="programLogInfo">프로그램 정보</param>
		/// <param name="sourceInfo">소스 정보</param>
		/// <param name="arg">추가 메시지</param>
		/// <param name="dt">시스템 시간</param>
		/// <returns></returns>
		private string MakeLogData(string msgID, clsMsgInfo msgInfo, clsProgramLogInfo programLogInfo
			, clsSourceInfo sourceInfo, string[] arg, System.DateTime dt)
		{
			string strType = msgInfo.MsgType;
			System.Text.StringBuilder sb = new StringBuilder();

			sb.Append("L");
			sb.Append(CFW.Common.Constant.DATA_DELIMITER);

			//일자
			sb.AppendFormat("{0:yyyyMMdd}", dt.Date);
			sb.Append(CFW.Common.Constant.DATA_DELIMITER);

			//시간
			sb.AppendFormat("{0:D2}{1:D2}{2:D2}", dt.Hour, dt.Minute, dt.Second);
			sb.Append(CFW.Common.Constant.DATA_DELIMITER);

			//프로그램 ID
			sb.Append(programLogInfo.ProgramID );
			sb.Append(CFW.Common.Constant.DATA_DELIMITER);

			//프로세스 ID
			sb.Append(programLogInfo.ProcessID );
			sb.Append(CFW.Common.Constant.DATA_DELIMITER);

			//Msg Level 
			sb.Append(msgInfo.MsgType);
			sb.Append(CFW.Common.Constant.DATA_DELIMITER);//1
			
			//Msg ID
			sb.Append(msgID);
			sb.Append(CFW.Common.Constant.DATA_DELIMITER);//2

			//Msg Text
			sb.Append(msgInfo.MsgText);
			sb.Append(CFW.Common.Constant.DATA_DELIMITER);//3	

			//USER 
			sb.Append(UserID);
			sb.Append(CFW.Common.Constant.DATA_DELIMITER);//4	

			//Source Info
			sb.Append(sourceInfo.File );
			sb.Append(".");
			sb.Append(sourceInfo.Method);
			sb.Append("(");
			sb.Append(sourceInfo.LineNumber);
			sb.Append(")");
			sb.Append(CFW.Common.Constant.DATA_DELIMITER);//5
			sb.Append(CFW.Common.Constant.DATA_DELIMITER);//6
			sb.Append(CFW.Common.Constant.DATA_DELIMITER);//7
			sb.Append(CFW.Common.Constant.DATA_DELIMITER);//8
			sb.Append(CFW.Common.Constant.DATA_DELIMITER);//9
			sb.Append(CFW.Common.Constant.DATA_DELIMITER);//10
			sb.Append(CFW.Common.Constant.DATA_DELIMITER);//11
			sb.Append(CFW.Common.Constant.DATA_DELIMITER);//12
			sb.Append(CFW.Common.Constant.DATA_DELIMITER);//13
			sb.Append(CFW.Common.Constant.DATA_DELIMITER);//14
			sb.Append(CFW.Common.Constant.DATA_DELIMITER);//15
			sb.Append(CFW.Common.Constant.DATA_DELIMITER);//16
			sb.Append(CFW.Common.Constant.DATA_DELIMITER);//17
			sb.Append(CFW.Common.Constant.DATA_DELIMITER);//18
			sb.Append(CFW.Common.Constant.DATA_DELIMITER);//19
			sb.Append(CFW.Common.Constant.DATA_DELIMITER);//20

			return sb.ToString();
		}

		#endregion  형식에 상관없이 데이터를 구분자를 이용하여 string 생성

		#region 형식에 상관없이 데이터를 구분자를 이용하여 string 생성(FileLog)

		/// <summary>
		/// 형식에 상관없이 데이터를 구분자를 이용하여 string 생성
		/// </summary>
		/// <param name="msgID">메시지 ID</param>
		/// <param name="msgInfo">메시지 정보</param>
		/// <param name="programLogInfo">프로그램 정보</param>
		/// <param name="sourceInfo">소스 정보</param>
		/// <param name="arg">추가 메시지</param>
		/// <param name="dt">시스템 시간</param>
		/// <returns></returns>
		private string MakeFileLogData(string msgID, clsMsgInfo msgInfo, clsProgramLogInfo programLogInfo
			, clsSourceInfo sourceInfo, string[] arg, System.DateTime dt)
		{
			string strType = msgInfo.MsgType;
			System.Text.StringBuilder sb = new StringBuilder();


			//일자
			sb.AppendFormat("{0:yyyyMMdd}", dt.Date);//1
			sb.Append(CFW.Common.Constant.DATA_DELIMITER);

			//시간
			sb.AppendFormat("{0:D2}{1:D2}{2:D2}", dt.Hour, dt.Minute, dt.Second);//2
			sb.Append(CFW.Common.Constant.DATA_DELIMITER);

			//프로그램 ID
			sb.Append(programLogInfo.ProgramID );//3
			sb.Append(CFW.Common.Constant.DATA_DELIMITER);

			//프로세스 ID
			sb.Append(programLogInfo.ProcessID );//4
			sb.Append(CFW.Common.Constant.DATA_DELIMITER);

			//Source Info

			int nIdx =sourceInfo.File.LastIndexOf('\\');
			string strTemp = sourceInfo.File.Substring(nIdx+1);
			sb.Append(strTemp );
			sb.Append(".");
			sb.Append(sourceInfo.Method);
			sb.Append("(");
			sb.Append(sourceInfo.LineNumber);
			sb.Append(")");
			sb.Append(CFW.Common.Constant.DATA_DELIMITER);//5

			//Msg Level 
			sb.Append(msgInfo.MsgType);
			sb.Append(CFW.Common.Constant.DATA_DELIMITER);//1
			
			//Msg ID
			sb.Append(msgID);
			sb.Append(CFW.Common.Constant.DATA_DELIMITER);//2

			//Msg Text
			sb.Append(msgInfo.MsgText);
			return sb.ToString();
		}

		#endregion  형식에 상관없이 데이터를 구분자를 이용하여 string 생성

		#region 추가된 정보로 Msg 정보 Update

		/// <summary>
		/// 추가된 정보로 Msg 정보 Update
		/// </summary>
		/// <param name="msgInfo">메시지 정보</param>
		/// <param name="args">추가 정보</param>
		/// <param name="type">메시지 레벨</param>
		private void UpdateMsgInfo(ref clsMsgInfo msgInfo, string[] args, string type)
		{
			string strFormat = "";
			string strFormatLocal = "";

			System.Text.StringBuilder sbMsg = new StringBuilder();
			System.Text.StringBuilder sbFormat = new StringBuilder();


		if(msgInfo == null)
			{  
				msgInfo = new clsMsgInfo();
				msgInfo.Dbflg = "N";
				for(int idx=0; idx<args.Length; idx++)
				{
					if(idx==0)
					{
						sbFormat.Append("NO-EXIST-MESSAGE");
					}
					sbFormat.Append("[{");
					sbFormat.Append(idx);
					sbFormat.Append("}]");
				}

				strFormat = sbFormat.ToString();
				strFormatLocal = strFormat;
			}
			else
			{
				strFormat = msgInfo.MsgText;
				strFormatLocal = msgInfo.MsgLocal;
			}

			//임의로 Type을 변경한 경우 DB의 값을 Update
			if(type != null && type.Trim() != MSG.LEVEL_NONE)
				msgInfo.MsgType = type;

			try
			{
				msgInfo.MsgText = string.Format(strFormat, args);
				
			}
			catch
			{
				for(int idx=0; idx<args.Length; idx++)
				{
					if(idx==0)
					{
						sbMsg.Append("WRONG-FORMAT-MESSAGE");
					}
					sbMsg.Append("[{");
					sbMsg.Append(args[idx]);
					sbMsg.Append("}]");
				}
				msgInfo.MsgText = strFormat + sbMsg.ToString();
			}

			try
			{
				msgInfo.MsgLocal = string.Format(strFormatLocal, args);
			}
			catch
			{
				if( sbMsg.Length <= 0)
				{
					for(int idx=0; idx<args.Length; idx++)
					{
						if(idx==0)
						{
							sbMsg.Append("WRONG-FORMAT-MESSAGE");
						}
						sbMsg.Append("[{");
						sbMsg.Append(args[idx]);
						sbMsg.Append("}]");
					}
				}
				msgInfo.MsgLocal = strFormatLocal + sbMsg.ToString();
			}
		}

		#endregion 추가된 정보로 Msg 정보 Update

		#endregion 메시지 조합
		
		//LogToFile ====================================

		#region -- 데몬 로그 파일 쓰기

		/// <summary>
		/// 로그를 파일에 씁니다.
		/// </summary>
		/// <param name="logMsg">Log Message</param>
		public void LogToFile(string logMsg)
		{
			string strLogMsg;
                        
			DirectoryInfo   dirInfo = new DirectoryInfo( LogFilePath + m_strPgmID);

			if( !dirInfo.Exists )   dirInfo.Create();

			DateTime dt1 = DateTime.Now;
			m_strPath = string.Format(@"{0}\{1}_{2}.TXT", dirInfo.ToString(), m_strPgmID, ((int)dt1.DayOfWeek).ToString());
	
			if( File.Exists(LogFilePath) )
			{
				DateTime dt2 = Directory.GetCreationTime(LogFilePath);
				if( dt1.Date != dt2.Date )
				{
					File.SetCreationTime(LogFilePath, DateTime.Now);
					File.Delete(LogFilePath);
				}
			}
            
            strLogMsg = logMsg;

			using(FileStream fs = new FileStream(LogFilePath, FileMode.OpenOrCreate, FileAccess.ReadWrite))
			{
				using(StreamWriter sw = new StreamWriter(fs, Encoding.Default))
				{
					sw.BaseStream.Seek(0, SeekOrigin.End);
					sw.WriteLine(strLogMsg);
					sw.Flush();
					sw.Close();
				}
			}
		}


		#endregion

		#region -- 파일 삭제(삭제용 Thread 생성)


		/// <summary>
		/// 기존 파일 중 날짜 범위에서 벗어나는 파일 삭제
		/// </summary>
		/// <param name="programID"></param>
		/// <param name="processID">프로세스 ID</param>
		/// <param name="filePath">파일 경로</param>
		/// <param name="logSaveDays">날짜 범위(day)</param>
		private void DeleteOldFile(string programID, string processID, string filePath, int logSaveDays)
		{
			string strProcessID = processID;

			System.Text.StringBuilder  sbFilePath = new StringBuilder();
			sbFilePath.Append(filePath);

		
			int dblLogSaveDays = logSaveDays;
			DateTime dLogTime = DateTime.Now;

			string strNewDate = string.Format("{0:yyyyMMdd}", dLogTime.Date);

			if(!m_htDelTime.Contains(strProcessID))
			{
				m_htDelTime.Add(strProcessID, strNewDate);
				clsDelLogFile pLogDel = new clsDelLogFile(sbFilePath.ToString(), strProcessID, dblLogSaveDays);
				System.Threading.Thread m_thrDel = new System.Threading.Thread( new System.Threading.ThreadStart(pLogDel.DelProcLogFile) );
				m_thrDel.Start();
			}
			else
			{
				string strDeleteDate = (string)m_htDelTime[strProcessID];
				if( !strNewDate.Equals(strDeleteDate) )
				{
					m_htDelTime[strProcessID] = strNewDate;
					clsDelLogFile pLogDel = new clsDelLogFile(sbFilePath.ToString(), strProcessID, dblLogSaveDays);
					System.Threading.Thread m_thrDel = new System.Threading.Thread( new System.Threading.ThreadStart(pLogDel.DelProcLogFile) );
					m_thrDel.Start();
				}
			}

					
		}


		#endregion 파일 삭제
		
		#region -- 파일에 쓰기

		/// <summary>
		/// 구분자로 구성된 String File에 쓰기
		/// 
		/// 구분자	: CFW.Common.Constant.DATA_DELIMITER
		/// 폴더명	: Parameter filePath + 구분자를 기준으로 3번째(zero base)값 (없으면 Unknown)
		/// 파일명	: 구분자를 기준으로 4번째값 (없으면 Unknown)
		///			  + _구분자를 기준으로 0번째 값 : LOG(L) / Status(나머지) 
		///			  + _현재 일자.txt
		///	내용	: Parameter dataString
		/// </summary>
		/// <param name="dataString">구분자로 이루어진 데이터</param>
		/// <param name="filePath">파일 저장 경로</param>
		public void WriteStringDataToFile(string dataString, string filePath)
		{

			System.IO.FileStream oFs = null;
			System.IO.StreamWriter oWriter = null;

			System.Text.StringBuilder sb = null;
			string strPath = "";
			string fPath = "";
			string[] arrLogInfo = null;
			int iCount = 0;

			try
			{
				arrLogInfo = dataString.Split(CFW.Common.Constant.DATA_DELIMITER.ToCharArray());
				iCount = arrLogInfo.Length;

				sb = new System.Text.StringBuilder();

				//파일 폴더 경로
				sb.Append(filePath);

				strPath = sb.ToString(); 

				sb.Append("\\");

				//파일명
				if(iCount > 3)
					sb.Append(arrLogInfo[3]);
				else
					sb.Append("Unknown");

				sb.Append("_");

				sb.Append("LOG");
				
				sb.Append("_");
				sb.Append(System.DateTime.Now.Year.ToString());
				sb.Append(System.DateTime.Now.Month.ToString("00"));
				sb.Append(System.DateTime.Now.Day.ToString("00"));
				sb.Append(".txt");

				fPath = sb.ToString();
				
						
				if ( !System.IO.Directory.Exists(strPath) )
					System.IO.Directory.CreateDirectory(strPath);

				oFs = new System.IO.FileStream(fPath, FileMode.Append,FileAccess.Write,FileShare.Write);
				oWriter = new System.IO.StreamWriter(oFs, System.Text.Encoding.Default);

				oWriter.WriteLine(dataString);
				
				oWriter.Flush();
				oWriter.Close();

			}
			catch(Exception ex)
			{
				throw ex;
			}
			finally
			{
				if ( oWriter != null )
				{
					oWriter.Close();
				}
				if ( oFs != null )
				{
					oFs.Close();
				}
			}
		}


		#endregion 파일에 쓰기


        //터미널 로그 파일 2015.01.09

        public void LogToFile(string FileType, string FileName, string p_strStat, string p_strExplain, string p_strLogMsg)
        {
            string strLogMsg;
            string strLogFile;
            string LogPath = System.Configuration.ConfigurationSettings.AppSettings["LOG_PATH"];

            DateTime dt1 = DateTime.Now;
            strLogFile = string.Format(LogPath + @"\{0}_{1}_{2}.TXT", FileType, FileName, dt1.ToString("yyyyMMdd"));


            if (p_strExplain != "")
            {
                strLogMsg = string.Format("[{0:D4}/{1:D2}/{2:D2} {3:D2}:{4:D2}:{5:D2}] [{6}] - [{7}] {8}",
                                            dt1.Year, dt1.Month, dt1.Day, dt1.Hour, dt1.Minute, dt1.Second, p_strStat, p_strExplain, p_strLogMsg);
            }
            else
            {
                strLogMsg = string.Format("[{0:D4}/{1:D2}/{2:D2} {3:D2}:{4:D2}:{5:D2}] [{6}] - {7}",
                                            dt1.Year, dt1.Month, dt1.Day, dt1.Hour, dt1.Minute, dt1.Second, p_strStat, p_strLogMsg);
            }

            using (FileStream fs = new FileStream(strLogFile, FileMode.OpenOrCreate, FileAccess.ReadWrite))
            {
                using (StreamWriter sw = new StreamWriter(fs, Encoding.Default))
                {
                    sw.BaseStream.Seek(0, SeekOrigin.End);
                    sw.WriteLine(strLogMsg);
                    sw.Flush();
                    sw.Close();
                }
            }
        }

		//Log Agent 전달 ====================================
		
		#region == 터미널 로깅[Multi Thread 환경](기존) ==

		/// <summary>
		/// 터미널 프로그램 구동 시 Multi Thread 환경에서 로깅을 남김니다. 
		/// </summary>
		/// <param name="type">메시지유형</param>
		/// <param name="msgID">메시지 아이디</param>
		/// <param name="programLogInfo">DB연결Hint, 로깅 간격(초), ProgramID, ProcessID</param>
		/// <param name="sourceInfo">로깅 소스 정보 구조체</param>
		/// <param name="args">추가 파라메터</param>
		/// <returns>메시지 구조체</returns>
		/// <example>
		/// CFW.Common.Logging.WriteLogInMultiThread(pEventInfo.Level,pEventInfo.MsgID, 3, pEventInfo.ProgramID,
		/// pEventInfo.ProcessID,pLogWrite.GetSouceInfo(),m_languageType);
		/// </example>
		/// <returns></returns>
		public clsMessage WriteLogInMultiThread(string type, string msgID, clsProgramLogInfo programLogInfo
												, clsSourceInfo sourceInfo,	 params string[] args)
		{
//            DateTime dLogTime = DateTime.Now;
//            string strDataString = "";
//            string strFilePath = LogFilePath;
//            int iLogSaveDays = LogSaveDays; // TODO : Config 에서 읽어야 함.
			
//            // 프로그램 ID 확인
//            if(programLogInfo.ProgramID.Length <=0)
//                programLogInfo.ProgramID = m_sProgramID;

//            // Update Msg Info
//            clsMsgInfo msgInfo = CFW.Common.Messaging.SearchMsgStr(msgID);
//            UpdateMsgInfo(ref msgInfo, args, type);

//            // Diplay 용 정보 생성
            clsMessage pMsg = new clsMessage();
//            pMsg.LogTime	= dLogTime; //로그 남기는 시간 Term
//            pMsg.ProgramID	= programLogInfo.ProgramID;
//            pMsg.ProcessID	= programLogInfo.ProcessID;
//            pMsg.Method		= sourceInfo.Method;
//            pMsg.MsgID		= msgID;
//            pMsg.UserID		= m_sUserID;

//            pMsg.MsgText    = msgInfo.MsgLocal;//MakeDisplayData(msgID, msgInfo, programLogInfo, sourceInfo, args, dLogTime);
//            pMsg.Level		= msgInfo.MsgType;
			
//            /* 	- 모든 메시지에 대하여 중복 체크
//                - 파일 삭제
//                - 파일 로그 
//                - DB Log Flag가 Y 인 경우 MQ로 전송
//            */
//            if( !m_BefMsgID.Equals(msgID) || !m_BefMsgStr.Equals(pMsg.MsgText) || 
//                Convert.ToInt32((dLogTime - m_dBefLogTm).TotalSeconds) > programLogInfo.LogTerm )
//            {

//                // 파일 삭제
//                DeleteOldFile(pMsg.ProgramID, pMsg.ProcessID, strFilePath, iLogSaveDays);
				
//                // DB 로그 string 만들기
//                strDataString = MakeLogData(msgID, msgInfo, programLogInfo, sourceInfo, args, dLogTime);

//                // 파일 로그 string 만들기
//                string strFileLogData = "";
//                strFileLogData = MakeFileLogData(msgID, msgInfo, programLogInfo, sourceInfo, args, dLogTime);


//                #region 파일 로그 (Multi Thread)

//                try
//                {

//                    mutMsgLog.WaitOne();

//                    WriteStringDataToFile(strFileLogData, strFilePath);

//                    mutMsgLog.ReleaseMutex();

//                }
//                finally
//                {
////					if(mutMsgLog.GetLifetimeService() != null) 
////					{
////						mutMsgLog.ReleaseMutex();
////					}
//                }

//                #endregion 파일 로그  (Multi Thread)

//                #region MSMQ로 데이터 전송

//                if(msgInfo.Dbflg.Equals("Y")) // DB Log Flag 확인하여 MQ로 전송
//                {
//                    // MSMQ Method 호출
//                    CFW.Msmq.clsMQMessage qMsg = new CFW.Msmq.clsMQMessage();
//                    qMsg.Label = programLogInfo.DBInfo;

//                    //파일 로그할 string
//                    qMsg.Body = strDataString;
		
//                    if( DBLogQueueName.Length > 0 )
//                    {
//                        CFW.Msmq.Sender.SendMessage(qMsg, DBLogQueueName, QueueSize);
//                    }
//                    else
//                    {
//                        pMsg.MsgText += "There is no a config file.";
//                    }
//                }

//                #endregion MSMQ로 데이터 전송
				
				
//            }
			
//            // 중복 체크 하기 위해 전역 변수에 값 저장
//            m_dBefLogTm = dLogTime;
//            m_BefMsgID  = msgID;
//            m_BefMsgStr = pMsg.MsgText;

            return pMsg;
		}
		#endregion

		#region == 터미널 로깅[Multi Thread 환경](Message Level 삭제) ==
		
		/// <summary>
		/// 터미널 프로그램 구동 시 Multi Thread 환경에서 로깅을 남김니다. 
		/// </summary>
		/// <param name="msgID">메시지 아이디</param>
		/// <param name="programLogInfo">DB연결Hint, 로깅 간격(초), ProgramID, ProcessID</param>
		/// <param name="sourceInfo">로깅 소스 정보 구조체</param>
		/// <param name="args">추가 파라메터</param>
		/// <returns></returns>
		public clsMessage WriteLogInMultiThread(string msgID, clsProgramLogInfo programLogInfo
			, clsSourceInfo sourceInfo,	 params string[] args)
		{
            //DateTime dLogTime = DateTime.Now;
            //string strDataString = "";
            //string strFilePath = LogFilePath;
            //int iLogSaveDays = LogSaveDays; // TODO : Config 에서 읽어야 함.
			
            //// 프로그램 ID 확인
            //if(programLogInfo.ProgramID.Length <=0)
            //    programLogInfo.ProgramID = m_sProgramID;

            //// Update Msg Info
            //clsMsgInfo msgInfo = CFW.Common.Messaging.SearchMsgStr(msgID);
            //UpdateMsgInfo(ref msgInfo, args, "");

            //// Diplay 용 정보 생성
            clsMessage pMsg = new clsMessage();
            //pMsg.LogTime	= dLogTime; //로그 남기는 시간 Term
            //pMsg.ProgramID	= programLogInfo.ProgramID;
            //pMsg.ProcessID	= programLogInfo.ProcessID;
            //pMsg.Method		= sourceInfo.Method;
            //pMsg.MsgID		= msgID;
            //pMsg.UserID		= m_sUserID;

            //pMsg.MsgText    = msgInfo.MsgLocal;//MakeDisplayData(msgID, msgInfo, programLogInfo, sourceInfo, args, dLogTime);
            //pMsg.Level		= msgInfo.MsgType;
			
            ///* 	- 모든 메시지에 대하여 중복 체크
            //    - 파일 삭제
            //    - 파일 로그 
            //    - DB Log Flag가 Y 인 경우 MQ로 전송
            //*/
            //if( !m_BefMsgID.Equals(msgID) || !m_BefMsgStr.Equals(pMsg.MsgText) || 
            //    Convert.ToInt32((dLogTime - m_dBefLogTm).TotalSeconds) > programLogInfo.LogTerm )
            //{

            //    // 파일 삭제
            //    DeleteOldFile(pMsg.ProgramID, pMsg.ProcessID, strFilePath, iLogSaveDays);
				
            //    // DB 로그 string 만들기
            //    strDataString = MakeLogData(msgID, msgInfo, programLogInfo, sourceInfo, args, dLogTime);

            //    // 파일 로그 string 만들기
            //    string strFileLogData = "";
            //    strFileLogData = MakeFileLogData(msgID, msgInfo, programLogInfo, sourceInfo, args, dLogTime);


            //    #region 파일 로그 (Multi Thread)

            //    try
            //    {

            //        mutMsgLog.WaitOne();

            //        WriteStringDataToFile(strFileLogData, strFilePath);

            //        mutMsgLog.ReleaseMutex();

            //    }
            //    finally
            //    {
            //        //					if(mutMsgLog.GetLifetimeService() != null) 
            //        //					{
            //        //						mutMsgLog.ReleaseMutex();
            //        //					}
            //    }

            //    #endregion 파일 로그  (Multi Thread)

            //    #region MSMQ로 데이터 전송

            //    if(msgInfo.Dbflg.Equals("Y")) // DB Log Flag 확인하여 MQ로 전송
            //    {
            //        // MSMQ Method 호출
            //        CFW.Msmq.clsMQMessage qMsg = new CFW.Msmq.clsMQMessage();
            //        qMsg.Label = programLogInfo.DBInfo;

            //        //파일 로그할 string
            //        qMsg.Body = strDataString;
		
            //        if( DBLogQueueName.Length > 0 )
            //        {
            //            CFW.Msmq.Sender.SendMessage(qMsg, DBLogQueueName, QueueSize);
            //        }
            //        else
            //        {
            //            pMsg.MsgText += "There is no a config file.";
            //        }
            //    }

            //    #endregion MSMQ로 데이터 전송
				
				
            //}
			
            //// 중복 체크 하기 위해 전역 변수에 값 저장
            //m_dBefLogTm = dLogTime;
            //m_BefMsgID  = msgID;
            //m_BefMsgStr = pMsg.MsgText;
			
			return pMsg;

		}
		#endregion


		#region == 데몬 로깅(기존) ==

		/// <summary>
		/// 데몬 프로그램 구동 시 로깅을 남김니다.
		/// </summary>
		/// <param name="type">메시지유형</param>
		/// <param name="msgID">메시지 아이디</param>
		/// <param name="programLogInfo">DB연결Hint, 로깅 간격(초), ProgramID, ProcessID</param>
		/// <param name="sourceInfo">로깅 소스 정보 구조체</param>
		/// <param name="args">추가 파라메터</param>
		/// <returns>메시지 문자열</returns>
		/// <example>
		/// CFW.Common.Logging.WriteLog("E", "AC8113", m_programLogInfo,m_FileLog.GetSouceInfo(),);
		/// </example>
		/// <returns></returns>
		public string WriteLog(string type, string msgID, clsProgramLogInfo programLogInfo,	clsSourceInfo sourceInfo, params string[] args)
		{

			string strDisplayData = "";
			DateTime dLogTime = DateTime.Now;
			string strDataString = "";
			string strFilePath = LogFilePath;
			string strReturnMsg = "";
			int iLogSaveDays = LogSaveDays; // TODO : Config 에서 읽어야 함.

			// 프로그램 ID 확인
			if(programLogInfo.ProgramID.Length <=0)
				programLogInfo.ProgramID = m_sProgramID;

			// Update Msg Info
			clsMsgInfo msgInfo = CFW.Common.Messaging.SearchMsgStr(msgID);
			UpdateMsgInfo(ref msgInfo, args, type);

			strReturnMsg = msgInfo.MsgLocal;
			strDisplayData = MakeDisplayData(msgID, msgInfo, programLogInfo, sourceInfo, args, dLogTime);

			strDataString = MakeLogData(msgID, msgInfo, programLogInfo, sourceInfo, args, dLogTime);

			// TODO : 파일 삭제/ 파일 쓰기 방식으로 Test 필요
			// 파일 삭제
			DeleteOldFile(programLogInfo.ProgramID, programLogInfo.ProcessID, strFilePath, iLogSaveDays);
			//파일 쓰기
			WriteStringDataToFile(strDataString, strFilePath);


			if( !m_BefMsgID.Equals(msgID) || !m_BefMsgStr.Equals(msgInfo.MsgText) || 
				Convert.ToInt32((dLogTime - m_dBefLogTm).TotalSeconds) > programLogInfo.LogTerm )
			{

				
				
				#region [주석] 데몬 파일 삭제 후 저장하는 방식
				
				//기존 데몬에서 파일 삭제후 저장하는 방식
				//m_strPgmID	= programLogInfo.ProgramID;
				//LogToFile(strDataString); 

				#endregion
				
				#region MSMQ로 데이터 전송

                //if(msgInfo.Dbflg.Equals("Y")) // DB Log Flag 확인하여 MQ로 전송
                //{
                //    // MSMQ Method 호출
                //    CFW.Msmq.clsMQMessage qMsg = new CFW.Msmq.clsMQMessage();
                //    qMsg.Label = programLogInfo.DBInfo;

                //    //파일 로그할 string
                //    qMsg.Body = strDataString;
		
                //    if( DBLogQueueName.Length > 0 )
                //    {
                //        CFW.Msmq.Sender.SendMessage(qMsg, DBLogQueueName, QueueSize);
                //    }
                //    else
                //    {
                //        strDisplayData += "There is no a config file.";
                //    }
                //}

				#endregion MSMQ로 데이터 전송
			}

			// 중복 체크 하기 위해 전역 변수에 값 저장
			m_dBefLogTm = dLogTime;
			m_BefMsgID  = msgID;
			m_BefMsgStr = msgInfo.MsgText;

			return strReturnMsg;
			
		}


		/// <summary>
		/// 데몬 프로그램 구동 시 로깅을 남김니다.
		/// </summary>
		/// <param name="type">메시지유형</param>
		/// <param name="msgID">메시지 아이디</param>
		/// <param name="programLogInfo">DB연결Hint, 로깅 간격(초), ProgramID, ProcessID</param>
		/// <param name="sourceInfo">로깅 소스 정보 구조체</param>
		/// <returns></returns>
		/// <remarks>프로세스 아이디 및 추가 파라메터 추가 하여 Main WriteLog메소드 호출</remarks>
		public string WriteLog(string type, string msgID, clsProgramLogInfo programLogInfo, clsSourceInfo sourceInfo)
		{
			string[] args = new string[0];
			return WriteLog( type, msgID, programLogInfo, sourceInfo, args);//  (Additional message parameter)
		}

		#endregion

		#region == 데몬 로깅(Message Level 삭제) ==
		/// <summary>
		/// 데몬 프로그램 구동 시 로깅을 남김니다.
		/// </summary>
		/// <param name="msgID">메시지 아이디</param>
		/// <param name="programLogInfo">DB연결Hint, 로깅 간격(초), ProgramID, ProcessID</param>
		/// <param name="sourceInfo">로깅 소스 정보 구조체</param>
		/// <returns></returns>
		public string WriteLog(string msgID, clsProgramLogInfo programLogInfo, clsSourceInfo sourceInfo)
		{
			string[] args = new string[0];
			return WriteLog( msgID, programLogInfo, sourceInfo, args);//  (Additional message parameter)
		}


		/// <summary>
		/// 데몬 프로그램 구동 시 로깅을 남김니다.
		/// </summary>
		/// <param name="msgID">메시지 아이디</param>
		/// <param name="programLogInfo">DB연결Hint, 로깅 간격(초), ProgramID, ProcessID</param>
		/// <param name="sourceInfo">로깅 소스 정보 구조체</param>
		/// <param name="args">추가 파라미터</param>
		/// <returns></returns>
		public string WriteLog(string msgID, clsProgramLogInfo programLogInfo, clsSourceInfo sourceInfo ,params string[] args)
		{
			string strDisplayData = "";
			DateTime dLogTime = DateTime.Now;
			string strDataString = "";
			string strFilePath = LogFilePath;
			string strReturnMsg = "";
			int iLogSaveDays = LogSaveDays; // TODO : Config 에서 읽어야 함.

			// 프로그램 ID 확인
			if(programLogInfo.ProgramID.Length <=0)
				programLogInfo.ProgramID = m_sProgramID;

			// Update Msg Info
			clsMsgInfo msgInfo = CFW.Common.Messaging.SearchMsgStr(msgID);
			UpdateMsgInfo(ref msgInfo, args, "");
			clsMsgInfo msgInfo1 = CFW.Common.Messaging.SearchMsgStr(msgID);

			strReturnMsg = msgInfo.MsgLocal;
			strDisplayData = MakeDisplayData(msgID, msgInfo, programLogInfo, sourceInfo, args, dLogTime);

			strDataString = MakeLogData(msgID, msgInfo, programLogInfo, sourceInfo, args, dLogTime);

			// TODO : 파일 삭제/ 파일 쓰기 방식으로 Test 필요
			// 파일 삭제
			DeleteOldFile(programLogInfo.ProgramID, programLogInfo.ProcessID, strFilePath, iLogSaveDays);
			//파일 쓰기
			WriteStringDataToFile(strDataString, strFilePath);


			if( !m_BefMsgID.Equals(msgID) || !m_BefMsgStr.Equals(msgInfo.MsgText) || 
				Convert.ToInt32((dLogTime - m_dBefLogTm).TotalSeconds) > programLogInfo.LogTerm )
			{

				
				
				#region [주석] 데몬 파일 삭제 후 저장하는 방식
				
				//기존 데몬에서 파일 삭제후 저장하는 방식
				//m_strPgmID	= programLogInfo.ProgramID;
				//LogToFile(strDataString); 

				#endregion
				
				#region MSMQ로 데이터 전송

				if(msgInfo.Dbflg.Equals("Y")) // DB Log Flag 확인하여 MQ로 전송
				{
					// MSMQ Method 호출
                    //CFW.Msmq.clsMQMessage qMsg = new CFW.Msmq.clsMQMessage();
                    //qMsg.Label = programLogInfo.DBInfo;

                    ////파일 로그할 string
                    //qMsg.Body = strDataString;
		
                    //if( DBLogQueueName.Length > 0 )
                    //{
                    //    CFW.Msmq.Sender.SendMessage(qMsg, DBLogQueueName, QueueSize);
                    //}
                    //else
                    //{
                    //    strDisplayData += "There is no a config file.";
                    //}
				}

				#endregion MSMQ로 데이터 전송
			}

			// 중복 체크 하기 위해 전역 변수에 값 저장
			m_dBefLogTm = dLogTime;
			m_BefMsgID  = msgID;
			m_BefMsgStr = msgInfo.MsgText;

			return strReturnMsg;
		}
		#endregion

		//==============================================

		#region -- LOG Delete Class - Terminal

		/// <summary>
		/// Log 삭제 클래스
		/// </summary>
		public class clsDelLogFile
		{
			private string	m_FilePath = "";
			private string	m_ProcessID = "";
			private int		m_LogSaveDays = 30;
			/// <summary>
			/// clsLogDelete - 삭제 정보 변수 할당
			/// </summary>
			/// <param name="sFilePath">Log 파일 경로</param>
			/// <param name="sProcessID">Process ID</param>
			/// <param name="nLogSaveDays">LogSaveDays</param>
			public clsDelLogFile(string sFilePath, string sProcessID, int nLogSaveDays)
			{
				m_FilePath		= sFilePath;
				m_ProcessID		= sProcessID;
				m_LogSaveDays	= nLogSaveDays;

				if( m_LogSaveDays<1 )
					m_LogSaveDays = 1;
			}

			/// <summary>
			/// DelProcLogFile-로그 삭제(Terminal)
			/// </summary>
			public void DelProcLogFile()
			{
				/* TODO : 파일 삭제에서 성능 문제가 발생한다면, 파일명 생성방식을 바꾸면된다.
				 *  현재 파일명에 년월일 정보가 들어가 있기 때문에 모두 검색하는 구조로 가야 된다.
				 *  그러나 파일명에 일자만 기입(데몬)하는 경우
				 * 존재하는 파일 유무 식별하여 삭제하는 로직이 단순해지며 성능 향상도 기대할 수 있다.
				 */ 

				Console.WriteLine("=========== DELETE FILE LIST START ============");
				if(Directory.Exists(m_FilePath))
				{
					string[] dirs = Directory.GetFiles(m_FilePath, m_ProcessID+"_LOG_*.txt");
					DateTime dDelTime = DateTime.Now;
					foreach (string dir in dirs) 
					{
						try
						{
							DateTime dFileTime = File.GetLastWriteTime(dir);

							double nGapDay = Math.Round( ((TimeSpan)(dDelTime - dFileTime)).TotalDays );
							if( nGapDay > m_LogSaveDays )
							{
								Console.WriteLine(dir + "-FROM["+ nGapDay +"]");
								File.Delete(dir);
							}
						}
						catch(Exception e){}

					}
				}
				Console.WriteLine("=========== DELETE FILE LIST END ============");
				
			}
		}
		#endregion
		
		#region -- GetLogSourceFileInfo - 공통
		/// <summary>
		/// GetSourceInfo
		/// </summary>
		/// <returns>로그 소스파일 정보</returns>
		public clsSourceInfo GetSourceInfo()
		{
			StackTrace st = new StackTrace(true);
			StackFrame sf = st.GetFrame(1);
			clsSourceInfo SrcInfo = new clsSourceInfo();
			SrcInfo.File = sf.GetFileName();
			SrcInfo.Method = sf.GetMethod().Name;
			SrcInfo.LineNumber = sf.GetFileLineNumber();
			return SrcInfo;
		}
		#endregion

	}

	#region -- class clsMessage : Message Structure
	/// <summary>
	/// clsMessage Structure - 메시지 구조체
	/// </summary>
	public class clsMessage
	{
		#region -- Member
		public DateTime LogTime     ;				// LogTIme 
		public string	LogTimeStr  = "";			// LogTIme String
		public string	ProgramID	= "";			// 프로그램 ID
		public string	ProcessID	= "";			// 프로세스 ID
		public string	Method		= "";			// Method Name
		public string	Level		= "";			// 메시지 Level
		public string	MsgID		= "";			// 메시지 ID
		public string	MsgText     = "";			// 메시지 Text
		public string	UserID		= "";			// User ID
		#endregion
	}
	#endregion

	#region -- class clsSourceInfo : SourceInfo Structure
	/// <summary>
	/// clsSourceInfo Structure - 소스파일 정보
	/// </summary>
	public class clsSourceInfo
	{
		#region -- Member

		public string	File		 = "";				
		public string	Method		 = "";			
		public int   	LineNumber		 = 0;		
	
		#endregion
	}
	#endregion

	#region -- class clsProgramLogInfo

	/// <summary>
	/// Logging 에서 사용할 Program 정보
	/// </summary>
	public class clsProgramLogInfo
	{
		/// <summary>
		/// DB연결 Hint(V,E,S) 기본값:V
		/// </summary>
		public string DBInfo = "V";

		/// <summary>
		/// 로깅 남기는 시간 간격(초단위)
		/// </summary>
		public int LogTerm = 0;

		/// <summary>
		/// 프로그램 ID
		/// </summary>
		public string ProgramID = "";

		/// <summary>
		/// 프로세스 ID
		/// </summary>
		public string ProcessID = "";
		
	}

	#endregion

	#region -- LOG control code
	/// <summary>
	/// LOG control code 추상 클래스
	/// Define System Area
	/// Define usage of messages
	/// Define level of messages
	/// Define DB Commit
	/// </summary>
	public abstract class LOG
	{
		// System Area
		public const string			SYS_DAEMON    	= "D";// Daemon
		public const string			SYS_TERMIAL    	= "T";// Terminal
		public const string			SYS_WEB    		= "W";// Web

		// Define usage of messages
		public const string			USE_GET			= "0";// For getting the message string
		public const string			USE_LOG			= "1";// For loggin the message

//		// Define level of messages
//		public const string			LEVEL_NONE		= " ";//Information
//		public const string			LEVEL_INFO		= "I";//Information
//		public const string			LEVEL_ALARM		= "A";//Alarm, warnning
//		public const string			LEVEL_ERROR		= "E";//error

		// Define DB Commit
		public const string			DBCOMMIT_YES	= "Y";//DB commit after loging
		public const string			DBCOMMIT_NO		= "N";//DB no commit after loging

	}
	#endregion
}
