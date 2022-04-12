using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Data;
using System.Collections;
using System.EnterpriseServices;
using System.Xml;
//using System.Windows.Forms;
using System.Web;
using System.Data.SqlClient;
//using Oracle.DataAccess.Client;

namespace CFW.Common
{
	/// <summary>
	/// Dictionary에 대한 요약 설명입니다.
	/// </summary>
	public class Dictionary
	{
		#region ==== 변수 선언 ====

		public static Hashtable htDicList = null;
		public static Hashtable htControl= new Hashtable();
		//private static Hashtable htLangType= new Hashtable();

        public static string     m_ComConfigPath = "";//CFW.Config.ComConfigPath;
		
		private static ArrayList alKeys     = null;
		private static ArrayList alValues   = null;

        private static CFW.Data.OracleComDB m_ComDB = new CFW.Data.OracleComDB();
		//private static Cookie oCookie             = new Cookie(); 
		private static string   m_BaseFormNM = "";
		public  static DataView dvDicList    = null;
		private static bool     bDicLoad     = false;

        //private static string   m_strDefaultLang    = CFW.Configuration.ConfigManager.Default.ReadConfig("CULTURE", "CULTURE_DEFALT");
        //private static string   m_strWebDefaultLang = CFW.Configuration.ConfigManager.Default.ReadConfig("Language","LanguageType");

		#endregion

		/// <summary>
		/// 생성자입니다.
		/// </summary>
		public Dictionary()
		{
			//
			// TODO: 여기에 생성자 논리를 추가합니다.
			//
		}

		#region ==== GetLanguageList ====  미사용
        ///// <summary>
        ///// 다국어 리스트를 읽어와 DataTable을 생성합니다.
        ///// </summary>
        ///// <param name="langType">언어 타입</param>
        ///// <returns>다국어 리스트 DataTable </returns>
        //public static DataTable GetLanguageList(string langType)
        //{
        //    DataTable dt = new DataTable();
        //    string strLanguageList = string.Empty;
        //    strLanguageList = CFW.Configuration.ConfigManager.Default.ReadConfig("CULTURE", "CUL_LIST");
        //    string [] TempLanguageList = strLanguageList.Split(',');
        //    dt.Columns.Add("DisplayMember");
        //    dt.Columns.Add("ValueMember");
        //    for(int i = 0; i<TempLanguageList.Length; i++)
        //    {
        //        DataRow dr = dt.NewRow();
        //        string strTemp = SearchDic(langType,TempLanguageList[i]);
        //        dr["DisplayMember"] = strTemp;
        //        dr["ValueMember"]   = TempLanguageList[i];
        //        dt.Rows.Add(dr);
        //    }
        //    return dt;
        //}
		#endregion

		#region ==== SerchDic (web) ====

        /// <summary>
        /// 로드 된 Dictionary Hashtable에서 DicID로 검색하여 설정 된 언어타입에 맞는 값을 리턴합니다.
        /// </summary>
        /// <remarks>Web에서 사용됩니다.</remarks>
        /// <param name="dicID">Dictionary ID</param>
        /// <returns>다국어 처리 값</returns>
        // 웹 시스템에서 더 이상 사용 하지 않음  대신 SearchStaticDicWeb 사용함 
        public static string SearchStaticDic(string dicID)
        {
            clsDicInfo dicInfo = new clsDicInfo();

            if (dicID != null)
            {
                dicInfo = (clsDicInfo)htDicList[dicID];

                if (dicInfo == null)
                {
                    dicInfo = new clsDicInfo();
                    dicInfo.DicLabel = dicID;
                }
                else
                {
                    if (dicInfo.DicLabel.Trim().Length == 0)
                    {
                        if (dicInfo.DicText.Trim().Length == 0)
                        {
                            dicInfo.DicLabel = dicID;
                        }
                        else
                        {
                            dicInfo.DicLabel = dicInfo.DicText;
                        }
                    }
                }
            }

            return dicInfo.DicLabel;
        }

		/// <summary>
		/// 로드 된 Dictionary Hashtable에서 DicID로 검색하여 설정 된 언어타입에 맞는 값을 리턴합니다.
		/// 로그인시 선택한 언어로 Dictionary 조회 
		/// </summary>
		/// <remarks>Web에서 사용됩니다.</remarks>
		/// <param name="dicID">Dictionary ID</param>
		/// <returns>다국어 처리 값</returns>
		public static string SearchStaticDicWeb(string dicID,string langType )
		{            
			clsDicInfo dicInfo      = new clsDicInfo();
			clsDicInfo dicInfoNew   = new clsDicInfo();

			if(dicID != null)
			{
                // 로그인시 접속할 DB를 체크함 해당 로직 삭제 
				if(htDicList == null)
				{
					// Dictionary hashtable아 없는 경우 다시 hashtable에 로드 시킴 
					//CFW.Common.Dictionary.LoadStaticDicList(langType);			
                    CFW.Common.Dictionary.LoadStaticDicList();			
				}
			
				if(htDicList != null)
				{
					dicInfo = (clsDicInfo)htDicList[dicID];

					if( dicInfo !=  null)
					{	
						//dicInfoNew.DicLabel = dicInfo.DicTextEn; 
						if(langType.Equals("ko-kr"))        dicInfoNew.DicLabel = dicInfo.DicTextKo;
						else if(langType.Equals("en-us"))   dicInfoNew.DicLabel = dicInfo.DicTextEn;
                        else dicInfoNew.DicLabel = dicInfo.DicTextLo;
                        
			
						// 해당 언어의 용어가 없는 경우 1. 영어 , 2. ID 순으로 나오게 함 
						if(dicInfoNew.DicLabel.Trim().Length == 0 )
						{
							if( dicInfo.DicText.Trim().Length ==0)  dicInfoNew.DicLabel =  dicID; //영어 용어도 없는 경우 ID 출력 
							else                                    dicInfoNew.DicLabel =  dicInfo.DicText; //해당 언어가 없는 경우 디폴트 영어로 출력	         
						}
					}
					else
					{
						dicInfoNew.DicLabel = dicID;
					}
				}
				else
				{
					dicInfoNew.DicLabel = dicID;
				}
			}
			return dicInfoNew.DicLabel;
		}
			#endregion

		#region ==== LoadDicList(web) ====  미사용 향후 필요시 참조소스 활용
		/// <summary>
		/// Dictionary 테이블에서 화면에 Display될 언어를 읽어와 Hashtable에 로드 합니다.
		/// DB연결 실패 시 xml 파일을 읽어 로드합니다.
		/// </summary>
		/// <remarks>Web에서 사용됩니다.</remarks>
		/// <param name="LanguageType">언어 타입</param>
        //public static void LoadStaticDicList(string LanguageType)
        //{
        //    DataSet ds = null;
			
        //    ds = m_ComDB.GetDicListWeb(LanguageType);

        //    if((ds == null) || (ds.Tables.Count == 0) || (ds.Tables[0].Rows.Count == 0))
        //    {
        //        ReadFromResource();
        //    }
        //    else
        //    {
        //        ReadFromDB(ds);
        //    }
        //}


		/// <summary>
		/// Dictionary 테이블에서 화면에 Display될 언어를 읽어와 Hashtable에 로드 합니다.
		/// Web 로그인 시 한번만 Dictionary 로드함  (로그인시 접속할 DB 지정 ) 
		/// DB연결 실패 시 xml 파일을 읽어 로드합니다.
		/// </summary>
		/// <remarks>Web에서 사용됩니다.</remarks>
		/// <param name="LanguageType">언어 타입</param>
		/// <param name="dbType">DB 타입</param>
        //public static void LoadStaticDicList(string LanguageType, string dbType)
        //{
        //    DataSet ds = null;
			
        //    ds = m_ComDB.GetDicListWeb(LanguageType, dbType); 
        //    if((ds == null) || (ds.Tables.Count == 0) || (ds.Tables[0].Rows.Count == 0))
        //    {
        //        ReadFromResource();
        //    }
        //    else
        //    {
        //        ReadFromDB(ds);
        //    }
        //}

		#endregion

		#region ==== Hashtable에 로드합니다.====	미사용 향후 필요시 참조소스 활용	
		
		/// <summary>
		/// Dictionary 테이블을 읽어 Hashtable에 로드합니다.
		
		/// </summary>
		/// <param name="ds">DataSet</param>
        //private static void ReadFromDB(DataSet	ds)
        //{
        //    alKeys    = new ArrayList();
        //    alValues  = new ArrayList();
        //    htDicList = new Hashtable();

        //    string strMsgText= ""; //기본 설정 언어 

        //    if( htDicList != null )
        //    {
        //        htDicList.Clear();
        //    }

        //    if(m_strWebDefaultLang == null) strMsgText = "DIC_NM_" +m_strDefaultLang.ToUpper().Replace("-","_");
        //    else                            strMsgText = "DIC_NM_" +m_strWebDefaultLang.ToUpper().Replace("-","_");

        //    for(int i = 0; i < ds.Tables[0].Rows.Count; i++)
        //    {
        //        clsDicInfo dicInfo = new clsDicInfo();

        //        alKeys.Add(ds.Tables[0].Rows[i]["DIC_ID"].ToString().Trim());
        //        dicInfo.DicText    = ds.Tables[0].Rows[i][strMsgText].ToString();
        //        dicInfo.DicTextKo  = ds.Tables[0].Rows[i]["DIC_NM_KO_KR"].ToString();
        //        dicInfo.DicTextEn  = ds.Tables[0].Rows[i]["DIC_NM_EN_US"].ToString();
        //        alValues.Add(dicInfo);
        //    }

        //    if(alKeys.Count>0)
        //    {
        //        htDicList = new Hashtable();
        //        htDicList.Clear();

        //        for(int idx=0; idx<alKeys.Count; idx++)
        //            htDicList.Add( (string)alKeys[idx], (clsDicInfo)alValues[idx]);
        //    } 
        //}

		/// <summary>
		/// Dictionary.xml파일을 읽어Hashtable에 로드합니다.
		/// </summary>
        //private static void ReadFromResource()
        //{	
        //    string strFullPath = null;
        //    strFullPath = CFW.Configuration.ConfigManager.Default.ReadConfig("FILEPATH", "DICTIONARY");		
        //    DataSet dsData = new DataSet();
        //    dsData.ReadXml(strFullPath);		
        //    ReadFromDB(dsData);
			
        //}
		#endregion

		#region ==== LoadDicList(cs) ==== 미사용 향후 필요시 참조소스 활용
		/// <summary>
		/// Dictionary 테이블에서 화면에 Display될 언어를 읽어와 Hashtable에 로드 합니다.
		/// DB연결 실패 시 xml 파일을 읽어 로드합니다.
		/// </summary>
		/// <remarks>CS에서 사용됩니다.</remarks>
		/// <param name="LanguageType">언어 타입</param>
        //public static void LoadDicList(string LanguageType)
        //{
        //    try
        //    {
        //        DataSet ds = new DataSet();
        //        ds = m_ComDB.GetDicList(LanguageType);
        //        string strFullPath = null;
        //        strFullPath = CFW.Configuration.ConfigManager.Default.ReadConfig("FILEPATH", "DICTIONARY");		
				
        //        if((ds == null) || (ds.Tables.Count == 0) || (ds.Tables[0].Rows.Count == 0))
        //        {	
        //            ds = new DataSet();			
        //            ds.ReadXml(strFullPath);//DB에러 -> xml 읽어오기
        //        }
        //        CFW.Common.Util.MakeXML(strFullPath,ds);	
        //        dvDicList = new DataView(ds.Tables[0],"","DIC_ID",DataViewRowState.CurrentRows);
        //        bDicLoad = true;
        //    }		
        //    finally
        //    {
        //    }
        //}
		#endregion

        #region ==== SearchDic(cs) ====    미사용 향후 필요시 재정리
        /// <summary>
		/// 로드 된 Dictionary Hashtable에서 DicID로 검색하여 설정 된 언어타입에 맞는 값을 리턴합니다.
		/// </summary>
		/// <remarks>CS에서 사용됩니다.</remarks>
		/// <param name="LanguageType">언어 타입</param>
		/// <param name="dicID">Dictionary ID</param>
		/// <returns>다국어 처리 값</returns>
        //public static string SearchDic(string LanguageType, string dicID)
        //{
        //    if(LanguageType.Length == 0 || LanguageType.Trim() == "")
        //    {
        //        LanguageType = CFW.Configuration.ConfigManager.Default.ReadConfig("CULTURE", "CUL_DEFALT");
        //    }

        //    if(bDicLoad)
        //    {
        //        int rowIndex = dvDicList.Find(dicID);//
        //        if(rowIndex == -1)// 해당 값이 없을 때
        //        {
        //            return dicID; 
        //        }
        //        else
        //        {
        //            string strTemp = dvDicList[rowIndex]["DIC_NM_"+LanguageType.ToUpper().Replace('-','_')].ToString();
        //            if(strTemp.Trim().Length ==0)
        //            {
        //                string strDLang = dvDicList[rowIndex]["DIC_NM_"+m_strDefaultLang.ToUpper().Replace('-','_')].ToString();
        //                if(strDLang.Trim().Length == 0)
        //                {
        //                    return dicID;
        //                }
        //                else return dvDicList[rowIndex]["DIC_NM_"+m_strDefaultLang.ToUpper().Replace('-','_')].ToString();
        //            }
        //            else
        //            {
        //                return dvDicList[rowIndex]["DIC_NM_"+LanguageType.ToUpper().Replace('-','_')].ToString();
        //            }
        //        }					
        //    }
        //    else//언어 테이블에 값이 없을 때
        //    {
        //        string strLangList = CFW.Configuration.ConfigManager.Default.ReadConfig("CULTURE", "CUL_LIST");
        //        LoadDicList(strLangList);
			
        //        //로드 된 DataView에서 검색한다 단, 검색조건으로 oldLanguageType으로 idx를 구한다. 
        //        int rowIndex = dvDicList.Find(dicID);
        //        if(rowIndex == -1)// 해당 값이 없을 때
        //        {
        //            return dicID; 
        //        }
        //        else
        //        {
        //            string strTemp = dvDicList[rowIndex]["DIC_NM_"+LanguageType.ToUpper().Replace('-','_')].ToString();
        //            if(strTemp.Trim().Length ==0)
        //            {
        //                string strDLang = dvDicList[rowIndex]["DIC_NM_"+m_strDefaultLang.ToUpper().Replace('-','_')].ToString();
        //                if(strDLang.Trim().Length == 0)
        //                {
        //                    return dicID;
        //                }
        //                else return dvDicList[rowIndex]["DIC_NM_"+m_strDefaultLang.ToUpper().Replace('-','_')].ToString();
        //            }
        //            else
        //            {
        //                return dvDicList[rowIndex]["DIC_NM_"+LanguageType.ToUpper().Replace('-','_')].ToString();
        //            }
        //        }
        //    }
        //}
		#endregion

		#region ==== DisplayLanguage ====   미사용 향후 필요시 재정리
		/// <summary>
		/// 설정된 언어로 화면을 Display 합니다. 
		/// </summary>
		/// <param name="BaseForm">Display 될 화면</param>
		/// <param name="LanguageType">언어타입</param>
        //public static void DisplayLanguage(System.Windows.Forms.Control BaseForm, string LanguageType)
        //{
        //    m_BaseFormNM = BaseForm.Text;
        //    foreach(Control ctl in BaseForm.Controls)
        //    {
        //        foreach (Control childc in ctl.Controls)
        //        {
        //            foreach (Control childc1 in childc.Controls)
        //            {
        //                foreach (Control childc2 in childc1.Controls)
        //                {
        //                    CheckControl(childc2,LanguageType);
        //                }
        //                CheckControl(childc1,LanguageType);
        //            }
        //            CheckControl(childc,LanguageType);
        //        }
        //        CheckControl(ctl,LanguageType);
        //    }
        //}
		
        ///// <summary>
        ///// Display이된 콘트롤 체크해 설정 언어로 변경합니다.
        ///// </summary>
        ///// <param name="Control">체크 콘트롤</param>
        ///// <param name="LanguageType">언어타입</param>
        //private static void CheckControl(System.Windows.Forms.Control Control, string LanguageType)
        //{
        //    string strControlKey = "";
        //    try
        //    { 
        //        //string ss = Control.GetType().ToString();
        //        if(Control.GetType().ToString() == "DevExpress.XtraEditors.TextEdit" )
        //        {
        //            return;
        //        }
        //        if(Control.GetType().ToString() == "TextEdit" )
        //        {
        //            return;
        //        }

        //        if(Control.GetType().ToString() == "System.Windows.Forms.ComboBox" )
        //        {
        //            return;
        //        }
        //        if(Control.Text != null && Control.Text.Trim().Length > 0)
        //        {
        //            strControlKey = m_BaseFormNM + Control.Name;
        //            if(htControl[strControlKey]!= null)
        //            {
        //                Control.Text = SearchDic(LanguageType, htControl[strControlKey].ToString());
        //            }
        //            else
        //            {

        //                htControl.Add(strControlKey,Control.Text);
        //                Control.Text = SearchDic(LanguageType, Control.Text);
        //            }
        //        }
        //    } 
        //    finally
        //    {}
        //}

        ///// <summary>
        ///// 설정된 언어로 화면을 Display 하고 ComboBox에 다국어 리스트를 바인딩 합니다.
        ///// </summary>
        ///// <param name="BaseForm">Display 될 화면</param>
        ///// <param name="LanguageType">언어타입</param>
        ///// <param name="cb">다국어 리스트가 바인딩 될 ComboBox</param>
        //public static void DisplayLanguage(System.Windows.Forms.Control BaseForm, string LanguageType , ComboBox cb)
        //{
        //    foreach(Control ctl in BaseForm.Controls)
        //    {
        //        foreach (Control childc in ctl.Controls)
        //        {
        //            foreach (Control childc1 in childc.Controls)
        //            {
        //                foreach (Control childc2 in childc1.Controls)
        //                {
        //                    CheckControl(childc2,LanguageType);
        //                }
        //                CheckControl(childc1,LanguageType);
        //            }
        //            CheckControl(childc,LanguageType);
        //        }
        //        CheckControl(ctl,LanguageType);
        //    }
			
        //    BindLanguageComboBox(cb,LanguageType);
        //}

		#region ==== BindLanguageComboBox ====
		/// <summary>
		/// 다국어리스트를 해당 ComboBox에 바인딩합니다.
		/// </summary>
		/// <param name="cb">다국어리스트가 바인딩 될 컨트롤</param>
		/// <param name="langType">언어 타입</param>
        //private static void BindLanguageComboBox(ComboBox cb, string langType)
        //{
        //    string strLanguageList = string.Empty;
        //    int iCount = cb.Items.Count;
        //    DataTable dt = new DataTable();
        //    strLanguageList = CFW.Configuration.ConfigManager.Default.ReadConfig("CULTURE", "CUL_LIST");
        //    string [] TempLanguageList = strLanguageList.Split(',');

        //    if( iCount == 0)
        //    {
        //        dt.Columns.Add("DisplayMember");
        //        dt.Columns.Add("ValueMember");

        //        if(langType.Length == 0 || langType == null)
        //        {
        //            langType = CFW.Configuration.ConfigManager.Default.ReadConfig("CULTURE", "CUL_DEFALT");
        //        }

        //        for(int i = 0; i<TempLanguageList.Length; i++)
        //        {
        //            DataRow dr = dt.NewRow();
        //            string strTemp = SearchDic(langType,TempLanguageList[i]);
        //            dr["DisplayMember"] = strTemp;
        //            dr["ValueMember"]   = TempLanguageList[i];
        //            dt.Rows.Add(dr);
        //        }
        //        cb.DataSource = dt;
        //        cb.DisplayMember = dt.Columns["DisplayMember"].ToString();
        //        cb.ValueMember = dt.Columns["ValueMember"].ToString();			
        //    }

        //    if(iCount >0)
        //    {
        //        dt.Columns.Add("DisplayMember");
        //        dt.Columns.Add("ValueMember");
        //        for(int i = 0; i<iCount; i++)
        //        {
        //            DataRow dr = dt.NewRow();

        //            string aa = cb.GetItemText(cb.Items[i]);
						
					
        //            string strTemp = SearchDic(langType, aa);
        //            dr["DisplayMember"] = strTemp;
        //            dr["ValueMember"]   = TempLanguageList[i];
        //            dt.Rows.Add(dr);
        //        }
        //        cb.DataSource = dt;
        //        cb.DisplayMember = dt.Columns["DisplayMember"].ToString();
        //        cb.ValueMember = dt.Columns["ValueMember"].ToString();
        //    }
		
        //    cb.SelectedValue = langType;
        //}
		#endregion

        #endregion

        #region 2014.12.23 OHS 추가
        /// <summary>
        /// Load dictionary
        /// </summary>       
        public static void LoadStaticDicList()
        {
            DataSet ds = new DataSet();

            string strFullPath    = CFW.Configuration.ConfigManager.Default.ReadConfig("Dictionary", "DictionaryFilePath");
            string strLoadingType = CFW.Configuration.ConfigManager.Default.ReadConfig("Dictionary", "DictionaryLoadingType");

            try
            {
                if (HttpContext.Current == null)    ds = CFW.Data.OracleComDB.GetDicList("C");          // CS용 Dictionary
                else                                ds = CFW.Data.OracleComDB.GetDicList("W");          // Web용 Dictionary

                if (((ds == null) || (ds.Tables.Count == 0)) || (ds.Tables[0].Rows.Count == 0))
                {
                    if (strLoadingType == "all")    ReadDicFromXml();
                }
                else
                {
                    if (strLoadingType == "all")    SetDicMemory(ds);
                    Util.MakeXML(strFullPath, ds);
                }
            }
            catch (Exception e)
            {
                bool bXml = false;
                if (strLoadingType == "all")    bXml = ReadDicFromXml();
                if (!bXml)  throw e;
            }
        }

        private static bool ReadDicFromXml()
        {
            string  strFullPath = null;
            DataSet dsData      = new DataSet();

            try
            {
                strFullPath = CFW.Configuration.ConfigManager.Default.ReadConfig("Dictionary", "DictionaryFilePath");

                if (!File.Exists(strFullPath))
                {
                    throw new ApplicationException(string.Format("Dictionary file not found : {0}", strFullPath));
                }

                dsData.ReadXml(strFullPath);
                SetDicMemory(dsData);
            }
            catch (Exception e)
            {
                throw e;
            }
            return true;
        }

        private static void SetDicMemory(DataSet ds)
        {
            if (htDicList == null)
            {
                htDicList = new Hashtable();
                htDicList.Clear();
            }
            SetDicMemory(ds.Tables[0]);
        }

        private static void SetDicMemory(DataTable dt)
        {
            if (htDicList == null)
            {
                htDicList = new Hashtable();
                htDicList.Clear();
            }

            alKeys   = new ArrayList();
            alValues = new ArrayList();
            try
            {
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    clsDicInfo dicInfo = new clsDicInfo();
                    alKeys.Add(dt.Rows[i]["DIC_ID"].ToString().Trim());
                    dicInfo.DicTextLo = dt.Rows[i]["DIC_NM_LO_LN"].ToString().Trim();
                    dicInfo.DicTextEn = dt.Rows[i]["DIC_NM_EN_US"].ToString().Trim();
                    dicInfo.DicTextKo = dt.Rows[i]["DIC_NM_KO_KR"].ToString().Trim();
                    alValues.Add(dicInfo);
                }

                if (alKeys.Count > 0)
                {
                    for (int idx = 0; idx < alKeys.Count; idx++)
                    {
                        if (!htDicList.Contains(alKeys[idx]))
                        {
                            htDicList.Add((string)alKeys[idx], (clsDicInfo)alValues[idx]);
                        }
                    }
                }
                else
                {
                    htDicList = new Hashtable();
                    htDicList.Clear();
                }
            }
            catch (Exception e)
            {
                throw e;
            }
        }
        #endregion
    }

	#region -- class clsDicInfo : Dictionary Structure
	/// <summary>
	/// clsMsgInfo Structure - 메시지 정보 구조체
	/// C_DICTIONARY_DF 테이블 구조에 따라 DIC_NM_KO_KR, DIC_NM_EN_US
	/// </summary>
	public class clsDicInfo
	{
		#region -- Member
		public string DicLabel       = "";
		public string DicText		 = "";
		public string DicTextKo		 = "";
		public string DicTextEn		 = "";
		public string DicTextLo		 = "";
		#endregion
	}
	#endregion

}
