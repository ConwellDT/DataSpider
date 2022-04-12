using System;
using System.Data;
using System.Collections;
using Oracle.DataAccess.Client;

namespace CFW.Data
{
	/// <summary>
	/// Class1에 대한 요약 설명입니다.
	/// </summary>
	public class OracleDbAccess
	{
		// Member Variables
		protected OracleConnection		m_DBCon;
		protected OracleCommand			m_DBCmd;
		protected OracleDataReader		m_DataReader, m_DataReader1;
		protected OracleTransaction		m_DBTrans;

		//protected RsArray				cRsArray = new RsArray();

		protected int					m_nRows;
		protected string				m_strSQL;
		protected string				m_strRET;
		protected string				m_strOraErr = "ORA";

        //internal static string          m_SQLTraceMode = "";//System.Configuration.ConfigurationSettings.AppSettings["SQLTraceMode"].Trim().ToLower();
        //internal static string          m_SQLTracePath = "";//System.Configuration.ConfigurationSettings.AppSettings["SQLTracePath"].Trim();
        internal const int              commandTimeout = 30;

		/// <summary>
		/// 생성자입니다.
		/// </summary>
		public OracleDbAccess()
		{
			m_nRows	= 0;
		}

		#region -- DBConnectState/GetOracleConnection
		
		/// <summary>
		/// OracleConnection을 가져옵니다.
		/// </summary>
		/// <returns>OracleConnection</returns>
		public OracleConnection GetOracleConnection()
		{
			return	m_DBCon;
		}

		/// <summary>
		/// DB 커넥션 상태를 가져옵니다.
		/// </summary>
		/// <returns>DB 커넥션 상태</returns>
		public bool DBConnectState()
		{
			bool bConnStat = false;
			if( m_DBCon == null || m_DBCon.State.ToString() == "Closed" )	return false;

			try
			{
				string sDate = "";
				m_DBCmd = m_DBCon.CreateCommand();
				m_DBCmd.CommandType = CommandType.Text;
				m_DBCmd.CommandText = "SELECT TO_CHAR(SYSDATE, 'YYYYMMDD') FROM DUAL";
				m_DataReader = m_DBCmd.ExecuteReader();
				while( m_DataReader.Read() )
				{
					sDate = m_DataReader[0].ToString();
					bConnStat = true;
					break;
				}
			}
			catch(Exception e)
			{
				bConnStat = false;
			}
			finally
			{
				m_DBCmd.Dispose();
				if(m_DataReader != null)
					m_DataReader.Dispose();
				if( !bConnStat )
					DBDisConnect();
			}

			return bConnStat;
		}

        /// <summary>
        /// DB 커넥션 상태를 가져옵니다.
        /// </summary>
        /// <returns>DB 커넥션 상태</returns>
        public static bool DBConnectCheck()
        {
            bool bConnStat = false;

            OracleConnection    conn = null;
            OracleCommand       cmd = null;
            OracleDataReader    dr = null;

            try
            {
                string sDate = "";
                cmd = new OracleCommand();
                cmd.CommandText = "SELECT TO_CHAR(SYSDATE, 'YYYYMMDD') FROM DUAL"; 
                cmd.CommandType = CommandType.Text;
                cmd.CommandTimeout = commandTimeout;
                cmd.BindByName = true;

                conn = new OracleConnection(OracleDBDef.ConnectionString);
                conn.Open();

                cmd.Connection = conn;
                dr = cmd.ExecuteReader(CommandBehavior.CloseConnection);

                while (dr.Read())
                {
                    sDate = dr[0].ToString();
                    bConnStat = true;
                    break;
                }
            }
            catch (Exception e)
            {
                bConnStat = false;
            }
            finally
            {
                cmd.Dispose();
                if (dr != null)
                    dr.Dispose();
                if (!bConnStat)
                {
                    if (conn.State == ConnectionState.Open)
                        conn.Close();
                }
            }

            return bConnStat;
        }
		#endregion

		#region -- DBConnect / DBDisConnect

        /// <summary>
        /// DBConnect
        /// </summary>
        /// <param name="p_strUSER">UserName</param>
        /// <param name="p_strPW">PassWord</param>
        /// <param name="p_strAlias">Alias</param>
        /// <returns></returns>
        private void TransactionDBConnect(string p_strUSER, string p_strPW, string p_strAlias)
        {
            try
            {
                m_DBCon = new OracleConnection("user id		= " + p_strUSER + ";" +
                                                "data source	= " + p_strAlias + ";" +
                                                "password		= " + p_strPW);
                m_DBCon.Open();
                m_DBTrans = m_DBCon.BeginTransaction();
            }
            catch
            {
            }
        }

        /// <summary>
        /// DBConnect
        /// </summary>
        /// <param name="p_strUSER">UserName</param>
        /// <param name="p_strPW">PassWord</param>
        /// <param name="p_strAlias">Alias</param>
        /// <returns></returns>
        public bool DBConnect(string p_strUSER, string p_strPW, string p_strAlias)
        {
            try
            {
                TransactionDBConnect(p_strUSER, p_strPW, p_strAlias);
            }
            catch
            {
                return false;
            }

            return true;
        }
        
        /// <summary>
        /// DBConnect
        /// </summary>
        /// <param name="p_strUSER">UserName</param>
        /// <param name="p_strPW">PassWord</param>
        /// <param name="p_strAlias">Alias</param>
        /// <param name="p_strErrCode">ErrorCode(out)</param>
        /// <param name="p_strErrText">ErrorText(out)</param>
        /// <returns></returns>
        public bool DBConnect(string p_strUSER, string p_strPW, string p_strAlias, ref string p_strErrCode, ref string p_strErrText)
        {
            try
            {
                TransactionDBConnect(p_strUSER, p_strPW, p_strAlias);
            }
            catch (Exception e)
            {
                GetErrorCode(e, ref p_strErrCode, ref p_strErrText);
                return false;
            }

            return true;
        }


		/// <summary>
		/// DBConnect
		/// </summary>
		/// <param name="p_strUSER">UserName</param>
		/// <param name="p_strPW">PassWord</param>
		/// <param name="p_strAlias">Alias</param>
		/// <returns></returns>
        public bool DBConnect(string p_strConnectionString)
		{
			try
			{
                m_DBCon = new OracleConnection(p_strConnectionString);
				m_DBCon.Open();

				m_DBTrans	= m_DBCon.BeginTransaction();
			}
			catch
			{
				return false;
			}

			return	true;
		}

        /// <summary>
        /// DBConnect
        /// </summary>
        /// <param name="p_strUSER">UserName</param>
        /// <param name="p_strPW">PassWord</param>
        /// <param name="p_strAlias">Alias</param>
        /// <returns></returns>
        public bool DBConnect()
        {
            return DBConnect(OracleDBDef.ConnectionString);
        }

		

		/// <summary>
		/// DB 연결을 끊습니다.
		/// </summary>
		public void DBDisConnect()
		{
			try
			{
				m_DBTrans.Dispose();
				if ( m_DBCon.State == ConnectionState.Open )	m_DBCon.Close();
			}
			catch(Exception e)
			{
				return;
			}
		}
		
		/// <summary>
		/// DB 연결을 끊습니다.
		/// </summary>
		/// <param name="p_strErrCode">ErrorCode(out)</param>
		/// <param name="p_strErrText">ErrorText(out)</param>
		/// <returns></returns>
		public bool DBDisConnect(ref string p_strErrCode, ref string p_strErrText)
		{
			try
			{
				m_DBTrans.Dispose();
				if ( m_DBCon.State == ConnectionState.Open )	m_DBCon.Close();
			}
			catch(Exception e)
			{
				GetErrorCode(e, ref p_strErrCode, ref p_strErrText);
				return false;
			}

			return	true;
		}
		
		#endregion

		#region Commit / RollBack
		/// <summary>
		/// Commit
		/// </summary>
		public void Commit()
		{
			try
			{
				m_DBTrans.Commit();
				m_DBTrans	= m_DBCon.BeginTransaction();
			}
			catch(Exception e)
			{
				return;
			}
		}		

		/// <summary>
		/// Commit
		/// </summary>
		/// <param name="p_strErrCode">ErrorCode(out)</param>
		/// <param name="p_strErrText">ErrorText(out)</param>
		/// <returns>성공여부</returns>
		public bool Commit(ref string p_strErrCode, ref string p_strErrText)
		{
			try
			{
				m_DBTrans.Commit();
				m_DBTrans	= m_DBCon.BeginTransaction();
			}
			catch(Exception e)
			{
				GetErrorCode(e, ref p_strErrCode, ref p_strErrText);
				return false;
			}

			return true;
		}		

		/// <summary>
		/// RollBack
		/// </summary>
		public void RollBack()
		{
			try
			{
				m_DBTrans.Rollback();
				m_DBTrans = m_DBCon.BeginTransaction();
			}
			catch(Exception e)
			{
				return;
			}
		}

		/// <summary>
		/// RollBack
		/// </summary>
		/// <param name="p_strErrCode">ErrorCode(out)</param>
		/// <param name="p_strErrText">ErrorText(out)</param>
		/// <returns>성공여부</returns>
		public bool RollBack(ref string p_strErrCode, ref string p_strErrText)
		{
			try
			{
				m_DBTrans.Rollback();
				m_DBTrans = m_DBCon.BeginTransaction();
			}
			catch(Exception e)
			{
				GetErrorCode(e, ref p_strErrCode, ref p_strErrText);
				return false;
			}

			return true;
		}
		#endregion

        
        #region transaction

		#region OraAgent : ExecuteNonQuery
		/// <summary>
		/// SQL문을 실행합니다.
		/// </summary>
		/// <param name="p_strSQL">SQL문</param>
		/// <param name="p_strErrCode">Error Code(out)</param>
		/// <param name="p_strErrText">Error Text(out)</param>
		/// <returns>성공여부</returns>
        public bool ExecuteNonQuery(string commandText, OracleParameter[] oraParameters, CommandType commandType, ref string p_strErrCode, ref string p_strErrText)
		{
			try
			{
				m_DBCmd	= m_DBCon.CreateCommand();
                m_DBCmd.CommandType = commandType;
                m_DBCmd.CommandText = commandText;
                m_DBCmd.InitialLOBFetchSize  = -1;
                m_DBCmd.InitialLONGFetchSize = -1;      //LONG형 컬럼 패치 옵션 설정
                m_DBCmd.BindByName = true;

                if (oraParameters != null)
                {
                    foreach (OracleParameter param in oraParameters)
                    {
                        AddParameter(m_DBCmd, param);
                    }
                }

				m_nRows = m_DBCmd.ExecuteNonQuery();

				if( m_nRows == 0 )
				{					
					p_strErrCode = OracleDBDef.ORAMID_NOFOUND;
					return false;
				}
			}
			catch(Exception e)
			{
				GetErrorCode(e, ref p_strErrCode, ref p_strErrText);
				return false;
			}
			finally
			{
				m_DBCmd.Dispose();
			}

			return true;
		}

		#endregion

        #region OraAgent : ExecuteMultiple

        /// <summary>
        /// 동일 SQL 문을 다른 입력 파라미터값으로 여러번 실행한다.
        /// </summary>
        /// <param name="connectionHint"></param>
        /// <param name="commandTimeout"></param>
        /// <param name="commandText"></param>
        /// <param name="oraParameters"></param>
        /// <param name="paramValues"></param>
        /// <param name="commandType"></param>
        /// <returns></returns>
        public int ExecuteMultiple(string commandText, OracleParameter[] oraParameters, string[,] paramValues, CommandType commandType, ref string p_strErrCode, ref string p_strErrText)
        {
            if (oraParameters == null) throw new System.Exception("");//에러 메시지 어떻게 가져올 것인가?

            int iReturn = 0;

            if (paramValues.Length > 0)
            {
                int iColumnCount = paramValues.GetUpperBound(1) + 1;
                int iRowCount = paramValues.GetUpperBound(0) + 1;
                int iCol = 0;
                int iRow = 0;

                string[] paramVals = new string[iColumnCount];
                for (iCol = 0; iCol < iColumnCount; iCol++)
                    paramVals[iCol] = paramValues[iRow, iCol];

                try
                {
                    m_DBCmd = m_DBCon.CreateCommand();
                    m_DBCmd.CommandType = commandType;
                    m_DBCmd.CommandText = commandText;
                    m_DBCmd.BindByName = true;

                    foreach (OracleParameter param in oraParameters)
                    {
                        AddParameter(m_DBCmd, param);
                    }

                    if (m_DBCmd != null)
                    {
                        for (iRow = 0; iRow < iRowCount; iRow++)
                        {
                            for (iCol = 0; iCol < iColumnCount; iCol++)
                            {
                                string strValue = paramValues[iRow, iCol];
                                if ((strValue == null) || (strValue.Length == 0)) m_DBCmd.Parameters[iCol].Value = System.DBNull.Value;
                                else m_DBCmd.Parameters[iCol].Value = strValue;
                            }

                            m_nRows += m_DBCmd.ExecuteNonQuery();

                            if (m_nRows == 0)
                            {
                                p_strErrCode = OracleDBDef.ORAMID_NOFOUND;
                                return 0;
                            }
                            iReturn += m_nRows;
                        }
                    }
                }
                catch (Exception e)
                {
                    GetErrorCode(e, ref p_strErrCode, ref p_strErrText);
                    return 0;
                }
                finally
                {
                    m_DBCmd.Dispose();
                }
            }

            return iReturn;
        }

        #endregion

		#region OraAgent : ExecuteReader
		/// <summary>
		/// SQL문을 실행합니다.
		/// OracleDataReader 형태로 데이터 반환합니다.
		/// </summary>
		/// <param name="p_strSQL">SQL문</param>
		/// <param name="p_strErrCode">Error Code(out)</param>
		/// <param name="p_strErrText">Error Text(out)</param>
		/// <returns>OracleDataReader</returns>
        public OracleDataReader ExecuteReader(string commandText, OracleParameter[] oraParameters, CommandType commandType, ref string p_strErrCode, ref string p_strErrText)
		{
			try
			{
				m_DBCmd	= m_DBCon.CreateCommand();
                m_DBCmd.CommandType = commandType;
                m_DBCmd.CommandText = commandText;
                m_DBCmd.InitialLOBFetchSize  = -1;
                m_DBCmd.InitialLONGFetchSize = -1;
                m_DBCmd.BindByName = true;

                if (oraParameters != null)
                {
                    foreach (OracleParameter param in oraParameters)
                    {
                        AddParameter(m_DBCmd, param);
                    }
                }

				m_DataReader = m_DBCmd.ExecuteReader();

				return m_DataReader;
			}
			catch(Exception e)
			{
				GetErrorCode(e, ref p_strErrCode, ref p_strErrText);
				return null;
			}
			finally
			{
				m_DBCmd.Dispose();
			}
		}
		#endregion

		#region OraAgent : ExecuteScalar
		/// <summary>
		/// SQL문을 실행합니다.
		/// </summary>
		/// <param name="p_strSQL">SQL문</param>
		/// <returns></returns>
        private object ExecuteScalarObject(string commandText, OracleParameter[] oraParameters, CommandType commandType)
		{
            object objRet = null;

            try
            {
                m_DBCmd = m_DBCon.CreateCommand();
                m_DBCmd.CommandType = commandType;
                m_DBCmd.CommandText = commandText;
                m_DBCmd.BindByName = true;

                if (oraParameters != null)
                {
                    foreach (OracleParameter param in oraParameters)
                    {
                        AddParameter(m_DBCmd, param);
                    }
                }

                objRet = m_DBCmd.ExecuteScalar();
                m_DBCmd.Dispose();
            }
            catch
            {
            }

            return objRet;
		}

		/// <summary>
		/// SQL문을 실행합니다.
		/// </summary>
		/// <param name="p_strSQL">SQL문</param>
		/// <param name="p_strErrCode">Error Code(out)</param>
		/// <param name="p_strErrText">Error Text(out)</param>
		/// <returns></returns>
        public object ExecuteScalar(string commandText, OracleParameter[] oraParameters, CommandType commandType, ref string p_strErrCode, ref string p_strErrText)
		{
            object objRet = null;

			try
			{
                objRet = ExecuteScalarObject(commandText, oraParameters, commandType);
			}
			catch(Exception e)
			{
				GetErrorCode(e, ref p_strErrCode, ref p_strErrText);
                return objRet;
			}
			finally
			{
				m_DBCmd.Dispose();
			}

            return objRet;
		}

		/// <summary>
		/// SQL문을 실행합니다.
		/// </summary>
		/// <param name="p_strSQL">SQL문</param>
		/// <param name="p_nValue">int Value(out)</param>
		/// <param name="p_strErrCode">Error Code(out)</param>
		/// <param name="p_strErrText">Error Text(out)</param>
		/// <returns></returns>
        public bool ExecuteScalar(string commandText, OracleParameter[] oraParameters, CommandType commandType, ref int p_nValue, ref string p_strErrCode, ref string p_strErrText)
		{			
			try
            {
                #region
                //m_DBCmd	= m_DBCon.CreateCommand();
                //m_DBCmd.CommandType = commandType;
                //m_DBCmd.CommandText = commandText;
                //m_DBCmd.BindByName = true;

                //if (oraParameters != null)
                //{
                //    foreach (OracleParameter param in oraParameters)
                //    {
                //        AddParameter(m_DBCmd, param);
                //    }
                //}

                //p_nValue = CFW.Common.Util.StringToInt(m_DBCmd.ExecuteScalar().ToString());
                #endregion

                p_nValue = CFW.Common.Util.StringToInt(ExecuteScalarObject(commandText, oraParameters, commandType).ToString());
			}
			catch(Exception e)
			{
				GetErrorCode(e, ref p_strErrCode, ref p_strErrText);
				return	false;
			}
			finally
			{
				m_DBCmd.Dispose();
			}
			return	true;
		}

		/// <summary>
		/// SQL문을 실행합니다.
		/// </summary>
		/// <param name="p_strSQL">SQL문</param>
		/// <param name="p_lgValue">long Value(out)</param>
		/// <param name="p_strErrCode">Error Code(out)</param>
		/// <param name="p_strErrText">Error Text(out)</param>
		/// <returns>성공여부</returns>
        public bool ExecuteScalar(string commandText, OracleParameter[] oraParameters, CommandType commandType, ref long p_lgValue, ref string p_strErrCode, ref string p_strErrText)
		{
			int nVal = 0 ;
            try
            {
                if (!ExecuteScalar(commandText, oraParameters, commandType, ref nVal, ref p_strErrCode, ref p_strErrText)) return false;
                p_lgValue = nVal;
            }
            catch
            {
            }

			return true;
		}
		#endregion

		#region OraAgent : ExecuteScalar
		/// <summary>
		/// SQL문을 실행합니다.
		/// </summary>
		/// <param name="p_strSQL">SQL문</param>
		/// <param name="p_strValue">string Value(out)</param>
		/// <param name="p_strErrCode">Error Code(out)</param>
		/// <param name="p_strErrText">Error Text(out)</param>
		/// <returns>성공여부</returns>
        public bool ExecuteScalar(string commandText, OracleParameter[] oraParameters, CommandType commandType, ref string p_strValue, ref string p_strErrCode, ref string p_strErrText)
		{
            object objRet = null;

			try
            {
                #region
                //m_DBCmd	= m_DBCon.CreateCommand();
                //m_DBCmd.CommandType = commandType;
                //m_DBCmd.CommandText = commandText;
                //m_DBCmd.BindByName = true;

                //if (oraParameters != null)
                //{
                //    foreach (OracleParameter param in oraParameters)
                //    {
                //        AddParameter(m_DBCmd, param);
                //    }
                //}
				
                //if( m_DBCmd.ExecuteScalar() != null)
                //{
                //    p_strValue = m_DBCmd.ExecuteScalar().ToString();
                //}
                //else
                //{
                //    p_strValue = "";
                //}
                #endregion

                objRet = ExecuteScalarObject(commandText, oraParameters, commandType);

                if (objRet != null)
                {
                    p_strValue = objRet.ToString();
                }
                else
                {
                    p_strValue = "";
                }
				
			}
			catch(Exception e)
			{
				GetErrorCode(e, ref p_strErrCode, ref p_strErrText);
				return	false;
			}
			finally
			{
				m_DBCmd.Dispose();
			}

			return	true;
		}
		#endregion

        #region OraAgent : GetDataSet
        /// <summary>
        /// DataSet 형태로 데이터 반환
        /// </summary>
        /// <param name="connectionHint"></param>
        /// <param name="commandTimeout"></param>
        /// <param name="commandText"></param>
        /// <param name="oraParameters"></param>
        /// <param name="commandType"></param>
        /// <returns></returns>
        public DataSet GetDataSet(string commandText, OracleParameter[] oraParameters, CommandType commandType, ref string p_strErrCode, ref string p_strErrText)
        {
            OracleDataAdapter da = null;
            DataSet dsReturn = new DataSet();
                
            try
            {
                m_DBCmd = m_DBCon.CreateCommand();
                m_DBCmd.CommandType = commandType;
                m_DBCmd.CommandText = commandText;
                m_DBCmd.BindByName = true;

                if (oraParameters != null)
                {
                    foreach (OracleParameter param in oraParameters)
                    {
                        AddParameter(m_DBCmd, param);
                    }
                }

                da = new OracleDataAdapter(m_DBCmd);
                da.Fill(dsReturn);

            }
            catch (Exception e)
            {
                GetErrorCode(e, ref p_strErrCode, ref p_strErrText);
                return null;
            }
            finally
            {
                m_DBCmd.Dispose();
            }

            return dsReturn;
        }

        #endregion

        #endregion


        #region static Functions - Non Transaction

        #region ==== SQL 문을 실행(ExecuteNonQuery) ====
        public static int ExecuteNonQuery(string commandText, OracleParameter[] oraParameters, CommandType commandType)
        {
            int iReturn = 0;

            OracleConnection con = null;
            OracleCommand    cmd = null;

            try
            {
                cmd = new OracleCommand();
                cmd.CommandText = commandText;
                cmd.CommandType = commandType;
                cmd.CommandTimeout = commandTimeout;
                cmd.BindByName = true;

                if (oraParameters != null)
                {
                    foreach (OracleParameter param in oraParameters)
                    {
                        AddParameter(cmd, param);
                    }
                }

                //if (m_SQLTraceMode.Equals("on")) SQLTrace(m_SQLTracePath, cmd, true); // 미사용 향후 검토

                con = new OracleConnection(OracleDBDef.ConnectionString);
                con.Open();

                cmd.Connection = con;
                iReturn = cmd.ExecuteNonQuery();
            }
            catch (Exception e)
            {
                throw e;
            }
            finally
            {
                if (con != null)
                {
                    if (con.State == ConnectionState.Open)
                        con.Close();
                    con.Dispose();
                }
                if (cmd != null) cmd.Dispose();
            }

            return iReturn;
        }
        #endregion

        #region ==== SQL 문을 실행(ExecuteScalar) ====
        
        /// <summary>oraParameters
        /// ExecuteScalar
        /// </summary>
        /// <param name="connectionHint"></param>
        /// <param name="commandTimeout"></param>
        /// <param name="commandText"></param>
        /// <param name="oraParameters"></param>
        /// <param name="commandType"></param>
        /// <returns></returns>
        public static object ExecuteScalar(string commandText, OracleParameter[] oraParameters, CommandType commandType)
        {
            OracleConnection con = null;
            OracleCommand    cmd = null;
            object oReturn = null;

            try
            {
                cmd = new OracleCommand();
                cmd.CommandText = commandText;
                cmd.CommandType = commandType;
                cmd.CommandTimeout = commandTimeout;
                cmd.BindByName = true;

                if (oraParameters != null)
                {
                    foreach (OracleParameter param in oraParameters)
                    {
                        AddParameter(cmd, param);
                    }
                }

                //if (m_SQLTraceMode.Equals("on")) SQLTrace(m_SQLTracePath, cmd, true); // 미사용 향후 검토

                con = new OracleConnection(OracleDBDef.ConnectionString);
                con.Open();

                cmd.Connection = con;
                oReturn = cmd.ExecuteScalar();

            }
            finally
            {
                if (con != null)
                {
                    if (con.State == ConnectionState.Open)
                        con.Close();
                    con.Dispose();
                }
                if (cmd != null) cmd.Dispose();
            }
            return oReturn;
        }

        #endregion

        #region ==== SQL 문을 실행(ExecuteMultiple) ====

        /// <summary>
        /// 동일 SQL 문을 다른 입력 파라미터값으로 여러번 실행한다.
        /// </summary>
        /// <param name="connectionHint"></param>
        /// <param name="commandTimeout"></param>
        /// <param name="commandText"></param>
        /// <param name="oraParameters"></param>
        /// <param name="paramValues"></param>
        /// <param name="commandType"></param>
        /// <returns></returns>
        public static int ExecuteMultiple(string commandText, OracleParameter[] oraParameters, string[,] paramValues, CommandType commandType)
        {
            if (oraParameters == null) throw new System.Exception("");//에러 메시지 어떻게 가져올 것인가?

            int iReturn = 0;

            if (paramValues.Length > 0)
            {
                OracleConnection con = null;
                OracleCommand cmd = null;

                int iColumnCount = paramValues.GetUpperBound(1) + 1;
                int iRowCount = paramValues.GetUpperBound(0) + 1;
                int iCol = 0;
                int iRow = 0;

                string[] paramVals = new string[iColumnCount];
                for (iCol = 0; iCol < iColumnCount; iCol++)
                    paramVals[iCol] = paramValues[iRow, iCol];

                try
                {
                    cmd = new OracleCommand();
                    cmd.CommandText = commandText;
                    cmd.CommandType = commandType;
                    cmd.CommandTimeout = commandTimeout;
                    cmd.BindByName = true;

                    foreach (OracleParameter param in oraParameters)
                    {
                        AddParameter(cmd, param);
                    }

                    if (cmd != null)
                    {
                        con = new OracleConnection(OracleDBDef.ConnectionString);
                        con.Open();

                        cmd.Connection = con;
                        cmd.Prepare();

                        for (iRow = 0; iRow < iRowCount; iRow++)
                        {
                            for (iCol = 0; iCol < iColumnCount; iCol++)
                            {
                                string strValue = paramValues[iRow, iCol];
                                if ((strValue == null) || (strValue.Length == 0)) cmd.Parameters[iCol].Value = System.DBNull.Value;
                                else cmd.Parameters[iCol].Value = strValue;
                            }

                            //if (m_SQLTraceMode.Equals("on")) // 미사용 향후 검토
                            //{
                            //    if (iRow == (iRowCount - 1)) SQLTrace(m_SQLTracePath, cmd, true);
                            //    else SQLTrace(m_SQLTracePath, cmd, false);
                            //}

                            iReturn += cmd.ExecuteNonQuery();
                        }
                    }
                }
                finally
                {
                    if (con != null)
                    {
                        if (con.State == ConnectionState.Open)
                            con.Close();
                        con.Dispose();
                    }
                    if (cmd != null) cmd.Dispose();
                }
            }

            return iReturn;
        }

        #endregion
        
        #region ==== SQL 문을 실행(GetDataSet) ====
        /// <summary>
        /// DataSet 형태로 데이터 반환
        /// </summary>
        /// <param name="connectionHint"></param>
        /// <param name="commandTimeout"></param>
        /// <param name="commandText"></param>
        /// <param name="oraParameters"></param>
        /// <param name="commandType"></param>
        /// <returns></returns>
        public static DataSet GetDataSet(string commandText, OracleParameter[] oraParameters, CommandType commandType)
        {
            OracleConnection con = null;
            OracleCommand cmd = null;
            OracleDataAdapter da = null;
            DataSet dsReturn = null;


            try
            {
                cmd = new OracleCommand();
                cmd.CommandText = commandText;
                cmd.CommandType = commandType;
                cmd.CommandTimeout = commandTimeout;
                cmd.InitialLONGFetchSize = -1;
                cmd.BindByName = true;

                if (oraParameters != null)
                {
                    foreach (Oracle.DataAccess.Client.OracleParameter param in oraParameters)
                    {
                        AddParameter(cmd, param);
                    }
                }

                //if (m_SQLTraceMode.Equals("on")) SQLTrace(m_SQLTracePath, cmd, true); // 미사용 향후 검토

                con = new OracleConnection(OracleDBDef.ConnectionString);
                con.Open();

                cmd.Connection = con;

                dsReturn = new DataSet();
                da = new OracleDataAdapter(cmd);
                da.Fill(dsReturn);

            }
            finally
            {
                if (con != null)
                {
                    if (con.State == ConnectionState.Open)
                        con.Close();
                    con.Dispose();
                }
                if (cmd != null) cmd.Dispose();
                if (da != null) da.Dispose();
            }

            return dsReturn;
        }

        #endregion

        #region ==== SQL 문을 실행(ExecuteReader) ====

        /// <summary>
        /// OracleDataReader 형태로 데이터 반환
        /// </summary>
        /// <param name="connectionHint"></param>
        /// <param name="commandTimeout"></param>
        /// <param name="commandText"></param>
        /// <param name="oraParameters"></param>
        /// <param name="commandType"></param>
        /// <returns></returns>
        public static OracleDataReader ExecuteReader(string commandText, OracleParameter[] oraParameters, CommandType commandType)
        {
            OracleConnection con = null;
            OracleCommand    cmd = null;
            OracleDataReader dr = null;

            try
            {
                cmd = new OracleCommand();
                cmd.CommandText = commandText;
                cmd.CommandType = commandType;
                cmd.CommandTimeout = commandTimeout;
                cmd.BindByName = true;

                if (oraParameters != null)
                {
                    foreach (OracleParameter param in oraParameters)
                    {
                        AddParameter(cmd, param);
                    }
                }

                //if (m_SQLTraceMode.Equals("on")) SQLTrace(m_SQLTracePath, cmd, true); // 미사용 향후 검토

                con = new OracleConnection(OracleDBDef.ConnectionString);
                con.Open();

                cmd.Connection = con;
                dr = cmd.ExecuteReader(CommandBehavior.CloseConnection);

            }
            finally
            {
                //if(con != null) con.Dispose();
                if (cmd != null) cmd.Dispose();
            }

            return dr;
        }

        #endregion

        #endregion

        #region ==== AddParameter ====
        private static void AddParameter(Oracle.DataAccess.Client.OracleCommand cmd, Oracle.DataAccess.Client.OracleParameter param)
        {
            try
            {
                if ((param.Value == null) || ((param.Value.GetType().ToString().Equals("System.String")) && ((string)param.Value).Length == 0))
                    param.Value = System.DBNull.Value;
                cmd.Parameters.Add(param);
            }
            catch
            {

            }
        }
        #endregion

        #region ==== SQLTrace ==== 미사용 ==> 향후 검토 필요

        //private static void SQLTrace(string SQLTracePath, Oracle.DataAccess.Client.OracleCommand cmd, bool markEndLine)
        //{
        //    System.Text.StringBuilder oBuilder = new System.Text.StringBuilder();

        //    oBuilder.Append(System.DateTime.Now.ToString());
        //    oBuilder.Append("\r\n");
        //    oBuilder.Append(cmd.CommandText);

        //    if (cmd.Parameters.Count == 0)
        //    {
        //        oBuilder.Append(cmd.CommandText);
        //    }
        //    else
        //    {
        //        if (cmd.CommandType == CommandType.Text)
        //        {
        //            for (int iElemCnt = 0; iElemCnt < cmd.Parameters.Count; iElemCnt++)
        //            {
        //                oBuilder.Replace(cmd.Parameters[iElemCnt].ParameterName, SqlParameterValue2String(cmd.Parameters[iElemCnt].OracleDbType, cmd.Parameters[iElemCnt].Value));
        //            }
        //        }
        //        else
        //        {
        //            for (int iElemCnt = 0; iElemCnt < cmd.Parameters.Count; iElemCnt++)
        //            {
        //                oBuilder.Append("\r\n");
        //                oBuilder.Append(cmd.Parameters[iElemCnt].ParameterName);
        //                oBuilder.Append(" = ");
        //                oBuilder.Append(SqlParameterValue2String(cmd.Parameters[iElemCnt].OracleDbType, cmd.Parameters[iElemCnt].Value));
        //            }
        //        }

        //        if (markEndLine) oBuilder.Append("\r\n-------------------------------------------------------------");
        //        string sContents = oBuilder.ToString();

        //        string sFileName = string.Format("{0}_SQLQueryTrc_{1}.log", System.Net.Dns.GetHostName().ToLower(),
        //            System.DateTime.Now.ToShortDateString());

        //    }
        //    // File 로그 남기기
        //}

        #endregion

        #region ==== SqlParameterValue2String ====  미사용 ==> 향후 검토 필요
        //private static string SqlParameterValue2String(Oracle.DataAccess.Client.OracleDbType tp, object parameterValue)
        //{
        //    string strReturn = "NULL";

        //    if (parameterValue != null)
        //    {
        //        if (parameterValue == System.DBNull.Value)
        //            strReturn = "NULL";
        //        else
        //        {
        //            switch (tp)
        //            {
        //                case Oracle.DataAccess.Client.OracleDbType.Char:
        //                case Oracle.DataAccess.Client.OracleDbType.Varchar2:
        //                case Oracle.DataAccess.Client.OracleDbType.NChar:
        //                case Oracle.DataAccess.Client.OracleDbType.NVarchar2:
        //                    strReturn = string.Concat("'", (string)parameterValue, "'");
        //                    break;
        //                case Oracle.DataAccess.Client.OracleDbType.BFile:
        //                    strReturn = "<OracleBFile>";
        //                    break;
        //                case Oracle.DataAccess.Client.OracleDbType.Blob:
        //                    strReturn = "<OracleBlob>";
        //                    break;
        //                case Oracle.DataAccess.Client.OracleDbType.Clob:
        //                    strReturn = "<CLOB>";
        //                    break;
        //                case Oracle.DataAccess.Client.OracleDbType.Date:
        //                    strReturn = "<OracleDate>";
        //                    break;
        //                case Oracle.DataAccess.Client.OracleDbType.Raw:
        //                    strReturn = "<OracleBinary>";
        //                    break;
        //                case Oracle.DataAccess.Client.OracleDbType.LongRaw:
        //                    strReturn = "<OracleBinary>";
        //                    break;
        //                case Oracle.DataAccess.Client.OracleDbType.Byte:
        //                    strReturn = "<Binary>";
        //                    break;
        //                //						case Oracle.DataAccess.Client.OracleDbType.RefCursor:
        //                //							strReturn = "<OracleRefCursor>";
        //                //							break;
        //                default:
        //                    strReturn = parameterValue.ToString();
        //                    break;
        //            }
        //        }
        //    }

        //    return strReturn;
        //}
        #endregion


		#region OraAgent : GetErrorCode
		/// <summary>
		/// GetErrorCode
		/// </summary>
		/// <param name="e">Exception</param>
		/// <param name="p_strErrCode">ErrorCode(out)</param>
		/// <param name="p_strErrText">ErrorText(out)</param>
		public void GetErrorCode(Exception e, ref string p_strErrCode, ref string p_strErrText)
		{
            try
            {
                if (e.Message.Substring(0, 3) == m_strOraErr)
                {
                    p_strErrCode = "O" + e.Message.Substring(4, 5);
                    p_strErrText = e.Message.Substring(10);
                }
                else
                {
                    p_strErrCode = "AC7901";
                    p_strErrText = e.Message;
                }
            }
            catch
            {

            }
		}

		/// <summary>
		/// IsDBNoConnErrCode
		/// </summary>
		/// <param name="p_strErrCode">ErrorCode</param>
		/// <returns></returns>
		public bool IsDBNoConnErrCode(string p_strErrCode)
		{
            try
            {
                if (p_strErrCode == OracleDBDef.ORAMID_NOCONN1 ||
                    p_strErrCode == OracleDBDef.ORAMID_NOCONN2 ||
                    p_strErrCode == OracleDBDef.ORAMID_NOCONN3 ||
                    p_strErrCode == OracleDBDef.ORAMID_NOCONN4 ||
                    p_strErrCode == OracleDBDef.ORAMID_NOCONN5 ||
                    p_strErrCode == OracleDBDef.ORAMID_NOCONN6 ||
                    p_strErrCode == OracleDBDef.ORAMID_NOCONN7) return true;
            }
            catch
            {

            }
			return false;
		}

		#endregion

		#region OraAgent : MessageFormat -- 미사용
		
		/// <summary>
		/// 해당 에러 메시지를 형식에 맞게 변경시킨다. 
		/// </summary>
		/// <param name="e">Exception 변수</param>
		/// <param name="p_strTitle">Title</param>
		/// <param name="p_strAction">Action</param>
		/// <param name="p_strAdjust">Adjus</param>
		/// <param name="p_strErrCode">Error Code</param>
		/// <param name="p_strErrText">Error Text</param>
		/// <param name="p_strCondition">Condition</param>
        //public void MessageFormat( Exception e, string p_strTitle, string p_strAction, string p_strAdjust,
        //    ref string p_strErrCode, ref string p_strErrText, string p_strCondition )
        //{
        //    GetErrorCode(e, ref p_strErrCode, ref p_strErrText);
        //    p_strErrText = string.Format("{0} {1} {2} {3}", p_strTitle, p_strAction, p_strAdjust, p_strCondition) + p_strErrText;
        //}


		/// <summary>
		/// 해당 메시지를 형식에 맞게 변경시킨다.
		/// </summary>
		/// <param name="p_strTitle">Title</param>
		/// <param name="p_strAction">Action</param>
		/// <param name="p_strAdjust">Adjus</param>
		/// <param name="p_strErrText">Error Text</param>
		/// <param name="p_strCondition">Condition</param>
        //public void MessageFormat( string p_strTitle, string p_strAction, string p_strAdjust, ref string p_strErrText, string p_strCondition )
        //{
        //    p_strErrText = string.Format("{0} {1} {2} {3}", p_strTitle, p_strAction, p_strAdjust, p_strCondition) + p_strErrText;
        //}
		#endregion
    }

    public class DataCollection
    {
        private ArrayList arrlst = new ArrayList();

        public DataCollection() { }

        public void Clear()
        {
            arrlst.Clear();
        }

        public OracleParameter Add(OracleParameter p)
        {
            arrlst.Add(p);
            return p;
        }

        public OracleParameter Add(string name, object value)
        {
            OracleParameter Params = new OracleParameter(name, value);

            arrlst.Add(Params);
            return Params;
        }

        public OracleParameter Add(string name, OracleDbType oraType)
        {
            OracleParameter Params = new OracleParameter(name, oraType);

            arrlst.Add(Params);
            return Params;
        }

        public OracleParameter Add(string name, OracleDbType oraType, int size)
        {
            OracleParameter Params = new OracleParameter(name, oraType, size);

            arrlst.Add(Params);
            return Params;
        }

        public OracleParameter[] GetParameters()
        {
            OracleParameter[] Params = new OracleParameter[arrlst.Count];

            for(int i=0; i<Params.Length; i++)
            {
                Params[i] = (OracleParameter)arrlst[i];
            }

            return Params;
        }
    }
}
