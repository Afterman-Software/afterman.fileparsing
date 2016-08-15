
namespace AI.TextFileParsing.Parsers
{
    public interface IOnDemandTextFileParser : ITextFileParser
    {
        string ReadField(int fieldIndex);
        string ReadField(string fieldName);
    }
}