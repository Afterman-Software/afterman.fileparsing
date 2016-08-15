using System.ComponentModel.DataAnnotations;

namespace AI.TextFileParsing.Enums
{
	public enum DataTypeEnum : int
	{
		[Display(Description = "String")]
		String = 0,
		[Display(Description = "Integer")]
		Integer = 1,
		[Display(Description = "Decimal (Currency)")]
		DecimalCurrency = 2,
		[Display(Description = "Decimal (Percent)")]
		DecimalPercent = 3,
		[Display(Description = "Date")]
		Date = 4,
		[Display(Description = "Time")]
		Time = 5,
        [Display(Description = "Boolean (T/F)")]
        Boolean = 6,
	}
}