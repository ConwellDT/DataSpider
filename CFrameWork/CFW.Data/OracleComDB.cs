using System;
using System.Data;
using System.Collections;
using Oracle.DataAccess.Client;
using CFW.Data;
using System.Text;
using System.Web;

namespace CFW.Data
{
	/// <summary>
	/// ComDB에 대한 요약 설명입니다.
	/// </summary>
    public class OracleComDB : CFW.Data.OracleDbAccess
    {
        //public static string ConnectionString = CFW.Configuration.ConfigManager.Default.ReadConfig("connectionStrings", "Oracle_ConnectionString");
        /// <summary>
        /// OracleComDB 생성자입니다.
        /// </summary>
        public OracleComDB()
        {
            //
            // TODO: 여기에 생성자 논리를 추가합니다.
            //
        }

        #region 2014.12.23 OHS 추가
        /// <summary>
        /// Load dictionary
        /// <summary>
        //public static DataSet GetMsgListCS()
        //{
        //    return GetMsgList("M", "C");
        //}

        //public static DataSet GetDicListCS()
        //{
        //    return GetMsgList("D", "C");
        //}

        //public static DataSet GetMsgListWeb()
        //{
        //    return GetMsgList("M", "W");
        //}

        //public static DataSet GetDicListWeb()
        //{
        //    return GetMsgList("D", "W");
        //}

        public static DataSet GetMsgList(string sSysArea)
        {
            return GetMsgList("M", sSysArea);
        }

        public static DataSet GetDicList(string sSysArea)
        {
            return GetMsgList("D", sSysArea);
        }

        public static DataSet GetMsgList(string sDataType, string sSysArea)
        {
            OracleConnection    con = null;
            OracleCommand       cmd = null;
            OracleDataAdapter   da  = null;
            DataSet             dsReturn = null;
            
            try
            {
                con = new OracleConnection(OracleDBDef.ConnectionString);
                con.Open();

                if (cmd == null) cmd = new OracleCommand();

                if (sDataType == "D")   cmd.CommandText = string.Format("SELECT TRIM(MSG_ID) AS DIC_ID, MSG_TEXT_EN_US AS DIC_NM_EN_US, MSG_TEXT_KO_KR AS DIC_NM_KO_KR, MSG_TEXT_LO_LN AS DIC_NM_LO_LN FROM C_MSG_ID_MA WHERE DATA_TYPE = '{0}' AND SYS_AREA = '{1}' ORDER BY MSG_ID", sDataType, sSysArea);
                else                    cmd.CommandText = string.Format("SELECT TRIM(MSG_ID) AS MSG_ID, MSG_TEXT_EN_US AS MSG_TEXT_EN_US, MSG_TEXT_KO_KR AS MSG_TEXT_KO_KR, MSG_TEXT_LO_LN AS MSG_TEXT_LO_LN FROM C_MSG_ID_MA WHERE DATA_TYPE = '{0}' AND SYS_AREA = '{1}' ORDER BY MSG_ID", sDataType, sSysArea);
                cmd.CommandType = CommandType.Text;
                cmd.Connection = con;

                dsReturn = new DataSet();
                da = new OracleDataAdapter(cmd);
                da.Fill(dsReturn);
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                if (con != null)
                {
                    if (con.State == ConnectionState.Open)  con.Close();
                    con.Dispose();
                }
                if (cmd != null) cmd.Dispose();
                if (da != null) da.Dispose();
            }

            return dsReturn;
        }
        #endregion


        #region DB Message 로드 (web) -- 미사용

        /// <summary>
        /// DB Message를 로드합니다.
        /// Web 로그인 시 한번만 Message 로드함  
        /// </summary>
        /// <param name="languageType">language Type</param>
        /// <param name="sErrCode">Error Code(out)</param>
        /// <param name="sErrText">Error Text(out)</param>
        /// <returns>메시지 DataSet</returns>
        //public DataSet GetMsgListWeb(string languageType, ref string sErrCode, ref string sErrText)
        //{
        //    OracleConnection con = null;
        //    OracleCommand cmd = null;
        //    OracleDataAdapter da = null;
        //    DataSet dsReturn = null;
        //    string strConfigPath = CFW.Common.Configuration.ComConfigPath;
        //    string strDefaultLang = "";
        //    string strDefaultLang_ko = "";
        //    string strDefaultLang_en = "";
        //    string strDefaultLang_zh = "";

        //    strDefaultLang = CFW.Common.Configuration.ReadConfigValue("LanguageType");
        //    if (strDefaultLang == null)
        //    {
        //        strDefaultLang = CFW.Common.Configuration.ReadConfigValue(CFW.Common.SECTION.CULTURE, CFW.Common.KEY.CUL_DEFALT, CFW.Common.Configuration.ComConfigPath);
        //    }
        //    StringBuilder sbMsg = new StringBuilder();

        //    try
        //    {
        //        // DB Connection =============================================================================
        //        string User = CFW.Common.Configuration.ReadConfigValue(SECTION.COMMON, KEY.USER, strConfigPath);
        //        string PW = CFW.Common.Configuration.ReadConfigValue(SECTION.COMMON, KEY.PASS, strConfigPath);
        //        string Alias = CFW.Common.Configuration.ReadConfigValue(SECTION.COMMON, KEY.ALIAS, strConfigPath);

        //        con = new OracleConnection("user id		= " + User + ";" +
        //            "data source	= " + Alias + ";" +
        //            "password		= " + PW);

        //        con.Open();
        //        //m_DBTrans	= m_DBCon.BeginTransaction();
        //        //===========================================================================================

        //        string strlanguageType = "MSG_TEXT";
        //        //디폴트 언어
        //        strDefaultLang = strlanguageType + "_" + strDefaultLang.ToUpper().Replace("-", "_");
        //        //웹에서 전체 언어 데이터를 다 메모리에 로드함 
        //        //현재 테이블 구조와 동일하게  한국어, 영어, 중국어를 관리 할수 있으며 기타 이외의 언어를 
        //        //사용하는 경우 프레임워크의 수정이 필요함 ( 또는 기본을 한국어, 영어, 기타 언어로 설정 ) 
        //        strDefaultLang_ko = strlanguageType + "_KO_KR";
        //        strDefaultLang_en = strlanguageType + "_EN_US";
        //        strDefaultLang_zh = strlanguageType + "_ZH_CN";
        //        string strSelectLang = strDefaultLang_ko + "," + strDefaultLang_en + "," + strDefaultLang_zh;

        //        strlanguageType = strlanguageType + "_" + languageType.ToUpper().Replace("-", "_");
        //        cmd = new OracleCommand();
        //        sbMsg.Append("SELECT msg_id , msg_grp,");
        //        sbMsg.Append(strDefaultLang);
        //        sbMsg.Append(",db_log_flg,");
        //        sbMsg.Append(strlanguageType);
        //        sbMsg.Append(",msg_type ,");
        //        sbMsg.Append(strSelectLang);
        //        sbMsg.Append(" FROM C_MSG_ID_CD ");
        //        sbMsg.Append(" ORDER BY msg_id");
        //        cmd.CommandText = sbMsg.ToString();
        //        cmd.CommandType = CommandType.Text;
        //        cmd.Connection = con;
        //        dsReturn = new DataSet();
        //        da = new OracleDataAdapter(cmd);
        //        da.Fill(dsReturn);
        //    }
        //    catch (Exception e)
        //    {
        //        GetErrorCode(e, ref sErrCode, ref sErrText);
        //    }
        //    finally
        //    {
        //        if (con != null)
        //        {
        //            if (con.State == ConnectionState.Open)
        //                con.Close();
        //            con.Dispose();
        //        }
        //        if (cmd != null) cmd.Dispose();
        //        if (da != null) da.Dispose();
        //    }
        //    return dsReturn;
        //}

        /// <summary>
        /// DB Message를 로드합니다.
        /// Web 로그인 시 한번만 Dictionary 로드함  (로그인시 접속할 DB 지정 ) 
        /// </summary>
        /// <param name="languageType">language Type</param>
        /// <param name="languageType">DB Type</param>
        /// <param name="sErrCode">Error Code(out)</param>
        /// <param name="sErrText">Error Text(out)</param>
        /// <returns>메시지 DataSet</returns>
        public DataSet GetMsgListWeb(string languageType, string dbType, ref string sErrCode, ref string sErrText)
        {
            OracleConnection con = null;
            OracleCommand cmd = null;
            OracleDataAdapter da = null;
            DataSet dsReturn = null;
            string strDefaultLang = "";
            string strDefaultLang_ko = "";
            string strDefaultLang_en = "";
            string strDefaultLang_zh = "";

            strDefaultLang = CFW.Configuration.ConfigManager.Default.ReadConfig("Language","LanguageType");
            if (strDefaultLang == null)
            {
                strDefaultLang = CFW.Configuration.ConfigManager.Default.ReadConfig("CULTURE", "CUL_DEFALT");
            }
            StringBuilder sbMsg = new StringBuilder();
            // DB Connection =============================================================================
            string User = "";
            string PW = "";
            string Alias = "";

            try
            {
                //if(dbType.Equals("V"))
                //{
                //    User = "";//CFW.Common.Configuration.ReadConfigValue(SECTION.VEHICLE,KEY.USER,strConfigPath);
                //    PW = "";//CFW.Common.Configuration.ReadConfigValue(SECTION.VEHICLE,KEY.PASS,strConfigPath);
                //    Alias = "";//CFW.Common.Configuration.ReadConfigValue(SECTION.VEHICLE,KEY.ALIAS,strConfigPath);
                //}
                //else if(dbType.Equals("E"))
                //{
                //    User = "";//CFW.Common.Configuration.ReadConfigValue(SECTION.ENGINE,KEY.USER,strConfigPath);
                //    PW = "";//CFW.Common.Configuration.ReadConfigValue(SECTION.ENGINE,KEY.PASS,strConfigPath);
                //    Alias = "";//CFW.Common.Configuration.ReadConfigValue(SECTION.ENGINE,KEY.ALIAS,strConfigPath);
                //}
                //else if(dbType.Equals("P"))
                //{
                //    User = "";//CFW.Common.Configuration.ReadConfigValue(SECTION.PEMS,KEY.USER,strConfigPath);
                //    PW = "";//CFW.Common.Configuration.ReadConfigValue(SECTION.PEMS,KEY.PASS,strConfigPath);
                //    Alias = "";//CFW.Common.Configuration.ReadConfigValue(SECTION.PEMS,KEY.ALIAS,strConfigPath);
                //}
                //else if(dbType.Equals("S"))
                //{
                //    User = "";//CFW.Common.Configuration.ReadConfigValue(SECTION.PRESS,KEY.USER,strConfigPath);
                //    PW = "";//CFW.Common.Configuration.ReadConfigValue(SECTION.PRESS,KEY.PASS,strConfigPath);
                //    Alias = "";//CFW.Common.Configuration.ReadConfigValue(SECTION.PRESS,KEY.ALIAS,strConfigPath);
                //}
                //else
                //{
                // DB Connection =============================================================================
                //User = CFW.Config.ReadConfigValue(SECTION.COMMON, KEY.USER, strConfigPath);
                //PW = CFW.Config.ReadConfigValue(SECTION.COMMON, KEY.PASS, strConfigPath);
                //Alias = CFW.Config.ReadConfigValue(SECTION.COMMON, KEY.ALIAS, strConfigPath);
                //}

                con = new OracleConnection(OracleDBDef.ConnectionString);

                con.Open();
                //m_DBTrans	= m_DBCon.BeginTransaction();
                //===========================================================================================

                string strlanguageType = "MSG_TEXT";
                //디폴트 언어
                strDefaultLang = strlanguageType + "_" + strDefaultLang.ToUpper().Replace("-", "_");
                //웹에서 전체 언어 데이터를 다 메모리에 로드함 (2007.08.29 by 한주희) 
                //현재 테이블 구조와 동일하게  한국어, 영어, 중국어를 관리 할수 있으며 기타 이외의 언어를 
                //사용하는 경우 프레임워크의 수정이 필요함 ( 또는 기본을 한국어, 영어, 기타 언어로 설정 ) 
                strDefaultLang_ko = strlanguageType + "_KO_KR";
                strDefaultLang_en = strlanguageType + "_EN_US";
                strDefaultLang_zh = strlanguageType + "_ZH_CN";
                string strSelectLang = strDefaultLang_ko + "," + strDefaultLang_en + "," + strDefaultLang_zh;

                strlanguageType = strlanguageType + "_" + languageType.ToUpper().Replace("-", "_");
                cmd = new OracleCommand();
                sbMsg.Append("SELECT msg_id , msg_grp,");
                sbMsg.Append(strDefaultLang);
                sbMsg.Append(",db_log_flg,");
                sbMsg.Append(strlanguageType);
                sbMsg.Append(",msg_type ,");
                sbMsg.Append(strSelectLang);
                sbMsg.Append(" FROM C_MSG_ID_CD ");
                sbMsg.Append(" ORDER BY msg_id");
                cmd.CommandText = sbMsg.ToString();
                cmd.CommandType = CommandType.Text;
                cmd.Connection = con;
                dsReturn = new DataSet();
                da = new OracleDataAdapter(cmd);
                da.Fill(dsReturn);
            }
            catch (Exception e)
            {
                GetErrorCode(e, ref sErrCode, ref sErrText);
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

        #region DB Dictionary 로드	(web) - 미사용

        /// <summary>
        /// Dictionary를 로드합니다.
        /// </summary>
        /// <param name="languageType">language Type</param>
        /// <returns>Dictionary DataSet</returns>
        //public DataSet GetDicListWeb(string languageType)
        //{
        //    //string strConfigPath = CFW.Config.ComConfigPath;
        //    string m_strDefaultLang = CFW.Configuration.ConfigManager.Default.ReadConfig("CULTURE", "CULTURE_DEFALT");
        //    string slanguageType = "";
        //    OracleConnection con = null;
        //    OracleCommand cmd = null;
        //    OracleDataAdapter da = null;
        //    DataSet dsReturn = null;
        //    try
        //    {
        //        // DB Connection =============================================================================
        //        //string User = CFW.Config.ReadConfigValue(SECTION.COMMON, KEY.USER, strConfigPath);
        //        //string PW = CFW.Config.ReadConfigValue(SECTION.COMMON, KEY.PASS, strConfigPath);
        //        //string Alias = CFW.Config.ReadConfigValue(SECTION.COMMON, KEY.ALIAS, strConfigPath);
        //        StringBuilder sbDicList = new StringBuilder();

        //        con = new OracleConnection(OracleDBDef.ConnectionString);

        //        con.Open();
        //        //m_DBTrans	= m_DBCon.BeginTransaction();
        //        //===========================================================================================

        //        string strlanguageType = "DIC_NM";


        //        if (m_strDefaultLang.Length > 0)
        //        {
        //            slanguageType = strlanguageType + "_" + m_strDefaultLang.ToUpper().Replace("-", "_"); //config defult 언어  
        //        }

        //        //				if(languageType.Length > 0)
        //        //				{
        //        //					string [] arrDicList = null;
        //        //					arrDicList = languageType.Split(',');
        //        //					
        //        //					for(int i = 0; i< arrDicList.Length; i++)
        //        //					{
        //        //						if(i>0)
        //        //						{
        //        //							sbDicList.Append(","+slanguageType +"_"+arrDicList[i].ToString().ToUpper().Replace("-","_"));
        //        //						}
        //        //						else sbDicList.Append(slanguageType +"_"+arrDicList[i].ToString().ToUpper().Replace("-","_"));
        //        //					}
        //        //					
        //        //				}
        //        //웹에서 전체 언어 데이터를 다 메모리에 로드함
        //        string strDefaultLang_ko = strlanguageType + "_KO_KR";
        //        string strDefaultLang_en = strlanguageType + "_EN_US";
        //        string strSelectLang = slanguageType + "," + strDefaultLang_ko + "," + strDefaultLang_en;


        //        if (cmd == null) cmd = new OracleCommand();
        //        cmd.CommandText = string.Format("SELECT DIC_ID," + strSelectLang +
        //            " FROM DICTIONARY_CD " +
        //            " ORDER BY DIC_ID ");
        //        cmd.CommandType = CommandType.Text;
        //        cmd.Connection = con;

        //        dsReturn = new DataSet();
        //        da = new OracleDataAdapter(cmd);
        //        da.Fill(dsReturn);
        //        return dsReturn;
        //    }
        //    catch
        //    {
        //        return dsReturn;
        //    }
        //    finally
        //    {
        //        if (con != null)
        //        {
        //            if (con.State == ConnectionState.Open)
        //                con.Close();
        //            con.Dispose();
        //        }
        //        if (cmd != null) cmd.Dispose();
        //        if (da != null) da.Dispose();
        //    }


        //    //			OracleDataReader reDataReader = null; //"Common Info","CultureInfo","\\PROJECT\\G-MES\\Source\\Config\\ComConfig.cfg"
        //    //			
        //    //			//공통 계정 추가 - 환경설정 파일에서 커넥션 스트링 값 가져옴
        //    //			string cfgPath = Configuration.CommonCfgPath;
        //    //			string User = CFW.Common.Configuration.ReadConfigValue("Common Info","USER",cfgPath);
        //    //			string PW = CFW.Common.Configuration.ReadConfigValue("Common Info","PASSWORD",cfgPath);
        //    //			string Alias = CFW.Common.Configuration.ReadConfigValue("Common Info","Alias",cfgPath);
        //    //			
        //    //			string slanguageType = "DIC_NM";
        //    //			if(languageType.Length > 0)
        //    //			{
        //    //				slanguageType = slanguageType +"_"+languageType.ToUpper().Replace("-","_");
        //    //			}
        //    //			if(DBConnect(User,PW,Alias))
        //    //			{
        //    //				m_DBCmd	= m_DBCon.CreateCommand();
        //    //				m_DBCmd.CommandType = CommandType.Text;
        //    //				m_DBCmd.CommandText	= string.Format( 
        //    //					"SELECT DIC_ID," + slanguageType +
        //    //					" FROM C_DICTIONARY_DF "	+				
        //    //					" ORDER BY DIC_ID ");			
        //    //				reDataReader = m_DBCmd.ExecuteReader();
        //    //			}		
        //    //			return reDataReader;
        //}

        /// <summary>
        /// Dictionary를 로드합니다.
        /// Web 로그인 시 한번만 Dictionary 로드함  (HMI 요구 사항 로그인시 접속할 DB 지정 ) 
        /// </summary>
        /// <param name="languageType">language Type</param>
        /// <param name="dbType">DB Type</param>
        /// <returns>Dictionary DataSet</returns>
        //public DataSet GetDicListWeb(string languageType, string dbType)
        //{
        //    //string strConfigPath = CFW.Config.ComConfigPath;
        //    string m_strDefaultLang = CFW.Configuration.ConfigManager.Default.ReadConfig("CULTURE", "CULTURE_DEFALT");
        //    string slanguageType = "";
        //    OracleConnection con = null;
        //    OracleCommand cmd = null;
        //    OracleDataAdapter da = null;
        //    DataSet dsReturn = null;

        //    //string User = "";
        //    //string PW = "";
        //    //string Alias = "";

        //    try
        //    {
        //        StringBuilder sbDicList = new StringBuilder();

        //        //if(dbType.Equals("V"))
        //        //{
        //        //    User = "";//CFW.Common.Configuration.ReadConfigValue(SECTION.VEHICLE,KEY.USER,strConfigPath);
        //        //    PW = "";//CFW.Common.Configuration.ReadConfigValue(SECTION.VEHICLE,KEY.PASS,strConfigPath);
        //        //    Alias = "";//CFW.Common.Configuration.ReadConfigValue(SECTION.VEHICLE,KEY.ALIAS,strConfigPath);
        //        //}
        //        //else if(dbType.Equals("E"))
        //        //{
        //        //    User = "";//CFW.Common.Configuration.ReadConfigValue(SECTION.ENGINE,KEY.USER,strConfigPath);
        //        //    PW = "";//CFW.Common.Configuration.ReadConfigValue(SECTION.ENGINE,KEY.PASS,strConfigPath);
        //        //    Alias = "";//CFW.Common.Configuration.ReadConfigValue(SECTION.ENGINE,KEY.ALIAS,strConfigPath);
        //        //}
        //        //else if(dbType.Equals("P"))
        //        //{
        //        //    User = "";//CFW.Common.Configuration.ReadConfigValue(SECTION.PEMS,KEY.USER,strConfigPath);
        //        //    PW = "";//CFW.Common.Configuration.ReadConfigValue(SECTION.PEMS,KEY.PASS,strConfigPath);
        //        //    Alias = "";//CFW.Common.Configuration.ReadConfigValue(SECTION.PEMS,KEY.ALIAS,strConfigPath);
        //        //}
        //        //else if(dbType.Equals("S"))
        //        //{
        //        //    User = "";//CFW.Common.Configuration.ReadConfigValue(SECTION.PRESS,KEY.USER,strConfigPath);
        //        //    PW = "";//CFW.Common.Configuration.ReadConfigValue(SECTION.PRESS,KEY.PASS,strConfigPath);
        //        //    Alias = "";//CFW.Common.Configuration.ReadConfigValue(SECTION.PRESS,KEY.ALIAS,strConfigPath);
        //        //}
        //        //else
        //        //{
        //        // DB Connection =============================================================================
        //        //User = CFW.Config.ReadConfigValue(SECTION.COMMON, KEY.USER, strConfigPath);
        //        //PW = CFW.Config.ReadConfigValue(SECTION.COMMON, KEY.PASS, strConfigPath);
        //        //Alias = CFW.Config.ReadConfigValue(SECTION.COMMON, KEY.ALIAS, strConfigPath);
        //        //}



        //        con = new OracleConnection(OracleDBDef.ConnectionString);

        //        con.Open();
        //        //m_DBTrans	= m_DBCon.BeginTransaction();
        //        //===========================================================================================

        //        string strlanguageType = "DIC_NM";


        //        if (m_strDefaultLang.Length > 0)
        //        {
        //            slanguageType = strlanguageType + "_" + m_strDefaultLang.ToUpper().Replace("-", "_"); //config defult 언어  
        //        }

        //        //웹에서 전체 언어 데이터를 다 메모리에 로드함 (2007.08.29 by 한주희) 
        //        //현재 테이블 구조와 동일하게  한국어, 영어, 중국어를 관리 할수 있으며 기타 이외의 언어를 
        //        //사용하는 경우 프레임워크의 수정이 필요함 ( 또는 기본을 한국어, 영어, 기타 언어로 설정 ) 
        //        string strDefaultLang_ko = strlanguageType + "_KO_KR";
        //        string strDefaultLang_en = strlanguageType + "_EN_US";
        //        string strDefaultLang_zh = strlanguageType + "_ZH_CN";
        //        string strSelectLang = slanguageType + "," + strDefaultLang_ko + "," + strDefaultLang_en + "," + strDefaultLang_zh;


        //        if (cmd == null) cmd = new OracleCommand();
        //        cmd.CommandText = string.Format("SELECT DIC_ID," + strSelectLang +
        //            " FROM DICTIONARY_CD " +
        //            " ORDER BY DIC_ID ");
        //        cmd.CommandType = CommandType.Text;
        //        cmd.Connection = con;

        //        dsReturn = new DataSet();
        //        da = new OracleDataAdapter(cmd);
        //        da.Fill(dsReturn);
        //        return dsReturn;
        //    }
        //    catch
        //    {
        //        return dsReturn;
        //    }
        //    finally
        //    {
        //        if (con != null)
        //        {
        //            if (con.State == ConnectionState.Open)
        //                con.Close();
        //            con.Dispose();
        //        }
        //        if (cmd != null) cmd.Dispose();
        //        if (da != null) da.Dispose();
        //    }
        //}

        #endregion

        #region DB Message 로드 (cs) - 미사용

        /// <summary>
        /// DB Message를 로드합니다.
        /// </summary>
        /// <param name="languageType">language Type</param>
        /// <param name="sErrCode">Error Code(out)</param>
        /// <param name="sErrText">Error Text(out)</param>
        /// <returns>메시지 DataSet</returns>
        //public DataSet GetMsgList(string languageType, ref string sErrCode, ref string sErrText)
        //{
        //    OracleConnection con = null;
        //    OracleCommand cmd = null;
        //    OracleDataAdapter da = null;
        //    DataSet dsReturn = null;
        //    //string strConfigPath = CFW.Config.ComConfigPath;
        //    string strDefaultLang = "";

        //    strDefaultLang = CFW.Configuration.ConfigManager.Default.ReadConfig("Language","LanguageType");
        //    if (strDefaultLang == null)
        //    {
        //        strDefaultLang = CFW.Configuration.ConfigManager.Default.ReadConfig("CULTURE", "CUL_DEFALT");
        //    }
        //    StringBuilder sbMsg = new StringBuilder();
        //    try
        //    {
        //        // DB Connection =============================================================================
        //        //string User = CFW.Config.ReadConfigValue(SECTION.COMMON, KEY.USER, strConfigPath);
        //        //string PW = CFW.Config.ReadConfigValue(SECTION.COMMON, KEY.PASS, strConfigPath);
        //        //string Alias = CFW.Config.ReadConfigValue(SECTION.COMMON, KEY.ALIAS, strConfigPath);

        //        con = new OracleConnection(OracleDBDef.ConnectionString);
        //        con.Open();
        //        //m_DBTrans	= m_DBCon.BeginTransaction();
        //        //===========================================================================================

        //        string strlanguageType = "msg_text";
        //        //디폴트 언어
        //        strDefaultLang = strlanguageType + "_" + strDefaultLang.ToUpper().Replace("-", "_");
        //        strlanguageType = strlanguageType + "_" + languageType.ToUpper().Replace("-", "_");
        //        cmd = new OracleCommand();
        //        sbMsg.Append("SELECT msg_id , msg_grp,");
        //        sbMsg.Append(strDefaultLang);
        //        sbMsg.Append(",db_log_flg,");
        //        sbMsg.Append(strlanguageType);
        //        sbMsg.Append(",msg_type FROM C_MSG_ID_CD ");
        //        sbMsg.Append(" ORDER BY msg_id");
        //        cmd.CommandText = sbMsg.ToString();
        //        cmd.CommandType = CommandType.Text;
        //        cmd.Connection = con;
        //        dsReturn = new DataSet();
        //        da = new OracleDataAdapter(cmd);
        //        da.Fill(dsReturn);
        //    }
        //    catch (Exception e)
        //    {
        //        GetErrorCode(e, ref sErrCode, ref sErrText);
        //    }
        //    finally
        //    {
        //        if (con != null)
        //        {
        //            if (con.State == ConnectionState.Open)
        //                con.Close();
        //            con.Dispose();
        //        }
        //        if (cmd != null) cmd.Dispose();
        //        if (da != null) da.Dispose();
        //    }
        //    return dsReturn;
        //}
        #endregion

        #region DB Dictionary 로드	(cs) - 미사용

        /// <summary>
        /// Dictionary를 로드합니다.
        /// </summary>
        /// <param name="languageType">language Type</param>
        /// <returns>Dictionary DataSet</returns>
        //public DataSet GetDicList(string languageType)
        //{
        //    //string strConfigPath = CFW.Config.ComConfigPath;
        //    OracleConnection con = null;
        //    OracleCommand cmd = null;
        //    OracleDataAdapter da = null;
        //    DataSet dsReturn = null;
        //    try
        //    {
        //        // DB Connection =============================================================================
        //        //string User = CFW.Config.ReadConfigValue(SECTION.COMMON, KEY.USER, strConfigPath);
        //        //string PW = CFW.Config.ReadConfigValue(SECTION.COMMON, KEY.PASS, strConfigPath);
        //        //string Alias = CFW.Config.ReadConfigValue(SECTION.COMMON, KEY.ALIAS, strConfigPath);
        //        StringBuilder sbDicList = new StringBuilder();

        //        con = new OracleConnection(OracleDBDef.ConnectionString);

        //        con.Open();
        //        //m_DBTrans	= m_DBCon.BeginTransaction();
        //        //===========================================================================================

        //        string slanguageType = "DIC_NM";

        //        if (languageType.Length > 0)
        //        {
        //            string[] arrDicList = null;
        //            arrDicList = languageType.Split(',');

        //            for (int i = 0; i < arrDicList.Length; i++)
        //            {
        //                if (i > 0)
        //                {
        //                    sbDicList.Append("," + slanguageType + "_" + arrDicList[i].ToString().ToUpper().Replace("-", "_"));
        //                }
        //                else sbDicList.Append(slanguageType + "_" + arrDicList[i].ToString().ToUpper().Replace("-", "_"));
        //            }

        //        }
        //        if (cmd == null) cmd = new OracleCommand();
        //        cmd.CommandText = string.Format("SELECT DIC_ID," + sbDicList.ToString() +
        //            " FROM DICTIONARY_CD " +
        //            " ORDER BY DIC_ID ");
        //        cmd.CommandType = CommandType.Text;
        //        cmd.Connection = con;

        //        dsReturn = new DataSet();
        //        da = new OracleDataAdapter(cmd);
        //        da.Fill(dsReturn);
        //        return dsReturn;
        //    }
        //    catch
        //    {
        //        return dsReturn;
        //    }
        //    finally
        //    {
        //        if (con != null)
        //        {
        //            if (con.State == ConnectionState.Open)
        //                con.Close();
        //            con.Dispose();
        //        }
        //        if (cmd != null) cmd.Dispose();
        //        if (da != null) da.Dispose();
        //    }

        //}
        #endregion        
    }
}
