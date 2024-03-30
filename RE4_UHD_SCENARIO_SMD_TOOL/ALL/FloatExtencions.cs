using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RE4_UHD_BIN_TOOL.ALL
{
    public static class FloatExtencions
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
