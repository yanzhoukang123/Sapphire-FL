using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Azure.EthernetCommLib
{
    //新增加的字段，必须添加到最后，不允许在起始和中间位置添加
    //Newly added fields must be added to the end and are not allowed to be added at the beginning or middle positions
    [Serializable]
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1, Size = 256)]  // aligned by 1 byte
    public struct AvocadoDeviceProperties
    {
        public float LogicalHomeX;
        public float LogicalHomeY;
        public float OpticalLR1Distance;
        public int PixelOffsetR1;
        public float OpticalLR2Distance;
        public int PixelOffsetR2;
        public int PixelOffsetDxCHR1;
        public int PixelOffsetDyCHR1;
        public int PixelOffsetDxCHR2;
        public int PixelOffsetDyCHR2;
        public float ZFocusPosition;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
        public byte[] SysSN;
        public int PixelOffsetDxCHL;
        public int PixelOffsetDyCHL;
        public float XEncoderSubdivision;
        public int FanReserveTemperature;
        public int FanSwitchInterval;
        public float LCoefficient;
        public float R1Coefficient;
        public float R2Coefficient;
        public float R2532Coefficient;
        public ushort ShellFanDefaultSpeed;  //Default speed when the device is turned on
        public ushort LaserFanDefaultSpeed;  //reserve
        public float L375Coefficient;
        public ushort CH1AlertWarningSwitch;  //CH1 Ambient Temperature  Alert
        public float CH1WarningTemperature;
        public byte VersionExtension;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 129)]   // sizeconst = total(256) - used(32+22*4+2*3)+1
        public byte[] ReservedBytes;
    }
}
