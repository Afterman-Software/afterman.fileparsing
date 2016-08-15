using System.ComponentModel.DataAnnotations;

namespace AI.TextFileParsing.Enums
{
	public enum CurrencyFormatEnum
	{
		[Display(Description = "0 implied decimals")]
		Format0 = 0,
		[Display(Description = "1 implied decimals")]
		Format1 = 1,
		[Display(Description = "2 implied decimals")]
		Format2 = 2,
		[Display(Description = "3 implied decimals")]
		Format3 = 3,
		[Display(Description = "4 implied decimals")]
		Format4 = 4,
		[Display(Description = "5 implied decimals")]
		Format5 = 5,
		[Display(Description = "6 implied decimals")]
		Format6 = 6
	}
}