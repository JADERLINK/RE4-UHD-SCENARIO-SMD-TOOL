using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RE4_UHD_BIN_TOOL.REPACK.Structures;
using RE4_UHD_BIN_TOOL.ALL;

namespace RE4_UHD_SCENARIO_SMD_TOOL.SCENARIO
{
    public static class BINrepackIntermediary
    {
        public static IntermediaryStructure MakeIntermediaryStructure(StartStructure startStructure, SMDLineIdx smdLine, bool UseExtendedNormals)
        {
            float NORMAL_FIX = UseExtendedNormals ? CONSTs.GLOBAL_NORMAL_FIX_EXTENDED : CONSTs.GLOBAL_NORMAL_FIX_REDUCED;

            IntermediaryStructure intermediary = new IntermediaryStructure();

            foreach (var item in startStructure.FacesByMaterial)
            {
                IntermediaryMesh mesh = new IntermediaryMesh();

                for (int i = 0; i < item.Value.Faces.Count; i++)
                {
                    IntermediaryFace face = new IntermediaryFace();

                    for (int iv = 0; iv < item.Value.Faces[i].Count; iv++)
                    {
                        IntermediaryVertex vertex = new IntermediaryVertex();

                        float[] pos1 = new float[3];// 0 = x, 1 = y, 2 = z
                        pos1[0] = item.Value.Faces[i][iv].Position.X * CONSTs.GLOBAL_POSITION_SCALE;
                        pos1[1] = item.Value.Faces[i][iv].Position.Y * CONSTs.GLOBAL_POSITION_SCALE;
                        pos1[2] = item.Value.Faces[i][iv].Position.Z * CONSTs.GLOBAL_POSITION_SCALE;

                        pos1[0] = ((pos1[0]) - (smdLine.positionX * CONSTs.GLOBAL_POSITION_SCALE)) / smdLine.scaleX;
                        pos1[1] = ((pos1[1]) - (smdLine.positionY * CONSTs.GLOBAL_POSITION_SCALE)) / smdLine.scaleY;
                        pos1[2] = ((pos1[2]) - (smdLine.positionZ * CONSTs.GLOBAL_POSITION_SCALE)) / smdLine.scaleZ;

                        pos1 = RotationUtils.RotationInZ(pos1, -smdLine.angleZ);
                        pos1 = RotationUtils.RotationInY(pos1, -smdLine.angleY);
                        pos1 = RotationUtils.RotationInX(pos1, -smdLine.angleX);

                        vertex.PosX = pos1[0];
                        vertex.PosY = pos1[1];
                        vertex.PosZ = pos1[2];

                        float[] normal1 = new float[3];// 0 = x, 1 = y, 2 = z
                        normal1[0] = item.Value.Faces[i][iv].Normal.X;
                        normal1[1] = item.Value.Faces[i][iv].Normal.Y;
                        normal1[2] = item.Value.Faces[i][iv].Normal.Z;

                        normal1 = RotationUtils.RotationInZ(normal1, -smdLine.angleZ);
                        normal1 = RotationUtils.RotationInY(normal1, -smdLine.angleY);
                        normal1 = RotationUtils.RotationInX(normal1, -smdLine.angleX);

                        vertex.NormalX = normal1[0] * NORMAL_FIX;
                        vertex.NormalY = normal1[1] * NORMAL_FIX;
                        vertex.NormalZ = normal1[2] * NORMAL_FIX;

                        vertex.TextureU = item.Value.Faces[i][iv].Texture.U;
                        vertex.TextureV = item.Value.Faces[i][iv].Texture.V;

                        vertex.ColorR = (byte)(item.Value.Faces[i][iv].Color.R * 255);
                        vertex.ColorG = (byte)(item.Value.Faces[i][iv].Color.G * 255);
                        vertex.ColorB = (byte)(item.Value.Faces[i][iv].Color.B * 255);
                        vertex.ColorA = (byte)(item.Value.Faces[i][iv].Color.A * 255);

                        vertex.Links = (byte)item.Value.Faces[i][iv].WeightMap.Links;


                        vertex.BoneID1 = (ushort)item.Value.Faces[i][iv].WeightMap.BoneID1;
                        vertex.BoneID2 = (ushort)item.Value.Faces[i][iv].WeightMap.BoneID2;
                        vertex.BoneID3 = (ushort)item.Value.Faces[i][iv].WeightMap.BoneID3;

                        vertex.Weight1 = (byte)(item.Value.Faces[i][iv].WeightMap.Weight1 * 100);
                        vertex.Weight2 = (byte)(item.Value.Faces[i][iv].WeightMap.Weight2 * 100);
                        vertex.Weight3 = (byte)(item.Value.Faces[i][iv].WeightMap.Weight3 * 100);

                        face.Vertexs.Add(vertex);
                    }

                    mesh.Faces.Add(face);
                }

                mesh.MaterialName = item.Key.ToUpperInvariant();
                intermediary.Groups.Add(mesh.MaterialName, mesh);
            }

            return intermediary;
        }

    }
}
