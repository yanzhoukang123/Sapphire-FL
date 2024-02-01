using Azure.CommandLib;
using Azure.EthernetCommLib;
using Azure.Image.Processing;
using Azure.MotionLib;
using Microsoft.Research.DynamicDataDisplay.DataSources;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Windows.Media.Imaging;

namespace Azure.ImagingSystem
{
    public enum TestJigScanTypes
    {
        Unknown,
        Static,
        Vertical,
        Horizontal,
        XAxis,
        LightGainVertical,
    }
    public class TestJigScanProcessing : ThreadBase
    {
        public delegate void ReceivedScanDataHandle(string dataName);
        public event ReceivedScanDataHandle OnScanDataReceived;

        #region Private Fields
        private EthernetController _CommController;
        private MotionController _MotionController;
        public TestJigScanParameterStruct _ScanSettings;
        private const int PACKSIZE = 1286;
        private const int FramesPerPackage = 80;
        public ushort[] _APDChannelA;
        public ushort[] _APDChannelB;
        public ushort[] _APDChannelC;
        private IntPtr _AChannelPtr;
        private IntPtr _BChannelPtr;
        private IntPtr _CChannelPtr;

        #endregion Private Fields

        #region Public Properties
        public int[] SampleIndex { get; private set; }
        public int[] SampleValueChannelA { get; private set; }
        public int[] SampleValueChannelB { get; private set; }
        public int[] SampleValueChannelC { get; private set; }
        public int[] LightGainValueChannel0 { get; private set; }
        public int[] LightGainValueChannel1 { get; private set; }
        public ImageInfo ImageInfo { get; private set; }
        public TestJigScanTypes ScanType { get; private set; }
        //public WriteableBitmap ChannelAImage { get; private set; }
        //public WriteableBitmap ChannelBImage { get; private set; }
        //public WriteableBitmap ChannelCImage { get; private set; }
        public int RemainingTime { get; private set; }
        public int XCalibratingSpeed { get; set; }
        public double posOfThresholdLeft = 0;
        public double posOfThresholdRight = 0;
        #region  point
        System.Windows.Point[] _StaticArrayChannelA;
        System.Windows.Point[] _StaticArrayChannelB;
        System.Windows.Point[] _StaticArrayChannelC;
        System.Windows.Point[] _StaticLightGainValueChannel0;
        EnumerableDataSource<System.Windows.Point> _StaticChannelA;
        EnumerableDataSource<System.Windows.Point> _StaticChannelB;
        EnumerableDataSource<System.Windows.Point> _StaticChannelC;

        System.Windows.Point _StaticChannelAMax;
        System.Windows.Point _StaticChannelBMax;
        System.Windows.Point _StaticChannelCMax;

        public System.Windows.Point StaticChannelAMax
        {
            get
            {
                return _StaticChannelAMax;
            }
        }

        public System.Windows.Point StaticChannelBMax
        {
            get
            {
                return _StaticChannelBMax;
            }
        }

        public System.Windows.Point StaticChannelCMax
        {
            get
            {
                return _StaticChannelCMax;
            }
        }


        public EnumerableDataSource<System.Windows.Point> StaticChannelA
        {
            get
            {
                return _StaticChannelA;
            }
        }
        public EnumerableDataSource<System.Windows.Point> StaticChannelB
        {
            get
            {
                return _StaticChannelB;
            }
        }
        public EnumerableDataSource<System.Windows.Point> StaticChannelC
        {
            get
            {
                return _StaticChannelC;
            }
        }

        #endregion

        #endregion Public Properties

        public TestJigScanProcessing(EthernetController ethernet, MotionController motion, TestJigScanParameterStruct scanParameter, IntPtr[] Channel)
        {
            _CommController = ethernet;
            _MotionController = motion;
            _ScanSettings = scanParameter;

            if (Channel != null && Channel.Length > 0)
            {
                _AChannelPtr = Channel[0];//scan _ChannelAImage 
                _BChannelPtr = Channel[1];//scan _ChannelBImage 
                _CChannelPtr = Channel[2];//scan _ChannelCImage 
            }
        }

        public override void ThreadFunction()
        {
            RemainingTime = _ScanSettings.Time;

            if (_ScanSettings.ScanDeltaX == 0 && _ScanSettings.ScanDeltaY == 0 && _ScanSettings.ScanDeltaZ == 0)
            {
                ScanType = TestJigScanTypes.Static;
                StaticScanProcess();
            }
            else if (_ScanSettings.ScanDeltaX == 0 && _ScanSettings.ScanDeltaY == 0 && !_ScanSettings.IsLightGainModule)
            {
                ScanType = TestJigScanTypes.Vertical;
                VerticalScanProcess();
            }
            else if (_ScanSettings.ScanDeltaX > 0 && _ScanSettings.ScanDeltaY > 0)
            {
                ScanType = TestJigScanTypes.Horizontal;
                HorizontalScanProcess();
            }
            else if (_ScanSettings.ScanDeltaX > 0 && _ScanSettings.DataRate > 0)
            {
                ScanType = TestJigScanTypes.XAxis;
                XAxisScanProcess();
            }
            else if (_ScanSettings.ScanDeltaX == 0 && _ScanSettings.ScanDeltaY == 0 && _ScanSettings.IsLightGainModule && !_ScanSettings.IsGlassScan)
            {
                ScanType = TestJigScanTypes.LightGainVertical;
                if (_ScanSettings.ScanPreciseAt)
                {
                    LightGainVerticalScanProcess(_ScanSettings.ScanZ0, _ScanSettings.ScanDeltaZ, 1000);
                }
                else
                {
                    LightGainVerticalScanProcess(_ScanSettings.ScanZ0, _ScanSettings.ScanDeltaZ, _ScanSettings.ScanPreciseParameter);
                }


            }
        }

        public override void AbortWork()
        {
            _CommController.StopScan();
        }

        void VerticalFols()
        {

            double offsetX;//垂直的z坐标算法
            int samplePts = _ScanSettings.ScanDeltaZ / 8;
            System.Windows.Point staticChannelA = new System.Windows.Point();
            System.Windows.Point staticChannelB = new System.Windows.Point();
            System.Windows.Point staticChannelC = new System.Windows.Point();
            _StaticArrayChannelA = new System.Windows.Point[samplePts];
            _StaticArrayChannelB = new System.Windows.Point[samplePts];
            _StaticArrayChannelC = new System.Windows.Point[samplePts];
            if (_ScanSettings.ScanDeltaZ > 0)
            {
                offsetX = (double)_ScanSettings.ScanDeltaZ / (double)_ScanSettings.ZMotorSubdivision / (double)samplePts;
            }
            else
            {
                offsetX = (double)_ScanSettings.DataRate;
            }
            int[] tempValCHA = SampleValueChannelA;
            int[] tempValCHB = SampleValueChannelB;
            int[] tempValCHC = SampleValueChannelC;
            FilterProcess(ref tempValCHA, 51);
            FilterProcess(ref tempValCHB, 51);
            FilterProcess(ref tempValCHC, 51);
            double x = 0;
            for (int offsetPointNum = 0; offsetPointNum < samplePts; offsetPointNum++)
            {
                x += offsetX;
                staticChannelA.Y = tempValCHA[offsetPointNum];
                staticChannelA.X = x;

                staticChannelB.Y = tempValCHB[offsetPointNum];
                staticChannelB.X = x;

                staticChannelC.Y = tempValCHC[offsetPointNum];
                staticChannelC.X = x;

                _StaticArrayChannelA[offsetPointNum] = staticChannelA;
                _StaticArrayChannelB[offsetPointNum] = staticChannelB;
                _StaticArrayChannelC[offsetPointNum] = staticChannelC;
            }
            double posMaxValChA = 0;
            double posMaxValChB = 0;
            double posMaxValChC = 0;
            double MaxValChA = 0;
            double MaxValChB = 0;
            double MaxValChC = 0;
            int ZSanValue = _ScanSettings.ZScanValueThreshold;
            //ThresholdProcess(_StaticArrayChannelA, out posMaxValChA, out MaxValChA);
            ThresholdProcess(_StaticArrayChannelA, ZSanValue, out posMaxValChA, out MaxValChA);
            ThresholdProcess(_StaticArrayChannelB, ZSanValue, out posMaxValChB, out MaxValChB);
            ThresholdProcess(_StaticArrayChannelC, ZSanValue, out posMaxValChC, out MaxValChC);
            _StaticChannelAMax.X = posMaxValChA;
            _StaticChannelAMax.Y = MaxValChA;
            _StaticChannelBMax.X = posMaxValChB;
            _StaticChannelBMax.Y = MaxValChB;
            _StaticChannelCMax.X = posMaxValChC;
            _StaticChannelCMax.Y = MaxValChC;
        }

        void LightVerticalFols()
        {

            double offsetX = 0;//垂直的z坐标算法
            System.Windows.Point staticChannel0 = new System.Windows.Point();
            _StaticLightGainValueChannel0 = new System.Windows.Point[LightGainValueChannel0.Length];
            if (_ScanSettings.ScanDeltaZ > 0)
            {
                offsetX = 8 / _ScanSettings.ZMotorSubdivision;
            }
            int[] tempValCHA = LightGainValueChannel0;
            FilterProcess(ref tempValCHA, 51);
            double coeffChLight = 1;
            for (int i = 0; i < _StaticLightGainValueChannel0.Length; i++)
            {
                staticChannel0.X = SampleIndex[i] * offsetX;
                staticChannel0.Y = tempValCHA[i] * coeffChLight;
                _StaticLightGainValueChannel0[i] = staticChannel0;

            }
            double posMaxValChA = 0;
            double MaxValChA = 0;
            int ZSanValue = _ScanSettings.ZScanValueThreshold;
            ThresholdProcess(_StaticLightGainValueChannel0, ZSanValue, out posMaxValChA, out MaxValChA);
        }

        public override void Finish()
        {
            LightIndex = 0;
            _CommController.StopScan();
            //_CommController.SetLaserPower(LaserChannels.ChannelA, 0);
            //_CommController.SetLaserPower(LaserChannels.ChannelB, 0);
            //_CommController.SetLaserPower(LaserChannels.ChannelC, 0);
            //_MotionController.AutoQuery = true;
            //_MotionController.SetStart(MotorTypes.X | MotorTypes.Y | MotorTypes.Z, new bool[] { false, false, false });

            if (ExitStat == ThreadExitStat.None)
            {
                if (ScanType == TestJigScanTypes.Static || ScanType == TestJigScanTypes.Vertical || ScanType == TestJigScanTypes.XAxis)
                {
                    try
                    {

                        string fileName;
                        switch (ScanType)
                        {
                            case TestJigScanTypes.Static:
                                fileName = "StaticScanData.csv";
                                break;
                            case TestJigScanTypes.Vertical:
                                VerticalFols();
                                fileName = "VerticalScanData.csv";
                                string[] testdata = new string[SampleValueChannelA.Length];
                                for (int y = 0; y < SampleValueChannelA.Length; y++)
                                {
                                    testdata[y] = SampleIndex[y].ToString() + "," +
                                        SampleValueChannelA[y].ToString() + "," +
                                        SampleValueChannelB[y].ToString() + "," +
                                        SampleValueChannelC[y].ToString();
                                }
                                File.WriteAllLines(@"static.csv", testdata, System.Text.Encoding.UTF8);
                                break;
                            case TestJigScanTypes.LightGainVertical:
                                fileName = "LightGainVerticalScanData.csv";
                                string[] Lightdata = new string[LightGainValueChannel0.Length];
                                for (int y = 0; y < LightGainValueChannel0.Length; y++)
                                {
                                    Lightdata[y] = SampleIndex[y].ToString() + "," +
                                        LightGainValueChannel0[y].ToString();
                                }
                                File.WriteAllLines(@"static.csv", Lightdata, System.Text.Encoding.UTF8);
                                break;
                            case TestJigScanTypes.XAxis:
                                fileName = "XScanData.csv";
                                break;
                        }
                        //if (_ScanSettings.IsGlassScan)
                        //{//玻璃面调平
                        //    OnScanDataReceived?.Invoke("IsGlassCheck");
                        //}
                    }
                    catch (System.Exception ex)
                    {
                        throw ex;
                    }
                }
                else if (ScanType == TestJigScanTypes.Horizontal)
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
                    catch (Exception ex)
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
            SampleIndex = new int[_ScanSettings.LineCounts];
            SampleValueChannelA = new int[_ScanSettings.LineCounts];
            SampleValueChannelB = new int[_ScanSettings.LineCounts];
            SampleValueChannelC = new int[_ScanSettings.LineCounts];
            _CommController.ReceivingBuf.Reset();
            //_MotionController.AutoQuery = false;
            _CommController.TriggerTimingScan((uint)_ScanSettings.DataRate);
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
                        if (index >= _ScanSettings.LineCounts)
                        {
                            return;
                        }
                        SampleIndex[index] = index * _ScanSettings.DataRate;
                        SampleValueChannelA[index] = BitConverter.ToInt32(tempBuf, 8 + i * 16);
                        SampleValueChannelB[index] = BitConverter.ToInt32(tempBuf, 12 + i * 16);
                        SampleValueChannelC[index] = BitConverter.ToInt32(tempBuf, 16 + i * 16);
                    }
                }
            }
        }

        private void VerticalScanProcess()
        {
            _ScanSettings.ScanZ0 = 0;
            byte[] tempBuf = new byte[PACKSIZE];
            int index = 0;
            int samplePts = _ScanSettings.ScanDeltaZ / 8;   // every 8 z clocks generating 1 sample result
            SampleIndex = new int[samplePts];
            SampleValueChannelA = new int[samplePts];
            SampleValueChannelB = new int[samplePts];
            SampleValueChannelC = new int[samplePts];
            if (PresetMotion(60000) == false)       // wait 1 minutes for the motions to be at the starting positions
            {
                ExitStat = ThreadExitStat.Error;
                throw new Exception("Failed to move the motions to the start positions");
            }
            if (_ScanSettings.IsGlassScan)
            {//玻璃面调平
                OnScanDataReceived?.Invoke("GlassScan");
            }
            _CommController.ReceivingBuf.Reset();
            int zPulses = _ScanSettings.ScanDeltaZ + 80 * 8;    // go more 80 sample points to finish all data transport
            _MotionController.AbsoluteMoveSingleMotion(MotorTypes.Z, 256, _ScanSettings.ZMotorSpeed, _ScanSettings.ZMotionAccVal, _ScanSettings.ZMotionDccVal, zPulses, true, false);
            _MotionController.AutoQuery = false;
            _CommController.TriggerZScan((uint)_ScanSettings.ScanDeltaZ);

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
        List<byte[]> _tempData = new List<byte[]>();
        private bool singe = false;
        private void LightGainVerticalScanProcess(double ScanZ0, double ScanDeltaZ, int SampleInterval)
        {
            LightIndex = 0;
            singe = true;
            _ScanSettings.ScanZ0 = (int)ScanZ0;
            _ScanSettings.ScanDeltaZ = (int)ScanDeltaZ;
            _CommController.LightGainData = null;
            byte[] tempBuf = new byte[PACKSIZE];
            int zPulses = _ScanSettings.ScanDeltaZ - _ScanSettings.ScanZ0;
            double trypars = (double)(_ScanSettings.ScanDeltaZ - _ScanSettings.ScanZ0) / SampleInterval;
            int indexLight = 0;
            if (!int.TryParse(trypars.ToString(), out indexLight))
            {
                indexLight = (int)trypars + 2;
            }
            else
            {
                indexLight = (int)trypars + 1;
            }
            int samplePts = _ScanSettings.ScanDeltaZ / 8;   // every 8 z clocks generating 1 sample result
            SampleIndex = new int[indexLight];
            LightGainValueChannel0 = new int[indexLight];
            LightGainValueChannel1 = new int[indexLight];
            //起始位置
            if (PresetMotion(60000) == false)       // wait 1 minutes for the motions to be at the starting positions
            {
                ExitStat = ThreadExitStat.Error;
                throw new Exception("Failed to move the motions to the start positions");
            }
            if (_ScanSettings.IsGlassScan)
            {//玻璃面调平
                OnScanDataReceived?.Invoke("GlassScan");
            }
            _CommController.ReceivingBuf.Reset();
            //写入增益
            bool gain = _CommController.SetLightGain((UInt16)_ScanSettings.LightGain);
            Thread.Sleep(300);
            //采样间隔粗调1000/最小10，
            bool sampleinterval = _CommController.SetLightSampleInterval((UInt16)SampleInterval);
            Thread.Sleep(200);
            //采样范围
            bool sampleRange = _CommController.SetLightSampleRange((Int32)zPulses);
            Thread.Sleep(200);
            //启动采集
            bool sampleStart = _CommController.SetLightSampleStart((UInt16)1);
            if (!gain || !sampleinterval || !sampleRange || !sampleStart)
            {
                ExitStat = ThreadExitStat.Error;
                throw new Exception("设置光学扫描参数失败！");
            }
            Thread.Sleep(200);
            double result = ((double)SampleInterval / (double)20);
            double Speed = (double)result * 1000;
            if (Speed > 5000)
            {
                Speed = 5000;
            }
            _MotionController.AbsoluteMoveSingleMotion(MotorTypes.Z, 256, (int)Speed, _ScanSettings.ZMotionAccVal, _ScanSettings.ZMotionDccVal, _ScanSettings.ScanDeltaZ, true, false);
            _MotionController.AutoQuery = false;
            //// 触发z扫描
            _CommController.TriggerZScan((uint)_ScanSettings.ScanDeltaZ);
            Thread td = new Thread(LightGainVerticalDataProcess);
            td.IsBackground = true;
            td.Start(SampleInterval);
            //数据状态
            while (singe)
            {
                bool State = _CommController.GetDataState();//获取数据状态
                if (_CommController.LightGainDataState == 1)//状态为YES
                {
                    while (singe)
                    {
                        bool lightdata = _CommController.GetLightData();//读取数据指令
                        if (lightdata)
                        {
                            tempBuf = _CommController.LightGainData;//将数据拿走
                            if (tempBuf == null || tempBuf.Length == 0)
                            {
                                continue;
                            }
                            _tempData.Add(tempBuf);
                        }
                        //_CommController.GetTest();
                        //string test= _CommController.Test;
                    }
                }
            }
        }
        int LightIndex = 0;
        void LightGainVerticalDataProcess(object SampleInterval)
        {
            int _SampleInterval = (int)SampleInterval;
            while (singe)
            {
                if (_tempData.Count > 0)
                {
                    int zCurrentPos = 0;
                    int offset = 0;
                    for (int j = 0; j < _tempData.Count; j++)
                    {
                        byte[] tempBuf = _tempData[j];
                        if (tempBuf == null)
                        {
                            continue;
                        }
                        if (tempBuf[0] == 0x6b && tempBuf[1] == 0x01 && tempBuf[2] == 0x08 && tempBuf[3] == 0x06 && tempBuf[1287] == 0x6f)
                        {
                            tempBuf = _tempData[j].Skip(7).Take(1287).ToArray();
                            tempBuf = tempBuf.Skip(0).Take(1280).ToArray();
                            for (int i = 0; i < 160; i++)
                            {

                                offset = i * 8;
                                zCurrentPos = (BitConverter.ToInt32(tempBuf, offset)) / 8;
                                if (zCurrentPos >= (_ScanSettings.ScanDeltaZ) / 8)
                                {
                                    // _CommController.GetTest();
                                    // string test = _CommController.Test;
                                    //_CommController.Test = "";
                                    _CommController.SetLightSampleStart((UInt16)0);//采集完成
                                    SampleIndex[LightIndex] = zCurrentPos;
                                    LightGainValueChannel0[LightIndex] = BitConverter.ToInt16(tempBuf, 4 + offset);
                                    LightGainValueChannel1[LightIndex] = BitConverter.ToInt16(tempBuf, 6 + offset);
                                    singe = false;
                                    _tempData.Clear();
                                    _CommController.LightGainData = null;
                                    if (_SampleInterval != _ScanSettings.ScanPreciseParameter && _ScanSettings.ScanPreciseAt)
                                    {
                                        LightVerticalFols();
                                        double posScan0 = Math.Round((double)posOfThresholdLeft / (double)_ScanSettings.ZMotorSubdivision, 3);
                                        double posScanDeltaZ = Math.Round((double)posOfThresholdRight / (double)_ScanSettings.ZMotorSubdivision, 3);
                                        if (posScan0 <= 0 && posScanDeltaZ <= 0)
                                        {
                                            return;
                                        }
                                        LightGainVerticalScanProcess(posScan0, posScanDeltaZ, _ScanSettings.ScanPreciseParameter);
                                        posOfThresholdLeft = 0;
                                        posOfThresholdRight = 0;
                                        return;
                                    }
                                    return;

                                }
                                if (LightIndex < LightGainValueChannel0.Length)
                                {
                                    SampleIndex[LightIndex] = zCurrentPos;
                                    LightGainValueChannel0[LightIndex] = BitConverter.ToInt16(tempBuf, 4 + offset);
                                    LightGainValueChannel1[LightIndex] = BitConverter.ToInt16(tempBuf, 6 + offset);
                                }
                                LightIndex++;

                            }

                        }
                        _tempData.RemoveAt(j);
                    }

                }
            }
        }

        private void HorizontalScanProcess()
        {
            if (PresetMotion(60000) == false)       // wait 1 minutes for the motions to be at the starting positions
            {
                ExitStat = ThreadExitStat.Error;
                throw new Exception("Failed to move the motions to the start positions");
            }
            _CommController.ReceivingBuf.Reset();

            #region Step1: Set Scanning Motion Parameters
            double extraMove = _ScanSettings.XMotionExtraMoveLength;
            int singleTrip = _ScanSettings.ScanDeltaX + (int)(extraMove * 2 * _ScanSettings.XMotorSubdivision);
            double singleTripTime = _ScanSettings.Quality / 2.0 - _ScanSettings.XmotionTurnAroundDelay / 1000.0;
            //double singleTripTime = _ScanSettings.Quality / 2.0;
            int xMotorSpeed = XMotorSpeedCalibration.GetSpeed(_ScanSettings.XMotionAccVal, 256, singleTrip, singleTripTime);
            //when the motor is ready,switch APD on
            int speedY = (int)Math.Round(_ScanSettings.Res * 2 * _ScanSettings.YMotorSubdivision / _ScanSettings.Quality / 1000);
            int tgtPosY = _ScanSettings.ScanY0 + _ScanSettings.ScanDeltaY + (int)Math.Round(3 * _ScanSettings.YMotorSubdivision);
            int tgtPosX1 = _ScanSettings.ScanX0 + _ScanSettings.ScanDeltaX + (int)Math.Round(extraMove * _ScanSettings.XMotorSubdivision);
            int tgtPosX2 = _ScanSettings.ScanX0 - (int)Math.Round(extraMove * _ScanSettings.XMotorSubdivision);
            int repeats = _ScanSettings.Height / 2 + 10;
            _MotionController.AbsoluteMoveSingleMotion(MotorTypes.Y, 256, speedY, _ScanSettings.YMotionAccVal, _ScanSettings.YMotionDccVal, tgtPosY, false, false);
            _MotionController.AbsoluteMoveSingleMotion(MotorTypes.X, 256, xMotorSpeed, _ScanSettings.XMotionAccVal,
                _ScanSettings.XMotionDccVal, tgtPosX1, tgtPosX2, repeats, _ScanSettings.XmotionTurnAroundDelay, false, false);
            //_MotionController.AutoQuery = false;
            #endregion Step1: Set Scanning Motion Parameters

            #region Step2: Set ImageInfo
            DateTime dateTime = DateTime.Now;
            ImageInfo = new Azure.Image.Processing.ImageInfo();
            ImageInfo.DateTime = System.String.Format("{0:G}", dateTime.ToString());
            ImageInfo.CaptureType = "Scanner";
            ImageInfo.IsScannedImage = true;
            ImageInfo.ScanResolution = _ScanSettings.Res;
            ImageInfo.ScanQuality = _ScanSettings.Quality.ToString();
            ImageInfo.ScanX0 = _ScanSettings.ScanX0;
            ImageInfo.ScanY0 = _ScanSettings.ScanY0;
            ImageInfo.DeltaX = (int)((double)_ScanSettings.ScanDeltaX / (double)_ScanSettings.XMotorSubdivision);
            ImageInfo.DeltaY = (int)((double)_ScanSettings.ScanDeltaY / (double)_ScanSettings.YMotorSubdivision);
            ImageInfo.ScanZ0 = (double)_ScanSettings.ScanZ0 / (double)_ScanSettings.ZMotorSubdivision;
            #endregion Step2: Set ImageInfo

            #region Step3: Start Scanning
            byte[] tempBuf = new byte[PACKSIZE];
            ushort coordX = 0;
            ushort coordY = 0;
            int index = 0;
            //ChannelAImage = new WriteableBitmap(_ScanSettings.Width, _ScanSettings.Height, 90, 90, System.Windows.Media.PixelFormats.Gray16, null);
            //ChannelBImage = new WriteableBitmap(_ScanSettings.Width, _ScanSettings.Height, 90, 90, System.Windows.Media.PixelFormats.Gray16, null);
            //ChannelCImage = new WriteableBitmap(_ScanSettings.Width, _ScanSettings.Height, 90, 90, System.Windows.Media.PixelFormats.Gray16, null);
            //int bufferWidth = ChannelAImage.BackBufferStride / 2;
            int bufferWidth = _ScanSettings.BackBufferStride / 2;
            int Length = bufferWidth * _ScanSettings.Height;
            _APDChannelA = new ushort[bufferWidth * _ScanSettings.Height];
            _APDChannelB = new ushort[bufferWidth * _ScanSettings.Height];
            _APDChannelC = new ushort[bufferWidth * _ScanSettings.Height];

            uint resToEncoderCount = (uint)Math.Round(_ScanSettings.Res * _ScanSettings.XEncoderSubdivision / 1000.0);
            if (resToEncoderCount < 1)
            {
                resToEncoderCount = 1;
            }
            _CommController.SetHorizontalScanExtraMove((int)Math.Round(_ScanSettings.XMotionExtraMoveLength * _ScanSettings.XEncoderSubdivision));
            uint deltaXEncoder = (uint)(_ScanSettings.ScanDeltaX / _ScanSettings.XMotorSubdivision * _ScanSettings.XEncoderSubdivision);
            // calculate the bit depth
            if (_ScanSettings.DynamicBitsAt)
            {
                double adcSampleRate = 300000;  // adc sample rate at 300 kHz
                double pixelSampleDuration = _ScanSettings.Res * 0.001 / (xMotorSpeed / _ScanSettings.XMotorSubdivision);
                double pixelSamplePoints = pixelSampleDuration * adcSampleRate;
                int samplePointsWidth = (int)Math.Log(pixelSamplePoints, 2);
                _ScanSettings.DynamicBits = samplePointsWidth;
            }
            else
            {
                _ScanSettings.DynamicBits = 0;
            }
            _CommController.TriggerHorizontalScan(deltaXEncoder, (uint)_ScanSettings.Height, resToEncoderCount, (uint)(Math.Pow(2, _ScanSettings.DynamicBits)));
            _MotionController.SetStart(MotorTypes.X | MotorTypes.Y, new bool[] { true, true });
            #endregion Step3: Start Scanning

            #region Step4: Reading frames
            int offset = 0;
            double compressCoeff = 16.0 / (16 + _ScanSettings.DynamicBits);
            unsafe
            {
                ushort* pbuffScanA = (ushort*)_AChannelPtr.ToPointer();
                ushort* pbuffScanB = (ushort*)_BChannelPtr.ToPointer();
                ushort* pbuffScanC = (ushort*)_CChannelPtr.ToPointer();
                while (true)
                {
                    while (_CommController.ReceivingBuf.StoredSize < PACKSIZE)
                    {
                        //Thread.Sleep(1);
                    }
                    _CommController.ReceivingBuf.ReadDataOut(tempBuf, 0, PACKSIZE);

                    if (tempBuf[0] == 0x6b && tempBuf[1285] == 0x6f)
                    {
                        for (int i = 0; i < FramesPerPackage; i++)
                        {
                            offset = i * 16;
                            coordY = (ushort)(BitConverter.ToUInt16(tempBuf, 4 + offset) - 1);
                            coordX = (ushort)(BitConverter.ToUInt16(tempBuf, 6 + offset) - 1);
                            if (coordY >= _ScanSettings.Height)
                            {
                                RemainingTime = 0;
                                OnScanDataReceived?.Invoke("RemainingTime");
                                return;
                            }
                            index = coordX + coordY * bufferWidth;
                            if (index >= Length)
                            {
                                throw new Exception(string.Format("Coordinates Error, X={0},Y={1}", coordX, coordY));
                            }
                            ushort tempPixA = (ushort)(Math.Pow(BitConverter.ToUInt32(tempBuf, 8 + offset), compressCoeff));
                            ushort tempPixB = (ushort)(Math.Pow(BitConverter.ToUInt32(tempBuf, 12 + offset), compressCoeff));
                            ushort tempPixC = (ushort)(Math.Pow(BitConverter.ToUInt32(tempBuf, 16 + offset), compressCoeff));
                            pbuffScanA[index] = tempPixA;
                            pbuffScanB[index] = tempPixB;
                            pbuffScanC[index] = tempPixC;
                        }
                        if (RemainingTime != _ScanSettings.Time - _ScanSettings.Time * coordY / _ScanSettings.Height)
                        {
                            RemainingTime = _ScanSettings.Time - _ScanSettings.Time * coordY / _ScanSettings.Height;
                            OnScanDataReceived?.Invoke("RemainingTime");
                        }
                    }
                }
            }
            #endregion Step4: Reading frames
        }

        private void XAxisScanProcess()
        {
            byte[] tempBuf = new byte[PACKSIZE];
            int index = 0;
            SampleIndex = new int[_ScanSettings.LineCounts];
            SampleValueChannelA = new int[_ScanSettings.LineCounts];
            SampleValueChannelB = new int[_ScanSettings.LineCounts];
            SampleValueChannelC = new int[_ScanSettings.LineCounts];
            if (PresetMotion(60000) == false)       // wait 1 minutes for the motions to be at the starting positions
            {
                ExitStat = ThreadExitStat.Error;
                throw new Exception("Failed to move the motions to the start positions");
            }
            _CommController.ReceivingBuf.Reset();

            int singleTrip = _ScanSettings.ScanDeltaX;
            double singleTripTime = _ScanSettings.Quality / 2.0 - _ScanSettings.XmotionTurnAroundDelay / 1000.0;
            //double singleTripTime = _ScanSettings.Quality / 2.0;
            int xMotorSpeed = XMotorSpeedCalibration.GetSpeed(_ScanSettings.XMotionAccVal, 256, singleTrip, singleTripTime);
            if (_ScanSettings.HorizontalCalibrationSpeed > 0)
            {
                xMotorSpeed = _ScanSettings.HorizontalCalibrationSpeed;
            }
            int tgtPosX1 = _ScanSettings.ScanX0 + _ScanSettings.ScanDeltaX;
            int tgtPosX2 = _ScanSettings.ScanX0;
            int repeats = 50000;
            _MotionController.AbsoluteMoveSingleMotion(MotorTypes.X, 256, xMotorSpeed, _ScanSettings.XMotionAccVal,
                _ScanSettings.XMotionDccVal, tgtPosX1, tgtPosX2, repeats, _ScanSettings.XmotionTurnAroundDelay, false, false);
            //_MotionController.AutoQuery = false;
            _CommController.TriggerXScan((uint)_ScanSettings.ScanDeltaX, (uint)_ScanSettings.DataRate);
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
                        if (index >= _ScanSettings.LineCounts)
                        {
                            return;
                        }
                        SampleIndex[index] = index * _ScanSettings.DataRate;
                        SampleValueChannelA[index] = BitConverter.ToInt32(tempBuf, 8 + i * 16);
                        SampleValueChannelB[index] = BitConverter.ToInt32(tempBuf, 12 + i * 16);
                        SampleValueChannelC[index] = BitConverter.ToInt32(tempBuf, 16 + i * 16);
                    }
                }
            }
        }

        public bool PresetMotion(int timeout)
        {
            bool isThere = true;
            if (timeout < 500)
            {
                timeout = 500;
            }
            bool result;
            int tryCnts = 0;
            int tgtPosX = _ScanSettings.ScanX0;
            if (!_ScanSettings.IsFrockTest)//当是工装测试时不需要调用XY轴电机。
            {
                do
                {
                    if (++tryCnts > 5)
                    {
                        return false;
                    }
                    if (ScanType == TestJigScanTypes.Horizontal)
                    {
                        tgtPosX = _ScanSettings.ScanX0 - (int)Math.Round(_ScanSettings.XMotionExtraMoveLength * _ScanSettings.XMotorSubdivision);
                    }
                    result = _MotionController.AbsoluteMoveSingleMotion(MotorTypes.X,
                        256, _ScanSettings.XMotorSpeed, _ScanSettings.XMotionAccVal, _ScanSettings.XMotionAccVal, tgtPosX, true, false);
                } while (result == false);

                tryCnts = 0;
                do
                {
                    if (++tryCnts > 5)
                    {
                        return false;
                    }
                    result = _MotionController.AbsoluteMoveSingleMotion(MotorTypes.Y,
                        256, _ScanSettings.YMotorSpeed, _ScanSettings.YMotionAccVal, _ScanSettings.YMotionAccVal, _ScanSettings.ScanY0, true, false);
                } while (result == false);
            }
            tryCnts = 0;
            do
            {
                if (++tryCnts > 5)
                {
                    return false;
                }
                result = _MotionController.AbsoluteMoveSingleMotion(MotorTypes.Z,
                    256, _ScanSettings.ZMotorSpeed, _ScanSettings.ZMotionAccVal, _ScanSettings.ZMotionDccVal, _ScanSettings.ScanZ0, true, false);
            } while (result == false);

            do
            {

                if (timeout < 0)
                {
                    return false;
                }

                isThere = true;
                if (!_ScanSettings.IsFrockTest)//当是工装测试时不需要调用XY轴点击。
                {
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
                    else if (_MotionController.CrntPositions[MotorTypes.Y] != _ScanSettings.ScanY0)
                    {
                        isThere = false;
                    }
                }
                if (_MotionController.CrntState[MotorTypes.Z].IsBusy)
                {
                    isThere = false;
                }
                else if (_MotionController.CrntPositions[MotorTypes.Z] != _ScanSettings.ScanZ0)
                {
                    isThere = false;
                }

                if (isThere)
                {
                    return true;
                }
                else
                {
                    Thread.Sleep(500);
                    timeout -= 500;
                }
            }
            while (isThere == false);
            return true;
        }

        public void ThresholdProcess(System.Windows.Point[] rawArray, out double positionOfMinValue, out double valueMin)
        {

            //`如果是小大小，就是波峰，大小大就是波谷
            //波峰比附近的值都大，波谷比附近的值都小。。。
            System.Windows.Point Data = new System.Windows.Point();
            List<System.Windows.Point> Datalist = new List<System.Windows.Point>();
            int direction = rawArray[0].Y > 0 ? -1 : 1;
            //找到所有峰值
            for (int i = 0; i < rawArray.Length - 1; i++)
            {
                if ((rawArray[i + 1].Y - rawArray[i].Y) * direction > 0)
                {
                    direction *= -1;
                    if (direction == 1)
                    {
                        Data.X = rawArray[i].X;
                        Data.Y = rawArray[i].Y;
                        Datalist.Add(Data);
                        //Console.WriteLine("(" + rawArray[i].X + "," + rawArray[i].Y + ")" + "波峰");
                        //获取数据中多个波峰
                    }
                    //else
                    //{
                    //    positionOfMinValue = rawArray[i].X;
                    //    valueMin = rawArray[i].Y;
                    //    Console.WriteLine("(" + rawArray[i].X + "," + rawArray[i].Y + ")" + "波谷");
                    //    //获取数据中多个波谷
                    //}
                }
            }

            //找到两个最大峰值的下标
            double _positionOfMinValue = 0;
            double _valueMin = 65535;
            double larges = 0;
            double second = 0;
            double largestIndex = 0;
            double secondIndex = 0;

            foreach (System.Windows.Point i in Datalist)
            {
                if (larges < i.Y)
                {
                    larges = i.Y;
                    largestIndex = i.X;
                    Data = i;
                }
            }
            Datalist.Remove(Data);
            foreach (System.Windows.Point i in Datalist)
            {
                if (second < i.Y)
                {
                    second = i.Y;
                    secondIndex = i.X;
                    Data = i;
                }
            }
            if (Math.Abs(secondIndex - largestIndex) < 0.3)
            {
                Datalist.Remove(Data);
                foreach (System.Windows.Point i in Datalist)
                {
                    if (second < i.Y)
                    {
                        second = i.Y;
                        secondIndex = i.X;
                        Data = i;
                    }
                }
            }
            if (Math.Abs(secondIndex - largestIndex) < 0.3)
            {
                Datalist.Remove(Data);
                foreach (System.Windows.Point i in Datalist)
                {
                    if (second < i.Y)
                    {
                        second = i.Y;
                        secondIndex = i.X;
                        Data = i;
                    }
                }
            }
            if (Math.Abs(secondIndex - largestIndex) < 0.3)
            {
                Datalist.Remove(Data);
                foreach (System.Windows.Point i in Datalist)
                {
                    if (second < i.Y)
                    {
                        second = i.Y;
                        secondIndex = i.X;
                        Data = i;
                    }
                }
            }
            if (largestIndex < secondIndex)  //最大波峰在前
            {
                //找寻这两个位置之间最低的值
                for (int i = 0; i < rawArray.Length - 1; i++)
                {

                    if (rawArray[i].X > largestIndex && rawArray[i].X < secondIndex)
                    {
                        //if (_valueMin >= rawArray[i].Y)
                        //{
                        //    _valueMin = rawArray[i].Y;
                        //    _positionOfMinValue = rawArray[i].X;
                        //Console.WriteLine("X :" + _positionOfMinValue);
                        //Console.WriteLine("Y : " + _valueMin);
                        //}
                        Console.WriteLine("X: " + rawArray[i].X + "  Y: " + rawArray[i].Y);
                    }
                }

                positionOfMinValue = _positionOfMinValue;
                valueMin = _valueMin;
                return;
            }
            if (secondIndex > largestIndex)
            {
                //找寻这两个位置之间最低的值
                for (int i = 0; i < rawArray.Length - 1; i++)
                {

                    if (rawArray[i].X > secondIndex && rawArray[i].X < largestIndex)
                    {
                        if (_valueMin >= rawArray[i].Y)
                        {
                            _valueMin = rawArray[i].Y;
                            _positionOfMinValue = rawArray[i].X;
                        }
                        //Console.WriteLine("X: "+ rawArray[i].X+"  Y: "+rawArray[i].Y);
                    }
                }

                positionOfMinValue = _positionOfMinValue;
                valueMin = _valueMin;
                return;
            }
            positionOfMinValue = _positionOfMinValue;
            valueMin = _valueMin;
        }

        public void ThresholdProcess(System.Windows.Point[] rawArray, int thresholdPercent, out double positionOfMaxValue, out double valueMax)
        {
            //double posOfThresholdLeft = 0;
            //double posOfThresholdRight = 0;
            double maxValue = 0;
            double minValue = 1e8;
            double thresholdValue = 0;
            int indexMaxValue = 0;
            int indexLeft = 0;
            int indexRight = 0;
            // find index of max, min values and index of max value
            for (int i = 0; i < rawArray.Length; i++)
            {
                if (rawArray[i].Y > maxValue)
                {
                    maxValue = rawArray[i].Y;
                    indexMaxValue = i;
                }
                if (rawArray[i].Y < minValue)
                {
                    minValue = rawArray[i].Y;
                }
            }
            // calculate threshold value
            thresholdValue = (maxValue - minValue) * thresholdPercent * 0.01 + minValue;
            // find left position of threshold value
            for (int i = indexMaxValue - 1; i >= 0; i--)
            {
                if (rawArray[i].Y <= thresholdValue)
                {
                    if (rawArray[i].Y == rawArray[i + 1].Y)
                    {
                        posOfThresholdLeft = rawArray[i].X;
                    }
                    else
                    {
                        posOfThresholdLeft = (thresholdValue - rawArray[i].Y) / (rawArray[i + 1].Y - rawArray[i].Y) * (rawArray[i + 1].X - rawArray[i].X) + rawArray[i].X;
                    }
                    indexLeft = i;
                    break;
                }
            }
            // find right position of threshold value
            for (int i = indexMaxValue + 1; i < rawArray.Length; i++)
            {
                if (rawArray[i].Y <= thresholdValue)
                {
                    if (rawArray[i].Y == rawArray[i - 1].Y)
                    {
                        posOfThresholdRight = rawArray[i].X;
                    }
                    else
                    {
                        posOfThresholdRight = (thresholdValue - rawArray[i].Y) / (rawArray[i - 1].Y - rawArray[i].Y) * (rawArray[i - 1].X - rawArray[i].X) + rawArray[i].X;
                    }
                    indexRight = i;
                    break;
                }
            }
            // calculate the real position of max value
            positionOfMaxValue = (posOfThresholdLeft + posOfThresholdRight) / 2;
            valueMax = 0;
            // find the value of the real position
            for (int i = indexLeft + 1; i < indexRight; i++)
            {
                if (positionOfMaxValue <= rawArray[i].X)
                {
                    valueMax = (positionOfMaxValue - rawArray[i - 1].X) / (rawArray[i].X - rawArray[i - 1].X) * (rawArray[i].Y - rawArray[i - 1].Y) + rawArray[i - 1].Y;
                    break;
                }
            }
        }

        static public void FilterProcess(ref int[] rawArray, int meanSize)
        {
            int totals = 0;
            int halfSize = meanSize / 2;
            for (int i = 0; i < rawArray.Length; i++)
            {
                if (i < halfSize)
                {
                    totals = totals + rawArray[i];
                }
                else if (i == halfSize)
                {
                    for (int j = 1; j < halfSize + 1; j++)
                    {
                        totals = totals + rawArray[j + halfSize];
                    }

                    rawArray[i] = (ushort)(totals / (meanSize - 1));
                }
                else if (i < rawArray.Length - halfSize)
                {
                    totals = totals - rawArray[i] + rawArray[i - 1] - rawArray[i - halfSize - 1] + rawArray[i + halfSize];
                    rawArray[i] = (ushort)(totals / (meanSize - 1));
                }
            }
        }


    }

    public class TestJigScanParameterStruct
    {
        public int Width { get; set; }
        public int Height { get; set; }
        public int XMotorSpeed { get; set; }
        public int YMotorSpeed { get; set; }
        public int ZMotorSpeed { get; set; }
        public int ScanX0 { get; set; }
        public int ScanY0 { get; set; }
        public int ScanZ0 { get; set; }
        public int ScanDeltaX { get; set; }
        public int ScanDeltaY { get; set; }
        public int ScanDeltaZ { get; set; }
        public int Res { get; set; }
        public int Quality { get; set; }
        public int DataRate { get; set; }
        public int LineCounts { get; set; }
        public int Time { get; set; }
        public double XMotorSubdivision { get; set; }
        public double YMotorSubdivision { get; set; }
        public double ZMotorSubdivision { get; set; }
        public double XEncoderSubdivision { get; set; }
        /// <summary>
        /// new firmware is version after 2.0.1.1 (included 2.0.1.1);
        /// new firmware takes control of X,Y,Z motion;
        /// </summary>
        public bool IsNewFirmwire { get; set; }
        /// <summary>
        /// Unit of msec
        /// 
        /// </summary>
        public int XmotionTurnAroundDelay { get; set; }
        /// <summary>
        /// Unit of mm
        /// </summary>
        public int XMotionExtraMoveLength { get; set; }
        public int XMotionAccVal { get; set; }
        public int YMotionAccVal { get; set; }
        public int ZMotionAccVal { get; set; }
        public int XMotionDccVal { get; set; }
        public int YMotionDccVal { get; set; }
        public int ZMotionDccVal { get; set; }
        public int DynamicBits { get; set; }
        public int HorizontalCalibrationSpeed { get; set; }
        public bool DynamicBitsAt { get; set; }
        public bool ScanPreciseAt { get; set; }
        public int ZScanValueThreshold { get; set; }
        public bool IsGlassScan { get; set; }
        public bool IsFrockTest { get; set; }
        public string singeName { get; set; }
        public int BackBufferStride { get; set; }
        public bool IsLightGainModule { get; set; }
        public int LightGain { get; set; }
        public int ScanPreciseParameter { get; set; }
    }

}
