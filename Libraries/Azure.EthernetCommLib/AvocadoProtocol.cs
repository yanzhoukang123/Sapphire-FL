using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Azure.EthernetCommLib
{
    public enum CameraResolutions
    {
        Res320x240 = 1,
        Res640x480,
        Res800x600,
        Res1024x768,
        Res1280x960,
        Res1600x1200,
        Res2048x1536,
        Res2592x1944
    }

    public enum CameraQualities
    {
        ExtremeHigh = 0x01,
        High,
        Medium,
        Low,
    }

    public enum CameraCoclor
    {
        Colored = 0x00,
        BlackWhite
    }

    public enum CameraFormat
    {
        JPG = 0x01,
        RGB565,
        YUV422,
        Gray8,
    }

    public enum MotorTypes
    {
        X = 0x12,
        Y = 0x22,
        Z = 0x42,
        W = 0x82
    }
    public enum IVChannels
    {
        ChannelA = 0x15,
        ChannelB = 0x25,
        ChannelC = 0x45,
    }

    [Serializable]
    public enum LaserChannels
    {
        None = 0,
        ChannelA = 0x16,
        ChannelB = 0x26,
        ChannelC = 0x46,
    }
    [Serializable]
    public enum IvSensorType
    {
        APD = 0,
        PMT = 1,
        NA = 0xff
    }
    public enum AmbientTemperatureChannel
    {
        CH1 = 0,
        CH2 = 1,
    }

    public enum LEDBarChannel
    {
        LEDBarRed = 1,
        LEDBarGreen = 2,
        LEDBarBlue = 4,
    }

    public class MotionState
    {
        public bool Enabled { get; set; }
        public bool AtFwdLimit { get; set; }
        public bool AtBwdLimit { get; set; }
        public bool AtHome { get; set; }
        public bool IsBusy { get; set; }
        public static MotionState MapFromByte(byte data)
        {
            MotionState result = new MotionState()
            {
                Enabled = (data & 0x01) == 0x01 ? true : false,
                AtFwdLimit = (data & 0x08) == 0x08 ? true : false,
                AtBwdLimit = (data & 0x04) == 0x04 ? true : false,
                AtHome = (data & 0x10) == 0x10 ? true : false,
                IsBusy = (data & 0x02) == 0x02 ? true : false,
            };
            return result;
        }
    }

    public class CameraParameter
    {
        public byte BaudRate { get; set; }
        public byte HWVersion { get; set; }
        public byte MotherFWVersion { get; set; }
        public byte SonFWVersion { get; set; }
    }

    public static class AvocadoProtocol
    {
        public static event GotDataHandle OnReceivedScanningData;
        public static event GotDataHandle OnReceivedMotionData;
        public static event GotDataHandle OnRecievedSingleSampleData;

        #region Enumeration defination
        public enum CommandTypes
        {
            Read = 0x01,
            Write = 0x02,
        }

        public enum SubSys
        {
            Default = 0x00,
            Mainboard = 0x01,
            Motor_X = 0x12,
            Motor_Y = 0x22,
            Motor_Z = 0x42,
            Motor_W = 0x82,
            ScanControl = 0x03,
            Camera = 0x04,
            LaserChA = 0x16,
            LaserChB = 0x26,
            LaserChC = 0x46,
            LEDBar = 0x07,
            Iv_ChA = 0x15,
            Iv_ChB = 0x25,
            Iv_ChC = 0x45,
            LaserPickoff = 0x08,
        }

        public enum Properties
        {
            Default = 0x00,
            // Mainboard properties
            HWVersion = 0x01,
            FWVersion,
            IndividualParameter,
            XEncoderSubdivision = 0x04,
            XMotionSubdivision = 0x11,
            IncrustationFan = 0x12,  //Housing fan
            CommunicationControl = 0x13,//485 Communication polling
            AmbientTemperature = 0x14,
            PromptForPressingShutdown = 0x15,  //图像扫描过程中按下前面板关机键  Press the front panel shutdown button during image scanning
            Register2Control = 0x16, //继电器2控制
            // Motor properties
            StartSpeed = 0x01,
            TopSpeed,
            Accel,
            Decel,
            DccPos1,
            TgtPos1,
            DccPos2,
            TgtPos2,
            SingleTripTime,
            Repeats,
            StartCtrl,
            Home,
            EnableCtrl,
            Polarity,
            CrntPos,
            CrntStates,

            // Scan Control properties
            ScanDx = 0x01,
            ScanDy,
            ScanDz,
            ScanResolution,
            ScanQuality,
            ScanRate,
            ScanStart,
            ScanData,
            ScanExtraMove,

            // Camera properties
            CameraSettings = 0x01,
            CameraFocus,
            CameraCapture,
            CameraReadImage,

            // IV properties
            IVEstimatedVersionNumberBoard = 0x00, //IV board software version number
            IvSensorType = 0x01, //sensor type
            IvSensorSN = 0x02,   //sensor number
            IvPGA = 0x03,        //PGA
            IvApdGain = 0x04,    //APD Gain
            IvPmtGain = 0x05,    //PMTcontrol voltage
            APDHighVoltage = 0x06,//APD High voltage value
            LightIntensityCalibrationTemperature = 0x07,//Temperature during light intensity calibration
            APDGainCalVol = 0x08,//APD gain calibration voltage B0-B1: APD gain (50,100,150,200,250,300,400,500); B2-B3: Calibration voltage, unit V, amplified by 100 times transmission, range 0-200
            APDTemp = 0x09,//APD温度
            PMTCompensationCoefficient = 0x0a,//PMT compensation coefficient
            TempSenser1282AD = 0x0b,//12.82℃（1.05k)
            TempSenser6459AD = 0x0c,//64.59℃（1.25k)
            TemperatureSensorSamplingValue = 0x0d,//Real time sampling value of temperature sensor
            RunningState = 0x0e, //running state
            TemperatureSensorSamplingVoltage = 0x0f, //Temperature sensor sampling voltage
            APDTemperatureCalibrationCoefficient = 0x10, //APD temperature calibration coefficient
            ErrorCode = 0x11, //error code
            OpticalModuleSerialNumber = 0x12,//Optical module serial number

            // Laser properties
            LaserBoardFirmwareVersionNumber = 0x00,
            LaserSensorSN = 0x01,
            LaserWaveLength = 0x02,
            //LaserCurrentAt5mW = 0x03,
            //LaserCurrentAt10mW = 0x04,
            //LaserCurrentAt15mW = 0x05,
            //LaserCurrentAt20mW = 0x06,
            //LaserCurrentAt25mW = 0x07,
            //LaserCurrentAt30mW = 0x08,
            LaserCurrent = 0x03,
            LaserPower = 0x04,
            LaserTemper = 0x05,
            RadiatorTemper = 0x06,
            ReserveFan = 0x07,
            LaserCalPower = 0x08, //APD gain calibration voltage
            TECControlTemperature = 0x09,
            TECMaximumCurrent = 0x0a,
            TECControlKp = 0x0b,
            TECControlKi = 0x0c,
            TECControlKd = 0x0d,
            TECWorkingStatus = 0x0e,
            TECCurrentDirection = 0x0f,
            TECCurrentCompensationCoefficient = 0x10,
            LaserErrorCode = 0x11,
            LaserOpticalModuleSerialNumber = 0x12,
            OpticalPowerLessThanOrEqual15mWKp = 0x13,     //Optical power (<=15mW) control Kp
            OpticalPowerLessThanOrEqual15mWKi = 0x14,     //Optical power (<=15mW) control Ki
            OpticalPowerLessThanOrEqual15mWKd = 0x15,     //Optical power (<=15mW) control Kd
            Laser532CurrentPower = 0x16,                 //Current optical power
            OpticalPowerGreaterThan15mWKp = 0x17,           //Optical power (>15mW) control Kp
            OpticalPowerGreaterThan15mWKi = 0x18,           //Optical power (>15mW) control Ki
            OpticalPowerGreaterThan15mWKd = 0x19,           //Optical power (>15mW) control Kd
            OpticalPowerControlvoltage = 0x1a,     //Optical power control voltage
            RadioDiodeVoltage = 0x20,             //Photodiode voltage
            RadioDiodeCalibrationSlope = 0x21,   //Photodiode calibration slope
            RadioAndTelevisionDiodeCalibrationConstant = 0x22, //Photodiode calibration constant
            OpticalPowerControlKpUpperLimitLessThanOrEqual15 = 0x23,  //Optical power (<=15mW) control Kp upper limit
            OpticalPowerControlKpDownLimitLessThanOrEqual15 = 0x24,  //Optical power (<=15mW) control Kp down limit
            OpticalPowerControlKiUpperLimitLessThanOrEqual15 = 0x25,  //Optical power (<=15mW) control Ki upper limit
            OpticalPowerControlKiDownLimitLessThanOrEqual15 = 0x26,  //Optical power (<=15mW) control Ki down limit
            OpticalPowerControlKpUpperLimitLessThan15 = 0x27,  //Optical power (>15mW) control Kp upper limit
            OpticalPowerControlKpDownLimitLessThan15 = 0x28,  //Optical power (>15mW) control Kp down limit
            OpticalPowerControlKiUpperLimitLessThan15 = 0x29,  //Optical power (>15mW) control Ki upper limit
            OpticalPowerControlKiDownLimitLessThan15 = 0x2a,  //Optical power (>15mW) control Ki down limit
            LaserMaximumCurrent = 0x2b,
            LaserMinimumCurrent = 0x2c,

            // LED Bar properties
            LEDBoardFirmwareVersionNumber = 0x00,
            LEDFlicker = 0x01,
            LEDFProgress = 0x02,
            Buzzer = 0x03,
            Shutdown = 0x04,   //Optical module power on/off
            LEDMarquee = 0x05,

            //LightIntensitySensor
            LigthGain = 0x01,
            LightSampleInterval = 0x02,
            LightSampleRange = 0x03,
            LightStart = 0x04,
            LightDataComplete = 0x05,
            LightSampleData = 0x06,
            //LaserPickoff
        }

        #endregion Enumeration defination

        #region Private Fields
        public class FrameDefination
        {
            public byte Head { get; } = 0x6A;
            public CommandTypes Command { get; set; }
            public SubSys System { get; set; }
            public Properties StartingProperty { get; set; }
            public byte PropertyNums { get; set; }
            public ushort DataLength { get; set; }
            public List<byte> DataField { get; set; }
            public byte End { get; } = 0x6E;

            /// <summary>
            /// used by sending frame
            /// </summary>
            /// <returns></returns>
            public byte[] GetBytes()
            {
                List<byte> result = new List<byte>();
                result.Add(Head);
                result.Add((byte)Command);
                result.Add((byte)System);
                result.Add((byte)StartingProperty);
                result.Add(PropertyNums);
                if (Command == CommandTypes.Read)
                {
                    result.AddRange(DataField);
                }
                else
                {
                    //result.Add((byte)(DataField.Count >> 8));
                    //result.Add((byte)(DataField.Count & 0xff));
                    result.AddRange(BitConverter.GetBytes(DataLength));
                    result.AddRange(DataField);

                }
                result.Add(End);

                return result.ToArray();
            }

            public static FrameDefination MapFromBytes(byte[] bytes, int offset)
            {
                try
                {
                    byte head = bytes[0 + offset];
                    if (head != 0x6B) { return null; }
                    CommandTypes cmd = (CommandTypes)bytes[1 + offset];
                    SubSys sys = (SubSys)bytes[2 + offset];
                    byte startingProperty = bytes[3 + offset];
                    byte propertyNums = bytes[4 + offset];
                    ushort dataLength = 0;
                    byte[] dataField;
                    byte end = 0;
                    if (cmd == CommandTypes.Read)
                    {
                        dataLength = BitConverter.ToUInt16(bytes, 5 + offset);
                        dataField = new byte[dataLength];
                        Buffer.BlockCopy(bytes, 7 + offset, dataField, 0, dataLength);
                        end = bytes[7 + offset + dataLength];
                    }
                    else if (cmd == CommandTypes.Write)
                    {
                        dataLength = 4;
                        dataField = new byte[4];
                        Buffer.BlockCopy(bytes, 5 + offset, dataField, 0, dataLength);
                        end = bytes[9];
                    }
                    else { return null; }
                    if (end != 0x6F) { return null; }
                    FrameDefination result = new FrameDefination()
                    {
                        Command = cmd,
                        System = sys,
                        StartingProperty = (Properties)startingProperty,
                        PropertyNums = propertyNums,
                        DataLength = dataLength,
                        DataField = new List<byte>(dataField),
                    };
                    return result;
                }
                catch
                {
                    return null;
                }
            }
        }

        private static FrameDefination _SendingFrame = new FrameDefination();
        private static byte[] _IndividualParameter;
        #endregion Private Fields

        #region Public Properties
        public static string HWVersion { get; private set; }
        public static string FWVersion { get; private set; }
        public static string LEDVersion { get; private set; }

        public static Dictionary<MotorTypes, MotionState> MotionStates { get; }
        public static Dictionary<MotorTypes, int> MotionCrntPositions { get; }
        public static uint SingleSampleChA { get; private set; }
        public static uint SingleSampleChB { get; private set; }
        public static uint SingleSampleChC { get; private set; }
        #region IvSensorLaserBoardFirmwareVersionNumber
        public static Dictionary<IVChannels, IvSensorType> IvSensorTypes { get; private set; }

        public static Dictionary<IVChannels, string> IVEstimatedVersionNumberBoard { get; private set; }
        public static Dictionary<AmbientTemperatureChannel, double> AmbientTemperature { get; private set; }
        public static Dictionary<IVChannels, string> IVOpticalModuleSerialNumber { get; private set; }

        public static Dictionary<IVChannels, string> IVErrorCode { get; private set; }
        public static Dictionary<IVChannels, uint> IvSensorSerialNumbers { get; private set; }

        public static Dictionary<IVChannels, double> TempSenser1282AD { get; private set; }
        public static Dictionary<IVChannels, double> TempSenser6459AD { get; private set; }
        public static Dictionary<IVChannels, double> LightIntensityCalibrationTemperature { get; private set; }

        public static Dictionary<IVChannels, double> APDGainCalVol { get; private set; }

        public static Dictionary<IVChannels, double> PMTCompensationCoefficient { get; private set; }

        #endregion
        public static Dictionary<LaserChannels, string> LaserErrorCode { get; private set; }
        public static Dictionary<LaserChannels, string> LaserOpticalModuleSerialNumber { get; private set; }
        public static Dictionary<LaserChannels, string> LaserBoardFirmwareVersionNumber { get; private set; }
        public static Dictionary<LaserChannels, uint> LaserSerialNumbers { get; private set; }
        public static Dictionary<LaserChannels, uint> LaserWaveLengths { get; private set; }
        public static Dictionary<LaserChannels, double> LaserTemperatures { get; private set; }

        public static Dictionary<LaserChannels, double> TECControlTemperature { get; private set; }

        public static Dictionary<LaserChannels, double> RadiatorTemperatures { get; private set; }
        public static Dictionary<LaserChannels, double> FanTemperatures { get; private set; }
        public static Dictionary<LaserChannels, double> TECMaximumCoolingCurrentValue { get; private set; }

        public static Dictionary<LaserChannels, double> TECRefrigerationControlParameterKp { get; private set; }

        public static Dictionary<LaserChannels, double> TECRefrigerationControlParameterKi { get; private set; }
        public static Dictionary<LaserChannels, double> TECRefrigerationControlParameterKd { get; private set; }

        public static Dictionary<LaserChannels, double> AllIntensity { get; private set; }

        public static Dictionary<LaserChannels, double> TECMaximumCurrent { get; private set; }

        public static Dictionary<LaserChannels, double> AllCurrentLightPower { get; private set; }
        public static Dictionary<LaserChannels, double> AllOpticalPowerLessThanOrEqual15mWKp { get; private set; }
        public static Dictionary<LaserChannels, double> AllOpticalPowerLessThanOrEqual15mWKi { get; private set; }
        public static Dictionary<LaserChannels, double> AllOpticalPowerLessThanOrEqual15mWKd { get; private set; }
        public static Dictionary<LaserChannels, double> AllOpticalPowerGreaterThan15mWKp { get; private set; }
        public static Dictionary<LaserChannels, double> AllOpticalPowerGreaterThan15mWKi { get; private set; }
        public static Dictionary<LaserChannels, double> AllOpticalPowerGreaterThan15mWKd { get; private set; }

        public static Dictionary<LaserChannels, double> AllRadioDiodeVoltage { get; private set; }
        public static Dictionary<LaserChannels, double> AllGetRadioDiodeSlope { get; private set; }
        public static Dictionary<LaserChannels, double> AllGetRadioAndTelevisionDiodeCalibrationConstant { get; private set; }
        public static Dictionary<LaserChannels, double> AllOpticalPowerControlvoltage { get; private set; }
        public static Dictionary<LaserChannels, double> AllCurrentValuelaser { get; private set; }

        public static Dictionary<LaserChannels, double> AllGetOpticalPowerControlKpUpperLimitLessThanOrEqual15 { get; private set; }
        public static Dictionary<LaserChannels, double> AllGetOpticalPowerControlKpDownLimitLessThanOrEqual15 { get; private set; }
        public static Dictionary<LaserChannels, double> AllGetOpticalPowerControlKiUpperLimitLessThanOrEqual15 { get; private set; }
        public static Dictionary<LaserChannels, double> AllGetOpticalPowerControlKiDownLimitLessThanOrEqual15 { get; private set; }
        public static Dictionary<LaserChannels, double> AllGetOpticalPowerControlKpUpperLimitLessThan15 { get; private set; }
        public static Dictionary<LaserChannels, double> AllGetOpticalPowerControlKpDownLimitLessThan15 { get; private set; }
        public static Dictionary<LaserChannels, double> AllGetOpticalPowerControlKiUpperLimitLessThan15 { get; private set; }
        public static Dictionary<LaserChannels, double> AllGetOpticalPowerControlKiDownLimitLessThan15 { get; private set; }
        public static Dictionary<LaserChannels, double> AllGetLaserMaximumCurrent { get; private set; }
        public static Dictionary<LaserChannels, double> AllGetLaserMinimumCurrent { get; private set; }

        public static AvocadoDeviceProperties DeviceProperties { get; private set; }
        public static bool OpticalModulePowerMonitor { get; private set; }    //Optical module power monitoring   光学模块电源监测（FW Version 1.1.0.0）
        public static bool DevicePowerStatus { get; private set; }   //Front panel button power status  前面板按钮电源状态（FW Version 1.1.0.0）
        public static bool LidIsOpen { get; private set; }
        public static bool TopLidIsOpen { get; private set; }
        public static bool TopCoverLock { get; private set; } ////Top cover  status（FW Version 1.1.0.0）   顶盖状态(硬件版本V1.1)
        public static bool TopMagneticState { get; private set; }       // Front lid  status （FW Version 1.1.0.0） 前盖状态(硬件版本V1.1)
        public static bool OpticalModulePowerStatus { get; private set; } //Optical module power status （FW Version 1.1.0.0）  光学模块电源状态(硬件版本V1.1)

        public static bool ShutdownDuringScanStatus { get; set; }      //State when pressing the front panel button while scanning images  （FW Version 1.1.0.0）  //扫描图像时按下前面板按钮时的状态
        public static UInt16 Register2ControlStatus { get; private set; }  //RGB light board control（FW Version 1.1.0.0）
        public static uint XEncoderSubdivision { get; private set; }
        public static UInt16 LightGainDataState { get; private set; }
        public static byte[] LightGainData { get; set; }
        public static string Test { get; set; }
        #endregion Public Properties

        #region Constructor
        static AvocadoProtocol()
        {
            MotionStates = new Dictionary<MotorTypes, MotionState>();
            MotionStates.Add(MotorTypes.X, new MotionState());
            MotionStates.Add(MotorTypes.Y, new MotionState());
            MotionStates.Add(MotorTypes.Z, new MotionState());
            MotionStates.Add(MotorTypes.W, new MotionState());
            MotionCrntPositions = new Dictionary<MotorTypes, int>();
            MotionCrntPositions.Add(MotorTypes.X, 0);
            MotionCrntPositions.Add(MotorTypes.Y, 0);
            MotionCrntPositions.Add(MotorTypes.Z, 0);
            MotionCrntPositions.Add(MotorTypes.W, 0);

            IvSensorTypes = new Dictionary<IVChannels, IvSensorType>();
            IvSensorTypes.Add(IVChannels.ChannelA, IvSensorType.NA);
            IvSensorTypes.Add(IVChannels.ChannelB, IvSensorType.NA);
            IvSensorTypes.Add(IVChannels.ChannelC, IvSensorType.NA);

            IVEstimatedVersionNumberBoard = new Dictionary<IVChannels, string>();
            IVEstimatedVersionNumberBoard.Add(IVChannels.ChannelA, "");
            IVEstimatedVersionNumberBoard.Add(IVChannels.ChannelB, "");
            IVEstimatedVersionNumberBoard.Add(IVChannels.ChannelC, "");

            IVOpticalModuleSerialNumber = new Dictionary<IVChannels, string>();
            IVOpticalModuleSerialNumber.Add(IVChannels.ChannelA, "");
            IVOpticalModuleSerialNumber.Add(IVChannels.ChannelB, "");
            IVOpticalModuleSerialNumber.Add(IVChannels.ChannelC, "");

            IVErrorCode = new Dictionary<IVChannels, string>();
            IVErrorCode.Add(IVChannels.ChannelA, "");
            IVErrorCode.Add(IVChannels.ChannelB, "");
            IVErrorCode.Add(IVChannels.ChannelC, "");

            LaserBoardFirmwareVersionNumber = new Dictionary<LaserChannels, string>();
            LaserBoardFirmwareVersionNumber.Add(LaserChannels.ChannelA, "");
            LaserBoardFirmwareVersionNumber.Add(LaserChannels.ChannelB, "");
            LaserBoardFirmwareVersionNumber.Add(LaserChannels.ChannelC, "");

            LaserErrorCode = new Dictionary<LaserChannels, string>();
            LaserErrorCode.Add(LaserChannels.ChannelA, "");
            LaserErrorCode.Add(LaserChannels.ChannelB, "");
            LaserErrorCode.Add(LaserChannels.ChannelC, "");

            LaserOpticalModuleSerialNumber = new Dictionary<LaserChannels, string>();
            LaserOpticalModuleSerialNumber.Add(LaserChannels.ChannelA, "");
            LaserOpticalModuleSerialNumber.Add(LaserChannels.ChannelB, "");
            LaserOpticalModuleSerialNumber.Add(LaserChannels.ChannelC, "");

            IvSensorSerialNumbers = new Dictionary<IVChannels, uint>();
            IvSensorSerialNumbers.Add(IVChannels.ChannelA, 0);
            IvSensorSerialNumbers.Add(IVChannels.ChannelB, 0);
            IvSensorSerialNumbers.Add(IVChannels.ChannelC, 0);

            TempSenser1282AD = new Dictionary<IVChannels, double>();
            TempSenser1282AD.Add(IVChannels.ChannelA, 0);
            TempSenser1282AD.Add(IVChannels.ChannelB, 0);
            TempSenser1282AD.Add(IVChannels.ChannelC, 0);

            TempSenser6459AD = new Dictionary<IVChannels, double>();
            TempSenser6459AD.Add(IVChannels.ChannelA, 0);
            TempSenser6459AD.Add(IVChannels.ChannelB, 0);
            TempSenser6459AD.Add(IVChannels.ChannelC, 0);

            LightIntensityCalibrationTemperature = new Dictionary<IVChannels, double>();
            LightIntensityCalibrationTemperature.Add(IVChannels.ChannelA, 0);
            LightIntensityCalibrationTemperature.Add(IVChannels.ChannelB, 0);
            LightIntensityCalibrationTemperature.Add(IVChannels.ChannelC, 0);

            APDGainCalVol = new Dictionary<IVChannels, double>();
            APDGainCalVol.Add(IVChannels.ChannelA, 0);
            APDGainCalVol.Add(IVChannels.ChannelB, 0);
            APDGainCalVol.Add(IVChannels.ChannelC, 0);

            PMTCompensationCoefficient = new Dictionary<IVChannels, double>();
            PMTCompensationCoefficient.Add(IVChannels.ChannelA, 0);
            PMTCompensationCoefficient.Add(IVChannels.ChannelB, 0);
            PMTCompensationCoefficient.Add(IVChannels.ChannelC, 0);

            LaserSerialNumbers = new Dictionary<LaserChannels, uint>();
            LaserSerialNumbers.Add(LaserChannels.ChannelA, 0);
            LaserSerialNumbers.Add(LaserChannels.ChannelB, 0);
            LaserSerialNumbers.Add(LaserChannels.ChannelC, 0);
            LaserWaveLengths = new Dictionary<LaserChannels, uint>();
            LaserWaveLengths.Add(LaserChannels.ChannelA, 0);
            LaserWaveLengths.Add(LaserChannels.ChannelB, 0);
            LaserWaveLengths.Add(LaserChannels.ChannelC, 0);
            LaserTemperatures = new Dictionary<LaserChannels, double>();
            LaserTemperatures.Add(LaserChannels.ChannelA, 0);
            LaserTemperatures.Add(LaserChannels.ChannelB, 0);
            LaserTemperatures.Add(LaserChannels.ChannelC, 0);
            RadiatorTemperatures = new Dictionary<LaserChannels, double>();
            RadiatorTemperatures.Add(LaserChannels.ChannelA, 0);
            RadiatorTemperatures.Add(LaserChannels.ChannelB, 0);
            RadiatorTemperatures.Add(LaserChannels.ChannelC, 0);

            TECControlTemperature = new Dictionary<LaserChannels, double>();
            TECControlTemperature.Add(LaserChannels.ChannelA, 0);
            TECControlTemperature.Add(LaserChannels.ChannelB, 0);
            TECControlTemperature.Add(LaserChannels.ChannelC, 0);
            FanTemperatures = new Dictionary<LaserChannels, double>();
            FanTemperatures.Add(LaserChannels.ChannelA, 0);
            FanTemperatures.Add(LaserChannels.ChannelB, 0);
            FanTemperatures.Add(LaserChannels.ChannelC, 0);

            AllIntensity = new Dictionary<LaserChannels, double>();
            AllIntensity.Add(LaserChannels.ChannelA, 0);
            AllIntensity.Add(LaserChannels.ChannelB, 0);
            AllIntensity.Add(LaserChannels.ChannelC, 0);

            TECMaximumCurrent = new Dictionary<LaserChannels, double>();
            TECMaximumCurrent.Add(LaserChannels.ChannelA, 0);
            TECMaximumCurrent.Add(LaserChannels.ChannelB, 0);
            TECMaximumCurrent.Add(LaserChannels.ChannelC, 0);

            TECMaximumCoolingCurrentValue = new Dictionary<LaserChannels, double>();
            TECMaximumCoolingCurrentValue.Add(LaserChannels.ChannelA, 0);
            TECMaximumCoolingCurrentValue.Add(LaserChannels.ChannelB, 0);
            TECMaximumCoolingCurrentValue.Add(LaserChannels.ChannelC, 0);

            TECRefrigerationControlParameterKp = new Dictionary<LaserChannels, double>();
            TECRefrigerationControlParameterKp.Add(LaserChannels.ChannelA, 0);
            TECRefrigerationControlParameterKp.Add(LaserChannels.ChannelB, 0);
            TECRefrigerationControlParameterKp.Add(LaserChannels.ChannelC, 0);

            TECRefrigerationControlParameterKi = new Dictionary<LaserChannels, double>();
            TECRefrigerationControlParameterKi.Add(LaserChannels.ChannelA, 0);
            TECRefrigerationControlParameterKi.Add(LaserChannels.ChannelB, 0);
            TECRefrigerationControlParameterKi.Add(LaserChannels.ChannelC, 0);

            TECRefrigerationControlParameterKd = new Dictionary<LaserChannels, double>();
            TECRefrigerationControlParameterKd.Add(LaserChannels.ChannelA, 0);
            TECRefrigerationControlParameterKd.Add(LaserChannels.ChannelB, 0);
            TECRefrigerationControlParameterKd.Add(LaserChannels.ChannelC, 0);

            AllCurrentLightPower = new Dictionary<LaserChannels, double>();
            AllCurrentLightPower.Add(LaserChannels.ChannelA, 0);
            AllCurrentLightPower.Add(LaserChannels.ChannelB, 0);
            AllCurrentLightPower.Add(LaserChannels.ChannelC, 0);

            AllOpticalPowerLessThanOrEqual15mWKp = new Dictionary<LaserChannels, double>();
            AllOpticalPowerLessThanOrEqual15mWKp.Add(LaserChannels.ChannelA, 0);
            AllOpticalPowerLessThanOrEqual15mWKp.Add(LaserChannels.ChannelB, 0);
            AllOpticalPowerLessThanOrEqual15mWKp.Add(LaserChannels.ChannelC, 0);

            AllOpticalPowerLessThanOrEqual15mWKi = new Dictionary<LaserChannels, double>();
            AllOpticalPowerLessThanOrEqual15mWKi.Add(LaserChannels.ChannelA, 0);
            AllOpticalPowerLessThanOrEqual15mWKi.Add(LaserChannels.ChannelB, 0);
            AllOpticalPowerLessThanOrEqual15mWKi.Add(LaserChannels.ChannelC, 0);

            AllOpticalPowerLessThanOrEqual15mWKd = new Dictionary<LaserChannels, double>();
            AllOpticalPowerLessThanOrEqual15mWKd.Add(LaserChannels.ChannelA, 0);
            AllOpticalPowerLessThanOrEqual15mWKd.Add(LaserChannels.ChannelB, 0);
            AllOpticalPowerLessThanOrEqual15mWKd.Add(LaserChannels.ChannelC, 0);

            AllOpticalPowerGreaterThan15mWKp = new Dictionary<LaserChannels, double>();
            AllOpticalPowerGreaterThan15mWKp.Add(LaserChannels.ChannelA, 0);
            AllOpticalPowerGreaterThan15mWKp.Add(LaserChannels.ChannelB, 0);
            AllOpticalPowerGreaterThan15mWKp.Add(LaserChannels.ChannelC, 0);

            AllOpticalPowerGreaterThan15mWKi = new Dictionary<LaserChannels, double>();
            AllOpticalPowerGreaterThan15mWKi.Add(LaserChannels.ChannelA, 0);
            AllOpticalPowerGreaterThan15mWKi.Add(LaserChannels.ChannelB, 0);
            AllOpticalPowerGreaterThan15mWKi.Add(LaserChannels.ChannelC, 0);

            AllOpticalPowerGreaterThan15mWKd = new Dictionary<LaserChannels, double>();
            AllOpticalPowerGreaterThan15mWKd.Add(LaserChannels.ChannelA, 0);
            AllOpticalPowerGreaterThan15mWKd.Add(LaserChannels.ChannelB, 0);
            AllOpticalPowerGreaterThan15mWKd.Add(LaserChannels.ChannelC, 0);

            AllRadioDiodeVoltage = new Dictionary<LaserChannels, double>();
            AllRadioDiodeVoltage.Add(LaserChannels.ChannelA, 0);
            AllRadioDiodeVoltage.Add(LaserChannels.ChannelB, 0);
            AllRadioDiodeVoltage.Add(LaserChannels.ChannelC, 0);

            AllGetRadioDiodeSlope = new Dictionary<LaserChannels, double>();
            AllGetRadioDiodeSlope.Add(LaserChannels.ChannelA, 0);
            AllGetRadioDiodeSlope.Add(LaserChannels.ChannelB, 0);
            AllGetRadioDiodeSlope.Add(LaserChannels.ChannelC, 0);

            AllGetRadioAndTelevisionDiodeCalibrationConstant = new Dictionary<LaserChannels, double>();
            AllGetRadioAndTelevisionDiodeCalibrationConstant.Add(LaserChannels.ChannelA, 0);
            AllGetRadioAndTelevisionDiodeCalibrationConstant.Add(LaserChannels.ChannelB, 0);
            AllGetRadioAndTelevisionDiodeCalibrationConstant.Add(LaserChannels.ChannelC, 0);

            AllOpticalPowerControlvoltage = new Dictionary<LaserChannels, double>();
            AllOpticalPowerControlvoltage.Add(LaserChannels.ChannelA, 0);
            AllOpticalPowerControlvoltage.Add(LaserChannels.ChannelB, 0);
            AllOpticalPowerControlvoltage.Add(LaserChannels.ChannelC, 0);

            AmbientTemperature = new Dictionary<AmbientTemperatureChannel, double>();
            AmbientTemperature.Add(AmbientTemperatureChannel.CH1, 0);
            AmbientTemperature.Add(AmbientTemperatureChannel.CH2, 0);

            AllCurrentValuelaser = new Dictionary<LaserChannels, double>();
            AllCurrentValuelaser.Add(LaserChannels.ChannelA, 0);
            AllCurrentValuelaser.Add(LaserChannels.ChannelB, 0);
            AllCurrentValuelaser.Add(LaserChannels.ChannelC, 0);

            AllGetOpticalPowerControlKpUpperLimitLessThanOrEqual15 = new Dictionary<LaserChannels, double>();
            AllGetOpticalPowerControlKpUpperLimitLessThanOrEqual15.Add(LaserChannels.ChannelA, 0);
            AllGetOpticalPowerControlKpUpperLimitLessThanOrEqual15.Add(LaserChannels.ChannelB, 0);
            AllGetOpticalPowerControlKpUpperLimitLessThanOrEqual15.Add(LaserChannels.ChannelC, 0);

            AllGetOpticalPowerControlKpDownLimitLessThanOrEqual15 = new Dictionary<LaserChannels, double>();
            AllGetOpticalPowerControlKpDownLimitLessThanOrEqual15.Add(LaserChannels.ChannelA, 0);
            AllGetOpticalPowerControlKpDownLimitLessThanOrEqual15.Add(LaserChannels.ChannelB, 0);
            AllGetOpticalPowerControlKpDownLimitLessThanOrEqual15.Add(LaserChannels.ChannelC, 0);

            AllGetOpticalPowerControlKiUpperLimitLessThanOrEqual15 = new Dictionary<LaserChannels, double>();
            AllGetOpticalPowerControlKiUpperLimitLessThanOrEqual15.Add(LaserChannels.ChannelA, 0);
            AllGetOpticalPowerControlKiUpperLimitLessThanOrEqual15.Add(LaserChannels.ChannelB, 0);
            AllGetOpticalPowerControlKiUpperLimitLessThanOrEqual15.Add(LaserChannels.ChannelC, 0);

            AllGetOpticalPowerControlKiDownLimitLessThanOrEqual15 = new Dictionary<LaserChannels, double>();
            AllGetOpticalPowerControlKiDownLimitLessThanOrEqual15.Add(LaserChannels.ChannelA, 0);
            AllGetOpticalPowerControlKiDownLimitLessThanOrEqual15.Add(LaserChannels.ChannelB, 0);
            AllGetOpticalPowerControlKiDownLimitLessThanOrEqual15.Add(LaserChannels.ChannelC, 0);

            AllGetOpticalPowerControlKpUpperLimitLessThan15 = new Dictionary<LaserChannels, double>();
            AllGetOpticalPowerControlKpUpperLimitLessThan15.Add(LaserChannels.ChannelA, 0);
            AllGetOpticalPowerControlKpUpperLimitLessThan15.Add(LaserChannels.ChannelB, 0);
            AllGetOpticalPowerControlKpUpperLimitLessThan15.Add(LaserChannels.ChannelC, 0);

            AllGetOpticalPowerControlKpDownLimitLessThan15 = new Dictionary<LaserChannels, double>();
            AllGetOpticalPowerControlKpDownLimitLessThan15.Add(LaserChannels.ChannelA, 0);
            AllGetOpticalPowerControlKpDownLimitLessThan15.Add(LaserChannels.ChannelB, 0);
            AllGetOpticalPowerControlKpDownLimitLessThan15.Add(LaserChannels.ChannelC, 0);

            AllGetOpticalPowerControlKiUpperLimitLessThan15 = new Dictionary<LaserChannels, double>();
            AllGetOpticalPowerControlKiUpperLimitLessThan15.Add(LaserChannels.ChannelA, 0);
            AllGetOpticalPowerControlKiUpperLimitLessThan15.Add(LaserChannels.ChannelB, 0);
            AllGetOpticalPowerControlKiUpperLimitLessThan15.Add(LaserChannels.ChannelC, 0);

            AllGetOpticalPowerControlKiDownLimitLessThan15 = new Dictionary<LaserChannels, double>();
            AllGetOpticalPowerControlKiDownLimitLessThan15.Add(LaserChannels.ChannelA, 0);
            AllGetOpticalPowerControlKiDownLimitLessThan15.Add(LaserChannels.ChannelB, 0);
            AllGetOpticalPowerControlKiDownLimitLessThan15.Add(LaserChannels.ChannelC, 0);

            AllGetLaserMaximumCurrent = new Dictionary<LaserChannels, double>();
            AllGetLaserMaximumCurrent.Add(LaserChannels.ChannelA, 0);
            AllGetLaserMaximumCurrent.Add(LaserChannels.ChannelB, 0);
            AllGetLaserMaximumCurrent.Add(LaserChannels.ChannelC, 0);

            AllGetLaserMinimumCurrent = new Dictionary<LaserChannels, double>();
            AllGetLaserMinimumCurrent.Add(LaserChannels.ChannelA, 0);
            AllGetLaserMinimumCurrent.Add(LaserChannels.ChannelB, 0);
            AllGetLaserMinimumCurrent.Add(LaserChannels.ChannelC, 0);

            _IndividualParameter = new byte[256];
            DeviceProperties = new AvocadoDeviceProperties();
        }

        #endregion Constructor

        #region Public Functions
        public static byte[] GetLEDVersions()
        {
            _SendingFrame.Command = CommandTypes.Read;
            _SendingFrame.System = SubSys.LEDBar;
            _SendingFrame.StartingProperty = Properties.LEDBoardFirmwareVersionNumber;
            _SendingFrame.PropertyNums = 2;
            _SendingFrame.DataField = new List<byte>(new byte[4]);

            return _SendingFrame.GetBytes();
        }
        public static byte[] GetSystemVersions()
        {
            _SendingFrame.Command = CommandTypes.Read;
            _SendingFrame.System = SubSys.Mainboard;
            _SendingFrame.StartingProperty = Properties.HWVersion;
            _SendingFrame.PropertyNums = 2;
            _SendingFrame.DataField = new List<byte>(new byte[4]);

            return _SendingFrame.GetBytes();
        }

        public static byte[] GetIVSystemVersions(IVChannels channel)
        {
            _SendingFrame.Command = CommandTypes.Read;
            SubSys sys = SubSys.Default;
            switch (channel)
            {
                case IVChannels.ChannelA:
                    sys = SubSys.Iv_ChA;
                    break;
                case IVChannels.ChannelB:
                    sys = SubSys.Iv_ChB;
                    break;
                case IVChannels.ChannelC:
                    sys = SubSys.Iv_ChC;
                    break;
                default:
                    return null;
            }
            _SendingFrame.System = sys;
            _SendingFrame.StartingProperty = Properties.IVEstimatedVersionNumberBoard;
            _SendingFrame.PropertyNums = 1;
            _SendingFrame.DataField = new List<byte>(new byte[4]);

            return _SendingFrame.GetBytes();
        }

        public static byte[] GetIVSystemVersions()
        {
            _SendingFrame.Command = CommandTypes.Read;
            _SendingFrame.System = SubSys.Iv_ChA | SubSys.Iv_ChB | SubSys.Iv_ChC;
            _SendingFrame.StartingProperty = Properties.IVEstimatedVersionNumberBoard;
            _SendingFrame.PropertyNums = 1;
            _SendingFrame.DataField = new List<byte>(new byte[4]);

            return _SendingFrame.GetBytes();
        }

        public static byte[] GetIVOpticalModuleSerialNumber()
        {
            _SendingFrame.Command = CommandTypes.Read;
            _SendingFrame.System = SubSys.Iv_ChA | SubSys.Iv_ChB | SubSys.Iv_ChC;
            _SendingFrame.StartingProperty = Properties.OpticalModuleSerialNumber;
            _SendingFrame.PropertyNums = 1;
            _SendingFrame.DataField = new List<byte>(new byte[4]);

            return _SendingFrame.GetBytes();
        }
        public static byte[] GetIVOpticalModuleSerialNumber(IVChannels channel)
        {
            _SendingFrame.Command = CommandTypes.Read;
            SubSys sys = SubSys.Default;
            switch (channel)
            {
                case IVChannels.ChannelA:
                    sys = SubSys.Iv_ChA;
                    break;
                case IVChannels.ChannelB:
                    sys = SubSys.Iv_ChB;
                    break;
                case IVChannels.ChannelC:
                    sys = SubSys.Iv_ChC;
                    break;
                default:
                    return null;
            }
            _SendingFrame.System = sys;
            _SendingFrame.StartingProperty = Properties.OpticalModuleSerialNumber;
            _SendingFrame.PropertyNums = 1;
            _SendingFrame.DataField = new List<byte>(new byte[4]);

            return _SendingFrame.GetBytes();
        }

        public static byte[] GetIVSystemErrorCode()
        {
            _SendingFrame.Command = CommandTypes.Read;
            _SendingFrame.System = SubSys.Iv_ChA | SubSys.Iv_ChB | SubSys.Iv_ChC;
            _SendingFrame.StartingProperty = Properties.ErrorCode;
            _SendingFrame.PropertyNums = 1;
            _SendingFrame.DataField = new List<byte>(new byte[4]);

            return _SendingFrame.GetBytes();
        }

        public static byte[] GetLaserSystemVersions(LaserChannels channel)
        {
            _SendingFrame.Command = CommandTypes.Read;
            SubSys sys = SubSys.Default;
            switch (channel)
            {
                case LaserChannels.ChannelA:
                    sys = SubSys.LaserChA;
                    break;
                case LaserChannels.ChannelB:
                    sys = SubSys.LaserChB;
                    break;
                case LaserChannels.ChannelC:
                    sys = SubSys.LaserChC;
                    break;
                default:
                    return null;
            }
            _SendingFrame.System = sys;
            _SendingFrame.StartingProperty = Properties.IVEstimatedVersionNumberBoard;
            _SendingFrame.PropertyNums = 1;
            _SendingFrame.DataField = new List<byte>(new byte[4]);

            return _SendingFrame.GetBytes();
        }

        public static byte[] GetLaserSystemVersions()
        {
            _SendingFrame.Command = CommandTypes.Read;
            _SendingFrame.System = SubSys.LaserChA | SubSys.LaserChB | SubSys.LaserChC;
            _SendingFrame.StartingProperty = Properties.LaserBoardFirmwareVersionNumber;
            _SendingFrame.PropertyNums = 1;
            _SendingFrame.DataField = new List<byte>(new byte[4]);

            return _SendingFrame.GetBytes();
        }

        public static byte[] GetLaserErrorCode()
        {
            _SendingFrame.Command = CommandTypes.Read;
            _SendingFrame.System = SubSys.LaserChA | SubSys.LaserChB | SubSys.LaserChC;
            _SendingFrame.StartingProperty = Properties.LaserErrorCode;
            _SendingFrame.PropertyNums = 1;
            _SendingFrame.DataField = new List<byte>(new byte[4]);

            return _SendingFrame.GetBytes();
        }

        public static byte[] GetLaserOpticalModuleSerialNumber(LaserChannels channel)
        {
            _SendingFrame.Command = CommandTypes.Read;
            SubSys sys = SubSys.Default;
            switch (channel)
            {
                case LaserChannels.ChannelA:
                    sys = SubSys.LaserChA;
                    break;
                case LaserChannels.ChannelB:
                    sys = SubSys.LaserChB;
                    break;
                case LaserChannels.ChannelC:
                    sys = SubSys.LaserChC;
                    break;
                default:
                    return null;
            }
            _SendingFrame.System = sys;
            _SendingFrame.StartingProperty = Properties.OpticalModuleSerialNumber;
            _SendingFrame.PropertyNums = 1;
            _SendingFrame.DataField = new List<byte>(new byte[4]);

            return _SendingFrame.GetBytes();
        }

        public static byte[] GetLaserOpticalModuleSerialNumber()
        {
            _SendingFrame.Command = CommandTypes.Read;
            _SendingFrame.System = SubSys.LaserChA | SubSys.LaserChB | SubSys.LaserChC;
            _SendingFrame.StartingProperty = Properties.LaserOpticalModuleSerialNumber;
            _SendingFrame.PropertyNums = 1;
            _SendingFrame.DataField = new List<byte>(new byte[4]);

            return _SendingFrame.GetBytes();
        }
        public static byte[] GetDataState()
        {
            _SendingFrame.Command = CommandTypes.Read;
            _SendingFrame.System = SubSys.LaserPickoff;
            _SendingFrame.StartingProperty = Properties.LightDataComplete;
            _SendingFrame.PropertyNums = 1;
            _SendingFrame.DataField = new List<byte>(new byte[4]);

            return _SendingFrame.GetBytes();
        }
        public static byte[] GetLightData()
        {
            _SendingFrame.Command = CommandTypes.Read;
            _SendingFrame.System = SubSys.LaserPickoff;
            _SendingFrame.StartingProperty = Properties.LightSampleData;
            _SendingFrame.PropertyNums = 1;
            _SendingFrame.DataField = new List<byte>(new byte[4]);

            return _SendingFrame.GetBytes();
        }
        public static byte[] GetIndividualParameters()
        {
            _SendingFrame.Command = CommandTypes.Read;
            _SendingFrame.System = SubSys.Mainboard;
            _SendingFrame.StartingProperty = Properties.IndividualParameter;
            _SendingFrame.PropertyNums = 1;
            _SendingFrame.DataField = new List<byte>(new byte[4]);

            return _SendingFrame.GetBytes();
        }
        public static byte[] GetXEncoderSubdivision()
        {
            _SendingFrame.Command = CommandTypes.Read;
            _SendingFrame.System = SubSys.Mainboard;
            _SendingFrame.StartingProperty = Properties.XEncoderSubdivision;
            _SendingFrame.PropertyNums = 1;
            _SendingFrame.DataField = new List<byte>(new byte[4]);

            return _SendingFrame.GetBytes();
        }
        public static byte[] SetIndividualParameters(AvocadoDeviceProperties properties)
        {
            _SendingFrame.Command = CommandTypes.Write;
            _SendingFrame.System = SubSys.Mainboard;
            _SendingFrame.StartingProperty = Properties.IndividualParameter;
            _SendingFrame.PropertyNums = 1;
            DeviceProperties = properties;
            int size = Marshal.SizeOf(typeof(AvocadoDeviceProperties));
            IntPtr bufferPtr = Marshal.AllocHGlobal(size);
            try
            {
                Marshal.StructureToPtr(DeviceProperties, bufferPtr, false);
                byte[] bytes = new byte[size];
                Marshal.Copy(bufferPtr, bytes, 0, size);

                _SendingFrame.DataLength = (ushort)bytes.Length;
                _SendingFrame.DataField = new List<byte>(bytes);
                return _SendingFrame.GetBytes();
            }
            catch (Exception ex)
            {
                throw new Exception("Error in StructToBytes ! " + ex.Message);
            }
            finally
            {
                Marshal.FreeHGlobal(bufferPtr);
            }
        }

        public static byte[] SetMotionSpeedsAndAccs(SubSys motions, int[] startSpeeds, int[] topSpeeds, int[] accVals, int[] dccVals)
        {
            _SendingFrame.Command = CommandTypes.Write;
            _SendingFrame.System = motions;
            _SendingFrame.StartingProperty = Properties.StartSpeed;
            _SendingFrame.PropertyNums = 4;
            int motorNums = 0;
            bool containsX = ((byte)motions & 0x10) == 0x10;
            bool containsY = ((byte)motions & 0x20) == 0x20;
            bool containsZ = ((byte)motions & 0x40) == 0x40;
            bool containsW = ((byte)motions & 0x80) == 0x80;
            if (containsX) { motorNums++; }
            if (containsY) { motorNums++; }
            if (containsZ) { motorNums++; }
            if (containsW) { motorNums++; }
            _SendingFrame.DataLength = (ushort)(16 * motorNums);
            _SendingFrame.DataField = new List<byte>();
            for (int i = 0; i < motorNums; i++)
            {
                _SendingFrame.DataField.AddRange(BitConverter.GetBytes(startSpeeds[i]));
            }
            for (int i = 0; i < motorNums; i++)
            {
                _SendingFrame.DataField.AddRange(BitConverter.GetBytes(topSpeeds[i]));
            }
            for (int i = 0; i < motorNums; i++)
            {
                _SendingFrame.DataField.AddRange(BitConverter.GetBytes(accVals[i]));
            }
            for (int i = 0; i < motorNums; i++)
            {
                _SendingFrame.DataField.AddRange(BitConverter.GetBytes(dccVals[i]));
            }

            return _SendingFrame.GetBytes();
        }

        public static byte[] SetMotion(SubSys motor, uint startSpeed, uint topSpeed,
                                        uint accel, uint decel,
                                        int dccPos, int tgtPos)
        {
            _SendingFrame.Command = CommandTypes.Write;
            _SendingFrame.System = motor;
            _SendingFrame.StartingProperty = Properties.StartSpeed;
            _SendingFrame.PropertyNums = 10;
            _SendingFrame.DataLength = 24;
            _SendingFrame.DataField = new List<byte>(24);
            _SendingFrame.DataField.AddRange(BitConverter.GetBytes(startSpeed));
            _SendingFrame.DataField.AddRange(BitConverter.GetBytes(topSpeed));
            _SendingFrame.DataField.AddRange(BitConverter.GetBytes(accel));
            _SendingFrame.DataField.AddRange(BitConverter.GetBytes(decel));
            _SendingFrame.DataField.AddRange(BitConverter.GetBytes(dccPos));
            _SendingFrame.DataField.AddRange(BitConverter.GetBytes(tgtPos));
            _SendingFrame.DataField.AddRange(BitConverter.GetBytes(0));         // dccPos2, set to 0
            _SendingFrame.DataField.AddRange(BitConverter.GetBytes(0));         // tgtPos2, set to 0
            _SendingFrame.DataField.AddRange(BitConverter.GetBytes(0));         // delay time, set to 0
            _SendingFrame.DataField.AddRange(BitConverter.GetBytes(0));         // repeats, set to 0

            return _SendingFrame.GetBytes();
        }

        public static byte[] SetMotionParameters(SubSys motionSys, int[] startSpeeds, int[] topSpeeds, int[] accVals, int[] dccVals, int[] dccPosL, int[] tgtPosL, int[] dccPosR, int[] tgtPosR, int[] singleTripTimes, int[] repeats)
        {
            _SendingFrame.Command = CommandTypes.Write;
            _SendingFrame.System = motionSys;
            _SendingFrame.StartingProperty = Properties.StartSpeed;
            _SendingFrame.PropertyNums = 10;
            int motorNums = 0;
            bool containsX = ((byte)motionSys & 0x10) == 0x10;
            bool containsY = ((byte)motionSys & 0x20) == 0x20;
            bool containsZ = ((byte)motionSys & 0x40) == 0x40;
            bool containsW = ((byte)motionSys & 0x80) == 0x80;
            if (containsX) { motorNums++; }
            if (containsY) { motorNums++; }
            if (containsZ) { motorNums++; }
            if (containsW) { motorNums++; }
            _SendingFrame.DataLength = (ushort)(40 * motorNums);
            _SendingFrame.DataField = new List<byte>();
            for (int i = 0; i < motorNums; i++)
            {
                _SendingFrame.DataField.AddRange(BitConverter.GetBytes(startSpeeds[i]));
            }
            for (int i = 0; i < motorNums; i++)
            {
                _SendingFrame.DataField.AddRange(BitConverter.GetBytes(topSpeeds[i]));
            }
            for (int i = 0; i < motorNums; i++)
            {
                _SendingFrame.DataField.AddRange(BitConverter.GetBytes(accVals[i]));
            }
            for (int i = 0; i < motorNums; i++)
            {
                _SendingFrame.DataField.AddRange(BitConverter.GetBytes(dccVals[i]));
            }
            for (int i = 0; i < motorNums; i++)
            {
                _SendingFrame.DataField.AddRange(BitConverter.GetBytes(dccPosL[i]));
            }
            for (int i = 0; i < motorNums; i++)
            {
                _SendingFrame.DataField.AddRange(BitConverter.GetBytes(tgtPosL[i]));
            }
            for (int i = 0; i < motorNums; i++)
            {
                _SendingFrame.DataField.AddRange(BitConverter.GetBytes(dccPosR[i]));
            }
            for (int i = 0; i < motorNums; i++)
            {
                _SendingFrame.DataField.AddRange(BitConverter.GetBytes(tgtPosR[i]));
            }
            for (int i = 0; i < motorNums; i++)
            {
                _SendingFrame.DataField.AddRange(BitConverter.GetBytes(singleTripTimes[i]));
            }
            for (int i = 0; i < motorNums; i++)
            {
                _SendingFrame.DataField.AddRange(BitConverter.GetBytes(repeats[i]));
            }
            return _SendingFrame.GetBytes();
        }
        public static byte[] SetMotionPolarities(ushort x, ushort y, ushort z, ushort w)
        {
            _SendingFrame.Command = CommandTypes.Write;
            _SendingFrame.System = SubSys.Motor_X | SubSys.Motor_Y | SubSys.Motor_Z | SubSys.Motor_W;
            _SendingFrame.StartingProperty = Properties.Polarity;
            _SendingFrame.PropertyNums = 1;
            _SendingFrame.DataLength = 8;
            _SendingFrame.DataField = new List<byte>(8);
            _SendingFrame.DataField.AddRange(BitConverter.GetBytes(x));
            _SendingFrame.DataField.AddRange(BitConverter.GetBytes(y));
            _SendingFrame.DataField.AddRange(BitConverter.GetBytes(z));
            _SendingFrame.DataField.AddRange(BitConverter.GetBytes(w));

            return _SendingFrame.GetBytes();
        }
        /// <summary>
        /// enable Motor control
        /// </summary>
        /// <param name="motor"></param>
        /// <param name="enables"></param>
        /// <returns></returns>
        public static byte[] SetMotionsEnable(SubSys motor, bool[] enables)
        {
            _SendingFrame.Command = CommandTypes.Write;
            _SendingFrame.System = motor;
            _SendingFrame.StartingProperty = Properties.EnableCtrl;
            _SendingFrame.PropertyNums = 1;
            int motorNums = 0;
            bool containsX = ((byte)motor & 0x10) == 0x10;
            bool containsY = ((byte)motor & 0x20) == 0x20;
            bool containsZ = ((byte)motor & 0x40) == 0x40;
            bool containsW = ((byte)motor & 0x80) == 0x80;
            if (containsX) { motorNums++; }
            if (containsY) { motorNums++; }
            if (containsZ) { motorNums++; }
            if (containsW) { motorNums++; }
            _SendingFrame.DataLength = (ushort)(2 * motorNums);
            _SendingFrame.DataField = new List<byte>();
            for (int i = 0; i < motorNums; i++)
            {
                _SendingFrame.DataField.AddRange(BitConverter.GetBytes(enables[i] ? (ushort)0x0001 : (ushort)0));
            }

            return _SendingFrame.GetBytes();
        }

        public static byte[] SetMotionsStart(SubSys motor, bool[] starts)
        {
            _SendingFrame.Command = CommandTypes.Write;
            _SendingFrame.System = motor;
            _SendingFrame.StartingProperty = Properties.StartCtrl;
            _SendingFrame.PropertyNums = 1;
            int motorNums = 0;
            bool containsX = ((byte)motor & 0x10) == 0x10;
            bool containsY = ((byte)motor & 0x20) == 0x20;
            bool containsZ = ((byte)motor & 0x40) == 0x40;
            bool containsW = ((byte)motor & 0x80) == 0x80;
            if (containsX) { motorNums++; }
            if (containsY) { motorNums++; }
            if (containsZ) { motorNums++; }
            if (containsW) { motorNums++; }
            _SendingFrame.DataLength = (ushort)(2 * motorNums);
            _SendingFrame.DataField = new List<byte>();
            for (int i = 0; i < motorNums; i++)
            {
                _SendingFrame.DataField.AddRange(BitConverter.GetBytes(starts[i] ? (ushort)0x0001 : (ushort)0));
            }

            return _SendingFrame.GetBytes();
        }

        public static byte[] HomeMotions(SubSys motor)
        {
            _SendingFrame.Command = CommandTypes.Write;
            _SendingFrame.System = motor;
            _SendingFrame.StartingProperty = Properties.Home;
            _SendingFrame.PropertyNums = 1;
            int motorNums = 0;
            bool containsX = ((byte)motor & 0x10) == 0x10;
            bool containsY = ((byte)motor & 0x20) == 0x20;
            bool containsZ = ((byte)motor & 0x40) == 0x40;
            bool containsW = ((byte)motor & 0x80) == 0x80;
            if (containsX) { motorNums++; }
            if (containsY) { motorNums++; }
            if (containsZ) { motorNums++; }
            if (containsW) { motorNums++; }
            _SendingFrame.DataLength = (ushort)(2 * motorNums);
            _SendingFrame.DataField = new List<byte>();
            for (int i = 0; i < motorNums; i++)
            {
                _SendingFrame.DataField.AddRange(BitConverter.GetBytes((ushort)0x0001));
            }

            return _SendingFrame.GetBytes();
        }

        public static byte[] GetMotionCrntPos(SubSys motor)
        {
            _SendingFrame.Command = CommandTypes.Read;
            _SendingFrame.System = motor;
            _SendingFrame.StartingProperty = Properties.CrntPos;
            _SendingFrame.PropertyNums = 1;
            _SendingFrame.DataField = new List<byte>(new byte[4]);

            return _SendingFrame.GetBytes();
        }

        public static byte[] GetMotionCrntStates(SubSys motor)
        {
            _SendingFrame.Command = CommandTypes.Read;
            _SendingFrame.System = motor;
            _SendingFrame.StartingProperty = Properties.CrntStates;
            _SendingFrame.PropertyNums = 1;
            _SendingFrame.DataField = new List<byte>(new byte[4]);

            return _SendingFrame.GetBytes();
        }

        public static byte[] GetMotionPosAndState(SubSys motor)
        {
            _SendingFrame.Command = CommandTypes.Read;
            _SendingFrame.System = motor;
            _SendingFrame.StartingProperty = Properties.CrntPos;
            _SendingFrame.PropertyNums = 2;
            _SendingFrame.DataField = new List<byte>(new byte[4]);

            return _SendingFrame.GetBytes();
        }

        #region get iv info
        // get sensor type & seiral numbers of all IV modules
        public static byte[] GetAllIvModulesInfo()
        {
            _SendingFrame.Command = CommandTypes.Read;
            _SendingFrame.System = SubSys.Iv_ChA | SubSys.Iv_ChB | SubSys.Iv_ChC;
            _SendingFrame.StartingProperty = Properties.IvSensorType;//iv type
            _SendingFrame.PropertyNums = 2;
            _SendingFrame.DataField = new List<byte>(new byte[4]);

            return _SendingFrame.GetBytes();
        }
        #endregion

        /// <summary>
        /// get serial numbers & wavelengths of all laser modules
        /// </summary>
        /// <returns></returns>
        public static byte[] GetAllLaserModulseInfo()
        {
            _SendingFrame.Command = CommandTypes.Read;
            _SendingFrame.System = SubSys.LaserChA | SubSys.LaserChB | SubSys.LaserChC;
            _SendingFrame.StartingProperty = Properties.LaserSensorSN;//iv type
            _SendingFrame.PropertyNums = 2;
            _SendingFrame.DataField = new List<byte>(new byte[4]);

            return _SendingFrame.GetBytes();
        }

        public static byte[] GetAllLaserTemperatures()
        {
            _SendingFrame.Command = CommandTypes.Read;
            _SendingFrame.System = SubSys.LaserChA | SubSys.LaserChB | SubSys.LaserChC;
            _SendingFrame.StartingProperty = Properties.LaserTemper;
            _SendingFrame.PropertyNums = 1;
            _SendingFrame.DataField = new List<byte>(new byte[4]);

            return _SendingFrame.GetBytes();
        }

        public static byte[] GetAllTempSenser1282AD()
        {
            _SendingFrame.Command = CommandTypes.Read;
            _SendingFrame.System = SubSys.Iv_ChA | SubSys.Iv_ChB | SubSys.Iv_ChC;
            _SendingFrame.StartingProperty = Properties.TempSenser1282AD;
            _SendingFrame.PropertyNums = 1;
            _SendingFrame.DataField = new List<byte>(new byte[4]);
            return _SendingFrame.GetBytes();
        }

        public static byte[] GetAllTempSenser6459AD()
        {
            _SendingFrame.Command = CommandTypes.Read;
            _SendingFrame.System = SubSys.Iv_ChA | SubSys.Iv_ChB | SubSys.Iv_ChC;
            _SendingFrame.StartingProperty = Properties.TempSenser6459AD;
            _SendingFrame.PropertyNums = 1;
            _SendingFrame.DataField = new List<byte>(new byte[4]);
            return _SendingFrame.GetBytes();
        }

        public static byte[] GetAllPMTCompensationCoefficient()
        {
            _SendingFrame.Command = CommandTypes.Read;
            _SendingFrame.System = SubSys.Iv_ChA | SubSys.Iv_ChB | SubSys.Iv_ChC;
            _SendingFrame.StartingProperty = Properties.PMTCompensationCoefficient;
            _SendingFrame.PropertyNums = 1;
            _SendingFrame.DataField = new List<byte>(new byte[4]);
            return _SendingFrame.GetBytes();
        }

        public static byte[] GetAllPMTCompensationCoefficient(IVChannels iVChannels)
        {
            _SendingFrame.Command = CommandTypes.Read;
            SubSys sys = SubSys.Default;
            switch (iVChannels)
            {
                case IVChannels.ChannelA:
                    sys = SubSys.Iv_ChA;
                    break;
                case IVChannels.ChannelB:
                    sys = SubSys.Iv_ChB;
                    break;
                case IVChannels.ChannelC:
                    sys = SubSys.Iv_ChC;
                    break;
                default:
                    return null;
            }
            _SendingFrame.System = sys;
            _SendingFrame.StartingProperty = Properties.PMTCompensationCoefficient;
            _SendingFrame.PropertyNums = 1;
            _SendingFrame.DataField = new List<byte>(new byte[4]);

            return _SendingFrame.GetBytes();
        }

        public static byte[] GetAllLightIntensityCalibrationTemperature()
        {
            _SendingFrame.Command = CommandTypes.Read;
            _SendingFrame.System = SubSys.Iv_ChA | SubSys.Iv_ChB | SubSys.Iv_ChC;
            _SendingFrame.StartingProperty = Properties.LightIntensityCalibrationTemperature;
            _SendingFrame.PropertyNums = 1;
            _SendingFrame.DataField = new List<byte>(new byte[4]);
            return _SendingFrame.GetBytes();
        }

        public static byte[] GetSingeLaserTemperatures(SubSys Type)
        {
            _SendingFrame.Command = CommandTypes.Read;
            _SendingFrame.System = Type;
            _SendingFrame.StartingProperty = Properties.LaserTemper;
            _SendingFrame.PropertyNums = 1;
            _SendingFrame.DataField = new List<byte>(new byte[4]);

            return _SendingFrame.GetBytes();
        }

        public static byte[] GetAllTECControlTTemperatures()
        {
            _SendingFrame.Command = CommandTypes.Read;
            _SendingFrame.System = SubSys.LaserChA | SubSys.LaserChB | SubSys.LaserChC;
            _SendingFrame.StartingProperty = Properties.TECControlTemperature;
            _SendingFrame.PropertyNums = 1;
            _SendingFrame.DataField = new List<byte>(new byte[4]);

            return _SendingFrame.GetBytes();
        }

        public static byte[] GetAllTECControlTTemperatures(LaserChannels channel)
        {
            _SendingFrame.Command = CommandTypes.Read;
            SubSys sys = SubSys.Default;
            switch (channel)
            {
                case LaserChannels.ChannelA:
                    sys = SubSys.LaserChA;
                    break;
                case LaserChannels.ChannelB:
                    sys = SubSys.LaserChB;
                    break;
                case LaserChannels.ChannelC:
                    sys = SubSys.LaserChC;
                    break;
                default:
                    return null;
            }
            _SendingFrame.System = sys;
            _SendingFrame.StartingProperty = Properties.TECControlTemperature;
            _SendingFrame.PropertyNums = 1;
            _SendingFrame.DataField = new List<byte>(new byte[4]);

            return _SendingFrame.GetBytes();
        }

        public static byte[] GetAllRadiatorTemperatures()
        {
            _SendingFrame.Command = CommandTypes.Read;
            _SendingFrame.System = SubSys.LaserChA | SubSys.LaserChB | SubSys.LaserChC;
            _SendingFrame.StartingProperty = Properties.RadiatorTemper;
            _SendingFrame.PropertyNums = 1;
            _SendingFrame.DataField = new List<byte>(new byte[4]);

            return _SendingFrame.GetBytes();
        }
        public static byte[] GetSingeRadiatorTemperatures(SubSys Type)
        {
            _SendingFrame.Command = CommandTypes.Read;
            _SendingFrame.System = Type;
            _SendingFrame.StartingProperty = Properties.RadiatorTemper;
            _SendingFrame.PropertyNums = 1;
            _SendingFrame.DataField = new List<byte>(new byte[4]);

            return _SendingFrame.GetBytes();
        }
        public static byte[] GetAllFanTemperatures()
        {
            _SendingFrame.Command = CommandTypes.Read;
            _SendingFrame.System = SubSys.LaserChA | SubSys.LaserChB | SubSys.LaserChC;
            _SendingFrame.StartingProperty = Properties.ReserveFan;
            _SendingFrame.PropertyNums = 1;
            _SendingFrame.DataField = new List<byte>(new byte[4]);

            return _SendingFrame.GetBytes();
        }

        public static byte[] GetAllTECMaximumCoolingCurrentValue()
        {
            _SendingFrame.Command = CommandTypes.Read;
            _SendingFrame.System = SubSys.LaserChA | SubSys.LaserChB | SubSys.LaserChC;
            _SendingFrame.StartingProperty = Properties.TECMaximumCurrent;
            _SendingFrame.PropertyNums = 1;
            _SendingFrame.DataField = new List<byte>(new byte[4]);

            return _SendingFrame.GetBytes();
        }

        public static byte[] GetAllTECMaximumCoolingCurrentValue(LaserChannels channel)
        {
            _SendingFrame.Command = CommandTypes.Read;
            SubSys sys = SubSys.Default;
            switch (channel)
            {
                case LaserChannels.ChannelA:
                    sys = SubSys.LaserChA;
                    break;
                case LaserChannels.ChannelB:
                    sys = SubSys.LaserChB;
                    break;
                case LaserChannels.ChannelC:
                    sys = SubSys.LaserChC;
                    break;
                default:
                    return null;
            }
            _SendingFrame.System = sys;
            _SendingFrame.StartingProperty = Properties.TECMaximumCurrent;
            _SendingFrame.PropertyNums = 1;
            _SendingFrame.DataField = new List<byte>(new byte[4]);

            return _SendingFrame.GetBytes();
        }
        public static byte[] GetAllCurrentTECRefrigerationControlParameterKp()
        {
            _SendingFrame.Command = CommandTypes.Read;
            _SendingFrame.System = SubSys.LaserChA | SubSys.LaserChB | SubSys.LaserChC;
            _SendingFrame.StartingProperty = Properties.TECControlKp;
            _SendingFrame.PropertyNums = 1;
            _SendingFrame.DataField = new List<byte>(new byte[4]);

            return _SendingFrame.GetBytes();
        }
        public static byte[] GetAllCurrentTECRefrigerationControlParameterKp(LaserChannels channel)
        {
            _SendingFrame.Command = CommandTypes.Read;
            SubSys sys = SubSys.Default;
            switch (channel)
            {
                case LaserChannels.ChannelA:
                    sys = SubSys.LaserChA;
                    break;
                case LaserChannels.ChannelB:
                    sys = SubSys.LaserChB;
                    break;
                case LaserChannels.ChannelC:
                    sys = SubSys.LaserChC;
                    break;
                default:
                    return null;
            }
            _SendingFrame.System = sys;
            _SendingFrame.StartingProperty = Properties.TECControlKp;
            _SendingFrame.PropertyNums = 1;
            _SendingFrame.DataField = new List<byte>(new byte[4]);

            return _SendingFrame.GetBytes();
        }
        public static byte[] GetAllCurrentTECRefrigerationControlParameterKi()
        {
            _SendingFrame.Command = CommandTypes.Read;
            _SendingFrame.System = SubSys.LaserChA | SubSys.LaserChB | SubSys.LaserChC;
            _SendingFrame.StartingProperty = Properties.TECControlKi;
            _SendingFrame.PropertyNums = 1;
            _SendingFrame.DataField = new List<byte>(new byte[4]);

            return _SendingFrame.GetBytes();
        }
        public static byte[] GetAllCurrentTECRefrigerationControlParameterKi(LaserChannels channel)
        {
            _SendingFrame.Command = CommandTypes.Read;
            SubSys sys = SubSys.Default;
            switch (channel)
            {
                case LaserChannels.ChannelA:
                    sys = SubSys.LaserChA;
                    break;
                case LaserChannels.ChannelB:
                    sys = SubSys.LaserChB;
                    break;
                case LaserChannels.ChannelC:
                    sys = SubSys.LaserChC;
                    break;
                default:
                    return null;
            }
            _SendingFrame.System = sys;
            _SendingFrame.StartingProperty = Properties.TECControlKi;
            _SendingFrame.PropertyNums = 1;
            _SendingFrame.DataField = new List<byte>(new byte[4]);

            return _SendingFrame.GetBytes();
        }

        public static byte[] GetAllCurrentTECRefrigerationControlParameterKd()
        {
            _SendingFrame.Command = CommandTypes.Read;
            _SendingFrame.System = SubSys.LaserChA | SubSys.LaserChB | SubSys.LaserChC;
            _SendingFrame.StartingProperty = Properties.TECControlKd;
            _SendingFrame.PropertyNums = 1;
            _SendingFrame.DataField = new List<byte>(new byte[4]);

            return _SendingFrame.GetBytes();
        }
        public static byte[] GetAllCurrentTECRefrigerationControlParameterKd(LaserChannels channel)
        {
            _SendingFrame.Command = CommandTypes.Read;
            SubSys sys = SubSys.Default;
            switch (channel)
            {
                case LaserChannels.ChannelA:
                    sys = SubSys.LaserChA;
                    break;
                case LaserChannels.ChannelB:
                    sys = SubSys.LaserChB;
                    break;
                case LaserChannels.ChannelC:
                    sys = SubSys.LaserChC;
                    break;
                default:
                    return null;
            }
            _SendingFrame.System = sys;
            _SendingFrame.StartingProperty = Properties.TECControlKd;
            _SendingFrame.PropertyNums = 1;
            _SendingFrame.DataField = new List<byte>(new byte[4]);

            return _SendingFrame.GetBytes();
        }

        public static byte[] SetScanParameters(uint dx, uint dy, uint dz, uint res, uint accVal, uint interval, bool startScan)
        {
            _SendingFrame.Command = CommandTypes.Write;
            _SendingFrame.System = SubSys.ScanControl;
            _SendingFrame.StartingProperty = Properties.ScanDx;
            _SendingFrame.PropertyNums = (byte)(startScan ? 7 : 6);
            _SendingFrame.DataLength = (ushort)(4 * _SendingFrame.PropertyNums);
            _SendingFrame.DataField = new List<byte>();
            _SendingFrame.DataField.AddRange(BitConverter.GetBytes(dx));
            _SendingFrame.DataField.AddRange(BitConverter.GetBytes(dy));
            _SendingFrame.DataField.AddRange(BitConverter.GetBytes(dz));
            _SendingFrame.DataField.AddRange(BitConverter.GetBytes(res));
            _SendingFrame.DataField.AddRange(BitConverter.GetBytes(accVal));
            _SendingFrame.DataField.AddRange(BitConverter.GetBytes(interval));
            if (startScan)
            {
                _SendingFrame.DataField.AddRange(BitConverter.GetBytes((int)1));
            }
            return _SendingFrame.GetBytes();
        }

        public static byte[] SetHorizontalScanExtraMove(int extraMove)
        {
            _SendingFrame.Command = CommandTypes.Write;
            _SendingFrame.System = SubSys.ScanControl;
            _SendingFrame.StartingProperty = Properties.ScanExtraMove;
            _SendingFrame.PropertyNums = 1;
            _SendingFrame.DataLength = 4;
            _SendingFrame.DataField = new List<byte>();
            _SendingFrame.DataField.AddRange(BitConverter.GetBytes(extraMove));
            return _SendingFrame.GetBytes();
        }
        public static byte[] StopScan()
        {
            _SendingFrame.Command = CommandTypes.Write;
            _SendingFrame.System = SubSys.ScanControl;
            _SendingFrame.StartingProperty = Properties.ScanStart;
            _SendingFrame.PropertyNums = 1;
            _SendingFrame.DataLength = 4;
            _SendingFrame.DataField = new List<byte>();
            _SendingFrame.DataField.AddRange(BitConverter.GetBytes((int)0));
            return _SendingFrame.GetBytes();
        }

        public static byte[] GetCameraConfiguration()
        {
            _SendingFrame.Command = CommandTypes.Read;
            _SendingFrame.System = SubSys.Camera;
            _SendingFrame.StartingProperty = Properties.CameraSettings;
            _SendingFrame.PropertyNums = 1;
            _SendingFrame.DataField = new List<byte>(new byte[4]);

            return _SendingFrame.GetBytes();
        }

        public static byte[] GetCameraFocus()
        {
            _SendingFrame.Command = CommandTypes.Read;
            _SendingFrame.System = SubSys.Camera;
            _SendingFrame.StartingProperty = Properties.CameraFocus;
            _SendingFrame.PropertyNums = 1;
            _SendingFrame.DataLength = 4;
            _SendingFrame.DataField = new List<byte>(4);
            _SendingFrame.DataField.Add(0x02);  // read focus
            _SendingFrame.DataField.Add(0);
            _SendingFrame.DataField.Add(0);
            _SendingFrame.DataField.Add(0);

            return _SendingFrame.GetBytes();
        }
        public static byte[] GetLightDataState()
        {
            _SendingFrame.Command = CommandTypes.Read;
            _SendingFrame.System = SubSys.LaserPickoff;
            _SendingFrame.StartingProperty = Properties.LightDataComplete;
            _SendingFrame.PropertyNums = 1;
            _SendingFrame.DataField = new List<byte>(new byte[4]);
            return _SendingFrame.GetBytes();
        }
        public static byte[] GetLaserValueCurrent(LaserChannels channel, int current)
        {
            _SendingFrame.Command = CommandTypes.Read;
            SubSys sys = SubSys.Default;
            switch (channel)
            {
                case LaserChannels.ChannelA:
                    sys = SubSys.LaserChA;
                    break;
                case LaserChannels.ChannelB:
                    sys = SubSys.LaserChB;
                    break;
                case LaserChannels.ChannelC:
                    sys = SubSys.LaserChC;
                    break;
                default:
                    return null;
            }
            _SendingFrame.System = sys;
            _SendingFrame.StartingProperty = Properties.LaserCalPower;
            _SendingFrame.PropertyNums = 1;
            _SendingFrame.DataLength = 4;
            _SendingFrame.DataField = new List<byte>(BitConverter.GetBytes(current));

            return _SendingFrame.GetBytes();
        }

        public static byte[] GetOpticalPowerControlvoltaget(LaserChannels channel, int current)
        {
            _SendingFrame.Command = CommandTypes.Read;
            SubSys sys = SubSys.Default;
            switch (channel)
            {
                case LaserChannels.ChannelA:
                    sys = SubSys.LaserChA;
                    break;
                case LaserChannels.ChannelB:
                    sys = SubSys.LaserChB;
                    break;
                case LaserChannels.ChannelC:
                    sys = SubSys.LaserChC;
                    break;
                default:
                    return null;
            }
            _SendingFrame.System = sys;
            _SendingFrame.StartingProperty = Properties.OpticalPowerControlvoltage;
            _SendingFrame.PropertyNums = 1;
            _SendingFrame.DataLength = 4;
            _SendingFrame.DataField = new List<byte>(BitConverter.GetBytes(current));

            return _SendingFrame.GetBytes();
        }

        public static byte[] GetAPDGainCalVol(LaserChannels channel, int current)
        {
            _SendingFrame.Command = CommandTypes.Read;
            SubSys sys = SubSys.Default;
            switch (channel)
            {
                case LaserChannels.ChannelA:
                    sys = SubSys.Iv_ChA;
                    break;
                case LaserChannels.ChannelB:
                    sys = SubSys.Iv_ChB;
                    break;
                case LaserChannels.ChannelC:
                    sys = SubSys.Iv_ChC;
                    break;
                default:
                    return null;
            }
            _SendingFrame.System = sys;
            _SendingFrame.StartingProperty = Properties.APDGainCalVol;
            _SendingFrame.PropertyNums = 1;
            _SendingFrame.DataLength = 4;
            _SendingFrame.DataField = new List<byte>(BitConverter.GetBytes(current));

            return _SendingFrame.GetBytes();
        }
        //读取环境温度
        public static byte[] GetAmbientTemperature()
        {
            _SendingFrame.Command = CommandTypes.Read;
            _SendingFrame.System = SubSys.Mainboard;
            _SendingFrame.StartingProperty = Properties.AmbientTemperature;
            _SendingFrame.PropertyNums = 1;
            _SendingFrame.DataField = new List<byte>(new byte[4]);
            return _SendingFrame.GetBytes();
        }
        //Is the front panel power status pressed during the scanning process
        public static byte[] PromptForPressingShutdown()
        {
            _SendingFrame.Command = CommandTypes.Read;
            _SendingFrame.System = SubSys.Mainboard;
            _SendingFrame.StartingProperty = Properties.PromptForPressingShutdown;
            _SendingFrame.PropertyNums = 1;
            _SendingFrame.DataField = new List<byte>(new byte[4]);
            return _SendingFrame.GetBytes();
        }

        public static byte[] SetCameraAutoFocus(bool LEDOn)
        {
            _SendingFrame.Command = CommandTypes.Read;
            _SendingFrame.System = SubSys.Camera;
            _SendingFrame.StartingProperty = Properties.CameraFocus;
            _SendingFrame.PropertyNums = 1;
            _SendingFrame.DataLength = 4;
            _SendingFrame.DataField = new List<byte>(4);
            _SendingFrame.DataField.Add(0x01);  // auto focus
            _SendingFrame.DataField.Add(0);
            _SendingFrame.DataField.Add(0);
            _SendingFrame.DataField.Add(LEDOn ? (byte)0x01 : (byte)0x00);

            return _SendingFrame.GetBytes();
        }

        public static byte[] SetCameraManualFocus(ushort focus)
        {
            _SendingFrame.Command = CommandTypes.Read;
            _SendingFrame.System = SubSys.Camera;
            _SendingFrame.StartingProperty = Properties.CameraFocus;
            _SendingFrame.PropertyNums = 1;
            _SendingFrame.DataLength = 4;
            _SendingFrame.DataField = new List<byte>(4);
            _SendingFrame.DataField.Add(0x03);  // manual focus
            _SendingFrame.DataField.Add((byte)(focus >> 8));
            _SendingFrame.DataField.Add((byte)(focus & 0xff));
            _SendingFrame.DataField.Add(0);

            return _SendingFrame.GetBytes();
        }

        public static byte[] CameraCaptureImage(CameraResolutions resolution,
                                                CameraQualities quality, CameraCoclor color,
                                                CameraFormat format, bool ledOn, bool autoExposure, byte exposureLevel = 0)
        {
            _SendingFrame.Command = CommandTypes.Write;
            _SendingFrame.System = SubSys.Camera;
            _SendingFrame.StartingProperty = Properties.CameraCapture;
            _SendingFrame.PropertyNums = 1;
            _SendingFrame.DataLength = 6;
            _SendingFrame.DataField = new List<byte>(6);
            _SendingFrame.DataField.Add((byte)resolution);
            _SendingFrame.DataField.Add((byte)((byte)color << 4 & (byte)quality));
            _SendingFrame.DataField.Add((byte)format);
            _SendingFrame.DataField.Add((byte)((autoExposure ? 0x00 : 0x80) & exposureLevel));
            _SendingFrame.DataField.Add(0);
            _SendingFrame.DataField.Add((byte)(ledOn ? 0x01 : 0x00));
            return _SendingFrame.GetBytes();
        }

        public static byte[] SetIvPga(IVChannels channel, ushort pga)
        {
            _SendingFrame.Command = CommandTypes.Write;
            SubSys sys = SubSys.Default;
            switch (channel)
            {
                case IVChannels.ChannelA:
                    sys = SubSys.Iv_ChA;
                    break;
                case IVChannels.ChannelB:
                    sys = SubSys.Iv_ChB;
                    break;
                case IVChannels.ChannelC:
                    sys = SubSys.Iv_ChC;
                    break;
                default:
                    return null;
            }
            _SendingFrame.System = sys;
            _SendingFrame.StartingProperty = Properties.IvPGA;
            _SendingFrame.PropertyNums = 1;
            _SendingFrame.DataLength = 2;
            _SendingFrame.DataField = new List<byte>(BitConverter.GetBytes(pga));

            return _SendingFrame.GetBytes();
        }
        public static byte[] SetIvApdGain(IVChannels channel, ushort apdGain)
        {
            _SendingFrame.Command = CommandTypes.Write;
            SubSys sys = SubSys.Default;
            switch (channel)
            {
                case IVChannels.ChannelA:
                    sys = SubSys.Iv_ChA;
                    break;
                case IVChannels.ChannelB:
                    sys = SubSys.Iv_ChB;
                    break;
                case IVChannels.ChannelC:
                    sys = SubSys.Iv_ChC;
                    break;
                default:
                    return null;
            }
            _SendingFrame.System = sys;
            _SendingFrame.StartingProperty = Properties.IvApdGain;
            _SendingFrame.PropertyNums = 1;
            _SendingFrame.DataLength = 2;
            _SendingFrame.DataField = new List<byte>(BitConverter.GetBytes(apdGain));

            return _SendingFrame.GetBytes();
        }

        public static byte[] SetIvPmtGain(IVChannels channel, ushort pmtGain)
        {
            _SendingFrame.Command = CommandTypes.Write;
            SubSys sys = SubSys.Default;
            switch (channel)
            {
                case IVChannels.ChannelA:
                    sys = SubSys.Iv_ChA;
                    break;
                case IVChannels.ChannelB:
                    sys = SubSys.Iv_ChB;
                    break;
                case IVChannels.ChannelC:
                    sys = SubSys.Iv_ChC;
                    break;
                default:
                    return null;
            }
            _SendingFrame.System = sys;
            _SendingFrame.StartingProperty = Properties.IvPmtGain;
            _SendingFrame.PropertyNums = 1;
            _SendingFrame.DataLength = 2;
            _SendingFrame.DataField = new List<byte>(BitConverter.GetBytes(pmtGain));

            return _SendingFrame.GetBytes();
        }

        public static byte[] SetLaserCurrent(LaserChannels channel, double current)
        {
            _SendingFrame.Command = CommandTypes.Write;
            SubSys sys = SubSys.Default;
            switch (channel)
            {
                case LaserChannels.ChannelA:
                    sys = SubSys.LaserChA;
                    break;
                case LaserChannels.ChannelB:
                    sys = SubSys.LaserChB;
                    break;
                case LaserChannels.ChannelC:
                    sys = SubSys.LaserChC;
                    break;
                default:
                    return null;
            }
            _SendingFrame.System = sys;
            _SendingFrame.StartingProperty = Properties.LaserCurrent;
            _SendingFrame.PropertyNums = 1;
            _SendingFrame.DataLength = 4;
            _SendingFrame.DataField = new List<byte>(BitConverter.GetBytes((int)(current * 100)));

            return _SendingFrame.GetBytes();
        }

        public static byte[] SetLaserPower(LaserChannels channel, double power)
        {
            _SendingFrame.Command = CommandTypes.Write;
            SubSys sys = SubSys.Default;
            switch (channel)
            {
                case LaserChannels.ChannelA:
                    sys = SubSys.LaserChA;
                    break;
                case LaserChannels.ChannelB:
                    sys = SubSys.LaserChB;
                    break;
                case LaserChannels.ChannelC:
                    sys = SubSys.LaserChC;
                    break;
                default:
                    return null;
            }
            _SendingFrame.System = sys;
            _SendingFrame.StartingProperty = Properties.LaserPower;
            _SendingFrame.PropertyNums = 1;
            _SendingFrame.DataLength = 4;
            _SendingFrame.DataField = new List<byte>(BitConverter.GetBytes((int)(power * 10)));

            return _SendingFrame.GetBytes();
        }
        public static byte[] SetLaserValuePower(LaserChannels channel, double powerValue, int LaserAIntensity)
        {
            _SendingFrame = null;
            _SendingFrame = new FrameDefination();
            _SendingFrame.Command = CommandTypes.Write;
            SubSys sys = SubSys.Default;
            switch (channel)
            {
                case LaserChannels.ChannelA:
                    sys = SubSys.LaserChA;
                    break;
                case LaserChannels.ChannelB:
                    sys = SubSys.LaserChB;
                    break;
                case LaserChannels.ChannelC:
                    sys = SubSys.LaserChC;
                    break;
                default:
                    return null;
            }
            _SendingFrame.System = sys;
            _SendingFrame.StartingProperty = Properties.LaserCalPower;
            _SendingFrame.PropertyNums = 1;
            _SendingFrame.DataLength = 4;
            _SendingFrame.DataField = new List<byte>(BitConverter.GetBytes((int)(powerValue)));
            byte[] LaserCalIntensity = (BitConverter.GetBytes((int)(LaserAIntensity)));
            _SendingFrame.DataField[2] = LaserCalIntensity[0];
            _SendingFrame.DataField[3] = LaserCalIntensity[1];
            //_SendingFrame.DataField.Reverse();
            return _SendingFrame.GetBytes();
        }

        public static byte[] SetLed(int ColorLight)
        {
            _SendingFrame.Command = CommandTypes.Write;
            _SendingFrame.System = SubSys.LEDBar;
            _SendingFrame.StartingProperty = Properties.LEDFlicker;
            _SendingFrame.PropertyNums = 1;
            _SendingFrame.DataLength = 4;
            _SendingFrame.DataField = new List<byte>();
            _SendingFrame.DataField.AddRange(BitConverter.GetBytes(ColorLight));
            _SendingFrame.DataField.Reverse();
            return _SendingFrame.GetBytes();
        }
        public static byte[] SetBuzzer(int count, int time)
        {
            _SendingFrame.Command = CommandTypes.Write;
            _SendingFrame.System = SubSys.LEDBar;
            _SendingFrame.StartingProperty = Properties.Buzzer;
            _SendingFrame.PropertyNums = 1;
            _SendingFrame.DataLength = 4;
            _SendingFrame.DataField = new List<byte>();
            _SendingFrame.DataField.AddRange(BitConverter.GetBytes(time));
            _SendingFrame.DataField.Reverse();
            _SendingFrame.DataField[2] = (byte)(count);
            return _SendingFrame.GetBytes();
        }
        public static byte[] SetLEDBarMarquee(LEDBarChannel channels, byte interval, bool enable)
        {
            _SendingFrame.Command = CommandTypes.Write;
            _SendingFrame.System = SubSys.LEDBar;
            _SendingFrame.StartingProperty = Properties.LEDMarquee;
            _SendingFrame.PropertyNums = 1;
            _SendingFrame.DataLength = 4;
            _SendingFrame.DataField = new List<byte>();
            _SendingFrame.DataField.AddRange(new byte[4]);
            _SendingFrame.DataField[3] = (byte)((((byte)channels) << 4) | (interval & 0x0f) | (enable ? 0x80 : 0x00));
            return _SendingFrame.GetBytes();
        }
        public static byte[] SetLEDBarProgress(byte progress)
        {
            _SendingFrame.Command = CommandTypes.Write;
            _SendingFrame.System = SubSys.LEDBar;
            _SendingFrame.StartingProperty = Properties.LEDFProgress;
            _SendingFrame.PropertyNums = 1;
            _SendingFrame.DataLength = 4;
            _SendingFrame.DataField = new List<byte>();
            _SendingFrame.DataField.AddRange(new byte[4]);
            _SendingFrame.DataField[3] = progress;
            return _SendingFrame.GetBytes();
        }
        //设置外壳风扇
        public static byte[] SetIncrustationFan(int IsAuto, int coefficient)
        {
            _SendingFrame.Command = CommandTypes.Write;
            _SendingFrame.System = SubSys.Mainboard;
            _SendingFrame.StartingProperty = Properties.IncrustationFan;
            _SendingFrame.PropertyNums = 1;
            _SendingFrame.DataLength = 2;
            _SendingFrame.DataField = new List<byte>();
            _SendingFrame.DataField.Add(Convert.ToByte(IsAuto));
            _SendingFrame.DataField.Add((byte)(coefficient));
            return _SendingFrame.GetBytes();
        }

        public static byte[] SetCommunicationControl(int sige)
        {
            uint _sige = Convert.ToUInt16(sige);
            _SendingFrame.Command = CommandTypes.Write;
            _SendingFrame.System = SubSys.Mainboard;
            _SendingFrame.StartingProperty = Properties.CommunicationControl;
            _SendingFrame.PropertyNums = 1;
            _SendingFrame.DataLength = 2;
            _SendingFrame.DataField = new List<byte>();
            _SendingFrame.DataField.Add((byte)(sige));
            _SendingFrame.DataField.Add(0);
            return _SendingFrame.GetBytes();
        }
        public static byte[] SetRegisterControl(int sige)
        {
            uint _sige = Convert.ToUInt16(sige);
            _SendingFrame.Command = CommandTypes.Write;
            _SendingFrame.System = SubSys.Mainboard;
            _SendingFrame.StartingProperty = Properties.Register2Control;
            _SendingFrame.PropertyNums = 1;
            _SendingFrame.DataLength = 2;
            _SendingFrame.DataField = new List<byte>();
            _SendingFrame.DataField.Add((byte)(sige));
            _SendingFrame.DataField.Add(0);
            return _SendingFrame.GetBytes();
        }
        public static byte[] SetIncrustationFan()
        {
            _SendingFrame.Command = CommandTypes.Write;
            _SendingFrame.System = SubSys.Mainboard;
            _SendingFrame.StartingProperty = Properties.IncrustationFan;
            _SendingFrame.PropertyNums = 1;
            _SendingFrame.DataLength = 2;
            _SendingFrame.DataField = new List<byte>();
            _SendingFrame.DataField.Add(0);
            _SendingFrame.DataField.Add(0);
            return _SendingFrame.GetBytes();
        }

        //Optical module 0=on 1=off
        public static byte[] SetShutdown(int Flag)
        {
            _SendingFrame.Command = CommandTypes.Write;
            _SendingFrame.System = SubSys.LEDBar;
            _SendingFrame.StartingProperty = Properties.Shutdown;
            _SendingFrame.PropertyNums = 1;
            _SendingFrame.DataLength = 4;
            _SendingFrame.DataField = new List<byte>();
            _SendingFrame.DataField.AddRange(BitConverter.GetBytes(Flag));
            _SendingFrame.DataField.Reverse();
            return _SendingFrame.GetBytes();
        }
        public static byte[] SetFanTemperature(LaserChannels channel, double ReseTemperature)
        {
            _SendingFrame.Command = CommandTypes.Write;
            SubSys sys = SubSys.Default;
            switch (channel)
            {
                case LaserChannels.ChannelA:
                    sys = SubSys.LaserChA;
                    break;
                case LaserChannels.ChannelB:
                    sys = SubSys.LaserChB;
                    break;
                case LaserChannels.ChannelC:
                    sys = SubSys.LaserChC;
                    break;
                default:
                    return null;
            }
            _SendingFrame.System = sys;
            _SendingFrame.StartingProperty = Properties.ReserveFan;
            _SendingFrame.PropertyNums = 1;
            _SendingFrame.DataLength = 4;
            _SendingFrame.DataField = new List<byte>(BitConverter.GetBytes((int)(ReseTemperature * 10)));

            return _SendingFrame.GetBytes();
        }
        public static byte[] SetLightGain(UInt16 gain)
        {
            _SendingFrame.Command = CommandTypes.Write;
            _SendingFrame.System = SubSys.LaserPickoff;
            _SendingFrame.StartingProperty = Properties.LigthGain;
            _SendingFrame.PropertyNums = 1;
            _SendingFrame.DataLength = 2;
            _SendingFrame.DataField = new List<byte>();
            _SendingFrame.DataField.AddRange(BitConverter.GetBytes(gain));
            return _SendingFrame.GetBytes();
        }
        public static byte[] SetLightSampleInterval(UInt16 sampleinterval)
        {
            _SendingFrame.Command = CommandTypes.Write;
            _SendingFrame.System = SubSys.LaserPickoff;
            _SendingFrame.StartingProperty = Properties.LightSampleInterval;
            _SendingFrame.PropertyNums = 1;
            _SendingFrame.DataLength = 2;
            _SendingFrame.DataField = new List<byte>();
            _SendingFrame.DataField.AddRange(BitConverter.GetBytes(sampleinterval));
            return _SendingFrame.GetBytes();
        }
        public static byte[] SetLightSampleStart(UInt16 isstart)
        {
            _SendingFrame.Command = CommandTypes.Write;
            _SendingFrame.System = SubSys.LaserPickoff;
            _SendingFrame.StartingProperty = Properties.LightStart;
            _SendingFrame.PropertyNums = 1;
            _SendingFrame.DataLength = 2;
            _SendingFrame.DataField = new List<byte>();
            _SendingFrame.DataField.AddRange(BitConverter.GetBytes(isstart));
            return _SendingFrame.GetBytes();
        }
        public static byte[] SetLightSampleRange(Int32 sampleinrange)
        {
            _SendingFrame.Command = CommandTypes.Write;
            _SendingFrame.System = SubSys.LaserPickoff;
            _SendingFrame.StartingProperty = Properties.LightSampleRange;
            _SendingFrame.PropertyNums = 1;
            _SendingFrame.DataLength = 4;
            _SendingFrame.DataField = new List<byte>();
            _SendingFrame.DataField.AddRange(BitConverter.GetBytes(sampleinrange));
            return _SendingFrame.GetBytes();
        }
        public static byte[] SetXMotionSubdivision(double XMotionrSubdivision)
        {
            _SendingFrame.Command = CommandTypes.Write;
            SubSys sys = SubSys.Mainboard;
            _SendingFrame.System = sys;
            _SendingFrame.StartingProperty = Properties.XMotionSubdivision;
            _SendingFrame.PropertyNums = 1;
            _SendingFrame.DataLength = 4;
            _SendingFrame.DataField = new List<byte>(BitConverter.GetBytes((int)(XMotionrSubdivision * 100)));

            return _SendingFrame.GetBytes();
        }
        #region 532 Moudle
        public static byte[] GetCurrentLightPower(LaserChannels channel)
        {
            _SendingFrame.Command = CommandTypes.Read;
            SubSys sys = SubSys.Default;
            switch (channel)
            {
                case LaserChannels.ChannelA:
                    sys = SubSys.LaserChA;
                    break;
                case LaserChannels.ChannelB:
                    sys = SubSys.LaserChB;
                    break;
                case LaserChannels.ChannelC:
                    sys = SubSys.LaserChC;
                    break;
                default:
                    return null;
            }
            _SendingFrame.System = sys;
            _SendingFrame.StartingProperty = Properties.Laser532CurrentPower;
            _SendingFrame.PropertyNums = 1;
            _SendingFrame.DataField = new List<byte>(new byte[4]);

            return _SendingFrame.GetBytes();
        }
        //光功率控制Kp
        public static byte[] GetOpticalPowerGreaterThan15mWKp(LaserChannels channel)
        {
            _SendingFrame.Command = CommandTypes.Read;
            SubSys sys = SubSys.Default;
            switch (channel)
            {
                case LaserChannels.ChannelA:
                    sys = SubSys.LaserChA;
                    break;
                case LaserChannels.ChannelB:
                    sys = SubSys.LaserChB;
                    break;
                case LaserChannels.ChannelC:
                    sys = SubSys.LaserChC;
                    break;
                default:
                    return null;
            }
            _SendingFrame.System = sys;
            _SendingFrame.StartingProperty = Properties.OpticalPowerGreaterThan15mWKp;
            _SendingFrame.PropertyNums = 1;
            _SendingFrame.DataField = new List<byte>(new byte[4]);

            return _SendingFrame.GetBytes();
        }

        public static byte[] GetAllOpticalPowerGreaterThan15mWKp()
        {
            _SendingFrame.Command = CommandTypes.Read;
            _SendingFrame.System = SubSys.LaserChA | SubSys.LaserChB | SubSys.LaserChC;
            _SendingFrame.StartingProperty = Properties.OpticalPowerGreaterThan15mWKp;
            _SendingFrame.PropertyNums = 1;
            _SendingFrame.DataField = new List<byte>(new byte[4]);

            return _SendingFrame.GetBytes();
        }
        public static byte[] GetAllOpticalPowerGreaterThan15mWKp(LaserChannels channel)
        {
            _SendingFrame.Command = CommandTypes.Read;
            SubSys sys = SubSys.Default;
            switch (channel)
            {
                case LaserChannels.ChannelA:
                    sys = SubSys.LaserChA;
                    break;
                case LaserChannels.ChannelB:
                    sys = SubSys.LaserChB;
                    break;
                case LaserChannels.ChannelC:
                    sys = SubSys.LaserChC;
                    break;
                default:
                    return null;
            }
            _SendingFrame.System = sys;
            _SendingFrame.StartingProperty = Properties.OpticalPowerGreaterThan15mWKp;
            _SendingFrame.PropertyNums = 1;
            _SendingFrame.DataField = new List<byte>(new byte[4]);

            return _SendingFrame.GetBytes();
        }

        public static byte[] GetAllOpticalPowerGreaterThan15mWKi()
        {
            _SendingFrame.Command = CommandTypes.Read;
            _SendingFrame.System = SubSys.LaserChA | SubSys.LaserChB | SubSys.LaserChC;
            _SendingFrame.StartingProperty = Properties.OpticalPowerGreaterThan15mWKi;
            _SendingFrame.PropertyNums = 1;
            _SendingFrame.DataField = new List<byte>(new byte[4]);

            return _SendingFrame.GetBytes();
        }

        public static byte[] GetAllOpticalPowerGreaterThan15mWKi(LaserChannels channel)
        {
            _SendingFrame.Command = CommandTypes.Read;
            SubSys sys = SubSys.Default;
            switch (channel)
            {
                case LaserChannels.ChannelA:
                    sys = SubSys.LaserChA;
                    break;
                case LaserChannels.ChannelB:
                    sys = SubSys.LaserChB;
                    break;
                case LaserChannels.ChannelC:
                    sys = SubSys.LaserChC;
                    break;
                default:
                    return null;
            }
            _SendingFrame.System = sys;
            _SendingFrame.StartingProperty = Properties.OpticalPowerGreaterThan15mWKi;
            _SendingFrame.PropertyNums = 1;
            _SendingFrame.DataField = new List<byte>(new byte[4]);

            return _SendingFrame.GetBytes();
        }

        public static byte[] GetAllOpticalPowerGreaterThan15mWKd()
        {
            _SendingFrame.Command = CommandTypes.Read;
            _SendingFrame.System = SubSys.LaserChA | SubSys.LaserChB | SubSys.LaserChC;
            _SendingFrame.StartingProperty = Properties.OpticalPowerGreaterThan15mWKd;
            _SendingFrame.PropertyNums = 1;
            _SendingFrame.DataField = new List<byte>(new byte[4]);

            return _SendingFrame.GetBytes();
        }
        public static byte[] GetAllOpticalPowerGreaterThan15mWKd(LaserChannels channel)
        {
            _SendingFrame.Command = CommandTypes.Read;
            SubSys sys = SubSys.Default;
            switch (channel)
            {
                case LaserChannels.ChannelA:
                    sys = SubSys.LaserChA;
                    break;
                case LaserChannels.ChannelB:
                    sys = SubSys.LaserChB;
                    break;
                case LaserChannels.ChannelC:
                    sys = SubSys.LaserChC;
                    break;
                default:
                    return null;
            }
            _SendingFrame.System = sys;
            _SendingFrame.StartingProperty = Properties.OpticalPowerGreaterThan15mWKd;
            _SendingFrame.PropertyNums = 1;
            _SendingFrame.DataField = new List<byte>(new byte[4]);

            return _SendingFrame.GetBytes();
        }
        public static byte[] GetOpticalPowerLessThanOrEqual15mWKp()
        {
            _SendingFrame.Command = CommandTypes.Read;
            _SendingFrame.System = SubSys.LaserChA | SubSys.LaserChB | SubSys.LaserChC;
            _SendingFrame.StartingProperty = Properties.OpticalPowerLessThanOrEqual15mWKp;
            _SendingFrame.PropertyNums = 1;
            _SendingFrame.DataField = new List<byte>(new byte[4]);

            return _SendingFrame.GetBytes();
        }

        public static byte[] GetOpticalPowerLessThanOrEqual15mWKp(LaserChannels channel)
        {
            _SendingFrame.Command = CommandTypes.Read;
            SubSys sys = SubSys.Default;
            switch (channel)
            {
                case LaserChannels.ChannelA:
                    sys = SubSys.LaserChA;
                    break;
                case LaserChannels.ChannelB:
                    sys = SubSys.LaserChB;
                    break;
                case LaserChannels.ChannelC:
                    sys = SubSys.LaserChC;
                    break;
                default:
                    return null;
            }
            _SendingFrame.System = sys;
            _SendingFrame.StartingProperty = Properties.OpticalPowerLessThanOrEqual15mWKp;
            _SendingFrame.PropertyNums = 1;
            _SendingFrame.DataField = new List<byte>(new byte[4]);

            return _SendingFrame.GetBytes();
        }

        public static byte[] GetOpticalPowerLessThanOrEqual15mWKi()
        {
            _SendingFrame.Command = CommandTypes.Read;
            _SendingFrame.System = SubSys.LaserChA | SubSys.LaserChB | SubSys.LaserChC;
            _SendingFrame.StartingProperty = Properties.OpticalPowerLessThanOrEqual15mWKi;
            _SendingFrame.PropertyNums = 1;
            _SendingFrame.DataField = new List<byte>(new byte[4]);

            return _SendingFrame.GetBytes();
        }
        public static byte[] GetOpticalPowerLessThanOrEqual15mWKi(LaserChannels channel)
        {
            _SendingFrame.Command = CommandTypes.Read;
            SubSys sys = SubSys.Default;
            switch (channel)
            {
                case LaserChannels.ChannelA:
                    sys = SubSys.LaserChA;
                    break;
                case LaserChannels.ChannelB:
                    sys = SubSys.LaserChB;
                    break;
                case LaserChannels.ChannelC:
                    sys = SubSys.LaserChC;
                    break;
                default:
                    return null;
            }
            _SendingFrame.System = sys;
            _SendingFrame.StartingProperty = Properties.OpticalPowerLessThanOrEqual15mWKi;
            _SendingFrame.PropertyNums = 1;
            _SendingFrame.DataField = new List<byte>(new byte[4]);

            return _SendingFrame.GetBytes();
        }
        public static byte[] GetOpticalPowerLessThanOrEqual15mWKd()
        {
            _SendingFrame.Command = CommandTypes.Read;
            _SendingFrame.System = SubSys.LaserChA | SubSys.LaserChB | SubSys.LaserChC;
            _SendingFrame.StartingProperty = Properties.OpticalPowerLessThanOrEqual15mWKd;
            _SendingFrame.PropertyNums = 1;
            _SendingFrame.DataField = new List<byte>(new byte[4]);

            return _SendingFrame.GetBytes();
        }
        public static byte[] GetOpticalPowerLessThanOrEqual15mWKd(LaserChannels channel)
        {
            _SendingFrame.Command = CommandTypes.Read;
            SubSys sys = SubSys.Default;
            switch (channel)
            {
                case LaserChannels.ChannelA:
                    sys = SubSys.LaserChA;
                    break;
                case LaserChannels.ChannelB:
                    sys = SubSys.LaserChB;
                    break;
                case LaserChannels.ChannelC:
                    sys = SubSys.LaserChC;
                    break;
                default:
                    return null;
            }
            _SendingFrame.System = sys;
            _SendingFrame.StartingProperty = Properties.OpticalPowerLessThanOrEqual15mWKd;
            _SendingFrame.PropertyNums = 1;
            _SendingFrame.DataField = new List<byte>(new byte[4]);

            return _SendingFrame.GetBytes();
        }
        public static byte[] SetOpticalPowerGreaterThan15mWKp(LaserChannels channel, double current)
        {
            _SendingFrame.Command = CommandTypes.Write;
            SubSys sys = SubSys.Default;
            switch (channel)
            {
                case LaserChannels.ChannelA:
                    sys = SubSys.LaserChA;
                    break;
                case LaserChannels.ChannelB:
                    sys = SubSys.LaserChB;
                    break;
                case LaserChannels.ChannelC:
                    sys = SubSys.LaserChC;
                    break;
                default:
                    return null;
            }
            _SendingFrame.System = sys;
            _SendingFrame.StartingProperty = Properties.OpticalPowerGreaterThan15mWKp;
            _SendingFrame.PropertyNums = 1;
            _SendingFrame.DataLength = 4;
            _SendingFrame.DataField = new List<byte>(BitConverter.GetBytes((int)(current)));
            //_SendingFrame.DataField = new List<byte>(BitConverter.GetBytes((int)(current * 100)));

            return _SendingFrame.GetBytes();
        }
        //光功率控制Ki
        public static byte[] GetOpticalPowerGreaterThan15mWKi(LaserChannels channel)
        {
            _SendingFrame.Command = CommandTypes.Read;
            SubSys sys = SubSys.Default;
            switch (channel)
            {
                case LaserChannels.ChannelA:
                    sys = SubSys.LaserChA;
                    break;
                case LaserChannels.ChannelB:
                    sys = SubSys.LaserChB;
                    break;
                case LaserChannels.ChannelC:
                    sys = SubSys.LaserChC;
                    break;
                default:
                    return null;
            }
            _SendingFrame.System = sys;
            _SendingFrame.StartingProperty = Properties.OpticalPowerGreaterThan15mWKi;
            _SendingFrame.PropertyNums = 1;
            _SendingFrame.DataField = new List<byte>(new byte[4]);

            return _SendingFrame.GetBytes();
        }
        public static byte[] SetOpticalPowerGreaterThan15mWKi(LaserChannels channel, double current)
        {
            _SendingFrame.Command = CommandTypes.Write;
            SubSys sys = SubSys.Default;
            switch (channel)
            {
                case LaserChannels.ChannelA:
                    sys = SubSys.LaserChA;
                    break;
                case LaserChannels.ChannelB:
                    sys = SubSys.LaserChB;
                    break;
                case LaserChannels.ChannelC:
                    sys = SubSys.LaserChC;
                    break;
                default:
                    return null;
            }
            _SendingFrame.System = sys;
            _SendingFrame.StartingProperty = Properties.OpticalPowerGreaterThan15mWKi;
            _SendingFrame.PropertyNums = 1;
            _SendingFrame.DataLength = 4;
            _SendingFrame.DataField = new List<byte>(BitConverter.GetBytes((int)(current)));
            //_SendingFrame.DataField = new List<byte>(BitConverter.GetBytes((int)(current * 100)));

            return _SendingFrame.GetBytes();
        }
        //光功率控制Kd
        public static byte[] GetOpticalPowerGreaterThan15mWKd(LaserChannels channel)
        {
            _SendingFrame.Command = CommandTypes.Read;
            SubSys sys = SubSys.Default;
            switch (channel)
            {
                case LaserChannels.ChannelA:
                    sys = SubSys.LaserChA;
                    break;
                case LaserChannels.ChannelB:
                    sys = SubSys.LaserChB;
                    break;
                case LaserChannels.ChannelC:
                    sys = SubSys.LaserChC;
                    break;
                default:
                    return null;
            }
            _SendingFrame.System = sys;
            _SendingFrame.StartingProperty = Properties.OpticalPowerGreaterThan15mWKd;
            _SendingFrame.PropertyNums = 1;
            _SendingFrame.DataField = new List<byte>(new byte[4]);

            return _SendingFrame.GetBytes();
        }

        public static byte[] SetOpticalPowerGreaterThan15mWKd(LaserChannels channel, double current)
        {
            _SendingFrame.Command = CommandTypes.Write;
            SubSys sys = SubSys.Default;
            switch (channel)
            {
                case LaserChannels.ChannelA:
                    sys = SubSys.LaserChA;
                    break;
                case LaserChannels.ChannelB:
                    sys = SubSys.LaserChB;
                    break;
                case LaserChannels.ChannelC:
                    sys = SubSys.LaserChC;
                    break;
                default:
                    return null;
            }
            _SendingFrame.System = sys;
            _SendingFrame.StartingProperty = Properties.OpticalPowerGreaterThan15mWKd;
            _SendingFrame.PropertyNums = 1;
            _SendingFrame.DataLength = 4;
            _SendingFrame.DataField = new List<byte>(BitConverter.GetBytes((int)(current)));
            //_SendingFrame.DataField = new List<byte>(BitConverter.GetBytes((int)(current * 100)));

            return _SendingFrame.GetBytes();
        }

        public static byte[] SetOpticalPowerLessThanOrEqual15mWKp(LaserChannels channel, double current)
        {
            _SendingFrame.Command = CommandTypes.Write;
            SubSys sys = SubSys.Default;
            switch (channel)
            {
                case LaserChannels.ChannelA:
                    sys = SubSys.LaserChA;
                    break;
                case LaserChannels.ChannelB:
                    sys = SubSys.LaserChB;
                    break;
                case LaserChannels.ChannelC:
                    sys = SubSys.LaserChC;
                    break;
                default:
                    return null;
            }
            _SendingFrame.System = sys;
            _SendingFrame.StartingProperty = Properties.OpticalPowerLessThanOrEqual15mWKp;
            _SendingFrame.PropertyNums = 1;
            _SendingFrame.DataLength = 4;
            _SendingFrame.DataField = new List<byte>(BitConverter.GetBytes((int)(current)));
            //_SendingFrame.DataField = new List<byte>(BitConverter.GetBytes((int)(current * 100)));

            return _SendingFrame.GetBytes();
        }

        public static byte[] SetOpticalPowerLessThanOrEqual15mWKi(LaserChannels channel, double current)
        {
            _SendingFrame.Command = CommandTypes.Write;
            SubSys sys = SubSys.Default;
            switch (channel)
            {
                case LaserChannels.ChannelA:
                    sys = SubSys.LaserChA;
                    break;
                case LaserChannels.ChannelB:
                    sys = SubSys.LaserChB;
                    break;
                case LaserChannels.ChannelC:
                    sys = SubSys.LaserChC;
                    break;
                default:
                    return null;
            }
            _SendingFrame.System = sys;
            _SendingFrame.StartingProperty = Properties.OpticalPowerLessThanOrEqual15mWKi;
            _SendingFrame.PropertyNums = 1;
            _SendingFrame.DataLength = 4;
            _SendingFrame.DataField = new List<byte>(BitConverter.GetBytes((int)(current)));
            //_SendingFrame.DataField = new List<byte>(BitConverter.GetBytes((int)(current * 100)));

            return _SendingFrame.GetBytes();
        }
        public static byte[] SetOpticalPowerLessThanOrEqual15mWKd(LaserChannels channel, double current)
        {
            _SendingFrame.Command = CommandTypes.Write;
            SubSys sys = SubSys.Default;
            switch (channel)
            {
                case LaserChannels.ChannelA:
                    sys = SubSys.LaserChA;
                    break;
                case LaserChannels.ChannelB:
                    sys = SubSys.LaserChB;
                    break;
                case LaserChannels.ChannelC:
                    sys = SubSys.LaserChC;
                    break;
                default:
                    return null;
            }
            _SendingFrame.System = sys;
            _SendingFrame.StartingProperty = Properties.OpticalPowerLessThanOrEqual15mWKd;
            _SendingFrame.PropertyNums = 1;
            _SendingFrame.DataLength = 4;
            _SendingFrame.DataField = new List<byte>(BitConverter.GetBytes((int)(current)));
            //_SendingFrame.DataField = new List<byte>(BitConverter.GetBytes((int)(current * 100)));

            return _SendingFrame.GetBytes();
        }

        //光电二极管电压
        public static byte[] GetRadioDiodeVoltage(LaserChannels channel)
        {
            _SendingFrame.Command = CommandTypes.Read;
            SubSys sys = SubSys.Default;
            switch (channel)
            {
                case LaserChannels.ChannelA:
                    sys = SubSys.LaserChA;
                    break;
                case LaserChannels.ChannelB:
                    sys = SubSys.LaserChB;
                    break;
                case LaserChannels.ChannelC:
                    sys = SubSys.LaserChC;
                    break;
                default:
                    return null;
            }
            _SendingFrame.System = sys;
            _SendingFrame.StartingProperty = Properties.RadioDiodeVoltage;
            _SendingFrame.PropertyNums = 1;
            _SendingFrame.DataField = new List<byte>(new byte[4]);

            return _SendingFrame.GetBytes();
        }
        //光电二极管校准斜率
        public static byte[] GetRadioDiodeSlope(LaserChannels channel)
        {
            _SendingFrame.Command = CommandTypes.Read;
            SubSys sys = SubSys.Default;
            switch (channel)
            {
                case LaserChannels.ChannelA:
                    sys = SubSys.LaserChA;
                    break;
                case LaserChannels.ChannelB:
                    sys = SubSys.LaserChB;
                    break;
                case LaserChannels.ChannelC:
                    sys = SubSys.LaserChC;
                    break;
                default:
                    return null;
            }
            _SendingFrame.System = sys;
            _SendingFrame.StartingProperty = Properties.RadioDiodeCalibrationSlope;
            _SendingFrame.PropertyNums = 1;
            _SendingFrame.DataField = new List<byte>(new byte[4]);

            return _SendingFrame.GetBytes();
        }
        public static byte[] SetRadioDiodeSlope(LaserChannels channel, double current)
        {
            _SendingFrame.Command = CommandTypes.Write;
            SubSys sys = SubSys.Default;
            switch (channel)
            {
                case LaserChannels.ChannelA:
                    sys = SubSys.LaserChA;
                    break;
                case LaserChannels.ChannelB:
                    sys = SubSys.LaserChB;
                    break;
                case LaserChannels.ChannelC:
                    sys = SubSys.LaserChC;
                    break;
                default:
                    return null;
            }
            _SendingFrame.System = sys;
            _SendingFrame.StartingProperty = Properties.RadioDiodeCalibrationSlope;
            _SendingFrame.PropertyNums = 1;
            _SendingFrame.DataLength = 4;
            _SendingFrame.DataField = new List<byte>(BitConverter.GetBytes((int)(current)));
            //_SendingFrame.DataField = new List<byte>(BitConverter.GetBytes((int)(current * 100)));

            return _SendingFrame.GetBytes();
        }
        //光电二极管校准常数
        public static byte[] GetRadioDiodeConstant(LaserChannels channel)
        {
            _SendingFrame.Command = CommandTypes.Read;
            SubSys sys = SubSys.Default;
            switch (channel)
            {
                case LaserChannels.ChannelA:
                    sys = SubSys.LaserChA;
                    break;
                case LaserChannels.ChannelB:
                    sys = SubSys.LaserChB;
                    break;
                case LaserChannels.ChannelC:
                    sys = SubSys.LaserChC;
                    break;
                default:
                    return null;
            }
            _SendingFrame.System = sys;
            _SendingFrame.StartingProperty = Properties.RadioAndTelevisionDiodeCalibrationConstant;
            _SendingFrame.PropertyNums = 1;
            _SendingFrame.DataField = new List<byte>(new byte[4]);

            return _SendingFrame.GetBytes();
        }
        public static byte[] SetRadioDiodeConstant(LaserChannels channel, double current)
        {
            _SendingFrame.Command = CommandTypes.Write;
            SubSys sys = SubSys.Default;
            switch (channel)
            {
                case LaserChannels.ChannelA:
                    sys = SubSys.LaserChA;
                    break;
                case LaserChannels.ChannelB:
                    sys = SubSys.LaserChB;
                    break;
                case LaserChannels.ChannelC:
                    sys = SubSys.LaserChC;
                    break;
                default:
                    return null;
            }
            _SendingFrame.System = sys;
            _SendingFrame.StartingProperty = Properties.RadioAndTelevisionDiodeCalibrationConstant;
            _SendingFrame.PropertyNums = 1;
            _SendingFrame.DataLength = 4;
            _SendingFrame.DataField = new List<byte>(BitConverter.GetBytes((int)(current)));
            //_SendingFrame.DataField = new List<byte>(BitConverter.GetBytes((int)(current * 100)));

            return _SendingFrame.GetBytes();
        }
        //光功率控制电压
        public static byte[] GetLaserControlVoltageCurrent(LaserChannels channel, int current)
        {
            _SendingFrame.Command = CommandTypes.Read;
            SubSys sys = SubSys.Default;
            switch (channel)
            {
                case LaserChannels.ChannelA:
                    sys = SubSys.LaserChA;
                    break;
                case LaserChannels.ChannelB:
                    sys = SubSys.LaserChB;
                    break;
                case LaserChannels.ChannelC:
                    sys = SubSys.LaserChC;
                    break;
                default:
                    return null;
            }
            _SendingFrame.System = sys;
            _SendingFrame.StartingProperty = Properties.OpticalPowerControlvoltage;
            _SendingFrame.PropertyNums = 1;
            _SendingFrame.DataLength = 4;
            _SendingFrame.DataField = new List<byte>(BitConverter.GetBytes(current));

            return _SendingFrame.GetBytes();
        }
        public static byte[] SetLaserControlVoltageCurrent(LaserChannels channel, double powerValue, int LaserAIntensity)
        {
            _SendingFrame = null;
            _SendingFrame = new FrameDefination();
            _SendingFrame.Command = CommandTypes.Write;
            SubSys sys = SubSys.Default;
            switch (channel)
            {
                case LaserChannels.ChannelA:
                    sys = SubSys.LaserChA;
                    break;
                case LaserChannels.ChannelB:
                    sys = SubSys.LaserChB;
                    break;
                case LaserChannels.ChannelC:
                    sys = SubSys.LaserChC;
                    break;
                default:
                    return null;
            }
            _SendingFrame.System = sys;
            _SendingFrame.StartingProperty = Properties.OpticalPowerControlvoltage;
            _SendingFrame.PropertyNums = 1;
            _SendingFrame.DataLength = 4;
            _SendingFrame.DataField = new List<byte>(BitConverter.GetBytes((int)(powerValue)));
            byte[] LaserCalIntensity = (BitConverter.GetBytes((int)(LaserAIntensity)));
            _SendingFrame.DataField[2] = LaserCalIntensity[0];
            _SendingFrame.DataField[3] = LaserCalIntensity[1];
            //_SendingFrame.DataField.Reverse();
            return _SendingFrame.GetBytes();
        }
        public static byte[] GetCurrentValuelaser(LaserChannels channel)
        {
            _SendingFrame.Command = CommandTypes.Read;
            SubSys sys = SubSys.Default;
            switch (channel)
            {
                case LaserChannels.ChannelA:
                    sys = SubSys.LaserChA;
                    break;
                case LaserChannels.ChannelB:
                    sys = SubSys.LaserChB;
                    break;
                case LaserChannels.ChannelC:
                    sys = SubSys.LaserChC;
                    break;
                default:
                    return null;
            }
            _SendingFrame.System = sys;
            _SendingFrame.StartingProperty = Properties.LaserCurrent;
            _SendingFrame.PropertyNums = 1;
            _SendingFrame.DataField = new List<byte>(new byte[4]);

            return _SendingFrame.GetBytes();
        }
        public static byte[] GetAllOpticalPowerControlKpUpperLimitLessThanOrEqual15()
        {
            _SendingFrame.Command = CommandTypes.Read;
            _SendingFrame.System = SubSys.LaserChA | SubSys.LaserChB | SubSys.LaserChC;
            _SendingFrame.StartingProperty = Properties.OpticalPowerControlKpUpperLimitLessThanOrEqual15;
            _SendingFrame.PropertyNums = 1;
            _SendingFrame.DataField = new List<byte>(new byte[4]);
            return _SendingFrame.GetBytes();
        }
        public static byte[] GetAllOpticalPowerControlKpUpperLimitLessThanOrEqual15(LaserChannels channel)
        {
            _SendingFrame.Command = CommandTypes.Read;
            SubSys sys = SubSys.Default;
            switch (channel)
            {
                case LaserChannels.ChannelA:
                    sys = SubSys.LaserChA;
                    break;
                case LaserChannels.ChannelB:
                    sys = SubSys.LaserChB;
                    break;
                case LaserChannels.ChannelC:
                    sys = SubSys.LaserChC;
                    break;
                default:
                    return null;
            }
            _SendingFrame.System = sys;
            _SendingFrame.StartingProperty = Properties.OpticalPowerControlKpUpperLimitLessThanOrEqual15;
            _SendingFrame.PropertyNums = 1;
            _SendingFrame.DataField = new List<byte>(new byte[4]);
            return _SendingFrame.GetBytes();
        }
        public static byte[] GetAllOpticalPowerControlKpDownLimitLessThanOrEqual15()
        {
            _SendingFrame.Command = CommandTypes.Read;
            _SendingFrame.System = SubSys.LaserChA | SubSys.LaserChB | SubSys.LaserChC;
            _SendingFrame.StartingProperty = Properties.OpticalPowerControlKpDownLimitLessThanOrEqual15;
            _SendingFrame.PropertyNums = 1;
            _SendingFrame.DataField = new List<byte>(new byte[4]);
            return _SendingFrame.GetBytes();
        }
        public static byte[] GetAllOpticalPowerControlKpDownLimitLessThanOrEqual15(LaserChannels channel)
        {
            _SendingFrame.Command = CommandTypes.Read;
            SubSys sys = SubSys.Default;
            switch (channel)
            {
                case LaserChannels.ChannelA:
                    sys = SubSys.LaserChA;
                    break;
                case LaserChannels.ChannelB:
                    sys = SubSys.LaserChB;
                    break;
                case LaserChannels.ChannelC:
                    sys = SubSys.LaserChC;
                    break;
                default:
                    return null;
            }
            _SendingFrame.System = sys;
            _SendingFrame.StartingProperty = Properties.OpticalPowerControlKpDownLimitLessThanOrEqual15;
            _SendingFrame.PropertyNums = 1;
            _SendingFrame.DataField = new List<byte>(new byte[4]);
            return _SendingFrame.GetBytes();
        }
        public static byte[] GetAllOpticalPowerControlKiUpperLimitLessThanOrEqual15()
        {
            _SendingFrame.Command = CommandTypes.Read;
            _SendingFrame.System = SubSys.LaserChA | SubSys.LaserChB | SubSys.LaserChC;
            _SendingFrame.StartingProperty = Properties.OpticalPowerControlKiUpperLimitLessThanOrEqual15;
            _SendingFrame.PropertyNums = 1;
            _SendingFrame.DataField = new List<byte>(new byte[4]);
            return _SendingFrame.GetBytes();
        }
        public static byte[] GetAllOpticalPowerControlKiUpperLimitLessThanOrEqual15(LaserChannels channel)
        {
            _SendingFrame.Command = CommandTypes.Read;
            SubSys sys = SubSys.Default;
            switch (channel)
            {
                case LaserChannels.ChannelA:
                    sys = SubSys.LaserChA;
                    break;
                case LaserChannels.ChannelB:
                    sys = SubSys.LaserChB;
                    break;
                case LaserChannels.ChannelC:
                    sys = SubSys.LaserChC;
                    break;
                default:
                    return null;
            }
            _SendingFrame.System = sys;
            _SendingFrame.StartingProperty = Properties.OpticalPowerControlKiUpperLimitLessThanOrEqual15;
            _SendingFrame.PropertyNums = 1;
            _SendingFrame.DataField = new List<byte>(new byte[4]);
            return _SendingFrame.GetBytes();
        }
        public static byte[] GetAllOpticalPowerControlKiDownLimitLessThanOrEqual15()
        {
            _SendingFrame.Command = CommandTypes.Read;
            _SendingFrame.System = SubSys.LaserChA | SubSys.LaserChB | SubSys.LaserChC;
            _SendingFrame.StartingProperty = Properties.OpticalPowerControlKiDownLimitLessThanOrEqual15;
            _SendingFrame.PropertyNums = 1;
            _SendingFrame.DataField = new List<byte>(new byte[4]);
            return _SendingFrame.GetBytes();
        }
        public static byte[] GetAllOpticalPowerControlKiDownLimitLessThanOrEqual15(LaserChannels channel)
        {
            _SendingFrame.Command = CommandTypes.Read;
            SubSys sys = SubSys.Default;
            switch (channel)
            {
                case LaserChannels.ChannelA:
                    sys = SubSys.LaserChA;
                    break;
                case LaserChannels.ChannelB:
                    sys = SubSys.LaserChB;
                    break;
                case LaserChannels.ChannelC:
                    sys = SubSys.LaserChC;
                    break;
                default:
                    return null;
            }
            _SendingFrame.System = sys;
            _SendingFrame.StartingProperty = Properties.OpticalPowerControlKiDownLimitLessThanOrEqual15;
            _SendingFrame.PropertyNums = 1;
            _SendingFrame.DataField = new List<byte>(new byte[4]);
            return _SendingFrame.GetBytes();
        }
        public static byte[] GetAllOpticalPowerControlKpUpperLimitLessThan15()
        {
            _SendingFrame.Command = CommandTypes.Read;
            _SendingFrame.System = SubSys.LaserChA | SubSys.LaserChB | SubSys.LaserChC;
            _SendingFrame.StartingProperty = Properties.OpticalPowerControlKpUpperLimitLessThan15;
            _SendingFrame.PropertyNums = 1;
            _SendingFrame.DataField = new List<byte>(new byte[4]);
            return _SendingFrame.GetBytes();
        }
        public static byte[] GetAllOpticalPowerControlKpUpperLimitLessThan15(LaserChannels channel)
        {
            _SendingFrame.Command = CommandTypes.Read;
            SubSys sys = SubSys.Default;
            switch (channel)
            {
                case LaserChannels.ChannelA:
                    sys = SubSys.LaserChA;
                    break;
                case LaserChannels.ChannelB:
                    sys = SubSys.LaserChB;
                    break;
                case LaserChannels.ChannelC:
                    sys = SubSys.LaserChC;
                    break;
                default:
                    return null;
            }
            _SendingFrame.System = sys;
            _SendingFrame.StartingProperty = Properties.OpticalPowerControlKpUpperLimitLessThan15;
            _SendingFrame.PropertyNums = 1;
            _SendingFrame.DataField = new List<byte>(new byte[4]);
            return _SendingFrame.GetBytes();
        }
        public static byte[] GetAllOpticalPowerControlKpDownLimitLessThan15()
        {
            _SendingFrame.Command = CommandTypes.Read;
            _SendingFrame.System = SubSys.LaserChA | SubSys.LaserChB | SubSys.LaserChC;
            _SendingFrame.StartingProperty = Properties.OpticalPowerControlKpDownLimitLessThan15;
            _SendingFrame.PropertyNums = 1;
            _SendingFrame.DataField = new List<byte>(new byte[4]);
            return _SendingFrame.GetBytes();
        }
        public static byte[] GetAllOpticalPowerControlKpDownLimitLessThan15(LaserChannels channel)
        {
            _SendingFrame.Command = CommandTypes.Read;
            SubSys sys = SubSys.Default;
            switch (channel)
            {
                case LaserChannels.ChannelA:
                    sys = SubSys.LaserChA;
                    break;
                case LaserChannels.ChannelB:
                    sys = SubSys.LaserChB;
                    break;
                case LaserChannels.ChannelC:
                    sys = SubSys.LaserChC;
                    break;
                default:
                    return null;
            }
            _SendingFrame.System = sys;
            _SendingFrame.StartingProperty = Properties.OpticalPowerControlKpDownLimitLessThan15;
            _SendingFrame.PropertyNums = 1;
            _SendingFrame.DataField = new List<byte>(new byte[4]);
            return _SendingFrame.GetBytes();
        }
        public static byte[] GetAllOpticalPowerControlKiUpperLimitLessThan15()
        {
            _SendingFrame.Command = CommandTypes.Read;
            _SendingFrame.System = SubSys.LaserChA | SubSys.LaserChB | SubSys.LaserChC;
            _SendingFrame.StartingProperty = Properties.OpticalPowerControlKiUpperLimitLessThan15;
            _SendingFrame.PropertyNums = 1;
            _SendingFrame.DataField = new List<byte>(new byte[4]);
            return _SendingFrame.GetBytes();
        }
        public static byte[] GetAllOpticalPowerControlKiUpperLimitLessThan15(LaserChannels channel)
        {
            _SendingFrame.Command = CommandTypes.Read;
            SubSys sys = SubSys.Default;
            switch (channel)
            {
                case LaserChannels.ChannelA:
                    sys = SubSys.LaserChA;
                    break;
                case LaserChannels.ChannelB:
                    sys = SubSys.LaserChB;
                    break;
                case LaserChannels.ChannelC:
                    sys = SubSys.LaserChC;
                    break;
                default:
                    return null;
            }
            _SendingFrame.System = sys;
            _SendingFrame.StartingProperty = Properties.OpticalPowerControlKiUpperLimitLessThan15;
            _SendingFrame.PropertyNums = 1;
            _SendingFrame.DataField = new List<byte>(new byte[4]);
            return _SendingFrame.GetBytes();
        }
        public static byte[] GetAllOpticalPowerControlKiDownLimitLessThan15()
        {
            _SendingFrame.Command = CommandTypes.Read;
            _SendingFrame.System = SubSys.LaserChA | SubSys.LaserChB | SubSys.LaserChC;
            _SendingFrame.StartingProperty = Properties.OpticalPowerControlKiDownLimitLessThan15;
            _SendingFrame.PropertyNums = 1;
            _SendingFrame.DataField = new List<byte>(new byte[4]);
            return _SendingFrame.GetBytes();
        }
        public static byte[] GetAllOpticalPowerControlKiDownLimitLessThan15(LaserChannels channel)
        {
            _SendingFrame.Command = CommandTypes.Read;
            SubSys sys = SubSys.Default;
            switch (channel)
            {
                case LaserChannels.ChannelA:
                    sys = SubSys.LaserChA;
                    break;
                case LaserChannels.ChannelB:
                    sys = SubSys.LaserChB;
                    break;
                case LaserChannels.ChannelC:
                    sys = SubSys.LaserChC;
                    break;
                default:
                    return null;
            }
            _SendingFrame.System = sys;
            _SendingFrame.StartingProperty = Properties.OpticalPowerControlKiDownLimitLessThan15;
            _SendingFrame.PropertyNums = 1;
            _SendingFrame.DataField = new List<byte>(new byte[4]);
            return _SendingFrame.GetBytes();
        }
        public static byte[] GetAllLaserMaximumCurrent()
        {
            _SendingFrame.Command = CommandTypes.Read;
            _SendingFrame.System = SubSys.LaserChA | SubSys.LaserChB | SubSys.LaserChC;
            _SendingFrame.StartingProperty = Properties.LaserMaximumCurrent;
            _SendingFrame.PropertyNums = 1;
            _SendingFrame.DataField = new List<byte>(new byte[4]);
            return _SendingFrame.GetBytes();
        }
        public static byte[] GetAllLaserMaximumCurrent(LaserChannels channel)
        {
            _SendingFrame.Command = CommandTypes.Read;
            SubSys sys = SubSys.Default;
            switch (channel)
            {
                case LaserChannels.ChannelA:
                    sys = SubSys.LaserChA;
                    break;
                case LaserChannels.ChannelB:
                    sys = SubSys.LaserChB;
                    break;
                case LaserChannels.ChannelC:
                    sys = SubSys.LaserChC;
                    break;
                default:
                    return null;
            }
            _SendingFrame.System = sys;
            _SendingFrame.StartingProperty = Properties.LaserMaximumCurrent;
            _SendingFrame.PropertyNums = 1;
            _SendingFrame.DataField = new List<byte>(new byte[4]);
            return _SendingFrame.GetBytes();
        }

        public static byte[] GetAllLaserMinimumCurrent()
        {
            _SendingFrame.Command = CommandTypes.Read;
            _SendingFrame.System = SubSys.LaserChA | SubSys.LaserChB | SubSys.LaserChC;
            _SendingFrame.StartingProperty = Properties.LaserMinimumCurrent;
            _SendingFrame.PropertyNums = 1;
            _SendingFrame.DataField = new List<byte>(new byte[4]);
            return _SendingFrame.GetBytes();
        }
        public static byte[] GetAllLaserMinimumCurrent(LaserChannels channel)
        {
            _SendingFrame.Command = CommandTypes.Read;
            SubSys sys = SubSys.Default;
            switch (channel)
            {
                case LaserChannels.ChannelA:
                    sys = SubSys.LaserChA;
                    break;
                case LaserChannels.ChannelB:
                    sys = SubSys.LaserChB;
                    break;
                case LaserChannels.ChannelC:
                    sys = SubSys.LaserChC;
                    break;
                default:
                    return null;
            }
            _SendingFrame.System = sys;
            _SendingFrame.StartingProperty = Properties.LaserMinimumCurrent;
            _SendingFrame.PropertyNums = 1;
            _SendingFrame.DataField = new List<byte>(new byte[4]);
            return _SendingFrame.GetBytes();
        }


        public static byte[] SetOpticalPowerControlKpUpperLimitLessThanOrEqual15(LaserChannels channel, double current)
        {
            _SendingFrame.Command = CommandTypes.Write;
            SubSys sys = SubSys.Default;
            switch (channel)
            {
                case LaserChannels.ChannelA:
                    sys = SubSys.LaserChA;
                    break;
                case LaserChannels.ChannelB:
                    sys = SubSys.LaserChB;
                    break;
                case LaserChannels.ChannelC:
                    sys = SubSys.LaserChC;
                    break;
                default:
                    return null;
            }
            _SendingFrame.System = sys;
            _SendingFrame.StartingProperty = Properties.OpticalPowerControlKpUpperLimitLessThanOrEqual15;
            _SendingFrame.PropertyNums = 1;
            _SendingFrame.DataLength = 4;
            _SendingFrame.DataField = new List<byte>(BitConverter.GetBytes((int)(current)));
            //_SendingFrame.DataField = new List<byte>(BitConverter.GetBytes((int)(current * 100)));

            return _SendingFrame.GetBytes();
        }

        public static byte[] SetOpticalPowerControlKpDownLimitLessThanOrEqual15(LaserChannels channel, double current)
        {
            _SendingFrame.Command = CommandTypes.Write;
            SubSys sys = SubSys.Default;
            switch (channel)
            {
                case LaserChannels.ChannelA:
                    sys = SubSys.LaserChA;
                    break;
                case LaserChannels.ChannelB:
                    sys = SubSys.LaserChB;
                    break;
                case LaserChannels.ChannelC:
                    sys = SubSys.LaserChC;
                    break;
                default:
                    return null;
            }
            _SendingFrame.System = sys;
            _SendingFrame.StartingProperty = Properties.OpticalPowerControlKpDownLimitLessThanOrEqual15;
            _SendingFrame.PropertyNums = 1;
            _SendingFrame.DataLength = 4;
            _SendingFrame.DataField = new List<byte>(BitConverter.GetBytes((int)(current)));
            //_SendingFrame.DataField = new List<byte>(BitConverter.GetBytes((int)(current * 100)));

            return _SendingFrame.GetBytes();
        }

        public static byte[] SetOpticalPowerControlKiUpperLimitLessThanOrEqual15(LaserChannels channel, double current)
        {
            _SendingFrame.Command = CommandTypes.Write;
            SubSys sys = SubSys.Default;
            switch (channel)
            {
                case LaserChannels.ChannelA:
                    sys = SubSys.LaserChA;
                    break;
                case LaserChannels.ChannelB:
                    sys = SubSys.LaserChB;
                    break;
                case LaserChannels.ChannelC:
                    sys = SubSys.LaserChC;
                    break;
                default:
                    return null;
            }
            _SendingFrame.System = sys;
            _SendingFrame.StartingProperty = Properties.OpticalPowerControlKiUpperLimitLessThanOrEqual15;
            _SendingFrame.PropertyNums = 1;
            _SendingFrame.DataLength = 4;
            _SendingFrame.DataField = new List<byte>(BitConverter.GetBytes((int)(current)));
            //_SendingFrame.DataField = new List<byte>(BitConverter.GetBytes((int)(current * 100)));

            return _SendingFrame.GetBytes();
        }

        public static byte[] SetOpticalPowerControlKiDownLimitLessThanOrEqual15(LaserChannels channel, double current)
        {
            _SendingFrame.Command = CommandTypes.Write;
            SubSys sys = SubSys.Default;
            switch (channel)
            {
                case LaserChannels.ChannelA:
                    sys = SubSys.LaserChA;
                    break;
                case LaserChannels.ChannelB:
                    sys = SubSys.LaserChB;
                    break;
                case LaserChannels.ChannelC:
                    sys = SubSys.LaserChC;
                    break;
                default:
                    return null;
            }
            _SendingFrame.System = sys;
            _SendingFrame.StartingProperty = Properties.OpticalPowerControlKiDownLimitLessThanOrEqual15;
            _SendingFrame.PropertyNums = 1;
            _SendingFrame.DataLength = 4;
            _SendingFrame.DataField = new List<byte>(BitConverter.GetBytes((int)(current)));
            //_SendingFrame.DataField = new List<byte>(BitConverter.GetBytes((int)(current * 100)));

            return _SendingFrame.GetBytes();
        }

        public static byte[] SetOpticalPowerControlKpUpperLimitLessThan15(LaserChannels channel, double current)
        {
            _SendingFrame.Command = CommandTypes.Write;
            SubSys sys = SubSys.Default;
            switch (channel)
            {
                case LaserChannels.ChannelA:
                    sys = SubSys.LaserChA;
                    break;
                case LaserChannels.ChannelB:
                    sys = SubSys.LaserChB;
                    break;
                case LaserChannels.ChannelC:
                    sys = SubSys.LaserChC;
                    break;
                default:
                    return null;
            }
            _SendingFrame.System = sys;
            _SendingFrame.StartingProperty = Properties.OpticalPowerControlKpUpperLimitLessThan15;
            _SendingFrame.PropertyNums = 1;
            _SendingFrame.DataLength = 4;
            _SendingFrame.DataField = new List<byte>(BitConverter.GetBytes((int)(current)));
            //_SendingFrame.DataField = new List<byte>(BitConverter.GetBytes((int)(current * 100)));

            return _SendingFrame.GetBytes();
        }

        public static byte[] SetOpticalPowerControlKpDownLimitLessThan15(LaserChannels channel, double current)
        {
            _SendingFrame.Command = CommandTypes.Write;
            SubSys sys = SubSys.Default;
            switch (channel)
            {
                case LaserChannels.ChannelA:
                    sys = SubSys.LaserChA;
                    break;
                case LaserChannels.ChannelB:
                    sys = SubSys.LaserChB;
                    break;
                case LaserChannels.ChannelC:
                    sys = SubSys.LaserChC;
                    break;
                default:
                    return null;
            }
            _SendingFrame.System = sys;
            _SendingFrame.StartingProperty = Properties.OpticalPowerControlKpDownLimitLessThan15;
            _SendingFrame.PropertyNums = 1;
            _SendingFrame.DataLength = 4;
            _SendingFrame.DataField = new List<byte>(BitConverter.GetBytes((int)(current)));
            //_SendingFrame.DataField = new List<byte>(BitConverter.GetBytes((int)(current * 100)));

            return _SendingFrame.GetBytes();
        }

        public static byte[] SetOpticalPowerControlKiUpperLimitLessThan15(LaserChannels channel, double current)
        {
            _SendingFrame.Command = CommandTypes.Write;
            SubSys sys = SubSys.Default;
            switch (channel)
            {
                case LaserChannels.ChannelA:
                    sys = SubSys.LaserChA;
                    break;
                case LaserChannels.ChannelB:
                    sys = SubSys.LaserChB;
                    break;
                case LaserChannels.ChannelC:
                    sys = SubSys.LaserChC;
                    break;
                default:
                    return null;
            }
            _SendingFrame.System = sys;
            _SendingFrame.StartingProperty = Properties.OpticalPowerControlKiUpperLimitLessThan15;
            _SendingFrame.PropertyNums = 1;
            _SendingFrame.DataLength = 4;
            _SendingFrame.DataField = new List<byte>(BitConverter.GetBytes((int)(current)));
            //_SendingFrame.DataField = new List<byte>(BitConverter.GetBytes((int)(current * 100)));

            return _SendingFrame.GetBytes();
        }

        public static byte[] SetOpticalPowerControlKiDownLimitLessThan15(LaserChannels channel, double current)
        {
            _SendingFrame.Command = CommandTypes.Write;
            SubSys sys = SubSys.Default;
            switch (channel)
            {
                case LaserChannels.ChannelA:
                    sys = SubSys.LaserChA;
                    break;
                case LaserChannels.ChannelB:
                    sys = SubSys.LaserChB;
                    break;
                case LaserChannels.ChannelC:
                    sys = SubSys.LaserChC;
                    break;
                default:
                    return null;
            }
            _SendingFrame.System = sys;
            _SendingFrame.StartingProperty = Properties.OpticalPowerControlKiDownLimitLessThan15;
            _SendingFrame.PropertyNums = 1;
            _SendingFrame.DataLength = 4;
            _SendingFrame.DataField = new List<byte>(BitConverter.GetBytes((int)(current)));
            //_SendingFrame.DataField = new List<byte>(BitConverter.GetBytes((int)(current * 100)));

            return _SendingFrame.GetBytes();
        }

        public static byte[] SetLaserMaximumCurrent(LaserChannels channel, double current)
        {
            _SendingFrame.Command = CommandTypes.Write;
            SubSys sys = SubSys.Default;
            switch (channel)
            {
                case LaserChannels.ChannelA:
                    sys = SubSys.LaserChA;
                    break;
                case LaserChannels.ChannelB:
                    sys = SubSys.LaserChB;
                    break;
                case LaserChannels.ChannelC:
                    sys = SubSys.LaserChC;
                    break;
                default:
                    return null;
            }
            _SendingFrame.System = sys;
            _SendingFrame.StartingProperty = Properties.LaserMaximumCurrent;
            _SendingFrame.PropertyNums = 1;
            _SendingFrame.DataLength = 4;
            _SendingFrame.DataField = new List<byte>(BitConverter.GetBytes((int)(current * 10)));

            return _SendingFrame.GetBytes();
        }
        public static byte[] SetLaserMinimumCurrent(LaserChannels channel, double current)
        {
            _SendingFrame.Command = CommandTypes.Write;
            SubSys sys = SubSys.Default;
            switch (channel)
            {
                case LaserChannels.ChannelA:
                    sys = SubSys.LaserChA;
                    break;
                case LaserChannels.ChannelB:
                    sys = SubSys.LaserChB;
                    break;
                case LaserChannels.ChannelC:
                    sys = SubSys.LaserChC;
                    break;
                default:
                    return null;
            }
            _SendingFrame.System = sys;
            _SendingFrame.StartingProperty = Properties.LaserMinimumCurrent;
            _SendingFrame.PropertyNums = 1;
            _SendingFrame.DataLength = 4;
            _SendingFrame.DataField = new List<byte>(BitConverter.GetBytes((int)(current * 10)));

            return _SendingFrame.GetBytes();
        }

        public static byte[] SetTECControlTemperature(LaserChannels channel, double current)
        {
            _SendingFrame.Command = CommandTypes.Write;
            SubSys sys = SubSys.Default;
            switch (channel)
            {
                case LaserChannels.ChannelA:
                    sys = SubSys.LaserChA;
                    break;
                case LaserChannels.ChannelB:
                    sys = SubSys.LaserChB;
                    break;
                case LaserChannels.ChannelC:
                    sys = SubSys.LaserChC;
                    break;
                default:
                    return null;
            }
            _SendingFrame.System = sys;
            _SendingFrame.StartingProperty = Properties.TECControlTemperature;
            _SendingFrame.PropertyNums = 1;
            _SendingFrame.DataLength = 4;
            _SendingFrame.DataField = new List<byte>(BitConverter.GetBytes((int)(current * 10)));

            return _SendingFrame.GetBytes();
        }
        public static byte[] SetWavelength(LaserChannels channel, double Wavelength)
        {
            _SendingFrame.Command = CommandTypes.Write;
            SubSys sys = SubSys.Default;
            switch (channel)
            {
                case LaserChannels.ChannelA:
                    sys = SubSys.LaserChA;
                    break;
                case LaserChannels.ChannelB:
                    sys = SubSys.LaserChB;
                    break;
                case LaserChannels.ChannelC:
                    sys = SubSys.LaserChC;
                    break;
                default:
                    return null;
            }
            _SendingFrame.System = sys;
            _SendingFrame.StartingProperty = Properties.LaserWaveLength;
            _SendingFrame.PropertyNums = 1;
            _SendingFrame.DataLength = 4;
            _SendingFrame.DataField = new List<byte>(BitConverter.GetBytes((int)(Wavelength)));

            return _SendingFrame.GetBytes();
        }
        public static byte[] SetTECMaximumCurrent(LaserChannels channel, double current)
        {
            _SendingFrame.Command = CommandTypes.Write;
            SubSys sys = SubSys.Default;
            switch (channel)
            {
                case LaserChannels.ChannelA:
                    sys = SubSys.LaserChA;
                    break;
                case LaserChannels.ChannelB:
                    sys = SubSys.LaserChB;
                    break;
                case LaserChannels.ChannelC:
                    sys = SubSys.LaserChC;
                    break;
                default:
                    return null;
            }
            _SendingFrame.System = sys;
            _SendingFrame.StartingProperty = Properties.TECMaximumCurrent;
            _SendingFrame.PropertyNums = 1;
            _SendingFrame.DataLength = 4;
            _SendingFrame.DataField = new List<byte>(BitConverter.GetBytes((int)(current)));

            return _SendingFrame.GetBytes();
        }

        public static byte[] SetTECControlKp(LaserChannels channel, double current)
        {
            _SendingFrame.Command = CommandTypes.Write;
            SubSys sys = SubSys.Default;
            switch (channel)
            {
                case LaserChannels.ChannelA:
                    sys = SubSys.LaserChA;
                    break;
                case LaserChannels.ChannelB:
                    sys = SubSys.LaserChB;
                    break;
                case LaserChannels.ChannelC:
                    sys = SubSys.LaserChC;
                    break;
                default:
                    return null;
            }
            _SendingFrame.System = sys;
            _SendingFrame.StartingProperty = Properties.TECControlKp;
            _SendingFrame.PropertyNums = 1;
            _SendingFrame.DataLength = 4;
            _SendingFrame.DataField = new List<byte>(BitConverter.GetBytes((int)(current)));

            return _SendingFrame.GetBytes();
        }

        public static byte[] SetTECControlKi(LaserChannels channel, double current)
        {
            _SendingFrame.Command = CommandTypes.Write;
            SubSys sys = SubSys.Default;
            switch (channel)
            {
                case LaserChannels.ChannelA:
                    sys = SubSys.LaserChA;
                    break;
                case LaserChannels.ChannelB:
                    sys = SubSys.LaserChB;
                    break;
                case LaserChannels.ChannelC:
                    sys = SubSys.LaserChC;
                    break;
                default:
                    return null;
            }
            _SendingFrame.System = sys;
            _SendingFrame.StartingProperty = Properties.TECControlKi;
            _SendingFrame.PropertyNums = 1;
            _SendingFrame.DataLength = 4;
            _SendingFrame.DataField = new List<byte>(BitConverter.GetBytes((int)(current)));

            return _SendingFrame.GetBytes();
        }

        public static byte[] SetTECControlKd(LaserChannels channel, double current)
        {
            _SendingFrame.Command = CommandTypes.Write;
            SubSys sys = SubSys.Default;
            switch (channel)
            {
                case LaserChannels.ChannelA:
                    sys = SubSys.LaserChA;
                    break;
                case LaserChannels.ChannelB:
                    sys = SubSys.LaserChB;
                    break;
                case LaserChannels.ChannelC:
                    sys = SubSys.LaserChC;
                    break;
                default:
                    return null;
            }
            _SendingFrame.System = sys;
            _SendingFrame.StartingProperty = Properties.TECControlKd;
            _SendingFrame.PropertyNums = 1;
            _SendingFrame.DataLength = 4;
            _SendingFrame.DataField = new List<byte>(BitConverter.GetBytes((int)(current)));

            return _SendingFrame.GetBytes();
        }

        public static byte[] SetPMTCoefficient(IVChannels channel, double current)
        {
            _SendingFrame.Command = CommandTypes.Write;
            SubSys sys = SubSys.Default;
            switch (channel)
            {
                case IVChannels.ChannelA:
                    sys = SubSys.Iv_ChA;
                    break;
                case IVChannels.ChannelB:
                    sys = SubSys.Iv_ChB;
                    break;
                case IVChannels.ChannelC:
                    sys = SubSys.Iv_ChC;
                    break;
                default:
                    return null;
            }
            _SendingFrame.System = sys;
            _SendingFrame.StartingProperty = Properties.PMTCompensationCoefficient;
            _SendingFrame.PropertyNums = 1;
            _SendingFrame.DataLength = 2;
            //double _tempNumber = current * 10000;
            string hexString = string.Format("{0:X8}", (uint)current);
            byte[] _tempDataField = Enumerable.Range(0, hexString.Length)
                  .Where(x => x % 2 == 0)
                  .Select(x => Convert.ToByte(hexString.Substring(x, 2), 16))
                  .ToArray();
            byte[] DataField = new byte[2];
            DataField[1] = _tempDataField[2];
            DataField[0] = _tempDataField[3];
            _SendingFrame.DataField = new List<byte>(DataField);

            return _SendingFrame.GetBytes();
        }

        #endregion

        internal static bool ResponseDecoding(byte[] rxbuf, int index)
        {
            if (index < 10) { return false; }
            int offset = 0;
            do
            {
                if (rxbuf[offset] == 0x6B)
                {
                    CommandTypes commandType = (CommandTypes)(rxbuf[offset + 1] & 0x03);
                    SubSys subSys = (SubSys)rxbuf[offset + 2];
                    Properties property = (Properties)rxbuf[offset + 3];
                    int propertyNums = rxbuf[offset + 4];
                    #region Read response
                    if (commandType == CommandTypes.Read)
                    {
                        int dataFieldLength = BitConverter.ToUInt16(rxbuf, offset + 5);
                        if (index < offset + 8 + dataFieldLength) { return false; }
                        if (rxbuf[offset + 7 + dataFieldLength] != 0x6F) { return false; }
                        OpticalModulePowerMonitor = (((rxbuf[offset + 1] >> 2) & 0x01) == 0x01);
                        DevicePowerStatus = !(((rxbuf[offset + 1] >> 3) & 0x01) == 0x01);
                        LidIsOpen = !(((rxbuf[offset + 1] >> 4) & 0x01) == 0x01);
                        TopCoverLock = (((rxbuf[offset + 1] >> 5) & 0x01) == 0x01);
                        TopMagneticState = (((rxbuf[offset + 1] >> 6) & 0x01) == 0x01);
                        OpticalModulePowerStatus = (((rxbuf[offset + 1] >> 7) & 0x01) == 0x01);
                        int dataFieldOffset = offset + 7;
                        offset += 8 + dataFieldLength;
                        #region Mainboard
                        if (subSys == SubSys.Mainboard)
                        {
                            for (int i = 0; i < propertyNums; i++)
                            {
                                switch (property + i)
                                {
                                    case Properties.HWVersion:
                                        HWVersion = string.Format("{0}.{1}.{2}.{3}", rxbuf[dataFieldOffset], rxbuf[dataFieldOffset + 1], rxbuf[dataFieldOffset + 2], rxbuf[dataFieldOffset + 3]);
                                        dataFieldOffset += 4;
                                        break;
                                    case Properties.FWVersion:
                                        FWVersion = string.Format("{0}.{1}.{2}.{3}", rxbuf[dataFieldOffset], rxbuf[dataFieldOffset + 1], rxbuf[dataFieldOffset + 2], rxbuf[dataFieldOffset + 3]);
                                        dataFieldOffset += 4;
                                        break;
                                    case Properties.IndividualParameter:
                                        Buffer.BlockCopy(rxbuf, dataFieldOffset, _IndividualParameter, 0, 256);
                                        int size = Marshal.SizeOf(typeof(AvocadoDeviceProperties));
                                        IntPtr structPtr = Marshal.AllocHGlobal(size);
                                        Marshal.Copy(_IndividualParameter, 0, structPtr, size);
                                        DeviceProperties = (AvocadoDeviceProperties)Marshal.PtrToStructure(structPtr, typeof(AvocadoDeviceProperties));
                                        Marshal.FreeHGlobal(structPtr);
                                        dataFieldOffset += 256;
                                        break;
                                    case Properties.XEncoderSubdivision:
                                        XEncoderSubdivision = BitConverter.ToUInt32(rxbuf, dataFieldOffset);
                                        dataFieldOffset += 4;
                                        break;
                                    case Properties.AmbientTemperature:
                                        AmbientTemperature[AmbientTemperatureChannel.CH1] = BitConverter.ToUInt16(rxbuf, dataFieldOffset) * 0.1;
                                        dataFieldOffset += 2;
                                        AmbientTemperature[AmbientTemperatureChannel.CH2] = BitConverter.ToUInt16(rxbuf, dataFieldOffset) * 0.1;
                                        dataFieldOffset += 2;
                                        break;
                                    case Properties.Register2Control:
                                        Register2ControlStatus = BitConverter.ToUInt16(rxbuf, dataFieldOffset);
                                        dataFieldOffset += 2;
                                        break;
                                }
                            }
                        }
                        #endregion Mainboard
                        #region Motion
                        else if (((byte)subSys & 0x0f) == 0x02)      // motor system
                        {
                            bool containsX = ((byte)subSys & 0x10) == 0x10;
                            bool containsY = ((byte)subSys & 0x20) == 0x20;
                            bool containsZ = ((byte)subSys & 0x40) == 0x40;
                            bool containsW = ((byte)subSys & 0x80) == 0x80;

                            for (int i = 0; i < propertyNums; i++)
                            {
                                switch (property + i)
                                {
                                    case Properties.CrntPos:
                                        if (containsX)
                                        {
                                            MotionCrntPositions[MotorTypes.X] = BitConverter.ToInt32(rxbuf, dataFieldOffset);
                                            dataFieldOffset += 4;
                                        }
                                        if (containsY)
                                        {
                                            MotionCrntPositions[MotorTypes.Y] = BitConverter.ToInt32(rxbuf, dataFieldOffset);
                                            dataFieldOffset += 4;
                                        }
                                        if (containsZ)
                                        {
                                            MotionCrntPositions[MotorTypes.Z] = BitConverter.ToInt32(rxbuf, dataFieldOffset);
                                            dataFieldOffset += 4;
                                        }
                                        if (containsW)
                                        {
                                            MotionCrntPositions[MotorTypes.W] = BitConverter.ToInt32(rxbuf, dataFieldOffset);
                                            dataFieldOffset += 4;
                                        }
                                        break;
                                    case Properties.CrntStates:
                                        if (containsX)
                                        {
                                            MotionStates[MotorTypes.X] = MotionState.MapFromByte(rxbuf[dataFieldOffset]);
                                            dataFieldOffset += 2;
                                        }
                                        if (containsY)
                                        {
                                            MotionStates[MotorTypes.Y] = MotionState.MapFromByte(rxbuf[dataFieldOffset]);
                                            dataFieldOffset += 2;
                                        }
                                        if (containsZ)
                                        {
                                            MotionStates[MotorTypes.Z] = MotionState.MapFromByte(rxbuf[dataFieldOffset]);
                                            dataFieldOffset += 2;
                                        }
                                        if (containsW)
                                        {
                                            MotionStates[MotorTypes.W] = MotionState.MapFromByte(rxbuf[dataFieldOffset]);
                                            dataFieldOffset += 2;
                                        }
                                        break;
                                }
                            }
                            OnReceivedMotionData?.Invoke();
                        }
                        #endregion Motion
                        #region Scan data
                        else if (subSys == SubSys.ScanControl)
                        {
                            if (property == Properties.ScanData)
                            {
                                dataFieldOffset += 4;   // first 4 bytes are sample counts, here ignored since it's single frame
                                SingleSampleChA = BitConverter.ToUInt32(rxbuf, dataFieldOffset);
                                dataFieldOffset += 4;
                                SingleSampleChB = BitConverter.ToUInt32(rxbuf, dataFieldOffset);
                                dataFieldOffset += 4;
                                SingleSampleChC = BitConverter.ToUInt32(rxbuf, dataFieldOffset);
                                dataFieldOffset += 4;
                                OnRecievedSingleSampleData?.Invoke();
                            }
                        }
                        #endregion Scan data
                        #region IV Module
                        else if (((byte)subSys & 0x0f) == 0x05)
                        {
                            bool containsA = ((byte)subSys & 0x10) == 0x10;
                            bool containsB = ((byte)subSys & 0x20) == 0x20;
                            bool containsC = ((byte)subSys & 0x40) == 0x40;

                            for (int i = 0; i < propertyNums; i++)
                            {
                                switch (property + i)
                                {

                                    case Properties.OpticalModuleSerialNumber:
                                        if (containsC)
                                        {
                                            byte[] _temp = { rxbuf[10], rxbuf[9] };
                                            string strtemp = rxbuf[7].ToString();
                                            if (strtemp.Length == 1)
                                            {
                                                strtemp = "0" + strtemp;
                                            }
                                            IVOpticalModuleSerialNumber[IVChannels.ChannelC] = string.Format("{0}", BitConverter.ToString(_temp).Replace("-", "") + rxbuf[8].ToString() + strtemp);
                                        }
                                        if (containsB)
                                        {
                                            byte[] _temp = { rxbuf[10], rxbuf[9] };
                                            string strtemp = rxbuf[7].ToString();
                                            if (strtemp.Length == 1)
                                            {
                                                strtemp = "0" + strtemp;
                                            }
                                            IVOpticalModuleSerialNumber[IVChannels.ChannelB] = string.Format("{0}", BitConverter.ToString(_temp).Replace("-", "") + rxbuf[8].ToString() + strtemp);
                                        }
                                        if (containsA)
                                        {
                                            byte[] _temp = { rxbuf[10], rxbuf[9] };
                                            string strtemp = rxbuf[7].ToString();
                                            if (strtemp.Length == 1)
                                            {
                                                strtemp = "0" + strtemp;
                                            }
                                            IVOpticalModuleSerialNumber[IVChannels.ChannelA] = string.Format("{0}", BitConverter.ToString(_temp).Replace("-", "") + rxbuf[8].ToString() + strtemp);
                                        }
                                        break;
                                    case Properties.IVEstimatedVersionNumberBoard:
                                        if (containsA)
                                        {
                                            byte[] _temp = { rxbuf[10], rxbuf[9], rxbuf[8], rxbuf[7] };
                                            IVEstimatedVersionNumberBoard[IVChannels.ChannelA] = string.Format("{0}", BitConverter.ToString(_temp).Replace("-", ""));
                                        }
                                        if (containsB)
                                        {
                                            byte[] _temp = { rxbuf[10], rxbuf[9], rxbuf[8], rxbuf[7] };
                                            IVEstimatedVersionNumberBoard[IVChannels.ChannelB] = string.Format("{0}", BitConverter.ToString(_temp).Replace("-", ""));
                                        }
                                        if (containsC)
                                        {
                                            byte[] _temp = { rxbuf[10], rxbuf[9], rxbuf[8], rxbuf[7] };
                                            IVEstimatedVersionNumberBoard[IVChannels.ChannelC] = string.Format("{0}", BitConverter.ToString(_temp).Replace("-", ""));
                                        }
                                        break;
                                    case Properties.ErrorCode:
                                        if (containsA)
                                        {
                                            IVErrorCode[IVChannels.ChannelA] = rxbuf[dataFieldOffset].ToString();
                                            dataFieldOffset += 2;
                                        }
                                        if (containsB)
                                        {
                                            IVErrorCode[IVChannels.ChannelB] = rxbuf[dataFieldOffset].ToString();
                                            dataFieldOffset += 2;
                                        }
                                        if (containsC)
                                        {
                                            IVErrorCode[IVChannels.ChannelC] = rxbuf[dataFieldOffset].ToString();
                                            dataFieldOffset += 2;
                                        }
                                        break;
                                    case Properties.IvSensorType:
                                        if (containsA)
                                        {
                                            IvSensorTypes[IVChannels.ChannelA] = (IvSensorType)rxbuf[dataFieldOffset];
                                            dataFieldOffset += 2;
                                        }
                                        if (containsB)
                                        {
                                            IvSensorTypes[IVChannels.ChannelB] = (IvSensorType)rxbuf[dataFieldOffset];
                                            dataFieldOffset += 2;
                                        }
                                        if (containsC)
                                        {
                                            IvSensorTypes[IVChannels.ChannelC] = (IvSensorType)rxbuf[dataFieldOffset];
                                            dataFieldOffset += 2;
                                        }
                                        break;
                                    case Properties.IvSensorSN:
                                        if (containsA)
                                        {
                                            IvSensorSerialNumbers[IVChannels.ChannelA] = BitConverter.ToUInt32(rxbuf, dataFieldOffset);
                                            dataFieldOffset += 4;
                                        }
                                        if (containsB)
                                        {
                                            IvSensorSerialNumbers[IVChannels.ChannelB] = BitConverter.ToUInt32(rxbuf, dataFieldOffset);
                                            dataFieldOffset += 4;
                                        }
                                        if (containsC)
                                        {
                                            IvSensorSerialNumbers[IVChannels.ChannelC] = BitConverter.ToUInt32(rxbuf, dataFieldOffset);
                                            dataFieldOffset += 4;
                                        }
                                        break;
                                    case Properties.TempSenser1282AD:
                                        if (containsA)
                                        {
                                            TempSenser1282AD[IVChannels.ChannelA] = BitConverter.ToUInt16(rxbuf, dataFieldOffset) * 0.01;
                                            dataFieldOffset += 2;
                                        }
                                        if (containsB)
                                        {
                                            TempSenser1282AD[IVChannels.ChannelB] = BitConverter.ToUInt16(rxbuf, dataFieldOffset) * 0.01;
                                            dataFieldOffset += 2;
                                        }
                                        if (containsC)
                                        {
                                            TempSenser1282AD[IVChannels.ChannelC] = BitConverter.ToUInt16(rxbuf, dataFieldOffset) * 0.01;
                                            dataFieldOffset += 2;
                                        }
                                        break;
                                    case Properties.TempSenser6459AD:
                                        if (containsA)
                                        {
                                            TempSenser6459AD[IVChannels.ChannelA] = BitConverter.ToUInt16(rxbuf, dataFieldOffset) * 0.01;
                                            dataFieldOffset += 2;
                                        }
                                        if (containsB)
                                        {
                                            TempSenser6459AD[IVChannels.ChannelB] = BitConverter.ToUInt16(rxbuf, dataFieldOffset) * 0.01;
                                            dataFieldOffset += 2;
                                        }
                                        if (containsC)
                                        {
                                            TempSenser6459AD[IVChannels.ChannelC] = BitConverter.ToUInt16(rxbuf, dataFieldOffset) * 0.01;
                                            dataFieldOffset += 2;
                                        }
                                        break;
                                    case Properties.LightIntensityCalibrationTemperature:
                                        if (containsA)
                                        {
                                            LightIntensityCalibrationTemperature[IVChannels.ChannelA] = BitConverter.ToUInt16(rxbuf, dataFieldOffset) * 0.01;
                                            dataFieldOffset += 2;
                                        }
                                        if (containsB)
                                        {
                                            LightIntensityCalibrationTemperature[IVChannels.ChannelB] = BitConverter.ToUInt16(rxbuf, dataFieldOffset) * 0.01;
                                            dataFieldOffset += 2;
                                        }
                                        if (containsC)
                                        {
                                            LightIntensityCalibrationTemperature[IVChannels.ChannelC] = BitConverter.ToUInt16(rxbuf, dataFieldOffset) * 0.01;
                                            dataFieldOffset += 2;
                                        }
                                        break;
                                    case Properties.APDGainCalVol:
                                        if (containsA)
                                        {
                                            APDGainCalVol[IVChannels.ChannelA] = BitConverter.ToUInt16(rxbuf, dataFieldOffset + 2) * 0.01;
                                            dataFieldOffset += 4;
                                        }
                                        if (containsB)
                                        {
                                            APDGainCalVol[IVChannels.ChannelB] = BitConverter.ToUInt16(rxbuf, dataFieldOffset + 2) * 0.01;
                                            dataFieldOffset += 4;
                                        }
                                        if (containsC)
                                        {
                                            APDGainCalVol[IVChannels.ChannelC] = BitConverter.ToUInt16(rxbuf, dataFieldOffset + 2) * 0.01;
                                            dataFieldOffset += 4;
                                        }
                                        break;
                                    case Properties.PMTCompensationCoefficient:
                                        if (containsA)
                                        {
                                            PMTCompensationCoefficient[IVChannels.ChannelA] = BitConverter.ToInt16(rxbuf, dataFieldOffset);
                                            dataFieldOffset += 2;
                                        }
                                        if (containsB)
                                        {
                                            PMTCompensationCoefficient[IVChannels.ChannelB] = BitConverter.ToInt16(rxbuf, dataFieldOffset);
                                            dataFieldOffset += 2;
                                        }
                                        if (containsC)
                                        {
                                            PMTCompensationCoefficient[IVChannels.ChannelC] = BitConverter.ToInt16(rxbuf, dataFieldOffset);
                                            dataFieldOffset += 2;
                                        }
                                        break;
                                }
                            }
                        }
                        #endregion IV Module
                        #region Laser Module
                        else if (((byte)subSys & 0x0f) == 0x06)
                        {
                            bool containsA = ((byte)subSys & 0x10) == 0x10;
                            bool containsB = ((byte)subSys & 0x20) == 0x20;
                            bool containsC = ((byte)subSys & 0x40) == 0x40;

                            for (int i = 0; i < propertyNums; i++)
                            {
                                switch (property + i)
                                {
                                    case Properties.LaserBoardFirmwareVersionNumber:
                                        if (containsA)
                                        {
                                            byte[] _temp = { rxbuf[10], rxbuf[9], rxbuf[8], rxbuf[7] };
                                            IVEstimatedVersionNumberBoard[IVChannels.ChannelA] = string.Format("{0}", BitConverter.ToString(_temp).Replace("-", ""));
                                        }
                                        if (containsB)
                                        {
                                            byte[] _temp = { rxbuf[10], rxbuf[9], rxbuf[8], rxbuf[7] };
                                            IVEstimatedVersionNumberBoard[IVChannels.ChannelB] = string.Format("{0}", BitConverter.ToString(_temp).Replace("-", ""));
                                        }
                                        if (containsC)
                                        {
                                            byte[] _temp = { rxbuf[10], rxbuf[9], rxbuf[8], rxbuf[7] };
                                            IVEstimatedVersionNumberBoard[IVChannels.ChannelC] = string.Format("{0}", BitConverter.ToString(_temp).Replace("-", ""));
                                        }
                                        break;
                                    case Properties.LaserOpticalModuleSerialNumber:
                                        if (containsA)
                                        {
                                            byte[] _temp = { rxbuf[10], rxbuf[9] };
                                            string strtemp = rxbuf[7].ToString();
                                            if (strtemp.Length == 1)
                                            {
                                                strtemp = "0" + strtemp;
                                            }
                                            IVOpticalModuleSerialNumber[IVChannels.ChannelA] = string.Format("{0}", BitConverter.ToString(_temp).Replace("-", "") + rxbuf[8].ToString() + strtemp);
                                        }
                                        if (containsB)
                                        {
                                            byte[] _temp = { rxbuf[10], rxbuf[9] };
                                            string strtemp = rxbuf[7].ToString();
                                            if (strtemp.Length == 1)
                                            {
                                                strtemp = "0" + strtemp;
                                            }
                                            IVOpticalModuleSerialNumber[IVChannels.ChannelB] = string.Format("{0}", BitConverter.ToString(_temp).Replace("-", "") + rxbuf[8].ToString() + strtemp);
                                        }
                                        if (containsC)
                                        {
                                            byte[] _temp = { rxbuf[10], rxbuf[9] };
                                            string strtemp = rxbuf[7].ToString();
                                            if (strtemp.Length == 1)
                                            {
                                                strtemp = "0" + strtemp;
                                            }
                                            IVOpticalModuleSerialNumber[IVChannels.ChannelC] = string.Format("{0}", BitConverter.ToString(_temp).Replace("-", "") + rxbuf[8].ToString() + strtemp);
                                        }
                                        break;
                                    case Properties.LaserErrorCode:
                                        if (containsA)
                                        {
                                            LaserErrorCode[LaserChannels.ChannelA] = rxbuf[dataFieldOffset].ToString();
                                            dataFieldOffset += 4;
                                        }
                                        if (containsB)
                                        {
                                            LaserErrorCode[LaserChannels.ChannelB] = rxbuf[dataFieldOffset].ToString();
                                            dataFieldOffset += 4;
                                        }
                                        if (containsC)
                                        {
                                            LaserErrorCode[LaserChannels.ChannelC] = rxbuf[dataFieldOffset].ToString();
                                            dataFieldOffset += 4;
                                        }
                                        break;
                                    case Properties.LaserSensorSN:
                                        if (containsA)
                                        {
                                            LaserSerialNumbers[LaserChannels.ChannelA] = BitConverter.ToUInt32(rxbuf, dataFieldOffset);
                                            dataFieldOffset += 4;
                                        }
                                        if (containsB)
                                        {
                                            LaserSerialNumbers[LaserChannels.ChannelB] = BitConverter.ToUInt32(rxbuf, dataFieldOffset);
                                            dataFieldOffset += 4;
                                        }
                                        if (containsC)
                                        {
                                            LaserSerialNumbers[LaserChannels.ChannelC] = BitConverter.ToUInt32(rxbuf, dataFieldOffset);
                                            dataFieldOffset += 4;
                                        }
                                        break;
                                    case Properties.LaserWaveLength:
                                        if (containsA)
                                        {
                                            LaserWaveLengths[LaserChannels.ChannelA] = BitConverter.ToUInt32(rxbuf, dataFieldOffset);
                                            dataFieldOffset += 4;
                                        }
                                        if (containsB)
                                        {
                                            LaserWaveLengths[LaserChannels.ChannelB] = BitConverter.ToUInt32(rxbuf, dataFieldOffset);
                                            dataFieldOffset += 4;
                                        }
                                        if (containsC)
                                        {
                                            LaserWaveLengths[LaserChannels.ChannelC] = BitConverter.ToUInt32(rxbuf, dataFieldOffset);
                                            dataFieldOffset += 4;
                                        }
                                        break;
                                    case Properties.LaserTemper:
                                        if (containsA)
                                        {
                                            LaserTemperatures[LaserChannels.ChannelA] = BitConverter.ToInt16(rxbuf, dataFieldOffset) * 0.1;
                                            dataFieldOffset += 4;
                                        }
                                        if (containsB)
                                        {
                                            LaserTemperatures[LaserChannels.ChannelB] = BitConverter.ToInt16(rxbuf, dataFieldOffset) * 0.1;
                                            dataFieldOffset += 4;
                                        }
                                        if (containsC)
                                        {
                                            LaserTemperatures[LaserChannels.ChannelC] = BitConverter.ToInt16(rxbuf, dataFieldOffset) * 0.1;
                                            dataFieldOffset += 4;
                                        }
                                        break;
                                    case Properties.TECControlTemperature:
                                        if (containsA)
                                        {
                                            TECControlTemperature[LaserChannels.ChannelA] = BitConverter.ToUInt16(rxbuf, dataFieldOffset) * 0.1;
                                            dataFieldOffset += 2;
                                        }
                                        if (containsB)
                                        {
                                            TECControlTemperature[LaserChannels.ChannelB] = BitConverter.ToUInt16(rxbuf, dataFieldOffset) * 0.1;
                                            dataFieldOffset += 2;
                                        }
                                        if (containsC)
                                        {
                                            TECControlTemperature[LaserChannels.ChannelC] = BitConverter.ToUInt16(rxbuf, dataFieldOffset) * 0.1;
                                            dataFieldOffset += 2;
                                        }
                                        break;
                                    case Properties.RadiatorTemper:
                                        if (containsA)
                                        {
                                            RadiatorTemperatures[LaserChannels.ChannelA] = BitConverter.ToInt16(rxbuf, dataFieldOffset) * 0.1;
                                            dataFieldOffset += 4;
                                        }
                                        if (containsB)
                                        {
                                            RadiatorTemperatures[LaserChannels.ChannelB] = BitConverter.ToInt16(rxbuf, dataFieldOffset) * 0.1;
                                            dataFieldOffset += 4;
                                        }
                                        if (containsC)
                                        {
                                            RadiatorTemperatures[LaserChannels.ChannelC] = BitConverter.ToInt16(rxbuf, dataFieldOffset) * 0.1;
                                            dataFieldOffset += 4;
                                        }
                                        break;
                                    case Properties.ReserveFan:
                                        if (containsA)
                                        {
                                            FanTemperatures[LaserChannels.ChannelA] = BitConverter.ToInt32(rxbuf, dataFieldOffset) * 0.1;
                                            dataFieldOffset += 4;
                                        }
                                        if (containsB)
                                        {
                                            FanTemperatures[LaserChannels.ChannelB] = BitConverter.ToInt32(rxbuf, dataFieldOffset) * 0.1;
                                            dataFieldOffset += 4;
                                        }
                                        if (containsC)
                                        {
                                            FanTemperatures[LaserChannels.ChannelC] = BitConverter.ToInt32(rxbuf, dataFieldOffset) * 0.1;
                                            dataFieldOffset += 4;
                                        }
                                        break;
                                    case Properties.LaserCalPower:
                                        if (containsA)
                                        {

                                            AllIntensity[LaserChannels.ChannelA] = BitConverter.ToInt16(rxbuf, dataFieldOffset + 2);
                                            dataFieldOffset += 4;
                                        }
                                        if (containsB)
                                        {
                                            AllIntensity[LaserChannels.ChannelB] = BitConverter.ToInt16(rxbuf, dataFieldOffset + 2);
                                            dataFieldOffset += 4;
                                        }
                                        if (containsC)
                                        {
                                            AllIntensity[LaserChannels.ChannelC] = BitConverter.ToInt16(rxbuf, dataFieldOffset + 2);
                                            dataFieldOffset += 4;
                                        }
                                        break;
                                    case Properties.TECMaximumCurrent:
                                        if (containsA)
                                        {

                                            TECMaximumCurrent[LaserChannels.ChannelA] = BitConverter.ToUInt32(rxbuf, dataFieldOffset);
                                            dataFieldOffset += 4;
                                        }
                                        if (containsB)
                                        {

                                            TECMaximumCurrent[LaserChannels.ChannelB] = BitConverter.ToUInt32(rxbuf, dataFieldOffset);
                                            dataFieldOffset += 4;
                                        }
                                        if (containsC)
                                        {
                                            TECMaximumCurrent[LaserChannels.ChannelC] = BitConverter.ToUInt32(rxbuf, dataFieldOffset);
                                            dataFieldOffset += 4;
                                        }
                                        break;
                                    case Properties.TECControlKp:
                                        if (containsA)
                                        {

                                            TECRefrigerationControlParameterKp[LaserChannels.ChannelA] = BitConverter.ToUInt32(rxbuf, dataFieldOffset);
                                            dataFieldOffset += 4;
                                        }
                                        if (containsB)
                                        {

                                            TECRefrigerationControlParameterKp[LaserChannels.ChannelB] = BitConverter.ToUInt32(rxbuf, dataFieldOffset);
                                            dataFieldOffset += 4;
                                        }
                                        if (containsC)
                                        {
                                            TECRefrigerationControlParameterKp[LaserChannels.ChannelC] = BitConverter.ToUInt32(rxbuf, dataFieldOffset);
                                            dataFieldOffset += 4;
                                        }
                                        break;
                                    case Properties.TECControlKi:
                                        if (containsA)
                                        {

                                            TECRefrigerationControlParameterKi[LaserChannels.ChannelA] = BitConverter.ToUInt32(rxbuf, dataFieldOffset);
                                            dataFieldOffset += 4;
                                        }
                                        if (containsB)
                                        {

                                            TECRefrigerationControlParameterKi[LaserChannels.ChannelB] = BitConverter.ToUInt32(rxbuf, dataFieldOffset);
                                            dataFieldOffset += 4;
                                        }
                                        if (containsC)
                                        {
                                            TECRefrigerationControlParameterKi[LaserChannels.ChannelC] = BitConverter.ToUInt32(rxbuf, dataFieldOffset);
                                            dataFieldOffset += 4;
                                        }
                                        break;
                                    case Properties.TECControlKd:
                                        if (containsA)
                                        {

                                            TECRefrigerationControlParameterKd[LaserChannels.ChannelA] = BitConverter.ToUInt32(rxbuf, dataFieldOffset);
                                            dataFieldOffset += 4;
                                        }
                                        if (containsB)
                                        {

                                            TECRefrigerationControlParameterKd[LaserChannels.ChannelB] = BitConverter.ToUInt32(rxbuf, dataFieldOffset);
                                            dataFieldOffset += 4;
                                        }
                                        if (containsC)
                                        {
                                            TECRefrigerationControlParameterKd[LaserChannels.ChannelC] = BitConverter.ToUInt32(rxbuf, dataFieldOffset);
                                            dataFieldOffset += 4;
                                        }
                                        break;
                                    case Properties.Laser532CurrentPower:
                                        if (containsA)
                                        {

                                            AllCurrentLightPower[LaserChannels.ChannelA] = BitConverter.ToInt32(rxbuf, dataFieldOffset);
                                            dataFieldOffset += 4;
                                        }
                                        if (containsB)
                                        {

                                            AllCurrentLightPower[LaserChannels.ChannelB] = BitConverter.ToInt32(rxbuf, dataFieldOffset);
                                            dataFieldOffset += 4;
                                        }
                                        if (containsC)
                                        {
                                            AllCurrentLightPower[LaserChannels.ChannelC] = BitConverter.ToInt32(rxbuf, dataFieldOffset);
                                            dataFieldOffset += 4;
                                        }
                                        break;
                                    case Properties.OpticalPowerLessThanOrEqual15mWKp:
                                        if (containsA)
                                        {

                                            AllOpticalPowerLessThanOrEqual15mWKp[LaserChannels.ChannelA] = BitConverter.ToInt32(rxbuf, dataFieldOffset);
                                            dataFieldOffset += 4;
                                        }
                                        if (containsB)
                                        {

                                            AllOpticalPowerLessThanOrEqual15mWKp[LaserChannels.ChannelB] = BitConverter.ToInt32(rxbuf, dataFieldOffset);
                                            dataFieldOffset += 4;
                                        }
                                        if (containsC)
                                        {
                                            AllOpticalPowerLessThanOrEqual15mWKp[LaserChannels.ChannelC] = BitConverter.ToInt32(rxbuf, dataFieldOffset);
                                            dataFieldOffset += 4;
                                        }
                                        break;
                                    case Properties.OpticalPowerLessThanOrEqual15mWKi:
                                        if (containsA)
                                        {

                                            AllOpticalPowerLessThanOrEqual15mWKi[LaserChannels.ChannelA] = BitConverter.ToInt32(rxbuf, dataFieldOffset);
                                            dataFieldOffset += 4;
                                        }
                                        if (containsB)
                                        {

                                            AllOpticalPowerLessThanOrEqual15mWKi[LaserChannels.ChannelB] = BitConverter.ToInt32(rxbuf, dataFieldOffset);
                                            dataFieldOffset += 4;
                                        }
                                        if (containsC)
                                        {
                                            AllOpticalPowerLessThanOrEqual15mWKi[LaserChannels.ChannelC] = BitConverter.ToInt32(rxbuf, dataFieldOffset);
                                            dataFieldOffset += 4;
                                        }
                                        break;
                                    case Properties.OpticalPowerLessThanOrEqual15mWKd:
                                        if (containsA)
                                        {

                                            AllOpticalPowerLessThanOrEqual15mWKd[LaserChannels.ChannelA] = BitConverter.ToInt32(rxbuf, dataFieldOffset);
                                            dataFieldOffset += 4;
                                        }
                                        if (containsB)
                                        {

                                            AllOpticalPowerLessThanOrEqual15mWKd[LaserChannels.ChannelB] = BitConverter.ToInt32(rxbuf, dataFieldOffset);
                                            dataFieldOffset += 4;
                                        }
                                        if (containsC)
                                        {
                                            AllOpticalPowerLessThanOrEqual15mWKd[LaserChannels.ChannelC] = BitConverter.ToInt32(rxbuf, dataFieldOffset);
                                            dataFieldOffset += 4;
                                        }
                                        break;
                                    case Properties.OpticalPowerGreaterThan15mWKp:
                                        if (containsA)
                                        {

                                            AllOpticalPowerGreaterThan15mWKp[LaserChannels.ChannelA] = BitConverter.ToInt32(rxbuf, dataFieldOffset);
                                            dataFieldOffset += 4;
                                        }
                                        if (containsB)
                                        {

                                            AllOpticalPowerGreaterThan15mWKp[LaserChannels.ChannelB] = BitConverter.ToInt32(rxbuf, dataFieldOffset);
                                            dataFieldOffset += 4;
                                        }
                                        if (containsC)
                                        {
                                            AllOpticalPowerGreaterThan15mWKp[LaserChannels.ChannelC] = BitConverter.ToInt32(rxbuf, dataFieldOffset);
                                            dataFieldOffset += 4;
                                        }
                                        break;
                                    case Properties.OpticalPowerGreaterThan15mWKi:
                                        if (containsA)
                                        {

                                            AllOpticalPowerGreaterThan15mWKi[LaserChannels.ChannelA] = BitConverter.ToInt32(rxbuf, dataFieldOffset);
                                            dataFieldOffset += 4;
                                        }
                                        if (containsB)
                                        {

                                            AllOpticalPowerGreaterThan15mWKi[LaserChannels.ChannelB] = BitConverter.ToInt32(rxbuf, dataFieldOffset);
                                            dataFieldOffset += 4;
                                        }
                                        if (containsC)
                                        {
                                            AllOpticalPowerGreaterThan15mWKi[LaserChannels.ChannelC] = BitConverter.ToInt32(rxbuf, dataFieldOffset);
                                            dataFieldOffset += 4;
                                        }
                                        break;
                                    case Properties.OpticalPowerGreaterThan15mWKd:
                                        if (containsA)
                                        {

                                            AllOpticalPowerGreaterThan15mWKd[LaserChannels.ChannelA] = BitConverter.ToInt32(rxbuf, dataFieldOffset);
                                            dataFieldOffset += 4;
                                        }
                                        if (containsB)
                                        {

                                            AllOpticalPowerGreaterThan15mWKd[LaserChannels.ChannelB] = BitConverter.ToInt32(rxbuf, dataFieldOffset);
                                            dataFieldOffset += 4;
                                        }
                                        if (containsC)
                                        {
                                            AllOpticalPowerGreaterThan15mWKd[LaserChannels.ChannelC] = BitConverter.ToInt32(rxbuf, dataFieldOffset);
                                            dataFieldOffset += 4;
                                        }
                                        break;
                                    case Properties.RadioDiodeVoltage:
                                        if (containsA)
                                        {

                                            AllRadioDiodeVoltage[LaserChannels.ChannelA] = BitConverter.ToInt32(rxbuf, dataFieldOffset);
                                            dataFieldOffset += 4;
                                        }
                                        if (containsB)
                                        {

                                            AllRadioDiodeVoltage[LaserChannels.ChannelB] = BitConverter.ToInt32(rxbuf, dataFieldOffset);
                                            dataFieldOffset += 4;
                                        }
                                        if (containsC)
                                        {
                                            AllRadioDiodeVoltage[LaserChannels.ChannelC] = BitConverter.ToInt32(rxbuf, dataFieldOffset);
                                            dataFieldOffset += 4;
                                        }
                                        break;
                                    case Properties.RadioDiodeCalibrationSlope:
                                        if (containsA)
                                        {

                                            AllGetRadioDiodeSlope[LaserChannels.ChannelA] = BitConverter.ToInt32(rxbuf, dataFieldOffset);
                                            dataFieldOffset += 4;
                                        }
                                        if (containsB)
                                        {

                                            AllGetRadioDiodeSlope[LaserChannels.ChannelB] = BitConverter.ToInt32(rxbuf, dataFieldOffset);
                                            dataFieldOffset += 4;
                                        }
                                        if (containsC)
                                        {
                                            AllGetRadioDiodeSlope[LaserChannels.ChannelC] = BitConverter.ToInt32(rxbuf, dataFieldOffset);
                                            dataFieldOffset += 4;
                                        }
                                        break;
                                    case Properties.RadioAndTelevisionDiodeCalibrationConstant:
                                        if (containsA)
                                        {

                                            AllGetRadioAndTelevisionDiodeCalibrationConstant[LaserChannels.ChannelA] = BitConverter.ToInt32(rxbuf, dataFieldOffset);
                                            dataFieldOffset += 4;
                                        }
                                        if (containsB)
                                        {

                                            AllGetRadioAndTelevisionDiodeCalibrationConstant[LaserChannels.ChannelB] = BitConverter.ToInt32(rxbuf, dataFieldOffset);
                                            dataFieldOffset += 4;
                                        }
                                        if (containsC)
                                        {
                                            AllGetRadioAndTelevisionDiodeCalibrationConstant[LaserChannels.ChannelC] = BitConverter.ToInt32(rxbuf, dataFieldOffset);
                                            dataFieldOffset += 4;
                                        }
                                        break;
                                    case Properties.OpticalPowerControlvoltage:
                                        if (containsA)
                                        {

                                            AllOpticalPowerControlvoltage[LaserChannels.ChannelA] = BitConverter.ToInt16(rxbuf, dataFieldOffset + 2) * 0.0001;
                                            dataFieldOffset += 4;
                                        }
                                        if (containsB)
                                        {

                                            AllOpticalPowerControlvoltage[LaserChannels.ChannelB] = BitConverter.ToInt16(rxbuf, dataFieldOffset + 2) * 0.0001;
                                            dataFieldOffset += 4;
                                        }
                                        if (containsC)
                                        {
                                            AllOpticalPowerControlvoltage[LaserChannels.ChannelC] = BitConverter.ToInt16(rxbuf, dataFieldOffset + 2) * 0.0001;
                                            dataFieldOffset += 4;
                                        }
                                        break;
                                    case Properties.LaserCurrent:
                                        if (containsA)
                                        {

                                            AllCurrentValuelaser[LaserChannels.ChannelA] = BitConverter.ToInt32(rxbuf, dataFieldOffset);
                                            dataFieldOffset += 4;
                                        }
                                        if (containsB)
                                        {

                                            AllCurrentValuelaser[LaserChannels.ChannelB] = BitConverter.ToInt32(rxbuf, dataFieldOffset);
                                            dataFieldOffset += 4;
                                        }
                                        if (containsC)
                                        {
                                            AllCurrentValuelaser[LaserChannels.ChannelC] = BitConverter.ToInt32(rxbuf, dataFieldOffset);
                                            dataFieldOffset += 4;
                                        }
                                        break;
                                    case Properties.OpticalPowerControlKpUpperLimitLessThanOrEqual15:
                                        if (containsA)
                                        {

                                            AllGetOpticalPowerControlKpUpperLimitLessThanOrEqual15[LaserChannels.ChannelA] = BitConverter.ToInt32(rxbuf, dataFieldOffset);
                                            dataFieldOffset += 4;
                                        }
                                        if (containsB)
                                        {

                                            AllGetOpticalPowerControlKpUpperLimitLessThanOrEqual15[LaserChannels.ChannelB] = BitConverter.ToInt32(rxbuf, dataFieldOffset);
                                            dataFieldOffset += 4;
                                        }
                                        if (containsC)
                                        {
                                            AllGetOpticalPowerControlKpUpperLimitLessThanOrEqual15[LaserChannels.ChannelC] = BitConverter.ToInt32(rxbuf, dataFieldOffset);
                                            dataFieldOffset += 4;
                                        }
                                        break;
                                    case Properties.OpticalPowerControlKpDownLimitLessThanOrEqual15:
                                        if (containsA)
                                        {

                                            AllGetOpticalPowerControlKpDownLimitLessThanOrEqual15[LaserChannels.ChannelA] = BitConverter.ToInt32(rxbuf, dataFieldOffset);
                                            dataFieldOffset += 4;
                                        }
                                        if (containsB)
                                        {

                                            AllGetOpticalPowerControlKpDownLimitLessThanOrEqual15[LaserChannels.ChannelB] = BitConverter.ToInt32(rxbuf, dataFieldOffset);
                                            dataFieldOffset += 4;
                                        }
                                        if (containsC)
                                        {
                                            AllGetOpticalPowerControlKpDownLimitLessThanOrEqual15[LaserChannels.ChannelC] = BitConverter.ToInt32(rxbuf, dataFieldOffset);
                                            dataFieldOffset += 4;
                                        }
                                        break;
                                    case Properties.OpticalPowerControlKiUpperLimitLessThanOrEqual15:
                                        if (containsA)
                                        {

                                            AllGetOpticalPowerControlKiUpperLimitLessThanOrEqual15[LaserChannels.ChannelA] = BitConverter.ToInt32(rxbuf, dataFieldOffset);
                                            dataFieldOffset += 4;
                                        }
                                        if (containsB)
                                        {

                                            AllGetOpticalPowerControlKiUpperLimitLessThanOrEqual15[LaserChannels.ChannelB] = BitConverter.ToInt32(rxbuf, dataFieldOffset);
                                            dataFieldOffset += 4;
                                        }
                                        if (containsC)
                                        {
                                            AllGetOpticalPowerControlKiUpperLimitLessThanOrEqual15[LaserChannels.ChannelC] = BitConverter.ToInt32(rxbuf, dataFieldOffset);
                                            dataFieldOffset += 4;
                                        }
                                        break;
                                    case Properties.OpticalPowerControlKiDownLimitLessThanOrEqual15:
                                        if (containsA)
                                        {

                                            AllGetOpticalPowerControlKiDownLimitLessThanOrEqual15[LaserChannels.ChannelA] = BitConverter.ToInt32(rxbuf, dataFieldOffset);
                                            dataFieldOffset += 4;
                                        }
                                        if (containsB)
                                        {

                                            AllGetOpticalPowerControlKiDownLimitLessThanOrEqual15[LaserChannels.ChannelB] = BitConverter.ToInt32(rxbuf, dataFieldOffset);
                                            dataFieldOffset += 4;
                                        }
                                        if (containsC)
                                        {
                                            AllGetOpticalPowerControlKiDownLimitLessThanOrEqual15[LaserChannels.ChannelC] = BitConverter.ToInt32(rxbuf, dataFieldOffset);
                                            dataFieldOffset += 4;
                                        }
                                        break;
                                    case Properties.OpticalPowerControlKpUpperLimitLessThan15:
                                        if (containsA)
                                        {

                                            AllGetOpticalPowerControlKpUpperLimitLessThan15[LaserChannels.ChannelA] = BitConverter.ToInt32(rxbuf, dataFieldOffset);
                                            dataFieldOffset += 4;
                                        }
                                        if (containsB)
                                        {

                                            AllGetOpticalPowerControlKpUpperLimitLessThan15[LaserChannels.ChannelB] = BitConverter.ToInt32(rxbuf, dataFieldOffset);
                                            dataFieldOffset += 4;
                                        }
                                        if (containsC)
                                        {
                                            AllGetOpticalPowerControlKpUpperLimitLessThan15[LaserChannels.ChannelC] = BitConverter.ToInt32(rxbuf, dataFieldOffset);
                                            dataFieldOffset += 4;
                                        }
                                        break;
                                    case Properties.OpticalPowerControlKpDownLimitLessThan15:
                                        if (containsA)
                                        {

                                            AllGetOpticalPowerControlKpDownLimitLessThan15[LaserChannels.ChannelA] = BitConverter.ToInt32(rxbuf, dataFieldOffset);
                                            dataFieldOffset += 4;
                                        }
                                        if (containsB)
                                        {

                                            AllGetOpticalPowerControlKpDownLimitLessThan15[LaserChannels.ChannelB] = BitConverter.ToInt32(rxbuf, dataFieldOffset);
                                            dataFieldOffset += 4;
                                        }
                                        if (containsC)
                                        {
                                            AllGetOpticalPowerControlKpDownLimitLessThan15[LaserChannels.ChannelC] = BitConverter.ToInt32(rxbuf, dataFieldOffset);
                                            dataFieldOffset += 4;
                                        }
                                        break;
                                    case Properties.OpticalPowerControlKiUpperLimitLessThan15:
                                        if (containsA)
                                        {

                                            AllGetOpticalPowerControlKiUpperLimitLessThan15[LaserChannels.ChannelA] = BitConverter.ToInt32(rxbuf, dataFieldOffset);
                                            dataFieldOffset += 4;
                                        }
                                        if (containsB)
                                        {

                                            AllGetOpticalPowerControlKiUpperLimitLessThan15[LaserChannels.ChannelB] = BitConverter.ToInt32(rxbuf, dataFieldOffset);
                                            dataFieldOffset += 4;
                                        }
                                        if (containsC)
                                        {
                                            AllGetOpticalPowerControlKiUpperLimitLessThan15[LaserChannels.ChannelC] = BitConverter.ToInt32(rxbuf, dataFieldOffset);
                                            dataFieldOffset += 4;
                                        }
                                        break;
                                    case Properties.OpticalPowerControlKiDownLimitLessThan15:
                                        if (containsA)
                                        {

                                            AllGetOpticalPowerControlKiDownLimitLessThan15[LaserChannels.ChannelA] = BitConverter.ToInt32(rxbuf, dataFieldOffset);
                                            dataFieldOffset += 4;
                                        }
                                        if (containsB)
                                        {

                                            AllGetOpticalPowerControlKiDownLimitLessThan15[LaserChannels.ChannelB] = BitConverter.ToInt32(rxbuf, dataFieldOffset);
                                            dataFieldOffset += 4;
                                        }
                                        if (containsC)
                                        {
                                            AllGetOpticalPowerControlKiDownLimitLessThan15[LaserChannels.ChannelC] = BitConverter.ToInt32(rxbuf, dataFieldOffset);
                                            dataFieldOffset += 4;
                                        }
                                        break;
                                    case Properties.LaserMaximumCurrent:
                                        if (containsA)
                                        {

                                            AllGetLaserMaximumCurrent[LaserChannels.ChannelA] = BitConverter.ToInt32(rxbuf, dataFieldOffset) * 0.1;
                                            dataFieldOffset += 4;
                                        }
                                        if (containsB)
                                        {

                                            AllGetLaserMaximumCurrent[LaserChannels.ChannelB] = BitConverter.ToInt32(rxbuf, dataFieldOffset) * 0.1;
                                            dataFieldOffset += 4;
                                        }
                                        if (containsC)
                                        {
                                            AllGetLaserMaximumCurrent[LaserChannels.ChannelC] = BitConverter.ToInt32(rxbuf, dataFieldOffset) * 0.1;
                                            dataFieldOffset += 4;
                                        }
                                        break;
                                    case Properties.LaserMinimumCurrent:
                                        if (containsA)
                                        {
                                            AllGetLaserMinimumCurrent[LaserChannels.ChannelA] = BitConverter.ToInt32(rxbuf, dataFieldOffset) * 0.1;
                                            dataFieldOffset += 4;
                                        }
                                        if (containsB)
                                        {

                                            AllGetLaserMinimumCurrent[LaserChannels.ChannelB] = BitConverter.ToInt32(rxbuf, dataFieldOffset) * 0.1;
                                            dataFieldOffset += 4;
                                        }
                                        if (containsC)
                                        {
                                            AllGetLaserMinimumCurrent[LaserChannels.ChannelC] = BitConverter.ToInt32(rxbuf, dataFieldOffset) * 0.1;
                                            dataFieldOffset += 4;
                                        }
                                        break;
                                }
                            }
                        }
                        #endregion Laser Module
                        #region LED
                        else if (((byte)subSys & 0x0f) == 0x07)
                        {
                            for (int i = 0; i < propertyNums; i++)
                            {
                                switch (property + i)
                                {
                                    case Properties.LEDBoardFirmwareVersionNumber:
                                        LEDVersion = string.Format("{0}.{1}.{2}.{3}", rxbuf[dataFieldOffset], rxbuf[dataFieldOffset + 1], rxbuf[dataFieldOffset + 2], rxbuf[dataFieldOffset + 3]);
                                        dataFieldOffset += 4;
                                        break;
                                }
                            }
                        }
                        #endregion
                        #region LightGain
                        else if (((byte)subSys & 0x0f) == 0x08)
                        {
                            for (int i = 0; i < propertyNums; i++)
                            {
                                switch (property + i)
                                {
                                    case Properties.LightDataComplete:
                                        LightGainDataState = BitConverter.ToUInt16(rxbuf, dataFieldOffset);
                                        dataFieldOffset += 2;
                                        break;
                                    case Properties.LightSampleData:
                                        LightGainData = rxbuf.Skip(0).Take(1288).ToArray();
                                        //Test = "";
                                        //Test += "\n";
                                        //for (int j = 0; j < LightGainData.Length; j++)
                                        //{
                                        //    Test += LightGainData[j].ToString("X2") + " ";
                                        //}
                                        dataFieldOffset += 1280;
                                        break;
                                }
                            }
                        }
                        #endregion
                        // unknown system
                        else
                        {
                            return false;
                        }
                    }
                    #endregion Read response
                    #region Write response
                    else if (commandType == CommandTypes.Write)
                    {
                        offset += 10;
                        #region Mainboard
                        if (subSys == SubSys.Mainboard)
                        {
                            for (int i = 0; i < propertyNums; i++)
                            {
                                switch (property + i)
                                {
                                    //State when pressing the front panel button while scanning images  （FW Version 1.1.0.0）  //扫描图像时按下前面板按钮时的状态
                                    case Properties.PromptForPressingShutdown:
                                        ShutdownDuringScanStatus = true;
                                        break;
                                }

                            }
                        }
                        #endregion Mainboard
                        continue;
                        //return true;
                    }
                    #endregion Write response
                }
            }
            while (offset < index);

            return true;
        }

        #endregion Public Functions
    }
}
