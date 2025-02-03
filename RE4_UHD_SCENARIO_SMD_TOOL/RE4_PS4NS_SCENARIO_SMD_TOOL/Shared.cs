using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SHARED_UHD_SCENARIO_SMD
{
    public static class Shared
    {
        public const string VERSION = "B.1.2.2 (2024-02-03)";

        public static string HeaderText()
        {
            return "# github.com/JADERLINK/RE4-UHD-SCENARIO-SMD-TOOL" + Environment.NewLine +
                   "# youtube.com/@JADERLINK" + Environment.NewLine +
                   "# RE4_PS4NS_SCENARIO_SMD_TOOL by: JADERLINK" + Environment.NewLine +
                   "# Thanks to \"mariokart64n\" and \"CodeMan02Fr\"" + Environment.NewLine +
                   "# Material information by \"Albert\"" + Environment.NewLine +
                  $"# Version {VERSION}";
        }
    }
}
