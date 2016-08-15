using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AI.TextFileParsing.DSL
{
    public class DslCompileException : Exception
    {
        public DslCompileException(string message)
            :base(message)
        {
            
        }

        public DslCompileException()
        {

        }
    }
}
