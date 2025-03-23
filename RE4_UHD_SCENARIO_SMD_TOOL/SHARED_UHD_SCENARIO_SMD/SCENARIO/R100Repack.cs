using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using SHARED_UHD_BIN.ALL;
using SHARED_UHD_BIN.EXTRACT;
using SimpleEndianBinaryIO;

namespace SHARED_UHD_SCENARIO_SMD.SCENARIO
{
    public static partial class R100Repack
    {
        private const int r100_00_000 = 0;  //r100_00.dat/r100_00_000.SMD
        private const int r100_01_000 = 1;  //r100_01.dat/r100_01_000.SMD
        private const int r100_02_000 = 2;  //r100_02.dat/r100_02_000.SMD
        private const int r100_03_000 = 3;  //r100_03.dat/r100_03_000.SMD
        private const int r100_04_000 = 4;  //r100_04.dat/r100_04_000.SMD
        private const int r100_005 = 5;     //r100.udas/r100_005.SMD
        private const int r100_004 = 6;     //r100.udas/r100_006.SMD

        private const int FileAmount = 7;

        public static void Repack(FileInfo fileInfo1, bool IsPS4NS, Endianness endianness) 
        {
            string baseFileName = Path.GetFileNameWithoutExtension(fileInfo1.Name);
            string baseDirectory = fileInfo1.Directory.FullName + "\\";

            Stream idxFile = fileInfo1.OpenRead();
            R100RepackIdx idx = Loader(new StreamReader(idxFile, Encoding.ASCII));

            // outros arquivos

            string objPath = baseDirectory + baseFileName + ".obj";
            string mtlPath = baseDirectory + baseFileName + ".mtl";
            string idxmaterialPath = baseDirectory + baseFileName + ".idxmaterial";
            string idxuhdtplPath = baseDirectory + baseFileName + ".idxuhdtpl";

            Stream objFile = null;
            Stream mtlFile = null;
            Stream idxmaterialFile = null;
            Stream idxuhdtplFile = null;

            #region verifica a existencia dos arquivos
            if (File.Exists(objPath))
            {

                Console.WriteLine("Load File: " + baseFileName + ".obj");
                objFile = new FileInfo(objPath).OpenRead();
            }
            else
            {
                Console.WriteLine("Error: .obj file not found;");
                return;
            }

            if (idx.UseIdxMaterial)
            {
                if (File.Exists(idxmaterialPath))
                {
                    Console.WriteLine("Load File: " + baseFileName + ".idxmaterial");
                    idxmaterialFile = new FileInfo(idxmaterialPath).OpenRead();
                }
                else
                {
                    Console.WriteLine("Error: .idxmaterial file not found;");
                    return;
                }

                if (File.Exists(idxuhdtplPath))
                {
                    Console.WriteLine("Load File: " + baseFileName + ".idxuhdtpl");
                    idxuhdtplFile = new FileInfo(idxuhdtplPath).OpenRead();
                }
                else
                {
                    Console.WriteLine("Error: .idxuhdtpl file not found. This file is required when using .idxmaterial;");
                    return;
                }

            }
            else
            {
                if (File.Exists(mtlPath))
                {
                    Console.WriteLine("Load File: " + baseFileName + ".mtl");
                    mtlFile = new FileInfo(mtlPath).OpenRead();
                }
                else
                {
                    Console.WriteLine("Error: .mtl file not found.");
                    return;
                }

                if (idx.UseIdxUhdTpl && File.Exists(idxuhdtplPath))
                {
                    Console.WriteLine("Load File: " + baseFileName + ".idxuhdtpl");
                    idxuhdtplFile = new FileInfo(idxuhdtplPath).OpenRead();
                }

            }
            #endregion

            // carrega os materiais
            
            UhdTPL uhdTPL = null;
            IdxMaterial material = null;
            IdxMtl idxMtl = null;

            #region materiais load
            if (idxuhdtplFile != null) // .IDXUHDTPL
            {
                uhdTPL = SHARED_UHD_BIN.ALL.IdxUhdTplLoad.Load(idxuhdtplFile);
                idxuhdtplFile.Close();
            }

            if (idxmaterialFile != null)
            {
                material = SHARED_UHD_BIN.ALL.IdxMaterialLoad.Load(idxmaterialFile);
                idxmaterialFile.Close();
            }

            if (mtlFile != null) // .MTL
            {
                SHARED_UHD_BIN.REPACK.MtlLoad.Load(mtlFile, out idxMtl);
                mtlFile.Close();
            }

            if (idxMtl != null)
            {
                Console.WriteLine("Converting .mtl");
                new SHARED_UHD_BIN.REPACK.MtlConverter(baseDirectory).Convert(idxMtl, ref uhdTPL, out material);
                SHARED_UHD_BIN.EXTRACT.OutputMaterial.CreateIdxUhdTpl(uhdTPL, baseDirectory, baseFileName + ".Repack");
                SHARED_UHD_BIN.EXTRACT.OutputMaterial.CreateIdxMaterial(material, baseDirectory, baseFileName + ".Repack");
            }
            #endregion

            // parte do .obj
            Console.WriteLine("Reading and converting .obj");
            Dictionary<int, Dictionary<int, SmdBaseLine>> objGroupInfosList = null;
            Dictionary<int, Dictionary<int, SHARED_UHD_BIN.REPACK.Structures.FinalStructure>> FinalBinListDic = null;
            int[] maxBin = null;
            RepackOBJ(objFile, ref idx, out objGroupInfosList, out FinalBinListDic, out maxBin, idx.EnableVertexColor || idx.EnableDinamicVertexColor);

            //cria arquivos .smd
            Console.WriteLine("Creating .SMD files");
            for (int fil = 0; fil < FileAmount; fil++)
            {
                UhdTPL _uhdTPL = new UhdTPL();
                _uhdTPL.TplArray = new TplInfo[0];

                Dictionary<int, SmdBaseLine> objGroupInfos = objGroupInfosList[fil];

                IdxUhdScenario idxUhdScenario = new IdxUhdScenario();
                idxUhdScenario.EnableDinamicVertexColor = idx.EnableDinamicVertexColor;
                idxUhdScenario.EnableVertexColor = idx.EnableVertexColor;
                idxUhdScenario.UseIdxMaterial = idx.UseIdxMaterial;
                idxUhdScenario.UseIdxUhdTpl = idx.UseIdxUhdTpl;
                idxUhdScenario.BinAmount = maxBin[fil];
                idxUhdScenario.BinFolder = idx.BinFolder[fil];
                idxUhdScenario.SmdAmount = idx.SmdAmount[fil];
                idxUhdScenario.SmdFileName = idx.SmdFileName[fil];
                idxUhdScenario.Magic = 0x0040;
                idxUhdScenario.ExtraParameters = new uint[0];
                idxUhdScenario.SmdLines = idx.SmdLines[fil];
                idxUhdScenario.SmdLinesExtras = new SMDLineIdxExtras[0];

                if (fil == r100_004)
                {
                    idxUhdScenario.Magic = 0x0140;
                    idxUhdScenario.ExtraParameters = new uint[5];
                    for (int i = 0; i < 5; i++)
                    {
                        idxUhdScenario.ExtraParameters[i] = (uint)idx.SmdAmount[i];
                    }
                   
                }

                if (fil == r100_005)
                {
                    //adiciona o tpl correto, esse é oque tem o tpl que é usado pelo jogo;
                    _uhdTPL = uhdTPL;

                    //adiciona smdEntry fake (nesse arquivo os smdEntry não são carregados pelo jogo)
                    idxUhdScenario.SmdAmount = maxBin[fil];
                    idxUhdScenario.SmdLines = new SMDLineIdx[0];
                    objGroupInfos = new Dictionary<int, SmdBaseLine>();
                    for (int i = 0; i < maxBin[fil]; i++)
                    {
                        SmdBaseLine line = new SmdBaseLine();
                        line.BinId = i;
                        line.SmdId = i;
                        line.SmxId = 254;
                        line.Type = 0;
                        objGroupInfos.Add(i, line);
                    }
                }

                MakeSMD_Scenario.CreateSMD(baseDirectory, idx.SmdFileName[fil], objGroupInfos, idxUhdScenario, FinalBinListDic[fil], material, _uhdTPL, idx.EnableVertexColor, idx.EnableDinamicVertexColor, true, IsPS4NS, endianness);


                //create new R100.FILE_?.Repack.idxuhdsmd
                SMDLine[] smdLines = SmdLineParcer.Parser(idxUhdScenario.SmdAmount, idxUhdScenario.SmdLines, objGroupInfos);
                SmdMagic smdMagic = new SmdMagic();
                smdMagic.magic = idxUhdScenario.Magic;
                smdMagic.extraParameters = idxUhdScenario.ExtraParameters;
                UhdScenarioExtract.CreateIdxuhdSmd(smdLines, idxUhdScenario.BinFolder, baseDirectory, baseFileName + ".FILE_" + fil + ".Repack", idxUhdScenario.SmdFileName, idxUhdScenario.BinAmount, smdMagic);

            }

        }


        public static R100RepackIdx Loader(StreamReader idxFile)
        {
            Dictionary<string, string> pair = new Dictionary<string, string>();

            string line = "";
            while (line != null)
            {
                line = idxFile.ReadLine();
                if (line != null && line.Length != 0)
                {
                    var split = line.Trim().Split(new char[] { ':' });

                    if (line.TrimStart().StartsWith(":") || line.TrimStart().StartsWith("#") || line.TrimStart().StartsWith("/") || line.TrimStart().StartsWith("\\"))
                    {
                        continue;
                    }
                    else if (split.Length >= 2)
                    {
                        string key = split[0].ToUpper().Trim();

                        if (!pair.ContainsKey(key))
                        {
                            pair.Add(key, split[1].Trim());
                        }

                    }

                }
            }

            //----

            R100RepackIdx idx = new R100RepackIdx();

             int[] SmdAmount = new int[FileAmount];

             string[] SmdFileName = new string[FileAmount];

             string[] BinFolder = new string[FileAmount];
    
             SMDLineIdx[][] SmdLines = new SMDLineIdx[FileAmount][];


            for (int fil = 0; fil < FileAmount; fil++)
            {


                //FILE_?_SmdAmount
                try
                {
                    string value = Utils.ReturnValidDecValue(pair["FILE_"+fil+"_SMDAMOUNT"]);
                    SmdAmount[fil] = int.Parse(value, System.Globalization.NumberStyles.Integer, System.Globalization.CultureInfo.InvariantCulture);
                }
                catch (Exception)
                {
                }

                //FILE_?_SmdFileName
                try
                {
                    string value = pair["FILE_"+fil+"_SMDFILENAME"].Trim();
                    value = value.Replace('/', '\\')
                  .Replace(":", "").Replace("*", "").Replace("\"", "").Replace("|", "")
                  .Replace("<", "").Replace(">", "").Replace("?", "").Replace(" ", "_");

                    value = value.Split('\\').Last();

                    if (value.Length == 0)
                    {
                        value = "null";
                    }

                    var fileinfo = new FileInfo(value);
                    SmdFileName[fil] = fileinfo.Name.Remove(fileinfo.Name.Length - fileinfo.Extension.Length, fileinfo.Extension.Length) + ".SMD";
                }
                catch (Exception)
                {
                }

                //FILE_?_BinFolder
                try
                {
                    string value = pair["FILE_"+fil+"_BINFOLDER"].Trim();
                    value = value.Replace('/', '\\')
                  .Replace(":", "").Replace("*", "").Replace("\"", "").Replace("|", "")
                  .Replace("<", "").Replace(">", "").Replace("?", "");

                    value = value.Split('\\').Last();

                    if (value.Length == 0)
                    {
                        value = "null";
                    }
                    BinFolder[fil] = value;
                }
                catch (Exception)
                {
                }

            }

            idx.UseIdxMaterial = IdxUhdScenarioLoader.GetBool(ref pair, "USEIDXMATERIAL");
            idx.EnableVertexColor = IdxUhdScenarioLoader.GetBool(ref pair, "ENABLEVERTEXCOLOR");
            idx.UseIdxUhdTpl = IdxUhdScenarioLoader.GetBool(ref pair, "USEIDXUHDTPL");
            idx.EnableDinamicVertexColor = IdxUhdScenarioLoader.GetBool(ref pair, "ENABLEDINAMICVERTEXCOLOR");

            //---

            for (int fil = 0; fil < FileAmount; fil++)
            {
                SMDLineIdx[] smdLines = new SMDLineIdx[SmdAmount[fil]];

                for (int i = 0; i < SmdAmount[fil]; i++)
                {
                    #region SMDLineIdx
                    string scaleXkey = "FILE_" + fil + "_SMD_" + i.ToString("D3") + "_SCALEX";
                    string scaleYkey = "FILE_" + fil + "_SMD_" + i.ToString("D3") + "_SCALEY";
                    string scaleZkey = "FILE_" + fil + "_SMD_" + i.ToString("D3") + "_SCALEZ";

                    string positionXkey = "FILE_" + fil + "_SMD_" + i.ToString("D3") + "_POSITIONX";
                    string positionYkey = "FILE_" + fil + "_SMD_" + i.ToString("D3") + "_POSITIONY";
                    string positionZkey = "FILE_" + fil + "_SMD_" + i.ToString("D3") + "_POSITIONZ";

                    string angleXkey = "FILE_" + fil + "_SMD_" + i.ToString("D3") + "_ANGLEX";
                    string angleYkey = "FILE_" + fil + "_SMD_" + i.ToString("D3") + "_ANGLEY";
                    string angleZkey = "FILE_" + fil + "_SMD_" + i.ToString("D3") + "_ANGLEZ";

                    SMDLineIdx smdline = new SMDLineIdx();

                    smdline.scaleX = IdxUhdScenarioLoader.GetFloat(ref pair, scaleXkey, 1f);
                    smdline.scaleY = IdxUhdScenarioLoader.GetFloat(ref pair, scaleYkey, 1f);
                    smdline.scaleZ = IdxUhdScenarioLoader.GetFloat(ref pair, scaleZkey, 1f);
                    smdline.positionX = IdxUhdScenarioLoader.GetFloat(ref pair, positionXkey, 0f);
                    smdline.positionY = IdxUhdScenarioLoader.GetFloat(ref pair, positionYkey, 0f);
                    smdline.positionZ = IdxUhdScenarioLoader.GetFloat(ref pair, positionZkey, 0f);
                    smdline.angleX = IdxUhdScenarioLoader.GetFloat(ref pair, angleXkey, 0f);
                    smdline.angleY = IdxUhdScenarioLoader.GetFloat(ref pair, angleYkey, 0f);
                    smdline.angleZ = IdxUhdScenarioLoader.GetFloat(ref pair, angleZkey, 0f);

                    smdLines[i] = smdline;
                    #endregion

                }

                SmdLines[fil] = smdLines;
            }

            // ----

            idx.SmdAmount = SmdAmount;
            idx.SmdFileName = SmdFileName;
            idx.BinFolder = BinFolder;
            idx.SmdLines = SmdLines;

            //---
            idxFile.Close();


            return idx;
        }

    }


    public class R100RepackIdx 
    {
        public int[] SmdAmount;

        public string[] SmdFileName;

        public string[] BinFolder;

        public SMDLineIdx[][] SmdLines;

        public bool UseIdxMaterial = false;
        public bool UseIdxUhdTpl = false;
        public bool EnableVertexColor = false;
        public bool EnableDinamicVertexColor = false;
    }

}
