using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using RE4_UHD_BIN_TOOL.ALL;
using RE4_UHD_BIN_TOOL.EXTRACT;

namespace RE4_UHD_BIN_TOOL.REPACK
{
    public class MtlConverter
    {
        private string baseDirectory;

        private Dictionary<TexPathRef, (ushort width, ushort height)> TexturesDimension;

        public MtlConverter(string baseDirectory) 
        {
            this.baseDirectory = baseDirectory;
            TexturesDimension = new Dictionary<TexPathRef, (ushort width, ushort height)>();
        }

        public void Convert(IdxMtl idxmtl, ref UhdTPL uhdTpl, out IdxMaterial idxMaterial) 
        {
            idxMaterial = new IdxMaterial();
            idxMaterial.MaterialDic = new Dictionary<string, MaterialPart>();

            List<TplInfo> tplInfos = new List<TplInfo>();

            if (uhdTpl == null)
            {
                uhdTpl = new UhdTPL();
            }
            else 
            {
                tplInfos = uhdTpl.TplArray.ToList();
            }
            //-----

            foreach (var item in idxmtl.MtlDic)
            {
                MaterialPart mat = new MaterialPart();
                mat.custom_specular_map = 255;
                mat.generic_specular_map = 255;
                mat.opacity_map = 255;
                mat.bump_map = 255;

                mat.diffuse_map = TextureIndex(ref tplInfos, item.Value.map_Kd, TexType.diffuse);

                if (item.Value.map_Bump != null)
                {
                    mat.material_flag |= 0x01; // bump flag

                    mat.bump_map = TextureIndex(ref tplInfos, item.Value.map_Bump, TexType.bump);
                    mat.generic_specular_map = 0;
                    mat.intensity_specular_b = 255;
                    mat.intensity_specular_g = 255;
                    mat.intensity_specular_r = 255;
                    mat.specular_scale = 0x00;
                }

                if (item.Value.map_d != null)
                {
                    mat.material_flag |= 0x04; // opacity flag

                    mat.opacity_map = TextureIndex(ref tplInfos, item.Value.map_d, TexType.opacity);
                }

                if (item.Value.ref_specular_map != null)
                {
                    mat.material_flag |= 0x02; //generic specular flag

                    mat.intensity_specular_b = item.Value.Ks.GetR();
                    mat.intensity_specular_g = item.Value.Ks.GetG();
                    mat.intensity_specular_r = item.Value.Ks.GetB();
                    mat.specular_scale = item.Value.specular_scale;

                    if (item.Value.ref_specular_map.PackID == 0x07000000)
                    {
                        mat.generic_specular_map = (byte)item.Value.ref_specular_map.TextureID;
                    }
                    else
                    {
                        mat.material_flag |= 0x10; // custom specular flag

                        mat.generic_specular_map = 0;
                        mat.custom_specular_map = TextureIndex(ref tplInfos, item.Value.ref_specular_map, TexType.custom_specular);
                    }
                }

                idxMaterial.MaterialDic.Add(item.Key, mat);
            }

            //----
            uhdTpl.TplArray = tplInfos.ToArray();
        }

        private enum TexType 
        {
            diffuse, // 0x0E
            bump,    // 0x00
            opacity, // 0x03
            custom_specular //0x06
        }

        private byte TextureIndex(ref List<TplInfo> tplInfos, TexPathRef texPathRef, TexType type) 
        {
            TplInfo info = null;

            int iindex = tplInfos.FindIndex(t => t.PackID == texPathRef.PackID && t.TextureID == texPathRef.TextureID);
            if (iindex > -1)
            {
                info = tplInfos[iindex];
            }
            else 
            {
                info = new TplInfo();
                info.PackID = texPathRef.PackID;
                info.TextureID = texPathRef.TextureID;
                info.wrap_t = 1;
                info.wrap_s = 1;
                info.min_filter = 1;
                info.mag_filter = 1;
                info.width = 1;
                info.height = 1;
                tplInfos.Add(info);
            }

            switch (type)
            {
                case TexType.diffuse:
                    info.PixelFormatType = 0x0E;
                    break;
                case TexType.bump:
                    info.PixelFormatType = 0x03;
                    break;
                case TexType.opacity:
                    info.PixelFormatType = 0x00;
                    break;
                case TexType.custom_specular:
                    info.PixelFormatType = 0x06;
                    break;
                default:
                    break;
            }

            (ushort width, ushort height) dimension = (info.width, info.height);
            GetDimension(texPathRef, ref dimension);
            info.width = dimension.width;
            info.height = dimension.height;


            byte index = (byte)tplInfos.IndexOf(info);

            return index;
        }

        //TexturesDimension
        private void GetDimension(TexPathRef texPathRef, ref (ushort width, ushort height) dimension) 
        {
            if (TexturesDimension.ContainsKey(texPathRef))
            {
                dimension = TexturesDimension[texPathRef];
            }
            else 
            {
                GetImagemDimension(texPathRef, ref dimension);
                TexturesDimension.Add(texPathRef, dimension);
            }
        }

        private void GetImagemDimension(TexPathRef texPathRef, ref (ushort width, ushort height) dimension)
        {
            try
            {
                string file = Path.Combine(baseDirectory, texPathRef.GetPath());
                if (File.Exists(file))
                {
                    BinaryReader br = new BinaryReader(new FileInfo(file).OpenRead());

                    if (texPathRef.Format.ToUpper() == "DDS")
                    {
                        br.BaseStream.Position = 0xC;
                        dimension.height = (ushort)br.ReadUInt32();
                        dimension.width = (ushort)br.ReadUInt32();
                        Console.WriteLine("Image: " + texPathRef.ToString());
                        Console.WriteLine("Dimension: " + dimension.width + "x" + dimension.height);
                    }
                    else if (texPathRef.Format.ToUpper() == "TGA")
                    {
                        br.BaseStream.Position = 0xC;
                        dimension.width = br.ReadUInt16();
                        dimension.height = br.ReadUInt16();
                        Console.WriteLine("Image: " + texPathRef.ToString());
                        Console.WriteLine("Dimension: " + dimension.width + "x" + dimension.height);
                    }
                    else
                    {
                        Console.WriteLine("Image: " + texPathRef.ToString());
                        Console.WriteLine("Invalid image format.");
                    }

                    br.Close();
                }
                else 
                {
                    Console.WriteLine("Error when getting image dimension: " + texPathRef + Environment.NewLine + "The file does not exist.");
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine("Error when getting image dimension: " + texPathRef + Environment.NewLine + ex.Message);

            }
        
        }


    }
}
