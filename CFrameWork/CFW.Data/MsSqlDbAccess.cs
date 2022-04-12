using System;
using System.Data;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Sql;
using System.Data.SqlClient;

namespace CFW.Data
{
    public class MsSqlDbAccess
    {
        protected SqlConnection mCon;
        protected SqlCommand mCommand;
        protected SqlDataReader mReader;
        protected SqlTransaction mTrans;

        protected string mConStr;
        protected int mRows;
        protected const int commandTimeout = 30;

        public MsSqlDbAccess() { }

        public bool DBConnect()
        {
            try
            {
                this.mConStr = MsSqlDbDef.ConnectionString;
                DBConnect(this.mConStr);
            }
            catch
            {
                return false;
            }

            return this.mCon.State == ConnectionState.Open;
        }

        public bool DBConnect(string ServerIP, string ServerPort, string mDataSour, string UserID, string PassWord, ref string strErrText)
        {
            try
            {
                this.mConStr = string.Format("Data Source={0};Initial Catalog={1};User ID={2};Password={3}", ServerIP + "," + ServerPort, mDataSour, UserID, PassWord);
                DBConnect(this.mConStr);
            }
            catch
            {
                return false;
            }

            return this.mCon.State == ConnectionState.Open;
        }

        public bool DBConnect(string p_strConnectionString)
        {
            try
            {
                this.mCon = new SqlConnection(p_strConnectionString);
                this.mCon.Open();

                this.mTrans = this.mCon.BeginTransaction();
            }
            catch
            {
                return false;
            }

            return this.mCon.State == ConnectionState.Open;
        }

        private void TransactionDBConnect(string p_strUSER, string p_strPW, string p_strAlias)
        {
            try
            {
                mCon = new SqlConnection("user id		= " + p_strUSER + ";" +
                                           "data source	= " + p_strAlias + ";" +
                                           "password	= " + p_strPW);

                mCon.Open();
                mTrans = mCon.BeginTransaction();
            }
            catch { }

        }

        public static bool DBConnectCheck()
        {
            bool bConnStat = false;

            SqlConnection conn = null;
            SqlCommand cmd = null;
            SqlDataReader dr = null;

            try
            {
                string sDate = "";
                cmd = new SqlCommand();
                cmd.CommandText = "SELECT SYSDATE()";
                cmd.CommandType = CommandType.Text;
                cmd.CommandTimeout = commandTimeout;
                //cmd.BindByName = true;

                conn = new SqlConnection(MsSqlDbDef.ConnectionString);
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
                {
                    dr.Dispose();
                }

                if (!bConnStat)
                {
                    if (conn.State == ConnectionState.Open)
                    {
                        conn.Close();
                    }
                }
            }

            return bConnStat;
        }

        public bool DBConnectState()
        {
            bool bConnStat = false;

            if (mCon == null || mCon.State == ConnectionState.Closed) return false;

            try
            {
                string sDate = "";
                mCommand = mCon.CreateCommand();
                mCommand.CommandType = CommandType.Text;
                mCommand.CommandText = "SELECT SYSDATE()";
                mReader = mCommand.ExecuteReader();

                while (mReader.Read())
                {
                    sDate = mReader[0].ToString();
                    bConnStat = true;

                    break;
                }
            }
            catch
            {
                bConnStat = false;
            }
            finally
            {
                mCommand.Dispose();

                if (mReader != null)
                {
                    mReader.Dispose();
                }

                if (!bConnStat)
                {
                    DisConnect();
                }
            }

            return bConnStat;
        }

        public void Commit()
        {
            try
            {
                mTrans.Commit();
                mTrans = mCon.BeginTransaction();
            }
            catch
            {
                return;
            }
        }

        public void Commit(ref string p_strErrCode, ref string p_strErrText)
        {
            try
            {
                mTrans.Commit();
                mTrans = mCon.BeginTransaction();
            }
            catch (Exception ex)
            {
                GetErrorCode(ex, ref p_strErrCode, ref p_strErrText);
                return;
            }
        }

        public void RollBack()
        {
            try
            {
                this.mTrans.Rollback();
                this.mTrans = mCon.BeginTransaction();
            }
            catch
            {
                return;
            }
        }

        public void RollBack(ref string p_strErrCode, ref string p_strErrText)
        {
            try
            {
                this.mTrans.Rollback();
                this.mTrans = mCon.BeginTransaction();
            }
            catch (Exception ex)
            {
                GetErrorCode(ex, ref p_strErrCode, ref p_strErrText);
                return;
            }
        }

        public bool DisConnect()
        {
            try
            {
                if (this.mTrans != null)
                {
                    this.mTrans.Dispose();
                }

                if (this.mCon != null &&
                    this.mCon.State == ConnectionState.Open)
                {
                    this.mCon.Close();
                    this.mCon.Dispose();
                }
            }
            catch
            {
                return false;
            }

            return true;
        }

        public void GetErrorCode(Exception ex, ref string p_strErrCode, ref string p_strErrText)
        {
            try
            {
                //if (e.Message.Substring(0, 3) == m_strOraErr)
                //{
                //    p_strErrCode = "O" + e.Message.Substring(4, 5);
                //    p_strErrText = e.Message.Substring(10);
                //}
                //else
                //{
                //    p_strErrCode = "AC7901";
                //    p_strErrText = e.Message;
                //}

                p_strErrText = ex.ToString();
            }
            catch { }

        }

        public static bool ExecuteNonQuery(string commandText, SqlParameter[] oraParameters, CommandType commandType, ref string p_strErrCode, ref string p_strErrText)
        {
            SqlConnection mCon = null;
            SqlCommand mCommand = null;
            int mRows = 0;
            try
            {
                mCommand = new SqlCommand();
                mCommand.CommandType = commandType;
                mCommand.CommandText = commandText;

                if (oraParameters != null)
                {
                    foreach (SqlParameter param in oraParameters)
                    {
                        mCommand.Parameters.Add(param);
                    }
                }

                mCon = new SqlConnection(MsSqlDbDef.ConnectionString);
                mCon.Open();

                mCommand.Connection = mCon;

                mRows = mCommand.ExecuteNonQuery();

                if (mRows == 0)
                {
                    return false;
                }
            }
            finally
            {
                if (mCon != null)
                {
                    if (mCon.State == ConnectionState.Open)
                        mCon.Close();
                    mCon.Dispose();
                }
                if (mCommand != null) mCommand.Dispose();
            }

            return true;
        }

        public int ExecuteMultiple(string commandText, SqlParameter[] oraParameters, string[,] paramValues, CommandType commandType, ref string p_strErrCode, ref string p_strErrText)
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
                {
                    paramVals[iCol] = paramValues[iRow, iCol];
                }

                try
                {
                    mCommand = mCon.CreateCommand();
                    mCommand.CommandType = commandType;
                    mCommand.CommandText = commandText;
                    //m_DBCmd.BindByName = true;

                    foreach (SqlParameter param in oraParameters)
                    {
                        mCommand.Parameters.Add(param);
                    }

                    if (mCommand != null)
                    {
                        for (iRow = 0; iRow < iRowCount; iRow++)
                        {
                            for (iCol = 0; iCol < iColumnCount; iCol++)
                            {
                                string strValue = paramValues[iRow, iCol];

                                if ((strValue == null) || (strValue.Length == 0))
                                {
                                    mCommand.Parameters[iCol].Value = System.DBNull.Value;
                                }
                                else
                                {
                                    mCommand.Parameters[iCol].Value = strValue;
                                }
                            }

                            mRows += mCommand.ExecuteNonQuery();

                            if (mRows == 0)
                            {
                                //p_strErrCode = OracleDBDef.ORAMID_NOFOUND;
                                return 0;
                            }

                            iReturn += mRows;
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
                    mCommand.Dispose();
                }
            }

            return iReturn;
        }


        public SqlDataReader ExecuteReader(string commandText, SqlParameter[] MsSqlParameters, CommandType commandType, ref string p_strErrCode, ref string p_strErrText)
        {
            try
            {
                mCommand = mCon.CreateCommand();
                mCommand.CommandType = commandType;
                mCommand.CommandText = commandText;
                //mCommand.InitialLOBFetchSize = -1;
                //mCommand.InitialLONGFetchSize = -1;
                //mCommand.BindByName = true;

                if (MsSqlParameters != null)
                {
                    foreach (SqlParameter param in MsSqlParameters)
                    {
                        mCommand.Parameters.Add(param);
                    }
                }

                mReader = mCommand.ExecuteReader();

                return mReader;
            }
            catch (Exception ex)
            {
                GetErrorCode(ex, ref p_strErrCode, ref p_strErrText);

                return null;
            }
            finally
            {
                mCommand.Dispose();
            }
        }

        private object ExecuteScalarObject(string commandText, SqlParameter[] oraParameters, CommandType commandType)
        {
            object objRet = null;

            try
            {
                mCommand = mCon.CreateCommand();
                mCommand.CommandType = commandType;
                mCommand.CommandText = commandText;
                //mCommand.BindByName = true;

                if (oraParameters != null)
                {
                    foreach (SqlParameter param in oraParameters)
                    {
                        mCommand.Parameters.Add(param);
                    }
                }

                objRet = mCommand.ExecuteScalar();
                mCommand.Dispose();
            }
            catch { }

            return objRet;
        }

        public bool ExecuteScalar(string commandText, SqlParameter[] oraParameters, CommandType commandType, ref string p_strValue, ref string p_strErrCode, ref string p_strErrText)
        {
            object objRet = null;

            try
            {
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
            catch (Exception ex)
            {
                GetErrorCode(ex, ref p_strErrCode, ref p_strErrText);

                return false;
            }
            finally
            {
                mCommand.Dispose();
            }

            return true;
        }

        public static DataSet GetDataSet(string commandText, SqlParameter[] oraParameters, CommandType commandType, ref string p_strErrCode, ref string p_strErrText)
        {
            SqlConnection mCon = null;
            SqlCommand mCommand = null;
            SqlDataAdapter da = null;
            DataSet dsReturn = new DataSet();

            try
            {
                mCommand = new SqlCommand();
                mCommand.CommandType = commandType;
                mCommand.CommandText = commandText;
                //mCommand.BindByName = true;

                if (oraParameters != null)
                {
                    foreach (SqlParameter param in oraParameters)
                    {
                        mCommand.Parameters.Add(param);
                    }
                }

                mCon = new SqlConnection(MsSqlDbDef.ConnectionString);
                mCon.Open();

                mCommand.Connection = mCon;

                dsReturn = new DataSet();
                da = new SqlDataAdapter(mCommand);
                da.Fill(dsReturn);

            }
            catch (Exception ex)
            {
                p_strErrCode = ex.ToString();
            }
            finally
            {
                if (mCon != null)
                {
                    if (mCon.State == ConnectionState.Open)
                        mCon.Close();
                    mCon.Dispose();
                }
                if (mCommand != null) mCommand.Dispose();
                if (da != null) da.Dispose();
            }

            return dsReturn;
        }


    }
}
