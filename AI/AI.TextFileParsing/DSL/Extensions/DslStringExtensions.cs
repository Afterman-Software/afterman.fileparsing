using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace AI.TextFileParsing.DSL.Extensions
{
    public class DslStringExtensions
    {


        public string Reverse(string value)
        {
            if (null == value)
                return null;
            return value.Reverse().ToString();
        }

        public static string Remove(string value, string itemToRemove)
        {
            if (null == value)
                return null;
            return value.Replace(itemToRemove, String.Empty);
        }

        public static string Concat(string val1, string val2)
        {
            return String.Format("{0}{1}", val1, val2);
        }

        public string Trim(string value)
        {
            if (null == value)
                return null;
            return value.Trim();
        }

        public string ToLower(string value)
        {
            if (null == value)
                return null;
            return value.ToLower();
        }

       

     


        public string ToUpper(string value)
        {
            if (null == value)
                return null;
            return value.ToUpper();
        }

        public bool IsMatch(string value, string regex)
        {
            if (null == value)
                return false;
            if (null == regex)
                return false;

            return Regex.IsMatch(value, regex);

        }

    }
}
