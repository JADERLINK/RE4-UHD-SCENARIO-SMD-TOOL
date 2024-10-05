using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Globalization;
using SHARED_UHD_BIN.EXTRACT;

namespace SHARED_UHD_BIN.ALL
{
   public static class IdxUhdTplLoad
    {
        public static UhdTPL Load(Stream stream)
        {
            UhdTPL tpl = new UhdTPL();

            StreamReader reader = new StreamReader(stream, Encoding.ASCII);

            Dictionary<int, TplInfo> tplDic = new Dictionary<int, TplInfo>();

            TplInfo tpltemp = new TplInfo();

            int AmountTpl = 0;

            while (!reader.EndOfStream)
            {
                string line = reader.ReadLine().Trim().ToUpperInvariant();

                if (line == null || line.Length == 0 || line.StartsWith("\\") || line.StartsWith("/") || line.StartsWith("#") || line.StartsWith(":"))
                {
                    continue;
                }

                else if (line.StartsWith("TPL")) 
                {
                    int tplID = -1;
                    try
                    {
                        tplID = int.Parse(Utils.ReturnValidDecValue(line), NumberStyles.Integer, CultureInfo.InvariantCulture);
                    }
                    catch (Exception)
                    {
                    }

                    if (tplID >= AmountTpl)
                    {
                        AmountTpl = tplID+1;
                    }

                    if (tplID != -1)
                    {
                        tpltemp = new TplInfo();
                        tpltemp.wrap_t = 1;
                        tpltemp.wrap_s = 1;
                        tpltemp.min_filter = 1;
                        tpltemp.mag_filter = 1;

                        if (!tplDic.ContainsKey(tplID))
                        {
                            tplDic.Add(tplID, tpltemp);
                        }

                    }
                    else 
                    {
                        tpltemp = new TplInfo();
                    }
                }

                else if (line.StartsWith("PACKID"))
                {
                    var split = line.Split(':');
                    if (split.Length >= 2)
                    {
                        try
                        {
                            tpltemp.PackID = uint.Parse(Utils.ReturnValidHexValue(split[1]), NumberStyles.HexNumber, CultureInfo.InvariantCulture);
                        }
                        catch (Exception)
                        {
                        }
                    }
                }

                else if (line.StartsWith("TEXTUREID"))
                {
                    var split = line.Split(':');
                    if (split.Length >= 2)
                    {
                        try
                        {
                            tpltemp.TextureID = uint.Parse(Utils.ReturnValidDecValue(split[1]), NumberStyles.Integer, CultureInfo.InvariantCulture);
                        }
                        catch (Exception)
                        {
                        }
                    }
                }

                else if (line.StartsWith("PIXELFORMATTYPE"))
                {
                    var split = line.Split(':');
                    if (split.Length >= 2)
                    {
                        try
                        {
                            tpltemp.PixelFormatType = uint.Parse(Utils.ReturnValidHexValue(split[1]), NumberStyles.HexNumber, CultureInfo.InvariantCulture);
                        }
                        catch (Exception)
                        {
                        }
                    }
                }

                else if (line.StartsWith("WIDTH"))
                {
                    var split = line.Split(':');
                    if (split.Length >= 2)
                    {
                        try
                        {
                            tpltemp.width = ushort.Parse(Utils.ReturnValidDecValue(split[1]), NumberStyles.Integer, CultureInfo.InvariantCulture);
                        }
                        catch (Exception)
                        {
                        }
                    }
                }

                else if (line.StartsWith("HEIGHT"))
                {
                    var split = line.Split(':');
                    if (split.Length >= 2)
                    {
                        try
                        {
                            tpltemp.height = ushort.Parse(Utils.ReturnValidDecValue(split[1]), NumberStyles.Integer, CultureInfo.InvariantCulture);
                        }
                        catch (Exception)
                        {
                        }
                    }
                }


                else if (line.StartsWith("WRAP_S"))
                {
                    var split = line.Split(':');
                    if (split.Length >= 2)
                    {
                        try
                        {
                            tpltemp.wrap_s = uint.Parse(Utils.ReturnValidDecValue(split[1]), NumberStyles.Integer, CultureInfo.InvariantCulture);
                        }
                        catch (Exception)
                        {
                        }
                    }
                }

                else if (line.StartsWith("WRAP_T"))
                {
                    var split = line.Split(':');
                    if (split.Length >= 2)
                    {
                        try
                        {
                            tpltemp.wrap_t = uint.Parse(Utils.ReturnValidDecValue(split[1]), NumberStyles.Integer, CultureInfo.InvariantCulture);
                        }
                        catch (Exception)
                        {
                        }
                    }
                }

                else if (line.StartsWith("MIN_FILTER"))
                {
                    var split = line.Split(':');
                    if (split.Length >= 2)
                    {
                        try
                        {
                            tpltemp.min_filter = uint.Parse(Utils.ReturnValidDecValue(split[1]), NumberStyles.Integer, CultureInfo.InvariantCulture);
                        }
                        catch (Exception)
                        {
                        }
                    }
                }

                else if (line.StartsWith("MAG_FILTER"))
                {
                    var split = line.Split(':');
                    if (split.Length >= 2)
                    {
                        try
                        {
                            tpltemp.mag_filter = uint.Parse(Utils.ReturnValidDecValue(split[1]), NumberStyles.Integer, CultureInfo.InvariantCulture);
                        }
                        catch (Exception)
                        {
                        }
                    }
                }

                else if (line.StartsWith("LOD_BIAS"))
                {
                    var split = line.Split(':');
                    if (split.Length >= 2)
                    {
                        try
                        {
                            tpltemp.lod_bias = float.Parse(Utils.ReturnValidFloatValue(split[1]), NumberStyles.Integer, CultureInfo.InvariantCulture);
                        }
                        catch (Exception)
                        {
                        }
                    }
                }

                else if (line.StartsWith("ENABLE_LOD"))
                {
                    var split = line.Split(':');
                    if (split.Length >= 2)
                    {
                        try
                        {
                            tpltemp.enable_lod = byte.Parse(Utils.ReturnValidDecValue(split[1]), NumberStyles.Integer, CultureInfo.InvariantCulture);
                        }
                        catch (Exception)
                        {
                        }
                    }
                }

                else if (line.StartsWith("MIN_LOD"))
                {
                    var split = line.Split(':');
                    if (split.Length >= 2)
                    {
                        try
                        {
                            tpltemp.min_lod = byte.Parse(Utils.ReturnValidDecValue(split[1]), NumberStyles.Integer, CultureInfo.InvariantCulture);
                        }
                        catch (Exception)
                        {
                        }
                    }
                }

                else if (line.StartsWith("MAX_LOD"))
                {
                    var split = line.Split(':');
                    if (split.Length >= 2)
                    {
                        try
                        {
                            tpltemp.max_lod = byte.Parse(Utils.ReturnValidDecValue(split[1]), NumberStyles.Integer, CultureInfo.InvariantCulture);
                        }
                        catch (Exception)
                        {
                        }
                    }
                }

                else if (line.StartsWith("IS_COMPRESSED"))
                {
                    var split = line.Split(':');
                    if (split.Length >= 2)
                    {
                        try
                        {
                            tpltemp.is_compressed = byte.Parse(Utils.ReturnValidDecValue(split[1]), NumberStyles.Integer, CultureInfo.InvariantCulture);
                        }
                        catch (Exception)
                        {
                        }
                    }
                }

            }

            if (AmountTpl > 255)
            {
                AmountTpl = 255;
            }


            TplInfo[] tplArray = new TplInfo[AmountTpl];

            for (int i = 0; i < AmountTpl; i++)
            {
                if (tplDic.ContainsKey(i))
                {
                    tplArray[i] = tplDic[i];
                }
                else
                {
                    tpltemp = new TplInfo();
                    tpltemp.wrap_t = 1;
                    tpltemp.wrap_s = 1;
                    tpltemp.min_filter = 1;
                    tpltemp.mag_filter = 1;
                    tplArray[i] = tpltemp;
                }
            }

            tpl.TplArray = tplArray;
            return tpl;
        }
    }
}
