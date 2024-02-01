using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.IO.Ports;
using System.Windows;
using System.Threading;

namespace Azure.APDCalibrationBench
{
    public class LightModeSettingsPort : IDisposable
    {
        public delegate void CalibrationCommUpdateHandler();
        public event CalibrationCommUpdateHandler UpdateCommOutput;
        private bool _IsBusy = false;
        private bool _IsPrint = true;
        //byte Position1 = 0x11;  //激光版
        //byte Position2 = 0x12;  //IV板子

        byte Position1 = 0x21;  //激光版   L单仓
        byte Position2 = 0x22;  //IV板子
        public enum ModuleType
        {
            IV = 0x22,//
            Laser = 0x21//

            //IV = 0x12,//
            //Laser = 0x11//
        }
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
        public enum LaserModeRegsAddressType
        {
            LaserSoftwareVersionNumber = 0x00, //软件版本号 1
            LaserNumberNo = 0x01,//激光器编号 1
            LaserWaveLength = 0x02,//激光器波长 1
            LaserPowerValue = 0x03,//激光器电流值 1
            //0x04  保留
            Not532LaserPowerTo5mvElectric = 0x05,//5mW激光功率对应的电流值（单位0.01mA） 1
            Not532LaserPowerTo10mvElectric = 0x06,//10mW激光功率对应的电流值（单位0.01mA） 1
            Not532LaserPowerTo15mvElectric = 0x07,//15mW激光功率对应的电流值（单位0.01mA） 1
            Not532LaserPowerTo20mvElectric = 0x08,//20mW激光功率对应的电流值（单位0.01mA） 1
            Not532LaserPowerTo25mvElectric = 0x09,//25mW激光功率对应的电流值（单位0.01mA） 1
            Not532LaserPowerTo30mvElectric = 0x0A,//30mW激光功率对应的电流值（单位0.01mA） 1
            TECActualTemperature =0x0B,//TEC实际温度 1
            TEControlTemperature=0x0C,//TEC控制温度 1
            TECMaximumCoolingCurrent=0x0D,//TEC最大制冷电流 1
            TECRefrigerationControlParameterKp=0x0E,//TEC制冷控制参数Kp 1
            TECRefrigerationControlParameterKi = 0x0F,//TEC制冷控制参数Ki 1
            TECRefrigerationControlParameterKd=0x10,//TEC制冷控制参数Kd 1
            LaserLightPowerValueValue=0x11,//激光光功率值 1
            TECWorkingStatus=0x12,//TEC工作状态 1
            TECCurrentDirection=0x13,//TEC电流方向 1
            RadiatorTemperature=0x14,//散热器温度 1
            TECCurrentCompensationCoefficient=0x15,//TEC电流补偿系数 1
            LaserLightPowerValue=0x16,//当前激光光功率值 1
            PowerClosedloopControlParameterKpGreaterThan15 = 0x17,//功率闭环控制参数Kp >15 1
            PowerClosedloopControlParameterKiGreaterThan15 = 0x18,//功率闭环控制参数Ki 1
            PowerClosedloopControlParameterKdGreaterThan15 = 0x19,//功率闭环控制参数Kd 1
            True532LaserPowerTo5mvElectric = 0x1A,//5mW激光功率对应的光电二极管电压（单位0.01mA）1
            True532LaserPowerTo10mvElectric = 0x1B,//10mW激光功率对应的光电二极管电压（单位0.01mA）1
            True532LaserPowerTo15mvElectric = 0x1C,//15mW激光功率对应的光电二极管电压（单位0.01mA）1
            True532LaserPowerTo20mvElectric = 0x1D,//20mW激光功率对应的光电二极管电压（单位0.01mA）1
            True532LaserPowerTo25mvElectric = 0x1E,//25mW激光功率对应的光电二极管电压（单位0.01mA）1
            True532LaserPowerTo30mvElectric = 0x1F,//30mW激光功率对应的光电二极管电压（单位0.01mA）1
            LightElectricTowVole = 0x20,            //光电二极管电压 1
            KValueofPhotodiodeTemperatureCurveValue = 0x21,            //光电二极管温度曲线K值 1
            BValueofPhotodiodeTemperatureCurveValue = 0x22,            //光电二极管温度曲线B值 1
            ErrorCode = 0x23, //错误码 1
            Not532LaserPowerTo35mvElectric = 0x24,//35mW激光功率对应的电流值（单位0.01mA）1
            Not532LaserPowerTo40mvElectric = 0x25,//40mW激光功率对应的电流值（单位0.01mA）1
            Not532LaserPowerTo45mvElectric = 0x26,//45mW激光功率对应的电流值（单位0.01mA）1
            Not532LaserPowerTo50mvElectric = 0x27,//50mW激光功率对应的电流值（单位0.01mA）1
            OpticalModuleSerialNumber = 0x28,//光学模块序列号 1
            True532LaserPowerTo35mvElectric = 0x29,//35mW激光功率对应的光电二极管电压（单位0.01mA）1
            PowerClosedloopControlParameterKpLessThanOrEqual15 = 0x2A,//功率闭环控制参数kp <=15
            PowerClosedloopControlParameterKiLessThanOrEqual15 = 0x2B,//功率闭环控制参数ki <=15
            PowerClosedloopControlParameterKdLessThanOrEqual15 = 0x2C,//功率闭环控制参数kd <=15
            UpperLimitKpLessThan15  = 0x2D,//15MW及以下的KP的最大设置值 
            LowerLimitKpLessThan15 = 0x2E,//15MW及以下的Kp的最小设置值
            UpperLimitKiLessThan15 = 0x2F,//15MW及以下的Ki的最大设置值
            LowerLimitKiLessThan15 = 0x30,//15MW及以下的Ki的最小设置值
            UpperLimitKpGreaterThan15 = 0x31,//15MW以上的Kp的最大设置值
            LowerLimitKpGreaterThan15 = 0x32,//15MW以上的Kp的最小设置值
            UpperLimitKiGreaterThan15 = 0x33,//15MW以上的Ki的最大设置值
            LowerLimitKiGreaterThan15 = 0x34,//15MW以上的Ki的最小设置值
            MaximumOperatingCurrentLaser = 0x35,//激光器的最大工作电流
            MinimumOperatingCurrentLaser = 0x36,//激光器的最小工作电流



        }
        public enum IVModeRegsAddressType
        {
            IVSoftwareVersionNumber = 0x00, //软件版本号 1
            IVSensorType = 0x01,//传感器类型
            IVSensorNo= 0x02,//传感器编号 1
            PGAMultiple=0x03,//PGA倍数 1
            APDGain = 0x04,//APD增益 1
            APDTemp=0x05,//APD温度 1
            APDHighVoltage=0x06,//APD高压值 1
            LightIntensityCalibrationTemperature = 0x07,//光强校准时温度  1
            Gain50CalVol = 0x08,//APD增益50时校准电压值  1
            Gain100CalVol = 0x09,//APD增益100时校准电压值 1
            Gain150CalVol = 0x0A,//APD增益150时校准电压值 1
            Gain200CalVol = 0x0B,//APD增益200时校准电压值 1
            Gain250CalVol = 0x0C,//APD增益250时校准电压值 1
            Gain300CalVol = 0x0D,//APD增益300时校准电压值 1
            Gain400CalVol = 0x0E,//APD增益400时校准电压值 1
            Gain500CalVol = 0x0F,//APD增益500时校准电压值 1
            PMTCtrlVol = 0x10,//PMT的控制电压 1
            PMTCompensationCoefficient = 0x11,//PMT补偿系数 1
            TempSenser1282AD = 0x12,//温度传感器12.82℃（1.05k)时的AD值 1
            TempSenser6459AD = 0x13,//温度传感器64.59℃（1.25k)时的AD值 1
            TempSenserAD = 0x14,//温度传感器的AD值 1
            IVBoardRunningState = 0x15,//读取IV板运行状态 1
            TemperatureSensorSamplingVoltage = 0x16,//温度传感器采样电压 1
            ADPTemperatureCalibrationFactor = 0x17,//ADP温度校准系数 1
            ErrorCode = 0x18, //错误码 1
            OpticalModuleSerialNumber = 0x19,//光学模块序列号 1
        }

        private byte _FrameHeader = 0x3a;
        private byte _ByteReserved = 0x00;
        private int _DataField = 0;
        private byte _FrameEnd = 0x3b;
        private SerialPort _Port = null;
        private string[] _AvailablePorts;
        //private byte[] _PortReceiveBuf;
        private bool _IsConnected = false;
        List<byte> PortListData = new List<byte>();
        private Thread DataProcessThread;
        private DeviceAddressType _DeviceAddress;
        private FunctionCodeType _FunctionCode;
        public LightModeSettingsPort()
        {
            //_PortReceiveBuf = new byte[1000];
            _DeviceAddress = new DeviceAddressType();
            _FunctionCode = FunctionCodeType.ReadReg;
            DataProcessThread = new Thread(ProcessData);
            DataProcessThread.IsBackground = true;
            DataProcessThread.Start();
            _IsConnected = false;
            _IsBusy = false;
        }

        public void SearchPort()
        {
            if (!_IsConnected)
            {
                _AvailablePorts = SerialPort.GetPortNames();
                for (int i = 0; i < _AvailablePorts.Length; i++)
                {
                    _Port = new SerialPort(_AvailablePorts[i], 9600, Parity.None, 8, StopBits.One);
                    try
                    {

                        _Port.Open();
                        _Port.DataReceived += _Port_DataReceived;
                        _IsConnected = true;
                        _Port.ReceivedBytesThreshold = 10;
                    }
                    catch (Exception exception)
                    {
                        _IsConnected = false;
                        _IsBusy = false;
                    }
                }
            }
        }
        private void _Port_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            try
            {
                int _receiveBufLth = _Port.BytesToRead;
                byte[] _PortReceiveBuf = new byte[_receiveBufLth];
                _Port.Read(_PortReceiveBuf, 0, _receiveBufLth);
                if (_receiveBufLth == 10)
                {
                    PortListData.AddRange(_PortReceiveBuf);
                }
                else
                {
                    Thread.Sleep(20);
                    _receiveBufLth = _Port.BytesToRead;
                    byte[] __PortReceiveBuf = new byte[_receiveBufLth];
                    _Port.Read(__PortReceiveBuf, 0, _receiveBufLth);
                    byte[] newPortReceiveBuf = new byte[10];

                    Array.Copy(_PortReceiveBuf, 0, newPortReceiveBuf, 0, _PortReceiveBuf.Length);

                    Array.Copy(__PortReceiveBuf, 0, newPortReceiveBuf, _PortReceiveBuf.Length, __PortReceiveBuf.Length);
                    //newPortReceiveBuf[0] = _PortReceiveBuf[0];
                    //newPortReceiveBuf[1] = _PortReceiveBuf[1];
                    //newPortReceiveBuf[2] = _PortReceiveBuf[2];
                    //newPortReceiveBuf[3] = _PortReceiveBuf[3];
                    //newPortReceiveBuf[4] = __PortReceiveBuf[0];
                    //newPortReceiveBuf[5] = __PortReceiveBuf[1];
                    //newPortReceiveBuf[6] = __PortReceiveBuf[2];
                    //newPortReceiveBuf[7] = __PortReceiveBuf[3];
                    //newPortReceiveBuf[8] = __PortReceiveBuf[4];
                    //newPortReceiveBuf[9] = __PortReceiveBuf[5];
                    PortListData.AddRange(newPortReceiveBuf);
                }

            }
            catch (Exception exception)
            {
                
            }

        }

        private void RunReceiveDataCallback(byte[] ReceiveBytes)
        {
            //Console.WriteLine(BitConverter.ToString(ReceiveBytes));
            //return;
            byte[] _tempArray = new byte[4];
            ModuleType type = (ModuleType)ReceiveBytes[1];

            _tempArray[0] = ReceiveBytes[8];
            _tempArray[1] = ReceiveBytes[7];
            _tempArray[2] = ReceiveBytes[6];
            _tempArray[3] = ReceiveBytes[5];

            _DataField = BitConverter.ToInt32(_tempArray, 0);
            if (_FunctionCode == FunctionCodeType.ReadReg)
            {
                if (type == ModuleType.Laser)   //激光版
                {
                    LaserModeRegsAddressType Address = (LaserModeRegsAddressType)ReceiveBytes[3];
                    switch (Address)
                    {
                        case LaserModeRegsAddressType.OpticalModuleSerialNumber:
                            byte[] _temp = { _tempArray[3], _tempArray[2] };
                            string strtemp = _tempArray[0].ToString();
                            if (strtemp.Length == 1)
                            {
                                strtemp = "0" + strtemp;
                            }
                            LaserOpticalModuleNumberValue = BitConverter.ToString(_temp).Replace("-", "") + _tempArray[1].ToString() + strtemp;
                            break;
                        case LaserModeRegsAddressType.ErrorCode:
                            LaserErrorCode = ToHexString(_tempArray);
                            break;
                        case LaserModeRegsAddressType.LaserSoftwareVersionNumber:
                            LaserSoftwareVersionNumber = ToHexString(_tempArray);
                            break;
                        case LaserModeRegsAddressType.LaserNumberNo:
                            LaserNumberNo = ToHexString(_tempArray);
                            CurrentLaserNoValue = LaserNumberNo;
                            break;
                        case LaserModeRegsAddressType.LaserWaveLength:
                            LaserWaveLength = _DataField.ToString();
                            CurrentLaserWavaLengthValue = _DataField;
                            break;
                        case LaserModeRegsAddressType.TECActualTemperature:
                            _DataField = BitConverter.ToInt16(_tempArray, 0);
                            CurrentTECActualTemperatureValue = _DataField * 0.1;
                            break;
                        case LaserModeRegsAddressType.TEControlTemperature:
                            TECCtrlTemp = _DataField * 0.1;
                            CurrentTEControlTemperatureValue = TECCtrlTemp;
                            break;
                        case LaserModeRegsAddressType.LaserPowerValue:
                            LaserPowerValue = _DataField * 0.01;
                            CurrentLaserCurrentValueValue = LaserPowerValue;
                            IsPrint = true;
                            break;
                        case LaserModeRegsAddressType.LightElectricTowVole:
                            LightElectricTowVole = _DataField * 0.0001;
                            CurrentPhotodiodeVoltageValue = LightElectricTowVole;
                            break;
                        case LaserModeRegsAddressType.Not532LaserPowerTo5mvElectric:
                            Not532LaserPower = _DataField * 0.01;
                            CurrentLaserCorrespondingCurrentValueValue = Not532LaserPower;
                            CurrentLaserCorrespondingCurrent5ValueValue = _DataField;
                            break;
                        case LaserModeRegsAddressType.Not532LaserPowerTo10mvElectric:
                            Not532LaserPower = _DataField * 0.01;
                            CurrentLaserCorrespondingCurrentValueValue = Not532LaserPower;
                            CurrentLaserCorrespondingCurrent10ValueValue = _DataField;
                            break;
                        case LaserModeRegsAddressType.Not532LaserPowerTo15mvElectric:
                            Not532LaserPower = _DataField * 0.01;
                            CurrentLaserCorrespondingCurrentValueValue = Not532LaserPower;
                            CurrentLaserCorrespondingCurrent15ValueValue = _DataField;
                            break;
                        case LaserModeRegsAddressType.Not532LaserPowerTo20mvElectric:
                            Not532LaserPower = _DataField * 0.01;
                            CurrentLaserCorrespondingCurrentValueValue = Not532LaserPower;
                            CurrentLaserCorrespondingCurrent20ValueValue = _DataField;
                            break;
                        case LaserModeRegsAddressType.Not532LaserPowerTo25mvElectric:
                            Not532LaserPower = _DataField * 0.01;
                            CurrentLaserCorrespondingCurrentValueValue = Not532LaserPower;
                            CurrentLaserCorrespondingCurrent25ValueValue = _DataField;
                            break;
                        case LaserModeRegsAddressType.Not532LaserPowerTo30mvElectric:
                            Not532LaserPower = _DataField * 0.01;
                            CurrentLaserCorrespondingCurrentValueValue = Not532LaserPower;
                            CurrentLaserCorrespondingCurrent30ValueValue = _DataField;
                            break;
                        case LaserModeRegsAddressType.Not532LaserPowerTo35mvElectric:
                            Not532LaserPower = _DataField * 0.01;
                            CurrentLaserCorrespondingCurrentValueValue = Not532LaserPower;
                            CurrentLaserCorrespondingCurrent35ValueValue = _DataField;
                            break;
                        case LaserModeRegsAddressType.Not532LaserPowerTo40mvElectric:
                            Not532LaserPower = _DataField * 0.01;
                            CurrentLaserCorrespondingCurrentValueValue = Not532LaserPower;
                            CurrentLaserCorrespondingCurrent40ValueValue = _DataField;
                            break;
                        case LaserModeRegsAddressType.Not532LaserPowerTo45mvElectric:
                            Not532LaserPower = _DataField * 0.01;
                            CurrentLaserCorrespondingCurrentValueValue = Not532LaserPower;
                            CurrentLaserCorrespondingCurrent45ValueValue = _DataField;
                            break;
                        case LaserModeRegsAddressType.Not532LaserPowerTo50mvElectric:
                            Not532LaserPower = _DataField * 0.01;
                            CurrentLaserCorrespondingCurrentValueValue = Not532LaserPower;
                            CurrentLaserCorrespondingCurrent50ValueValue = _DataField;
                            break;
                        case LaserModeRegsAddressType.True532LaserPowerTo5mvElectric:
                            True532LaserPower = _DataField * 0.01;
                            CurrentPhotodiodeVoltageCorrespondingToLaserPowerValue = _DataField * 0.0001;
                            CurrentPhotodiodeVoltageCorrespondingTo5LaserPowerValue = _DataField;
                            break;
                        case LaserModeRegsAddressType.True532LaserPowerTo10mvElectric:
                            True532LaserPower = _DataField * 0.01;
                            CurrentPhotodiodeVoltageCorrespondingToLaserPowerValue = _DataField * 0.0001;
                            CurrentPhotodiodeVoltageCorrespondingTo10LaserPowerValue = _DataField;
                            break;
                        case LaserModeRegsAddressType.True532LaserPowerTo15mvElectric:
                            True532LaserPower = _DataField * 0.01;
                            CurrentPhotodiodeVoltageCorrespondingToLaserPowerValue = _DataField * 0.0001;
                            CurrentPhotodiodeVoltageCorrespondingTo15LaserPowerValue = _DataField;
                            break;
                        case LaserModeRegsAddressType.True532LaserPowerTo20mvElectric:
                            True532LaserPower = _DataField * 0.01;
                            CurrentPhotodiodeVoltageCorrespondingToLaserPowerValue = _DataField * 0.0001;
                            CurrentPhotodiodeVoltageCorrespondingTo20LaserPowerValue = _DataField;
                            break;
                        case LaserModeRegsAddressType.True532LaserPowerTo25mvElectric:
                            True532LaserPower = _DataField * 0.01;
                            CurrentPhotodiodeVoltageCorrespondingToLaserPowerValue = _DataField * 0.0001;
                            CurrentPhotodiodeVoltageCorrespondingTo25LaserPowerValue = _DataField;
                            break;
                        case LaserModeRegsAddressType.True532LaserPowerTo30mvElectric:
                            True532LaserPower = _DataField * 0.01;
                            CurrentPhotodiodeVoltageCorrespondingToLaserPowerValue = _DataField * 0.0001;
                            CurrentPhotodiodeVoltageCorrespondingTo30LaserPowerValue = _DataField;
                            break;
                        case LaserModeRegsAddressType.True532LaserPowerTo35mvElectric:
                            True532LaserPower = _DataField * 0.01;
                            CurrentPhotodiodeVoltageCorrespondingToLaserPowerValue = _DataField * 0.0001;
                            CurrentPhotodiodeVoltageCorrespondingTo35LaserPowerValue = _DataField;
                            break;
                        case LaserModeRegsAddressType.TECMaximumCoolingCurrent:
                           CurrentTECMaximumCoolingCurrentValue = _DataField;
                            break;
                        case LaserModeRegsAddressType.TECRefrigerationControlParameterKp:
                            CurrentTECRefrigerationControlParameterKpValue = _DataField;
                            break;
                        case LaserModeRegsAddressType.TECRefrigerationControlParameterKi:
                            CurrentTECRefrigerationControlParameterKiValue = _DataField;
                            break;
                        case LaserModeRegsAddressType.TECRefrigerationControlParameterKd:
                            CurrentTECRefrigerationControlParameterKdValue = _DataField;
                            break;
                        case LaserModeRegsAddressType.LaserLightPowerValueValue:
                            CurrentLaserLightPowerValueValue = _DataField * 0.1;
                            break;
                        case LaserModeRegsAddressType.TECWorkingStatus:
                            CurrentTECWorkingStatusValue = _DataField;
                            break;
                        case LaserModeRegsAddressType.TECCurrentDirection:
                            CurrentTECCurrentDirectionValue = _DataField;
                            break;
                        case LaserModeRegsAddressType.RadiatorTemperature:
                            CurrenRadiatorTemperatureValue = _DataField * 0.1;
                            break;
                        case LaserModeRegsAddressType.TECCurrentCompensationCoefficient:
                            CurrenTECCurrentCompensationCoefficientValue = _DataField;
                            break;
                        case LaserModeRegsAddressType.LaserLightPowerValue:
                            GetGet_CurrentLaserLightPowerValueValue = _DataField * 0.1;
                            break;
                        case LaserModeRegsAddressType.PowerClosedloopControlParameterKpGreaterThan15:
                            CurrentPowerClosedloopControlParameterKpValue = _DataField;
                            break;
                        case LaserModeRegsAddressType.PowerClosedloopControlParameterKiGreaterThan15:
                            CurrentPowerClosedloopControlParameterKiValue = _DataField;
                            break;
                        case LaserModeRegsAddressType.PowerClosedloopControlParameterKdGreaterThan15:
                            CurrentPowerClosedloopControlParameterKdValue = _DataField;
                            break;
                        case LaserModeRegsAddressType.KValueofPhotodiodeTemperatureCurveValue:
                            CurrentKValueofPhotodiodeTemperatureCurveValue = _DataField * 1000;
                            break;
                        case LaserModeRegsAddressType.BValueofPhotodiodeTemperatureCurveValue:
                            CurrentBValueofPhotodiodeTemperatureCurveValue = _DataField * 1000;
                            break;
                        case LaserModeRegsAddressType.PowerClosedloopControlParameterKpLessThanOrEqual15:
                            PowerClosedloopControlParameterKpLessThanOrEqual15 = _DataField;
                            break;
                        case LaserModeRegsAddressType.PowerClosedloopControlParameterKiLessThanOrEqual15:
                            PowerClosedloopControlParameterKiLessThanOrEqual15 = _DataField;
                            break;
                        case LaserModeRegsAddressType.PowerClosedloopControlParameterKdLessThanOrEqual15:
                            PowerClosedloopControlParameterKdLessThanOrEqual15 = _DataField;
                            break;
                        case LaserModeRegsAddressType.UpperLimitKpLessThan15:
                            UpperLimitKpLessThan15 = _DataField;
                            break;
                        case LaserModeRegsAddressType.LowerLimitKpLessThan15:
                            LowerLimitKpLessThan15 = _DataField;
                            break;
                        case LaserModeRegsAddressType.UpperLimitKiLessThan15:
                            UpperLimitKiLessThan15 = _DataField;
                            break;
                        case LaserModeRegsAddressType.LowerLimitKiLessThan15:
                            LowerLimitKiLessThan15 = _DataField;
                            break;
                        case LaserModeRegsAddressType.UpperLimitKpGreaterThan15:
                            UpperLimitKpGreaterThan15 = _DataField;
                            break;
                        case LaserModeRegsAddressType.LowerLimitKpGreaterThan15:
                            LowerLimitKpGreaterThan15 = _DataField;
                            break;
                        case LaserModeRegsAddressType.UpperLimitKiGreaterThan15:
                            UpperLimitKiGreaterThan15 = _DataField;
                            break;
                        case LaserModeRegsAddressType.LowerLimitKiGreaterThan15:
                            LowerLimitKiGreaterThan15 = _DataField;
                            break;
                        case LaserModeRegsAddressType.MaximumOperatingCurrentLaser:
                            MaximumOperatingCurrentLaser = _DataField;
                            break;
                        case LaserModeRegsAddressType.MinimumOperatingCurrentLaser:
                            MinimumOperatingCurrentLaser = _DataField;
                            break;

                    }
                }
                else    //IV板子
                {
                    IVModeRegsAddressType Address = (IVModeRegsAddressType)ReceiveBytes[3];
                    switch (Address)
                    {
                        case IVModeRegsAddressType.OpticalModuleSerialNumber:
                            byte[] _temp = { _tempArray[3], _tempArray[2] };
                            string strtemp = _tempArray[0].ToString();
                            if (strtemp.Length == 1)
                            {
                                strtemp = "0" + strtemp;
                            }
                            IVOpticalModuleNumberValue = BitConverter.ToString(_temp).Replace("-", "") + _tempArray[1].ToString() + strtemp;
                            break;
                        case IVModeRegsAddressType.IVSensorType:
                            IVSensorType = ToHexString(_tempArray);
                            break;
                        case IVModeRegsAddressType.ErrorCode:
                            IVErrorCode = ToHexString(_tempArray);
                            break;
                        case IVModeRegsAddressType.IVSoftwareVersionNumber:
                            IVSoftwareVersionNumber = ToHexString(_tempArray);
                            break;
                        case IVModeRegsAddressType.IVSensorNo:
                            PMTNumberNo = ToHexString(_tempArray);
                            CurrentSensorNoValue = PMTNumberNo;
                            break;
                        case IVModeRegsAddressType.PGAMultiple:
                            CurrentPGAMultipleValue = (int)Math.Pow(2, _DataField);
                            break;
                        case IVModeRegsAddressType.APDGain:
                            CurrentAPDGainValue = _DataField;
                            break;
                        case IVModeRegsAddressType.APDTemp:
                            CurrentAPDTempValue = _DataField * 0.01; 
                            break;
                        case IVModeRegsAddressType.APDHighVoltage:
                            CurrentAPDHighVoltageValue = _DataField * 0.01; 
                            break;
                        case IVModeRegsAddressType.LightIntensityCalibrationTemperature:
                            CurrentLightIntensityCalibrationTemperatureValue = _DataField * 0.01; 
                            break;
                        case IVModeRegsAddressType.Gain50CalVol:
                            CurrentGain50CalVolValue = _DataField * 0.01; 
                            break;
                        case IVModeRegsAddressType.Gain100CalVol:
                            CurrentGain100CalVolValue = _DataField * 0.01; 
                            break;
                        case IVModeRegsAddressType.Gain150CalVol:
                            CurrentGain150CalVolValue = _DataField * 0.01; 
                            break;
                        case IVModeRegsAddressType.Gain200CalVol:
                            CurrentGain200CalVolValue = _DataField * 0.01; 
                            break;
                        case IVModeRegsAddressType.Gain250CalVol:
                            CurrentGain250CalVolValue = _DataField * 0.01; 
                            break;
                        case IVModeRegsAddressType.Gain300CalVol:
                            CurrentGain300CalVolValue = _DataField * 0.01; 
                            break;
                        case IVModeRegsAddressType.Gain400CalVol:
                            CurrentGain400CalVolValue = _DataField * 0.01; 
                            break;
                        case IVModeRegsAddressType.Gain500CalVol:
                            CurrentGain500CalVolValue = _DataField * 0.01; 
                            break;
                        case IVModeRegsAddressType.PMTCtrlVol:
                            CurrentPMTCtrlVolValue = _DataField * 0.1; 
                            break;
                        case IVModeRegsAddressType.PMTCompensationCoefficient:
                            //CurrentPMTCompensationCoefficientValue = _DataField * 0.0001;//非10000000版本号，允许输入负数
                            CurrentPMTCompensationCoefficientValue = _DataField;//非10000000版本号，允许输入负数
                            break;
                        case IVModeRegsAddressType.TempSenser1282AD:
                            CurrentTempSenser1282ADValue = _DataField; 
                            break;
                        case IVModeRegsAddressType.TempSenser6459AD:
                            CurrentTempSenser6459ADValue = _DataField; 
                            break;
                        case IVModeRegsAddressType.TempSenserAD:
                            CurrentTempSenserADValue = _DataField; 
                            break;
                        case IVModeRegsAddressType.IVBoardRunningState:
                            CurrentIVBoardRunningStateValue = _DataField;
                            break;
                        case IVModeRegsAddressType.TemperatureSensorSamplingVoltage:
                            CurrentTemperatureSensorSamplingVoltageValue = _DataField * 0.01;
                            break;
                        case IVModeRegsAddressType.ADPTemperatureCalibrationFactor:
                            CurrentADPTemperatureCalibrationFactorValue = _DataField * 0.01;
                            break;

                    }
                }
                if(IsPrint)
                   UpdateCommOutput();
            }
        }

        private List<byte[]> ByteGroup(byte[] data, int groups)
        {
            if (data.Count() % groups != 0) throw new Exception("ERR:索引数出错");//检查序号数 
            List<byte[]> vs = new List<byte[]>();
            int forCount = data.Count() / groups;//获取循环次数
            int index = 0;//从第一个开始
            for (int i = 0; i < forCount; i++)
            {
                List<byte> byteps = new List<byte>();
                for (int f = index; f < groups + index; f++)
                {
                    byteps.Add(data[f]);
                    //vs.Add(data.Skip(a).Take(groups).ToArray());//表示从第*个位置截取*个字节 这写法会排序不知道为啥
                }
                vs.Add(byteps.ToArray());
                index += groups;
            }
            return vs;
        }

        public void ProcessData()
        {
            while (true)
            {
                int index = 0;
                int datalength = 10;
                try
                {
                    while (PortListData.Count >= datalength)
                    {
                        if (PortListData[0] == _FrameHeader) //判断收到的首字符 [
                        {
                            if (PortListData[index] != _FrameEnd) //直到收到尾字符为 ] 停下
                            {
                                index++;
                                if (index >= PortListData.Count)
                                {
                                    break;
                                }
                            }
                            else
                            {
                                if (index + 1 == datalength)
                                {
                                    byte[] ReceiveBytes = new byte[index + 1];
                                    PortListData.CopyTo(0, ReceiveBytes, 0, index + 1);
                                    RunReceiveDataCallback(ReceiveBytes);
                                }
                                PortListData.RemoveRange(0, index+1); //数据处理
                            }
                        }
                        else
                        {
                            PortListData.RemoveAt(0);//首字符不是我们想要的,删除
                        }
                    }
                }
                catch
                {
                    index = 0;
                }

            }
          
        }

        #region Pri
        //
        public bool IsBusy { get => _IsBusy; set => _IsBusy = value; }
        string _LaserWaveLength;
        string _LaserNumberNo;
        string _PMTNumberNo;
        double _TECCtrlTemp = 0.0;
        double _LaserPowerValue = 0.0;
        double _LightElectricTowVole = 0.0;
        double _Not532LaserPower = 0.0;
        double _True532LaserPower = 0.0;
        string _GetCurrentSensorNoValue;
        int _GetCurrentPGAMultipleValue;
        int _GetCurrentAPDGainValue;
        double _GetCurrentAPDTempValue;
        double _GetCurrentAPDHighVoltageValue;
        double _GetCurrentLightIntensityCalibrationTemperatureValue;
        double _GetCurrentGain50CalVolValue;
        double _GetCurrentGain100CalVolValue;
        double _GetCurrentGain150CalVolValue;
        double _GetCurrentGain200CalVolValue;
        double _GetCurrentGain250CalVolValue;
        double _GetCurrentGain300CalVolValue;
        double _GetCurrentGain400CalVolValue;
        double _GetCurrentGain500CalVolValue;
        double _GetCurrentPMTCtrlVolValue;
        double _GetCurrentPMTCompensationCoefficientValue;
        double _GetCurrentTempSenser1282ADValue;
        double _GetCurrentTempSenser6459ADValue;
        double _GetCurrentIVBoardRunningStateValue;
        double _GetCurrentTempSenserADValue;
        double _GetCurrentTemperatureSensorSamplingVoltageValue;
        double _GetCurrentADPTemperatureCalibrationFactorValue;

        string _GetCurrentLaserNoValue;
        int _GetCurrentLaserWavaLengthValue;
        double _GetCurrentLaserCurrentValueValue;
        double _GetCurrentLaserCorrespondingCurrentValueValue;
        double _GetCurrentLaserCorrespondingCurrent5ValueValue;
        double _GetCurrentLaserCorrespondingCurrent10ValueValue;
        double _GetCurrentLaserCorrespondingCurrent15ValueValue;
        double _GetCurrentLaserCorrespondingCurrent20ValueValue;
        double _GetCurrentLaserCorrespondingCurrent25ValueValue;
        double _GetCurrentLaserCorrespondingCurrent30ValueValue;
        double _GetCurrentLaserCorrespondingCurrent35ValueValue;
        double _GetCurrentLaserCorrespondingCurrent40ValueValue;
        double _GetCurrentLaserCorrespondingCurrent45ValueValue;
        double _GetCurrentLaserCorrespondingCurrent50ValueValue;
        double _GetCurrentTECActualTemperatureValue;
        double _GetCurrentTEControlTemperatureValue;
        double _GetCurrentTECMaximumCoolingCurrentValue;
        double _GetCurrentTECRefrigerationControlParameterKpValue;
        double _GetCurrentTECRefrigerationControlParameterKiValue;
        double _GetCurrentTECRefrigerationControlParameterKdValue;
        double _GetCurrentLaserLightPowerValueValue;
        double _GetCurrentTECWorkingStatusValue;
        double _GetCurrentTECCurrentDirectionValue;
        double _GetCurrenRadiatorTemperatureValue;
        double _GetCurrenTECCurrentCompensationCoefficientValue;
        double _Get_CurrentLaserLightPowerValueValue;
        double _GetCurrentPowerClosedloopControlParameterKpValue;
        double _GetCurrentPowerClosedloopControlParameterKiValue;
        double _GetCurrentPowerClosedloopControlParameterKdValue;
        double _GetCurrentPhotodiodeVoltageCorrespondingToLaserPowerValue;
        double _GetCurrentPhotodiodeVoltageCorrespondingTo5LaserPowerValue;
        double _GetCurrentPhotodiodeVoltageCorrespondingTo10LaserPowerValue;
        double _GetCurrentPhotodiodeVoltageCorrespondingTo15LaserPowerValue;
        double _GetCurrentPhotodiodeVoltageCorrespondingTo20LaserPowerValue;
        double _GetCurrentPhotodiodeVoltageCorrespondingTo25LaserPowerValue;
        double _GetCurrentPhotodiodeVoltageCorrespondingTo30LaserPowerValue;
        double _GetCurrentPhotodiodeVoltageCorrespondingTo35LaserPowerValue;
        double _GetCurrentPhotodiodeVoltageCorrespondingTo40LaserPowerValue;
        double _GetCurrentPhotodiodeVoltageCorrespondingTo45LaserPowerValue;
        double _GetCurrentPhotodiodeVoltageCorrespondingTo50LaserPowerValue;
        double _GetCurrentPhotodiodeVoltageCorrespondingTo55LaserPowerValue;
        double _GetCurrentPhotodiodeVoltageValue;
        double _GetCurrentKValueofPhotodiodeTemperatureCurveValue;
        double _GetCurrentBValueofPhotodiodeTemperatureCurveValue;
        double _PowerClosedloopControlParameterKpLessThanOrEqual15;//功率闭环控制参数kp <=15
        double _PowerClosedloopControlParameterKiLessThanOrEqual15;//功率闭环控制参数ki <=15
        double _PowerClosedloopControlParameterKdLessThanOrEqual15;//功率闭环控制参数kd <=15
        double _UpperLimitKpLessThan15;//15MW及以下的KP的最大设置值 
        double _LowerLimitKpLessThan15;//15MW及以下的Kp的最小设置值
        double _UpperLimitKiLessThan15;//15MW及以下的Ki的最大设置值
        double _LowerLimitKiLessThan15;//15MW及以下的Ki的最小设置值
        double _UpperLimitKpGreaterThan15;//15MW以上的Kp的最大设置值
        double _LowerLimitKpGreaterThan15;//15MW以上的Kp的最小设置值
        double _UpperLimitKiGreaterThan15;//15MW以上的Ki的最大设置值
        double _LowerLimitKiGreaterThan15;//15MW以上的Ki的最小设置值
        double _MaximumOperatingCurrentLaser;//激光器的最大工作电流
        double _MinimumOperatingCurrentLaser;//激光器的最小工作电流

        public double True532LaserPower
        {
            get
            {
                return _True532LaserPower;
            }
            set
            {
                if (_True532LaserPower != value)
                {
                    _True532LaserPower = value;
                }
            }
        }
        public double Not532LaserPower
        {
            get
            {
                return _Not532LaserPower;
            }
            set
            {
                if (_Not532LaserPower != value)
                {
                    _Not532LaserPower = value;
                }
            }
        }
        public double LightElectricTowVole
        {
            get
            {
                return _LightElectricTowVole;
            }
            set
            {
                if (_LightElectricTowVole != value)
                {
                    _LightElectricTowVole = value;
                }
            }
        }
        public double LaserPowerValue
        {
            get
            {
                return _LaserPowerValue;
            }
            set
            {
                if (_LaserPowerValue != value)
                {
                    _LaserPowerValue = value;
                }
            }
        }
        public double TECCtrlTemp
        {
            get
            {
                return _TECCtrlTemp;
            }
            set
            {
                if (_TECCtrlTemp != value)
                {
                    _TECCtrlTemp = value;
                }
            }
        }
        private string _IVSoftwareVersionNumber = "";
        public string IVSoftwareVersionNumber
        {
            get { return _IVSoftwareVersionNumber; }
            set
            {

                if (_IVSoftwareVersionNumber != value)
                {
                    _IVSoftwareVersionNumber = value;
                }
            }
        }
        private string _IVSensorType = "";
        public string IVSensorType
        {
            get { return _IVSensorType; }
            set
            {

                if (_IVSensorType != value)
                {
                    _IVSensorType = value;
                }
            }
        }
        private string _IVErrorCode = "";
        public string IVErrorCode
        {
            get { return _IVErrorCode; }
            set
            {

                if (_IVErrorCode != value)
                {
                    _IVErrorCode = value;
                }
            }
        }
        private string _IVOpticalModuleNumberValue = "";
        public string IVOpticalModuleNumberValue
        {
            get { return _IVOpticalModuleNumberValue; }
            set
            {

                if (_IVOpticalModuleNumberValue != value)
                {
                    _IVOpticalModuleNumberValue = value;
                }
            }
        }
        public string PMTNumberNo
        {
            get
            {
                return _PMTNumberNo;
            }
            set
            {
                if (_PMTNumberNo != value)
                {
                    _PMTNumberNo = value;
                }
            }
        }
        public string LaserNumberNo
        {
            get
            {
                return _LaserNumberNo;
            }
            set
            {
                if (_LaserNumberNo != value)
                {
                    _LaserNumberNo = value;
                }
            }
        }
        public string LaserWaveLength
        {
            get
            {
                return _LaserWaveLength;
            }
            set
            {
                if (_LaserWaveLength != value)
                {
                    _LaserWaveLength = value;
                }
            }
        }

        public string CurrentSensorNoValue
        {
            get { return _GetCurrentSensorNoValue; }
            set
            {

                if (_GetCurrentSensorNoValue != value)
                {
                    _GetCurrentSensorNoValue = value;
                }
            }
        }

        public int CurrentPGAMultipleValue
        {
            get { return _GetCurrentPGAMultipleValue; }
            set
            {

                if (_GetCurrentPGAMultipleValue != value)
                {
                    _GetCurrentPGAMultipleValue = value;
                }
            }
        }

        public int CurrentAPDGainValue
        {
            get { return _GetCurrentAPDGainValue; }
            set
            {

                if (_GetCurrentAPDGainValue != value)
                {
                    _GetCurrentAPDGainValue = value;
                }
            }
        }

        public double CurrentAPDHighVoltageValue
        {
            get { return _GetCurrentAPDHighVoltageValue; }
            set
            {

                if (_GetCurrentAPDHighVoltageValue != value)
                {
                    _GetCurrentAPDHighVoltageValue = value;
                }
            }
        }

        public double CurrentLightIntensityCalibrationTemperatureValue
        {
            get { return _GetCurrentLightIntensityCalibrationTemperatureValue; }
            set
            {

                if (_GetCurrentLightIntensityCalibrationTemperatureValue != value)
                {
                    _GetCurrentLightIntensityCalibrationTemperatureValue = value;
                }
            }
        }

        public double CurrentGain50CalVolValue
        {
            get { return _GetCurrentGain50CalVolValue; }
            set
            {

                if (_GetCurrentGain50CalVolValue != value)
                {
                    _GetCurrentGain50CalVolValue = value;
                }
            }
        }

        public double CurrentGain100CalVolValue
        {
            get { return _GetCurrentGain100CalVolValue; }
            set
            {

                if (_GetCurrentGain100CalVolValue != value)
                {
                    _GetCurrentGain100CalVolValue = value;
                }
            }
        }

        public double CurrentGain150CalVolValue
        {
            get { return _GetCurrentGain150CalVolValue; }
            set
            {

                if (_GetCurrentGain150CalVolValue != value)
                {
                    _GetCurrentGain150CalVolValue = value;
                }
            }
        }

        public double CurrentGain200CalVolValue
        {
            get { return _GetCurrentGain200CalVolValue; }
            set
            {

                if (_GetCurrentGain200CalVolValue != value)
                {
                    _GetCurrentGain200CalVolValue = value;
                }
            }
        }

        public double CurrentGain250CalVolValue
        {
            get { return _GetCurrentGain250CalVolValue; }
            set
            {

                if (_GetCurrentGain250CalVolValue != value)
                {
                    _GetCurrentGain250CalVolValue = value;
                }
            }
        }

        public double CurrentGain300CalVolValue
        {
            get { return _GetCurrentGain300CalVolValue; }
            set
            {

                if (_GetCurrentGain300CalVolValue != value)
                {
                    _GetCurrentGain300CalVolValue = value;
                }
            }
        }

        public double CurrentGain400CalVolValue
        {
            get { return _GetCurrentGain400CalVolValue; }
            set
            {

                if (_GetCurrentGain400CalVolValue != value)
                {
                    _GetCurrentGain400CalVolValue = value;
                }
            }
        }

        public double CurrentGain500CalVolValue
        {
            get { return _GetCurrentGain500CalVolValue; }
            set
            {

                if (_GetCurrentGain500CalVolValue != value)
                {
                    _GetCurrentGain500CalVolValue = value;
                }
            }
        }

        public double CurrentPMTCtrlVolValue
        {
            get { return _GetCurrentPMTCtrlVolValue; }
            set
            {

                if (_GetCurrentPMTCtrlVolValue != value)
                {
                    _GetCurrentPMTCtrlVolValue = value;
                }
            }
        }

        public double CurrentAPDTempValue
        {
            get { return _GetCurrentAPDTempValue; }
            set
            {

                if (_GetCurrentAPDTempValue != value)
                {
                    _GetCurrentAPDTempValue = value;
                }
            }
        }

        public double CurrentPMTCompensationCoefficientValue
        {
            get { return _GetCurrentPMTCompensationCoefficientValue; }
            set
            {

                if (_GetCurrentPMTCompensationCoefficientValue != value)
                {
                    _GetCurrentPMTCompensationCoefficientValue = value;
                }
            }
        }

        public double CurrentTempSenser6459ADValue
        {
            get { return _GetCurrentTempSenser6459ADValue; }
            set
            {

                if (_GetCurrentTempSenser6459ADValue != value)
                {
                    _GetCurrentTempSenser6459ADValue = value;
                }
            }
        }

        public double CurrentTempSenser1282ADValue
        {
            get { return _GetCurrentTempSenser1282ADValue; }
            set
            {

                if (_GetCurrentTempSenser1282ADValue != value)
                {
                    _GetCurrentTempSenser1282ADValue = value;
                }
            }
        }

        public double CurrentTempSenserADValue
        {
            get { return _GetCurrentTempSenserADValue; }
            set
            {

                if (_GetCurrentTempSenserADValue != value)
                {
                    _GetCurrentTempSenserADValue = value;
                }
            }
        }

        public double CurrentIVBoardRunningStateValue
        {
            get { return _GetCurrentIVBoardRunningStateValue; }
            set
            {

                if (_GetCurrentIVBoardRunningStateValue != value)
                {
                    _GetCurrentIVBoardRunningStateValue = value;
                }
            }
        }

        public double CurrentTemperatureSensorSamplingVoltageValue
        {
            get { return _GetCurrentTemperatureSensorSamplingVoltageValue; }
            set
            {

                if (_GetCurrentTemperatureSensorSamplingVoltageValue != value)
                {
                    _GetCurrentTemperatureSensorSamplingVoltageValue = value;
                }
            }
        }

        public double CurrentADPTemperatureCalibrationFactorValue
        {
            get { return _GetCurrentADPTemperatureCalibrationFactorValue; }
            set
            {

                if (_GetCurrentADPTemperatureCalibrationFactorValue != value)
                {
                    _GetCurrentADPTemperatureCalibrationFactorValue = value;
                }
            }
        }
        private string _LaserSoftwareVersionNumber = "";
        public string LaserSoftwareVersionNumber
        {
            get { return _LaserSoftwareVersionNumber; }
            set
            {

                if (_LaserSoftwareVersionNumber != value)
                {
                    _LaserSoftwareVersionNumber = value;
                }
            }
        }

        private string _LaserErrorCode = "";
        public string LaserErrorCode
        {
            get { return _LaserErrorCode; }
            set
            {

                if (_LaserErrorCode != value)
                {
                    _LaserErrorCode = value;
                }
            }
        }
        private string _LaserOpticalModuleNumberValue = "";
        public string LaserOpticalModuleNumberValue
        {
            get { return _LaserOpticalModuleNumberValue; }
            set
            {

                if (_LaserOpticalModuleNumberValue != value)
                {
                    _LaserOpticalModuleNumberValue = value;
                }
            }
        }

        public string CurrentLaserNoValue
        {
            get { return _GetCurrentLaserNoValue; }
            set
            {

                if (_GetCurrentLaserNoValue != value)
                {
                    _GetCurrentLaserNoValue = value;
                }
            }
        }

        public int CurrentLaserWavaLengthValue
        {
            get { return _GetCurrentLaserWavaLengthValue; }
            set
            {

                if (_GetCurrentLaserWavaLengthValue != value)
                {
                    _GetCurrentLaserWavaLengthValue = value;
                }
            }
        }

        public double CurrentLaserCurrentValueValue
        {
            get { return _GetCurrentLaserCurrentValueValue; }
            set
            {

                if (_GetCurrentLaserCurrentValueValue != value)
                {
                    _GetCurrentLaserCurrentValueValue = value;
                }
            }
        }

        public double CurrentLaserCorrespondingCurrentValueValue
        {
            get { return _GetCurrentLaserCorrespondingCurrentValueValue; }
            set
            {

                if (_GetCurrentLaserCorrespondingCurrentValueValue != value)
                {
                    _GetCurrentLaserCorrespondingCurrentValueValue = value;
                }
            }
        }
        public double CurrentLaserCorrespondingCurrent5ValueValue
        {
            get { return _GetCurrentLaserCorrespondingCurrent5ValueValue; }
            set
            {

                if (_GetCurrentLaserCorrespondingCurrent5ValueValue != value)
                {
                    _GetCurrentLaserCorrespondingCurrent5ValueValue = value;
                }
            }
        }

        public double CurrentLaserCorrespondingCurrent10ValueValue
        {
            get { return _GetCurrentLaserCorrespondingCurrent10ValueValue; }
            set
            {

                if (_GetCurrentLaserCorrespondingCurrent10ValueValue != value)
                {
                    _GetCurrentLaserCorrespondingCurrent10ValueValue = value;
                }
            }
        }

        public double CurrentLaserCorrespondingCurrent15ValueValue
        {
            get { return _GetCurrentLaserCorrespondingCurrent15ValueValue; }
            set
            {

                if (_GetCurrentLaserCorrespondingCurrent15ValueValue != value)
                {
                    _GetCurrentLaserCorrespondingCurrent15ValueValue = value;
                }
            }
        }

        public double CurrentLaserCorrespondingCurrent20ValueValue
        {
            get { return _GetCurrentLaserCorrespondingCurrent20ValueValue; }
            set
            {

                if (_GetCurrentLaserCorrespondingCurrent20ValueValue != value)
                {
                    _GetCurrentLaserCorrespondingCurrent20ValueValue = value;
                }
            }
        }

        public double CurrentLaserCorrespondingCurrent25ValueValue
        {
            get { return _GetCurrentLaserCorrespondingCurrent25ValueValue; }
            set
            {

                if (_GetCurrentLaserCorrespondingCurrent25ValueValue != value)
                {
                    _GetCurrentLaserCorrespondingCurrent25ValueValue = value;
                }
            }
        }

        public double CurrentLaserCorrespondingCurrent30ValueValue
        {
            get { return _GetCurrentLaserCorrespondingCurrent30ValueValue; }
            set
            {

                if (_GetCurrentLaserCorrespondingCurrent30ValueValue != value)
                {
                    _GetCurrentLaserCorrespondingCurrent30ValueValue = value;
                }
            }
        }
        public double CurrentLaserCorrespondingCurrent35ValueValue
        {
            get { return _GetCurrentLaserCorrespondingCurrent35ValueValue; }
            set
            {

                if (_GetCurrentLaserCorrespondingCurrent35ValueValue != value)
                {
                    _GetCurrentLaserCorrespondingCurrent35ValueValue = value;
                }
            }
        }

        public double CurrentLaserCorrespondingCurrent40ValueValue
        {
            get { return _GetCurrentLaserCorrespondingCurrent40ValueValue; }
            set
            {

                if (_GetCurrentLaserCorrespondingCurrent40ValueValue != value)
                {
                    _GetCurrentLaserCorrespondingCurrent40ValueValue = value;
                }
            }
        }

        public double CurrentLaserCorrespondingCurrent45ValueValue
        {
            get { return _GetCurrentLaserCorrespondingCurrent45ValueValue; }
            set
            {

                if (_GetCurrentLaserCorrespondingCurrent45ValueValue != value)
                {
                    _GetCurrentLaserCorrespondingCurrent45ValueValue = value;
                }
            }
        }

        public double CurrentLaserCorrespondingCurrent50ValueValue
        {
            get { return _GetCurrentLaserCorrespondingCurrent50ValueValue; }
            set
            {

                if (_GetCurrentLaserCorrespondingCurrent50ValueValue != value)
                {
                    _GetCurrentLaserCorrespondingCurrent50ValueValue = value;
                }
            }
        }

        public double CurrentTECActualTemperatureValue
        {
            get { return _GetCurrentTECActualTemperatureValue; }
            set
            {

                if (_GetCurrentTECActualTemperatureValue != value)
                {
                    _GetCurrentTECActualTemperatureValue = value;
                }
            }
        }

        public double CurrentTEControlTemperatureValue
        {
            get { return _GetCurrentTEControlTemperatureValue; }
            set
            {

                if (_GetCurrentTEControlTemperatureValue != value)
                {
                    _GetCurrentTEControlTemperatureValue = value;
                }
            }
        }

        public double CurrentTECMaximumCoolingCurrentValue
        {
            get { return _GetCurrentTECMaximumCoolingCurrentValue; }
            set
            {

                if (_GetCurrentTECMaximumCoolingCurrentValue != value)
                {
                    _GetCurrentTECMaximumCoolingCurrentValue = value;
                }
            }
        }

        public double CurrentTECRefrigerationControlParameterKpValue
        {
            get { return _GetCurrentTECRefrigerationControlParameterKpValue; }
            set
            {

                if (_GetCurrentTECRefrigerationControlParameterKpValue != value)
                {
                    _GetCurrentTECRefrigerationControlParameterKpValue = value;
                }
            }
        }

        public double CurrentTECRefrigerationControlParameterKiValue
        {
            get { return _GetCurrentTECRefrigerationControlParameterKiValue; }
            set
            {

                if (_GetCurrentTECRefrigerationControlParameterKiValue != value)
                {
                    _GetCurrentTECRefrigerationControlParameterKiValue = value;
                }
            }
        }

        public double CurrentTECRefrigerationControlParameterKdValue
        {
            get { return _GetCurrentTECRefrigerationControlParameterKdValue; }
            set
            {

                if (_GetCurrentTECRefrigerationControlParameterKdValue != value)
                {
                    _GetCurrentTECRefrigerationControlParameterKdValue = value;
                }
            }
        }

        public double CurrentLaserLightPowerValueValue
        {
            get { return _GetCurrentLaserLightPowerValueValue; }
            set
            {

                if (_GetCurrentLaserLightPowerValueValue != value)
                {
                    _GetCurrentLaserLightPowerValueValue = value;
                }
            }
        }

        public double CurrentTECWorkingStatusValue
        {
            get { return _GetCurrentTECWorkingStatusValue; }
            set
            {

                if (_GetCurrentTECWorkingStatusValue != value)
                {
                    _GetCurrentTECWorkingStatusValue = value;
                }
            }
        }

        public double CurrentTECCurrentDirectionValue
        {
            get { return _GetCurrentTECCurrentDirectionValue; }
            set
            {

                if (_GetCurrentTECCurrentDirectionValue != value)
                {
                    _GetCurrentTECCurrentDirectionValue = value;
                }
            }
        }

        public double CurrenRadiatorTemperatureValue
        {
            get { return _GetCurrenRadiatorTemperatureValue; }
            set
            {

                if (_GetCurrenRadiatorTemperatureValue != value)
                {
                    _GetCurrenRadiatorTemperatureValue = value;
                }
            }
        }

        public double CurrenTECCurrentCompensationCoefficientValue
        {
            get { return _GetCurrenTECCurrentCompensationCoefficientValue; }
            set
            {

                if (_GetCurrenTECCurrentCompensationCoefficientValue != value)
                {
                    _GetCurrenTECCurrentCompensationCoefficientValue = value;
                }
            }
        }

        public double GetGet_CurrentLaserLightPowerValueValue
        {
            get { return _Get_CurrentLaserLightPowerValueValue; }
            set
            {

                if (_Get_CurrentLaserLightPowerValueValue != value)
                {
                    _Get_CurrentLaserLightPowerValueValue = value;
                }
            }
        }

        public double CurrentPowerClosedloopControlParameterKpValue
        {
            get { return _GetCurrentPowerClosedloopControlParameterKpValue; }
            set
            {

                if (_GetCurrentPowerClosedloopControlParameterKpValue != value)
                {
                    _GetCurrentPowerClosedloopControlParameterKpValue = value;
                }
            }
        }

        public double CurrentPowerClosedloopControlParameterKiValue
        {
            get { return _GetCurrentPowerClosedloopControlParameterKiValue; }
            set
            {

                if (_GetCurrentPowerClosedloopControlParameterKiValue != value)
                {
                    _GetCurrentPowerClosedloopControlParameterKiValue = value;
                }
            }
        }

        public double CurrentPowerClosedloopControlParameterKdValue
        {
            get { return _GetCurrentPowerClosedloopControlParameterKdValue; }
            set
            {

                if (_GetCurrentPowerClosedloopControlParameterKdValue != value)
                {
                    _GetCurrentPowerClosedloopControlParameterKdValue = value;
                }
            }
        }

        public double CurrentPhotodiodeVoltageCorrespondingToLaserPowerValue
        {
            get { return _GetCurrentPhotodiodeVoltageCorrespondingToLaserPowerValue; }
            set
            {

                if (_GetCurrentPhotodiodeVoltageCorrespondingToLaserPowerValue != value)
                {
                    _GetCurrentPhotodiodeVoltageCorrespondingToLaserPowerValue = value;
                }
            }
        }

        public double CurrentPhotodiodeVoltageCorrespondingTo5LaserPowerValue
        {
            get { return _GetCurrentPhotodiodeVoltageCorrespondingTo5LaserPowerValue; }
            set
            {

                if (_GetCurrentPhotodiodeVoltageCorrespondingTo5LaserPowerValue != value)
                {
                    _GetCurrentPhotodiodeVoltageCorrespondingTo5LaserPowerValue = value;
                }
            }
        }

        public double CurrentPhotodiodeVoltageCorrespondingTo10LaserPowerValue
        {
            get { return _GetCurrentPhotodiodeVoltageCorrespondingTo10LaserPowerValue; }
            set
            {

                if (_GetCurrentPhotodiodeVoltageCorrespondingTo10LaserPowerValue != value)
                {
                    _GetCurrentPhotodiodeVoltageCorrespondingTo10LaserPowerValue = value;
                }
            }
        }

        public double CurrentPhotodiodeVoltageCorrespondingTo15LaserPowerValue
        {
            get { return _GetCurrentPhotodiodeVoltageCorrespondingTo15LaserPowerValue; }
            set
            {

                if (_GetCurrentPhotodiodeVoltageCorrespondingTo15LaserPowerValue != value)
                {
                    _GetCurrentPhotodiodeVoltageCorrespondingTo15LaserPowerValue = value;
                }
            }
        }

        public double CurrentPhotodiodeVoltageCorrespondingTo20LaserPowerValue
        {
            get { return _GetCurrentPhotodiodeVoltageCorrespondingTo20LaserPowerValue; }
            set
            {

                if (_GetCurrentPhotodiodeVoltageCorrespondingTo20LaserPowerValue != value)
                {
                    _GetCurrentPhotodiodeVoltageCorrespondingTo20LaserPowerValue = value;
                }
            }
        }

        public double CurrentPhotodiodeVoltageCorrespondingTo25LaserPowerValue
        {
            get { return _GetCurrentPhotodiodeVoltageCorrespondingTo25LaserPowerValue; }
            set
            {

                if (_GetCurrentPhotodiodeVoltageCorrespondingTo25LaserPowerValue != value)
                {
                    _GetCurrentPhotodiodeVoltageCorrespondingTo25LaserPowerValue = value;
                }
            }
        }

        public double CurrentPhotodiodeVoltageCorrespondingTo30LaserPowerValue
        {
            get { return _GetCurrentPhotodiodeVoltageCorrespondingTo30LaserPowerValue; }
            set
            {

                if (_GetCurrentPhotodiodeVoltageCorrespondingTo30LaserPowerValue != value)
                {
                    _GetCurrentPhotodiodeVoltageCorrespondingTo30LaserPowerValue = value;
                }
            }
        }

        public double CurrentPhotodiodeVoltageCorrespondingTo35LaserPowerValue
        {
            get { return _GetCurrentPhotodiodeVoltageCorrespondingTo35LaserPowerValue; }
            set
            {

                if (_GetCurrentPhotodiodeVoltageCorrespondingTo35LaserPowerValue != value)
                {
                    _GetCurrentPhotodiodeVoltageCorrespondingTo35LaserPowerValue = value;
                }
            }
        }

        public double CurrentPhotodiodeVoltageCorrespondingTo40LaserPowerValue
        {
            get { return _GetCurrentPhotodiodeVoltageCorrespondingTo40LaserPowerValue; }
            set
            {

                if (_GetCurrentPhotodiodeVoltageCorrespondingTo40LaserPowerValue != value)
                {
                    _GetCurrentPhotodiodeVoltageCorrespondingTo40LaserPowerValue = value;
                }
            }
        }

        public double CurrentPhotodiodeVoltageCorrespondingTo45LaserPowerValue
        {
            get { return _GetCurrentPhotodiodeVoltageCorrespondingTo45LaserPowerValue; }
            set
            {

                if (_GetCurrentPhotodiodeVoltageCorrespondingTo45LaserPowerValue != value)
                {
                    _GetCurrentPhotodiodeVoltageCorrespondingTo45LaserPowerValue = value;
                }
            }
        }

        public double CurrentPhotodiodeVoltageCorrespondingTo50LaserPowerValue
        {
            get { return _GetCurrentPhotodiodeVoltageCorrespondingTo50LaserPowerValue; }
            set
            {

                if (_GetCurrentPhotodiodeVoltageCorrespondingTo50LaserPowerValue != value)
                {
                    _GetCurrentPhotodiodeVoltageCorrespondingTo50LaserPowerValue = value;
                }
            }
        }

        public double CurrentPhotodiodeVoltageValue
        {
            get { return _GetCurrentPhotodiodeVoltageValue; }
            set
            {

                if (_GetCurrentPhotodiodeVoltageValue != value)
                {
                    _GetCurrentPhotodiodeVoltageValue = value;
                }
            }
        }

        public double CurrentKValueofPhotodiodeTemperatureCurveValue
        {
            get { return _GetCurrentKValueofPhotodiodeTemperatureCurveValue; }
            set
            {

                if (_GetCurrentKValueofPhotodiodeTemperatureCurveValue != value)
                {
                    _GetCurrentKValueofPhotodiodeTemperatureCurveValue = value;
                }
            }
        }

        public double CurrentBValueofPhotodiodeTemperatureCurveValue
        {
            get { return _GetCurrentBValueofPhotodiodeTemperatureCurveValue; }
            set
            {

                if (_GetCurrentBValueofPhotodiodeTemperatureCurveValue != value)
                {
                    _GetCurrentBValueofPhotodiodeTemperatureCurveValue = value;
                }
            }
        }
        public double PowerClosedloopControlParameterKpLessThanOrEqual15
        {
            get { return _PowerClosedloopControlParameterKpLessThanOrEqual15; }
            set
            {

                if (_PowerClosedloopControlParameterKpLessThanOrEqual15 != value)
                {
                    _PowerClosedloopControlParameterKpLessThanOrEqual15 = value;
                }
            }
        }
        public double PowerClosedloopControlParameterKiLessThanOrEqual15
        {
            get { return _PowerClosedloopControlParameterKiLessThanOrEqual15; }
            set
            {

                if (_PowerClosedloopControlParameterKiLessThanOrEqual15 != value)
                {
                    _PowerClosedloopControlParameterKiLessThanOrEqual15 = value;
                }
            }
        }
        public double PowerClosedloopControlParameterKdLessThanOrEqual15
        {
            get { return _PowerClosedloopControlParameterKdLessThanOrEqual15; }
            set
            {

                if (_PowerClosedloopControlParameterKdLessThanOrEqual15 != value)
                {
                    _PowerClosedloopControlParameterKdLessThanOrEqual15 = value;
                }
            }
        }
        public double UpperLimitKpLessThan15
        {
            get { return _UpperLimitKpLessThan15; }
            set
            {

                if (_UpperLimitKpLessThan15 != value)
                {
                    _UpperLimitKpLessThan15 = value;
                }
            }
        }
        public double LowerLimitKpLessThan15
        {
            get { return _LowerLimitKpLessThan15; }
            set
            {

                if (_LowerLimitKpLessThan15 != value)
                {
                    _LowerLimitKpLessThan15 = value;
                }
            }
        }
        public double UpperLimitKiLessThan15
        {
            get { return _UpperLimitKiLessThan15; }
            set
            {

                if (_UpperLimitKiLessThan15 != value)
                {
                    _UpperLimitKiLessThan15 = value;
                }
            }
        }
        public double LowerLimitKiLessThan15
        {
            get { return _LowerLimitKiLessThan15; }
            set
            {

                if (_LowerLimitKiLessThan15 != value)
                {
                    _LowerLimitKiLessThan15 = value;
                }
            }
        }
        public double UpperLimitKpGreaterThan15
        {
            get { return _UpperLimitKpGreaterThan15; }
            set
            {

                if (_UpperLimitKpGreaterThan15 != value)
                {
                    _UpperLimitKpGreaterThan15 = value;
                }
            }
        }
        public double LowerLimitKpGreaterThan15
        {
            get { return _LowerLimitKpGreaterThan15; }
            set
            {

                if (_LowerLimitKpGreaterThan15 != value)
                {
                    _LowerLimitKpGreaterThan15 = value;
                }
            }
        }

        public double UpperLimitKiGreaterThan15
        {
            get { return _UpperLimitKiGreaterThan15; }
            set
            {

                if (_UpperLimitKiGreaterThan15 != value)
                {
                    _UpperLimitKiGreaterThan15 = value;
                }
            }
        }
        public double LowerLimitKiGreaterThan15
        {
            get { return _LowerLimitKiGreaterThan15; }
            set
            {

                if (_LowerLimitKiGreaterThan15 != value)
                {
                    _LowerLimitKiGreaterThan15 = value;
                }
            }
        }
        public double MaximumOperatingCurrentLaser
        {
            get { return _MaximumOperatingCurrentLaser; }
            set
            {

                if (_MaximumOperatingCurrentLaser != value)
                {
                    _MaximumOperatingCurrentLaser = value;
                }
            }
        }
        public double MinimumOperatingCurrentLaser
        {
            get { return _MinimumOperatingCurrentLaser; }
            set
            {

                if (_MinimumOperatingCurrentLaser != value)
                {
                    _MinimumOperatingCurrentLaser = value;
                }
            }
        }
        

        #endregion
        public void Dispose()
        {
            if (_Port != null)
            {
                _Port.DataReceived -= _Port_DataReceived;
                _Port.Close();
                _Port = null;
                DataProcessThread.Abort();
                _IsConnected = false;
            }

        }
        public bool IsConnected
        {
            get
            {
                return _IsConnected;
            }
        }
        public string[] AvailablePorts
        {
            get { return _AvailablePorts; }
        }

        public bool IsPrint { get => _IsPrint; set => _IsPrint = value; }

        #region IV板子
        public void SetCurrentSensorNoValue(string number)
        {
            _IsBusy = true;
            byte[] _tempDataField = new byte[4];
            _tempDataField = HexStringToByteArray(number);
            byte[] NumByte = new byte[] { 0x3a, Position2, 0x01, 0x02, 0x00, 0x00, 0x00, 0x00, 0x00, 0x3b };
            if (_tempDataField.Length == 1)
            {
                NumByte[5] = _tempDataField[0];    // data field
            }
            if (_tempDataField.Length == 2)
            {
                NumByte[5] = _tempDataField[0];    // data field
                NumByte[6] = _tempDataField[1];    // data field
            }
            if (_tempDataField.Length == 3)
            {
                NumByte[5] = _tempDataField[0];    // data field
                NumByte[6] = _tempDataField[1];    // data field
                NumByte[7] = _tempDataField[2];    // data field
            }
            if (_tempDataField.Length == 4)
            {
                NumByte[5] = _tempDataField[0];    // data field
                NumByte[6] = _tempDataField[1];    // data field
                NumByte[7] = _tempDataField[2];    // data field
                NumByte[8] = _tempDataField[3];    // data field
            }
            if (_Port != null)
            {
                _Port.Write(NumByte, 0, NumByte.Length);
            }
            _IsBusy = false;
        }

        public void SetCurrentPGAMultiple(int PAG)
        {
            _IsBusy = true;
            byte[] _tempDataField = new byte[4];
            _tempDataField = BitConverter.GetBytes(PAG);
            _DataField = BitConverter.ToInt32(_tempDataField, 0);
            byte[] NumByte = new byte[] { 0x3a, Position2, 0x01, 0x03, 0x00, 0x00, 0x00, 0x00, 0x00, 0x3b };
            if (_tempDataField.Length == 1)
            {
                NumByte[5] = _tempDataField[3];    // data field
            }
            if (_tempDataField.Length == 2)
            {
                NumByte[5] = _tempDataField[3];    // data field
                NumByte[6] = _tempDataField[2];    // data field
            }
            if (_tempDataField.Length == 3)
            {
                NumByte[5] = _tempDataField[3];    // data field
                NumByte[6] = _tempDataField[2];    // data field
                NumByte[7] = _tempDataField[1];    // data field
            }
            if (_tempDataField.Length == 4)
            {
                NumByte[5] = _tempDataField[3];    // data field
                NumByte[6] = _tempDataField[2];    // data field
                NumByte[7] = _tempDataField[1];    // data field
                NumByte[8] = _tempDataField[0];    // data field
            }
            if (_Port != null)
            {
                _Port.Write(NumByte, 0, NumByte.Length);
            }
            _IsBusy = false;
        }

        public void SetCurrentAPDGain(int Gain)
        {
            _IsBusy = true;
            byte[] _tempDataField = new byte[4];
            _tempDataField = BitConverter.GetBytes(Gain);
            _DataField = BitConverter.ToInt32(_tempDataField, 0);
            byte[] NumByte = new byte[] { 0x3a, Position2, 0x01, 0x04, 0x00, 0x00, 0x00, 0x00, 0x00, 0x3b };
            if (_tempDataField.Length == 1)
            {
                NumByte[5] = _tempDataField[3];    // data field
            }
            if (_tempDataField.Length == 2)
            {
                NumByte[5] = _tempDataField[3];    // data field
                NumByte[6] = _tempDataField[2];    // data field
            }
            if (_tempDataField.Length == 3)
            {
                NumByte[5] = _tempDataField[3];    // data field
                NumByte[6] = _tempDataField[2];    // data field
                NumByte[7] = _tempDataField[1];    // data field
            }
            if (_tempDataField.Length == 4)
            {
                NumByte[5] = _tempDataField[3];    // data field
                NumByte[6] = _tempDataField[2];    // data field
                NumByte[7] = _tempDataField[1];    // data field
                NumByte[8] = _tempDataField[0];    // data field
            }
            if (_Port != null)
            {
                _Port.Write(NumByte, 0, NumByte.Length);
            }
            _IsBusy = false;
        }

        public void SetCurrenLightIntensityCalibrationTemperature(double Temp)
        {
            _IsBusy = true;
            byte[] _tempDataField = new byte[4];
            int _tempNumber = (int)Math.Round((double)Temp * 100, 0);
            _tempDataField = BitConverter.GetBytes(_tempNumber);
            byte[] NumByte = new byte[] { 0x3a, Position2, 0x01, 0x07, 0x00, 0x00, 0x00, 0x00, 0x00, 0x3b };
            if (_tempDataField.Length == 1)
            {
                NumByte[5] = _tempDataField[0];    // data field
            }
            if (_tempDataField.Length == 2)
            {
                NumByte[5] = _tempDataField[1];    // data field
                NumByte[6] = _tempDataField[0];    // data field
            }
            if (_tempDataField.Length == 3)
            {
                NumByte[5] = _tempDataField[2];    // data field
                NumByte[6] = _tempDataField[1];    // data field
                NumByte[7] = _tempDataField[0];    // data field
            }
            if (_tempDataField.Length == 4)
            {
                NumByte[5] = _tempDataField[3];    // data field
                NumByte[6] = _tempDataField[2];    // data field
                NumByte[7] = _tempDataField[1];    // data field
                NumByte[8] = _tempDataField[0];    // data field
            }
            if (_Port != null)
            {
                _Port.Write(NumByte, 0, NumByte.Length);
            }
            _IsBusy = false;
        }

        public void SetCurrenCurrenttGain50CalVol(double Value)
        {
            _IsBusy = true;
            byte[] _tempDataField = new byte[4];
            int _tempNumber = (int)Math.Round((double)Value * 100, 0);
            _tempDataField = BitConverter.GetBytes(_tempNumber);
            _DataField = BitConverter.ToInt32(_tempDataField, 0);
            byte[] NumByte = new byte[] { 0x3a, Position2, 0x01, 0x08, 0x00, 0x00, 0x00, 0x00, 0x00, 0x3b };
            if (_tempDataField.Length == 1)
            {
                NumByte[5] = _tempDataField[0];    // data field
            }
            if (_tempDataField.Length == 2)
            {
                NumByte[5] = _tempDataField[1];    // data field
                NumByte[6] = _tempDataField[0];    // data field
            }
            if (_tempDataField.Length == 3)
            {
                NumByte[5] = _tempDataField[2];    // data field
                NumByte[6] = _tempDataField[1];    // data field
                NumByte[7] = _tempDataField[0];    // data field
            }
            if (_tempDataField.Length == 4)
            {
                NumByte[5] = _tempDataField[3];    // data field
                NumByte[6] = _tempDataField[2];    // data field
                NumByte[7] = _tempDataField[1];    // data field
                NumByte[8] = _tempDataField[0];    // data field
            }
            if (_Port != null)
            {
                _Port.Write(NumByte, 0, NumByte.Length);
            }
            _IsBusy = false;
        }

        public void SetCurrenCurrenttGain100CalVol(double Value)
        {
            _IsBusy = true;
            byte[] _tempDataField = new byte[4];
            int _tempNumber = (int)Math.Round((double)Value * 100, 0);
            _tempDataField = BitConverter.GetBytes(_tempNumber);
            _DataField = BitConverter.ToInt32(_tempDataField, 0);
            byte[] NumByte = new byte[] { 0x3a, Position2, 0x01, 0x09, 0x00, 0x00, 0x00, 0x00, 0x00, 0x3b };
            if (_tempDataField.Length == 1)
            {
                NumByte[5] = _tempDataField[0];    // data field
            }
            if (_tempDataField.Length == 2)
            {
                NumByte[5] = _tempDataField[1];    // data field
                NumByte[6] = _tempDataField[0];    // data field
            }
            if (_tempDataField.Length == 3)
            {
                NumByte[5] = _tempDataField[2];    // data field
                NumByte[6] = _tempDataField[1];    // data field
                NumByte[7] = _tempDataField[0];    // data field
            }
            if (_tempDataField.Length == 4)
            {
                NumByte[5] = _tempDataField[3];    // data field
                NumByte[6] = _tempDataField[2];    // data field
                NumByte[7] = _tempDataField[1];    // data field
                NumByte[8] = _tempDataField[0];    // data field
            }
            if (_Port != null)
            {
                _Port.Write(NumByte, 0, NumByte.Length);
            }
            _IsBusy = false;
        }

        public void SetCurrenCurrenttGain150CalVol(double Value)
        {
            _IsBusy = true;
            byte[] _tempDataField = new byte[4];
            int _tempNumber = (int)Math.Round((double)Value * 100, 0);
            _tempDataField = BitConverter.GetBytes(_tempNumber);
            _DataField = BitConverter.ToInt32(_tempDataField, 0);
            byte[] NumByte = new byte[] { 0x3a, Position2, 0x01, 0x0A, 0x00, 0x00, 0x00, 0x00, 0x00, 0x3b };
            if (_tempDataField.Length == 1)
            {
                NumByte[5] = _tempDataField[0];    // data field
            }
            if (_tempDataField.Length == 2)
            {
                NumByte[5] = _tempDataField[1];    // data field
                NumByte[6] = _tempDataField[0];    // data field
            }
            if (_tempDataField.Length == 3)
            {
                NumByte[5] = _tempDataField[2];    // data field
                NumByte[6] = _tempDataField[1];    // data field
                NumByte[7] = _tempDataField[0];    // data field
            }
            if (_tempDataField.Length == 4)
            {
                NumByte[5] = _tempDataField[3];    // data field
                NumByte[6] = _tempDataField[2];    // data field
                NumByte[7] = _tempDataField[1];    // data field
                NumByte[8] = _tempDataField[0];    // data field
            }
            if (_Port != null)
            {
                _Port.Write(NumByte, 0, NumByte.Length);
            }
            _IsBusy = false;
        }

        public void SetCurrenCurrenttGain200CalVol(double Value)
        {
            _IsBusy = true;
            byte[] _tempDataField = new byte[4];
            int _tempNumber = (int)Math.Round((double)Value * 100, 0);
            _tempDataField = BitConverter.GetBytes(_tempNumber);
            _DataField = BitConverter.ToInt32(_tempDataField, 0);
            byte[] NumByte = new byte[] { 0x3a, Position2, 0x01, 0x0B, 0x00, 0x00, 0x00, 0x00, 0x00, 0x3b };
            if (_tempDataField.Length == 1)
            {
                NumByte[5] = _tempDataField[0];    // data field
            }
            if (_tempDataField.Length == 2)
            {
                NumByte[5] = _tempDataField[1];    // data field
                NumByte[6] = _tempDataField[0];    // data field
            }
            if (_tempDataField.Length == 3)
            {
                NumByte[5] = _tempDataField[2];    // data field
                NumByte[6] = _tempDataField[1];    // data field
                NumByte[7] = _tempDataField[0];    // data field
            }
            if (_tempDataField.Length == 4)
            {
                NumByte[5] = _tempDataField[3];    // data field
                NumByte[6] = _tempDataField[2];    // data field
                NumByte[7] = _tempDataField[1];    // data field
                NumByte[8] = _tempDataField[0];    // data field
            }
            if (_Port != null)
            {
                _Port.Write(NumByte, 0, NumByte.Length);
            }
            _IsBusy = false;
        }

        public void SetCurrenCurrenttGain250CalVol(double Value)
        {
            _IsBusy = true;
            byte[] _tempDataField = new byte[4];
            int _tempNumber = (int)Math.Round((double)Value * 100, 0);
            _tempDataField = BitConverter.GetBytes(_tempNumber);
            _DataField = BitConverter.ToInt32(_tempDataField, 0);
            byte[] NumByte = new byte[] { 0x3a, Position2, 0x01, 0x0C, 0x00, 0x00, 0x00, 0x00, 0x00, 0x3b };
            if (_tempDataField.Length == 1)
            {
                NumByte[5] = _tempDataField[0];    // data field
            }
            if (_tempDataField.Length == 2)
            {
                NumByte[5] = _tempDataField[1];    // data field
                NumByte[6] = _tempDataField[0];    // data field
            }
            if (_tempDataField.Length == 3)
            {
                NumByte[5] = _tempDataField[2];    // data field
                NumByte[6] = _tempDataField[1];    // data field
                NumByte[7] = _tempDataField[0];    // data field
            }
            if (_tempDataField.Length == 4)
            {
                NumByte[5] = _tempDataField[3];    // data field
                NumByte[6] = _tempDataField[2];    // data field
                NumByte[7] = _tempDataField[1];    // data field
                NumByte[8] = _tempDataField[0];    // data field
            }
            if (_Port != null)
            {
                _Port.Write(NumByte, 0, NumByte.Length);
            }
            _IsBusy = false;
        }

        public void SetCurrenCurrenttGain300CalVol(double Value)
        {
            _IsBusy = true;
            byte[] _tempDataField = new byte[4];
            int _tempNumber = (int)Math.Round((double)Value * 100, 0);
            _tempDataField = BitConverter.GetBytes(_tempNumber);
            _DataField = BitConverter.ToInt32(_tempDataField, 0);
            byte[] NumByte = new byte[] { 0x3a, Position2, 0x01, 0x0D, 0x00, 0x00, 0x00, 0x00, 0x00, 0x3b };
            if (_tempDataField.Length == 1)
            {
                NumByte[5] = _tempDataField[0];    // data field
            }
            if (_tempDataField.Length == 2)
            {
                NumByte[5] = _tempDataField[1];    // data field
                NumByte[6] = _tempDataField[0];    // data field
            }
            if (_tempDataField.Length == 3)
            {
                NumByte[5] = _tempDataField[2];    // data field
                NumByte[6] = _tempDataField[1];    // data field
                NumByte[7] = _tempDataField[0];    // data field
            }
            if (_tempDataField.Length == 4)
            {
                NumByte[5] = _tempDataField[3];    // data field
                NumByte[6] = _tempDataField[2];    // data field
                NumByte[7] = _tempDataField[1];    // data field
                NumByte[8] = _tempDataField[0];    // data field
            }
            if (_Port != null)
            {
                _Port.Write(NumByte, 0, NumByte.Length);
            }
            _IsBusy = false;
        }

        public void SetCurrenCurrenttGain400CalVol(double Value)
        {
            _IsBusy = true;
            byte[] _tempDataField = new byte[4];
            int _tempNumber = (int)Math.Round((double)Value * 100, 0);
            _tempDataField = BitConverter.GetBytes(_tempNumber);
            _DataField = BitConverter.ToInt32(_tempDataField, 0);
            byte[] NumByte = new byte[] { 0x3a, Position2, 0x01, 0x0E, 0x00, 0x00, 0x00, 0x00, 0x00, 0x3b };
            if (_tempDataField.Length == 1)
            {
                NumByte[5] = _tempDataField[0];    // data field
            }
            if (_tempDataField.Length == 2)
            {
                NumByte[5] = _tempDataField[1];    // data field
                NumByte[6] = _tempDataField[0];    // data field
            }
            if (_tempDataField.Length == 3)
            {
                NumByte[5] = _tempDataField[2];    // data field
                NumByte[6] = _tempDataField[1];    // data field
                NumByte[7] = _tempDataField[0];    // data field
            }
            if (_tempDataField.Length == 4)
            {
                NumByte[5] = _tempDataField[3];    // data field
                NumByte[6] = _tempDataField[2];    // data field
                NumByte[7] = _tempDataField[1];    // data field
                NumByte[8] = _tempDataField[0];    // data field
            }
            if (_Port != null)
            {
                _Port.Write(NumByte, 0, NumByte.Length);
            }
            _IsBusy = false;
        }

        public void SetCurrenCurrenttGain500CalVol(double Value)
        {
            _IsBusy = true;
            byte[] _tempDataField = new byte[4];
            int _tempNumber = (int)Math.Round((double)Value * 100, 0);
            _tempDataField = BitConverter.GetBytes(_tempNumber);
            _DataField = BitConverter.ToInt32(_tempDataField, 0);
            byte[] NumByte = new byte[] { 0x3a, Position2, 0x01, 0x0F, 0x00, 0x00, 0x00, 0x00, 0x00, 0x3b };
            if (_tempDataField.Length == 1)
            {
                NumByte[5] = _tempDataField[0];    // data field
            }
            if (_tempDataField.Length == 2)
            {
                NumByte[5] = _tempDataField[1];    // data field
                NumByte[6] = _tempDataField[0];    // data field
            }
            if (_tempDataField.Length == 3)
            {
                NumByte[5] = _tempDataField[2];    // data field
                NumByte[6] = _tempDataField[1];    // data field
                NumByte[7] = _tempDataField[0];    // data field
            }
            if (_tempDataField.Length == 4)
            {
                NumByte[5] = _tempDataField[3];    // data field
                NumByte[6] = _tempDataField[2];    // data field
                NumByte[7] = _tempDataField[1];    // data field
                NumByte[8] = _tempDataField[0];    // data field
            }
            if (_Port != null)
            {
                _Port.Write(NumByte, 0, NumByte.Length);
            }
            _IsBusy = false;
        }

        public void SetCurrenPMTCtrlVol(double Value)
        {
            _IsBusy = true;
            byte[] _tempDataField = new byte[4];
            int _tempNumber = (int)Math.Round((double)Value * 10, 0);
            _tempDataField = BitConverter.GetBytes(_tempNumber);
            _DataField = BitConverter.ToInt32(_tempDataField, 0);
            byte[] NumByte = new byte[] { 0x3a, Position2, 0x01, 0x10, 0x00, 0x00, 0x00, 0x00, 0x00, 0x3b };
            if (_tempDataField.Length == 1)
            {
                NumByte[5] = _tempDataField[0];    // data field
            }
            if (_tempDataField.Length == 2)
            {
                NumByte[5] = _tempDataField[1];    // data field
                NumByte[6] = _tempDataField[0];    // data field
            }
            if (_tempDataField.Length == 3)
            {
                NumByte[5] = _tempDataField[2];    // data field
                NumByte[6] = _tempDataField[1];    // data field
                NumByte[7] = _tempDataField[0];    // data field
            }
            if (_tempDataField.Length == 4)
            {
                NumByte[5] = _tempDataField[3];    // data field
                NumByte[6] = _tempDataField[2];    // data field
                NumByte[7] = _tempDataField[1];    // data field
                NumByte[8] = _tempDataField[0];    // data field
            }
            if (_Port != null)
            {
                _Port.Write(NumByte, 0, NumByte.Length);
            }
            _IsBusy = false;
        }

        public void SetCurrentPMTCompensationCoefficientValue(double Value)
        {
            _IsBusy = true;
            byte[] _tempDataField = new byte[4];
            //int _tempNumber = (int)Math.Round((double)Value * 10, 0);
            //int _tempNumber = (int)Math.Round((double)Value, 0);
            //double _tempNumber = Value * 10000;

            string hexString = string.Format("{0:X8}", (uint)Value); // 将整数转换为 8 位的十六进制字符串
            _tempDataField = Enumerable.Range(0, hexString.Length)
                     .Where(x => x % 2 == 0)
                     .Select(x => Convert.ToByte(hexString.Substring(x, 2), 16))
                     .ToArray();

            byte[] NumByte = new byte[] { 0x3a, Position2, 0x01, 0x11, 0x00, 0xFF, 0xFF, 0x00, 0x00, 0x3b };
            if (_tempDataField.Length == 1)
            {
                NumByte[5] = _tempDataField[0];    // data field
            }
            if (_tempDataField.Length == 2)
            {
                NumByte[5] = _tempDataField[1];    // data field
                NumByte[6] = _tempDataField[0];    // data field
            }
            if (_tempDataField.Length == 3)
            {
                NumByte[5] = _tempDataField[2];    // data field
                NumByte[6] = _tempDataField[1];    // data field
                NumByte[7] = _tempDataField[0];    // data field
            }
            if (_tempDataField.Length == 4)
            {
                NumByte[5] = _tempDataField[0];    // data field
                NumByte[6] = _tempDataField[1];    // data field
                NumByte[7] = _tempDataField[2];    // data field
                NumByte[8] = _tempDataField[3];    // data field
            }
            if (_Port != null)
            {
                //3a 22 01 11 00 ff ff 0b b8 3b
                //3a 22 02 11 00 00 00 00 00 3b
                _Port.Write(NumByte, 0, NumByte.Length);
            }
            _IsBusy = false;
        }

        public void SetCurrentADPTemperatureCalibrationFactorValue(double Value)
        {
            _IsBusy = true;
            byte[] _tempDataField = new byte[4];
            int _tempNumber = (int)Math.Round((double)Value * 100, 0);
            _tempDataField = BitConverter.GetBytes(_tempNumber);
            _DataField = BitConverter.ToInt32(_tempDataField, 0);
            byte[] NumByte = new byte[] { 0x3a, Position2, 0x01, 0x17, 0x00, 0x00, 0x00, 0x00, 0x00, 0x3b };
            if (_tempDataField.Length == 1)
            {
                NumByte[5] = _tempDataField[0];    // data field
            }
            if (_tempDataField.Length == 2)
            {
                NumByte[5] = _tempDataField[1];    // data field
                NumByte[6] = _tempDataField[0];    // data field
            }
            if (_tempDataField.Length == 3)
            {
                NumByte[5] = _tempDataField[2];    // data field
                NumByte[6] = _tempDataField[1];    // data field
                NumByte[7] = _tempDataField[0];    // data field
            }
            if (_tempDataField.Length == 4)
            {
                NumByte[5] = _tempDataField[3];    // data field
                NumByte[6] = _tempDataField[2];    // data field
                NumByte[7] = _tempDataField[1];    // data field
                NumByte[8] = _tempDataField[0];    // data field
            }
            if (_Port != null)
            {
                _Port.Write(NumByte, 0, NumByte.Length);
            }
            _IsBusy = false;
        }

        public void SetIVOpticalModuleNumberValue(string number)
        {
            _IsBusy = true;
            byte[] _tempDataField = new byte[4];
            _tempDataField = HexStringToByteArray(number);
            byte[] NumByte = new byte[] { 0x3a, Position2, 0x01, 0x19, 0x00, 0x00, 0x00, 0x00, 0x00, 0x3b };
            if (_tempDataField.Length == 1)
            {
                NumByte[5] = _tempDataField[0];    // data field
            }
            else if (_tempDataField.Length == 2)
            {
                NumByte[5] = _tempDataField[0];    // data field
                NumByte[6] = _tempDataField[1];    // data field
            }
            else if (_tempDataField.Length == 3)
            {
                NumByte[5] = _tempDataField[0];    // data field
                NumByte[6] = _tempDataField[1];    // data field
                string temp = _tempDataField[2].ToString("X6");
                byte b1 = Convert.ToByte(temp);
                NumByte[7] = b1;    
            }
            else if (_tempDataField.Length == 4)
            {
                string temp = _tempDataField[2].ToString("X6");
                string temp1 = _tempDataField[3].ToString("X6");
                byte b1 = Convert.ToByte(temp);
                byte b2 = Convert.ToByte(temp1);
                NumByte[5] = _tempDataField[0];    // data field
                NumByte[6] = _tempDataField[1];    // data field
                NumByte[7] = b1;    // data field
                NumByte[8] = b2;    // data field
            }
            else if (_tempDataField.Length == 5)
            {
                string fore = number.Substring(0, 6);
                _tempDataField = HexStringToByteArray(fore);
                string back = Convert.ToInt16(number.Substring(6,3)).ToString("X6").Substring(4,2);
                byte[] aa = new byte[3];
                aa = HexStringToByteArray(back);
                string temp = _tempDataField[2].ToString("X6");
                //string temp1 = _tempDataField[3].ToString("X6");
                byte b1 = Convert.ToByte(temp);
               // byte b2 = Convert.ToByte(temp1);
                NumByte[5] = _tempDataField[0];    // data field
                NumByte[6] = _tempDataField[1];    // data field
                NumByte[7] = b1;    // data field
                NumByte[8] = aa[0];    // data field
            }
            if (_Port != null)
            {
                _Port.Write(NumByte, 0, NumByte.Length);
            }
            _IsBusy = false;
        }

        public void GetCurrentSensorType()
        {
            _IsBusy = true;
            byte[] NumByte = new byte[] { 0x3a, Position2, 0x02, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x3b };
            if (_Port != null)
            {
                _Port.Write(NumByte, 0, NumByte.Length);
            }
            _IsBusy = false;
        }

        public void GetCurrentSensorNoValue()
        {
            _IsBusy = true;
            byte[] NumByte = new byte[] { 0x3a, Position2, 0x02, 0x02, 0x00, 0x00, 0x00, 0x00, 0x00, 0x3b };
            if (_Port != null)
            {
                _Port.Write(NumByte, 0, NumByte.Length);
            }
            _IsBusy = false;
        }

        public void GetCurrentPGAMultiple()
        {
            _IsBusy = true;
            byte[] NumByte = new byte[] { 0x3a, Position2, 0x02, 0x3, 0x00, 0x00, 0x00, 0x00, 0x00, 0x3b };
            if (_Port != null)
            {
                _Port.Write(NumByte, 0, NumByte.Length);
            }
            _IsBusy = false;
        }

        public void GetCurrentAPDGain()
        {
            _IsBusy = true;
            byte[] NumByte = new byte[] { 0x3a, Position2, 0x02, 0x4, 0x00, 0x00, 0x00, 0x00, 0x00, 0x3b };
            if (_Port != null)
            {
                _Port.Write(NumByte, 0, NumByte.Length);
            }
            _IsBusy = false;
        }

        public void GetCurrentAPDTempValue()
        {
            _IsBusy = true;
            byte[] NumByte = new byte[] { 0x3a, Position2, 0x02, 0x5, 0x00, 0x00, 0x00, 0x00, 0x00, 0x3b };
            if (_Port != null)
            {
                _Port.Write(NumByte, 0, NumByte.Length);
            }
            _IsBusy = false;
        }

        public void GetCurrentAPDHighVoltageValue()
        {
            _IsBusy = true;
            byte[] NumByte = new byte[] { 0x3a, Position2, 0x02, 0x6, 0x00, 0x00, 0x00, 0x00, 0x00, 0x3b };
            if (_Port != null)
            {
                _Port.Write(NumByte, 0, NumByte.Length);
            }
            _IsBusy = false;
        }

        public void GetCurrentLightIntensityCalibrationTemperatureValue()
        {
            _IsBusy = true;
            byte[] NumByte = new byte[] { 0x3a, Position2, 0x02, 0x7, 0x00, 0x00, 0x00, 0x00, 0x00, 0x3b };
            if (_Port != null)
            {
                _Port.Write(NumByte, 0, NumByte.Length);
            }
            _IsBusy = false;
        }

        public void GetCurrentGain50CalVolValue()
        {
            _IsBusy = true;
            byte[] NumByte = new byte[] { 0x3a, Position2, 0x02, 0x08, 0x00, 0x00, 0x00, 0x00, 0x00, 0x3b };
            if (_Port != null)
            {
                _Port.Write(NumByte, 0, NumByte.Length);
            }
            _IsBusy = false;
        }

        public void GetCurrentGain100CalVolValue()
        {
            _IsBusy = true;
            byte[] NumByte = new byte[] { 0x3a, Position2, 0x02, 0x09, 0x00, 0x00, 0x00, 0x00, 0x00, 0x3b };
            if (_Port != null)
            {
                _Port.Write(NumByte, 0, NumByte.Length);
            }
            _IsBusy = false;
        }

        public void GetCurrentGain150CalVolValue()
        {
            _IsBusy = true;
            byte[] NumByte = new byte[] { 0x3a, Position2, 0x02, 0x0A, 0x00, 0x00, 0x00, 0x00, 0x00, 0x3b };
            if (_Port != null)
            {
                _Port.Write(NumByte, 0, NumByte.Length);
            }
            _IsBusy = false;
        }

        public void GetCurrentGain200CalVolValue()
        {
            _IsBusy = true;
            byte[] NumByte = new byte[] { 0x3a, Position2, 0x02, 0x0B, 0x00, 0x00, 0x00, 0x00, 0x00, 0x3b };
            if (_Port != null)
            {
                _Port.Write(NumByte, 0, NumByte.Length);
            }
            _IsBusy = false;
        }

        public void GetCurrentGain250CalVolValue()
        {
            _IsBusy = true;
            byte[] NumByte = new byte[] { 0x3a, Position2, 0x02, 0x0C, 0x00, 0x00, 0x00, 0x00, 0x00, 0x3b };
            if (_Port != null)
            {
                _Port.Write(NumByte, 0, NumByte.Length);
            }
            _IsBusy = false;
        }

        public void GetCurrentGain300CalVolValue()
        {
            _IsBusy = true;
            byte[] NumByte = new byte[] { 0x3a, Position2, 0x02, 0x0D, 0x00, 0x00, 0x00, 0x00, 0x00, 0x3b };
            if (_Port != null)
            {
                _Port.Write(NumByte, 0, NumByte.Length);
            }
            _IsBusy = false;
        }

        public void GetCurrentGain400CalVolValue()
        {
            _IsBusy = true;
            byte[] NumByte = new byte[] { 0x3a, Position2, 0x02, 0x0E, 0x00, 0x00, 0x00, 0x00, 0x00, 0x3b };
            if (_Port != null)
            {
                _Port.Write(NumByte, 0, NumByte.Length);
            }
            _IsBusy = false;
        }

        public void GetCurrentGain500CalVolValue()
        {
            _IsBusy = true;
            byte[] NumByte = new byte[] { 0x3a, Position2, 0x02, 0x0F, 0x00, 0x00, 0x00, 0x00, 0x00, 0x3b };
            if (_Port != null)
            {
                _Port.Write(NumByte, 0, NumByte.Length);
            }
            _IsBusy = false;
        }

        public void GetCurrentPMTCtrlVolValue()
        {
            _IsBusy = true;
            byte[] NumByte = new byte[] { 0x3a, Position2, 0x02, 0x10, 0x00, 0x00, 0x00, 0x00, 0x00, 0x3b };
            if (_Port != null)
            {
                _Port.Write(NumByte, 0, NumByte.Length);
            }
            _IsBusy = false;
        }

        public void GetCurrentPMTCompensationCoefficientValue()
        {
            _IsBusy = true;
            byte[] NumByte = new byte[] { 0x3a, Position2, 0x02, 0x11, 0x00, 0x00, 0x00, 0x00, 0x00, 0x3b };
            if (_Port != null)
            {
                _Port.Write(NumByte, 0, NumByte.Length);
            }
            _IsBusy = false;
        }

        public void GetCurrentTempSenser1282ADValue()
        {
            _IsBusy = true;
            byte[] NumByte = new byte[] { 0x3a, Position2, 0x02, 0x12, 0x00, 0x00, 0x00, 0x00, 0x00, 0x3b };
            if (_Port != null)
            {
                _Port.Write(NumByte, 0, NumByte.Length);
            }
            _IsBusy = false;
        }

        public void GetCurrentTempSenser6459ADValue()
        {
            _IsBusy = true;
            byte[] NumByte = new byte[] { 0x3a, Position2, 0x02, 0x13, 0x00, 0x00, 0x00, 0x00, 0x00, 0x3b };
            if (_Port != null)
            {
                _Port.Write(NumByte, 0, NumByte.Length);
            }
            _IsBusy = false;
        }

        public void GetCurrentTempSenserADValue()
        {
            _IsBusy = true;
            byte[] NumByte = new byte[] { 0x3a, Position2, 0x02, 0x14, 0x00, 0x00, 0x00, 0x00, 0x00, 0x3b };
            if (_Port != null)
            {
                _Port.Write(NumByte, 0, NumByte.Length);
            }
            _IsBusy = false;
        }

        public void GetCurrentIVBoardRunningStateValue()
        {
            _IsBusy = true;
            byte[] NumByte = new byte[] { 0x3a, Position2, 0x02, 0x15, 0x00, 0x00, 0x00, 0x00, 0x00, 0x3b };
            if (_Port != null)
            {
                _Port.Write(NumByte, 0, NumByte.Length);
            }
            _IsBusy = false;
        }

        public void GetCurrentTemperatureSensorSamplingVoltageValue()
        {
            _IsBusy = true;
            byte[] NumByte = new byte[] { 0x3a, Position2, 0x02, 0x16, 0x00, 0x00, 0x00, 0x00, 0x00, 0x3b };
            if (_Port != null)
            {
                _Port.Write(NumByte, 0, NumByte.Length);
            }
            _IsBusy = false;
        }

        public void GetCurrentADPTemperatureCalibrationFactorValue()
        {
            _IsBusy = true;
            byte[] NumByte = new byte[] { 0x3a, Position2, 0x02, 0x17, 0x00, 0x00, 0x00, 0x00, 0x00, 0x3b };
            if (_Port != null)
            {
                _Port.Write(NumByte, 0, NumByte.Length);
            }
            _IsBusy = false;
        }

        public void GetIVvNumberValue()
        {
            _IsBusy = true;
            byte[] NumByte = new byte[] { 0x3a, Position2, 0x02, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x3b };
            if (_Port != null)
            {
                _Port.Write(NumByte, 0, NumByte.Length);
            }
            _IsBusy = false;
        }
        public void GetIVErrorCodeValue()
        {
            _IsBusy = true;
            byte[] NumByte = new byte[] { 0x3a, Position2, 0x02, 0x18, 0x00, 0x00, 0x00, 0x00, 0x00, 0x3b };
            if (_Port != null)
            {
                _Port.Write(NumByte, 0, NumByte.Length);
            }
            _IsBusy = false;
        }
        public void GetIVOpticalModuleNumberValue()
        {
            _IsBusy = true;
            byte[] NumByte = new byte[] { 0x3a, Position2, 0x02, 0x19, 0x00, 0x00, 0x00, 0x00, 0x00, 0x3b };
            if (_Port != null)
            {
                _Port.Write(NumByte, 0, NumByte.Length);
            }
            _IsBusy = false;
        }
        #endregion

        #region  激光板子
        public void SetLaserOpticalModuleNumberValue(string number)
        {
            _IsBusy = true;
            byte[] _tempDataField = new byte[4];
            _tempDataField = HexStringToByteArray(number);
            byte[] NumByte = new byte[] { 0x3a, Position1, 0x01, 0x28, 0x00, 0x00, 0x00, 0x00, 0x00, 0x3b };
            if (_tempDataField.Length == 1)
            {
                NumByte[5] = _tempDataField[0];    // data field
            }
            else if (_tempDataField.Length == 2)
            {
                NumByte[5] = _tempDataField[0];    // data field
                NumByte[6] = _tempDataField[1];    // data field
            }
            else if (_tempDataField.Length == 3)
            {
                NumByte[5] = _tempDataField[0];    // data field
                NumByte[6] = _tempDataField[1];    // data field
                string temp = _tempDataField[2].ToString("X6");
                byte b1 = Convert.ToByte(temp);
                NumByte[7] = b1;
            }
            else if (_tempDataField.Length == 4)
            {
                string temp = _tempDataField[2].ToString("X6");
                string temp1 = _tempDataField[3].ToString("X6");
                byte b1 = Convert.ToByte(temp);
                byte b2 = Convert.ToByte(temp1);
                NumByte[5] = _tempDataField[0];    // data field
                NumByte[6] = _tempDataField[1];    // data field
                NumByte[7] = b1;    // data field
                NumByte[8] = b2;    // data field
            }
            else if (_tempDataField.Length == 5)
            {
                string fore = number.Substring(0, 6);
                _tempDataField = HexStringToByteArray(fore);
                string back = Convert.ToInt16(number.Substring(6, 3)).ToString("X6").Substring(4, 2);
                byte[] aa = new byte[3];
                aa = HexStringToByteArray(back);
                string temp = _tempDataField[2].ToString("X6");
                //string temp1 = _tempDataField[3].ToString("X6");
                byte b1 = Convert.ToByte(temp);
                // byte b2 = Convert.ToByte(temp1);
                NumByte[5] = _tempDataField[0];    // data field
                NumByte[6] = _tempDataField[1];    // data field
                NumByte[7] = b1;    // data field
                NumByte[8] = aa[0];    // data field
            }

            if (_Port != null)
            {
                _Port.Write(NumByte, 0, NumByte.Length);
            }
            _IsBusy = false;
        }
        public void SetCurrentLaserNoValue(string number)
        {
            _IsBusy = true;
            byte[] _tempDataField = new byte[4];
            _tempDataField = HexStringToByteArray(number);
            byte[] NumByte = new byte[] { 0x3a, Position1, 0x01, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x3b };
            if (_tempDataField.Length == 1)
            {
                NumByte[5] = _tempDataField[0];    // data field
            }
            if (_tempDataField.Length == 2)
            {
                NumByte[5] = _tempDataField[0];    // data field
                NumByte[6] = _tempDataField[1];    // data field
            }
            if (_tempDataField.Length == 3)
            {
                NumByte[5] = _tempDataField[0];    // data field
                NumByte[6] = _tempDataField[1];    // data field
                NumByte[7] = _tempDataField[2];    // data field
            }
            if (_tempDataField.Length == 4)
            {
                NumByte[5] = _tempDataField[0];    // data field
                NumByte[6] = _tempDataField[1];    // data field
                NumByte[7] = _tempDataField[2];    // data field
                NumByte[8] = _tempDataField[3];    // data field
            }
            if (_Port != null)
            {
                _Port.Write(NumByte, 0, NumByte.Length);
            }
            _IsBusy = false;
        }

        public void SetCurrentLaserWavaLengthValue(int number)
        {
            _IsBusy = true;
            byte[] _tempDataField = new byte[4];
            _tempDataField = BitConverter.GetBytes(number);
            _DataField = BitConverter.ToInt32(_tempDataField, 0);
            byte[] NumByte = new byte[] { 0x3a, Position1, 0x01, 0x02, 0x00, 0x00, 0x00, 0x00, 0x00, 0x3b };
            if (_tempDataField.Length == 1)
            {
                NumByte[5] = _tempDataField[3];    // data field
            }
            if (_tempDataField.Length == 2)
            {
                NumByte[5] = _tempDataField[3];    // data field
                NumByte[6] = _tempDataField[2];    // data field
            }
            if (_tempDataField.Length == 3)
            {
                NumByte[5] = _tempDataField[3];    // data field
                NumByte[6] = _tempDataField[2];    // data field
                NumByte[7] = _tempDataField[1];    // data field
            }
            if (_tempDataField.Length == 4)
            {
                NumByte[5] = _tempDataField[3];    // data field
                NumByte[6] = _tempDataField[2];    // data field
                NumByte[7] = _tempDataField[1];    // data field
                NumByte[8] = _tempDataField[0];    // data field
            }
            if (_Port != null)
            {
                _Port.Write(NumByte, 0, NumByte.Length);
            }
            _IsBusy = false;
        }

        public void SetCurrentLaserCurrentValueValue(double Value)
        {
            _IsBusy = true;
            byte[] _tempDataField = new byte[4];
            int _tempNumber = (int)Math.Round((double)Value * 100, 0);
            _tempDataField = BitConverter.GetBytes(_tempNumber);
            _DataField = BitConverter.ToInt32(_tempDataField, 0);
            byte[] NumByte = new byte[] { 0x3a, Position1, 0x01, 0x03, 0x00, 0x00, 0x00, 0x00, 0x00, 0x3b };
            if (_tempDataField.Length == 1)
            {
                NumByte[5] = _tempDataField[0];    // data field
            }
            if (_tempDataField.Length == 2)
            {
                NumByte[5] = _tempDataField[1];    // data field
                NumByte[6] = _tempDataField[0];    // data field
            }
            if (_tempDataField.Length == 3)
            {
                NumByte[5] = _tempDataField[2];    // data field
                NumByte[6] = _tempDataField[1];    // data field
                NumByte[7] = _tempDataField[0];    // data field
            }
            if (_tempDataField.Length == 4)
            {
                NumByte[5] = _tempDataField[3];    // data field
                NumByte[6] = _tempDataField[2];    // data field
                NumByte[7] = _tempDataField[1];    // data field
                NumByte[8] = _tempDataField[0];    // data field
            }
            if (_Port != null)
            {
                _Port.Write(NumByte, 0, NumByte.Length);
            }
            _IsBusy = false;
        }

        public void SetCurrentLaserCorrespondingCurrentValue(int CurrentMv, double power)
        {
            _IsBusy = true;
            byte flag = 0x05;
            if (CurrentMv == 5)
            {
                flag = 0x05;
            }
            else if (CurrentMv == 10)
            {
                flag = 0x06;
            }
            else if (CurrentMv == 15)
            {
                flag = 0x07;
            }
            else if (CurrentMv == 20)
            {
                flag = 0x08;
            }
            else if (CurrentMv == 25)
            {
                flag = 0x09;
            }
            else if (CurrentMv == 30)
            {
                flag = 0x0A;
            }
            else if (CurrentMv == 35)
            {
                flag = 0x24;
            }
            else if (CurrentMv == 40)
            {
                flag = 0x25;
            }
            else if (CurrentMv == 45)
            {
                flag = 0x26;
            }
            else if (CurrentMv == 50)
            {
                flag = 0x27;
            }
            byte[] _tempDataField = new byte[4];
            int _tempNumber = (int)Math.Round((double)power * 100, 0);
            _tempDataField = BitConverter.GetBytes(_tempNumber);
            byte[] NumByte = new byte[] { 0x3a, Position1, 0x01, flag, 0x00, 0x00, 0x00, 0x00, 0x00, 0x3b };
            if (_tempDataField.Length == 1)
            {
                NumByte[5] = _tempDataField[0];    // data field
            }
            if (_tempDataField.Length == 2)
            {
                NumByte[5] = _tempDataField[1];    // data field
                NumByte[6] = _tempDataField[0];    // data field
            }
            if (_tempDataField.Length == 3)
            {
                NumByte[5] = _tempDataField[2];    // data field
                NumByte[6] = _tempDataField[1];    // data field
                NumByte[7] = _tempDataField[0];    // data field
            }
            if (_tempDataField.Length == 4)
            {
                NumByte[5] = _tempDataField[3];    // data field
                NumByte[6] = _tempDataField[2];    // data field
                NumByte[7] = _tempDataField[1];    // data field
                NumByte[8] = _tempDataField[0];    // data field
            }
            if (_Port != null)
            {
                _Port.Write(NumByte, 0, NumByte.Length);
            }
            _IsBusy = false;
        }

        public void SetCurrentTEControlTemperatureValue(double Temp)
        {
            _IsBusy = true;
            byte[] _tempDataField = new byte[4];
            int _tempNumber = (int)Math.Round((double)Temp * 10, 0);
            _tempDataField = BitConverter.GetBytes(_tempNumber);
            byte[] NumByte = new byte[] { 0x3a, Position1, 0x01, 0x0C, 0x00, 0x00, 0x00, 0x00, 0x00, 0x3b };
            if (_tempDataField.Length == 1)
            {
                NumByte[5] = _tempDataField[0];    // data field
            }
            if (_tempDataField.Length == 2)
            {
                NumByte[5] = _tempDataField[1];    // data field
                NumByte[6] = _tempDataField[0];    // data field
            }
            if (_tempDataField.Length == 3)
            {
                NumByte[5] = _tempDataField[2];    // data field
                NumByte[6] = _tempDataField[1];    // data field
                NumByte[7] = _tempDataField[0];    // data field
            }
            if (_tempDataField.Length == 4)
            {
                NumByte[5] = _tempDataField[3];    // data field
                NumByte[6] = _tempDataField[2];    // data field
                NumByte[7] = _tempDataField[1];    // data field
                NumByte[8] = _tempDataField[0];    // data field
            }
            if (_Port != null)
            {
                _Port.Write(NumByte, 0, NumByte.Length);
            }
            _IsBusy = false;
        }

        public void SetCurrentTECMaximumCoolingCurrentValue(double Temp)
        {
            _IsBusy = true;
            byte[] _tempDataField = new byte[4];
            int _tempNumber = (int)Math.Round((double)Temp, 0);
            _tempDataField = BitConverter.GetBytes(_tempNumber);
            byte[] NumByte = new byte[] { 0x3a, Position1, 0x01, 0x0D, 0x00, 0x00, 0x00, 0x00, 0x00, 0x3b };
            if (_tempDataField.Length == 1)
            {
                NumByte[5] = _tempDataField[0];    // data field
            }
            if (_tempDataField.Length == 2)
            {
                NumByte[5] = _tempDataField[1];    // data field
                NumByte[6] = _tempDataField[0];    // data field
            }
            if (_tempDataField.Length == 3)
            {
                NumByte[5] = _tempDataField[2];    // data field
                NumByte[6] = _tempDataField[1];    // data field
                NumByte[7] = _tempDataField[0];    // data field
            }
            if (_tempDataField.Length == 4)
            {
                NumByte[5] = _tempDataField[3];    // data field
                NumByte[6] = _tempDataField[2];    // data field
                NumByte[7] = _tempDataField[1];    // data field
                NumByte[8] = _tempDataField[0];    // data field
            }
            if (_Port != null)
            {
                _Port.Write(NumByte, 0, NumByte.Length);
            }
            _IsBusy = false;
        }

        public void SetCurrentTECRefrigerationControlParameterKpValue(double kp)
        {
            _IsBusy = true;
            byte[] _tempDataField = new byte[4];
            int _tempNumber = (int)Math.Round((double)kp, 0);
            _tempDataField = BitConverter.GetBytes(_tempNumber);
            byte[] NumByte = new byte[] { 0x3a, Position1, 0x01, 0x0E, 0x00, 0x00, 0x00, 0x00, 0x00, 0x3b };
            if (_tempDataField.Length == 1)
            {
                NumByte[5] = _tempDataField[0];    // data field
            }
            if (_tempDataField.Length == 2)
            {
                NumByte[5] = _tempDataField[1];    // data field
                NumByte[6] = _tempDataField[0];    // data field
            }
            if (_tempDataField.Length == 3)
            {
                NumByte[5] = _tempDataField[2];    // data field
                NumByte[6] = _tempDataField[1];    // data field
                NumByte[7] = _tempDataField[0];    // data field
            }
            if (_tempDataField.Length == 4)
            {
                NumByte[5] = _tempDataField[3];    // data field
                NumByte[6] = _tempDataField[2];    // data field
                NumByte[7] = _tempDataField[1];    // data field
                NumByte[8] = _tempDataField[0];    // data field
            }
            if (_Port != null)
            {
                _Port.Write(NumByte, 0, NumByte.Length);
            }
            _IsBusy = false;
        }

        public void SetCurrentTECRefrigerationControlParameterKiValue(double ki)
        {
            _IsBusy = true;
            byte[] _tempDataField = new byte[4];
            int _tempNumber = (int)Math.Round((double)ki, 0);
            _tempDataField = BitConverter.GetBytes(_tempNumber);
            byte[] NumByte = new byte[] { 0x3a, Position1, 0x01, 0x0F, 0x00, 0x00, 0x00, 0x00, 0x00, 0x3b };
            if (_tempDataField.Length == 1)
            {
                NumByte[5] = _tempDataField[0];    // data field
            }
            if (_tempDataField.Length == 2)
            {
                NumByte[5] = _tempDataField[1];    // data field
                NumByte[6] = _tempDataField[0];    // data field
            }
            if (_tempDataField.Length == 3)
            {
                NumByte[5] = _tempDataField[2];    // data field
                NumByte[6] = _tempDataField[1];    // data field
                NumByte[7] = _tempDataField[0];    // data field
            }
            if (_tempDataField.Length == 4)
            {
                NumByte[5] = _tempDataField[3];    // data field
                NumByte[6] = _tempDataField[2];    // data field
                NumByte[7] = _tempDataField[1];    // data field
                NumByte[8] = _tempDataField[0];    // data field
            }
            if (_Port != null)
            {
                _Port.Write(NumByte, 0, NumByte.Length);
            }
            _IsBusy = false;
        }

        public void SetCurrentTECRefrigerationControlParameterKdValue(double kd)
        {
            _IsBusy = true;
            byte[] _tempDataField = new byte[4];
            int _tempNumber = (int)Math.Round((double)kd, 0);
            _tempDataField = BitConverter.GetBytes(_tempNumber);
            byte[] NumByte = new byte[] { 0x3a, Position1, 0x01, 0x10, 0x00, 0x00, 0x00, 0x00, 0x00, 0x3b };
            if (_tempDataField.Length == 1)
            {
                NumByte[5] = _tempDataField[0];    // data field
            }
            if (_tempDataField.Length == 2)
            {
                NumByte[5] = _tempDataField[1];    // data field
                NumByte[6] = _tempDataField[0];    // data field
            }
            if (_tempDataField.Length == 3)
            {
                NumByte[5] = _tempDataField[2];    // data field
                NumByte[6] = _tempDataField[1];    // data field
                NumByte[7] = _tempDataField[0];    // data field
            }
            if (_tempDataField.Length == 4)
            {
                NumByte[5] = _tempDataField[3];    // data field
                NumByte[6] = _tempDataField[2];    // data field
                NumByte[7] = _tempDataField[1];    // data field
                NumByte[8] = _tempDataField[0];    // data field
            }
            if (_Port != null)
            {
                _Port.Write(NumByte, 0, NumByte.Length);
            }
            _IsBusy = false;
        }

        public void SetCurrentLaserLightPowerValueValue(double Value)
        {
            _IsBusy = true;
            byte[] _tempDataField = new byte[4];
            int _tempNumber = (int)Math.Round((double)Value * 10, 0);
            _tempDataField = BitConverter.GetBytes(_tempNumber);
            _DataField = BitConverter.ToInt32(_tempDataField, 0);
            byte[] NumByte = new byte[] { 0x3a, Position1, 0x01, 0x11, 0x00, 0x00, 0x00, 0x00, 0x00, 0x3b };
            if (_tempDataField.Length == 1)
            {
                NumByte[5] = _tempDataField[0];    // data field
            }
            if (_tempDataField.Length == 2)
            {
                NumByte[5] = _tempDataField[1];    // data field
                NumByte[6] = _tempDataField[0];    // data field
            }
            if (_tempDataField.Length == 3)
            {
                NumByte[5] = _tempDataField[2];    // data field
                NumByte[6] = _tempDataField[1];    // data field
                NumByte[7] = _tempDataField[0];    // data field
            }
            if (_tempDataField.Length == 4)
            {
                NumByte[5] = _tempDataField[3];    // data field
                NumByte[6] = _tempDataField[2];    // data field
                NumByte[7] = _tempDataField[1];    // data field
                NumByte[8] = _tempDataField[0];    // data field
            }
            if (_Port != null)
            {
                _Port.Write(NumByte, 0, NumByte.Length);
            }
            _IsBusy = false;
        }

        public void SetCurrentTECCurrentCompensationCoefficientValue(double Value)
        {
            _IsBusy = true;
            byte[] _tempDataField = new byte[4];
            int _tempNumber = (int)Math.Round((double)Value, 0);
            _tempDataField = BitConverter.GetBytes(_tempNumber);
            _DataField = BitConverter.ToInt32(_tempDataField, 0);
            byte[] NumByte = new byte[] { 0x3a, Position1, 0x01, 0x15, 0x00, 0x00, 0x00, 0x00, 0x00, 0x3b };
            if (_tempDataField.Length == 1)
            {
                NumByte[5] = _tempDataField[0];    // data field
            }
            if (_tempDataField.Length == 2)
            {
                NumByte[5] = _tempDataField[1];    // data field
                NumByte[6] = _tempDataField[0];    // data field
            }
            if (_tempDataField.Length == 3)
            {
                NumByte[5] = _tempDataField[2];    // data field
                NumByte[6] = _tempDataField[1];    // data field
                NumByte[7] = _tempDataField[0];    // data field
            }
            if (_tempDataField.Length == 4)
            {
                NumByte[5] = _tempDataField[3];    // data field
                NumByte[6] = _tempDataField[2];    // data field
                NumByte[7] = _tempDataField[1];    // data field
                NumByte[8] = _tempDataField[0];    // data field
            }
            if (_Port != null)
            {
                _Port.Write(NumByte, 0, NumByte.Length);
            }
            _IsBusy = false;
        }


        public void SetCurrentPowerClosedloopControlParameterKpValue(double kp)
        {
            _IsBusy = true;
            byte[] _tempDataField = new byte[4];
            int _tempNumber = (int)Math.Round((double)kp, 0);
            _tempDataField = BitConverter.GetBytes(_tempNumber);
            byte[] NumByte = new byte[] { 0x3a, Position1, 0x01, 0x17, 0x00, 0x00, 0x00, 0x00, 0x00, 0x3b };
            if (_tempDataField.Length == 1)
            {
                NumByte[5] = _tempDataField[0];    // data field
            }
            if (_tempDataField.Length == 2)
            {
                NumByte[5] = _tempDataField[1];    // data field
                NumByte[6] = _tempDataField[0];    // data field
            }
            if (_tempDataField.Length == 3)
            {
                NumByte[5] = _tempDataField[2];    // data field
                NumByte[6] = _tempDataField[1];    // data field
                NumByte[7] = _tempDataField[0];    // data field
            }
            if (_tempDataField.Length == 4)
            {
                NumByte[5] = _tempDataField[3];    // data field
                NumByte[6] = _tempDataField[2];    // data field
                NumByte[7] = _tempDataField[1];    // data field
                NumByte[8] = _tempDataField[0];    // data field
            }
            if (_Port != null)
            {
                _Port.Write(NumByte, 0, NumByte.Length);
            }
            _IsBusy = false;
        }

        public void SetCurrentPowerClosedloopControlParameterKiValue(double ki)
        {
            _IsBusy = true;
            byte[] _tempDataField = new byte[4];
            int _tempNumber = (int)Math.Round((double)ki, 0);
            _tempDataField = BitConverter.GetBytes(_tempNumber);
            byte[] NumByte = new byte[] { 0x3a, Position1, 0x01, 0x18, 0x00, 0x00, 0x00, 0x00, 0x00, 0x3b };
            if (_tempDataField.Length == 1)
            {
                NumByte[5] = _tempDataField[0];    // data field
            }
            if (_tempDataField.Length == 2)
            {
                NumByte[5] = _tempDataField[1];    // data field
                NumByte[6] = _tempDataField[0];    // data field
            }
            if (_tempDataField.Length == 3)
            {
                NumByte[5] = _tempDataField[2];    // data field
                NumByte[6] = _tempDataField[1];    // data field
                NumByte[7] = _tempDataField[0];    // data field
            }
            if (_tempDataField.Length == 4)
            {
                NumByte[5] = _tempDataField[3];    // data field
                NumByte[6] = _tempDataField[2];    // data field
                NumByte[7] = _tempDataField[1];    // data field
                NumByte[8] = _tempDataField[0];    // data field
            }
            if (_Port != null)
            {
                _Port.Write(NumByte, 0, NumByte.Length);
            }
            _IsBusy = false;
        }

        public void SetCurrentPowerClosedloopControlParameterKdValue(double ki)
        {
            _IsBusy = true;
            byte[] _tempDataField = new byte[4];
            int _tempNumber = (int)Math.Round((double)ki, 0);
            _tempDataField = BitConverter.GetBytes(_tempNumber);
            byte[] NumByte = new byte[] { 0x3a, Position1, 0x01, 0x19, 0x00, 0x00, 0x00, 0x00, 0x00, 0x3b };
            if (_tempDataField.Length == 1)
            {
                NumByte[5] = _tempDataField[0];    // data field
            }
            if (_tempDataField.Length == 2)
            {
                NumByte[5] = _tempDataField[1];    // data field
                NumByte[6] = _tempDataField[0];    // data field
            }
            if (_tempDataField.Length == 3)
            {
                NumByte[5] = _tempDataField[2];    // data field
                NumByte[6] = _tempDataField[1];    // data field
                NumByte[7] = _tempDataField[0];    // data field
            }
            if (_tempDataField.Length == 4)
            {
                NumByte[5] = _tempDataField[3];    // data field
                NumByte[6] = _tempDataField[2];    // data field
                NumByte[7] = _tempDataField[1];    // data field
                NumByte[8] = _tempDataField[0];    // data field
            }
            if (_Port != null)
            {
                _Port.Write(NumByte, 0, NumByte.Length);
            }
            _IsBusy = false;
        }

        public void SetCurrentPhotodiodeVoltageCorrespondingToLaserPowerValue(int CurrentMv, double power)
        {
            _IsBusy = true;
            byte flag = 0x05;
            if (CurrentMv == 5)
            {
                flag = 0x1A;
            }
            else if (CurrentMv == 10)
            {
                flag = 0x1B;
            }
            else if (CurrentMv == 15)
            {
                flag = 0x1C;
            }
            else if (CurrentMv == 20)
            {
                flag = 0x1D;
            }
            else if (CurrentMv == 25)
            {
                flag = 0x1E;
            }
            else if (CurrentMv == 30)
            {
                flag = 0x1F;
            }
            else if (CurrentMv == 35)
            {
                flag = 0x29;
            }
            else
            {
                return;
            }
            byte[] _tempDataField = new byte[4];
            int _tempNumber = (int)Math.Round((double)power * 10000, 0);
            _tempDataField = BitConverter.GetBytes(_tempNumber);
            byte[] NumByte = new byte[] { 0x3a, Position1, 0x01, flag, 0x00, 0x00, 0x00, 0x00, 0x00, 0x3b };
            if (_tempDataField.Length == 1)
            {
                NumByte[5] = _tempDataField[0];    // data field
            }
            if (_tempDataField.Length == 2)
            {
                NumByte[5] = _tempDataField[1];    // data field
                NumByte[6] = _tempDataField[0];    // data field
            }
            if (_tempDataField.Length == 3)
            {
                NumByte[5] = _tempDataField[2];    // data field
                NumByte[6] = _tempDataField[1];    // data field
                NumByte[7] = _tempDataField[0];    // data field
            }
            if (_tempDataField.Length == 4)
            {
                NumByte[5] = _tempDataField[3];    // data field
                NumByte[6] = _tempDataField[2];    // data field
                NumByte[7] = _tempDataField[1];    // data field
                NumByte[8] = _tempDataField[0];    // data field
            }
            if (_Port != null)
            {
                _Port.Write(NumByte, 0, NumByte.Length);
            }
            _IsBusy = false;
        }

        public void SetCurrentKValueofPhotodiodeTemperatureCurveValue(double K)
        {
            _IsBusy = true;
            byte[] _tempDataField = new byte[4];
            int _tempNumber = (int)Math.Round((double)K*1000, 0);
            _tempDataField = BitConverter.GetBytes(_tempNumber);
            byte[] NumByte = new byte[] { 0x3a, Position1, 0x01, 0x21, 0x00, 0x00, 0x00, 0x00, 0x00, 0x3b };
            if (_tempDataField.Length == 1)
            {
                NumByte[5] = _tempDataField[0];    // data field
            }
            if (_tempDataField.Length == 2)
            {
                NumByte[5] = _tempDataField[1];    // data field
                NumByte[6] = _tempDataField[0];    // data field
            }
            if (_tempDataField.Length == 3)
            {
                NumByte[5] = _tempDataField[2];    // data field
                NumByte[6] = _tempDataField[1];    // data field
                NumByte[7] = _tempDataField[0];    // data field
            }
            if (_tempDataField.Length == 4)
            {
                NumByte[5] = _tempDataField[3];    // data field
                NumByte[6] = _tempDataField[2];    // data field
                NumByte[7] = _tempDataField[1];    // data field
                NumByte[8] = _tempDataField[0];    // data field
            }
            if (_Port != null)
            {
                _Port.Write(NumByte, 0, NumByte.Length);
            }
            _IsBusy = false;
        }

        public void SetCurrentBValueofPhotodiodeTemperatureCurveValue(double B)
        {
            _IsBusy = true;
            byte[] _tempDataField = new byte[4];
            int _tempNumber = (int)Math.Round((double)B * 1000, 0);
            _tempDataField = BitConverter.GetBytes(_tempNumber);
            byte[] NumByte = new byte[] { 0x3a, Position1, 0x01, 0x22, 0x00, 0x00, 0x00, 0x00, 0x00, 0x3b };
            if (_tempDataField.Length == 1)
            {
                NumByte[5] = _tempDataField[0];    // data field
            }
            if (_tempDataField.Length == 2)
            {
                NumByte[5] = _tempDataField[1];    // data field
                NumByte[6] = _tempDataField[0];    // data field
            }
            if (_tempDataField.Length == 3)
            {
                NumByte[5] = _tempDataField[2];    // data field
                NumByte[6] = _tempDataField[1];    // data field
                NumByte[7] = _tempDataField[0];    // data field
            }
            if (_tempDataField.Length == 4)
            {
                NumByte[5] = _tempDataField[3];    // data field
                NumByte[6] = _tempDataField[2];    // data field
                NumByte[7] = _tempDataField[1];    // data field
                NumByte[8] = _tempDataField[0];    // data field
            }
            if (_Port != null)
            {
                _Port.Write(NumByte, 0, NumByte.Length);
            }
            _IsBusy = false;
        }

        public void GetCurrentLaserNoValue()
        {
            _IsBusy = true;
            byte[] NumByte = new byte[] { 0x3a, Position1, 0x02, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x3b };
            if (_Port != null)
            {
                _Port.Write(NumByte, 0, NumByte.Length);
            }
            _IsBusy = false;
        }

        public void GetCurrentLaserWavaLengthValue()
        {
            _IsBusy = true;
            byte[] NumByte = new byte[] { 0x3a, Position1, 0x02, 0x02, 0x00, 0x00, 0x00, 0x00, 0x00, 0x3b };
            if (_Port != null)
            {
                _Port.Write(NumByte, 0, NumByte.Length);
            }
            _IsBusy = false;
        }

        public void GetCurrentLaserCurrentValueValue()
        {
            _IsBusy = true;
            byte[] NumByte = new byte[] { 0x3a, Position1, 0x02, 0x03, 0x00, 0x00, 0x00, 0x00, 0x00, 0x3b };
            if (_Port != null)
            {
                _Port.Write(NumByte, 0, NumByte.Length);
            }
            _IsBusy = false;
        }

        public void GetCurrentLaserCorrespondingCurrentValueValue(int CurrentMv)
        {
            _IsBusy = true;
            byte flag = 0x05;
            if (CurrentMv == 5)
            {
                flag = 0x05;
            }
            else if (CurrentMv == 10)
            {
                flag = 0x06;
            }
            else if (CurrentMv == 15)
            {
                flag = 0x07;
            }
            else if (CurrentMv == 20)
            {
                flag = 0x08;
            }
            else if (CurrentMv == 25)
            {
                flag = 0x09;
            }
            else if (CurrentMv == 30)
            {
                flag = 0x0A;
            }
            else if (CurrentMv == 35)
            {
                flag = 0x24;
            }
            else if (CurrentMv == 40)
            {
                flag = 0x25;
            }
            else if (CurrentMv == 45)
            {
                flag = 0x26;
            }
            else if (CurrentMv == 50)
            {
                flag = 0x27;
            }
            byte[] NumByte = new byte[] { 0x3a, Position1, 0x02, flag, 0x00, 0x00, 0x00, 0x00, 0x00, 0x3b };
            if (_Port != null)
            {
                _Port.Write(NumByte, 0, NumByte.Length);
            }
            _IsBusy = false;
        }

        public void GetCurrentTECActualTemperatureValue()
        {
            _IsBusy = true;
            byte[] NumByte = new byte[] { 0x3a, Position1, 0x02, 0x0B, 0x00, 0x00, 0x00, 0x00, 0x00, 0x3b };
            if (_Port != null)
            {
                _Port.Write(NumByte, 0, NumByte.Length);
            }
            _IsBusy = false;
        }

        public void GetCurrentTEControlTemperatureValue()
        {
            _IsBusy = true;
            byte[] NumByte = new byte[] { 0x3a, Position1, 0x02, 0x0C, 0x00, 0x00, 0x00, 0x00, 0x00, 0x3b };
            if (_Port != null)
            {
                _Port.Write(NumByte, 0, NumByte.Length);
            }
            _IsBusy = false;
        }

        public void GetCurrentTECMaximumCoolingCurrentValue()
        {
            _IsBusy = true;
            byte[] NumByte = new byte[] { 0x3a, Position1, 0x02, 0x0D, 0x00, 0x00, 0x00, 0x00, 0x00, 0x3b };
            if (_Port != null)
            {
                _Port.Write(NumByte, 0, NumByte.Length);
            }
            _IsBusy = false;
        }

        public void GetCurrentTECRefrigerationControlParameterKpValue()
        {
            _IsBusy = true;
            byte[] NumByte = new byte[] { 0x3a, Position1, 0x02, 0x0E, 0x00, 0x00, 0x00, 0x00, 0x00, 0x3b };
            if (_Port != null)
            {
                _Port.Write(NumByte, 0, NumByte.Length);
            }
            _IsBusy = false;
        }

        public void GetCurrentTECRefrigerationControlParameterKiValue()
        {
            _IsBusy = true;
            byte[] NumByte = new byte[] { 0x3a, Position1, 0x02, 0x0F, 0x00, 0x00, 0x00, 0x00, 0x00, 0x3b };
            if (_Port != null)
            {
                _Port.Write(NumByte, 0, NumByte.Length);
            }
            _IsBusy = false;
        }

        public void GetCurrentTECRefrigerationControlParameterKdValue()
        {
            _IsBusy = true;
            byte[] NumByte = new byte[] { 0x3a, Position1, 0x02, 0x10, 0x00, 0x00, 0x00, 0x00, 0x00, 0x3b };
            if (_Port != null)
            {
                _Port.Write(NumByte, 0, NumByte.Length);
            }
            _IsBusy = false;
        }

        public void GetCurrentLaserLightPowerValueValue()
        {
            _IsBusy = true;
            byte[] NumByte = new byte[] { 0x3a, Position1, 0x02, 0x11, 0x00, 0x00, 0x00, 0x00, 0x00, 0x3b };
            if (_Port != null)
            {
                _Port.Write(NumByte, 0, NumByte.Length);
            }
            _IsBusy = false;
        }

        public void GetCurrentTECWorkingStatusValue()
        {
            _IsBusy = true;
            byte[] NumByte = new byte[] { 0x3a, Position1, 0x02, 0x12, 0x00, 0x00, 0x00, 0x00, 0x00, 0x3b };
            if (_Port != null)
            {
                _Port.Write(NumByte, 0, NumByte.Length);
            }
            _IsBusy = false;
        }

        public void GetCurrentTECCurrentDirectionValue()
        {
            _IsBusy = true;
            byte[] NumByte = new byte[] { 0x3a, Position1, 0x02, 0x13, 0x00, 0x00, 0x00, 0x00, 0x00, 0x3b };
            if (_Port != null)
            {
                _Port.Write(NumByte, 0, NumByte.Length);
            }
            _IsBusy = false;
        }

        public void GetCurrenRadiatorTemperatureValue()
        {
            _IsBusy = true;
            byte[] NumByte = new byte[] { 0x3a, Position1, 0x02, 0x14, 0x00, 0x00, 0x00, 0x00, 0x00, 0x3b };
            if (_Port != null)
            {
                _Port.Write(NumByte, 0, NumByte.Length);
            }
            _IsBusy = false;
        }

        public void GetCurrentTECCurrentCompensationCoefficientValue()
        {
            _IsBusy = true;
            byte[] NumByte = new byte[] { 0x3a, Position1, 0x02, 0x15, 0x00, 0x00, 0x00, 0x00, 0x00, 0x3b };
            if (_Port != null)
            {
                _Port.Write(NumByte, 0, NumByte.Length);
            }
            _IsBusy = false;
        }

        public void Get_CurrentLaserLightPowerValueValue()
        {
            _IsBusy = true;
            byte[] NumByte = new byte[] { 0x3a, Position1, 0x02, 0x16, 0x00, 0x00, 0x00, 0x00, 0x00, 0x3b };
            if (_Port != null)
            {
                _Port.Write(NumByte, 0, NumByte.Length);
            }
            _IsBusy = false;
        }

        public void GetCurrentPowerClosedloopControlParameterKpValue()
        {
            _IsBusy = true;
            byte[] NumByte = new byte[] { 0x3a, Position1, 0x02, 0x17, 0x00, 0x00, 0x00, 0x00, 0x00, 0x3b };
            if (_Port != null)
            {
                _Port.Write(NumByte, 0, NumByte.Length);
            }
            _IsBusy = false;
        }

        public void GetCurrentPowerClosedloopControlParameterKiValue()
        {
            _IsBusy = true;
            byte[] NumByte = new byte[] { 0x3a, Position1, 0x02, 0x18, 0x00, 0x00, 0x00, 0x00, 0x00, 0x3b };
            if (_Port != null)
            {
                _Port.Write(NumByte, 0, NumByte.Length);
            }
            _IsBusy = false;
        }

        public void GetCurrentPowerClosedloopControlParameterKdValue()
        {
            _IsBusy = true;
            byte[] NumByte = new byte[] { 0x3a, Position1, 0x02, 0x19, 0x00, 0x00, 0x00, 0x00, 0x00, 0x3b };
            if (_Port != null)
            {
                _Port.Write(NumByte, 0, NumByte.Length);
            }
            _IsBusy = false;
        }

        public void GetCurrentPhotodiodeVoltageCorrespondingToLaserPowerValue(int CurrentMv)
        {
            _IsBusy = true;
            byte flag = 0x05;
            if (CurrentMv == 5)
            {
                flag = 0x1A;
            }
            else if (CurrentMv == 10)
            {
                flag = 0x1B;
            }
            else if (CurrentMv == 15)
            {
                flag = 0x1C;
            }
            else if (CurrentMv == 20)
            {
                flag = 0x1D;
            }
            else if (CurrentMv == 25)
            {
                flag = 0x1E;
            }
            else if (CurrentMv == 30)
            {
                flag = 0x1F;
            }
            else if (CurrentMv == 35)
            {
                flag = 0x29;
            }
            else
            {
                return;
            }
            byte[] NumByte = new byte[] { 0x3a, Position1, 0x02, flag, 0x00, 0x00, 0x00, 0x00, 0x00, 0x3b };
            if (_Port != null)
            {
                _Port.Write(NumByte, 0, NumByte.Length);
            }
            _IsBusy = false;
        }

        public void GetCurrentPhotodiodeVoltageValue()
        {
            _IsBusy = true;
            byte[] NumByte = new byte[] { 0x3a, Position1, 0x02, 0x20, 0x00, 0x00, 0x00, 0x00, 0x00, 0x3b };
            if (_Port != null)
            {
                _Port.Write(NumByte, 0, NumByte.Length);
            }
            _IsBusy = false;
        }

        public void GetCurrentKValueofPhotodiodeTemperatureCurveValue()
        {
            _IsBusy = true;
            byte[] NumByte = new byte[] { 0x3a, Position1, 0x02, 0x21, 0x00, 0x00, 0x00, 0x00, 0x00, 0x3b };
            if (_Port != null)
            {
                _Port.Write(NumByte, 0, NumByte.Length);
            }
            _IsBusy = false;
        }

        public void GetCurrentBValueofPhotodiodeTemperatureCurveValue()
        {
            _IsBusy = true;
            byte[] NumByte = new byte[] { 0x3a, Position1, 0x02, 0x22, 0x00, 0x00, 0x00, 0x00, 0x00, 0x3b };
            if (_Port != null)
            {
                _Port.Write(NumByte, 0, NumByte.Length);
            }
            _IsBusy = false;
        }


        public void GetPowerClosedloopControlParameterKpLessThanOrEqual15Value()
        {
            _IsBusy = true;
            byte[] NumByte = new byte[] { 0x3a, Position1, 0x02, 0x2A, 0x00, 0x00, 0x00, 0x00, 0x00, 0x3b };
            if (_Port != null)
            {
                _Port.Write(NumByte, 0, NumByte.Length);
            }
            _IsBusy = false;
        }

        public void GetPowerClosedloopControlParameterKiLessThanOrEqual15Value()
        {
            _IsBusy = true;
            byte[] NumByte = new byte[] { 0x3a, Position1, 0x02, 0x2B, 0x00, 0x00, 0x00, 0x00, 0x00, 0x3b };
            if (_Port != null)
            {
                _Port.Write(NumByte, 0, NumByte.Length);
            }
            _IsBusy = false;
        }

        public void GetPowerClosedloopControlParameterKdLessThanOrEqual15Value()
        {
            _IsBusy = true;
            byte[] NumByte = new byte[] { 0x3a, Position1, 0x02, 0x2C, 0x00, 0x00, 0x00, 0x00, 0x00, 0x3b };
            if (_Port != null)
            {
                _Port.Write(NumByte, 0, NumByte.Length);
            }
            _IsBusy = false;
        }
        public void GetUpperLimitKpLessThan15Value()
        {
            _IsBusy = true;
            byte[] NumByte = new byte[] { 0x3a, Position1, 0x02, 0x2D, 0x00, 0x00, 0x00, 0x00, 0x00, 0x3b };
            if (_Port != null)
            {
                _Port.Write(NumByte, 0, NumByte.Length);
            }
            _IsBusy = false;
        }
        public void GetLowerLimitKpLessThan15Value()
        {
            _IsBusy = true;
            byte[] NumByte = new byte[] { 0x3a, Position1, 0x02, 0x2E, 0x00, 0x00, 0x00, 0x00, 0x00, 0x3b };
            if (_Port != null)
            {
                _Port.Write(NumByte, 0, NumByte.Length);
            }
            _IsBusy = false;
        }
        public void GetUpperLimitKiLessThan15Value()
        {
            _IsBusy = true;
            byte[] NumByte = new byte[] { 0x3a, Position1, 0x02, 0x2F, 0x00, 0x00, 0x00, 0x00, 0x00, 0x3b };
            if (_Port != null)
            {
                _Port.Write(NumByte, 0, NumByte.Length);
            }
            _IsBusy = false;
        }
        public void GetLowerLimitKiLessThan15Value()
        {
            _IsBusy = true;
            byte[] NumByte = new byte[] { 0x3a, Position1, 0x02, 0x30, 0x00, 0x00, 0x00, 0x00, 0x00, 0x3b };
            if (_Port != null)
            {
                _Port.Write(NumByte, 0, NumByte.Length);
            }
            _IsBusy = false;
        }
        public void GetUpperLimitKpGreaterThan15Value()
        {
            _IsBusy = true;
            byte[] NumByte = new byte[] { 0x3a, Position1, 0x02, 0x31, 0x00, 0x00, 0x00, 0x00, 0x00, 0x3b };
            if (_Port != null)
            {
                _Port.Write(NumByte, 0, NumByte.Length);
            }
            _IsBusy = false;
        }
        public void GetLowerLimitKpGreaterThan15Value()
        {
            _IsBusy = true;
            byte[] NumByte = new byte[] { 0x3a, Position1, 0x02, 0x32, 0x00, 0x00, 0x00, 0x00, 0x00, 0x3b };
            if (_Port != null)
            {
                _Port.Write(NumByte, 0, NumByte.Length);
            }
            _IsBusy = false;
        }
        public void GetUpperLimitKiGreaterThan15Value()
        {
            _IsBusy = true;
            byte[] NumByte = new byte[] { 0x3a, Position1, 0x02, 0x33, 0x00, 0x00, 0x00, 0x00, 0x00, 0x3b };
            if (_Port != null)
            {
                _Port.Write(NumByte, 0, NumByte.Length);
            }
            _IsBusy = false;
        }
        public void GetLowerLimitKiGreaterThan15Value()
        {
            _IsBusy = true;
            byte[] NumByte = new byte[] { 0x3a, Position1, 0x02, 0x34, 0x00, 0x00, 0x00, 0x00, 0x00, 0x3b };
            if (_Port != null)
            {
                _Port.Write(NumByte, 0, NumByte.Length);
            }
            _IsBusy = false;
        }
        public void GetMaximumOperatingCurrentLaserValue()
        {
            _IsBusy = true;
            byte[] NumByte = new byte[] { 0x3a, Position1, 0x02, 0x35, 0x00, 0x00, 0x00, 0x00, 0x00, 0x3b };
            if (_Port != null)
            {
                _Port.Write(NumByte, 0, NumByte.Length);
            }
            _IsBusy = false;
        }
        public void GetMinimumOperatingCurrentLaserValue()
        {
            _IsBusy = true;
            byte[] NumByte = new byte[] { 0x3a, Position1, 0x02, 0x36, 0x00, 0x00, 0x00, 0x00, 0x00, 0x3b };
            if (_Port != null)
            {
                _Port.Write(NumByte, 0, NumByte.Length);
            }
            _IsBusy = false;
        }
        #endregion

        #region 光学模块设置
        public void SetCurrentLaserWava(double Wava)
        {
            _IsBusy = true;
            byte[] _tempDataField = new byte[4];
            int WavaInt = Convert.ToInt32(Wava);
            _tempDataField = BitConverter.GetBytes(WavaInt);
            _DataField = BitConverter.ToInt32(_tempDataField, 0);
            byte[] NumByte = new byte[] { 0x3a, Position1, 0x01, 0x02, 0x00, 0x00, 0x00, 0x00, 0x00, 0x3b };
            if (_tempDataField.Length == 1)
            {
                NumByte[5] = _tempDataField[3];    // data field
            }
            if (_tempDataField.Length == 2)
            {
                NumByte[5] = _tempDataField[3];    // data field
                NumByte[6] = _tempDataField[2];    // data field
            }
            if (_tempDataField.Length == 3)
            {
                NumByte[5] = _tempDataField[3];    // data field
                NumByte[6] = _tempDataField[2];    // data field
                NumByte[7] = _tempDataField[1];    // data field
            }
            if (_tempDataField.Length == 4)
            {
                NumByte[5] = _tempDataField[3];    // data field
                NumByte[6] = _tempDataField[2];    // data field
                NumByte[7] = _tempDataField[1];    // data field
                NumByte[8] = _tempDataField[0];    // data field
            }
            if (_Port != null)
            {
                _Port.Write(NumByte, 0, NumByte.Length);
            }
            _IsBusy = false;
        }
        public void SetCurrenLaserNumber(string number)
        {
            _IsBusy = true;
            byte[] _tempDataField = new byte[4];
            _tempDataField = HexStringToByteArray(number);
            byte[] NumByte = new byte[] { 0x3a, Position1, 0x01, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x3b };
            if (_tempDataField.Length == 1)
            {
                NumByte[5] = _tempDataField[0];    // data field
            }
            if (_tempDataField.Length == 2)
            {
                NumByte[5] = _tempDataField[0];    // data field
                NumByte[6] = _tempDataField[1];    // data field
            }
            if (_tempDataField.Length == 3)
            {
                NumByte[5] = _tempDataField[0];    // data field
                NumByte[6] = _tempDataField[1];    // data field
                NumByte[7] = _tempDataField[2];    // data field
            }
            if (_tempDataField.Length == 4)
            {
                NumByte[5] = _tempDataField[0];    // data field
                NumByte[6] = _tempDataField[1];    // data field
                NumByte[7] = _tempDataField[2];    // data field
                NumByte[8] = _tempDataField[3];    // data field
            }
            if (_Port != null)
            {
                _Port.Write(NumByte, 0, NumByte.Length);
            }
            _IsBusy = false;
        }
        public void SetCurrenPMTNumber(string number)
        {
            _IsBusy = true;
            byte[] _tempDataField = new byte[4];
            _tempDataField = HexStringToByteArray(number);
            byte[] NumByte = new byte[] { 0x3a, Position2, 0x01, 0x02, 0x00, 0x00, 0x00, 0x00, 0x00, 0x3b };
            if (_tempDataField.Length == 1)
            {
                NumByte[5] = _tempDataField[0];    // data field
            }
            if (_tempDataField.Length == 2)
            {
                NumByte[5] = _tempDataField[0];    // data field
                NumByte[6] = _tempDataField[1];    // data field
            }
            if (_tempDataField.Length == 3)
            {
                NumByte[5] = _tempDataField[0];    // data field
                NumByte[6] = _tempDataField[1];    // data field
                NumByte[7] = _tempDataField[2];    // data field
            }
            if (_tempDataField.Length == 4)
            {
                NumByte[5] = _tempDataField[0];    // data field
                NumByte[6] = _tempDataField[1];    // data field
                NumByte[7] = _tempDataField[2];    // data field
                NumByte[8] = _tempDataField[3];    // data field
            }

            if (_Port != null)
            {
                _Port.Write(NumByte, 0, NumByte.Length);
            }
            _IsBusy = false;
        }
        public void SetCurrenTECctrlTemp(double Temp)
        {
            _IsBusy = true;
            byte[] _tempDataField = new byte[4];
            int _tempNumber = (int)Math.Round((double)Temp * 10, 0);
            _tempDataField = BitConverter.GetBytes(_tempNumber);
            byte[] NumByte = new byte[] { 0x3a, Position1, 0x01, 0x0C, 0x00, 0x00, 0x00, 0x00, 0x00, 0x3b };
            if (_tempDataField.Length == 1)
            {
                NumByte[5] = _tempDataField[0];    // data field
            }
            if (_tempDataField.Length == 2)
            {
                NumByte[5] = _tempDataField[1];    // data field
                NumByte[6] = _tempDataField[0];    // data field
            }
            if (_tempDataField.Length == 3)
            {
                NumByte[5] = _tempDataField[2];    // data field
                NumByte[6] = _tempDataField[1];    // data field
                NumByte[7] = _tempDataField[0];    // data field
            }
            if (_tempDataField.Length == 4)
            {
                NumByte[5] = _tempDataField[3];    // data field
                NumByte[6] = _tempDataField[2];    // data field
                NumByte[7] = _tempDataField[1];    // data field
                NumByte[8] = _tempDataField[0];    // data field
            }
            if (_Port != null)
            {
                _Port.Write(NumByte, 0, NumByte.Length);
            }
            _IsBusy = false;
        }
        public void SetCurrenLaserNot532Powser(int CurrentMv, double power)
        {
            _IsBusy = true;
            byte flag = 0x05;
            if (CurrentMv == 5)
            {
                flag = 0x05;
            }
            else if (CurrentMv == 10)
            {
                flag = 0x06;
            }
            else if (CurrentMv == 15)
            {
                flag = 0x07;
            }
            else if (CurrentMv == 20)
            {
                flag = 0x08;
            }
            else if (CurrentMv == 25)
            {
                flag = 0x09;
            }
            else if (CurrentMv == 30)
            {
                flag = 0x0A;
            }
            else if (CurrentMv == 35)
            {
                flag = 0x24;
            }
            else if (CurrentMv == 40)
            {
                flag = 0x25;
            }
            else if (CurrentMv == 45)
            {
                flag = 0x26;
            }
            else if (CurrentMv == 50)
            {
                flag = 0x27;
            }
            byte[] _tempDataField = new byte[4];
            int _tempNumber = (int)Math.Round((double)power * 100, 0);
            _tempDataField = BitConverter.GetBytes(_tempNumber);
            byte[] NumByte = new byte[] { 0x3a, Position1, 0x01, flag, 0x00, 0x00, 0x00, 0x00, 0x00, 0x3b };
            if (_tempDataField.Length == 1)
            {
                NumByte[5] = _tempDataField[0];    // data field
            }
            if (_tempDataField.Length == 2)
            {
                NumByte[5] = _tempDataField[1];    // data field
                NumByte[6] = _tempDataField[0];    // data field
            }
            if (_tempDataField.Length == 3)
            {
                NumByte[5] = _tempDataField[2];    // data field
                NumByte[6] = _tempDataField[1];    // data field
                NumByte[7] = _tempDataField[0];    // data field
            }
            if (_tempDataField.Length == 4)
            {
                NumByte[5] = _tempDataField[3];    // data field
                NumByte[6] = _tempDataField[2];    // data field
                NumByte[7] = _tempDataField[1];    // data field
                NumByte[8] = _tempDataField[0];    // data field
            }
            if (_Port != null)
            {
                _Port.Write(NumByte, 0, NumByte.Length);
            }
            _IsBusy = false;
        }

        public void SetCurrenTrue532LaserPower(int CurrentMv, double power)
        {
            _IsBusy = true;
            byte flag = 0x05;
            if (CurrentMv == 5)
            {
                flag = 0x1A;
            }
            else if (CurrentMv == 10)
            {
                flag = 0x1B;
            }
            else if (CurrentMv == 15)
            {
                flag = 0x1C;
            }
            else if (CurrentMv == 20)
            {
                flag = 0x1D;
            }
            else if (CurrentMv == 25)
            {
                flag = 0x1E;
            }
            else if (CurrentMv == 30)
            {
                flag = 0x1F;
            }
            byte[] _tempDataField = new byte[4];
            int _tempNumber = (int)Math.Round((double)power * 100, 0);
            _tempDataField = BitConverter.GetBytes(_tempNumber);
            byte[] NumByte = new byte[] { 0x3a, Position1, 0x01, flag, 0x00, 0x00, 0x00, 0x00, 0x00, 0x3b };
            if (_tempDataField.Length == 1)
            {
                NumByte[5] = _tempDataField[0];    // data field
            }
            if (_tempDataField.Length == 2)
            {
                NumByte[5] = _tempDataField[1];    // data field
                NumByte[6] = _tempDataField[0];    // data field
            }
            if (_tempDataField.Length == 3)
            {
                NumByte[5] = _tempDataField[2];    // data field
                NumByte[6] = _tempDataField[1];    // data field
                NumByte[7] = _tempDataField[0];    // data field
            }
            if (_tempDataField.Length == 4)
            {
                NumByte[5] = _tempDataField[3];    // data field
                NumByte[6] = _tempDataField[2];    // data field
                NumByte[7] = _tempDataField[1];    // data field
                NumByte[8] = _tempDataField[0];    // data field
            }
            if (_Port != null)
            {
                _Port.Write(NumByte, 0, NumByte.Length);
            }
            _IsBusy = false;
        }
        public void SetCurrenLaserPowerValue(double Value)
        {
            _IsBusy = true;
            byte[] _tempDataField = new byte[4];
            int _tempNumber = (int)Math.Round((double)Value * 100, 0);
            _tempDataField = BitConverter.GetBytes(_tempNumber);
            _DataField = BitConverter.ToInt32(_tempDataField, 0);
            byte[] NumByte = new byte[] { 0x3a, Position1, 0x01, 0x03, 0x00, 0x00, 0x00, 0x00, 0x00, 0x3b };
            if (_tempDataField.Length == 1)
            {
                NumByte[5] = _tempDataField[0];    // data field
            }
            if (_tempDataField.Length == 2)
            {
                NumByte[5] = _tempDataField[1];    // data field
                NumByte[6] = _tempDataField[0];    // data field
            }
            if (_tempDataField.Length == 3)
            {
                NumByte[5] = _tempDataField[2];    // data field
                NumByte[6] = _tempDataField[1];    // data field
                NumByte[7] = _tempDataField[0];    // data field
            }
            if (_tempDataField.Length == 4)
            {
                NumByte[5] = _tempDataField[3];    // data field
                NumByte[6] = _tempDataField[2];    // data field
                NumByte[7] = _tempDataField[1];    // data field
                NumByte[8] = _tempDataField[0];    // data field
            }
            if (_Port != null)
            {
                _Port.Write(NumByte, 0, NumByte.Length);
            }
            _IsBusy = true;
        }
        public void GetCurrentLaserWava()
        {
            _IsBusy = true;
            byte[] NumByte = new byte[] { 0x3a, Position1, 0x02, 0x02, 0x00, 0x00, 0x00, 0x00, 0x00, 0x3b };
            if (_Port != null)
            {
                _Port.Write(NumByte, 0, NumByte.Length);
            }
            _IsBusy = false;
        }
        public void GetCurrentLaserNumberNo()
        {
            _IsBusy = true;
            byte[] NumByte = new byte[] { 0x3a, Position1, 0x02, 0x1, 0x00, 0x00, 0x00, 0x00, 0x00, 0x3b };
            if (_Port != null)
            {
                _Port.Write(NumByte, 0, NumByte.Length);
            }
            _IsBusy = false;
        }
        public void GetCurrentPMTNumberNo()
        {
            _IsBusy = true;
            byte[] NumByte = new byte[] { 0x3a, Position2, 0x02, 0x02, 0x00, 0x00, 0x00, 0x00, 0x00, 0x3b };
            if (_Port != null)
            {
                _Port.Write(NumByte, 0, NumByte.Length);
            }
            _IsBusy = false;
        }
        public void GetCurrentTECCtrlTemp()
        {
            _IsBusy = true;
            byte[] NumByte = new byte[] { 0x3a, Position1, 0x02, 0xC, 0x00, 0x00, 0x00, 0x00, 0x00, 0x3b };
            if (_Port != null)
            {
                _Port.Write(NumByte, 0, NumByte.Length);
            }
            _IsBusy = false;

        }
        public void GetCurrentLaserPowerValue()
        {
            _IsBusy = true;
            byte[] NumByte = new byte[] { 0x3a, Position2, 0x02, 0x15, 0x00, 0x00, 0x00, 0x00, 0x00, 0x3b };
            if (_Port != null)
            {
                _Port.Write(NumByte, 0, NumByte.Length);
            }
            _IsBusy = false;
        }
        public void GetCurrentLightElectricTowVole()
        {
            _IsBusy = true;
            byte[] NumByte = new byte[] { 0x3a, Position1, 0x02, 0x20, 0x00, 0x00, 0x00, 0x00, 0x00, 0x3b };
            if (_Port != null)
            {
                _Port.Write(NumByte, 0, NumByte.Length);
            }
            _IsBusy = false;
        }
        public void GetCurrentNot532LaserPower(int CurrentMv)
        {
            _IsBusy = true;
            byte flag = 0x05;
            if (CurrentMv == 5)
            {
                flag = 0x05;
            }
            else if (CurrentMv == 10)
            {
                flag = 0x06;
            }
            else if (CurrentMv == 15)
            {
                flag = 0x07;
            }
            else if (CurrentMv == 20)
            {
                flag = 0x08;
            }
            else if (CurrentMv == 25)
            {
                flag = 0x09;
            }
            else if (CurrentMv == 30)
            {
                flag = 0x0A;
            }
            else if (CurrentMv == 35)
            {
                flag = 0x24;
            }
            else if (CurrentMv == 40)
            {
                flag = 0x25;
            }
            else if (CurrentMv == 45)
            {
                flag = 0x26;
            }
            else if (CurrentMv == 50)
            {
                flag = 0x27;
            }
            byte[] NumByte = new byte[] { 0x3a, Position1, 0x02, flag, 0x00, 0x00, 0x00, 0x00, 0x00, 0x3b };
            if (_Port != null)
            {
                _Port.Write(NumByte, 0, NumByte.Length);
            }
            _IsBusy = false;
        }
        public void GetCurrentTrue532LaserPower(int CurrentMv)
        {
            _IsBusy = true;
            byte flag = 0x05;
            if (CurrentMv == 5)
            {
                flag = 0x1A;
            }
            else if (CurrentMv == 10)
            {
                flag = 0x1B;
            }
            else if (CurrentMv == 15)
            {
                flag = 0x1C;
            }
            else if (CurrentMv == 20)
            {
                flag = 0x1D;
            }
            else if (CurrentMv == 25)
            {
                flag = 0x1E;
            }
            else if (CurrentMv == 30)
            {
                flag = 0x1F;
            }
            byte[] NumByte = new byte[] { 0x3a, Position1, 0x02, flag, 0x00, 0x00, 0x00, 0x00, 0x00, 0x3b };
            if (_Port != null)
            {
                _Port.Write(NumByte, 0, NumByte.Length);
            }
            _IsBusy = false;
        }
        public void GetLaservNumberValue()
        {
            _IsBusy = true;
            byte[] NumByte = new byte[] { 0x3a, Position1, 0x02, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x3b };
            if (_Port != null)
            {
                _Port.Write(NumByte, 0, NumByte.Length);
            }
            _IsBusy = false;
        }
        public void GetLaserErrorCodeValue()
        {
            _IsBusy = true;
            byte[] NumByte = new byte[] { 0x3a, Position1, 0x02, 0x23, 0x00, 0x00, 0x00, 0x00, 0x00, 0x3b };
            if (_Port != null)
            {
                _Port.Write(NumByte, 0, NumByte.Length);
            }
            _IsBusy = false;
        }
        public void GetLaserOpticalModuleNumberValue()
        {
            _IsBusy = true;
            byte[] NumByte = new byte[] { 0x3a, Position1, 0x02, 0x28, 0x00, 0x00, 0x00, 0x00, 0x00, 0x3b };
            if (_Port != null)
            {
                _Port.Write(NumByte, 0, NumByte.Length);
            }
            _IsBusy = false;
        }



        #endregion

        /// <summary>  
        /// 16进制字符串转换bai成字节du数zhi组  
        /// </summary>  
        /// <param name="s"></param>  
        /// <returns></returns>  
        public static byte[] HexStringToByteArray(string s)
        {
            s = s.Replace(" ", "");
            if (s.Length % 2 == 1)
            {
                s = "0" + s;
            }
            byte[] buffer = new byte[s.Length / 2];
            for (int i = 0; i < s.Length; i += 2)
            {
                buffer[i / 2] = (byte)Convert.ToByte(s.Substring(i, 2), 16);
            }
            return buffer;
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
        public static string ToHexString(byte[] bytes) // 0xae00cf => "AE00CF "
        {
            Array.Reverse(bytes);
            string hexString = string.Empty;
            if (bytes != null)
            {
                
                System.Text.StringBuilder strB = new System.Text.StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    strB.Append(bytes[i].ToString("X2"));
                }
                hexString = strB.ToString();
            }
            return hexString;
        }

    }
}
