using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using RE4_UHD_BIN_TOOL.REPACK.Structures;
using RE4_UHD_BIN_TOOL.REPACK;

namespace RE4_UHD_SCENARIO_SMD_TOOL.SCENARIO
{
    public static partial class UhdScenarioRepack
    {
        public static void RepackOBJ(Stream objFile, ref IdxUhdScenario idxScenario, out Dictionary<int, SmdBaseLine> objGroupInfos, out Dictionary<int, FinalStructure> FinalBinList)
        {
            string patternUHDSCENARIO = "^(UHDSCENARIO#SMD_)([0]{0,})([0-9]{1,3})(#SMX_)([0]{0,})([0-9]{1,3})(#TYPE_)([0]{0,})([0-9|A-F]{1,8})(#BIN_)([0]{0,})([0-9]{1,3})(#).*$";
            System.Text.RegularExpressions.Regex regex = new System.Text.RegularExpressions.Regex(patternUHDSCENARIO, System.Text.RegularExpressions.RegexOptions.CultureInvariant);

            bool LoadColorsFromObjFile = true;

            // load .obj file
            var objLoaderFactory = new ObjLoader.Loader.Loaders.ObjLoaderFactory();
            var objLoader = objLoaderFactory.Create();
            var streamReader = new StreamReader(objFile, Encoding.ASCII);
            ObjLoader.Loader.Loaders.LoadResult arqObj = objLoader.Load(streamReader);
            streamReader.Close();

            //lista de materiais usados no modelo
            HashSet<string> ModelMaterials = new HashSet<string>();
            HashSet<string> ModelMaterialsToUpper = new HashSet<string>();


            Vector4 color = new Vector4(1, 1, 1, 1);
            StartWeightMap weightMap = new StartWeightMap(1, 0, 1, 0, 0, 0, 0);


            //conjunto de struturas
            //id do SMD/ conteudo para o SMD/BIN
            Dictionary<int, StartStructure> ObjList = new Dictionary<int, StartStructure>();
            //id do SMD, outras informações 
            objGroupInfos = new Dictionary<int, SmdBaseLine>();
            int maxSmd = 0;
            int maxBin = 0;

            for (int iG = 0; iG < arqObj.Groups.Count; iG++)
            {
                string GroupName = arqObj.Groups[iG].GroupName.ToUpperInvariant().Trim();

                if (GroupName.StartsWith("UHDSCENARIO"))
                {
                    string materialNameInvariant = arqObj.Groups[iG].MaterialName.ToUpperInvariant().Trim();
                    string materialName = arqObj.Groups[iG].MaterialName.Trim();

                    //FIX NAME
                    GroupName = GroupName.Replace("_", "#")
                        .Replace("SMD#", "SMD_")
                        .Replace("SMX#", "SMX_")
                        .Replace("TYPE#", "TYPE_")
                        .Replace("BIN#", "BIN_")
                        ;

                    //REGEX
                    if (regex.IsMatch(GroupName))
                    {
                        Console.WriteLine("Loading in Obj: " + GroupName + " | " + materialNameInvariant);
                    }
                    else
                    {
                        Console.WriteLine("Loading in Obj: " + GroupName + " | " + materialNameInvariant + "  The group name is wrong;");
                    }


                    SmdBaseLine info = getGroupInfo(GroupName);

                    if (!objGroupInfos.ContainsKey(info.SmdId))
                    {
                        objGroupInfos.Add(info.SmdId, info);
                    }

                    if (info.SmdId >= maxSmd)
                    {
                        maxSmd = info.SmdId +1;
                    }

                    if (info.BinId >= maxBin)
                    {
                        maxBin = info.BinId +1;
                    }

                    List<List<StartVertex>> facesList = new List<List<StartVertex>>();

                    for (int iF = 0; iF < arqObj.Groups[iG].Faces.Count; iF++)
                    {
                        List<StartVertex> verticeListInObjFace = new List<StartVertex>();

                        for (int iI = 0; iI < arqObj.Groups[iG].Faces[iF].Count; iI++)
                        {
                            StartVertex vertice = new StartVertex();

                            if (arqObj.Groups[iG].Faces[iF][iI].VertexIndex <= 0 || arqObj.Groups[iG].Faces[iF][iI].VertexIndex - 1 >= arqObj.Vertices.Count)
                            {
                                throw new ArgumentException("Vertex Position Index is invalid! Value: " + arqObj.Groups[iG].Faces[iF][iI].VertexIndex);
                            }

                            Vector3 position = new Vector3(
                                arqObj.Vertices[arqObj.Groups[iG].Faces[iF][iI].VertexIndex - 1].X,
                                arqObj.Vertices[arqObj.Groups[iG].Faces[iF][iI].VertexIndex - 1].Y,
                                arqObj.Vertices[arqObj.Groups[iG].Faces[iF][iI].VertexIndex - 1].Z
                                );

                            vertice.Position = position;


                            if (arqObj.Groups[iG].Faces[iF][iI].TextureIndex <= 0 || arqObj.Groups[iG].Faces[iF][iI].TextureIndex - 1 >= arqObj.Textures.Count)
                            {
                                vertice.Texture = new Vector2(0, 0);
                            }
                            else
                            {
                                Vector2 texture = new Vector2(
                                arqObj.Textures[arqObj.Groups[iG].Faces[iF][iI].TextureIndex - 1].U,
                                ((arqObj.Textures[arqObj.Groups[iG].Faces[iF][iI].TextureIndex - 1].V - 1) * -1)
                                );

                                vertice.Texture = texture;
                            }


                            if (arqObj.Groups[iG].Faces[iF][iI].NormalIndex <= 0 || arqObj.Groups[iG].Faces[iF][iI].NormalIndex - 1 >= arqObj.Normals.Count)
                            {
                                vertice.Normal = new Vector3(0, 0, 0);
                            }
                            else
                            {
                                Vector3 normal = new Vector3(
                                arqObj.Normals[arqObj.Groups[iG].Faces[iF][iI].NormalIndex - 1].X,
                                arqObj.Normals[arqObj.Groups[iG].Faces[iF][iI].NormalIndex - 1].Y,
                                arqObj.Normals[arqObj.Groups[iG].Faces[iF][iI].NormalIndex - 1].Z
                                );

                                vertice.Normal = normal;
                            }

                            vertice.Color = color;
                            vertice.WeightMap = weightMap;

                            if (LoadColorsFromObjFile)
                            {
                                Vector4 vColor = new Vector4(
                               arqObj.Vertices[arqObj.Groups[iG].Faces[iF][iI].VertexIndex - 1].R,
                               arqObj.Vertices[arqObj.Groups[iG].Faces[iF][iI].VertexIndex - 1].G,
                               arqObj.Vertices[arqObj.Groups[iG].Faces[iF][iI].VertexIndex - 1].B,
                               arqObj.Vertices[arqObj.Groups[iG].Faces[iF][iI].VertexIndex - 1].A
                               );
                                vertice.Color = vColor;
                            }

                            verticeListInObjFace.Add(vertice);

                        }

                        if (verticeListInObjFace.Count >= 3)
                        {
                            for (int i = 2; i < verticeListInObjFace.Count; i++)
                            {
                                List<StartVertex> face = new List<StartVertex>();
                                face.Add(verticeListInObjFace[0]);
                                face.Add(verticeListInObjFace[i - 1]);
                                face.Add(verticeListInObjFace[i]);
                                facesList.Add(face);
                            }
                        }

                    }


                    if (ObjList.ContainsKey(info.SmdId))
                    {
                        if (ObjList[info.SmdId].FacesByMaterial.ContainsKey(materialNameInvariant))
                        {
                            ObjList[info.SmdId].FacesByMaterial[materialNameInvariant].Faces.AddRange(facesList);
                        }
                        else
                        {
                            ModelMaterials.Add(materialName);
                            ModelMaterialsToUpper.Add(materialNameInvariant);
                            ObjList[info.SmdId].FacesByMaterial.Add(materialNameInvariant, new StartFacesGroup(facesList));
                        }
                    }
                    else
                    {
                        StartStructure startStructure = new StartStructure();
                        ModelMaterials.Add(materialName);
                        ModelMaterialsToUpper.Add(materialNameInvariant);
                        startStructure.FacesByMaterial.Add(materialNameInvariant, new StartFacesGroup(facesList));
                        ObjList.Add(info.SmdId, startStructure);
                    }

                }
                else
                {
                    Console.WriteLine("Loading in Obj: " + GroupName + "   Warning: Group not used;");
                }

            }


            if (idxScenario.SmdAmount < maxSmd)
            {
                idxScenario.SmdAmount = maxSmd;
            }

            if (idxScenario.BinAmount < maxBin)
            {
                idxScenario.BinAmount = maxBin;
            }


            for (int i = 0; i < idxScenario.SmdAmount; i++)
            {
                if (!objGroupInfos.ContainsKey(i))
                {
                    SmdBaseLine smdBaseLine = new SmdBaseLine();
                    smdBaseLine.SmdId = i;
                    smdBaseLine.SmxId = 0xFE;
                    smdBaseLine.Type = 0;
                    smdBaseLine.BinId = 0;
                    objGroupInfos.Add(i, smdBaseLine);
                }
            }

            //----
            FinalBinList = new Dictionary<int, FinalStructure>();

            foreach (var item in ObjList)
            {
                int BinID = objGroupInfos[item.Key].BinId;

                if (!FinalBinList.ContainsKey(BinID))
                {
                    // faz a compressão das vertives
                    Console.WriteLine("BIN ID: " + BinID.ToString("D3"));
                    item.Value.CompressAllFaces();
                    //-----

                    SMDLineIdx smdLineIdx = new SMDLineIdx();
                    smdLineIdx.scaleX = 1f;
                    smdLineIdx.scaleY = 1f;
                    smdLineIdx.scaleZ = 1f;

                    if (idxScenario.SmdLines.Length > item.Key)
                    {
                        smdLineIdx = idxScenario.SmdLines[item.Key];
                    }

                    var intermediary = BINrepackIntermediary.MakeIntermediaryStructure(item.Value, smdLineIdx, true);
                    var level2 = BinRepack.MakeIntermediaryLevel2(intermediary);
                    var final = BinRepack.MakeFinalStructure(level2);
                    FinalBinList.Add(BinID, final);
                }
            }

        }


        private static SmdBaseLine getGroupInfo(string GroupName)
        {
            SmdBaseLine line = new SmdBaseLine();

            var split = GroupName.Split('#');

            try
            {
                var subSplit = split[1].Split('_');
                int id = int.Parse(subSplit[1].Trim(), System.Globalization.NumberStyles.Integer, System.Globalization.CultureInfo.InvariantCulture);
                line.SmdId = id;
            }
            catch (Exception)
            {
            }

            try
            {
                var subSplit = split[2].Split('_');
                int id = int.Parse(subSplit[1].Trim(), System.Globalization.NumberStyles.Integer, System.Globalization.CultureInfo.InvariantCulture);
                line.SmxId = id;
            }
            catch (Exception)
            {
            }

            try
            {
                var subSplit = split[3].Split('_');
                uint type = uint.Parse(subSplit[1].Trim(), System.Globalization.NumberStyles.HexNumber, System.Globalization.CultureInfo.InvariantCulture);
                line.Type = type;
            }
            catch (Exception)
            {
            }

            try
            {
                var subSplit = split[4].Split('_');
                int id = int.Parse(subSplit[1].Trim(), System.Globalization.NumberStyles.Integer, System.Globalization.CultureInfo.InvariantCulture);
                line.BinId = id;
            }
            catch (Exception)
            {
            }

            return line;
        }


    }
}
