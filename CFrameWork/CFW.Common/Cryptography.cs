using System;
using System.IO;
using System.Security.Cryptography;

namespace CFW.Common
{
	/// <summary>
	/// Cryptography�� ���� ��� �����Դϴ�.
	/// </summary>
	public class Cryptography
	{
		internal static byte[] INIT_VEC = {0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01};
		// BYTES_KEY �� ���� ��� !!!!!!!!!!!!!!!!!
		internal static byte[] BYTES_KEY = { 0x74, 0x81, 0x3C, 0x71, 0x9C, 0x23, 0xD4, 0x82, 0xA1, 0x63, 0xD3, 0xA3, 0x32, 0x59, 0x75, 0x57 };

		/// <summary>
		/// �������Դϴ�.
		/// </summary>
		public Cryptography()
		{
		}

		/// <summary>
		/// �����͸� ��ȣȭ�մϴ�.
		/// </summary>
		/// <param name="data">��ȣȭ�� ���ڿ� �������Դϴ�.</param>
		/// <returns>��ȣȭ�� ���ڿ��Դϴ�.</returns>
		public static string Encrypt(string data)
		{
			return Encrypt(data, null);
		}

		/// <summary>
		/// �����͸� ��ȣȭ�մϴ�.
		/// </summary>
		/// <param name="data">��ȣȭ�� ���ڿ� �������Դϴ�.</param>
		/// <param name="key">��ȣȭ�� �� ����� Ű ���� ������ ����Ʈ �迭�Դϴ�.</param>
		/// <returns>��ȣȭ�� ���ڿ��Դϴ�.</returns>
		public static string Encrypt(string data, byte[] key)
		{
			System.Text.UTF8Encoding oEncoding = new System.Text.UTF8Encoding();
			byte[] ba = oEncoding.GetBytes(data);
			byte[] ba2 = Encrypt(ba, key);

			return System.Convert.ToBase64String(ba2);
		}

		/// <summary>
		/// �����͸� ��ȣȭ�մϴ�.
		/// </summary>
		/// <param name="bytesData">��ȣȭ�� ����Ʈ �迭�Դϴ�.</param>
		/// <returns>��ȣȭ�� ����Ʈ �迭�Դϴ�.</returns>
		public static byte[] Encrypt(byte[] bytesData)
		{
			return Encrypt(bytesData, null);
		}
		/// <summary>
		/// �����͸� ��ȣȭ�մϴ�.
		/// </summary>
		/// <param name="bytesData">��ȣȭ�� ����Ʈ �迭�Դϴ�.</param>
		/// <param name="key">��ȣȭ�� �� ����� Ű ���� ������ ����Ʈ �迭�Դϴ�.</param>
		/// <returns>��ȣȭ�� ����Ʈ �迭�� �������Դϴ�.</returns>
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
		/// �����͸� ��ȣȭ�մϴ�.
		/// </summary>
		/// <param name="data">��ȣȭ�� ���ڿ� �������Դϴ�.</param>
		/// <returns>��ȣȭ�� ���ڿ��Դϴ�.</returns>
		public static string Decrypt(string data)
		{
			return Decrypt(data, null);
		}
		/// <summary>
		/// �����͸� ��ȣȭ�մϴ�.
		/// </summary>
		/// <param name="data">��ȣȭ�� ���ڿ� �������Դϴ�.</param>
		/// <param name="key">��ȣȭ �� ����� Ű ���� ������ ����Ʈ �迭�Դϴ�.</param>
		/// <returns>��ȣȭ�� ���ڿ��Դϴ�.</returns>
		public static string Decrypt(string data, byte[] key)
		{
			System.Text.UTF8Encoding oEncoding = null;

			byte[] ba = System.Convert.FromBase64String(data);
			byte[] ba2 = Decrypt(ba, key);
			oEncoding = new System.Text.UTF8Encoding();
			return oEncoding.GetString(ba2);
		}

		/// <summary>
		/// �����͸� ��ȣȭ�մϴ�.
		/// </summary>
		/// <param name="bytesData">��ȣȭ�� ����Ʈ �迭�� �������Դϴ�.</param>
		/// <returns>��ȣȭ�� ����Ʈ �迭�� �������Դϴ�.</returns>
		public static byte[] Decrypt(byte[] bytesData)
		{
			return Decrypt(bytesData, null);
		}

		/// <summary>
		/// �����͸� ��ȣȭ�մϴ�.
		/// </summary>
		/// <param name="bytesData">��ȣȭ�� ����Ʈ �迭�� �������Դϴ�.</param>
		/// <param name="key">��ȣȭ �� ����� Ű ���� ������ ����Ʈ �迭�Դϴ�.</param>
		/// <returns>��ȣȭ�� ����Ʈ �迭�� �������Դϴ�.</returns>
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
