using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AI.TextFileParsing.DSL.Extensions
{
    public class DslCommonExtensions
    {
        public string UniqueKey()
        {
            return Guid.NewGuid().ToString();
        }
    }
}
