using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;

namespace Azure.APDCalibrationBench
{
    public class UpgradePort : IDisposable
    {
        public delegate void DataProcess();
        public event DataProcess DataProcessUpdateCommOutput;

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
        private byte _PUBAddress = 0x01;
        private byte[] _ReResetByte = null;
        private byte[] _ReCleanByte = null;
        private byte[] _ReWriteFlashByte = null;
        private byte[] _ReCheckByte = null;
        public UpgradePort()
        {
            //_FunctionCode = FunctionCodeType.ReadReg;
            DataProcessThread = new Thread(ProcessData);
            DataProcessThread.IsBackground = true;
            DataProcessThread.Start();
            _IsConnected = false;
            _IsBusy = false;

        }
        public enum RegsAddressType
        {
            Reset=0xFC,//复位
            Clean=0xFF,//清除扇区
            WriteFlash=0xFE,//写入Flash
            Check=0xFD,//程序校验

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
            RegsAddressType Address = (RegsAddressType)ReceiveBytes[3];
            if (ReceiveBytes[1]== PUBAddress) {

                switch (Address)
                {
                    case RegsAddressType.Reset:
                        ReResetByte = ReceiveBytes;
                        DataProcessUpdateCommOutput();
                        break;
                    case RegsAddressType.WriteFlash:
                        ReWriteFlashByte = ReceiveBytes;
                        DataProcessUpdateCommOutput();
                        break;
                    case RegsAddressType.Clean:
                        ReCleanByte = ReceiveBytes;
                        DataProcessUpdateCommOutput();
                        break;
                    case RegsAddressType.Check:
                        ReCheckByte = ReceiveBytes;
                        DataProcessUpdateCommOutput();
                        break;
                }
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

        public void SetInstruct485(string Instruct)
        {
            byte[] NumByte = StringToBytes(Instruct);
            if (_Port != null)
            {
                _Port.Write(NumByte, 0, NumByte.Length);
            }
        }

        public void SetInstruct485(byte[] Instruct)
        {
            if (_Port != null)
            {
                _Port.Write(Instruct, 0, Instruct.Length);
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
        public void SetByteNull()
        {
            ReResetByte = null;
            ReWriteFlashByte = null;
            ReCheckByte = null;
            ReCleanByte = null;
        }
        private string getByte;
        public string GetByte { get => getByte; set => getByte = value; }
        public byte PUBAddress { get => _PUBAddress; set => _PUBAddress = value; }
        public byte[] ReResetByte { get => _ReResetByte; set => _ReResetByte = value; }
        public byte[] ReCleanByte { get => _ReCleanByte; set => _ReCleanByte = value; }
        public byte[] ReWriteFlashByte { get => _ReWriteFlashByte; set => _ReWriteFlashByte = value; }
        public byte[] ReCheckByte { get => _ReCheckByte; set => _ReCheckByte = value; }
    }
}
