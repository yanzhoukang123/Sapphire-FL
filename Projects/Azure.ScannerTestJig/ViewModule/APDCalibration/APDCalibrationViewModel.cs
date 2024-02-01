using Azure.APDCalibrationBench;
using Azure.Configuration.Settings;
using Azure.ImagingSystem;
using Azure.ScannerTestJig.View.APDCalibration;
using Azure.WPF.Framework;
using Hywire.FileAccess;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace Azure.ScannerTestJig.ViewModule.APDCalibration
{
    public class APDCalibrationViewModel : ViewModelBase
    {
        #region privert
        private RelayCommand _APDCalibrationCommand = null;
        private RelayCommand _StartCalibrationCommand = null;
        private RelayCommand _StopCalibrationCommand = null;
        private RelayCommand _FinishCalibrationCommand = null;
        private TestAPDCalibration _TestAPDCalibration = null;
        public APDCalibrationSubWind _APDCalibrationWind = null;

        private APDCalibrationPort _APDCalibrationPort = null;
        private APDCalibrationProcess.CalibrationParameterStruct _CalibrationParameters = null;
        private APDCalibrationProcess _APDCalibrationProcess = null;
        private PMTCalibrationProcess _PMTCalibrationProcess = null;
        private CalibrationRecords _APDCalibrationRecords = null;

        private ObservableCollection<string> _CalibrationResultOptions = new ObservableCollection<string>();
        private string _SelectedCalibrationResult;
        private string _DateTimeNow;
        private string _TestDate;
        private string _TestTime;
        private string _StartTime = string.Empty;
        private string _EndTime = string.Empty;
        private System.Timers.Timer _timer;

        private bool _IsCHASelected = false;
        //private bool _IsCHBSelected = false;

        private int _ReadErrorCount = 0;
        private int _WriteErrorCount = 0;

        private string _IVPCBNumber = string.Empty;
        private string _LaserPCBNumber = string.Empty;
        private string _TestOperator = string.Empty;
        private bool _IsChAHighVoltErrorMasked = false;
        //private bool _IsChBHighVoltErrorMasked = false;
        private bool _IsChAHighVoltError = false;
        //private bool _IsChBHighVoltError = false;
        private bool _IsShowErrorChA = true;
        //private bool _IsShowErrorChB = true;
        private bool _IsChATemperErrorMasked = false;
        //private bool _IsChBTemperErrorMasked = false;
        BrushConverter bc = null;
        private double? _TECTemperature = null;
        private int tecTempflag = 0;
        public bool CalibrationButtonState = false;
        public bool APDPowreValueFlat = false;
        public int IVType = 0;

        #endregion

        #region APD Calibration Command
        public ICommand APDCalibrationCommand
        {
            get
            {
                if (_APDCalibrationCommand == null)
                {
                    _APDCalibrationCommand = new RelayCommand(ExcuteAPDCalibrationCommand, CanExcuteAPDCalibrationCommand);
                }
                return _APDCalibrationCommand;
            }
        }
        public void ExcuteAPDCalibrationCommand(object parameter)
        {
            try
            {
                IVType = 0;
                SelectedIVTypeResult = IVType;
                bc = new BrushConverter();
                tecTempflag = 0;
                CalibrationButtonState = false;
                if (Workspace.This.APDCalibrationVM.APDCalibrationPort.IsConnected == false)
                {
                    Workspace.This.APDCalibrationVM.APDCalibrationPort.SearchPort();
                    IsCHASelected = _APDCalibrationPort.IsAPDCHAAlive;
                    // IsCHBSelected = _APDCalibrationPort.IsAPDCHBAlive;
                }
                if (Workspace.This.APDCalibrationVM.APDCalibrationPort.AvailablePorts.Length == 0)
                {
                    MessageBox.Show("未找到串口设备！", "连接错误", MessageBoxButton.OK, MessageBoxImage.Error);
                    //return;
                }
                if (Workspace.This.APDCalibrationVM.APDCalibrationPort.IsConnected == false)
                {
                    MessageBoxResult messageBoxResult = MessageBox.Show("485 错误！", "连接错误", MessageBoxButton.YesNo, MessageBoxImage.Error);
                    if (messageBoxResult == MessageBoxResult.Yes)
                    {
                        Workspace.This.APDCalibrationChannelAVM.LightBlue = (SolidColorBrush)bc.ConvertFrom("#F4F4F4");
                        Workspace.This.APDCalibrationChannelAVM.LightReseda = (SolidColorBrush)bc.ConvertFrom("#F4F4F4");
                        _APDCalibrationWind = new APDCalibrationSubWind();
                        _APDCalibrationWind.Title = Workspace.This.Owner.Title + "-APD Calibration Kit";
                        // _APDCalibrationWind.comboBox.Background = (SolidColorBrush)bc.ConvertFrom("#66FFFF");
                        _APDCalibrationWind.calibrationOperatorBox.Background = (SolidColorBrush)bc.ConvertFrom("#F4F4F4");
                        _APDCalibrationWind.startCalibrationBtn.IsEnabled = false;
                        _APDCalibrationWind.ShowDialog();
                        _APDCalibrationWind = null;
                        return;
                    }
                    else
                    {
                        Workspace.This.Owner.Show();
                        return;
                    }

                    //return;

                }
                else
                {
                    Workspace.This.APDPowreValueFlat = false;
                    SetTempscale();
                    _APDCalibrationPort.getIvType();
                    Workspace.This.APDCalibrationChannelAVM.LightBlue = (SolidColorBrush)bc.ConvertFrom("#66FFFF");
                    Workspace.This.APDCalibrationChannelAVM.LightReseda = (SolidColorBrush)bc.ConvertFrom("#91CF50");
                    _APDCalibrationWind = new APDCalibrationSubWind();
                    _APDCalibrationWind.Title = Workspace.This.Owner.Title + "-APD Calibration Kit";
                    // _APDCalibrationWind.comboBox.Background = (SolidColorBrush)bc.ConvertFrom("#66FFFF");
                    _APDCalibrationWind.calibrationOperatorBox.Background = (SolidColorBrush)bc.ConvertFrom("#66FFFF");
                    APDCalibrationChannelViewModel.Emtiy();
                    _APDCalibrationWind.startCalibrationBtn.IsEnabled = false;
                    _APDCalibrationWind.ShowDialog();
                    _APDCalibrationWind = null;
                }
                CalibrationButtonState = true;
                Workspace.This.APDCalibrationChannelAVM.CalibrationStepCount = 0;
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.ToString());
            }

        }
        public bool CanExcuteAPDCalibrationCommand(object parameter)
        {
            return true;
        }
        #endregion

        #region Start Calibration Command
        public ICommand StartCalibrationCommand
        {
            get
            {
                if (_StartCalibrationCommand == null)
                {
                    _StartCalibrationCommand = new RelayCommand(ExcuteStartCalibrationCommand, CanExcuteStartCalibrationCommand);
                }
                return _StartCalibrationCommand;
            }
        }
        public void ExcuteStartCalibrationCommand(object parameter)
        {
            //if (!IsCHASelected && !IsCHBSelected)
            //    if (!IsCHASelected)
            //    {
            //        MessageBox.Show("485错误！!");
            //        return;
            //    }
            MessageBoxResult _ConfirmResult = new MessageBoxResult();
            if (IVType != 0)   //PMT
            {
                    _ConfirmResult = MessageBox.Show("开始标定，确认？", "", MessageBoxButton.YesNo);
                    if (_ConfirmResult != MessageBoxResult.Yes)
                    {
                        return;
                    }
                _PMTCalibrationProcess = new PMTCalibrationProcess(_APDCalibrationPort, _CalibrationParameters, _APDCalibrationRecords);
                _PMTCalibrationProcess.Completed += _APDCalibrationProcess_Completed;
                //_APDCalibrationProcess.ShowMessageBox += _APDCalibrationProcess_ShowMessageBox;
                _PMTCalibrationProcess.Start();
                _TestAPDCalibration.IsChecked = false;
                _TestAPDCalibration.IsChecking = true;
                _TestAPDCalibration.IsPassed = false;
                Workspace.This.APDCalibrationChannelAVM.IsAPDModuleAlive = false;
                //Workspace.This.APDCalibrationChannelBVM.IsAPDModuleAlive = false;
                SelectedCalibrationResult = _CalibrationResultOptions[0];//默认设置为标定
                StartTime = DateTime.Now.ToString("HH:mm");
                EndTime = string.Empty;
                return;
            }
            if (IsCHASelected)
            {
                if (APDCalibrationChannelViewModel.IVPCBNumber == string.Empty ||              //IV编号
                                                                                               //APDCalibrationChannelViewModel.LaserPCBNumber == string.Empty ||
                    Workspace.This.APDCalibrationChannelAVM.APDSerialNumber == string.Empty ||   //APD编号
                    Workspace.This.APDCalibrationChannelAVM.SelectedWaveLength == -1 ||
                    Workspace.This.APDCalibrationChannelAVM.CalibrationVoltage == null ||        //标准电压
                    Workspace.This.APDCalibrationChannelAVM.BreakdownVoltage == null ||          //击穿电压
                    Workspace.This.APDCalibrationChannelAVM.SelectedCalibrationGain == null ||
                    Workspace.This.APDCalibrationChannelAVM.CalibrationTemperature == null ||
                    Workspace.This.APDCalibrationChannelAVM.SelectedTemperatureCoeff == 0)
                {
                    MessageBox.Show("请检查APD信息输入是否完整！");
                    return;
                }
            }
            //MessageBoxResult _ConfirmResult = new MessageBoxResult();
            _ConfirmResult = MessageBox.Show("开始标定，确认？", "", MessageBoxButton.YesNo);
            if (_ConfirmResult != MessageBoxResult.Yes)
            {
                return;
            }
            Workspace.This.APDPowreValueFlat = true;
            _CalibrationParameters = new APDCalibrationProcess.CalibrationParameterStruct();
            if (IsCHASelected)
            {
                _CalibrationParameters.IVNum = Workspace.This.APDCalibrationChannelAVM.APDSerialNumber;//APD编号
                _CalibrationParameters.APDType = (int)SelectedIVTypeResult;//APD类型
                _CalibrationParameters.APDWaveLengthChA = Workspace.This.APDCalibrationChannelAVM.SelectedWaveLength;//波长
                _CalibrationParameters.CalibrationGainChA = Workspace.This.APDCalibrationChannelAVM.SelectedCalibrationGain.APDGain;//APD增益
                _CalibrationParameters.CalibrationTemperatureChA = Workspace.This.APDCalibrationChannelAVM.CalibrationTemperature;//校准温度
                _CalibrationParameters.CalibrationVoltageChA = Workspace.This.APDCalibrationChannelAVM.CalibrationVoltage;//标定电压
                _CalibrationParameters.BreakdownVoltageChA = Workspace.This.APDCalibrationChannelAVM.BreakdownVoltage;//击穿电压
                _CalibrationParameters.TemperatureCoeffChA = Workspace.This.APDCalibrationChannelAVM.SelectedTemperatureCoeff;//温度
                _CalibrationParameters.IsCHASelected = IsCHASelected;

                for (int i = 0; i < Workspace.This.APDCalibrationChannelAVM.GainOptions.Count; i++)
                {
                    Workspace.This.APDCalibrationChannelAVM.GainOptions[i].APDOutput = null;
                    Workspace.This.APDCalibrationChannelAVM.GainOptions[i].CalibrationVolt = null;
                    Workspace.This.APDCalibrationChannelAVM.GainOptions[i].CalibrationTemper = null;
                    Workspace.This.APDCalibrationChannelAVM.GainOptions[i].VerifyAPDOutput = null;
                    Workspace.This.APDCalibrationChannelAVM.GainOptions[i].VerifyAPDOutput2 = null;
                }
                Workspace.This.APDCalibrationChannelAVM.FittedLine = string.Empty;
                Workspace.This.APDCalibrationChannelAVM.FittedLine2 = string.Empty;
                _IsShowErrorChA = true;
                for (int i = 0; i < 5; i++)
                {
                    _APDCalibrationPort.IVNum = _CalibrationParameters.IVNum;
                    Thread.Sleep(500);
                    _APDCalibrationPort.IVType = _CalibrationParameters.APDType;
                    Thread.Sleep(500);
                }
                //_APDCalibrationPort.LaserCHA = 0;
            }
            //step 3: set PGACHA =8 目前读取配置文件
            _CalibrationParameters.APDPGA = SettingsManager.ConfigSettings.APDPGA;
            _CalibrationParameters.APDOutputAtG0 = SettingsManager.ConfigSettings.APDOutputAtG0;
            _CalibrationParameters.APDOutputAtG0Range = SettingsManager.ConfigSettings.APDOutputErrorAtG0;
            _CalibrationParameters.DarkCurrentAdjustLimitH = SettingsManager.ConfigSettings.APDDarkCurrentLimitH;
            _CalibrationParameters.DarkCurrentAdjustLimitL = SettingsManager.ConfigSettings.APDDarkCurrentLimitL;
            _CalibrationParameters.APDOutputStableLongTime = SettingsManager.ConfigSettings.APDOutputStableLongTime;
            _CalibrationParameters.APDOutputStableShortTime = SettingsManager.ConfigSettings.APDOutputStableShortTime;
            _APDCalibrationProcess = new APDCalibrationProcess(_APDCalibrationPort, _CalibrationParameters, _APDCalibrationRecords);
            _APDCalibrationProcess.UpdateStepCounts += _APDCalibrationProcess_UpdateStepCounts;
            _APDCalibrationProcess.UpdateRecordsAtGains += _APDCalibrationProcess_UpdateRecordsAtGains;
            _APDCalibrationProcess.UpdateVerificationAtGains += _APDCalibrationProcess_UpdateVerificationAtGains;
            _APDCalibrationProcess.UpdateFittedLine += _APDCalibrationProcess_UpdateFittedLine;
            _APDCalibrationProcess.Completed += _APDCalibrationProcess_Completed;
            _APDCalibrationProcess.ShowMessageBox += _APDCalibrationProcess_ShowMessageBox;
            _APDCalibrationProcess.Start();
            _TestAPDCalibration.IsChecked = false;
            _TestAPDCalibration.IsChecking = true;
            _TestAPDCalibration.IsPassed = false;
            Workspace.This.APDCalibrationChannelAVM.IsAPDModuleAlive = false;
            //Workspace.This.APDCalibrationChannelBVM.IsAPDModuleAlive = false;
            SelectedCalibrationResult = _CalibrationResultOptions[0];//默认设置为标定
            StartTime = DateTime.Now.ToString("HH:mm");
            EndTime = string.Empty;
        }

        private MessageBoxResult _APDCalibrationProcess_ShowMessageBox(string message, string title, MessageBoxButton button, MessageBoxImage image)
        {
            MessageBoxResult result = MessageBoxResult.None;
            Workspace.This.Owner.Dispatcher.Invoke(new Action(() =>
            {
                result = MessageBox.Show(message, title, button, image);
            }));
            return result;
        }

        private void _APDCalibrationProcess_UpdateFittedLine(APDCommProtocol.APDChannelType channel, bool isReverseVerification, string line)
        {
            if (channel == APDCommProtocol.APDChannelType.CHA)
            {
                if (isReverseVerification)
                {
                    Workspace.This.APDCalibrationChannelAVM.FittedLine2 = line;
                }
                else
                {
                    Workspace.This.APDCalibrationChannelAVM.FittedLine = line;
                }
            }
        }

        private void _APDCalibrationProcess_UpdateVerificationAtGains(APDCommProtocol.APDChannelType channel, bool isReverseVerification, int gainIndex, double APDOutput)
        {
            if (channel == APDCommProtocol.APDChannelType.CHA)
            {
                if (isReverseVerification)
                {
                    Workspace.This.APDCalibrationChannelAVM.GainOptions[gainIndex].VerifyAPDOutput2 = APDOutput;
                }
                else
                {
                    Workspace.This.APDCalibrationChannelAVM.GainOptions[gainIndex].VerifyAPDOutput = APDOutput;
                }
            }
        }

        private void _APDCalibrationProcess_UpdateRecordsAtGains(
            APDCommProtocol.APDChannelType channel,
            int gainIndex,
            double APDOutput,
            double calibrationVolt,
            double calibrationTemper)
        {
            if (channel == APDCommProtocol.APDChannelType.CHA)
            {
                Workspace.This.APDCalibrationChannelAVM.GainOptions[gainIndex].APDOutput = APDOutput;
                Workspace.This.APDCalibrationChannelAVM.GainOptions[gainIndex].CalibrationVolt = calibrationVolt;
                Workspace.This.APDCalibrationChannelAVM.GainOptions[gainIndex].CalibrationTemper = calibrationTemper;
            }
        }

        private void _APDCalibrationProcess_Completed(CommandLib.ThreadBase sender, CommandLib.ThreadBase.ThreadExitStat exitState)
        {
            EndTime = DateTime.Now.ToString("HH:mm");
            if(IVType != 0)
            {
                //PMT
                if (exitState == CommandLib.ThreadBase.ThreadExitStat.None)
                {
                    MessageBox.Show("标定完成！");
                    _TestAPDCalibration.IsChecked = true;
                    _TestAPDCalibration.IsChecking = false;
                    _TestAPDCalibration.IsPassed = true;
                }
                else if (exitState == CommandLib.ThreadBase.ThreadExitStat.Abort)
                {
                    _TestAPDCalibration.IsChecked = false;
                    _TestAPDCalibration.IsChecking = false;
                    _TestAPDCalibration.IsPassed = false;
                }
                else
                {
                    _TestAPDCalibration.IsChecked = false;
                    _TestAPDCalibration.IsChecking = false;
                    _TestAPDCalibration.IsPassed = false;
                    MessageBox.Show("标定过程中出现错误，标定失败！", "出错啦", MessageBoxButton.OK, MessageBoxImage.Stop);

                }
            }
            else
            {
                if (exitState == CommandLib.ThreadBase.ThreadExitStat.None)
                {

                    int rewriteCounter = 0;
                    do
                    {
                        rewriteCounter++;
                        //将激光写为0
                        _APDCalibrationPort.LaserCHA = 0;
                        Thread.Sleep(500);
                    }
                    while (rewriteCounter < 10);
                    _TestAPDCalibration.IsChecked = true;
                    _TestAPDCalibration.IsChecking = false;
                    _TestAPDCalibration.IsPassed = true;
                    SelectedCalibrationResult = _CalibrationResultOptions[1];

                    double CalTempera = (double)_APDCalibrationPort.Caltemperature;//校准温度
                    double Tempera500 = (double)_APDCalibrationPort.Temperature500;//500校准温度
                    if (Math.Abs((CalTempera - Tempera500)) >= 0.5)
                    {
                        MessageBox.Show("标定已完成！但标定过程中温度不稳定，光强校准时温度是" + CalTempera + ",500倍增益校准时温度是" + Tempera500, "警告");
                    }
                    else
                    {
                        MessageBox.Show("标定完成！");
                    }

                }
                else if (exitState == CommandLib.ThreadBase.ThreadExitStat.Abort)
                {
                    _TestAPDCalibration.IsChecked = false;
                    _TestAPDCalibration.IsChecking = false;
                    _TestAPDCalibration.IsPassed = false;
                    SelectedCalibrationResult = _CalibrationResultOptions[0];

                    MessageBox.Show("标定取消！");
                }
                else
                {
                    _TestAPDCalibration.IsChecked = false;
                    _TestAPDCalibration.IsChecking = false;
                    _TestAPDCalibration.IsPassed = false;
                    SelectedCalibrationResult = _CalibrationResultOptions[0];

                    if (exitState == CommandLib.ThreadBase.ThreadExitStat.Error)
                    {
                        MessageBox.Show("标定过程中出现错误，标定失败！", "出错啦", MessageBoxButton.OK, MessageBoxImage.Stop);
                    }
                    Workspace.This.APDCalibrationChannelAVM.CalibrationStepCount = 0;
                }

                Workspace.This.APDCalibrationChannelAVM.IsAPDModuleAlive = IsCHASelected;
                _APDCalibrationPort.LaserCHA = 0;
            }
           
        }

        private void _APDCalibrationProcess_UpdateStepCounts()
        {
            if (IsCHASelected)
            {
                Workspace.This.APDCalibrationChannelAVM.CalibrationStepCount = _APDCalibrationProcess.CalibrationStepCounts;
            }
        }

        public bool CanExcuteStartCalibrationCommand(object parameter)
        {
            return true;
        }

        public void SetTempscale()
        {
            Workspace.This.APDCalibrationVM.APDCalibrationPort.Tempscale = Workspace.This.APDCalibrationChannelAVM.SelectedTemperatureCoeff;//温度

        }
        #endregion

        #region Stop Calibration Command
        public ICommand StopCalibrationCommand
        {
            get
            {
                if (_StopCalibrationCommand == null)
                {
                    _StopCalibrationCommand = new RelayCommand(ExcuteStopCalibrationCommand, CanExcuteStopCalibrationCommand);
                }
                return _StopCalibrationCommand;
            }
        }
        public void ExcuteStopCalibrationCommand(object parameter)
        {
            if (IVType != 0)
            {
                if (_PMTCalibrationProcess != null)
                {
                    _PMTCalibrationProcess.Abort();  // Abort the scanning thread
                    _APDCalibrationWind.startCalibrationBtn.Content = "重新标定";
                    // _APDCalibrationWind.startCalibrationBtn.Background= (SolidColorBrush)bc.ConvertFrom("#6B9AC6");
                }
               
            }
            else {
                if (_APDCalibrationProcess != null)
                {
                    _APDCalibrationProcess.Abort();  // Abort the scanning thread
                    _APDCalibrationWind.startCalibrationBtn.Content = "重新标定";
                    // _APDCalibrationWind.startCalibrationBtn.Background= (SolidColorBrush)bc.ConvertFrom("#6B9AC6");
                }
            }
        }
        public bool CanExcuteStopCalibrationCommand(object parameter)
        {
            return true;
        }
        #endregion Stop Calibration Command

        #region Finish Calibration Command
        public ICommand FinishCalibrationCommand
        {
            get
            {
                if (_FinishCalibrationCommand == null)
                {
                    _FinishCalibrationCommand = new RelayCommand(ExcuteFinishCalibrationCommand, CanExcuteFinishCalibrationCommand);
                }
                return _FinishCalibrationCommand;
            }
        }
        public void ExcuteFinishCalibrationCommand(object parameter)
        {
            _GenerateReportAPDCalibration();
            try
            {
                string _filePath = string.Format(".\\测试报告\\APD Calibration Report-{0}.csv",
                    Workspace.This.APDCalibrationChannelAVM.APDSerialNumber);
                //Workspace.This.APDCalibrationChannelBVM.APDSerialNumber);
                FileInfo fileInfo = new FileInfo(_filePath);
                Microsoft.Office.Interop.Excel.Application excel = new Microsoft.Office.Interop.Excel.Application();
                Microsoft.Office.Interop.Excel.Workbook book = excel.Application.Workbooks.Add(fileInfo.FullName);
                excel.Visible = true;
            }
            catch
            {
                if (Directory.Exists(@".\测试报告"))
                {
                    System.Diagnostics.Process.Start("Explorer.exe", @".\测试报告");
                }

            }
            _APDCalibrationWind.rec.Width = _APDCalibrationWind.sWidth;
            _APDCalibrationWind.rec.Height = _APDCalibrationWind.sHight;
        }
        private void _GenerateReportAPDCalibration()
        {
            string _filePath = string.Format(".\\测试报告\\Scanner APD标定报告v2.0-NA{0}.csv",
                Workspace.This.APDCalibrationChannelAVM.APDSerialNumber);
            //Workspace.This.APDCalibrationChannelBVM.APDSerialNumber);
            var _APDCalibrationReportFile = new FileVisit(_filePath);
            _APDCalibrationReportFile.VisitError += _APDCalibrationReport_VisitError;
            if (_APDCalibrationReportFile.Open(System.IO.FileAccess.ReadWrite))
            {
                StringBuilder _header = new StringBuilder("APD标定报告" + "\n", 1024);
                _header.Append("标定员：" + Workspace.This.APDCalibrationVM.TestOperator + "\n");
                _header.Append("标定日期：" + Workspace.This.APDCalibrationVM.TestDate + Workspace.This.APDCalibrationVM.TestTime + "\n");
                _header.Append("IV板编号：" + Workspace.This.APDCalibrationVM.IVPCBNumber + "\n");
                // _header.Append("Laser板编号：" + Workspace.This.APDCalibrationVM.LaserPCBNumber + "\n");

                _header.Append("APD编号：" + Workspace.This.APDCalibrationChannelAVM.APDSerialNumber + "\n");
                //_header.Append("通道APD波长：" +
                //    Workspace.This.APDCalibrationChannelAVM.WaveLengthOptions[Workspace.This.APDCalibrationChannelAVM.SelectedWaveLength]
                //    + "\n");
                // _header.Append("通道标定增益：" + Workspace.This.APDCalibrationChannelAVM.SelectedCalibrationGain.APDGain + "\n");
                _header.Append("标定电压：" + Workspace.This.APDCalibrationChannelAVM.CalibrationVoltage + "\n");
                _header.Append("击穿电压：" + Workspace.This.APDCalibrationChannelAVM.BreakdownVoltage + "\n");
                _header.Append("标定温度：" + Workspace.This.APDCalibrationChannelAVM.CalibrationTemperature + "\n");
                _header.Append("温度系数(V/℃)：" + Workspace.This.APDCalibrationChannelAVM.SelectedTemperatureCoeff + "\n");

                // _header.Append("B通道APD编号：" + Workspace.This.APDCalibrationChannelBVM.APDSerialNumber + "\n");
                //_header.Append("B通道APD波长：" +
                //Workspace.This.APDCalibrationChannelBVM.WaveLengthOptions[Workspace.This.APDCalibrationChannelBVM.SelectedWaveLength]
                //+ "\n");
                //_header.Append("B通道标定增益：" + Workspace.This.APDCalibrationChannelBVM.SelectedCalibrationGain.APDGain + "\n");
                //_header.Append("B通道标定电压：" + Workspace.This.APDCalibrationChannelBVM.CalibrationVoltage + "\n");
                //_header.Append("B通道击穿电压：" + Workspace.This.APDCalibrationChannelBVM.BreakdownVoltage + "\n");
                //_header.Append("B通道标定温度：" + Workspace.This.APDCalibrationChannelBVM.CalibrationTemperature + "\n");
                //_header.Append("B通道温度系数(V/℃)：" + Workspace.This.APDCalibrationChannelBVM.SelectedTemperatureCoeff + "\n");

                string _testResult = Workspace.This.APDCalibrationVM.TestItemAPDCalibration.IsPassed ? "通过" : "未通过";
                _header.Append("标定结果：" + _testResult + "\n");

                _header.Append("正向拟合曲线：" + Workspace.This.APDCalibrationChannelAVM.FittedLine);
                _header.Append("反向拟合曲线：" + Workspace.This.APDCalibrationChannelAVM.FittedLine2);
                //_header.Append("B通道正向拟合曲线：" + Workspace.This.APDCalibrationChannelBVM.FittedLine);
                //_header.Append("B通道反向拟合曲线：" + Workspace.This.APDCalibrationChannelBVM.FittedLine2);
                _header.Append("\n" + "通道标定记录" + "," + "," + ",");
                // _header.Append("\n" + "通道标定记录" + ",");
                List<string> columHeaderList = new List<string>();
                columHeaderList.Add("标定增益");
                columHeaderList.Add("APD输出(V)");
                columHeaderList.Add("标定电压(V)");
                columHeaderList.Add("标定温度(℃)");
                columHeaderList.Add("正向校准电压(V)");
                columHeaderList.Add("反向校准电压(V)");
                string[] _columnHeaders = columHeaderList.ToArray();
                CSVFile _APDCalibrationReport = new CSVFile(_header.ToString(), _columnHeaders);

                string[] _columnGain = new string[Workspace.This.APDCalibrationChannelAVM.GainOptions.Count];
                string[] _columnAPDOutput = new string[_columnGain.Length];
                string[] _columnCalibVolt = new string[_columnGain.Length];
                string[] _columnCalibTemper = new string[_columnGain.Length];
                string[] _columnVerifyVolt = new string[_columnGain.Length];
                string[] _columnVerifyVolt2 = new string[_columnGain.Length];
                int columnWriteCount = 0;
                for (int i = 0; i < _columnGain.Length; i++)
                {
                    _columnGain[i] = Workspace.This.APDCalibrationChannelAVM.GainOptions[i].APDGain.ToString();
                    _columnAPDOutput[i] = Workspace.This.APDCalibrationChannelAVM.GainOptions[i].APDOutput.ToString();
                    _columnCalibVolt[i] = Workspace.This.APDCalibrationChannelAVM.GainOptions[i].CalibrationVolt.ToString();
                    _columnCalibTemper[i] = Workspace.This.APDCalibrationChannelAVM.GainOptions[i].CalibrationTemper.ToString();
                    _columnVerifyVolt[i] = Workspace.This.APDCalibrationChannelAVM.GainOptions[i].VerifyAPDOutput.ToString();
                    _columnVerifyVolt2[i] = Workspace.This.APDCalibrationChannelAVM.GainOptions[i].VerifyAPDOutput2.ToString();
                }
                _APDCalibrationReport.SetColumnContent(columnWriteCount++, _columnGain);
                _APDCalibrationReport.SetColumnContent(columnWriteCount++, _columnAPDOutput);
                _APDCalibrationReport.SetColumnContent(columnWriteCount++, _columnCalibVolt);
                _APDCalibrationReport.SetColumnContent(columnWriteCount++, _columnCalibTemper);
                _APDCalibrationReport.SetColumnContent(columnWriteCount++, _columnVerifyVolt);
                _APDCalibrationReport.SetColumnContent(columnWriteCount++, _columnVerifyVolt2);
                //for (int i = 0; i < _columnGain.Length; i++)
                //{
                //    //_columnGain[i] = Workspace.This.APDCalibrationChannelBVM.GainOptions[i].APDGain.ToString();
                //    //_columnAPDOutput[i] = Workspace.This.APDCalibrationChannelBVM.GainOptions[i].APDOutput.ToString();
                //    //_columnCalibVolt[i] = Workspace.This.APDCalibrationChannelBVM.GainOptions[i].CalibrationVolt.ToString();
                //    //_columnCalibTemper[i] = Workspace.This.APDCalibrationChannelBVM.GainOptions[i].CalibrationTemper.ToString();
                //    //_columnVerifyVolt[i] = Workspace.This.APDCalibrationChannelBVM.GainOptions[i].VerifyAPDOutput.ToString();
                //    //_columnVerifyVolt2[i] = Workspace.This.APDCalibrationChannelBVM.GainOptions[i].VerifyAPDOutput2.ToString();
                //}
                //_APDCalibrationReport.SetColumnContent(columnWriteCount++, _columnGain);
                //_APDCalibrationReport.SetColumnContent(columnWriteCount++, _columnAPDOutput);
                ////_APDCalibrationReport.SetColumnContent(columnWriteCount++, _columnCalibVolt);
                ////_APDCalibrationReport.SetColumnContent(columnWriteCount++, _columnCalibTemper);
                //_APDCalibrationReport.SetColumnContent(columnWriteCount++, _columnVerifyVolt);
                //_APDCalibrationReport.SetColumnContent(columnWriteCount++, _columnVerifyVolt2);

                _APDCalibrationReportFile.Write(_APDCalibrationReport.ToString());
            }
            _APDCalibrationReportFile.Close();
        }
        private void _APDCalibrationReport_VisitError(object sender, FileAccessErrorArgs e)
        {
            MessageBox.Show("文件访问出错！\n" + e.Message);
        }
        public bool CanExcuteFinishCalibrationCommand(object parameter)
        {
            return true;
        }
        #endregion Finish Calibration Command

        public APDCalibrationViewModel()
        {
            _temptrue = new List<double?>();
            _TestAPDCalibration = new TestAPDCalibration();
            _APDCalibrationPort = new APDCalibrationPort();
            _APDCalibrationPort.CommPortError += _APDCalibrationPort_CommPortError;
            _APDCalibrationPort.APDComm.UpdateCommOutput += APDComm_UpdateCommOutput;
            _APDCalibrationPort.BenchComm.UpdateCommOutput += BenchComm_UpdateCommOutput;
            _APDCalibrationPort.LaserComm.UpdateCommOutput += LaserComm_UpdateCommOutput;
            APDCommProtocol.CommTimeOut += APDCommProtocol_CommTimeOut;
            RaisePropertyChanged("IVTypeOption");
            _APDCalibrationRecords = new CalibrationRecords();
            IVTypeOptions.Add(0);
            IVTypeOptions.Add(1);
            _SelectedIVTypeResult = IVTypeOptions[0];
            _CalibrationResultOptions.Add("未标定");
            _CalibrationResultOptions.Add("合格");
            _CalibrationResultOptions.Add("不合格");
            _SelectedCalibrationResult = _CalibrationResultOptions[0];
            TestDate = DateTime.Now.ToString("yyyy年MM月dd日");
            TestTime = DateTime.Now.ToString("HH:mm");
            DateTimeNow = TestDate + "\r\n" + TestTime;
            _timer = new System.Timers.Timer();
            _timer.Interval = 1000;
            _timer.Elapsed += _timer_Elapsed;
            _timer.Start();
        }

        private void _APDCalibrationPort_CommPortError(string error)
        {
            MessageBox.Show("模块通讯失败！" + error);
        }

        private void APDCommProtocol_CommTimeOut(APDCommProtocol.CommErrorInfo errorInfo)
        {
            if (APDCommProtocol.CommStatus == APDCommProtocol.CommStatusType.TimeOut)    // ensure the Timeout event was not processed else where
            {
                if (_APDCalibrationPort.CommTimeOutCount >= 3)
                {
                    //MessageBox.Show("通讯超时！" + "\n" + errorInfo.ToString());
                    _APDCalibrationPort.CommTimeOutCount = 0;
                    APDCommProtocol.CommStatus = APDCommProtocol.CommStatusType.Idle;

                    if (errorInfo.FunctionCode == APDCommProtocol.FunctionCodeType.ReadReg)
                    {
                        ReadErrorCount++;
                    }
                    else if (errorInfo.FunctionCode == APDCommProtocol.FunctionCodeType.WriteReg)
                    {
                        WriteErrorCount++;
                    }
                    Thread.Sleep(500);
                    //if (errorInfo.FunctionCode == APDCommProtocol.FunctionCodeType.WriteReg && TestItemAPDCalibration.IsChecking)
                    //{
                    //    ExcuteStopCalibrationCommand(null);
                    //    MessageBox.Show("写入参数超时，标定失败！");
                    //}
                }
            }
        }
        List<double?> _temptrue = null;
        private void APDComm_UpdateCommOutput()
        {
            //Workspace.This.APDCalibrationChannelAVM.APDSerialNumber= _APDCalibrationPort.APDComm.CommOutputChA.CurrentIVNum;
            if (_APDCalibrationPort.APDComm.CommOutputChA.CurrentIVType == null)
            {
                IVType = 0;
            }
            else
            {
                IVType = (int)_APDCalibrationPort.APDComm.CommOutputChA.CurrentIVType;
            }
            SelectedIVTypeResult = IVType;
            if (IVType == 1)   //PMT
            {
                //_APDCalibrationPort.getIvType();
                Workspace.This.APDCalibrationChannelAVM.CurrentAPDGain = _APDCalibrationPort.APDComm.CommOutputChA.CurrentPMTGain;

            }
            else {
                Workspace.This.APDCalibrationChannelAVM.CurrentAPDGain = _APDCalibrationPort.APDComm.CommOutputChA.CurrentAPDGain;
            }
            Workspace.This.APDCalibrationChannelAVM.CurrentPGA = _APDCalibrationPort.APDComm.CommOutputChA.CurrentPGA;
            Workspace.This.APDCalibrationChannelAVM.APDHighVoltage = _APDCalibrationPort.APDComm.CommOutputChA.APDHighVoltage;
            Workspace.This.APDCalibrationChannelAVM.APDTemperature = _APDCalibrationPort.APDComm.CommOutputChA.APDTemperature;
            double? _AH = Workspace.This.APDCalibrationChannelAVM.BreakdownVoltage + (Workspace.This.APDCalibrationChannelAVM.SelectedTemperatureCoeff * (Workspace.This.APDCalibrationChannelAVM.APDTemperature - Workspace.This.APDCalibrationChannelAVM.CalibrationTemperature));
            double? _AHighVolt = _APDCalibrationPort.APDComm.CommOutputChA.APDHighVoltage;
            #region 如果APD电压>100
            if (Workspace.This.APDCalibrationChannelAVM.APDHighVoltage > 100 && Workspace.This.APDPowreValueFlat == false)
            {
                Workspace.This.APDPowreValueFlat = true;
                MessageBoxResult _ConfirmResult = new MessageBoxResult();
                _ConfirmResult = MessageBox.Show("高压过高，并清除APD校准电压（50/100/150/200/250/300/400/500）？", "", MessageBoxButton.YesNo);
                if (_ConfirmResult == MessageBoxResult.Yes)
                {
                    //将校准电压写成5000
                    Workspace.This.APDCalibrationVM.APDCalibrationPort.ChangeCalibrationValtage();
                }
                return;
            }
            #endregion

            #region High Volt Error process, cancel calibration  BreakdownVoltage
            if ((Workspace.This.APDCalibrationChannelAVM.BreakdownVoltage == null &&
             _AH < Workspace.This.APDCalibrationChannelAVM.APDHighVoltage))
            {
                _IsChAHighVoltError = true;
                _APDCalibrationProcess?.Abort();
                if (_IsShowErrorChA)
                {

                    _IsShowErrorChA = false;
                    Workspace.This.Owner.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        MessageBox.Show("APD高压故障！高压" + _AHighVolt + "标定电压" + _AH, "提示", MessageBoxButton.OK, MessageBoxImage.Error);
                    }));
                    return;
                }
            }
            #endregion High Volt Error process, cancel calibration

            #region Temper Error process, cancel calibration
            _temptrue.Add(Workspace.This.APDCalibrationChannelAVM.APDTemperature);
            if (_temptrue.Count > 9)
            {
                if (_temptrue[0] == 20 && _temptrue[1] == 20 && _temptrue[2] == 20 && _temptrue[3] == 20 && _temptrue[4] == 20 && _temptrue[5] == 20
                    && _temptrue[6] == 20 && _temptrue[7] == 20 && _temptrue[8] == 20 && _temptrue[9] == 20)
                {
                    _APDCalibrationProcess?.Abort();
                    if (_IsShowErrorChA)
                    {
                        _IsShowErrorChA = false;
                        Workspace.This.Owner.Dispatcher.BeginInvoke(new Action(() =>
                        {
                            _temptrue.Clear();
                            MessageBox.Show("温度传感器故障！", "提示", MessageBoxButton.OK, MessageBoxImage.Error);
                        }));
                        return;
                    }
                }
                _temptrue.Clear();
            }
            if (Workspace.This.APDCalibrationChannelAVM.APDTemperature < 0 || Workspace.This.APDCalibrationChannelAVM.APDTemperature > 40)
            {
                _APDCalibrationProcess?.Abort();
                if (_IsShowErrorChA)
                {
                    _IsShowErrorChA = false;
                    Workspace.This.Owner.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        MessageBox.Show("温度传感器故障！", "提示", MessageBoxButton.OK, MessageBoxImage.Error);
                    }));
                    return;
                }
            }

            #endregion Temper Error process, cancel calibration

        }

        private void BenchComm_UpdateCommOutput()
        {
            Workspace.This.APDCalibrationChannelAVM.APDOutput = _APDCalibrationPort.BenchComm.CommOutputChA.APDOutput;
            //Workspace.This.APDCalibrationChannelBVM.APDOutput = _APDCalibrationPort.BenchComm.CommOutputChB.APDOutput;
        }

        private void LaserComm_UpdateCommOutput()
        {

         Workspace.This.APDCalibrationChannelAVM.LaserPower = _APDCalibrationPort.LaserComm.CommOutputChA.LaserPower;
            Workspace.This.APDCalibrationVM.TECTemperature = _APDCalibrationPort.LaserComm.CommOutputChA.TECTemperature;
            if (!CalibrationButtonState)
            {
                if (_APDCalibrationPort.LaserComm.CommOutputChA.TECTemperature <= 255 && _APDCalibrationPort.LaserComm.CommOutputChA.TECTemperature >= 245&& IVType!=1)
                {

                    CalibrationButtonState = true;
                    APDCalibrationChannelViewModel.Emtiy();
                    tecTempflag = 100;
                    
                    //if (Workspace.This.APDCalibrationChannelAVM.CurrentAPDGain == null && Workspace.This.APDCalibrationChannelAVM.CurrentPGA == null && Workspace.This.APDCalibrationChannelAVM.APDTemperature == null)
                    //{
                    //    Workspace.This.APDCalibrationChannelAVM.IsAPDModuleAlive = true ;
                    //    Workspace.This.APDCalibrationVM.IsApdModulStateAlive(true);
                    //    IVType = 1;
                    //}
                    //SelectedIVTypeResult = IVType;
                }
                else if (IVType==1)
                {
                    //PMT
                    Workspace.This.APDCalibrationChannelAVM.IsAPDModuleAlive = true;
                    CalibrationButtonState = true;
                    Workspace.This.APDCalibrationVM.IsApdModulStateAlive(true);
                    tecTempflag = 100;
                }
                else
                {

                    if (tecTempflag == 100)
                    {
                        CalibrationButtonState = true;
                    }
                    else if (tecTempflag == 60)
                    {
                        CalibrationButtonState = false;
                        MessageBox.Show("TEC温度在一分钟内不在24.5~25.5之间");
                    }
                    tecTempflag++;
                }
            }
            //CalibrationButtonState = true;

        }

        private void _timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            if (_APDCalibrationWind != null)
            {
                TestDate = DateTime.Now.ToString("yyyy年MM月dd日");
                TestTime = DateTime.Now.ToString("HH:mm");
            }
            DateTimeNow = TestDate + "\r\n" + TestTime;

            if (_IsChAHighVoltError)
            {
                if (APDCalibrationPort.APDGainCHA != 100)
                {
                    APDCalibrationPort.APDGainCHA = 100;
                }
                else
                {
                    APDCalibrationPort.CalibrationVoltCHA = 0;
                    _IsChAHighVoltError = false;
                }
            }
            //else if (_IsChBHighVoltError)
            //{
            //    if (APDCalibrationPort.APDGainCHB != 100)
            //    {
            //        APDCalibrationPort.APDGainCHB = 100;
            //    }
            //    else
            //    {
            //        APDCalibrationPort.CalibrationVoltCHB = 0;
            //        _IsChBHighVoltError = false;
            //    }
            //}
        }

        #region Public Pro
        public TestAPDCalibration TestItemAPDCalibration
        {
            get
            {
                return _TestAPDCalibration;
            }
        }
        public APDCalibrationPort APDCalibrationPort
        {
            get
            {
                return _APDCalibrationPort;
            }
            set
            {
                if (_APDCalibrationPort != value)
                {
                    _APDCalibrationPort = value;
                }
            }
        }
        public APDCalibrationProcess APDCalibrationProcess
        {
            get
            {
                return _APDCalibrationProcess;
            }
        }
        public CalibrationRecords APDCalibrationRecords
        {
            get
            {
                return _APDCalibrationRecords;
            }
        }
        public ObservableCollection<string> CalibrationResultOptions
        {
            get
            {
                return _CalibrationResultOptions;
            }
        }
        public string SelectedCalibrationResult
        {
            get
            {
                return _SelectedCalibrationResult;
            }
            set
            {
                if (_SelectedCalibrationResult != value)
                {
                    _SelectedCalibrationResult = value;
                    RaisePropertyChanged("SelectedCalibrationResult");
                    if (_SelectedCalibrationResult == CalibrationResultOptions[1])
                    {
                        TestItemAPDCalibration.IsPassed = true;
                    }
                    else
                    {
                        TestItemAPDCalibration.IsPassed = false;
                    }
                }
            }
        }
        private ObservableCollection<int> _CaliIVTypeOptions = new ObservableCollection<int>();
        public ObservableCollection<int> IVTypeOptions
        {
            get
            {
                return _CaliIVTypeOptions;
            }
        }
        private int _SelectedIVTypeResult;
        public int SelectedIVTypeResult
        {
            get
            {
                return _SelectedIVTypeResult;
            }
            set
            {
                if (_SelectedIVTypeResult != value)
                {
                    _SelectedIVTypeResult = value;
                    RaisePropertyChanged("SelectedIVTypeResult");
                }
            }
        }
        public string DateTimeNow
        {
            get
            {
                return _DateTimeNow;
            }
            set
            {
                if (_DateTimeNow != value)
                {
                    _DateTimeNow = value;
                    RaisePropertyChanged("DateTimeNow");
                }
            }
        }
        public double? TECTemperature
        {
            get
            {
                return _TECTemperature;
            }
            set
            {
                if (_TECTemperature != value)
                {
                    _TECTemperature = value / 10;
                    Workspace.This.APDCalibrationChannelAVM.TECTemperature = _TECTemperature;
                    RaisePropertyChanged("TECTemperature");
                }
            }
        }
        public bool IsCHASelected
        {
            get
            {
                return _IsCHASelected;
            }
            set
            {
                if (_IsCHASelected != value)
                {
                    _IsCHASelected = value;
                    RaisePropertyChanged("IsCHASelected");
                    Workspace.This.APDCalibrationChannelAVM.IsAPDModuleAlive = _IsCHASelected;
                    if (_IsCHASelected == true)
                    {
                        if (_APDCalibrationPort.IsAPDCHAAlive == false)
                        {
                            MessageBox.Show("通道不可用！");
                            IsCHASelected = false;
                        }
                    }
                }
            }
        }
        //public bool IsCHBSelected
        //{
        //    get
        //    {
        //        return _IsCHBSelected;
        //    }
        //    set
        //    {
        //        if (_IsCHBSelected != value)
        //        {
        //            _IsCHBSelected = value;
        //            RaisePropertyChanged("IsCHBSelected");
        //            Workspace.This.APDCalibrationChannelBVM.IsAPDModuleAlive = _IsCHBSelected;
        //            if (_IsCHBSelected == true)
        //            {
        //                if (_APDCalibrationPort.IsAPDCHBAlive == false)
        //                {
        //                    MessageBox.Show("通道B不可用！");
        //                    IsCHBSelected = false;
        //                }
        //            }
        //        }
        //    }
        //}
        public int ReadErrorCount
        {
            get
            {
                return _ReadErrorCount;
            }

            set
            {
                if (_ReadErrorCount != value)
                {
                    _ReadErrorCount = value;
                    RaisePropertyChanged("ReadErrorCount");
                }
            }
        }
        public int WriteErrorCount
        {
            get
            {
                return _WriteErrorCount;
            }

            set
            {
                if (_WriteErrorCount != value)
                {
                    _WriteErrorCount = value;
                    RaisePropertyChanged("WriteErrorCount");
                }
            }
        }
        public string IVPCBNumber
        {
            get
            {
                return _IVPCBNumber;
            }

            set
            {
                if (_IVPCBNumber != value)
                {
                    _IVPCBNumber = value;
                    RaisePropertyChanged("IVPCBNumber");
                    APDCalibrationChannelViewModel.IVPCBNumber = value;
                }
            }
        }
        public string LaserPCBNumber
        {
            get { return _LaserPCBNumber; }
            set
            {
                if (_LaserPCBNumber != value)
                {
                    _LaserPCBNumber = value;
                    RaisePropertyChanged("LaserPCBNumber");
                    APDCalibrationChannelViewModel.LaserPCBNumber = value;
                }
            }
        }
        public string TestDate
        {
            get
            {
                return _TestDate;
            }

            set
            {
                if (_TestDate != value)
                {
                    _TestDate = value;
                    RaisePropertyChanged("TestDate");
                }
            }
        }
        public string TestTime
        {
            get
            {
                return _TestTime;
            }

            set
            {
                if (_TestTime != value)
                {
                    _TestTime = value;
                    RaisePropertyChanged("TestTime");
                }
            }
        }
        public string StartTime
        {
            get
            {
                return _StartTime;
            }
            set
            {
                if (_StartTime != value)
                {
                    _StartTime = value;
                    RaisePropertyChanged("StartTime");
                }
            }
        }
        public string EndTime
        {
            get
            {
                return _EndTime;
            }
            set
            {
                if (_EndTime != value)
                {
                    _EndTime = value;
                    RaisePropertyChanged("EndTime");
                }
            }
        }

        public bool IsChAHighVoltErrorMasked
        {
            get
            {
                return _IsChAHighVoltErrorMasked;
            }

            set
            {
                _IsChAHighVoltErrorMasked = value;
            }
        }
        public string TestOperator
        {
            get
            {
                return _TestOperator;
            }

            set
            {
                if (_TestOperator != value)
                {
                    _TestOperator = value;
                    RaisePropertyChanged("TestOperator");
                }
            }
        }
        //public bool IsChBHighVoltErrorMasked
        //{
        //    get
        //    {
        //        return _IsChBHighVoltErrorMasked;
        //    }

        //    set
        //    {
        //        _IsChBHighVoltErrorMasked = value;
        //    }
        //}

        public bool IsChATemperErrorMasked
        {
            get
            {
                return _IsChATemperErrorMasked;
            }

            set
            {
                _IsChATemperErrorMasked = value;
            }
        }
        private bool _IsAPDModuleAlive = false;
        public bool IsAPDModuleAlive
        {
            get
            {
                return _IsAPDModuleAlive;
            }
            set
            {
                if (_IsAPDModuleAlive != value)
                {
                    _IsAPDModuleAlive = value;
                    RaisePropertyChanged("IsAPDModuleAlive");
                }
            }
        }
        public void IsApdModulStateAlive(bool Alive)
        {
            if (_APDCalibrationWind != null)
            {
                Workspace.This.Owner.Dispatcher.BeginInvoke((Action)delegate
                {
                    _APDCalibrationWind.startCalibrationBtn.IsEnabled = Alive;
                });
            }

        }
        #endregion


    }
    public class TestAPDCalibration : TestItemBase
    {
        public class TestChannel : TestItemBase
        {
            private bool _IsIgnored = false;
            public bool IsIgnored
            {
                get
                {
                    return _IsIgnored;
                }

                set
                {
                    _IsIgnored = value;
                }
            }
        }
        private TestChannel _TestItemCHA;
        //private TestChannel _TestItemCHB;
        public TestAPDCalibration()
        {
            _TestItemCHA = new TestChannel();
            //_TestItemCHB = new TestChannel();
        }

        public TestChannel TestItemCHA
        {
            get
            {
                return _TestItemCHA;
            }
        }
        //public TestChannel TestItemCHB
        //{
        //    get
        //    {
        //        return _TestItemCHB;
        //    }
        //}
    }

    public class TestItemBase : ViewModelBase
    {
        private bool _IsChecking = false;
        private bool _IsChecked = false;
        private bool _IsPassed = false;
        private bool _IsNotChecking = true;
        private string _TestOperator = string.Empty;
        public virtual bool IsChecking
        {
            get
            {
                return _IsChecking;
            }
            set
            {
                if (_IsChecking != value)
                {
                    _IsChecking = value;
                    _IsNotChecking = !_IsChecking;
                    RaisePropertyChanged("IsChecking");
                    RaisePropertyChanged("IsNotChecking");
                }
            }
        }
        public bool IsNotChecking
        {
            get
            {
                return _IsNotChecking;
            }
        }
        public virtual bool IsChecked
        {
            get
            {
                return _IsChecked;
            }
            set
            {
                if (_IsChecked != value)
                {
                    _IsChecked = value;
                    RaisePropertyChanged("IsChecked");
                }
            }
        }
        public virtual bool IsPassed
        {
            get
            {
                return _IsPassed;
            }
            set
            {
                if (_IsPassed != value)
                {
                    _IsPassed = value;
                    RaisePropertyChanged("IsPassed");
                }
            }
        }
        public string TestOperator
        {
            get
            {
                return _TestOperator;
            }
            set
            {
                if (_TestOperator != value)
                {
                    _TestOperator = value;
                    RaisePropertyChanged("TestOperator");
                }
            }
        }
    }
}
