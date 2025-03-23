using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using SHARED_UHD_BIN.ALL;
using SimpleEndianBinaryIO;

namespace SHARED_UHD_SCENARIO_SMD.SCENARIO
{
    public static class MakeSMD_WithBinFolder
    {
        public static void CreateSMD(string baseDirectory, IdxUhdScenario idxScenario, Endianness endianness)
        {
            string binPath = baseDirectory + idxScenario.BinFolder + "\\";

            int SmdCount = idxScenario.SmdAmount;

            Stream stream = new FileInfo(baseDirectory + idxScenario.SmdFileName).Create();

            byte[] header = new byte[0x10];

            byte[] b_Magic = EndianBitConverter.GetBytes(idxScenario.Magic, Endianness.LittleEndian);
            header[0] = b_Magic[0];
            header[1] = b_Magic[1];

            byte[] b_SmdCount = EndianBitConverter.GetBytes((ushort)SmdCount, endianness);
            header[2] = b_SmdCount[0];
            header[3] = b_SmdCount[1];

            uint binStreamPosition = (uint)((SmdCount * 72) + 0x10);

            if (idxScenario.Magic == 0x0140)
            {
                uint amount = (uint)idxScenario.ExtraParameters.Length;
                binStreamPosition += ((amount +1) * 4);
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

            byte[] b_binStreamPosition = EndianBitConverter.GetBytes(binStreamPosition, endianness);
            header[4] = b_binStreamPosition[0];
            header[5] = b_binStreamPosition[1];
            header[6] = b_binStreamPosition[2];
            header[7] = b_binStreamPosition[3];

            stream.Write(header, 0, 0x10);

            if (idxScenario.Magic == 0x0140)
            {
                uint amount = (uint)idxScenario.ExtraParameters.Length;
                byte[] b_ExtraParameters = new byte[(amount + 1) * 4];
                EndianBitConverter.GetBytes(amount, endianness).CopyTo(b_ExtraParameters, 0);
                int tempcounter = 4;
                for (int i = 0; i < idxScenario.ExtraParameters.Length; i++)
                {
                    EndianBitConverter.GetBytes(idxScenario.ExtraParameters[i], endianness).CopyTo(b_ExtraParameters, tempcounter);
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

                ushort BinID = 0;
                byte FixedFF = 0xFF;
                byte SmxID = 0;
                uint objectStatus = 0;
                uint unused1 = 0;
                uint unused2 = 0;
                uint unused3 = 0;
                uint unused4 = 0;
                uint unused5 = 0;
                uint unused6 = 0;
                uint unused7 = 0;

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

                if (idxScenario.SmdLinesExtras.Length > i)
                {
                    BinID = idxScenario.SmdLinesExtras[i].BinID;
                    FixedFF = idxScenario.SmdLinesExtras[i].FixedFF;
                    SmxID = idxScenario.SmdLinesExtras[i].SmxID;
                    objectStatus = idxScenario.SmdLinesExtras[i].objectStatus;
                    unused1 = idxScenario.SmdLinesExtras[i].unused1;
                    unused2 = idxScenario.SmdLinesExtras[i].unused2;
                    unused3 = idxScenario.SmdLinesExtras[i].unused3;
                    unused4 = idxScenario.SmdLinesExtras[i].unused4;
                    unused5 = idxScenario.SmdLinesExtras[i].unused5;
                    unused6 = idxScenario.SmdLinesExtras[i].unused6;
                    unused7 = idxScenario.SmdLinesExtras[i].unused7;
                }


                //----

                byte[] SMDLine = new byte[72];

                EndianBitConverter.GetBytes(positionX, endianness).CopyTo(SMDLine, 0);
                EndianBitConverter.GetBytes(positionY, endianness).CopyTo(SMDLine, 4);
                EndianBitConverter.GetBytes(positionZ, endianness).CopyTo(SMDLine, 8);
                EndianBitConverter.GetBytes(angleX, endianness).CopyTo(SMDLine, 12);
                EndianBitConverter.GetBytes(angleY, endianness).CopyTo(SMDLine, 16);
                EndianBitConverter.GetBytes(angleZ, endianness).CopyTo(SMDLine, 20);
                EndianBitConverter.GetBytes(scaleX, endianness).CopyTo(SMDLine, 24);
                EndianBitConverter.GetBytes(scaleY, endianness).CopyTo(SMDLine, 28);
                EndianBitConverter.GetBytes(scaleZ, endianness).CopyTo(SMDLine, 32);
                EndianBitConverter.GetBytes(BinID, Endianness.LittleEndian).CopyTo(SMDLine, 36);
                SMDLine[38] = FixedFF;
                SMDLine[39] = SmxID;
                EndianBitConverter.GetBytes(unused1, endianness).CopyTo(SMDLine, 40);
                EndianBitConverter.GetBytes(unused2, endianness).CopyTo(SMDLine, 44);
                EndianBitConverter.GetBytes(unused3, endianness).CopyTo(SMDLine, 48);
                EndianBitConverter.GetBytes(unused4, endianness).CopyTo(SMDLine, 52);
                EndianBitConverter.GetBytes(unused5, endianness).CopyTo(SMDLine, 56);
                EndianBitConverter.GetBytes(unused6, endianness).CopyTo(SMDLine, 60);
                EndianBitConverter.GetBytes(unused7, endianness).CopyTo(SMDLine, 64);
                EndianBitConverter.GetBytes(objectStatus, endianness).CopyTo(SMDLine, 68);

                stream.Write(SMDLine, 0, 72);
            }

            //SmdLinePadding
            if (SmdLinePadding != 0)
            {
                stream.Write(new byte[SmdLinePadding], 0, (int)SmdLinePadding);
            }


            //---------------------------

            // PARTE DOS ARQUIVOS BINS

            // BLOCO DOS OFFSETS

            int BinCount = idxScenario.BinAmount;

            int offsetBlockCount = BinCount * 4;
            int CalcLines = offsetBlockCount / 0x10;
            CalcLines += 1;
            offsetBlockCount = CalcLines * 0x10;

            long StartOffset = stream.Position;

            stream.Write(new byte[offsetBlockCount], 0, offsetBlockCount);

            uint firtOffset = (uint)offsetBlockCount;

            stream.Position = StartOffset;
            stream.Write(EndianBitConverter.GetBytes(firtOffset, endianness), 0, 4);

            stream.Position = StartOffset + firtOffset;

            //
            long tempOffset = StartOffset + firtOffset;
            uint InternalOffset = firtOffset;

            for (int i = 0; i < BinCount; i++)
            {
                string binFilePath = binPath + i.ToString("D4") + ".BIN";

                uint FileLength = 0;
                byte[] bin = new byte[0];
                if (File.Exists(binFilePath))
                {
                    try
                    {
                        FileInfo info = new FileInfo(binFilePath);
                        var read = info.OpenRead();
                        bin = new byte[read.Length];
                        read.Read(bin, 0, bin.Length);
                        FileLength = (uint)read.Length; 
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Error: " + i.ToString("D4") + ".BIN, unable to read the file;" + Environment.NewLine + ex.ToString());
                    }
                }
                else 
                {
                    Console.WriteLine("Warning: " + i.ToString("D4") + ".BIN, file does not exist;");
                }

                stream.Position = tempOffset;
                stream.Write(bin, 0, bin.Length);

                tempOffset = stream.Position;

                stream.Position = StartOffset + (i * 4);
                stream.Write(EndianBitConverter.GetBytes(InternalOffset, endianness), 0, 4);

                stream.Position = tempOffset;

                InternalOffset += FileLength;
            }

            // tpl

            uint TplOffset = (uint)stream.Position;

            stream.Position = 8;
            stream.Write(EndianBitConverter.GetBytes(TplOffset, endianness), 0, 4);
            stream.Position = TplOffset;

            byte[] Tpl_Padding = new byte[0x10];
            EndianBitConverter.GetBytes((uint)0x10, endianness).CopyTo(Tpl_Padding, 0);
            stream.Write(Tpl_Padding, 0, 0x10);
            
            long startTplOffset = stream.Position;

            //tpl file
            string tplFilePath = binPath + "TPL.TPL";

            byte[] tpl = new byte[0];
            if (File.Exists(tplFilePath))
            {
                try
                {
                    FileInfo info = new FileInfo(tplFilePath);
                    var read = info.OpenRead();
                    tpl = new byte[read.Length];
                    read.Read(tpl, 0, tpl.Length);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error: TPL.TPL, unable to read the file;" + Environment.NewLine + ex.ToString());
                }
            }
            else
            {
                Console.WriteLine("Warning: TPL.TPL, file does not exist;");
            }

            stream.Position = startTplOffset;
            stream.Write(tpl, 0, tpl.Length);

            stream.Close();

        }




    }
}
