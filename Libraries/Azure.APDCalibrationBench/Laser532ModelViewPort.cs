using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;

namespace Azure.APDCalibrationBench
{
    public class Laser532ModelViewPort : IDisposable
    {
        public delegate void LaserPowerCalibrationCommUpdateHandler();
        public event LaserPowerCalibrationCommUpdateHandler LaserPowerUpdateCommOutput;

        public delegate void PDCalibrationCommUpdateHandler();
        public event PDCalibrationCommUpdateHandler PDUpdateCommOutput;

        public delegate void InteriorPDCalibrationCommUpdateHandler();
        public event InteriorPDCalibrationCommUpdateHandler InteriorPDUpdateCommOutput;

        public delegate void LaserOpticalPowerCalibrationCommUpdateHandler();
        public event LaserOpticalPowerCalibrationCommUpdateHandler LaserOpticalPowerUpdateCommOutput;


        public delegate void LaserElectricCalibrationCommUpdateHandler();
        public event LaserElectricCalibrationCommUpdateHandler LaserElectricUpdateCommOutput;

        public delegate void PhotodiodeVoltageCorrespondingToLaserPowerCalibrationCommUpdateHandler();
        public event PhotodiodeVoltageCorrespondingToLaserPowerCalibrationCommUpdateHandler PhotodiodeVoltageCorrespondingToLaserPowerUpdateCommOutput;

        public delegate void Instruct485CalibrationCommUpdateHandler();
        public event Instruct485CalibrationCommUpdateHandler Instruct485UpdateCommOutput;

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
        private double _LaserPowerValue;
        private double _PDVoltage;
        private double _InternalPDVoltage;
        private double _TecTemperValue;
        private double _PhotodiodeVoltageCorrespondingToLaserPower;
        private double _LaserElectricValue;
        private bool _DataComplete = false;
        private byte _PUBAddress = 0x01;
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
            LaserCurrentValue =0x03,//激光器电流值
            PhotodiodeVoltageCorrespondingToLaserPower5  = 0x1A,//5mW激光功率对应的光电二极管电压
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
            InternalPDVoltage = 0x08,//内部PD电压
        }
        public Laser532ModelViewPort()
        {
            //_FunctionCode = FunctionCodeType.ReadReg;
            DataProcessThread = new Thread(ProcessData);
            DataProcessThread.IsBackground = true;
            DataProcessThread.Start();
            _IsConnected = false;
            _IsBusy = false;

        }
        public void SearchPort(string COM,int BAUD)
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
            FunctionCodeType readType= (FunctionCodeType)ReceiveBytes[2];
            LaserModeRegsAddressType Address = (LaserModeRegsAddressType)ReceiveBytes[3];
            LaserInternalModeRegsAddressType InternalAddress = (LaserInternalModeRegsAddressType)ReceiveBytes[4];
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
                            TemperatureTime = _DataField * 0.05;
                            LaserPowerUpdateCommOutput();
                            DataComplete = true;
                            break;
                        case LaserModeRegsAddressType.TECActualTemperature:
                            _DataField = BitConverter.ToInt16(_tempArray, 0);
                            TecTemperValue = _DataField * 100;
                            if (TecTemperValue < 0)
                                TecTemperValue = 0;
                            ErrorCode = ReceiveBytes[6].ToString();
                            LaserPowerUpdateCommOutput();
                            break;
                        case LaserModeRegsAddressType.LaserLightPowerValue:
                            LaserPowerValue = _DataField * 100;
                            LaserOpticalPowerUpdateCommOutput();
                            break;
                        case LaserModeRegsAddressType.LaserCurrentValue:
                            LaserElectricValue = _DataField;
                            LaserElectricUpdateCommOutput();
                            break;
                        case LaserModeRegsAddressType.PDVoltage:
                            if (InternalAddress == LaserInternalModeRegsAddressType.InternalPDVoltage)
                            {
                                InternalPDVoltage = _DataField;
                                InteriorPDUpdateCommOutput();
                                break;
                            }
                            PDVoltage = _DataField;
                            PDUpdateCommOutput();
                            break;
                        case LaserModeRegsAddressType.PhotodiodeVoltageCorrespondingToLaserPower5:
                            PhotodiodeVoltageCorrespondingToLaserPower = _DataField;
                            PhotodiodeVoltageCorrespondingToLaserPowerUpdateCommOutput();
                            break;
                        case LaserModeRegsAddressType.PhotodiodeVoltageCorrespondingToLaserPower10:
                            PhotodiodeVoltageCorrespondingToLaserPower = _DataField;
                            PhotodiodeVoltageCorrespondingToLaserPowerUpdateCommOutput();
                            break;
                        case LaserModeRegsAddressType.PhotodiodeVoltageCorrespondingToLaserPower15:
                            PhotodiodeVoltageCorrespondingToLaserPower = _DataField;
                            PhotodiodeVoltageCorrespondingToLaserPowerUpdateCommOutput();
                            break;
                        case LaserModeRegsAddressType.PhotodiodeVoltageCorrespondingToLaserPower20:
                            PhotodiodeVoltageCorrespondingToLaserPower = _DataField;
                            PhotodiodeVoltageCorrespondingToLaserPowerUpdateCommOutput();
                            break;
                        case LaserModeRegsAddressType.PhotodiodeVoltageCorrespondingToLaserPower25:
                            PhotodiodeVoltageCorrespondingToLaserPower = _DataField;
                            PhotodiodeVoltageCorrespondingToLaserPowerUpdateCommOutput();
                            break;
                        case LaserModeRegsAddressType.PhotodiodeVoltageCorrespondingToLaserPower30:
                            PhotodiodeVoltageCorrespondingToLaserPower = _DataField;
                            PhotodiodeVoltageCorrespondingToLaserPowerUpdateCommOutput();
                            break;
                        case LaserModeRegsAddressType.PhotodiodeVoltageCorrespondingToLaserPower35:
                            PhotodiodeVoltageCorrespondingToLaserPower = _DataField;
                            PhotodiodeVoltageCorrespondingToLaserPowerUpdateCommOutput();
                            break;
                        case LaserModeRegsAddressType.PhotodiodeVoltageCorrespondingToLaserPower40:
                            PhotodiodeVoltageCorrespondingToLaserPower = _DataField;
                            PhotodiodeVoltageCorrespondingToLaserPowerUpdateCommOutput();
                            break;
                        case LaserModeRegsAddressType.PhotodiodeVoltageCorrespondingToLaserPower45:
                            PhotodiodeVoltageCorrespondingToLaserPower = _DataField;
                            PhotodiodeVoltageCorrespondingToLaserPowerUpdateCommOutput();
                            break;
                        case LaserModeRegsAddressType.PhotodiodeVoltageCorrespondingToLaserPower50:
                            PhotodiodeVoltageCorrespondingToLaserPower = _DataField;
                            PhotodiodeVoltageCorrespondingToLaserPowerUpdateCommOutput();
                            break;
                        case LaserModeRegsAddressType.PhotodiodeVoltageCorrespondingToLaserPower55:
                            PhotodiodeVoltageCorrespondingToLaserPower = _DataField;
                            PhotodiodeVoltageCorrespondingToLaserPowerUpdateCommOutput();
                            break;
                        case LaserModeRegsAddressType.PhotodiodeVoltageCorrespondingToLaserPower60:
                            PhotodiodeVoltageCorrespondingToLaserPower = _DataField;
                            PhotodiodeVoltageCorrespondingToLaserPowerUpdateCommOutput();
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
            Instruct485UpdateCommOutput();
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
                    strB.Append(bytes[i].ToString("X2")+" ");
                }
                hexString = strB.ToString();
            }
            return hexString;
        }

        //读激光器温度
        public void GetCurrentTECActualTemperatureValue()
        {
            byte[] NumByte = new byte[] { 0x3a, PUBAddress, 0x02, 0x0B, 0x00, 0x00, 0x00, 0x00, 0x00, 0x3b };
            if (_Port != null)
            {
                _Port.Write(NumByte, 0, NumByte.Length);
            }
        }

        public void SetInstruct485(string Instruct)
        {
            byte[] NumByte = StringToBytes(Instruct);
            if (_Port != null)
            {
                _Port.Write(NumByte, 0, NumByte.Length);
            }
        }

        //写入激光器电流值
        public void SetCurrentLaserCurrentValue(double Value)
        {
            byte[] _tempDataField = new byte[4];
            int _tempNumber = (int)Math.Round((double)Value, 0);
            _tempDataField = BitConverter.GetBytes(_tempNumber);
            _DataField = BitConverter.ToInt32(_tempDataField, 0);
            byte[] NumByte = new byte[] { 0x3a, PUBAddress, 0x01, 0x03, 0x00, 0x00, 0x00, 0x00, 0x00, 0x3b };
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
            byte[] NumByte = new byte[] { 0x3a, PUBAddress, 0x02, 0x03, 0x00, 0x00, 0x00, 0x00, 0x00, 0x3b };
            if (_Port != null)
            {
                _Port.Write(NumByte, 0, NumByte.Length);
            }
        }

        //读取激光器光功率
        public void GetCurrentLaserLightPowerValueValue()
        {
            byte[] NumByte = new byte[] { 0x3a, PUBAddress, 0x02, 0x16, 0x00, 0x00, 0x00, 0x00, 0x00, 0x3b };
            if (_Port != null)
            {
                _Port.Write(NumByte, 0, NumByte.Length);
            }
        }

        //读取散热器温度
        public void GetCurrentRadiatorValueValue()
        {
            DataComplete = false;
            byte[] NumByte = new byte[] { 0x3a, PUBAddress, 0x02, 0x14, 0x00, 0x00, 0x00, 0x00, 0x00, 0x3b };
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
            byte[] NumByte = new byte[] { 0x3a, PUBAddress, 0x01, 0x11, 0x00, 0x00, 0x00, 0x00, 0x00, 0x3b };
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
            byte[] NumByte = new byte[] { 0x3a, PUBAddress, 0x01, 0x03, 0x00, 0x00, 0x00, 0x00, 0x00, 0x3b };
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
            byte[] NumByte = new byte[] { 0x3a, PUBAddress, 0x02, 0x03, 0x00, 0x00, 0x00, 0x00, 0x00, 0x3b };
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
            byte[] NumByte = new byte[] { 0x3a, PUBAddress, 0x01, 0x20, 0x08, 0x00, 0x00, 0x00, 0x00, 0x3b };
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
            byte[] NumByte = new byte[] { 0x3a, PUBAddress, 0x02, 0x20, 0x08, 0x00, 0x00, 0x00, 0x00, 0x3b };
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
            byte[] NumByte = new byte[] { 0x3a, PUBAddress, 0x01, flag, 0x00, 0x00, 0x00, 0x00, 0x00, 0x3b };
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
            byte[] NumByte = new byte[] { 0x3a, PUBAddress, 0x02, flag, 0x00, 0x00, 0x00, 0x00, 0x00, 0x3b };
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
        public double LaserElectricValue { get => _LaserElectricValue; set => _LaserElectricValue = value; }
        public double TecTemperValue { get => _TecTemperValue; set => _TecTemperValue = value; }
        private double temperatureTime;
        private string errorCode;
        public double LaserPowerValue { get => _LaserPowerValue; set => _LaserPowerValue = value; }
        public double PDVoltage { get => _PDVoltage; set => _PDVoltage = value; }
        public double InternalPDVoltage { get => _InternalPDVoltage; set => _InternalPDVoltage = value; }
        public double PhotodiodeVoltageCorrespondingToLaserPower { get => _PhotodiodeVoltageCorrespondingToLaserPower; set => _PhotodiodeVoltageCorrespondingToLaserPower = value; }
        public string GetByte { get => getByte; set => getByte = value; }
        public bool DataComplete { get => _DataComplete; set => _DataComplete = value; }
        public byte PUBAddress { get => _PUBAddress; set => _PUBAddress = value; }
        public string ErrorCode { get => errorCode; set => errorCode = value; }
        public double TemperatureTime { get => temperatureTime; set => temperatureTime = value; }
    }
}
