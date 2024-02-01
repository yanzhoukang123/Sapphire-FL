using Azure.APDCalibrationBench;
using Azure.ScannerTestJig.View.APDCalibration;
using Azure.ScannerTestJig.View.APDIVTempCalibration;
using Azure.WPF.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;


namespace Azure.ScannerTestJig.ViewModule.APDIVTempCalibration
{
    public class APDIVTempCalibrationViewModel : ViewModelBase
    {
        #region privert
        private RelayCommand _APDIvTempCalibrationCommand = null;
        private RelayCommand _StartCalibrationCommand = null;
        private RelayCommand _StopCalibrationCommand = null;
        private RelayCommand _FinishCalibrationCommand = null;
        private TestAPDCalibration _TestAPDCalibration = null;
        public APDIvTempCalibrationSubWind _APDIvTempCalibrationWind = null;
        private APDCalibrationPort _APDIvTempCalibrationPort = null;
        BrushConverter bc = null;
        private string _DateTimeNow;
        private string _TestDate;
        private string _TestTime;
        private string _StartTime = string.Empty;
        private string _EndTime = string.Empty;
        private System.Timers.Timer _timer;
        private byte[] NumByte = new byte[] { 0x3a, 0x02, 0x02, 0x15, 0x00, 0x00, 0x00, 0x00, 0x00, 0x3b };
        private byte[] Num2Byte = new byte[] { 0x3a, 0x02, 0x02, 0x14, 0x00, 0x00, 0x00, 0x00, 0x00, 0x3b };
        private byte[] Num3Byte = new byte[] { 0x3a, 0x02, 0x02, 0x05, 0x00, 0x00, 0x00, 0x00, 0x00, 0x3b };
        private int _ReadErrorCount = 0;
        private int _WriteErrorCount = 0;
        private string _Temp;
        public Thread tdHome = null;
        #endregion

        public APDIVTempCalibrationViewModel()
        {
            TestDate = DateTime.Now.ToString("yyyy年MM月dd日");
            TestTime = DateTime.Now.ToString("HH:mm");
            DateTimeNow = TestDate + "\r\n" + TestTime;
            _timer = new System.Timers.Timer();
            _timer.Interval = 1000;
            _timer.Elapsed += _timer_Elapsed;
            _timer.Start();
            tdHome = new Thread(FlowS);
            tdHome.IsBackground = true;
        }

        #region APDIvTempCalibration Command
        public ICommand APDIvTempCalibrationCommand
        {
            get
            {
                if (_APDIvTempCalibrationCommand == null)
                {
                    _APDIvTempCalibrationCommand = new RelayCommand(ExcuteAPDIvTempCalibrationCommand, CanExcuteAPDIvTempCalibrationCommand);
                }
                return _APDIvTempCalibrationCommand;
            }
        }
        public void ExcuteAPDIvTempCalibrationCommand(object parameter)
        {
            try
            {
                if (_APDIvTempCalibrationPort == null)
                {
                    _APDIvTempCalibrationPort =new APDCalibrationPort();
                }
                bc = new BrushConverter();
                Workspace.This.APDCalibrationVM.APDCalibrationPort.SearchApdIvTempPort(NumByte);
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
                        _APDIvTempCalibrationWind = new APDIvTempCalibrationSubWind();
                        _APDIvTempCalibrationWind.Title = Workspace.This.Owner.Title + "-APD IvTemp Calibration Kit";
                        _APDIvTempCalibrationWind.IsEnabled = false;
                        _APDIvTempCalibrationWind.ShowDialog();
                        _APDIvTempCalibrationWind = null;
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
                    Thread.Sleep(2000);
                    Temp = Workspace.This.APDCalibrationVM.APDCalibrationPort.ApdIvTempValue;
                    _APDIvTempCalibrationWind = new APDIvTempCalibrationSubWind();
                    _APDIvTempCalibrationWind.Title = Workspace.This.Owner.Title + "-APD IvTemp Calibration Kit";
                    _APDIvTempCalibrationWind.ShowDialog();
                    _APDIvTempCalibrationWind = null;
                }
                Workspace.This.APDCalibrationChannelAVM.CalibrationStepCount = 0;
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.ToString());
            }

        }
        public bool CanExcuteAPDIvTempCalibrationCommand(object parameter)
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
            _APDIvTempCalibrationWind.startCalibrationBtn.Visibility = Visibility.Hidden;
            _APDIvTempCalibrationWind.stopCalibrationBtn.Visibility = Visibility.Visible;
            tdHome.Start();
        }
        public bool CanExcuteStartCalibrationCommand(object parameter)
        {
            return true;
        }
        #endregion

        void FlowS()
        {
            Application.Current.Dispatcher.Invoke((Action)delegate
            {
                string _temp = "";
                MessageBoxResult messageBoxResult;

                // step1
                Workspace.This.APDCalibrationVM.APDCalibrationPort.ApdIvTempProtSend(NumByte);
                Thread.Sleep(500);
                Temp = (Convert.ToDouble(Workspace.This.APDCalibrationVM.APDCalibrationPort.ApdIvTempValue) / 100).ToString();
                _temp = Workspace.This.APDCalibrationVM.APDCalibrationPort.ApdIvTempValue;
                if (_temp == "0")
                {
                    //messageBoxResult = MessageBox.Show("点击确认进行下一步", "确认！", MessageBoxButton.YesNo, MessageBoxImage.Error);
                    //if (messageBoxResult == MessageBoxResult.Yes)
                    //{
                    //    //
                    //}
                    //else
                    //{
                    //    tdHome.Abort();
                    //    return;
                    //}
                }
                else if (_temp == "NotFullFilled")
                {
                    MessageBox.Show("通讯失败，请断电检查通讯线。");
                    _APDIvTempCalibrationWind.stopCalibrationBtn.Visibility = Visibility.Hidden;
                    _APDIvTempCalibrationWind.startCalibrationBtn.Visibility = Visibility.Visible;
                    tdHome.Abort();
                    return;
                }
                else
                {
                    messageBoxResult = MessageBox.Show("数据状态异常，请断电检查拨码开关后重新上电。", "确认！", MessageBoxButton.YesNo, MessageBoxImage.Error);
                    _APDIvTempCalibrationWind.stopCalibrationBtn.Visibility = Visibility.Hidden;
                    _APDIvTempCalibrationWind.startCalibrationBtn.Visibility = Visibility.Visible;
                    tdHome.Abort();
                    return;
                }
                //step2
                //Thread.Sleep(2000);
                //Workspace.This.APDCalibrationVM.APDCalibrationPort.ApdIvTempProtSend(NumByte);
                //Thread.Sleep(500);
                //Temp = Workspace.This.APDCalibrationVM.APDCalibrationPort.ApdIvTempValue;
                //if (Temp == "NotFullFilled")//两秒后没有接收到数据
                //{
                //    MessageBox.Show("通讯失败，请断电检查通讯线。");
                //    tdHome.Abort();
                //    return;
                //}
                ////step3
                //MessageBox.Show("点击确认进行下一步，点击后您需要等待20秒");
                //for (int i = 0; i < 20; i++)
                //{
                //    Workspace.This.APDCalibrationVM.APDCalibrationPort.ApdIvTempProtSend(NumByte);
                //    Thread.Sleep(1000);
                //    Temp = Workspace.This.APDCalibrationVM.APDCalibrationPort.ApdIvTempValue;
                //    if (Temp == "NotFullFilled")
                //    {
                //        MessageBox.Show("数据状态异常，请断电检查拨码开关后重新上电。。");
                //        tdHome.Abort();
                //        return;
                //    }
                //    else if (Temp != "0")
                //    {
                //        MessageBox.Show("数据状态异常，请断电检查拨码开关后重新上电。。");
                //        tdHome.Abort();
                //        return;
                //    }
                //}
                //step4
                messageBoxResult = MessageBox.Show("请将工装H1拨到10位置，点击确认进行下一步", "确认！", MessageBoxButton.YesNo, MessageBoxImage.Error);
                if (messageBoxResult == MessageBoxResult.Yes)
                {

                }
                else
                {
                    tdHome.Abort();
                    return;
                }
                for (int i = 0; i < 5; i++)
                {
                    Workspace.This.APDCalibrationVM.APDCalibrationPort.ApdIvTempProtSend(Num2Byte);
                    Thread.Sleep(500);
                    Temp = (Convert.ToDouble(Workspace.This.APDCalibrationVM.APDCalibrationPort.ApdIvTempValue) / 100).ToString();
                    _temp = Workspace.This.APDCalibrationVM.APDCalibrationPort.ApdIvTempValue;
                    if (_temp == "NotFullFilled")
                    {
                        MessageBox.Show("通讯失败，请断电检查通讯线。");
                        _APDIvTempCalibrationWind.stopCalibrationBtn.Visibility = Visibility.Hidden;
                        _APDIvTempCalibrationWind.startCalibrationBtn.Visibility = Visibility.Visible;
                        tdHome.Abort();
                        return;
                    }
                    else if (_temp == "0")
                    {
                        MessageBox.Show("数据状态异常，请断电检查拨码开关后重新上电。。");
                        _APDIvTempCalibrationWind.stopCalibrationBtn.Visibility = Visibility.Hidden;
                        _APDIvTempCalibrationWind.startCalibrationBtn.Visibility = Visibility.Visible;
                        tdHome.Abort();
                        return;
                    }
                    else
                    {
                        int result = 0x08c0;
                        if (Convert.ToDouble(_temp) < result)
                        {
                            MessageBox.Show("5次内XXXX 小于 0X08C0。已结束标定");
                            _APDIvTempCalibrationWind.stopCalibrationBtn.Visibility = Visibility.Hidden;
                            _APDIvTempCalibrationWind.startCalibrationBtn.Visibility = Visibility.Visible;
                            tdHome.Abort();
                            return;
                        }
                    }
                }

                //step5
                messageBoxResult = MessageBox.Show("请将APD板S1的拨码3拨到ON位置，点击确认下一步", "确认！", MessageBoxButton.YesNo, MessageBoxImage.Error);
                if (messageBoxResult == MessageBoxResult.Yes)
                {

                }
                else
                {
                    tdHome.Abort();
                    return;
                }
                int flat = 0;
                for (int i = 0; i < 5; i++)
                {
                    Workspace.This.APDCalibrationVM.APDCalibrationPort.ApdIvTempProtSend(NumByte);
                    Thread.Sleep(1000);
                    Temp = Workspace.This.APDCalibrationVM.APDCalibrationPort.ApdIvTempValue;
                    _temp = Workspace.This.APDCalibrationVM.APDCalibrationPort.ApdIvTempValue;
                    if (_temp == "NotFullFilled")
                    {
                        MessageBox.Show("通讯失败，请断电检查通讯线。");
                        _APDIvTempCalibrationWind.stopCalibrationBtn.Visibility = Visibility.Hidden;
                        _APDIvTempCalibrationWind.startCalibrationBtn.Visibility = Visibility.Visible;
                        tdHome.Abort();
                        return;
                    }
                    else if (_temp == "0")
                    {
                        MessageBox.Show("数据状态异常，请断电检查拨码开关后重新上电。。");
                        _APDIvTempCalibrationWind.stopCalibrationBtn.Visibility = Visibility.Hidden;
                        _APDIvTempCalibrationWind.startCalibrationBtn.Visibility = Visibility.Visible;
                        tdHome.Abort();
                        return;
                    }
                    else
                    {
                        if (Convert.ToInt32(_temp) == 4)
                        {
                            flat++;
                        }
                    }
                }
                if (flat == 5)
                {
                    messageBoxResult = MessageBox.Show("请将APD板S1的拨码1拨到ON位置，点击确认下一步", "确认！", MessageBoxButton.YesNo, MessageBoxImage.Error);
                    if (messageBoxResult == MessageBoxResult.Yes)
                    {

                    }
                    else
                    {
                        tdHome.Abort();
                        return;
                    }

                }
                //step6
                //int result1 = 0X1944;
                int result1 = 0X1949;
                int result2 = 0X193A;
                flat = 0;
                for (int i = 0; i < 5; i++)
                {
                    Workspace.This.APDCalibrationVM.APDCalibrationPort.ApdIvTempProtSend(Num3Byte);
                    Thread.Sleep(1000);
                    Temp = (Convert.ToDouble(Workspace.This.APDCalibrationVM.APDCalibrationPort.ApdIvTempValue) / 100).ToString();
                    _temp = Workspace.This.APDCalibrationVM.APDCalibrationPort.ApdIvTempValue;
                    if (_temp == "NotFullFilled")
                    {
                        MessageBox.Show("通讯失败，请断电检查通讯线。");
                        _APDIvTempCalibrationWind.stopCalibrationBtn.Visibility = Visibility.Hidden;
                        _APDIvTempCalibrationWind.startCalibrationBtn.Visibility = Visibility.Visible;
                        tdHome.Abort();
                        return;
                    }
                    else if (_temp == "0")
                    {
                        MessageBox.Show("数据状态异常，请断电检查拨码开关后重新上电。。");
                        _APDIvTempCalibrationWind.stopCalibrationBtn.Visibility = Visibility.Hidden;
                        _APDIvTempCalibrationWind.startCalibrationBtn.Visibility = Visibility.Visible;
                        tdHome.Abort();
                        return;
                    }
                    else
                    {
                        if (Convert.ToDouble(_temp) <= result1 && Convert.ToDouble(_temp) >= result2)
                        {
                            flat++;
                        }
                        else if (Convert.ToDouble(_temp) > result1 || Convert.ToDouble(_temp) < result2)
                        {
                            MessageBox.Show("标定异常，请重新标定。", "提示");
                            _APDIvTempCalibrationWind.stopCalibrationBtn.Visibility = Visibility.Hidden;
                            _APDIvTempCalibrationWind.startCalibrationBtn.Visibility = Visibility.Visible;
                            tdHome.Abort();
                            return;
                        }
                    }
                }
                if (flat == 5)
                {
                    MessageBox.Show("标定成功", "提示");
                    _APDIvTempCalibrationWind.stopCalibrationBtn.Visibility = Visibility.Hidden;
                    _APDIvTempCalibrationWind.startCalibrationBtn.Visibility = Visibility.Visible;
                    tdHome.Abort();
                    return;
                }
                else
                {
                    MessageBox.Show("标定失败", "提示");
                    _APDIvTempCalibrationWind.stopCalibrationBtn.Visibility = Visibility.Hidden;
                    _APDIvTempCalibrationWind.startCalibrationBtn.Visibility = Visibility.Visible;
                    tdHome.Abort();
                    return;
                }
            });
        }

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
            _APDIvTempCalibrationWind.stopCalibrationBtn.Visibility = Visibility.Hidden;
            _APDIvTempCalibrationWind.startCalibrationBtn.Visibility = Visibility.Visible;
            tdHome.Abort();
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
        
        }
        public bool CanExcuteFinishCalibrationCommand(object parameter)
        {
            return true;
        }
        #endregion

        #region Public Pro
        public TestAPDCalibration TestItemAPDCalibration
        {
            get
            {
                return _TestAPDCalibration;
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
        public string Temp
        {
            get
            {
                return _Temp;
            }

            set
            {
                if (_Temp != value)
                {
                    _Temp = value;
                    RaisePropertyChanged("Temp");
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

        #endregion

        private void _timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            if (_APDIvTempCalibrationWind != null)
            {
                TestDate = DateTime.Now.ToString("yyyy年MM月dd日");
                TestTime = DateTime.Now.ToString("HH:mm");
            }
            DateTimeNow = TestDate + "\r\n" + TestTime;
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
}
