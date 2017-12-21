using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyRevit.Utilities
{
    public static class VLStringHelper
    {
        public static string TrimEnd(this string str, string strToTrim)
        {
            var length = strToTrim.Length;
            if (str.Length <= length)
                return str;
            if (str.Substring(str.Length - length) == strToTrim)
            {
                return str.Substring(0, str.Length - length);
            }
            return str;
        }
    }
}
