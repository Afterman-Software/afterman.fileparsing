using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace AI.TextFileParsing.DSL
{
    public interface IDslScriptCompiler
    {
        DslAssembly Execute(string script);

        IEnumerable<DslMacro> GetAvailableMacros();
    }
}
