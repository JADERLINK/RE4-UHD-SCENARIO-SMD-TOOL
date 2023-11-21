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

            IdxUhdScenario idxScenario = new IdxUhdScenario();

            int smdAmount = 0;

            //SMDAMOUNT
            try
            {
                string value = Utils.ReturnValidDecValue(pair["SMDAMOUNT"]);
                smdAmount = int.Parse(value, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture);
            }
            catch (Exception)
            {
            }

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

                var fileinfo = new FileInfo(value);
                idxScenario.SmdFileName = fileinfo.Name.Remove(fileinfo.Name.Length - fileinfo.Extension.Length, fileinfo.Extension.Length) + ".SMD";
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
            try
            {
                string value = Utils.ReturnValidDecValue(pair["BINAMOUNT"]);
                idxScenario.BinAmount = int.Parse(value, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture);
            }
            catch (Exception)
            {
            }

            //UseIdxMaterial
            try
            {
                idxScenario.UseIdxMaterial = bool.Parse(pair["USEIDXMATERIAL"].ToLower());
            }
            catch (Exception)
            {
            }

            //EnableVertexColor
            try
            {
                idxScenario.EnableVertexColor = bool.Parse(pair["ENABLEVERTEXCOLOR"].ToLower());
            }
            catch (Exception)
            {
            }

            //---

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

                try
                {
                    string value = Utils.ReturnValidFloatValue(pair[scaleXkey]);
                    smdline.scaleX = float.Parse(value, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture);
                }
                catch (Exception)
                {
                    smdline.scaleX = 1f;
                }
                try
                {
                    string value = Utils.ReturnValidFloatValue(pair[scaleYkey]);
                    smdline.scaleY = float.Parse(value, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture);
                }
                catch (Exception)
                {
                    smdline.scaleY = 1f;
                }
                try
                {
                    string value = Utils.ReturnValidFloatValue(pair[scaleZkey]);
                    smdline.scaleZ = float.Parse(value, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture);
                }
                catch (Exception)
                {
                    smdline.scaleZ = 1f;
                }


                try
                {
                    string value = Utils.ReturnValidFloatValue(pair[positionXkey]);
                    smdline.positionX = float.Parse(value, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture);
                }
                catch (Exception)
                {
                }
                try
                {
                    string value = Utils.ReturnValidFloatValue(pair[positionYkey]);
                    smdline.positionY = float.Parse(value, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture);
                }
                catch (Exception)
                {
                }
                try
                {
                    string value = Utils.ReturnValidFloatValue(pair[positionZkey]);
                    smdline.positionZ = float.Parse(value, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture);
                }
                catch (Exception)
                {
                }


                try
                {
                    string value = Utils.ReturnValidFloatValue(pair[angleXkey]);
                    smdline.angleX = float.Parse(value, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture);
                }
                catch (Exception)
                {
                }
                try
                {
                    string value = Utils.ReturnValidFloatValue(pair[angleYkey]);
                    smdline.angleY = float.Parse(value, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture);
                }
                catch (Exception)
                {
                }
                try
                {
                    string value = Utils.ReturnValidFloatValue(pair[angleZkey]);
                    smdline.angleZ = float.Parse(value, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture);
                }
                catch (Exception)
                {
                }

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

                try
                {
                    string value = Utils.ReturnValidDecValue(pair[binIDkey]);
                    extra.BinID = ushort.Parse(value, System.Globalization.NumberStyles.Integer, System.Globalization.CultureInfo.InvariantCulture);
                }
                catch (Exception)
                {
                    extra.BinID = 0;
                }

                try
                {
                    string value = Utils.ReturnValidDecValue(pair[smdIDkey]);
                    extra.SmxID = byte.Parse(value, System.Globalization.NumberStyles.Integer, System.Globalization.CultureInfo.InvariantCulture);
                }
                catch (Exception)
                {
                    extra.SmxID = 0;
                }

                try
                {
                    string value = Utils.ReturnValidHexValue(pair[fixedFFkey]);
                    extra.FixedFF = byte.Parse(value, System.Globalization.NumberStyles.HexNumber, System.Globalization.CultureInfo.InvariantCulture);
                }
                catch (Exception)
                {
                    extra.FixedFF = 0xFF;
                }

                try
                {
                    string value = Utils.ReturnValidHexValue(pair[unused1key]);
                    extra.unused1 = byte.Parse(value, System.Globalization.NumberStyles.HexNumber, System.Globalization.CultureInfo.InvariantCulture);
                }
                catch (Exception)
                {
                    extra.unused1 = 0;
                }

                try
                {
                    string value = Utils.ReturnValidHexValue(pair[unused2key]);
                    extra.unused2 = byte.Parse(value, System.Globalization.NumberStyles.HexNumber, System.Globalization.CultureInfo.InvariantCulture);
                }
                catch (Exception)
                {
                    extra.unused2 = 0;
                }

                try
                {
                    string value = Utils.ReturnValidHexValue(pair[unused3key]);
                    extra.unused3 = byte.Parse(value, System.Globalization.NumberStyles.HexNumber, System.Globalization.CultureInfo.InvariantCulture);
                }
                catch (Exception)
                {
                    extra.unused3 = 0;
                }

                try
                {
                    string value = Utils.ReturnValidHexValue(pair[unused4key]);
                    extra.unused4 = byte.Parse(value, System.Globalization.NumberStyles.HexNumber, System.Globalization.CultureInfo.InvariantCulture);
                }
                catch (Exception)
                {
                    extra.unused4 = 0;
                }

                try
                {
                    string value = Utils.ReturnValidHexValue(pair[unused5key]);
                    extra.unused5 = byte.Parse(value, System.Globalization.NumberStyles.HexNumber, System.Globalization.CultureInfo.InvariantCulture);
                }
                catch (Exception)
                {
                    extra.unused5 = 0;
                }

                try
                {
                    string value = Utils.ReturnValidHexValue(pair[unused6key]);
                    extra.unused6 = byte.Parse(value, System.Globalization.NumberStyles.HexNumber, System.Globalization.CultureInfo.InvariantCulture);
                }
                catch (Exception)
                {
                    extra.unused6 = 0;
                }

                try
                {
                    string value = Utils.ReturnValidHexValue(pair[unused7key]);
                    extra.unused7 = byte.Parse(value, System.Globalization.NumberStyles.HexNumber, System.Globalization.CultureInfo.InvariantCulture);
                }
                catch (Exception)
                {
                    extra.unused7 = 0;
                }

                try
                {
                    string value = Utils.ReturnValidHexValue(pair[objectStatuskey]);
                    extra.objectStatus = byte.Parse(value, System.Globalization.NumberStyles.HexNumber, System.Globalization.CultureInfo.InvariantCulture);
                }
                catch (Exception)
                {
                    extra.objectStatus = 0;
                }

                SmdLinesExtras[i] = extra;
                #endregion
            }

            // ----

            idxScenario.SmdAmount = smdAmount;
            idxScenario.SmdLines = smdLines;
            idxScenario.SmdLinesExtras = SmdLinesExtras;

            //---
            idxFile.Close();


            return idxScenario;
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
        public bool EnableVertexColor = false;

        // only in .idxuhdsmd
        public int BinAmount = 0;
        public SMDLineIdxExtras[] SmdLinesExtras;
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
