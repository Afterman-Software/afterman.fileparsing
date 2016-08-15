using System;

namespace AI.Common.Security
{
	public static class RandomStringGenerator
	{
		public static int MinimumLength = 8;
		public static int MaximumLength = 16;
		public static int MinimumLowerAlphaCount = 1;
		public static int MinimumUpperAlphaCount = 1;
		public static int MinimumNumericCount = 1;
		public static int MinimumNonAlphaNumericCount = 1;
		public static int MaximumPatternMatchLength = 3;

		public const string LowerAlphaChars = "abcdefghijklmnopqrstuvwxyz";
		public const string UpperAlphaChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
		public const string NumericChars = "0123456789";
		public const string NonAlphaNumChars = "`~!@#$%^&*()-_=+[]{}|';:,.<>?";
		public const string AllChars = LowerAlphaChars + UpperAlphaChars + NumericChars + NonAlphaNumChars;

		public enum CharacterClass
		{
			LowerAlpha = 1,
			UpperAlpha = 2,
			Numeric = 3,
			NonAlphaNumeric = 4,
			AllChars = 5
		}

		public static string GetCharacterClass(CharacterClass charClass)
		{
			switch (charClass)
			{
				case CharacterClass.LowerAlpha:
					return LowerAlphaChars;
				case CharacterClass.UpperAlpha:
					return UpperAlphaChars;
				case CharacterClass.Numeric:
					return NumericChars;
				case CharacterClass.NonAlphaNumeric:
					return NonAlphaNumChars;
				default:
					return AllChars;
			}
		}

		private static Random rnd = new Random();

		private static int _randomizationSeed = -1;
		public static int RandomizationSeed
		{
			get
			{
				return _randomizationSeed;
			}
			set
			{
				if (value != _randomizationSeed)
				{
					_randomizationSeed = value;
					rnd = new Random(_randomizationSeed);
				}
			}
		}

		public static string GenerateRandomString(int minimumLength = -1, int maximumLength = -1, int minimumLowerAlphaCount = -1, int minimumUpperAlphaCount = -1, int minimumNumericCount = -1, int minimumNonAlphaNumericCount = -1, int maximumPatternMatchLength = -1, string pattern = null)
		{
			if (minimumLength <= -1)
				minimumLength = MinimumLength;
			if (maximumLength <= -1)
				maximumLength = MaximumLength;
			if (minimumLowerAlphaCount <= -1)
				minimumLowerAlphaCount = MinimumLowerAlphaCount;
			if (minimumUpperAlphaCount <= -1)
				minimumUpperAlphaCount = MinimumUpperAlphaCount;
			if (minimumNumericCount <= -1)
				minimumNumericCount = MinimumNumericCount;
			if (minimumNonAlphaNumericCount <= -1)
				minimumNonAlphaNumericCount = MinimumNonAlphaNumericCount;
			if (maximumPatternMatchLength <= -1)
				maximumPatternMatchLength = MaximumPatternMatchLength;

			string currentString = "";
			string newString = "";

			while (string.IsNullOrWhiteSpace(currentString))
			{
				//get random length between min and max
				int newLength = GetRandomNumber(minimumLength, maximumLength);

				//get random characters from each category by count
				for (int i = 0; i < minimumLowerAlphaCount; i++)
				{
					string newRandomLower = GetRandomChar(CharacterClass.LowerAlpha);
					newString += newRandomLower;
				}
				for (int i = 0; i < minimumUpperAlphaCount; i++)
				{
					string newRandomUpper = GetRandomChar(CharacterClass.UpperAlpha);
					newString += newRandomUpper;
				}
				for (int i = 0; i < minimumNumericCount; i++)
				{
					string newRandomNumeric = GetRandomChar(CharacterClass.Numeric);
					newString += newRandomNumeric;
				}
				for (int i = 0; i < minimumNonAlphaNumericCount; i++)
				{
					string newRandomNonAlphaNum = GetRandomChar(CharacterClass.NonAlphaNumeric);
					newString += newRandomNonAlphaNum;
				}

				//get random characters from any category to complete required length
				int remainingLength = newLength - newString.Length;
				for (var i = 0; i < remainingLength; i++)
				{
					int whichChars = GetRandomNumber(1, 4);
					CharacterClass randomCharClass = CharacterClass.AllChars;
					switch (whichChars)
					{
						case 1:
							randomCharClass = CharacterClass.LowerAlpha;
							break;
						case 2:
							randomCharClass = CharacterClass.UpperAlpha;
							break;
						case 3:
							randomCharClass = CharacterClass.Numeric;
							break;
						case 4:
							randomCharClass = CharacterClass.NonAlphaNumeric;
							break;
					}

					string newRandomChar = GetRandomChar(randomCharClass);
					newString += newRandomChar;
				}

				//jumble random characters
				while (currentString.Length != newLength)
				{
					int index = GetRandomNumber(0, newString.Length - 1);
					currentString += newString.Substring(index, 1);
					newString = newString.Remove(index, 1);
				}

				if (maximumPatternMatchLength > 0 && !string.IsNullOrWhiteSpace(pattern))
				{
					for (int i = 0; i <= currentString.Length - maximumPatternMatchLength; i++)
					{
						string newSubString = currentString.Substring(i, maximumPatternMatchLength);
						if (pattern.ToLower().IndexOf(newSubString.ToLower()) != -1)
						{
							currentString = "";
							break;
						}
					}
				}
			}

			return currentString;
		}

		public static bool ValidateRandomString(string value, int minimumLength = -1, int maximumLength = -1, int minimumLowerAlphaCount = -1, int minimumUpperAlphaCount = -1, int minimumNumericCount = -1, int minimumNonAlphaNumericCount = -1, int maximumPatternMatchLength = -1, string pattern = null)
		{
			if (minimumLength <= -1)
				minimumLength = MinimumLength;
			if (maximumLength <= -1)
				maximumLength = MaximumLength;
			if (minimumLowerAlphaCount <= -1)
				minimumLowerAlphaCount = MinimumLowerAlphaCount;
			if (minimumUpperAlphaCount <= -1)
				minimumUpperAlphaCount = MinimumUpperAlphaCount;
			if (minimumNumericCount <= -1)
				minimumNumericCount = MinimumNumericCount;
			if (minimumNonAlphaNumericCount <= -1)
				minimumNonAlphaNumericCount = MinimumNonAlphaNumericCount;
			if (maximumPatternMatchLength <= -1)
				maximumPatternMatchLength = MaximumPatternMatchLength;

			if (value.Length < minimumLength || value.Length > maximumLength)
				return false;

			int count = 0;

			count = GetCharCount(value, CharacterClass.LowerAlpha);
			if (count < minimumLowerAlphaCount)
				return false;

			count = GetCharCount(value, CharacterClass.UpperAlpha);
			if (count < minimumUpperAlphaCount)
				return false;

			count = GetCharCount(value, CharacterClass.Numeric);
			if (count < minimumNumericCount)
				return false;

			count = GetCharCount(value, CharacterClass.NonAlphaNumeric);
			if (count < minimumNonAlphaNumericCount)
				return false;

			if (maximumPatternMatchLength > 0 && !string.IsNullOrWhiteSpace(pattern))
			{
				for (int i = 0; i <= value.Length - maximumPatternMatchLength; i++)
				{
					string newSubString = value.Substring(i, maximumPatternMatchLength);
					if (pattern.ToLower().IndexOf(newSubString.ToLower()) != -1)
						return false;
				}
			}

			return true;
		}

		public static string GetRandomChar(string charString)
		{
			int length = charString.Length;
			int randomIndex = GetRandomNumber(1, length) - 1;
			return charString.Substring(randomIndex, 1);
		}

		public static string GetRandomChar(CharacterClass charClass)
		{
			string charString = GetCharacterClass(charClass);
			return GetRandomChar(charString);
		}

		public static int GetCharCount(string value, string charString)
		{
			int count = 0;
			for (var i = 0; i < value.Length; i++)
			{
				if (charString.IndexOf(value.Substring(i, 1)) != -1)
					count++;
			}
			return count;
		}

		public static int GetCharCount(string value, CharacterClass charClass)
		{
			string charString = GetCharacterClass(charClass);
			return GetCharCount(value, charString);
		}

		public static int GetRandomNumber(int min, int max)
		{
			return rnd.Next(min, max + 1);
		}

		public static string GetStringFromBytes(byte[] bytesToString)
		{
			return System.Text.Encoding.ASCII.GetString(bytesToString);
		}

		public static byte[] GetBytesFromString(string stringToBytes)
		{
			return System.Text.Encoding.ASCII.GetBytes(stringToBytes);
		}

		public static string ToEncodedString(byte[] bytesToEncode)
		{
			return Convert.ToBase64String(bytesToEncode);
		}

		public static byte[] FromEncodedString(string stringToDecode)
		{
			return Convert.FromBase64String(stringToDecode);
		}
	}
}