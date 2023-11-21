using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RE4_UHD_BIN_TOOL.REPACK.Structures
{
    public class FinalStructure
    {
        public (float vx, float vy, float vz)[] Vertex_Position_Array;
        public (float nx, float ny, float nz)[] Vertex_Normal_Array;
        public (float tu, float tv)[] Vertex_UV_Array;
        public (byte a, byte r, byte g, byte b)[] Vertex_Color_Array;

        public FinalWeightMap[] WeightMaps;
        public ushort[] WeightIndex;

        public FinalMaterialGroup[] Groups;
    }

    public class FinalMaterialGroup
    {
        // nome do material usado
        public string materialName;

        public FinalFace[] Mesh;
    }

    public class FinalFace
    {
        public ushort Type;
        public ushort Count;
    }


    public class FinalWeightMap : IEquatable<FinalWeightMap>
    {
        public ushort Links { get; set; }

        public ushort BoneID1 { get; set; }
        public byte Weight1 { get; set; }

        public ushort BoneID2 { get; set; }
        public byte Weight2 { get; set; }

        public ushort BoneID3 { get; set; }
        public byte Weight3 { get; set; }

        public override bool Equals(object obj)
        {
            return obj is FinalWeightMap map
                && map.Links == Links
                && map.BoneID1 == BoneID1
                && map.BoneID2 == BoneID2
                && map.BoneID3 == BoneID3
                && map.Weight1 == Weight1
                && map.Weight2 == Weight2
                && map.Weight3 == Weight3;
        }

        public bool Equals(FinalWeightMap other)
        {
            return other.Links == Links
                && other.BoneID1 == BoneID1
                && other.BoneID2 == BoneID2
                && other.BoneID3 == BoneID3
                && other.Weight1 == Weight1
                && other.Weight2 == Weight2
                && other.Weight3 == Weight3;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 23 + Links.GetHashCode();
                hash = hash * 23 + BoneID1.GetHashCode();
                hash = hash * 23 + Weight1.GetHashCode();
                hash = hash * 23 + BoneID2.GetHashCode();
                hash = hash * 23 + Weight2.GetHashCode();
                hash = hash * 23 + BoneID3.GetHashCode();
                hash = hash * 23 + Weight3.GetHashCode();
                return hash;
            }
        }

    }

}
