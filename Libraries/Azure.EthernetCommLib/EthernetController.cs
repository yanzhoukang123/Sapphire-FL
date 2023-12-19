using Azure.CommunicationLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static Azure.EthernetCommLib.AvocadoProtocol;

namespace Azure.EthernetCommLib
{
    public delegate void GotDataHandle();
    public enum CommStatusTypes
    {
        Idle,
        Waiting,
        Received,
        TimeOut,
    }

    public class EthernetController
    {
        public event GotDataHandle OnReceivedScanningData;
        public event GotDataHandle OnReceivedMotionData;
        public event GotDataHandle OnRecievedSingleSampleData;
        public event GotDataHandle OnDataRateChanged;
        #region Private Fields
        private Socket _CommandSocket;
        private Socket _StreamSocket;
        private object _Token;
        //private Thread _CommandsHearingThread;
        //private Thread _StreamHearingThread;
        private System.Timers.Timer _RefreshTimer;
        private int _ReceivedBytes;
        private System.Timers.Timer _StatusTimer;
        private SocketAsyncEventArgs m_CommandReceiveSAEA;
        private SocketAsyncEventArgs m_CommandSendSAEA;
        private SocketAsyncEventArgs m_StreamReceiveSAEA;
        //private SocketAsyncEventArgs m_StreamSendSAEA;
        private EndPoint pt;
        private EndPoint streamPt;
        #endregion Private Fields

        public EthernetController()
        {
            ReceivingBuf = new FifoBuffer(1048576 * 10);      // 1MB*10 buffer size
            _Token = new object();
            _RefreshTimer = new System.Timers.Timer();
            _RefreshTimer.AutoReset = true;
            _RefreshTimer.Elapsed += _RefreshTimer_Elapsed;
            _RefreshTimer.Interval = 500;
            _RefreshTimer.Start();

            //_StatusTimer = new Thread(_StatusTimer_Elapsed);
            //_StatusTimer.IsBackground = true;
            //_StatusTimer.Start();

            _StatusTimer = new System.Timers.Timer();
            _StatusTimer.AutoReset = false;
            _StatusTimer.Interval = 500;
            _StatusTimer.Elapsed += _StatusTimer_Elapsed;

            AvocadoProtocol.OnReceivedMotionData += AvocadoProtocol_OnReceivedMotionData;
            AvocadoProtocol.OnReceivedScanningData += AvocadoProtocol_OnReceivedScanningData;
            AvocadoProtocol.OnRecievedSingleSampleData += AvocadoProtocol_OnRecievedSingleSampleData;
        }

        private void AvocadoProtocol_OnRecievedSingleSampleData()
        {
            OnRecievedSingleSampleData?.Invoke();
        }

        private void _StatusTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            if (CommStatus == CommStatusTypes.Waiting)
            {
                CommStatus = CommStatusTypes.TimeOut;
                _StatusTimer.Stop();
            }
        }

        private void AvocadoProtocol_OnReceivedScanningData()
        {
            OnReceivedScanningData?.Invoke();
        }

        private void AvocadoProtocol_OnReceivedMotionData()
        {
            OnReceivedMotionData?.Invoke();
        }

        private void _RefreshTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            if (_ReceivedBytes == 0)
            {
                ReadingRate = 0;
                OnDataRateChanged?.Invoke();
            }
            else
            {
                ReadingRate = _ReceivedBytes / 512.0;   // convert to KB/Sec
                _ReceivedBytes = 0;
                OnDataRateChanged?.Invoke();
            }
        }

        #region Public Properties
        public bool IsConnected { get; private set; }

        public string ErrorMessage { get; private set; }
        /// <summary>
        /// received data will be stored in ReceivingBuf in streaming mode, that is, scanning data;
        /// otherwise the received data will be regarded as command response;
        /// </summary>
        //public bool IsStreamingMode { get; set; }
        /// <summary>
        /// unit of KB/Sec
        /// </summary>
        public double ReadingRate { get; private set; }
        public FifoBuffer ReceivingBuf { get; private set; }

        public string HWVersion { get { return AvocadoProtocol.HWVersion; } }
        public string FWVersion { get { return AvocadoProtocol.FWVersion; } }
        public string LEDVersion { get { return AvocadoProtocol.LEDVersion; } }
        public AvocadoDeviceProperties DeviceProperties { get { return AvocadoProtocol.DeviceProperties; } }
        public Dictionary<MotorTypes, MotionState> MotionStates { get { return AvocadoProtocol.MotionStates; } }
        public Dictionary<MotorTypes, int> MotionCrntPositions { get { return AvocadoProtocol.MotionCrntPositions; } }

        public uint SampleValueChA { get { return AvocadoProtocol.SingleSampleChA; } }
        public uint SampleValueChB { get { return AvocadoProtocol.SingleSampleChB; } }
        public uint SampleValueChC { get { return AvocadoProtocol.SingleSampleChC; } }

        public static Dictionary<IVChannels, IvSensorType> IvSensorTypes { get { return AvocadoProtocol.IvSensorTypes; } }
        public static Dictionary<IVChannels, string> IVEstimatedVersionNumberBoard { get { return AvocadoProtocol.IVEstimatedVersionNumberBoard; } }

        public  Dictionary<AmbientTemperatureChannel, double> AmbientTemperature { get { return AvocadoProtocol.AmbientTemperature; } }

        public static Dictionary<IVChannels, string> IVErrorCode { get { return AvocadoProtocol.IVErrorCode; } }

        public static Dictionary<IVChannels, string> IVOpticalModuleSerialNumber { get { return AvocadoProtocol.IVOpticalModuleSerialNumber; } }

        public static Dictionary<IVChannels, uint> IvSensorSerialNumbers { get { return AvocadoProtocol.IvSensorSerialNumbers; } }

        public static Dictionary<IVChannels, double> TempSenser1282AD { get { return AvocadoProtocol.TempSenser1282AD; } }

        public static Dictionary<IVChannels, double> TempSenser6459AD { get { return AvocadoProtocol.TempSenser6459AD; } }

        public static Dictionary<IVChannels, uint> PMTCompensationCoefficient { get { return AvocadoProtocol.PMTCompensationCoefficient; } }

        public static Dictionary<IVChannels, double> LightIntensityCalibrationTemperature { get { return AvocadoProtocol.LightIntensityCalibrationTemperature; } }
        public static Dictionary<IVChannels, double> APDGainCalVol { get { return AvocadoProtocol.APDGainCalVol; } }

        public static Dictionary<LaserChannels, uint> LaserSerialNumbers { get { return AvocadoProtocol.LaserSerialNumbers; } }
        public static Dictionary<LaserChannels, uint> LaserWaveLengths { get { return AvocadoProtocol.LaserWaveLengths; } }

        public static Dictionary<LaserChannels, string> LaserErrorCode { get { return AvocadoProtocol.LaserErrorCode; } }

        public static Dictionary<LaserChannels, string> LaserOpticalModuleSerialNumber { get { return AvocadoProtocol.LaserOpticalModuleSerialNumber; } }

        public static Dictionary<LaserChannels, string> LaserBoardFirmwareVersionNumber { get { return AvocadoProtocol.LaserBoardFirmwareVersionNumber; } }
        private CommStatusTypes _CommStatus;
        public CommStatusTypes CommStatus
        {
            get { return _CommStatus; }
            set
            {
                if (_CommStatus != value)
                {
                    _CommStatus = value;
                    if (_CommStatus == CommStatusTypes.Waiting)
                    {
                        if (_StatusTimer != null)
                        {
                            _StatusTimer.Start();
                        }
                        else
                        {
                            _StatusTimer = new System.Timers.Timer();
                            _StatusTimer.AutoReset = false;
                            _StatusTimer.Interval = 500;
                            _StatusTimer.Elapsed += _StatusTimer_Elapsed;
                        }
                    }
                    else if (_CommStatus != CommStatusTypes.TimeOut)
                    {
                        if (_StatusTimer != null)
                        {
                            _StatusTimer.Stop();
                        }
                    }
                }
            }
        }
        public bool OpticalModulePowerMonitor { get { return AvocadoProtocol.OpticalModulePowerMonitor; } }  ////Optical module power monitoring   光学模块电源监测（FW Version 1.1.0.0）
        public bool DevicePowerStatus { get { return AvocadoProtocol.DevicePowerStatus; } } //Front panel button power status  前面板按钮电源状态（FW Version 1.1.0.0）
        public bool LidIsOpen { get { return AvocadoProtocol.LidIsOpen; } }

        public bool TopCoverLock { get { return AvocadoProtocol.TopCoverLock; } } //Top cover  status（FW Version 1.1.0.0）   顶盖状态(硬件版本V1.1)
        public bool TopMagneticState { get { return AvocadoProtocol.TopMagneticState; } }  // Front lid  status （FW Version 1.1.0.0） 前盖状态(硬件版本V1.1)
        public bool OpticalModulePowerStatus { get { return AvocadoProtocol.OpticalModulePowerStatus; } } //Optical module power status （FW Version 1.1.0.0）  光学模块电源状态(硬件版本V1.1)

        public bool ShutdownDuringScanStatus
        {
            get { return AvocadoProtocol.ShutdownDuringScanStatus; }  //State when pressing the front panel button while scanning images  （FW Version 1.1.0.0）  //扫描图像时按下前面板按钮时的状态
            set
            {
                if (AvocadoProtocol.ShutdownDuringScanStatus != value)
                {
                    AvocadoProtocol.ShutdownDuringScanStatus = value;
                }
            }
        }

        public Dictionary<LaserChannels, double> LaserTemperatures { get { return AvocadoProtocol.LaserTemperatures; } }
        public Dictionary<LaserChannels, double> RadiatorTemperatures { get { return AvocadoProtocol.RadiatorTemperatures; } }

        public static Dictionary<LaserChannels, double> TECControlTemperature { get { return AvocadoProtocol.TECControlTemperature; } }
        public Dictionary<LaserChannels, double> FanTemperatures { get { return AvocadoProtocol.FanTemperatures; } }

        public Dictionary<LaserChannels, double> TECMaximumCoolingCurrentValue { get { return AvocadoProtocol.TECMaximumCoolingCurrentValue; } }

        public static Dictionary<LaserChannels, double> TECRefrigerationControlParameterKp { get { return AvocadoProtocol.TECRefrigerationControlParameterKp; } }

        public static Dictionary<LaserChannels, double> TECRefrigerationControlParameterKi { get { return AvocadoProtocol.TECRefrigerationControlParameterKi; } }

        public static Dictionary<LaserChannels, double> TECRefrigerationControlParameterKd { get { return AvocadoProtocol.TECRefrigerationControlParameterKd; } }
        public uint XEncoderSubdivision { get { return AvocadoProtocol.XEncoderSubdivision; } }
        public Dictionary<LaserChannels, double> AllIntensity { get { return AvocadoProtocol.AllIntensity; } }
        public static Dictionary<LaserChannels, double> TECMaximumCurrent { get { return AvocadoProtocol.TECMaximumCurrent; } }

        public Dictionary<LaserChannels, double> AllCurrentLightPower { get { return AvocadoProtocol.AllCurrentLightPower; } }
        public static Dictionary<LaserChannels, double> AllOpticalPowerGreaterThan15mWKp { get { return AvocadoProtocol.AllOpticalPowerGreaterThan15mWKp; } }
        public static Dictionary<LaserChannels, double> AllOpticalPowerGreaterThan15mWKi { get { return AvocadoProtocol.AllOpticalPowerGreaterThan15mWKi; } }
        public static Dictionary<LaserChannels, double> AllOpticalPowerGreaterThan15mWKd { get { return AvocadoProtocol.AllOpticalPowerGreaterThan15mWKd; } }

        public static Dictionary<LaserChannels, double> AllOpticalPowerLessThanOrEqual15mWKp { get { return AvocadoProtocol.AllOpticalPowerLessThanOrEqual15mWKp; } }
        public static Dictionary<LaserChannels, double> AllOpticalPowerLessThanOrEqual15mWKi { get { return AvocadoProtocol.AllOpticalPowerLessThanOrEqual15mWKi; } }
        public static Dictionary<LaserChannels, double> AllOpticalPowerLessThanOrEqual15mWKd { get { return AvocadoProtocol.AllOpticalPowerLessThanOrEqual15mWKd; } }
        public Dictionary<LaserChannels, double> AllRadioDiodeVoltage { get { return AvocadoProtocol.AllRadioDiodeVoltage; } }
        public Dictionary<LaserChannels, double> AllGetRadioDiodeSlope { get { return AvocadoProtocol.AllGetRadioDiodeSlope; } }
        public Dictionary<LaserChannels, double> AllGetRadioAndTelevisionDiodeCalibrationConstant { get { return AvocadoProtocol.AllGetRadioAndTelevisionDiodeCalibrationConstant; } }
        public Dictionary<LaserChannels, double> AllOpticalPowerControlvoltage { get { return AvocadoProtocol.AllOpticalPowerControlvoltage; } }
        public Dictionary<LaserChannels, double> AllCurrentValuelaser { get { return AvocadoProtocol.AllCurrentValuelaser; } }

        public static Dictionary<LaserChannels, double> AllGetOpticalPowerControlKpUpperLimitLessThanOrEqual15 { get { return AvocadoProtocol.AllGetOpticalPowerControlKpUpperLimitLessThanOrEqual15; } }
        public static Dictionary<LaserChannels, double> AllGetOpticalPowerControlKpDownLimitLessThanOrEqual15 { get { return AvocadoProtocol.AllGetOpticalPowerControlKpDownLimitLessThanOrEqual15; } }
        public static Dictionary<LaserChannels, double> AllGetOpticalPowerControlKiUpperLimitLessThanOrEqual15 { get { return AvocadoProtocol.AllGetOpticalPowerControlKiUpperLimitLessThanOrEqual15; } }
        public static Dictionary<LaserChannels, double> AllGetOpticalPowerControlKiDownLimitLessThanOrEqual15 { get { return AvocadoProtocol.AllGetOpticalPowerControlKiDownLimitLessThanOrEqual15; } }
        public static Dictionary<LaserChannels, double> AllGetOpticalPowerControlKpUpperLimitLessThan15 { get { return AvocadoProtocol.AllGetOpticalPowerControlKpUpperLimitLessThan15; } }
        public static Dictionary<LaserChannels, double> AllGetOpticalPowerControlKpDownLimitLessThan15 { get { return AvocadoProtocol.AllGetOpticalPowerControlKpDownLimitLessThan15; } }
        public static Dictionary<LaserChannels, double> AllGetOpticalPowerControlKiUpperLimitLessThan15 { get { return AvocadoProtocol.AllGetOpticalPowerControlKiUpperLimitLessThan15; } }
        public static Dictionary<LaserChannels, double> AllGetOpticalPowerControlKiDownLimitLessThan15 { get { return AvocadoProtocol.AllGetOpticalPowerControlKiDownLimitLessThan15; } }
        public static Dictionary<LaserChannels, double> AllGetLaserMaximumCurrent { get { return AvocadoProtocol.AllGetLaserMaximumCurrent; } }
        public static Dictionary<LaserChannels, double> AllGetLaserMinimumCurrent { get { return AvocadoProtocol.AllGetLaserMinimumCurrent; } }

        public uint LightGainDataState { get { return AvocadoProtocol.LightGainDataState; } }
        private byte[] _LightGainData = null;
        public byte[] LightGainData
        {
            get
            {
                return AvocadoProtocol.LightGainData;
            }
            set
            {
                if (_LightGainData != value)
                {
                    _LightGainData = value;
                }
            }
        }
        public string Test { get; set; }
        //public int LaserAIntensity { get { return AvocadoProtocol.LightGainDataState; } }
        //public int LaserBIntensity { get { return AvocadoProtocol.LightGainDataState; } }
        //public int LaserCIntensity { get { return AvocadoProtocol.LightGainDataState; } }
        public void GetTest()
        {

            Test = AvocadoProtocol.Test;
        }
        #endregion Public Properties

        #region Public Functions
        public bool Connect(IPAddress serverIp, int commandportNum, int streamPortNum, IPAddress networkCardIp)
        {
            try
            {
                if (IsConnected) { return true; }
                _CommandSocket = new Socket(SocketType.Stream, ProtocolType.Tcp);
                pt = new IPEndPoint(serverIp, commandportNum);
                _CommandSocket.Bind(new IPEndPoint(networkCardIp, 0));
                m_CommandReceiveSAEA = new SocketAsyncEventArgs { RemoteEndPoint = pt };
                m_CommandReceiveSAEA.Completed += CommandReceiveArgs_Completed;
                _CommandSocket.ConnectAsync(m_CommandReceiveSAEA);

                _StreamSocket = new Socket(SocketType.Stream, ProtocolType.Tcp);
                _StreamSocket.Bind(new IPEndPoint(networkCardIp, 0));
                streamPt = new IPEndPoint(serverIp, streamPortNum);
                m_StreamReceiveSAEA = new SocketAsyncEventArgs { RemoteEndPoint = streamPt };
                m_StreamReceiveSAEA.Completed += StreamReceiveArgs_Completed;
                _StreamSocket.ConnectAsync(m_StreamReceiveSAEA);

                //_CommandsHearingThread = new Thread(CommandsHearingProcess);
                //_CommandsHearingThread.IsBackground = true;
                //_CommandsHearingThread.Start();

                //_StreamHearingThread = new Thread(StreamHearingProcess);
                //_StreamHearingThread.IsBackground = true;
                //_StreamHearingThread.Start();
                ErrorMessage = null;
                IsConnected = true;
                return true;
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
                return false;
            }
        }

        public bool ReConnect(IPAddress serverIp, int commandportNum, int streamPortNum, IPAddress networkCardIp)
        {
            try
            {
                if (IsConnected) { return true; }
                _CommandSocket = new Socket(SocketType.Stream, ProtocolType.Tcp);
                pt = new IPEndPoint(serverIp, commandportNum);
                _CommandSocket.Bind(new IPEndPoint(networkCardIp, 0));
                m_CommandReceiveSAEA = new SocketAsyncEventArgs { RemoteEndPoint = pt };
                m_CommandReceiveSAEA.Completed += CommandReceiveArgs_Completed;
                _CommandSocket.ConnectAsync(m_CommandReceiveSAEA);

                //_StreamSocket = new Socket(SocketType.Stream, ProtocolType.Tcp);
                //_StreamSocket.Bind(new IPEndPoint(networkCardIp, 0));
                //streamPt = new IPEndPoint(serverIp, streamPortNum);
                //m_StreamReceiveSAEA = new SocketAsyncEventArgs { RemoteEndPoint = streamPt };
                //m_StreamReceiveSAEA.Completed += StreamReceiveArgs_Completed;
                //_StreamSocket.ConnectAsync(m_StreamReceiveSAEA);


                //_CommandsHearingThread = new Thread(CommandsHearingProcess);
                //_CommandsHearingThread.IsBackground = true;
                //_CommandsHearingThread.Start();

                //_StreamHearingThread = new Thread(StreamHearingProcess);
                //_StreamHearingThread.IsBackground = true;
                //_StreamHearingThread.Start();
                ErrorMessage = null;
                IsConnected = true;
                return true;
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
                return false;
            }
        }
        public void Disconnect()
        {
            //if (IsConnected)
            //{
            //    _CommandSocket.Disconnect(true);
            //    _StreamSocket.Disconnect(true);
            //    IsConnected = false;
            //}

            if (_CommandSocket.Connected)
            {
                m_CommandReceiveSAEA.Completed -= CommandReceiveArgs_Completed;
                _CommandSocket.DisconnectAsync(m_CommandReceiveSAEA);
                _CommandSocket.Disconnect(true);
                pt = null;
                IsConnected = false;
            }

        }

        public bool SendBytes(byte[] cmd)
        {
            if (!IsConnected) { return false; }
            //int waitCnt = 0;
            //while(CommStatus!= CommStatusTypes.Idle)
            //{
            //    Thread.Sleep(1);
            //    waitCnt++;
            //    if(waitCnt> 500)
            //    {
            //        return false;
            //    }
            //}
            lock (_Token)
            {
                if(CommStatus == CommStatusTypes.Received)
                {
                    CommStatus = CommStatusTypes.Idle;
                }
                else if (CommStatus != CommStatusTypes.Idle) { return false; }
                try
                {
                    if (m_CommandSendSAEA == null)
                    {
                        m_CommandSendSAEA = new SocketAsyncEventArgs();
                        //m_sendSAEA.Completed += OnSendCompleted;
                    }

                    m_CommandSendSAEA.SetBuffer(cmd, 0, cmd.Length);
                    if (_CommandSocket != null)
                    {
                        // _CommandSocket.Send(cmd);
                        _CommandSocket.SendAsync(m_CommandSendSAEA);
                    }
                    CommStatus = CommStatusTypes.Waiting;

                    while (CommStatus == CommStatusTypes.Waiting)
                    {
                        Thread.Sleep(1);
                    }
                    if (CommStatus == CommStatusTypes.TimeOut)
                    {
                        CommStatus = CommStatusTypes.Idle;
                        return false;
                    }
                    else
                    {
                        CommStatus = CommStatusTypes.Idle;
                        return true;
                    }
                }
                catch(System.Net.Sockets.SocketException ex)
                {
                    IsConnected = false;
                    Connect(new IPAddress(new byte[] { 192, 168, 1, 110 }), 5000, 8000, new IPAddress(new byte[] { 192, 168, 1, 100 }));
                    ErrorMessage = ex.Message;
                    return false;
                }
                catch (Exception ex)
                {
                    return false;
                }
            }
        }

        public bool TriggerSingleScan()
        {
            uint dx = 0;
            uint dy = 0;
            uint dz = 0;
            uint res = 0;
            uint accVal = 0;
            uint interval = 0;
            byte[] cmd = AvocadoProtocol.SetScanParameters(dx, dy, dz, res, accVal, interval, true);
            return SendBytes(cmd);
        }

        public bool SetHorizontalScanExtraMove(int extraMove)
        {
            byte[] cmd = AvocadoProtocol.SetHorizontalScanExtraMove(extraMove);
            return SendBytes(cmd);
        }
        public bool TriggerHorizontalScan(uint dx, uint dy, uint res, uint accVal)
        {
            uint dz = 0;
            uint interval = 0;
            byte[] cmd = AvocadoProtocol.SetScanParameters(dx, dy, dz, res, accVal, interval, true);
            return SendBytes(cmd);
        }
        public bool TriggerTimingScan(uint interval)
        {
            uint dx = 0;
            uint dy = 0;
            uint dz = 0;
            uint res = 0;
            uint accVal = 0;
            byte[] cmd = AvocadoProtocol.SetScanParameters(dx, dy, dz, res, accVal, interval, true);
            return SendBytes(cmd);
        }
        public bool TriggerZScan(uint dz)
        {
            uint dx = 0;
            uint dy = 0;
            uint res = 0;
            uint accVal = 0;
            uint interval = 0;
            byte[] cmd = AvocadoProtocol.SetScanParameters(dx, dy, dz, res, accVal, interval, true);
            return SendBytes(cmd);
        }
        public bool TriggerXScan(uint dx, uint interval)
        {
            uint dy = 0;
            uint dz = 0;
            uint res = 0;
            uint accVal = 0;
            byte[] cmd = AvocadoProtocol.SetScanParameters(dx, dy, dz, res, accVal, interval, true);
            return SendBytes(cmd);
        }
        public bool StopScan()
        {
            byte[] cmd = AvocadoProtocol.StopScan();
            return SendBytes(cmd);
        }
        /// <summary>
        /// get hardware version & firmware version
        /// </summary>
        /// <returns></returns>
        /// 
        public bool GetLEDVersions()
        {
            byte[] cmd = AvocadoProtocol.GetLEDVersions();
            return SendBytes(cmd);
        }
        public bool GetSystemVersions()
        {
            byte[] cmd = AvocadoProtocol.GetSystemVersions();
            return SendBytes(cmd);
        }
        public bool GetIVSystemVersions(IVChannels channel)
        {
            byte[] cmd = AvocadoProtocol.GetIVSystemVersions(channel);
            return SendBytes(cmd);
        }
        public bool GetIVSystemVersions()
        {
            byte[] cmd = AvocadoProtocol.GetIVSystemVersions();
            return SendBytes(cmd);
        }

        public bool GetIVSystemErrorCode()
        {
            byte[] cmd = AvocadoProtocol.GetIVSystemErrorCode();
            return SendBytes(cmd);
        }

        public bool GetIVOpticalModuleSerialNumber()
        {
            byte[] cmd = AvocadoProtocol.GetIVOpticalModuleSerialNumber();
            return SendBytes(cmd);
        }
        public bool GetIVOpticalModuleSerialNumber(IVChannels channel)
        {
            byte[] cmd = AvocadoProtocol.GetIVOpticalModuleSerialNumber(channel);
            return SendBytes(cmd);
        }

        public bool GetLaserErrorCode()
        {
            byte[] cmd = AvocadoProtocol.GetLaserErrorCode();
            return SendBytes(cmd);
        }
        public bool GetLaserOpticalModuleSerialNumber(LaserChannels channel)
        {
            byte[] cmd = AvocadoProtocol.GetLaserOpticalModuleSerialNumber(channel);
            return SendBytes(cmd);
        }
        public bool GetLaserOpticalModuleSerialNumber()
        {
            byte[] cmd = AvocadoProtocol.GetLaserOpticalModuleSerialNumber();
            return SendBytes(cmd);
        }

        public bool GetLaserSystemVersions(LaserChannels channel)
        {
            byte[] cmd = AvocadoProtocol.GetLaserSystemVersions(channel);
            return SendBytes(cmd);
        }

        public bool GetLaserSystemVersions()
        {
            byte[] cmd = AvocadoProtocol.GetLaserSystemVersions();
            return SendBytes(cmd);
        }

        public bool GetDeviceProperties()
        {
            byte[] cmd = AvocadoProtocol.GetIndividualParameters();
            return SendBytes(cmd);
        }
        public bool GetLaserValueCurrent(LaserChannels channel, int currentPower)
        {
            byte[] cmd = AvocadoProtocol.GetLaserValueCurrent(channel, currentPower);
            return SendBytes(cmd);
        }

        public bool GetOpticalPowerControlvoltaget(LaserChannels channel, int currentPower)
        {
            byte[] cmd = AvocadoProtocol.GetOpticalPowerControlvoltaget(channel, currentPower);
            return SendBytes(cmd);
        }
        public bool GetXEncoderSubdivision()
        {
            byte[] cmd = AvocadoProtocol.GetXEncoderSubdivision();
            return SendBytes(cmd);
        }
        public bool SetDeviceProperties(AvocadoDeviceProperties properties)
        {
            byte[] cmd = AvocadoProtocol.SetIndividualParameters(properties);
            return SendBytes(cmd);
        }
        public bool SetIvPga(IVChannels channel, ushort pga)
        {
            byte[] cmd = AvocadoProtocol.SetIvPga(channel, pga);
            return SendBytes(cmd);
        }
        public bool SetIvApdGain(IVChannels channel, ushort apdGain)
        {
            byte[] cmd = AvocadoProtocol.SetIvApdGain(channel, apdGain);
            return SendBytes(cmd);
        }
        public bool SetIvPmtGain(IVChannels channel, ushort pmtGain)
        {
            byte[] cmd = AvocadoProtocol.SetIvPmtGain(channel, pmtGain);
            return SendBytes(cmd);
        }
        public bool SetLaserCurrent(LaserChannels channel, double current)
        {
            byte[] cmd = AvocadoProtocol.SetLaserCurrent(channel, current);
            return SendBytes(cmd);
        }
        public bool SetLaserPower(LaserChannels channel, double power)
        {
            byte[] cmd = AvocadoProtocol.SetLaserPower(channel, power);
            return SendBytes(cmd);
        }
        public bool SetLaserValuePower(LaserChannels channel, double power, int LaserAIntensity)
        {
            byte[] cmd = AvocadoProtocol.SetLaserValuePower(channel, power, LaserAIntensity);
            return SendBytes(cmd);
        }
        public bool SetLed(int ColorLight)
        {
            byte[] cmd = AvocadoProtocol.SetLed(ColorLight);
            return SendBytes(cmd);
        }
        //Optical module 0=on 1=off
        public bool SetShutdown(int Flag)
        {
            byte[] cmd = AvocadoProtocol.SetShutdown(Flag);
            return SendBytes(cmd);
        }
        public bool SetBuzzer(int count, int time)
        {
            byte[] cmd = AvocadoProtocol.SetBuzzer(count, time);
            return SendBytes(cmd);
        }
        public bool SetLedBarMarquee(LEDBarChannel channels, byte interval, bool enable)
        {
            byte[] cmd = AvocadoProtocol.SetLEDBarMarquee(channels, interval, enable);
            return SendBytes(cmd);
        }
        public bool SetLedBarProgress(byte progress)
        {
            byte[] cmd = AvocadoProtocol.SetLEDBarProgress(progress);
            return SendBytes(cmd);
        }
        public bool SetIncrustationFan(int IsAuto,int coefficient)
        {
            byte[] cmd = AvocadoProtocol.SetIncrustationFan(IsAuto,coefficient);
            return SendBytes(cmd);
        }
        public bool SetCommunicationControl(int sige)
        {
            byte[] cmd = AvocadoProtocol.SetCommunicationControl(sige);
            return SendBytes(cmd);
        }
        public bool SetIncrustationFan()
        {
            byte[] cmd = AvocadoProtocol.SetIncrustationFan();
            return SendBytes(cmd);
        }
        public bool SetFanTemperature(LaserChannels channel, double ReseTemperature)
        {
            byte[] cmd = AvocadoProtocol.SetFanTemperature(channel, ReseTemperature);
            return SendBytes(cmd);
        }
        public bool SetXMotionSubdivision(double _xMotionSubdivision)
        {
            byte[] cmd = AvocadoProtocol.SetXMotionSubdivision(_xMotionSubdivision);
            return SendBytes(cmd);
        }
        public bool SetLightSampleInterval(UInt16 sampleinterva)
        {
            byte[] cmd = AvocadoProtocol.SetLightSampleInterval(sampleinterva);
            return SendBytes(cmd);
        }
        public bool GetDataState()
        {
            byte[] cmd = AvocadoProtocol.GetDataState();
            return SendBytes(cmd);
        }
        public bool GetLightData()
        {
            byte[] cmd = AvocadoProtocol.GetLightData();
            return SendBytes(cmd);
        }

        public bool SetLightGain(UInt16 gain)
        {
            byte[] cmd = AvocadoProtocol.SetLightGain(gain);
            return SendBytes(cmd);
        }
        public bool SetLightSampleRange(Int32 samplerange)
        {
            byte[] cmd = AvocadoProtocol.SetLightSampleRange(samplerange);
            return SendBytes(cmd);
        }
        public bool SetLightSampleStart(UInt16 isstart)
        {
            byte[] cmd = AvocadoProtocol.SetLightSampleStart(isstart);
            return SendBytes(cmd);
        }
        public bool GetAllIvModulesInfo()
        {
            byte[] cmd = AvocadoProtocol.GetAllIvModulesInfo();
            return SendBytes(cmd);
        }
        public bool GetAllLaserModulseInfo()
        {
            byte[] cmd = AvocadoProtocol.GetAllLaserModulseInfo();
            return SendBytes(cmd);
        }
        public bool GetAllLaserTemperatures()
        {
            byte[] cmd = AvocadoProtocol.GetAllLaserTemperatures();
            return SendBytes(cmd);
        }

        public bool GetAllTempSenser1282AD()
        {
            byte[] cmd = AvocadoProtocol.GetAllTempSenser1282AD();
            return SendBytes(cmd);
        }

        public bool GetAllTempSenser6459AD()
        {
            byte[] cmd = AvocadoProtocol.GetAllTempSenser6459AD();
            return SendBytes(cmd);
        }

        public bool GetAllPMTCompensationCoefficient()
        {
            byte[] cmd = AvocadoProtocol.GetAllPMTCompensationCoefficient();
            return SendBytes(cmd);
        }
        public bool GetAllLightIntensityCalibrationTemperature()
        {
            byte[] cmd = AvocadoProtocol.GetAllLightIntensityCalibrationTemperature();
            return SendBytes(cmd);
        }
        public bool GetSingeLaserTemperatures(SubSys Type)
        {
            byte[] cmd = AvocadoProtocol.GetSingeLaserTemperatures(Type);
            return SendBytes(cmd);
        }
        public bool GetAllRadiatorTemperatures()
        {
            byte[] cmd = AvocadoProtocol.GetAllRadiatorTemperatures();
            return SendBytes(cmd);
        }

        public bool GetAllTECControlTTemperatures()
        {
            byte[] cmd = AvocadoProtocol.GetAllTECControlTTemperatures();
            return SendBytes(cmd);
        }
        public bool GetAllTECControlTTemperatures(LaserChannels channel)
        {
            byte[] cmd = AvocadoProtocol.GetAllTECControlTTemperatures(channel);
            return SendBytes(cmd);
        }

        public bool GetSingeRadiatorTemperatures(SubSys Type)
        {
            byte[] cmd = AvocadoProtocol.GetSingeRadiatorTemperatures(Type);
            return SendBytes(cmd);
        }
        public bool GetAllFanTemperatures()
        {
            byte[] cmd = AvocadoProtocol.GetAllFanTemperatures();
            return SendBytes(cmd);
        }

        public bool GetAllTECMaximumCoolingCurrentValue()
        {
            byte[] cmd = AvocadoProtocol.GetAllTECMaximumCoolingCurrentValue();
            return SendBytes(cmd);
        }
        public bool GetAllTECMaximumCoolingCurrentValue(LaserChannels channel)
        {
            byte[] cmd = AvocadoProtocol.GetAllTECMaximumCoolingCurrentValue(channel);
            return SendBytes(cmd);
        }

        public bool GetAllCurrentTECRefrigerationControlParameterKp()
        {
            byte[] cmd = AvocadoProtocol.GetAllCurrentTECRefrigerationControlParameterKp();
            return SendBytes(cmd);
        }
        public bool GetAllCurrentTECRefrigerationControlParameterKp(LaserChannels channel)
        {
            byte[] cmd = AvocadoProtocol.GetAllCurrentTECRefrigerationControlParameterKp(channel);
            return SendBytes(cmd);
        }
        public bool GetAllCurrentTECRefrigerationControlParameterKi()
        {
            byte[] cmd = AvocadoProtocol.GetAllCurrentTECRefrigerationControlParameterKi();
            return SendBytes(cmd);
        }
        public bool GetAllCurrentTECRefrigerationControlParameterKi(LaserChannels channel)
        {
            byte[] cmd = AvocadoProtocol.GetAllCurrentTECRefrigerationControlParameterKi(channel);
            return SendBytes(cmd);
        }
        public bool GetAllCurrentTECRefrigerationControlParameterKd()
        {
            byte[] cmd = AvocadoProtocol.GetAllCurrentTECRefrigerationControlParameterKd();
            return SendBytes(cmd);
        }
        public bool GetAllCurrentTECRefrigerationControlParameterKd(LaserChannels channel)
        {
            byte[] cmd = AvocadoProtocol.GetAllCurrentTECRefrigerationControlParameterKd(channel);
            return SendBytes(cmd);
        }
        //读取环境温度
        public bool GetAmbientTemperature()
        {
            byte[] cmd = AvocadoProtocol.GetAmbientTemperature();
            return SendBytes(cmd);
        }
        //Is the front panel power status pressed during the scanning process
        public bool PromptForPressingShutdown()
        {
            byte[] cmd = AvocadoProtocol.PromptForPressingShutdown();
            return SendBytes(cmd);
        }
        #region  532 Moudle
        public bool GetCurrentLightPower(LaserChannels channel)
        {
            byte[] cmd = AvocadoProtocol.GetCurrentLightPower(channel);
            return SendBytes(cmd);
        }
        public bool GetOpticalPowerLessThanOrEqual15mWKp()
        {
            byte[] cmd = AvocadoProtocol.GetOpticalPowerLessThanOrEqual15mWKp();
            return SendBytes(cmd);
        }
        public bool GetOpticalPowerLessThanOrEqual15mWKp(LaserChannels channel)
        {
            byte[] cmd = AvocadoProtocol.GetOpticalPowerLessThanOrEqual15mWKp(channel);
            return SendBytes(cmd);
        }

        public bool GetOpticalPowerLessThanOrEqual15mWKi()
        {
            byte[] cmd = AvocadoProtocol.GetOpticalPowerLessThanOrEqual15mWKi();
            return SendBytes(cmd);
        }
        public bool GetOpticalPowerLessThanOrEqual15mWKi(LaserChannels channel)
        {
            byte[] cmd = AvocadoProtocol.GetOpticalPowerLessThanOrEqual15mWKi(channel);
            return SendBytes(cmd);
        }

        public bool GetOpticalPowerLessThanOrEqual15mWKd()
        {
            byte[] cmd = AvocadoProtocol.GetOpticalPowerLessThanOrEqual15mWKd();
            return SendBytes(cmd);
        }
        public bool GetOpticalPowerLessThanOrEqual15mWKd(LaserChannels channel)
        {
            byte[] cmd = AvocadoProtocol.GetOpticalPowerLessThanOrEqual15mWKd(channel);
            return SendBytes(cmd);
        }
        public bool GetOpticalPowerGreaterThan15mWKp(LaserChannels channel)
        {
            byte[] cmd = AvocadoProtocol.GetOpticalPowerGreaterThan15mWKp(channel);
            return SendBytes(cmd);
        }

        public bool GetAllOpticalPowerGreaterThan15mWKp()
        {
            byte[] cmd = AvocadoProtocol.GetAllOpticalPowerGreaterThan15mWKp();
            return SendBytes(cmd);
        }
        public bool GetAllOpticalPowerGreaterThan15mWKp(LaserChannels channel)
        {
            byte[] cmd = AvocadoProtocol.GetAllOpticalPowerGreaterThan15mWKp(channel);
            return SendBytes(cmd);
        }
        public bool GetAllOpticalPowerGreaterThan15mWKi()
        {
            byte[] cmd = AvocadoProtocol.GetAllOpticalPowerGreaterThan15mWKi();
            return SendBytes(cmd);
        }

        public bool GetAllOpticalPowerGreaterThan15mWKi(LaserChannels channel)
        {
            byte[] cmd = AvocadoProtocol.GetAllOpticalPowerGreaterThan15mWKi(channel);
            return SendBytes(cmd);
        }
        public bool GetAllOpticalPowerGreaterThan15mWKd()
        {
            byte[] cmd = AvocadoProtocol.GetAllOpticalPowerGreaterThan15mWKd();
            return SendBytes(cmd);
        }
        public bool GetAllOpticalPowerGreaterThan15mWKd(LaserChannels channel)
        {
            byte[] cmd = AvocadoProtocol.GetAllOpticalPowerGreaterThan15mWKd(channel);
            return SendBytes(cmd);
        }
        public bool GetOpticalPowerGreaterThan15mWKi(LaserChannels channel)
        {
            byte[] cmd = AvocadoProtocol.GetOpticalPowerGreaterThan15mWKi(channel);
            return SendBytes(cmd);
        }
        public bool GetCurrentLightPowerKd(LaserChannels channel)
        {
            byte[] cmd = AvocadoProtocol.GetOpticalPowerGreaterThan15mWKd(channel);
            return SendBytes(cmd);
        }
        public bool SetOpticalPowerGreaterThan15mWKp(LaserChannels channel, double current)
        {
            byte[] cmd = AvocadoProtocol.SetOpticalPowerGreaterThan15mWKp(channel, current);
            return SendBytes(cmd);
        }

        public bool SetOpticalPowerGreaterThan15mWKi(LaserChannels channel, double current)
        {
            byte[] cmd = AvocadoProtocol.SetOpticalPowerGreaterThan15mWKi(channel, current);
            return SendBytes(cmd);
        }
        public bool SetOpticalPowerGreaterThan15mWKd(LaserChannels channel, double current)
        {
            byte[] cmd = AvocadoProtocol.SetOpticalPowerGreaterThan15mWKd(channel, current);
            return SendBytes(cmd);
        }
        public bool SetOpticalPowerLessThanOrEqual15mWKp(LaserChannels channel, double current)
        {
            byte[] cmd = AvocadoProtocol.SetOpticalPowerLessThanOrEqual15mWKp(channel, current);
            return SendBytes(cmd);
        }

        public bool SetOpticalPowerLessThanOrEqual15mWKi(LaserChannels channel, double current)
        {
            byte[] cmd = AvocadoProtocol.SetOpticalPowerLessThanOrEqual15mWKi(channel, current);
            return SendBytes(cmd);
        }
        public bool SetOpticalPowerLessThanOrEqual15mWKd(LaserChannels channel, double current)
        {
            byte[] cmd = AvocadoProtocol.SetOpticalPowerLessThanOrEqual15mWKd(channel, current);
            return SendBytes(cmd);
        }
        public bool GetRadioDiodeVoltage(LaserChannels channel)
        {
            byte[] cmd = AvocadoProtocol.GetRadioDiodeVoltage(channel);
            return SendBytes(cmd);
        }

        public bool GetAPDGainCalVol(LaserChannels channel, int current)
        {
            byte[] cmd = AvocadoProtocol.GetAPDGainCalVol(channel, current);
            return SendBytes(cmd);
        }
        public bool GetRadioDiodeSlope(LaserChannels channel)
        {
            byte[] cmd = AvocadoProtocol.GetRadioDiodeSlope(channel);
            return SendBytes(cmd);
        }
        public bool SetRadioDiodeSlope(LaserChannels channel, double current)
        {
            byte[] cmd = AvocadoProtocol.SetRadioDiodeSlope(channel, current);
            return SendBytes(cmd);
        }
        public bool GetRadioDiodeConstant(LaserChannels channel)
        {
            byte[] cmd = AvocadoProtocol.GetRadioDiodeConstant(channel);
            return SendBytes(cmd);
        }
        public bool GetCurrentValuelaser(LaserChannels channel)
        {
            byte[] cmd = AvocadoProtocol.GetCurrentValuelaser(channel);
            return SendBytes(cmd);
        }

        public bool SetRadioDiodeConstant(LaserChannels channel, double current)
        {
            byte[] cmd = AvocadoProtocol.SetRadioDiodeConstant(channel, current);
            return SendBytes(cmd);
        }
        public bool GetLaserControlVoltageCurrent(LaserChannels channel, int currentPower)
        {
            byte[] cmd = AvocadoProtocol.GetLaserControlVoltageCurrent(channel, currentPower);
            return SendBytes(cmd);
        }
        public bool SetLaserControlVoltageCurrent(LaserChannels channel, double power, int LaserAIntensity)
        {
            byte[] cmd = AvocadoProtocol.SetLaserControlVoltageCurrent(channel, power, LaserAIntensity);
            return SendBytes(cmd);
        }

        public bool GetAllOpticalPowerControlKpUpperLimitLessThanOrEqual15()
        {
            byte[] cmd = AvocadoProtocol.GetAllOpticalPowerControlKpUpperLimitLessThanOrEqual15();
            return SendBytes(cmd);
        }
        public bool GetAllOpticalPowerControlKpUpperLimitLessThanOrEqual15(LaserChannels channel)
        {
            byte[] cmd = AvocadoProtocol.GetAllOpticalPowerControlKpUpperLimitLessThanOrEqual15(channel);
            return SendBytes(cmd);
        }
        public bool GetAllOpticalPowerControlKpDownLimitLessThanOrEqual15()
        {
            byte[] cmd = AvocadoProtocol.GetAllOpticalPowerControlKpDownLimitLessThanOrEqual15();
            return SendBytes(cmd);
        }
        public bool GetAllOpticalPowerControlKpDownLimitLessThanOrEqual15(LaserChannels channel)
        {
            byte[] cmd = AvocadoProtocol.GetAllOpticalPowerControlKpDownLimitLessThanOrEqual15(channel);
            return SendBytes(cmd);
        }
        public bool GetAllOpticalPowerControlKiUpperLimitLessThanOrEqual15()
        {
            byte[] cmd = AvocadoProtocol.GetAllOpticalPowerControlKiUpperLimitLessThanOrEqual15();
            return SendBytes(cmd);
        }
        public bool GetAllOpticalPowerControlKiUpperLimitLessThanOrEqual15(LaserChannels channel)
        {
            byte[] cmd = AvocadoProtocol.GetAllOpticalPowerControlKiUpperLimitLessThanOrEqual15(channel);
            return SendBytes(cmd);
        }
        public bool GetAllOpticalPowerControlKiDownLimitLessThanOrEqual15()
        {
            byte[] cmd = AvocadoProtocol.GetAllOpticalPowerControlKiDownLimitLessThanOrEqual15();
            return SendBytes(cmd);
        }
        public bool GetAllOpticalPowerControlKiDownLimitLessThanOrEqual15(LaserChannels channel)
        {
            byte[] cmd = AvocadoProtocol.GetAllOpticalPowerControlKiDownLimitLessThanOrEqual15(channel);
            return SendBytes(cmd);
        }
        public bool GetAllOpticalPowerControlKpUpperLimitLessThan15()
        {
            byte[] cmd = AvocadoProtocol.GetAllOpticalPowerControlKpUpperLimitLessThan15();
            return SendBytes(cmd);
        }
        public bool GetAllOpticalPowerControlKpUpperLimitLessThan15(LaserChannels channel)
        {
            byte[] cmd = AvocadoProtocol.GetAllOpticalPowerControlKpUpperLimitLessThan15(channel);
            return SendBytes(cmd);
        }
        public bool GetAllOpticalPowerControlKpDownLimitLessThan15()
        {
            byte[] cmd = AvocadoProtocol.GetAllOpticalPowerControlKpDownLimitLessThan15();
            return SendBytes(cmd);
        }
        public bool GetAllOpticalPowerControlKpDownLimitLessThan15(LaserChannels channel)
        {
            byte[] cmd = AvocadoProtocol.GetAllOpticalPowerControlKpDownLimitLessThan15(channel);
            return SendBytes(cmd);
        }
        public bool GetAllOpticalPowerControlKiUpperLimitLessThan15()
        {
            byte[] cmd = AvocadoProtocol.GetAllOpticalPowerControlKiUpperLimitLessThan15();
            return SendBytes(cmd);
        }
        public bool GetAllOpticalPowerControlKiUpperLimitLessThan15(LaserChannels channel)
        {
            byte[] cmd = AvocadoProtocol.GetAllOpticalPowerControlKiUpperLimitLessThan15(channel);
            return SendBytes(cmd);
        }
        public bool GetAllOpticalPowerControlKiDownLimitLessThan15()
        {
            byte[] cmd = AvocadoProtocol.GetAllOpticalPowerControlKiDownLimitLessThan15();
            return SendBytes(cmd);
        }
        public bool GetAllOpticalPowerControlKiDownLimitLessThan15(LaserChannels channel)
        {
            byte[] cmd = AvocadoProtocol.GetAllOpticalPowerControlKiDownLimitLessThan15(channel);
            return SendBytes(cmd);
        }
        public bool GetAllLaserMaximumCurrent()
        {
            byte[] cmd = AvocadoProtocol.GetAllLaserMaximumCurrent();
            return SendBytes(cmd);
        }
        public bool GetAllLaserMaximumCurrent(LaserChannels channel)
        {
            byte[] cmd = AvocadoProtocol.GetAllLaserMaximumCurrent(channel);
            return SendBytes(cmd);
        }
        public bool GetAllLaserMinimumCurrent()
        {
            byte[] cmd = AvocadoProtocol.GetAllLaserMinimumCurrent();
            return SendBytes(cmd);
        }
        public bool GetAllLaserMinimumCurrent(LaserChannels channel)
        {
            byte[] cmd = AvocadoProtocol.GetAllLaserMinimumCurrent(channel);
            return SendBytes(cmd);
        }

        public bool SetOpticalPowerControlKpUpperLimitLessThanOrEqual15(LaserChannels channel, double current)
        {
            byte[] cmd = AvocadoProtocol.SetOpticalPowerControlKpUpperLimitLessThanOrEqual15(channel, current);
            return SendBytes(cmd);
        }
        public bool SetOpticalPowerControlKpDownLimitLessThanOrEqual15(LaserChannels channel, double current)
        {
            byte[] cmd = AvocadoProtocol.SetOpticalPowerControlKpDownLimitLessThanOrEqual15(channel, current);
            return SendBytes(cmd);
        }
        public bool SetOpticalPowerControlKiUpperLimitLessThanOrEqual15(LaserChannels channel, double current)
        {
            byte[] cmd = AvocadoProtocol.SetOpticalPowerControlKiUpperLimitLessThanOrEqual15(channel, current);
            return SendBytes(cmd);
        }
        public bool SetOpticalPowerControlKiDownLimitLessThanOrEqual15(LaserChannels channel, double current)
        {
            byte[] cmd = AvocadoProtocol.SetOpticalPowerControlKiDownLimitLessThanOrEqual15(channel, current);
            return SendBytes(cmd);
        }
        public bool SetOpticalPowerControlKpUpperLimitLessThan15(LaserChannels channel, double current)
        {
            byte[] cmd = AvocadoProtocol.SetOpticalPowerControlKpUpperLimitLessThan15(channel, current);
            return SendBytes(cmd);
        }
        public bool SetOpticalPowerControlKpDownLimitLessThan15(LaserChannels channel, double current)
        {
            byte[] cmd = AvocadoProtocol.SetOpticalPowerControlKpDownLimitLessThan15(channel, current);
            return SendBytes(cmd);
        }
        public bool SetOpticalPowerControlKiUpperLimitLessThan15(LaserChannels channel, double current)
        {
            byte[] cmd = AvocadoProtocol.SetOpticalPowerControlKiUpperLimitLessThan15(channel, current);
            return SendBytes(cmd);
        }
        public bool SetOpticalPowerControlKiDownLimitLessThan15(LaserChannels channel, double current)
        {
            byte[] cmd = AvocadoProtocol.SetOpticalPowerControlKiDownLimitLessThan15(channel, current);
            return SendBytes(cmd);
        }
        public bool SetLaserMaximumCurrent(LaserChannels channel, double current)
        {
            byte[] cmd = AvocadoProtocol.SetLaserMaximumCurrent(channel, current);
            return SendBytes(cmd);
        }
        public bool SetLaserMinimumCurrent(LaserChannels channel, double current)
        {
            byte[] cmd = AvocadoProtocol.SetLaserMinimumCurrent(channel, current);
            return SendBytes(cmd);
        }

        public bool SetTECControlTemperature(LaserChannels channel, double current)
        {
            byte[] cmd = AvocadoProtocol.SetTECControlTemperature(channel, current);
            return SendBytes(cmd);
        }
        public bool SetWavelength(LaserChannels channel, int Wavelength)
        {
            byte[] cmd = AvocadoProtocol.SetWavelength(channel, Wavelength);
            return SendBytes(cmd);
        }
        public bool SetTECMaximumCurrent(LaserChannels channel, double current)
        {
            byte[] cmd = AvocadoProtocol.SetTECMaximumCurrent(channel, current);
            return SendBytes(cmd);
        }

        public bool SetTECControlKp(LaserChannels channel, double current)
        {
            byte[] cmd = AvocadoProtocol.SetTECControlKp(channel, current);
            return SendBytes(cmd);
        }
        public bool SetTECControlKi(LaserChannels channel, double current)
        {
            byte[] cmd = AvocadoProtocol.SetTECControlKi(channel, current);
            return SendBytes(cmd);
        }
        public bool SetTECControlKd(LaserChannels channel, double current)
        {
            byte[] cmd = AvocadoProtocol.SetTECControlKd(channel, current);
            return SendBytes(cmd);
        }
        #endregion
        #endregion Public Functions


        #region Firmware Upgrade Functions
        public bool UpgraderReadEpcsId()
        {
            byte[] cmd = { 0x6a, 0x01, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x6e };
            return SendBytes(cmd);
        }
        #endregion Firmware Upgrade Functions

        #region Private Functions
        private void CommandsHearingProcess()
        {
            int readSize = 0;
            byte[] readingBuf = new byte[1048576];

            try
            {
                while (true)
                {
                    if (_CommandSocket.Available > 0)
                    {
                        _ReceivedBytes += _CommandSocket.Available;

                        if (_CommandSocket.Available > readingBuf.Length)
                        {
                            readSize = readingBuf.Length;
                        }
                        else
                        {
                            readSize = _CommandSocket.Available;
                        }
                        _CommandSocket.Receive(readingBuf, 0, readSize, SocketFlags.None);
                        if (AvocadoProtocol.ResponseDecoding(readingBuf, readSize))
                        {
                            if (CommStatus == CommStatusTypes.Waiting)
                            {
                                CommStatus = CommStatusTypes.Received;
                            }
                        }
                    }
                }
            }
            catch(SocketException ex)
            {
                if (!_CommandSocket.Connected)
                {
                    return;
                }
            }
            catch(Exception ex)
            {
                //Disconnect();
            }
        }

        private void StreamHearingProcess()
        {
            int readSize = 0;
            byte[] readingBuf = new byte[1048576];

            try
            {
                while (IsConnected)
                {
                    if (_StreamSocket.Available > 0)
                    {
                        _ReceivedBytes += _StreamSocket.Available;

                        if (_StreamSocket.Available > ReceivingBuf.FreeSize)
                        {
                            readSize = ReceivingBuf.FreeSize;
                        }
                        else
                        {
                            readSize = _StreamSocket.Available;
                        }
                        _StreamSocket.Receive(readingBuf, 0, readSize, SocketFlags.None);
                        ReceivingBuf.WriteDataIn(readingBuf, 0, readSize);
                        OnReceivedScanningData?.Invoke();
                    }
                }
            }
            catch(SocketException ex)
            {
                if (!_StreamSocket.Connected)
                {
                    return;
                }
            }
            catch (Exception ex)
            {
                //Disconnect();
            }
        }

        private void CommandReceiveArgs_Completed(object sender, SocketAsyncEventArgs e)
        {
            if (e.SocketError != SocketError.Success) return;
            Socket socket = sender as Socket;
            string iPRemote = socket.RemoteEndPoint.ToString();

            // Console.WriteLine("Client : Successfully connected to the server [ {0} ] Success", iPRemote);

            SocketAsyncEventArgs receiveSAEA = new SocketAsyncEventArgs();
            byte[] readingBuf = new byte[1048576];
            receiveSAEA.SetBuffer(readingBuf, 0, readingBuf.Length);
            receiveSAEA.Completed += CommandReceiveSAEA_Completed;
            receiveSAEA.RemoteEndPoint = pt;
            socket.ReceiveAsync(receiveSAEA);

        }

        private void CommandReceiveSAEA_Completed(object sender, SocketAsyncEventArgs e)
        {
            if (e.SocketError == SocketError.OperationAborted) return;
            Socket socket = sender as Socket;
            if (e.SocketError == SocketError.Success && e.BytesTransferred > 0)
            {
                //byte[] readingBuf = new byte[1048576];
                int readSize = e.BytesTransferred;
                _ReceivedBytes += readSize;
                byte[] receiveBuffer = e.Buffer;
                //byte[] buffer = new byte[readSize];
                //Buffer.BlockCopy(receiveBuffer, 0, buffer, 0, readSize);
                if (AvocadoProtocol.ResponseDecoding(receiveBuffer, readSize))
                {
                    if (CommStatus == CommStatusTypes.Waiting)
                    {
                        CommStatus = CommStatusTypes.Received;
                    }
                }
                socket.ReceiveAsync(e);
            }
            else if (e.SocketError == SocketError.ConnectionReset && e.BytesTransferred == 0)
            {
                Console.WriteLine("Client: Server Disconnected ");
            }
            else
            {
                ErrorMessage = "Network Anomaly";
                return;
            }
        }

        private void StreamReceiveArgs_Completed(object sender, SocketAsyncEventArgs e)
        {
            if (e.SocketError != SocketError.Success)
            {
                IsConnected = false;
                return;
            }
            Socket socket = sender as Socket;
            string iPRemote = socket.RemoteEndPoint.ToString();

            // Console.WriteLine("Client : Successfully connected to the server [ {0} ] Success", iPRemote);

            SocketAsyncEventArgs StreamSAEA = new SocketAsyncEventArgs();
            byte[] readingBuf = new byte[1048576];
            StreamSAEA.SetBuffer(readingBuf, 0, readingBuf.Length);
            StreamSAEA.Completed += StreamReceiveSAEA_Completed;
            StreamSAEA.RemoteEndPoint = streamPt;
            socket.ReceiveAsync(StreamSAEA);

        }

        private void StreamReceiveSAEA_Completed(object sender, SocketAsyncEventArgs e)
        {
            if (e.SocketError == SocketError.OperationAborted) return;
            Socket socket = sender as Socket;
            if (e.SocketError == SocketError.Success && e.BytesTransferred > 0)
            {
                //byte[] readingBuf = new byte[1048576];
                int readSize = e.BytesTransferred;
                _ReceivedBytes += readSize;
                byte[] receiveBuffer = e.Buffer;
                //byte[] buffer = new byte[readSize];
                //Buffer.BlockCopy(receiveBuffer, 0, buffer, 0, readSize);
                ReceivingBuf.WriteDataIn(receiveBuffer, 0, readSize);
                OnReceivedScanningData?.Invoke();
                socket.ReceiveAsync(e);
            }
            else if (e.SocketError == SocketError.ConnectionReset && e.BytesTransferred == 0)
            {
                Console.WriteLine("Client: Server Disconnected ");
            }
            else
            {
                return;
            }
        }
        private void OnSendCompleted(object sender, SocketAsyncEventArgs e)
        {
            if (e.SocketError != SocketError.Success) return;
            Socket socket = sender as Socket;
            byte[] sendBuffer = e.Buffer;

            string sendMsg = Encoding.Unicode.GetString(sendBuffer);

            Console.WriteLine("Client : Send message [ {0} ] to Serer[ {1} ]", sendMsg, socket.RemoteEndPoint.ToString());
        }
        #endregion Private Functions
    }
}
