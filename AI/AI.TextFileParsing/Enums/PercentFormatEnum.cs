using System.ComponentModel.DataAnnotations;

namespace AI.TextFileParsing.Enums
{
	public enum PercentFormatEnum
	{
		[Display(Description = "5.555%")]
		Format0 = 0,
		[Display(Description = "5.5500")]
		Format1 = 1,
		[Display(Description = ".05555")]
		Format2 = 2,
		[Display(Description = "%5.555")]
		Format3 = 3,
		[Display(Description = "0 implied decimals")]
		Format4 = 4,
		[Display(Description = "1 implied decimals")]
		Format5 = 5,
		[Display(Description = "2 implied decimals")]
		Format6 = 6,
		[Display(Description = "3 implied decimals")]
		Format7 = 7,
		[Display(Description = "4 implied decimals")]
		Format8 = 8,
		[Display(Description = "5 implied decimals")]
		Format9 = 9,
		[Display(Description = "6 implied decimals")]
		Format10 = 10
	}
}