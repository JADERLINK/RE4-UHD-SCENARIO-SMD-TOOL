using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RE4_UHD_BIN_TOOL.EXTRACT;

namespace RE4_UHD_BIN_TOOL.ALL
{
    public static class IdxMaterialParser
    {
        public static IdxMaterial Parser(UhdBIN uhdBIN) 
        {
            IdxMaterial idx = new IdxMaterial();
            idx.MaterialDic = new Dictionary<string, MaterialPart>();

            for (int i = 0; i < uhdBIN.Materials.Length; i++)
            {
                idx.MaterialDic.Add(CONSTs.UHD_MATERIAL + i.ToString("D3"), uhdBIN.Materials[i].material);
            }

            return idx;
        }

    }
}
