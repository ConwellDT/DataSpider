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
	/// TODO : static �Լ� ����
	/// </summary>
	public class Logging
	{
		#region -- ���� ����(Demon)
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

		#region -- ���� ����(Terminal)
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

		#region -- ���� ����(CS)
		private string m_QueuePath = "";
		private string m_QueueSize = "";
		#endregion


		#region -- Logging������
		/// <summary>
		/// �������Դϴ�.
		/// </summary>
		public Logging()
		{
			m_dtOLD = DateTime.Now;
			//
			// TODO: ���⿡ ������ ���� �߰��մϴ�.
			//
		}
		#endregion

		#region -- Property 

		/// <summary>
		/// �α� ���� ��θ� �����ϰų� ���� �о�� �� �ֽ��ϴ�.
		/// </summary>
		/// <param name="filePath">�α� ���� ���</param>
		public void UpdateFilePath(string filePath)      // (IN) LOG File Path
		{
			m_FilePath    = filePath;
		}	

		/// <summary>
		/// �α� ���� ��θ� �о�ɴϴ�.
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
		/// ����� ���̵� �����ϰų� �о�� �� �ֽ��ϴ�.
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
		/// DB�α� ť ����� �о�ɴϴ�.
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
		/// DB�α� ť �̸��� �о�ɴϴ�.
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
		/// �α� ���� �ϼ��� �����ϰų� ���� �о�� �� �ֽ��ϴ�.
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

		#region -- �� �α� (�̺�Ʈ ǥ�ñ�)

		/// <summary>
		/// �α׸� �̺�Ʈ ǥ�ñ⿡ ���ϴ�.
		/// </summary>
		/// <param name="eventLog">"���� ���α׷�", "����", "�ý���" �� �ƴ� �ٸ� ��ġ�� ������ �� �ֽ��ϴ�.</param>
		/// <param name="eventLogSource">�̺�Ʈ �ҽ����Դϴ�.</param>
		/// <param name="logType">�̺�Ʈ �����Դϴ�.����/���/���� ���� ī�װ��� �ֽ��ϴ�.</param>
		/// <param name="contents">�α� �����Դϴ�.</param>
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

		//�޽��� ���� ====================================

		#region -- Log/Display �� �޽��� ����

		#region ȭ�鿡 Display �� ���Ŀ� ���߾� ���� ����

		/// <summary>
		/// ȭ�鿡 Display �� ���Ŀ� ���߾� ���� ����
		/// </summary>
		/// <returns>Display ���� ����</returns>
		private string MakeDisplayData(string msgID, clsMsgInfo msgInfo, clsProgramLogInfo programLogInfo
			, clsSourceInfo sourceInfo, string[] arg, System.DateTime dt)
		{
			string strType = msgInfo.MsgType;
			System.Text.StringBuilder sb = new StringBuilder();

			//����
			sb.AppendFormat("{0:yyyy-MM-dd}", dt.Date);
			sb.Append(CFW.Common.Constant.DATA_DELIMITER);
			//�ð�
			sb.AppendFormat("{0:D2}:{1:D2}:{2:D2}", dt.Hour, dt.Minute, dt.Second);
			sb.Append(CFW.Common.Constant.DATA_DELIMITER);
			//���α׷� ID
			sb.Append(programLogInfo.ProgramID );
			sb.Append(CFW.Common.Constant.DATA_DELIMITER);
			//���μ��� ID
			sb.Append(programLogInfo.ProcessID );
			sb.Append(CFW.Common.Constant.DATA_DELIMITER);
			
			//�޽��� Level
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

		#endregion Display �� ���Ŀ� ���߾� ���� ����

		#region ���Ŀ� ������� �����͸� �����ڸ� �̿��Ͽ� string ����(DB Log)
		
		/// <summary>
		/// ���Ŀ� ������� �����͸� �����ڸ� �̿��Ͽ� string ���� 
		/// </summary>
		/// <param name="msgID">�޽��� ID</param>
		/// <param name="msgInfo">�޽��� ����</param>
		/// <param name="programLogInfo">���α׷� ����</param>
		/// <param name="sourceInfo">�ҽ� ����</param>
		/// <param name="arg">�߰� �޽���</param>
		/// <param name="dt">�ý��� �ð�</param>
		/// <returns></returns>
		private string MakeLogData(string msgID, clsMsgInfo msgInfo, clsProgramLogInfo programLogInfo
			, clsSourceInfo sourceInfo, string[] arg, System.DateTime dt)
		{
			string strType = msgInfo.MsgType;
			System.Text.StringBuilder sb = new StringBuilder();

			sb.Append("L");
			sb.Append(CFW.Common.Constant.DATA_DELIMITER);

			//����
			sb.AppendFormat("{0:yyyyMMdd}", dt.Date);
			sb.Append(CFW.Common.Constant.DATA_DELIMITER);

			//�ð�
			sb.AppendFormat("{0:D2}{1:D2}{2:D2}", dt.Hour, dt.Minute, dt.Second);
			sb.Append(CFW.Common.Constant.DATA_DELIMITER);

			//���α׷� ID
			sb.Append(programLogInfo.ProgramID );
			sb.Append(CFW.Common.Constant.DATA_DELIMITER);

			//���μ��� ID
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

		#endregion  ���Ŀ� ������� �����͸� �����ڸ� �̿��Ͽ� string ����

		#region ���Ŀ� ������� �����͸� �����ڸ� �̿��Ͽ� string ����(FileLog)

		/// <summary>
		/// ���Ŀ� ������� �����͸� �����ڸ� �̿��Ͽ� string ����
		/// </summary>
		/// <param name="msgID">�޽��� ID</param>
		/// <param name="msgInfo">�޽��� ����</param>
		/// <param name="programLogInfo">���α׷� ����</param>
		/// <param name="sourceInfo">�ҽ� ����</param>
		/// <param name="arg">�߰� �޽���</param>
		/// <param name="dt">�ý��� �ð�</param>
		/// <returns></returns>
		private string MakeFileLogData(string msgID, clsMsgInfo msgInfo, clsProgramLogInfo programLogInfo
			, clsSourceInfo sourceInfo, string[] arg, System.DateTime dt)
		{
			string strType = msgInfo.MsgType;
			System.Text.StringBuilder sb = new StringBuilder();


			//����
			sb.AppendFormat("{0:yyyyMMdd}", dt.Date);//1
			sb.Append(CFW.Common.Constant.DATA_DELIMITER);

			//�ð�
			sb.AppendFormat("{0:D2}{1:D2}{2:D2}", dt.Hour, dt.Minute, dt.Second);//2
			sb.Append(CFW.Common.Constant.DATA_DELIMITER);

			//���α׷� ID
			sb.Append(programLogInfo.ProgramID );//3
			sb.Append(CFW.Common.Constant.DATA_DELIMITER);

			//���μ��� ID
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

		#endregion  ���Ŀ� ������� �����͸� �����ڸ� �̿��Ͽ� string ����

		#region �߰��� ������ Msg ���� Update

		/// <summary>
		/// �߰��� ������ Msg ���� Update
		/// </summary>
		/// <param name="msgInfo">�޽��� ����</param>
		/// <param name="args">�߰� ����</param>
		/// <param name="type">�޽��� ����</param>
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

			//���Ƿ� Type�� ������ ��� DB�� ���� Update
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

		#endregion �߰��� ������ Msg ���� Update

		#endregion �޽��� ����
		
		//LogToFile ====================================

		#region -- ���� �α� ���� ����

		/// <summary>
		/// �α׸� ���Ͽ� ���ϴ�.
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

		#region -- ���� ����(������ Thread ����)


		/// <summary>
		/// ���� ���� �� ��¥ �������� ����� ���� ����
		/// </summary>
		/// <param name="programID"></param>
		/// <param name="processID">���μ��� ID</param>
		/// <param name="filePath">���� ���</param>
		/// <param name="logSaveDays">��¥ ����(day)</param>
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


		#endregion ���� ����
		
		#region -- ���Ͽ� ����

		/// <summary>
		/// �����ڷ� ������ String File�� ����
		/// 
		/// ������	: CFW.Common.Constant.DATA_DELIMITER
		/// ������	: Parameter filePath + �����ڸ� �������� 3��°(zero base)�� (������ Unknown)
		/// ���ϸ�	: �����ڸ� �������� 4��°�� (������ Unknown)
		///			  + _�����ڸ� �������� 0��° �� : LOG(L) / Status(������) 
		///			  + _���� ����.txt
		///	����	: Parameter dataString
		/// </summary>
		/// <param name="dataString">�����ڷ� �̷���� ������</param>
		/// <param name="filePath">���� ���� ���</param>
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

				//���� ���� ���
				sb.Append(filePath);

				strPath = sb.ToString(); 

				sb.Append("\\");

				//���ϸ�
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


		#endregion ���Ͽ� ����


        //�͹̳� �α� ���� 2015.01.09

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

		//Log Agent ���� ====================================
		
		#region == �͹̳� �α�[Multi Thread ȯ��](����) ==

		/// <summary>
		/// �͹̳� ���α׷� ���� �� Multi Thread ȯ�濡�� �α��� ����ϴ�. 
		/// </summary>
		/// <param name="type">�޽�������</param>
		/// <param name="msgID">�޽��� ���̵�</param>
		/// <param name="programLogInfo">DB����Hint, �α� ����(��), ProgramID, ProcessID</param>
		/// <param name="sourceInfo">�α� �ҽ� ���� ����ü</param>
		/// <param name="args">�߰� �Ķ����</param>
		/// <returns>�޽��� ����ü</returns>
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
//            int iLogSaveDays = LogSaveDays; // TODO : Config ���� �о�� ��.
			
//            // ���α׷� ID Ȯ��
//            if(programLogInfo.ProgramID.Length <=0)
//                programLogInfo.ProgramID = m_sProgramID;

//            // Update Msg Info
//            clsMsgInfo msgInfo = CFW.Common.Messaging.SearchMsgStr(msgID);
//            UpdateMsgInfo(ref msgInfo, args, type);

//            // Diplay �� ���� ����
            clsMessage pMsg = new clsMessage();
//            pMsg.LogTime	= dLogTime; //�α� ����� �ð� Term
//            pMsg.ProgramID	= programLogInfo.ProgramID;
//            pMsg.ProcessID	= programLogInfo.ProcessID;
//            pMsg.Method		= sourceInfo.Method;
//            pMsg.MsgID		= msgID;
//            pMsg.UserID		= m_sUserID;

//            pMsg.MsgText    = msgInfo.MsgLocal;//MakeDisplayData(msgID, msgInfo, programLogInfo, sourceInfo, args, dLogTime);
//            pMsg.Level		= msgInfo.MsgType;
			
//            /* 	- ��� �޽����� ���Ͽ� �ߺ� üũ
//                - ���� ����
//                - ���� �α� 
//                - DB Log Flag�� Y �� ��� MQ�� ����
//            */
//            if( !m_BefMsgID.Equals(msgID) || !m_BefMsgStr.Equals(pMsg.MsgText) || 
//                Convert.ToInt32((dLogTime - m_dBefLogTm).TotalSeconds) > programLogInfo.LogTerm )
//            {

//                // ���� ����
//                DeleteOldFile(pMsg.ProgramID, pMsg.ProcessID, strFilePath, iLogSaveDays);
				
//                // DB �α� string �����
//                strDataString = MakeLogData(msgID, msgInfo, programLogInfo, sourceInfo, args, dLogTime);

//                // ���� �α� string �����
//                string strFileLogData = "";
//                strFileLogData = MakeFileLogData(msgID, msgInfo, programLogInfo, sourceInfo, args, dLogTime);


//                #region ���� �α� (Multi Thread)

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

//                #endregion ���� �α�  (Multi Thread)

//                #region MSMQ�� ������ ����

//                if(msgInfo.Dbflg.Equals("Y")) // DB Log Flag Ȯ���Ͽ� MQ�� ����
//                {
//                    // MSMQ Method ȣ��
//                    CFW.Msmq.clsMQMessage qMsg = new CFW.Msmq.clsMQMessage();
//                    qMsg.Label = programLogInfo.DBInfo;

//                    //���� �α��� string
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

//                #endregion MSMQ�� ������ ����
				
				
//            }
			
//            // �ߺ� üũ �ϱ� ���� ���� ������ �� ����
//            m_dBefLogTm = dLogTime;
//            m_BefMsgID  = msgID;
//            m_BefMsgStr = pMsg.MsgText;

            return pMsg;
		}
		#endregion

		#region == �͹̳� �α�[Multi Thread ȯ��](Message Level ����) ==
		
		/// <summary>
		/// �͹̳� ���α׷� ���� �� Multi Thread ȯ�濡�� �α��� ����ϴ�. 
		/// </summary>
		/// <param name="msgID">�޽��� ���̵�</param>
		/// <param name="programLogInfo">DB����Hint, �α� ����(��), ProgramID, ProcessID</param>
		/// <param name="sourceInfo">�α� �ҽ� ���� ����ü</param>
		/// <param name="args">�߰� �Ķ����</param>
		/// <returns></returns>
		public clsMessage WriteLogInMultiThread(string msgID, clsProgramLogInfo programLogInfo
			, clsSourceInfo sourceInfo,	 params string[] args)
		{
            //DateTime dLogTime = DateTime.Now;
            //string strDataString = "";
            //string strFilePath = LogFilePath;
            //int iLogSaveDays = LogSaveDays; // TODO : Config ���� �о�� ��.
			
            //// ���α׷� ID Ȯ��
            //if(programLogInfo.ProgramID.Length <=0)
            //    programLogInfo.ProgramID = m_sProgramID;

            //// Update Msg Info
            //clsMsgInfo msgInfo = CFW.Common.Messaging.SearchMsgStr(msgID);
            //UpdateMsgInfo(ref msgInfo, args, "");

            //// Diplay �� ���� ����
            clsMessage pMsg = new clsMessage();
            //pMsg.LogTime	= dLogTime; //�α� ����� �ð� Term
            //pMsg.ProgramID	= programLogInfo.ProgramID;
            //pMsg.ProcessID	= programLogInfo.ProcessID;
            //pMsg.Method		= sourceInfo.Method;
            //pMsg.MsgID		= msgID;
            //pMsg.UserID		= m_sUserID;

            //pMsg.MsgText    = msgInfo.MsgLocal;//MakeDisplayData(msgID, msgInfo, programLogInfo, sourceInfo, args, dLogTime);
            //pMsg.Level		= msgInfo.MsgType;
			
            ///* 	- ��� �޽����� ���Ͽ� �ߺ� üũ
            //    - ���� ����
            //    - ���� �α� 
            //    - DB Log Flag�� Y �� ��� MQ�� ����
            //*/
            //if( !m_BefMsgID.Equals(msgID) || !m_BefMsgStr.Equals(pMsg.MsgText) || 
            //    Convert.ToInt32((dLogTime - m_dBefLogTm).TotalSeconds) > programLogInfo.LogTerm )
            //{

            //    // ���� ����
            //    DeleteOldFile(pMsg.ProgramID, pMsg.ProcessID, strFilePath, iLogSaveDays);
				
            //    // DB �α� string �����
            //    strDataString = MakeLogData(msgID, msgInfo, programLogInfo, sourceInfo, args, dLogTime);

            //    // ���� �α� string �����
            //    string strFileLogData = "";
            //    strFileLogData = MakeFileLogData(msgID, msgInfo, programLogInfo, sourceInfo, args, dLogTime);


            //    #region ���� �α� (Multi Thread)

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

            //    #endregion ���� �α�  (Multi Thread)

            //    #region MSMQ�� ������ ����

            //    if(msgInfo.Dbflg.Equals("Y")) // DB Log Flag Ȯ���Ͽ� MQ�� ����
            //    {
            //        // MSMQ Method ȣ��
            //        CFW.Msmq.clsMQMessage qMsg = new CFW.Msmq.clsMQMessage();
            //        qMsg.Label = programLogInfo.DBInfo;

            //        //���� �α��� string
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

            //    #endregion MSMQ�� ������ ����
				
				
            //}
			
            //// �ߺ� üũ �ϱ� ���� ���� ������ �� ����
            //m_dBefLogTm = dLogTime;
            //m_BefMsgID  = msgID;
            //m_BefMsgStr = pMsg.MsgText;
			
			return pMsg;

		}
		#endregion


		#region == ���� �α�(����) ==

		/// <summary>
		/// ���� ���α׷� ���� �� �α��� ����ϴ�.
		/// </summary>
		/// <param name="type">�޽�������</param>
		/// <param name="msgID">�޽��� ���̵�</param>
		/// <param name="programLogInfo">DB����Hint, �α� ����(��), ProgramID, ProcessID</param>
		/// <param name="sourceInfo">�α� �ҽ� ���� ����ü</param>
		/// <param name="args">�߰� �Ķ����</param>
		/// <returns>�޽��� ���ڿ�</returns>
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
			int iLogSaveDays = LogSaveDays; // TODO : Config ���� �о�� ��.

			// ���α׷� ID Ȯ��
			if(programLogInfo.ProgramID.Length <=0)
				programLogInfo.ProgramID = m_sProgramID;

			// Update Msg Info
			clsMsgInfo msgInfo = CFW.Common.Messaging.SearchMsgStr(msgID);
			UpdateMsgInfo(ref msgInfo, args, type);

			strReturnMsg = msgInfo.MsgLocal;
			strDisplayData = MakeDisplayData(msgID, msgInfo, programLogInfo, sourceInfo, args, dLogTime);

			strDataString = MakeLogData(msgID, msgInfo, programLogInfo, sourceInfo, args, dLogTime);

			// TODO : ���� ����/ ���� ���� ������� Test �ʿ�
			// ���� ����
			DeleteOldFile(programLogInfo.ProgramID, programLogInfo.ProcessID, strFilePath, iLogSaveDays);
			//���� ����
			WriteStringDataToFile(strDataString, strFilePath);


			if( !m_BefMsgID.Equals(msgID) || !m_BefMsgStr.Equals(msgInfo.MsgText) || 
				Convert.ToInt32((dLogTime - m_dBefLogTm).TotalSeconds) > programLogInfo.LogTerm )
			{

				
				
				#region [�ּ�] ���� ���� ���� �� �����ϴ� ���
				
				//���� ���󿡼� ���� ������ �����ϴ� ���
				//m_strPgmID	= programLogInfo.ProgramID;
				//LogToFile(strDataString); 

				#endregion
				
				#region MSMQ�� ������ ����

                //if(msgInfo.Dbflg.Equals("Y")) // DB Log Flag Ȯ���Ͽ� MQ�� ����
                //{
                //    // MSMQ Method ȣ��
                //    CFW.Msmq.clsMQMessage qMsg = new CFW.Msmq.clsMQMessage();
                //    qMsg.Label = programLogInfo.DBInfo;

                //    //���� �α��� string
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

				#endregion MSMQ�� ������ ����
			}

			// �ߺ� üũ �ϱ� ���� ���� ������ �� ����
			m_dBefLogTm = dLogTime;
			m_BefMsgID  = msgID;
			m_BefMsgStr = msgInfo.MsgText;

			return strReturnMsg;
			
		}


		/// <summary>
		/// ���� ���α׷� ���� �� �α��� ����ϴ�.
		/// </summary>
		/// <param name="type">�޽�������</param>
		/// <param name="msgID">�޽��� ���̵�</param>
		/// <param name="programLogInfo">DB����Hint, �α� ����(��), ProgramID, ProcessID</param>
		/// <param name="sourceInfo">�α� �ҽ� ���� ����ü</param>
		/// <returns></returns>
		/// <remarks>���μ��� ���̵� �� �߰� �Ķ���� �߰� �Ͽ� Main WriteLog�޼ҵ� ȣ��</remarks>
		public string WriteLog(string type, string msgID, clsProgramLogInfo programLogInfo, clsSourceInfo sourceInfo)
		{
			string[] args = new string[0];
			return WriteLog( type, msgID, programLogInfo, sourceInfo, args);//  (Additional message parameter)
		}

		#endregion

		#region == ���� �α�(Message Level ����) ==
		/// <summary>
		/// ���� ���α׷� ���� �� �α��� ����ϴ�.
		/// </summary>
		/// <param name="msgID">�޽��� ���̵�</param>
		/// <param name="programLogInfo">DB����Hint, �α� ����(��), ProgramID, ProcessID</param>
		/// <param name="sourceInfo">�α� �ҽ� ���� ����ü</param>
		/// <returns></returns>
		public string WriteLog(string msgID, clsProgramLogInfo programLogInfo, clsSourceInfo sourceInfo)
		{
			string[] args = new string[0];
			return WriteLog( msgID, programLogInfo, sourceInfo, args);//  (Additional message parameter)
		}


		/// <summary>
		/// ���� ���α׷� ���� �� �α��� ����ϴ�.
		/// </summary>
		/// <param name="msgID">�޽��� ���̵�</param>
		/// <param name="programLogInfo">DB����Hint, �α� ����(��), ProgramID, ProcessID</param>
		/// <param name="sourceInfo">�α� �ҽ� ���� ����ü</param>
		/// <param name="args">�߰� �Ķ����</param>
		/// <returns></returns>
		public string WriteLog(string msgID, clsProgramLogInfo programLogInfo, clsSourceInfo sourceInfo ,params string[] args)
		{
			string strDisplayData = "";
			DateTime dLogTime = DateTime.Now;
			string strDataString = "";
			string strFilePath = LogFilePath;
			string strReturnMsg = "";
			int iLogSaveDays = LogSaveDays; // TODO : Config ���� �о�� ��.

			// ���α׷� ID Ȯ��
			if(programLogInfo.ProgramID.Length <=0)
				programLogInfo.ProgramID = m_sProgramID;

			// Update Msg Info
			clsMsgInfo msgInfo = CFW.Common.Messaging.SearchMsgStr(msgID);
			UpdateMsgInfo(ref msgInfo, args, "");
			clsMsgInfo msgInfo1 = CFW.Common.Messaging.SearchMsgStr(msgID);

			strReturnMsg = msgInfo.MsgLocal;
			strDisplayData = MakeDisplayData(msgID, msgInfo, programLogInfo, sourceInfo, args, dLogTime);

			strDataString = MakeLogData(msgID, msgInfo, programLogInfo, sourceInfo, args, dLogTime);

			// TODO : ���� ����/ ���� ���� ������� Test �ʿ�
			// ���� ����
			DeleteOldFile(programLogInfo.ProgramID, programLogInfo.ProcessID, strFilePath, iLogSaveDays);
			//���� ����
			WriteStringDataToFile(strDataString, strFilePath);


			if( !m_BefMsgID.Equals(msgID) || !m_BefMsgStr.Equals(msgInfo.MsgText) || 
				Convert.ToInt32((dLogTime - m_dBefLogTm).TotalSeconds) > programLogInfo.LogTerm )
			{

				
				
				#region [�ּ�] ���� ���� ���� �� �����ϴ� ���
				
				//���� ���󿡼� ���� ������ �����ϴ� ���
				//m_strPgmID	= programLogInfo.ProgramID;
				//LogToFile(strDataString); 

				#endregion
				
				#region MSMQ�� ������ ����

				if(msgInfo.Dbflg.Equals("Y")) // DB Log Flag Ȯ���Ͽ� MQ�� ����
				{
					// MSMQ Method ȣ��
                    //CFW.Msmq.clsMQMessage qMsg = new CFW.Msmq.clsMQMessage();
                    //qMsg.Label = programLogInfo.DBInfo;

                    ////���� �α��� string
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

				#endregion MSMQ�� ������ ����
			}

			// �ߺ� üũ �ϱ� ���� ���� ������ �� ����
			m_dBefLogTm = dLogTime;
			m_BefMsgID  = msgID;
			m_BefMsgStr = msgInfo.MsgText;

			return strReturnMsg;
		}
		#endregion

		//==============================================

		#region -- LOG Delete Class - Terminal

		/// <summary>
		/// Log ���� Ŭ����
		/// </summary>
		public class clsDelLogFile
		{
			private string	m_FilePath = "";
			private string	m_ProcessID = "";
			private int		m_LogSaveDays = 30;
			/// <summary>
			/// clsLogDelete - ���� ���� ���� �Ҵ�
			/// </summary>
			/// <param name="sFilePath">Log ���� ���</param>
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
			/// DelProcLogFile-�α� ����(Terminal)
			/// </summary>
			public void DelProcLogFile()
			{
				/* TODO : ���� �������� ���� ������ �߻��Ѵٸ�, ���ϸ� ��������� �ٲٸ�ȴ�.
				 *  ���� ���ϸ� ����� ������ �� �ֱ� ������ ��� �˻��ϴ� ������ ���� �ȴ�.
				 *  �׷��� ���ϸ� ���ڸ� ����(����)�ϴ� ���
				 * �����ϴ� ���� ���� �ĺ��Ͽ� �����ϴ� ������ �ܼ������� ���� ��� ����� �� �ִ�.
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
		
		#region -- GetLogSourceFileInfo - ����
		/// <summary>
		/// GetSourceInfo
		/// </summary>
		/// <returns>�α� �ҽ����� ����</returns>
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
	/// clsMessage Structure - �޽��� ����ü
	/// </summary>
	public class clsMessage
	{
		#region -- Member
		public DateTime LogTime     ;				// LogTIme 
		public string	LogTimeStr  = "";			// LogTIme String
		public string	ProgramID	= "";			// ���α׷� ID
		public string	ProcessID	= "";			// ���μ��� ID
		public string	Method		= "";			// Method Name
		public string	Level		= "";			// �޽��� Level
		public string	MsgID		= "";			// �޽��� ID
		public string	MsgText     = "";			// �޽��� Text
		public string	UserID		= "";			// User ID
		#endregion
	}
	#endregion

	#region -- class clsSourceInfo : SourceInfo Structure
	/// <summary>
	/// clsSourceInfo Structure - �ҽ����� ����
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
	/// Logging ���� ����� Program ����
	/// </summary>
	public class clsProgramLogInfo
	{
		/// <summary>
		/// DB���� Hint(V,E,S) �⺻��:V
		/// </summary>
		public string DBInfo = "V";

		/// <summary>
		/// �α� ����� �ð� ����(�ʴ���)
		/// </summary>
		public int LogTerm = 0;

		/// <summary>
		/// ���α׷� ID
		/// </summary>
		public string ProgramID = "";

		/// <summary>
		/// ���μ��� ID
		/// </summary>
		public string ProcessID = "";
		
	}

	#endregion

	#region -- LOG control code
	/// <summary>
	/// LOG control code �߻� Ŭ����
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
