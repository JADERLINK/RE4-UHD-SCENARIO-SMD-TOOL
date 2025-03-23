using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using SimpleEndianBinaryIO;

namespace SHARED_UHD_SCENARIO_SMD
{
    public static class MainAction
    {
        public static void MainContinue(string[] args, bool isPS4NS, Endianness endianness) 
        {
            System.Globalization.CultureInfo.CurrentCulture = System.Globalization.CultureInfo.InvariantCulture;

            Console.WriteLine(Shared.HeaderText());

            if (args.Length == 0)
            {
                Console.WriteLine("For more information read:");
                Console.WriteLine("https://github.com/JADERLINK/RE4-UHD-SCENARIO-SMD-TOOL");
                Console.WriteLine("Press any key to close the console.");
                Console.ReadKey();
            }
            else if (args.Length >= 1 && File.Exists(args[0]))
            {
                try
                {
                    FileInfo fileInfo1 = new FileInfo(args[0]);
                    string file1Extension = fileInfo1.Extension.ToUpperInvariant();
                    Console.WriteLine("File1: " + fileInfo1.Name);

                    Actions(fileInfo1, file1Extension, isPS4NS, endianness);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error1: " + ex);
                }
            }
            else
            {
                Console.WriteLine("File specified does not exist.");
            }

            Console.WriteLine("Finished!!!");
        }

        private static void Actions(FileInfo fileInfo1, string file1Extension, bool IsPS4NS, Endianness endianness)
        {
            // modo de extração
            if (file1Extension == ".SMD")
            {
                string baseFileName = Path.GetFileNameWithoutExtension(fileInfo1.Name);
                string baseNameScenario = baseFileName + ".scenario";
                string baseNameBinFolder = baseFileName + "_BIN";
                string baseDirectory = fileInfo1.Directory.FullName + "\\";
                string baseSubDirectory = Path.Combine(baseDirectory, baseNameBinFolder);
                Stream smdfile = fileInfo1.OpenRead();

                SCENARIO.SmdMagic smdMagic;
                Dictionary<int, SHARED_UHD_BIN.EXTRACT.UhdBIN> uhdBinDic;
                SHARED_UHD_BIN.EXTRACT.UhdTPL uhdTpl;
                SCENARIO.ToFileMethods toFileMethods = new SCENARIO.ToFileMethods(baseSubDirectory, true);
                SCENARIO.UhdSmdExtract uhdSmdExtract = new SCENARIO.UhdSmdExtract();
                uhdSmdExtract.ToFileBin += toFileMethods.ToFileBin;
                uhdSmdExtract.ToFileTpl += toFileMethods.ToFileTpl;
                int binAmount = 0;
                SCENARIO.SMDLine[] smdLines = uhdSmdExtract.Extract(smdfile, out uhdBinDic, out uhdTpl, out smdMagic, ref binAmount, IsPS4NS, endianness);
                smdfile.Close();

                Dictionary<SHARED_UHD_BIN.ALL.MaterialPart, string> materialList;

                var idxMaterial = SCENARIO.UhdScenarioExtract.IdxMaterialMultParser(uhdBinDic, out materialList);

                SCENARIO.UhdScenarioExtract.CreateOBJ(smdLines, uhdBinDic, materialList, baseDirectory, baseNameScenario, true);

                SHARED_UHD_BIN.EXTRACT.OutputMaterial.CreateIdxMaterial(idxMaterial, baseDirectory, baseNameScenario);
                SHARED_UHD_BIN.EXTRACT.OutputMaterial.CreateIdxUhdTpl(uhdTpl, baseDirectory, baseNameScenario);

                var mtl = SHARED_UHD_BIN.ALL.IdxMtlParser.Parser(idxMaterial, uhdTpl, IsPS4NS);
                SHARED_UHD_BIN.EXTRACT.OutputMaterial.CreateMTL(mtl, baseDirectory, baseNameScenario);

                SCENARIO.UhdScenarioExtract.CreateIdxScenario(smdLines, baseNameBinFolder, baseDirectory, baseNameScenario, fileInfo1.Name, smdMagic);
                SCENARIO.UhdScenarioExtract.CreateIdxuhdSmd(smdLines, baseNameBinFolder, baseDirectory, baseNameScenario, fileInfo1.Name, binAmount, smdMagic);
            }

            // mode de repack (com um obj, para o cenario todo)
            else if (file1Extension == ".IDXUHDSCENARIO")
            {
                string baseFileName = Path.GetFileNameWithoutExtension(fileInfo1.Name);
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

                    if (idxUhdScenario.UseIdxUhdTpl && File.Exists(idxuhdtplPath))
                    {
                        Console.WriteLine("Load File: " + baseFileName + ".idxuhdtpl");
                        idxuhdtplFile = new FileInfo(idxuhdtplPath).OpenRead();
                    }

                }
                #endregion

                // carrega os materiais

                SHARED_UHD_BIN.EXTRACT.UhdTPL uhdTPL = null;
                SHARED_UHD_BIN.ALL.IdxMaterial material = null;
                SHARED_UHD_BIN.ALL.IdxMtl idxMtl = null;

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

                // cria o arquivo smd e os bins

                Console.WriteLine("Reading and converting .obj");
                Dictionary<int, SCENARIO.SmdBaseLine> objGroupInfos = null;
                Dictionary<int, SHARED_UHD_BIN.REPACK.Structures.FinalStructure> finalBinList = null;
                SCENARIO.UhdScenarioRepack.RepackOBJ(objFile, ref idxUhdScenario, out objGroupInfos, out finalBinList, idxUhdScenario.EnableVertexColor || idxUhdScenario.EnableDinamicVertexColor);


                //cria arquivo .smd
                Console.WriteLine("Creating .SMD file");
                SCENARIO.MakeSMD_Scenario.CreateSMD(baseDirectory, idxUhdScenario.SmdFileName, objGroupInfos, idxUhdScenario, finalBinList, material, uhdTPL, idxUhdScenario.EnableVertexColor, idxUhdScenario.EnableDinamicVertexColor, true, IsPS4NS, endianness);

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

                SCENARIO.MakeSMD_WithBinFolder.CreateSMD(baseDirectory, idxUhdSmd, endianness);
            }

            //R100 extract
            else if (file1Extension == ".R100EXTRACT")
            {
                SCENARIO.R100Extract.Extract(fileInfo1, IsPS4NS, endianness);
            }

            //R100 repack
            else if (file1Extension == ".R100REPACK")
            {
                SCENARIO.R100Repack.Repack(fileInfo1, IsPS4NS, endianness);
            }

            else
            {
                Console.WriteLine("Invalid file format: " + file1Extension);
            }
        }
    }
}
