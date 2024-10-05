using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SHARED_UHD_BIN.ALL
{
    public static class FloatExtensions
    {
        public static string ToFloatString(this float value)
        {
            string s = value.ToString("F9",  System.Globalization.CultureInfo.InvariantCulture);
            s = s.TrimEnd('0');
            s = s.EndsWith(".") ? s+'0' : s;
            return s;
        }

    }
}
