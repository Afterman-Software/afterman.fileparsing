using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AI.TextFileParsing.Generation
{
    public interface IOutboundHeaderGenerator
    {
        IList<string> GetHeaders(Object recordPartType);
    }
}
