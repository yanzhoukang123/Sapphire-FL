using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Azure.CommandLib;
using System.Threading;
//using System.Windows.Forms;
using System.Windows;
using Hywire.DataProcessing.Methods;

namespace Azure.APDCalibrationBench
{
    public class APDCalibrationProcess : ThreadBase
    {
        #region delegates statement
        public delegate void CalibrationStepCountsHandler();
        public event CalibrationStepCountsHandler UpdateStepCounts;

        public delegate void CalibrationRecordsHandler(APDCommProtocol.APDChannelType channel, int gainIndex, double APDOutput, double calibrationVolt, double calibrationTemper);
        public event CalibrationRecordsHandler UpdateRecordsAtGains;

        public delegate void CalibrationVerificationHandler(APDCommProtocol.APDChannelType channel, bool isReverseVerification, int gainIndex, double APDOutput);
        public event CalibrationVerificationHandler UpdateVerificationAtGains;

        public delegate void CalibrationFittedLineHandler(APDCommProtocol.APDChannelType channel, bool isReverseVerification, string line);
        public event CalibrationFittedLineHandler UpdateFittedLine;

        public delegate MessageBoxResult ShowMessageBoxHandler(string message, string title, MessageBoxButton button, MessageBoxImage image);
        public event ShowMessageBoxHandler ShowMessageBox;
        #endregion

        #region Private Data
        private int _CalibrationStepCounts = 0;
        private APDCalibrationPort _CalibrationPort = null;
        private CalibrationParameterStruct _CalibrationParameters = null;
        private CalibrationRecords _CalibrationRecords = null;

        private double _DarkCurrentAdjustLimitH = 0;
        private double _DarkCurrentAdjustLimitL = 0;
        private double _APDOutputAtG0 = 0;
        private double _APDOutputAtG0Range = 0;
        private int _APDPGA = 0;
        private readonly string _MsgHighVoltUnstable = "APD高压故障！";
        private readonly string _MsgTemperSensorError = "温度传感器故障";
        private readonly string _MsgHighVoltOver = "APD高压过高！";
        private readonly string _MsgIVCommError = "IV板通讯错误，请检查标定参数和线缆连接！";
        private readonly string _MsgBenchCommError = "标定板通讯错误，请检查线缆连接！";
        private readonly string _MsgOpeningLaser = "即将打开激光，";
        private int loop = 150;
        #endregion

        public class CalibrationParameterStruct
        {
            public int? APDWaveLengthChA;
            public double? CalibrationVoltageChA;
            public double? BreakdownVoltageChA;
            public int? CalibrationGainChA;
            public double? CalibrationTemperatureChA;
            public double? TemperatureCoeffChA;

            public int? APDWaveLengthChB;
            public double? CalibrationVoltageChB;
            public double? BreakdownVoltageChB;
            public int? CalibrationGainChB;
            public double? CalibrationTemperatureChB;
            public double? TemperatureCoeffChB;

            public bool IsCHASelected;
            public bool IsCHBSelected;

            public double DarkCurrentAdjustLimitH;
            public double DarkCurrentAdjustLimitL;
            public double APDOutputAtG0;
            public double APDOutputAtG0Range;
            public int APDPGA;

            public int APDOutputStableLongTime;
            public int APDOutputStableShortTime;
            public string IVNum = "";
            public int APDType = 0;
        }

        #region Constructor
        public APDCalibrationProcess(
            APDCalibrationPort apdCalibrationPort,
            CalibrationParameterStruct calibrationParameters,
            CalibrationRecords calibrationRecords)
        {
            _CalibrationPort = apdCalibrationPort;
            _CalibrationParameters = calibrationParameters;
            _CalibrationRecords = calibrationRecords;

            _DarkCurrentAdjustLimitH = _CalibrationParameters.DarkCurrentAdjustLimitH;
            _DarkCurrentAdjustLimitL = _CalibrationParameters.DarkCurrentAdjustLimitL;
            _APDOutputAtG0 = _CalibrationParameters.APDOutputAtG0;
            _APDOutputAtG0Range = _CalibrationParameters.APDOutputAtG0Range;
            _APDPGA = _CalibrationParameters.APDPGA;

        }
        #endregion
        
        #region Thread Functions
        public override void ThreadFunction()
        {
            MessageBoxResult messageBoxResult = MessageBoxResult.None;
            MessageBoxResult _ConfirmResult = new MessageBoxResult();
            int rewriteCounter = 0;
            if (_CalibrationPort == null)
            {
                return;
            }
            else if (!_CalibrationPort.IsConnected)
            {
                return;
            }
            rewriteCounter = 0;
            string channelString = string.Format("{0}",
                _CalibrationParameters.IsCHASelected ? "通道" : "", _CalibrationParameters.IsCHASelected);
            #region Step 1: Set gain to G0
            if (_CalibrationParameters.IsCHASelected)
            {     //将“APD增益”写入100

                rewriteCounter = 0;
                do
                {
                    if (rewriteCounter++ >= 100)
                    {
                        _ConfirmResult = MessageBox.Show("Step 1:" + _MsgIVCommError, "通讯失败", MessageBoxButton.YesNo);
                        if (_ConfirmResult == MessageBoxResult.No)
                        {
                            ExitStat = ThreadExitStat.Error;
                            return;
                        }
                        else
                        {
                            MessageBoxResult faultBoxResult = MessageBoxResult.None;
                            faultBoxResult = MessageBox.Show("故障是否排除!", "通讯失败", MessageBoxButton.YesNo);
                            if (faultBoxResult == MessageBoxResult.No)
                            {
                                ExitStat = ThreadExitStat.Error;
                                return;

                            }
                            ThreadFunction();
                        }
                    }
                    _CalibrationPort.APDGainCHA = _CalibrationParameters.CalibrationGainChA;
                    Thread.Sleep(1000);
                }
                while (_CalibrationPort.APDGainCHA != _CalibrationParameters.CalibrationGainChA);


                //写入APD编号，IV类型
                //do
                //{
                //    rewriteCounter++;
                //    if (rewriteCounter++ >= loop)
                //    {
                //        ShowMessageBox.Invoke(_MsgIVCommError + "_激光连续10次写入和读取不一致", "通讯失败", MessageBoxButton.OK, MessageBoxImage.Stop);
                //        ExitStat = ThreadExitStat.Error;
                //        return;
                //    }
                //    _CalibrationPort.IVNum = _CalibrationParameters.IVNum;
                //    Thread.Sleep(1000);
                //    _CalibrationPort.IVType = _CalibrationParameters.APDType;
                //    Thread.Sleep(1000);
                //    标定前将激光写为0
                //    _CalibrationPort.LaserCHA = 0;
                //    Thread.Sleep(1000);
                //}
                //while (rewriteCounter < 2);
            }
            _CalibrationStepCounts = 1;
            UpdateStepCounts();
            #endregion

            #region step 2: adjust dark current
            if (_CalibrationParameters.IsCHASelected)
            {
                while (true)
                {
                    if (_CalibrationPort.APDHighVoltCHA > _CalibrationParameters.CalibrationVoltageChA-30)
                    {
                        break;
                    }
                    _CalibrationPort.CalibrationVoltCHA = _CalibrationParameters.CalibrationVoltageChA;
                    //Thread.Sleep(1000);
                    Thread.Sleep(2000);
                }
                rewriteCounter = 0;
                do
                {
                    if (rewriteCounter++ >= loop)
                    {
                        ShowMessageBox.Invoke("Step 2:" + _MsgIVCommError, "通讯失败", MessageBoxButton.OK, MessageBoxImage.Stop);
                        ExitStat = ThreadExitStat.Error;
                        return;
                    }
                    //Thread.Sleep(2000);
                    _CalibrationPort.PGACHA = 8;
                    //Thread.Sleep(2000);
                    Thread.Sleep(1000);
                }
                while (_CalibrationPort.PGACHA != 8);
            }
            #region wait for stable
            //Thread.Sleep(40000);
            Thread.Sleep(10000);
            double averValA, minValA, maxValA;
            if (_CalibrationParameters.IsCHASelected)
            {
                SampleAPDHighVolt(10, APDCommProtocol.APDChannelType.CHA, out averValA, out maxValA, out minValA);
                if ((maxValA - minValA) > 0.020)
                {
                    messageBoxResult = ShowMessageBox.Invoke(_MsgHighVoltUnstable, "高压不稳", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                    if (messageBoxResult == MessageBoxResult.No)
                    {
                        ExitStat = ThreadExitStat.Error;
                        return;
                    }
                }
            }

            #endregion wait for stable  ADC输入电压≈3mV
            string caption = "暗电流调节";
            string message = string.Format("请在暗环境下调整电位器使{0}的APD输出至{1}~{2}mV,然后点击确定继续下一步!",
                channelString, _DarkCurrentAdjustLimitL * 1000, _DarkCurrentAdjustLimitH * 1000);
            while (_CalibrationStepCounts == 1)
            {
                messageBoxResult = ShowMessageBox.Invoke(message, caption, MessageBoxButton.OKCancel, MessageBoxImage.Information);
                if (messageBoxResult == MessageBoxResult.Cancel)
                {
                    _CalibrationStepCounts = 0;
                    UpdateStepCounts();
                    ExitStat = ThreadExitStat.Abort;
                    return;
                }
                //Thread.Sleep(10000);        // wait for APD Output stable
                Thread.Sleep(5000);
                if (_CalibrationParameters.IsCHASelected)
                {
                    //ADC输入电压Vo在 0.003±0.002V范围内
                    if (_CalibrationPort.APDOutputCHA <= (_DarkCurrentAdjustLimitH+0.002) && _CalibrationPort.APDOutputCHA >= _DarkCurrentAdjustLimitL)
                    {
                        _CalibrationStepCounts = 2;
                    }
                    else
                    {
                        _CalibrationStepCounts = 1;
                        continue;
                    }
                }
            }
            UpdateStepCounts();
            #endregion

            #region step 3: set PGACHA =8
            if (_CalibrationParameters.IsCHASelected)
            {
                rewriteCounter = 0;
                do
                {
                    if (rewriteCounter++ >= loop)
                    {
                        ShowMessageBox.Invoke("Step 3:" + _MsgIVCommError, "通讯失败", MessageBoxButton.OK, MessageBoxImage.Stop);
                        ExitStat = ThreadExitStat.Error;
                        return;
                    }
                    //以后还需要将PGA改为1
                    //Thread.Sleep(2000);
                    _CalibrationPort.PGACHA = _APDPGA;
                    //Thread.Sleep(2000);
                    Thread.Sleep(1000);
                }
                while (_CalibrationPort.PGACHA != _APDPGA);
            }
            _CalibrationStepCounts = 3;
            UpdateStepCounts();
            #endregion

            #region step 4~5:  write "temperature at calibration", "Calibration Voltage at Gain G0"
            Thread.Sleep(60000);       // wait 2 minutes for temperature stability
            double _UtCHA = 0;
            double _HighVoltSetCHA = 0;
            bool _IsHighVoltageSetOKCHA = true;
            if (_CalibrationParameters.IsCHASelected)
            {
                _IsHighVoltageSetOKCHA = false;
                //将读到的Tc写入“光强 校准时温度”
                //光强校准时温度
                double? APDTemperature= _CalibrationPort.APDTemperatureCHA;
                _CalibrationPort.Caltemperature = APDTemperature;


                rewriteCounter = 0;
                do
                {
                    if (rewriteCounter++ >= loop)
                    {
                        ShowMessageBox.Invoke("Step 4~5:" + _MsgIVCommError + "_校准电压连续30次写入失败，请重新校准，", "写入失败", MessageBoxButton.OK, MessageBoxImage.Stop);
                        ExitStat = ThreadExitStat.Error;
                        return;
                    }
                    _CalibrationPort.TemperatureAtCalibrationCHA = APDTemperature;
                    Thread.Sleep(2000);
                    //Thread.Sleep(1000);
                }
                while (_CalibrationPort.TemperatureAtCalibrationCHA != APDTemperature);
                //通过公式Ut=U0+(Tc-T0)*Ct计算 此时对应的反向电压值Ut
                _UtCHA = (double)_CalibrationParameters.CalibrationVoltageChA +
                    (double)(_CalibrationParameters.TemperatureCoeffChA *
                    (_CalibrationPort.APDTemperatureCHA - _CalibrationParameters.CalibrationTemperatureChA));
                //将Ut计算值写入“100倍增益时 的校准电压”
                //Thread.Sleep(1000);
                Thread.Sleep(1000);
                _CalibrationPort.CalibrationVoltCHA = _UtCHA;
                //将Ut计算值写入“100倍增益时 的校准电压”两次赋值，避免赋值失败
                //Thread.Sleep(1000);
                Thread.Sleep(1000);
                _HighVoltSetCHA = _UtCHA;
            }
            //Ut-0.2 <= HV <= Ut+0.2
            //Thread.Sleep(10000);
            Thread.Sleep(5000);
            if (!_IsHighVoltageSetOKCHA)
            {
                //读取APD高压值HV
                while (true)
                {
                    if (_CalibrationPort.APDHighVoltCHA > (_UtCHA - 1))
                    {
                        break;
                    }
                    _CalibrationPort.CalibrationVoltCHA = _UtCHA;
                    //Thread.Sleep(3000);
                    Thread.Sleep(1000);
                }
                double _HighVoltDifCHA = (double)_CalibrationPort.APDHighVoltCHA - _UtCHA;
                if (Math.Abs(_HighVoltDifCHA) > 0.2)
                {
                    messageBoxResult = ShowMessageBox.Invoke(_MsgHighVoltUnstable, "提示", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                    if (messageBoxResult == MessageBoxResult.No)
                    {
                        ExitStat = ThreadExitStat.Error;
                        return;
                    }
                }
            }
            _CalibrationStepCounts = 5;
            UpdateStepCounts();
            #endregion

            #region step 6: adjust APD output to 0.5V
            double _APDOutputAtInitialGainCHA = 0;
            double _gainCompareRateCHA = 0;
            caption = "提示";
            message = string.Format(_MsgOpeningLaser + "使{0}的APD输出至{1}±{2}V",
                channelString, _APDOutputAtG0, _APDOutputAtG0Range);
            messageBoxResult = ShowMessageBox.Invoke(message, caption, MessageBoxButton.OKCancel, MessageBoxImage.Information);
            if (messageBoxResult == MessageBoxResult.Cancel)
            {
                ExitStat = ThreadExitStat.Abort;
                return;
            }
            //从2000开始逐步增大激光光电 流，直到ADC输入电压≈0.5V
            int laserCHA = 2000;
            double detaAPDOutputCHA = 0;
            rewriteCounter = 0;
            if (_CalibrationParameters.IsCHASelected)
            {
                do
                {
                    rewriteCounter++;
                    //if (rewriteCounter++ >= 2)
                    //{
                    //    ShowMessageBox.Invoke(_MsgIVCommError, "通讯失败", MessageBoxButton.OK, MessageBoxImage.Stop);
                    //    ExitStat = ThreadExitStat.Error;
                    //    return;
                    //}
                    _CalibrationPort.LaserCHA = laserCHA;
                    Thread.Sleep(1000);
                }
                while (rewriteCounter < 2);

            }
            Thread.Sleep(4000);

            bool isChAReady = !_CalibrationParameters.IsCHASelected;
            bool isCHARoughAdjust = true;
            int laserAdjOverCntCHA = 0;
            double averADCValA, minADCValA, maxADCValA;
            while (_CalibrationStepCounts < 6)
            {
                if (_CalibrationParameters.IsCHASelected)
                {
                    bool _SampleADC = true;
                    while (_SampleADC)
                    {
                        ////采集10秒的电压，电压小于0.02V
                        //SampleADCOutput(10, APDCommProtocol.APDChannelType.CHA, out averADCValA, out maxADCValA, out minADCValA);
                        //if (averADCValA > 0.02)
                        //{
                        //    messageBoxResult = ShowMessageBox.Invoke("APD输出电压大于0.02V，是否继续执行", "电压波动幅度较大", MessageBoxButton.OKCancel, MessageBoxImage.Information);
                        //    if (messageBoxResult == MessageBoxResult.Cancel)
                        //    {
                        //        _CalibrationStepCounts = 0;
                        //        UpdateStepCounts();
                        //        ExitStat = ThreadExitStat.Abort;
                        //        return;
                        //    }
                        //    else
                        //    {
                        //        _SampleADC = false;
                        //    }
                        //}
                        //else {

                            _SampleADC = false;
                        //}
                    }
                    if (!isChAReady)
                    {
                        detaAPDOutputCHA = (double)_CalibrationPort.APDOutputCHA - _APDOutputAtG0;
                        if (Math.Abs(detaAPDOutputCHA) < _APDOutputAtG0Range)
                        {
                            isChAReady = true;
                        }
                        else
                        {
                            isChAReady = false;
                            laserAdjOverCntCHA += detaAPDOutputCHA > 0 ? 1 : 0;
                            if (detaAPDOutputCHA < _APDOutputAtG0Range * -2.0 && isCHARoughAdjust)
                            {
                                laserCHA += 500;
                            }
                            else if (Math.Abs(detaAPDOutputCHA) > _APDOutputAtG0Range * 1.5)
                            {
                                double laseradiover = Math.Pow(0.8, laserAdjOverCntCHA);
                                int detaLaser = (int)(-3.0 * detaAPDOutputCHA * 200 * Math.Pow(0.8, laserAdjOverCntCHA));
                                int ab = Math.Abs(detaLaser);
                                if (Math.Abs(detaLaser) < 25)
                                {
                                    detaLaser = detaAPDOutputCHA < 0 ? 25 : -25;
                                }
                                laserCHA += detaLaser;
                                isCHARoughAdjust = false;
                            }
                            else
                            {
                                laserCHA += (detaAPDOutputCHA < 0 ? 1 : -1) * 25;
                                isCHARoughAdjust = false;
                            }
                            rewriteCounter = 0;
                            do
                            {
                                rewriteCounter++;
                                //if (rewriteCounter++ >= 2)
                                //{
                                //    ShowMessageBox.Invoke(_MsgIVCommError, "通讯失败", MessageBoxButton.OK, MessageBoxImage.Stop);
                                //    ExitStat = ThreadExitStat.Error;
                                //    return;
                                //}
                                _CalibrationPort.LaserCHA = laserCHA;
                                Thread.Sleep(1000);
                            }
                              while (rewriteCounter < 2);
                        }
                    }
                }
                if (isChAReady)
                {
                    _CalibrationStepCounts = 6;
                    continue;
                }
                else
                {
                    _CalibrationStepCounts = 5;
                }

                if (laserCHA > 14000 || laserCHA < 2000)
                {
                    ShowMessageBox.Invoke("激光调节失败！", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                    ExitStat = ThreadExitStat.Error;
                    return;
                }
                //Thread.Sleep(4000);
                Thread.Sleep(2000);
            }

            //Thread.Sleep(5000);   
            isChAReady = true;
            laserAdjOverCntCHA = 0;
            if (_CalibrationParameters.IsCHASelected)
            {
                isChAReady = false;
            }
            while (!isChAReady)
            {  //采集10秒输出电压
                bool _SampleADC = true;
                while (_SampleADC)
                {
                    //采集10秒的电压，电压小于0.02V
                    //SampleADCOutput(10, APDCommProtocol.APDChannelType.CHA, out averADCValA, out maxADCValA, out minADCValA);
                    //if (averADCValA > 0.02)
                    //{
                    //    messageBoxResult = ShowMessageBox.Invoke("APD输出电压大于0.02V，是否继续执行", "电压波动幅度较大", MessageBoxButton.OKCancel, MessageBoxImage.Information);
                    //    if (messageBoxResult == MessageBoxResult.Cancel)
                    //    {
                    //        _CalibrationStepCounts = 0;
                    //        UpdateStepCounts();
                    //        ExitStat = ThreadExitStat.Abort;
                    //        return;
                    //    }
                    //    else
                    //    {
                    //        _SampleADC = false;
                    //    }
                    //}
                    //else
                    //{

                        _SampleADC = false;
                    //}
                }
                if (!isChAReady)
                {
                    detaAPDOutputCHA = (double)_CalibrationPort.APDOutputCHA - _APDOutputAtG0;
                    if (Math.Abs(detaAPDOutputCHA) < _APDOutputAtG0Range)
                    {
                        isChAReady = true;
                    }
                    else
                    {
                        isChAReady = false;
                        laserAdjOverCntCHA += detaAPDOutputCHA > 0 ? 1 : 0;
                        if (Math.Abs(detaAPDOutputCHA) > _APDOutputAtG0Range * 1.5)
                        {
                            int detaLaser = (int)(-3.0 * detaAPDOutputCHA * 200 * Math.Pow(0.8, laserAdjOverCntCHA));
                            if (Math.Abs(detaLaser) < 25)
                            {
                                detaLaser = detaAPDOutputCHA < 0 ? 25 : -25;
                            }
                            laserCHA += detaLaser;
                            isCHARoughAdjust = false;
                        }
                        else
                        {
                            laserCHA += (detaAPDOutputCHA < 0 ? 1 : -1) * 25;
                            isCHARoughAdjust = false;
                        }
                        rewriteCounter = 0;
                        do
                        {
                            rewriteCounter++;
                            //if (rewriteCounter++ >= 2)
                            //{
                            //    ShowMessageBox.Invoke(_MsgIVCommError, "通讯失败", MessageBoxButton.OK, MessageBoxImage.Stop);
                            //    ExitStat = ThreadExitStat.Error;
                            //    return;
                            //}
                            _CalibrationPort.LaserCHA = laserCHA;
                            Thread.Sleep(1000);
                        }
                        while (rewriteCounter < 2);
                    }
                }
                if (laserCHA > 14000 || laserCHA < 2000)
                {
                    ShowMessageBox.Invoke("激光调节失败！", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                    ExitStat = ThreadExitStat.Error;
                    return;
                }
                //Thread.Sleep(4000);
                Thread.Sleep(2000);
            }

            //Thread.Sleep(5000);
            //将100增益标定完成记录标定值
            if (_CalibrationParameters.IsCHASelected)
            {
                _APDOutputAtInitialGainCHA = (double)_CalibrationPort.APDOutputCHA;//将100倍的标定成功的adc值记录下来
                for (int i = 0; i < _CalibrationRecords.CalibrationRecordsCHA.Count; i++)
                {
                    if (_CalibrationRecords.CalibrationRecordsCHA[i].APDGain == _CalibrationParameters.CalibrationGainChA)
                    {
                        _CalibrationRecords.CalibrationRecordsCHA[i].APDOutput = _CalibrationPort.APDOutputCHA;
                        _CalibrationRecords.CalibrationRecordsCHA[i].CalibVolt = _UtCHA;
                        _CalibrationRecords.CalibrationRecordsCHA[i].CalibTemper = _CalibrationPort.APDTemperatureCHA;
                        UpdateRecordsAtGains(
                            APDCommProtocol.APDChannelType.CHA,
                            i,
                            (double)_CalibrationRecords.CalibrationRecordsCHA[i].APDOutput,
                            (double)_CalibrationRecords.CalibrationRecordsCHA[i].CalibVolt,
                            (double)_CalibrationRecords.CalibrationRecordsCHA[i].CalibTemper);
                        break;
                    }
                }
            }
            UpdateStepCounts();
            #endregion

            #region step 7~15: calibrates gains except G0
            bool isInitialGainCHA = false;
            double _CurrentAPDOutputCHA = 0;
            double _CurrentAPDOutputTobeCHA = 0;
            double _APDOutputDiffPercentCHA = 0;
            double _highVoltageSetCHA = 0;
            double _highVoltSetCoeffCHA = 1;
            int _PreviousGainCHA = 0;
            if (_CalibrationParameters.IsCHASelected)
            {
                _highVoltageSetCHA = _UtCHA;
                _PreviousGainCHA = (int)_CalibrationParameters.CalibrationGainChA;
            }
            for (int _calibratingGainIndex = 0; _calibratingGainIndex < _CalibrationRecords.CalibrationRecordsCHA.Count; _calibratingGainIndex++)
            {
                // write calibration gain and corresponding calibration voltage
                int calibratingGainCHA = _CalibrationRecords.CalibrationRecordsCHA[_calibratingGainIndex].APDGain;
                //int calibratingGainCHB = _CalibrationRecords.CalibrationRecordsCHB[_calibratingGainIndex].APDGain;
                if (_CalibrationParameters.IsCHASelected)
                {
                    if (calibratingGainCHA == 150 || calibratingGainCHA == 250 )
                    {
                        continue;
                    }
                    if (calibratingGainCHA == _CalibrationParameters.CalibrationGainChA)
                    {
                        isInitialGainCHA = true;
                    }
                    else
                    {
                        isInitialGainCHA = false;
                        rewriteCounter = 0;
                        do
                        {
                            if (rewriteCounter++ >= loop)
                            {
                                ShowMessageBox.Invoke("Step 7~15:" + _MsgIVCommError, "通讯失败", MessageBoxButton.OK, MessageBoxImage.Stop);
                                ExitStat = ThreadExitStat.Error;
                                return;
                            }
                            //Thread.Sleep(2000);
                            _CalibrationPort.APDGainCHA = calibratingGainCHA;
                            Thread.Sleep(1000);
                        }
                        while (_CalibrationPort.APDGainCHA != calibratingGainCHA);
                        _gainCompareRateCHA = (double)_CalibrationPort.APDGainCHA / (double)_CalibrationParameters.CalibrationGainChA;//当前增益（50，200，300，400，500）除以100倍增益

                        double _gainDif = ((double)_CalibrationPort.APDGainCHA - _PreviousGainCHA) / _PreviousGainCHA;//当前增益倍数减去上一次倍增益倍数
                        _highVoltageSetCHA = _highVoltageSetCHA + 2.0 * _gainDif;//100倍高压加上2.0乘以-0.5
                        _CalibrationPort.CalibrationVoltCHA = _highVoltageSetCHA;
                        //读取APD高压值HV
                        while (true)
                        {
                            if (_CalibrationPort.APDHighVoltCHA > (_highVoltageSetCHA - 30))
                            {
                                break;
                            }
                            //Thread.Sleep(1000);
                            Thread.Sleep(1000);
                        }
                    }
                }
                // wait for APD Output stable after setting Gains: 20 seconds
                if ((_CalibrationParameters.IsCHASelected && !isInitialGainCHA))
                {
                    //Thread.Sleep(20000);
                    Thread.Sleep(10000);
                }
                // compare APD Output at current gain with at G0, adjust calibration voltage
                bool _IsCHAFinishAdjust = false;
                if (!_CalibrationParameters.IsCHASelected)
                {
                    _IsCHAFinishAdjust = true;
                }
                else if (isInitialGainCHA)
                {
                    _IsCHAFinishAdjust = true;
                }
                // change adjust coefficient, the larger the gain, the lower the coefficient
                if (!_IsCHAFinishAdjust)
                {
                    if (_CalibrationPort.APDGainCHA <= 50)
                    {
                        _highVoltSetCoeffCHA = 5.0;
                    }
                    else if (_CalibrationPort.APDGainCHA <= 150)
                    {
                        _highVoltSetCoeffCHA = 4.0;
                    }
                    else if (_CalibrationPort.APDGainCHA <= 250)
                    {
                        _highVoltSetCoeffCHA = 3.0;
                    }
                    else
                    {
                        _highVoltSetCoeffCHA = 0.8;
                    }
                }
                // circularly check the APD gain and fix it by adjusting the APD high voltage
                int adjustLoop = 0;
                while (!_IsCHAFinishAdjust)
                {
                    if (!_IsCHAFinishAdjust)
                    {
                        _CurrentAPDOutputCHA = GetAverageAPDOutput(APDCommProtocol.APDChannelType.CHA, 1);//获取apd输出的平均数
                        _CurrentAPDOutputTobeCHA = _APDOutputAtInitialGainCHA * _gainCompareRateCHA;
                        _APDOutputDiffPercentCHA = (_CurrentAPDOutputCHA - _CurrentAPDOutputTobeCHA) / _CurrentAPDOutputTobeCHA;//当前adc输出-100倍增益的adc输出 乘以（当前增益除以100倍增益）除以
                        if (Math.Abs(_APDOutputDiffPercentCHA) <= 0.02)     // allowed difference percent: ±2%
                        {
                            _IsCHAFinishAdjust = true;
                            _CalibrationRecords.CalibrationRecordsCHA[_calibratingGainIndex].APDOutput = _CurrentAPDOutputCHA;
                            _CalibrationRecords.CalibrationRecordsCHA[_calibratingGainIndex].CalibVolt = _highVoltageSetCHA;
                            _CalibrationRecords.CalibrationRecordsCHA[_calibratingGainIndex].CalibTemper = _CalibrationPort.APDTemperatureCHA;
                            if (_calibratingGainIndex == _CalibrationRecords.CalibrationRecordsCHA.Count)
                            {
                                _CalibrationPort.Temperature500 = _CalibrationPort.APDTemperatureCHA;
                            }
                            UpdateRecordsAtGains(
                                APDCommProtocol.APDChannelType.CHA,
                                _calibratingGainIndex,
                                (double)_CalibrationRecords.CalibrationRecordsCHA[_calibratingGainIndex].APDOutput,
                                (double)_CalibrationRecords.CalibrationRecordsCHA[_calibratingGainIndex].CalibVolt,
                                (double)_CalibrationRecords.CalibrationRecordsCHA[_calibratingGainIndex].CalibTemper);
                        }
                        else
                        {
                            if (_APDOutputDiffPercentCHA >= 1.0)        // limits the adjust range to ±100%
                            {
                                _APDOutputDiffPercentCHA = 1.0;
                            }
                            else if (_APDOutputDiffPercentCHA <= -1.0)
                            {
                                _APDOutputDiffPercentCHA = -1.0;
                            }
                            if (Math.Abs(_APDOutputDiffPercentCHA) >= 0.08)
                            {
                                _highVoltageSetCHA -= _highVoltSetCoeffCHA * 5 * _APDOutputDiffPercentCHA * Math.Pow(0.85, adjustLoop);
                            }
                            else
                            {
                                _highVoltageSetCHA -= _highVoltSetCoeffCHA * 3 * _APDOutputDiffPercentCHA * Math.Pow(0.85, adjustLoop);
                            }
                            _CalibrationPort.CalibrationVoltCHA = _highVoltageSetCHA;
                            //读取APD高压值HV
                            while (true)
                            {
                                if (_CalibrationPort.APDHighVoltCHA > (_highVoltageSetCHA - 30))
                                {
                                    break;
                                }
                                //Thread.Sleep(1000);
                                Thread.Sleep(1000);
                            }
                        }
                    }
                    // wait 20 seconds for APD output stable
                    if (!_IsCHAFinishAdjust)
                    {
                        if (Math.Abs(_APDOutputDiffPercentCHA) > 0.04)
                        {
                            Thread.Sleep(5 * 1000);
                            //Thread.Sleep(_CalibrationParameters.APDOutputStableShortTime * 1000);
                        }
                        else
                        {
                            Thread.Sleep(5 * 1000);
                            //Thread.Sleep(_CalibrationParameters.APDOutputStableLongTime * 1000);
                        }
                        if (++adjustLoop > 10)
                        {
                            adjustLoop = 5;  //每次调整高压的倍数
                        }
                    }
                }
                if (_CalibrationParameters.IsCHASelected)
                {
                    _PreviousGainCHA = (int)_CalibrationPort.APDGainCHA;
                }
                _CalibrationStepCounts++;
                UpdateStepCounts();
            }
            #endregion

            #region step 15-1: write high voltage of other gains (150, 250) directly
            int[] otherGains = { 250, 150 };
            int otherGainIndex = 0;
            for (int i = _CalibrationRecords.CalibrationRecordsCHA.Count - 1; i >= 0; i--)
            {
                int calibratingGainCHA = _CalibrationRecords.CalibrationRecordsCHA[i].APDGain;
                if (_CalibrationParameters.IsCHASelected)
                {
                    if (calibratingGainCHA == otherGains[otherGainIndex])
                    {
                        rewriteCounter = 0;
                        do
                        {
                            if (rewriteCounter++ >= loop)
                            {
                                ShowMessageBox.Invoke("Step 15-1:" + _MsgIVCommError, "通讯失败", MessageBoxButton.OK, MessageBoxImage.Stop);
                                ExitStat = ThreadExitStat.Error;
                                return;
                            }
                            //Thread.Sleep(2000);
                            _CalibrationPort.APDGainCHA = calibratingGainCHA;
                            //Thread.Sleep(3000);
                            Thread.Sleep(1000);
                        }
                        while (_CalibrationPort.APDGainCHA != calibratingGainCHA);
                        //写入另外4个增益（150、250） 对应的高压值。例：Gain为100时对应高压 值为HV100，Gain为200时对应的高压值为 HV200，则Gain为150时对应的高压值 HV150=HV100+(HV200-HV100)/3*2。
                        _CalibrationRecords.CalibrationRecordsCHA[i].CalibVolt = _CalibrationRecords.CalibrationRecordsCHA[i - 1].CalibVolt +
                            (_CalibrationRecords.CalibrationRecordsCHA[i + 1].CalibVolt - _CalibrationRecords.CalibrationRecordsCHA[i - 1].CalibVolt) / 3.0 * 2.0;
                        //读取APD高压值HV
                        while (true)
                        {
                            _CalibrationPort.CalibrationVoltCHA = _CalibrationRecords.CalibrationRecordsCHA[i].CalibVolt;
                            //Thread.Sleep(5000);
                            Thread.Sleep(1000);
                            if (_CalibrationPort.APDHighVoltCHA > (_CalibrationRecords.CalibrationRecordsCHA[i].CalibVolt - 30))
                            {
                                break;
                            }
                        }
                    }
                    else
                    {
                        continue;
                    }
                }
                Thread.Sleep(15000);     // wait for high voltage stable, then read APD Output and Temperature
                if (_CalibrationParameters.IsCHASelected)
                {
                    _CalibrationRecords.CalibrationRecordsCHA[i].APDOutput = _CalibrationPort.APDOutputCHA;
                    _CalibrationRecords.CalibrationRecordsCHA[i].CalibTemper = _CalibrationPort.APDTemperatureCHA;
                    UpdateRecordsAtGains(
                        APDCommProtocol.APDChannelType.CHA,
                        i,
                        (double)_CalibrationRecords.CalibrationRecordsCHA[i].APDOutput,
                        (double)_CalibrationRecords.CalibrationRecordsCHA[i].CalibVolt,
                        (double)_CalibrationRecords.CalibrationRecordsCHA[i].CalibTemper);
                }
                otherGainIndex++;
                if (otherGainIndex >= otherGains.Length)
                {
                    break;
                }
            }
            #endregion step 15-1: write high voltage of other gains (150, 250) directly

            #region step 16: verification
            _CalibrationStepCounts = 16;
            UpdateStepCounts();

            for (int i = 0; i < _CalibrationRecords.CalibrationRecordsCHA.Count; i++)
            {
                if (_CalibrationParameters.IsCHASelected)
                {
                    rewriteCounter = 0;
                    do
                    {
                        if (rewriteCounter++ >= loop)
                        {
                            ShowMessageBox.Invoke("Step 16:" + _MsgIVCommError, "通讯失败", MessageBoxButton.OK, MessageBoxImage.Stop);
                            ExitStat = ThreadExitStat.Error;
                            return;
                        }
                        //Thread.Sleep(2000);
                        _CalibrationPort.APDGainCHA = _CalibrationRecords.CalibrationRecordsCHA[i].APDGain;
                        //Thread.Sleep(3000);
                        Thread.Sleep(1000);
                    }
                    while (_CalibrationPort.APDGainCHA != _CalibrationRecords.CalibrationRecordsCHA[i].APDGain);
                }
             
                Thread.Sleep(8000);        // wait for APD output stable
                if (_CalibrationParameters.IsCHASelected)
                {
                    _CurrentAPDOutputCHA = GetAverageAPDOutput(APDCommProtocol.APDChannelType.CHA, 1);
                    _CalibrationRecords.CalibrationRecordsCHA[i].VerifyAPDOutput = _CurrentAPDOutputCHA;
                    UpdateVerificationAtGains(APDCommProtocol.APDChannelType.CHA, false, i, _CurrentAPDOutputCHA);
                }
            }
            CalculateFittedLine(false);
            for (int i = _CalibrationRecords.CalibrationRecordsCHA.Count - 1; i >= 0; i--)
            {
                if (_CalibrationParameters.IsCHASelected)
                {
                    rewriteCounter = 0;
                    do
                    {
                        if (rewriteCounter++ >= loop)
                        {
                            ShowMessageBox.Invoke(_MsgIVCommError, "通讯失败", MessageBoxButton.OK, MessageBoxImage.Stop);
                            ExitStat = ThreadExitStat.Error;
                            return;
                        }
                        //Thread.Sleep(2000);
                        _CalibrationPort.APDGainCHA = _CalibrationRecords.CalibrationRecordsCHA[i].APDGain;
                        //Thread.Sleep(3000);
                        Thread.Sleep(1000);
                    }
                    while (_CalibrationPort.APDGainCHA != _CalibrationRecords.CalibrationRecordsCHA[i].APDGain);
                }
                Thread.Sleep(8000);        // wait for APD output stable
                if (_CalibrationParameters.IsCHASelected)
                {
                    _CurrentAPDOutputCHA = GetAverageAPDOutput(APDCommProtocol.APDChannelType.CHA, 1);
                    _CalibrationRecords.CalibrationRecordsCHA[i].VerifyAPDOutput2 = _CurrentAPDOutputCHA;
                    UpdateVerificationAtGains(APDCommProtocol.APDChannelType.CHA, true, i, _CurrentAPDOutputCHA);
                }
            }
            CalculateFittedLine(true);
            #endregion step 16: verification

            _CalibrationStepCounts = 17;
            UpdateStepCounts();
        }

        public void SetTempscale(double temp)
        {
            _CalibrationPort.Tempscale = temp;
        }

        private double GetAverageAPDOutput(APDCommProtocol.APDChannelType channel, int averageCount)
        {
            double _returnVal = 0;
            double _maxVal = 0;
            double _minVal = 5;
            if (averageCount <= 0)
            {
                averageCount = 1;
            }
            for (int i = 0; i < averageCount; i++)
            {
                if (channel == APDCommProtocol.APDChannelType.CHA)
                {
                    if (averageCount > 3)
                    {
                        if (_maxVal < (double)_CalibrationPort.APDOutputCHA)
                        {
                            _maxVal = (double)_CalibrationPort.APDOutputCHA;
                        }
                        if (_minVal > (double)_CalibrationPort.APDOutputCHA)
                        {
                            _minVal = (double)_CalibrationPort.APDOutputCHA;
                        }
                    }
                    _returnVal += (double)_CalibrationPort.APDOutputCHA;
                }
                //else if (channel == APDCommProtocol.APDChannelType.CHB)
                //{
                //    if (averageCount > 3)
                //    {
                //        if (_maxVal < (double)_CalibrationPort.APDOutputCHB)
                //        {
                //            _maxVal = (double)_CalibrationPort.APDOutputCHB;
                //        }
                //        if (_minVal > (double)_CalibrationPort.APDOutputCHB)
                //        {
                //            _minVal = (double)_CalibrationPort.APDOutputCHB;
                //        }
                //    }
                //    _returnVal += (double)_CalibrationPort.APDOutputCHB;
                //}
                Thread.Sleep(2000);         // APD Output updated every 2 seconds
            }
            if (averageCount > 3)
            {
                _returnVal = _returnVal - _minVal - _maxVal;
                _returnVal = _returnVal / (averageCount - 2);
            }
            else
            {
                _returnVal = _returnVal / averageCount;
            }
            return _returnVal;
        }

        private void SampleAPDHighVolt(int sampleCount, APDCommProtocol.APDChannelType channel, out double averValue, out double maxValue, out double minValue)
        {
            maxValue = 0;
            minValue = 700;
            averValue = 0;
            if (sampleCount <= 0)
            {
                sampleCount = 1;
            }
            double temp = 0;
            for (int i = 0; i < sampleCount; i++)
            {
                if (channel == APDCommProtocol.APDChannelType.CHA)
                {
                    temp = (double)_CalibrationPort.APDHighVoltCHA;
                }
                //else if (channel == APDCommProtocol.APDChannelType.CHB)
                //{
                //    temp = (double)_CalibrationPort.APDHighVoltCHB;
                //}
                if (maxValue < temp)
                {
                    maxValue = temp;
                }
                if (minValue > temp)
                {
                    minValue = temp;
                }
                averValue += temp;

                Thread.Sleep(2000);         // APD Output updated every 2 seconds
            }
            averValue /= sampleCount;
        }

        private void SampleADCOutput(int sampleCount, APDCommProtocol.APDChannelType channel, out double averValue, out double maxValue, out double minValue)
        {
            maxValue = 0;
            minValue = 0;
            averValue = 0;
            if (sampleCount <= 0)
            {
                sampleCount = 1;
            }
            double temp = 0;
            List<double> _templist = new List<double>();
            for (int i = 0; i < sampleCount; i++)
            {
                if (channel == APDCommProtocol.APDChannelType.CHA)
                {
                    temp = (double)_CalibrationPort.APDOutputCHA;
                    _templist.Add(temp);
                }
               // averValue += temp;

                Thread.Sleep(2000);         // APD Output updated every 2 seconds
            }
           // averValue /= sampleCount;
            maxValue = _templist.Max();
            minValue = _templist.Min();
            averValue = maxValue - minValue;

        }

        private void CalculateFittedLine(bool isReverseVerification)
        {
            double slope, intercept, correlation;
            if (_CalibrationParameters.IsCHASelected)
            {
                Point[] points = new Point[_CalibrationRecords.CalibrationRecordsCHA.Count];
                for (int i = 0; i < points.Length; i++)
                {
                    points[i].X = _CalibrationRecords.CalibrationRecordsCHA[i].APDGain;
                    if (isReverseVerification)
                    {
                        points[i].Y = _CalibrationRecords.CalibrationRecordsCHA[i].VerifyAPDOutput2;
                    }
                    else
                    {
                        points[i].Y = _CalibrationRecords.CalibrationRecordsCHA[i].VerifyAPDOutput;
                    }
                }
                LinearRegression.Process(points, out slope, out intercept, out correlation);
                string fittedLine = string.Format("y = {0:F4}x {3} {1:F4}, R2 = {2:F4}", slope, Math.Abs(intercept), correlation, intercept > 0 ? "+" : "-");
                UpdateFittedLine(APDCommProtocol.APDChannelType.CHA, isReverseVerification, fittedLine);
            }
        }
        #endregion

        #region Public Properties
        public int CalibrationStepCounts
        {
            get
            {
                return _CalibrationStepCounts;
            }
        }
        #endregion
    }

}
