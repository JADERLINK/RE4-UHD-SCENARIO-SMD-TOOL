using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace RE4_UHD_SCENARIO_SMD_TOOL
{
    class Program
    {
        public const string VERSION = "B.1.0.09 (2024-03-30)";

        public static string headerText()
        {
            return "# github.com/JADERLINK/RE4-UHD-SCENARIO-SMD-TOOL" + Environment.NewLine +
                   "# youtube.com/@JADERLINK" + Environment.NewLine +
                   "# RE4_UHD_SCENARIO_SMD_TOOL by: JADERLINK" + Environment.NewLine +
                   "# Thanks to \"mariokart64n\" and \"CodeMan02Fr\", " + Environment.NewLine +
                   "# Thanks to \"zatarita\", \"Mr.Curious\", \"Biohazard4X\" and \"kTeo\" for help with the r100 scenario;" + Environment.NewLine +
                   "# Material information by \"Albert\"" + Environment.NewLine +
                  $"# Version {VERSION}";
        }

        static void Main(string[] args)
        {
            Console.WriteLine(headerText());

            if (args.Length == 0)
            {
                Console.WriteLine("For more information read:");
                Console.WriteLine("https://github.com/JADERLINK/RE4-UHD-SCENARIO-SMD-TOOL");
                Console.WriteLine("Press any key to close the console.");
                Console.ReadKey();

            }
            else if (args.Length >= 1 && File.Exists(args[0]))
            {
                //FileInfo
                FileInfo fileInfo1 = new FileInfo(args[0]);
                //extension
                string file1Extension = fileInfo1.Extension.ToUpperInvariant();

                Console.WriteLine("File1: " + fileInfo1.Name);

                try
                {
                    Actions(fileInfo1, file1Extension);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error1: " + ex);
                }

            }
            else
            {
                Console.WriteLine("No arguments or file does not exist");
            }

            Console.WriteLine("End");
        }


        private static void Actions(FileInfo fileInfo1, string file1Extension)
        {
            // modo de extração
            if (file1Extension == ".SMD")
            {
                string baseFileName = fileInfo1.Name.Substring(0, fileInfo1.Name.Length - fileInfo1.Extension.Length);
                string baseNameScenario = baseFileName + ".scenario";
                string baseDirectory = fileInfo1.Directory.FullName + "\\";
                string baseSubDirectory = Path.Combine(new string[] { baseDirectory, baseFileName }) + "\\";
                Stream smdfile = fileInfo1.OpenRead();

                SCENARIO.SmdMagic smdMagic;
                Dictionary<int, RE4_UHD_BIN_TOOL.EXTRACT.UhdBIN> uhdBinDic;
                RE4_UHD_BIN_TOOL.EXTRACT.UhdTPL uhdTpl;
                SCENARIO.ToFileMethods toFileMethods = new SCENARIO.ToFileMethods(baseSubDirectory, true);
                SCENARIO.UhdSmdExtract uhdSmdExtract = new SCENARIO.UhdSmdExtract();
                uhdSmdExtract.ToFileBin += toFileMethods.ToFileBin;
                uhdSmdExtract.ToFileTpl += toFileMethods.ToFileTpl;
                int binAmount = 0;
                SCENARIO.SMDLine[] smdLines = uhdSmdExtract.Extract(smdfile, out uhdBinDic, out uhdTpl, out smdMagic, ref binAmount);
                smdfile.Close();

                Dictionary<RE4_UHD_BIN_TOOL.ALL.MaterialPart, string> materialList;

                var idxMaterial = SCENARIO.UhdScenarioExtract.IdxMaterialMultParser(uhdBinDic, out materialList);

                SCENARIO.UhdScenarioExtract.CreateOBJ(smdLines, uhdBinDic, materialList, baseDirectory, baseNameScenario, true);

                RE4_UHD_BIN_TOOL.EXTRACT.OutputMaterial.CreateIdxMaterial(idxMaterial, baseDirectory, baseNameScenario);
                RE4_UHD_BIN_TOOL.EXTRACT.OutputMaterial.CreateIdxUhdTpl(uhdTpl, baseDirectory, baseNameScenario);

                var mtl = RE4_UHD_BIN_TOOL.ALL.IdxMtlParser.Parser(idxMaterial, uhdTpl);
                RE4_UHD_BIN_TOOL.EXTRACT.OutputMaterial.CreateMTL(mtl, baseDirectory, baseNameScenario);

                SCENARIO.UhdScenarioExtract.CreateIdxScenario(smdLines, baseFileName, baseDirectory, baseNameScenario, fileInfo1.Name, smdMagic);
                SCENARIO.UhdScenarioExtract.CreateIdxuhdSmd(smdLines, baseFileName, baseDirectory, baseNameScenario, fileInfo1.Name, binAmount, smdMagic);
            }

            // mode de repack (com um obj, para o cenario todo)
            else if (file1Extension == ".IDXUHDSCENARIO")
            {
                string baseFileName = fileInfo1.Name.Substring(0, fileInfo1.Name.Length - fileInfo1.Extension.Length);
                string baseDirectory = fileInfo1.Directory.FullName + "\\";

                Stream idxFile = fileInfo1.OpenRead();
                SCENARIO.IdxUhdScenario idxUhdScenario = SCENARIO.IdxUhdScenarioLoader.Loader(new StreamReader(idxFile, Encoding.ASCII));


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

                if (idxUhdScenario.UseIdxMaterial)
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

                    //opcional nesse caso
                    if (File.Exists(idxuhdtplPath))
                    {
                        Console.WriteLine("Load File: " + baseFileName + ".idxuhdtpl");
                        idxuhdtplFile = new FileInfo(idxuhdtplPath).OpenRead();
                    }

                }
                #endregion

                // carrega os materiais

                RE4_UHD_BIN_TOOL.EXTRACT.UhdTPL uhdTPL = null;
                RE4_UHD_BIN_TOOL.ALL.IdxMaterial material = null;
                RE4_UHD_BIN_TOOL.ALL.IdxMtl idxMtl = null;

                if (idxuhdtplFile != null) // .IDXUHDTPL
                {
                    uhdTPL = RE4_UHD_BIN_TOOL.ALL.IdxUhdTplLoad.Load(idxuhdtplFile);
                    idxuhdtplFile.Close();
                }

                if (idxmaterialFile != null)
                {
                    material = RE4_UHD_BIN_TOOL.ALL.IdxMaterialLoad.Load(idxmaterialFile);
                    idxmaterialFile.Close();
                }

                if (mtlFile != null) // .MTL
                {
                    RE4_UHD_BIN_TOOL.REPACK.MtlLoad.Load(mtlFile, out idxMtl);
                    mtlFile.Close();
                }

                if (idxMtl != null)
                {
                    Console.WriteLine("Converting .mtl");

                    new RE4_UHD_BIN_TOOL.REPACK.MtlConverter(baseDirectory).Convert(idxMtl, ref uhdTPL, out material);
                    RE4_UHD_BIN_TOOL.EXTRACT.OutputMaterial.CreateIdxUhdTpl(uhdTPL, baseDirectory, baseFileName + ".Repack");
                    RE4_UHD_BIN_TOOL.EXTRACT.OutputMaterial.CreateIdxMaterial(material, baseDirectory, baseFileName + ".Repack");
                }

                // cria o arquivo smd e os bins

                Console.WriteLine("Reading and converting .obj");
                Dictionary<int, SCENARIO.SmdBaseLine> objGroupInfos = null;
                Dictionary<int, RE4_UHD_BIN_TOOL.REPACK.Structures.FinalStructure> finalBinList = null;
                SCENARIO.UhdScenarioRepack.RepackOBJ(objFile, ref idxUhdScenario, out objGroupInfos, out finalBinList);


                //cria arquivo .smd
                Console.WriteLine("Creating .SMD file");
                SCENARIO.MakeSMD_Scenario.CreateSMD(baseDirectory, idxUhdScenario.SmdFileName, objGroupInfos, idxUhdScenario, finalBinList, material, uhdTPL, idxUhdScenario.EnableVertexColor, true);

                //cria um novo idxuhdsmd
                Console.WriteLine("Creating new .idxuhdsmd");
                SCENARIO.SMDLine[] smdLines = SCENARIO.SmdLineParcer.Parser(idxUhdScenario.SmdAmount, idxUhdScenario.SmdLines, objGroupInfos);
                SCENARIO.SmdMagic smdMagic = new SCENARIO.SmdMagic();
                smdMagic.magic = idxUhdScenario.Magic;
                smdMagic.extraParameters = idxUhdScenario.ExtraParameters;
                SCENARIO.UhdScenarioExtract.CreateIdxuhdSmd(smdLines, idxUhdScenario.BinFolder, baseDirectory, baseFileName + ".Repack", idxUhdScenario.SmdFileName, idxUhdScenario.BinAmount, smdMagic);

            }

            // modo de repack usando os arquivos bins.
            else if (file1Extension == ".IDXUHDSMD")
            {
                string baseDirectory = fileInfo1.Directory.FullName + "\\";

                Stream idxFile = fileInfo1.OpenRead();
                SCENARIO.IdxUhdScenario idxUhdSmd = SCENARIO.IdxUhdScenarioLoader.Loader(new StreamReader(idxFile, Encoding.ASCII));

                SCENARIO.MakeSMD_WithBinFolder.CreateSMD(baseDirectory, idxUhdSmd);
            }

            //R100 extract
            else if (file1Extension == ".R100EXTRACT")
            {
                SCENARIO.R100Extract.Extract(fileInfo1);
            }

            //R100 repack
            else if (file1Extension == ".R100REPACK") 
            {
                SCENARIO.R100Repack.Repack(fileInfo1);
            }

            else
            {
                Console.WriteLine("Invalid file format: " + file1Extension);
            }
        }

    }
}
