using System.ComponentModel;

namespace AI.TextFileParsing.Enums
{
    public enum FileTypeGroupEnum
    {
        [Description("in")]
        Inbound = 0,
        [Description("out")]
        Outbound = 1,
        [Description("rpt")]
        Report = 3,
        [Description("inedi")]
        InboundEdi = 4,
    }
}