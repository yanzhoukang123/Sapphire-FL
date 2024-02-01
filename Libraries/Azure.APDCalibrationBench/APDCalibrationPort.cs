using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO.Ports;
using System.Threading;
using System.Windows;

namespace Azure.APDCalibrationBench
{
    public class APDCalibrationPort : IDisposable
    {
        #region delegates statement
        public delegate void CommPortErrorHandler(string error);
        public event CommPortErrorHandler CommPortError;
        #endregion

        #region Private Data...
        private SerialPort _Port = null;
        private string[] _AvailablePorts;
        public IList<string> PortList = new List<string>();         // 可用串口集合
        List<byte> PortListData = new List<byte>();
        private byte _FrameHeader = 0x3a;
        private byte _FrameEnd = 0x3b;
        private Thread DataProcessThread;
        public string ApdIvTempValue;
        private byte[] _PortReceiveBuf;
        private byte[] _PortSendBuf;
        private int _ReceiveIndex = 0;
        private bool _IsConnected = false;
        private bool _IsAPDCHAAlive = false;
        private bool _IsAPDCHBAlive = false;
        private bool _IsBenchAlive = false;

        private System.Timers.Timer _ReadOutputTimer = null;

        private APDCommProtocol _APDComm = null;
        private APDCommProtocol _BenchComm = null;
        private APDCommProtocol _LaserComm = null;

        private int _CommTimeOutCount = 0;
        private double? _Caltemperature = 0;
        private double? _emperature500 = 0;
        #endregion


        #region Construction
        public APDCalibrationPort()
        {
            DataProcessThread = new Thread(ProcessData);
            DataProcessThread.IsBackground = true;
            DataProcessThread.Start();
            _PortReceiveBuf = new byte[128];
            _APDComm = new APDCommProtocol(APDCommProtocol.DeviceAddressType.APDModuleA);
            //_APDComm.UpdateCommOutput += _APDComm_UpdateCommOutput;
            _BenchComm = new APDCommProtocol(APDCommProtocol.DeviceAddressType.TestBench);
            //_BenchComm.UpdateCommOutput += _BenchComm_UpdateCommOutput;
            _LaserComm = new APDCommProtocol(APDCommProtocol.DeviceAddressType.LaserModuleA);
        }
        #endregion

        #region Public Functions
        public void SearchPort()
        {
            _AvailablePorts = SerialPort.GetPortNames();
            for (int i = 0; i < _AvailablePorts.Length; i++)
            {
                _Port = new SerialPort(_AvailablePorts[i], 9600, Parity.None, 8, StopBits.One);
                try
                {
                    _Port.DataReceived += _Port_DataReceived;
                    _Port.Open();
                    _Port.ReceivedBytesThreshold = 10;
                    _PortSendBuf = _BenchComm.ReadAPDValue(APDCommProtocol.APDChannelType.CHA);
                    _Port.Write(_PortSendBuf, 0, _PortSendBuf.Length);
                    APDCommProtocol.CommStatus = APDCommProtocol.CommStatusType.Hearing;
                    while (APDCommProtocol.CommStatus == APDCommProtocol.CommStatusType.Hearing)
                    {
                        System.Threading.Thread.Sleep(10);
                    }
                    if (APDCommProtocol.CommStatus == APDCommProtocol.CommStatusType.TimeOut)
                    {
                        _Port.Close();
                        _Port.DataReceived -= _Port_DataReceived;
                        _ReceiveIndex = 0;
                        Array.Clear(_PortReceiveBuf, 0, _PortReceiveBuf.Length);
                    }
                    else
                    {
                        _IsConnected = true;
                        _IsBenchAlive = true;

                        _APDComm.DeviceAddress = APDCommProtocol.DeviceAddressType.APDModuleA;
                        _LaserComm.DeviceAddress = APDCommProtocol.DeviceAddressType.LaserModuleA;
                        _PortSendBuf = _APDComm.VisitAPDGain(APDCommProtocol.APDChannelType.CHA, APDCommProtocol.FunctionCodeType.ReadReg);
                        _Port.Write(_PortSendBuf, 0, _PortSendBuf.Length);
                        APDCommProtocol.CommStatus = APDCommProtocol.CommStatusType.Hearing;
                        while (APDCommProtocol.CommStatus == APDCommProtocol.CommStatusType.Hearing)
                        {
                            System.Threading.Thread.Sleep(10);
                        }
                        if (APDCommProtocol.CommStatus == APDCommProtocol.CommStatusType.TimeOut)
                        {
                            _ReceiveIndex = 0;
                            Array.Clear(_PortReceiveBuf, 0, _PortReceiveBuf.Length);
                        }
                        else
                        {
                            _IsAPDCHAAlive = true;
                            _IsConnected = true;


                        }

                        if (_IsConnected)
                        {
                            if (_ReadOutputTimer == null)
                            {
                                APDCommProtocol.CommTimeOut += APDCommProtocol_CommTimeOut;

                                _ReadOutputTimer = new System.Timers.Timer();
                                _ReadOutputTimer.Interval = 2000;
                                _ReadOutputTimer.Elapsed += _ReadOutputTimer_Elapsed;
                                _ReadOutputTimer.Start();
                                break;
                            }
                        }
                    }
                }
                catch
                {

                }
            }
        }

        public void SearchApdIvTempPort(byte[] NumByte)
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
                        _Port.DataReceived += _Port_APDIvTempDataReceived;
                        _PortSendBuf = NumByte;
                        ApdIvTempValue = "NotFullFilled";
                        _Port.Write(_PortSendBuf, 0, _PortSendBuf.Length);
                        _IsConnected = true;
                    }
                    catch (Exception exception)
                    {

                    }
                }
            }
        }
        public void ApdIvTempProtSend(byte[] NumByte)
        {
            ApdIvTempValue = "NotFullFilled";
            _PortSendBuf = NumByte;
            _Port.Write(_PortSendBuf, 0, _PortSendBuf.Length);
        }



        //连接中断
        private void APDCommProtocol_CommTimeOut(APDCommProtocol.CommErrorInfo errorInfo)
        {
            //APDCommProtocol.CommStatus = APDCommProtocol.CommStatusType.Idle;
            if (APDCommProtocol.CommStatus == APDCommProtocol.CommStatusType.TimeOut)
            {
                _CommTimeOutCount++;
                if (_CommTimeOutCount <= 3)     // trigger re-send, 3 times maximum
                {
                    _PortSendBuf = APDCommProtocol.ResumeErrorFrame();
                    // _Port.Write(_PortSendBuf, 0, _PortSendBuf.Length);
                    APDCommProtocol.CommStatus = APDCommProtocol.CommStatusType.Hearing;
                    Thread.Sleep(500);
                }
            }
        }


        public void Dispose()
        {
            _IsAPDCHAAlive = false;
            _IsAPDCHBAlive = false;
            _IsBenchAlive = false;
            _IsConnected = false;
            if (_ReadOutputTimer != null)
            {
                _ReadOutputTimer.Stop();
                _ReadOutputTimer.Elapsed -= _ReadOutputTimer_Elapsed;
                _ReadOutputTimer = null;
                APDCommProtocol.CommTimeOut -= APDCommProtocol_CommTimeOut;
            }

            if (_Port != null)
            {
                _Port.DataReceived -= _Port_DataReceived;
                _Port.Close();
                _Port = null;
            }

        }
        public void TempIvDispose()
        {
            if (_Port != null)
            {
                _Port.DataReceived -= _Port_APDIvTempDataReceived;
                _Port.Close();
                _Port = null;
            }

        }
        #endregion
        #region Private Functions
        /// <summary>
        /// Read PGA, APD Gain, High Voltage, Temperature and Output Voltage every two second
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private byte[] NumByte = new byte[] { 0x3a, 0x01, 0x02, 0x0b, 0x00, 0x00, 0x00, 0x00, 0x00, 0x3b };
        private byte[] GetTemperatureAtCalibration = new byte[] { 0x3a, 0x02, 0x02, 0x07, 0x00, 0x00, 0x00, 0x00, 0x00, 0x3b };
        private byte[] LaserPowerNumByte = new byte[] { 0x3a, 0x01, 0x02, 0x03, 0x00, 0x00, 0x00, 0x00, 0x00, 0x3b };
        private byte[] _IVType = new byte[] { 0x3a, 0x02, 0x02, 0x10, 0x00, 0x00, 0x00, 0x00, 0x00, 0x3b };
        private void _ReadOutputTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                if (_IsBenchAlive)
                {
                    if (APDCommProtocol.CommStatus == APDCommProtocol.CommStatusType.Idle && _IsConnected)
                    {
                        APDCommProtocol.CommStatus = APDCommProtocol.CommStatusType.Hearing;
                        if (_APDComm.DeviceAddress == APDCommProtocol.DeviceAddressType.APDModuleA)
                        {
                            _PortSendBuf = _BenchComm.ReadAPDValue(APDCommProtocol.APDChannelType.CHA);
                        }
                        _Port.Write(_PortSendBuf, 0, _PortSendBuf.Length);
                        for (int i = 0; i < 10; i++)
                        {
                            Thread.Sleep(10);
                            if (APDCommProtocol.CommStatus == APDCommProtocol.CommStatusType.Idle)
                            {
                                break;
                            }
                        }
                    }
                    else if (APDCommProtocol.CommStatus == APDCommProtocol.CommStatusType.TimeOut)
                    {
                        return;
                    }
                    Thread.Sleep(500);
                    if (APDCommProtocol.CommStatus == APDCommProtocol.CommStatusType.Idle && _IsConnected)
                    {
                        APDCommProtocol.CommStatus = APDCommProtocol.CommStatusType.Hearing;
                        if (_APDComm.DeviceAddress == APDCommProtocol.DeviceAddressType.APDModuleA)
                        {
                            _PortSendBuf = NumByte;
                        }
                        _Port.Write(_PortSendBuf, 0, _PortSendBuf.Length);
                        for (int i = 0; i < 10; i++)
                        {
                            Thread.Sleep(10);
                            if (APDCommProtocol.CommStatus == APDCommProtocol.CommStatusType.Idle)
                            {
                                break;
                            }
                        }
                    }
                    else if (APDCommProtocol.CommStatus == APDCommProtocol.CommStatusType.TimeOut)
                    {
                        return;
                    }
                    //Thread.Sleep(500);
                    //if (APDCommProtocol.CommStatus == APDCommProtocol.CommStatusType.Idle && _IsConnected)
                    //{
                    //    APDCommProtocol.CommStatus = APDCommProtocol.CommStatusType.Hearing;
                    //    if (_LaserComm.DeviceAddress == APDCommProtocol.DeviceAddressType.LaserModuleA)
                    //    {
                    //        _PortSendBuf = LaserPowerNumByte;
                    //    }
                    //    _Port.Write(_PortSendBuf, 0, _PortSendBuf.Length);
                    //    for (int i = 0; i < 10; i++)
                    //    {
                    //        Thread.Sleep(10);
                    //        if (APDCommProtocol.CommStatus == APDCommProtocol.CommStatusType.Idle)
                    //        {
                    //            break;
                    //        }
                    //    }
                    //}
                    //else if (APDCommProtocol.CommStatus == APDCommProtocol.CommStatusType.TimeOut)
                    //{
                    //    return;
                    //}
                    Thread.Sleep(500);
                    if (APDCommProtocol.CommStatus == APDCommProtocol.CommStatusType.Idle && _IsConnected)
                    {
                        APDCommProtocol.CommStatus = APDCommProtocol.CommStatusType.Hearing;
                        if (_LaserComm.DeviceAddress == APDCommProtocol.DeviceAddressType.LaserModuleA)
                        {
                            _PortSendBuf = GetTemperatureAtCalibration;
                        }
                        _Port.Write(_PortSendBuf, 0, _PortSendBuf.Length);
                        for (int i = 0; i < 10; i++)
                        {
                            Thread.Sleep(10);
                            if (APDCommProtocol.CommStatus == APDCommProtocol.CommStatusType.Idle)
                            {
                                break;
                            }
                        }
                    }
                    else if (APDCommProtocol.CommStatus == APDCommProtocol.CommStatusType.TimeOut)
                    {
                        return;
                    }
                    //Thread.Sleep(500);
                    //if (APDCommProtocol.CommStatus == APDCommProtocol.CommStatusType.Idle && _IsConnected)
                    //{
                    //    APDCommProtocol.CommStatus = APDCommProtocol.CommStatusType.Hearing;
                    //    if (_LaserComm.DeviceAddress == APDCommProtocol.DeviceAddressType.LaserModuleA)
                    //    {
                    //        _PortSendBuf = _IVType;
                    //    }
                    //    _Port.Write(_PortSendBuf, 0, _PortSendBuf.Length);
                    //    for (int i = 0; i < 10; i++)
                    //    {
                    //        Thread.Sleep(10);
                    //        if (APDCommProtocol.CommStatus == APDCommProtocol.CommStatusType.Idle)
                    //        {
                    //            break;
                    //        }
                    //    }
                    //}
                    //else if (APDCommProtocol.CommStatus == APDCommProtocol.CommStatusType.TimeOut)
                    //{
                    //    return;
                    //}
                }

                for (int i = 0; i < 4; i++)
                {
                    if (_IsAPDCHAAlive)
                    {
                        Thread.Sleep(500);
                        if (APDCommProtocol.CommStatus == APDCommProtocol.CommStatusType.Idle && _IsConnected)
                        {
                            APDCommProtocol.CommStatus = APDCommProtocol.CommStatusType.Hearing;
                            switch (i)
                            {
                                case 0:
                                    _PortSendBuf = _APDComm.VisitAPDGain(APDCommProtocol.APDChannelType.CHA, APDCommProtocol.FunctionCodeType.ReadReg);
                                    break;
                                case 1:
                                    _PortSendBuf = _APDComm.VisitPGA(APDCommProtocol.APDChannelType.CHA, APDCommProtocol.FunctionCodeType.ReadReg);
                                    break;
                                case 2:
                                    _PortSendBuf = _APDComm.ReadHighVoltage(APDCommProtocol.APDChannelType.CHA);
                                    break;
                                case 3:
                                    _PortSendBuf = _APDComm.ReadAPDTemperature(APDCommProtocol.APDChannelType.CHA);
                                    break;
                            }
                            _Port.Write(_PortSendBuf, 0, _PortSendBuf.Length);
                            for (int j = 0; j < 10; j++)
                            {
                                Thread.Sleep(10);
                                if (APDCommProtocol.CommStatus == APDCommProtocol.CommStatusType.Idle)
                                {
                                    break;
                                }
                            }
                        }
                        else if (APDCommProtocol.CommStatus == APDCommProtocol.CommStatusType.TimeOut)
                        {
                            return;
                        }
                    }

                }
            }
            catch (Exception exception)
            {
                if (_Port != null)
                {
                    _IsConnected = false;
                    CommPortError(exception.ToString());
                }
            }
        }

        private void _Port_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            int _receiveBufLth = _Port.BytesToRead;
            _PortReceiveBuf = new byte[_receiveBufLth];
            _Port.Read(_PortReceiveBuf, 0, _receiveBufLth);
            _ReceiveIndex += _receiveBufLth;
            PortListData.AddRange(_PortReceiveBuf);
           
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
                                    string _responseReceived = string.Empty;
                                    if (APDCommProtocol.CommStatus == APDCommProtocol.CommStatusType.Hearing)
                                    {
                                        _responseReceived = _APDComm.ResponseDetect(ReceiveBytes, 10);
                                        if (_responseReceived == "FrameDetected")
                                        {
                                            _ReceiveIndex = 0;
                                            Array.Clear(_PortReceiveBuf, 0, _PortReceiveBuf.Length);
                                            _CommTimeOutCount = 0;
                                        }
                                        else
                                        {
                                            _responseReceived = _BenchComm.ResponseDetect(ReceiveBytes, 10);
                                            if (_responseReceived == "FrameDetected")
                                            {
                                                _ReceiveIndex = 0;
                                                Array.Clear(_PortReceiveBuf, 0, _PortReceiveBuf.Length);
                                                _CommTimeOutCount = 0;
                                            }
                                            else
                                            {
                                                _responseReceived = _LaserComm.ResponseDetect(ReceiveBytes, 10);
                                                if (_responseReceived == "FrameDetected")
                                                {
                                                    _ReceiveIndex = 0;
                                                    Array.Clear(_PortReceiveBuf, 0, _PortReceiveBuf.Length);
                                                    _CommTimeOutCount = 0;
                                                }
                                            }
                                        }
                                    }
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
                catch(Exception ex)
                {
                    MessageBox.Show("数据模块失败!",ex.Message);
                    index = 0;
                }

            }
        }
        private void _ReadAPDIvTempDataOutputTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {

        }
        private void _Port_APDIvTempDataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            Thread.Sleep(100);
            int _receiveBufLth = _Port.BytesToRead;
            _PortReceiveBuf = new byte[_receiveBufLth];
            _Port.Read(_PortReceiveBuf, 0, _receiveBufLth);
            //_ReceiveIndex += _receiveBufLth;
            ApdIvTempValue = _APDComm.ApdIvTempData(_PortReceiveBuf, _receiveBufLth);
            //_ReceiveIndex = 0;
            //Array.Clear(_PortReceiveBuf, 0, _PortReceiveBuf.Length);
        }
        private bool _IsPortAvailable()
        {
            //int i = 0;
            //while (APDCommProtocol.CommStatus != APDCommProtocol.CommStatusType.Idle)
            //{
            //    Thread.Sleep(100);
            //    i++;
            //    if (i >= 10)
            //    {
            //        return false;
            //    }
            //}
            return true;
        }
        #endregion
        #region Public Properties
        public string[] AvailablePorts
        {
            get { return _AvailablePorts; }
        }
        public bool IsConnected
        {
            get
            {
                return _IsConnected;
            }
        }
        public bool IsAPDCHAAlive
        {
            get
            {
                return _IsAPDCHAAlive;
            }
        }

        public bool IsBenchAlive
        {
            get
            {
                return _IsBenchAlive;
            }
        }
        public APDCommProtocol APDComm
        {
            get
            {
                return _APDComm;
            }
        }
        public APDCommProtocol BenchComm
        {
            get
            {
                return _BenchComm;
            }
        }
        public APDCommProtocol LaserComm
        {
            get
            {
                return _LaserComm;
            }
        }
        public SerialPort Port
        {
            get
            {
                return _Port;
            }
        }
        public int? PGACHA
        {
            get
            {
                return _APDComm.CommOutputChA.CurrentPGA;
            }
            set
            {
                if (value != null)
                {
                    if (_IsPortAvailable() == false)
                    {
                        return;
                    }
                    _PortSendBuf = _APDComm.VisitPGA(APDCommProtocol.APDChannelType.CHA, APDCommProtocol.FunctionCodeType.WriteReg, value);
                    _Port.Write(_PortSendBuf, 0, _PortSendBuf.Length);
                    APDCommProtocol.CommStatus = APDCommProtocol.CommStatusType.Hearing;
                    Thread.Sleep(500);
                }
            }
        }

        public double? APDOutputCHA
        {
            get
            {
                return _BenchComm.CommOutputChA.APDOutput;
            }
        }

        public double? APDTemperatureCHA
        {
            get
            {
                return _APDComm.CommOutputChA.APDTemperature;
            }
        }

        public double? TemperatureAtCalibrationCHA
        {
            set
            {
                if (value != null)
                {
                    if (_IsPortAvailable() == false)
                    {
                        return;
                    }
                    _PortSendBuf = _APDComm.VisitTemperatureAtCalibration(APDCommProtocol.APDChannelType.CHA, APDCommProtocol.FunctionCodeType.WriteReg, value);
                    _Port.Write(_PortSendBuf, 0, _PortSendBuf.Length);
                    APDCommProtocol.CommStatus = APDCommProtocol.CommStatusType.Hearing;
                    Thread.Sleep(500);
                    //byte[] NumByte = new byte[] { 0x3a, 0x02, 0x02, 0x07, 0x00, 0x00, 0x00, 0x00, 0x00, 0x3b };
                    //_Port.Write(NumByte, 0, NumByte.Length);

                }
            }
            get
            {
                return _APDComm.CommOutputChA.TemperatureAtCalibrationCHA;
            }
        }

        public int? APDGainCHA
        {
            get
            {
                return _APDComm.CommOutputChA.CurrentAPDGain;
            }
            set
            {
                if (value != null)
                {
                    if (_IsPortAvailable() == false)
                    {
                        return;
                    }
                    _PortSendBuf = _APDComm.VisitAPDGain(APDCommProtocol.APDChannelType.CHA, APDCommProtocol.FunctionCodeType.WriteReg, value);
                    _Port.Write(_PortSendBuf, 0, _PortSendBuf.Length);
                    APDCommProtocol.CommStatus = APDCommProtocol.CommStatusType.Hearing;
                    Thread.Sleep(500);
                }
            }
        }

        public int? PMTGainCHA
        {
            get
            {
                return _APDComm.CommOutputChA.CurrentPMTGain;
            }
            set
            {
                if (value != null)
                {
                    if (_IsPortAvailable() == false)
                    {
                        return;
                    }
                    int _tempNumber = (int)Math.Round((double)value, 0);
                    byte[] _tempDataField = new byte[4];
                    _tempDataField = BitConverter.GetBytes(_tempNumber);
                    byte[] NumByte = new byte[] { 0x3a, 0x02, 0x01, 0x10, 0x00, _tempDataField[3], _tempDataField[2], _tempDataField[1], _tempDataField[0], 0x3b };
                    _PortSendBuf = NumByte;
                    _Port.Write(_PortSendBuf, 0, _PortSendBuf.Length);
                    APDCommProtocol.CommStatus = APDCommProtocol.CommStatusType.Hearing;
                    Thread.Sleep(500);
                    NumByte = new byte[] { 0x3a, 0x02, 0x02, 0x10, 0x00, 0x00, 0x00, 0x00, 0x00, 0x3b };
                    _Port.Write(NumByte, 0, NumByte.Length);
                    APDCommProtocol.CommStatus = APDCommProtocol.CommStatusType.Hearing;
                    Thread.Sleep(500);
                }
            }
        }
        public int? CurrentIVType
        {
            get
            {
                return _APDComm.CommOutputChA.CurrentIVType;
            }
        }

        public void  getIvType()
        {
            byte[] NumByte = new byte[] { 0x3a, 0x02, 0x02, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x3b };
            _Port.Write(NumByte, 0, NumByte.Length);
            APDCommProtocol.CommStatus = APDCommProtocol.CommStatusType.Hearing;
            Thread.Sleep(500);
           NumByte = new byte[] { 0x3a, 0x02, 0x02, 0x10, 0x00, 0x00, 0x00, 0x00, 0x00, 0x3b };
            _Port.Write(NumByte, 0, NumByte.Length);
            APDCommProtocol.CommStatus = APDCommProtocol.CommStatusType.Hearing;
            Thread.Sleep(500);
        }


        public double? CalibrationVoltCHA
        {
            set
            {
                if (value != null)
                {
                    if (_IsPortAvailable() == false)
                    {
                        return;
                    }
                    //int _tempNumber = (int)Math.Round((double)value * 100, 0);
                    //byte[] _tempDataField = new byte[4];
                    //_tempDataField = BitConverter.GetBytes(_tempNumber);
                    ////更改100增益校准电压
                    //byte[] _port100buf = { 0x3A, 0x02, 0x01, 0x09, 0x00, 0x00, 0x00, 0x56, 0x4F, 0x3B };
                    ////_port100buf[8] = _tempDataField[0];
                    ////_port100buf[9] = _tempDataField[1];
                    //_PortSendBuf = _port100buf;
                    //_Port.Write(_PortSendBuf, 0, _PortSendBuf.Length);
                    _PortSendBuf = _APDComm.VisitCalibrationVolt(APDCommProtocol.APDChannelType.CHA, APDCommProtocol.FunctionCodeType.WriteReg, value);
                    _Port.Write(_PortSendBuf, 0, _PortSendBuf.Length);
                    APDCommProtocol.CommStatus = APDCommProtocol.CommStatusType.Hearing;
                    Thread.Sleep(500);
                }
            }
        }

        public double? APDHighVoltCHA
        {
            get
            {
                return _APDComm.CommOutputChA.APDHighVoltage;
            }
        }

        public int LaserCHA
        {
            get
            {
                return _APDComm.CommOutputChA.LaserPower;
            }
            set
            {
                if (_IsPortAvailable() == false)
                {
                    return;
                }
                try
                {
                    //3a 01 02 03 00 00 00 00 00 3b
                    //_APDComm.CommOutputChA.LaserPower = value;
                    _PortSendBuf = _LaserComm.WriteLaser(APDCommProtocol.APDChannelType.CHA, value);
                    _Port.Write(_PortSendBuf, 0, _PortSendBuf.Length);
                    APDCommProtocol.CommStatus = APDCommProtocol.CommStatusType.Hearing;
                    Thread.Sleep(500);
                }
                catch { }
            }
        }

        public int CommTimeOutCount
        {
            get
            {
                return _CommTimeOutCount;
            }
            set
            {
                if (value == 0)
                {
                    _CommTimeOutCount = value;
                }
            }
        }
        public double? Caltemperature
        {
            get
            {
                return _Caltemperature;
            }
            set
            {
                if (value == 0)
                {
                    _Caltemperature = value;
                }
            }
        }
        public double? Temperature500
        {
            get
            {
                return _emperature500;
            }
            set
            {
                if (value == 0)
                {
                    _emperature500 = value;
                }
            }
        }

        #region testIVNum
        public string IVNum
        {
            //get
            //{
            //    return _APDComm.CommOutputChA.CurrentIVNum;
            //}
            set
            {
                if (_IsPortAvailable() == false)
                {
                    return;
                }
                try
                {
                    //string result = System.Text.RegularExpressions.Regex.Replace(value, @"[^0-9]+", "");
                    string result = value.Substring(2, value.Length - 2);
                    //if (result.Length > 7)
                    //{
                    //    MessageBox.Show("APD编号写入失败，字节越界！", "Error");
                    //    return;
                    //}
                    byte[] _buf = HexStringToByteArray(result);
                    byte[] _portbuf = { 0x3A, 0x02, 0x01, 0x02, 0x00, 0x00, 0x00, 0x00, 0x00, 0x3B };
                    //for (int i = 0; i < _buf.Length; i++)
                    {
                        if (_buf.Length == 1)
                        {
                            _portbuf[8] = _buf[_buf.Length - 1];
                        }
                        else if (_buf.Length == 2)
                        {
                            _portbuf[8] = _buf[_buf.Length - 1];
                            _portbuf[7] = _buf[_buf.Length - 2];
                        }
                        else if (_buf.Length == 3)
                        {
                            _portbuf[8] = _buf[_buf.Length - 1];
                            _portbuf[7] = _buf[_buf.Length - 2];
                            _portbuf[6] = _buf[_buf.Length - 3];
                        }
                        else if (_buf.Length == 4)
                        {
                            _portbuf[8] = _buf[_buf.Length - 1];
                            _portbuf[7] = _buf[_buf.Length - 2];
                            _portbuf[6] = _buf[_buf.Length - 3];
                            _portbuf[5] = _buf[_buf.Length - 4];
                        }

                    }
                    _PortSendBuf = _portbuf;
                    _Port.Write(_PortSendBuf, 0, _PortSendBuf.Length);
                    APDCommProtocol.CommStatus = APDCommProtocol.CommStatusType.Hearing;
                    Thread.Sleep(500);
                }
                catch { }
            }
        }
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
        public int IVType
        {
            //get
            //{
            //    return _APDComm.CommOutputChA.CurrentIVType;
            //}
            set
            {
                if (_IsPortAvailable() == false)
                {
                    return;
                }
                try
                {
                    byte[] _portbuf = { 0x3A, 0x02, 0x01, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x3B };
                    if (value == 0)//APD
                    {
                        _portbuf[8] = 0x00;
                    }
                    else
                    { //PMT
                        _portbuf[8] = 0x01;
                    }
                    _PortSendBuf = _portbuf;
                    _Port.Write(_PortSendBuf, 0, _PortSendBuf.Length);
                    APDCommProtocol.CommStatus = APDCommProtocol.CommStatusType.Hearing;
                    Thread.Sleep(500);
                }
                catch { }
            }
        }
        public double Tempscale
        {
            get
            {
                return _APDComm.CommOutputChA.Tempscale;
            }
            set
            {
                if (_IsPortAvailable() == false)
                {
                    return;
                }
                try
                {
                    byte[] _portbuf = { 0x3A, 0x02, 0x01, 0x17, 0x00, 0x00, 0x00, 0x00, 0x00, 0x3B };
                    if (value == 1.85)
                    {
                        _portbuf[8] = 0xb9;
                    }
                    else
                    {
                        _portbuf[8] = 0x41;
                    }
                    _PortSendBuf = _portbuf;
                    if (_Port != null)
                        _Port.Write(_PortSendBuf, 0, _PortSendBuf.Length);
                    APDCommProtocol.CommStatus = APDCommProtocol.CommStatusType.Hearing;
                    Thread.Sleep(500);
                }
                catch { }
            }
        }
        public void ChangeCalibrationValtage()
        {
            //更改50增益校准电压
            byte[] _port50buf = { 0x3A, 0x02, 0x01, 0x08, 0x00, 0x00, 0x00, 0x13, 0x88, 0x3B };
            _PortSendBuf = _port50buf;
            _Port.Write(_PortSendBuf, 0, _PortSendBuf.Length);
            Thread.Sleep(500);
            //更改100增益校准电压
            byte[] _port100buf = { 0x3A, 0x02, 0x01, 0x09, 0x00, 0x00, 0x00, 0x13, 0x88, 0x3B };
            _PortSendBuf = _port100buf;
            _Port.Write(_PortSendBuf, 0, _PortSendBuf.Length);
            Thread.Sleep(500);
            //更改150增益校准电压
            byte[] _port150buf = { 0x3A, 0x02, 0x01, 0x0A, 0x00, 0x00, 0x00, 0x13, 0x88, 0x3B };
            _PortSendBuf = _port150buf;
            _Port.Write(_PortSendBuf, 0, _PortSendBuf.Length);
            Thread.Sleep(500);
            //更改200增益校准电压
            byte[] _port200buf = { 0x3A, 0x02, 0x01, 0x0B, 0x00, 0x00, 0x00, 0x13, 0x88, 0x3B };
            _PortSendBuf = _port200buf;
            _Port.Write(_PortSendBuf, 0, _PortSendBuf.Length);
            Thread.Sleep(500);

            //更改250增益校准电压
            byte[] _port250buf = { 0x3A, 0x02, 0x01, 0x0C, 0x00, 0x00, 0x00, 0x13, 0x88, 0x3B };
            _PortSendBuf = _port250buf;
            _Port.Write(_PortSendBuf, 0, _PortSendBuf.Length);
            Thread.Sleep(500);

            //更改300增益校准电压
            byte[] _port300buf = { 0x3A, 0x02, 0x01, 0x0D, 0x00, 0x00, 0x00, 0x13, 0x88, 0x3B };
            _PortSendBuf = _port300buf;
            _Port.Write(_PortSendBuf, 0, _PortSendBuf.Length);
            Thread.Sleep(500);
            //更改400增益校准电压
            byte[] _port400buf = { 0x3A, 0x02, 0x01, 0x0E, 0x00, 0x00, 0x00, 0x13, 0x88, 0x3B };
            _PortSendBuf = _port400buf;
            _Port.Write(_PortSendBuf, 0, _PortSendBuf.Length);
            Thread.Sleep(500);
            //更改500增益校准电压
            byte[] _port500buf = { 0x3A, 0x02, 0x01, 0x0F, 0x00, 0x00, 0x00, 0x13, 0x88, 0x3B };
            _PortSendBuf = _port500buf;
            _Port.Write(_PortSendBuf, 0, _PortSendBuf.Length);
            Thread.Sleep(500);

        }
        public void GetApdHVote()
        {

            byte[] _portbuf = { 0x3A, 0x02, 0x02, 0x06, 0x00, 0x00, 0x00, 0x00, 0x00, 0x3B };
            _PortSendBuf = _portbuf;
            _Port.Write(_PortSendBuf, 0, _PortSendBuf.Length);
            Thread.Sleep(500);
        }
        #endregion
        #endregion Public Peroperties

    }
}
