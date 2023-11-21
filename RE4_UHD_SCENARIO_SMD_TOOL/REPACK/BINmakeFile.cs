using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using RE4_UHD_BIN_TOOL.REPACK.Structures;
using RE4_UHD_BIN_TOOL.ALL;
using RE4_UHD_BIN_TOOL.EXTRACT;

namespace RE4_UHD_BIN_TOOL.REPACK
{
    public static class BINmakeFile
    {
        public static void MakeFile(Stream stream, long startOffset, out long endOffset, FinalStructure finalStructure, FinalBoneLine[] boneLines, IdxMaterial material,
            byte[][] BonePairLines, bool UseExtendedNormals, bool UseWeightMap, bool EnableBonepairTag, bool EnableAdjacentBoneTag, bool UseColors)
        {
            //header 0x60 bytes

            //bone lines
            //weightMap
            //bonepair
            //adjacent bone
            //vertex position
            //vertex weight
            //vertex normal
            //vertex weight2
            //vertex color
            //vertex uv
            //material

            var bin = new BinaryWriter(stream);
            bin.BaseStream.Position = startOffset;

            UhdBinHeader header = GetHeader(finalStructure, boneLines, BonePairLines.Length, UseExtendedNormals, UseWeightMap, EnableBonepairTag, EnableAdjacentBoneTag, UseColors);
            byte[] byteHeader = MakeHeader(header);
            bin.Write(byteHeader, 0, byteHeader.Length);

            bin.BaseStream.Position = header.bone_offset + startOffset;
            byte[] bones = MakeBone(boneLines);
            bin.Write(bones, 0, bones.Length);

            if (header.weight_count != 0 && UseWeightMap)
            {
                bin.BaseStream.Position = header.weight_offset + startOffset;
                byte[] weightMap = MakeWeightMap(finalStructure.WeightMaps);
                bin.Write(weightMap, 0, weightMap.Length);
            }

            if (BonePairLines.Length != 0 && EnableBonepairTag)
            {
                bin.BaseStream.Position = header.bonepair_offset + startOffset;
                byte[] BonePair = MakeBonepair(BonePairLines);
                bin.Write(BonePair, 0, BonePair.Length);
            }


            //vertex position

            bin.BaseStream.Position = header.vertex_position_offset + startOffset;
            byte[] pos = MakeVertexPositionNormal(finalStructure.Vertex_Position_Array);
            bin.Write(pos, 0, pos.Length);

            if (header.vertex_weight_index_offset != 0 && UseWeightMap)
            {
                bin.BaseStream.Position = header.vertex_weight_index_offset + startOffset;
                byte[] weight_index = MakeVertexWeightIndex(finalStructure.WeightIndex);
                bin.Write(weight_index, 0, weight_index.Length);
            }

            //vertex normal
            bin.BaseStream.Position = header.vertex_normal_offset + startOffset;
            byte[] normal = MakeVertexPositionNormal(finalStructure.Vertex_Normal_Array);
            bin.Write(normal, 0, normal.Length);

            if (UseColors)
            {
                bin.BaseStream.Position = header.vertex_colour_offset + startOffset;
                byte[] colors = MakeVertexColors(finalStructure.Vertex_Color_Array);
                bin.Write(colors, 0, colors.Length);
            }

            //TexcoordUV
            bin.BaseStream.Position = header.vertex_texcoord_offset  + startOffset;
            byte[] texcoord = MakeVertexTexcoordUV(finalStructure.Vertex_UV_Array);
            bin.Write(texcoord, 0, texcoord.Length);

            //material
            bin.BaseStream.Position = header.material_offset + startOffset;
            byte[] materialGroup = MakeMaterial(finalStructure.Groups, material);
            bin.Write(materialGroup, 0, materialGroup.Length);


            endOffset = bin.BaseStream.Position;
        }


        private static byte[] MakeMaterial(FinalMaterialGroup[] Groups, IdxMaterial material) 
        {
            List<byte> b = new List<byte>();

            for (int i = 0; i < Groups.Length; i++)
            {
                if (material.MaterialDic.ContainsKey(Groups[i].materialName))
                {
                    b.AddRange(material.MaterialDic[Groups[i].materialName].GetArray());
                }
                else 
                {
                    //usado quando não encontrado um material valido
                    b.AddRange(EmpatyMaterialArray());
                }

                uint buffer = (uint)(4 + (Groups[i].Mesh.Length * 4));
                uint calc = buffer / 16;
                buffer = (calc+1) * 16;

                uint count = 0;
                for (int im = 0; im < Groups[i].Mesh.Length; im++)
                {
                    count += Groups[i].Mesh[im].Count;
                }

                b.AddRange(BitConverter.GetBytes(buffer));
                b.AddRange(BitConverter.GetBytes(count));

                byte[] bb = new byte[buffer];
                BitConverter.GetBytes((uint)Groups[i].Mesh.Length).CopyTo(bb, 0);
                int tempOffset = 4;
                for (int im = 0; im < Groups[i].Mesh.Length; im++)
                {
                    BitConverter.GetBytes(Groups[i].Mesh[im].Type).CopyTo(bb, tempOffset);
                    BitConverter.GetBytes(Groups[i].Mesh[im].Count).CopyTo(bb, tempOffset +2);
                    tempOffset += 4;
                }
                b.AddRange(bb);

            }

            return b.ToArray();
        }

        private static byte[] EmpatyMaterialArray() 
        {
            byte[] b = new byte[24];
            b[13] = 0xFF;
            b[14] = 0xFF;
            b[15] = 0xFF;
            b[23] = 0xFF;
            return b;
        }


        private static byte[] MakeVertexColors((byte a, byte r, byte g, byte b)[] Vertex_Color_Array) 
        {
            
            byte[] b = new byte[CalculateBytesVertexColor((ushort)Vertex_Color_Array.Length)];

            int tempOffset = 0;
            for (int i = 0; i < Vertex_Color_Array.Length; i++)
            {
                b[tempOffset] = Vertex_Color_Array[i].a;
                b[tempOffset+1] = Vertex_Color_Array[i].r;
                b[tempOffset+2] = Vertex_Color_Array[i].g;
                b[tempOffset+3] = Vertex_Color_Array[i].b;

                tempOffset += 4;
            }

            return b;
        }


        private static byte[] MakeVertexTexcoordUV((float tu, float tv)[] Vertex_UV_Array) 
        {
            byte[] b = new byte[CalculateBytesVertexTexcoordUV((ushort)Vertex_UV_Array.Length)];

            int tempOffset = 0;
            for (int i = 0; i < Vertex_UV_Array.Length; i++)
            {
                BitConverter.GetBytes(Vertex_UV_Array[i].tu).CopyTo(b, tempOffset);
                BitConverter.GetBytes(Vertex_UV_Array[i].tv).CopyTo(b, tempOffset + 4);
                tempOffset += 8;
            }

            return b;
        }


        private static byte[] MakeVertexWeightIndex(ushort[] WeightIndex)
        {
            byte[] b = new byte[CalculateBytesVertexWeightIndex((ushort)WeightIndex.Length)];

            int tempOffset = 0;
            for (int i = 0; i < WeightIndex.Length; i++)
            {
                BitConverter.GetBytes(WeightIndex[i]).CopyTo(b, tempOffset);
                tempOffset += 2;
            }
            return b;
        }

        private static byte[] MakeVertexPositionNormal((float x, float y, float z)[] Vertex_Array) 
        {
            byte[] b = new byte[CalculateBytesVertexPositonNormal((ushort)Vertex_Array.Length)];

            int tempOffset = 0;
            for (int i = 0; i < Vertex_Array.Length; i++)
            {
                BitConverter.GetBytes(Vertex_Array[i].x).CopyTo(b, tempOffset);
                BitConverter.GetBytes(Vertex_Array[i].y).CopyTo(b, tempOffset + 4);
                BitConverter.GetBytes(Vertex_Array[i].z).CopyTo(b, tempOffset + 8);
                tempOffset += 12;
            }
            return b;
        }

        private static byte[] MakeWeightMap(FinalWeightMap[] WeightMaps) 
        {
            byte[] b = new byte[CalculateBytesVertexWeightMap((ushort)WeightMaps.Length)];

            int tempOffset = 0;
            for (int i = 0; i < WeightMaps.Length; i++)
            {
                if (WeightMaps.Length > 255)
                {
                    BitConverter.GetBytes(WeightMaps[i].BoneID1).CopyTo(b, tempOffset);
                    BitConverter.GetBytes(WeightMaps[i].BoneID2).CopyTo(b, tempOffset + 0x2);
                    BitConverter.GetBytes(WeightMaps[i].BoneID3).CopyTo(b, tempOffset + 0x4);
                    BitConverter.GetBytes(WeightMaps[i].Links).CopyTo(b, tempOffset + 0x6);
                    b[tempOffset + 0x8] = WeightMaps[i].Weight1;
                    b[tempOffset + 0x9] = WeightMaps[i].Weight2;
                    b[tempOffset + 0xA] = WeightMaps[i].Weight3;
                    b[tempOffset + 0xB] = 0;

                    tempOffset += 12;
                }
                else 
                {
                    BitConverter.GetBytes(WeightMaps[i].BoneID1).CopyTo(b, tempOffset);
                    BitConverter.GetBytes(WeightMaps[i].BoneID2).CopyTo(b, tempOffset + 0x1);
                    BitConverter.GetBytes(WeightMaps[i].BoneID3).CopyTo(b, tempOffset + 0x2);
                    BitConverter.GetBytes(WeightMaps[i].Links).CopyTo(b, tempOffset + 0x3);
                    b[tempOffset + 0x4] = WeightMaps[i].Weight1;
                    b[tempOffset + 0x5] = WeightMaps[i].Weight2;
                    b[tempOffset + 0x6] = WeightMaps[i].Weight3;
                    b[tempOffset + 0x7] = 0;
                    tempOffset += 8;
                }
            }
            return b;
        }

        private static byte[] MakeBone(FinalBoneLine[] boneLines) 
        {
            byte[] b = new byte[boneLines.Length * 16];

            int offset = 0;
            for (int i = 0; i < boneLines.Length; i++)
            {
                boneLines[i].Line.CopyTo(b, offset);
                offset += 16;
            }

            return b;
        }

        private static byte[] MakeBonepair(byte[][] bonepairLines) 
        {
            byte[] b = new byte[CalculateBytesBonePairAmount((ushort)bonepairLines.Length)];
            BitConverter.GetBytes(bonepairLines.Length).CopyTo(b, 0);

            int offset = 4;
            for (int i = 0; i < bonepairLines.Length; i++)
            {
                bonepairLines[i].CopyTo(b, offset);
                offset += 8;
            }

            return b;
        }



        private static UhdBinHeader GetHeader(FinalStructure finalStructure, FinalBoneLine[] boneLines,
            int BonePairAmount, bool UseExtendedNormals, bool UseWeightMap, bool EnableBonepairTag, bool EnableAdjacentBoneTag, bool UseColors) 
        {
            UhdBinHeader header = new UhdBinHeader();

            //calcula offsets;
            uint BoneOffset = 0x60;
            uint WeightMapOffset = 0;
            uint BonePairOffset = 0; //
            uint AdjacentBoneOffset = 0; //
            uint VertexPositionOffset = 0;
            uint VertexWeightIndexOffset = 0;
            uint VertexNormalOffset = 0;
            uint VertexWeight2IndexOffset = 0;
            uint VertexColorsOffset = 0;
            uint VertexTexcoordOffset = 0;
            uint MaterialOffset = 0;

            uint tempOffset = (uint)(BoneOffset + (boneLines.Length * 16) + 16);

            if (finalStructure.WeightMaps != null && finalStructure.WeightMaps.Length != 0 && UseWeightMap)
            {
                WeightMapOffset = tempOffset;
                tempOffset += (uint)CalculateBytesVertexWeightMap((ushort)finalStructure.WeightMaps.Length);
            }


            //BonePair
            if (BonePairAmount != 0)
            {
                BonePairOffset = tempOffset;
                tempOffset += (uint)CalculateBytesBonePairAmount((ushort)BonePairAmount);
            }

            //vertexPosition
            VertexPositionOffset = tempOffset;
            tempOffset += (uint)CalculateBytesVertexPositonNormal((ushort)finalStructure.Vertex_Position_Array.Length);


            if (finalStructure.WeightIndex != null && finalStructure.WeightIndex.Length != 0 && UseWeightMap)
            {
                VertexWeightIndexOffset = tempOffset;
                VertexWeight2IndexOffset = tempOffset;

                tempOffset += (uint)CalculateBytesVertexWeightIndex((ushort)finalStructure.WeightIndex.Length);
            }

            //vertexNormal
            VertexNormalOffset = tempOffset;
            tempOffset += (uint)CalculateBytesVertexPositonNormal((ushort)finalStructure.Vertex_Normal_Array.Length);

            if (UseColors)
            {
                VertexColorsOffset = tempOffset;
                tempOffset += (uint)CalculateBytesVertexColor((ushort)finalStructure.Vertex_Color_Array.Length);
            }

            //VertexTexcoord
            VertexTexcoordOffset = tempOffset;
            tempOffset += (uint)CalculateBytesVertexTexcoordUV((ushort)finalStructure.Vertex_UV_Array.Length);


            //material
            MaterialOffset = tempOffset;


            //preenche o header
            header.bone_offset = BoneOffset;
            header.unknown_x04 = 0;
            header.unknown_x08 = 0; // offset // 50 00 00 00

            
            header.vertex_colour_offset = VertexColorsOffset;

            header.vertex_texcoord_offset = VertexTexcoordOffset;
            header.weight_offset = WeightMapOffset;

            if (UseWeightMap)
            {
                header.weight_count = (byte)finalStructure.WeightMaps.Length;
                header.weight2_count = (ushort)finalStructure.WeightMaps.Length; //--same as weightcount
            }
            else 
            {
                header.weight_count = 0;
                header.weight2_count = 0;
            }


            header.bone_count =(byte)boneLines.Length;
            header.material_count = (ushort)finalStructure.Groups.Length;
            header.material_offset = MaterialOffset;


            header.texture1_flags = 0x0000;

            if (EnableAdjacentBoneTag)
            {
                header.unknown_x08 = 0x50;
                header.texture1_flags = 0x0200;
            }

            if (EnableBonepairTag)
            {
                header.unknown_x08 = 0x50;
                header.texture1_flags = 0x0300;
            }

            if (header.texture1_flags != 0x0000)
            {
                header.version_flags = 0x20030818;
            }
            else 
            {
                header.version_flags = 0x20010801;
            }


            header.texture2_flags = 0x8000;

            if (UseExtendedNormals)
            {
                header.texture2_flags |= 0x2000;
            }

            if (UseColors)
            {
                header.texture2_flags |= 0x4000;
            }


            header.TPL_count = 0; // afazer
            header.vertex_scale = 0; // afazer
            header.unknown_x29 = 0;
          
            header.morph_offset = 0; // não suportado nessa versão do pragrama


            header.vertex_position_offset = VertexPositionOffset;
            header.vertex_normal_offset = VertexNormalOffset;
            header.vertex_position_count = (ushort)finalStructure.Vertex_Position_Array.Length;
            header.vertex_normal_count = (ushort)finalStructure.Vertex_Normal_Array.Length;
           


            header.bonepair_offset = BonePairOffset;
            header.adjacent_offset = AdjacentBoneOffset;
            header.vertex_weight_index_offset = VertexWeightIndexOffset;  //--vertex weights id's array (2 words )* numvertex
            header.vertex_weight2_index_offset = VertexWeight2IndexOffset; //--vertex weights array (2 words )* numvertex

            return header;
        }


        private static byte[] MakeHeader(UhdBinHeader header) 
        {
            byte[] b = new byte[0x60];

            BitConverter.GetBytes(header.bone_offset).CopyTo(b, 0x00);
            BitConverter.GetBytes(header.unknown_x04).CopyTo(b, 0x04);
            BitConverter.GetBytes(header.unknown_x08).CopyTo(b, 0x08);
            BitConverter.GetBytes(header.vertex_colour_offset).CopyTo(b, 0x0C);


            BitConverter.GetBytes(header.vertex_texcoord_offset).CopyTo(b, 0x10);
            BitConverter.GetBytes(header.weight_offset).CopyTo(b, 0x14);
            BitConverter.GetBytes(header.weight_count).CopyTo(b, 0x18);
            BitConverter.GetBytes(header.bone_count).CopyTo(b, 0x19);
            BitConverter.GetBytes(header.material_count).CopyTo(b, 0x1A);
            BitConverter.GetBytes(header.material_offset).CopyTo(b, 0x1C);


            BitConverter.GetBytes(header.texture1_flags).CopyTo(b, 0x20);
            BitConverter.GetBytes(header.texture2_flags).CopyTo(b, 0x22);
            BitConverter.GetBytes(header.TPL_count).CopyTo(b, 0x24);
            BitConverter.GetBytes(header.vertex_scale).CopyTo(b, 0x28);
            BitConverter.GetBytes(header.unknown_x29).CopyTo(b, 0x29);
            BitConverter.GetBytes(header.weight2_count).CopyTo(b, 0x2A);
            BitConverter.GetBytes(header.morph_offset).CopyTo(b, 0x2C);


            BitConverter.GetBytes(header.vertex_position_offset).CopyTo(b, 0x30);
            BitConverter.GetBytes(header.vertex_normal_offset).CopyTo(b, 0x34);
            BitConverter.GetBytes(header.vertex_position_count).CopyTo(b, 0x38);
            BitConverter.GetBytes(header.vertex_normal_count).CopyTo(b, 0x3A);
            BitConverter.GetBytes(header.version_flags).CopyTo(b, 0x3C);


            BitConverter.GetBytes(header.bonepair_offset).CopyTo(b, 0x40);
            BitConverter.GetBytes(header.adjacent_offset).CopyTo(b, 0x44);
            BitConverter.GetBytes(header.vertex_weight_index_offset).CopyTo(b, 0x48);
            BitConverter.GetBytes(header.vertex_weight2_index_offset).CopyTo(b, 0x4C);
 
            return b;
        }


        // calcula quantidade de bytes usado por BonePair
        //BonePairAmount
        private static int CalculateBytesBonePairAmount(ushort count)
        {
            int calc = 4 + (count * 8);

            int div = calc / 16;
            int rest = calc % 16;
            if (rest != 0)
            {
                div++;
            }
            int response = div * 16;
            return response;
        }


        // calcula a quantidade de bytes usado por weight map
        private static int CalculateBytesVertexWeightMap(ushort count)
        {
            int response = 0;
            if (count > 255)
            {
                int calc = count * 12;

                int div = calc / 16;
                int rest = calc % 16;
                if (rest != 0)
                {
                    div++;
                }
                response = div * 16;
            }
            else if (count != 0)
            {
                int calc = count * 8;

                int div = calc / 16;
                int rest = calc % 16;
                if (rest != 0)
                {
                    div++;
                }
                response = div * 16;
            }

            return response;
        }

        // calcula a quantidade de bytes usado por weight index
        private static int CalculateBytesVertexWeightIndex(ushort count)
        {
            int calc = count * 2;

            int div = calc / 16;
            int rest = calc % 16;
            if (rest != 0)
            {
                div++;
            }
            int response = div * 16;
            return response;
        }

        // calcula a quantidade de bytes usado por vertex position/normal
        private static int CalculateBytesVertexPositonNormal(ushort count) 
        {
            int calc = count * 3 * 4;

            int div = calc / 16;
            int rest = calc % 16;
            if (rest != 0)
            {
                div++;
            }
            int response = div * 16;
            return response;
        }

        //calcula a quantidade de bytes usado por texcoord UV
        private static int CalculateBytesVertexTexcoordUV(ushort count) 
        {
            int calc = count * 2 * 4;

            int div = calc / 16;
            int rest = calc % 16;
            if (rest != 0)
            {
                div++;
            }
            int response = div * 16;
            return response;
        }

        //calcula a quantidade de bytes usado por color
        private static int CalculateBytesVertexColor(ushort count)
        {
            int calc = count * 4; //color 1 uint

            int div = calc / 16;
            int rest = calc % 16;
            if (rest != 0)
            {
                div++;
            }
            int response = div * 16;
            return response;
        }
    }
}
