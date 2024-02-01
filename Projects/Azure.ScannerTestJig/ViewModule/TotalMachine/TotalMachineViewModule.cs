using Azure.CommandLib;
using Azure.Configuration.Settings;
using Azure.EthernetCommLib;
using Azure.ImagingSystem;
using Azure.WPF.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace Azure.ScannerTestJig.ViewModule.TotalMachine
{
    class TotalMachineViewModule : ViewModelBase
    {
        #region
        private TestJigScanProcessing _ScanningProcess;
        private EthernetController _EthernetController;
        private RelayCommand _StopCommand = null;
        private RelayCommand _LEDCommand = null;
        private RelayCommand _LEDStopCommand = null;
        private bool _x_L_Limit = false;
        private bool _x_R_Limit = false;
        private bool _y_F_Limit = false;
        private bool _y_B_Limit = false;
        private bool _z_L_Limit = false;
        //private double _X_Range = 0;
        //private double _Y_Range = 0;
        private bool _IsFansDrawerSelected = false;
        private bool _IsFansBackSelected = false;
        private double _XMotorSubdivision = 0;
        private double _YMotorSubdivision = 0;
        private double _ZMotorSubdivision = 0;
        private int _XMaxValue = 0;
        private int _YMaxValue = 0;
        private int _ZMaxValue = 0;
        private Visibility _x_IsScanVisibility = Visibility.Visible;
        private Visibility _x_IsStopVisibility = Visibility.Hidden;
        private Visibility _y_IsScanVisibility = Visibility.Visible;
        private Visibility _y_IsStopVisibility = Visibility.Hidden;
        private Visibility _z_IsScanVisibility = Visibility.Visible;
        private Visibility _z_IsStopVisibility = Visibility.Hidden;
        private Visibility _led_IsScanVisibility = Visibility.Visible;
        private Visibility _led_IsStopVisibility = Visibility.Hidden;
        private bool _x_IsScanCheck = true;
        private bool _x_IsStopCheck = false;
        private bool _y_IsScanCheck = true;
        private bool _y_IsStopCheck = false;
        private bool _z_IsScanCheck = true;
        private bool _z_IsStopCheck = false;
        private bool _led_IsScanCheck = true;
        private bool _led_IsStopCheck = false;
        private bool _x_IsNullEnabled = true;
        private bool _y_IsNullEnabled = true;
        private bool _z_IsNullEnabled = true;
        public Thread tdHome = null;
        #endregion

        #region Constructors...
        public TotalMachineViewModule(EthernetController ethernetController)
        {
            _EthernetController = ethernetController;
        }
        public EthernetController EthernetDevice
        {
            get
            {
                return _EthernetController;
            }
        }
        public void InitIVControls()
        {
            _XMotorSubdivision = SettingsManager.ConfigSettings.XMotorSubdivision;
            _YMotorSubdivision = SettingsManager.ConfigSettings.YMotorSubdivision;
            _ZMotorSubdivision = SettingsManager.ConfigSettings.ZMotorSubdivision;
            _XMaxValue = SettingsManager.ConfigSettings.XMaxValue;
            _YMaxValue = SettingsManager.ConfigSettings.YMaxValue;
            _ZMaxValue = SettingsManager.ConfigSettings.ZMaxValue;
            EnabledButton = false;
            tdHome = new Thread(HomeMotor);
            tdHome.IsBackground = true;
            tdHome.Start();
        }
        #endregion

        #region MotorXCommand
        private RelayCommand _MotorXCommand = null;
        public ICommand MotorXCommand
        {
            get
            {
                if (_MotorXCommand == null)
                {
                    _MotorXCommand = new RelayCommand(ExecuteGoMotorXCommand, CanExecuteMotorXCommand);
                }

                return _MotorXCommand;
            }
        }
        public void ExecuteGoMotorXCommand(object parameter)
        {
            if (!Workspace.This.InitControlValue())
            {
                return;
            }
            Thread _GlassTopAdjThread = new Thread(_GlassTopAdjustProcess);
            _GlassTopAdjThread.IsBackground = true;
            _GlassTopAdjThread.Start(parameter);
        }
        public bool CanExecuteMotorXCommand(object parameter)
        {
            return true;
        }
        #endregion

        #region MotorYCommand 
        private RelayCommand _MotorYCommand = null;
        public ICommand MotorYCommand
        {
            get
            {
                if (_MotorYCommand == null)
                {
                    _MotorYCommand = new RelayCommand(ExecuteGoMotorYCommand, CanExecuteMotorYCommand);
                }
                return _MotorYCommand;
            }
        }

        //void CrntAtBwdLimit()
        //{
        //    while (true)
        //    {
        //        Thread.Sleep(1000);
        //        try
        //        {
        //            if (Workspace.This.MotorVM.IsNewFirmware)
        //            {
        //                if (Workspace.This.MotorVM.MotionController.CrntState[Avocado.EthernetCommLib.MotorTypes.X].AtFwdLimit)
        //                {
        //                    x_R_Limit = true;
        //                    Workspace.This.MotorVM.MotionController.SetStart(Avocado.EthernetCommLib.MotorTypes.X,
        //                        new bool[] { false });
        //                    x_IsScanCheck = true;
        //                    x_IsStopCheck = false;
        //                }
        //                if (Workspace.This.MotorVM.MotionController.CrntState[Avocado.EthernetCommLib.MotorTypes.Y].AtFwdLimit)
        //                {
        //                    y_B_Limit = true;
        //                        Workspace.This.MotorVM.MotionController.SetStart(Avocado.EthernetCommLib.MotorTypes.Y,
        //                           new bool[] { false });
        //                    y_IsScanCheck = true;
        //                    y_IsStopCheck = false;
        //                }
        //                if (Workspace.This.MotorVM.CurrentZPos>= Workspace.This.MotorVM.ZMaxValue)
        //                {
        //                    Workspace.This.MotorVM.MotionController.SetStart(Avocado.EthernetCommLib.MotorTypes.Z,
        //                       new bool[] { false });
        //                    z_IsScanCheck = true;
        //                    z_IsStopCheck = false;
        //                    z_HomeMotor();
        //                }

        //            }
        //        }
        //        catch { }
        //    }

        //}
        public void ExecuteGoMotorYCommand(object parameter)
        {

            if (!Workspace.This.InitControlValue())
            {
                return;
            }

            Thread _GlassTopAdjThread = new Thread(_GlassTopAdjustProcess);
            _GlassTopAdjThread.IsBackground = true;
            _GlassTopAdjThread.Start(parameter);
        }
        void _GlassTopAdjustProcess(object parameter)
        {

            TestJigScanParameterStruct scanParameter = new TestJigScanParameterStruct();  //Set scan parameter
            MotorType motorType = (MotorType)parameter;
            if (motorType == MotorType.X)
            {

                if (x_IsScanCheck == true)
                {
                    x_L_Limit = false;
                    x_R_Limit = false;
                    x_IsStopCheck = true;
                    x_IsScanCheck = false;
                }
                x_HomeMotor();
                x_Limit();


            }
            else if (motorType == MotorType.Y)
            {
                if (y_IsScanCheck == true)
                {
                    y_F_Limit = false;
                    y_B_Limit = false;
                    y_IsStopCheck = true;
                    y_IsScanCheck = false;
                }
                y_HomeMotor();
                y_Limit();
            }
            else if (motorType == MotorType.Z)
            {
                z_L_Limit = false;
                z_IsStopCheck = true;
                z_IsScanCheck = false;
                z_HomeMotor();
                z_Limit();
            }

        }
        public bool CanExecuteMotorYCommand(object parameter)
        {
            return true;
        }
        private void _ScanningProcess_OnScanDataReceived(string dataName)
        {

        }
        private void _ImageScanCommand_Completed(CommandLib.ThreadBase sender, CommandLib.ThreadBase.ThreadExitStat exitState)
        {
            Workspace.This.Owner.Dispatcher.BeginInvoke((Action)delegate
            {
                TestJigScanProcessing scannedThread = (sender as TestJigScanProcessing);

                if (exitState == ThreadBase.ThreadExitStat.None)
                {

                    scannedThread = (sender as TestJigScanProcessing);
                    if (scannedThread.ScanType == TestJigScanTypes.Static || scannedThread.ScanType == TestJigScanTypes.Vertical || scannedThread.ScanType == TestJigScanTypes.XAxis)
                    {

                        if (scannedThread._ScanSettings.singeName == "Z")
                        {
                            Thread _GlassTopAdjThread = new Thread(z_HomeMotor);
                            _GlassTopAdjThread.IsBackground = true;
                            _GlassTopAdjThread.Start();

                        }
                        x_IsScanCheck = true;
                        x_IsStopCheck = false;
                        y_IsScanCheck = true;
                        y_IsStopCheck = false;
                        z_IsScanCheck = true;
                        z_IsStopCheck = false;
                        x_IsNullEnabled = true;
                        y_IsNullEnabled = true;
                        z_IsNullEnabled = true;


                    }
                }
                else if (exitState == ThreadBase.ThreadExitStat.Error)
                {
                    // Oh oh something went wrong - handle the error

                    string caption = "Scanning error...";
                    string message = string.Format("Scanning error: {0}", scannedThread.Error.Message);
                    MessageBox.Show(message, caption, MessageBoxButton.OK, MessageBoxImage.Exclamation);
                }

                //Turn off all the lasers after scanning is completed
                _ScanningProcess.Completed -= new CommandLib.ThreadBase.CommandCompletedHandler(_ImageScanCommand_Completed);
                //_ImageScanCommand.CommandStatus -= new ImageScanCommand.CommandStatusHandler(_ImageScanCommand_CommandStatus);
                _ScanningProcess = null;
            });

        }
        #endregion

        #region MotorZCommand
        private RelayCommand _MotorZCommand = null;
        public ICommand MotorZCommand
        {
            get
            {
                if (_MotorZCommand == null)
                {
                    _MotorZCommand = new RelayCommand(ExecuteGoMotorZCommand, CanExecuteMotorZCommand);
                }

                return _MotorZCommand;
            }
        }
        public void ExecuteGoMotorZCommand(object parameter)
        {
            if (!Workspace.This.InitControlValue())
            {
                return;
            }
            Thread _GlassTopAdjThread = new Thread(_GlassTopAdjustProcess);
            _GlassTopAdjThread.IsBackground = true;
            _GlassTopAdjThread.Start(parameter);
        }
        public bool CanExecuteMotorZCommand(object parameter)
        {
            return true;
        }
        #endregion

        #region StopCommand 
        public ICommand StopCommand
        {
            get
            {
                if (_StopCommand == null)
                {
                    _StopCommand = new RelayCommand(ExecuteStopCommand, CanExecuteStopCommand);
                }

                return _StopCommand;
            }
        }
        public void ExecuteStopCommand(object parameter)
        {
            MotorType motorType = (MotorType)parameter;
            if (motorType == MotorType.X)
            {
                if (Workspace.This.MotorVM.IsNewFirmware)
                {
                    Workspace.This.MotorVM.MotionController.SetStart(EthernetCommLib.MotorTypes.X,
                        new bool[] { false, false, false });
                }
                x_IsScanCheck = true;
                x_IsStopCheck = false;
                x_IsNullEnabled = true;
            }
            if (motorType == MotorType.Y)
            {
                if (Workspace.This.MotorVM.IsNewFirmware)
                {
                    Workspace.This.MotorVM.MotionController.SetStart(EthernetCommLib.MotorTypes.Y,
                        new bool[] { false, false, false });
                }
                y_IsScanCheck = true;
                y_IsStopCheck = false;
                y_IsNullEnabled = true;
            }
            if (motorType == MotorType.Z)
            {
                if (Workspace.This.MotorVM.IsNewFirmware)
                {
                    Workspace.This.MotorVM.MotionController.SetStart(EthernetCommLib.MotorTypes.Z,
                        new bool[] { false, false, false });
                }
                z_IsScanCheck = true;
                z_IsStopCheck = false;
                z_IsNullEnabled = true;

            }
        }

        public bool CanExecuteStopCommand(object parameter)
        {
            return true;
        }

        #endregion

        #region LEDCommand 
        public ICommand LEDCommand
        {
            get
            {
                if (_LEDCommand == null)
                {
                    _LEDCommand = new RelayCommand(ExecuteLedCommand, CanExecuteLedCommand);
                }

                return _LEDCommand;
            }
        }
        Thread led = null;
        Thread Buzzer = null;
        public void ExecuteLedCommand(object parameter)
        {
            if (Workspace.This.MotorVM.IsNewFirmware)
            {
                IsChecdLed = true;
                led_IsScanCheck = false;
                led_IsStopCheck = true;
                led = new Thread(LedTest);
                led.IsBackground = true;
                led.Start();
                //Buzzer = new Thread(BuzzerTest);
                //Buzzer.IsBackground = true;
                //Buzzer.Start();
            }
            else
            {
                MessageBox.Show("检查网络是否连接！", "", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
        }
        bool IsChecdLed = false;
        void LedTest()
        {
            while (IsChecdLed)
            {
                EthernetDevice.SetLed(175);
                EthernetDevice.SetBuzzer(1, 25);
                // Workspace.This.SerialPorts.SetLedContoll(175);//全绿
                Thread.Sleep(3000);
                EthernetDevice.SetLed(159);
                EthernetDevice.SetBuzzer(1, 25);
                //Workspace.This.SerialPorts.SetLedContoll(159);//全红
                Thread.Sleep(3000);
                EthernetDevice.SetLed(207);
                EthernetDevice.SetBuzzer(1, 25);
                //Workspace.This.SerialPorts.SetLedContoll(207);//全蓝
                Thread.Sleep(3000);
            }
        }
        void BuzzerTest()
        {
            while (true)
            {
                EthernetDevice.SetBuzzer(1, 25);
                //Workspace.This.SerialPorts.SetBuzzerContoll(1, 25);//25*40=time   响1次
                Thread.Sleep(2000);
            }
        }
        public bool CanExecuteLedCommand(object parameter)
        {
            return true;
        }

        #endregion

        #region LEDStopCommand 
        public ICommand StopLedCommand
        {
            get
            {
                if (_LEDStopCommand == null)
                {
                    _LEDStopCommand = new RelayCommand(ExecuteLedStopCommand, CanExecuteLedStopCommand);
                }

                return _LEDStopCommand;
            }
        }
        public void ExecuteLedStopCommand(object parameter)
        {
            if (led != null)
            {
                led.Abort();
            }
            //if (Buzzer != null)
            //{
            //    Buzzer.Abort();
            //}
            if (Workspace.This.MotorVM.IsNewFirmware)
            {
                EthernetDevice.SetLed(175);//绿色
            }
            else
            {

                MessageBox.Show("检查网络是否连接！", "", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            led_IsScanCheck = true;
            led_IsStopCheck = false;
        }

        public bool CanExecuteLedStopCommand(object parameter)
        {
            return true;
        }

        #endregion

        #region public pro
        public bool x_IsNullEnabled
        {
            get { return _x_IsNullEnabled; }
            set
            {
                if (_x_IsNullEnabled != value)
                {
                    _x_IsNullEnabled = value;
                    RaisePropertyChanged("x_IsNullEnabled");
                }
            }
        }

        public bool y_IsNullEnabled
        {
            get { return _y_IsNullEnabled; }
            set
            {
                if (_y_IsNullEnabled != value)
                {
                    _y_IsNullEnabled = value;
                    RaisePropertyChanged("y_IsNullEnabled");
                }
            }
        }


        public bool z_IsNullEnabled
        {
            get { return _z_IsNullEnabled; }
            set
            {
                if (_z_IsNullEnabled != value)
                {
                    _z_IsNullEnabled = value;
                    RaisePropertyChanged("z_IsNullEnabled");
                }
            }
        }
        public bool x_IsScanCheck
        {
            get { return _x_IsScanCheck; }
            set
            {
                if (_x_IsScanCheck != value)
                {
                    _x_IsScanCheck = value;
                    if (_x_IsScanCheck)
                    {
                        x_IsScanVisibility = Visibility.Visible;
                    }
                    else
                    {
                        x_IsScanVisibility = Visibility.Hidden;
                    }
                }
            }
        }
        public bool x_IsStopCheck
        {
            get { return _x_IsStopCheck; }
            set
            {
                if (_x_IsStopCheck != value)
                {
                    _x_IsStopCheck = value;
                    if (_x_IsStopCheck)
                    {
                        x_IsStopVisibility = Visibility.Visible;
                    }
                    else
                    {
                        x_IsStopVisibility = Visibility.Hidden;
                    }
                }
            }
        }
        public bool y_IsScanCheck
        {
            get { return _y_IsScanCheck; }
            set
            {
                if (_y_IsScanCheck != value)
                {
                    _y_IsScanCheck = value;
                    if (_y_IsScanCheck)
                    {
                        y_IsScanVisibility = Visibility.Visible;
                    }
                    else
                    {
                        y_IsScanVisibility = Visibility.Hidden;
                    }
                }
            }
        }

        public bool y_IsStopCheck
        {
            get { return _y_IsStopCheck; }
            set
            {
                if (_y_IsStopCheck != value)
                {
                    _y_IsStopCheck = value;
                    if (_y_IsStopCheck)
                    {
                        y_IsStopVisibility = Visibility.Visible;
                    }
                    else
                    {
                        y_IsStopVisibility = Visibility.Hidden;
                    }
                }
            }
        }
        public bool z_IsScanCheck
        {
            get { return _z_IsScanCheck; }
            set
            {
                if (_z_IsScanCheck != value)
                {
                    _z_IsScanCheck = value;
                    if (_z_IsScanCheck)
                    {
                        z_IsScanVisibility = Visibility.Visible;
                    }
                    else
                    {
                        z_IsScanVisibility = Visibility.Hidden;
                    }
                }
            }
        }

        public bool z_IsStopCheck
        {
            get { return _z_IsStopCheck; }
            set
            {
                if (_z_IsStopCheck != value)
                {
                    _z_IsStopCheck = value;
                    if (_z_IsStopCheck)
                    {
                        z_IsStopVisibility = Visibility.Visible;
                    }
                    else
                    {
                        z_IsStopVisibility = Visibility.Hidden;
                    }
                }
            }
        }
        public bool led_IsScanCheck
        {
            get { return _led_IsScanCheck; }
            set
            {
                if (_led_IsScanCheck != value)
                {
                    _led_IsScanCheck = value;
                    if (_led_IsScanCheck)
                    {
                        led_IsScanVisibility = Visibility.Visible;
                    }
                    else
                    {
                        led_IsScanVisibility = Visibility.Hidden;
                    }
                }
            }
        }

        public bool led_IsStopCheck
        {
            get { return _led_IsStopCheck; }
            set
            {
                if (_led_IsStopCheck != value)
                {
                    _led_IsStopCheck = value;
                    if (_led_IsStopCheck)
                    {
                        led_IsStopVisibility = Visibility.Visible;
                    }
                    else
                    {
                        led_IsStopVisibility = Visibility.Hidden;
                    }
                }
            }
        }

        public Visibility x_IsScanVisibility
        {
            get { return _x_IsScanVisibility; }
            set
            {
                if (_x_IsScanVisibility != value)
                {
                    _x_IsScanVisibility = value;
                    RaisePropertyChanged("x_IsScanVisibility");

                }
            }
        }
        public Visibility x_IsStopVisibility
        {
            get { return _x_IsStopVisibility; }
            set
            {
                if (_x_IsStopVisibility != value)
                {
                    _x_IsStopVisibility = value;
                    RaisePropertyChanged("x_IsStopVisibility");

                }
            }
        }
        public Visibility y_IsScanVisibility
        {
            get { return _y_IsScanVisibility; }
            set
            {
                if (_y_IsScanVisibility != value)
                {
                    _y_IsScanVisibility = value;
                    RaisePropertyChanged("y_IsScanVisibility");

                }
            }
        }
        public Visibility y_IsStopVisibility
        {
            get { return _y_IsStopVisibility; }
            set
            {
                if (_y_IsStopVisibility != value)
                {
                    _y_IsStopVisibility = value;
                    RaisePropertyChanged("y_IsStopVisibility");

                }
            }
        }
        public Visibility z_IsScanVisibility
        {
            get { return _z_IsScanVisibility; }
            set
            {
                if (_z_IsScanVisibility != value)
                {
                    _z_IsScanVisibility = value;
                    RaisePropertyChanged("z_IsScanVisibility");

                }
            }
        }
        public Visibility z_IsStopVisibility
        {
            get { return _z_IsStopVisibility; }
            set
            {
                if (_z_IsStopVisibility != value)
                {
                    _z_IsStopVisibility = value;
                    RaisePropertyChanged("z_IsStopVisibility");

                }
            }
        }
        public Visibility led_IsScanVisibility
        {
            get { return _led_IsScanVisibility; }
            set
            {
                if (_led_IsScanVisibility != value)
                {
                    _led_IsScanVisibility = value;
                    RaisePropertyChanged("led_IsScanVisibility");

                }
            }
        }
        public Visibility led_IsStopVisibility
        {
            get { return _led_IsStopVisibility; }
            set
            {
                if (_led_IsStopVisibility != value)
                {
                    _led_IsStopVisibility = value;
                    RaisePropertyChanged("led_IsStopVisibility");

                }
            }
        }
        public bool x_L_Limit
        {
            get { return _x_L_Limit; }
            set
            {
                if (_x_L_Limit != value)
                {
                    _x_L_Limit = value;
                    RaisePropertyChanged("x_L_Limit");
                }
            }
        }
        public bool x_R_Limit
        {
            get { return _x_R_Limit; }
            set
            {
                if (_x_R_Limit != value)
                {
                    _x_R_Limit = value;
                    RaisePropertyChanged("x_R_Limit");
                }
            }
        }
        public bool y_F_Limit
        {
            get { return _y_F_Limit; }
            set
            {
                if (_y_F_Limit != value)
                {
                    _y_F_Limit = value;
                    RaisePropertyChanged("y_F_Limit");
                }
            }
        }
        public bool y_B_Limit
        {
            get { return _y_B_Limit; }
            set
            {
                if (_y_B_Limit != value)
                {
                    _y_B_Limit = value;
                    RaisePropertyChanged("y_B_Limit");
                }
            }
        }

        public bool z_L_Limit
        {
            get { return _z_L_Limit; }
            set
            {
                if (_z_L_Limit != value)
                {
                    _z_L_Limit = value;
                    RaisePropertyChanged("z_L_Limit");
                }
            }
        }
        public bool IsFansDrawerSelected
        {
            get { return _IsFansDrawerSelected; }
            set
            {
                if (Workspace.This.MotorVM.IsNewFirmware)
                {
                    if (_IsFansDrawerSelected != value)
                    {
                        _IsFansDrawerSelected = value;
                        if (_IsFansDrawerSelected == true)
                        {
                            EthernetDevice.GetAllRadiatorTemperatures();
                            double rad = EthernetDevice.RadiatorTemperatures[LaserChannels.ChannelC];
                            EthernetDevice.SetFanTemperature(LaserChannels.ChannelC, rad - 1);
                        }
                        else
                        {
                            EthernetDevice.GetAllRadiatorTemperatures();
                            double rad = EthernetDevice.RadiatorTemperatures[LaserChannels.ChannelC];
                            EthernetDevice.SetFanTemperature(LaserChannels.ChannelC, rad + 1);

                        }
                        RaisePropertyChanged("IsFansDrawerSelected");
                    }
                }
                else
                {

                    MessageBox.Show("检查网络是否连接！", "", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

            }
        }
        public bool IsFansBackSelected
        {
            get { return _IsFansBackSelected; }
            set
            {
                if (Workspace.This.MotorVM.IsNewFirmware)
                {
                    if (_IsFansBackSelected != value)
                    {
                        _IsFansBackSelected = value;
                        if (_IsFansBackSelected == true)
                        {
                            EthernetDevice.GetAllRadiatorTemperatures();
                            double rad = EthernetDevice.RadiatorTemperatures[LaserChannels.ChannelC];
                            EthernetDevice.SetFanTemperature(LaserChannels.ChannelC, rad - 1);
                        }
                        else
                        {
                            EthernetDevice.GetAllRadiatorTemperatures();
                            double rad = EthernetDevice.RadiatorTemperatures[LaserChannels.ChannelC];
                            EthernetDevice.SetFanTemperature(LaserChannels.ChannelC, rad + 1);

                        }
                        RaisePropertyChanged("IsFansBackSelected");
                    }
                }
                else
                {

                    MessageBox.Show("检查网络是否连接！", "", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

            }
        }
        #endregion

        #region IsEnabled
        private bool _EnabledButton = false;
        public bool EnabledButton
        {
            get { return _EnabledButton; }
            set
            {
                _EnabledButton = value;
                RaisePropertyChanged("EnabledButton");
            }
        }
        #endregion
        void x_HomeMotor()
        {
            bool _tempCurrent = true;
            Workspace.This.MotorVM.ExecuteHomeCommand(MotorType.X);
            while (_tempCurrent)
            {
                Thread.Sleep(500);
                if (Workspace.This.MotorVM.MotionController.CrntState[EthernetCommLib.MotorTypes.X].AtHome)
                {
                    x_L_Limit = true;
                    _tempCurrent = false;
                }
                else
                {

                    _tempCurrent = true;
                }
            }

        }
        void x_Limit()
        {

            int Abs = (int)(Workspace.This.MotorVM.XMaxValue * _XMotorSubdivision);
            int speed = (int)Math.Round(SettingsManager.ConfigSettings.MotorSettings[0].Speed * SettingsManager.ConfigSettings.XMotorSubdivision);
            int MotorXAcce = (int)Math.Round(SettingsManager.ConfigSettings.MotorSettings[0].Accel * SettingsManager.ConfigSettings.XMotorSubdivision);
            if (Workspace.This.MotorVM.MotionController.AbsoluteMoveSingleMotion(EthernetCommLib.MotorTypes.X,
            256, speed, MotorXAcce, MotorXAcce, Abs, true, false) == false)
            {
                MessageBox.Show("Failed to Set new position");
            }
            bool _tempCurrent = true;
            while (_tempCurrent)
            {
                Thread.Sleep(500);
                if (Workspace.This.MotorVM.MotionController.CrntState[EthernetCommLib.MotorTypes.X].AtFwdLimit)
                {
                    x_R_Limit = true;
                    Workspace.This.MotorVM.MotionController.SetStart(EthernetCommLib.MotorTypes.X,
                        new bool[] { false });
                    x_IsScanCheck = true;
                    x_IsStopCheck = false;
                    _tempCurrent = false;
                }
                else
                {

                    _tempCurrent = true;
                }
            }

        }
        void y_HomeMotor()
        {
            bool _tempCurrent = true;
            Workspace.This.MotorVM.ExecuteHomeCommand(MotorType.Y);
            while (_tempCurrent)
            {
                Thread.Sleep(500);

                if (Workspace.This.MotorVM.MotionController.CrntState[EthernetCommLib.MotorTypes.Y].AtHome)
                {
                    y_F_Limit = true;
                    _tempCurrent = false;
                }
                else
                {
                    _tempCurrent = true;
                }
            }

        }
        void y_Limit()
        {
            int Abs = (int)(300 * _YMotorSubdivision);
            int speed = (int)Math.Round(SettingsManager.ConfigSettings.MotorSettings[1].Speed * SettingsManager.ConfigSettings.YMotorSubdivision);
            int MotorYAcce = (int)Math.Round(SettingsManager.ConfigSettings.MotorSettings[1].Accel * SettingsManager.ConfigSettings.YMotorSubdivision);

            //Workspace.This.ApdVM.APDTransfer.MotionControl.AbsoluteMove(Hats.APDCom.MotionTypes.Y,
            //    256, (int)_MotorYSpeed, (int)_MotorYAccel, (int)_MotorYAccel, (int)_AbsYPos, false, true);
            if (Workspace.This.MotorVM.MotionController.AbsoluteMoveSingleMotion(EthernetCommLib.MotorTypes.Y,
            256, (int)speed, (int)MotorYAcce, (int)MotorYAcce, (int)Abs, true, false) == false)
            {
                MessageBox.Show("Failed to Set new position");
            }
            bool _tempCurrent = true;
            while (_tempCurrent)
            {
                Thread.Sleep(500);
                if (Workspace.This.MotorVM.MotionController.CrntState[EthernetCommLib.MotorTypes.Y].AtFwdLimit)
                {
                    y_B_Limit = true;
                    Workspace.This.MotorVM.MotionController.SetStart(EthernetCommLib.MotorTypes.Y,
                       new bool[] { false });
                    y_IsScanCheck = true;
                    y_IsStopCheck = false;
                    _tempCurrent = false;
                }
                else
                {

                    _tempCurrent = true;
                }
            }

        }
        void z_HomeMotor()
        {
            bool _tempCurrent = true;
            Workspace.This.MotorVM.ExecuteHomeCommand(MotorType.Z);
            while (_tempCurrent)
            {
                Thread.Sleep(500);

                if (Workspace.This.MotorVM.MotionController.CrntState[EthernetCommLib.MotorTypes.Z].AtHome)
                {
                    z_L_Limit = true;
                    _tempCurrent = false;
                }
                else
                {
                    _tempCurrent = true;
                }
            }

        }
        void z_Limit()
        {
            int Abs = (int)(Workspace.This.MotorVM.ZMaxValue * _ZMotorSubdivision);
            int speed = (int)Math.Round(SettingsManager.ConfigSettings.MotorSettings[2].Speed * SettingsManager.ConfigSettings.ZMotorSubdivision);
            int MotorZAcce = (int)Math.Round(SettingsManager.ConfigSettings.MotorSettings[2].Accel * SettingsManager.ConfigSettings.ZMotorSubdivision);
            if (Workspace.This.MotorVM.MotionController.AbsoluteMoveSingleMotion(EthernetCommLib.MotorTypes.Z,
            256, speed, MotorZAcce, MotorZAcce, Abs, true, false) == false)
            {
                MessageBox.Show("Failed to Set new position");
            }

            bool _tempCurrent = true;
            while (_tempCurrent)
            {
                Thread.Sleep(500);
                if (Workspace.This.MotorVM.CurrentZPos >= Workspace.This.MotorVM.ZMaxValue)
                {
                    Workspace.This.MotorVM.MotionController.SetStart(EthernetCommLib.MotorTypes.Z,
                       new bool[] { false });
                    z_IsScanCheck = true;
                    z_IsStopCheck = false;
                    _tempCurrent = false;
                    z_HomeMotor();
                }

                else
                {

                    _tempCurrent = true;
                }
            }

        }
        void IsCheck(bool ische)
        {
            x_L_Limit = ische;
            x_R_Limit = ische;
            y_F_Limit = ische;
            y_B_Limit = ische;
            z_L_Limit = ische;
            IsFansDrawerSelected = ische;
            IsFansBackSelected = ische;

        }
        void HomeMotor()
        {
            bool _tempCurrent = true;
            //sif (SettingsManager.ConfigSettings.HomeMotionsAtLaunchTime)
            {
                if (!Workspace.This.MotorVM.HomeXYZmotor())
                {
                    //MessageBox.Show("无法回到Home位置，请检查连接", "", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                while (_tempCurrent)
                {
                    Thread.Sleep(500);
                    if (Workspace.This.MotorVM.MotionController.CrntState[EthernetCommLib.MotorTypes.X].AtHome &&
                  Workspace.This.MotorVM.MotionController.CrntState[EthernetCommLib.MotorTypes.Y].AtHome &&
                  Workspace.This.MotorVM.MotionController.CrntState[EthernetCommLib.MotorTypes.Z].AtHome)
                    {
                        EnabledButton = true;
                        _tempCurrent = false;
                    }
                    else
                    {
                        _tempCurrent = true;
                    }
                }
            }
            //else
            //{
            //    _tempCurrent = false;
            //}


        }
    }
}
