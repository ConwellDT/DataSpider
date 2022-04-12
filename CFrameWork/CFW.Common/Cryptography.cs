using System;
using System.IO;
using System.Security.Cryptography;

namespace CFW.Common
{
	/// <summary>
	/// Cryptography에 대한 요약 설명입니다.
	/// </summary>
	public class Cryptography
	{
		internal static byte[] INIT_VEC = {0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01};
		// BYTES_KEY 는 수정 요망 !!!!!!!!!!!!!!!!!
		internal static byte[] BYTES_KEY = { 0x74, 0x81, 0x3C, 0x71, 0x9C, 0x23, 0xD4, 0x82, 0xA1, 0x63, 0xD3, 0xA3, 0x32, 0x59, 0x75, 0x57 };

		/// <summary>
		/// 생성자입니다.
		/// </summary>
		public Cryptography()
		{
		}

		/// <summary>
		/// 데이터를 암호화합니다.
		/// </summary>
		/// <param name="data">암호화할 문자열 데이터입니다.</param>
		/// <returns>암호화된 문자열입니다.</returns>
		public static string Encrypt(string data)
		{
			return Encrypt(data, null);
		}

		/// <summary>
		/// 데이터를 암호화합니다.
		/// </summary>
		/// <param name="data">암호화할 문자열 데이터입니다.</param>
		/// <param name="key">암호화할 때 사용할 키 값을 가지는 바이트 배열입니다.</param>
		/// <returns>암호화된 문자열입니다.</returns>
		public static string Encrypt(string data, byte[] key)
		{
			System.Text.UTF8Encoding oEncoding = new System.Text.UTF8Encoding();
			byte[] ba = oEncoding.GetBytes(data);
			byte[] ba2 = Encrypt(ba, key);

			return System.Convert.ToBase64String(ba2);
		}

		/// <summary>
		/// 데이터를 암호화합니다.
		/// </summary>
		/// <param name="bytesData">암호화할 바이트 배열입니다.</param>
		/// <returns>암호화된 바이트 배열입니다.</returns>
		public static byte[] Encrypt(byte[] bytesData)
		{
			return Encrypt(bytesData, null);
		}
		/// <summary>
		/// 데이터를 암호화합니다.
		/// </summary>
		/// <param name="bytesData">암호화할 바이트 배열입니다.</param>
		/// <param name="key">암호화할 때 사용할 키 값을 가지는 바이트 배열입니다.</param>
		/// <returns>암호화된 바이트 배열의 데이터입니다.</returns>
		public static byte[] Encrypt(byte[] bytesData, byte[] key)
		{
			MemoryStream memStreamEncryptedData = null;
			ICryptoTransform transform = null;
			CryptoStream encStream = null;

			byte[] rt = null;

			try
			{
				memStreamEncryptedData = new MemoryStream();
				transform = GetCryptoServiceProvider(key);
				encStream = new CryptoStream(memStreamEncryptedData,
					transform,
					CryptoStreamMode.Write);

				encStream.Write(bytesData, 0, bytesData.Length);
				encStream.FlushFinalBlock();

				rt = memStreamEncryptedData.ToArray();
			}
			finally
			{
				if (encStream != null) encStream.Close();
				if (transform != null) transform.Dispose();
				if (memStreamEncryptedData != null) memStreamEncryptedData.Close();
			}

			return rt;
		}

		private static ICryptoTransform GetCryptoServiceProvider(byte[] key)
		{
			TripleDESCryptoServiceProvider des = new TripleDESCryptoServiceProvider();
			des.Mode = CipherMode.CBC;			
			if (key == null) des.Key = Cryptography.BYTES_KEY;
			else des.Key = key;
			des.IV = Cryptography.INIT_VEC;		
			return des.CreateEncryptor();
		}

		private static ICryptoTransform GetDeCryptoServiceProvider(byte[] key)
		{
			TripleDESCryptoServiceProvider des = new TripleDESCryptoServiceProvider();
			des.Mode = CipherMode.CBC;			
			if (key == null) des.Key = Cryptography.BYTES_KEY;
			else des.Key = key;
			des.IV = Cryptography.INIT_VEC;		
			return des.CreateDecryptor();
		}
		
		
		/// <summary>
		/// 데이터를 복호화합니다.
		/// </summary>
		/// <param name="data">암호화된 문자열 데이터입니다.</param>
		/// <returns>복호화된 문자열입니다.</returns>
		public static string Decrypt(string data)
		{
			return Decrypt(data, null);
		}
		/// <summary>
		/// 데이터를 복호화합니다.
		/// </summary>
		/// <param name="data">암호화된 문자열 데이터입니다.</param>
		/// <param name="key">암호화 시 사용한 키 값을 가지는 바이트 배열입니다.</param>
		/// <returns>복호화된 문자열입니다.</returns>
		public static string Decrypt(string data, byte[] key)
		{
			System.Text.UTF8Encoding oEncoding = null;

			byte[] ba = System.Convert.FromBase64String(data);
			byte[] ba2 = Decrypt(ba, key);
			oEncoding = new System.Text.UTF8Encoding();
			return oEncoding.GetString(ba2);
		}

		/// <summary>
		/// 데이터를 복호화합니다.
		/// </summary>
		/// <param name="bytesData">암호화된 바이트 배열의 데이터입니다.</param>
		/// <returns>복호화된 바이트 배열의 데이터입니다.</returns>
		public static byte[] Decrypt(byte[] bytesData)
		{
			return Decrypt(bytesData, null);
		}

		/// <summary>
		/// 데이터를 복호화합니다.
		/// </summary>
		/// <param name="bytesData">암호화된 바이트 배열의 데이터입니다.</param>
		/// <param name="key">암호화 시 사용한 키 값을 가지는 바이트 배열입니다.</param>
		/// <returns>복호화된 바이트 배열의 데이터입니다.</returns>
		public static byte[] Decrypt(byte[] bytesData, byte[] key)
		{
			MemoryStream memStreamDecryptedData = null;
			ICryptoTransform transform = null;
			CryptoStream decStream = null;

			byte[] rt = null;

			try
			{
				memStreamDecryptedData = new MemoryStream();
				transform = GetDeCryptoServiceProvider(key);
				decStream = new CryptoStream(memStreamDecryptedData,
					transform,
					CryptoStreamMode.Write);

				decStream.Write(bytesData, 0, bytesData.Length);
				decStream.FlushFinalBlock();

				rt = memStreamDecryptedData.ToArray();
			}
			finally
			{
				if (decStream != null) decStream.Close();
				if (transform != null) transform.Dispose();
				if (memStreamDecryptedData != null) memStreamDecryptedData.Close();
			}

			return rt;
		}
	}
}
