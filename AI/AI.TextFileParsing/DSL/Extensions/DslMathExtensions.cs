using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AI.TextFileParsing.DSL.Extensions
{
    public class DslMathExtensions
    {
        public decimal RoundUp(decimal value)
        {
            return Decimal.Ceiling(value);
        }

        public decimal RoundDown(decimal value)
        {
            return Decimal.Floor(value);
        }

        public decimal Round(decimal value, int precision)
        {
            return Decimal.Round(value, precision);
        }
    }
}
