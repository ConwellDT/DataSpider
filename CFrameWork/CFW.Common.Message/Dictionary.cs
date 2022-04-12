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
	/// Dictionary�� ���� ��� �����Դϴ�.
	/// </summary>
	public class Dictionary
	{
		#region ==== ���� ���� ====

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
		/// �������Դϴ�.
		/// </summary>
		public Dictionary()
		{
			//
			// TODO: ���⿡ ������ ���� �߰��մϴ�.
			//
		}

		#region ==== GetLanguageList ====  �̻��
        ///// <summary>
        ///// �ٱ��� ����Ʈ�� �о�� DataTable�� �����մϴ�.
        ///// </summary>
        ///// <param name="langType">��� Ÿ��</param>
        ///// <returns>�ٱ��� ����Ʈ DataTable </returns>
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
        /// �ε� �� Dictionary Hashtable���� DicID�� �˻��Ͽ� ���� �� ���Ÿ�Կ� �´� ���� �����մϴ�.
        /// </summary>
        /// <remarks>Web���� ���˴ϴ�.</remarks>
        /// <param name="dicID">Dictionary ID</param>
        /// <returns>�ٱ��� ó�� ��</returns>
        // �� �ý��ۿ��� �� �̻� ��� ���� ����  ��� SearchStaticDicWeb ����� 
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
		/// �ε� �� Dictionary Hashtable���� DicID�� �˻��Ͽ� ���� �� ���Ÿ�Կ� �´� ���� �����մϴ�.
		/// �α��ν� ������ ���� Dictionary ��ȸ 
		/// </summary>
		/// <remarks>Web���� ���˴ϴ�.</remarks>
		/// <param name="dicID">Dictionary ID</param>
		/// <returns>�ٱ��� ó�� ��</returns>
		public static string SearchStaticDicWeb(string dicID,string langType )
		{            
			clsDicInfo dicInfo      = new clsDicInfo();
			clsDicInfo dicInfoNew   = new clsDicInfo();

			if(dicID != null)
			{
                // �α��ν� ������ DB�� üũ�� �ش� ���� ���� 
				if(htDicList == null)
				{
					// Dictionary hashtable�� ���� ��� �ٽ� hashtable�� �ε� ��Ŵ 
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
                        
			
						// �ش� ����� �� ���� ��� 1. ���� , 2. ID ������ ������ �� 
						if(dicInfoNew.DicLabel.Trim().Length == 0 )
						{
							if( dicInfo.DicText.Trim().Length ==0)  dicInfoNew.DicLabel =  dicID; //���� �� ���� ��� ID ��� 
							else                                    dicInfoNew.DicLabel =  dicInfo.DicText; //�ش� �� ���� ��� ����Ʈ ����� ���	         
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

		#region ==== LoadDicList(web) ====  �̻�� ���� �ʿ�� �����ҽ� Ȱ��
		/// <summary>
		/// Dictionary ���̺��� ȭ�鿡 Display�� �� �о�� Hashtable�� �ε� �մϴ�.
		/// DB���� ���� �� xml ������ �о� �ε��մϴ�.
		/// </summary>
		/// <remarks>Web���� ���˴ϴ�.</remarks>
		/// <param name="LanguageType">��� Ÿ��</param>
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
		/// Dictionary ���̺��� ȭ�鿡 Display�� �� �о�� Hashtable�� �ε� �մϴ�.
		/// Web �α��� �� �ѹ��� Dictionary �ε���  (�α��ν� ������ DB ���� ) 
		/// DB���� ���� �� xml ������ �о� �ε��մϴ�.
		/// </summary>
		/// <remarks>Web���� ���˴ϴ�.</remarks>
		/// <param name="LanguageType">��� Ÿ��</param>
		/// <param name="dbType">DB Ÿ��</param>
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

		#region ==== Hashtable�� �ε��մϴ�.====	�̻�� ���� �ʿ�� �����ҽ� Ȱ��	
		
		/// <summary>
		/// Dictionary ���̺��� �о� Hashtable�� �ε��մϴ�.
		
		/// </summary>
		/// <param name="ds">DataSet</param>
        //private static void ReadFromDB(DataSet	ds)
        //{
        //    alKeys    = new ArrayList();
        //    alValues  = new ArrayList();
        //    htDicList = new Hashtable();

        //    string strMsgText= ""; //�⺻ ���� ��� 

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
		/// Dictionary.xml������ �о�Hashtable�� �ε��մϴ�.
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

		#region ==== LoadDicList(cs) ==== �̻�� ���� �ʿ�� �����ҽ� Ȱ��
		/// <summary>
		/// Dictionary ���̺��� ȭ�鿡 Display�� �� �о�� Hashtable�� �ε� �մϴ�.
		/// DB���� ���� �� xml ������ �о� �ε��մϴ�.
		/// </summary>
		/// <remarks>CS���� ���˴ϴ�.</remarks>
		/// <param name="LanguageType">��� Ÿ��</param>
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
        //            ds.ReadXml(strFullPath);//DB���� -> xml �о����
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

        #region ==== SearchDic(cs) ====    �̻�� ���� �ʿ�� ������
        /// <summary>
		/// �ε� �� Dictionary Hashtable���� DicID�� �˻��Ͽ� ���� �� ���Ÿ�Կ� �´� ���� �����մϴ�.
		/// </summary>
		/// <remarks>CS���� ���˴ϴ�.</remarks>
		/// <param name="LanguageType">��� Ÿ��</param>
		/// <param name="dicID">Dictionary ID</param>
		/// <returns>�ٱ��� ó�� ��</returns>
        //public static string SearchDic(string LanguageType, string dicID)
        //{
        //    if(LanguageType.Length == 0 || LanguageType.Trim() == "")
        //    {
        //        LanguageType = CFW.Configuration.ConfigManager.Default.ReadConfig("CULTURE", "CUL_DEFALT");
        //    }

        //    if(bDicLoad)
        //    {
        //        int rowIndex = dvDicList.Find(dicID);//
        //        if(rowIndex == -1)// �ش� ���� ���� ��
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
        //    else//��� ���̺� ���� ���� ��
        //    {
        //        string strLangList = CFW.Configuration.ConfigManager.Default.ReadConfig("CULTURE", "CUL_LIST");
        //        LoadDicList(strLangList);
			
        //        //�ε� �� DataView���� �˻��Ѵ� ��, �˻��������� oldLanguageType���� idx�� ���Ѵ�. 
        //        int rowIndex = dvDicList.Find(dicID);
        //        if(rowIndex == -1)// �ش� ���� ���� ��
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

		#region ==== DisplayLanguage ====   �̻�� ���� �ʿ�� ������
		/// <summary>
		/// ������ ���� ȭ���� Display �մϴ�. 
		/// </summary>
		/// <param name="BaseForm">Display �� ȭ��</param>
		/// <param name="LanguageType">���Ÿ��</param>
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
        ///// Display�̵� ��Ʈ�� üũ�� ���� ���� �����մϴ�.
        ///// </summary>
        ///// <param name="Control">üũ ��Ʈ��</param>
        ///// <param name="LanguageType">���Ÿ��</param>
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
        ///// ������ ���� ȭ���� Display �ϰ� ComboBox�� �ٱ��� ����Ʈ�� ���ε� �մϴ�.
        ///// </summary>
        ///// <param name="BaseForm">Display �� ȭ��</param>
        ///// <param name="LanguageType">���Ÿ��</param>
        ///// <param name="cb">�ٱ��� ����Ʈ�� ���ε� �� ComboBox</param>
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
		/// �ٱ����Ʈ�� �ش� ComboBox�� ���ε��մϴ�.
		/// </summary>
		/// <param name="cb">�ٱ����Ʈ�� ���ε� �� ��Ʈ��</param>
		/// <param name="langType">��� Ÿ��</param>
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

        #region 2014.12.23 OHS �߰�
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
                if (HttpContext.Current == null)    ds = CFW.Data.OracleComDB.GetDicList("C");          // CS�� Dictionary
                else                                ds = CFW.Data.OracleComDB.GetDicList("W");          // Web�� Dictionary

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
	/// clsMsgInfo Structure - �޽��� ���� ����ü
	/// C_DICTIONARY_DF ���̺� ������ ���� DIC_NM_KO_KR, DIC_NM_EN_US
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
