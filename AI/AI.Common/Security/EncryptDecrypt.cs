using System.IO;
using System.Security.Cryptography;

namespace AI.Common.Security
{
	public static class EncryptDecrypt
	{
		private const char separator = '*';

		private static string[] encryptKeyArray;
		private static string[] initVectorArray;

        private static readonly byte[] _rgbKey = System.Text.Encoding.ASCII.GetBytes(@"R0y@lA!1I@nc3s$#@!rul3z1"); // must be 16 or 24 bytes
        private static readonly byte[] _initVector = System.Text.Encoding.ASCII.GetBytes(@"@rc#iT3ct!ngINn0"); // must be 16 bytes

		private static int _obscurityLevel = -1;
		public static int ObscurityLevel
		{
			get
			{
				return _obscurityLevel;
			}
			set
			{
				if (value != _obscurityLevel)
				{
					_obscurityLevel = value;
					InitializeObscurity();
				}
			}
		}
		private static int _randomizationLevel = 10;
		public static int RandomizationLevel
		{
			get
			{
				return _randomizationLevel;
			}
			set
			{
				if (value != _randomizationLevel)
				{
					_randomizationLevel = value;
					InitializeObscurity();
				}
			}
		}

		public static void SetObscurityInitializationValues(int obscurityLevel, int randomizationLevel)
		{
			_obscurityLevel = obscurityLevel;
			_randomizationLevel = randomizationLevel;
			InitializeObscurity();
		}

		static EncryptDecrypt()
		{
			if (_obscurityLevel <= 0)
				return;

			if (_randomizationLevel <= 0)
				_randomizationLevel = 4;

			InitializeObscurity();
		}

		private static void InitializeObscurity()
		{
			encryptKeyArray = new string[_randomizationLevel];
			for (int i = 0; i < encryptKeyArray.Length; i++)
			{
				bool validated = false;
				while (!validated)
				{
					encryptKeyArray[i] = RandomStringGenerator.GenerateRandomString(24, 24, 2, 2, 2, 2);
					validated = RandomStringGenerator.ValidateRandomString(encryptKeyArray[i], 24, 24, 2, 2, 2, 2);
				}
			}

			initVectorArray = new string[_randomizationLevel];
			for (int i = 0; i < initVectorArray.Length; i++)
			{
				bool validated = false;
				while (!validated)
				{
					initVectorArray[i] = RandomStringGenerator.GenerateRandomString(16, 16, 2, 2, 2, 2);
					validated = RandomStringGenerator.ValidateRandomString(initVectorArray[i], 16, 16, 2, 2, 2, 2);
				}
			}
		}

		public static string Encrypt(string dataToEncrypt)
		{
			if (string.IsNullOrWhiteSpace(dataToEncrypt))
			{
				return dataToEncrypt;
			}

			byte[] inputBytes = RandomStringGenerator.GetBytesFromString(dataToEncrypt);

			var des3Encrypt = CreateEncryptor();

			MemoryStream mstream = new MemoryStream();
			CryptoStream cstream = new CryptoStream(mstream, des3Encrypt, CryptoStreamMode.Write);
			cstream.Write(inputBytes, 0, inputBytes.Length);
			cstream.FlushFinalBlock();

			byte[] encryptedDataBytes = mstream.ToArray();

			mstream.Close();
			cstream.Close();

			return RandomStringGenerator.ToEncodedString(encryptedDataBytes);
		}

		private static string EncryptForObscurity(string dataToEncrypt, int randomization)
		{
			if (string.IsNullOrWhiteSpace(dataToEncrypt))
			{
				return dataToEncrypt;
			}

			byte[] key = RandomStringGenerator.GetBytesFromString(encryptKeyArray[randomization]);
			byte[] iv = RandomStringGenerator.GetBytesFromString(initVectorArray[randomization]);

			byte[] inputBytes = RandomStringGenerator.GetBytesFromString(dataToEncrypt);

			var des3Encrypt = CreateEncryptor();

			MemoryStream mstream = new MemoryStream();
			CryptoStream cstream = new CryptoStream(mstream, des3Encrypt, CryptoStreamMode.Write);
			cstream.Write(inputBytes, 0, inputBytes.Length);
			cstream.FlushFinalBlock();

			byte[] encryptedDataBytes = mstream.ToArray();

			mstream.Close();
			cstream.Close();

			return randomization.ToString() + separator + RandomStringGenerator.ToEncodedString(encryptedDataBytes);
		}

		public static string Decrypt(string dataToDecrypt)
		{
			if (string.IsNullOrWhiteSpace(dataToDecrypt))
			{
				return dataToDecrypt;
			}

			byte[] decryptedDataBytes = null;
			MemoryStream mstream = null;
			CryptoStream cstream = null;
			try
			{
				byte[] encryptedBytes = RandomStringGenerator.FromEncodedString(dataToDecrypt);
				var des3Decrypt = CreateDecryptor();

				mstream = new MemoryStream();
				cstream = new CryptoStream(mstream, des3Decrypt, CryptoStreamMode.Write);
				cstream.Write(encryptedBytes, 0, encryptedBytes.Length);
				cstream.FlushFinalBlock();

				decryptedDataBytes = mstream.ToArray();
			}
			catch
			{
				//swallow errors
			}
			finally
			{
				if (mstream != null)
					mstream.Close();
				if (cstream != null)
					cstream.Close();
			}

			if (decryptedDataBytes == null)
				return null;

		    return RandomStringGenerator.GetStringFromBytes(decryptedDataBytes);
		}

		private static string DecryptForObscurity(string dataToDecrypt, int randomization)
		{
			if (string.IsNullOrWhiteSpace(dataToDecrypt))
			{
				return dataToDecrypt;
			}

			byte[] decryptedDataBytes = null;
			MemoryStream mstream = null;
			CryptoStream cstream = null;
			try
			{
				byte[] key = RandomStringGenerator.GetBytesFromString(encryptKeyArray[randomization]);
				byte[] iv = RandomStringGenerator.GetBytesFromString(initVectorArray[randomization]);

				byte[] encryptedBytes = RandomStringGenerator.FromEncodedString(dataToDecrypt);

				TripleDESCryptoServiceProvider des3provider = new TripleDESCryptoServiceProvider();
				des3provider.Mode = CipherMode.CBC;
				ICryptoTransform des3decrypt = des3provider.CreateDecryptor(key, iv);

				mstream = new MemoryStream();
				cstream = new CryptoStream(mstream, des3decrypt, CryptoStreamMode.Write);
				cstream.Write(encryptedBytes, 0, encryptedBytes.Length);
				cstream.FlushFinalBlock();

				decryptedDataBytes = mstream.ToArray();
			}
			catch
			{
				//swallow errors
			}
			finally
			{
				if (mstream != null)
					mstream.Close();
				if (cstream != null)
					cstream.Close();
			}

			if (decryptedDataBytes == null)
				return null;
			else
				return RandomStringGenerator.GetStringFromBytes(decryptedDataBytes);
		}

		public static string Encode(string base64String)
		{
			if (string.IsNullOrWhiteSpace(base64String))
				return base64String;
			else
				return base64String.Replace("/", "!").Replace("=", "-").Replace("+", "^");
		}

		public static string Decode(string encodedString)
		{
			if (string.IsNullOrWhiteSpace(encodedString))
				return encodedString;
			else
				return encodedString.Replace("!", "/").Replace("-", "=").Replace("^", "+");
		}

		public static string EncryptEncode(string dataToEncryptEncode)
		{
			if (_obscurityLevel <= 0)
			{
				string base64String = Encrypt(dataToEncryptEncode);
				string encodedString = Encode(base64String);
				return encodedString;
			}
			else
			{
				return EncryptEncodeForObscurity(dataToEncryptEncode);
			}
		}

		public static string DecodeDecrypt(string dataToDecodeDecrypt)
		{
			if (_obscurityLevel <= 0)
			{
				string encryptedData = Decode(dataToDecodeDecrypt);
				string decryptedString = Decrypt(encryptedData);
				return decryptedString;
			}
			else
			{
				return DecodeDecryptForObscurity(dataToDecodeDecrypt);
			}
		}

		private static string EncryptEncodeForObscurity(string dataToEncryptEncode)
		{
			int randomization;
			string base64String;
			string encodedString = dataToEncryptEncode;
			int counter = 0;

			while (counter < _obscurityLevel)
			{
				randomization = RandomStringGenerator.GetRandomNumber(0, _randomizationLevel - 1);
				base64String = EncryptForObscurity(encodedString, randomization);
				encodedString = Encode(base64String);
				counter++;
			}

			return encodedString;
		}

		private static string DecodeDecryptForObscurity(string dataToDecodeDecrypt)
		{
			string encryptedData;
			string[] encryptedDataArray;
			int randomization;
			string decryptedString = dataToDecodeDecrypt;
			int counter = 0;

			while (counter < _obscurityLevel)
			{
				encryptedData = Decode(decryptedString);
				encryptedDataArray = encryptedData.Split(separator);
				randomization = int.Parse(encryptedDataArray[0]);
				decryptedString = DecryptForObscurity(encryptedDataArray[1], randomization);
				counter++;
			}

			return decryptedString;
		}

	    public static ICryptoTransform CreateEncryptor()
	    {
            var des3Provider = new TripleDESCryptoServiceProvider {Mode = CipherMode.CBC};
	        return des3Provider.CreateEncryptor(_rgbKey, _initVector);
	    }

        public static ICryptoTransform CreateDecryptor()
        {
            var des3Provider = new TripleDESCryptoServiceProvider {Mode = CipherMode.CBC};
            return des3Provider.CreateDecryptor(_rgbKey, _initVector);
        }
	}
}