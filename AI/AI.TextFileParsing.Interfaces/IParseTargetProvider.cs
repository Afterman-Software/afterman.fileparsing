using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace AI.TextFileParsing.Interfaces
{
    public interface IParseTargetProvider
    {
        IList<IParseTarget> GetAvailableTargets();

        IList<IParseTargetField> GetAvailableFields(IParseTarget obj);
    }
}
