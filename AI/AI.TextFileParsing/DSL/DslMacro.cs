using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AI.TextFileParsing.DSL
{
    public class DslMacro
    {
        public DslMacro(string name, string category)
        {
            this.Name = name;
            this.Category = category;
        }

        public string Name { get; protected set; }

        public string Category { get; protected set; }
    }
}
