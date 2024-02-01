using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Azure.APDCalibrationBench
{
    public class APDCommProtocol
    {
        #region delegates statement
        public delegate void CalibrationCommUpdateHandler();
        public event CalibrationCommUpdateHandler UpdateCommOutput;

        public delegate void CalibrationCommTimeOutHandler(CommErrorInfo errorInfo);
        public static event CalibrationCommTimeOutHandler CommTimeOut;
        #endregion

        #region Private Data
        private byte _FrameHeader;
        private DeviceAddressType _DeviceAddress;
        private FunctionCodeType _FunctionCode;
        private APDRegsAddressType _RegAddress;
        private byte _ByteReserved = 0x00;
        private int _DataField;
        private byte _FrameEnd;

        private static CommStatusType _CommStatus;
        private static System.Timers.Timer _Timer;
        private static DeviceAddressType _staticDeviceAddress;
        private static FunctionCodeType _staticFunctionCode;
        private static APDRegsAddressType _staticRegAddress;
        private static byte[] _staticDataField;

        private CommOutputStruct _CommOutputChA = null;
        private CommOutputStruct _CommOutputChB = null;

        #endregion
        public class CommOutputStruct
        {
            //public string CurrentIVNum=null;
            //public int? CurrentIVType = null;
            public int? CurrentAPDGain = null;
            public int? CurrentIVType = null;
            public int? CurrentPMTGain = null;
            public int? CurrentPGA = null;
            public double? APDHighVoltage = null;
            public double? APDTemperature = null;
            public double? TemperatureAtCalibrationCHA = null;
            public double? TECTemperature = null;
            public double? APDOutput = null;
            public int LaserPower = 0;
            public double Tempscale = 0;
        }

        #region Constructor
        public APDCommProtocol(DeviceAddressType deviceAddress)
        {
            _FrameHeader = 0x3a;
            DeviceAddress = deviceAddress;
            FunctionCode = FunctionCodeType.ReadReg;
            _DataField = 0;
            _FrameEnd = 0x3b;
            _CommOutputChA = new CommOutputStruct();
            _CommOutputChB = new CommOutputStruct();
        }
        static APDCommProtocol()
        {
            _CommStatus = CommStatusType.Idle;

            _staticDataField = new byte[4];

            _Timer = new System.Timers.Timer();
            _Timer.Elapsed += _Timer_Elapsed;
            _Timer.AutoReset = false;
            _Timer.Interval = 500;
            _Timer.Enabled = false;
        }
        #endregion Constructor
        #region Timer process function

        public class CommErrorInfo
        {
            public DeviceAddressType DeviceAddress { get { return _staticDeviceAddress; } }
            public FunctionCodeType FunctionCode { get { return _staticFunctionCode; } }
            public APDRegsAddressType RegAddress { get { return _staticRegAddress; } }
            public byte[] DataField { get { return _staticDataField; } }
            public override string ToString()
            {
                string _error = string.Format("地址：0x{0:X2}" + "\n", (byte)DeviceAddress);
                _error += string.Format("功能码：0x{0:X2}" + "\n", (byte)FunctionCode);
                _error += string.Format("寄存器：0x{0:X2}", (byte)RegAddress);
                return _error;
            }
        }
        public static byte[] ResumeErrorFrame()
        {
            byte[] _returnBytes = new byte[10];
            _returnBytes[0] = 0x3a;
            _returnBytes[1] = (byte)_staticDeviceAddress;
            _returnBytes[2] = (byte)_staticFunctionCode;
            _returnBytes[3] = (byte)_staticRegAddress;  // reg address
            _returnBytes[4] = 0x00;                     // reserved byte field
            _returnBytes[5] = _staticDataField[0];      // data field
            _returnBytes[6] = _staticDataField[1];      // data field
            _returnBytes[7] = _staticDataField[2];      // data field
            _returnBytes[8] = _staticDataField[3];      // data field
            _returnBytes[9] = 0x3b;

            return _returnBytes;
        }
        private static void _Timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            if (CommStatus == CommStatusType.Hearing)
            {
                CommStatus = CommStatusType.TimeOut;
            }
        }
        #endregion Timer process function
        #region Public Properties
        public byte FrameHeader
        {
            get
            {
                return _FrameHeader;
            }
        }
        public DeviceAddressType DeviceAddress
        {
            get
            {
                return _DeviceAddress;
            }
            set
            {
                _DeviceAddress = value;
            }
        }
        public FunctionCodeType FunctionCode
        {
            get
            {
                return _FunctionCode;
            }
            private set
            {
                _FunctionCode = value;
                _staticFunctionCode = _FunctionCode;
            }
        }
        public APDRegsAddressType RegAddress
        {
            get
            {
                return _RegAddress;
            }
            private set
            {
                _RegAddress = value;
                _staticRegAddress = _RegAddress;
            }
        }
        public int DataField
        {
            get
            {
                return _DataField;
            }
        }
        public static CommStatusType CommStatus
        {
            get
            {
                return _CommStatus;
            }
            set
            {
                _CommStatus = value;
                if (_CommStatus == CommStatusType.Hearing)
                {
                    _Timer.Enabled = true;
                }
                else if (_CommStatus == CommStatusType.TimeOut)
                {
                    _Timer.Enabled = false;

                    CommErrorInfo _TimeOutError = new CommErrorInfo();
                    CommTimeOut(_TimeOutError);
                }
                else
                {
                    _Timer.Enabled = false;
                }
            }
        }

        public CommOutputStruct CommOutputChA
        {
            get
            {
                return _CommOutputChA;
            }
        }
        //public CommOutputStruct CommOutputChB
        //{
        //    get
        //    {
        //        return _CommOutputChB;
        //    }
        //}

        #endregion Public Properties

        #region Frame Enumeration
        public enum DeviceAddressType
        {
            TestBench = 0xf0,
            APDModuleA = 0x02,
            //APDModuleB = 0xf2,
            LaserModuleA = 0x01,
            //LaserModuleB = 0xf5,
            // LedBar = 0xf4
        }
        public enum FunctionCodeType
        {
            WriteReg = 0x01,
            ReadReg = 0x02
        }
        public enum APDRegsAddressType
        {
            IVType = 0x01,
            IVNum = 0x02,
            APDGainCHA = 0x04,
            PMTGainCHA = 0x10,
            LaserCHA = 0x03,
            //APDGainCHB = 0x03,
            //LaserCHB = 0x04,
            //TemperatureVoltCHA = 0x05,
            //TemperatureVOltCHB = 0x06,
            TemperatureCHA = 0x05,

            //TemperatureCHB = 0x08,
            TemperatureAtCalibrationCHA = 0x07,
            //HighVoltCalibrationCoeffCHA = 0x0c,
            //CalibrationVoltAtGain10CHA = 0x0d,
            //CalibrationVoltAtGain15CHA = 0x0e,
            //CalibrationVoltAtGain25CHA = 0x0f,
            CalibrationVoltAtGain50CHA = 0x08,
            CalibrationVoltAtGain100CHA = 0x09,
            CalibrationVoltAtGain150CHA = 0x0a,
            CalibrationVoltAtGain200CHA = 0x0b,
            CalibrationVoltAtGain250CHA = 0x0c,
            CalibrationVoltAtGain300CHA = 0x0d,
            CalibrationVoltAtGain400CHA = 0x0e,
            CalibrationVoltAtGain500CHA = 0x0f,
            //HighVoltCalibrationCoeffCHB = 0x18,
            //CalibrationVoltAtGain10CHB = 0x19,
            //CalibrationVoltAtGain15CHB = 0x1a,
            //CalibrationVoltAtGain25CHB = 0x1b,
            //CalibrationVoltAtGain50CHB = 0x1c,
            //CalibrationVoltAtGain100CHB = 0x1d,
            //CalibrationVoltAtGain150CHB = 0x1e,
            //CalibrationVoltAtGain200CHB = 0x1f,
            //CalibrationVoltAtGain250CHB = 0x20,
            //CalibrationVoltAtGain300CHB = 0x21,
            //CalibrationVoltAtGain400CHB = 0x22,
            //CalibrationVoltAtGain500CHB = 0x23,
            PGACHA = 0x03,
            //PGACHB = 0x25,
            //WaveLengthCHA = 0x26,
            //WaveLengthCHB = 0x27,
            //TemperatureAtCalibrationCHB = 0x28,
            HighVoltageCHA = 0x06,
            //HighVoltageCHB = 0x2a,
            //BiasVoltageCHA = 0x2d,
            // BiasVoltageCHB = 0x2e,

            APDValueModuleA = 0x55,
            //APDValueModuleB = 0x56
        }
        public enum CommStatusType
        {
            Idle,
            Hearing,        // request is sent, waiting for response
            Processing,     // got response, processing
            TimeOut,        // waiting response time out
        }
        public enum APDChannelType
        {
            CHA = 1,
            //CHB = 2,
        }
        #endregion Frame Enumeration

        #region Public Functions
        private byte[] PrepareRequestBytes()
        {
            byte[] _returnBytes = new byte[10];
            _returnBytes[0] = _FrameHeader;
            _returnBytes[1] = (byte)_DeviceAddress;
            _returnBytes[2] = (byte)_FunctionCode;
            _returnBytes[3] = (byte)_RegAddress;  // reg address
            _returnBytes[4] = _ByteReserved;// reserved byte field
            _returnBytes[5] = 0x00;         // data field
            _returnBytes[6] = 0x00;         // data field
            _returnBytes[7] = 0x00;         // data field
            _returnBytes[8] = 0x00;         // data field
            _returnBytes[9] = _FrameEnd;

            _staticDeviceAddress = _DeviceAddress;
            _staticDataField[0] = _returnBytes[5];
            _staticDataField[1] = _returnBytes[6];
            _staticDataField[2] = _returnBytes[7];
            _staticDataField[3] = _returnBytes[8];

            return _returnBytes;
        }
        private byte[] PrepareRequestBytes(int number)
        {
            byte[] _tempDataField = new byte[4];
            _tempDataField = BitConverter.GetBytes(number);

            byte[] _returnBytes = new byte[10];
            _returnBytes[0] = _FrameHeader;
            _returnBytes[1] = (byte)_DeviceAddress;
            _returnBytes[2] = (byte)_FunctionCode;
            _returnBytes[3] = (byte)_RegAddress;    // reg address
            _returnBytes[4] = _ByteReserved;        // reserved byte field
            _returnBytes[5] = _tempDataField[3];    // data field
            _returnBytes[6] = _tempDataField[2];    // data field
            _returnBytes[7] = _tempDataField[1];    // data field
            _returnBytes[8] = _tempDataField[0];    // data field
            _returnBytes[9] = _FrameEnd;

            _staticDeviceAddress = _DeviceAddress;
            _staticDataField[0] = _returnBytes[5];
            _staticDataField[1] = _returnBytes[6];
            _staticDataField[2] = _returnBytes[7];
            _staticDataField[3] = _returnBytes[8];

            return _returnBytes;
        }
        public byte[] VisitAPDGain(APDChannelType channel, FunctionCodeType writeOrRead, int? number = null)
        {
            byte[] _returnBytes = new byte[10];
            FunctionCode = writeOrRead;
            if (channel == APDChannelType.CHA)
            {
                RegAddress = APDRegsAddressType.APDGainCHA;
            }
            if (number == null)
            {
                _returnBytes = PrepareRequestBytes();
            }
            else
            {
                _returnBytes = PrepareRequestBytes((int)number);
            }
            return _returnBytes;
        }
        public byte[] VisitPGA(APDChannelType channel, FunctionCodeType writeOrRead, int? number = null)
        {
            byte[] _returnBytes = new byte[10];
            FunctionCode = writeOrRead;
            if (channel == APDChannelType.CHA)
            {
                RegAddress = APDRegsAddressType.PGACHA;
            }
            if (number == null)
            {
                _returnBytes = PrepareRequestBytes();
            }
            else
            {
                int _pgaNumber = (int)Math.Log((int)number, 2);
                _returnBytes = PrepareRequestBytes(_pgaNumber);
            }
            return _returnBytes;
        }
        public byte[] ReadHighVoltage(APDChannelType channel)
        {
            FunctionCode = FunctionCodeType.ReadReg;
            if (channel == APDChannelType.CHA)
            {
                RegAddress = APDRegsAddressType.HighVoltageCHA;
            }
            byte[] _returnBytes = PrepareRequestBytes();
            return _returnBytes;
        }
        //public byte[] ReadAPDTempertureVolt(APDChannelType channel)
        //{
        //    FunctionCode = FunctionCodeType.ReadReg;
        //    if (channel == APDChannelType.CHA)
        //    {
        //        RegAddress = APDRegsAddressType.TemperatureVoltCHA;
        //    }
        //    //else if (channel == APDChannelType.CHB)
        //    //{
        //    //    RegAddress = APDRegsAddressType.TemperatureVOltCHB;
        //    //}
        //    byte[] _returnBytes = PrepareRequestBytes();
        //    return _returnBytes;
        //}
        public byte[] ReadAPDTemperature(APDChannelType channel)
        {
            FunctionCode = FunctionCodeType.ReadReg;
            if (channel == APDChannelType.CHA)
            {
                RegAddress = APDRegsAddressType.TemperatureCHA;
            }
            byte[] _returnBytes = PrepareRequestBytes();
            return _returnBytes;
        }
        public byte[] ReadAPDValue(APDChannelType channel)
        {
            FunctionCode = FunctionCodeType.ReadReg;
            if (channel == APDChannelType.CHA)
            {
                RegAddress = APDRegsAddressType.APDValueModuleA;
            }
            byte[] _returnBytes = PrepareRequestBytes();
            return _returnBytes;
        }
        //public byte[] VisitWaveLength(APDChannelType channel, FunctionCodeType writeOrRead)
        //{
        //    FunctionCode = writeOrRead;
        //    if (channel == APDChannelType.CHA)
        //    {
        //        RegAddress = APDRegsAddressType.WaveLengthCHA;
        //    }
        //    //else if (channel == APDChannelType.CHB)
        //    //{
        //    //    RegAddress = APDRegsAddressType.WaveLengthCHB;
        //    //}
        //    byte[] _returnBytes = PrepareRequestBytes();
        //    return _returnBytes;
        //}
        public byte[] VisitTemperatureAtCalibration(APDChannelType channel, FunctionCodeType writeOrRead, double? number = null)
        {
            byte[] _returnBytes = new byte[10];
            FunctionCode = writeOrRead;
            if (channel == APDChannelType.CHA)
            {
                RegAddress = APDRegsAddressType.TemperatureAtCalibrationCHA;
            }
            if (number == null)
            {
                _returnBytes = PrepareRequestBytes();
            }
            else
            {
                int _tempNumber = (int)Math.Round((double)number * 100, 0);
                _returnBytes = PrepareRequestBytes(_tempNumber);
            }
            return _returnBytes;
        }
        public byte[] VisitCalibrationVolt(APDChannelType channel, FunctionCodeType writeOrRead, double? number = null)
        {
            byte[] _returnBytes = new byte[10];
            FunctionCode = writeOrRead;
            if (channel == APDChannelType.CHA)
            {
                switch (CommOutputChA.CurrentAPDGain)
                {
                    case 50:
                        RegAddress = APDRegsAddressType.CalibrationVoltAtGain50CHA;
                        break;
                    case 100:
                        RegAddress = APDRegsAddressType.CalibrationVoltAtGain100CHA;
                        break;
                    case 150:
                        RegAddress = APDRegsAddressType.CalibrationVoltAtGain150CHA;
                        break;
                    case 200:
                        RegAddress = APDRegsAddressType.CalibrationVoltAtGain200CHA;
                        break;
                    case 250:
                        RegAddress = APDRegsAddressType.CalibrationVoltAtGain250CHA;
                        break;
                    case 300:
                        RegAddress = APDRegsAddressType.CalibrationVoltAtGain300CHA;
                        break;
                    case 400:
                        RegAddress = APDRegsAddressType.CalibrationVoltAtGain400CHA;
                        break;
                    case 500:
                        RegAddress = APDRegsAddressType.CalibrationVoltAtGain500CHA;
                        break;
                    default:
                        RegAddress = APDRegsAddressType.CalibrationVoltAtGain100CHA;
                        break;
                }
            }
            if (number == null)
            {
                _returnBytes = PrepareRequestBytes();
            }
            else
            {
                int _tempNumber = (int)Math.Round((double)number * 100, 0);
                _returnBytes = PrepareRequestBytes(_tempNumber);
            }
            return _returnBytes;
        }

        public byte[] WriteLaser(APDChannelType channel, int number)
        {
            if (channel == APDChannelType.CHA)
            {
                RegAddress = APDRegsAddressType.LaserCHA;
            }
            //else if (channel == APDChannelType.CHB)
            //{
            //    RegAddress = APDRegsAddressType.LaserCHB;
            //}

            byte[] _returnBytes = new byte[10];
            FunctionCode = FunctionCodeType.WriteReg;
            _returnBytes = PrepareRequestBytes(number);
            return _returnBytes;
        }
        public byte[] WriteIVNum(APDChannelType channel, FunctionCodeType writeOrRead, int IVNum)
        {
            byte[] _returnBytes = new byte[10];
            FunctionCode = writeOrRead;
            if (channel == APDChannelType.CHA)
            {
                RegAddress = APDRegsAddressType.IVNum;
            }
            _returnBytes = PrepareRequestBytes(IVNum);
            return _returnBytes;
        }
        public byte[] WriteIVType(APDChannelType channel, FunctionCodeType writeOrRead, int number)
        {
            byte[] _returnBytes = new byte[10];
            FunctionCode = writeOrRead;
            if (channel == APDChannelType.CHA)
            {
                RegAddress = APDRegsAddressType.IVType;
            }
            _returnBytes = PrepareRequestBytes(number);
            return _returnBytes;
        }
        public static int Asc(string character)
        {
            if (character.Length == 1)
            {
                System.Text.ASCIIEncoding asciiEncoding = new System.Text.ASCIIEncoding();
                int intAsciiCode = (int)asciiEncoding.GetBytes(character)[0];
                return (intAsciiCode);
            }
            else
            {
                throw new Exception("Character is not valid.");
            }
        }

        public static string Chr(int asciiCode)
        {
            if (asciiCode >= 0 && asciiCode <= 255)
            {
                System.Text.ASCIIEncoding asciiEncoding = new System.Text.ASCIIEncoding();
                byte[] byteArray = new byte[] { (byte)asciiCode };
                string strCharacter = asciiEncoding.GetString(byteArray);
                return (strCharacter);
            }
            else
            {
                throw new Exception("ASCII Code is not valid.");
            }
        }
        public string ApdIvTempData(byte[] receiveBuf, int receiveBufLth)
        {
            if (receiveBufLth < 10)
            {
                return "NotFullFilled";
            }
            if (receiveBuf[0] == 0x3a && receiveBuf[9] == 0x3b)
            {
                int result = 0;
                result = result << 8 | receiveBuf[7];
                result = result << 8 | receiveBuf[8];
                return result.ToString();
            }
            else
            {
                return "NotFullFilled";
            }

        }
        public string ResponseDetect(byte[] receiveBuf, int receiveBufLth)
        {
            if (receiveBufLth < 10)
            {
                return "NotFullFilled";
            }
            else
            {
                int i = 0;
                for (; i < receiveBufLth - 9; i++)
                {
                    if (receiveBuf[i] == _FrameHeader && receiveBuf[i + 1] == (byte)_DeviceAddress && receiveBuf[i + 9] == (byte)_FrameEnd)
                    {
                        break;
                    }
                }
                if (i == receiveBufLth - 9)
                {
                    return "FrameNotFound";
                }
                if (receiveBuf[i + 2] == (byte)_FunctionCode)
                {
                    byte[] _tempArray = new byte[4];
                    for (int j = 0; j < 4; j++)
                    {
                        _tempArray[j] = receiveBuf[i + 8 - j];
                    }

                    _DataField = BitConverter.ToInt32(_tempArray, 0);
                    CommStatus = APDCommProtocol.CommStatusType.Processing;
                    if (_DeviceAddress == DeviceAddressType.TestBench)
                    {
                        if (_FunctionCode == FunctionCodeType.ReadReg)
                        {
                            APDRegsAddressType _tempRegAddress = (APDRegsAddressType)receiveBuf[i + 3];
                            switch (_tempRegAddress)
                            {
                                case APDRegsAddressType.APDValueModuleA:
                                    {
                                        // Vo = Dec/65535*5000 (mV)
                                        UInt16 APDOutputValFirstCH = BitConverter.ToUInt16(_tempArray, 2);
                                        //UInt16 APDOutputValSecondCH = BitConverter.ToUInt16(_tempArray, 0);
                                        _CommOutputChA.APDOutput = ((double)APDOutputValFirstCH / 65535.0) * 5.0;
                                        // _CommOutputChB.APDOutput = ((double)APDOutputValSecondCH / 65535.0) * 5.0;
                                        UpdateCommOutput();
                                        break;
                                    }
                                default:
                                    {
                                        break;
                                    }
                            }
                        }
                    }
                    //else if (_DeviceAddress == DeviceAddressType.APDModuleA || _DeviceAddress == DeviceAddressType.APDModuleB)
                    else if (_DeviceAddress == DeviceAddressType.APDModuleA)
                    {
                        if (_FunctionCode == FunctionCodeType.ReadReg)
                        {
                            APDRegsAddressType _tempRegAddress = (APDRegsAddressType)receiveBuf[i + 3];
                            switch (_tempRegAddress)
                            {
                                case APDRegsAddressType.APDGainCHA:
                                    {
                                        _CommOutputChA.CurrentAPDGain = _DataField;
                                        UpdateCommOutput();
                                        break;
                                    }
                                case APDRegsAddressType.PGACHA:
                                    {
                                        _CommOutputChA.CurrentPGA = (int)Math.Pow(2, _DataField);
                                        UpdateCommOutput();
                                        break;
                                    }
                                case APDRegsAddressType.HighVoltageCHA:
                                    {
                                        _CommOutputChA.APDHighVoltage = _DataField * 0.01;
                                        UpdateCommOutput();
                                        break;
                                    }
                                case APDRegsAddressType.TemperatureCHA:
                                    {
                                        _CommOutputChA.APDTemperature = _DataField * 0.01;
                                        UpdateCommOutput();
                                        break;
                                    }
                                case APDRegsAddressType.TemperatureAtCalibrationCHA:
                                    {
                                        _CommOutputChA.TemperatureAtCalibrationCHA = _DataField * 0.01;
                                        break;
                                    }
                                case APDRegsAddressType.PMTGainCHA:
                                    {
                                        _CommOutputChA.CurrentPMTGain = _DataField;
                                        UpdateCommOutput();
                                        break;
                                    }
                                //case APDRegsAddressType.IVNum:
                                //    {
                                //        _CommOutputChA.CurrentIVNum = Chr(_DataField);
                                //       // UpdateCommOutput();
                                //        break;
                                //    }
                                case APDRegsAddressType.IVType:
                                    {
                                        _CommOutputChA.CurrentIVType = _DataField;
                                        UpdateCommOutput();
                                        break;
                                    }
                                default:
                                    {
                                        break;
                                    }
                            }
                        }
                    }
                    //else if (DeviceAddress == DeviceAddressType.LaserModuleA || DeviceAddress == DeviceAddressType.LaserModuleB)
                    else if (DeviceAddress == DeviceAddressType.LaserModuleA)
                    {
                        DeviceAddressType _tempRegAddress = (DeviceAddressType)receiveBuf[i + 3];
                        ////switch (_tempRegAddress)
                        ////{
                        ////    case (DeviceAddressType)0x03:
                        ////        _CommOutputChA.LaserPower = _DataField;
                        ////        UpdateCommOutput();
                        ////        break;
                        ////    default:
                        //        _CommOutputChA.TECTemperature = _DataField;
                        //        UpdateCommOutput();
                        //        //break;
                        ////}
                        if (_FunctionCode == FunctionCodeType.WriteReg)
                        {
                            //APDRegsAddressType _tempRegAddress = (APDRegsAddressType)receiveBuf[i + 3];
                            //switch (_tempRegAddress)
                            //{
                            //    case APDRegsAddressType.LaserCHA:
                            if (_tempRegAddress == (DeviceAddressType)0x03)
                            {
                                _CommOutputChA.LaserPower = _DataField;
                                UpdateCommOutput();
                            }

                            //        break;
                            //}
                        }
                        if (_FunctionCode == FunctionCodeType.ReadReg)
                        {
                            if (receiveBuf[1] == 0x01 && receiveBuf[2] == 0x02)
                            {
                                _CommOutputChA.TECTemperature = _DataField;
                                UpdateCommOutput();
                            }
                            else
                            {
                                int a = 0;
                            }
                        }
                    }

                    CommStatus = APDCommProtocol.CommStatusType.Idle;
                    Thread.Sleep(10);        // add delay for slave devices
                    return "FrameDetected";
                }
                else
                {
                    return "FrameError";
                }
            }
        }
        #endregion Public Functions
    }
}
