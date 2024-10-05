using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using SHARED_UHD_BIN.ALL;
using SHARED_UHD_BIN.EXTRACT;


namespace SHARED_UHD_SCENARIO_SMD.SCENARIO
{
    public class R100Extract
    {
        /*
        R100.r100extract
        FILE_0:r100_00_000.SMD
        FILE_1:r100_01_000.SMD
        FILE_2:r100_02_000.SMD
        FILE_3:r100_03_000.SMD
        FILE_4:r100_04_000.SMD
        FILE_5:r100_005.SMD
        FILE_6:r100_004.SMD
        */

        private const int r100_00_000 = 0;  //r100_00.dat/r100_00_000.SMD
        private const int r100_01_000 = 1;  //r100_01.dat/r100_01_000.SMD
        private const int r100_02_000 = 2;  //r100_02.dat/r100_02_000.SMD
        private const int r100_03_000 = 3;  //r100_03.dat/r100_03_000.SMD
        private const int r100_04_000 = 4;  //r100_04.dat/r100_04_000.SMD
        private const int r100_005 = 5;     //r100.udas/r100_005.SMD
        private const int r100_004 = 6;     //r100.udas/r100_006.SMD

        private const int FileAmount = 7;

        public static void Extract(FileInfo fileInfo1, bool IsPS4NS) 
        {
            string baseDirectory = fileInfo1.Directory.FullName + "\\";
            string baseFileName = Path.GetFileNameWithoutExtension(fileInfo1.Name);
            string baseName_allparts = baseFileName + ".allparts";
            string baseName_custom = baseFileName + ".custom";
            string baseSubDirectory = Path.Combine(baseDirectory, baseFileName) + "\\";

            string[] files = Read_r100extract(fileInfo1);
            Console.WriteLine("SMD files:");
            for (int fil = 0; fil < FileAmount; fil++)
            {
                Console.WriteLine("FILE_"+fil+":" + files[fil]);
                string smdpath = baseDirectory + files[fil];
                if (!File.Exists(smdpath))
                {
                    string error = "Error the file does not exist: " + files[fil];
                    Console.WriteLine(error);
                    throw new ArgumentException(error);
                }
            }

            SMDLine[][] smdLinesList = new SMDLine[FileAmount][];
            Dictionary<int, UhdBIN>[] modelList = new Dictionary<int, UhdBIN>[FileAmount];
            UhdTPL useUhdTpl = null;

            r100ToFileMethods offsets = new r100ToFileMethods();

            int commonBinAmount = 0;

            int[] order = new int[FileAmount]{ r100_004, r100_00_000, r100_01_000, r100_02_000, r100_03_000, r100_04_000, r100_005 };
            for (int i = 0; i < FileAmount; i++)
            {
                int fil = order[i];

                string smdpath = baseDirectory + files[fil];
                FileInfo smdfileinfo = new FileInfo(smdpath);
                string smdFileName = smdfileinfo.Name.Substring(0, smdfileinfo.Name.Length - smdfileinfo.Extension.Length);
                string smdSubDirectory = Path.Combine(new string[] { baseDirectory, smdFileName }) + "\\";

                ToFileMethods toFileMethods = new ToFileMethods(smdSubDirectory, true);

                Stream smdfile = smdfileinfo.OpenRead();

                SmdMagic smdMagic;
                Dictionary<int, UhdBIN> uhdBinDic;
                UhdTPL uhdTpl;
                UhdSmdExtract uhdSmdExtract = new UhdSmdExtract();
                offsets.fileID = fil;
                uhdSmdExtract.ToFileBin += offsets.ToFileBin;
                uhdSmdExtract.ToFileTpl += offsets.ToFileTpl;
                uhdSmdExtract.ToFileBin += toFileMethods.ToFileBin;
                uhdSmdExtract.ToFileTpl += toFileMethods.ToFileTpl;
                int binAmount = 0;
                if (fil == r100_005)
                {
                    binAmount = commonBinAmount;
                }
                SMDLine[] smdLines = uhdSmdExtract.Extract(smdfile, out uhdBinDic, out uhdTpl, out smdMagic, ref binAmount, IsPS4NS);
                CommonBINcheck(ref commonBinAmount, smdLines);
                smdfile.Close();

                smdLinesList[fil] = smdLines;
                modelList[fil] = uhdBinDic;
                if (fil == r100_005)//r100_005.SMD contem o tpl valido
                {
                    useUhdTpl = uhdTpl;
                }

                //cria idxuhdsmd
                UhdScenarioExtract.CreateIdxuhdSmd(smdLines, smdFileName, baseDirectory, baseFileName + ".FILE_" + fil, smdfileinfo.Name, binAmount, smdMagic);
            }

            //materials
            Dictionary<MaterialPart, string> materialList;
            var idxMaterial = IdxMaterialMultParser(modelList, out materialList);
            var mtl = IdxMtlParser.Parser(idxMaterial, useUhdTpl, IsPS4NS);

            //allparts
            OutputMaterial.CreateIdxMaterial(idxMaterial, baseDirectory, baseName_allparts);
            OutputMaterial.CreateIdxUhdTpl(useUhdTpl, baseDirectory, baseName_allparts);
            OutputMaterial.CreateMTL(mtl, baseDirectory, baseName_allparts);
            CreateOBJ(smdLinesList, modelList, materialList, baseDirectory, baseName_allparts, true);
            Create_r100repack_File(smdLinesList, baseDirectory, baseName_allparts, files);

            //custom
            SMDLine[] newSmdLines = null;
            Dictionary<int, UhdBIN> binList = null;
            var newBinOrder = Converter(smdLinesList, modelList, out newSmdLines, out binList);
            UhdScenarioExtract.CreateOBJ(newSmdLines, binList, materialList, baseDirectory, baseName_custom, true);
            SmdMagic smdMagicCustom = new SmdMagic();
            UhdScenarioExtract.CreateIdxScenario(newSmdLines, baseFileName, baseDirectory, baseName_custom, files[r100_004], smdMagicCustom);
            UhdScenarioExtract.CreateIdxuhdSmd(newSmdLines, baseFileName, baseDirectory, baseName_custom, files[r100_004], binList.Count, smdMagicCustom);
            OutputMaterial.CreateIdxMaterial(idxMaterial, baseDirectory, baseName_custom);
            OutputMaterial.CreateIdxUhdTpl(useUhdTpl, baseDirectory, baseName_custom);
            OutputMaterial.CreateMTL(mtl, baseDirectory, baseName_custom);

            //extra os arquivos bin para custom

            for (int i = 0; i < FileAmount; i++)
            {
                string smdpath = baseDirectory + files[i];
                FileInfo smdfileinfo = new FileInfo(smdpath);
                string smdFileName = smdfileinfo.Name.Substring(0, fileInfo1.Name.Length - fileInfo1.Extension.Length);
                string smdSubDirectory = Path.Combine(new string[] { baseDirectory, smdFileName }) + "\\";
                Stream smdfile = smdfileinfo.OpenRead();
                ToFileMethods toFileMethods = new ToFileMethods(baseSubDirectory, true);

                var list = offsets.binOffsetList.Where(w => w.Key.fileID == i).ToList();
                foreach (var item in list)
                {
                    int newId = newBinOrder[(item.Key.fileID, item.Key.binID, false)];
                    var offset = offsets.binOffsetList[(item.Key.fileID, item.Key.binID)];
                    toFileMethods.ToFileBin(smdfile, offset.binOffset, offset.endOffset, newId);
                }
                
                if (i == r100_005)//r100_005.SMD contem o tpl valido
                {
                    var offsetToTpl = offsets.tplOffsetList[i];
                    toFileMethods.ToFileTpl(smdfile, offsetToTpl.tplOffset, offsetToTpl.endOffset);
                }

                smdfile.Close();
            }

        }

        // converter os varios SMD, para um so SMD
        private static Dictionary<(int file, int binID, bool type), ushort> Converter(SMDLine[][] smdLinesList, Dictionary<int, UhdBIN>[] modelList, out SMDLine[] newSmdLines, out Dictionary<int, UhdBIN> binList)
        {
            int Length = 0;
            int[] order = new int[] { r100_004, r100_00_000, r100_01_000, r100_02_000, r100_03_000, r100_04_000 };
            for (int o = 0; o < order.Length; o++)
            {
                Length += smdLinesList[order[o]].Length;
            }

            newSmdLines = new SMDLine[Length];
            binList = new Dictionary<int, UhdBIN>();

            // key (id do arquivo, seu bin, "type"), novo endereço do bin
            Dictionary<(int file, int binID, bool type), ushort> NewBinId = new Dictionary<(int file, int binID, bool type), ushort>();

            ushort newBinCounter = 0;

            //adiciono na lista os bins do arquivo r100_005.smd
            foreach (var binID in modelList[r100_005].Keys)
            {
                var key = (r100_005, binID, false);
                if (!NewBinId.ContainsKey(key))
                {
                    NewBinId.Add(key, (ushort)binID);
                    if (newBinCounter < binID)
                    {
                        newBinCounter = (ushort)binID;
                    }
                    
                }
            }
            newBinCounter++;

            // arquivo r100_004.smd

            for (int i = 0; i < smdLinesList[r100_004].Length; i++)
            {
                bool type = (smdLinesList[r100_004][i].objectStatus & 0x10) == 0x10;
                var key = (r100_004, smdLinesList[r100_004][i].BinID, type);
                if (!NewBinId.ContainsKey(key))
                {
                    if (type)
                    {
                        NewBinId.Add(key, smdLinesList[r100_004][i].BinID);
                    }
                    else
                    {
                        NewBinId.Add(key, newBinCounter);
                        newBinCounter++;
                    }
                }
               
            }

            // smd dentro dos .dat
            for (int fil = 0; fil < 5; fil++)
            {
                for (int i = 0; i < smdLinesList[fil].Length; i++)
                {
                    bool type = (smdLinesList[fil][i].objectStatus & 0x10) == 0x10;
                    var key = (fil, smdLinesList[fil][i].BinID, type);

                    if (!NewBinId.ContainsKey(key))
                    {
                        if (type)
                        {
                            NewBinId.Add(key, smdLinesList[fil][i].BinID);
                        }
                        else
                        {
                            NewBinId.Add(key, newBinCounter);
                            newBinCounter++;
                        }
                    }   
                }
            }

            //-----------------------
            int smdCounter = 0;

            // cria o novo newSmdLines
            for (int fil = 6; fil >= 0; fil--)
            {
                if (fil != r100_005)
                {
                    for (int i = 0; i < smdLinesList[fil].Length; i++)
                    {
                        var newLine = smdLinesList[fil][i].Clone();
                        bool type = (smdLinesList[fil][i].objectStatus & 0x10) == 0x10;
                        newLine.BinID = NewBinId[(fil, smdLinesList[fil][i].BinID, type)];
                        newLine.objectStatus = smdLinesList[fil][i].objectStatus & 0x0F;
                        newSmdLines[smdCounter] = newLine;
                        smdCounter++;
                    }
                }
            }

            //------------------
            // cria o novo binList
            foreach (var item in NewBinId)
            {
                if (item.Key.type == false)
                {
                    if (modelList[item.Key.file].ContainsKey(item.Key.binID))
                    {
                        if (!binList.ContainsKey(item.Value))
                        {
                            binList.Add(item.Value, modelList[item.Key.file][item.Key.binID]);
                        }
                    }
                }
               
            }

            return NewBinId;
        }

        private static void CommonBINcheck(ref int commonBinAmount, SMDLine[] smdLines) 
        {
            for (int i = 0; i < smdLines.Length; i++)
            {
                SMDLine smdLine = smdLines[i];
                int BinID = smdLine.BinID;
                bool type = (smdLine.objectStatus & 0x10) == 0x10;
                if (type)
                {
                    if (commonBinAmount <= BinID)
                    {
                        commonBinAmount = BinID + 1;
                    }
                }
            }
        }

        private static string[] Read_r100extract(FileInfo fileInfo1)
        {
            StreamReader r100extract = new StreamReader(fileInfo1.OpenRead(), Encoding.ASCII);

            Dictionary<string, string> pair = new Dictionary<string, string>();

            string line = "";
            while (line != null)
            {
                line = r100extract.ReadLine();
                if (line != null && line.Length != 0)
                {
                    var split = line.Trim().Split(new char[] { ':' });

                    if (line.TrimStart().StartsWith(":") || line.TrimStart().StartsWith("#") || line.TrimStart().StartsWith("/"))
                    {
                        continue;
                    }
                    else if (split.Length >= 2)
                    {
                        string key = split[0].ToUpper().Trim();

                        if (!pair.ContainsKey(key))
                        {
                            pair.Add(key, split[1]);
                        }

                    }

                }
            }

            //----

            string[] res = new string[FileAmount];

            for (int i = 0; i < FileAmount; i++)
            {
                try
                {
                    string value = pair["FILE_" + i].Trim();
                    value = value.Replace('/', '\\')
                  .Replace(":", "").Replace("*", "").Replace("\"", "").Replace("|", "")
                  .Replace("<", "").Replace(">", "").Replace("?", "").Replace(" ", "_");

                    value = value.Split('\\').Last();

                    if (value.Length == 0)
                    {
                        value = "null";
                    }

                    res[i] = Path.GetFileNameWithoutExtension(value) + ".SMD";
                }
                catch (Exception)
                {
                    res[i] = "Error";
                }

            }

            return res;
        }

        private static IdxMaterial IdxMaterialMultParser(Dictionary<int, UhdBIN>[] modelList, out Dictionary<MaterialPart, string> invDic)
        {
            IdxMaterial idx = new IdxMaterial();
            idx.MaterialDic = new Dictionary<string, MaterialPart>();
            invDic = new Dictionary<MaterialPart, string>();

            int counter = 0;

            for (int fil = 0; fil < modelList.Length; fil++)
            {
                foreach (var item in modelList[fil])
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
            }

            return idx;
        }

        private static void CreateOBJ(SMDLine[][] smdLinesList, Dictionary<int, UhdBIN>[] modelList, Dictionary<MaterialPart, string> materialList, string baseDirectory, string baseFileName, bool UseColorsInObjFile)
        {
            StreamWriter obj = new StreamWriter(baseDirectory + baseFileName + ".obj", false);
            obj.WriteLine(Shared.HeaderText());
            obj.WriteLine("");

            obj.WriteLine("mtllib " + baseFileName + ".mtl");

            uint indexCount = 0;

            int[] order = new int[] { r100_004, r100_00_000, r100_01_000, r100_02_000, r100_03_000, r100_04_000 };
            for (int o = 0; o < order.Length; o++)
            {
                int fil = order[o];

                for (int i = 0; i < smdLinesList[fil].Length; i++)
                {
                    SMDLine smdLine = smdLinesList[fil][i];

                    int key = smdLine.BinID;
                    bool type = (smdLine.objectStatus & 0x10) == 0x10;

                    int file = fil;
                    int smdID = i;

                    string group = "g " + "FILE_" + file + "#SMD_" + smdID.ToString("D3") + "#SMX_" + smdLine.SmxID.ToString("D3")
                          + "#TYPE_" + smdLine.objectStatus.ToString("X2") + "#BIN_" + smdLine.BinID.ToString("D3") + "#";

                    if (type)
                    {
                        group += "CommonBIN#";
                    }

                    obj.WriteLine(group);

                    if (type)
                    {
                        if (modelList[r100_005].ContainsKey(key))
                        {
                            UhdScenarioExtract.ObjCreatePart(obj, modelList[r100_005][key], smdLine, materialList, ref indexCount, UseColorsInObjFile);
                        }
                    }
                    else
                    {
                        if (modelList[fil].ContainsKey(key))
                        {
                            UhdScenarioExtract.ObjCreatePart(obj, modelList[fil][key], smdLine, materialList, ref indexCount, UseColorsInObjFile);
                        }

                    }
                }
            }

            obj.Close();
        }

        private static void Create_r100repack_File(SMDLine[][] smdLinesList, string baseDirectory, string baseFileName, string[] files)
        {
            TextWriter text = new FileInfo(baseDirectory + baseFileName + ".r100repack").CreateText();
            text.WriteLine(Shared.HeaderText());
            text.WriteLine("");


            for (int fil = 0; fil < FileAmount; fil++)
            {
                string smdpath = baseDirectory + files[fil];
                FileInfo smdfileinfo = new FileInfo(smdpath);
                string smdFileName = smdfileinfo.Name.Substring(0, smdfileinfo.Name.Length - smdfileinfo.Extension.Length);
                int SmdAmount = smdLinesList[fil].Length;
                if (fil == r100_005)
                {
                    SmdAmount = 0;
                }
                text.WriteLine("FILE_" + fil + "_SmdAmount:" + SmdAmount);
                text.WriteLine("FILE_" + fil + "_SmdFileName:" + smdfileinfo.Name);
                text.WriteLine("FILE_" + fil + "_BinFolder:" + smdFileName);
            }

            text.WriteLine("UseIdxMaterial:false");
            text.WriteLine("UseIdxUhdTpl:false");
            text.WriteLine("EnableVertexColor:false");
            text.WriteLine("EnableDinamicVertexColor:true");

            text.WriteLine("");

            int[] order = new int[] { r100_004, r100_00_000, r100_01_000, r100_02_000, r100_03_000, r100_04_000 };
            for (int o = 0; o < order.Length; o++)
            {
                int fil = order[o];
                for (int i = 0; i < smdLinesList[fil].Length; i++)
                {
                    text.WriteLine("");
                    CreateIdxScenario_parts(fil, i, ref text, smdLinesList[fil][i]);
                }

            }

            text.Close();
        }
        private static void CreateIdxScenario_parts(int fileID, int smdID, ref TextWriter text, SMDLine smdLine)
        {
            var inv = (System.Globalization.CultureInfo)System.Globalization.CultureInfo.InvariantCulture.Clone();
            inv.NumberFormat.NumberDecimalDigits = 9;

            string positionX = (smdLine.positionX / CONSTs.GLOBAL_POSITION_SCALE).ToString("f9", inv);
            string positionY = (smdLine.positionY / CONSTs.GLOBAL_POSITION_SCALE).ToString("f9", inv);
            string positionZ = (smdLine.positionZ / CONSTs.GLOBAL_POSITION_SCALE).ToString("f9", inv);
            text.WriteLine("FILE_" + fileID + "_SMD_" + smdID.ToString("D3") + "_positionX:" + positionX);
            text.WriteLine("FILE_" + fileID + "_SMD_" + smdID.ToString("D3") + "_positionY:" + positionY);
            text.WriteLine("FILE_" + fileID + "_SMD_" + smdID.ToString("D3") + "_positionZ:" + positionZ);

            string angleX = (smdLine.angleX).ToString("f9", inv);
            string angleY = (smdLine.angleY).ToString("f9", inv);
            string angleZ = (smdLine.angleZ).ToString("f9", inv);
            text.WriteLine("FILE_" + fileID + "_SMD_" + smdID.ToString("D3") + "_angleX:" + angleX);
            text.WriteLine("FILE_" + fileID + "_SMD_" + smdID.ToString("D3") + "_angleY:" + angleY);
            text.WriteLine("FILE_" + fileID + "_SMD_" + smdID.ToString("D3") + "_angleZ:" + angleZ);

            string scaleX = (smdLine.scaleX).ToString("f9", inv);
            string scaleY = (smdLine.scaleY).ToString("f9", inv);
            string scaleZ = (smdLine.scaleZ).ToString("f9", inv);
            text.WriteLine("FILE_" + fileID + "_SMD_" + smdID.ToString("D3") + "_scaleX:" + scaleX);
            text.WriteLine("FILE_" + fileID + "_SMD_" + smdID.ToString("D3") + "_scaleY:" + scaleY);
            text.WriteLine("FILE_" + fileID + "_SMD_" + smdID.ToString("D3") + "_scaleZ:" + scaleZ);
        }

    }

    public class r100ToFileMethods
    {
        public Dictionary<(int fileID, int binID), (long binOffset, long endOffset)> binOffsetList;
        public int fileID = 0;


        public Dictionary<int, (long tplOffset, long endOffset)> tplOffsetList;

        public r100ToFileMethods()
        {
            binOffsetList = new Dictionary<(int fileId, int binId), (long binOffset, long endOffset)>();
            tplOffsetList = new Dictionary<int, (long tplOffset, long endOffset)>();
        }

        public void ToFileBin(Stream fileStream, long binOffset, long endOffset, int binID)
        {
            var key = (fileID, binID);
            var value = (binOffset, endOffset);
            if (!binOffsetList.ContainsKey(key))
            {
                binOffsetList.Add(key, value);
            }
        }

        public void ToFileTpl(Stream fileStream, long tplOffset, long endOffset)
        {
            var key = fileID;
            var value = (tplOffset, endOffset);
            if (!tplOffsetList.ContainsKey(key))
            {
                tplOffsetList.Add(key, value);
            }
        }

    }


}
