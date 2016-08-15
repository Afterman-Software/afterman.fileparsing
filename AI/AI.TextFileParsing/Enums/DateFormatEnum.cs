using System.ComponentModel.DataAnnotations;

namespace AI.TextFileParsing.Enums
{
	public enum DateFormatEnum
	{
		[Display(Description = "Unknown Format")]
		Format0 = 0,
		[Display(Description = "MM/dd/yy")]
		Format1 = 1,
		[Display(Description = "M/d/yyyy")]
		Format2 = 2,
		[Display(Description = "MM/dd/yyyy")]
		Format3 = 3,
		[Display(Description = "MM-dd-yy")]
		Format4 = 4,
		[Display(Description = "MM-dd-yyyy")]
		Format5 = 5,
		[Display(Description = "dd-MMM-yy")]
		Format6 = 6,
		[Display(Description = "MMM-dd-yy")]
		Format7 = 7,
		[Display(Description = "MMM-dd-yyyy")]
		Format8 = 8,
		[Display(Description = "MMM-yy")]
		Format9 = 9,
		[Display(Description = "y-MMM/yy-MMM")]
		Format10 = 10,
		[Display(Description = "MMM-yyyy")]
		Format11 = 11,
		[Display(Description = "yyyy-MM-dd")]
		Format12 = 12,
		[Display(Description = "MMddyy")]
		Format13 = 13,
		[Display(Description = "MMddyy/Mddyy")]
		Format14 = 14,
		[Display(Description = "MMddyyyy/Mddyyyy")]
		Format15 = 15,
		[Display(Description = "yyMMdd")]
		Format16 = 16,
		[Display(Description = "ddMMyy")]
		Format17 = 17,
		[Display(Description = "MMddyyyy")]
		Format18 = 18,
		[Display(Description = "yyddMM")]
		Format19 = 19,
		[Display(Description = "yyyyddMM")]
		Format20 = 20,
		[Display(Description = "yyyyMMdd")]
		Format21 = 21,
		[Display(Description = "yyyyJJJ")]
		Format22 = 22,
		[Display(Description = "yyJJJ")]
		Format23 = 23,
		[Display(Description = "JJJyy")]
		Format24 = 24,
		[Display(Description = "JJJyyyy")]
		Format25 = 25,
		[Display(Description = "#yyyy-MM-dd#")]
		Format26 = 26,
	}
}