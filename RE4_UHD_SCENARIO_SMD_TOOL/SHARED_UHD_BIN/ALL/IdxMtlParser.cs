using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Globalization;
using SHARED_UHD_BIN.EXTRACT;

namespace SHARED_UHD_BIN.ALL
{
    public static class IdxMtlParser
    {
        public static IdxMtl Parser(IdxMaterial idxMaterial, UhdTPL uhdTPL, bool IsPS4NS)
        {
            IdxMtl idx = new IdxMtl();
            idx.MtlDic = new Dictionary<string, MtlObj>();

            foreach (var mat in idxMaterial.MaterialDic)
            {
                MtlObj mtl = new MtlObj();

                mtl.map_Kd = GetTexPathRef(uhdTPL.TplArray, mat.Value.diffuse_map, IsPS4NS);

                mtl.Ks = new KsClass(mat.Value.intensity_specular_r, mat.Value.intensity_specular_g, mat.Value.intensity_specular_b);

                mtl.specular_scale = mat.Value.specular_scale;

                if (mat.Value.bump_map != 255)
                {
                    mtl.map_Bump = GetTexPathRef(uhdTPL.TplArray, mat.Value.bump_map, IsPS4NS);
                }

                if (mat.Value.opacity_map != 255)
                {
                    mtl.map_d = GetTexPathRef(uhdTPL.TplArray, mat.Value.opacity_map, IsPS4NS);
                }

                if (mat.Value.generic_specular_map != 255)
                {
                    mtl.ref_specular_map = new TexPathRef(0x07000000, mat.Value.generic_specular_map, "dds");
                }

                if (mat.Value.custom_specular_map != 255)
                {
                    mtl.ref_specular_map = GetTexPathRef(uhdTPL.TplArray, mat.Value.custom_specular_map, IsPS4NS);
                }

                idx.MtlDic.Add(mat.Key, mtl);
            }

            return idx;
        }


        private static TexPathRef GetTexPathRef(TplInfo[] TplArray, byte Index, bool IsPS4NS) 
        {
            if (Index < TplArray.Length)
            {
                var tplInfo = TplArray[Index];
                return new TexPathRef(tplInfo.PackID, tplInfo.TextureID, tplInfo.PixelFormatType, IsPS4NS);
            }
            else 
            {
                return new TexPathRef(0x00000000, 0x00000000, "null");
            }
        }

    }
}
