using System;
using Microsoft.Win32;
using System.Text;
using System.Data;
using System.Runtime.InteropServices;

namespace CFW.Common
{
	/// <summary>
	/// Util�� ���� ��� �����Դϴ�.
	/// </summary>
	public class Util
	{
		public Util()
		{
			//
			// TODO: ���⿡ ������ ���� �߰��մϴ�.
			//
		}

		#region -- Win32 API Function ������

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
		/// ������ Ŭ������ ĸ������ �����츦 �˻��մϴ�.
		/// </summary>
		/// <param name="lpClassName">Ŭ���� �̸�</param>
		/// <param name="lpWindowName">������ �̸�</param>
		/// <returns>int(false : 0 / true : 0�� �ƴ� ��)</returns>
		[DllImport("User32.dll")]
		public  static extern int  FindWindow(string  lpClassName, string  lpWindowName);

		/// <summary>
		/// �������� ���̱� ���� �����մϴ�.
		/// </summary>
		/// <param name="nHwnd">��� ������ �ڵ�</param>
		/// <param name="nCmdShow">�����ϰ��� �ϴ� ���̱� ���� ����</param>
		/// <returns>bool(true/false)</returns>
		[DllImport("User32.dll")]
		public  static extern bool  ShowWindow(int nHwnd, int nCmdShow);

		/// <summary>
		/// �����찡 ���̴� �������� ������ ���������� �����մϴ�.
		/// </summary>
		/// <param name="nHwnd">��� ������ �ڵ�</param>
		/// <returns>bool(true/false)</returns>
		[DllImport("User32.dll")]
		public  static extern bool  IsWindowVisible(int nHwnd);

		/// <summary>
		/// Ÿ�� �����쿡�� ���� �������� �������� �޽����� �����մϴ�.
		/// </summary>
		/// <param name="nHwnd">��� ������ �ڵ�</param>
		/// <returns>bool(true/false)</returns>
		[DllImport("User32.dll")]
		public  static extern bool  BringWindowToTop(int nHwnd);

		/// <summary>
		/// ���丮�� �����մϴ�.
		/// </summary>
		/// <param name="lpPathName">���丮 ���</param>
		/// <param name="lpAttributes">���� �Ӽ�</param>
		/// <returns>bool(true/false)</returns>
		[DllImport("Kernel32.dll")]
		public  static extern bool  CreateDirectory(string  lpPathName, SECURITY_ATTRIBUTES lpAttributes);

		/// <summary>
		/// ������ �����մϴ�.
		/// </summary>
		/// <param name="lpExistingFileName">������ ���� ����. ���� ��θ� ���� ����</param>
		/// <param name="lpNewFileName">�����Ͽ� ���� ������ ����</param>
		/// <param name="bFailIfExists">���� ������ ������ �̹� ���� ����� ������ ����(TRUE:����/FALSE:���� ������ ���)</param>
		/// <returns>bool(true/false)</returns>
		[DllImport("Kernel32.dll")]
		public  static extern bool  CopyFile(string lpExistingFileName, string lpNewFileName, bool bFailIfExists);

		/// <summary>
		/// ������ �����մϴ�.
		/// </summary>
		/// <param name="lpFileName">������ ���� �̸�</param>
		/// <returns>bool(true/false)</returns>
		[DllImport("Kernel32.dll")]
		public  static extern bool  DeleteFile(string lpFileName);

		/// <summary>
		/// string�� unsigned long���� ��ȯ�մϴ�. 
		/// </summary>
		/// <param name="nptr">��ȯ �� ���ڿ�</param>
		/// <param name="endptr">������</param>
		/// <param name="p_nBase">����(10,16...)</param>
		/// <returns></returns>
		[DllImport("MSVCRT.DLL")]
		public  static extern int strtoul( string nptr, string endptr, int p_nBase );

		/// <summary>
		/// cfg Read �Լ�
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
		/// cfg Write �Լ�
		/// </summary>
		/// <param name="section">section</param>
		/// <param name="key">key</param>
		/// <param name="val">value</param>
		/// <param name="filePath">filePath</param>
		/// <returns></returns>
		[DllImport("kernel32.dll")]		
		public static extern long WritePrivateProfileString(string section, string key, string val, string filePath);
		
		#endregion

		#region -- RegistrySet() / RegistryGet()  == �̻��
		
		/// <summary>
		/// ������Ʈ�� ���� �����մϴ�.
		/// </summary>
		/// <param name="strPgmId">���α׷� ���̵�(in)</param>
		/// <param name="strKey">������Ʈ�� Ű(in)</param>
		/// <param name="strVal">������Ʈ�� ���� ��(in)</param>
		/// <param name="strErrText">���� �ؽ�Ʈ(out)</param>
		/// <returns>���� ����</returns>
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
		/// ������Ʈ�� ���� �о�ɴϴ�.
		/// </summary>
		/// <param name="strPgmId">���α׷� ���̵�(in)</param>
		/// <param name="strKey">������Ʈ�� Ű(in)</param>
		/// <param name="strVal">������Ʈ�� ��(out)</param>
		/// <param name="strErrText">���� �ؽ�Ʈ(out)</param>
		/// <returns>���� ����</returns>
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
		/// char�迭�� string���� ��ȯ�մϴ�.
		/// </summary>
		/// <param name="Value">char[] Value</param>
		/// <returns>��ȯ �� string</returns>
		public static string ConvertToString(char[] Value)
		{
			string str = "";
			for(int i = 0; i < Value.Length; i++)	str += Value[i];
			return str;
		}

		/// <summary>
		/// string�� ó�� �������� ��ȯ�մϴ�.
		/// </summary>
		/// <param name="str">string str</param>
        /// <returns>��ȯ�� string</returns>
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
		/// string�� int������ ��ȯ�մϴ�.
		/// ���� Method Name : sToi(string p_str)
		/// </summary>
		/// <param name="str">string str</param>
		/// <returns>��ȯ �� int</returns>
		public static int  StringToInt(string str)
		{
            return int.Parse(StringToNumber(str));
		}

		/// <summary>
		/// string�� Long������ ��ȯ�մϴ�.
		/// ���� Method Name : sTol(string p_str)
		/// </summary>
		/// <param name="str">string str</param>
		/// <returns>��ȯ �� long</returns>
		public static long StringToLong(string str)
		{
            return long.Parse(StringToNumber(str));
		}
		#endregion

		#region ==== MakeXML ====
		/// <summary>
		/// DataSet�� Xml���Ϸ� ��ȯ�մϴ�.
		/// </summary>
		/// <param name="fullPath">Xml���� ���� ���</param>
		/// <param name="ds">��ȯ �� DataSet</param>
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
					DateTime Dt1 = DateTime.Now;//����ð�					              
					DateTime Dt2 = System.IO.File.GetLastWriteTime(sFullPath);//���Ͻð�

					System.TimeSpan duration = Dt1 - Dt2;					
					int nTimeStamp = Convert.ToInt32(duration.TotalSeconds);

					if( nTimeStamp > 30 )//30�� �ȿ� 
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
