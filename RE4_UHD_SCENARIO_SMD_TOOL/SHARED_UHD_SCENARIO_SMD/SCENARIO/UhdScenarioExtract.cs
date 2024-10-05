using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Globalization;
using System.IO;
using SHARED_UHD_BIN.ALL;
using SHARED_UHD_BIN.EXTRACT;

namespace SHARED_UHD_SCENARIO_SMD.SCENARIO
{
    public static class UhdScenarioExtract
    {
        public static IdxMaterial IdxMaterialMultParser(Dictionary<int, UhdBIN> uhdBinDic, out Dictionary<MaterialPart, string> invDic)
        {
            IdxMaterial idx = new IdxMaterial();
            idx.MaterialDic = new Dictionary<string, MaterialPart>();
            invDic = new Dictionary<MaterialPart, string>();

            int counter = 0;

            foreach (var item in uhdBinDic)
            {
                for (int i = 0; i < item.Value.Materials.Length; i++)
                {
                    var mat = item.Value.Materials[i].material;

                    if (!invDic.ContainsKey(mat))
                    {
                        string matKey = CONSTs.SCENARIO_MATERIAL + counter.ToString("D3");
                        invDic.Add(mat, matKey);
                        idx.MaterialDic.Add(matKey, mat);
                        counter++;
                    }

                }
            }

            return idx;
        }

        public static void CreateOBJ(SMDLine[] smdLines, Dictionary<int, UhdBIN> uhdBinDic, Dictionary<MaterialPart, string> materialList, string baseDirectory, string baseFileName, bool UseColorsInObjFile)
        {
            StreamWriter obj = new StreamWriter(baseDirectory + baseFileName + ".obj", false);
            obj.WriteLine(Shared.HeaderText());
            obj.WriteLine("");

            obj.WriteLine("mtllib " + baseFileName + ".mtl");

            uint indexCount = 0;

            for (int i = 0; i < smdLines.Length; i++)
            {
                int key = smdLines[i].BinID;
                if (uhdBinDic.ContainsKey(key))
                {
                    int smdID = i;
                    SMDLine smdLine = smdLines[i];

                    obj.WriteLine("g " + "UHDSCENARIO#SMD_" + smdID.ToString("D3") + "#SMX_" + smdLine.SmxID.ToString("D3")
                    + "#TYPE_" + smdLine.objectStatus.ToString("X2") + "#BIN_" + smdLine.BinID.ToString("D3") + "#");

                    ObjCreatePart(obj, uhdBinDic[key], smdLine, materialList, ref indexCount, UseColorsInObjFile);
                }

            }

            obj.Close();
        }

        public static void ObjCreatePart(StreamWriter obj, UhdBIN uhdbin, SMDLine smdLine, Dictionary<MaterialPart, string> materialList, ref uint indexCount, bool UseColorsInObjFile)
        {
            for (int i = 0; i < uhdbin.Vertex_Position_Array.Length; i++)
            {
                float[] pos = new float[3];// 0 = x, 1 = y, 2 = z
                pos[0] = uhdbin.Vertex_Position_Array[i].vx;
                pos[1] = uhdbin.Vertex_Position_Array[i].vy;
                pos[2] = uhdbin.Vertex_Position_Array[i].vz;

                pos = RotationUtils.RotationInX(pos, smdLine.angleX);
                pos = RotationUtils.RotationInY(pos, smdLine.angleY);
                pos = RotationUtils.RotationInZ(pos, smdLine.angleZ);

                pos[0] = ((pos[0] * smdLine.scaleX) + (smdLine.positionX)) / CONSTs.GLOBAL_POSITION_SCALE;
                pos[1] = ((pos[1] * smdLine.scaleY) + (smdLine.positionY)) / CONSTs.GLOBAL_POSITION_SCALE;
                pos[2] = ((pos[2] * smdLine.scaleZ) + (smdLine.positionZ)) / CONSTs.GLOBAL_POSITION_SCALE;

                string v = "v " + (pos[0]).ToFloatString()
                          + " " + (pos[1]).ToFloatString()
                          + " " + (pos[2]).ToFloatString();

                if (UseColorsInObjFile && uhdbin.Header.ReturnsIsEnableVertexColors() && uhdbin.Vertex_Color_Array.Length > i)
                {
                    float r = uhdbin.Vertex_Color_Array[i].r / 255f;
                    float g = uhdbin.Vertex_Color_Array[i].g / 255f;
                    float b = uhdbin.Vertex_Color_Array[i].b / 255f;
                    float a = uhdbin.Vertex_Color_Array[i].a / 255f;

                    v += " " + (r).ToFloatString()
                       + " " + (g).ToFloatString()
                       + " " + (b).ToFloatString()
                       + " " + (a).ToFloatString();
                }
                obj.WriteLine(v);

            }

            for (int i = 0; i < uhdbin.Vertex_Normal_Array.Length; i++)
            {
                float nx = uhdbin.Vertex_Normal_Array[i].nx;
                float ny = uhdbin.Vertex_Normal_Array[i].ny;
                float nz = uhdbin.Vertex_Normal_Array[i].nz;

                float NORMAL_FIX = (float)Math.Sqrt((nx * nx) + (ny * ny) + (nz * nz));
                NORMAL_FIX = (NORMAL_FIX == 0) ? 1 : NORMAL_FIX;
                nx /= NORMAL_FIX;
                ny /= NORMAL_FIX;
                nz /= NORMAL_FIX;

                float[] normal = new float[3];// 0 = x, 1 = y, 2 = z
                normal[0] = nx;
                normal[1] = ny;
                normal[2] = nz;

                normal = RotationUtils.RotationInX(normal, smdLine.angleX);
                normal = RotationUtils.RotationInY(normal, smdLine.angleY);
                normal = RotationUtils.RotationInZ(normal, smdLine.angleZ);

                obj.WriteLine("vn " + 
                    (normal[0]).ToFloatString() + " " +
                    (normal[1]).ToFloatString() + " " +
                    (normal[2]).ToFloatString());
            }

            for (int i = 0; i < uhdbin.Vertex_UV_Array.Length; i++)
            {
                float tu = uhdbin.Vertex_UV_Array[i].tu;
                float tv = (uhdbin.Vertex_UV_Array[i].tv - 1) * -1;
                obj.WriteLine("vt " + tu.ToFloatString() + " " + tv.ToFloatString());
            }


            for (int g = 0; g < uhdbin.Materials.Length; g++)
            {
                obj.WriteLine("usemtl " + materialList[uhdbin.Materials[g].material]);

                for (int i = 0; i < uhdbin.Materials[g].face_index_array.Length; i++)
                {
                    string a = (uhdbin.Materials[g].face_index_array[i].i1 + indexCount + 1).ToString();
                    string b = (uhdbin.Materials[g].face_index_array[i].i2 + indexCount + 1).ToString();
                    string c = (uhdbin.Materials[g].face_index_array[i].i3 + indexCount + 1).ToString();

                    obj.WriteLine("f " + a + "/" + a + "/" + a
                                 + " " + b + "/" + b + "/" + b
                                 + " " + c + "/" + c + "/" + c);
                }


            }

            indexCount += (uint)uhdbin.Vertex_Position_Array.Length;

        }

        private static void PrintMagicInIDX(TextWriter text, SmdMagic smdMagic)
        {
            if (smdMagic.magic != 0x0040)
            {
                text.WriteLine("Magic:" + smdMagic.magic.ToString("X4"));
            }

            if (smdMagic.extraParameters.Length != 0)
            {
                text.WriteLine("ExtraParameterAmount:" + smdMagic.extraParameters.Length);
                for (int i = 0; i < smdMagic.extraParameters.Length; i++)
                {
                    text.WriteLine($"ExtraParameter{i}:" + smdMagic.extraParameters[i]);
                }
            }
        }

        public static void CreateIdxScenario(SMDLine[] smdLines, string binFolder, string baseDirectory, string baseFileName, string SmdFileName, SmdMagic smdMagic)
        {
            //
            TextWriter text = new FileInfo(baseDirectory + baseFileName + ".idxuhdscenario").CreateText();
            text.WriteLine(Shared.HeaderText());
            text.WriteLine("");

            PrintMagicInIDX(text, smdMagic);
            text.WriteLine("SmdAmount:" + smdLines.Length);
            text.WriteLine("SmdFileName:" + SmdFileName);
            text.WriteLine("BinFolder:" + binFolder);
            text.WriteLine("UseIdxMaterial:false");
            text.WriteLine("UseIdxUhdTpl:false");
            text.WriteLine("EnableVertexColor:false");
            text.WriteLine("EnableDinamicVertexColor:true");

            text.WriteLine("");
            for (int i = 0; i < smdLines.Length; i++)
            {
                text.WriteLine("");
                CreateIdxScenario_parts(i, ref text, smdLines[i]);
            }

            text.Close();


        }

        private static void CreateIdxScenario_parts(int id, ref TextWriter text, SMDLine smdLine)
        {
            var inv = (CultureInfo)CultureInfo.InvariantCulture.Clone();
            inv.NumberFormat.NumberDecimalDigits = 9;

            string positionX = (smdLine.positionX / CONSTs.GLOBAL_POSITION_SCALE).ToString("f9", inv);
            string positionY = (smdLine.positionY / CONSTs.GLOBAL_POSITION_SCALE).ToString("f9", inv);
            string positionZ = (smdLine.positionZ / CONSTs.GLOBAL_POSITION_SCALE).ToString("f9", inv);
            text.WriteLine(id.ToString("D3") + "_positionX:" + positionX);
            text.WriteLine(id.ToString("D3") + "_positionY:" + positionY);
            text.WriteLine(id.ToString("D3") + "_positionZ:" + positionZ);

            string angleX = (smdLine.angleX).ToString("f9", inv);
            string angleY = (smdLine.angleY).ToString("f9", inv);
            string angleZ = (smdLine.angleZ).ToString("f9", inv);
            text.WriteLine(id.ToString("D3") + "_angleX:" + angleX);
            text.WriteLine(id.ToString("D3") + "_angleY:" + angleY);
            text.WriteLine(id.ToString("D3") + "_angleZ:" + angleZ);

            string scaleX = (smdLine.scaleX).ToString("f9", inv);
            string scaleY = (smdLine.scaleY).ToString("f9", inv);
            string scaleZ = (smdLine.scaleZ).ToString("f9", inv);
            text.WriteLine(id.ToString("D3") + "_scaleX:" + scaleX);
            text.WriteLine(id.ToString("D3") + "_scaleY:" + scaleY);
            text.WriteLine(id.ToString("D3") + "_scaleZ:" + scaleZ);

        }

        public static void CreateIdxuhdSmd(SMDLine[] smdLines, string binFolder, string baseDirectory, string baseFileName, string SmdFileName, int binAmount, SmdMagic smdMagic)
        {
            TextWriter text = new FileInfo(baseDirectory + baseFileName + ".idxuhdsmd").CreateText();
            text.WriteLine(Shared.HeaderText());
            text.WriteLine("");

            PrintMagicInIDX(text, smdMagic);
            text.WriteLine("SmdAmount:" + smdLines.Length);
            text.WriteLine("SmdFileName:" + SmdFileName);
            text.WriteLine("BinFolder:" + binFolder);
            text.WriteLine("BinAmount:" + binAmount);

            text.WriteLine("");
            for (int i = 0; i < smdLines.Length; i++)
            {
                text.WriteLine("");
                CreateIdxScenario_parts(i, ref text, smdLines[i]);
                CreateIdxuhdSmd_parts(i, ref text, smdLines[i]);
            }

            text.Close();

        }

        private static void CreateIdxuhdSmd_parts(int id, ref TextWriter text, SMDLine smdLine) 
        {
            text.WriteLine(id.ToString("D3") + "_BinID:" + smdLine.BinID);
            text.WriteLine(id.ToString("D3") + "_FixedFF:" + smdLine.FixedFF.ToString("X2"));
            text.WriteLine(id.ToString("D3") + "_SmxID:" + smdLine.SmxID);
            text.WriteLine(id.ToString("D3") + "_unused1:" + smdLine.unused1.ToString("X8"));
            text.WriteLine(id.ToString("D3") + "_unused2:" + smdLine.unused2.ToString("X8"));
            text.WriteLine(id.ToString("D3") + "_unused3:" + smdLine.unused3.ToString("X8"));
            text.WriteLine(id.ToString("D3") + "_unused4:" + smdLine.unused4.ToString("X8"));
            text.WriteLine(id.ToString("D3") + "_unused5:" + smdLine.unused5.ToString("X8"));
            text.WriteLine(id.ToString("D3") + "_unused6:" + smdLine.unused6.ToString("X8"));
            text.WriteLine(id.ToString("D3") + "_unused7:" + smdLine.unused7.ToString("X8"));
            text.WriteLine(id.ToString("D3") + "_objectStatus:" + smdLine.objectStatus.ToString("X8"));
        }


    }

}
