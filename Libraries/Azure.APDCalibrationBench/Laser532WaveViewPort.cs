using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;

namespace Azure.APDCalibrationBench
{
    public class Laser532WaveViewPort : IDisposable
    {
        public delegate void CalibrationCommUpdateHandler();
        public event CalibrationCommUpdateHandler UpdateCommOutput;

       // private byte[] _PortReceiveBuf;
        private SerialPort _Port = null;
        private string[] _AvailablePorts;
        private bool _IsConnected = false;
        private bool _IsBusy = false;
        List<byte> PortListData = new List<byte>();
        private byte _FrameHeader = 0x3a;
        private byte _FrameEnd = 0x3b;
        private Thread DataProcessThread;
        private double _LaserPowerValue;
        private double _LaserElectricValue;
        private double _TecTemperValue;
        private FunctionCodeType _FunctionCode;
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
            LaserPowerValue = 0x03,//激光器电流值
            LaserLightPowerValue = 0x16,//当前激光光功率值
            TECActualTemperature = 0x0B,//TEC实际温度
        }
        public Laser532WaveViewPort()
        {
            _FunctionCode = FunctionCodeType.ReadReg;
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
                                if (index+1 == datalength)
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
        private void RunReceiveDataCallback(byte[] ReceiveBytes)
        {
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
                        case LaserModeRegsAddressType.TECActualTemperature:
                            _DataField = BitConverter.ToInt16(_tempArray, 0);
                            TecTemperValue = _DataField;
                            Console.WriteLine(TecTemperValue * 0.1);
                            if (TecTemperValue > 500)
                            {
                                TecTemperValue = 500;
                            }
                            break;
                        case LaserModeRegsAddressType.LaserPowerValue:
                            LaserElectricValue = _DataField * 0.01;
                            Console.WriteLine(LaserElectricValue);
                            break;
                        case LaserModeRegsAddressType.LaserLightPowerValue:
                            LaserPowerValue = _DataField;
                            Console.WriteLine(LaserPowerValue);
                            break;
                    }
                }
                //byte[] _laserPowerArray = new byte[3];
                //byte[] _laserElectricArray = new byte[3];
                //byte[] _tecTemperArray = new byte[3];
                //_laserPowerArray[0] = ReceiveBytes[1];
                //_laserPowerArray[1] = ReceiveBytes[2];
                //_laserPowerArray[2] = ReceiveBytes[3];
                //LaserPowerValue = System.Text.Encoding.Default.GetString(_laserPowerArray);
                //_laserElectricArray[0] = ReceiveBytes[5];
                //_laserElectricArray[1] = ReceiveBytes[6];
                //_laserElectricArray[2] = ReceiveBytes[7];
                //LaserElectricValue = System.Text.Encoding.Default.GetString(_laserElectricArray);
                //_tecTemperArray[0] = ReceiveBytes[9];
                //_tecTemperArray[1] = ReceiveBytes[10];
                //_tecTemperArray[2] = ReceiveBytes[11];
                //TecTemperValue = System.Text.Encoding.Default.GetString(_tecTemperArray);
                UpdateCommOutput();
            }
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
        public double LaserPowerValue { get => _LaserPowerValue; set => _LaserPowerValue = value; }
        public double LaserElectricValue { get => _LaserElectricValue; set => _LaserElectricValue = value; }
        public double TecTemperValue { get => _TecTemperValue; set => _TecTemperValue = value; }
        public bool IsBusy { get => _IsBusy; set => _IsBusy = value; }

        public void SetCurrentTEControlTemperatureValue(double Temp)
        {
            _IsBusy = true;
            byte[] _tempDataField = new byte[4];
            int _tempNumber = (int)Math.Round((double)Temp * 10, 0);
            _tempDataField = BitConverter.GetBytes(_tempNumber);
            byte[] NumByte = new byte[] { 0x3a, 0x01, 0x01, 0x0C, 0x00, 0x00, 0x00, 0x00, 0x00, 0x3b };
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
        private int _DataField = 0;
        public void SetCurrentLaserCurrentValue(double Value)
        {
            _IsBusy = true;
            byte[] _tempDataField = new byte[4];
            int _tempNumber = (int)Math.Round((double)Value * 100, 0);
            _tempDataField = BitConverter.GetBytes(_tempNumber);
            _DataField = BitConverter.ToInt32(_tempDataField, 0);
            byte[] NumByte = new byte[] { 0x3a, 0x01, 0x01, 0x03, 0x00, 0x00, 0x00, 0x00, 0x00, 0x3b };
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

        public void GetCurrentLaserCurrentValue()
        {
            _IsBusy = true;
            byte[] NumByte = new byte[] { 0x3a, 0x01, 0x02, 0x03, 0x00, 0x00, 0x00, 0x00, 0x00, 0x3b };
            if (_Port != null)
            {
                _Port.Write(NumByte, 0, NumByte.Length);
            }
            _IsBusy = false;
        }

        public void GetCurrentTECActualTemperatureValue()
        {
            _IsBusy = true;
            byte[] NumByte = new byte[] { 0x3a, 0x01, 0x02, 0x0B, 0x00, 0x00, 0x00, 0x00, 0x00, 0x3b };
            if (_Port != null)
            {
                _Port.Write(NumByte, 0, NumByte.Length);
            }
            _IsBusy = false;
        }
        public void GetCurrentLaserLightPowerValueValue()
        {
            _IsBusy = true;
            byte[] NumByte = new byte[] { 0x3a, 0x01, 0x02, 0x16, 0x00, 0x00, 0x00, 0x00, 0x00, 0x3b };
            if (_Port != null)
            {
                _Port.Write(NumByte, 0, NumByte.Length);
            }
            _IsBusy = false;
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

    }
}
