using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using SHARED_UHD_BIN.EXTRACT;

namespace SHARED_UHD_BIN.REPACK
{
   public static class TPLmakeFile
   {
        public static void MakeFile(UhdTPL uhdTPL, Stream stream, long startOffset, out long endOffset, bool IsPS4NS) 
        {
            BinaryWriter bw = new BinaryWriter(stream);
            bw.BaseStream.Position = startOffset;

            uint length = (uint)uhdTPL.TplArray.Length;

            uint Magic = 0x78563412;
            if (IsPS4NS)
            {
                Magic = 0x12345678;
            }

            bw.Write((uint)Magic); //magic
            bw.Write((uint)length); // quantidade de tpl

            uint FirstOffset = 0x0C;
            if (IsPS4NS) 
            {
                FirstOffset = 0x10;
            }

            bw.Write((uint)FirstOffset); // primeiro offset

            if (IsPS4NS)
            {
                bw.Write((uint)0); // FirstOffset part2
            }

            uint tempOffset = 0xC + (8 * length);

            if (IsPS4NS)
            {
                tempOffset = 0x10 + (16 * length);
            }

            for (int i = 0; i < length; i++)
            {
                bw.Write((uint)tempOffset);
                if (IsPS4NS)
                {
                    bw.Write((uint)0x0); // tempOffset part2
                }
                bw.Write((uint)0x0);
                if (IsPS4NS)
                {
                    bw.Write((uint)0x0); // part2
                }
                tempOffset += 36;
                if (IsPS4NS)
                {
                    tempOffset += 4;
                }
            }

            for (int i = 0; i < length; i++)
            {
                TplInfo tplInfo = uhdTPL.TplArray[i];

                bw.Write((ushort)tplInfo.height); // Primeiro Altura
                bw.Write((ushort)tplInfo.width);  // Segundo Largura
                bw.Write((uint)tplInfo.PixelFormatType);
                bw.Write((uint)tempOffset);
                if (IsPS4NS)
                {
                    bw.Write((uint)0x0); //tempOffset part2
                }
                bw.Write((uint)tplInfo.wrap_s);
                bw.Write((uint)tplInfo.wrap_t);
                bw.Write((uint)tplInfo.min_filter);
                bw.Write((uint)tplInfo.mag_filter);
                bw.Write((float)tplInfo.lod_bias);
                bw.Write((byte)tplInfo.enable_lod);
                bw.Write((byte)tplInfo.min_lod);
                bw.Write((byte)tplInfo.max_lod);
                bw.Write((byte)tplInfo.is_compressed);

                tempOffset += 8;
            }

            for (int i = 0; i < length; i++)
            {
                TplInfo tplInfo = uhdTPL.TplArray[i];
                bw.Write((uint)tplInfo.PackID);
                bw.Write((uint)tplInfo.TextureID);
            }

            //padding
            int rest = (int)bw.BaseStream.Position % 16;  
            if (rest != 0)
            {
                int div = (int)bw.BaseStream.Position / 16;
                int total = (div + 1) * 16;
                int padding = total - (int)bw.BaseStream.Position;
                bw.Write(new byte[padding]);
            }

            endOffset = bw.BaseStream.Position;
        }
        
   }
}
