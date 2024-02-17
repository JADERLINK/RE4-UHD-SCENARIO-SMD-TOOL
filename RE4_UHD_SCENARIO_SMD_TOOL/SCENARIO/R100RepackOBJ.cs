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
    public static partial class R100Repack
    {
        private static void RepackOBJ(Stream objFile, ref R100RepackIdx idx, 
            out Dictionary<int, Dictionary<int, SmdBaseLine>> objGroupInfosList, 
            out Dictionary<int, Dictionary<int, FinalStructure>> FinalBinListDic,
            out int[] maxBin)
        {
            string patternR100 = "^(FILE_)([0]{0,})([0-6]{1})(#SMD_)([0]{0,})([0-9]{1,3})(#SMX_)([0]{0,})([0-9]{1,3})(#TYPE_)([0]{0,})([0-9|A-F]{1,8})(#BIN_)([0]{0,})([0-9]{1,3})(#).*$";
            System.Text.RegularExpressions.Regex regex = new System.Text.RegularExpressions.Regex(patternR100, System.Text.RegularExpressions.RegexOptions.CultureInvariant);


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

            //id do file, (id do bin, conteudo do bin)
            FinalBinListDic = new Dictionary<int, Dictionary<int, FinalStructure>>();

            //conjunto de struturas

            //(id do file, "type")(id do SMD/ conteudo para o SMD/BIN)
            Dictionary<(int file, bool type), Dictionary<int, StartStructure>> ObjListDic = new Dictionary<(int file, bool type), Dictionary<int, StartStructure>>();
            
            //id do file, (id do SMD, outras informações)
            objGroupInfosList = new Dictionary<int, Dictionary<int, SmdBaseLine>>();

            for (int fil = 0; fil < FileAmount; fil++)
            {
                objGroupInfosList.Add(fil, new Dictionary<int, SmdBaseLine>());
            }

            int[] maxSmd = new int[FileAmount];
            maxBin = new int[FileAmount];


            //converte o obj
            for (int iG = 0; iG < arqObj.Groups.Count; iG++)
            {
                string GroupName = arqObj.Groups[iG].GroupName.ToUpperInvariant().Trim();

                if (GroupName.StartsWith("FILE"))
                {
                    string materialNameInvariant = arqObj.Groups[iG].MaterialName.ToUpperInvariant().Trim();
                    string materialName = arqObj.Groups[iG].MaterialName.Trim();

                    //FIX NAME
                    GroupName = GroupName.Replace("_", "#")
                        .Replace("FILE#", "FILE_")
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

                    int fileID = 0;

                    SmdBaseLine info = getGroupInfo(GroupName, out fileID);

                    if (!objGroupInfosList.ContainsKey(fileID))
                    {
                        objGroupInfosList.Add(fileID, new Dictionary<int, SmdBaseLine>());
                    }

                    if (!objGroupInfosList[fileID].ContainsKey(info.SmdId))
                    {
                        objGroupInfosList[fileID].Add(info.SmdId, info);
                    }

                    if (fileID < FileAmount)
                    {
                        if (info.SmdId >= maxSmd[fileID])
                        {
                            maxSmd[fileID] = info.SmdId + 1;
                        }
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


                    bool type = (info.Type & 0x10) == 0x10;
                    var key = (fileID, type);
                    if (!ObjListDic.ContainsKey(key))
                    {
                        ObjListDic.Add(key, new Dictionary<int, StartStructure>());
                    }


                    if (ObjListDic[key].ContainsKey(info.SmdId))
                    {
                        if (ObjListDic[key][info.SmdId].FacesByMaterial.ContainsKey(materialNameInvariant))
                        {
                            ObjListDic[key][info.SmdId].FacesByMaterial[materialNameInvariant].Faces.AddRange(facesList);
                        }
                        else
                        {
                            ModelMaterials.Add(materialName);
                            ModelMaterialsToUpper.Add(materialNameInvariant);
                            ObjListDic[key][info.SmdId].FacesByMaterial.Add(materialNameInvariant, new StartFacesGroup(facesList));
                        }
                    }
                    else
                    {
                        StartStructure startStructure = new StartStructure();
                        ModelMaterials.Add(materialName);
                        ModelMaterialsToUpper.Add(materialNameInvariant);
                        startStructure.FacesByMaterial.Add(materialNameInvariant, new StartFacesGroup(facesList));
                        ObjListDic[key].Add(info.SmdId, startStructure);
                    }

                }
                else
                {
                    Console.WriteLine("Loading in Obj: " + GroupName + "   Warning: Group not used;");
                }

            }


            // arruma quantidade de smd
            for (int fil = 0; fil < FileAmount; fil++)
            {
                if (idx.SmdAmount[fil] < maxSmd[fil])
                {
                    idx.SmdAmount[fil] = maxSmd[fil];
                }
            }

            // adiciona SmdBaseLine faltante
            for (int fil = 0; fil < FileAmount; fil++)
            {
                for (int i = 0; i < idx.SmdAmount[fil]; i++)
                {
                    if (!objGroupInfosList.ContainsKey(fil))
                    {
                        objGroupInfosList.Add(fil, new Dictionary<int, SmdBaseLine>());
                    }

                    if (!objGroupInfosList[fil].ContainsKey(i))
                    {
                        SmdBaseLine smdBaseLine = new SmdBaseLine();
                        smdBaseLine.SmdId = i;
                        smdBaseLine.SmxId = 0xFE;
                        smdBaseLine.Type = 0;
                        smdBaseLine.BinId = 0;
                        objGroupInfosList[fil].Add(i, smdBaseLine);
                    }
                }
            }

            //adiciona dos dictionary no FinalBinListDic
            for (int fil = 0; fil < FileAmount; fil++)
            {
                FinalBinListDic.Add(fil, new Dictionary<int, FinalStructure>());
            }

            //bin para cada file
            for (int fil = 0; fil < FileAmount; fil++)
            {
                var key = (fil, false);
                if (ObjListDic.ContainsKey(key))
                {
                    foreach (var item in ObjListDic[key])
                    {
                        int BinID = objGroupInfosList[fil][item.Key].BinId;
                        bool type = (objGroupInfosList[fil][item.Key].Type & 0x10) == 0x10;

                        if (type == false && !FinalBinListDic[fil].ContainsKey(BinID))
                        {
                            // faz a compressão das vertives
                            Console.WriteLine("FILE: " + fil + "  BIN ID: " + BinID.ToString("D3"));
                            item.Value.CompressAllFaces();
                            //-----

                            SMDLineIdx smdLineIdx = new SMDLineIdx();
                            smdLineIdx.scaleX = 1f;
                            smdLineIdx.scaleY = 1f;
                            smdLineIdx.scaleZ = 1f;

                            if (idx.SmdLines[fil].Length > item.Key)
                            {
                                smdLineIdx = idx.SmdLines[fil][item.Key];
                            }

                            var intermediary = BINrepackIntermediary.MakeIntermediaryStructure(item.Value, smdLineIdx, true);
                            var level2 = BinRepack.MakeIntermediaryLevel2(intermediary);
                            var final = BinRepack.MakeFinalStructure(level2);
                            FinalBinListDic[fil].Add(BinID, final);

                            if (maxBin[fil] <= BinID)
                            {
                                maxBin[fil] = BinID +1;
                            }

                            if (final.Vertex_Position_Array.Length > ushort.MaxValue)
                            {
                                Console.WriteLine("Warning: Number of vertices greater than the limit: " + final.Vertex_Position_Array.Length);
                                Console.WriteLine("The limit is: " + ushort.MaxValue +
                                    "; BIN ID: " + BinID.ToString("D3") +
                                    "; SMD ID: " + item.Key.ToString("D3") + ";");
                                Console.WriteLine("Use above the vertex limit is permitted, but use with caution;");
                            }
                        }
                    }
                }

            }
            
            //bin para o file 5 (faltantes)
            for (int fil = 0; fil < FileAmount; fil++)
            {
                var key = (fil, true);
                if (ObjListDic.ContainsKey(key))
                {
                    foreach (var item in ObjListDic[key])
                    {
                        int BinID = objGroupInfosList[fil][item.Key].BinId;
                        bool type = (objGroupInfosList[fil][item.Key].Type & 0x10) == 0x10;

                        if (type == true && !FinalBinListDic[r100_005].ContainsKey(BinID))
                        {
                            // faz a compressão das vertives
                            Console.WriteLine("FILE: " + r100_005 + "  BIN ID: " + BinID.ToString("D3"));
                            item.Value.CompressAllFaces();
                            //-----

                            SMDLineIdx smdLineIdx = new SMDLineIdx();
                            smdLineIdx.scaleX = 1f;
                            smdLineIdx.scaleY = 1f;
                            smdLineIdx.scaleZ = 1f;

                            if (idx.SmdLines[fil].Length > item.Key)
                            {
                                smdLineIdx = idx.SmdLines[fil][item.Key];
                            }

                            var intermediary = BINrepackIntermediary.MakeIntermediaryStructure(item.Value, smdLineIdx, true);
                            var level2 = BinRepack.MakeIntermediaryLevel2(intermediary);
                            var final = BinRepack.MakeFinalStructure(level2);
                            FinalBinListDic[r100_005].Add(BinID, final);

                            if (maxBin[r100_005] <= BinID)
                            {
                                maxBin[r100_005] = BinID + 1;
                            }

                            if (final.Vertex_Position_Array.Length > ushort.MaxValue)
                            {
                                Console.WriteLine("Warning: Number of vertices greater than the limit: " + final.Vertex_Position_Array.Length);
                                Console.WriteLine("The limit is: " + ushort.MaxValue +
                                    "; BIN ID: " + BinID.ToString("D3") +
                                    "; SMD ID: " + item.Key.ToString("D3") + ";");
                                Console.WriteLine("Use above the vertex limit is permitted, but use with caution;");
                            }
                        }


                    }
                }

            }

        }


        private static SmdBaseLine getGroupInfo(string GroupName, out int fileID)
        {
            SmdBaseLine line = new SmdBaseLine();

            var split = GroupName.Split('#').Where(v => v.Length != 0).ToArray();

            try
            {
                var subSplit = split[0].Split('_');
                int id = int.Parse(subSplit[1].Trim(), System.Globalization.NumberStyles.Integer, System.Globalization.CultureInfo.InvariantCulture);
                fileID = id;
            }
            catch (Exception)
            {
                fileID = 0;
            }

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
