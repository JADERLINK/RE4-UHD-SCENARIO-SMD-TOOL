using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using RE4_UHD_BIN_TOOL.ALL;
using RE4_UHD_BIN_TOOL.EXTRACT;

namespace RE4_UHD_SCENARIO_SMD_TOOL.SCENARIO
{
    public static class UhdSmdExtract
    {

        public static SMDLine[] Extract(Stream fileStream, out Dictionary<int, UhdBIN> uhdBinDic, out UhdTPL uhdTpl, string baseSubDirectory, bool extractBins)
        {
            if (extractBins)
            {
                Directory.CreateDirectory(baseSubDirectory);
            }

            BinaryReader br = new BinaryReader(fileStream);

            ushort magic = br.ReadUInt16();

            ushort SmdLinesAmount = br.ReadUInt16();

            uint offsetBin = br.ReadUInt32(); // para offset dos bins (são relativos)
            uint offsetTpl = br.ReadUInt32(); // tpl offset relativo (aponta para um offset, dele que vai pro tpl)
            uint offsetNone = br.ReadUInt32(); // no gc é o fim do arquivo, no de pc fica sendo o primeiro bin.

            // 72 bytes por linha smd

            int BinRealCount = 0;

            SMDLine[] SMDLines = new SMDLine[SmdLinesAmount];

            for (int i = 0; i < SmdLinesAmount; i++)
            {
                SMDLine smdLine = new SMDLine();

                smdLine.positionX = br.ReadSingle();
                smdLine.positionY = br.ReadSingle();
                smdLine.positionZ = br.ReadSingle();
                smdLine.angleX = br.ReadSingle();
                smdLine.angleY = br.ReadSingle();
                smdLine.angleZ = br.ReadSingle();
                smdLine.scaleX = br.ReadSingle();
                smdLine.scaleY = br.ReadSingle();
                smdLine.scaleZ = br.ReadSingle();
                smdLine.BinID = br.ReadUInt16();
                smdLine.FixedFF = br.ReadByte();
                smdLine.SmxID = br.ReadByte();
                smdLine.unused1 = br.ReadUInt32();
                smdLine.unused2 = br.ReadUInt32();
                smdLine.unused3 = br.ReadUInt32();
                smdLine.unused4 = br.ReadUInt32();
                smdLine.unused5 = br.ReadUInt32();
                smdLine.unused6 = br.ReadUInt32();
                smdLine.unused7 = br.ReadUInt32();
                smdLine.objectStatus = br.ReadUInt32();
               
                SMDLines[i] = smdLine;

                if (BinRealCount <= smdLine.BinID)
                {
                    BinRealCount = smdLine.BinID + 1;
                }
            }

            //------------------------------
            //BIN offset

            br.BaseStream.Position = offsetBin;

            List<uint> binOffsets = new List<uint>();

            for (int i = 0; i < BinRealCount && br.BaseStream.Position < br.BaseStream.Length; i++)
            {
                uint lastoffset = br.ReadUInt32();
                if (lastoffset == 0)
                {
                    break;
                }
                binOffsets.Add(lastoffset + offsetBin);
            }

            //get bin

            Dictionary<int, UhdBIN> UhdBINs = new Dictionary<int, UhdBIN>();

            for (int i = 0; i < binOffsets.Count; i++)
            {
                long endOffset = binOffsets[i];
                try
                {
                    var uhdBin = UhdBinDecoder.Decoder(fileStream, binOffsets[i], out endOffset);
                    UhdBINs.Add(i, uhdBin);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error on Read Bin in Smd: " + i.ToString("D3") + Environment.NewLine + ex.ToString());
                }

                if (extractBins)
                {
                    try
                    {
                        //le os bytes do bin e grava em um arquivo
                        fileStream.Position = binOffsets[i];
                        long lenght = endOffset - binOffsets[i];

                        byte[] binArray = new byte[lenght];
                        fileStream.Read(binArray, 0, (int)lenght);

                        string binPath = baseSubDirectory + i.ToString("D4") + ".BIN";
                        File.WriteAllBytes(binPath, binArray);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Error on write in file: " + i.ToString("D3") + ".BIN" + Environment.NewLine + ex.ToString());
                    }
                }

            }

            uhdBinDic = UhdBINs;

            //------------------------------
            //TPL offset

            br.BaseStream.Position = offsetTpl;

            uint tplOffset = br.ReadUInt32();
            tplOffset = tplOffset + offsetTpl;

            long tplEndOffset = tplOffset;
            try
            {
                uhdTpl = UhdTplDecoder.Decoder(fileStream, tplOffset, out tplEndOffset);
            }
            catch (Exception ex)
            {
                uhdTpl = null;
                Console.WriteLine("Error on Read: Tpl in Smd"  + Environment.NewLine + ex.ToString());
            }
           

            if (extractBins)
            {
                try
                {
                    //le os bytes do tpl e grava em um arquivo
                    fileStream.Position = tplOffset;
                    long tplLenght = tplEndOffset - tplOffset;

                    byte[] tplArray = new byte[tplLenght];
                    fileStream.Read(tplArray, 0, (int)tplLenght);

                    string tplPath = baseSubDirectory + "TPL.TPL";
                    File.WriteAllBytes(tplPath, tplArray);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error on write in file: TPL.TPL" + Environment.NewLine + ex.ToString());
                }

            }
            

            br.Close();
            return SMDLines;
        }

    }


    public class SMDLine
    {
        public float positionX;
        public float positionY;
        public float positionZ;
        public float angleX;
        public float angleY;
        public float angleZ;
        public float scaleX;
        public float scaleY;
        public float scaleZ;

        public ushort BinID;
        public byte FixedFF;
        public byte SmxID;
        public uint unused1;
        public uint unused2;
        public uint unused3;
        public uint unused4;
        public uint unused5;
        public uint unused6;
        public uint unused7;
        public uint objectStatus;
    }

}