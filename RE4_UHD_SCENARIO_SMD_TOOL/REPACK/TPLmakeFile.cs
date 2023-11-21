using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using RE4_UHD_BIN_TOOL.EXTRACT;

namespace RE4_UHD_BIN_TOOL.REPACK
{
   public static class TPLmakeFile
   {
        public static void MakeFile(UhdTPL uhdTPL, Stream stream, long startOffset, out long endOffset) 
        {
            BinaryWriter bw = new BinaryWriter(stream);
            bw.BaseStream.Position = startOffset;

            uint length = (uint)uhdTPL.TplArray.Length;

            bw.Write((uint)0x78563412); //magic
            bw.Write((uint)length); // quantidade de tpl
            bw.Write((uint)0xC); // primeiro offset

            uint tempoffset = 0xC + (8 * length);
            for (int i = 0; i < length; i++)
            {
                bw.Write((uint)tempoffset);
                bw.Write((uint)0x0);
                tempoffset += 36;
            }

            for (int i = 0; i < length; i++)
            {
                TplInfo tplInfo = uhdTPL.TplArray[i];

                bw.Write((ushort)tplInfo.width);
                bw.Write((ushort)tplInfo.height);
                bw.Write((uint)tplInfo.PixelFormatType);
                bw.Write((uint)tempoffset);
                bw.Write((uint)tplInfo.wrap_s);
                bw.Write((uint)tplInfo.wrap_t);
                bw.Write((uint)tplInfo.min_filter);
                bw.Write((uint)tplInfo.mag_filter);
                bw.Write((float)tplInfo.lod_bias);
                bw.Write((byte)tplInfo.enable_lod);
                bw.Write((byte)tplInfo.min_lod);
                bw.Write((byte)tplInfo.max_lod);
                bw.Write((byte)tplInfo.is_compressed);

                tempoffset += 8;
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
