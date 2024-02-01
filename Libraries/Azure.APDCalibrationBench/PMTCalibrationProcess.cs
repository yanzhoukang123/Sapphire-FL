using Azure.CommandLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;
using static Azure.APDCalibrationBench.APDCalibrationProcess;

namespace Azure.APDCalibrationBench
{
    public class PMTCalibrationProcess : ThreadBase
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
        #endregion
        public PMTCalibrationProcess(
           APDCalibrationPort apdCalibrationPort,
           CalibrationParameterStruct calibrationParameters,
           CalibrationRecords calibrationRecords)
        {
            _CalibrationPort = apdCalibrationPort;
            _CalibrationParameters = calibrationParameters;
            _CalibrationRecords = calibrationRecords;

            //_DarkCurrentAdjustLimitH = _CalibrationParameters.DarkCurrentAdjustLimitH;
            //_DarkCurrentAdjustLimitL = _CalibrationParameters.DarkCurrentAdjustLimitL;
            //_APDOutputAtG0 = _CalibrationParameters.APDOutputAtG0;
            //_APDOutputAtG0Range = _CalibrationParameters.APDOutputAtG0Range;
            //_APDPGA = _CalibrationParameters.APDPGA;

        }


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
            do
            {
                if (rewriteCounter++ >= 10)
                {
                    _ConfirmResult = MessageBox.Show(_MsgIVCommError, "通讯失败", MessageBoxButton.YesNo);
                    if (_ConfirmResult == MessageBoxResult.No)
                    {
                        ExitStat = ThreadExitStat.Error;
                        return;
                    }
                    else
                    {
                        messageBoxResult = MessageBox.Show("故障是否排除!", "通讯失败", MessageBoxButton.YesNo);
                        if (messageBoxResult == MessageBoxResult.No)
                        {
                            ExitStat = ThreadExitStat.Error;
                            return;

                        }
                        ThreadFunction();
                    }
                }
                _CalibrationPort.PMTGainCHA = 4000;
                Thread.Sleep(500);
            }
            while (_CalibrationPort.PMTGainCHA != 4000 - 1);

            string caption = "调节";
            string message = string.Format("将当前的APD输出调整范围到1~3mV,在调整完成后，点击确定按钮完成标定！");
            messageBoxResult = MessageBox.Show(message, caption, MessageBoxButton.YesNo);
            if (messageBoxResult == MessageBoxResult.No)
            {
                ExitStat = ThreadExitStat.Error;
                return;
            }


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
        #endregion
    }
}
