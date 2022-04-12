using System;

namespace CFW.Data
{
	/// <summary>
	/// OracleDBDef에 대한 요약 설명입니다.
	/// </summary>
	public abstract class OracleDBDef
	{
		/// <summary>
		/// OracleDBDef 생성자 입니다.
		/// </summary>
		public OracleDBDef(){}
		
		//------------------------------------------------------------------------------
		// Oracle DB Error Code
		//------------------------------------------------------------------------------        
		#region DEFINE : Oracle DB Error Code
		/// <summary>
		/// DB error code : Normal
		/// </summary>
		public const string ORAMID_GOOD		= "O00000";	   
		/// <summary>
		///  DB error code : Disconnect
		/// </summary>
		public const string ORAMID_NOCONN	= "O03114";		
		/// <summary>
		/// DB error code : Disconnect1
		/// </summary>
		public const string ORAMID_NOCONN1	= "O01012";		
		/// <summary>
		/// DB error code : Disconnect2
		/// </summary>
		public const string ORAMID_NOCONN2	= "O01089";		
		/// <summary>
		/// DB error code : Disconnect3
		/// </summary>
		public const string ORAMID_NOCONN3	= "O03113";		
		/// <summary>
		/// DB error code : Disconnect4
		/// </summary>
		public const string ORAMID_NOCONN4	= "O03114";	 
		/// <summary>
		/// DB error code : Disconnect5
		/// </summary>
		public const string ORAMID_NOCONN5	= "O12152";	 
		/// <summary>
		/// DB error code : Disconnect6
		/// </summary>
		public const string ORAMID_NOCONN6	= "O12560";		
		/// <summary>
		/// DB error code : Disconnect7
		/// </summary>
		public const string ORAMID_NOCONN7	= "O12571";	
		/// <summary>
		/// DB error code : No data found
		/// </summary>
		public const string ORAMID_NOFOUND	= "O01403";
		/// <summary>
		/// DB error code : No data found(AQ)
		/// </summary>
		public const string ORAMID_QUENODATA= "O25228";	 
		/// <summary>
		/// DB error code : Unique constraint 
		/// </summary>
		public const string ORAMID_OVERLAP	= "O00001";	    
		/// <summary>
		/// DB error code : Database lock 
		/// </summary>
		public const string ORAMID_LOCK		= "O00054";


        public static string ConnectionString    = CFW.Common.SecurityUtil.DecryptString(CFW.Configuration.ConfigManager.Default.ReadConfig("connectionStrings", "Oracle_ConnectionString"));
        //public static string ConnectionString = CFW.Configuration.ConfigManager.Default.ReadConfig("connectionStrings", "Oracle_ConnectionString");
        public static string SQLConnectionString = CFW.Configuration.ConfigManager.Default.ReadConfig("SQLStorage", "OracleSQLStorage_ConnectionString");
    
		#endregion
	}
}
