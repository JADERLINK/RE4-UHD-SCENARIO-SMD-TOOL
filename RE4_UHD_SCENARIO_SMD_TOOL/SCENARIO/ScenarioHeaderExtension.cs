using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RE4_UHD_BIN_TOOL.EXTRACT;

namespace RE4_UHD_SCENARIO_SMD_TOOL.SCENARIO
{
    public static class ScenarioHeaderExtension
    {
        public static bool ReturnsIsEnableVertexColors(this UhdBinHeader header)
        {
            return (header.texture2_flags & 0x4000) == 0x4000; // 0x4000 representa qual é o bit ativo para abilitar as vertex colors
        }

    }
}
