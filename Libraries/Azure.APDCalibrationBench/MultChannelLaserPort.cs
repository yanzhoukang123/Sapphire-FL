using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;

namespace Azure.APDCalibrationBench
{
    public class MultChannelLaserPort : IDisposable
    {
        public delegate void LaserPowerCalibrationCommUpdateHandlerCH1();
        public event LaserPowerCalibrationCommUpdateHandlerCH1 LaserPowerUpdateCommOutputCH1;
        public delegate void LaserPowerCalibrationCommUpdateHandlerCH2();
        public event LaserPowerCalibrationCommUpdateHandlerCH2 LaserPowerUpdateCommOutputCH2;
        public delegate void LaserPowerCalibrationCommUpdateHandlerCH3();
        public event LaserPowerCalibrationCommUpdateHandlerCH3 LaserPowerUpdateCommOutputCH3;
        public delegate void LaserPowerCalibrationCommUpdateHandlerCH4();
        public event LaserPowerCalibrationCommUpdateHandlerCH4 LaserPowerUpdateCommOutputCH4;
        public delegate void LaserPowerCalibrationCommUpdateHandlerCH5();
        public event LaserPowerCalibrationCommUpdateHandlerCH5 LaserPowerUpdateCommOutputCH5;
        public delegate void LaserPowerCalibrationCommUpdateHandlerCH6();
        public event LaserPowerCalibrationCommUpdateHandlerCH6 LaserPowerUpdateCommOutputCH6;
        public delegate void LaserPowerCalibrationCommUpdateHandlerCH7();
        public event LaserPowerCalibrationCommUpdateHandlerCH7 LaserPowerUpdateCommOutputCH7;




        public delegate void PDCalibrationCommUpdateHandlerCH1();
        public event PDCalibrationCommUpdateHandlerCH1 PDUpdateCommOutputCH1;
        public delegate void PDCalibrationCommUpdateHandlerCH2();
        public event PDCalibrationCommUpdateHandlerCH2 PDUpdateCommOutputCH2;
        public delegate void PDCalibrationCommUpdateHandlerCH3();
        public event PDCalibrationCommUpdateHandlerCH3 PDUpdateCommOutputCH3;
        public delegate void PDCalibrationCommUpdateHandlerCH4();
        public event PDCalibrationCommUpdateHandlerCH4 PDUpdateCommOutputCH4;
        public delegate void PDCalibrationCommUpdateHandlerCH5();
        public event PDCalibrationCommUpdateHandlerCH5 PDUpdateCommOutputCH5;
        public delegate void PDCalibrationCommUpdateHandlerCH6();
        public event PDCalibrationCommUpdateHandlerCH6 PDUpdateCommOutputCH6;
        public delegate void PDCalibrationCommUpdateHandlerCH7();
        public event PDCalibrationCommUpdateHandlerCH7 PDUpdateCommOutputCH7;


        public delegate void InteriorPDCalibrationCommUpdateHandlerCH1();
        public event InteriorPDCalibrationCommUpdateHandlerCH1 InteriorPDUpdateCommOutputCH1;
        public delegate void InteriorPDCalibrationCommUpdateHandlerCH2();
        public event InteriorPDCalibrationCommUpdateHandlerCH2 InteriorPDUpdateCommOutputCH2;
        public delegate void InteriorPDCalibrationCommUpdateHandlerCH3();
        public event InteriorPDCalibrationCommUpdateHandlerCH3 InteriorPDUpdateCommOutputCH3;
        public delegate void InteriorPDCalibrationCommUpdateHandlerCH4();
        public event InteriorPDCalibrationCommUpdateHandlerCH4 InteriorPDUpdateCommOutputCH4;
        public delegate void InteriorPDCalibrationCommUpdateHandlerCH5();
        public event InteriorPDCalibrationCommUpdateHandlerCH5 InteriorPDUpdateCommOutputCH5;
        public delegate void InteriorPDCalibrationCommUpdateHandlerCH6();
        public event InteriorPDCalibrationCommUpdateHandlerCH6 InteriorPDUpdateCommOutputCH6;
        public delegate void InteriorPDCalibrationCommUpdateHandlerCH7();
        public event InteriorPDCalibrationCommUpdateHandlerCH7 InteriorPDUpdateCommOutputCH7;


        public delegate void LaserOpticalPowerCalibrationCommUpdateHandlerCH1();
        public event LaserOpticalPowerCalibrationCommUpdateHandlerCH1 LaserOpticalPowerUpdateCommOutputCH1;
        public delegate void LaserOpticalPowerCalibrationCommUpdateHandlerCH2();
        public event LaserOpticalPowerCalibrationCommUpdateHandlerCH2 LaserOpticalPowerUpdateCommOutputCH2;
        public delegate void LaserOpticalPowerCalibrationCommUpdateHandlerCH3();
        public event LaserOpticalPowerCalibrationCommUpdateHandlerCH3 LaserOpticalPowerUpdateCommOutputCH3;
        public delegate void LaserOpticalPowerCalibrationCommUpdateHandlerCH4();
        public event LaserOpticalPowerCalibrationCommUpdateHandlerCH4 LaserOpticalPowerUpdateCommOutputCH4;
        public delegate void LaserOpticalPowerCalibrationCommUpdateHandlerCH5();
        public event LaserOpticalPowerCalibrationCommUpdateHandlerCH5 LaserOpticalPowerUpdateCommOutputCH5;
        public delegate void LaserOpticalPowerCalibrationCommUpdateHandlerCH6();
        public event LaserOpticalPowerCalibrationCommUpdateHandlerCH6 LaserOpticalPowerUpdateCommOutputCH6;
        public delegate void LaserOpticalPowerCalibrationCommUpdateHandlerCH7();
        public event LaserOpticalPowerCalibrationCommUpdateHandlerCH7 LaserOpticalPowerUpdateCommOutputCH7;


        public delegate void LaserElectricCalibrationCommUpdateHandlerCH1();
        public event LaserElectricCalibrationCommUpdateHandlerCH1 LaserElectricUpdateCommOutputCH1;
        public delegate void LaserElectricCalibrationCommUpdateHandlerCH2();
        public event LaserElectricCalibrationCommUpdateHandlerCH2 LaserElectricUpdateCommOutputCH2;
        public delegate void LaserElectricCalibrationCommUpdateHandlerCH3();
        public event LaserElectricCalibrationCommUpdateHandlerCH3 LaserElectricUpdateCommOutputCH3;
        public delegate void LaserElectricCalibrationCommUpdateHandlerCH4();
        public event LaserElectricCalibrationCommUpdateHandlerCH4 LaserElectricUpdateCommOutputCH4;
        public delegate void LaserElectricCalibrationCommUpdateHandlerCH5();
        public event LaserElectricCalibrationCommUpdateHandlerCH5 LaserElectricUpdateCommOutputCH5;
        public delegate void LaserElectricCalibrationCommUpdateHandlerCH6();
        public event LaserElectricCalibrationCommUpdateHandlerCH6 LaserElectricUpdateCommOutputCH6;
        public delegate void LaserElectricCalibrationCommUpdateHandlerCH7();
        public event LaserElectricCalibrationCommUpdateHandlerCH7 LaserElectricUpdateCommOutputCH7;


        public delegate void PhotodiodeVoltageCorrespondingToLaserPowerCalibrationCommUpdateHandlerCH1();
        public event PhotodiodeVoltageCorrespondingToLaserPowerCalibrationCommUpdateHandlerCH1 PhotodiodeVoltageCorrespondingToLaserPowerUpdateCommOutputCH1;
        public delegate void PhotodiodeVoltageCorrespondingToLaserPowerCalibrationCommUpdateHandlerCH2();
        public event PhotodiodeVoltageCorrespondingToLaserPowerCalibrationCommUpdateHandlerCH2 PhotodiodeVoltageCorrespondingToLaserPowerUpdateCommOutputCH2;
        public delegate void PhotodiodeVoltageCorrespondingToLaserPowerCalibrationCommUpdateHandlerCH3();
        public event PhotodiodeVoltageCorrespondingToLaserPowerCalibrationCommUpdateHandlerCH3 PhotodiodeVoltageCorrespondingToLaserPowerUpdateCommOutputCH3;
        public delegate void PhotodiodeVoltageCorrespondingToLaserPowerCalibrationCommUpdateHandlerCH4();
        public event PhotodiodeVoltageCorrespondingToLaserPowerCalibrationCommUpdateHandlerCH4 PhotodiodeVoltageCorrespondingToLaserPowerUpdateCommOutputCH4;
        public delegate void PhotodiodeVoltageCorrespondingToLaserPowerCalibrationCommUpdateHandlerCH5();
        public event PhotodiodeVoltageCorrespondingToLaserPowerCalibrationCommUpdateHandlerCH5 PhotodiodeVoltageCorrespondingToLaserPowerUpdateCommOutputCH5;
        public delegate void PhotodiodeVoltageCorrespondingToLaserPowerCalibrationCommUpdateHandlerCH6();
        public event PhotodiodeVoltageCorrespondingToLaserPowerCalibrationCommUpdateHandlerCH6 PhotodiodeVoltageCorrespondingToLaserPowerUpdateCommOutputCH6;
        public delegate void PhotodiodeVoltageCorrespondingToLaserPowerCalibrationCommUpdateHandlerCH7();
        public event PhotodiodeVoltageCorrespondingToLaserPowerCalibrationCommUpdateHandlerCH7 PhotodiodeVoltageCorrespondingToLaserPowerUpdateCommOutputCH7;


        public delegate void Instruct485CalibrationCommUpdateHandler();
        public event Instruct485CalibrationCommUpdateHandler Instruct485UpdateCommOutput;
        //public delegate void Instruct485CalibrationCommUpdateHandlerCH2();
        //public event Instruct485CalibrationCommUpdateHandlerCH2 Instruct485UpdateCommOutputCH2;
        //public delegate void Instruct485CalibrationCommUpdateHandlerCH3();
        //public event Instruct485CalibrationCommUpdateHandlerCH3 Instruct485UpdateCommOutputCH3;
        //public delegate void Instruct485CalibrationCommUpdateHandlerCH4();
        //public event Instruct485CalibrationCommUpdateHandlerCH4 Instruct485UpdateCommOutputCH4;
        //public delegate void Instruct485CalibrationCommUpdateHandlerCH5();
        //public event Instruct485CalibrationCommUpdateHandlerCH5 Instruct485UpdateCommOutputCH5;
        //public delegate void Instruct485CalibrationCommUpdateHandlerCH6();
        //public event Instruct485CalibrationCommUpdateHandlerCH6 Instruct485UpdateCommOutputCH6;
        //public delegate void Instruct485CalibrationCommUpdateHandlerCH7();
        //public event Instruct485CalibrationCommUpdateHandlerCH7 Instruct485UpdateCommOutputCH7;

        private SerialPort _Port = null;
        private string[] _AvailablePorts;
        private bool _IsConnected = false;
        private bool _IsBusy = false;
        List<byte> PortListData = new List<byte>();
        private byte _FrameHeader = 0x3a;
        private byte _FrameEnd = 0x3b;
        private Thread DataProcessThread;
        //private FunctionCodeType _FunctionCode;
        private int _DataField = 0;
        private double _LaserPowerValueCH1;
        private double _LaserPowerValueCH2;
        private double _LaserPowerValueCH3;
        private double _LaserPowerValueCH4;
        private double _LaserPowerValueCH5;
        private double _LaserPowerValueCH6;
        private double _LaserPowerValueCH7;

        private double _PDVoltageCH1;
        private double _PDVoltageCH2;
        private double _PDVoltageCH3;
        private double _PDVoltageCH4;
        private double _PDVoltageCH5;
        private double _PDVoltageCH6;
        private double _PDVoltageCH7;


        private double _InternalPDVoltageCH1;
        private double _InternalPDVoltageCH2;
        private double _InternalPDVoltageCH3;
        private double _InternalPDVoltageCH4;
        private double _InternalPDVoltageCH5;
        private double _InternalPDVoltageCH6;
        private double _InternalPDVoltageCH7;

        private double _TecTemperValueCH1;
        private double _TecTemperValueCH2;
        private double _TecTemperValueCH3;
        private double _TecTemperValueCH4;
        private double _TecTemperValueCH5;
        private double _TecTemperValueCH6;
        private double _TecTemperValueCH7;

        private double _PhotodiodeVoltageCorrespondingToLaserPowerCH1;
        private double _PhotodiodeVoltageCorrespondingToLaserPowerCH2;
        private double _PhotodiodeVoltageCorrespondingToLaserPowerCH3;
        private double _PhotodiodeVoltageCorrespondingToLaserPowerCH4;
        private double _PhotodiodeVoltageCorrespondingToLaserPowerCH5;
        private double _PhotodiodeVoltageCorrespondingToLaserPowerCH6;
        private double _PhotodiodeVoltageCorrespondingToLaserPowerCH7;

        private double _LaserElectricValueCH1;
        private double _LaserElectricValueCH2;
        private double _LaserElectricValueCH3;
        private double _LaserElectricValueCH4;
        private double _LaserElectricValueCH5;
        private double _LaserElectricValueCH6;
        private double _LaserElectricValueCH7;

        private double temperatureTimeCH1;
        private double temperatureTimeCH2;
        private double temperatureTimeCH3;
        private double temperatureTimeCH4;
        private double temperatureTimeCH5;
        private double temperatureTimeCH6;
        private double temperatureTimeCH7;

        private string errorCodeCH1;
        private string errorCodeCH2;
        private string errorCodeCH3;
        private string errorCodeCH4;
        private string errorCodeCH5;
        private string errorCodeCH6;
        private string errorCodeCH7;


        private bool _DataComplete = false;
        private byte _PUBAddress = 0x01;
        private byte _ChannelAddress = 0x01;
        public enum ModuleType
        {
            Laser = 0x01//
        }
        public enum FunctionCodeType
        {
            WriteReg = 0x01,
            ReadReg = 0x02
        }

        public enum LaserModeRegsAddressType
        {
            PDVoltage = 0x20,//PD电压
            RealityLaserLightPowerValue = 0x11,//实际功率值
            LaserLightPowerValue = 0x16,//功率值
            TECActualTemperature = 0x0B,//温度
            LaserCurrentValue = 0x03,//激光器电流值
            PhotodiodeVoltageCorrespondingToLaserPower5 = 0x1A,//5mW激光功率对应的光电二极管电压
            PhotodiodeVoltageCorrespondingToLaserPower10 = 0x1B,//10mW激光功率对应的光电二极管电压
            PhotodiodeVoltageCorrespondingToLaserPower15 = 0x1C,//15mW激光功率对应的光电二极管电压
            PhotodiodeVoltageCorrespondingToLaserPower20 = 0x1D,//20mW激光功率对应的光电二极管电压
            PhotodiodeVoltageCorrespondingToLaserPower25 = 0x1E,//25mW激光功率对应的光电二极管电压
            PhotodiodeVoltageCorrespondingToLaserPower30 = 0x1F,//30mW激光功率对应的光电二极管电压
            PhotodiodeVoltageCorrespondingToLaserPower35 = 0x05,//35mW激光功率对应的光电二极管电压
            PhotodiodeVoltageCorrespondingToLaserPower40 = 0x06,//40mW激光功率对应的光电二极管电压
            PhotodiodeVoltageCorrespondingToLaserPower45 = 0x07,//45mW激光功率对应的光电二极管电压
            PhotodiodeVoltageCorrespondingToLaserPower50 = 0x08,//50mW激光功率对应的光电二极管电压
            PhotodiodeVoltageCorrespondingToLaserPower55 = 0x09,//55mW激光功率对应的光电二极管电压
            PhotodiodeVoltageCorrespondingToLaserPower60 = 0x0A,//60mW激光功率对应的光电二极管电压
            RadiatorTemperature = 0x14,//散热器温度
        }
        public enum LaserInternalModeRegsAddressType
        {
            InternalAddressCH1 = 0x01,
            InternalAddressCH2 = 0x02,
            InternalAddressCH3 = 0x03,
            InternalAddressCH4 = 0x04,
            InternalAddressCH5 = 0x05,
            InternalAddressCH6 = 0x06,
            InternalAddressCH7 = 0x07,
        }
        public MultChannelLaserPort()
        {
            //_FunctionCode = FunctionCodeType.ReadReg;
            DataProcessThread = new Thread(ProcessData);
            DataProcessThread.IsBackground = true;
            DataProcessThread.Start();
            _IsConnected = false;
            _IsBusy = false;

        }
        public void SearchPort(string COM, int BAUD)
        {
            if (!_IsConnected)
            {
                //_AvailablePorts = SerialPort.GetPortNames();
                //for (int i = 0; i < _AvailablePorts.Length; i++)
                {
                    _Port = new SerialPort(COM, BAUD, Parity.None, 8, StopBits.One);
                    _Port.DataReceived += _Port_DataReceived;
                    try
                    {

                        _Port.Open();
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
                PortListData.AddRange(_PortReceiveBuf);
            }
            catch (Exception exception)
            {

            }

        }

        public void ProcessData()
        {
            while (true)
            {
                Thread.Sleep(20);
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
                                PortListData.RemoveRange(0, index + 1); //数据处理
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

        private void RunReceiveDataCallback(byte[] ReceiveBytes)
        {
            GetByte = ToHexString(ReceiveBytes);
            byte[] _tempArray = new byte[4];
            _tempArray[0] = ReceiveBytes[8];
            _tempArray[1] = ReceiveBytes[7];
            _tempArray[2] = ReceiveBytes[6];
            _tempArray[3] = ReceiveBytes[5];
            _DataField = BitConverter.ToInt32(_tempArray, 0);
            ModuleType type = (ModuleType)ReceiveBytes[1];
            FunctionCodeType readType = (FunctionCodeType)ReceiveBytes[2];
            LaserModeRegsAddressType Address = (LaserModeRegsAddressType)ReceiveBytes[3];
            LaserInternalModeRegsAddressType ChannelAddress = (LaserInternalModeRegsAddressType)ReceiveBytes[4];
            if (FunctionCodeType.ReadReg == readType)
            {
                if (type == (ModuleType)PUBAddress)   //激光版
                {
                    switch (Address)
                    {
                        case LaserModeRegsAddressType.RadiatorTemperature:
                            byte[] _tempArray1 = new byte[2];
                            _tempArray1[0] = ReceiveBytes[6];
                            _tempArray1[1] = ReceiveBytes[5];
                            _DataField = BitConverter.ToInt16(_tempArray1, 0);
                            if (ChannelAddress == LaserInternalModeRegsAddressType.InternalAddressCH1)
                            {
                                TemperatureTimeCH1 = _DataField * 0.05;
                                LaserPowerUpdateCommOutputCH1();
                            }
                            else if (ChannelAddress == LaserInternalModeRegsAddressType.InternalAddressCH2)
                            {
                                TemperatureTimeCH2 = _DataField * 0.05;
                                LaserPowerUpdateCommOutputCH2();
                            }
                            else if (ChannelAddress == LaserInternalModeRegsAddressType.InternalAddressCH3)
                            {
                                TemperatureTimeCH3 = _DataField * 0.05;
                                LaserPowerUpdateCommOutputCH3();
                            }
                            else if (ChannelAddress == LaserInternalModeRegsAddressType.InternalAddressCH4)
                            {
                                TemperatureTimeCH4 = _DataField * 0.05;
                                LaserPowerUpdateCommOutputCH4();
                            }
                            else if (ChannelAddress == LaserInternalModeRegsAddressType.InternalAddressCH5)
                            {
                                TemperatureTimeCH5 = _DataField * 0.05;
                                LaserPowerUpdateCommOutputCH5();
                            }
                            else if (ChannelAddress == LaserInternalModeRegsAddressType.InternalAddressCH6)
                            {
                                TemperatureTimeCH6 = _DataField * 0.05;
                                LaserPowerUpdateCommOutputCH6();
                            }
                            else if (ChannelAddress == LaserInternalModeRegsAddressType.InternalAddressCH7)
                            {
                                TemperatureTimeCH7 = _DataField * 0.05;
                                LaserPowerUpdateCommOutputCH7();
                            }
                            DataComplete = true;
                            break;
                        case LaserModeRegsAddressType.TECActualTemperature:
                            _DataField = BitConverter.ToInt16(_tempArray, 0);
                             if (ChannelAddress == LaserInternalModeRegsAddressType.InternalAddressCH1)
                            {
                                TecTemperValueCH1 = _DataField * 100;
                                if (TecTemperValueCH1 < 0)
                                    TecTemperValueCH1 = 0;
                                ErrorCodeCH1 = ReceiveBytes[6].ToString();
                                LaserPowerUpdateCommOutputCH1();
                            }
                            else if (ChannelAddress == LaserInternalModeRegsAddressType.InternalAddressCH2)
                            {
                                TecTemperValueCH2 = _DataField * 100;
                                if (TecTemperValueCH2 < 0)
                                    TecTemperValueCH2 = 0;
                                ErrorCodeCH2 = ReceiveBytes[6].ToString();
                                LaserPowerUpdateCommOutputCH2();
                            }
                            else if (ChannelAddress == LaserInternalModeRegsAddressType.InternalAddressCH3)
                            {
                                TecTemperValueCH3 = _DataField * 100;
                                if (TecTemperValueCH3 < 0)
                                    TecTemperValueCH3 = 0;
                                ErrorCodeCH3 = ReceiveBytes[6].ToString();
                                LaserPowerUpdateCommOutputCH3();
                            }
                            else if (ChannelAddress == LaserInternalModeRegsAddressType.InternalAddressCH4)
                            {
                                TecTemperValueCH4 = _DataField * 100;
                                if (TecTemperValueCH4 < 0)
                                    TecTemperValueCH4 = 0;
                                ErrorCodeCH4 = ReceiveBytes[6].ToString();
                                LaserPowerUpdateCommOutputCH4();
                            }
                            else if (ChannelAddress == LaserInternalModeRegsAddressType.InternalAddressCH5)
                            {
                                TecTemperValueCH5 = _DataField * 100;
                                if (TecTemperValueCH5 < 0)
                                    TecTemperValueCH5 = 0;
                                ErrorCodeCH5 = ReceiveBytes[6].ToString();
                                LaserPowerUpdateCommOutputCH5();
                            }
                            else if (ChannelAddress == LaserInternalModeRegsAddressType.InternalAddressCH6)
                            {
                                TecTemperValueCH6 = _DataField * 100;
                                if (TecTemperValueCH6 < 0)
                                    TecTemperValueCH6 = 0;
                                ErrorCodeCH6 = ReceiveBytes[6].ToString();
                                LaserPowerUpdateCommOutputCH6();
                            }
                            else if (ChannelAddress == LaserInternalModeRegsAddressType.InternalAddressCH7)
                            {
                                TecTemperValueCH7 = _DataField * 100;
                                if (TecTemperValueCH7 < 0)
                                    TecTemperValueCH7 = 0;
                                ErrorCodeCH7 = ReceiveBytes[6].ToString();
                                LaserPowerUpdateCommOutputCH7();
                            }
                            break;
                        case LaserModeRegsAddressType.LaserLightPowerValue:
                           if (ChannelAddress == LaserInternalModeRegsAddressType.InternalAddressCH1)
                            {
                                LaserPowerValueCH1 = _DataField * 100;
                                LaserOpticalPowerUpdateCommOutputCH1();
                            }
                            else if (ChannelAddress == LaserInternalModeRegsAddressType.InternalAddressCH2)
                            {
                                LaserPowerValueCH2 = _DataField * 100;
                                LaserOpticalPowerUpdateCommOutputCH2();
                            }
                            else if (ChannelAddress == LaserInternalModeRegsAddressType.InternalAddressCH3)
                            {
                                LaserPowerValueCH3 = _DataField * 100;
                                LaserOpticalPowerUpdateCommOutputCH3();
                            }
                            else if (ChannelAddress == LaserInternalModeRegsAddressType.InternalAddressCH4)
                            {
                                LaserPowerValueCH4 = _DataField * 100;
                                LaserOpticalPowerUpdateCommOutputCH4();
                            }
                            else if (ChannelAddress == LaserInternalModeRegsAddressType.InternalAddressCH5)
                            {
                                LaserPowerValueCH5 = _DataField * 100;
                                LaserOpticalPowerUpdateCommOutputCH5();
                            }
                            else if (ChannelAddress == LaserInternalModeRegsAddressType.InternalAddressCH6)
                            {
                                LaserPowerValueCH6 = _DataField * 100;
                                LaserOpticalPowerUpdateCommOutputCH6();
                            }
                            else if (ChannelAddress == LaserInternalModeRegsAddressType.InternalAddressCH7)
                            {
                                LaserPowerValueCH7 = _DataField * 100;
                                LaserOpticalPowerUpdateCommOutputCH7();
                            }
                            break;
                        case LaserModeRegsAddressType.LaserCurrentValue:
                            if (ChannelAddress == LaserInternalModeRegsAddressType.InternalAddressCH1)
                            {
                                LaserElectricValueCH1 = _DataField ;
                                LaserElectricUpdateCommOutputCH1();
                            }
                            else if (ChannelAddress == LaserInternalModeRegsAddressType.InternalAddressCH2)
                            {
                                LaserElectricValueCH2 = _DataField ;
                                LaserElectricUpdateCommOutputCH2();
                            }
                            else if (ChannelAddress == LaserInternalModeRegsAddressType.InternalAddressCH3)
                            {
                                LaserElectricValueCH3 = _DataField ;
                                LaserElectricUpdateCommOutputCH3();
                            }
                            else if (ChannelAddress == LaserInternalModeRegsAddressType.InternalAddressCH4)
                            {
                                LaserElectricValueCH4 = _DataField ;
                                LaserElectricUpdateCommOutputCH4();
                            }
                            else if (ChannelAddress == LaserInternalModeRegsAddressType.InternalAddressCH5)
                            {
                                LaserElectricValueCH5 = _DataField ;
                                LaserElectricUpdateCommOutputCH5();
                            }
                            else if (ChannelAddress == LaserInternalModeRegsAddressType.InternalAddressCH6)
                            {
                                LaserElectricValueCH6 = _DataField ;
                                LaserElectricUpdateCommOutputCH6();
                            }
                            else if (ChannelAddress == LaserInternalModeRegsAddressType.InternalAddressCH7)
                            {
                                LaserElectricValueCH7 = _DataField ;
                                LaserElectricUpdateCommOutputCH7();
                            }
                            break;
                        case LaserModeRegsAddressType.PDVoltage:
                            if (ChannelAddress == LaserInternalModeRegsAddressType.InternalAddressCH1)
                            {
                                PDVoltageCH1 = _DataField;
                                PDUpdateCommOutputCH1();
                            }
                            else if (ChannelAddress == LaserInternalModeRegsAddressType.InternalAddressCH2)
                            {
                                PDVoltageCH2 = _DataField;
                                PDUpdateCommOutputCH2();
                            }
                            else if (ChannelAddress == LaserInternalModeRegsAddressType.InternalAddressCH3)
                            {
                                PDVoltageCH3 = _DataField;
                                PDUpdateCommOutputCH3();
                            }
                            else if (ChannelAddress == LaserInternalModeRegsAddressType.InternalAddressCH4)
                            {
                                PDVoltageCH4 = _DataField;
                                PDUpdateCommOutputCH4();
                            }
                            else if (ChannelAddress == LaserInternalModeRegsAddressType.InternalAddressCH5)
                            {
                                PDVoltageCH5 = _DataField;
                                PDUpdateCommOutputCH5();
                            }
                            else if (ChannelAddress == LaserInternalModeRegsAddressType.InternalAddressCH6)
                            {
                                PDVoltageCH6 = _DataField;
                                PDUpdateCommOutputCH6();
                            }
                            else if (ChannelAddress == LaserInternalModeRegsAddressType.InternalAddressCH7)
                            {
                                PDVoltageCH7 = _DataField;
                                PDUpdateCommOutputCH7();
                            }
                            break;
                        case LaserModeRegsAddressType.PhotodiodeVoltageCorrespondingToLaserPower5:
                            if (ChannelAddress == LaserInternalModeRegsAddressType.InternalAddressCH1)
                            {
                                PhotodiodeVoltageCorrespondingToLaserPowerCH1 = _DataField;
                                PhotodiodeVoltageCorrespondingToLaserPowerUpdateCommOutputCH1();
                            }
                            else if (ChannelAddress == LaserInternalModeRegsAddressType.InternalAddressCH2)
                            {
                                PhotodiodeVoltageCorrespondingToLaserPowerCH2 = _DataField;
                                PhotodiodeVoltageCorrespondingToLaserPowerUpdateCommOutputCH2();
                            }
                            else if (ChannelAddress == LaserInternalModeRegsAddressType.InternalAddressCH3)
                            {
                                PhotodiodeVoltageCorrespondingToLaserPowerCH3 = _DataField;
                                PhotodiodeVoltageCorrespondingToLaserPowerUpdateCommOutputCH3();
                            }
                            else if (ChannelAddress == LaserInternalModeRegsAddressType.InternalAddressCH4)
                            {
                                PhotodiodeVoltageCorrespondingToLaserPowerCH4 = _DataField;
                                PhotodiodeVoltageCorrespondingToLaserPowerUpdateCommOutputCH4();
                            }
                            else if (ChannelAddress == LaserInternalModeRegsAddressType.InternalAddressCH5)
                            {
                                PhotodiodeVoltageCorrespondingToLaserPowerCH5 = _DataField;
                                PhotodiodeVoltageCorrespondingToLaserPowerUpdateCommOutputCH5();
                            }
                            else if (ChannelAddress == LaserInternalModeRegsAddressType.InternalAddressCH6)
                            {
                                PhotodiodeVoltageCorrespondingToLaserPowerCH6 = _DataField;
                                PhotodiodeVoltageCorrespondingToLaserPowerUpdateCommOutputCH6();
                            }
                            else if (ChannelAddress == LaserInternalModeRegsAddressType.InternalAddressCH7)
                            {
                                PhotodiodeVoltageCorrespondingToLaserPowerCH7 = _DataField;
                                PhotodiodeVoltageCorrespondingToLaserPowerUpdateCommOutputCH7();
                            }
                            break;
                        case LaserModeRegsAddressType.PhotodiodeVoltageCorrespondingToLaserPower10:
                            if (ChannelAddress == LaserInternalModeRegsAddressType.InternalAddressCH1)
                            {
                                PhotodiodeVoltageCorrespondingToLaserPowerCH1 = _DataField;
                                PhotodiodeVoltageCorrespondingToLaserPowerUpdateCommOutputCH1();
                            }
                            else if (ChannelAddress == LaserInternalModeRegsAddressType.InternalAddressCH2)
                            {
                                PhotodiodeVoltageCorrespondingToLaserPowerCH2 = _DataField;
                                PhotodiodeVoltageCorrespondingToLaserPowerUpdateCommOutputCH2();
                            }
                            else if (ChannelAddress == LaserInternalModeRegsAddressType.InternalAddressCH3)
                            {
                                PhotodiodeVoltageCorrespondingToLaserPowerCH3 = _DataField;
                                PhotodiodeVoltageCorrespondingToLaserPowerUpdateCommOutputCH3();
                            }
                            else if (ChannelAddress == LaserInternalModeRegsAddressType.InternalAddressCH4)
                            {
                                PhotodiodeVoltageCorrespondingToLaserPowerCH4 = _DataField;
                                PhotodiodeVoltageCorrespondingToLaserPowerUpdateCommOutputCH4();
                            }
                            else if (ChannelAddress == LaserInternalModeRegsAddressType.InternalAddressCH5)
                            {
                                PhotodiodeVoltageCorrespondingToLaserPowerCH5 = _DataField;
                                PhotodiodeVoltageCorrespondingToLaserPowerUpdateCommOutputCH5();
                            }
                            else if (ChannelAddress == LaserInternalModeRegsAddressType.InternalAddressCH6)
                            {
                                PhotodiodeVoltageCorrespondingToLaserPowerCH6 = _DataField;
                                PhotodiodeVoltageCorrespondingToLaserPowerUpdateCommOutputCH6();
                            }
                            else if (ChannelAddress == LaserInternalModeRegsAddressType.InternalAddressCH7)
                            {
                                PhotodiodeVoltageCorrespondingToLaserPowerCH7 = _DataField;
                                PhotodiodeVoltageCorrespondingToLaserPowerUpdateCommOutputCH7();
                            }
                            break;
                        case LaserModeRegsAddressType.PhotodiodeVoltageCorrespondingToLaserPower15:
                            if (ChannelAddress == LaserInternalModeRegsAddressType.InternalAddressCH1)
                            {
                                PhotodiodeVoltageCorrespondingToLaserPowerCH1 = _DataField;
                                PhotodiodeVoltageCorrespondingToLaserPowerUpdateCommOutputCH1();
                            }
                            else if (ChannelAddress == LaserInternalModeRegsAddressType.InternalAddressCH2)
                            {
                                PhotodiodeVoltageCorrespondingToLaserPowerCH2 = _DataField;
                                PhotodiodeVoltageCorrespondingToLaserPowerUpdateCommOutputCH2();
                            }
                            else if (ChannelAddress == LaserInternalModeRegsAddressType.InternalAddressCH3)
                            {
                                PhotodiodeVoltageCorrespondingToLaserPowerCH3 = _DataField;
                                PhotodiodeVoltageCorrespondingToLaserPowerUpdateCommOutputCH3();
                            }
                            else if (ChannelAddress == LaserInternalModeRegsAddressType.InternalAddressCH4)
                            {
                                PhotodiodeVoltageCorrespondingToLaserPowerCH4 = _DataField;
                                PhotodiodeVoltageCorrespondingToLaserPowerUpdateCommOutputCH4();
                            }
                            else if (ChannelAddress == LaserInternalModeRegsAddressType.InternalAddressCH5)
                            {
                                PhotodiodeVoltageCorrespondingToLaserPowerCH5 = _DataField;
                                PhotodiodeVoltageCorrespondingToLaserPowerUpdateCommOutputCH5();
                            }
                            else if (ChannelAddress == LaserInternalModeRegsAddressType.InternalAddressCH6)
                            {
                                PhotodiodeVoltageCorrespondingToLaserPowerCH6 = _DataField;
                                PhotodiodeVoltageCorrespondingToLaserPowerUpdateCommOutputCH6();
                            }
                            else if (ChannelAddress == LaserInternalModeRegsAddressType.InternalAddressCH7)
                            {
                                PhotodiodeVoltageCorrespondingToLaserPowerCH7 = _DataField;
                                PhotodiodeVoltageCorrespondingToLaserPowerUpdateCommOutputCH7();
                            }
                            break;
                        case LaserModeRegsAddressType.PhotodiodeVoltageCorrespondingToLaserPower20:
                            if (ChannelAddress == LaserInternalModeRegsAddressType.InternalAddressCH1)
                            {
                                PhotodiodeVoltageCorrespondingToLaserPowerCH1 = _DataField;
                                PhotodiodeVoltageCorrespondingToLaserPowerUpdateCommOutputCH1();
                            }
                            else if (ChannelAddress == LaserInternalModeRegsAddressType.InternalAddressCH2)
                            {
                                PhotodiodeVoltageCorrespondingToLaserPowerCH2 = _DataField;
                                PhotodiodeVoltageCorrespondingToLaserPowerUpdateCommOutputCH2();
                            }
                            else if (ChannelAddress == LaserInternalModeRegsAddressType.InternalAddressCH3)
                            {
                                PhotodiodeVoltageCorrespondingToLaserPowerCH3 = _DataField;
                                PhotodiodeVoltageCorrespondingToLaserPowerUpdateCommOutputCH3();
                            }
                            else if (ChannelAddress == LaserInternalModeRegsAddressType.InternalAddressCH4)
                            {
                                PhotodiodeVoltageCorrespondingToLaserPowerCH4 = _DataField;
                                PhotodiodeVoltageCorrespondingToLaserPowerUpdateCommOutputCH4();
                            }
                            else if (ChannelAddress == LaserInternalModeRegsAddressType.InternalAddressCH5)
                            {
                                PhotodiodeVoltageCorrespondingToLaserPowerCH5 = _DataField;
                                PhotodiodeVoltageCorrespondingToLaserPowerUpdateCommOutputCH5();
                            }
                            else if (ChannelAddress == LaserInternalModeRegsAddressType.InternalAddressCH6)
                            {
                                PhotodiodeVoltageCorrespondingToLaserPowerCH6 = _DataField;
                                PhotodiodeVoltageCorrespondingToLaserPowerUpdateCommOutputCH6();
                            }
                            else if (ChannelAddress == LaserInternalModeRegsAddressType.InternalAddressCH7)
                            {
                                PhotodiodeVoltageCorrespondingToLaserPowerCH7 = _DataField;
                                PhotodiodeVoltageCorrespondingToLaserPowerUpdateCommOutputCH7();
                            }
                            break;
                        case LaserModeRegsAddressType.PhotodiodeVoltageCorrespondingToLaserPower25:
                            if (ChannelAddress == LaserInternalModeRegsAddressType.InternalAddressCH1)
                            {
                                PhotodiodeVoltageCorrespondingToLaserPowerCH1 = _DataField;
                                PhotodiodeVoltageCorrespondingToLaserPowerUpdateCommOutputCH1();
                            }
                            else if (ChannelAddress == LaserInternalModeRegsAddressType.InternalAddressCH2)
                            {
                                PhotodiodeVoltageCorrespondingToLaserPowerCH2 = _DataField;
                                PhotodiodeVoltageCorrespondingToLaserPowerUpdateCommOutputCH2();
                            }
                            else if (ChannelAddress == LaserInternalModeRegsAddressType.InternalAddressCH3)
                            {
                                PhotodiodeVoltageCorrespondingToLaserPowerCH3 = _DataField;
                                PhotodiodeVoltageCorrespondingToLaserPowerUpdateCommOutputCH3();
                            }
                            else if (ChannelAddress == LaserInternalModeRegsAddressType.InternalAddressCH4)
                            {
                                PhotodiodeVoltageCorrespondingToLaserPowerCH4 = _DataField;
                                PhotodiodeVoltageCorrespondingToLaserPowerUpdateCommOutputCH4();
                            }
                            else if (ChannelAddress == LaserInternalModeRegsAddressType.InternalAddressCH5)
                            {
                                PhotodiodeVoltageCorrespondingToLaserPowerCH5 = _DataField;
                                PhotodiodeVoltageCorrespondingToLaserPowerUpdateCommOutputCH5();
                            }
                            else if (ChannelAddress == LaserInternalModeRegsAddressType.InternalAddressCH6)
                            {
                                PhotodiodeVoltageCorrespondingToLaserPowerCH6 = _DataField;
                                PhotodiodeVoltageCorrespondingToLaserPowerUpdateCommOutputCH6();
                            }
                            else if (ChannelAddress == LaserInternalModeRegsAddressType.InternalAddressCH7)
                            {
                                PhotodiodeVoltageCorrespondingToLaserPowerCH7 = _DataField;
                                PhotodiodeVoltageCorrespondingToLaserPowerUpdateCommOutputCH7();
                            }
                            break;
                        case LaserModeRegsAddressType.PhotodiodeVoltageCorrespondingToLaserPower30:
                            if (ChannelAddress == LaserInternalModeRegsAddressType.InternalAddressCH1)
                            {
                                PhotodiodeVoltageCorrespondingToLaserPowerCH1 = _DataField;
                                PhotodiodeVoltageCorrespondingToLaserPowerUpdateCommOutputCH1();
                            }
                            else if (ChannelAddress == LaserInternalModeRegsAddressType.InternalAddressCH2)
                            {
                                PhotodiodeVoltageCorrespondingToLaserPowerCH2 = _DataField;
                                PhotodiodeVoltageCorrespondingToLaserPowerUpdateCommOutputCH2();
                            }
                            else if (ChannelAddress == LaserInternalModeRegsAddressType.InternalAddressCH3)
                            {
                                PhotodiodeVoltageCorrespondingToLaserPowerCH3 = _DataField;
                                PhotodiodeVoltageCorrespondingToLaserPowerUpdateCommOutputCH3();
                            }
                            else if (ChannelAddress == LaserInternalModeRegsAddressType.InternalAddressCH4)
                            {
                                PhotodiodeVoltageCorrespondingToLaserPowerCH4 = _DataField;
                                PhotodiodeVoltageCorrespondingToLaserPowerUpdateCommOutputCH4();
                            }
                            else if (ChannelAddress == LaserInternalModeRegsAddressType.InternalAddressCH5)
                            {
                                PhotodiodeVoltageCorrespondingToLaserPowerCH5 = _DataField;
                                PhotodiodeVoltageCorrespondingToLaserPowerUpdateCommOutputCH5();
                            }
                            else if (ChannelAddress == LaserInternalModeRegsAddressType.InternalAddressCH6)
                            {
                                PhotodiodeVoltageCorrespondingToLaserPowerCH6 = _DataField;
                                PhotodiodeVoltageCorrespondingToLaserPowerUpdateCommOutputCH6();
                            }
                            else if (ChannelAddress == LaserInternalModeRegsAddressType.InternalAddressCH7)
                            {
                                PhotodiodeVoltageCorrespondingToLaserPowerCH7 = _DataField;
                                PhotodiodeVoltageCorrespondingToLaserPowerUpdateCommOutputCH7();
                            }
                            break;
                        case LaserModeRegsAddressType.PhotodiodeVoltageCorrespondingToLaserPower35:
                            if (ChannelAddress == LaserInternalModeRegsAddressType.InternalAddressCH1)
                            {
                                PhotodiodeVoltageCorrespondingToLaserPowerCH1 = _DataField;
                                PhotodiodeVoltageCorrespondingToLaserPowerUpdateCommOutputCH1();
                            }
                            else if (ChannelAddress == LaserInternalModeRegsAddressType.InternalAddressCH2)
                            {
                                PhotodiodeVoltageCorrespondingToLaserPowerCH2 = _DataField;
                                PhotodiodeVoltageCorrespondingToLaserPowerUpdateCommOutputCH2();
                            }
                            else if (ChannelAddress == LaserInternalModeRegsAddressType.InternalAddressCH3)
                            {
                                PhotodiodeVoltageCorrespondingToLaserPowerCH3 = _DataField;
                                PhotodiodeVoltageCorrespondingToLaserPowerUpdateCommOutputCH3();
                            }
                            else if (ChannelAddress == LaserInternalModeRegsAddressType.InternalAddressCH4)
                            {
                                PhotodiodeVoltageCorrespondingToLaserPowerCH4 = _DataField;
                                PhotodiodeVoltageCorrespondingToLaserPowerUpdateCommOutputCH4();
                            }
                            else if (ChannelAddress == LaserInternalModeRegsAddressType.InternalAddressCH5)
                            {
                                PhotodiodeVoltageCorrespondingToLaserPowerCH5 = _DataField;
                                PhotodiodeVoltageCorrespondingToLaserPowerUpdateCommOutputCH5();
                            }
                            else if (ChannelAddress == LaserInternalModeRegsAddressType.InternalAddressCH6)
                            {
                                PhotodiodeVoltageCorrespondingToLaserPowerCH6 = _DataField;
                                PhotodiodeVoltageCorrespondingToLaserPowerUpdateCommOutputCH6();
                            }
                            else if (ChannelAddress == LaserInternalModeRegsAddressType.InternalAddressCH7)
                            {
                                PhotodiodeVoltageCorrespondingToLaserPowerCH7 = _DataField;
                                PhotodiodeVoltageCorrespondingToLaserPowerUpdateCommOutputCH7();
                            }
                            break;
                        case LaserModeRegsAddressType.PhotodiodeVoltageCorrespondingToLaserPower40:
                            if (ChannelAddress == LaserInternalModeRegsAddressType.InternalAddressCH1)
                            {
                                PhotodiodeVoltageCorrespondingToLaserPowerCH1 = _DataField;
                                PhotodiodeVoltageCorrespondingToLaserPowerUpdateCommOutputCH1();
                            }
                            else if (ChannelAddress == LaserInternalModeRegsAddressType.InternalAddressCH2)
                            {
                                PhotodiodeVoltageCorrespondingToLaserPowerCH2 = _DataField;
                                PhotodiodeVoltageCorrespondingToLaserPowerUpdateCommOutputCH2();
                            }
                            else if (ChannelAddress == LaserInternalModeRegsAddressType.InternalAddressCH3)
                            {
                                PhotodiodeVoltageCorrespondingToLaserPowerCH3 = _DataField;
                                PhotodiodeVoltageCorrespondingToLaserPowerUpdateCommOutputCH3();
                            }
                            else if (ChannelAddress == LaserInternalModeRegsAddressType.InternalAddressCH4)
                            {
                                PhotodiodeVoltageCorrespondingToLaserPowerCH4 = _DataField;
                                PhotodiodeVoltageCorrespondingToLaserPowerUpdateCommOutputCH4();
                            }
                            else if (ChannelAddress == LaserInternalModeRegsAddressType.InternalAddressCH5)
                            {
                                PhotodiodeVoltageCorrespondingToLaserPowerCH5 = _DataField;
                                PhotodiodeVoltageCorrespondingToLaserPowerUpdateCommOutputCH5();
                            }
                            else if (ChannelAddress == LaserInternalModeRegsAddressType.InternalAddressCH6)
                            {
                                PhotodiodeVoltageCorrespondingToLaserPowerCH6 = _DataField;
                                PhotodiodeVoltageCorrespondingToLaserPowerUpdateCommOutputCH6();
                            }
                            else if (ChannelAddress == LaserInternalModeRegsAddressType.InternalAddressCH7)
                            {
                                PhotodiodeVoltageCorrespondingToLaserPowerCH7 = _DataField;
                                PhotodiodeVoltageCorrespondingToLaserPowerUpdateCommOutputCH7();
                            }
                            break;
                        case LaserModeRegsAddressType.PhotodiodeVoltageCorrespondingToLaserPower45:
                            if (ChannelAddress == LaserInternalModeRegsAddressType.InternalAddressCH1)
                            {
                                PhotodiodeVoltageCorrespondingToLaserPowerCH1 = _DataField;
                                PhotodiodeVoltageCorrespondingToLaserPowerUpdateCommOutputCH1();
                            }
                            else if (ChannelAddress == LaserInternalModeRegsAddressType.InternalAddressCH2)
                            {
                                PhotodiodeVoltageCorrespondingToLaserPowerCH2 = _DataField;
                                PhotodiodeVoltageCorrespondingToLaserPowerUpdateCommOutputCH2();
                            }
                            else if (ChannelAddress == LaserInternalModeRegsAddressType.InternalAddressCH3)
                            {
                                PhotodiodeVoltageCorrespondingToLaserPowerCH3 = _DataField;
                                PhotodiodeVoltageCorrespondingToLaserPowerUpdateCommOutputCH3();
                            }
                            else if (ChannelAddress == LaserInternalModeRegsAddressType.InternalAddressCH4)
                            {
                                PhotodiodeVoltageCorrespondingToLaserPowerCH4 = _DataField;
                                PhotodiodeVoltageCorrespondingToLaserPowerUpdateCommOutputCH4();
                            }
                            else if (ChannelAddress == LaserInternalModeRegsAddressType.InternalAddressCH5)
                            {
                                PhotodiodeVoltageCorrespondingToLaserPowerCH5 = _DataField;
                                PhotodiodeVoltageCorrespondingToLaserPowerUpdateCommOutputCH5();
                            }
                            else if (ChannelAddress == LaserInternalModeRegsAddressType.InternalAddressCH6)
                            {
                                PhotodiodeVoltageCorrespondingToLaserPowerCH6 = _DataField;
                                PhotodiodeVoltageCorrespondingToLaserPowerUpdateCommOutputCH6();
                            }
                            else if (ChannelAddress == LaserInternalModeRegsAddressType.InternalAddressCH7)
                            {
                                PhotodiodeVoltageCorrespondingToLaserPowerCH7 = _DataField;
                                PhotodiodeVoltageCorrespondingToLaserPowerUpdateCommOutputCH7();
                            }
                            break;
                        case LaserModeRegsAddressType.PhotodiodeVoltageCorrespondingToLaserPower50:
                            if (ChannelAddress == LaserInternalModeRegsAddressType.InternalAddressCH1)
                            {
                                PhotodiodeVoltageCorrespondingToLaserPowerCH1 = _DataField;
                                PhotodiodeVoltageCorrespondingToLaserPowerUpdateCommOutputCH1();
                            }
                            else if (ChannelAddress == LaserInternalModeRegsAddressType.InternalAddressCH2)
                            {
                                PhotodiodeVoltageCorrespondingToLaserPowerCH2 = _DataField;
                                PhotodiodeVoltageCorrespondingToLaserPowerUpdateCommOutputCH2();
                            }
                            else if (ChannelAddress == LaserInternalModeRegsAddressType.InternalAddressCH3)
                            {
                                PhotodiodeVoltageCorrespondingToLaserPowerCH3 = _DataField;
                                PhotodiodeVoltageCorrespondingToLaserPowerUpdateCommOutputCH3();
                            }
                            else if (ChannelAddress == LaserInternalModeRegsAddressType.InternalAddressCH4)
                            {
                                PhotodiodeVoltageCorrespondingToLaserPowerCH4 = _DataField;
                                PhotodiodeVoltageCorrespondingToLaserPowerUpdateCommOutputCH4();
                            }
                            else if (ChannelAddress == LaserInternalModeRegsAddressType.InternalAddressCH5)
                            {
                                PhotodiodeVoltageCorrespondingToLaserPowerCH5 = _DataField;
                                PhotodiodeVoltageCorrespondingToLaserPowerUpdateCommOutputCH5();
                            }
                            else if (ChannelAddress == LaserInternalModeRegsAddressType.InternalAddressCH6)
                            {
                                PhotodiodeVoltageCorrespondingToLaserPowerCH6 = _DataField;
                                PhotodiodeVoltageCorrespondingToLaserPowerUpdateCommOutputCH6();
                            }
                            else if (ChannelAddress == LaserInternalModeRegsAddressType.InternalAddressCH7)
                            {
                                PhotodiodeVoltageCorrespondingToLaserPowerCH7 = _DataField;
                                PhotodiodeVoltageCorrespondingToLaserPowerUpdateCommOutputCH7();
                            }
                            break;
                        case LaserModeRegsAddressType.PhotodiodeVoltageCorrespondingToLaserPower55:
                            if (ChannelAddress == LaserInternalModeRegsAddressType.InternalAddressCH1)
                            {
                                PhotodiodeVoltageCorrespondingToLaserPowerCH1 = _DataField;
                                PhotodiodeVoltageCorrespondingToLaserPowerUpdateCommOutputCH1();
                            }
                            else if (ChannelAddress == LaserInternalModeRegsAddressType.InternalAddressCH2)
                            {
                                PhotodiodeVoltageCorrespondingToLaserPowerCH2 = _DataField;
                                PhotodiodeVoltageCorrespondingToLaserPowerUpdateCommOutputCH2();
                            }
                            else if (ChannelAddress == LaserInternalModeRegsAddressType.InternalAddressCH3)
                            {
                                PhotodiodeVoltageCorrespondingToLaserPowerCH3 = _DataField;
                                PhotodiodeVoltageCorrespondingToLaserPowerUpdateCommOutputCH3();
                            }
                            else if (ChannelAddress == LaserInternalModeRegsAddressType.InternalAddressCH4)
                            {
                                PhotodiodeVoltageCorrespondingToLaserPowerCH4 = _DataField;
                                PhotodiodeVoltageCorrespondingToLaserPowerUpdateCommOutputCH4();
                            }
                            else if (ChannelAddress == LaserInternalModeRegsAddressType.InternalAddressCH5)
                            {
                                PhotodiodeVoltageCorrespondingToLaserPowerCH5 = _DataField;
                                PhotodiodeVoltageCorrespondingToLaserPowerUpdateCommOutputCH5();
                            }
                            else if (ChannelAddress == LaserInternalModeRegsAddressType.InternalAddressCH6)
                            {
                                PhotodiodeVoltageCorrespondingToLaserPowerCH6 = _DataField;
                                PhotodiodeVoltageCorrespondingToLaserPowerUpdateCommOutputCH6();
                            }
                            else if (ChannelAddress == LaserInternalModeRegsAddressType.InternalAddressCH7)
                            {
                                PhotodiodeVoltageCorrespondingToLaserPowerCH7 = _DataField;
                                PhotodiodeVoltageCorrespondingToLaserPowerUpdateCommOutputCH7();
                            }
                            break;
                        case LaserModeRegsAddressType.PhotodiodeVoltageCorrespondingToLaserPower60:
                            if (ChannelAddress == LaserInternalModeRegsAddressType.InternalAddressCH1)
                            {
                                PhotodiodeVoltageCorrespondingToLaserPowerCH1 = _DataField;
                                PhotodiodeVoltageCorrespondingToLaserPowerUpdateCommOutputCH1();
                            }
                            else if (ChannelAddress == LaserInternalModeRegsAddressType.InternalAddressCH2)
                            {
                                PhotodiodeVoltageCorrespondingToLaserPowerCH2 = _DataField;
                                PhotodiodeVoltageCorrespondingToLaserPowerUpdateCommOutputCH2();
                            }
                            else if (ChannelAddress == LaserInternalModeRegsAddressType.InternalAddressCH3)
                            {
                                PhotodiodeVoltageCorrespondingToLaserPowerCH3 = _DataField;
                                PhotodiodeVoltageCorrespondingToLaserPowerUpdateCommOutputCH3();
                            }
                            else if (ChannelAddress == LaserInternalModeRegsAddressType.InternalAddressCH4)
                            {
                                PhotodiodeVoltageCorrespondingToLaserPowerCH4 = _DataField;
                                PhotodiodeVoltageCorrespondingToLaserPowerUpdateCommOutputCH4();
                            }
                            else if (ChannelAddress == LaserInternalModeRegsAddressType.InternalAddressCH5)
                            {
                                PhotodiodeVoltageCorrespondingToLaserPowerCH5 = _DataField;
                                PhotodiodeVoltageCorrespondingToLaserPowerUpdateCommOutputCH5();
                            }
                            else if (ChannelAddress == LaserInternalModeRegsAddressType.InternalAddressCH6)
                            {
                                PhotodiodeVoltageCorrespondingToLaserPowerCH6 = _DataField;
                                PhotodiodeVoltageCorrespondingToLaserPowerUpdateCommOutputCH6();
                            }
                            else if (ChannelAddress == LaserInternalModeRegsAddressType.InternalAddressCH7)
                            {
                                PhotodiodeVoltageCorrespondingToLaserPowerCH7 = _DataField;
                                PhotodiodeVoltageCorrespondingToLaserPowerUpdateCommOutputCH7();
                            }
                            break;
                    }

                }
            }
            else
            {
                if (type == (ModuleType)PUBAddress)   //激光版
                {
                    switch (Address)
                    {
                        case LaserModeRegsAddressType.RealityLaserLightPowerValue:
                            DataComplete = true;
                            break;
                        case LaserModeRegsAddressType.RadiatorTemperature:
                            DataComplete = true;
                            break;
                    }
                }
            }
           //if (ChannelAddress == LaserInternalModeRegsAddressType.InternalAddressCH1)
           // {
                Instruct485UpdateCommOutput();
            //}
            //else if (ChannelAddress == LaserInternalModeRegsAddressType.InternalAddressCH2)
            //{
            //    Instruct485UpdateCommOutputCH2();
            //}
            //else if (ChannelAddress == LaserInternalModeRegsAddressType.InternalAddressCH3)
            //{
            //    Instruct485UpdateCommOutputCH3();
            //}
            //else if (ChannelAddress == LaserInternalModeRegsAddressType.InternalAddressCH4)
            //{
            //    Instruct485UpdateCommOutputCH4();
            //}
            //else if (ChannelAddress == LaserInternalModeRegsAddressType.InternalAddressCH5)
            //{
            //    Instruct485UpdateCommOutputCH5();
            //}
            //else if (ChannelAddress == LaserInternalModeRegsAddressType.InternalAddressCH6)
            //{
            //    Instruct485UpdateCommOutputCH6();
            //}
            //else if (ChannelAddress == LaserInternalModeRegsAddressType.InternalAddressCH7)
            //{
            //    Instruct485UpdateCommOutputCH7();
            //}
        }
        public void Dispose()
        {
            if (_Port != null)
            {
                _Port.DataReceived -= _Port_DataReceived;
                _Port.Close();
                _Port = null;
                //DataProcessThread.Abort();
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
            //Array.Reverse(bytes);
            string hexString = string.Empty;
            if (bytes != null)
            {

                System.Text.StringBuilder strB = new System.Text.StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    strB.Append(bytes[i].ToString("X2") + " ");
                }
                hexString = strB.ToString();
            }
            return hexString;
        }

        //读激光器温度
        public void GetCurrentTECActualTemperatureValue()
        {
            byte[] NumByte = new byte[] { 0x3a, PUBAddress, 0x02, 0x0B, ChannelAddress, 0x00, 0x00, 0x00, 0x00, 0x3b };
            if (_Port != null)
            {
                _Port.Write(NumByte, 0, NumByte.Length);
            }
        }

        public void SetInstruct485(string Instruct)
        {
            DataComplete = false;
            byte[] NumByte = StringToBytes(Instruct);
            if (_Port != null)
            {
                _Port.Write(NumByte, 0, NumByte.Length);
            }
        }

        //写入激光器电流值
        public void SetCurrentLaserCurrentValue(double Value)
        {
            DataComplete = false;
            byte[] _tempDataField = new byte[4];
            int _tempNumber = (int)Math.Round((double)Value, 0);
            _tempDataField = BitConverter.GetBytes(_tempNumber);
            _DataField = BitConverter.ToInt32(_tempDataField, 0);
            byte[] NumByte = new byte[] { 0x3a, PUBAddress, 0x01, 0x03, ChannelAddress, 0x00, 0x00, 0x00, 0x00, 0x3b };
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
        }

        public void SetCurrentLaserCurrentValue(double Value,int Address)
        {
            DataComplete = false;
            byte[] _tempDataField = new byte[4];
            int _tempNumber = (int)Math.Round((double)Value, 0);
            _tempDataField = BitConverter.GetBytes(_tempNumber);
            _DataField = BitConverter.ToInt32(_tempDataField, 0);
            byte[] NumByte = new byte[] { 0x3a, PUBAddress, 0x01, 0x03, Convert.ToByte(Address), 0x00, 0x00, 0x00, 0x00, 0x3b };
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
        }

        //读取激光器电流值
        public void GetCurrentLaserCurrentValue()
        {
            DataComplete = false;
            byte[] NumByte = new byte[] { 0x3a, PUBAddress, 0x02, 0x03, ChannelAddress, 0x00, 0x00, 0x00, 0x00, 0x3b };
            if (_Port != null)
            {
                _Port.Write(NumByte, 0, NumByte.Length);
            }
        }

        //读取激光器光功率
        public void GetCurrentLaserLightPowerValueValue()
        {
            DataComplete = false;
            byte[] NumByte = new byte[] { 0x3a, PUBAddress, 0x02, 0x16, ChannelAddress, 0x00, 0x00, 0x00, 0x00, 0x3b };
            if (_Port != null)
            {
                _Port.Write(NumByte, 0, NumByte.Length);
            }
        }

        public void GetCurrentLaserLightPowerValueValue(int Address)
        {
            DataComplete = false;
            byte[] NumByte = new byte[] { 0x3a, PUBAddress, 0x02, 0x16, Convert.ToByte(Address), 0x00, 0x00, 0x00, 0x00, 0x3b };
            if (_Port != null)
            {
                _Port.Write(NumByte, 0, NumByte.Length);
            }
        }

        //读取散热器温度
        public void GetCurrentRadiatorValueValue()
        {
            DataComplete = false;
            byte[] NumByte = new byte[] { 0x3a, PUBAddress, 0x02, 0x14, ChannelAddress, 0x00, 0x00, 0x00, 0x00, 0x3b };
            if (_Port != null)
            {
                _Port.Write(NumByte, 0, NumByte.Length);
            }
        }

        //设置激光器光功率
        public void SetCurrentLaserLightPowerValueValue(double Value)
        {
            DataComplete = false;
            byte[] _tempDataField = new byte[4];
            int _tempNumber = (int)Math.Round((double)Value, 0);
            _tempDataField = BitConverter.GetBytes(_tempNumber);
            _DataField = BitConverter.ToInt32(_tempDataField, 0);
            byte[] NumByte = new byte[] { 0x3a, PUBAddress, 0x01, 0x11, ChannelAddress, 0x00, 0x00, 0x00, 0x00, 0x3b };
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
        }

        //写入532PD
        public void SetPDVoltage(double Value)
        {
            byte[] _tempDataField = new byte[4];
            int _tempNumber = (int)Math.Round((double)Value, 0);
            _tempDataField = BitConverter.GetBytes(_tempNumber);
            _DataField = BitConverter.ToInt32(_tempDataField, 0);
            byte[] NumByte = new byte[] { 0x3a, PUBAddress, 0x01, 0x20, ChannelAddress, 0x00, 0x00, 0x00, 0x00, 0x3b };
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
        }

        //读取532PD
        public void GetPDVoltage()
        {
            byte[] NumByte = new byte[] { 0x3a, PUBAddress, 0x02, 0x20, ChannelAddress, 0x00, 0x00, 0x00, 0x00, 0x3b };
            if (_Port != null)
            {
                _Port.Write(NumByte, 0, NumByte.Length);
            }
        }


        //写入内部PD
        public void SetInternalPDVoltage(double Value)
        {
            byte[] _tempDataField = new byte[4];
            int _tempNumber = (int)Math.Round((double)Value, 0);
            _tempDataField = BitConverter.GetBytes(_tempNumber);
            _DataField = BitConverter.ToInt32(_tempDataField, 0);
            byte[] NumByte = new byte[] { 0x3a, PUBAddress, 0x01, 0x20, ChannelAddress, 0x00, 0x00, 0x00, 0x00, 0x3b };
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
        }

        //读取内部PD
        public void GetInternalPDVoltage()
        {
            byte[] NumByte = new byte[] { 0x3a, PUBAddress, 0x02, 0x20, ChannelAddress, 0x00, 0x00, 0x00, 0x00, 0x3b };
            if (_Port != null)
            {
                _Port.Write(NumByte, 0, NumByte.Length);
            }
        }


        //写入激光功率对应的光电二极管电压
        public void SetCurrentPhotodiodeVoltageCorrespondingToLaserPowerValue(int CurrentMv, double power)
        {
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
                flag = 0x05;
            }
            else if (CurrentMv == 40)
            {
                flag = 0x06;
            }
            else if (CurrentMv == 45)
            {
                flag = 0x07;
            }
            else if (CurrentMv == 50)
            {
                flag = 0x08;
            }
            else if (CurrentMv == 55)
            {
                flag = 0x09;
            }
            else if (CurrentMv == 60)
            {
                flag = 0x0A;
            }
            else
            {
                return;
            }
            byte[] _tempDataField = new byte[4];
            int _tempNumber = (int)Math.Round((double)power, 0);
            _tempDataField = BitConverter.GetBytes(_tempNumber);
            byte[] NumByte = new byte[] { 0x3a, PUBAddress, 0x01, flag, ChannelAddress, 0x00, 0x00, 0x00, 0x00, 0x3b };
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
        }

        //读取激光功率对应的光电二极管电压
        public void GetCurrentPhotodiodeVoltageCorrespondingToLaserPowerValue(int CurrentMv)
        {
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
                flag = 0x05;
            }
            else if (CurrentMv == 40)
            {
                flag = 0x06;
            }
            else if (CurrentMv == 45)
            {
                flag = 0x07;
            }
            else if (CurrentMv == 50)
            {
                flag = 0x08;
            }
            else if (CurrentMv == 55)
            {
                flag = 0x09;
            }
            else if (CurrentMv == 60)
            {
                flag = 0x0A;
            }
            byte[] NumByte = new byte[] { 0x3a, PUBAddress, 0x02, flag, ChannelAddress, 0x00, 0x00, 0x00, 0x00, 0x3b };
            if (_Port != null)
            {
                _Port.Write(NumByte, 0, NumByte.Length);
            }
        }

        private byte[] StringToBytes(string s)
        {
            string[] str = s.Split(' ');
            int n = str.Length;

            byte[] cmdBytes = null;
            int p = 0;


            for (int k = 0; k < n; k++)
            {
                int sLen = str[k].Length;
                int bytesLen = sLen / 2;
                int position = 0;
                byte[] bytes = new byte[bytesLen];
                for (int i = 0; i < bytesLen; i++)
                {
                    string abyte = str[k].Substring(position, 2);
                    bytes[i] = Convert.ToByte(abyte, 16);
                    position += 2;
                }

                if (position >= 2)
                {
                    byte[] cmdBytes2 = new byte[p + bytesLen];
                    if (cmdBytes != null)
                    {
                        Array.Copy(cmdBytes, 0, cmdBytes2, 0, p);
                    }
                    Array.Copy(bytes, 0, cmdBytes2, p, bytesLen);
                    cmdBytes = cmdBytes2;
                    p += bytesLen;
                }
            }

            return cmdBytes;
        }

        private string getByte;
        public bool IsBusy { get => _IsBusy; set => _IsBusy = value; }
        public double LaserElectricValueCH1 { get => _LaserElectricValueCH1; set => _LaserElectricValueCH1 = value; }
        public double LaserElectricValueCH2 { get => _LaserElectricValueCH2; set => _LaserElectricValueCH2 = value; }
        public double LaserElectricValueCH3 { get => _LaserElectricValueCH3; set => _LaserElectricValueCH3 = value; }
        public double LaserElectricValueCH4 { get => _LaserElectricValueCH4; set => _LaserElectricValueCH4 = value; }
        public double LaserElectricValueCH5 { get => _LaserElectricValueCH5; set => _LaserElectricValueCH5 = value; }
        public double LaserElectricValueCH6 { get => _LaserElectricValueCH6; set => _LaserElectricValueCH6 = value; }
        public double LaserElectricValueCH7 { get => _LaserElectricValueCH7; set => _LaserElectricValueCH7 = value; }


        public double TecTemperValueCH1 { get => _TecTemperValueCH1; set => _TecTemperValueCH1 = value; }
        public double TecTemperValueCH2 { get => _TecTemperValueCH2; set => _TecTemperValueCH2 = value; }
        public double TecTemperValueCH3 { get => _TecTemperValueCH3; set => _TecTemperValueCH3 = value; }
        public double TecTemperValueCH4 { get => _TecTemperValueCH4; set => _TecTemperValueCH4 = value; }
        public double TecTemperValueCH5 { get => _TecTemperValueCH5; set => _TecTemperValueCH5 = value; }
        public double TecTemperValueCH6 { get => _TecTemperValueCH6; set => _TecTemperValueCH6 = value; }
        public double TecTemperValueCH7 { get => _TecTemperValueCH7; set => _TecTemperValueCH7 = value; }


        public double LaserPowerValueCH1 { get => _LaserPowerValueCH1; set => _LaserPowerValueCH1 = value; }
        public double LaserPowerValueCH2 { get => _LaserPowerValueCH2; set => _LaserPowerValueCH2 = value; }
        public double LaserPowerValueCH3 { get => _LaserPowerValueCH3; set => _LaserPowerValueCH3 = value; }
        public double LaserPowerValueCH4 { get => _LaserPowerValueCH4; set => _LaserPowerValueCH4 = value; }
        public double LaserPowerValueCH5 { get => _LaserPowerValueCH5; set => _LaserPowerValueCH5 = value; }
        public double LaserPowerValueCH6 { get => _LaserPowerValueCH6; set => _LaserPowerValueCH6 = value; }
        public double LaserPowerValueCH7 { get => _LaserPowerValueCH7; set => _LaserPowerValueCH7 = value; }


        public double PDVoltageCH1 { get => _PDVoltageCH1; set => _PDVoltageCH1 = value; }
        public double PDVoltageCH2 { get => _PDVoltageCH2; set => _PDVoltageCH2 = value; }
        public double PDVoltageCH3 { get => _PDVoltageCH3; set => _PDVoltageCH3 = value; }
        public double PDVoltageCH4 { get => _PDVoltageCH4; set => _PDVoltageCH4 = value; }
        public double PDVoltageCH5 { get => _PDVoltageCH5; set => _PDVoltageCH5 = value; }
        public double PDVoltageCH6 { get => _PDVoltageCH6; set => _PDVoltageCH6 = value; }
        public double PDVoltageCH7 { get => _PDVoltageCH7; set => _PDVoltageCH7 = value; }


        public double InternalPDVoltageCH1 { get => _InternalPDVoltageCH1; set => _InternalPDVoltageCH1 = value; }
        public double InternalPDVoltageCH2 { get => _InternalPDVoltageCH2; set => _InternalPDVoltageCH2 = value; }
        public double InternalPDVoltageCH3 { get => _InternalPDVoltageCH3; set => _InternalPDVoltageCH3 = value; }
        public double InternalPDVoltageCH4 { get => _InternalPDVoltageCH4; set => _InternalPDVoltageCH4 = value; }
        public double InternalPDVoltageCH5 { get => _InternalPDVoltageCH5; set => _InternalPDVoltageCH5 = value; }
        public double InternalPDVoltageCH6{ get => _InternalPDVoltageCH6; set => _InternalPDVoltageCH6 = value; }
        public double InternalPDVoltageCH7 { get => _InternalPDVoltageCH7; set => _InternalPDVoltageCH7 = value; }



        public double PhotodiodeVoltageCorrespondingToLaserPowerCH1 { get => _PhotodiodeVoltageCorrespondingToLaserPowerCH1; set => _PhotodiodeVoltageCorrespondingToLaserPowerCH1 = value; }
        public double PhotodiodeVoltageCorrespondingToLaserPowerCH2 { get => _PhotodiodeVoltageCorrespondingToLaserPowerCH2; set => _PhotodiodeVoltageCorrespondingToLaserPowerCH2 = value; }
        public double PhotodiodeVoltageCorrespondingToLaserPowerCH3 { get => _PhotodiodeVoltageCorrespondingToLaserPowerCH3; set => _PhotodiodeVoltageCorrespondingToLaserPowerCH3 = value; }
        public double PhotodiodeVoltageCorrespondingToLaserPowerCH4 { get => _PhotodiodeVoltageCorrespondingToLaserPowerCH4; set => _PhotodiodeVoltageCorrespondingToLaserPowerCH4 = value; }
        public double PhotodiodeVoltageCorrespondingToLaserPowerCH5 { get => _PhotodiodeVoltageCorrespondingToLaserPowerCH5; set => _PhotodiodeVoltageCorrespondingToLaserPowerCH5 = value; }
        public double PhotodiodeVoltageCorrespondingToLaserPowerCH6 { get => _PhotodiodeVoltageCorrespondingToLaserPowerCH6; set => _PhotodiodeVoltageCorrespondingToLaserPowerCH6= value; }
        public double PhotodiodeVoltageCorrespondingToLaserPowerCH7 { get => _PhotodiodeVoltageCorrespondingToLaserPowerCH7; set => _PhotodiodeVoltageCorrespondingToLaserPowerCH7 = value; }
        
        public string GetByte { get => getByte; set => getByte = value; }
        public bool DataComplete { get => _DataComplete; set => _DataComplete = value; }
        public byte PUBAddress { get => _PUBAddress; set => _PUBAddress = value; }
        public string ErrorCodeCH1 { get => errorCodeCH1; set => errorCodeCH1 = value; }
        public string ErrorCodeCH2 { get => errorCodeCH2; set => errorCodeCH2 = value; }
        public string ErrorCodeCH3 { get => errorCodeCH3; set => errorCodeCH3 = value; }
        public string ErrorCodeCH4 { get => errorCodeCH4; set => errorCodeCH4 = value; }
        public string ErrorCodeCH5 { get => errorCodeCH5; set => errorCodeCH5 = value; }
        public string ErrorCodeCH6 { get => errorCodeCH6; set => errorCodeCH6 = value; }
        public string ErrorCodeCH7 { get => errorCodeCH7; set => errorCodeCH7 = value; }


        public double TemperatureTimeCH1 { get => temperatureTimeCH1; set => temperatureTimeCH1 = value; }
        public double TemperatureTimeCH2 { get => temperatureTimeCH2; set => temperatureTimeCH2 = value; }
        public double TemperatureTimeCH3 { get => temperatureTimeCH3; set => temperatureTimeCH3 = value; }
        public double TemperatureTimeCH4 { get => temperatureTimeCH4; set => temperatureTimeCH4 = value; }
        public double TemperatureTimeCH5 { get => temperatureTimeCH5; set => temperatureTimeCH5 = value; }
        public double TemperatureTimeCH6 { get => temperatureTimeCH6; set => temperatureTimeCH6 = value; }
        public double TemperatureTimeCH7 { get => temperatureTimeCH7; set => temperatureTimeCH7 = value; }

        public byte ChannelAddress { get => _ChannelAddress; set => _ChannelAddress = value; }
    }
}
