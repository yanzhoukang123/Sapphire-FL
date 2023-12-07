using Azure.CommandLib;
using Azure.EthernetCommLib;
using Azure.Image.Processing;
using Azure.MotionLib;
using Azure.WPF.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace Azure.ImagingSystem
{
    //public enum ScanTypes
    //{
    //    Unknown,
    //    Static,
    //    Vertical,
    //    Horizontal,
    //    XAxis,
    //}
    public class LaserScanCommand : ThreadBase
    {
        #region Delegate/Event handler

        public delegate void ReceivedScanDataHandle(object sender, string dataName);
        public event ReceivedScanDataHandle OnScanDataReceived;

        //public delegate void SpentTimeReceivedScanDataHandle();
        //public event SpentTimeReceivedScanDataHandle OnSpentTimeScanDataReceived;

        // Image scanning status delegate
        public delegate void CommandStatusHandler(object sender, string status);
        // Image scanning status event
        public event CommandStatusHandler CommandStatus;
        public delegate void CommandCompletionEstHandler(ThreadBase sender, DateTime dateTime, double estTime, double percentCompleted);
        // Image capture completion time estimate event
        public event CommandCompletionEstHandler CompletionEstimate;
        // update event while APD receives data
        //public delegate void ScanReceiveDataHandler(ThreadBase sender, string scanType);
        //public event ScanReceiveDataHandler ReceiveTransfer;

        //public delegate void DataReceivedHandler(ushort[] apdChannelA, ushort[] apdChannelB, ushort[] apdChannelC, ushort[] apdChannelD);
        //public event DataReceivedHandler DataReceived;
        public delegate void DataReceivedHandler(object sender);
        public event DataReceivedHandler DataReceived;

        //public delegate void SmartScanUpdateSignalHandler(ThreadBase sender);
        //public event SmartScanUpdateSignalHandler SmartScanUpdateSignalLevels;
        public delegate void SmartScanStartingHandler(ThreadBase sender);
        public event SmartScanStartingHandler SmartScanStarting;
        public delegate void SmartScanUpdatedHandler(ThreadBase sender);
        public event SmartScanUpdatedHandler SmartScanUpdated;
        public delegate void SmartScanCompletedHandler(ThreadBase sender);
        public event SmartScanCompletedHandler SmartScanCompleted;
        //public delegate void EDRTestScanStartingHandler(ThreadBase sender);
        //public event EDRTestScanStartingHandler EDRTestScanStarting;
        public delegate void EDRTestScanUpdatingHandler(ThreadBase sender);
        public event EDRTestScanUpdatingHandler EDRTestScanUpdating;
        //public delegate void EDRTestScanCompletedHandler(ThreadBase sender);
        //public event EDRTestScanCompletedHandler EDRTestScanCompleted;

        public delegate void SequentialChannelStartingHandler(ThreadBase sender, LaserChannels channel);
        public event SequentialChannelStartingHandler SequentialChannelStarting;
        public delegate void SequentialChannelCompletedHandler(ThreadBase sender, LaserChannels channel);
        public event SequentialChannelCompletedHandler SequentialChannelCompleted;

        public delegate void ZScanningCompletedHandler(ThreadBase sender, ImageInfo imageInfo, int nScanRegion);
        public event ZScanningCompletedHandler ZScanningCompleted;

        public delegate void ScanRegionCompletedHandler(ThreadBase sender, ImageInfo imageInfo, int nScanRegion);
        public event ScanRegionCompletedHandler ScanRegionCompleted;

        //private bool _IsScanning = false;

        #endregion

        #region Private Fields

        private Dispatcher _CallingDispatcher = null;
        private EthernetController _CommController;
        private MotionController _MotionController;
        //private ScanParameterStruct _ScanSettings;
        private Logger _Logger = null;
        private const int PACKSIZE = 1286;
        private const int FramesPerPackage = 80;
        //public ushort[] _APDChannelA;
        //public ushort[] _APDChannelB;
        //public ushort[] _APDChannelC;
        private WriteableBitmap _ChannelAImage = null;  //L1
        private WriteableBitmap _ChannelBImage = null;  //R1
        private WriteableBitmap _ChannelCImage = null;  //R2
        private WriteableBitmap _ChannelAImageEDR1 = null;  //L1
        private WriteableBitmap _ChannelBImageEDR1 = null;  //R1
        private WriteableBitmap _ChannelCImageEDR1 = null;  //R2
        //User's UI display image buffer
        private IntPtr _ChannelABackBuffer;
        private IntPtr _ChannelBBackBuffer;
        private IntPtr _ChannelCBackBuffer;

        private List<ScanParameterStruct> _ScanParams = null;
        private ScanParameterStruct _CurrentScanParam = null;
        private bool _IsSmartScanCalc = false;
        private bool _IsMultiRegionScan = false;
        private int _CurrentScanRegion = 0;
        private int _ZSequentialSetIndex = 0;
        private ImageProcessing.FlipAxis _FlipAxis = ImageProcessing.FlipAxis.None;
        private ImageInfo _ImageInfo;
        //private int _ImageWidth = 0;
        //private int _ImageHeight = 0;

        private bool _IsPhosphorImaging = false;
        private bool _IsChannelASelected = false;
        private bool _IsChannelBSelected = false;
        private bool _IsChannelCSelected = false;
        private bool _IsApplyImageSmoothing = false;   // 800 channel image smoothing
        //private int _ZSequentialSetIndex = 0;
        private double _ActualImageDPI = 300;
        //private bool _IsSmartScanCalc = false;
        //private bool _IsMultiRegionScan = false;
        //private int _CurrentScanRegion = 0;
        //ImageProcessing.FlipAxis _FlipAxis = ImageProcessing.FlipAxis.None;
        private bool _IsScanAborted = false;
        private const int _SaturationThreshold = 62000;
        private Dictionary<LaserChannels, string> _LaserChannelDict;
        private bool _IsEdrScanCalc = false;
        private bool _IsSaveDebuggingImages = false;
        private bool _IsSaveEdrAs24bit = false;
        private string _DBTIPath = string.Empty;
        private int _L1Wavelength = 0;
        private int _R1Wavelength = 0;
        private int _R2Wavelength = 0;

        #endregion Private Fields

        #region Constructors

        public LaserScanCommand(Dispatcher callingDispatcher,
                                EthernetController ethernet,
                                MotionController motion,
                                List<ScanParameterStruct> scanParams,
                                bool bIsPhosphorImaging,
                                bool bIsApplyImageSmooth,
                                string commonDataPath,
                                ImageProcessing.FlipAxis flipAxis = ImageProcessing.FlipAxis.None,
                                bool bIsSaveDebuggingImages = false,
                                bool bIsSaveEdrAs24bit = false)
        {
            _CallingDispatcher = callingDispatcher;
            _CommController = ethernet;
            _MotionController = motion;
            _ScanParams = scanParams;
            _IsMultiRegionScan = (scanParams.Count > 1);
            _IsPhosphorImaging = bIsPhosphorImaging;
            _IsApplyImageSmoothing = bIsApplyImageSmooth;
            _FlipAxis = flipAxis;   //user specified flip operation
            _IsSaveDebuggingImages = bIsSaveDebuggingImages;
            _IsSaveEdrAs24bit = bIsSaveEdrAs24bit;

            //PPI= 1/(Pixel size in microns) x 2.54 cm/inch x 10,000 microns/cm
            // Get the scanned image DPI
            //_ActualImageDPI = ((250 * 1000) / _ScanParams[0].Res / 25) * 2.54;

            //if (Channel.Length > 0)
            //{
            //    _AChannelPtr = Channel[0];//scan _ChannelAImage 
            //    _BChannelPtr = Channel[1];//scan _ChannelBImage 
            //    _CChannelPtr = Channel[2];//scan _ChannelCImage 
            //}

            _LaserChannelDict = new Dictionary<LaserChannels, string>();
            _LaserChannelDict.Add(LaserChannels.ChannelA, "R1");
            _LaserChannelDict.Add(LaserChannels.ChannelB, "R2");
            _LaserChannelDict.Add(LaserChannels.ChannelC, "L");

            if (_IsSaveDebuggingImages && !string.IsNullOrEmpty(commonDataPath))
            {
                _DBTIPath = System.IO.Path.Combine(commonDataPath, "DBTI");
                if (System.IO.Directory.Exists(_DBTIPath))
                {
                    var dir = new System.IO.DirectoryInfo(_DBTIPath);
                    foreach (var file in dir.EnumerateFiles("*.tif"))
                    {
                        try
                        {
                            file.Delete();
                        }
                        catch
                        {
                        }
                    }
                }
                else
                {
                    try
                    {
                        System.IO.Directory.CreateDirectory(_DBTIPath);
                    }
                    catch
                    {
                    }
                }
            }
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// The scanning was aborted (either manually be the user or automatically (i.e., when lid opened)
        /// </summary>
        public bool IsScanAborted
        {
            get { return _IsScanAborted; }
        }

        public Logger Logger
        {
            get { return _Logger; }
            set { _Logger = value; }
        }
        public void LogMessage(string msg)
        {
            if (_Logger == null) { return; }
            _Logger.LogMessage(msg);
        }
        public bool IsKeepRawImages { get; set; }
        public WriteableBitmap ChannelARawImage { get; set; } = null;   //L1 raw image
        public WriteableBitmap ChannelBRawImage { get; set; } = null;   //R1 raw image
        public WriteableBitmap ChannelCRawImage { get; set; } = null;   //R2 raw image

        public int[] SampleIndex { get; private set; }
        public int[] SampleValueChannelA { get; private set; }
        public int[] SampleValueChannelB { get; private set; }
        public int[] SampleValueChannelC { get; private set; }
        public ScanTypes ScanType { get; private set; }
        public int RemainingTime { get; private set; }
        public int XCalibratingSpeed { get; set; }
        public string ApplicationDataPath { get; set; }
        public List<Signal> Signals => _CurrentScanParam.Signals;
        public ScanParameterStruct CurrentScanParam => _CurrentScanParam;
        public List<ScanParameterStruct> ScanParams => _ScanParams;
        public int CurrentScanRegion => _CurrentScanRegion;
        public ImageInfo ImageInfo => _ImageInfo;
        public bool IsPhosphorImaging { get; set; }
        public bool IsSaveDataOnScanAbort { get; set; } = false;
        public int ZSequentialSetIndex => _ZSequentialSetIndex;
        /// <summary>
        /// Updated signal level (uses by SmartScan and EDR scan)
        /// </summary>
        public List<Signal> UpdatedSignalLevel { get; set; } = new List<Signal>();

        //public ushort[] APDChannelA => _APDChannelA;
        //public ushort[] APDChannelB => _APDChannelB;
        //public ushort[] APDChannelC => _APDChannelC;

        public WriteableBitmap ChannelAImage => _ChannelAImage;
        public WriteableBitmap ChannelBImage => _ChannelBImage;
        public WriteableBitmap ChannelCImage => _ChannelCImage;

        //Display image buffers
        public IntPtr ChannelABackBuffer
        {
            get { return _ChannelABackBuffer; }
            set { _ChannelABackBuffer = value; }
        }
        public IntPtr ChannelBBackBuffer
        {
            get { return _ChannelBBackBuffer; }
            set { _ChannelBBackBuffer = value; }
        }
        public IntPtr ChannelCBackBuffer
        {
            get { return _ChannelCBackBuffer; }
            set { _ChannelCBackBuffer = value; }
        }
        public bool IsChannelASelected => _IsChannelASelected;
        public bool IsChannelBSelected => _IsChannelBSelected;
        public bool IsChannelCSelected => _IsChannelCSelected;

        public bool IsSmartScanCalc => _IsSmartScanCalc;
        public bool IsEdrScanCalc => _IsEdrScanCalc;
        public bool IsEdrSaturationAbort { get; set; }
        public List<LaserChannels> EdrSaturatedChannels { get; set; }
        public bool IsDespeckleSmartScan { get; set; } = false;

        #endregion Public Properties


        public override void ThreadFunction()
        {
            _CurrentScanRegion = 0;

            // Scan each scan region
            for (int index = 0; index < _ScanParams.Count; index++)
            {
                _CurrentScanParam = _ScanParams[index];
                _CurrentScanRegion = index;
                _ImageInfo = new ImageInfo();
                //DateTime dateTime = DateTime.Now;
                //ImageInfo.DateTime = System.String.Format("{0:G}", dateTime.ToString());
                _IsChannelASelected = false;
                _IsChannelBSelected = false;
                _IsChannelCSelected = false;

                CommandStatus?.Invoke(this, string.Format("Preparing to scan....scan region {0} / {1}", index + 1, _ScanParams.Count));

                foreach (Signal signal in _CurrentScanParam.Signals)
                {
                    switch (signal.LaserChannel)
                    {
                        case LaserChannels.ChannelA:    //R1
                            _IsChannelASelected = true;
                            _R1Wavelength = signal.LaserWavelength;
                            break;
                        case LaserChannels.ChannelB:    //R2
                            _IsChannelBSelected = true;
                            _R2Wavelength = signal.LaserWavelength;
                            break;
                        case LaserChannels.ChannelC:    //L1
                            _IsChannelCSelected = true;
                            _L1Wavelength = signal.LaserWavelength;
                            break;
                    }
                }

                try
                {
                    if (_CurrentScanParam.ScanDeltaX == 0 && _CurrentScanParam.ScanDeltaY == 0)
                    {
                        ScanType = ScanTypes.Vertical;
                        VerticalScanProcess();
                    }
                    else
                    {
                        if (_CurrentScanParam.IsSmartScanning && !_CurrentScanParam.IsZScanning && !_CurrentScanParam.IsEdrScanning)
                        {
                            SmartscanProcess();
                        }
                        else if (_CurrentScanParam.IsSequentialScanning && !_CurrentScanParam.IsZScanning && !_CurrentScanParam.IsEdrScanning)
                        {
                            SequentialScanProcess();
                        }
                        else if (_CurrentScanParam.IsZScanning)
                        {
                            ZScanningProcess();
                        }
                        else if (_CurrentScanParam.IsEdrScanning)
                        {
                            EDRScanningProcess();
                        }
                        else
                        {
                            HorizontalScanProcess();

                            StopScan();

                            if (!_IsScanAborted)
                            {
                                // Process the current completed scan region
                                ProcessCompletedScanRegion();
                            }
                        }

                        // ZScanning already processed the scan image(s) on each Z layer.
                        //if (!_CurrentScanParam.IsZScanning)
                        //{
                        //    StopScan();
                        //
                        //    // Process the current completed scan region
                        //    ProcessCompletedScanRegion();
                        //}

                        //int currScanRegion = _CurrentScanRegion;
                        // Don't automatically save the image(s) when the scanning is aborted; will prompt to save the scan data
                        if (!_IsScanAborted)
                        {
                            if (ScanRegionCompleted != null)
                            {
                                ScanRegionCompleted?.Invoke(this, (ImageInfo)_ImageInfo.Clone(), _CurrentScanRegion);
                                Thread.Sleep(1000);
                            }
                        }
                    }
                }
                catch (System.Threading.ThreadAbortException)
                {
                    // don't throw exception if the user abort the process.
                }
                catch (System.Runtime.InteropServices.SEHException)
                {
                    // The SEHException class handles SEH errors that are thrown from unmanaged code,
                    // but have not been mapped to another .NET Framework exception.

                    throw new OutOfMemoryException();
                }
                catch (System.Runtime.InteropServices.COMException cex)
                {
                    if (cex.ErrorCode == unchecked((int)0x88980003))
                    {
                        throw new OutOfMemoryException();
                    }
                    else
                    {
                        throw cex;
                    }
                }
                catch (Exception ex)
                {
                    string message = ex.Message + "\n\nStack Trace:\n" + ex.StackTrace;
                    LogMessage(message);
                    throw;
                }
                //finally
                //{
                //    // Forces an immediate garbage collection.
                //    //GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced);
                //    //GC.WaitForPendingFinalizers();
                //    //GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced);
                //}
            }
        }

        public override void AbortWork()
        {
            _IsScanAborted = true;
            StopScan();
        }

        public override void Finish()
        {
            /*if (_CommController.IsConnected)
            {
                TurnOffAllLasers();

                if (!_IsScanAborted)    //StopScan already called
                {
                    _CommController.StopScan();
                }
                //_CommController.SetLaserPower(LaserChannels.ChannelA, 0);
                //_CommController.SetLaserPower(LaserChannels.ChannelB, 0);
                //_CommController.SetLaserPower(LaserChannels.ChannelC, 0);
                //_MotionController.AutoQuery = true;

                _MotionController.SetStart(MotorTypes.X | MotorTypes.Y | MotorTypes.Z, new bool[] { false, false, false });
            }*/

            // Stop the scan
            StopScan();

            // Reset PMT gain to 4000 (value used in EUI)
            foreach (var scanParam in _ScanParams)
            {
                foreach (var signal in scanParam.Signals)
                {
                    if (signal.SensorType == IvSensorType.PMT)
                    {
                        if (signal.LaserChannel == LaserChannels.ChannelA)
                        {
                            _CommController.SetIvPmtGain(IVChannels.ChannelA, 4000);
                        }
                        else if (signal.LaserChannel == LaserChannels.ChannelB)
                        {
                            _CommController.SetIvPmtGain(IVChannels.ChannelB, 4000);
                        }
                        else if (signal.LaserChannel == LaserChannels.ChannelC)
                        {
                            _CommController.SetIvPmtGain(IVChannels.ChannelC, 4000);
                        }
                    }
                }
            }

            // Process/save the scanned data on abort
            if (_IsScanAborted && IsSaveDataOnScanAbort)
            {
                ProcessCompletedScanRegion();
            }

            if (ExitStat == ThreadExitStat.None)
            {
                if (ScanType == ScanTypes.Static || ScanType == ScanTypes.Vertical || ScanType == ScanTypes.XAxis)
                {
                    try
                    {
                        string fileName = string.Empty;
                        switch (ScanType)
                        {
                            case ScanTypes.Static:
                                fileName = "StaticScanData.csv";
                                break;
                            case ScanTypes.Vertical:
                                fileName = "VerticalScanData.csv";
                                break;
                            case ScanTypes.XAxis:
                                fileName = "XScanData.csv";
                                break;
                        }
                        string[] testdata = new string[SampleValueChannelA.Length];
                        for (int y = 0; y < SampleValueChannelA.Length; y++)
                        {
                            testdata[y] = SampleIndex[y].ToString() + "," +
                                SampleValueChannelA[y].ToString() + "," +
                                SampleValueChannelB[y].ToString() + "," +
                                SampleValueChannelC[y].ToString();
                        }
                        string filePath = System.IO.Path.Combine(ApplicationDataPath, fileName);
                        File.WriteAllLines(filePath, testdata, System.Text.Encoding.UTF8);
                    }
                    catch (System.Exception ex)
                    {
                        throw ex;
                    }
                }
                else if (ScanType == ScanTypes.Horizontal)
                {
                    try
                    {
                        //System.Windows.Int32Rect rect = new System.Windows.Int32Rect();
                        //rect.X = 0;
                        //rect.Width = ChannelAImage.PixelWidth;
                        //rect.Height = 1;
                        //for (int row = 0; row < ChannelAImage.PixelHeight; row++)
                        //{
                        //    rect.Y = row;
                        //    ChannelAImage.WritePixels(rect, _APDChannelA, ChannelAImage.BackBufferStride, row * ChannelAImage.PixelWidth * 2);
                        //    ChannelBImage.WritePixels(rect, _APDChannelB, ChannelBImage.BackBufferStride, row * ChannelBImage.PixelWidth * 2);
                        //    ChannelCImage.WritePixels(rect, _APDChannelC, ChannelCImage.BackBufferStride, row * ChannelCImage.PixelWidth * 2);
                        //}
                        //rect.X = 0;
                        //rect.Y = 0;
                        //rect.Width = ChannelAImage.PixelWidth;
                        //rect.Height = ChannelAImage.PixelHeight;
                        //ChannelAImage.WritePixels(rect, _APDChannelA, ChannelAImage.BackBufferStride, 0);
                        //ChannelBImage.WritePixels(rect, _APDChannelB, ChannelBImage.BackBufferStride, 0);
                        //ChannelCImage.WritePixels(rect, _APDChannelC, ChannelCImage.BackBufferStride, 0);
                        //ChannelAImage.AddDirtyRect(new System.Windows.Int32Rect(0, 0, ChannelAImage.PixelWidth, ChannelAImage.PixelHeight));
                        //ChannelBImage.AddDirtyRect(new System.Windows.Int32Rect(0, 0, ChannelBImage.PixelWidth, ChannelBImage.PixelHeight));
                        //ChannelCImage.AddDirtyRect(new System.Windows.Int32Rect(0, 0, ChannelCImage.PixelWidth, ChannelCImage.PixelHeight));
                        //ChannelAImage.Unlock();
                        //ChannelBImage.Unlock();
                        //ChannelCImage.Unlock();
                        //ChannelAImage.Freeze();
                        //ChannelBImage.Freeze();
                        //ChannelCImage.Freeze();
                    }
                    catch (Exception)
                    {
                        ExitStat = ThreadExitStat.Error;
                    }
                }
            }
        }

        private void StaticScanProcess()
        {
            byte[] tempBuf = new byte[PACKSIZE];
            int index = 0;
            SampleIndex = new int[_CurrentScanParam.LineCounts];
            SampleValueChannelA = new int[_CurrentScanParam.LineCounts];
            SampleValueChannelB = new int[_CurrentScanParam.LineCounts];
            SampleValueChannelC = new int[_CurrentScanParam.LineCounts];
            _CommController.ReceivingBuf.Reset();
            //_MotionController.AutoQuery = false;
            _CommController.TriggerTimingScan((uint)_CurrentScanParam.DataRate);
            while (true)
            {
                while (_CommController.ReceivingBuf.StoredSize < PACKSIZE)
                {
                    Thread.Sleep(1);
                }
                _CommController.ReceivingBuf.ReadDataOut(tempBuf, 0, PACKSIZE);

                if (tempBuf[0] == 0x6b && tempBuf[1] == 0x01 && tempBuf[2] == 0x03 && tempBuf[3] == 0x08 && tempBuf[1285] == 0x6f)
                {
                    for (int i = 0; i < FramesPerPackage; i++)
                    {
                        index = BitConverter.ToInt32(tempBuf, 4 + i * 16) - 1;
                        if (index >= _CurrentScanParam.LineCounts)
                        {
                            return;
                        }
                        SampleIndex[index] = index * _CurrentScanParam.DataRate;
                        SampleValueChannelA[index] = BitConverter.ToInt32(tempBuf, 8 + i * 16);
                        SampleValueChannelB[index] = BitConverter.ToInt32(tempBuf, 12 + i * 16);
                        SampleValueChannelC[index] = BitConverter.ToInt32(tempBuf, 16 + i * 16);
                    }
                }
            }
        }

        private void VerticalScanProcess()
        {
            byte[] tempBuf = new byte[PACKSIZE];
            int index = 0;
            int samplePts = _CurrentScanParam.ScanDeltaZ / 8;   // every 8 z clocks generating 1 sample result
            SampleIndex = new int[samplePts];
            SampleValueChannelA = new int[samplePts];
            SampleValueChannelB = new int[samplePts];
            SampleValueChannelC = new int[samplePts];
            if (PresetMotion(60000) == false)       // wait 1 minutes for the motions to be at the starting positions
            {
                ExitStat = ThreadExitStat.Error;
                throw new Exception("Failed to move the motions to the start positions");
            }
            _CommController.ReceivingBuf.Reset();
            int zPulses = _CurrentScanParam.ScanDeltaZ + 80 * 8;    // go more 80 sample points to finish all data transport
            _MotionController.AbsoluteMoveSingleMotion(MotorTypes.Z, 256, _CurrentScanParam.ZMotorSpeed, _CurrentScanParam.ZMotionAccVal, _CurrentScanParam.ZMotionDccVal, zPulses, true, false);
            //_MotionController.AutoQuery = false;
            _CommController.TriggerZScan((uint)_CurrentScanParam.ScanDeltaZ);

            int offset = 0;
            while (true)
            {
                while (_CommController.ReceivingBuf.StoredSize < PACKSIZE)
                {
                    Thread.Sleep(1);
                }
                _CommController.ReceivingBuf.ReadDataOut(tempBuf, 0, PACKSIZE);

                if (tempBuf[0] == 0x6b && tempBuf[1] == 0x01 && tempBuf[2] == 0x03 && tempBuf[3] == 0x08 && tempBuf[1285] == 0x6f)
                {
                    for (int i = 0; i < FramesPerPackage; i++)
                    {
                        offset = i * 16;
                        index = (BitConverter.ToInt32(tempBuf, 4 + offset) - 1) / 8;
                        if (index >= samplePts)
                        {
                            return;
                        }
                        SampleIndex[index] = index;
                        SampleValueChannelA[index] = BitConverter.ToInt32(tempBuf, 8 + offset) / 8;
                        SampleValueChannelB[index] = BitConverter.ToInt32(tempBuf, 12 + offset) / 8;
                        SampleValueChannelC[index] = BitConverter.ToInt32(tempBuf, 16 + offset) / 8;
                    }
                }
            }
        }

        private unsafe void HorizontalScanProcess()
        {
            ScanType = ScanTypes.Horizontal;
            LogMessage("HorizontalScanProcess starting...");
            string unidirectionScan = string.Empty;
            unidirectionScan = (_CurrentScanParam.IsUnidirectionalScan) ? "Yes" : "No";
            LogMessage("IsUnidirectionalScan: " + unidirectionScan);

            try
            {
                OnScanDataReceived?.Invoke(this, "ScanningPrepStarted");
                CommandStatus?.Invoke(this, string.Format("Preparing to scan....scan region {0} / {1}", _CurrentScanRegion + 1, _ScanParams.Count));

                if (PresetMotion(60000) == false)       // wait 1 minutes for the motions to be at the starting positions
                {
                    ExitStat = ThreadExitStat.Error;
                    throw new Exception("Failed to move the motions to the start positions");
                }
                _CommController.ReceivingBuf.Reset();

                // Set lasers settings
                SetLaserPower();

                DateTime dateTime = DateTime.Now;
                _ImageInfo.DateTime = System.String.Format("{0:G}", dateTime.ToString());

                LogMessage("Setting scanning motion parameters...");

                #region Step1: Set Scanning Motion Parameters
                double xExtraMove = _CurrentScanParam.XMotionExtraMoveLength;
                double yExtraMove = _CurrentScanParam.YMotionExtraMoveLength;
                int yOverscanRows = (int)Math.Round(yExtraMove * 1000.0 / _CurrentScanParam.Res);
                int singleTrip = _CurrentScanParam.ScanDeltaX + (int)(xExtraMove * 2 * _CurrentScanParam.XMotorSubdivision);
                //double singleTripTime = _ScanSettings.Quality / 2.0 - _ScanSettings.XmotionTurnAroundDelay / 1000.0;
                double singleTripTime = _CurrentScanParam.Quality / 2.0;
                //Calculate the X-axis velocity
                int xMotorSpeed = XMotorSpeedCalibration.GetSpeed(_CurrentScanParam.XMotionAccVal, 256, singleTrip, singleTripTime);
                //when the motor is ready,switch APD on
                //Calculate the Y-axis velocity
                int speedY = 0;
                if (_CurrentScanParam.IsUnidirectionalScan)
                {
                    speedY = (int)Math.Round(_CurrentScanParam.Res * _CurrentScanParam.YMotorSubdivision / _CurrentScanParam.Quality / 1000.0);
                }
                else
                {
                    speedY = (int)Math.Round(_CurrentScanParam.Res * 2 * _CurrentScanParam.YMotorSubdivision / _CurrentScanParam.Quality / 1000.0);
                }

                int tgtPosY = _CurrentScanParam.ScanY0 + _CurrentScanParam.ScanDeltaY + (int)Math.Round(yExtraMove / 2.0 * _CurrentScanParam.YMotorSubdivision);
                int tgtPosX1 = _CurrentScanParam.ScanX0 + _CurrentScanParam.ScanDeltaX + (int)Math.Round(xExtraMove * _CurrentScanParam.XMotorSubdivision);
                int tgtPosX2 = _CurrentScanParam.ScanX0 - (int)Math.Round(xExtraMove * _CurrentScanParam.XMotorSubdivision);
                int repeats = _CurrentScanParam.Height / 2 + 10;
                if (tgtPosX2 < 0) { tgtPosX2 = 0; }
                //int IsUnidirectiona = 0;
                if (_CurrentScanParam.IsUnidirectionalScan)
                {
                    // The motion speed of Y axis will be twice slower, so the motion distance is the same, dy is the height of the pixel * 2
                    tgtPosY = _CurrentScanParam.ScanY0 + (_CurrentScanParam.ScanDeltaY * 2) + (int)Math.Round(yExtraMove / 2.0 * _CurrentScanParam.YMotorSubdivision);
                    // X Number of motor round trips
                    repeats = _CurrentScanParam.Height + 10;
                }

                LogMessage("Setting X and Y axis speed, and start and stop position.");
                LogMessage(string.Format("X speed: {0}", xMotorSpeed));
                LogMessage(string.Format("Y speed: {0}", speedY));
                LogMessage(string.Format("Target Y position: {0}", tgtPosY));
                LogMessage(string.Format("Target X1: {0}, Target X2: {1}", tgtPosX1, tgtPosX2));

                //Set X and Y axis speed, start and stop position
                _MotionController.AbsoluteMoveSingleMotion(MotorTypes.Y, 256, speedY, _CurrentScanParam.YMotionAccVal, _CurrentScanParam.YMotionDccVal, tgtPosY, false, false);
                _MotionController.AbsoluteMoveSingleMotion(MotorTypes.X, 256, xMotorSpeed, _CurrentScanParam.XMotionAccVal,
                    _CurrentScanParam.XMotionDccVal, tgtPosX1, tgtPosX2, repeats, (int)(singleTripTime * 1000), false, false);
                //_MotionController.AutoQuery = false;
                #endregion Step1: Set Scanning Motion Parameters

                // These info added in ProcessCompletedScanRegion()
                //#region Step2: Set ImageInfo
                //DateTime dateTime = DateTime.Now;
                //_ImageInfo = new Azure.Image.Processing.ImageInfo();
                //ImageInfo.DateTime = System.String.Format("{0:G}", dateTime.ToString());
                //ImageInfo.CaptureType = "Scanner";
                //ImageInfo.IsScannedImage = true;
                //ImageInfo.ScanResolution = _CurrentScanParam.Res;
                //ImageInfo.ScanQuality = _CurrentScanParam.Quality;
                //ImageInfo.ScanX0 = _CurrentScanParam.ScanX0;
                //ImageInfo.ScanY0 = _CurrentScanParam.ScanY0;
                //ImageInfo.DeltaX = (int)((double)_CurrentScanParam.ScanDeltaX / (double)_CurrentScanParam.XMotorSubdivision);
                //ImageInfo.DeltaY = (int)((double)_CurrentScanParam.ScanDeltaY / (double)_CurrentScanParam.YMotorSubdivision);
                //ImageInfo.ScanZ0 = (double)_CurrentScanParam.ScanZ0 / (double)_CurrentScanParam.ZMotorSubdivision;
                //#endregion Step2: Set ImageInfo

                //_IsChannelASelected = false;
                //_IsChannelBSelected = false;
                //_IsChannelCSelected = false;
                //foreach (Signal signal in _CurrentScanParam.Signals)
                //{
                //    switch(signal.LaserChannel)
                //    {
                //        case LaserChannels.ChannelA:    //R1
                //            _IsChannelASelected = true;
                //            break;
                //        case LaserChannels.ChannelB:    //R2
                //            _IsChannelBSelected = true;
                //            break;
                //        case LaserChannels.ChannelC:    //L1
                //            _IsChannelCSelected = true;
                //            break;
                //    }
                //}

                // Flags used in Smartscan to turn off the laser when there are saturated pixel(s)
                bool bIsLaserL1Saturated = false;
                bool bIsLaserR1Saturated = false;
                bool bIsLaserR2Saturated = false;
                bool bIsLaserL1TurnedOff = false;
                bool bIsLaserR1TurnedOff = false;
                bool bIsLaserR2TurnedOff = false;
                // First saturated row number
                int nLaserL1SaturatedRow = 0;
                int nLaserR1SaturatedRow = 0;
                int nLaserR2SaturatedRow = 0;

                #region Step3: Start Scanning
                byte[] tempBuf = new byte[PACKSIZE];
                int coordX = 0;
                int coordY = 0;
                int index = 0;
                int bufferWidth = _CurrentScanParam.BackBufferStride;
                int Length = 0;
                ushort* pBufChannelA16 = null;
                ushort* pBufChannelB16 = null;
                ushort* pBufChannelC16 = null;
                ushort* pBufChannelADI16 = null;
                ushort* pBufChannelBDI16 = null;
                ushort* pBufChannelCDI16 = null;
                int pixelWidth = 0;
                int pixelHeight = 0;

                if (_CurrentScanParam.Is5micronScan)
                {
                    // When the user select 5um scan - we scan at 10um then resize the image to 5um pixel dimension
                    _ActualImageDPI = ((250 * 1000) / 5 / 25) * 2.54;
                }
                else
                {
                    _ActualImageDPI = ((250 * 1000) / _ScanParams[0].Res / 25) * 2.54;
                }

                int height = _CurrentScanParam.Height;
                if (_CurrentScanParam.IsUnidirectionalScan)
                {
                    // Unidirectional scan param setup double the height to synchronize the scanner's LED progress bar
                    // Actual scan height
                    height = _CurrentScanParam.Height / 2;
                }

                if (_IsChannelASelected)
                {
                    _ChannelAImage = new WriteableBitmap(_CurrentScanParam.Width, height, _ActualImageDPI, _ActualImageDPI, System.Windows.Media.PixelFormats.Gray16, null);
                    pBufChannelA16 = (ushort*)(byte*)_ChannelAImage.BackBuffer.ToPointer();
                    bufferWidth = _ChannelAImage.BackBufferStride;
                    bufferWidth /= 2;
                    Length = bufferWidth * height;
                    pixelWidth = _ChannelAImage.PixelWidth;
                    pixelHeight = _ChannelAImage.PixelHeight; 
                    //_APDChannelA = new ushort[bufferWidth * _CurrentScanParam.Height];
                    if (_ChannelABackBuffer != null)
                    {
                        pBufChannelADI16 = (ushort*)(byte*)_ChannelABackBuffer.ToPointer();
                    }
                }
                if (_IsChannelBSelected)
                {
                    _ChannelBImage = new WriteableBitmap(_CurrentScanParam.Width, height, _ActualImageDPI, _ActualImageDPI, System.Windows.Media.PixelFormats.Gray16, null);
                    pBufChannelB16 = (ushort*)(byte*)(void*)_ChannelBImage.BackBuffer.ToPointer();
                    bufferWidth = _ChannelBImage.BackBufferStride;
                    bufferWidth /= 2;
                    Length = bufferWidth * height;
                    pixelWidth = _ChannelBImage.PixelWidth;
                    pixelHeight = _ChannelBImage.PixelHeight;
                    //_APDChannelB = new ushort[bufferWidth * _CurrentScanParam.Height];
                    if (_ChannelBBackBuffer != null)
                    {
                        pBufChannelBDI16 = (ushort*)(byte*)_ChannelBBackBuffer.ToPointer();
                    }
                }
                if (_IsChannelCSelected)
                {
                    _ChannelCImage = new WriteableBitmap(_CurrentScanParam.Width, height, _ActualImageDPI, _ActualImageDPI, System.Windows.Media.PixelFormats.Gray16, null);
                    pBufChannelC16 = (ushort*)(byte*)_ChannelCImage.BackBuffer.ToPointer();
                    bufferWidth = _ChannelCImage.BackBufferStride;
                    bufferWidth /= 2;
                    Length = bufferWidth * height;
                    pixelWidth = _ChannelCImage.PixelWidth;
                    pixelHeight = _ChannelCImage.PixelHeight;
                    //_APDChannelC = new ushort[bufferWidth * _CurrentScanParam.Height];
                    if (_ChannelCBackBuffer != null)
                    {
                        pBufChannelCDI16 = (ushort*)(byte*)_ChannelCBackBuffer.ToPointer();
                    }
                }

                uint resToEncoderCount = (uint)Math.Round(_CurrentScanParam.Res * _CurrentScanParam.XEncoderSubdivision / 1000.0);
                if (resToEncoderCount < 1)
                {
                    resToEncoderCount = 1;
                }
                _CommController.SetHorizontalScanExtraMove((int)Math.Round(_CurrentScanParam.XMotionExtraMoveLength * _CurrentScanParam.XEncoderSubdivision));
                uint deltaXEncoder = (uint)(_CurrentScanParam.ScanDeltaX / _CurrentScanParam.XMotorSubdivision * _CurrentScanParam.XEncoderSubdivision);
                int saturationThreshold = _SaturationThreshold;
                // calculate the bit depth
                if (_CurrentScanParam.DynamicBitsAt)
                {
                    double adcSampleRate = 300000;  // adc sample rate at 300 kHz
                    double pixelSampleDuration = _CurrentScanParam.Res * 0.001 / (xMotorSpeed / _CurrentScanParam.XMotorSubdivision);
                    double pixelSamplePoints = pixelSampleDuration * adcSampleRate;
                    int samplePointsWidth = (int)Math.Log(pixelSamplePoints, 2);
                    _CurrentScanParam.DynamicBits = samplePointsWidth;
                    _ImageInfo.DynamicBit = 16 + samplePointsWidth;
                    int maxPixelValue = 0;
                    maxPixelValue = (int)Math.Pow(2, _ImageInfo.DynamicBit) - 1;
                    saturationThreshold = (int)(maxPixelValue * ((double)_SaturationThreshold / 65535.0));
                }
                else
                {
                    _ImageInfo.DynamicBit = 16;
                    _CurrentScanParam.DynamicBits = 0;
                    saturationThreshold = _SaturationThreshold;
                }

                // The stage (or scan head) is in position to start scanning and the scan buffers setup completed
                OnScanDataReceived?.Invoke(this, "ScanningPrepCompleted");

                LogMessage("Start scanning...");
                //_IsScanning = true;

                _CommController.TriggerHorizontalScan(deltaXEncoder, (uint)_CurrentScanParam.Height, resToEncoderCount, (uint)(Math.Pow(2, _CurrentScanParam.DynamicBits)));
                _MotionController.SetStart(MotorTypes.X | MotorTypes.Y, new bool[] { true, true });
                //OnSpentTimeScanDataReceived?.Invoke();
                #endregion Step3: Start Scanning

                #region Step4: Reading frames
                int offset = 0;
                double compressCoeff = 16.0 / (16 + _CurrentScanParam.DynamicBits);
                ushort tempPixA = 0;
                ushort tempPixB = 0;
                ushort tempPixC = 0;
                int waitCounter = 0;

                // Crop region compensation (don't check for saturated pixel outside of the crop region)
                int L1R2OpticalDist = (int)(_CurrentScanParam.AlignmentParam.OpticalL_R1Distance * 1000.0 / _CurrentScanParam.Res);
                int R1R2OpticalDist = (int)(_CurrentScanParam.AlignmentParam.OpticalR2_R1Distance * 1000.0 / _CurrentScanParam.Res);
                double pixel10Dist = _CurrentScanParam.Res / 10.0;
                int nYOverscanInPixels = (int)(_CurrentScanParam.AlignmentParam.YMotionExtraMoveLength / 2.0 * 1000.0 / _CurrentScanParam.Res);
                int L1Pixel10DX = L1R2OpticalDist - (int)(_CurrentScanParam.AlignmentParam.Pixel_10_L_DX / pixel10Dist);
                int L1Pixel10DY = nYOverscanInPixels - (int)(_CurrentScanParam.AlignmentParam.Pixel_10_L_DY / pixel10Dist);
                int R1Pixel10DX = R1R2OpticalDist - (int)(_CurrentScanParam.AlignmentParam.Pixel_10_R2_DX / pixel10Dist);
                int R1Pixel10DY = nYOverscanInPixels - (int)(_CurrentScanParam.AlignmentParam.Pixel_10_R2_DY / pixel10Dist);

                unsafe
                {
                    while (true)
                    {
                        while (_CommController.ReceivingBuf.StoredSize < PACKSIZE)
                        {
                            //Must use Thread.Sleep(1) to force a context switch (to avoid using 100% of the CPU) 
                            // http://joeduffyblog.com/2006/08/22/priorityinduced-starvation-why-sleep1-is-better-than-sleep0-and-the-windows-balance-set-manager/
                            Thread.Sleep(1);
                            waitCounter++;
                            if (waitCounter > 15000)
                            {
                                // Waited for 15 seconds; 'StoredSize' is still < 'PACKSIZE'
                                throw new Exception("Error retrieving the data from the scanner.\nTook too long to retrieve the data (StoredSize < PACKSIZE).");
                            }
                        }
                        waitCounter = 0;
                        _CommController.ReceivingBuf.ReadDataOut(tempBuf, 0, PACKSIZE);

                        if (_CommController.LidIsOpen)
                        {
                            OnScanDataReceived?.Invoke(this, "LIDOpened");
                        }

                        //Determine the start byte of data
                        if (tempBuf[0] == 0x6b && tempBuf[1285] == 0x6f)
                        {
                            for (int i = 0; i < FramesPerPackage; i++)
                            {
                                offset = i * 16;
                                coordY = (ushort)(BitConverter.ToUInt16(tempBuf, 4 + offset) - 1);
                                coordX = (ushort)(BitConverter.ToUInt16(tempBuf, 6 + offset) - 1);
                                if (!_CurrentScanParam.IsUnidirectionalScan)
                                {
                                    if (coordY >= _CurrentScanParam.Height)
                                    {
                                        RemainingTime = 0;
                                        OnScanDataReceived?.Invoke(this, "RemainingTime");
                                        return;
                                    }
                                    index = coordX + coordY * bufferWidth;
                                    if (index >= Length)
                                    {
                                        LogMessage(string.Format("Image Width: {0}, Image Height: {1}", _CurrentScanParam.Width, _CurrentScanParam.Height));
                                        LogMessage(string.Format("Pixel Coordinates Error, X={0}, Y={1}", coordX, coordY));
                                        //throw new Exception(string.Format("Coordinates Error, X={0},Y={1}", coordX, coordY));
                                        return;
                                    }
                                    //ushort tempPixA = (ushort)(Math.Pow(BitConverter.ToUInt32(tempBuf, 8 + offset), compressCoeff));
                                    //ushort tempPixB = (ushort)(Math.Pow(BitConverter.ToUInt32(tempBuf, 12 + offset), compressCoeff));
                                    //ushort tempPixC = (ushort)(Math.Pow(BitConverter.ToUInt32(tempBuf, 16 + offset), compressCoeff));
                                    if (_IsChannelASelected)    //R1
                                    {
                                        tempPixA = (ushort)BitConverter.ToUInt32(tempBuf, 8 + offset);
                                        //_APDChannelA[index] = tempPixA;
                                        pBufChannelA16[index] = tempPixA;
                                        if (pBufChannelADI16 != null)
                                            pBufChannelADI16[index] = tempPixA;

                                        // SmartScan: turn off laser if pixel saturated
                                        if (_CurrentScanParam.IsSmartScanning && _IsSmartScanCalc)
                                        {
                                            if (!bIsLaserR1TurnedOff)
                                            {
                                                int x1 = R1Pixel10DX;
                                                int y1 = R1Pixel10DY;
                                                int x2 = x1 + pixelWidth - (R1R2OpticalDist * 2);
                                                int y2 = y1 + pixelHeight - (int)(_CurrentScanParam.AlignmentParam.YMotionExtraMoveLength * 1000.0 / _CurrentScanParam.Res);
                                                if (coordX > x1 && coordX < x2 && coordY > y1 && coordY < y2)
                                                {
                                                    if (!bIsLaserR1Saturated && pBufChannelA16[index] >= saturationThreshold)
                                                    {
                                                        nLaserR1SaturatedRow = coordY;
                                                        bIsLaserR1Saturated = true;
                                                    }
                                                    else if (bIsLaserR1Saturated && coordY >= nLaserR1SaturatedRow + 10)
                                                    {
                                                        //Scan 10 rows before turning off the laser
                                                        //_CommController.SetLaserPower(LaserChannels.ChannelA, 0);   //R1
                                                        bIsLaserR1TurnedOff = true;
                                                    }
                                                }
                                            }
                                        }
                                    }
                                    if (_IsChannelBSelected)    //R2
                                    {
                                        tempPixB = (ushort)BitConverter.ToUInt32(tempBuf, 12 + offset);
                                        //_APDChannelB[index] = tempPixB;
                                        pBufChannelB16[index] = tempPixB;
                                        if (pBufChannelBDI16 != null)
                                            pBufChannelBDI16[index] = tempPixB;

                                        // SmartScan: turn off laser if pixel saturated
                                        if (_CurrentScanParam.IsSmartScanning && _IsSmartScanCalc)
                                        {
                                            if (!bIsLaserR2TurnedOff)
                                            {
                                                int x1 = 0;
                                                int y1 = nYOverscanInPixels;
                                                int x2 = x1 + pixelWidth - L1R2OpticalDist;
                                                int y2 = y1 + pixelHeight - (int)(_CurrentScanParam.AlignmentParam.YMotionExtraMoveLength * 1000.0 / _CurrentScanParam.Res);
                                                if (coordX < x2 && coordY > y1 && coordY < y2)
                                                {
                                                    if (!bIsLaserR2Saturated && pBufChannelB16[index] >= saturationThreshold)
                                                    {
                                                        nLaserR2SaturatedRow = coordY;
                                                        bIsLaserR2Saturated = true;
                                                    }
                                                    else if (bIsLaserR2Saturated && coordY >= nLaserR2SaturatedRow + 10)
                                                    {
                                                        //Scan 10 rows before turning off the laser
                                                        //_CommController.SetLaserPower(LaserChannels.ChannelB, 0);   //R2
                                                        bIsLaserR2TurnedOff = true;
                                                    }
                                                }
                                            }
                                        }
                                    }
                                    if (_IsChannelCSelected)    //L1
                                    {
                                        tempPixC = (ushort)BitConverter.ToUInt32(tempBuf, 16 + offset);
                                        //_APDChannelC[index] = tempPixC;
                                        pBufChannelC16[index] = tempPixC;
                                        if (pBufChannelCDI16 != null)
                                            pBufChannelCDI16[index] = tempPixC;

                                        // SmartScan: turn off laser if pixel saturated
                                        if (_CurrentScanParam.IsSmartScanning && _IsSmartScanCalc)
                                        {
                                            if (!bIsLaserL1TurnedOff)
                                            {
                                                int x1 = L1Pixel10DX;
                                                int y1 = L1Pixel10DY;
                                                int x2 = x1 + pixelWidth - L1R2OpticalDist;
                                                int y2 = y1 + pixelHeight - (int)(_CurrentScanParam.AlignmentParam.YMotionExtraMoveLength * 1000.0 / _CurrentScanParam.Res);
                                                if (coordX > x1 && coordX < x2 && coordY > y1 && coordY < y2)
                                                {
                                                    if (!bIsLaserL1Saturated && pBufChannelC16[index] >= saturationThreshold)
                                                    {
                                                        nLaserL1SaturatedRow = coordY;
                                                        bIsLaserL1Saturated = true;
                                                    }
                                                    else if (bIsLaserL1Saturated && coordY >= nLaserL1SaturatedRow + 10)
                                                    {
                                                        //Scan 10 rows before turning off the laser
                                                        //_CommController.SetLaserPower(LaserChannels.ChannelC, 0);   //L1
                                                        bIsLaserL1TurnedOff = true;
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    // Unidirectional scan:

                                    // Unidirectional scan double the height in scan param setup
                                    // to synchronize the scanner's LED progress bar
                                    //coordY /= 2;
                                    //index = coordX + coordY * bufferWidth;

                                    // Actual location of image data on the image buffer
                                    index = coordX + (coordY / 2) * bufferWidth;
                                    // For access convenience
                                    int actualCoordY = coordY / 2;
                                    
                                    bool isEvenLine = coordY % 2 == 0;
                                    if (coordY >= _CurrentScanParam.Height)
                                    {
                                        RemainingTime = 0;
                                        OnScanDataReceived?.Invoke(this, "RemainingTime");
                                        return;
                                    }
                                    if (index >= Length)
                                    {
                                        LogMessage(string.Format("Image Width: {0}, Image Height: {1}", _CurrentScanParam.Width, _CurrentScanParam.Height));
                                        LogMessage(string.Format("Pixel Coordinates Error, X={0}, Y={1}", coordX, actualCoordY));
                                        //throw new Exception(string.Format("Coordinates Error, X={0},Y={1}", coordX, coordY));
                                        return;
                                    }

                                    //ushort tempPixA = (ushort)(Math.Pow(BitConverter.ToUInt32(tempBuf, 8 + offset), compressCoeff));
                                    //ushort tempPixB = (ushort)(Math.Pow(BitConverter.ToUInt32(tempBuf, 12 + offset), compressCoeff));
                                    //ushort tempPixC = (ushort)(Math.Pow(BitConverter.ToUInt32(tempBuf, 16 + offset), compressCoeff));
                                    tempPixA = (ushort)BitConverter.ToUInt32(tempBuf, 8 + offset);
                                    tempPixB = (ushort)BitConverter.ToUInt32(tempBuf, 12 + offset);
                                    tempPixC = (ushort)BitConverter.ToUInt32(tempBuf, 16 + offset);

                                    if (!isEvenLine)
                                    {
                                        // 2 Lines average:
                                        // Average of the odd and even lines (and put average on the odd number lines)
                                        //
                                        //int indexEven = coordX + ((actualCoordY - 1) * bufferWidth);    // Even line index
                                        //if (_IsChannelASelected)    //R1
                                        //{
                                        //    tempPixA = (ushort)((*(pBufChannelA16 + indexEven) + tempPixA) / 2);
                                        //}
                                        //if (_IsChannelBSelected)    //R2
                                        //{
                                        //    tempPixB = (ushort)((*(pBufChannelB16 + indexEven) + tempPixB) / 2);
                                        //}
                                        //if (_IsChannelCSelected)    //L1
                                        //{
                                        //    tempPixC = (ushort)((*(pBufChannelC16 + indexEven) + tempPixC) / 2);
                                        //}

                                        if (_IsChannelASelected)    //R1
                                        {
                                            tempPixA = (ushort)((*(pBufChannelA16 + index) + tempPixA) / 2);
                                        }
                                        if (_IsChannelBSelected)    //R2
                                        {
                                            tempPixB = (ushort)((*(pBufChannelB16 + index) + tempPixB) / 2);
                                        }
                                        if (_IsChannelCSelected)    //L1
                                        {
                                            tempPixC = (ushort)((*(pBufChannelC16 + index) + tempPixC) / 2);
                                        }
                                    }

                                    if (_IsChannelASelected)    //R1
                                    {
                                        //_APDChannelA[index] = tempPixA;
                                        pBufChannelA16[index] = tempPixA;
                                        if (pBufChannelADI16 != null)
                                            pBufChannelADI16[index] = tempPixA;

                                        // SmartScan: turn off laser if pixel saturated
                                        if (_CurrentScanParam.IsSmartScanning && _IsSmartScanCalc)
                                        {
                                            if (!bIsLaserR1TurnedOff)
                                            {
                                                int x1 = R1Pixel10DX;
                                                int y1 = R1Pixel10DY;
                                                int x2 = x1 + pixelWidth - (R1R2OpticalDist * 2);
                                                int y2 = y1 + pixelHeight - (int)(_CurrentScanParam.AlignmentParam.YMotionExtraMoveLength * 1000.0 / _CurrentScanParam.Res);
                                                if (coordX > x1 && coordX < x2 && actualCoordY > y1 && actualCoordY < y2)
                                                {
                                                    if (!bIsLaserR1Saturated && pBufChannelA16[index] >= saturationThreshold)
                                                    {
                                                        nLaserR1SaturatedRow = actualCoordY;  //First saturated row
                                                        bIsLaserR1Saturated = true;
                                                    }
                                                    else if (bIsLaserR1Saturated && actualCoordY >= nLaserR1SaturatedRow + 10)
                                                    {
                                                        //Scan 10 rows before turning off the laser
                                                        //_CommController.SetLaserPower(LaserChannels.ChannelA, 0);   //R1
                                                        bIsLaserR1TurnedOff = true;
                                                    }
                                                }
                                            }
                                        }
                                    }
                                    if (_IsChannelBSelected)    //R2
                                    {
                                        //_APDChannelB[index] = tempPixB;
                                        pBufChannelB16[index] = tempPixB;
                                        if (pBufChannelBDI16 != null)
                                            pBufChannelBDI16[index] = tempPixB;

                                        // SmartScan: turn off laser if pixel saturated
                                        if (_CurrentScanParam.IsSmartScanning && _IsSmartScanCalc)
                                        {
                                            if (!bIsLaserR2TurnedOff)
                                            {
                                                int x1 = 0;
                                                int y1 = nYOverscanInPixels;
                                                int x2 = x1 + pixelWidth - L1R2OpticalDist;
                                                int y2 = y1 + pixelHeight - (int)(_CurrentScanParam.AlignmentParam.YMotionExtraMoveLength * 1000.0 / _CurrentScanParam.Res);
                                                if (coordX < x2 && actualCoordY > y1 && actualCoordY < y2)
                                                {
                                                    if (!bIsLaserR2Saturated && pBufChannelB16[index] >= saturationThreshold)
                                                    {
                                                        nLaserR2SaturatedRow = actualCoordY;  //First saturated row
                                                        bIsLaserR2Saturated = true;
                                                    }
                                                    else if (bIsLaserR2Saturated && actualCoordY >= nLaserR2SaturatedRow + 10)
                                                    {
                                                        //Scan 10 rows before turning off the laser
                                                        //_CommController.SetLaserPower(LaserChannels.ChannelB, 0);   //R2
                                                        bIsLaserR2TurnedOff = true;
                                                    }
                                                }
                                            }
                                        }
                                    }
                                    if (_IsChannelCSelected)    //L1
                                    {
                                        //_APDChannelC[index] = tempPixC;
                                        pBufChannelC16[index] = tempPixC;
                                        if (pBufChannelCDI16 != null)
                                            pBufChannelCDI16[index] = tempPixC;

                                        // SmartScan: turn off laser if pixel saturated
                                        if (_CurrentScanParam.IsSmartScanning && _IsSmartScanCalc)
                                        {
                                            if (!bIsLaserL1TurnedOff)
                                            {
                                                int x1 = L1Pixel10DX;
                                                int y1 = L1Pixel10DY;
                                                int x2 = x1 + pixelWidth - L1R2OpticalDist;
                                                int y2 = y1 + pixelHeight - (int)(_CurrentScanParam.AlignmentParam.YMotionExtraMoveLength * 1000.0 / _CurrentScanParam.Res);
                                                if (coordX > x1 && coordX < x2 && actualCoordY > y1 && actualCoordY < y2)
                                                {
                                                    if (!bIsLaserL1Saturated && pBufChannelC16[index] >= saturationThreshold)
                                                    {
                                                        nLaserL1SaturatedRow = actualCoordY;  //First saturated row
                                                        bIsLaserL1Saturated = true;
                                                    }
                                                    else if (bIsLaserL1Saturated && actualCoordY >= nLaserL1SaturatedRow + 10)
                                                    {
                                                        //Scan 10 rows before turning off the laser
                                                        //_CommController.SetLaserPower(LaserChannels.ChannelC, 0);   //L1
                                                        bIsLaserL1TurnedOff = true;
                                                    }
                                                }
                                            }
                                        }
                                    }

                                    //pbuffScanA[index] = (ushort)(tempPixA / 2);
                                    //pbuffScanB[index] = (ushort)(tempPixB / 2);
                                    //pbuffScanC[index] = (ushort)(tempPixC / 2);
                                    //_APDChannelA[index] = (ushort)(tempPixA / 2);
                                    //_APDChannelB[index] = (ushort)(tempPixB / 2);
                                    //_APDChannelC[index] = (ushort)(tempPixC / 2);
                                }
                            }

                            // Terminate/stop Smartscan when all scanning channels are saturated
                            if (_CurrentScanParam.IsSmartScanning && _IsSmartScanCalc)
                            {
                                // Despeckle smart scan test image (don't terminate the smart scan (even if all channels has saturated pixels)
                                if (!IsDespeckleSmartScan)
                                {
                                    int nNumOfLasersTurnedOff = 0;  // Number of lasers turned off because of saturation
                                    if (_IsChannelASelected && bIsLaserR1TurnedOff) { nNumOfLasersTurnedOff++; }
                                    if (_IsChannelBSelected && bIsLaserR2TurnedOff) { nNumOfLasersTurnedOff++; }
                                    if (_IsChannelCSelected && bIsLaserL1TurnedOff) { nNumOfLasersTurnedOff++; }
                                    // All the scanning channels are turned off
                                    if (nNumOfLasersTurnedOff == _CurrentScanParam.Signals.Count)
                                    {
                                        //End the scan
                                        break;
                                    }
                                }
                            }

                            if (RemainingTime != _CurrentScanParam.Time - _CurrentScanParam.Time * coordY / _CurrentScanParam.Height)
                            {
                                RemainingTime = _CurrentScanParam.Time - _CurrentScanParam.Time * coordY / _CurrentScanParam.Height;
                                OnScanDataReceived?.Invoke(this, "RemainingTime");
                                DataReceived?.Invoke(this);
                            }
                        }
                    }
                }
                #endregion Step4: Reading frames
            }
            catch (System.Threading.ThreadAbortException)
            {
                // don't throw exception if the user abort the process.
            }
            catch (System.Runtime.InteropServices.SEHException)
            {
                // The SEHException class handles SEH errors that are thrown from unmanaged code,
                // but have not been mapped to another .NET Framework exception.

                throw new OutOfMemoryException();
            }
            catch (System.Runtime.InteropServices.COMException cex)
            {
                if (cex.ErrorCode == unchecked((int)0x88980003))
                {
                    throw new OutOfMemoryException();
                }
                else
                {
                    throw;
                }
            }
            catch (Exception)
            {
                throw;
            }
            //finally
            //{
            //    // Forces an immediate garbage collection.
            //    //GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced);
            //    //GC.WaitForPendingFinalizers();
            //    //GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced);
            //}
        }

        private void XAxisScanProcess()
        {
            byte[] tempBuf = new byte[PACKSIZE];
            int index = 0;
            SampleIndex = new int[_CurrentScanParam.LineCounts];
            SampleValueChannelA = new int[_CurrentScanParam.LineCounts];
            SampleValueChannelB = new int[_CurrentScanParam.LineCounts];
            SampleValueChannelC = new int[_CurrentScanParam.LineCounts];
            if (PresetMotion(60000) == false)       // wait 1 minutes for the motions to be at the starting positions
            {
                ExitStat = ThreadExitStat.Error;
                throw new Exception("Failed to move the motions to the start positions");
            }
            _CommController.ReceivingBuf.Reset();

            int singleTrip = _CurrentScanParam.ScanDeltaX;
            double singleTripTime = _CurrentScanParam.Quality / 2.0 - _CurrentScanParam.XmotionTurnAroundDelay / 1000.0;
            //double singleTripTime = _ScanSettings.Quality / 2.0;
            int xMotorSpeed = XMotorSpeedCalibration.GetSpeed(_CurrentScanParam.XMotionAccVal, 256, singleTrip, singleTripTime);
            if (_CurrentScanParam.HorizontalCalibrationSpeed > 0)
            {
                xMotorSpeed = _CurrentScanParam.HorizontalCalibrationSpeed;
            }
            int tgtPosX1 = _CurrentScanParam.ScanX0 + _CurrentScanParam.ScanDeltaX;
            int tgtPosX2 = _CurrentScanParam.ScanX0;
            int repeats = 50000;
            _MotionController.AbsoluteMoveSingleMotion(MotorTypes.X, 256, xMotorSpeed, _CurrentScanParam.XMotionAccVal,
                _CurrentScanParam.XMotionDccVal, tgtPosX1, tgtPosX2, repeats, _CurrentScanParam.XmotionTurnAroundDelay, false, false);
            //_MotionController.AutoQuery = false;
            _CommController.TriggerXScan((uint)_CurrentScanParam.ScanDeltaX, (uint)_CurrentScanParam.DataRate);
            RemainingTime = _CurrentScanParam.DataRate * _CurrentScanParam.LineCounts / 1000;
            OnScanDataReceived?.Invoke(this, "RemainingTime");
            OnScanDataReceived?.Invoke(this, "OnXRemainingTime");
            while (_MotionController.SetStart(MotorTypes.X, new bool[] { true }) == false) {; }
            while (true)
            {
                while (_CommController.ReceivingBuf.StoredSize < PACKSIZE)
                {
                    Thread.Sleep(1);
                }
                _CommController.ReceivingBuf.ReadDataOut(tempBuf, 0, PACKSIZE);

                if (tempBuf[0] == 0x6b && tempBuf[1285] == 0x6f)
                {
                    for (int i = 0; i < FramesPerPackage; i++)
                    {
                        index = BitConverter.ToInt32(tempBuf, 4 + i * 16) - 1;
                        if (index >= _CurrentScanParam.LineCounts)
                        {
                            return;
                        }
                        SampleIndex[index] = index * _CurrentScanParam.DataRate;
                        SampleValueChannelA[index] = BitConverter.ToInt32(tempBuf, 8 + i * 16);
                        SampleValueChannelB[index] = BitConverter.ToInt32(tempBuf, 12 + i * 16);
                        SampleValueChannelC[index] = BitConverter.ToInt32(tempBuf, 16 + i * 16);
                    }
                }
            }
        }

        private bool PresetMotion(int timeout)
        {
            bool isThere = true;
            if (timeout < 500)
            {
                timeout = 500;
            }

            LogMessage("Move X motor");

            bool result;
            int tryCnts = 0;
            int tgtPosX = _CurrentScanParam.ScanX0;
            do
            {
                if (++tryCnts > 5)
                {
                    return false;
                }
                if (ScanType == ScanTypes.Horizontal)
                {
                    tgtPosX = _CurrentScanParam.ScanX0 - (int)Math.Round(_CurrentScanParam.XMotionExtraMoveLength * _CurrentScanParam.XMotorSubdivision);
                    if (tgtPosX < 0) { tgtPosX = 0; }
                }
                result = _MotionController.AbsoluteMoveSingleMotion(MotorTypes.X,
                    256, _CurrentScanParam.XMotorSpeed, _CurrentScanParam.XMotionAccVal, _CurrentScanParam.XMotionAccVal, tgtPosX, true, false);
            } while (result == false);

            LogMessage("Move Y motor");

            int tgtPosY = _CurrentScanParam.ScanY0;
            tryCnts = 0;
            do
            {
                if (++tryCnts > 5)
                {
                    return false;
                }
                if (ScanType == ScanTypes.Horizontal)
                {
                    tgtPosY = _CurrentScanParam.ScanY0 - (int)Math.Round(_CurrentScanParam.YMotionExtraMoveLength / 2.0 * _CurrentScanParam.YMotorSubdivision);
                }
                //result = _MotionController.AbsoluteMoveSingleMotion(MotorTypes.Y,
                //    256, _CurrentScanParam.YMotorSpeed, _CurrentScanParam.YMotionAccVal, _CurrentScanParam.YMotionAccVal, _CurrentScanParam.ScanY0, true, false);
                result = _MotionController.AbsoluteMoveSingleMotion(MotorTypes.Y,
                    256, _CurrentScanParam.YMotorSpeed, _CurrentScanParam.YMotionAccVal, _CurrentScanParam.YMotionAccVal, tgtPosY, true, false);
            } while (result == false);

            LogMessage("Move Z motor");

            tryCnts = 0;
            do
            {
                if (++tryCnts > 5)
                {
                    return false;
                }
                result = _MotionController.AbsoluteMoveSingleMotion(MotorTypes.Z,
                    256, _CurrentScanParam.ZMotorSpeed, _CurrentScanParam.ZMotionAccVal, _CurrentScanParam.ZMotionDccVal, _CurrentScanParam.ScanZ0, true, false);
            } while (result == false);

            LogMessage("X, Y and Z motors position check");

            do
            {
                if (timeout < 0)
                {
                    LogMessage("ALL motors NOT in position: position check TIMEOUT!");
                    return false;
                }

                isThere = true;
                if (_MotionController.CrntState[MotorTypes.X].IsBusy)
                {
                    isThere = false;
                }
                else if (_MotionController.CrntPositions[MotorTypes.X] != tgtPosX)
                {
                    isThere = false;
                }
                if (_MotionController.CrntState[MotorTypes.Y].IsBusy)
                {
                    isThere = false;
                }
                else if (_MotionController.CrntPositions[MotorTypes.Y] != tgtPosY)
                {
                    isThere = false;
                }
                if (_MotionController.CrntState[MotorTypes.Z].IsBusy)
                {
                    isThere = false;
                }
                else if (_MotionController.CrntPositions[MotorTypes.Z] != _CurrentScanParam.ScanZ0)
                {
                    isThere = false;
                }

                if (isThere)
                {
                    LogMessage("ALL motors in position: Ready to Scan");
                    return true;
                }
                else
                {
                    //Thread.Sleep(500);
                    //timeout -= 500;
                    Thread.Sleep(1);
                    timeout--;
                }
            }
            while (isThere == false);
            return true;
        }

        /// <summary>
        /// Set laser power, gain, and pga
        /// </summary>
        private void SetLaserPower()
        {
            //Turn off (or reset) lasers/apd/pga
            TurnOffAllLasers();

            #region === Set laser's intensity, apd gain, and pga ===
            //
            // Set laser's intensity, apd gain and apd pga
            // after the motors are in the set position
            //
            if (_CommController.IsConnected)
            {
                foreach (var signal in _CurrentScanParam.Signals)
                {
                    if (signal.SensorType == IvSensorType.NA)
                    {
                        if (IsPhosphorImaging)
                            signal.SensorType = IvSensorType.PMT;
                        else
                            signal.SensorType = IvSensorType.APD;
                    }
                    double laserPower = signal.LaserIntensity;
                    switch (signal.LaserChannel)
                    {
                        case LaserChannels.ChannelA:    //R1
                            if (!_CurrentScanParam.IsIgnoreCompCoefficient)
                            {
                                if (_CurrentScanParam.R1Coefficient > 0 && _CurrentScanParam.R1Coefficient < 0.6)
                                {
                                    //  laserPower = 1.0 / (1.0 - compCoeff) * laserPower
                                    laserPower = 1.0 / (1.0 - _CurrentScanParam.R1Coefficient) * laserPower;
                                }
                            }
                            _CommController.SetLaserPower(LaserChannels.ChannelA, laserPower);
                            Thread.Sleep(20);    //set a short delay to try to avoid flooding the control board with multiple commands
                            if (signal.SensorType == IvSensorType.APD)
                            {
                                _CommController.SetIvApdGain(IVChannels.ChannelA, (ushort)signal.ApdGain);
                                Thread.Sleep(20);
                            }
                            else if (signal.SensorType == IvSensorType.PMT)
                            {
                                _CommController.SetIvPmtGain(IVChannels.ChannelA, (ushort)signal.ApdGain);
                                Thread.Sleep(20);
                            }
                            _CommController.SetIvPga(IVChannels.ChannelA, (ushort)signal.ApdPga);
                            Thread.Sleep(20);

                            LogMessage(string.Format("R1 Laser Power: {0}", signal.LaserIntensity));
                            if (signal.SensorType == IvSensorType.APD)
                                LogMessage(string.Format("R1 APD Gain: {0}", (ushort)signal.ApdGain));
                            else if (signal.SensorType == IvSensorType.PMT)
                                LogMessage(string.Format("R1 PMT Gain: {0}", (ushort)signal.ApdGain));
                            LogMessage(string.Format("R1 PGA: {0}", (ushort)signal.ApdPga));

                            break;
                        case LaserChannels.ChannelB:    //R2
                            if (!_CurrentScanParam.IsIgnoreCompCoefficient)
                            {
                                if (signal.LaserWavelength == 532)
                                {
                                    if (_CurrentScanParam.R2532Coefficient > 0 && _CurrentScanParam.R2532Coefficient < 0.6)
                                    {
                                        //  laserPower = 1.0 / (1.0 - compCoeff) * laserPower
                                        laserPower = 1.0 / (1.0 - _CurrentScanParam.R2532Coefficient) * laserPower;
                                    }
                                }
                                else
                                {
                                    if (_CurrentScanParam.R2Coefficient > 0 && _CurrentScanParam.R2Coefficient < 0.6)
                                    {
                                        //  laserPower = 1.0 / (1.0 - compCoeff) * laserPower
                                        laserPower = 1.0 / (1.0 - _CurrentScanParam.R2Coefficient) * laserPower;
                                    }
                                }
                            }
                            _CommController.SetLaserPower(LaserChannels.ChannelB, laserPower);
                            Thread.Sleep(20);
                            if (signal.SensorType == IvSensorType.APD)
                            {
                                _CommController.SetIvApdGain(IVChannels.ChannelB, (ushort)signal.ApdGain);
                                Thread.Sleep(20);
                            }
                            else if (signal.SensorType == IvSensorType.PMT)
                            {
                                _CommController.SetIvPmtGain(IVChannels.ChannelB, (ushort)signal.ApdGain);
                                Thread.Sleep(20);
                            }
                            _CommController.SetIvPga(IVChannels.ChannelB, (ushort)signal.ApdPga);
                            Thread.Sleep(20);

                            LogMessage(string.Format("R2 Laser Power: {0}", signal.LaserIntensity));
                            if (signal.SensorType == IvSensorType.APD)
                                LogMessage(string.Format("R2 APD Gain: {0}", (ushort)signal.ApdGain));
                            else
                                LogMessage(string.Format("R2 PMT Gain: {0}", (ushort)signal.ApdGain));
                            LogMessage(string.Format("R2 PGA: {0}", (ushort)signal.ApdPga));
                            break;
                        case LaserChannels.ChannelC:    //L1
                            if (!_CurrentScanParam.IsIgnoreCompCoefficient)
                            {
                                if (signal.LaserWavelength == 375)                          // Use the specified 375 module coefficient  使用指定的375模块系数
                                {
                                    if (_CurrentScanParam.L375Coefficient > 0 && _CurrentScanParam.L375Coefficient < 0.6)
                                    {
                                        //  lp = 1.0 / (1.0 - compCoeff) * lp
                                        laserPower = 1.0 / (1.0 - _CurrentScanParam.L375Coefficient) * laserPower;
                                    }
                                }
                                else
                                {
                                    if (_CurrentScanParam.LCoefficient > 0 && _CurrentScanParam.LCoefficient < 0.6)
                                    {
                                        //  lp = 1.0 / (1.0 - compCoeff) * lp
                                        laserPower = 1.0 / (1.0 - _CurrentScanParam.LCoefficient) * laserPower;
                                    }
                                }
                            }
                            _CommController.SetLaserPower(LaserChannels.ChannelC, laserPower);
                            Thread.Sleep(20);
                            if (signal.SensorType == IvSensorType.APD)
                            {
                                _CommController.SetIvApdGain(IVChannels.ChannelC, (ushort)signal.ApdGain);
                                Thread.Sleep(20);
                            }
                            else if (signal.SensorType == IvSensorType.PMT)
                            {
                                _CommController.SetIvPmtGain(IVChannels.ChannelC, (ushort)signal.ApdGain);
                                Thread.Sleep(20);
                            }
                            _CommController.SetIvPga(IVChannels.ChannelC, (ushort)signal.ApdPga);

                            LogMessage(string.Format("L Laser Power: {0}", signal.LaserIntensity));
                            if (signal.SensorType == IvSensorType.APD)
                                LogMessage(string.Format("L APD Gain: {0}", (ushort)signal.ApdGain));
                            else
                                LogMessage(string.Format("L PMT Gain: {0}", (ushort)signal.ApdGain));
                            LogMessage(string.Format("L PGA: {0}", (ushort)signal.ApdPga));
                            break;
                    }
                }

                // Set a 3 sec delay to allow the lasers to ramp up
                Thread.Sleep(3000);
            }
            else
            {
                LogMessage("SetLaserPower(): Scanner not connected!");
                throw new Exception("SetLaserPower(): Scanner not connected!");
            }
            #endregion
        }

        //private void ResetLasersConfiguration()
        //{
        //    if (_CommController != null && _CommController.IsConnected)
        //    {
        //        _CommController.SetLaserPower(LaserChannels.ChannelA, 0);
        //        _CommController.SetLaserPower(LaserChannels.ChannelB, 0);
        //        _CommController.SetLaserPower(LaserChannels.ChannelC, 0);
        //        _CommController.SetIvApdGain(IVChannels.ChannelA, 0);
        //        _CommController.SetIvApdGain(IVChannels.ChannelB, 0);
        //        _CommController.SetIvApdGain(IVChannels.ChannelC, 0);
        //        _CommController.SetIvPga(IVChannels.ChannelA, 0);
        //        _CommController.SetIvPga(IVChannels.ChannelB, 0);
        //        _CommController.SetIvPga(IVChannels.ChannelC, 0);
        //    }
        //}

        private void TurnOffAllLasers()
        {
            /*foreach (var signal in _CurrentScanParam.Signals)
            {
                switch (signal.LaserChannel)
                {
                    case LaserChannels.ChannelA:    //R1
                        _CommController.SetLaserPower(LaserChannels.ChannelA, 0);
                        ImagingHelper.Delay(20);
                        break;
                    case LaserChannels.ChannelB:    //2R
                        _CommController.SetLaserPower(LaserChannels.ChannelB, 0);
                        ImagingHelper.Delay(20);
                        break;
                    case LaserChannels.ChannelC:    //L1
                        _CommController.SetLaserPower(LaserChannels.ChannelC, 0);
                        ImagingHelper.Delay(20);
                        break;
                }
            }*/

            _CommController.SetLaserPower(LaserChannels.ChannelA, 0);   //R1
            Thread.Sleep(20);
            _CommController.SetLaserPower(LaserChannels.ChannelB, 0);   //R2
            Thread.Sleep(20);
            _CommController.SetLaserPower(LaserChannels.ChannelC, 0);   //L1
            Thread.Sleep(20);
        }

        /// <summary>
        /// Stop current scan job.
        /// </summary>
        private void StopScan()
        {
            if (!IsSimulationMode)
            {
                if (_CommController.IsConnected)
                {
                    TurnOffAllLasers();

                    //if (!_IsScanAborted)    //StopScan already called
                    //{
                    //    _CommController.StopScan();
                    //}
                    //_CommController.SetLaserPower(LaserChannels.ChannelA, 0);
                    //_CommController.SetLaserPower(LaserChannels.ChannelB, 0);
                    //_CommController.SetLaserPower(LaserChannels.ChannelC, 0);
                    //_MotionController.AutoQuery = true;

                    //if (_IsScanning)
                    //{
                    //    _CommController.StopScan();
                    //    _MotionController.SetStart(MotorTypes.X | MotorTypes.Y | MotorTypes.Z, new bool[] { false, false, false });
                    //    _IsScanning = false;
                    //}
                    _CommController.StopScan();
                    _MotionController.SetStart(MotorTypes.X | MotorTypes.Y | MotorTypes.Z, new bool[] { false, false, false });
                    //_IsScanning = false;
                }
                else
                {
                    LogMessage("StopScan(): Scanner not connected!");
                    throw new Exception("StopScan(): Scanner not connected!");
                }
            }
        }


        #region SmartScan

        /// <summary>
        /// Calculate the signal levels and update _Signals list
        /// </summary>
        private void SmartscanProcess()
        {
            int nFloor = _CurrentScanParam.SmartScanFloor;
            int nCeiling = _CurrentScanParam.SmartScanCeiling;
            int nSignalLevels = _CurrentScanParam.SmartScanSignalLevels;
            int nOptimalMin = (int)((double)_CurrentScanParam.SmartScanOptimalVal - _CurrentScanParam.SmartScanOptimalDelta * 65535.0);
            int nOptimalMax = (int)((double)_CurrentScanParam.SmartScanOptimalVal + _CurrentScanParam.SmartScanOptimalDelta * 65535.0);
            int nSignalStepdownLevel1 = _CurrentScanParam.SmartScanSignalStepdownLevel;
            int nSignalStepdownLevel2 = 2;
            int nSignalStepUpLevel1 = nSignalLevels - 1;
            WriteableBitmap scannedImage = null;
            int nAutoScanCount = 0;         // Number of test scan passes
            Signal currentSignal = null;    // Current signal (test scan)
            List<Signal> signalOptionsList = new List<Signal>();
            Signal laserL1SignalLevel = null;
            Signal laserR1SignalLevel = null;
            Signal laserR2SignalLevel = null;
            bool bIsRescanRequired = false;
            var signals = new List<Signal>();
            int LaserL1Wavelength = 0;
            int LaserR1Wavelength = 0;
            int LaserR2Wavelength = 0;
            bool L1signalLevelFound = true;
            bool R1signalLevelFound = true;
            bool R2signalLevelFound = true;
            LaserChannels currentLaserChannel = LaserChannels.ChannelC;

            // Initial scan setup (signal level specified in config file
            int startingPosition = _CurrentScanParam.SmartScanInitSignalLevel - 1;     // zero index
            if (startingPosition <= 0)
            {
                startingPosition = 8;
            }
            for (int i = 0; i < _CurrentScanParam.Signals.Count; i++)
            {
                if (_CurrentScanParam.Signals[i].LaserChannel == LaserChannels.ChannelC)        //L1
                {
                    //int midLevel = (int)((double)_ScanParameter.LaserASignalOptions.Count / 2.0f);
                    //laserASignalLevel = _ScanParameter.LaserASignalOptions[midLevel - 1]; // default auto-scan signal level
                    // default initial/starting auto-scan signal level
                    //int startingPosition = _CurrentScanParam.SmartScanInitSignalLevel - 1;     // zero index
                    //if (startingPosition <= 0)
                    //{
                    //    startingPosition = (int)((double)_CurrentScanParam.LaserL1SignalOptions.Count / 2.0f);
                    //}
                    laserL1SignalLevel = (Signal)_CurrentScanParam.LaserL1SignalOptions[startingPosition].Clone();
                    laserL1SignalLevel.ColorChannel = _CurrentScanParam.Signals[i].ColorChannel;
                    laserL1SignalLevel.LaserChannel = _CurrentScanParam.Signals[i].LaserChannel;
                    laserL1SignalLevel.LaserWavelength = _CurrentScanParam.Signals[i].LaserWavelength;
                    laserL1SignalLevel.SignalLevel = startingPosition;
                    LaserL1Wavelength = _CurrentScanParam.Signals[i].LaserWavelength;
                    signals.Add(laserL1SignalLevel);
                    L1signalLevelFound = false;
                }
                else if (_CurrentScanParam.Signals[i].LaserChannel == LaserChannels.ChannelA)   //R1
                {
                    //int midLevel = (int)((double)_ScanParameter.LaserBSignalOptions.Count / 2.0f);
                    //laserBSignalLevel = _ScanParameter.LaserBSignalOptions[midLevel - 1]; // default auto-scan signal level
                    // default initial/starting auto-scan signal level
                    //int startingPosition = _CurrentScanParam.SmartScanInitSignalLevel - 1;
                    //if (startingPosition <= 0)
                    //{
                    //    startingPosition = (int)((double)_CurrentScanParam.LaserR1SignalOptions.Count / 2.0f);
                    //}
                    laserR1SignalLevel = (Signal)_CurrentScanParam.LaserR1SignalOptions[startingPosition].Clone();
                    laserR1SignalLevel.ColorChannel = _CurrentScanParam.Signals[i].ColorChannel;
                    laserR1SignalLevel.LaserChannel = _CurrentScanParam.Signals[i].LaserChannel;
                    laserR1SignalLevel.LaserWavelength = _CurrentScanParam.Signals[i].LaserWavelength;
                    laserR1SignalLevel.SignalLevel = startingPosition;
                    LaserR1Wavelength = _CurrentScanParam.Signals[i].LaserWavelength;
                    signals.Add(laserR1SignalLevel);
                    R1signalLevelFound = false;
                }
                else if (_CurrentScanParam.Signals[i].LaserChannel == LaserChannels.ChannelB)   //R2
                {
                    //int midLevel = (int)((double)_ScanParameter.LaserCSignalOptions.Count / 2.0f);
                    //laserCSignalLevel = _ScanParameter.LaserCSignalOptions[midLevel - 1]; // default auto-scan signal level
                    // default initial/starting auto-scan signal level
                    //int startingPosition = _CurrentScanParam.SmartScanInitSignalLevel - 1;
                    //if (startingPosition <= 0)
                    //{
                    //    startingPosition = (int)((double)_CurrentScanParam.LaserR2SignalOptions.Count / 2.0f);
                    //}
                    laserR2SignalLevel = (Signal)_CurrentScanParam.LaserR2SignalOptions[startingPosition].Clone();
                    laserR2SignalLevel.ColorChannel = _CurrentScanParam.Signals[i].ColorChannel;
                    laserR2SignalLevel.LaserChannel = _CurrentScanParam.Signals[i].LaserChannel;
                    laserR2SignalLevel.LaserWavelength = _CurrentScanParam.Signals[i].LaserWavelength;
                    laserR2SignalLevel.SignalLevel = startingPosition;
                    LaserR2Wavelength = _CurrentScanParam.Signals[i].LaserWavelength;
                    signals.Add(laserR2SignalLevel);
                    R2signalLevelFound = false;
                }
            }

            // Initial scan setup
            _CurrentScanParam.Signals = new List<Signal>(signals);

            int origWidth = _CurrentScanParam.Width;
            int origHeight = _CurrentScanParam.Height;
            int origResolution = _CurrentScanParam.Res;
            int origQuality = _CurrentScanParam.Quality; // scan speed
            bool origIsExtDynamicRange = _CurrentScanParam.DynamicBitsAt;
            bool origIsUndirectional = _CurrentScanParam.IsUnidirectionalScan;
            //int origScanTime = _CurrentScanParam.Time;
            _CurrentScanParam.Width = (int)(_CurrentScanParam.ScanDeltaX / _CurrentScanParam.XMotorSubdivision * 1000 / _CurrentScanParam.SmartScanResolution);
            _CurrentScanParam.Height = (int)(_CurrentScanParam.ScanDeltaY / _CurrentScanParam.YMotorSubdivision * 1000 / _CurrentScanParam.SmartScanResolution);
            _CurrentScanParam.Res = _CurrentScanParam.SmartScanResolution;
            _CurrentScanParam.AlignmentParam.Resolution = _CurrentScanParam.SmartScanResolution;
            _CurrentScanParam.Quality = 1; // SmartScan scans at highest speed (1 = Highest)
            _CurrentScanParam.DynamicBitsAt = false;
            _CurrentScanParam.IsUnidirectionalScan = false;

            UpdatedSignalLevel = new List<Signal>(_CurrentScanParam.Signals);

            // Notify any subscriber that smart scan is starting.
            if (SmartScanStarting != null)
            {
                SmartScanStarting(this);
                // Allow subscriber some time to process
                Thread.Sleep(1000);
            }

            while (true)
            {
                _IsSmartScanCalc = true;

                if (_CurrentScanParam.IsEdrScanning)
                {
                    CommandStatus?.Invoke(this, "EDR scan in progress....");
                }
                else 
                {
                    CommandStatus?.Invoke(this, "SMARTSCAN in progress....");
                }

                if (_CurrentScanParam.IsSequentialScanning)
                {
                    SequentialScanProcess();
                }
                else
                {
                    HorizontalScanProcess();
                    // Scanning completed. Stop scanning.
                    StopScan();
                    ProcessCompletedScanRegion();
                }

                bIsRescanRequired = false;

                int nCurrPixelMax = 0;          // Current scanned max pixel value
                int nCurrentSignalLevel = 0;    // Current scanned signal level
                int nLaserSignalLevel = 0;      // New signal level
                signals.Clear();
                signalOptionsList.Clear();

                for (int i = 0; i < _CurrentScanParam.Signals.Count; i++)
                {
                    if (_CurrentScanParam.Signals[i].LaserChannel == LaserChannels.ChannelC)        //L1
                    {
                        nCurrentSignalLevel = laserL1SignalLevel.Position;
                        currentSignal = laserL1SignalLevel;
                        signalOptionsList = new List<Signal>(_CurrentScanParam.LaserL1SignalOptions);
                        if (_IsSaveDebuggingImages)
                        {
                            string filePath = Path.Combine(_DBTIPath, string.Format("ssTestImageL_SL{0}.tif", nCurrentSignalLevel));
                            ImagingHelper.SaveFile(filePath, _ChannelCImage);
                        }
                        // Signal level already found; don't recalculate
                        //if (L1signalLevelFound)
                        //{
                        //    continue;
                        //}
                    }
                    else if (_CurrentScanParam.Signals[i].LaserChannel == LaserChannels.ChannelA)   //R1
                    {
                        nCurrentSignalLevel = laserR1SignalLevel.Position;
                        currentSignal = laserR1SignalLevel;
                        signalOptionsList = new List<Signal>(_CurrentScanParam.LaserR1SignalOptions);
                        if (_IsSaveDebuggingImages)
                        {
                            string filePath = Path.Combine(_DBTIPath, string.Format("ssTestImageR1_SL{0}.tif", nCurrentSignalLevel));
                            ImagingHelper.SaveFile(filePath, _ChannelAImage);
                        }
                        // Signal level already found; don't recalculate
                        //if (R1signalLevelFound)
                        //{
                        //    continue;
                        //}
                    }
                    else if (_CurrentScanParam.Signals[i].LaserChannel == LaserChannels.ChannelB)   //R2
                    {
                        nCurrentSignalLevel = laserR2SignalLevel.Position;
                        currentSignal = laserR2SignalLevel;
                        signalOptionsList = new List<Signal>(_CurrentScanParam.LaserR2SignalOptions);
                        if (_IsSaveDebuggingImages)
                        {
                            string filePath = Path.Combine(_DBTIPath, string.Format("ssTestImageR2_SL{0}.tif", nCurrentSignalLevel));
                            ImagingHelper.SaveFile(filePath, _ChannelBImage);
                        }
                        // Signal level already found; don't recalculate
                        //if (R2signalLevelFound)
                        //{
                        //    continue;
                        //}
                    }

                    int laserWavelength = _CurrentScanParam.Signals[i].LaserWavelength;
                    switch (_CurrentScanParam.Signals[i].LaserChannel)
                    {
                        case LaserChannels.ChannelA:
                            scannedImage = _ChannelAImage;
                            currentLaserChannel = LaserChannels.ChannelA;
                            break;
                        case LaserChannels.ChannelB:
                            scannedImage = _ChannelBImage;
                            currentLaserChannel = LaserChannels.ChannelB;
                            break;
                        case LaserChannels.ChannelC:
                            scannedImage = _ChannelCImage;
                            currentLaserChannel = LaserChannels.ChannelC;
                            break;
                    }

                    #region Calculate signal level

                    if (scannedImage != null)
                    {
                        // Did smart test-scan stopped on saturation?
                        if (IsDespeckleSmartScan)
                        {
                            scannedImage = ImageProcessing.MedianFilter(scannedImage);
                        }
                        nCurrPixelMax = ImageProcessing.Max(scannedImage, new Rect(0, 0, scannedImage.PixelWidth, scannedImage.PixelHeight));
                    }
                    else
                    {
                        throw new Exception("Scanned Image is NULL");
                    }

                    if (nCurrPixelMax <= nFloor)
                    {
                        if (nAutoScanCount == 0)                            // First pass
                        {
                            nLaserSignalLevel = nSignalStepUpLevel1;        // 13 (level 9)
                            bIsRescanRequired = true;
                        }
                        else if (nAutoScanCount == 1)                       // Second pass
                        {
                            if (nCurrentSignalLevel == nSignalStepUpLevel1) // 13 [level 9]
                            {
                                nLaserSignalLevel = nSignalLevels;          // 14 (level 10)
                                if (currentLaserChannel == LaserChannels.ChannelA) { R1signalLevelFound = true; }
                                if (currentLaserChannel == LaserChannels.ChannelB) { R2signalLevelFound = true; }
                                if (currentLaserChannel == LaserChannels.ChannelC) { L1signalLevelFound = true; }
                            }
                            else if (nCurrentSignalLevel == nSignalStepdownLevel1)  // 6 [level 2]
                            {
                                // 6 + 1 = 7 [level 3] [scenerio 1: start at 9 and stepdown to 6]
                                // 6 + 2 = 8 [level 4] [scenerio 2: start at 9 and stepdown to 4]
                                nLaserSignalLevel = nSignalStepdownLevel1 + (int)((_CurrentScanParam.SmartScanInitSignalLevel - nSignalStepdownLevel1) / 2);
                                bIsRescanRequired = true;                           // do a third pass?
                            }
                            else
                            {
                                nLaserSignalLevel = nCurrentSignalLevel;
                                if (currentLaserChannel == LaserChannels.ChannelA) { R1signalLevelFound = true; }
                                if (currentLaserChannel == LaserChannels.ChannelB) { R2signalLevelFound = true; }
                                if (currentLaserChannel == LaserChannels.ChannelC) { L1signalLevelFound = true; }
                            }
                        }
                        else if (nAutoScanCount == 2)                       // Third pass
                        {
                            if (nCurrentSignalLevel == nSignalLevels)
                            {
                                nLaserSignalLevel = nSignalLevels;
                            }
                            else if (nCurrentSignalLevel == nSignalStepUpLevel1 - 2)    // 11 [level 7]
                            {
                                // 7 + 1 = 8 [level 4]
                                nLaserSignalLevel = nCurrentSignalLevel + 1;
                            }
                            else
                            {
                                nLaserSignalLevel = nCurrentSignalLevel;
                            }
                            if (currentLaserChannel == LaserChannels.ChannelA) { R1signalLevelFound = true; }
                            if (currentLaserChannel == LaserChannels.ChannelB) { R2signalLevelFound = true; }
                            if (currentLaserChannel == LaserChannels.ChannelC) { L1signalLevelFound = true; }
                        }
                    }
                    else if (nCurrPixelMax > nFloor && nCurrPixelMax < nOptimalMin)
                    {
                        double dMultiplier = 0;
                        int nCalcIntensity = 0;

                        if (nCurrentSignalLevel == nSignalLevels)
                        {
                            // Already at the highest level; no need to estimate the next signal level
                            nLaserSignalLevel = nCurrentSignalLevel;
                        }
                        else
                        {
                            for (int n = nCurrentSignalLevel; n < nSignalLevels; n++)
                            {
                                if (_CurrentScanParam.Signals[i].LaserWavelength == 375 ||
                                    _CurrentScanParam.Signals[i].LaserWavelength == 450 ||
                                    _CurrentScanParam.Signals[i].LaserWavelength == 488)
                                {
                                    //Mn = Ln/Li * (Gn/Gi)^Alpha488
                                    //Sn = Mn * Si
                                    dMultiplier = ((double)signalOptionsList[n].LaserIntInmW / (double)currentSignal.LaserIntInmW) * Math.Pow((double)signalOptionsList[n].ApdGain / (double)currentSignal.ApdGain, _CurrentScanParam.SmartScanAlpha488);
                                    nCalcIntensity = (int)(dMultiplier * (double)nCurrPixelMax);
                                }
                                else
                                {
                                    //SOG
                                    //Mn = Ln/Li*Gn/Gi*Pn/Pi
                                    //Sn = Mn * Si
                                    //dMultiplier = ((double)signalOptionsList[n].LaserIntInmW / (double)currentSignal.LaserIntInmW) * ((double)signalOptionsList[n].ApdGain / (double)currentSignal.ApdGain) * ((double)signalOptionsList[n].ApdPga / (double)currentSignal.ApdPga);
                                    //SFL
                                    //Mn = Ln/Li*Gn/Gi*2^(Pn-Pi)
                                    //Sn = Mn * Si
                                    dMultiplier = ((double)signalOptionsList[n].LaserIntInmW / (double)currentSignal.LaserIntInmW) * ((double)signalOptionsList[n].ApdGain / (double)currentSignal.ApdGain) * Math.Pow(2, signalOptionsList[n].ApdPga - currentSignal.ApdPga);
                                    nCalcIntensity = (int)(dMultiplier * (double)nCurrPixelMax);
                                }

                                if (nCalcIntensity > nFloor && nCalcIntensity < nOptimalMin)
                                {
                                    nLaserSignalLevel = signalOptionsList[n].Position;
                                    continue;
                                }
                                else if (nCalcIntensity >= nOptimalMin && nCalcIntensity <= nCeiling)
                                {
                                    // compensating for signal options list is 0 index
                                    nLaserSignalLevel = signalOptionsList[n].Position;
                                    LogMessage(string.Format("Calculated intensity: {0}. Calculated level: {1}", nCalcIntensity, nLaserSignalLevel));
                                    if (currentLaserChannel == LaserChannels.ChannelA) { R1signalLevelFound = true; }
                                    if (currentLaserChannel == LaserChannels.ChannelB) { R2signalLevelFound = true; }
                                    if (currentLaserChannel == LaserChannels.ChannelC) { L1signalLevelFound = true; }
                                    break;
                                }
                                else if (nCalcIntensity > nCeiling)
                                {
                                    n--;
                                    nLaserSignalLevel = signalOptionsList[n].Position;

                                    if (_CurrentScanParam.Signals[i].LaserWavelength == 488)
                                    {
                                        //Mn = Ln/Li * (Gn/Gi)^Alpha488
                                        //Sn = Mn * Si
                                        dMultiplier = ((double)signalOptionsList[n].LaserIntInmW / (double)currentSignal.LaserIntInmW) * Math.Pow((double)signalOptionsList[n].ApdGain / (double)currentSignal.ApdGain, _CurrentScanParam.SmartScanAlpha488);
                                        nCalcIntensity = (int)(dMultiplier * (double)nCurrPixelMax);
                                    }
                                    else
                                    {
                                        //SOG
                                        //Mn = Ln/Li*Gn/Gi*Pn/Pi
                                        //Sn = Mn * Si
                                        //dMultiplier = ((double)signalOptionsList[n].LaserIntInmW / (double)currentSignal.LaserIntInmW) * ((double)signalOptionsList[n].ApdGain / (double)currentSignal.ApdGain) * ((double)signalOptionsList[n].ApdPga / (double)currentSignal.ApdPga);
                                        //SFL
                                        //Mn = Ln/Li*Gn/Gi*2^(Pn-Pi)
                                        //Sn = Mn * Si
                                        dMultiplier = ((double)signalOptionsList[n].LaserIntInmW / (double)currentSignal.LaserIntInmW) * ((double)signalOptionsList[n].ApdGain / (double)currentSignal.ApdGain) * Math.Pow(2, signalOptionsList[n].ApdPga - currentSignal.ApdPga);
                                        nCalcIntensity = (int)(dMultiplier * (double)nCurrPixelMax);
                                    }
                                    LogMessage(string.Format("Calculated intensity: {0}. Stepping back to level: {1}", nCalcIntensity, nLaserSignalLevel));
                                    if (currentLaserChannel == LaserChannels.ChannelA) { R1signalLevelFound = true; }
                                    if (currentLaserChannel == LaserChannels.ChannelB) { R2signalLevelFound = true; }
                                    if (currentLaserChannel == LaserChannels.ChannelC) { L1signalLevelFound = true; }
                                    break;
                                }
                            }
                        }
                    }
                    else if (nCurrPixelMax >= nOptimalMin && nCurrPixelMax <= nCeiling)
                    {
                        nLaserSignalLevel = nCurrentSignalLevel;
                        if (currentLaserChannel == LaserChannels.ChannelA) { R1signalLevelFound = true; }
                        if (currentLaserChannel == LaserChannels.ChannelB) { R2signalLevelFound = true; }
                        if (currentLaserChannel == LaserChannels.ChannelC) { L1signalLevelFound = true; }
                    }
                    else if (nCurrPixelMax > nCeiling)
                    {
                        bool bLevelPreviouslyFound = false;
                        if (_CurrentScanParam.Signals[i].LaserChannel == LaserChannels.ChannelA && R1signalLevelFound)
                        {
                            nLaserSignalLevel = nCurrentSignalLevel - 1;
                            bLevelPreviouslyFound = true;
                        }
                        else if (_CurrentScanParam.Signals[i].LaserChannel == LaserChannels.ChannelB && R2signalLevelFound)
                        {
                            nLaserSignalLevel = nCurrentSignalLevel - 1;
                            bLevelPreviouslyFound = true;
                        }
                        else if (_CurrentScanParam.Signals[i].LaserChannel == LaserChannels.ChannelC && L1signalLevelFound)
                        {
                            nLaserSignalLevel = nCurrentSignalLevel - 1;
                            bLevelPreviouslyFound = true;
                        }

                        if (!bLevelPreviouslyFound)
                        {
                            if (nAutoScanCount == 0)                                    // First pass
                            {
                                nLaserSignalLevel = nSignalStepdownLevel1;              // 6 (level 2 : default)
                                bIsRescanRequired = true;
                            }
                            else if (nAutoScanCount == 1)                               // Second pass
                            {
                                if (nCurrentSignalLevel == nSignalStepdownLevel1)       // 6 (level 2 : default)
                                {
                                    nLaserSignalLevel = nSignalStepdownLevel2;          // 2 (level L2 : default)
                                    bIsRescanRequired = true;
                                }
                                else if (nCurrentSignalLevel == nSignalStepUpLevel1)    // 13 (level 9)
                                {
                                    nLaserSignalLevel = nSignalStepUpLevel1 - 2;        // 11 (level 7)
                                    bIsRescanRequired = true;                           // do a third pass?
                                }
                                else
                                {
                                    nLaserSignalLevel = nCurrentSignalLevel - 1;
                                    if (currentLaserChannel == LaserChannels.ChannelA) { R1signalLevelFound = true; }
                                    if (currentLaserChannel == LaserChannels.ChannelB) { R2signalLevelFound = true; }
                                    if (currentLaserChannel == LaserChannels.ChannelC) { L1signalLevelFound = true; }
                                }
                            }
                            else if (nAutoScanCount == 2)                                   // Third pass
                            {
                                if (nCurrentSignalLevel == nSignalStepdownLevel2)           // default: 2 (level L2)
                                    nLaserSignalLevel = 1;                                  //  1 (level L1)
                                else if (nCurrentSignalLevel == nSignalStepUpLevel1 - 2)    // 11 (level 7)
                                    nLaserSignalLevel = nCurrentSignalLevel - 1;            // 10 (level 6)
                                else
                                    nLaserSignalLevel = nCurrentSignalLevel;
                                if (currentLaserChannel == LaserChannels.ChannelA) { R1signalLevelFound = true; }
                                if (currentLaserChannel == LaserChannels.ChannelB) { R2signalLevelFound = true; }
                                if (currentLaserChannel == LaserChannels.ChannelC) { L1signalLevelFound = true; }
                            }
                        }
                    }

                    // Signal intensity level must be from 1 to nSignalLevels
                    if (nLaserSignalLevel > nSignalLevels)
                        nLaserSignalLevel = nSignalLevels;
                    else if (nLaserSignalLevel < 1)
                        nLaserSignalLevel = 1;

                    #endregion

                    if (_CurrentScanParam.Signals[i].LaserChannel == LaserChannels.ChannelC)        //L1
                    {
                        laserL1SignalLevel = (Signal)_CurrentScanParam.LaserL1SignalOptions[nLaserSignalLevel - 1].Clone();
                        laserL1SignalLevel.ColorChannel = _CurrentScanParam.Signals[i].ColorChannel;
                        laserL1SignalLevel.LaserChannel = LaserChannels.ChannelC;
                        laserL1SignalLevel.LaserWavelength = LaserL1Wavelength;
                        laserL1SignalLevel.SignalLevel = nLaserSignalLevel;
                        int index = UpdatedSignalLevel.FindIndex(x => x.LaserChannel == LaserChannels.ChannelC);
                        UpdatedSignalLevel[index] = laserL1SignalLevel;
                        // NOTE: removing sequential scanning check will behave like
                        // Smart + Sequential (don't scan the scan channel already found the optimal level)
                        if (_CurrentScanParam.IsSequentialScanning)
                        {
                            if (!L1signalLevelFound)
                            {
                                signals.Add(laserL1SignalLevel);
                            }
                        }
                        else
                        {
                            signals.Add(laserL1SignalLevel);
                        }
                    }
                    else if (_CurrentScanParam.Signals[i].LaserChannel == LaserChannels.ChannelA)   //R1
                    {
                        laserR1SignalLevel = (Signal)_CurrentScanParam.LaserR1SignalOptions[nLaserSignalLevel - 1].Clone();
                        laserR1SignalLevel.ColorChannel = _CurrentScanParam.Signals[i].ColorChannel;
                        laserR1SignalLevel.LaserChannel = LaserChannels.ChannelA;
                        laserR1SignalLevel.LaserWavelength = LaserR1Wavelength;
                        laserR1SignalLevel.SignalLevel = nLaserSignalLevel;
                        int index = UpdatedSignalLevel.FindIndex(x => x.LaserChannel == LaserChannels.ChannelA);
                        UpdatedSignalLevel[index] = laserR1SignalLevel;
                        // NOTE: removing sequential scanning check will behave like
                        // Smart + Sequential (don't scan the scan channel already found the optimal level)
                        if (_CurrentScanParam.IsSequentialScanning)
                        {
                            if (!R1signalLevelFound)
                            {
                                signals.Add(laserR1SignalLevel);
                            }
                        }
                        else
                        {
                            signals.Add(laserR1SignalLevel);
                        }
                    }
                    else if (_CurrentScanParam.Signals[i].LaserChannel == LaserChannels.ChannelB)   //R2
                    {
                        laserR2SignalLevel = (Signal)_CurrentScanParam.LaserR2SignalOptions[nLaserSignalLevel - 1].Clone();
                        laserR2SignalLevel.ColorChannel = _CurrentScanParam.Signals[i].ColorChannel;
                        laserR2SignalLevel.LaserChannel = LaserChannels.ChannelB;
                        laserR2SignalLevel.LaserWavelength = LaserR2Wavelength;
                        laserR2SignalLevel.SignalLevel = nLaserSignalLevel;
                        int index = UpdatedSignalLevel.FindIndex(x => x.LaserChannel == LaserChannels.ChannelB);
                        UpdatedSignalLevel[index] = laserR2SignalLevel;
                        // NOTE: removing sequential scanning check will behave like
                        // Smart + Sequential (don't scan the scan channel already found the optimal level)
                        if (_CurrentScanParam.IsSequentialScanning)
                        {
                            if (!R2signalLevelFound)
                            {
                                signals.Add(laserR2SignalLevel);
                            }
                        }
                        else
                        {
                            signals.Add(laserR2SignalLevel);
                        }
                    }
                }   //for loop

                if (signals.Count > 0)
                {
                    // Calculate signal levels
                    _CurrentScanParam.Signals = new List<Signal>(signals);
                }
                else
                {
                    _CurrentScanParam.Signals = new List<Signal>(UpdatedSignalLevel);
                }

                // Notify subscriber to update the intensity level
                if (SmartScanUpdated != null)
                {
                    SmartScanUpdated(this);
                    Thread.Sleep(1000);
                }

                if (bIsRescanRequired)
                {
                    nAutoScanCount++;
                    continue;
                }
                else
                {
                    break;
                }
            } //while loop

            if (_CurrentScanParam.Signals.Count < UpdatedSignalLevel.Count)
            {
                _CurrentScanParam.Signals.Clear();
                _CurrentScanParam.Signals = new List<Signal>(UpdatedSignalLevel);
            }

            // Smartscan calculation completed
            _IsSmartScanCalc = false;

            LogMessage("SmartScan completed: restoring selected scan parameters...");

            // Restore 'normal' scan parameters
            _CurrentScanParam.Width = origWidth;
            _CurrentScanParam.Height = origHeight;
            _CurrentScanParam.Res = origResolution;
            _CurrentScanParam.AlignmentParam.Resolution = origResolution;
            _CurrentScanParam.Quality = origQuality;
            _CurrentScanParam.DynamicBitsAt = origIsExtDynamicRange;
            //_CurrentScanParam.Time = origScanTime;
            _CurrentScanParam.IsUnidirectionalScan = origIsUndirectional;

            if (SmartScanCompleted != null)
            {
                SmartScanCompleted(this);
                // Allow subscriber some time to process
                Thread.Sleep(1000);
            }

            LogMessage("SmartScan: starting normal scan with user selected parameters...");

            // Start normal scan process.
            if (!_CurrentScanParam.IsZScanning && !_CurrentScanParam.IsEdrScanning)
            {
                if (_CurrentScanParam.IsSequentialScanning)
                {
                    SequentialScanProcess();
                }
                else
                {
                    HorizontalScanProcess();
                    StopScan();
                    ProcessCompletedScanRegion();
                }
            }
        }

        #endregion

        #region Sequential Scan

        private void SequentialScanProcess()
        {
            // These info added in ProcessCompletedScanRegion()
            //_ImageInfo.CaptureType = "Fluorescence";
            //_ImageInfo.IsScannedImage = true;
            //_ImageInfo.ScanResolution = _CurrentScanParam.Res;
            //_ImageInfo.ScanQuality = _CurrentScanParam.Quality;
            //_ImageInfo.ScanX0 = _CurrentScanParam.ScanX0;
            //_ImageInfo.ScanY0 = _CurrentScanParam.ScanY0;
            //_ImageInfo.DeltaX = (int)((double)_CurrentScanParam.ScanDeltaX / (double)_CurrentScanParam.XMotorSubdivision);
            //_ImageInfo.DeltaY = (int)((double)_CurrentScanParam.ScanDeltaY / (double)_CurrentScanParam.YMotorSubdivision);
            //_ImageInfo.ScanZ0 = Math.Round((double)_CurrentScanParam.ScanZ0 / (double)_CurrentScanParam.ZMotorSubdivision, 3);
            ScanType = ScanTypes.Horizontal;

            var signals = new List<Signal>(_CurrentScanParam.Signals);

            for (int index = 0; index < signals.Count && !_IsScanAborted; index++)
            {
                // Don't display this status message while sequentially smart scanning
                if (_IsEdrScanCalc)
                {
                    string statusMsg = string.Format("EDR scan in progress....[{0}]", signals[index].LaserWavelength);
                    CommandStatus?.Invoke(this, statusMsg);
                }
                else if (_IsSmartScanCalc)
                {
                    string statusMsg = string.Format("SMARTSCAN in progress....[{0}]", signals[index].LaserWavelength);
                    CommandStatus?.Invoke(this, statusMsg);
                }
                else
                {
                    if (_CurrentScanParam.IsEdrScanning)
                    {
                        CommandStatus?.Invoke(this, string.Format("EDR scan....[{0}] {1} / {2}", signals[index].LaserWavelength, index + 1, signals.Count));
                    }
                    else
                    {
                        CommandStatus?.Invoke(this, string.Format("Sequential scan....[{0}] {1} / {2}", signals[index].LaserWavelength, index + 1, signals.Count));
                    }
                }

                _CurrentScanParam.Signals.Clear();
                _CurrentScanParam.Signals.Add(signals[index]);

                // Display only the current scan channel
                _IsChannelASelected = (signals[index].LaserChannel == LaserChannels.ChannelA);  //R1
                _IsChannelBSelected = (signals[index].LaserChannel == LaserChannels.ChannelB);  //R2
                _IsChannelCSelected = (signals[index].LaserChannel == LaserChannels.ChannelC);  //L1

                if (SequentialChannelStarting != null)
                {
                    SequentialChannelStarting?.Invoke(this, signals[index].LaserChannel);
                    Thread.Sleep(1000);
                }

                HorizontalScanProcess();
                StopScan();
                if (!_IsScanAborted)
                {
                    ProcessCompletedScanRegion();
                    SequentialChannelCompleted?.Invoke(this, signals[index].LaserChannel);
                }
            }

            // Restore _Signals from backup
            _CurrentScanParam.Signals = new List<Signal>(signals);

            // Restore selected channels
            foreach (Signal signal in _CurrentScanParam.Signals)
            {
                if (signal.LaserChannel == LaserChannels.ChannelC)      //L1
                {
                    _IsChannelCSelected = true;
                }
                else if (signal.LaserChannel == LaserChannels.ChannelA) //R1
                {
                    _IsChannelASelected = true;
                }
                else if (signal.LaserChannel == LaserChannels.ChannelB) //R2
                {
                    _IsChannelBSelected = true;
                }
            }
        }

        #endregion Sequential Scan

        #region Z-Scanning

        private void ZScanningProcess()
        {
            // These info added in ProcessCompletedScanRegion()
            //_ImageInfo.CaptureType = "Fluorescence";
            //_ImageInfo.IsScannedImage = true;
            //_ImageInfo.ScanResolution = _CurrentScanParam.Res;
            //_ImageInfo.ScanQuality = _CurrentScanParam.Quality;
            //_ImageInfo.ScanX0 = _CurrentScanParam.ScanX0;
            //_ImageInfo.ScanY0 = _CurrentScanParam.ScanY0;
            //_ImageInfo.DeltaX = (int)((double)_CurrentScanParam.ScanDeltaX / (double)_CurrentScanParam.XMotorSubdivision);
            //_ImageInfo.DeltaY = (int)((double)_CurrentScanParam.ScanDeltaY / (double)_CurrentScanParam.YMotorSubdivision);
            ScanType = ScanTypes.Horizontal;
            _ZSequentialSetIndex = 0;

            List<Signal> signalsBackup = null;
            if (_CurrentScanParam.IsSequentialScanning)
            {
                signalsBackup = new List<Signal>(_CurrentScanParam.Signals);
            }

            for (int index = 0; index < _CurrentScanParam.NumOfImages && !_IsScanAborted; index++)
            {
                _CurrentScanParam.ScanZ0 = (int)((_CurrentScanParam.AbsFocusPosition + (_CurrentScanParam.BottomImageFocus + index * _CurrentScanParam.DeltaFocus)) * (double)_CurrentScanParam.ZMotorSubdivision);
                _ImageInfo.ScanZ0 = Math.Round((double)_CurrentScanParam.ScanZ0 / (double)_CurrentScanParam.ZMotorSubdivision, 3);

                if (_CurrentScanParam.IsEdrScanning)
                {
                    EDRScanningProcess();
                }
                else
                {
                    // Start Smartscan to calculate signal level
                    if (_CurrentScanParam.IsSmartScanning)
                    {
                        // Notify any subscriber that smart scan is starting.
                        //if (SmartScanStarting != null)
                        //{
                        //    SmartScanStarting(this);
                        //    // Allow subscriber some time to process
                        //    Thread.Sleep(1000);
                        //}
                        // Start smart scan process.
                        SmartscanProcess();
                    }

                    if (_CurrentScanParam.IsSequentialScanning)
                    {
                        _ZSequentialSetIndex = index;
                        _CurrentScanParam.Signals = new List<Signal>(signalsBackup);
                        SequentialScanProcess();
                    }
                    else
                    {
                        CommandStatus?.Invoke(this, string.Format("Preparing to scan....{0}/{1}", index + 1, _CurrentScanParam.NumOfImages));
                        HorizontalScanProcess();
                        // SequentialScanProcess called Stop() and ProcessCompletedScanRegion()
                        StopScan();
                        if (!_IsScanAborted)
                        {
                            // Process the current completed scan region
                            ProcessCompletedScanRegion();
                        }
                    }
                }

                if (!_IsScanAborted)
                {
                    if (ZScanningCompleted != null)
                    {
                        ZScanningCompleted?.Invoke(this, (ImageInfo)_ImageInfo.Clone(), _CurrentScanRegion);
                        Thread.Sleep(1000);
                    }
                }
            }
        }

        #endregion

        #region Extended Dynamic Range (EDR) scan [OLD]

        /*private void EDRScanningProcess()
        {
            // These info added in ProcessCompletedScanRegion()
            //_ImageInfo.CaptureType = "Fluorescence";
            //_ImageInfo.IsScannedImage = true;
            //_ImageInfo.ScanResolution = _CurrentScanParam.Res;
            //_ImageInfo.ScanQuality = _CurrentScanParam.Quality;
            //_ImageInfo.ScanX0 = _CurrentScanParam.ScanX0;
            //_ImageInfo.ScanY0 = _CurrentScanParam.ScanY0;
            //_ImageInfo.DeltaX = (int)((double)_CurrentScanParam.ScanDeltaX / (double)_CurrentScanParam.XMotorSubdivision);
            //_ImageInfo.DeltaY = (int)((double)_CurrentScanParam.ScanDeltaY / (double)_CurrentScanParam.YMotorSubdivision);
            //ScanType = ScanTypes.Horizontal;
            _IsEdrScanCalc = true;
            _CurrentScanParam.IsSmartScanning = true;
            _IsSmartScanCalc = true;

            //int origResolution = _CurrentScanParam.Res;
            int L1SignalLevel = 0;
            int R1SignalLevel = 0;
            int R2SignalLevel = 0;

            LogMessage("EDR: Start SmartScan");
            CommandStatus?.Invoke(this, "EDR scan in progress....");

            // 1. Use smart scan to acquire an unsaturated scan image I1
            // Start smart scan process.
            SmartscanProcess();

            CommandStatus?.Invoke(this, "EDR scan in progress....");

            EdrSaturatedChannels = new List<LaserChannels>();
            foreach (var signal in _CurrentScanParam.Signals)
            {
                if (signal.LaserChannel == LaserChannels.ChannelC)
                {
                    L1SignalLevel = signal.SignalLevel;
                    //Check for saturation, abort operation if saturated
                    if (_ChannelCImage != null)
                    {
                        int width = _ChannelCImage.PixelWidth;
                        int height = _ChannelCImage.PixelHeight;
                        _ChannelCImage = ImageProcessing.MedianFilter(_ChannelCImage);
                        int pixelMax = ImageProcessing.Max(_ChannelCImage, new Rect(0, 0, width, height));
                        if (pixelMax > _SaturationThreshold)
                        {
                            IsEdrSaturationAbort = true;
                            EdrSaturatedChannels.Add(LaserChannels.ChannelC);
                            Abort();
                        }
                    }
                }
                else if (signal.LaserChannel == LaserChannels.ChannelA)
                {
                    R1SignalLevel = signal.SignalLevel;
                    //Check for saturation, abort operation if saturated
                    if (_ChannelAImage != null)
                    {
                        int width = _ChannelAImage.PixelWidth;
                        int height = _ChannelAImage.PixelHeight;
                        _ChannelAImage = ImageProcessing.MedianFilter(_ChannelAImage);
                        int pixelMax = ImageProcessing.Max(_ChannelAImage, new Rect(0, 0, width, height));
                        if (pixelMax > _SaturationThreshold)
                        {
                            IsEdrSaturationAbort = true;
                            EdrSaturatedChannels.Add(LaserChannels.ChannelA);
                            Abort();
                        }
                    }
                }
                else if (signal.LaserChannel == LaserChannels.ChannelB)
                {
                    R2SignalLevel = signal.SignalLevel;
                    //Check for saturation, abort operation if saturated
                    if (_ChannelBImage != null)
                    {
                        int width = _ChannelBImage.PixelWidth;
                        int height = _ChannelBImage.PixelHeight;
                        _ChannelBImage = ImageProcessing.MedianFilter(_ChannelBImage);
                        int pixelMax = ImageProcessing.Max(_ChannelBImage, new Rect(0, 0, width, height));
                        if (pixelMax > _SaturationThreshold)
                        {
                            IsEdrSaturationAbort = true;
                            EdrSaturatedChannels.Add(LaserChannels.ChannelB);
                            Abort();
                        }
                    }
                }
            }

            LogMessage("EDR: Scanning the reference image [1]: " + _CurrentScanParam.Res.ToString());

            // Scan the first reference image:
            // Scan with SmartScan calculated signal level at the user's selected resolution.
            // NOTE: SmartscanProcess already restore the user selected scan parameters.
            //
            if (_CurrentScanParam.IsSequentialScanning)
            {
                // Start sequential scan
                //
                SequentialScanProcess();
            }
            else
            {
                // Start normal scan
                HorizontalScanProcess();
                // SequentialScanProcess called Stop() and ProcessCompletedScanRegion()
                StopScan();
                ProcessCompletedScanRegion();
            }

            // Save scanned image(s) for later use.
            //
            if (_IsChannelCSelected)    //L1 laser channel
            {
                if (ChannelCImage != null)
                {
                    _ChannelCImageEDR1 = _ChannelCImage.Clone();
                    if (_IsSaveDebuggingImages)
                    {
                        string filePath = Path.Combine(_DBTIPath, "EDRSmart_ChannelC.tif");
                        ImagingHelper.SaveFile(filePath, _ChannelCImageEDR1);
                    }
                }
            }
            if (_IsChannelASelected)    //R1 laser channel
            {
                if (_ChannelAImage != null)
                {
                    _ChannelAImageEDR1 = _ChannelAImage.Clone();
                    if (_IsSaveDebuggingImages)
                    {
                        string filePath = Path.Combine(_DBTIPath, "EDRSmart_ChannelA.tif");
                        ImagingHelper.SaveFile(filePath, _ChannelAImageEDR1);
                    }
                }
            }
            if (_IsChannelBSelected)    //R2 laser channel
            {
                if (_ChannelBImage != null)
                {
                    _ChannelBImageEDR1 = _ChannelBImage.Clone();
                    if (_IsSaveDebuggingImages)
                    {
                        string filePath = Path.Combine(_DBTIPath, "EDRSmart_ChannelB.tif");
                        ImagingHelper.SaveFile(filePath, _ChannelBImageEDR1);
                    }
                }
            }

            // EDR processing scans
            //
            int origWidth = _CurrentScanParam.Width;
            int origHeight = _CurrentScanParam.Height;
            int origResolution = _CurrentScanParam.Res;
            int origQuality = _CurrentScanParam.Quality; // scan speed
            //bool origIsExtDynamicRange = _CurrentScanParam.DynamicBitsAt;
            //int origScanTime = _CurrentScanParam.Time;
            bool origIsUndirectional = _CurrentScanParam.IsUnidirectionalScan;
            _CurrentScanParam.Res = 500;
            _CurrentScanParam.Width = (int)(_CurrentScanParam.ScanDeltaX / _CurrentScanParam.XMotorSubdivision * 1000.0 / _CurrentScanParam.Res);
            _CurrentScanParam.Height = (int)(_CurrentScanParam.ScanDeltaY / _CurrentScanParam.YMotorSubdivision * 1000.0 / _CurrentScanParam.Res);
            _CurrentScanParam.AlignmentParam.Resolution = _CurrentScanParam.Res;
            _CurrentScanParam.Quality = 1; // SmartScan scans at highest speed (1 = Highest)
            //_CurrentScanParam.DynamicBitsAt = false;
            _CurrentScanParam.IsUnidirectionalScan = false;

            //List<Signal> bkupSignals = new List<Signal>(_CurrentScanParam.Signals);
            UpdatedSignalLevel = new List<Signal>(_CurrentScanParam.Signals);

            // Notify subscriber to update the intensity level
            if (EDRTestScanStarting != null)
            {
                EDRTestScanStarting(this);
                // Allow subscriber some time to process
                Thread.Sleep(1000);
            }

            CommandStatus?.Invoke(this, "EDR scan in progress....");

            LogMessage("EDR: Scanning test image: " + _CurrentScanParam.Res.ToString());

            // Scan test image 1 at 500 resolution
            //
            if (_CurrentScanParam.IsSequentialScanning)
            {
                // Start sequential scan
                //
                SequentialScanProcess();
            }
            else
            {
                HorizontalScanProcess();
                StopScan();
                ProcessCompletedScanRegion();
            }

            WriteableBitmap edrTestImage1_L1 = null;
            WriteableBitmap edrTestImage1_R1 = null;
            WriteableBitmap edrTestImage1_R2 = null;
            Point offsetL1 = new Point();
            Point offsetR1 = new Point();
            Point offsetR2 = new Point();
            bool bHasOffsetL1 = false;
            bool bHasOffsetR1 = false;
            bool bHasOffsetR2 = false;
            bool bIsBlobFound = false;
            int testImageIndex = 1;
            string fileName = string.Empty;

            // 3. For any I2 pixel P(x, y) with intensity > 60000, replace it with I1 pixel Q(x, y) intensity multiplied by 16 and the new image is saved as I3;
            // 4. Compress the new image I3 to I4 by power of 16 / 20;

            if (_IsChannelCSelected)    //L1
            {
                if (_ChannelCImage != null)
                {
                    edrTestImage1_L1 = _ChannelCImage.Clone();
                    if (_IsSaveDebuggingImages)
                    {
                        fileName = string.Format("EDRTestImage_ChannelC_{0}.tif", testImageIndex);
                        string filePath = Path.Combine(_DBTIPath, fileName);
                        ImagingHelper.SaveFile(filePath, _ChannelCImage);
                    }
                    if (!bHasOffsetL1)
                    {
                        bIsBlobFound = false;
                        offsetL1 = GetOffSet(edrTestImage1_L1, ref bIsBlobFound);
                        if (offsetL1.X != 0 && offsetL1.Y != 0)
                        {
                            bHasOffsetL1 = true;
                        }
                        // Blobs found use the same offset (if multi-channel scan)
                        if (bIsBlobFound)
                        {
                            offsetR1.X = offsetL1.X;
                            offsetR1.Y = offsetL1.Y;
                            offsetR2.X = offsetL1.X;
                            offsetR2.Y = offsetL1.Y;
                            bHasOffsetR1 = true;
                            bHasOffsetR2 = true;
                        }
                    }
                }
            }
            if (_IsChannelASelected)    //R1
            {
                if (_ChannelAImage != null)
                {
                    edrTestImage1_R1 = _ChannelAImage.Clone();
                    if (_IsSaveDebuggingImages)
                    {
                        fileName = string.Format("EDRTestImage_ChannelA_{0}.tif", testImageIndex);
                        string filePath = Path.Combine(_DBTIPath, fileName);
                        ImagingHelper.SaveFile(filePath, _ChannelAImage);
                    }
                    if (!bHasOffsetR1)
                    {
                        bIsBlobFound = false;
                        offsetR1 = GetOffSet(edrTestImage1_R1, ref bIsBlobFound);
                        if (offsetR1.X != 0 && offsetR1.Y != 0)
                        {
                            bHasOffsetR1 = true;
                        }
                        // Blobs found use the same offset (if multi-channel scan)
                        if (bIsBlobFound)
                        {
                            offsetL1.X = offsetR1.X;
                            offsetL1.Y = offsetR1.Y;
                            offsetR2.X = offsetR1.X;
                            offsetR2.Y = offsetR1.Y;
                            bHasOffsetL1 = true;
                            bHasOffsetR2 = true;
                        }
                    }
                }
            }
            if (_IsChannelBSelected)    //R2
            {
                if (_ChannelBImage != null)
                {
                    edrTestImage1_R2 = _ChannelBImage.Clone();
                    if (_IsSaveDebuggingImages)
                    {
                        fileName = string.Format("EDRTestImage_ChannelB_{0}.tif", testImageIndex);
                        string filePath = Path.Combine(_DBTIPath, fileName);
                        ImagingHelper.SaveFile(filePath, _ChannelBImage);
                    }
                    if (!bHasOffsetR2)
                    {
                        bIsBlobFound = false;
                        offsetR2 = GetOffSet(edrTestImage1_R2, ref bIsBlobFound);
                        if (offsetR2.X != 0 && offsetR2.Y != 0)
                        {
                            bHasOffsetR2 = true;
                        }
                        // Blobs found use the same offset (if multi-channel scan)
                        if (bIsBlobFound)
                        {
                            offsetL1.X = offsetR2.X;
                            offsetL1.Y = offsetR2.Y;
                            offsetR1.X = offsetR2.X;
                            offsetR1.Y = offsetR2.Y;
                            bHasOffsetL1 = true;
                            bHasOffsetR1 = true;
                        }
                    }
                }
            }

            WriteableBitmap edrTestImage2_L1 = null;
            WriteableBitmap edrTestImage2_R1 = null;
            WriteableBitmap edrTestImage2_R2 = null;
            double L1ScaleFactor = 0;
            double R1ScaleFactor = 0;
            double R2ScaleFactor = 0;
            bool bIsThresholdFound = false;
            bool bIsL1LevelFound = false;
            bool bIsR1LevelFound = false;
            bool bIsR2LevelFound = false;
            _IsEdrScanCalc = true;
            testImageIndex = 2; // save debugging/test image(s) index
            int L1GetScaleFactorErrorCount = 0;
            int R1GetScaleFactorErrorCount = 0;
            int R2GetScaleFactorErrorCount = 0;

            if (!_IsChannelASelected)
            {
                bIsR1LevelFound = true;
            }
            if (!_IsChannelBSelected)
            {
                bIsR2LevelFound = true;
            }
            if (!_IsChannelCSelected)
            {
                bIsL1LevelFound = true;
            }

            do
            {
                for (int i = 0; i < _CurrentScanParam.Signals.Count; i++)
                {
                    #region Update Signal Level...

                    if (_CurrentScanParam.Signals[i].LaserChannel == LaserChannels.ChannelC && _IsChannelCSelected && !bIsL1LevelFound)
                    {
                        L1SignalLevel++;
                        if (L1SignalLevel <= _CurrentScanParam.LaserL1SignalOptions.Count)
                        {
                            var colorChannel = _CurrentScanParam.Signals[i].ColorChannel;
                            _CurrentScanParam.Signals[i] = _CurrentScanParam.LaserL1SignalOptions[L1SignalLevel - 1];
                            _CurrentScanParam.Signals[i].LaserChannel = LaserChannels.ChannelC;
                            _CurrentScanParam.Signals[i].ColorChannel = colorChannel;
                            _CurrentScanParam.Signals[i].SignalLevel = _CurrentScanParam.Signals[i].Position;
                            int index = UpdatedSignalLevel.FindIndex(x => x.LaserChannel == LaserChannels.ChannelC);
                            UpdatedSignalLevel[index] = _CurrentScanParam.Signals[i];
                        }
                        else
                        {
                            L1SignalLevel = _CurrentScanParam.LaserL1SignalOptions[_CurrentScanParam.LaserL1SignalOptions.Count - 1].Position;
                            bIsL1LevelFound = true;
                            if (_CurrentScanParam.IsSequentialScanning && _CurrentScanParam.Signals.Count > 1)
                            {
                                //remove laser channel to avoid scanning the laser channel that already the optimal level
                                foreach (var signal in _CurrentScanParam.Signals)
                                {
                                    if (signal.LaserChannel == LaserChannels.ChannelC)
                                    {
                                        _CurrentScanParam.Signals.Remove(signal);
                                        break;
                                    }
                                }
                            }
                        }
                    }
                    else if (_CurrentScanParam.Signals[i].LaserChannel == LaserChannels.ChannelA && _IsChannelASelected && !bIsR1LevelFound)
                    {
                        R1SignalLevel++;
                        if (R1SignalLevel <= _CurrentScanParam.LaserR1SignalOptions.Count)
                        {
                            var colorChannel = _CurrentScanParam.Signals[i].ColorChannel;
                            _CurrentScanParam.Signals[i] = _CurrentScanParam.LaserR1SignalOptions[R1SignalLevel - 1];
                            _CurrentScanParam.Signals[i].LaserChannel = LaserChannels.ChannelA;
                            _CurrentScanParam.Signals[i].ColorChannel = colorChannel;
                            _CurrentScanParam.Signals[i].SignalLevel = _CurrentScanParam.Signals[i].Position;
                            int index = UpdatedSignalLevel.FindIndex(x => x.LaserChannel == LaserChannels.ChannelA);
                            UpdatedSignalLevel[index] = _CurrentScanParam.Signals[i];
                        }
                        else
                        {
                            R1SignalLevel = _CurrentScanParam.LaserR1SignalOptions[_CurrentScanParam.LaserR1SignalOptions.Count - 1].Position;
                            bIsR1LevelFound = true;
                            if (_CurrentScanParam.IsSequentialScanning && _CurrentScanParam.Signals.Count > 1)
                            {
                                //remove laser channel to avoid scanning the laser channel that already the optimal level
                                foreach (var signal in _CurrentScanParam.Signals)
                                {
                                    if (signal.LaserChannel == LaserChannels.ChannelA)
                                    {
                                        _CurrentScanParam.Signals.Remove(signal);
                                        break;
                                    }
                                }
                            }
                        }
                    }
                    else if (_CurrentScanParam.Signals[i].LaserChannel == LaserChannels.ChannelB && _IsChannelBSelected && !bIsR2LevelFound)
                    {
                        R2SignalLevel++;
                        if (R2SignalLevel <= _CurrentScanParam.LaserR2SignalOptions.Count)
                        {
                            var colorChannel = _CurrentScanParam.Signals[i].ColorChannel;
                            _CurrentScanParam.Signals[i] = _CurrentScanParam.LaserR2SignalOptions[R2SignalLevel - 1];
                            _CurrentScanParam.Signals[i].LaserChannel = LaserChannels.ChannelB;
                            _CurrentScanParam.Signals[i].ColorChannel = colorChannel;
                            _CurrentScanParam.Signals[i].SignalLevel = _CurrentScanParam.Signals[i].Position;
                            int index = UpdatedSignalLevel.FindIndex(x => x.LaserChannel == LaserChannels.ChannelB);
                            UpdatedSignalLevel[index] = _CurrentScanParam.Signals[i];
                        }
                        else
                        {
                            R2SignalLevel = _CurrentScanParam.LaserR2SignalOptions[_CurrentScanParam.LaserR2SignalOptions.Count - 1].Position;
                            bIsR2LevelFound = true;
                            if (_CurrentScanParam.IsSequentialScanning && _CurrentScanParam.Signals.Count > 1)
                            {
                                //remove laser channel to avoid scanning the laser channel that already the optimal level
                                foreach (var signal in _CurrentScanParam.Signals)
                                {
                                    if (signal.LaserChannel == LaserChannels.ChannelB)
                                    {
                                        _CurrentScanParam.Signals.Remove(signal);
                                        break;
                                    }
                                }
                            }
                        }
                    }

                    #endregion
                }

                if (bIsL1LevelFound && bIsR1LevelFound && bIsR2LevelFound)
                {
                    bIsThresholdFound = true;
                }
                else
                {
                    // Notify subscriber to update the intensity level
                    if (EDRTestScanUpdating != null)
                    {
                        EDRTestScanUpdating(this);
                        // Allow subscriber some time to process
                        Thread.Sleep(1000);
                    }

                    CommandStatus?.Invoke(this, "EDR scan in progress....");

                    if (_CurrentScanParam.IsSequentialScanning)
                    {
                        // Start sequential scan
                        //
                        SequentialScanProcess();
                    }
                    else
                    {
                        HorizontalScanProcess();
                        StopScan();
                        ProcessCompletedScanRegion();
                    }

                    if (_IsChannelCSelected)    //L1
                    {
                        if (_ChannelCImage != null)
                        {
                            edrTestImage2_L1 = _ChannelCImage.Clone();
                            if (_IsSaveDebuggingImages)
                            {
                                fileName = string.Format("EDRTestImage_ChannelC_{0}.tif", testImageIndex);
                                string filePath = Path.Combine(_DBTIPath, fileName);
                                ImagingHelper.SaveFile(filePath, _ChannelCImage);
                            }
                        }
                    }
                    if (_IsChannelASelected)    //R1
                    {
                        if (_ChannelAImage != null)
                        {
                            edrTestImage2_R1 = _ChannelAImage.Clone();
                            if (_IsSaveDebuggingImages)
                            {
                                fileName = string.Format("EDRTestImage_ChannelA_{0}.tif", testImageIndex);
                                string filePath = Path.Combine(_DBTIPath, fileName);
                                ImagingHelper.SaveFile(filePath, _ChannelAImage);
                            }
                        }
                    }
                    if (_IsChannelBSelected)    //R2
                    {
                        if (_ChannelBImage != null)
                        {
                            edrTestImage2_R2 = _ChannelBImage.Clone();
                            if (_IsSaveDebuggingImages)
                            {
                                fileName = string.Format("EDRTestImage_ChannelB_{0}.tif", testImageIndex);
                                string filePath = Path.Combine(_DBTIPath, fileName);
                                ImagingHelper.SaveFile(filePath, _ChannelBImage);
                            }
                        }
                    }

                    if (_IsChannelCSelected && !bIsL1LevelFound)
                    {
                        #region Calculating L1 channel scale factor
                        //EL: TODO: sometimes the calculated scale factor tailed off on the next intensity level up
                        // if tailed off use the previous high as the scale factor
                        double dScaleFactor = Math.Round(GetScaleFactor(edrTestImage1_L1, edrTestImage2_L1, offsetL1), 3);
                        if (dScaleFactor == 0)
                        {
                            L1GetScaleFactorErrorCount++;
                            if (L1GetScaleFactorErrorCount == 2)
                            {
                                throw new Exception("Could not do EDR scan on this sample.\nLaser module Port #1: unable to calculate the scale factor.");
                            }
                        }
                        if (double.IsNaN(dScaleFactor))
                        {
                            // Went to far (probably totally saturated)
                            // Use the previous scan level and scale factor
                            L1SignalLevel--;
                            bIsL1LevelFound = true;
                        }
                        else if (dScaleFactor > L1ScaleFactor)
                        {
                            if (Math.Round(dScaleFactor) >= _CurrentScanParam.EdrScaleFactor)
                            {
                                double remainder = dScaleFactor % _CurrentScanParam.EdrScaleFactor;
                                if (remainder >= 1)
                                {
                                    // Use the previous scan level and scale factor
                                    L1SignalLevel--;
                                }
                                else
                                {
                                    // Use the new scale factor and current scan level
                                    L1ScaleFactor = dScaleFactor;
                                }
                                bIsL1LevelFound = true;
                            }
                            else
                            {
                                L1ScaleFactor = dScaleFactor;
                            }
                        }
                        else if (dScaleFactor < L1ScaleFactor)
                        {
                            // Signal tailing off
                            // Use the previous scan level and scale factor
                            L1SignalLevel--;
                            bIsL1LevelFound = true;
                        }
                        if (bIsL1LevelFound)
                        {
                            if (_CurrentScanParam.IsSequentialScanning && _CurrentScanParam.Signals.Count > 1)
                            {
                                // Remove laser channel to avoid scanning the laser channel that already the optimal level
                                foreach (var signal in _CurrentScanParam.Signals)
                                {
                                    if (signal.LaserChannel == LaserChannels.ChannelC)
                                    {
                                        _CurrentScanParam.Signals.Remove(signal);
                                        break;
                                    }
                                }
                            }
                        }
                        LogMessage("EDR: Get scale factor (L): " + L1ScaleFactor.ToString());
                        #endregion
                    }
                    if (_IsChannelASelected && !bIsR1LevelFound)
                    {
                        //EL: TODO: sometimes the calculated scale factor tailed off on the next intensity level up
                        // if tailed off use the previous high as the scale factor
                        double dScaleFactor = Math.Round(GetScaleFactor(edrTestImage1_R1, edrTestImage2_R1, offsetR1), 3);
                        if (dScaleFactor == 0)
                        {
                            R1GetScaleFactorErrorCount++;
                            if (R1GetScaleFactorErrorCount == 2)
                            {
                                throw new Exception("Could not do EDR scan on this sample.\nLaser module Port #2: unable to calculate the scale factor.");
                            }
                        }
                        if (double.IsNaN(dScaleFactor))
                        {
                            // Went to far (probably totally saturated)
                            // Use the previous scan level and scale factor
                            R1SignalLevel--;
                            bIsR1LevelFound = true;
                        }
                        else if (dScaleFactor > R1ScaleFactor)
                        {
                            if (Math.Round(dScaleFactor) >= _CurrentScanParam.EdrScaleFactor)
                            {
                                double remainder = dScaleFactor % _CurrentScanParam.EdrScaleFactor;
                                if (remainder >= 1)
                                {
                                    // Use the previous scan level and scale factor
                                    R1SignalLevel--;
                                }
                                else
                                {
                                    // Use the new scale factor and current scan level
                                    R1ScaleFactor = dScaleFactor;
                                }
                                bIsR1LevelFound = true;
                            }
                            else
                            {
                                R1ScaleFactor = dScaleFactor;
                            }
                        }
                        else if (dScaleFactor < R1ScaleFactor)
                        {
                            // Signal tailing off
                            // Use the previous scan level and scale factor
                            R1SignalLevel--;
                            bIsR1LevelFound = true;
                        }
                        if (bIsR1LevelFound)
                        {
                            if (_CurrentScanParam.IsSequentialScanning && _CurrentScanParam.Signals.Count > 1)
                            {
                                // Remove laser channel to avoid scanning the laser channel that already the optimal level
                                foreach (var signal in _CurrentScanParam.Signals)
                                {
                                    if (signal.LaserChannel == LaserChannels.ChannelA)
                                    {
                                        _CurrentScanParam.Signals.Remove(signal);
                                        break;
                                    }
                                }
                            }
                        }
                        LogMessage("EDR: Get scale factor (R1): " + R1ScaleFactor.ToString());
                    }
                    if (_IsChannelBSelected)
                    {
                        //EL: TODO: sometimes the calculated scale factor tailed off on the next intensity level up
                        // if tailed off use the previous high as the scale factor
                        double dScaleFactor = Math.Round(GetScaleFactor(edrTestImage1_R2, edrTestImage2_R2, offsetR2), 3);
                        if (dScaleFactor == 0)
                        {
                            R2GetScaleFactorErrorCount++;
                            if (R2GetScaleFactorErrorCount == 2)
                            {
                                throw new Exception("Could not do EDR scan on this sample.\nLaser module Port #3: unable to calculate the scale factor.");
                            }
                        }
                        if (double.IsNaN(dScaleFactor))
                        {
                            // Went to far (probably totally saturated)
                            // Use the previous scan level and scale factor
                            R2SignalLevel--;
                            bIsR2LevelFound = true;
                        }
                        else if (dScaleFactor > R2ScaleFactor)
                        {
                            if (Math.Round(dScaleFactor) >= _CurrentScanParam.EdrScaleFactor)
                            {
                                double remainder = dScaleFactor % _CurrentScanParam.EdrScaleFactor;
                                if (remainder >= 1)
                                {
                                    // Use the previous scan level and scale factor
                                    R2SignalLevel--;
                                }
                                else
                                {
                                    // Use the new scale factor and current scan level
                                    R2ScaleFactor = dScaleFactor;
                                }
                                bIsR2LevelFound = true;
                            }
                            else
                            {
                                R2ScaleFactor = dScaleFactor;
                            }
                        }
                        else if (dScaleFactor < R2ScaleFactor)
                        {
                            // Signal tailing off
                            // Use the previous scan level and scale factor
                            R2SignalLevel--;
                            bIsR2LevelFound = true;
                        }
                        if (bIsR2LevelFound)
                        {
                            if (_CurrentScanParam.IsSequentialScanning && _CurrentScanParam.Signals.Count > 1)
                            {
                                // Remove laser channel to avoid scanning the laser channel that already the optimal level
                                foreach (var signal in _CurrentScanParam.Signals)
                                {
                                    if (signal.LaserChannel == LaserChannels.ChannelB)
                                    {
                                        _CurrentScanParam.Signals.Remove(signal);
                                        break;
                                    }
                                }
                            }
                        }
                        LogMessage("EDR: Get scale factor (R2): " + R2ScaleFactor.ToString());
                    }
                    if ((L1ScaleFactor >= _CurrentScanParam.EdrScaleFactor || bIsL1LevelFound) &&
                        (R1ScaleFactor >= _CurrentScanParam.EdrScaleFactor || bIsR1LevelFound) &&
                        (R2ScaleFactor >= _CurrentScanParam.EdrScaleFactor || bIsR2LevelFound))
                    {
                        bIsThresholdFound = true;
                        LogMessage("EDR: scale factor >= threshold");
                    }

                    testImageIndex++;
                }
            }
            while (!bIsThresholdFound);

            int nColorChannel = 0;
            for (int i = 0; i < UpdatedSignalLevel.Count; i++)
            {
                nColorChannel = UpdatedSignalLevel[i].ColorChannel;
                if (UpdatedSignalLevel[i].LaserChannel == LaserChannels.ChannelA)
                {
                    // R1
                    UpdatedSignalLevel[i] = _CurrentScanParam.LaserR1SignalOptions[R1SignalLevel - 1];
                    UpdatedSignalLevel[i].ColorChannel = nColorChannel;
                    UpdatedSignalLevel[i].LaserChannel = LaserChannels.ChannelA;
                    UpdatedSignalLevel[i].SignalLevel = UpdatedSignalLevel[i].Position;
                }
                else if (UpdatedSignalLevel[i].LaserChannel == LaserChannels.ChannelB)
                {
                    // R2
                    UpdatedSignalLevel[i] = _CurrentScanParam.LaserR2SignalOptions[R2SignalLevel - 1];
                    UpdatedSignalLevel[i].ColorChannel = nColorChannel;
                    UpdatedSignalLevel[i].LaserChannel = LaserChannels.ChannelB;
                    UpdatedSignalLevel[i].SignalLevel = UpdatedSignalLevel[i].Position;
                }
                else if (UpdatedSignalLevel[i].LaserChannel == LaserChannels.ChannelC)
                {
                    // L1
                    UpdatedSignalLevel[i] = _CurrentScanParam.LaserL1SignalOptions[L1SignalLevel - 1];
                    UpdatedSignalLevel[i].ColorChannel = nColorChannel;
                    UpdatedSignalLevel[i].LaserChannel = LaserChannels.ChannelC;
                    UpdatedSignalLevel[i].SignalLevel = UpdatedSignalLevel[i].Position;
                }
            }

            _CurrentScanParam.Signals = new List<Signal>(UpdatedSignalLevel);

            // Restore 'normal' scan parameters
            _CurrentScanParam.Width = origWidth;
            _CurrentScanParam.Height = origHeight;
            _CurrentScanParam.Res = origResolution;
            _CurrentScanParam.AlignmentParam.Resolution = origResolution;
            _CurrentScanParam.Quality = origQuality;
            _CurrentScanParam.IsUnidirectionalScan = origIsUndirectional;

            //EDR calculation/test scans completed
            _IsEdrScanCalc = false;

            // Notify subscriber to update the intensity level
            if (EDRTestScanCompleted != null)
            {
                EDRTestScanCompleted(this);
                // Allow subscriber some time to process
                Thread.Sleep(1000);
            }

            LogMessage("EDR: Scanning the EDR reference image [2]: " + _CurrentScanParam.Res.ToString());

            if (_CurrentScanParam.IsSequentialScanning)
            {
                // Start sequential scan
                //
                SequentialScanProcess();
            }
            else
            {
                // Start normal scan process.
                //
                HorizontalScanProcess();
                StopScan();
                ProcessCompletedScanRegion();
            }

            if (_IsSaveDebuggingImages)
            {
                if (_IsChannelCSelected)    //L1
                {
                    string filePath = Path.Combine(_DBTIPath, "EDRSaturated_ChannelC.tif");
                    ImagingHelper.SaveFile(filePath, _ChannelCImage);
                }
                if (_IsChannelASelected)    //R1
                {
                    string filePath = Path.Combine(_DBTIPath, "EDRSaturated_ChannelA.tif");
                    ImagingHelper.SaveFile(filePath, _ChannelAImage);
                }
                if (_IsChannelBSelected)    //R2
                {
                    string filePath = Path.Combine(_DBTIPath, "EDRSaturated_ChannelB.tif");
                    ImagingHelper.SaveFile(filePath, _ChannelBImage);
                }
            }

            int dynamicBits = 0;
            L1ScaleFactor = 1;
            R1ScaleFactor = 1;
            R2ScaleFactor = 1;

            CommandStatus?.Invoke(this, "EDR scan downloading....");
            LogMessage("EDR: Processing EDR image: " + _CurrentScanParam.Res.ToString());

            bHasOffsetL1 = false;
            bHasOffsetR1 = false;
            bHasOffsetR2 = false;

            #region Get offset
            offsetL1 = new Point();
            offsetR1 = new Point();
            offsetR2 = new Point();
            if (_IsChannelCSelected && !bHasOffsetL1)    //L1
            {
                LogMessage("EDR: Get L scale factor offset...");
                bIsBlobFound = false;
                offsetL1 = GetOffSet(_ChannelCImageEDR1, ref bIsBlobFound);
                bHasOffsetL1 = true;
                if (bIsBlobFound)
                {
                    offsetR1.X = offsetL1.X;
                    offsetR1.Y = offsetL1.Y;
                    offsetR2.X = offsetL1.X;
                    offsetR2.Y = offsetL1.Y;
                    bHasOffsetR1 = true;
                    bHasOffsetR2 = true;
                }
                LogMessage("EDR: L scale factor offset: " + offsetL1.ToString());
            }
            if (_IsChannelASelected && !bHasOffsetR1)    //R1
            {
                LogMessage("EDR: Get R1 scale factor offset...");
                bIsBlobFound = false;
                offsetR1 = GetOffSet(_ChannelAImageEDR1, ref bIsBlobFound);
                bHasOffsetR1 = true;
                if (bIsBlobFound)
                {
                    offsetL1.X = offsetR1.X;
                    offsetL1.Y = offsetR1.Y;
                    offsetR2.X = offsetR1.X;
                    offsetR2.Y = offsetR1.Y;
                    bHasOffsetL1 = true;
                    bHasOffsetR2 = true;
                }
                LogMessage("EDR: R1 scale factor offset: " + offsetR1.ToString());
            }
            if (_IsChannelBSelected && !bHasOffsetR2)    //R2
            {
                LogMessage("EDR: Get R2 scale factor offset...");
                bIsBlobFound = false;
                offsetR2 = GetOffSet(_ChannelBImageEDR1, ref bIsBlobFound);
                bHasOffsetR2 = true;
                if (bIsBlobFound)
                {
                    offsetL1.X = offsetR2.X;
                    offsetL1.Y = offsetR2.Y;
                    offsetR1.X = offsetR2.X;
                    offsetR1.Y = offsetR2.Y;
                    bHasOffsetL1 = true;
                    bHasOffsetR1 = true;
                }
                LogMessage("EDR: R2 scale factor offset: " + offsetR2.ToString());
            }
            #endregion

            #region Get scale factor
            if (_IsChannelCSelected)    //L1
            {
                LogMessage("EDR: Get L scale factor...");
                L1ScaleFactor = Math.Round(GetScaleFactor(_ChannelCImageEDR1, _ChannelCImage, offsetL1), 3);
                LogMessage("EDR: L scale factor: " + L1ScaleFactor.ToString());
            }
            if (_IsChannelASelected)    //R1
            {
                LogMessage("EDR: Get R1 scale factor...");
                R1ScaleFactor = Math.Round(GetScaleFactor(_ChannelAImageEDR1, _ChannelAImage, offsetR1), 3);
                LogMessage("EDR: R1 scale factor:: " + R1ScaleFactor.ToString());
            }
            if (_IsChannelBSelected)    //R2
            {
                LogMessage("EDR: Get R2 scale factor...");
                R2ScaleFactor = Math.Round(GetScaleFactor(_ChannelBImageEDR1, _ChannelBImage, offsetR2), 3);
                LogMessage("EDR: R2 scale factor: " + R2ScaleFactor.ToString());
            }
            #endregion

            //if (_IsChannelCSelected)    //L1
            //{
            //    LogMessage("EDR: Get L scale factor offset...");
            //
            //    offsetL1 = GetOffSet(_ChannelCImageEDR1);
            //    LogMessage("EDR: L scale factor offset: " + offsetL1.ToString());
            //    LogMessage("EDR: Get L scale factor...");
            //    L1ScaleFactor = Math.Round(GetScaleFactor(_ChannelCImageEDR1, _ChannelCImage, offsetL1), 3);
            //    LogMessage("EDR: L scale factor: " + L1ScaleFactor.ToString());
            //}
            //if (_IsChannelASelected)    //R1
            //{
            //    LogMessage("EDR: Get R1 scale factor offset...");
            //    offsetR1 = GetOffSet(_ChannelAImageEDR1);
            //    LogMessage("EDR: R1 scale factor offset: " + offsetR1.ToString());
            //    LogMessage("EDR: Get R1 scale factor...");
            //    R1ScaleFactor = Math.Round(GetScaleFactor(_ChannelAImageEDR1, _ChannelAImage, offsetR1), 3);
            //    LogMessage("EDR: R1 scale factor:: " + R1ScaleFactor.ToString());
            //}
            //if (_IsChannelBSelected)    //R2
            //{
            //    LogMessage("EDR: Get R2 scale factor offset...");
            //    offsetR2 = GetOffSet(_ChannelBImageEDR1);
            //    LogMessage("EDR: R2 scale factor offset: " + offsetR2.ToString());
            //    LogMessage("EDR: Get R2 scale factor...");
            //    R2ScaleFactor = Math.Round(GetScaleFactor(_ChannelBImageEDR1, _ChannelBImage, offsetR2), 3);
            //    LogMessage("EDR: R2 scale factor: " + R2ScaleFactor.ToString());
            //}

            double scaleFactor = 1;
            scaleFactor = Math.Max(Math.Max(L1ScaleFactor, R1ScaleFactor), R2ScaleFactor);
            LogMessage("EDR: scale factor: " + scaleFactor.ToString());

            if (_IsChannelCSelected)    //L1
            {
                LogMessage("EDR: Processing L image...Convert and Compressing");
                ProcessEDRPixelAndCompress(ref _ChannelCImageEDR1, ref _ChannelCImage, ref dynamicBits, scaleFactor);
                if (_CurrentScanParam.Is5micronScan)
                {
                    LogMessage("EDR: Processing L1 to 5 micron...");
                    // Resize the 10um scan to 5um
                    _ChannelCImage = ImageProcessing.Resize(_ChannelCImage, new Size(2 * _ChannelCImage.PixelWidth, 2 * _ChannelCImage.PixelHeight));
                }
                if (_ChannelCImage.CanFreeze) { _ChannelCImage.Freeze(); }
            }
            if (_IsChannelASelected)    //R1
            {
                LogMessage("EDR: Processing R1 image...Convert and Compressing");
                ProcessEDRPixelAndCompress(ref _ChannelAImageEDR1, ref _ChannelAImage, ref dynamicBits, scaleFactor);
                if (_CurrentScanParam.Is5micronScan)
                {
                    LogMessage("EDR: Processing R1 to 5 micron...");
                    // Resize the 10um scan to 5um
                    _ChannelAImage = ImageProcessing.Resize(_ChannelAImage, new Size(2 * _ChannelAImage.PixelWidth, 2 * _ChannelAImage.PixelHeight));
                }
                if (_ChannelAImage.CanFreeze) { _ChannelAImage.Freeze(); }
            }
            if (_IsChannelBSelected)    //R2
            {
                LogMessage("EDR: Processing R2 image...Convert and Compressing");
                ProcessEDRPixelAndCompress(ref _ChannelBImageEDR1, ref _ChannelBImage, ref dynamicBits, scaleFactor);
                if (_CurrentScanParam.Is5micronScan)
                {
                    LogMessage("EDR: Processing R2 to 5 micron...");
                    // Resize the 10um scan to 5um
                    _ChannelBImage = ImageProcessing.Resize(_ChannelBImage, new Size(2 * _ChannelBImage.PixelWidth, 2 * _ChannelBImage.PixelHeight));
                }
                if (_ChannelBImage.CanFreeze) { _ChannelBImage.Freeze(); }
            }

            _ImageInfo.DynamicBit = dynamicBits;
        }*/

        #endregion

        #region EDR (Extended Dynamic Range) scan [NEW]

        private void EDRScanningProcess()
        {
            _IsEdrScanCalc = true;
            _CurrentScanParam.IsSmartScanning = true;
            _IsSmartScanCalc = true;

            LogMessage("EDR: Start SmartScan");
            CommandStatus?.Invoke(this, "EDR scan in progress....");

            // 1. Use smart scan to acquire an unsaturated scan image I1
            // Start smart scan process.
            SmartscanProcess();

            //int L1SignalLevel = 0;
            //int R1SignalLevel = 0;
            //int R2SignalLevel = 0;
            EdrSaturatedChannels = new List<LaserChannels>();
            foreach (var signal in _CurrentScanParam.Signals)
            {
                if (signal.LaserChannel == LaserChannels.ChannelC)
                {
                    //L1SignalLevel = signal.SignalLevel;
                    //Check for saturation, abort operation if saturated
                    if (_ChannelCImage != null)
                    {
                        int width = _ChannelCImage.PixelWidth;
                        int height = _ChannelCImage.PixelHeight;
                        _ChannelCImage = ImageProcessing.MedianFilter(_ChannelCImage);
                        int pixelMax = ImageProcessing.Max(_ChannelCImage, new Rect(0, 0, width, height));
                        if (pixelMax > _SaturationThreshold)
                        {
                            IsEdrSaturationAbort = true;
                            EdrSaturatedChannels.Add(LaserChannels.ChannelC);
                            Abort();
                        }
                    }
                }
                else if (signal.LaserChannel == LaserChannels.ChannelA)
                {
                    //R1SignalLevel = signal.SignalLevel;
                    //Check for saturation, abort operation if saturated
                    if (_ChannelAImage != null)
                    {
                        int width = _ChannelAImage.PixelWidth;
                        int height = _ChannelAImage.PixelHeight;
                        _ChannelAImage = ImageProcessing.MedianFilter(_ChannelAImage);
                        int pixelMax = ImageProcessing.Max(_ChannelAImage, new Rect(0, 0, width, height));
                        if (pixelMax > _SaturationThreshold)
                        {
                            IsEdrSaturationAbort = true;
                            EdrSaturatedChannels.Add(LaserChannels.ChannelA);
                            Abort();
                        }
                    }
                }
                else if (signal.LaserChannel == LaserChannels.ChannelB)
                {
                    //R2SignalLevel = signal.SignalLevel;
                    //Check for saturation, abort operation if saturated
                    if (_ChannelBImage != null)
                    {
                        int width = _ChannelBImage.PixelWidth;
                        int height = _ChannelBImage.PixelHeight;
                        _ChannelBImage = ImageProcessing.MedianFilter(_ChannelBImage);
                        int pixelMax = ImageProcessing.Max(_ChannelBImage, new Rect(0, 0, width, height));
                        if (pixelMax > _SaturationThreshold)
                        {
                            IsEdrSaturationAbort = true;
                            EdrSaturatedChannels.Add(LaserChannels.ChannelB);
                            Abort();
                        }
                    }
                }
            }

            LogMessage("EDR: Scanning the reference image [1]: " + _CurrentScanParam.Res.ToString());
            CommandStatus?.Invoke(this, "EDR scan in progress....");

            // Scan the first reference image:
            // Scan with SmartScan calculated signal level at the user's selected resolution.
            // NOTE: SmartscanProcess already restore the user selected scan parameters.
            //
            if (_CurrentScanParam.IsSequentialScanning)
            {
                // Start sequential scan
                //
                SequentialScanProcess();
                // SequentialScanProcess called Stop() and ProcessCompletedScanRegion()
            }
            else
            {
                // Start resolution normal scan (with smartscan signal level)
                HorizontalScanProcess();
                StopScan();
                ProcessCompletedScanRegion();
            }

            // EDR reference image 1: save scanned image(s) for later use.
            //
            if (_IsChannelCSelected)    //L1 laser channel
            {
                if (ChannelCImage != null)
                {
                    _ChannelCImageEDR1 = _ChannelCImage.Clone();
                    if (_IsSaveDebuggingImages)
                    {
                        string filePath = Path.Combine(_DBTIPath, "EDRSmart_ChannelC.tif");
                        ImagingHelper.SaveFile(filePath, _ChannelCImageEDR1);
                    }
                }
            }
            if (_IsChannelASelected)    //R1 laser channel
            {
                if (_ChannelAImage != null)
                {
                    _ChannelAImageEDR1 = _ChannelAImage.Clone();
                    if (_IsSaveDebuggingImages)
                    {
                        string filePath = Path.Combine(_DBTIPath, "EDRSmart_ChannelA.tif");
                        ImagingHelper.SaveFile(filePath, _ChannelAImageEDR1);
                    }
                }
            }
            if (_IsChannelBSelected)    //R2 laser channel
            {
                if (_ChannelBImage != null)
                {
                    _ChannelBImageEDR1 = _ChannelBImage.Clone();
                    if (_IsSaveDebuggingImages)
                    {
                        string filePath = Path.Combine(_DBTIPath, "EDRSmart_ChannelB.tif");
                        ImagingHelper.SaveFile(filePath, _ChannelBImageEDR1);
                    }
                }
            }

            Signal currentSignal = null;    // Current signal (test scan)
            List<Signal> signalOptionsList = new List<Signal>();
            Signal laserL1SignalLevel = null;
            Signal laserR1SignalLevel = null;
            Signal laserR2SignalLevel = null;
            int nCurrPixelMax = 0;          // Current scanned max pixel value
            int nCurrentSignalLevel = 0;    // Current scanned signal level
            int nLaserSignalLevel = 0;      // New signal level
            List<Signal> signals = new List<Signal>();
            WriteableBitmap scannedImage = null;
            //LaserChannels currentLaserChannel = LaserChannels.ChannelC;

            //int nFloor = _CurrentScanParam.SmartScanFloor;
            //int nCeiling = _CurrentScanParam.SmartScanCeiling;
            int nSignalLevels = _CurrentScanParam.SmartScanSignalLevels;

            int LaserL1Wavelength = 0;
            int LaserR1Wavelength = 0;
            int LaserR2Wavelength = 0;
            //bool L1signalLevelFound = true;
            //bool R1signalLevelFound = true;
            //bool R2signalLevelFound = true;

            for (int i = 0; i < _CurrentScanParam.Signals.Count; i++)
            {
                if (_CurrentScanParam.Signals[i].LaserChannel == LaserChannels.ChannelC)        //L1
                {
                    nLaserSignalLevel = _CurrentScanParam.Signals[i].Position;
                    laserL1SignalLevel = (Signal)_CurrentScanParam.LaserL1SignalOptions[nLaserSignalLevel - 1].Clone(); //1 index in config.xml
                    laserL1SignalLevel.ColorChannel = _CurrentScanParam.Signals[i].ColorChannel;
                    laserL1SignalLevel.LaserChannel = _CurrentScanParam.Signals[i].LaserChannel;
                    laserL1SignalLevel.LaserWavelength = _CurrentScanParam.Signals[i].LaserWavelength;
                    laserL1SignalLevel.SignalLevel = nLaserSignalLevel;
                    LaserL1Wavelength = _CurrentScanParam.Signals[i].LaserWavelength;
                    currentSignal = laserL1SignalLevel;
                    signalOptionsList = new List<Signal>(_CurrentScanParam.LaserL1SignalOptions);
                    //L1signalLevelFound = false;

                    scannedImage = _ChannelCImage;
                    //currentLaserChannel = LaserChannels.ChannelC;
                    nCurrentSignalLevel = laserL1SignalLevel.Position;
                }
                else if (_CurrentScanParam.Signals[i].LaserChannel == LaserChannels.ChannelA)   //R1
                {
                    nLaserSignalLevel = _CurrentScanParam.Signals[i].Position;
                    laserR1SignalLevel = (Signal)_CurrentScanParam.LaserR1SignalOptions[nLaserSignalLevel - 1].Clone(); //1 index in config.xml
                    laserR1SignalLevel.ColorChannel = _CurrentScanParam.Signals[i].ColorChannel;
                    laserR1SignalLevel.LaserChannel = _CurrentScanParam.Signals[i].LaserChannel;
                    laserR1SignalLevel.LaserWavelength = _CurrentScanParam.Signals[i].LaserWavelength;
                    laserR1SignalLevel.SignalLevel = nLaserSignalLevel;
                    LaserR1Wavelength = _CurrentScanParam.Signals[i].LaserWavelength;
                    currentSignal = laserR1SignalLevel;
                    signalOptionsList = new List<Signal>(_CurrentScanParam.LaserR1SignalOptions);
                    //R1signalLevelFound = false;

                    scannedImage = _ChannelAImage;
                    //currentLaserChannel = LaserChannels.ChannelA;
                    nCurrentSignalLevel = laserR1SignalLevel.Position;
                }
                else if (_CurrentScanParam.Signals[i].LaserChannel == LaserChannels.ChannelB)   //R2
                {
                    nLaserSignalLevel = _CurrentScanParam.Signals[i].Position;
                    laserR2SignalLevel = (Signal)_CurrentScanParam.LaserR2SignalOptions[nLaserSignalLevel - 1].Clone(); //1 index in config.xml
                    laserR2SignalLevel.ColorChannel = _CurrentScanParam.Signals[i].ColorChannel;
                    laserR2SignalLevel.LaserChannel = _CurrentScanParam.Signals[i].LaserChannel;
                    laserR2SignalLevel.LaserWavelength = _CurrentScanParam.Signals[i].LaserWavelength;
                    laserR2SignalLevel.SignalLevel = nLaserSignalLevel;
                    LaserR2Wavelength = _CurrentScanParam.Signals[i].LaserWavelength;
                    currentSignal = laserR2SignalLevel;
                    signalOptionsList = new List<Signal>(_CurrentScanParam.LaserR2SignalOptions);
                    //R2signalLevelFound = false;

                    scannedImage = _ChannelBImage;
                    //currentLaserChannel = LaserChannels.ChannelB;
                    nCurrentSignalLevel = laserR2SignalLevel.Position;
                }

                //int laserWavelength = _CurrentScanParam.Signals[i].LaserWavelength;

                #region Calculate signal level

                if (scannedImage != null)
                {
                    nCurrPixelMax = ImageProcessing.Max(scannedImage, new Rect(0, 0, scannedImage.PixelWidth, scannedImage.PixelHeight));
                }
                else
                {
                    throw new Exception("Scanned Image is NULL");
                }

                double dMultiplier = 0;
                int nCalcIntensity = 0;

                for (int n = nCurrentSignalLevel; n < nSignalLevels; n++)
                {
                    if (_CurrentScanParam.Signals[i].LaserWavelength == 488)
                    {
                        //Mn = Ln/Li * (Gn/Gi)^Alpha488
                        //Sn = Mn * Si
                        dMultiplier = ((double)signalOptionsList[n].LaserIntInmW / (double)currentSignal.LaserIntInmW) * Math.Pow((double)signalOptionsList[n].ApdGain / (double)currentSignal.ApdGain, _CurrentScanParam.SmartScanAlpha488);
                        nCalcIntensity = (int)(dMultiplier * (double)nCurrPixelMax);
                    }
                    else
                    {
                        //SOG
                        //Mn = Ln/Li*Gn/Gi*Pn/Pi
                        //Sn = Mn * Si
                        //dMultiplier = ((double)signalOptionsList[n].LaserIntInmW / (double)currentSignal.LaserIntInmW) * ((double)signalOptionsList[n].ApdGain / (double)currentSignal.ApdGain) * ((double)signalOptionsList[n].ApdPga / (double)currentSignal.ApdPga);
                        //SFL
                        //Mn = Ln/Li*Gn/Gi*2^(Pn-Pi)
                        //Sn = Mn * Si
                        dMultiplier = ((double)signalOptionsList[n].LaserIntInmW / (double)currentSignal.LaserIntInmW) * ((double)signalOptionsList[n].ApdGain / (double)currentSignal.ApdGain) * Math.Pow(2, signalOptionsList[n].ApdPga - currentSignal.ApdPga);
                        nCalcIntensity = (int)(dMultiplier * (double)nCurrPixelMax);
                    }

                    if (nCalcIntensity < 10 * nCurrPixelMax)
                    {
                        nLaserSignalLevel = signalOptionsList[n].Position;
                        continue;
                    }
                    else if (nCalcIntensity > 10 * nCurrPixelMax)
                    {
                        n--;
                        nLaserSignalLevel = signalOptionsList[n].Position;

                        if (_CurrentScanParam.Signals[i].LaserWavelength == 488)
                        {
                            //Mn = Ln/Li * (Gn/Gi)^Alpha488
                            //Sn = Mn * Si
                            dMultiplier = ((double)signalOptionsList[n].LaserIntInmW / (double)currentSignal.LaserIntInmW) * Math.Pow((double)signalOptionsList[n].ApdGain / (double)currentSignal.ApdGain, _CurrentScanParam.SmartScanAlpha488);
                            nCalcIntensity = (int)(dMultiplier * (double)nCurrPixelMax);
                        }
                        else
                        {
                            //SOG
                            //Mn = Ln/Li*Gn/Gi*Pn/Pi
                            //Sn = Mn * Si
                            //dMultiplier = ((double)signalOptionsList[n].LaserIntInmW / (double)currentSignal.LaserIntInmW) * ((double)signalOptionsList[n].ApdGain / (double)currentSignal.ApdGain) * ((double)signalOptionsList[n].ApdPga / (double)currentSignal.ApdPga);
                            //SFL
                            //Mn = Ln/Li*Gn/Gi*2^(Pn-Pi)
                            //Sn = Mn * Si
                            dMultiplier = ((double)signalOptionsList[n].LaserIntInmW / (double)currentSignal.LaserIntInmW) * ((double)signalOptionsList[n].ApdGain / (double)currentSignal.ApdGain) * Math.Pow(2, signalOptionsList[n].ApdPga - currentSignal.ApdPga);
                            nCalcIntensity = (int)(dMultiplier * (double)nCurrPixelMax);
                        }
                        LogMessage(string.Format("Calculated intensity: {0}. Stepping back to level: {1}", nCalcIntensity, nLaserSignalLevel));
                        //if (currentLaserChannel == LaserChannels.ChannelA) { R1signalLevelFound = true; }
                        //if (currentLaserChannel == LaserChannels.ChannelB) { R2signalLevelFound = true; }
                        //if (currentLaserChannel == LaserChannels.ChannelC) { L1signalLevelFound = true; }
                        break;
                    }
                    else
                    {
                        nLaserSignalLevel = signalOptionsList[n].Position;
                        break;
                    }
                }

                if (_CurrentScanParam.Signals[i].LaserChannel == LaserChannels.ChannelC)        //L1
                {
                    laserL1SignalLevel = (Signal)_CurrentScanParam.LaserL1SignalOptions[nLaserSignalLevel - 1].Clone();
                    laserL1SignalLevel.ColorChannel = _CurrentScanParam.Signals[i].ColorChannel;
                    laserL1SignalLevel.LaserChannel = LaserChannels.ChannelC;
                    laserL1SignalLevel.LaserWavelength = LaserL1Wavelength;     //is this needed?
                    laserL1SignalLevel.SignalLevel = nLaserSignalLevel;
                    int index = UpdatedSignalLevel.FindIndex(x => x.LaserChannel == LaserChannels.ChannelC);
                    UpdatedSignalLevel[index] = laserL1SignalLevel;
                    // NOTE: removing sequential scanning check will behave like
                    // Smart + Sequential (don't scan the scan channel already found the optimal level)
                    //if (_CurrentScanParam.IsSequentialScanning)
                    //{
                    //    if (!L1signalLevelFound)
                    //    {
                    //        signals.Add(laserL1SignalLevel);
                    //    }
                    //}
                    //else
                    //{
                    //    signals.Add(laserL1SignalLevel);
                    //}
                }
                else if (_CurrentScanParam.Signals[i].LaserChannel == LaserChannels.ChannelA)   //R1
                {
                    laserR1SignalLevel = (Signal)_CurrentScanParam.LaserR1SignalOptions[nLaserSignalLevel - 1].Clone();
                    laserR1SignalLevel.ColorChannel = _CurrentScanParam.Signals[i].ColorChannel;
                    laserR1SignalLevel.LaserChannel = LaserChannels.ChannelA;
                    laserR1SignalLevel.LaserWavelength = LaserR1Wavelength;     //is this needed?
                    laserR1SignalLevel.SignalLevel = nLaserSignalLevel;
                    int index = UpdatedSignalLevel.FindIndex(x => x.LaserChannel == LaserChannels.ChannelA);
                    UpdatedSignalLevel[index] = laserR1SignalLevel;
                    // NOTE: removing sequential scanning check will behave like
                    // Smart + Sequential (don't scan the scan channel already found the optimal level)
                    //if (_CurrentScanParam.IsSequentialScanning)
                    //{
                    //    if (!R1signalLevelFound)
                    //    {
                    //        signals.Add(laserR1SignalLevel);
                    //    }
                    //}
                    //else
                    //{
                    //    signals.Add(laserR1SignalLevel);
                    //}
                }
                else if (_CurrentScanParam.Signals[i].LaserChannel == LaserChannels.ChannelB)   //R2
                {
                    laserR2SignalLevel = (Signal)_CurrentScanParam.LaserR2SignalOptions[nLaserSignalLevel - 1].Clone();
                    laserR2SignalLevel.ColorChannel = _CurrentScanParam.Signals[i].ColorChannel;
                    laserR2SignalLevel.LaserChannel = LaserChannels.ChannelB;
                    laserR2SignalLevel.LaserWavelength = LaserR2Wavelength;     //is this needed?
                    laserR2SignalLevel.SignalLevel = nLaserSignalLevel;
                    int index = UpdatedSignalLevel.FindIndex(x => x.LaserChannel == LaserChannels.ChannelB);
                    UpdatedSignalLevel[index] = laserR2SignalLevel;
                    // NOTE: removing sequential scanning check will behave like
                    // Smart + Sequential (don't scan the scan channel already found the optimal level)
                    //if (_CurrentScanParam.IsSequentialScanning)
                    //{
                    //    if (!R2signalLevelFound)
                    //    {
                    //        signals.Add(laserR2SignalLevel);
                    //    }
                    //}
                    //else
                    //{
                    //    signals.Add(laserR2SignalLevel);
                    //}
                }

                #endregion

            }

            //EDR calculation/test scans completed: allow scan time count down
            _IsEdrScanCalc = false;

            LogMessage("EDR: Scanning the reference image [2]: " + _CurrentScanParam.Res.ToString());
            CommandStatus?.Invoke(this, "EDR scan in progress....");

            //if (signals.Count > 0)
            //{
            //    // Calculate signal levels
            //    _CurrentScanParam.Signals = new List<Signal>(signals);
            //}
            _CurrentScanParam.Signals = new List<Signal>(UpdatedSignalLevel);

            // Notify subscriber to update the intensity level
            if (EDRTestScanUpdating != null)
            {
                EDRTestScanUpdating(this);
                // Allow subscriber some time to process
                Thread.Sleep(1000);
            }

            // Notify subscriber to update the intensity level
            //if (EDRTestScanCompleted != null)
            //{
            //    EDRTestScanCompleted(this);
            //    // Allow subscriber some time to process
            //    Thread.Sleep(1000);
            //}

            // Scan the first reference image:
            // Scan with SmartScan calculated signal level at the user's selected resolution.
            // NOTE: SmartscanProcess already restore the user selected scan parameters.
            //
            if (_CurrentScanParam.IsSequentialScanning)
            {
                // Start sequential scan
                //
                SequentialScanProcess();
                // SequentialScanProcess called Stop() and ProcessCompletedScanRegion()
            }
            else
            {
                // Start resolution normal scan (with smartscan signal level)
                HorizontalScanProcess();
                StopScan();
                ProcessCompletedScanRegion();
            }

            // EDR reference image 1: save scanned image(s) for later use.
            //
            if (_IsChannelCSelected)    //L1 laser channel
            {
                if (ChannelCImage != null)
                {
                    if (_IsSaveDebuggingImages)
                    {
                        string filePath = Path.Combine(_DBTIPath, "EDRSaturated_ChannelC.tif");
                        ImagingHelper.SaveFile(filePath, _ChannelCImage);
                    }
                }
            }
            if (_IsChannelASelected)    //R1 laser channel
            {
                if (_ChannelAImage != null)
                {
                    if (_IsSaveDebuggingImages)
                    {
                        string filePath = Path.Combine(_DBTIPath, "EDRSaturated_ChannelA.tif");
                        ImagingHelper.SaveFile(filePath, _ChannelAImage);
                    }
                }
            }
            if (_IsChannelBSelected)    //R2 laser channel
            {
                if (_ChannelBImage != null)
                {
                    if (_IsSaveDebuggingImages)
                    {
                        string filePath = Path.Combine(_DBTIPath, "EDRSaturated_ChannelB.tif");
                        ImagingHelper.SaveFile(filePath, _ChannelBImage);
                    }
                }
            }

            CommandStatus?.Invoke(this, "EDR scan downloading....");
            LogMessage("EDR: Processing EDR image: " + _CurrentScanParam.Res.ToString());

            #region Get scale factor

            double L1ScaleFactor = 0;
            double R1ScaleFactor = 0;
            double R2ScaleFactor = 0;
            int dynamicBit = 0;
            int actualEdrBitDepth = 0;
            //bool bIsL1ScaleFactorError = false;
            //bool bIsR1ScaleFactorError = false;
            //bool bIsR2ScaleFactorError = false;

            if (_IsChannelCSelected)    //L1
            {
                LogMessage("EDR: Get L scale factor...");
                //L1ScaleFactor = Math.Round(GetScaleFactor(_ChannelCImageEDR1, _ChannelCImage, offsetL1), 3);
                L1ScaleFactor = GetScaleFactor(_ChannelCImageEDR1, _ChannelCImage);
                LogMessage("EDR: L scale factor: " + L1ScaleFactor.ToString());
                if (L1ScaleFactor <= 1)
                {
                    L1ScaleFactor = 1;  // No scaling
                    //bIsL1ScaleFactorError = true;
                }
            }
            if (_IsChannelASelected)    //R1
            {
                LogMessage("EDR: Get R1 scale factor...");
                R1ScaleFactor = GetScaleFactor(_ChannelAImageEDR1, _ChannelAImage);
                LogMessage("EDR: R1 scale factor:: " + R1ScaleFactor.ToString());
                if (R1ScaleFactor <= 1)
                {
                    R1ScaleFactor = 1;  // No scaling
                    //bIsR1ScaleFactorError = true;
                }
            }
            if (_IsChannelBSelected)    //R2
            {
                LogMessage("EDR: Get R2 scale factor...");
                R2ScaleFactor = GetScaleFactor(_ChannelBImageEDR1, _ChannelBImage);
                LogMessage("EDR: R2 scale factor: " + R2ScaleFactor.ToString());
                if (R2ScaleFactor <= 1)
                {
                    R2ScaleFactor = 1;  // No scaling
                    //bIsR2ScaleFactorError = true;
                }
            }
            #endregion

            double compressScaleFactor = Math.Max(Math.Max(L1ScaleFactor, R1ScaleFactor), R2ScaleFactor);
            LogMessage("EDR: scale factor: " + compressScaleFactor.ToString());

            if (_IsChannelCSelected)    //L1
            {
                if (compressScaleFactor > 1)
                {
                    LogMessage("EDR: Processing L image...Convert and Compressing");
                    ProcessEDRPixelAndCompress(ref _ChannelCImageEDR1, ref _ChannelCImage, ref dynamicBit, ref actualEdrBitDepth, L1ScaleFactor, compressScaleFactor);
                }
                else
                {
                    _ChannelCImage = _ChannelCImageEDR1;
                }
                if (_CurrentScanParam.Is5micronScan)
                {
                    LogMessage("EDR: Processing L1 to 5 micron...");
                    // Resize the 10um scan to 5um
                    _ChannelCImage = ImageProcessing.Resize(_ChannelCImage, new Size(2 * _ChannelCImage.PixelWidth, 2 * _ChannelCImage.PixelHeight));
                }
                if (_ChannelCImage.CanFreeze) { _ChannelCImage.Freeze(); }
            }
            if (_IsChannelASelected)    //R1
            {
                if (compressScaleFactor > 1)
                {
                    LogMessage("EDR: Processing R1 image...Convert and Compressing");
                    ProcessEDRPixelAndCompress(ref _ChannelAImageEDR1, ref _ChannelAImage, ref dynamicBit, ref actualEdrBitDepth, R1ScaleFactor, compressScaleFactor);
                }
                else
                {
                    _ChannelAImage = _ChannelAImageEDR1;
                }
                if (_CurrentScanParam.Is5micronScan)
                {
                    LogMessage("EDR: Processing R1 to 5 micron...");
                    // Resize the 10um scan to 5um
                    _ChannelAImage = ImageProcessing.Resize(_ChannelAImage, new Size(2 * _ChannelAImage.PixelWidth, 2 * _ChannelAImage.PixelHeight));
                }
                if (_ChannelAImage.CanFreeze) { _ChannelAImage.Freeze(); }
            }
            if (_IsChannelBSelected)    //R2
            {
                if (compressScaleFactor > 1)
                {
                    LogMessage("EDR: Processing R2 image...Convert and Compressing");
                    ProcessEDRPixelAndCompress(ref _ChannelBImageEDR1, ref _ChannelBImage, ref dynamicBit, ref actualEdrBitDepth, R2ScaleFactor, compressScaleFactor);
                }
                else
                {
                    _ChannelBImage = _ChannelBImageEDR1;
                }
                if (_CurrentScanParam.Is5micronScan)
                {
                    LogMessage("EDR: Processing R2 to 5 micron...");
                    // Resize the 10um scan to 5um
                    _ChannelBImage = ImageProcessing.Resize(_ChannelBImage, new Size(2 * _ChannelBImage.PixelWidth, 2 * _ChannelBImage.PixelHeight));
                }
                if (_ChannelBImage.CanFreeze) { _ChannelBImage.Freeze(); }
            }

            _ImageInfo.DynamicBit = dynamicBit;
            _ImageInfo.EdrBitDepth = actualEdrBitDepth;

            //if (bIsL1ScaleFactorError || bIsR1ScaleFactorError || bIsR2ScaleFactorError)
            //{
            //    string portL1 = string.Empty;
            //    string portR1 = string.Empty;
            //    string portR2 = string.Empty;
            //    if (bIsL1ScaleFactorError) { portL1 = "Port #1"; }
            //    if (bIsR1ScaleFactorError) { portR1 = "Port #2"; }
            //    if (bIsR2ScaleFactorError) { portR2 = "Port #3"; }
            //    string message = string.Format("{0} {1} {2}", portL1, portR1, portR2);
            //    throw new Exception("Unable to find the EDR scale factor for the following laser ports: " + message);
            //}
        }

        #endregion

        #region ProcessCompletedScanRegion

        private void ProcessCompletedScanRegion()
        {
            try
            {
                if (!_IsSmartScanCalc && !_IsEdrScanCalc)
                {
                    CommandStatus?.Invoke(this, "Downloading....");
                }

                if (_IsPhosphorImaging)
                {
                    _ImageInfo.CaptureType = "Phosphor Imaging";
                }
                else
                {
                    _ImageInfo.CaptureType = "Fluorescence";
                }
                _ImageInfo.CaptureType = "Scanner";
                _ImageInfo.IsScannedImage = true;
                _ImageInfo.ScanResolution = _CurrentScanParam.Is5micronScan ? 5 : _CurrentScanParam.Res;
                // 2023/02/14: 'Quality' in ImageInfo refers to unidirectional (or 2-line average) ON/OFF (NOT scan speed)
                _ImageInfo.ScanQuality = _CurrentScanParam.Quality.ToString();
                _ImageInfo.ScanX0 = _CurrentScanParam.ScanX0;
                _ImageInfo.ScanY0 = _CurrentScanParam.ScanY0;
                _ImageInfo.DeltaX = (int)((double)_CurrentScanParam.ScanDeltaX / (double)_CurrentScanParam.XMotorSubdivision);
                _ImageInfo.DeltaY = (int)((double)_CurrentScanParam.ScanDeltaY / (double)_CurrentScanParam.YMotorSubdivision);
                _ImageInfo.IsSmartscanDespeckled = IsDespeckleSmartScan; 
                if (!_CurrentScanParam.IsZScanning)
                {
                    _ImageInfo.ScanZ0 = Math.Round((double)_CurrentScanParam.ScanZ0 / (double)_CurrentScanParam.ZMotorSubdivision, 3);
                }
                foreach (Signal currSignal in _CurrentScanParam.Signals)
                {
                    if (currSignal.ColorChannel == (int)ImageChannelType.Red)
                    {
                        _ImageInfo.RedChannel.ApdGain = currSignal.ApdGain;
                        _ImageInfo.RedChannel.ApdPga = currSignal.ApdPga;
                        _ImageInfo.RedChannel.LaserIntensity = currSignal.LaserIntensity;
                        _ImageInfo.RedChannel.LaserWavelength = currSignal.LaserWavelength.ToString();
                        _ImageInfo.RedChannel.LaserIntensityLevel = currSignal.Position;
                        _ImageInfo.RedChannel.LaserChannel = _LaserChannelDict[currSignal.LaserChannel];
                        _ImageInfo.RedChannel.ScanZ0 = _ImageInfo.ScanZ0;
                    }
                    else if (currSignal.ColorChannel == (int)ImageChannelType.Green)
                    {
                        _ImageInfo.GreenChannel.ApdGain = currSignal.ApdGain;
                        _ImageInfo.GreenChannel.ApdPga = currSignal.ApdPga;
                        _ImageInfo.GreenChannel.LaserIntensity = currSignal.LaserIntensity;
                        _ImageInfo.GreenChannel.LaserWavelength = currSignal.LaserWavelength.ToString();
                        _ImageInfo.GreenChannel.LaserIntensityLevel = currSignal.Position;
                        _ImageInfo.GreenChannel.LaserChannel = _LaserChannelDict[currSignal.LaserChannel];
                        _ImageInfo.GreenChannel.ScanZ0 = _ImageInfo.ScanZ0;
                    }
                    else if (currSignal.ColorChannel == (int)ImageChannelType.Blue)
                    {
                        _ImageInfo.BlueChannel.ApdGain = currSignal.ApdGain;
                        _ImageInfo.BlueChannel.ApdPga = currSignal.ApdPga;
                        _ImageInfo.BlueChannel.LaserIntensity = currSignal.LaserIntensity;
                        _ImageInfo.BlueChannel.LaserWavelength = currSignal.LaserWavelength.ToString();
                        _ImageInfo.BlueChannel.LaserIntensityLevel = currSignal.Position;
                        _ImageInfo.BlueChannel.LaserChannel = _LaserChannelDict[currSignal.LaserChannel];
                        _ImageInfo.BlueChannel.ScanZ0 = _ImageInfo.ScanZ0;
                    }
                    else if (currSignal.ColorChannel == (int)ImageChannelType.Gray)
                    {
                        if (_CurrentScanParam.Signals.Count > 1)
                        {
                            _ImageInfo.GrayChannel.ApdGain = currSignal.ApdGain;
                            _ImageInfo.GrayChannel.ApdPga = currSignal.ApdPga;
                            _ImageInfo.GrayChannel.LaserIntensity = currSignal.LaserIntensity;
                            _ImageInfo.GrayChannel.LaserWavelength = currSignal.LaserWavelength.ToString();
                            _ImageInfo.GrayChannel.LaserIntensityLevel = currSignal.SignalLevel;
                            _ImageInfo.GrayChannel.LaserChannel = _LaserChannelDict[currSignal.LaserChannel];
                            _ImageInfo.GrayChannel.ScanZ0 = _ImageInfo.ScanZ0;
                        }
                        else
                        {
                            _ImageInfo.MixChannel.ApdGain = currSignal.ApdGain;
                            _ImageInfo.MixChannel.ApdPga = currSignal.ApdPga;
                            _ImageInfo.MixChannel.LaserIntensity = currSignal.LaserIntensity;
                            _ImageInfo.MixChannel.LaserWavelength = currSignal.LaserWavelength.ToString();
                            _ImageInfo.MixChannel.LaserIntensityLevel = currSignal.SignalLevel;
                            _ImageInfo.MixChannel.LaserChannel = _LaserChannelDict[currSignal.LaserChannel];
                            _ImageInfo.MixChannel.ScanZ0 = _ImageInfo.ScanZ0;
                        }
                    }
                }

                if (!_IsScanAborted || IsSaveDataOnScanAbort)
                {
                    if (ScanType == ScanTypes.Horizontal)
                    {
                        ImageAlignParam alignParam = _CurrentScanParam.AlignmentParam;

                        #region Process L1 channel

                        if (_IsChannelCSelected)    //L1
                        {
                            if (_ChannelCImage != null)
                            {
                                try
                                {
                                    // Unaligned image buffer
                                    //_ChannelCImage = new WriteableBitmap(_CurrentScanParam.Width, _CurrentScanParam.Height, _ActualImageDPI, _ActualImageDPI, System.Windows.Media.PixelFormats.Gray16, null);
                                    //ImageProcessing.FrameToBitmap(_APDChannelC, ref _ChannelCImage);
                                    if (IsKeepRawImages)
                                    {
                                        if (_ChannelCImage != null)
                                        {
                                            ChannelCRawImage = _ChannelCImage.Clone();
                                            if (ChannelCRawImage.CanFreeze)
                                            {
                                                ChannelCRawImage.Freeze();
                                            }
                                        }
                                    }
                                    // Aligned image buffer
                                    //double offsetWidth = (int)(alignParam.OpticalL_R1Distance * 1000 / alignParam.Resolution);
                                    //int alignedWidth = _CurrentScanParam.Width - (int)offsetWidth;
                                    //var alignedImage = new WriteableBitmap(alignedWidth, _CurrentScanParam.Height, _ActualImageDPI, _ActualImageDPI, System.Windows.Media.PixelFormats.Gray16, null);
                                    //alignParam.LaserChannel = LaserChannels.ChannelC;
                                    //ImagingHelper.AlignImage(ref _ChannelCImage, ref alignedImage, alignParam);
                                    //_ChannelCImage = alignedImage;
                                    alignParam.LaserChannel = LaserChannels.ChannelC;
                                    //ImagingHelper.AlignImage(ref _ChannelCImage, alignParam);
                                    _ChannelCImage = ImagingHelper.SFLImageAlign(_ChannelCImage, alignParam);

                                    if (_IsPhosphorImaging && !_CurrentScanParam.IsUnidirectionalScan)
                                    {
                                        _ChannelCImage = OpenCvSharpHelper.Filter2D(_ChannelCImage);
                                    }

                                    //unsafe
                                    //{
                                    //    // Aligned image buffer
                                    //    double offsetWidth = (int)(alignParam.OpticalL_R1Distance * 1000 / alignParam.Resolution);
                                    //    int alignedWidth = _CurrentScanParam.Width - (int)offsetWidth;
                                    //    var alignedImage = new WriteableBitmap(alignedWidth, _CurrentScanParam.Height, _ActualImageDPI, _ActualImageDPI, System.Windows.Media.PixelFormats.Gray16, null);
                                    //
                                    //    int srcwidth = _ChannelCImage.PixelWidth;
                                    //    int srcheight = _ChannelCImage.PixelHeight;
                                    //    int srcstride = _ChannelCImage.BackBufferStride;
                                    //    int dstwidth = alignedImage.PixelWidth;
                                    //    int dstheight = alignedImage.PixelHeight;
                                    //    int dststride = alignedImage.BackBufferStride;
                                    //    byte* psrcimg = (byte*)_ChannelCImage.BackBuffer.ToPointer();
                                    //    byte* pdstimg = (byte*)alignedImage.BackBuffer.ToPointer();
                                    //
                                    //    //var alignParam = _ImageScanCommand.CurrentScanParam.AlignmentParam;
                                    //    alignParam.LaserChannel = LaserChannels.ChannelA;
                                    //    ImagingHelper.AlignImage(psrcimg, srcwidth, srcheight, srcstride, pdstimg, dstwidth, dstheight, dststride, alignParam, null, null);
                                    //    _ChannelCImage = alignedImage;
                                    //}

                                    //Default image flipping (vertical)
                                    _ChannelCImage = ImageProcessing.WpfFlip(_ChannelCImage, ImageProcessing.FlipAxis.Vertical);

                                    //User specified flip operation in Settings
                                    if (_FlipAxis == ImageProcessing.FlipAxis.Horizontal)
                                    {
                                        _ChannelCImage = ImageProcessing.WpfFlip(_ChannelCImage, ImageProcessing.FlipAxis.Horizontal);
                                    }
                                    else if (_FlipAxis == ImageProcessing.FlipAxis.Vertical)
                                    {
                                        _ChannelCImage = ImageProcessing.WpfFlip(_ChannelCImage, ImageProcessing.FlipAxis.Vertical);
                                    }

                                    if (!_CurrentScanParam.IsEdrScanning)
                                    {
                                        if (_CurrentScanParam.Is5micronScan)
                                        {
                                            // Resize the 10um scan to 5um
                                            _ChannelCImage = ImageProcessing.Resize(_ChannelCImage, new Size(2 * _ChannelCImage.PixelWidth, 2 * _ChannelCImage.PixelHeight));
                                        }
                                        if (_ChannelCImage.CanFreeze) { _ChannelCImage.Freeze(); }
                                    }
                                }
                                catch
                                {
                                    throw;
                                }
                            }
                        }
                        #endregion

                        #region Process R1 channel

                        if (_IsChannelASelected)    //R1
                        {
                            if (_ChannelAImage != null)
                            {
                                try
                                {
                                    // Unaligned image buffer
                                    //_ChannelAImage = new WriteableBitmap(_CurrentScanParam.Width, _CurrentScanParam.Height, _ActualImageDPI, _ActualImageDPI, System.Windows.Media.PixelFormats.Gray16, null);
                                    //ImageProcessing.FrameToBitmap(_APDChannelA, ref _ChannelAImage);
                                    if (IsKeepRawImages)
                                    {
                                        if (_ChannelAImage != null)
                                        {
                                            ChannelARawImage = _ChannelAImage.Clone();
                                            if (ChannelARawImage.CanFreeze)
                                            {
                                                ChannelARawImage.Freeze();
                                            }
                                        }
                                    }
                                    // Aligned image buffer
                                    //double offsetWidth = (int)(alignParam.OpticalL_R1Distance * 1000 / alignParam.Resolution);
                                    //int alignedWidth = _CurrentScanParam.Width - (int)offsetWidth;
                                    //var alignedImage = new WriteableBitmap(alignedWidth, _CurrentScanParam.Height, _ActualImageDPI, _ActualImageDPI, System.Windows.Media.PixelFormats.Gray16, null);
                                    //alignParam.LaserChannel = LaserChannels.ChannelA;
                                    //ImagingHelper.AlignImage(ref _ChannelAImage, ref alignedImage, alignParam);
                                    //_ChannelAImage = alignedImage;
                                    alignParam.LaserChannel = LaserChannels.ChannelA;
                                    //ImagingHelper.AlignImage(ref _ChannelAImage, alignParam);
                                    _ChannelAImage = ImagingHelper.SFLImageAlign(_ChannelAImage, alignParam);

                                    /*unsafe
                                    {
                                        // Aligned image buffer
                                        double offsetWidth = (int)(alignParam.OpticalL_R1Distance * 1000 / alignParam.Resolution);
                                        int alignedWidth = _CurrentScanParam.Width - (int)offsetWidth;
                                        var alignedImage = new WriteableBitmap(alignedWidth, _CurrentScanParam.Height, _ActualImageDPI, _ActualImageDPI, System.Windows.Media.PixelFormats.Gray16, null);

                                        int srcwidth = _ChannelAImage.PixelWidth;
                                        int srcheight = _ChannelAImage.PixelHeight;
                                        int srcstride = _ChannelAImage.BackBufferStride;
                                        int dstwidth = alignedImage.PixelWidth;
                                        int dstheight = alignedImage.PixelHeight;
                                        int dststride = alignedImage.BackBufferStride;
                                        byte* psrcimg = (byte*)_ChannelAImage.BackBuffer.ToPointer();
                                        byte* pdstimg = (byte*)alignedImage.BackBuffer.ToPointer();

                                        //var alignParam = _ImageScanCommand.CurrentScanParam.AlignmentParam;
                                        alignParam.LaserChannel = LaserChannels.ChannelA;
                                        ImagingHelper.AlignImage(psrcimg, srcwidth, srcheight, srcstride, pdstimg, dstwidth, dstheight, dststride, alignParam, null, null);
                                        _ChannelAImage = alignedImage;
                                    }*/

                                    //Default image flipping (vertical)
                                    _ChannelAImage = ImageProcessing.WpfFlip(_ChannelAImage, ImageProcessing.FlipAxis.Vertical);

                                    //User specified flip operation in Settings
                                    if (_FlipAxis == ImageProcessing.FlipAxis.Horizontal)
                                    {
                                        _ChannelAImage = ImageProcessing.WpfFlip(_ChannelAImage, ImageProcessing.FlipAxis.Horizontal);
                                    }
                                    else if (_FlipAxis == ImageProcessing.FlipAxis.Vertical)
                                    {
                                        _ChannelAImage = ImageProcessing.WpfFlip(_ChannelAImage, ImageProcessing.FlipAxis.Vertical);
                                    }

                                    if (!_CurrentScanParam.IsEdrScanning)
                                    {
                                        if (_CurrentScanParam.Is5micronScan)
                                        {
                                            // Resize the 10um scan to 5um
                                            _ChannelAImage = ImageProcessing.Resize(_ChannelAImage, new Size(2 * _ChannelAImage.PixelWidth, 2 * _ChannelAImage.PixelHeight));
                                        }
                                        if (_ChannelAImage.CanFreeze) { _ChannelAImage.Freeze(); }
                                    }
                                }
                                catch
                                {
                                    throw;
                                }
                            }
                        }

                        #endregion

                        #region Process R2 channel 

                        if (_IsChannelBSelected)    //R2
                        {
                            if (_ChannelBImage != null)
                            {
                                try
                                {
                                    // Unaligned image buffer
                                    //_ChannelBImage = new WriteableBitmap(_CurrentScanParam.Width, _CurrentScanParam.Height, _ActualImageDPI, _ActualImageDPI, System.Windows.Media.PixelFormats.Gray16, null);
                                    //ImageProcessing.FrameToBitmap(_APDChannelB, ref _ChannelBImage);
                                    if (IsKeepRawImages)
                                    {
                                        if (_ChannelBImage != null)
                                        {
                                            ChannelBRawImage = _ChannelBImage.Clone();
                                            if (ChannelBRawImage.CanFreeze)
                                            {
                                                ChannelBRawImage.Freeze();
                                            }
                                        }
                                    }
                                    // Aligned image buffer
                                    //double offsetWidth = (int)(alignParam.OpticalL_R1Distance * 1000 / alignParam.Resolution);
                                    //int alignedWidth = _CurrentScanParam.Width - (int)offsetWidth;
                                    //var alignedImage = new WriteableBitmap(alignedWidth, _CurrentScanParam.Height, _ActualImageDPI, _ActualImageDPI, System.Windows.Media.PixelFormats.Gray16, null);
                                    //alignParam.LaserChannel = LaserChannels.ChannelB;
                                    //ImagingHelper.AlignImage(ref _ChannelBImage, ref alignedImage, alignParam);
                                    //_ChannelBImage = alignedImage;
                                    alignParam.LaserChannel = LaserChannels.ChannelB;
                                    //ImagingHelper.AlignImage(ref _ChannelBImage, alignParam);
                                    _ChannelBImage = ImagingHelper.SFLImageAlign(_ChannelBImage, alignParam);

                                    //unsafe
                                    //{
                                    //    // Aligned image buffer
                                    //    double offsetWidth = (int)(alignParam.OpticalL_R1Distance * 1000 / alignParam.Resolution);
                                    //    int alignedWidth = _CurrentScanParam.Width - (int)offsetWidth;
                                    //    var alignedImage = new WriteableBitmap(alignedWidth, _CurrentScanParam.Height, _ActualImageDPI, _ActualImageDPI, System.Windows.Media.PixelFormats.Gray16, null);
                                    //
                                    //    int srcwidth = _ChannelBImage.PixelWidth;
                                    //    int srcheight = _ChannelBImage.PixelHeight;
                                    //    int srcstride = _ChannelBImage.BackBufferStride;
                                    //    int dstwidth = alignedImage.PixelWidth;
                                    //    int dstheight = alignedImage.PixelHeight;
                                    //    int dststride = alignedImage.BackBufferStride;
                                    //    byte* psrcimg = (byte*)_ChannelBImage.BackBuffer.ToPointer();
                                    //    byte* pdstimg = (byte*)alignedImage.BackBuffer.ToPointer();
                                    //
                                    //    //var alignParam = _ImageScanCommand.CurrentScanParam.AlignmentParam;
                                    //    alignParam.LaserChannel = LaserChannels.ChannelB;
                                    //    ImagingHelper.AlignImage(psrcimg, srcwidth, srcheight, srcstride, pdstimg, dstwidth, dstheight, dststride, alignParam, null, null);
                                    //    _ChannelBImage = alignedImage;
                                    //}

                                    //Default image flipping (vertical)
                                    _ChannelBImage = ImageProcessing.WpfFlip(_ChannelBImage, ImageProcessing.FlipAxis.Vertical);

                                    //User specified flip operation in Settings
                                    if (_FlipAxis == ImageProcessing.FlipAxis.Horizontal)
                                    {
                                        _ChannelBImage = ImageProcessing.WpfFlip(_ChannelBImage, ImageProcessing.FlipAxis.Horizontal);
                                    }
                                    else if (_FlipAxis == ImageProcessing.FlipAxis.Vertical)
                                    {
                                        _ChannelBImage = ImageProcessing.WpfFlip(_ChannelBImage, ImageProcessing.FlipAxis.Vertical);
                                    }

                                    if (!_CurrentScanParam.IsEdrScanning)
                                    {
                                        if (_CurrentScanParam.Is5micronScan)
                                        {
                                            // Resize the 10um scan to 5um
                                            _ChannelBImage = ImageProcessing.Resize(_ChannelBImage, new Size(2 * _ChannelBImage.PixelWidth, 2 * _ChannelBImage.PixelHeight));
                                        }
                                        if (_ChannelBImage.CanFreeze) { _ChannelBImage.Freeze(); }
                                    }
                                }
                                catch
                                {
                                    throw;
                                }
                            }
                        }

                        #endregion

                        //if (IsSaveDataOnScanAbort)
                        //{
                        //    if (ScanRegionCompleted != null)
                        //    {
                        //        ScanRegionCompleted?.Invoke(this, (ImageInfo)_ImageInfo.Clone(), _CurrentScanRegion);
                        //        Thread.Sleep(1000);
                        //    }
                        //}
                    }
                }

                CommandStatus?.Invoke(this, string.Empty);
            }
            catch (Exception ex)
            {
                throw new Exception("ProcessCompletedScanRegion: Error\n" + ex.Message);
            }
            //finally
            //{
            //    // Forces an immediate garbage collection.
            //    //GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced);
            //    //GC.WaitForPendingFinalizers();
            //    //GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced);
            //}
        }

        #endregion

        public void KeepDataOnAbort()
        {
            if (IsScanAborted && IsSaveDataOnScanAbort)
            {
                if (_CurrentScanParam.IsZScanning)
                {
                    if (ZScanningCompleted != null)
                    {
                        ZScanningCompleted?.Invoke(this, (ImageInfo)_ImageInfo.Clone(), _CurrentScanRegion);
                        Thread.Sleep(1000);
                    }
                }
                else
                {
                    if (ScanRegionCompleted != null)
                    {
                        ScanRegionCompleted?.Invoke(this, (ImageInfo)_ImageInfo.Clone(), _CurrentScanRegion);
                        Thread.Sleep(1000);
                    }
                }
            }
        }

        private double GetScaleFactor(WriteableBitmap wbmLowLevel, WriteableBitmap wbmHighLevel)
        {
            //System.Diagnostics.Stopwatch stopWatch = new System.Diagnostics.Stopwatch();

            int width = wbmHighLevel.PixelWidth;
            int height = wbmHighLevel.PixelHeight;
            int stride = wbmHighLevel.BackBufferStride;
            int numDataPoints = 10;
            List<DataPoint>[] dataGroupHi = new List<DataPoint>[numDataPoints];

            uint nMin = 0, nMax = 0;
            int offsetX = (int)(width * 0.05);
            int offsetY = (int)(height * 0.05);
            ImageProcessing.MinMax(wbmHighLevel, new Rect(offsetX, offsetY, width - 2 * offsetX, height - 2 * offsetY), ref nMin, ref nMax);
            if (nMax > 60000) { nMax = 60000; }
            int rangeMax = (int)(nMax - nMin);
            int blockRange = (int)(rangeMax / numDataPoints);
            for (int i = 0; i < numDataPoints; i++)
            {
                //stopWatch.Start();

                dataGroupHi[i] = GetDataPoints(wbmHighLevel, (int)nMax - (i * blockRange) - blockRange, (int)nMax - (i * blockRange));

                //stopWatch.Stop();
                //Logger.LogMessage(string.Format("Group {0}: {1}", i, stopWatch.ElapsedMilliseconds));
            }

            ImageStatistics imageStats = new ImageStatistics();
            List<DataPoint>[] dataGroupLo = GetDataPoints(wbmLowLevel, dataGroupHi);
            List<RsquaredData>[] arrRsquaredData = new List<RsquaredData>[dataGroupHi.Length];
            double rsquared;
            double yintercept;
            double slopetemp;
            int dataSets = dataGroupHi.Length;
            int inclusiveStart = 0;
            int exclusiveEnd = dataSets;
            double rsquaredMax = 0;
            RsquaredData[] rsquaredDataHi = new RsquaredData[dataSets];
            RsquaredData[] rsquaredDataLo = new RsquaredData[dataSets];
            // get the rsquared values on the corresponding data set from smart (lo) and saturated (hi)
            for (int i = 0; i < dataSets; i++)
            {
                var loVals = dataGroupLo[i].Select(x => (double)x.Value).ToArray();
                var hiVals = dataGroupHi[i].Select(x => (double)x.Value).ToArray();
                imageStats.LinearRegression(loVals, hiVals, 0, hiVals.Length, out rsquared, out yintercept, out slopetemp);
                rsquaredDataHi[i] = new RsquaredData(dataGroupHi[i], rsquared);
                rsquaredDataLo[i] = new RsquaredData(dataGroupLo[i], rsquared);
                if (rsquared > rsquaredMax)
                {
                    rsquaredMax = rsquared;
                }
                //System.Diagnostics.Trace.WriteLine("rsquared max: " + rsquaredMax.ToString());
                LogMessage("EDR: rsquared max: " + rsquaredMax.ToString());
            }

            try
            {
                // sort by rsquared value
                var sortedRsquaredDataHi = rsquaredDataHi.OrderBy(o => o.RsquaredValue != null).ThenBy(o => o.RsquaredValue).ToArray();
                var sortedRsquaredDataLo = rsquaredDataLo.OrderBy(o => o.RsquaredValue != null).ThenBy(o => o.RsquaredValue).ToArray();
                dataGroupHi = sortedRsquaredDataHi.Select(x => x.DataPoints).ToArray();
                dataGroupLo = sortedRsquaredDataLo.Select(x => x.DataPoints).ToArray();
            }
            catch
            {
            }

            //System.Diagnostics.Trace.WriteLine("inclusiveStart: " + inclusiveStart.ToString());
            LogMessage("EDR: inclusiveStart: " + inclusiveStart.ToString());

            if (rsquaredMax > 0.80)
            {
                // Use the last 3 data sets
                inclusiveStart = dataGroupHi.Length - 3;
                exclusiveEnd = dataGroupHi.Length;
            }
            else
            {
                // Use the first 3 data sets
                inclusiveStart = 0;
                exclusiveEnd = 3;
            }

            //System.Diagnostics.Trace.WriteLine("inclusiveStart value used: " + inclusiveStart.ToString());
            //System.Diagnostics.Trace.WriteLine("exclusiveEnd value used: " + exclusiveEnd.ToString());
            LogMessage("EDR: inclusiveStart value used: " + inclusiveStart.ToString());
            LogMessage("EDR: exclusiveEnd value used: " + exclusiveEnd.ToString());

            List<double> hiLevelMeanList = GetMean(dataGroupHi, inclusiveStart, exclusiveEnd);
            List<double> loLevelMeanList = GetMean(dataGroupLo, inclusiveStart, exclusiveEnd);
            FilterMean(ref loLevelMeanList, ref hiLevelMeanList);
            double slope = Azure.ImagingSystem.LinearRegression.Slope(loLevelMeanList.ToArray(), hiLevelMeanList.ToArray());
            return slope;
        }
        internal unsafe List<DataPoint> GetDataPoints(WriteableBitmap img, int lower, int upper)
        {
            List<DataPoint> result = new List<DataPoint>();
            int width = img.PixelWidth;
            int height = img.PixelHeight;
            int stride = img.BackBufferStride;
            byte* pData = (byte*)img.BackBuffer.ToPointer();
            ushort* pData16;
            int offsetX = (int)(width * 0.10);
            int offsetY = (int)(height * 0.10);

            for (int iRow = offsetY; iRow < height - offsetY; iRow++)
            {
                pData16 = (ushort*)(pData + (iRow * stride));
                for (int iCol = offsetX; iCol < width - offsetX; iCol++)
                {
                    if (*(pData16 + iCol) < upper && *(pData16 + iCol) > lower)
                    {
                        result.Add(new DataPoint(new Point(iCol, iRow), *(pData16 + iCol)));
                        if (result.Count == 20)
                        {
                            break;
                        }
                    }
                }
                if (result.Count == 20)
                {
                    break;
                }
            }
            return result;
        }
        internal unsafe List<DataPoint>[] GetDataPoints(WriteableBitmap img, List<DataPoint>[] dataPoints)
        {
            int width = img.PixelWidth;
            int height = img.PixelHeight;
            int stride = img.BackBufferStride;
            byte* pData = (byte*)img.BackBuffer.ToPointer();
            ushort* pData16;
            int iRow = 0;
            int iCol = 0;
            int dataCount = 0;

            int groupCount = dataPoints.Length;
            List<DataPoint>[] newDataPoints = new List<DataPoint>[groupCount];

            for (int i = 0; i < groupCount; i++)
            {
                newDataPoints[i] = new List<DataPoint>();
                dataCount = dataPoints[i].Count;
                for (int j = 0; j < dataCount; j++)
                {
                    iRow = (int)dataPoints[i][j].Point.Y;
                    iCol = (int)dataPoints[i][j].Point.X;
                    pData16 = (ushort*)(pData + (iRow * stride));
                    newDataPoints[i].Add(new DataPoint(new Point(iCol, iRow), *(pData16 + iCol)));
                }
            }
            return newDataPoints;
        }
        internal List<double> GetMean(List<DataPoint>[] dataGroup, int inclusiveStart, int exclusiveEnd)
        {
            List<double> meanList = new List<double>();

            int groupCount = dataGroup.Length;
            for (int i = inclusiveStart; i < exclusiveEnd; i++)
            {
                double dMean = 0;
                double dTotal = 0;
                int dataCount = dataGroup[i].Count;
                for (int j = 0; j < dataCount; j++)
                {
                    dTotal += dataGroup[i][j].Value;
                }
                dMean = dTotal / dataCount;
                meanList.Add(dMean);
            }
            return meanList;
        }
        internal unsafe List<double> GetMean(WriteableBitmap img, List<DataPoint>[] dataGroup)
        {
            int width = img.PixelWidth;
            int height = img.PixelHeight;
            int stride = img.BackBufferStride;
            byte* pData = (byte*)img.BackBuffer.ToPointer();
            ushort* pData16;
            int iRow = 0;
            int iCol = 0;
            double dTotal = 0;
            double dMean = 0;
            int dataCount = 0;
            List<double> meanList = new List<double>();

            int groupCount = dataGroup.Length;
            for (int i = 0; i < groupCount; i++)
            {
                dMean = 0;
                dTotal = 0;
                dataCount = dataGroup[i].Count;
                for (int j = 0; j < dataCount; j++)
                {
                    iRow = (int)dataGroup[i][j].Point.Y;
                    iCol = (int)dataGroup[i][j].Point.X;
                    pData16 = (ushort*)(pData + (iRow * stride));
                    dTotal += *(pData16 + iCol);
                }
                dMean = dTotal / dataCount;
                meanList.Add(dMean);
            }
            return meanList;
        }
        internal void FilterMean(ref List<double> meanLow, ref List<double> meanHigh)
        {
            const int threshold = 55000;
            if (meanLow != null && meanHigh != null && meanLow.Count > 0 && meanHigh.Count > 0)
            {
                for (int i = meanHigh.Count - 1; i >= 0; i--)
                {
                    if (meanHigh[i] > threshold || (Double.IsNaN(meanHigh[i]) || Double.IsInfinity(meanHigh[i])))
                    {
                        meanLow.Remove(meanLow[i]);
                        meanHigh.Remove(meanHigh[i]);
                    }
                }
            }
        }
        private unsafe void ProcessEDRPixelAndCompress(ref WriteableBitmap lowSignalFrame, ref WriteableBitmap highSignalFrame, ref int dynamicBit, ref int actualEdrBitDepth, double scaleFactor, double compressFactor)
        {
            int width = 0;
            int height = 0;
            int bufferWidth = 0;

            dynamicBit = (int)Math.Ceiling((Math.Log10(60000 * compressFactor) / Math.Log10(2)));
            // The capture software will now use 'EdrBitDepth' and AzureSpot will use 'DynamicBit' to load and display
            actualEdrBitDepth = dynamicBit;
            // AzureSpot work-around for EDR image (AzureSpot assumes EDR images are saved with 24-bit compression)
            if (_IsSaveEdrAs24bit) { dynamicBit = 24; }
            double compressCoeff = 16.0 / (double)dynamicBit;

            if (lowSignalFrame != null && highSignalFrame != null)
            {
                width = lowSignalFrame.PixelWidth;
                height = highSignalFrame.PixelHeight;
                bufferWidth = lowSignalFrame.BackBufferStride;
                byte* pImgBufferLow = (byte*)lowSignalFrame.BackBuffer.ToPointer();
                byte* pImgBufferHigh = (byte*)highSignalFrame.BackBuffer.ToPointer();

                ushort* pLowSignal16;
                ushort* pHighSignal16;
                double dTempLoVal = 0;
                int nTempVal = 0;
                for (int iRow = 0; iRow < height; iRow++)
                {
                    pLowSignal16 = (ushort*)(pImgBufferLow + (iRow * bufferWidth));
                    pHighSignal16 = (ushort*)(pImgBufferHigh + (iRow * bufferWidth));
                    for (int iCol = 0; iCol < width; iCol++)
                    {
                        if (*(pHighSignal16 + iCol) > 60000)
                        {
                            dTempLoVal = *(pLowSignal16 + iCol);
                            dTempLoVal *= scaleFactor;
                            nTempVal = (int)Math.Pow(dTempLoVal, compressCoeff);
                            // Just in case there's an overflow
                            // If low level frame (smart-scan image) is saturated (or above 60000), we're going to have an overflow
                            if (nTempVal > 65535)
                            {
                                nTempVal = 65535;
                            }
                            *(pHighSignal16 + iCol) = (ushort)nTempVal;
                        }
                        else
                        {
                            dTempLoVal = *(pHighSignal16 + iCol);
                            *(pHighSignal16 + iCol) = (ushort)Math.Pow(dTempLoVal, compressCoeff);
                        }
                    }
                }
            }
        }

        /*private unsafe List<double> GetMean(WriteableBitmap srcimg, Point offset)
        {
            int width = srcimg.PixelWidth;
            int height = srcimg.PixelHeight;
            int bufStride = srcimg.BackBufferStride;
            int numOf5x5Cols = 25;
            int kernel = 5;    // 5x5
            int populationSize = 25;
            ushort[] pixelPopulation = new ushort[populationSize];
            byte* pSrcData = (byte*)srcimg.BackBuffer.ToPointer();
            ushort* pData16;
            int popIndex = 0;
            double mean = 0;

            List<double> meanList = new List<double>();

            fixed (void* pPtr = &pixelPopulation[0])
            {
                ushort* pPopulation = (ushort*)pPtr;
                for (int iRow = (int)offset.Y; iRow <= height - (2 * (int)offset.Y); iRow += 5)
                {
                    for (int iCol = (int)offset.X; iCol <= width - (2 * (int)offset.X); iCol += 5)
                    {
                        popIndex = 0;
                        for (int i = 0; i < kernel; i++)   // 5x5 block
                        {
                            pData16 = (ushort*)(pSrcData + ((iRow + i) * bufStride));
                            for (int j = 0; j < kernel; j++)
                            {
                                *(pPopulation + popIndex++) = (ushort)*(pData16 + iCol + j);
                            }
                        }

                        mean = 0.0;
                        for (int i = 0; i < populationSize; ++i)
                        {
                            mean += *(pPopulation + i);
                        }
                        for (int i = 0; i < populationSize; ++i)
                        {
                            *(pPopulation + i) = 0;
                        }

                        mean = mean / populationSize;
                        meanList.Add(mean);
                        if (meanList.Count == numOf5x5Cols)
                        {
                            break;
                        }
                    } //end for (iCol)
                    break;
                } //end for (iRow)
            } //end fixed
            return meanList;
        }*/

        /*private void FilterMean(ref List<double> meanLow, ref List<double> meanHigh)
        {
            const int threshold = 55000;
            if (meanLow != null && meanHigh != null && meanLow.Count > 0 && meanHigh.Count > 0)
            {
                for (int i = meanHigh.Count - 1; i >= 0; i--)
                {
                    if (meanHigh[i] > threshold)
                    {
                        meanLow.Remove(meanLow[i]);
                        meanHigh.Remove(meanHigh[i]);
                    }
                }
            }
        }*/

        /*private double GetScaleFactor(WriteableBitmap lowLevel, WriteableBitmap highLevel, Point offset)
        {
            List<double> loLevelMeanList = GetMean(lowLevel, offset);
            List<double> hiLevelMeanList = GetMean(highLevel, offset);
            FilterMean(ref loLevelMeanList, ref hiLevelMeanList);
            //EL: TODO: high level as X or Y?
            //double slope = LinearRegression.Slope(hiLevelMeanList.ToArray(), loLevelMeanList.ToArray());
            double slope = LinearRegression.Slope(loLevelMeanList.ToArray(), hiLevelMeanList.ToArray());
            return slope;
        }*/

        /*private Point GetOffSet(WriteableBitmap srcimg, ref bool bIsBlobFound)
        {
            Point offset = new Point();

            // Scale the image to find the blobs and use the blobs to find the coordinate of where we're getting the data sets
            //
            Ipp.Imaging.PixelFormatType pixelFormat = Ipp.Imaging.IppImaging.GetPixelFormatType(srcimg.Format);
            var srcPixelFormat = (pixelFormat == Ipp.Imaging.PixelFormatType.P8u_C1) ? System.Windows.Media.PixelFormats.Gray8 : System.Windows.Media.PixelFormats.Gray16;
            int width = srcimg.PixelWidth;
            int height = srcimg.PixelHeight;
            int srcStep = srcimg.BackBufferStride;
            WriteableBitmap blobsImage = null;
            List<System.Drawing.Rectangle> blobsRects = new List<System.Drawing.Rectangle>();
            bIsBlobFound = false;

            try
            {
                // Create 8-bit indexed image buffer.
                var palette = new BitmapPalette(ImageProcessing.GetColorTableIndexed(false));
                WriteableBitmap scaledimg = new WriteableBitmap(width, height, 96, 96, System.Windows.Media.PixelFormats.Indexed8, palette);
                int dstStep = scaledimg.BackBufferStride;

                unsafe
                {
                    // Scale the image to make it easier to find the blobs
                    //
                    byte* psrcimg = (byte*)srcimg.BackBuffer.ToPointer();
                    byte* pdstimg = (byte*)scaledimg.BackBuffer.ToPointer();
                    int x = (int)(width * 0.05);
                    int y = (int)(height * 0.05);
                    int w = width - (x * 2);
                    int h = height - (y * 2);
                    //int max = ImageProcessing.Max(srcimg, new Rect(x, y, w, h));
                    //int nBlackVal = 0, nWhiteVal = max / 4;
                    //int min = ImageProcessing.Min(srcimg, new Rect(0, 0, width, height));
                    //int nBlackVal = 0, nWhiteVal = min + 2000;
                    uint min = 0;
                    uint max = 0;
                    ImageProcessing.MinMax(srcimg, new Rect(0, 0, width, height), ref min, ref max);
                    int nBlackVal = (int)min, nWhiteVal = (int)min + 5000;
                    if (nWhiteVal > max)
                    {
                        nBlackVal = (int)min;
                        nWhiteVal = (int)max;
                    }
                    double dGammaVal = 1.0;
                    ImageProcessing.Scale_16u8u_C1_Indexed(psrcimg, width, height, srcStep, srcPixelFormat,
                                       pdstimg, dstStep,
                                       nBlackVal, nWhiteVal, dGammaVal,
                                       false,
                                       62000,
                                       false);
                }

                if (scaledimg != null)
                {
                    if (_IsSaveDebuggingImages)
                    {
                        string filePath = Path.Combine(_DBTIPath, "EDRTestImage_Scaled.tif");
                        ImagingHelper.SaveFile(filePath, scaledimg);
                    }
                    //int bw = (int)(width * 0.10);
                    //int bh = (int)(height * 0.10);
                    //int min = Math.Min(width, height);
                    //int blobsize = (int)(min * 0.10);
                    int blobsize = 50;  //50 x 50px
                    ImageProcessing.FindBlobs8bpp(scaledimg, new System.Windows.Size(blobsize, blobsize), ref blobsImage, ref blobsRects);

                    if (_IsSaveDebuggingImages && blobsImage != null)
                    {
                        bIsBlobFound = true;
                        string filePath = Path.Combine(_DBTIPath, "EDRTestImage_Blob.tif");
                        ImagingHelper.SaveFile(filePath, blobsImage);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("GetOffset() error...\n" + ex.Message);
            }

            int offsetX = 0;
            int offsetY = 0;
            if (blobsImage != null && blobsRects != null && blobsRects.Count > 0)
            {
                offsetX = blobsRects[0].X;
                offsetY = blobsRects[0].Y;
                if (offsetX == 0)
                {
                    offsetX = (int)(width * 0.10);
                }
                if (offsetY == 0)
                {
                    offsetY = (int)(height * 0.10);
                }
            }
            else
            {
                offsetX = (int)(width * 0.10);
                offsetY = (int)(height * 0.10);
            }

            offset.X = offsetX;
            offset.Y = offsetY;

            return offset;
        }*/

        /// <summary>
        /// Simulation Mode: Simulate image scanning.
        /// </summary>
        protected override void SimulateThreadFunction()
        {
            //TODO: handle simulation mode
            //
            // Hang around here for 5 seconds...
            Thread.Sleep(5000);
        }
    }

    #region Helper Class

    public class DataPoint
    {
        public Point Point { get; set; }
        public int Value { get; set; }
        public DataPoint(Point pt, int value)
        {
            Point = pt;
            Value = value;
        }
        public DataPoint(DataPoint other)
        {
            Point = other.Point;
            Value = other.Value;
        }
    }

    public class RsquaredData
    {
        public List<DataPoint> DataPoints { get; set; }
        public double? RsquaredValue;
        public RsquaredData(List<DataPoint> dataPoints, double rsquared)
        {
            DataPoints = new List<DataPoint>(dataPoints);
            RsquaredValue = rsquared;
        }
    }
    #endregion

}
