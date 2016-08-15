using System.ComponentModel.DataAnnotations;

namespace AI.TextFileParsing.Enums
{
	public enum TimeFormatEnum
	{
		[Display(Description = "Unknown Format")]
		TimeFormat0 = 0,
		[Display(Description = "h:mm")]
		TimeFormat1 = 1,
		[Display(Description = "h:mm:ss")]
		TimeFormat2 = 2,
		[Display(Description = "hh:mm")]
		TimeFormat3 = 3,
		[Display(Description = "hh:mm:ss")]
		TimeFormat4 = 4,
		[Display(Description = "h:mm tt")]
		TimeFormat5 = 5,
		[Display(Description = "h:mmtt")]
		TimeFormat6 = 6,
		[Display(Description = "h:mm:ss tt")]
		TimeFormat7 = 7,
		[Display(Description = "h:mm:sstt")]
		TimeFormat8 = 8,
		[Display(Description = "hh:mm tt")]
		TimeFormat9 = 9,
		[Display(Description = "hh:mmtt")]
		TimeFormat10 = 10,
		[Display(Description = "hh:mm:ss tt")]
		TimeFormat11 = 11,
		[Display(Description = "hh:mm:sstt")]
		TimeFormat12 = 12,
		[Display(Description = "hhmm")]
		TimeFormat13 = 13,
		[Display(Description = "hhmmss")]
		TimeFormat14 = 14,
		[Display(Description = "hhmmtt")]
		TimeFormat15 = 15,
		[Display(Description = "hhmmsstt")]
		TimeFormat16 = 16,
		[Display(Description = "H:mm")]
		TimeFormat17 = 17,
		[Display(Description = "H:mm:ss")]
		TimeFormat18 = 18,
		[Display(Description = "HH:mm")]
		TimeFormat19 = 19,
		[Display(Description = "HH:mm:ss")]
		TimeFormat20 = 20,
		[Display(Description = "H:mm tt")]
		TimeFormat21 = 21,
		[Display(Description = "H:mmtt")]
		TimeFormat22 = 22,
		[Display(Description = "H:mm:ss tt")]
		TimeFormat23 = 23,
		[Display(Description = "H:mm:sstt")]
		TimeFormat24 = 24,
		[Display(Description = "HH:mm tt")]
		TimeFormat25 = 25,
		[Display(Description = "HH:mmtt")]
		TimeFormat26 = 26,
		[Display(Description = "HH:mm:ss tt")]
		TimeFormat27 = 27,
		[Display(Description = "HH:mm:sstt")]
		TimeFormat28 = 28,
		[Display(Description = "HHmm")]
		TimeFormat29 = 29,
		[Display(Description = "HHmmss")]
		TimeFormat30 = 30,
		[Display(Description = "HHmmtt")]
		TimeFormat31 = 31,
		[Display(Description = "HHmmsstt")]
		TimeFormat32 = 32
	}
}