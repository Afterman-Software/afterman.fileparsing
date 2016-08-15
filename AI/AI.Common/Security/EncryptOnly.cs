using System;
using System.Security.Cryptography;

namespace AI.Common.Security
{
	public static class EncryptOnly
	{
		public static int SaltSize = 16;

		public enum SaltMethod
		{
			PreSalt = 1,
			PostSalt = 2,
			SplitSalt = 3,
			SparseSalt = 4,
			DoubleSalt = 5
		}

		public enum HashSize
		{
			S160 = 1,
			S256 = 2,
			S384 = 3,
			S512 = 4
		}

		public static bool VerifyPassword(string password, string salt, string storedHash, SaltMethod saltMethod = SaltMethod.DoubleSalt, HashSize hashSize = HashSize.S160)
		{
			if (string.IsNullOrWhiteSpace(password))
			{
				throw new ArgumentNullException("password");
			}
			if (string.IsNullOrWhiteSpace(salt))
			{
				throw new ArgumentNullException("salt");
			}

			string verifyHash = ComputeHash(password, salt, saltMethod, hashSize);
			return (verifyHash == storedHash);
		}

		public static string CreateRandomSalt(int saltSize = -1)
		{
			if (saltSize <= -1)
				saltSize = SaltSize;

			byte[] randomBytes = new byte[saltSize];
			RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider();
			rng.GetBytes(randomBytes);
			rng.Dispose();
			string salt = RandomStringGenerator.ToEncodedString(randomBytes);
			return salt;
		}

		public static string GetSaltedPassword(string password, string salt, SaltMethod saltMethod = SaltMethod.DoubleSalt)
		{
			if (string.IsNullOrWhiteSpace(password))
			{
				throw new ArgumentNullException("password");
			}
			if (string.IsNullOrWhiteSpace(salt))
			{
				throw new ArgumentNullException("salt");
			}

			switch (saltMethod)
			{
				case SaltMethod.PreSalt:
					return salt + password;
				case SaltMethod.PostSalt:
					return password + salt;
				case SaltMethod.SplitSalt:
					int saltSize = salt.Length;
					int halfSaltSize = saltSize / 2;
					string preSalt = salt.Substring(0, halfSaltSize);
					string postSalt = salt.Substring(halfSaltSize);
					return preSalt + password + postSalt;
				case SaltMethod.SparseSalt:
					string result = "";
					char[] passwordChars = password.ToCharArray();
					char[] saltChars = salt.ToCharArray();
					int maxLoop = Math.Min(passwordChars.Length, saltChars.Length);
					for (int i = 0; i < maxLoop; i++)
					{
						result += passwordChars[i] + saltChars[i];
					}
					if (passwordChars.Length > maxLoop)
					{
						result += password.Substring(maxLoop);
					}
					else if (saltChars.Length > maxLoop)
					{
						result += salt.Substring(maxLoop);
					}
					return result;
				default: //DoubleSalt
					return salt + password + salt;
			}
		}

		public static byte[] ComputeHash(byte[] bytesToHash, HashSize hashSize = HashSize.S160)
		{
			if (bytesToHash == null || bytesToHash.Length <= 0)
			{
				throw new ArgumentNullException("bytesToHash");
			}

			HashAlgorithm hasher;
			switch (hashSize)
			{
				case HashSize.S256:
					hasher = new SHA256CryptoServiceProvider();
					break;
				case HashSize.S384:
					hasher = new SHA384CryptoServiceProvider();
					break;
				case HashSize.S512:
					hasher = new SHA512CryptoServiceProvider();
					break;
				default: //HashSize.S160:
					hasher = new SHA1CryptoServiceProvider();
					break;
			}
			byte[] hash = hasher.ComputeHash(bytesToHash);
			hasher.Dispose();
			return hash;
		}

		public static string ComputeHash(string password, string salt, SaltMethod saltMethod = SaltMethod.DoubleSalt, HashSize hashSize = HashSize.S160)
		{
			if (string.IsNullOrWhiteSpace(password))
			{
				throw new ArgumentNullException("password");
			}
			if (string.IsNullOrWhiteSpace(salt))
			{
				throw new ArgumentNullException("salt");
			}

			string saltedPassword = GetSaltedPassword(password, salt, saltMethod);
			byte[] bytesOfString = RandomStringGenerator.GetBytesFromString(saltedPassword);
			byte[] hash = ComputeHash(bytesOfString, hashSize);
			return RandomStringGenerator.ToEncodedString(hash);
		}

		public static string GeneratePassword(int minimumLength = -1, int maximumLength = -1, int minimumLowerAlphaCount = -1, int minimumUpperAlphaCount = -1, int minimumNumericCount = -1, int minimumNonAlphaNumericCount = -1, int maximumPatternMatchLength = -1, string pattern = null)
		{
			string result;
			while (true)
			{
				result = RandomStringGenerator.GenerateRandomString(minimumLength, maximumLength, minimumLowerAlphaCount, minimumUpperAlphaCount, minimumNumericCount, minimumNonAlphaNumericCount, maximumPatternMatchLength, pattern);
				if (VerifyPasswordComplexity(result, minimumLength, maximumLength, minimumLowerAlphaCount, minimumUpperAlphaCount, minimumNumericCount, minimumNonAlphaNumericCount, maximumPatternMatchLength, pattern))
				{
					break;
				}
			}
			return result;
		}

		public static bool VerifyPasswordComplexity(string password, int minimumLength = -1, int maximumLength = -1, int minimumLowerAlphaCount = -1, int minimumUpperAlphaCount = -1, int minimumNumericCount = -1, int minimumNonAlphaNumericCount = -1, int maximumPatternMatchLength = -1, string pattern = null)
		{
			if (string.IsNullOrWhiteSpace(password))
			{
				throw new ArgumentNullException("password");
			}

			return RandomStringGenerator.ValidateRandomString(password, minimumLength, maximumLength, minimumLowerAlphaCount, minimumUpperAlphaCount, minimumNumericCount, minimumNonAlphaNumericCount, maximumPatternMatchLength, pattern);
		}
	}
}