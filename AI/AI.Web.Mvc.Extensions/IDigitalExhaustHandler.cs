using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AI.Web.Mvc.Extensions
{
    using AI.Common.Tracking;

    public interface IDigitalExhaustHandler
    {
        void Send(DigitalExhaust exhaust);
    }

    public static class Hack
    {
        public static IDigitalExhaustHandler DigitalExhaustHandler
        {
            get; set;
        }
    }
}
