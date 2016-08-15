using AI.TextFileParsing.DSL.Extensions;

namespace AI.TextFileParsing.DSL
{
	public interface IDslScript
	{
		DslStringExtensions Text { get; }

		DslMathExtensions Math { get; }

		DslCommonExtensions Common { get; }

		DslTableExtensions Tables { get; }
	}
}