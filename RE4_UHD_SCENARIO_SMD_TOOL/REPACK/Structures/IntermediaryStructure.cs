using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RE4_UHD_BIN_TOOL.REPACK.Structures
{
    public class IntermediaryStructure
    {
        public Dictionary<string, IntermediaryMesh> Groups { get; set; }

        public IntermediaryStructure()
        {
            Groups = new Dictionary<string, IntermediaryMesh>();
        }
    }

    public class IntermediaryMesh
    {
        public string MaterialName { get; set; }

        public List<IntermediaryFace> Faces { get; set; }

        public IntermediaryMesh()
        {
            Faces = new List<IntermediaryFace>();
        }
    }


    public class IntermediaryFace
    {
        public List<IntermediaryVertex> Vertexs { get; set; }

        public IntermediaryFace()
        {
            Vertexs = new List<IntermediaryVertex>();
        }
    }

    public class IntermediaryVertex
    {
        public float PosX { get; set; }
        public float PosY { get; set; }
        public float PosZ { get; set; }

        public float NormalX { get; set; }
        public float NormalY { get; set; }
        public float NormalZ { get; set; }

        public float TextureU { get; set; }
        public float TextureV { get; set; }

        public byte ColorR { get; set; }
        public byte ColorG { get; set; }
        public byte ColorB { get; set; }
        public byte ColorA { get; set; }
        public ushort Links { get; set; }
        public ushort BoneID1 { get; set; }
        public byte Weight1 { get; set; }

        public ushort BoneID2 { get; set; }
        public byte Weight2 { get; set; }

        public ushort BoneID3 { get; set; }
        public byte Weight3 { get; set; }

        public FinalWeightMap GetFinalWeightMap()
        {
            FinalWeightMap weightMap = new FinalWeightMap();
            weightMap.Links = Links;
            weightMap.BoneID1 = BoneID1;
            weightMap.BoneID2 = BoneID2;
            weightMap.BoneID3 = BoneID3;
            weightMap.Weight1 = Weight1;
            weightMap.Weight2 = Weight2;
            weightMap.Weight3 = Weight3;
            return weightMap;
        }
    }


}
