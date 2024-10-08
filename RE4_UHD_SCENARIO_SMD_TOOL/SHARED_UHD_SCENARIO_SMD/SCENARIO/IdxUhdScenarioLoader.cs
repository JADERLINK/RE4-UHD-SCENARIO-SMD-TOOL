﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using SHARED_UHD_BIN.ALL;
using SHARED_UHD_BIN.EXTRACT;

namespace SHARED_UHD_SCENARIO_SMD.SCENARIO
{
    public class IdxUhdScenarioLoader
    {
        public static IdxUhdScenario Loader(StreamReader idxFile)
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

            IdxUhdScenario idxScenario = new IdxUhdScenario();

            int smdAmount = GetIntDec(ref pair, "SMDAMOUNT", 0); ;

            //SMDFILENAME
            try
            {
                string value = pair["SMDFILENAME"].Trim();
                value = value.Replace('/', '\\')
              .Replace(":", "").Replace("*", "").Replace("\"", "").Replace("|", "")
              .Replace("<", "").Replace(">", "").Replace("?", "").Replace(" ", "_");

                value = value.Split('\\').Last();

                if (value.Length == 0)
                {
                    value = "null";
                }

                idxScenario.SmdFileName = Path.GetFileNameWithoutExtension(value) + ".SMD";
            }
            catch (Exception)
            {
            }

            //BinFolder
            try
            {
                string value = pair["BINFOLDER"].Trim();
                value = value.Replace('/', '\\')
              .Replace(":", "").Replace("*", "").Replace("\"", "").Replace("|", "")
              .Replace("<", "").Replace(">", "").Replace("?", "");

                value = value.Split('\\').Last();

                if (value.Length == 0)
                {
                    value = "null";
                }
                idxScenario.BinFolder = value;
            }
            catch (Exception)
            {
            }

            //BinAmount
            idxScenario.BinAmount = GetIntDec(ref pair, "BINAMOUNT", 0);


            idxScenario.UseIdxMaterial = GetBool(ref pair, "USEIDXMATERIAL");
            idxScenario.EnableVertexColor = GetBool(ref pair, "ENABLEVERTEXCOLOR");
            idxScenario.UseIdxUhdTpl = GetBool(ref pair, "USEIDXUHDTPL");
            idxScenario.EnableDinamicVertexColor = GetBool(ref pair, "ENABLEDINAMICVERTEXCOLOR");


            //Magic
            idxScenario.Magic = GetUshortHex(ref pair, "MAGIC", 0x0040);

            int ExtraParameterAmount = GetIntDec(ref pair, "EXTRAPARAMETERAMOUNT", 0);

            uint[] ExtraParameters = new uint[ExtraParameterAmount];

            for (int i = 0; i < ExtraParameterAmount; i++)
            {
                ExtraParameters[i] = GetUintDec(ref pair, "EXTRAPARAMETER" + i, 0);
            }

            //-------

            SMDLineIdx[] smdLines = new SMDLineIdx[smdAmount];
            SMDLineIdxExtras[] SmdLinesExtras = new SMDLineIdxExtras[smdAmount];

            for (int i = 0; i < smdAmount; i++)
            {
                #region SMDLineIdx
                string scaleXkey = i.ToString("D3") + "_SCALEX";
                string scaleYkey = i.ToString("D3") + "_SCALEY";
                string scaleZkey = i.ToString("D3") + "_SCALEZ";

                string positionXkey = i.ToString("D3") + "_POSITIONX";
                string positionYkey = i.ToString("D3") + "_POSITIONY";
                string positionZkey = i.ToString("D3") + "_POSITIONZ";

                string angleXkey = i.ToString("D3") + "_ANGLEX";
                string angleYkey = i.ToString("D3") + "_ANGLEY";
                string angleZkey = i.ToString("D3") + "_ANGLEZ";

                SMDLineIdx smdline = new SMDLineIdx();

                smdline.scaleX = GetFloat(ref pair, scaleXkey, 1f);
                smdline.scaleY = GetFloat(ref pair, scaleYkey, 1f);
                smdline.scaleZ = GetFloat(ref pair, scaleZkey, 1f);
                smdline.positionX = GetFloat(ref pair, positionXkey, 0f);
                smdline.positionY = GetFloat(ref pair, positionYkey, 0f);
                smdline.positionZ = GetFloat(ref pair, positionZkey, 0f);
                smdline.angleX = GetFloat(ref pair, angleXkey, 0f);
                smdline.angleY = GetFloat(ref pair, angleYkey, 0f);
                smdline.angleZ = GetFloat(ref pair, angleZkey, 0f);

                smdLines[i] = smdline;
                #endregion

                #region SMDLineIdxExtras

                string binIDkey = i.ToString("D3") + "_BINID";
                string fixedFFkey = i.ToString("D3") + "_FIXEDFF";
                string smdIDkey = i.ToString("D3") + "_SMXID";
                string unused1key = i.ToString("D3") + "_UNUSED1";
                string unused2key = i.ToString("D3") + "_UNUSED2";
                string unused3key = i.ToString("D3") + "_UNUSED3";
                string unused4key = i.ToString("D3") + "_UNUSED4";
                string unused5key = i.ToString("D3") + "_UNUSED5";
                string unused6key = i.ToString("D3") + "_UNUSED6";
                string unused7key = i.ToString("D3") + "_UNUSED7";
                string objectStatuskey = i.ToString("D3") + "_OBJECTSTATUS";

                SMDLineIdxExtras extra = new SMDLineIdxExtras();

                extra.BinID = GetUshortDec(ref pair, binIDkey, 0);
                extra.SmxID = GetByteDec(ref pair, smdIDkey, 0);
                extra.FixedFF = GetByteHex(ref pair, fixedFFkey, 0xFF);
                extra.unused1 = GetUintHex(ref pair, unused1key, 0);
                extra.unused2 = GetUintHex(ref pair, unused2key, 0);
                extra.unused3 = GetUintHex(ref pair, unused3key, 0);
                extra.unused4 = GetUintHex(ref pair, unused4key, 0);
                extra.unused5 = GetUintHex(ref pair, unused5key, 0);
                extra.unused6 = GetUintHex(ref pair, unused6key, 0);
                extra.unused7 = GetUintHex(ref pair, unused7key, 0);
                extra.objectStatus = GetUintHex(ref pair, objectStatuskey, 0);

                SmdLinesExtras[i] = extra;
                #endregion
            }

            // ----

            idxScenario.SmdAmount = smdAmount;
            idxScenario.SmdLines = smdLines;
            idxScenario.SmdLinesExtras = SmdLinesExtras;
            idxScenario.ExtraParameters = ExtraParameters;

            //---
            idxFile.Close();


            return idxScenario;
        }

        public static float GetFloat(ref Dictionary<string, string> pair, string key, float DefaultValue) 
        {
            float res = DefaultValue;

            if (pair.ContainsKey(key))
            {
                try
                {
                    string value = Utils.ReturnValidFloatValue(pair[key]);
                    res = float.Parse(value, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture);
                }
                catch (Exception)
                {
                    res = DefaultValue;
                }
            }

            return res;
        }

        public static ushort GetUshortDec(ref Dictionary<string, string> pair, string key, ushort DefaultValue) 
        {
            ushort res = DefaultValue;

            if (pair.ContainsKey(key))
            {
                try
                {
                    string value = Utils.ReturnValidDecValue(pair[key]);
                    res = ushort.Parse(value, System.Globalization.NumberStyles.Integer, System.Globalization.CultureInfo.InvariantCulture);
                }
                catch (Exception)
                {
                    res = DefaultValue;
                }
            }

            return res;
        }

        public static ushort GetUshortHex(ref Dictionary<string, string> pair, string key, ushort DefaultValue)
        {
            ushort res = DefaultValue;

            if (pair.ContainsKey(key))
            {
                try
                {
                    string value = Utils.ReturnValidHexValue(pair[key]);
                    res = ushort.Parse(value, System.Globalization.NumberStyles.HexNumber, System.Globalization.CultureInfo.InvariantCulture);
                }
                catch (Exception)
                {
                    res = DefaultValue;
                }
            }

            return res;
        }

        public static byte GetByteDec(ref Dictionary<string, string> pair, string key, byte DefaultValue) 
        {
            byte res = DefaultValue;

            if (pair.ContainsKey(key))
            {
                try
                {
                    string value = Utils.ReturnValidDecValue(pair[key]);
                    res = byte.Parse(value, System.Globalization.NumberStyles.Integer, System.Globalization.CultureInfo.InvariantCulture);
                }
                catch (Exception)
                {
                    res = DefaultValue;
                }
            }

            return res;
        }

        public static byte GetByteHex(ref Dictionary<string, string> pair, string key, byte DefaultValue)
        {
            byte res = DefaultValue;

            if (pair.ContainsKey(key))
            {
                try
                {
                    string value = Utils.ReturnValidHexValue(pair[key]);
                    res = byte.Parse(value, System.Globalization.NumberStyles.HexNumber, System.Globalization.CultureInfo.InvariantCulture);
                }
                catch (Exception)
                {
                    res = DefaultValue;
                }
            }

            return res;
        }

        public static uint GetUintHex(ref Dictionary<string, string> pair, string key, uint DefaultValue) 
        {
            uint res = DefaultValue;

            if (pair.ContainsKey(key))
            {
                try
                {
                    string value = Utils.ReturnValidHexValue(pair[key]);
                    res = uint.Parse(value, System.Globalization.NumberStyles.HexNumber, System.Globalization.CultureInfo.InvariantCulture);
                }
                catch (Exception)
                {
                    res = DefaultValue;
                }
            }

            return res;
        }

        public static uint GetUintDec(ref Dictionary<string, string> pair, string key, uint DefaultValue)
        {
            uint res = DefaultValue;

            if (pair.ContainsKey(key))
            {
                try
                {
                    string value = Utils.ReturnValidDecValue(pair[key]);
                    res = uint.Parse(value, System.Globalization.NumberStyles.Integer, System.Globalization.CultureInfo.InvariantCulture);
                }
                catch (Exception)
                {
                    res = DefaultValue;
                }
            }

            return res;
        }

        public static int GetIntDec(ref Dictionary<string, string> pair, string key, int DefaultValue)
        {
            int res = DefaultValue;

            if (pair.ContainsKey(key))
            {
                try
                {
                    string value = Utils.ReturnValidDecValue(pair[key]);
                    res = int.Parse(value, System.Globalization.NumberStyles.Integer, System.Globalization.CultureInfo.InvariantCulture);
                }
                catch (Exception)
                {
                    res = DefaultValue;
                }
            }

            return res;
        }

        public static bool GetBool(ref Dictionary<string, string> pair, string key)
        {
            bool res = false;

            if (pair.ContainsKey(key))
            {
                try
                {
                    res = bool.Parse(pair[key].Trim().ToLower());
                }
                catch (Exception)
                {
                    res = false;
                }
            }

            return res;
        }
    }



    public class IdxUhdScenario
    {
        public int SmdAmount = 0;

        public string SmdFileName = "null.smd";

        public string BinFolder = "null";

        public SMDLineIdx[] SmdLines;

        // only in .idxuhdscenario
        public bool UseIdxMaterial = false;
        public bool UseIdxUhdTpl = false;
        public bool EnableVertexColor = false;
        public bool EnableDinamicVertexColor = false;


        // only in .idxuhdsmd
        public int BinAmount = 0;
        public SMDLineIdxExtras[] SmdLinesExtras;

        //new in B.1.0.0.1
        public ushort Magic = 0x0040;
        public uint[] ExtraParameters = new uint[0];
    }

    public class SMDLineIdx
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
    }

    public class SMDLineIdxExtras
    {
        // only in .idxuhdsmd
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
