using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using SHARED_UHD_BIN.ALL;
using SHARED_UHD_BIN.EXTRACT;
using SHARED_UHD_BIN.REPACK.Structures;

namespace SHARED_UHD_SCENARIO_SMD.SCENARIO
{
    public static class MakeSMD_Scenario
    {
        public static void CreateSMD(
            string baseDirectory, 
            string smdFileName, 
            Dictionary<int, SmdBaseLine> lines, 
            IdxUhdScenario idxScenario, 
            Dictionary<int, FinalStructure> finalBinList, 
            IdxMaterial material, 
            UhdTPL uhdTPL, 
            bool EnableVertexColors, 
            bool EnableDinamicVertexColor,
            bool createBinFiles,
            bool IsPS4NS)
        {
            string binPath = baseDirectory + idxScenario.BinFolder + "\\";

            if (createBinFiles)
            {
                Directory.CreateDirectory(binPath);
            }

            int SmdCount = idxScenario.SmdAmount;

            Stream stream = new FileInfo(baseDirectory + smdFileName).Create();

            byte[] header = new byte[0x10];

            byte[] b_Magic = BitConverter.GetBytes(idxScenario.Magic);
            header[0] = b_Magic[0];
            header[1] = b_Magic[1];

            byte[] b_SmdCount = BitConverter.GetBytes(SmdCount);
            header[2] = b_SmdCount[0];
            header[3] = b_SmdCount[1];

            uint binStreamPosition = (uint)((SmdCount * 72) + 0x10);

            if (idxScenario.Magic == 0x0140)
            {
                uint amount = (uint)idxScenario.ExtraParameters.Length;
                binStreamPosition += ((amount + 1) * 4);
            }

            uint SmdLinePadding = 0;
            {
                uint div = (binStreamPosition) / 16;
                if (binStreamPosition % 16 != 0)
                {
                    div++;
                }
                SmdLinePadding = (div * 16) - (binStreamPosition);
                binStreamPosition = div * 16;
            }

            byte[] b_binStreamPosition = BitConverter.GetBytes(binStreamPosition);
            header[4] = b_binStreamPosition[0];
            header[5] = b_binStreamPosition[1];
            header[6] = b_binStreamPosition[2];
            header[7] = b_binStreamPosition[3];

            stream.Write(header, 0, 0x10);

            if (idxScenario.Magic == 0x0140)
            {
                uint amount = (uint)idxScenario.ExtraParameters.Length;
                byte[] b_ExtraParameters = new byte[(amount + 1) * 4];
                BitConverter.GetBytes(amount).CopyTo(b_ExtraParameters, 0);
                int tempcounter = 4;
                for (int i = 0; i < idxScenario.ExtraParameters.Length; i++)
                {
                    BitConverter.GetBytes(idxScenario.ExtraParameters[i]).CopyTo(b_ExtraParameters, tempcounter);
                    tempcounter += 4;
                }

                stream.Write(b_ExtraParameters, 0, b_ExtraParameters.Length);
            }


            for (int i = 0; i < SmdCount; i++)
            {
                float positionX = 0f;
                float positionY = 0f;
                float positionZ = 0f;
                float angleX = 0f;
                float angleY = 0f;
                float angleZ = 0f;
                float scaleX = 1f;
                float scaleY = 1f;
                float scaleZ = 1f;

                ushort BinID = (ushort)lines[i].BinId;
                byte FixedFF = 0xFF;
                byte SmxID = (byte)lines[i].SmxId;
                uint objectStatus = lines[i].Type;

                if (idxScenario.SmdLines.Length > i)
                {
                    positionX = idxScenario.SmdLines[i].positionX * CONSTs.GLOBAL_POSITION_SCALE;
                    positionY = idxScenario.SmdLines[i].positionY * CONSTs.GLOBAL_POSITION_SCALE;
                    positionZ = idxScenario.SmdLines[i].positionZ * CONSTs.GLOBAL_POSITION_SCALE;

                    angleX = idxScenario.SmdLines[i].angleX;
                    angleY = idxScenario.SmdLines[i].angleY;
                    angleZ = idxScenario.SmdLines[i].angleZ;

                    scaleX = idxScenario.SmdLines[i].scaleX;
                    scaleY = idxScenario.SmdLines[i].scaleY;
                    scaleZ = idxScenario.SmdLines[i].scaleZ;

                }

                //----

                byte[] SMDLine = new byte[72];

                BitConverter.GetBytes(positionX).CopyTo(SMDLine, 0);
                BitConverter.GetBytes(positionY).CopyTo(SMDLine, 4);
                BitConverter.GetBytes(positionZ).CopyTo(SMDLine, 8);
                BitConverter.GetBytes(angleX).CopyTo(SMDLine, 12);
                BitConverter.GetBytes(angleY).CopyTo(SMDLine, 16);
                BitConverter.GetBytes(angleZ).CopyTo(SMDLine, 20);
                BitConverter.GetBytes(scaleX).CopyTo(SMDLine, 24);
                BitConverter.GetBytes(scaleY).CopyTo(SMDLine, 28);
                BitConverter.GetBytes(scaleZ).CopyTo(SMDLine, 32);
                BitConverter.GetBytes(BinID).CopyTo(SMDLine, 36);
                SMDLine[38] = FixedFF;
                SMDLine[39] = SmxID;
                BitConverter.GetBytes(objectStatus).CopyTo(SMDLine, 68);

                stream.Write(SMDLine, 0, 72);
            }


            //SmdLinePadding
            if (SmdLinePadding != 0)
            {
                stream.Write(new byte[SmdLinePadding], 0, (int)SmdLinePadding);
            }


            //---------------------------

            //boneLine
            SHARED_UHD_BIN.REPACK.FinalBoneLine[] boneLineArray = new SHARED_UHD_BIN.REPACK.FinalBoneLine[1];
            boneLineArray[0] = new SHARED_UHD_BIN.REPACK.FinalBoneLine();
            boneLineArray[0].BoneId = 0;
            boneLineArray[0].BoneParent = 0xFF; //-1;

            // PARTE DOS ARQUIVOS BINS

            // BLOCO DOS OFFSETS

            int BinCount = idxScenario.BinAmount; // variavel setada no metodo: RepackOBJ

            int offsetBlockCount = BinCount * 4;
            int CalcLines = offsetBlockCount / 0x10;
            CalcLines += 1;
            offsetBlockCount = CalcLines * 0x10;

            long StartOffset = stream.Position;

            stream.Write(new byte[offsetBlockCount], 0, offsetBlockCount);

            uint firtOffset = (uint)offsetBlockCount;

            stream.Position = StartOffset;
            stream.Write(BitConverter.GetBytes(firtOffset), 0, 4);

            stream.Position = StartOffset + firtOffset;

            //
            long tempOffset = StartOffset + firtOffset;
            uint InternalOffset = firtOffset;

            for (int i = 0; i < BinCount; i++)
            {
                long outOffset = tempOffset;

                if (finalBinList.ContainsKey(i))
                {
                    bool EnableColor = EnableVertexColors || CheckDinamicVertexColor.Check(finalBinList[i], EnableDinamicVertexColor);

                    SHARED_UHD_BIN.REPACK.BINmakeFile.MakeFile(stream, tempOffset, out outOffset, finalBinList[i],
                        boneLineArray, material, new byte[0][], true, false, false, false, EnableColor, IsPS4NS);
                }
                else 
                {
                    PutEmptyBin.Action(stream, tempOffset, out outOffset, IsPS4NS);
                }

                if (createBinFiles)
                {
                    try
                    {
                        //--salva em um arquivo
                        stream.Position = tempOffset;
                        int lenght = (int)(outOffset - tempOffset);
                        byte[] bin = new byte[lenght];
                        stream.Read(bin, 0, lenght);
                        File.WriteAllBytes(binPath + i.ToString("D4") + ".BIN", bin);
                        stream.Position = outOffset;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Error on write in file: " + i.ToString("D3") + ".BIN" + Environment.NewLine + ex.ToString());
                    }

                }

                uint FileLength = (uint)(outOffset - tempOffset);
                tempOffset = stream.Position;

                stream.Position = StartOffset + (i * 4);
                stream.Write(BitConverter.GetBytes(InternalOffset), 0, 4);

                stream.Position = tempOffset;

                InternalOffset += FileLength;
            }

            // tpl

            uint TplOffset = (uint)stream.Position;

            stream.Position = 8;
            stream.Write(BitConverter.GetBytes(TplOffset), 0, 4);
            stream.Position = TplOffset;

            byte[] Tpl_Padding = new byte[0x10];
            Tpl_Padding[0] = 0x10;
            stream.Write(Tpl_Padding, 0, 0x10);

            long startTplOffset = stream.Position;
            long endTplOffset = 0;

            //tpl file
            SHARED_UHD_BIN.REPACK.TPLmakeFile.MakeFile(uhdTPL, stream, startTplOffset, out endTplOffset, IsPS4NS);

            if (createBinFiles)
            {
                try
                {
                    //salva o tpl em arquivo
                    stream.Position = startTplOffset;
                    int tplLenght = (int)(endTplOffset - startTplOffset);
                    byte[] tpl = new byte[tplLenght];
                    stream.Read(tpl, 0, tplLenght);
                    File.WriteAllBytes(binPath + "TPL.TPL", tpl);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error on write in file: TPL.TPL" + Environment.NewLine + ex.ToString());
                }
            }

        
            stream.Close();

        }

    }
}
