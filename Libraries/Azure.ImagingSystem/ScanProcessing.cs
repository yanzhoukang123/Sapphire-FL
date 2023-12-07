using Azure.EthernetCommLib;
using Azure.MotionLib;
using Azure.CommandLib;
using Azure.Image.Processing;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Media.Imaging;

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
    public class ScanProcessing : ThreadBase
    {
        public delegate void ReceivedScanDataHandle(string dataName);
        public event ReceivedScanDataHandle OnScanDataReceived;

        public delegate void SpentTimeReceivedScanDataHandle();
        public event SpentTimeReceivedScanDataHandle OnSpentTimeScanDataReceived;

        #region Private Fields
        private EthernetController _CommController;
        private MotionController _MotionController;
        private ScanParameterStruct _ScanSettings;
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
        public ImageInfo ImageInfo { get; private set; }
        public ScanTypes ScanType { get; private set; }
        //public WriteableBitmap ChannelAImage { get; private set; }
        //public WriteableBitmap ChannelBImage { get; private set; }
        //public WriteableBitmap ChannelCImage { get; private set; }
        public int RemainingTime { get; private set; }
        public int XCalibratingSpeed { get; set; }
        public string ApplicationDataPath { get; set; }
        #endregion Public Properties

        public ScanProcessing(EthernetController ethernet, MotionController motion, ScanParameterStruct scanParameter, IntPtr[] Channel)
        {
            _CommController = ethernet;
            _MotionController = motion;
            _ScanSettings = scanParameter;
            if (Channel.Length > 0)
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
                ScanType = ScanTypes.Static;
                StaticScanProcess();
            }
            else if (_ScanSettings.ScanDeltaX == 0 && _ScanSettings.ScanDeltaY == 0)
            {
                ScanType = ScanTypes.Vertical;
                VerticalScanProcess();
            }
            else if (_ScanSettings.ScanDeltaX > 0 && _ScanSettings.ScanDeltaY > 0)
            {
                ScanType = ScanTypes.Horizontal;
                HorizontalScanProcess();
            }
            else if (_ScanSettings.ScanDeltaX > 0 && _ScanSettings.DataRate > 0)
            {
                ScanType = ScanTypes.XAxis;
                XAxisScanProcess();
            }
        }

        public override void AbortWork()
        {
            _CommController.StopScan();
        }

        public override void Finish()
        {
            _CommController.StopScan();
            //_CommController.SetLaserPower(LaserChannels.ChannelA, 0);
            //_CommController.SetLaserPower(LaserChannels.ChannelB, 0);
            //_CommController.SetLaserPower(LaserChannels.ChannelC, 0);
            //_MotionController.AutoQuery = true;
            _MotionController.SetStart(MotorTypes.X | MotorTypes.Y | MotorTypes.Z, new bool[] { false, false, false });

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
            _CommController.ReceivingBuf.Reset();
            int zPulses = _ScanSettings.ScanDeltaZ + 80 * 8;    // go more 80 sample points to finish all data transport
            _MotionController.AbsoluteMoveSingleMotion(MotorTypes.Z, 256, _ScanSettings.ZMotorSpeed, _ScanSettings.ZMotionAccVal, _ScanSettings.ZMotionDccVal, zPulses, true, false);
            //_MotionController.AutoQuery = false;
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
            //double singleTripTime = _ScanSettings.Quality / 2.0 - _ScanSettings.XmotionTurnAroundDelay / 1000.0;
            double singleTripTime = _ScanSettings.Quality / 2.0;
            //Calculate the X-axis velocity
            int xMotorSpeed = XMotorSpeedCalibration.GetSpeed(_ScanSettings.XMotionAccVal, 256, singleTrip, singleTripTime);
            //when the motor is ready,switch APD on
            //Calculate the Y-axis velocity
            int speedY = 0;
            if (_ScanSettings.IsUnidirectionalScan)
            {
                speedY = (int)Math.Round(((double)_ScanSettings.Res * _ScanSettings.YMotorSubdivision / _ScanSettings.Quality / 1000));
            }
            else
            {
                speedY = (int)Math.Round((double)_ScanSettings.Res * 2 * _ScanSettings.YMotorSubdivision / _ScanSettings.Quality / 1000);
            }
      
            int tgtPosY = _ScanSettings.ScanY0 + _ScanSettings.ScanDeltaY + (int)Math.Round(3 * _ScanSettings.YMotorSubdivision);
            int tgtPosX1 = _ScanSettings.ScanX0 + _ScanSettings.ScanDeltaX + (int)Math.Round(extraMove * _ScanSettings.XMotorSubdivision);
            int tgtPosX2 = _ScanSettings.ScanX0 - (int)Math.Round(extraMove * _ScanSettings.XMotorSubdivision);
            int repeats = _ScanSettings.Height / 2 + 10;
            int bufferWidth = _ScanSettings.BackBufferStride / 2;
            int Length = bufferWidth * _ScanSettings.Height;
            //int IsUnidirectiona = 0;
            if (_ScanSettings.IsUnidirectionalScan)
            {
                // The motion speed of Y axis will be twice slower, so the motion distance is the same, dy is the height of the pixel * 2
                tgtPosY = _ScanSettings.ScanY0 + (_ScanSettings.ScanDeltaY * 2) + (int)Math.Round(3 * _ScanSettings.YMotorSubdivision);
                // X Number of motor round trips
                repeats = _ScanSettings.Height + 10;
                //Create an image based on actual height
                _APDChannelA = new ushort[bufferWidth * _ScanSettings.Height / 2];
                _APDChannelB = new ushort[bufferWidth * _ScanSettings.Height / 2];
                _APDChannelC = new ushort[bufferWidth * _ScanSettings.Height / 2];
            }
            else
            {
                _APDChannelA = new ushort[bufferWidth * _ScanSettings.Height];
                _APDChannelB = new ushort[bufferWidth * _ScanSettings.Height];
                _APDChannelC = new ushort[bufferWidth * _ScanSettings.Height];
            }
            //Set X and Y axis speed, start and stop position
            _MotionController.AbsoluteMoveSingleMotion(MotorTypes.Y, 256, speedY, _ScanSettings.YMotionAccVal, _ScanSettings.YMotionDccVal, tgtPosY, false, false);
            _MotionController.AbsoluteMoveSingleMotion(MotorTypes.X, 256, xMotorSpeed, _ScanSettings.XMotionAccVal,
                _ScanSettings.XMotionDccVal, tgtPosX1, tgtPosX2, repeats, (int)(singleTripTime * 1000), false, false);
            //_MotionController.AutoQuery = false;
            #endregion Step1: Set Scanning Motion Parameters

            #region Step2: Set ImageInfo
            DateTime dateTime = DateTime.Now;
            ImageInfo = new Azure.Image.Processing.ImageInfo();
            ImageInfo.DateTime = System.String.Format("{0:G}", dateTime.ToString());
            ImageInfo.CaptureType = "Scanner";
            ImageInfo.IsScannedImage = true;
            ImageInfo.ScanResolution = _ScanSettings.Res;
            // 2023/02/14: 'Quality' in ImageInfo refers to unidirectional (or 2-line average) ON/OFF (NOT scan speed)
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

            uint resToEncoderCount = (uint)Math.Round((double)_ScanSettings.Res * _ScanSettings.XEncoderSubdivision / 1000.0);
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
                double pixelSampleDuration = (double)_ScanSettings.Res * 0.001 / (xMotorSpeed / _ScanSettings.XMotorSubdivision);
                double pixelSamplePoints = pixelSampleDuration * adcSampleRate;
                int samplePointsWidth = (int)Math.Log(pixelSamplePoints, 2);
                _ScanSettings.DynamicBits = samplePointsWidth;
                ImageInfo.DynamicBit = 16 + samplePointsWidth;
            }
            else
            {
                ImageInfo.DynamicBit = 16;
                _ScanSettings.DynamicBits = 0;
            }
            _CommController.TriggerHorizontalScan(deltaXEncoder, (uint)_ScanSettings.Height, resToEncoderCount, (uint)(Math.Pow(2, _ScanSettings.DynamicBits)));
            _MotionController.SetStart(MotorTypes.X | MotorTypes.Y, new bool[] { true, true });
            OnSpentTimeScanDataReceived?.Invoke();
            #endregion Step3: Start Scanning

            #region Step4: Reading frames
            int offset = 0;
            ushort avgPixelA = 0;
            ushort avgPixelB = 0;
            ushort avgPixelC = 0;
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
                        //Must use Thread.Sleep(1) to force a context switch (to avoid using 100% of the CPU) 
                        // http://joeduffyblog.com/2006/08/22/priorityinduced-starvation-why-sleep1-is-better-than-sleep0-and-the-windows-balance-set-manager/
                        Thread.Sleep(1);
                    }
                    _CommController.ReceivingBuf.ReadDataOut(tempBuf, 0, PACKSIZE);
                    //Determine the start byte of data
                    if (tempBuf[0] == 0x6b && tempBuf[1285] == 0x6f)
                    {
                        for (int i = 0; i < FramesPerPackage; i++)
                        {
                            offset = i * 16;
                            coordY = (ushort)(BitConverter.ToUInt16(tempBuf, 4 + offset) - 1);
                            coordX = (ushort)(BitConverter.ToUInt16(tempBuf, 6 + offset) - 1);
                            if (!_ScanSettings.IsUnidirectionalScan)
                            {
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
                                _APDChannelA[index] = tempPixA;
                                _APDChannelB[index] = tempPixB;
                                _APDChannelC[index] = tempPixC;
                            }
                            else
                            {
                               if (coordY >= _ScanSettings.Height)
                                {
                                    RemainingTime = 0;
                                    OnScanDataReceived?.Invoke("RemainingTime");
                                    return;
                                }
                                //Previously, we have changed the image height * 2
                                //coordY /= 2;
                                //index = coordX + coordY * bufferWidth;
                                //Actual location of image data
                                index = coordX + (coordY / 2) * bufferWidth;
                                if (index >= Length)
                                {
                                    throw new Exception(string.Format("Coordinates Error, X={0},Y={1}", coordX, coordY));
                                }

                                bool isEvenLine = coordY % 2 == 0;
                                if (!isEvenLine)
                                {
                                    int tempPixA = _APDChannelA[index] + (ushort)(Math.Pow(BitConverter.ToUInt32(tempBuf, 8 + offset), compressCoeff));
                                    int tempPixB = _APDChannelB[index] + (ushort)(Math.Pow(BitConverter.ToUInt32(tempBuf, 12 + offset), compressCoeff));
                                    int tempPixC = _APDChannelC[index] + (ushort)(Math.Pow(BitConverter.ToUInt32(tempBuf, 16 + offset), compressCoeff));
                                    avgPixelA = (ushort)(tempPixA / 2);
                                    avgPixelB = (ushort)(tempPixB / 2);
                                    avgPixelC = (ushort)(tempPixC / 2);
                                    pbuffScanA[index] = avgPixelA;
                                    pbuffScanB[index] = avgPixelB;
                                    pbuffScanC[index] = avgPixelC;
                                    _APDChannelA[index] = avgPixelA;
                                    _APDChannelB[index] = avgPixelB;
                                    _APDChannelC[index] = avgPixelC;
                                }
                                else //odd
                                {
                                    //record odd lines
                                    ushort tempPixA = (ushort)(Math.Pow(BitConverter.ToUInt32(tempBuf, 8 + offset), compressCoeff));
                                    ushort tempPixB = (ushort)(Math.Pow(BitConverter.ToUInt32(tempBuf, 12 + offset), compressCoeff));
                                    ushort tempPixC = (ushort)(Math.Pow(BitConverter.ToUInt32(tempBuf, 16 + offset), compressCoeff));
                                    pbuffScanA[index] = avgPixelA;
                                    pbuffScanB[index] = avgPixelB;
                                    pbuffScanC[index] = avgPixelC;
                                    _APDChannelA[index] = tempPixA;
                                    _APDChannelB[index] = tempPixB;
                                    _APDChannelC[index] = tempPixC;
                                }

                            }
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
            RemainingTime = _ScanSettings.DataRate * _ScanSettings.LineCounts / 1000;
            OnScanDataReceived?.Invoke("RemainingTime");
            OnScanDataReceived?.Invoke("OnXRemainingTime");
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

        private bool PresetMotion(int timeout)
        {


            bool isThere = true;
            if (timeout < 500)
            {
                timeout = 500;
            }
            bool result;
            int tryCnts = 0;
            int tgtPosX = _ScanSettings.ScanX0;
            do
            {
                if (++tryCnts > 5)
                {
                    return false;
                }
                if (ScanType == ScanTypes.Horizontal)
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
    }

    /*public class ScanParameterStruct
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
        public int BackBufferStride { get; set; }
        public bool IsUnidirectionalScan { get; set; }
    }*/

}
