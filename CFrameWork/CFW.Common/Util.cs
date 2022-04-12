using System;
using Microsoft.Win32;
using System.Text;
using System.Data;
using System.Runtime.InteropServices;

namespace CFW.Common
{
	/// <summary>
	/// Util에 대한 요약 설명입니다.
	/// </summary>
	public class Util
	{
		public Util()
		{
			//
			// TODO: 여기에 생성자 논리를 추가합니다.
			//
		}

		#region -- Win32 API Function 재정의

		[StructLayout( LayoutKind.Sequential ) ]
		public class SECURITY_ATTRIBUTES 
		{ 
			public int		nLength; 
			public object	lpSecurityDescriptor; 
			public bool		bInheritHandle; 

			public SECURITY_ATTRIBUTES()
			{
				nLength = Marshal.SizeOf( typeof( SECURITY_ATTRIBUTES ) );

				lpSecurityDescriptor = null;
				bInheritHandle		 = false;
			}
		}
 
		/// <summary>
		/// 윈도우 클래스와 캡션으로 윈도우를 검색합니다.
		/// </summary>
		/// <param name="lpClassName">클래스 이름</param>
		/// <param name="lpWindowName">윈도우 이름</param>
		/// <returns>int(false : 0 / true : 0이 아닌 값)</returns>
		[DllImport("User32.dll")]
		public  static extern int  FindWindow(string  lpClassName, string  lpWindowName);

		/// <summary>
		/// 윈도우의 보이기 상태 지정합니다.
		/// </summary>
		/// <param name="nHwnd">대상 윈도우 핸들</param>
		/// <param name="nCmdShow">지정하고자 하는 보이기 상태 지정</param>
		/// <returns>bool(true/false)</returns>
		[DllImport("User32.dll")]
		public  static extern bool  ShowWindow(int nHwnd, int nCmdShow);

		/// <summary>
		/// 윈도우가 보이는 상태인지 숨겨진 상태인지를 조사합니다.
		/// </summary>
		/// <param name="nHwnd">대상 윈도우 핸들</param>
		/// <returns>bool(true/false)</returns>
		[DllImport("User32.dll")]
		public  static extern bool  IsWindowVisible(int nHwnd);

		/// <summary>
		/// 타겟 윈도우에게 가장 위쪽으로 나오도록 메시지를 전달합니다.
		/// </summary>
		/// <param name="nHwnd">대상 윈도우 핸들</param>
		/// <returns>bool(true/false)</returns>
		[DllImport("User32.dll")]
		public  static extern bool  BringWindowToTop(int nHwnd);

		/// <summary>
		/// 디렉토리를 생성합니다.
		/// </summary>
		/// <param name="lpPathName">디렉토리 경로</param>
		/// <param name="lpAttributes">보안 속성</param>
		/// <returns>bool(true/false)</returns>
		[DllImport("Kernel32.dll")]
		public  static extern bool  CreateDirectory(string  lpPathName, SECURITY_ATTRIBUTES lpAttributes);

		/// <summary>
		/// 파일을 복사합니다.
		/// </summary>
		/// <param name="lpExistingFileName">복사할 원본 파일. 완전 경로를 지정 가능</param>
		/// <param name="lpNewFileName">복사하여 새로 생성될 파일</param>
		/// <param name="bFailIfExists">새로 생성될 파일이 이미 있을 경우의 동작을 지정(TRUE:실패/FALSE:기존 파일을 덮어씀)</param>
		/// <returns>bool(true/false)</returns>
		[DllImport("Kernel32.dll")]
		public  static extern bool  CopyFile(string lpExistingFileName, string lpNewFileName, bool bFailIfExists);

		/// <summary>
		/// 파일을 삭제합니다.
		/// </summary>
		/// <param name="lpFileName">삭제할 파일 이름</param>
		/// <returns>bool(true/false)</returns>
		[DllImport("Kernel32.dll")]
		public  static extern bool  DeleteFile(string lpFileName);

		/// <summary>
		/// string을 unsigned long으로 변환합니다. 
		/// </summary>
		/// <param name="nptr">변환 할 문자열</param>
		/// <param name="endptr">포인터</param>
		/// <param name="p_nBase">진수(10,16...)</param>
		/// <returns></returns>
		[DllImport("MSVCRT.DLL")]
		public  static extern int strtoul( string nptr, string endptr, int p_nBase );

		/// <summary>
		/// cfg Read 함수
		/// </summary>
		/// <param name="section">section</param>
		/// <param name="key">key</param>
		/// <param name="def">def</param>
		/// <param name="retVal">returnValue</param>
		/// <param name="size">size</param>
		/// <param name="filePath">filePath</param>
		/// <returns></returns>
		[DllImport("kernel32.dll")]		
		public static extern int GetPrivateProfileString(string section, string key, string def, StringBuilder retVal, int size, string filePath); 

		/// <summary>
		/// cfg Write 함수
		/// </summary>
		/// <param name="section">section</param>
		/// <param name="key">key</param>
		/// <param name="val">value</param>
		/// <param name="filePath">filePath</param>
		/// <returns></returns>
		[DllImport("kernel32.dll")]		
		public static extern long WritePrivateProfileString(string section, string key, string val, string filePath);
		
		#endregion

		#region -- RegistrySet() / RegistryGet()  == 미사용
		
		/// <summary>
		/// 레지스트리 값을 설정합니다.
		/// </summary>
		/// <param name="strPgmId">프로그램 아이디(in)</param>
		/// <param name="strKey">레지스트리 키(in)</param>
		/// <param name="strVal">레지스트리 설정 값(in)</param>
		/// <param name="strErrText">에러 텍스트(out)</param>
		/// <returns>성공 여부</returns>
        //public static bool RegistrySet( string strPgmId, string strKey, string strVal, ref string strErrText )
        //{
        //    string strRegRoot = "";
        //    string strRegRootPath = "";
        //    RegistryKey rkRegKey = Registry.CurrentUser;

        //    try
        //    {
        //        strRegRootPath = Config.ReadConfigValue("REGISTRY", "REG_ROOT", Config.ComConfigPath);
        //        strRegRoot = strRegRootPath + strPgmId ;
        //        rkRegKey = rkRegKey.OpenSubKey(strRegRoot, true);

        //        if( rkRegKey == null )
        //        {
        //            rkRegKey = Registry.CurrentUser;
        //            rkRegKey = rkRegKey.CreateSubKey(strRegRoot);
        //        }

        //        rkRegKey.SetValue(strKey, strVal);
        //        return true;
        //    }
        //    catch (Exception e)
        //    {
        //        strErrText = e.Message;
        //        return false;
        //    }
        //}
		

		/// <summary>
		/// 레지스트리 값을 읽어옵니다.
		/// </summary>
		/// <param name="strPgmId">프로그램 아이디(in)</param>
		/// <param name="strKey">레지스트리 키(in)</param>
		/// <param name="strVal">레지스트리 값(out)</param>
		/// <param name="strErrText">에러 텍스트(out)</param>
		/// <returns>성공 여부</returns>
        //public static bool RegistryGet( string strPgmId, string strKey, ref string strVal, ref string strErrText )
        //{
        //    string strRegRoot = "";;
        //    string strRegRootPath = "";
        //    RegistryKey rkRegKey = Registry.CurrentUser;
        //    try
        //    {
        //        strRegRootPath = Config.ReadConfigValue("REGISTRY", "REG_ROOT", Config.ComConfigPath);
        //        strRegRoot = strRegRootPath + strPgmId;
        //        rkRegKey = rkRegKey.OpenSubKey(strRegRoot, true);

        //        if( rkRegKey != null )   strVal  = (string)rkRegKey.GetValue(strKey, "");
        //        return true;
        //    }
        //    catch (Exception e)
        //    {
        //        strErrText = e.Message;
        //        return false;
        //    }
		
        //}
		#endregion

		#region -- Convert [ConvertToString]/[StringToInt]/[StringToLong]
		/// <summary>
		/// char배열을 string으로 변환합니다.
		/// </summary>
		/// <param name="Value">char[] Value</param>
		/// <returns>변환 된 string</returns>
		public static string ConvertToString(char[] Value)
		{
			string str = "";
			for(int i = 0; i < Value.Length; i++)	str += Value[i];
			return str;
		}

		/// <summary>
		/// string을 처음 숫자형만 반환합니다.
		/// </summary>
		/// <param name="str">string str</param>
        /// <returns>반환된 string</returns>
		private static string StringToNumber(string str)
		{
			string	strSign	= "";
			string	strNum	= "";
			int		nStart	= 0;

			str = str.Trim();
			if( str.Length == 0 )   return "0";

			if( str[0] == '+' || str[0] == '-' )
			{
				strSign = str[0].ToString();
				nStart	= 1;
			}

			for(int i = nStart; i < str.Length; i++)
			{
				if( str[i] >= '0' && str[i] <= '9' )    strNum += str[i];
				else                                    break;
			}

			if( strNum.Length == 0 )    return "0";
			
			strNum = strSign + strNum;
			return strNum;
		}

		/// <summary>
		/// string을 int형으로 변환합니다.
		/// 기존 Method Name : sToi(string p_str)
		/// </summary>
		/// <param name="str">string str</param>
		/// <returns>변환 된 int</returns>
		public static int  StringToInt(string str)
		{
            return int.Parse(StringToNumber(str));
		}

		/// <summary>
		/// string을 Long형으로 변환합니다.
		/// 기존 Method Name : sTol(string p_str)
		/// </summary>
		/// <param name="str">string str</param>
		/// <returns>변환 된 long</returns>
		public static long StringToLong(string str)
		{
            return long.Parse(StringToNumber(str));
		}
		#endregion

		#region ==== MakeXML ====
		/// <summary>
		/// DataSet을 Xml파일로 변환합니다.
		/// </summary>
		/// <param name="fullPath">Xml파일 저장 경로</param>
		/// <param name="ds">변환 할 DataSet</param>
		public static void MakeXML(string fullPath, DataSet ds)
		{
			string sFullPath = fullPath;
			//ds.WriteXml(strFullPath,System.Data.XmlWriteMode.DiffGram);
			System.IO.FileStream    fs      = null;
			System.IO.StreamWriter  sWriter = null;

			try
			{
				if(System.IO.File.Exists(sFullPath))
				{
					DateTime Dt1 = DateTime.Now;//현재시간					              
					DateTime Dt2 = System.IO.File.GetLastWriteTime(sFullPath);//파일시간

					System.TimeSpan duration = Dt1 - Dt2;					
					int nTimeStamp = Convert.ToInt32(duration.TotalSeconds);

					if( nTimeStamp > 30 )//30초 안에 
					{
						fs = new System.IO.FileStream(  sFullPath, 
							                            System.IO.FileMode.Create, 
							                            System.IO.FileAccess.Write,
							                            System.IO.FileShare.ReadWrite);       

						sWriter = new System.IO.StreamWriter(fs, System.Text.Encoding.UTF8);
						sWriter.WriteLine("<?xml version=\"1.0\" encoding=\"utf-8\"  ?>");
						sWriter.Write(ds.GetXml());
						sWriter.Flush();
						fs.Flush();
					}
					else
					{
						return;
					}					
				}
				else
				{
					fs = new System.IO.FileStream(  sFullPath, 
						                            System.IO.FileMode.Create, 
						                            System.IO.FileAccess.Write,
						                            System.IO.FileShare.ReadWrite);

					sWriter = new System.IO.StreamWriter(fs, System.Text.Encoding.UTF8);
					sWriter.WriteLine("<?xml version=\"1.0\" encoding=\"utf-8\"  ?>");
					sWriter.Write(ds.GetXml());
					sWriter.Flush();
					fs.Flush();
				}
			}
			finally
			{
				if (sWriter != null)    sWriter.Close();
				if (fs != null)         fs.Close();
			}
		}
		#endregion
	}
}
