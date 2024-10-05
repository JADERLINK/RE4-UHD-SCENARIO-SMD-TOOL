using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SHARED_UHD_BIN.REPACK
{
    public class FinalBoneLine
    {
        public FinalBoneLine()
        {
        }

        public FinalBoneLine(byte[] line)
        {
            Line = line;
        }

        public FinalBoneLine(sbyte boneId, sbyte boneParent, float posX, float posY, float posZ)
        {
            BoneId = boneId;
            BoneParent = boneParent;
            PosX = posX;
            PosY = posY;
            PosZ = posZ;
        }

        private byte[] _line = new byte[16];

        public byte[] Line
        {
            get
            {
                return _line.ToArray();
            }
            set
            {
                for (int i = 0; i < value.Length && i < _line.Length; i++)
                {
                    _line[i] = value[i];
                }
            }
        }

        public sbyte BoneId
        {
            get
            {
                return (sbyte)_line[0];
            }
            set
            {
                _line[0] = (byte)value;
            }
        }

        public sbyte BoneParent
        {
            get
            {
                return (sbyte)_line[1];
            }
            set
            {
                _line[1] = (byte)value;
            }
        }

        public float PosX
        {
            get
            {
                return BitConverter.ToSingle(_line, 0x4);
            }
            set
            {
                var bvalue = BitConverter.GetBytes(value);
                bvalue.CopyTo(_line, 0x4);
            }
        }

        public float PosY
        {
            get
            {
                return BitConverter.ToSingle(_line, 0x8);
            }
            set
            {
                var bvalue = BitConverter.GetBytes(value);
                bvalue.CopyTo(_line, 0x8);
            }
        }

        public float PosZ
        {
            get
            {
                return BitConverter.ToSingle(_line, 0xC);
            }
            set
            {
                var bvalue = BitConverter.GetBytes(value);
                bvalue.CopyTo(_line, 0xC);
            }
        }

    }
}
