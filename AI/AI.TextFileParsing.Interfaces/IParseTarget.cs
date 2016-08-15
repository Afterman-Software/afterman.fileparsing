using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace AI.TextFileParsing.Interfaces
{
    public interface IParseTarget
    {
        string Name { get; set; }

        string FullName { get; set; }

    }

    public interface IParseTargetField
    {
        string Name { get; set; }

        string FullName { get; set; }

        string TypeName { get; set; }

    }
}
