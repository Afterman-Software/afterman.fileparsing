/// <summary>
/// Extensions for number data types, such as Integers and Longs.
/// </summary>
public static class NumberExtensions
{
	/// <summary>
	/// Convert an Integer representing a file size to a file size string, such as 1000B, 1000KB, 1000GB, or 1000TB,
	/// including up to 3 decimal points.
	/// </summary>
	/// <param name="fileSize">The Integer instance to convert to a file size String.</param>
	/// <returns>A String, such as such as 1000B, 1000KB, 1000GB, or 1000TB, representing a file size.</returns>
	public static string ToFileSizeString(this int fileSize)
	{
		decimal workingFileSize = fileSize;
		string suffix = "B";
		bool cont = true;
		while (cont && workingFileSize > 1024.0M)
		{
			workingFileSize = workingFileSize / 1024.0M;
			switch (suffix.ToLower().Trim())
			{
				case "b":
					suffix = "KB";
					break;
				case "kb":
					suffix = "MB";
					break;
				case "mb":
					suffix = "GB";
					break;
				case "gb":
					suffix = "TB";
					cont = false;
					break;
			}
		}
		return workingFileSize.ToString("0.0##") + suffix;
	}

	/// <summary>
	/// Convert a Long representing a file size to a file size string, such as 1000B, 1000KB, 1000GB, or 1000TB,
	/// including up to 3 decimal points.
	/// </summary>
	/// <param name="fileSize">The Long instance to convert to a file size String.</param>
	/// <returns>A String, such as such as 1000B, 1000KB, 1000GB, or 1000TB, representing a file size.</returns>
	public static string ToFileSizeString(this long fileSize)
	{
		decimal workingFileSize = fileSize;
		string suffix = "B";
		bool cont = true;
		while (cont && workingFileSize > 1024.0M)
		{
			workingFileSize = workingFileSize / 1024.0M;
			switch (suffix.ToLower().Trim())
			{
				case "b":
					suffix = "KB";
					break;
				case "kb":
					suffix = "MB";
					break;
				case "mb":
					suffix = "GB";
					break;
				case "gb":
					suffix = "TB";
					cont = false;
					break;
			}
		}
		return workingFileSize.ToString("0.0##") + suffix;
	}

	public static bool Between(this int value, int minValue, int maxValue, bool minInclusive = true,
							   bool maxInclusive = true)
	{
		if (minInclusive && maxInclusive)
			return value >= minValue && value <= maxValue;
		if (minInclusive)
			return value > minValue && value <= maxValue;
		if (maxInclusive)
			return value >= minValue && value < maxValue;
		return value > minValue && value < maxValue;
	}

	public static bool Between(this long value, long minValue, long maxValue, bool minInclusive = true,
							   bool maxInclusive = true)
	{
		if (minInclusive && maxInclusive)
			return value >= minValue && value <= maxValue;
		if (minInclusive)
			return value > minValue && value <= maxValue;
		if (maxInclusive)
			return value >= minValue && value < maxValue;
		return value > minValue && value < maxValue;
	}
}