/************************************************************************

   AvalonDock

   Copyright (C) 2007-2013 Xceed Software Inc.

   This program is provided to you under the terms of the New BSD
   License (BSD) as published at http://avalondock.codeplex.com/license 

   For more features, controls, and fast professional support,
   pick up AvalonDock in Extended WPF Toolkit Plus at http://xceed.com/wpf_toolkit

   Stay informed: follow @datagrid on Twitter or Like facebook.com/datagrids

  **********************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using System.Windows.Input;
using Microsoft.Win32;
using System.IO;
//using System.IO.Ports;
using System.Windows;
//using System.Windows.Controls;  //ContentControl
using System.Windows.Media.Imaging;
using System.ComponentModel;    //BackgroundWorker
using System.Threading;     //Thead
using System.Text.RegularExpressions;   //Regex
using System.Windows.Controls;  //PrintDialog
using System.Windows.Media;     //DrawingVisual
using System.Globalization;     //CultureInfo
using System.Windows.Threading; //DispatcherTimer
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;
//using Xceed.Wpf.AvalonDock.Layout;
using Azure.WPF.Framework;
//using DrawToolsLib;
//using ColorFont;
//using Azure.CameraLib;
//using Azure.CommandLib;
//using Azure.Controller;
//using Azure.ImagingSystem;
using Azure.Image.Processing;
//using Azure.WindowsAPI.Interop;     //WindowsInvoke
//using Azure.Resources;
using Azure.Utilities;
using TaskDialogInterop;
using Azure.Ipp.Imaging;
using Azure.Configuration.Settings;
using Azure.ScannerEUI.View;
using Azure.EthernetCommLib;
using LogW;

namespace Azure.ScannerEUI.ViewModel
{
    //These value must match application tab index
    public enum ApplicationTabType
    {
        Imaging = 0,
        ScanChart = 1,
        Gallery = 2,
    }

    class Workspace : ViewModelBase, IDisposable
    {
        // Flag: Has Dispose already been called?
        bool disposed = false;
        // Instantiate a SafeHandle instance.
        SafeHandle handle = new SafeFileHandle(IntPtr.Zero, true);

        #region Private data...

        private int _SelectedTabIndex = (int)ApplicationTabType.Imaging;
        private FileViewModel _ActiveDocument = null;
        private AbstractPaneViewModel activePane = null;

        private const int _MaxOpenFileAllowed = 10;
        private int _FileNameCount = 0;
        private int _FileNameSetCount = 0;

        //private CommandMediator _CommandMediator = new CommandMediator();

        private bool _IsLoading = false;
        //private bool _IsSaving = false;
        private bool _Is8bpp = false;
        private bool _Is16bpp = false;
        private bool _Is32bppImage = false;

        private LoadingAnimationManager _LoadingAnimManager = null;

        private RelayCommand _OpenCommand = null;
        private RelayCommand _NewCommand = null;
        private RelayCommand _ShowCropAdornerCommand = null;
        private RelayCommand _ZoomInCommand = null;
        private RelayCommand _ZoomOutCommand = null;
        private RelayCommand _ZoomFitCommand = null;

        //private RelayCommand _ImageResizeCommand = null;
        private RelayCommand _ShowOSKBCommand = null;

        // Drawing commands
        //private RelayCommand _DrawingUndoCommand = null;
        //private RelayCommand _DrawingRedoCommand = null;
        //private RelayCommand _DrawingDeleteCommand = null;
        
        //private bool _IsActiveDocument = false;
        //private double _DefaultZoomFactor = 0.1;
        //private bool _IsDarkroom = false;
        //private bool _IsSettings = false;

        //private ManualContrast _ManualContrast = null;
        //private ObservableCollection<LineWidthType> _LineWidthOptions = new ObservableCollection<LineWidthType>();

        //private TimeSpan _ConnectWaitTime = new TimeSpan(0, 0, 45);
        //private Thread _ConnectThread = null;

        //private bool _HasControllerBoard = true;
        //private string _ControlBoardPortName = "COM1";
        //private System.IO.Ports.SerialPort _ControlBoardSerialPort = new System.IO.Ports.SerialPort();

        private bool _IsProcessingContrast = false;
        private bool _IsImagingMode = true;
        private bool _IsScannerMode = true;
        //private bool _IsCameraMode = false;
        private bool _IsPreparing = false;
        //private bool _IsReadyScanning = false;
        private bool _IsScanning = false;
        private bool _IsCapturing = false;
        private bool _IsContinuous = false;
		private bool _MotorIsAlive = false;

        private string _DoorStatus = string.Empty;

        private DispatcherTimer _DispatcherTimer = new DispatcherTimer();
        private DateTime _CaptureStartTime;
        private string _CapturingTopStatusText = string.Empty;
        private string _CurrentScanWorkIndex = string.Empty;
        private string _EstimatedTimeRemaining = "";
        private double _EstimatedCaptureTime = 0.0;
        private double _PercentComplete = 0.0;

        private MotorViewModel _MotorViewModel = null;
        private ScannerViewModel _ScannerViewModel = null;
        private ApdViewModel _ApdViewModel = null;
        private IvViewModel _IVSensorViewModel = null;
        private ParameterSetupViewModel _ParamViewModel = null; 
        private NewParameterSetupViewModel _NewParamViewModel = null;
        private TransportLockViewModel _TransportLockViewModel = null;
        private ZAutomaticallyFocalViewModel _ZAutomaticallyFocal = null;
        private ImageRotatingPrcessViewModel _ImageRotatingPrcess = null;
        private AgingViewModel _AgingVM;
        private bool _IsAuthenticated = false;
        private Visibility _WorkIndexTitleVisBility = Visibility.Hidden;
        private EthernetController _EthernetController;
        private double _EthernetTransactionRate;
        private bool _ScanIsAlive = false;
        //（FW Version 1.1.0.0） If the firmware version is 1.1, the added UI for "Optical Module Power Down Button", "Top Cover Lock Status", "Top Cover Magnet Status", and "Optical Module Power Status" will be displayed, while the UI for "Optical Module Power Status" will be hidden
        //如何固件版本是1.1则将增加的“光学模块下电按钮”和“顶盖锁状态”和“顶盖磁吸状态”和“光学模块电源状态”UI显示，相反则隐藏
        private Visibility _VesionVisFlag = Visibility.Visible;   //当硬件版本是1.1时显示光学模块下电按钮和相应的功能，When the hardware version is 1.1, the optical module power-off button and corresponding functions are displayed
        private Visibility _OldVesionVisFlag = Visibility.Visible;   //隐藏或者显示版本1.0.0.0中对应的UI和功能 Hide or display the corresponding UI and functions in version 1.0.0.0
        private Visibility _LEDVesionVisFlag = Visibility.Visible;   //当硬件版本是1.1，并且LED版本是0.0.0.0则隐藏光学模块下电按钮 When the hardware version is 1.1, the optical module power-off button is displayed, and the LED version is 0.0.0.0, the optical module power-off button is hidden
        private bool _DisconnectDeviceEnable = true; //光学模块按钮启用状态  Optical module button activation status （FW Version 1.1.0.0）
        private bool _TopCoverLockStatus;             //顶盖锁状态(硬件版本V1.1)  monitor Top cover lock status（FW Version 1.1.0.0）
        private bool _TopMagneticStatus;         //顶盖磁吸状态(硬件版本V1.1)            monitor Top cover magnetic suction status （FW Version 1.1.0.0）
        private bool _OpticalModulePowerStatus;   //光学模块下电状态 False下电，True上电  Optical module power-off status False power-off, True power-off （FW Version 1.1.0.0）
        private bool _OpticalModulePowerMonitor;   //光学模块电源监测 Optical module power monitoring  False下电，True上电  Optical module power-off status False power-off, True power-off （FW Version 1.1.0.0）
        //Indicates the device status, while Fasle indicates that the optical module power off button has been manually pressed. At this time, the software pop-up reminds the device that it has been powered off and needs to restart the software. The software is always set to gray and is not allowed to be used
        //表示设备状态，Fasle表示手动按下了光学模块下电按钮，此时软件弹框提醒设备已经下电请重启软件，软件始终置为灰色不允许使用
        private string _DefaultHWversion = "1.1.0.0";
        private double _AmbientTemperatureCH1 = 0;
        private double _AmbientTemperatureCH2 = 0;
        #endregion

        #region Constructors...

        public Workspace()
        {
            //
            // Initialize the 'Open Documents' pane view-model.
            //
            //this.OpenDocumentsPaneViewModel = new OpenDocumentsPaneViewModel();
            //
            // Add view-models for panes to the 'Panes' collection.
            //
            this.Panes = new ObservableCollection<AbstractPaneViewModel>();
            //this.Panes.Add(this.OpenDocumentsPaneViewModel);

            this.Files = new ObservableCollection<FileViewModel>();

            _EthernetController = new EthernetController();
            _EthernetController.OnDataRateChanged += _EthernetController_OnDataRateChanged;

            _MotorViewModel = new MotorViewModel();
            _ScannerViewModel = new ScannerViewModel();
            _ApdViewModel = new ApdViewModel(_EthernetController);
            _IVSensorViewModel = new IvViewModel(_EthernetController);
             _ParamViewModel = new ParameterSetupViewModel();
            _NewParamViewModel = new NewParameterSetupViewModel();
            _TransportLockViewModel = new TransportLockViewModel();
            _ZAutomaticallyFocal = new ZAutomaticallyFocalViewModel();
            _ImageRotatingPrcess = new ImageRotatingPrcessViewModel();
            _AgingVM = new AgingViewModel();
            // image capturing status
            _DispatcherTimer.Tick += new EventHandler(_DispatcherTimer_Tick);
            _DispatcherTimer.Interval = new TimeSpan(0, 0, 1);

            //IsStartUpInCameraMode = false;

            _MultiplexVm = new MultiplexViewModel();
        }

        private void _EthernetController_OnDataRateChanged()
        {
            EthernetDataRate = Math.Round(_EthernetController.ReadingRate);
        }

        #endregion

        protected override void Dispose(bool disposing)
        {
            if (disposed)
                return;

            if (disposing)
            {
                handle.Dispose();
                // Free any other managed objects here.
                //

            }

            // Free any unmanaged objects here.
            //

            disposed = true;
            // Call base class implementation.
            base.Dispose(disposing);
        }

        static Workspace _this = new Workspace();
        public static Workspace This
        {
            get { return _this; }
            set { _this = value; }
        }

        //private ObservableCollection<FileViewModel> _Files = new ObservableCollection<FileViewModel>();
        //private ReadOnlyObservableCollection<FileViewModel> _ReadOnyFiles = null;
        //public ReadOnlyObservableCollection<FileViewModel> Files
        //{
        //    get
        //    {
        //        if (_ReadOnyFiles == null)
        //        {
        //            _ReadOnyFiles = new ReadOnlyObservableCollection<FileViewModel>(_Files);
        //        }

        //        return _ReadOnyFiles;
        //    }
        //}

        /// <summary>
        /// View-models for documents.
        /// </summary>
        public ObservableCollection<FileViewModel> Files
        {
            get;
            private set;
        }

        /// <summary>
        /// View-models for panes.
        /// </summary>
        public ObservableCollection<AbstractPaneViewModel> Panes
        {
            get;
            private set;
        }

        /// <summary>
        /// View-model for the active pane.
        /// </summary>
        public AbstractPaneViewModel ActivePane
        {
            get
            {
                return activePane;
            }
            set
            {
                if (activePane == value)
                {
                    return;
                }

                activePane = value;

                RaisePropertyChanged("ActivePane");
            }
        }

        /// <summary>
        /// View-model for the 'Open Documents' pane.
        /// </summary>
        //public OpenDocumentsPaneViewModel OpenDocumentsPaneViewModel
        //{
        //    get;
        //    private set;
        //}

        public Action CloseAction { get; set; }
        public MainWindow Owner { get; set; }
        public string CompanyName { get; set; }
        public string ProductName { get; set; }
        public string ProductVersion { get; set; }

        public bool MotorIsAlive
        {
            get
            {
                return _MotorIsAlive;
            }
            set
            {
                    _MotorIsAlive = value;
                    RaisePropertyChanged("MotorIsAlive"); 
            }
            
        }
        public int SelectedTabIndex
        {
            get { return _SelectedTabIndex; }
            set
            {
                if (_SelectedTabIndex != value)
                {
                    _SelectedTabIndex = value;
                    RaisePropertyChanged("SelectedTabIndex");
                    //RaisePropertyChanged("IsImagingStudio");
                    if (_SelectedTabIndex == (int)ApplicationTabType.Imaging || _SelectedTabIndex == (int)ApplicationTabType.ScanChart)
                    {
                        IsImagingMode = true;
                    }
                    else if (_SelectedTabIndex == (int)ApplicationTabType.Gallery)
                    {
                        IsImagingMode = false;
                    }
                }
            }
        }

        //public bool IsStartUpInCameraMode { get; set; }

        #region 
        public bool DeviceStatus
        {
            get
            {
                return DeviceStatus;
            }
            set
            {
                DeviceStatus = value;
            }
        }

        //（FW Version 1.1.0.0）
        public string DefaultHWversion
        {
            get
            {
                return _DefaultHWversion;
            }
            set
            {
                _DefaultHWversion = value;
            }
        }
        //Hide or display the Scan button Stop button Remaining Spent UI
        //扫描区域UI("Scan buttion  Stop buttion Remaining Spent")隐藏或显示
        public bool ScanIsAlive
        {
            get
            {
                return _ScanIsAlive;
            }
            set
            {
                _ScanIsAlive = value;
                RaisePropertyChanged("ScanIsAlive");
            }
        }

        //If the firmware version is 1.1, the added UI for "Optical Module Power Down Button", "Top Cover Lock Status", "Top Cover Magnet Status", and "Optical Module Power Status" will be displayed, while the UI for "Optical Module Power Status" will be hidden
        //如果固件版本是1.1则将增加的“光学模块下电按钮”和“顶盖锁状态”和“顶盖磁吸状态”和“光学模块电源状态”UI显示，相反则隐藏
        public Visibility VesionVisFlag
        {
            get
            {
                return _VesionVisFlag;

            }
            set
            {
                if (_VesionVisFlag != value)
                {
                    _VesionVisFlag = value;
                    RaisePropertyChanged("VesionVisFlag");
                }
            }
        }
        public Visibility OldVesionVisFlag
        {
            get
            {
                return _OldVesionVisFlag;

            }
            set
            {
                if (_OldVesionVisFlag != value)
                {
                    _OldVesionVisFlag = value;
                    RaisePropertyChanged("OldVesionVisFlag");
                }
            }
        }
        //当硬件版本是1.1，并且LED版本是0.0.0.0则隐藏光学模块下电按钮 When the hardware version is 1.1, the optical module power-off button is displayed, and the LED version is 0.0.0.0, the optical module power-off button is hidden
        public Visibility LEDVesionVisFlag
        {
            get
            {
                return _LEDVesionVisFlag;

            }
            set
            {
                if (_LEDVesionVisFlag != value)
                {
                    _LEDVesionVisFlag = value;
                    RaisePropertyChanged("LEDVesionVisFlag");
                }
            }
        }
        //光学模块按钮启动状态  Optical module button activation status （FW Version 1.1.0.0）
        public bool DisconnectDeviceEnable
        {
            get
            {
                return _DisconnectDeviceEnable;

            }
            set
            {
                if (_DisconnectDeviceEnable != value)
                {
                    _DisconnectDeviceEnable = value;
                    RaisePropertyChanged("DisconnectDeviceEnable");
                }
            }
        }
        //Top cover  status（FW Version 1.1.0.0）   顶盖状态(硬件版本V1.1)
        public bool TopCoverLockStatus
        {
            get { return _TopCoverLockStatus; }
            set
            {
                if (_TopCoverLockStatus != value)
                {
                    _TopCoverLockStatus = value;
                    RaisePropertyChanged("TopCoverLockStatus");
                }
            }
        }
        // Front cover status （FW Version 1.1.0.0） 前盖状态(硬件版本V1.1)
        private bool IsTopMagneticOpen = false;
        private bool IsTopMagneticClose = false;
        public bool TopMagneticStatus
        {
            get { return _TopMagneticStatus; }
            set
            {
                if (_TopMagneticStatus != value)
                {
                    _TopMagneticStatus = value;
                    //After the optical module is powered off
                    if (OpticalModulePowerStatus == false)
                    {
                        if (value)
                        {
                            IsTopMagneticOpen = true;
                        }
                        else
                        {
                            IsTopMagneticClose = true;
                        }
                    }
                    RaisePropertyChanged("TopMagneticStatus");
                }
            }
        }
        //Rear panel fan 1  （FW Version 1.1.0.0） 
        public double AmbientTemperatureCH1
        {
            get { return _AmbientTemperatureCH1; }
            set
            {
                if (_AmbientTemperatureCH1 != value)
                {
                    _AmbientTemperatureCH1 = value;
                    RaisePropertyChanged("AmbientTemperatureCH1");
                }
            }
        }
        //Rear panel fan 2 （FW Version 1.1.0.0）
        public double AmbientTemperatureCH2
        {
            get { return _AmbientTemperatureCH2; }
            set
            {
                if (_AmbientTemperatureCH2 != value)
                {
                    _AmbientTemperatureCH2 = value;
                    RaisePropertyChanged("AmbientTemperatureCH2");
                }
            }
        }
        //Optical module power Monitor （FW Version 1.1.0.0）  光学模块电源监测(硬件版本V1.1)
        public bool OpticalModulePowerMonitor
        {

            get { return _OpticalModulePowerMonitor; }
            set
            {
                if (_OpticalModulePowerMonitor != value)
                {
                    _OpticalModulePowerMonitor = value;
                }
            }
        }

        //首次打开软件 Opening the software for the first time
        private bool OneLoadInitSplashScreen = false;
        //Optical module power status （FW Version 1.1.0.0）  光学模块电源状态(硬件版本V1.1)
        public bool OpticalModulePowerStatus
        {

            get { return _OpticalModulePowerStatus; }
            set
            {
                if (_OpticalModulePowerStatus != value)
                {
                    _OpticalModulePowerStatus = value;
                    if (_OpticalModulePowerStatus == true)
                    {
                        //When opening the software for the first time OneLoadInitSplashScreen==false
                        //首次打开软件时 OneLoadInitSplashScreen==false
                        if (!OneLoadInitSplashScreen)
                        {
                            //进入登录界面并输入密码后在跳转到MainWindow
                            //After entering the login interface and entering the password, jump to MainWindow
                            LoadInitSplashScreen();
                        }
                        else
                        {
                            // Open the document in a different thread
                            NoShowLoadInit();

                        }
                        //After successfully opening the software for the first time, set OneLoadInitSplashScreen==true
                        //首次打开软件成功后将 OneLoadInitSplashScreen==true
                        OneLoadInitSplashScreen = true;
                        //After successfully opening the software or powering on the optical module, retrieve the information of the IV board and Laser module again
                        //每次软件打开成功后或者光学模块上电成功后，重新获取一次IV板子信息和Laser模块信息

                        //Get "OtherSettings" parameter
                        Workspace.This.NewParameterVM.GetOtherSetting();
                        if (Workspace.This.EthernetController.OpticalModulePowerStatus)
                        {
                            This.MotorVM.AbsZPos = This.EthernetController.DeviceProperties.ZFocusPosition;
                            This.MotorVM.AbsXPos = This.EthernetController.DeviceProperties.LogicalHomeX;
                            This.MotorVM.AbsYPos = This.EthernetController.DeviceProperties.LogicalHomeY;
                            Workspace.This.IVVM.SensorML1 = EthernetController.IvSensorTypes[IVChannels.ChannelC];//C代表L通道     Enumeration C represents an L channel 
                            Workspace.This.IVVM.SensorMR1 = EthernetController.IvSensorTypes[IVChannels.ChannelA];//A代表R1通道    Enumeration A represents an R1 channel
                            Workspace.This.IVVM.SensorMR2 = EthernetController.IvSensorTypes[IVChannels.ChannelB];//B代表R2通道    Enumeration B represents an R2 channel
                            Workspace.This.IVVM.SensorSNL1 = EthernetController.IvSensorSerialNumbers[IVChannels.ChannelC].ToString("X8");//C代表L通道 Enumeration C represents an L channel 
                            Workspace.This.IVVM.SensorSNR1 = EthernetController.IvSensorSerialNumbers[IVChannels.ChannelA].ToString("X8");//A代表R1通道  Enumeration A represents an R1 channel
                            Workspace.This.IVVM.SensorSNR2 = EthernetController.IvSensorSerialNumbers[IVChannels.ChannelB].ToString("X8");//B代表R2通道 Enumeration B represents an R2 channel
                            Workspace.This.IVVM.LaserSNL1 = EthernetController.LaserSerialNumbers[LaserChannels.ChannelC].ToString("X8");//C代表L通道 Enumeration C represents an L channel 
                            Workspace.This.IVVM.LaserSNR1 = EthernetController.LaserSerialNumbers[LaserChannels.ChannelA].ToString("X8");//A代表R1通道 Enumeration A represents an R1 channel
                            Workspace.This.IVVM.LaserSNR2 = EthernetController.LaserSerialNumbers[LaserChannels.ChannelB].ToString("X8");//B代表R2通道  Enumeration B represents an R2 channel
                            Workspace.This.IVVM.WL1 = (int)EthernetController.LaserWaveLengths[LaserChannels.ChannelC];//C代表L通道 Enumeration C represents an L channel 
                            Workspace.This.IVVM.WR1 = (int)EthernetController.LaserWaveLengths[LaserChannels.ChannelA];//A代表R1通道 Enumeration A represents an R1 channel
                            Workspace.This.IVVM.WR2 = (int)EthernetController.LaserWaveLengths[LaserChannels.ChannelB];//B代表R2通道  Enumeration B represents an R2 channel
                            Workspace.This.IVVM.SensorRadTemperaTureR2 = 0;
                            Workspace.This.IVVM.SensorTemperatureR2 = "0";
                            Workspace.This.IVVM.SensorRadTemperaTureR1 = 0;
                            Workspace.This.IVVM.SensorTemperatureR1 = "0";
                            Workspace.This.IVVM.SensorRadTemperaTureL1 = 0;
                            Workspace.This.IVVM.SensorTemperatureL1 = "0";
                            //Reset PGA and Gain
                            Workspace.This.IVVM.ResetPGAandGain();
                            Workspace.This.IVVM.VisbleAndEnable();
                            // Work has completed. You can now interact with the UI
                            Application.Current.Dispatcher.Invoke((Action)delegate
                            {
                                StopWaitAnimation();
                                Workspace.This.MotorIsAlive = true;
                                ScanIsAlive = true;
                                DisconnectDeviceEnable = true;
                                _IsLoading = false;
                            });
                            //If the firmware version is 1.1.0.0 and the LED version number is "254.255.255.255", perform the following operation
                            if (Workspace.This.ScannerVM.LEDVersion == Workspace.This.NewParameterVM.Str16Code)
                            {
                                Workspace.This.LEDVesionVisFlag = Visibility.Hidden;
                                Workspace.This.NewParameterVM.LEDVesionVisFlag = Visibility.Hidden;
                            }
                            Workspace.This.OldVesionVisFlag = Visibility.Hidden;
                        }
                        else
                        {
                            if (Workspace.This.ScannerVM.HWversion == "1.0.0.0")
                            {
                                Workspace.This.NewParameterVM.VesionVisFlag = Visibility.Hidden;
                                Workspace.This.VesionVisFlag = Visibility.Hidden;
                                Workspace.This.OldVesionVisFlag = Visibility.Visible;
                                Workspace.This.MotorIsAlive = true;
                                ScanIsAlive = true;
                            }
                            else
                            {
                                IsTopMagneticOpen = false;
                                IsTopMagneticClose = false;
                                Workspace.This.OldVesionVisFlag = Visibility.Hidden;
                                ScanIsAlive = false;
                                this.MotorIsAlive = false;
                            }
                        }
                    }
                    else
                    {
                        //Get "OtherSettings" parameter
                        //Workspace.This.NewParameterVM.GetOtherSetting();
                        if (Workspace.This.ScannerVM.HWversion == "1.0.0.0")
                        {
                            Workspace.This.NewParameterVM.VesionVisFlag = Visibility.Hidden;
                            Workspace.This.VesionVisFlag = Visibility.Hidden;
                            Workspace.This.OldVesionVisFlag = Visibility.Visible;
                            Workspace.This.MotorIsAlive = true;
                            ScanIsAlive = true;
                        }
                        else
                        {
                            //If the firmware version is 1.1.0.0 and the LED version number is "254.255.255.255", perform the following operation
                            if (Workspace.This.ScannerVM.LEDVersion == Workspace.This.NewParameterVM.Str16Code)
                            {
                                Workspace.This.Owner.Dispatcher.BeginInvoke((Action)delegate
                                {
                                    Window window = new Window();
                                    MessageBox.Show(window, "connection fail！\n", "warning");
                                    Workspace.This.CloseAppliction();
                                });
                            }
                            else
                            {
                                IsTopMagneticOpen = false;
                                IsTopMagneticClose = false;
                                Workspace.This.OldVesionVisFlag = Visibility.Hidden;
                                ScanIsAlive = false;
                                this.MotorIsAlive = false;
                                //The front panel buttons are in a powered on state, and only the power down operation has been performed on the software
                                //前面板按钮是通电状态，只在软件上操作了下电操作
                                if (Workspace.This.EthernetController.DevicePowerStatus)
                                {
                                    Application.Current.Dispatcher.Invoke((Action)delegate
                                    {
                                        StartWaitAnimation("Please wait ...");
                                        _IsLoading = true;
                                    });
                                    MonitorOpticalModule();
                                }
                            }
                        }
                    }
                }
            }
        }

        // The software is opened for the first time, with a Progress bar 
        private ProgressDialogHelper _ProgressDialogHelper = null;
        private int _temp = 0;
        /// <summary>
        /// ProgressValue And string Red,string Black And Message
        /// </summary>
        /// <param name="ProgressValue"></param>
        /// <param name="Message"></param>
        private void ProgressValueAndMessage(int Value, string Color, string Message)
        {
            _ProgressDialogHelper.SetMessage(Color, Message);
            for (; _temp < Value; _temp++)
            {
                System.Threading.Thread.Sleep(50);
                _ProgressDialogHelper.SetValue(Value);
            }
        }
        private void LoadInitSplashScreen()
        {
            try
            {
                Owner.Dispatcher.Invoke((Action)delegate
                {

                    bool _tempCurrent = true;
                    _ProgressDialogHelper = new ProgressDialogHelper();
                    _ProgressDialogHelper.Show(() =>
                    {
                        ProgressValueAndMessage(10, "Black", "System Initializing…");

                        //首次打开软件 Opening the software for the first time
                        if (!Workspace.This.ConnectEthernetSlave())
                        {
                            ProgressValueAndMessage(30, "Red", "Failed to connect to the main board.\n" + Workspace.This.EthernetController.ErrorMessage);
                            Log.Fatal(this, "Failed to connect to the main board.…");
                            return;
                        }
                        Workspace.This.MotorVM.IsNewFirmware = true;//isconnect
                        ProgressValueAndMessage(40, "Black", "Connect to Mainboard… Succeeded…");
                        if (!Workspace.This.EthernetController.GetSystemVersions())
                        {
                            ProgressValueAndMessage(40, "Red", "SystemVersions…Failed…");
                            Log.Fatal(this, "SystemVersions…Failed.…");
                            return;
                        }
                        Workspace.This.ScannerVM.FPGAVersion = Workspace.This.EthernetController.FWVersion;
                        Workspace.This.ScannerVM.HWversion = Workspace.This.EthernetController.HWVersion;
                        //硬件版本是1.0.0.0时 不支持读取当前的顶部锁状态和顶部磁吸状态，光学模块下电状态 （FW Version 1.1.0.0）
                        if (Workspace.This.ScannerVM.HWversion == DefaultHWversion)
                        {
                            //只有主板是1.1.0.0时获取LED版本号，如果获取LED版本号失败就默认使用老版本PC的功能。
                            //Only when the motherboard is 1.1.0.0, obtain the LED version number. If obtaining the LED version number fails, it defaults to using the functionality of the old version PC.
                            if (!Workspace.This.EthernetController.GetLEDVersions())
                            {
                                Workspace.This.ScannerVM.LEDVersion = Workspace.This.NewParameterVM.Str16Code;
                                //ProgressValueAndMessage(45, "Red", "GetLEDVersions…Failed…");
                                //Log.Fatal(this, "GetLEDVersions…Failed.…");
                                //return;
                            }
                            else
                            {
                                ProgressValueAndMessage(45, "Black", "GetLEDVersions… Succeeded…");
                                Workspace.This.ScannerVM.LEDVersion = Workspace.This.EthernetController.LEDVersion;
                            }
                            if (Workspace.This.EthernetController.DevicePowerStatus == false)
                            {
                                ProgressValueAndMessage(100, "Red", "Please restart the software after the device is powered on!…");
                                return;
                            }
                            ProgressValueAndMessage(50, "Black", "SystemVersions… Succeeded…");
                            bool OpticalModulePowerStatus = Workspace.This.EthernetController.OpticalModulePowerStatus;
                            if (!OpticalModulePowerStatus)
                            {
                                //If the firmware version is 1.1.0.0 and the LED version number is "254.255.255.255", perform the following operation
                                if (Workspace.This.ScannerVM.LEDVersion == Workspace.This.NewParameterVM.Str16Code)
                                {
                                    ProgressValueAndMessage(100, "Red", "Please restart the software after the device is powered on!…");
                                    return;
                                }
                                else
                                {
                                    ProgressValueAndMessage(100, "Black", "The Optical module is not powered on…");
                                    //删除多余用来创建GIF的图像
                                    //Remove unnecessary images used to create GIFs
                                    Workspace.This.ImageRotatingPrcessVM.DirectoryUpdate();
                                    _ProgressDialogHelper.CloseProgressDialog();
                                    Workspace.This.MotorIsAlive = false;
                                    ScanIsAlive = false;
                                }
                            }
                        }
                        else
                        {

                        }
                        Workspace.This.MotorVM.InitMotionController();
                        //软件启动XYZ电机回到原点
                        //Software starts XYZ motor back to Home
                        if (SettingsManager.ConfigSettings.HomeMotionsAtLaunchTime)
                        {
                            if (!Workspace.This.MotorVM.MotionController.CrntState[EthernetCommLib.MotorTypes.X].AtHome ||
                               !Workspace.This.MotorVM.MotionController.CrntState[EthernetCommLib.MotorTypes.Y].AtHome ||
                               !Workspace.This.MotorVM.MotionController.CrntState[EthernetCommLib.MotorTypes.Z].AtHome)
                            {
                                ProgressValueAndMessage(60, "Black", "Home Motion X, Y and Z…");
                                if (!Workspace.This.MotorVM.HomeXYZmotor())
                                {
                                    ProgressValueAndMessage(50, "Red", "Home Motion X, Y and Z…Failed…");
                                    return;
                                }
                                while (_tempCurrent)
                                {
                                    Thread.Sleep(500);

                                    if (Workspace.This.MotorVM.MotionController.CrntState[EthernetCommLib.MotorTypes.X].AtHome &&
                                  Workspace.This.MotorVM.MotionController.CrntState[EthernetCommLib.MotorTypes.Y].AtHome &&
                                  Workspace.This.MotorVM.MotionController.CrntState[EthernetCommLib.MotorTypes.Z].AtHome)
                                    {
                                        _tempCurrent = false;
                                        ProgressValueAndMessage(70, "Black", "Home Motion X, Y and Z…Succeeded…");
                                    }
                                    else
                                    {
                                        _tempCurrent = true;
                                    }
                                }
                            }
                            else
                            {
                                _tempCurrent = false;
                            }
                        }
                        else
                        {
                            _tempCurrent = false;
                        }
                        //获取所有IV板子的信息
                        //Get information about all IV boards
                        ProgressValueAndMessage(80, "Black", "Identify Optic Modules A, B and C…Succeeded…");
                        Workspace.This.EthernetController.GetAllIvModulesInfo();
                        ProgressValueAndMessage(90, "Black", "LaserModulseInfo…Succeeded…");
                        //获取所有通道激光信息
                        //Get all channel laser information
                        Workspace.This.EthernetController.GetAllLaserModulseInfo();
                        Thread.Sleep(1000);
                        ProgressValueAndMessage(100, "Black", "Please Wait For System Preparation…");
                        Thread.Sleep(3000);
                        if (!_tempCurrent)
                        {
                            //删除多余用来创建GIF的图像
                            //Remove unnecessary images used to create GIFs
                            Workspace.This.ImageRotatingPrcessVM.DirectoryUpdate();
                            _ProgressDialogHelper.CloseProgressDialog();
                        }
                    });
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                ProgressValueAndMessage(0, "Red", " Error   ProgressDialog…");
                _ProgressDialogHelper.CloseProgressDialog();
                _ProgressDialogHelper.WorkerThreadAbort();
            }
        }
        //Retrieve module information after powering on the optical module
        //光学模块上电后重新获取模块信息（FW Version 1.1.0.0）
        private void NoShowLoadInit()
        {
            try
            {
                if (This.MotorVM.IsNewFirmware)
                {
                    //Reconnect Socket
                    EthernetController.Disconnect();
                    if (!Workspace.This.ReConnectEthernetSlave())
                    {
                        Log.Fatal(this, "Failed to connect to the main board.…");
                        return;
                    }
                    Thread.Sleep(10000);
                    //获取所有IV板子的信息
                    //Get information about all IV boards
                    Workspace.This.EthernetController.GetAllIvModulesInfo();
                    //获取所有通道激光信息
                    //Get all channel laser information
                    Workspace.This.EthernetController.GetAllLaserModulseInfo();
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        //关闭程序 Close program （FW Version 1.1.0.0）
        public void CloseAppliction()
        {
            if (This.IsPreparing)
            {
                ScannerViewModel viewModel = This.ScannerVM;
                viewModel.ExecuteStopScanCommand(null);
            }
            if (This.IsScanning)
            {
                ScannerViewModel viewModel = This.ScannerVM;
                viewModel.ExecuteStopScanCommand(null);
            }
            CloseAll();
            SettingsManager.OnExit();
            System.Environment.Exit(0);
        }
        //Monitor whether the current optical module has been successfully powered down
        private void MonitorOpticalModule()
        {
            void msgSend()
            {
                int index = 0;
                //状态如果不一样，光学模块下电失败 (They should always keep the same state) 
                // If the status is inconsistent within 5 seconds, it indicates that the optical module has failed to power down, which may be a hardware issue
                while (Workspace.This.EthernetController.OpticalModulePowerStatus != Workspace.This.EthernetController.OpticalModulePowerMonitor)
                {
                    Thread.Sleep(500);
                    index++;
                    if (index == 10)
                    {
                        break;
                    }
                }
                if (index == 10)
                {
                    Application.Current.Dispatcher.Invoke((Action)delegate
                    {
                        StopWaitAnimation();
                        _IsLoading = false;
                        string message = "Optical module power fail. Please turn off the device power and try again!";
                        Window window = new Window();
                        MessageBox.Show(window, message);
                        _EthernetController.SetShutdown(0);  //上电 Optical module Power up
                        Thread.Sleep(1000);
                        if (Workspace.This.IsScanning)
                        {
                            ScannerViewModel viewModel = Workspace.This.ScannerVM;
                            viewModel.ExecuteStopScanCommand(null);
                        }
                        Workspace.This.Owner.Dispatcher.BeginInvoke((Action)delegate
                        {
                            Workspace.This.CloseAppliction();
                        });
                    });
                }
                else
                {
                    Application.Current.Dispatcher.Invoke((Action)delegate
                    {
                        StopWaitAnimation();
                        _IsLoading = false;
                    });
                    DisconnectDevice();
                }
            }
            Thread td_msg = new Thread(msgSend);
            td_msg.Start();
        }
        //Pop up a message pop-up message box without interrupting program execution （FW Version 1.1.0.0）
        private void DisconnectDevice()
        {
            void msgSend()
            {
                MessageBoxResult boxResult = MessageBoxResult.None;
                //The optical module is powered on
                if (_EthernetController.OpticalModulePowerStatus)
                {
                    Application.Current.Dispatcher.Invoke((Action)delegate
                    {
                        Window window = new Window();
                        boxResult = MessageBox.Show(window, "Do you want to replace/install/uninstall the optical modules ?", "Warning", MessageBoxButton.YesNo);
                        if (boxResult == MessageBoxResult.Yes)
                        {
                            StartWaitAnimation("Please wait X Y and Z be Home…");
                            _IsLoading = true;
                            DisconnectDeviceEnable = false;
                            bool _tempCurrent = true;
                            //软件下电启动XYZ电机回到原点，方便换模块
                            //Software starts XYZ motor back to Home ，Convenient replace optical modules
                            if (!Workspace.This.MotorVM.MotionController.CrntState[EthernetCommLib.MotorTypes.X].AtHome ||
                                                 !Workspace.This.MotorVM.MotionController.CrntState[EthernetCommLib.MotorTypes.Y].AtHome ||
                                                 !Workspace.This.MotorVM.MotionController.CrntState[EthernetCommLib.MotorTypes.Z].AtHome)
                            {
                                if (!Workspace.This.MotorVM.HomeXYZmotor())
                                {
                                    return;
                                }
                                while (_tempCurrent)
                                {
                                    Thread.Sleep(500);

                                    if (Workspace.This.MotorVM.MotionController.CrntState[EthernetCommLib.MotorTypes.X].AtHome &&
                                  Workspace.This.MotorVM.MotionController.CrntState[EthernetCommLib.MotorTypes.Y].AtHome &&
                                  Workspace.This.MotorVM.MotionController.CrntState[EthernetCommLib.MotorTypes.Z].AtHome)
                                    {
                                        _tempCurrent = false;
                                    }
                                    else
                                    {
                                        _tempCurrent = true;
                                    }
                                }
                            }
                            Workspace.This.IVVM.TurnOffAllLasers();//关闭所有激光器 Turn off all lasers
                            //for (int i = 0; i < 10; i++)
                            //{
                            //    Thread.Sleep(500);
                            //    Console.WriteLine("下电之前状态:"+Workspace.This.EthernetController.OpticalModulePowerStatus + "-" + "控制:"+Workspace.This.EthernetController.OpticalModulePowerMonitor);
                            //}
                            _EthernetController.SetShutdown(1);  //下电 optical module Power Down
                            StopWaitAnimation();
                            _IsLoading = false;
                        }
                    });

                }
                else
                {
                    //The optical module is powered off
                    Application.Current.Dispatcher.Invoke((Action)delegate
                    {
                        Window window = new Window();
                        window.Topmost = true;
                        boxResult = MessageBox.Show(window, "Please open the front door and replace/install/uninstall the optical modules. Done ?", "Warning", MessageBoxButton.YesNo);
                        if (boxResult == MessageBoxResult.Yes)
                        {
                            //Manually open and close the front door of the device when the module is powered down
                            //在模块下电过程中，您操作了前门开和关，这是换模块必须要执行的操作
                            if (TopMagneticStatus)    //The front door is open
                            {
                                Window window1 = new Window();
                                window1.Topmost = true;
                                boxResult = MessageBox.Show(window1, "Power on with the front door not closed ?", "Warning", MessageBoxButton.YesNo);
                                if (boxResult == MessageBoxResult.Yes)
                                {
                                    Application.Current.Dispatcher.Invoke((Action)delegate
                                    {
                                        StartWaitAnimation("Please wait System Initializing…");
                                        _IsLoading = true;
                                    });
                                    _EthernetController.SetShutdown(0);  //上电 Optical module Power up
                                }
                                else
                                {
                                    DisconnectDevice();
                                }
                            }
                            else if (IsTopMagneticOpen && IsTopMagneticClose)  //Open the front door and close it again  打开前门再关上
                            {
                                //The front door is closed
                                if (!TopMagneticStatus)
                                {
                                    Application.Current.Dispatcher.Invoke((Action)delegate
                                    {
                                        StartWaitAnimation("Please wait System Initializing…");
                                        _IsLoading = true;
                                    });
                                    _EthernetController.SetShutdown(0);  //上电 Optical module Power up
                                }
                            }
                            else if (!IsTopMagneticOpen || !IsTopMagneticClose)  //There is no operation on the front door    前门没有任何操作
                            {
                                //The front door is closed
                                if (!TopMagneticStatus)
                                {
                                    Window window2 = new Window();
                                    window2.Topmost = true;
                                    boxResult = MessageBox.Show(window2, "optical module  successfully replace ?", "Warning", MessageBoxButton.YesNo);
                                    if (boxResult == MessageBoxResult.Yes)
                                    {
                                        Application.Current.Dispatcher.Invoke((Action)delegate
                                        {
                                            StartWaitAnimation("Please wait System Initializing…");
                                            _IsLoading = true;
                                        });
                                        _EthernetController.SetShutdown(0);  //上电 Optical module Power up
                                    }
                                    else
                                    {
                                        DisconnectDevice(); //
                                    }
                                }
                            }

                        }
                        else
                        {
                            //The front panel buttons are in a powered on state, and only the power down operation has been performed on the software
                            //前面板按钮是通电状态，只在软件上操作了下电操作
                            if (Workspace.This.EthernetController.DevicePowerStatus)
                            {
                                DisconnectDevice();
                            }
                        }
                    });
                }
            }
            Thread td_msg = new Thread(msgSend);
            td_msg.Start();
        }

        //获取othe Setting中的三个仓位参数  Obtain the three bin parameters in the Other Setting 
        public void PreloadModuleInformation()
        {

            StartWaitAnimation("Loading...");
            This.NewParameterVM.OtherSettingBtnIsEnable = false;
            _IsLoading = true;
            BackgroundWorker worker = new BackgroundWorker();
            // Open the document in a different thread
            worker.DoWork += (o, ea) =>
            {
                This.NewParameterVM.LIVFirmwareVersionSN = "NaN";
                This.NewParameterVM.LIVFirmwareSN = "NaN";
                This.NewParameterVM.LWavelength = 0;
                This.NewParameterVM.LTEControlTemperature = 0;
                This.NewParameterVM.LTECMaximumCoolingCurrent = 0;
                This.NewParameterVM.LOpticalPowerLessThanOrEqual15mWKp = 0;
                This.NewParameterVM.LOpticalPowerLessThanOrEqual15mWKi = 0;
                This.NewParameterVM.LOpticalPowerLessThanOrEqual15mWKd = 0;
                This.NewParameterVM.LOpticalPowerGreaterThan15mWKp = 0;
                This.NewParameterVM.LOpticalPowerGreaterThan15mWKi = 0;
                This.NewParameterVM.LOpticalPowerGreaterThan15mWKd = 0;
                This.NewParameterVM.LLaserMaxCurrent = 0;
                This.NewParameterVM.LLaserMinCurrent = 0;
                This.NewParameterVM.LTECControlKp = 0;
                This.NewParameterVM.LTECControlKi = 0;
                This.NewParameterVM.LTECControlKd = 0;


                This.NewParameterVM.R1IVFirmwareVersionSN = "NaN";
                This.NewParameterVM.R1IVFirmwareSN = "NaN";
                This.NewParameterVM.R1Wavelength = 0;
                This.NewParameterVM.R1TEControlTemperature = 0;
                This.NewParameterVM.R1TECMaximumCoolingCurrent = 0;
                This.NewParameterVM.R1OpticalPowerLessThanOrEqual15mWKp = 0;
                This.NewParameterVM.R1OpticalPowerLessThanOrEqual15mWKi = 0;
                This.NewParameterVM.R1OpticalPowerLessThanOrEqual15mWKd = 0;
                This.NewParameterVM.R1OpticalPowerGreaterThan15mWKp = 0;
                This.NewParameterVM.R1OpticalPowerGreaterThan15mWKi = 0;
                This.NewParameterVM.R1OpticalPowerGreaterThan15mWKd = 0;
                This.NewParameterVM.R1LaserMaxCurrent = 0;
                This.NewParameterVM.R1LaserMinCurrent = 0;
                This.NewParameterVM.R1TECControlKp = 0;
                This.NewParameterVM.R1TECControlKi = 0;
                This.NewParameterVM.R1TECControlKd = 0;


                This.NewParameterVM.R2IVFirmwareVersionSN = "NaN";
                This.NewParameterVM.R2IVFirmwareSN = "NaN";
                This.NewParameterVM.R2Wavelength = 0;
                This.NewParameterVM.R2TEControlTemperature = 0;
                This.NewParameterVM.R2TECMaximumCoolingCurrent = 0;
                This.NewParameterVM.R2OpticalPowerLessThanOrEqual15mWKp = 0;
                This.NewParameterVM.R2OpticalPowerLessThanOrEqual15mWKi = 0;
                This.NewParameterVM.R2OpticalPowerLessThanOrEqual15mWKd = 0;
                This.NewParameterVM.R2OpticalPowerGreaterThan15mWKp = 0;
                This.NewParameterVM.R2OpticalPowerGreaterThan15mWKi = 0;
                This.NewParameterVM.R2OpticalPowerGreaterThan15mWKd = 0;
                This.NewParameterVM.R2LaserMaxCurrent = 0;
                This.NewParameterVM.R2LaserMinCurrent = 0;
                This.NewParameterVM.R2TECControlKp = 0;
                This.NewParameterVM.R2TECControlKi = 0;
                This.NewParameterVM.R2TECControlKd = 0;

                EthernetController.IVOpticalModuleSerialNumber[IVChannels.ChannelA] = "0";
                EthernetController.IVOpticalModuleSerialNumber[IVChannels.ChannelB] = "0";
                EthernetController.IVOpticalModuleSerialNumber[IVChannels.ChannelC] = "0";

                EthernetController.IVEstimatedVersionNumberBoard[IVChannels.ChannelA] = "0";
                EthernetController.IVEstimatedVersionNumberBoard[IVChannels.ChannelB] = "0";
                EthernetController.IVEstimatedVersionNumberBoard[IVChannels.ChannelC] = "0";

                EthernetController.LaserWaveLengths[LaserChannels.ChannelA] = 0;
                EthernetController.LaserWaveLengths[LaserChannels.ChannelB] = 0;
                EthernetController.LaserWaveLengths[LaserChannels.ChannelC] = 0;

                EthernetController.TECControlTemperature[LaserChannels.ChannelA] = 0;
                EthernetController.TECControlTemperature[LaserChannels.ChannelB] = 0;
                EthernetController.TECControlTemperature[LaserChannels.ChannelC] = 0;

                EthernetController.TECMaximumCurrent[LaserChannels.ChannelA] = 0;
                EthernetController.TECMaximumCurrent[LaserChannels.ChannelB] = 0;
                EthernetController.TECMaximumCurrent[LaserChannels.ChannelC] = 0;

                EthernetController.AllOpticalPowerLessThanOrEqual15mWKp[LaserChannels.ChannelA] = 0;
                EthernetController.AllOpticalPowerLessThanOrEqual15mWKp[LaserChannels.ChannelB] = 0;
                EthernetController.AllOpticalPowerLessThanOrEqual15mWKp[LaserChannels.ChannelC] = 0;

                EthernetController.AllOpticalPowerLessThanOrEqual15mWKi[LaserChannels.ChannelA] = 0;
                EthernetController.AllOpticalPowerLessThanOrEqual15mWKi[LaserChannels.ChannelB] = 0;
                EthernetController.AllOpticalPowerLessThanOrEqual15mWKi[LaserChannels.ChannelC] = 0;

                EthernetController.AllOpticalPowerLessThanOrEqual15mWKd[LaserChannels.ChannelA] = 0;
                EthernetController.AllOpticalPowerLessThanOrEqual15mWKd[LaserChannels.ChannelB] = 0;
                EthernetController.AllOpticalPowerLessThanOrEqual15mWKd[LaserChannels.ChannelC] = 0;

                EthernetController.AllOpticalPowerGreaterThan15mWKp[LaserChannels.ChannelA] = 0;
                EthernetController.AllOpticalPowerGreaterThan15mWKp[LaserChannels.ChannelB] = 0;
                EthernetController.AllOpticalPowerGreaterThan15mWKp[LaserChannels.ChannelC] = 0;

                EthernetController.AllOpticalPowerGreaterThan15mWKi[LaserChannels.ChannelA] = 0;
                EthernetController.AllOpticalPowerGreaterThan15mWKi[LaserChannels.ChannelB] = 0;
                EthernetController.AllOpticalPowerGreaterThan15mWKi[LaserChannels.ChannelC] = 0;

                EthernetController.AllOpticalPowerGreaterThan15mWKd[LaserChannels.ChannelA] = 0;
                EthernetController.AllOpticalPowerGreaterThan15mWKd[LaserChannels.ChannelB] = 0;
                EthernetController.AllOpticalPowerGreaterThan15mWKd[LaserChannels.ChannelC] = 0;

                EthernetController.AllGetLaserMaximumCurrent[LaserChannels.ChannelA] = 0;
                EthernetController.AllGetLaserMaximumCurrent[LaserChannels.ChannelB] = 0;
                EthernetController.AllGetLaserMaximumCurrent[LaserChannels.ChannelC] = 0;

                EthernetController.AllGetLaserMinimumCurrent[LaserChannels.ChannelA] = 0;
                EthernetController.AllGetLaserMinimumCurrent[LaserChannels.ChannelB] = 0;
                EthernetController.AllGetLaserMinimumCurrent[LaserChannels.ChannelC] = 0;

                EthernetController.TECRefrigerationControlParameterKp[LaserChannels.ChannelA] = 0;
                EthernetController.TECRefrigerationControlParameterKp[LaserChannels.ChannelB] = 0;
                EthernetController.TECRefrigerationControlParameterKp[LaserChannels.ChannelC] = 0;

                EthernetController.TECRefrigerationControlParameterKi[LaserChannels.ChannelA] = 0;
                EthernetController.TECRefrigerationControlParameterKi[LaserChannels.ChannelB] = 0;
                EthernetController.TECRefrigerationControlParameterKi[LaserChannels.ChannelC] = 0;

                EthernetController.TECRefrigerationControlParameterKd[LaserChannels.ChannelA] = 0;
                EthernetController.TECRefrigerationControlParameterKd[LaserChannels.ChannelB] = 0;
                EthernetController.TECRefrigerationControlParameterKd[LaserChannels.ChannelC] = 0;
                int sleeptime = 200;
                //获取所有通道激光信息
                //Get all channel laser information
                Workspace.This.EthernetController.GetAllLaserModulseInfo();
                Thread.Sleep(sleeptime);
                This.NewParameterVM.LWavelength = (int)EthernetController.LaserWaveLengths[LaserChannels.ChannelC];
                This.NewParameterVM.R1Wavelength = (int)EthernetController.LaserWaveLengths[LaserChannels.ChannelA];
                This.NewParameterVM.R2Wavelength = (int)EthernetController.LaserWaveLengths[LaserChannels.ChannelB];
                Workspace.This.IVVM.WL1 = (int)EthernetController.LaserWaveLengths[LaserChannels.ChannelC];//C代表L通道 Enumeration C represents an L channel 
                Workspace.This.IVVM.WR1 = (int)EthernetController.LaserWaveLengths[LaserChannels.ChannelA];//A代表R1通道 Enumeration A represents an R1 channel
                Workspace.This.IVVM.WR2 = (int)EthernetController.LaserWaveLengths[LaserChannels.ChannelB];//B代表R2通道  Enumeration B represents an R2 channel
                for (int i = 0; i < 2; i++)
                {
                    if (This.IVVM.WL1 != 0 && Workspace.This.IVVM.WL1 != Workspace.This.NewParameterVM.Uint16Code)
                    {
                        //Laser 板子固件版本号
                        //Laser board firmware version number
                        if (!This.EthernetController.GetLaserSystemVersions(LaserChannels.ChannelC))
                        {
                            This.NewParameterVM.OtherSettingBtnIsEnable = true;
                            StopWaitAnimation();
                            _IsLoading = false;
                            Application.Current.Dispatcher.Invoke((Action)delegate
                            {
                                Window window = new Window();
                                MessageBox.Show(window, "GetLaserSystemVersions failed.");
                            });
                            return;
                        }
                        Thread.Sleep(sleeptime);
                        if (!This.EthernetController.GetLaserOpticalModuleSerialNumber(LaserChannels.ChannelC))
                        {
                            This.NewParameterVM.OtherSettingBtnIsEnable = true;
                            StopWaitAnimation();
                            _IsLoading = false;
                            Application.Current.Dispatcher.Invoke((Action)delegate
                            {
                                Window window = new Window();
                                MessageBox.Show(window, "GetLaserOpticalModuleSerialNumber failed.");
                            });
                            return;
                        }
                        Thread.Sleep(sleeptime);
                        //TEC 控制温度
                        //TEC control temperature
                        if (!This.EthernetController.GetAllTECControlTTemperatures(LaserChannels.ChannelC))
                        {
                            This.NewParameterVM.OtherSettingBtnIsEnable = true;
                            StopWaitAnimation();
                            _IsLoading = false;
                            Application.Current.Dispatcher.Invoke((Action)delegate
                            {
                                Window window = new Window();
                                MessageBox.Show(window, "GetAllTECControlTTemperatures failed.");
                            });
                            return;
                        }
                        Thread.Sleep(sleeptime);
                        //TEC 最大制冷电流
                        //TEC maximum cooling current
                        if (!This.EthernetController.GetAllTECMaximumCoolingCurrentValue(LaserChannels.ChannelC))
                        {
                            This.NewParameterVM.OtherSettingBtnIsEnable = true;
                            StopWaitAnimation();
                            _IsLoading = false;
                            Application.Current.Dispatcher.Invoke((Action)delegate
                            {
                                Window window = new Window();
                                MessageBox.Show(window, "GetAllTECMaximumCoolingCurrentValue failed.");
                            });
                            return;
                        }
                        Thread.Sleep(sleeptime);
                        //TEC控制kp
                        //TEC control kp
                        if (!This.EthernetController.GetAllCurrentTECRefrigerationControlParameterKp(LaserChannels.ChannelC))
                        {
                            This.NewParameterVM.OtherSettingBtnIsEnable = true;
                            StopWaitAnimation();
                            _IsLoading = false;
                            Application.Current.Dispatcher.Invoke((Action)delegate
                            {
                                Window window = new Window();
                                MessageBox.Show(window, "GetAllCurrentTECRefrigerationControlParameterKp failed.");
                            });
                            return;
                        }
                        Thread.Sleep(sleeptime);
                        //TEC控制ki
                        //TEC control ki
                        if (!This.EthernetController.GetAllCurrentTECRefrigerationControlParameterKi(LaserChannels.ChannelC))
                        {
                            This.NewParameterVM.OtherSettingBtnIsEnable = true;
                            StopWaitAnimation();
                            _IsLoading = false;
                            Application.Current.Dispatcher.Invoke((Action)delegate
                            {
                                Window window = new Window();
                                MessageBox.Show(window, "GetAllCurrentTECRefrigerationControlParameterKi failed.");
                            });
                            return;
                        }
                        Thread.Sleep(sleeptime);
                        //TEC控制kd
                        //TEC control kd
                        if (!This.EthernetController.GetAllCurrentTECRefrigerationControlParameterKd(LaserChannels.ChannelC))
                        {
                            This.NewParameterVM.OtherSettingBtnIsEnable = true;
                            StopWaitAnimation();
                            _IsLoading = false;
                            Application.Current.Dispatcher.Invoke((Action)delegate
                            {
                                Window window = new Window();
                                MessageBox.Show(window, "GetAllCurrentTECRefrigerationControlParameterKd failed.");
                            });
                            return;
                        }
                        if (This.IVVM.WL1 == 532)
                        {
                            //光功率(<=15mW)控制kp
                            ////Optical power (<=15mW) control kp
                            Thread.Sleep(sleeptime);
                            if (!This.EthernetController.GetOpticalPowerLessThanOrEqual15mWKp(LaserChannels.ChannelC))
                            {
                                This.NewParameterVM.OtherSettingBtnIsEnable = true;
                                StopWaitAnimation();
                                _IsLoading = false;
                                Application.Current.Dispatcher.Invoke((Action)delegate
                                {
                                    Window window = new Window();
                                    MessageBox.Show(window, "GetOpticalPowerLessThanOrEqual15mWKp failed.");
                                });
                                return;
                            }
                            Thread.Sleep(sleeptime);
                            //光功率(<=15mW)控制ki
                            //Optical power (<=15mW) control ki
                            if (!This.EthernetController.GetOpticalPowerLessThanOrEqual15mWKi(LaserChannels.ChannelC))
                            {
                                This.NewParameterVM.OtherSettingBtnIsEnable = true;
                                StopWaitAnimation();
                                _IsLoading = false;
                                Application.Current.Dispatcher.Invoke((Action)delegate
                                {
                                    Window window = new Window();
                                    MessageBox.Show(window, "GetOpticalPowerLessThanOrEqual15mWKi failed.");
                                });
                                return;
                            }
                            Thread.Sleep(sleeptime);
                            //光功率(<=15mW)控制kd
                            //Optical power (<=15mW) control kd
                            if (!This.EthernetController.GetOpticalPowerLessThanOrEqual15mWKd(LaserChannels.ChannelC))
                            {
                                This.NewParameterVM.OtherSettingBtnIsEnable = true;
                                StopWaitAnimation();
                                _IsLoading = false;
                                Application.Current.Dispatcher.Invoke((Action)delegate
                                {
                                    Window window = new Window();
                                    MessageBox.Show(window, "GetOpticalPowerLessThanOrEqual15mWKd failed.");
                                });
                                return;
                            }
                            Thread.Sleep(sleeptime);
                            //   光功率(>15mW)控制kp
                            //Optical power (>15mW) control kp
                            if (!This.EthernetController.GetAllOpticalPowerGreaterThan15mWKp(LaserChannels.ChannelC))
                            {
                                This.NewParameterVM.OtherSettingBtnIsEnable = true;
                                StopWaitAnimation();
                                _IsLoading = false;
                                Application.Current.Dispatcher.Invoke((Action)delegate
                                {
                                    Window window = new Window();
                                    MessageBox.Show(window, "GetAllOpticalPowerGreaterThan15mWKp failed.");
                                });
                                return;
                            }
                            Thread.Sleep(sleeptime);
                            //   光功率(>15mW)控制ki
                            //Optical power (>15mW) control ki
                            if (!This.EthernetController.GetAllOpticalPowerGreaterThan15mWKi(LaserChannels.ChannelC))
                            {
                                This.NewParameterVM.OtherSettingBtnIsEnable = true;
                                StopWaitAnimation();
                                _IsLoading = false;
                                Application.Current.Dispatcher.Invoke((Action)delegate
                                {
                                    Window window = new Window();
                                    MessageBox.Show(window, "GetAllOpticalPowerGreaterThan15mWKi failed.");
                                });
                                return;
                            }
                            Thread.Sleep(sleeptime);
                            //   光功率(>15mW)控制kd
                            //Optical power (>15mW) control kd
                            if (!This.EthernetController.GetAllOpticalPowerGreaterThan15mWKd(LaserChannels.ChannelC))
                            {
                                This.NewParameterVM.OtherSettingBtnIsEnable = true;
                                StopWaitAnimation();
                                _IsLoading = false;
                                Application.Current.Dispatcher.Invoke((Action)delegate
                                {
                                    Window window = new Window();
                                    MessageBox.Show(window, "GetAllOpticalPowerGreaterThan15mWKd failed.");
                                });
                                return;
                            }
                            Thread.Sleep(sleeptime);
                            //激光器最大电流
                            //Laser maximum current
                            if (!This.EthernetController.GetAllLaserMaximumCurrent(LaserChannels.ChannelC))
                            {
                                This.NewParameterVM.OtherSettingBtnIsEnable = true;
                                StopWaitAnimation();
                                _IsLoading = false;
                                Application.Current.Dispatcher.Invoke((Action)delegate
                                {
                                    Window window = new Window();
                                    MessageBox.Show(window, "GetAllLaserMaximumCurrent failed.");
                                });
                                return;
                            }
                            Thread.Sleep(sleeptime);
                            //激光器最小电流
                            //Laser minimum current
                            if (!This.EthernetController.GetAllLaserMinimumCurrent(LaserChannels.ChannelC))
                            {
                                This.NewParameterVM.OtherSettingBtnIsEnable = true;
                                StopWaitAnimation();
                                _IsLoading = false;
                                Application.Current.Dispatcher.Invoke((Action)delegate
                                {
                                    Window window = new Window();
                                    MessageBox.Show(window, "GetAllLaserMinimumCurrent failed.");
                                });
                                return;
                            }
                        }
                        Thread.Sleep(sleeptime);

                    }

                    if (This.IVVM.WR1 != 0 && Workspace.This.IVVM.WR1 != Workspace.This.NewParameterVM.Uint16Code)
                    {
                        //Laser 板子固件版本号
                        //Laser board firmware version number
                        if (!This.EthernetController.GetLaserSystemVersions(LaserChannels.ChannelA))
                        {
                            This.NewParameterVM.OtherSettingBtnIsEnable = true;
                            StopWaitAnimation();
                            _IsLoading = false;
                            Application.Current.Dispatcher.Invoke((Action)delegate
                            {
                                Window window = new Window();
                                MessageBox.Show(window, "GetLaserSystemVersions failed.");
                            });
                            return;
                        }
                        Thread.Sleep(sleeptime);
                        //Laser 板子软件版本号
                        //Software versioning of board Laser
                        if (!This.EthernetController.GetLaserOpticalModuleSerialNumber(LaserChannels.ChannelA))
                        {
                            This.NewParameterVM.OtherSettingBtnIsEnable = true;
                            StopWaitAnimation();
                            _IsLoading = false;
                            Application.Current.Dispatcher.Invoke((Action)delegate
                            {
                                Window window = new Window();
                                MessageBox.Show(window, "GetLaserOpticalModuleSerialNumber failed.");
                            });
                            return;
                        }
                        Thread.Sleep(sleeptime);
                        //TEC 控制温度
                        //TEC control temperature
                        if (!This.EthernetController.GetAllTECControlTTemperatures(LaserChannels.ChannelA))
                        {
                            This.NewParameterVM.OtherSettingBtnIsEnable = true;
                            StopWaitAnimation();
                            _IsLoading = false;
                            Application.Current.Dispatcher.Invoke((Action)delegate
                            {
                                Window window = new Window();
                                MessageBox.Show(window, "GetAllTECControlTTemperatures failed.");
                            });
                            return;
                        }
                        Thread.Sleep(sleeptime);
                        //TEC 最大制冷电流
                        //TEC maximum cooling current
                        if (!This.EthernetController.GetAllTECMaximumCoolingCurrentValue(LaserChannels.ChannelA))
                        {
                            This.NewParameterVM.OtherSettingBtnIsEnable = true;
                            StopWaitAnimation();
                            _IsLoading = false;
                            Application.Current.Dispatcher.Invoke((Action)delegate
                            {
                                Window window = new Window();
                                MessageBox.Show(window, "GetAllTECMaximumCoolingCurrentValue failed.");
                            });
                            return;
                        }

                        Thread.Sleep(sleeptime);
                        //TEC控制kp
                        //TEC control kp
                        if (!This.EthernetController.GetAllCurrentTECRefrigerationControlParameterKp(LaserChannels.ChannelA))
                        {
                            This.NewParameterVM.OtherSettingBtnIsEnable = true;
                            StopWaitAnimation();
                            _IsLoading = false;
                            Application.Current.Dispatcher.Invoke((Action)delegate
                            {
                                Window window = new Window();
                                MessageBox.Show(window, "GetAllCurrentTECRefrigerationControlParameterKp failed.");
                            });
                            return;
                        }
                        Thread.Sleep(sleeptime);
                        //TEC控制ki
                        //TEC control ki
                        if (!This.EthernetController.GetAllCurrentTECRefrigerationControlParameterKi(LaserChannels.ChannelA))
                        {
                            This.NewParameterVM.OtherSettingBtnIsEnable = true;
                            StopWaitAnimation();
                            _IsLoading = false;
                            Application.Current.Dispatcher.Invoke((Action)delegate
                            {
                                Window window = new Window();
                                MessageBox.Show(window, "GetAllCurrentTECRefrigerationControlParameterKi failed.");
                            });
                            return;
                        }
                        Thread.Sleep(sleeptime);
                        //TEC控制kd
                        //TEC control kd
                        if (!This.EthernetController.GetAllCurrentTECRefrigerationControlParameterKd(LaserChannels.ChannelA))
                        {
                            This.NewParameterVM.OtherSettingBtnIsEnable = true;
                            StopWaitAnimation();
                            _IsLoading = false;
                            Application.Current.Dispatcher.Invoke((Action)delegate
                            {
                                Window window = new Window();
                                MessageBox.Show(window, "GetAllCurrentTECRefrigerationControlParameterKd failed.");
                            });
                            return;
                        }
                        if (This.IVVM.WR1 == 532)
                        {
                            //光功率(<=15mW)控制kp
                            //Optical power (<=15mW) control kp
                            Thread.Sleep(sleeptime);
                            if (!This.EthernetController.GetOpticalPowerLessThanOrEqual15mWKp(LaserChannels.ChannelA))
                            {
                                This.NewParameterVM.OtherSettingBtnIsEnable = true;
                                StopWaitAnimation();
                                _IsLoading = false;
                                Application.Current.Dispatcher.Invoke((Action)delegate
                                {
                                    Window window = new Window();
                                    MessageBox.Show(window, "GetOpticalPowerLessThanOrEqual15mWKp failed.");
                                });
                                return;
                            }
                            Thread.Sleep(sleeptime);
                            //光功率(<=15mW)控制ki
                            //Optical power (<=15mW) control ki
                            if (!This.EthernetController.GetOpticalPowerLessThanOrEqual15mWKi(LaserChannels.ChannelA))
                            {
                                This.NewParameterVM.OtherSettingBtnIsEnable = true;
                                StopWaitAnimation();
                                _IsLoading = false;
                                Application.Current.Dispatcher.Invoke((Action)delegate
                                {
                                    Window window = new Window();
                                    MessageBox.Show(window, "GetOpticalPowerLessThanOrEqual15mWKi failed.");
                                });
                                return;
                            }
                            Thread.Sleep(sleeptime);
                            //光功率(<=15mW)控制kd
                            //Optical power (<=15mW) control kd
                            if (!This.EthernetController.GetOpticalPowerLessThanOrEqual15mWKd(LaserChannels.ChannelA))
                            {
                                This.NewParameterVM.OtherSettingBtnIsEnable = true;
                                StopWaitAnimation();
                                _IsLoading = false;
                                Application.Current.Dispatcher.Invoke((Action)delegate
                                {
                                    Window window = new Window();
                                    MessageBox.Show(window, "GetOpticalPowerLessThanOrEqual15mWKd failed.");
                                });
                                return;
                            }
                            Thread.Sleep(sleeptime);
                            //   光功率(>15mW)控制kp
                            //Optical power (>15mW) control kp
                            if (!This.EthernetController.GetAllOpticalPowerGreaterThan15mWKp(LaserChannels.ChannelA))
                            {
                                This.NewParameterVM.OtherSettingBtnIsEnable = true;
                                StopWaitAnimation();
                                _IsLoading = false;
                                Application.Current.Dispatcher.Invoke((Action)delegate
                                {
                                    Window window = new Window();
                                    MessageBox.Show(window, "GetAllOpticalPowerGreaterThan15mWKp failed.");
                                });
                                return;
                            }
                            Thread.Sleep(sleeptime);
                            //   光功率(>15mW)控制ki
                            //Optical power (>15mW) control ki
                            if (!This.EthernetController.GetAllOpticalPowerGreaterThan15mWKi(LaserChannels.ChannelA))
                            {
                                This.NewParameterVM.OtherSettingBtnIsEnable = true;
                                StopWaitAnimation();
                                _IsLoading = false;
                                Application.Current.Dispatcher.Invoke((Action)delegate
                                {
                                    Window window = new Window();
                                    MessageBox.Show(window, "GetAllOpticalPowerGreaterThan15mWKi failed.");
                                });
                                return;
                            }
                            Thread.Sleep(sleeptime);
                            //   光功率(>15mW)控制kd
                            //Optical power (>15mW) control kd
                            if (!This.EthernetController.GetAllOpticalPowerGreaterThan15mWKd(LaserChannels.ChannelA))
                            {
                                This.NewParameterVM.OtherSettingBtnIsEnable = true;
                                StopWaitAnimation();
                                _IsLoading = false;
                                Application.Current.Dispatcher.Invoke((Action)delegate
                                {
                                    Window window = new Window();
                                    MessageBox.Show(window, "GetAllOpticalPowerGreaterThan15mWKd failed.");
                                });
                                return;
                            }
                            Thread.Sleep(sleeptime);
                            //激光器最大电流
                            //Laser maximum current
                            if (!This.EthernetController.GetAllLaserMaximumCurrent(LaserChannels.ChannelA))
                            {
                                This.NewParameterVM.OtherSettingBtnIsEnable = true;
                                StopWaitAnimation();
                                _IsLoading = false;
                                Application.Current.Dispatcher.Invoke((Action)delegate
                                {
                                    Window window = new Window();
                                    MessageBox.Show(window, "GetAllLaserMaximumCurrent failed.");
                                });
                                return;
                            }
                            Thread.Sleep(sleeptime);
                            //激光器最小电流
                            //Laser minimum current
                            if (!This.EthernetController.GetAllLaserMinimumCurrent(LaserChannels.ChannelA))
                            {
                                This.NewParameterVM.OtherSettingBtnIsEnable = true;
                                StopWaitAnimation();
                                _IsLoading = false;
                                Application.Current.Dispatcher.Invoke((Action)delegate
                                {
                                    Window window = new Window();
                                    MessageBox.Show(window, "GetAllLaserMinimumCurrent failed.");
                                });
                                return;
                            }
                        }
                        Thread.Sleep(sleeptime);
                    }

                    if (This.IVVM.WR2 != 0 && Workspace.This.IVVM.WR2 != Workspace.This.NewParameterVM.Uint16Code)
                    {
                        //Laser 板子固件版本号
                        //Laser board firmware version number
                        if (!This.EthernetController.GetLaserSystemVersions(LaserChannels.ChannelB))
                        {
                            This.NewParameterVM.OtherSettingBtnIsEnable = true;
                            StopWaitAnimation();
                            _IsLoading = false;
                            Application.Current.Dispatcher.Invoke((Action)delegate
                            {
                                Window window = new Window();
                                MessageBox.Show(window, "GetLaserSystemVersions failed.");
                            });
                            return;
                        }
                        Thread.Sleep(sleeptime);
                        //Laser 板子软件版本号
                        //Software versioning of board Laser
                        if (!This.EthernetController.GetLaserOpticalModuleSerialNumber(LaserChannels.ChannelB))
                        {
                            This.NewParameterVM.OtherSettingBtnIsEnable = true;
                            StopWaitAnimation();
                            _IsLoading = false;
                            Application.Current.Dispatcher.Invoke((Action)delegate
                            {
                                Window window = new Window();
                                MessageBox.Show(window, "GetLaserOpticalModuleSerialNumber failed.");
                            });
                            return;
                        }
                        Thread.Sleep(sleeptime);
                        //TEC 控制温度
                        //TEC control temperature
                        if (!This.EthernetController.GetAllTECControlTTemperatures(LaserChannels.ChannelB))
                        {
                            This.NewParameterVM.OtherSettingBtnIsEnable = true;
                            StopWaitAnimation();
                            _IsLoading = false;
                            Application.Current.Dispatcher.Invoke((Action)delegate
                            {
                                Window window = new Window();
                                MessageBox.Show(window, "GetAllTECControlTTemperatures failed.");
                            });
                            return;
                        }
                        Thread.Sleep(sleeptime);
                        //TEC 最大制冷电流
                        //TEC maximum cooling current
                        if (!This.EthernetController.GetAllTECMaximumCoolingCurrentValue(LaserChannels.ChannelB))
                        {
                            This.NewParameterVM.OtherSettingBtnIsEnable = true;
                            StopWaitAnimation();
                            _IsLoading = false;
                            Application.Current.Dispatcher.Invoke((Action)delegate
                            {
                                Window window = new Window();
                                MessageBox.Show(window, "GetAllTECMaximumCoolingCurrentValue failed.");
                            });
                            return;
                        }
                        Thread.Sleep(sleeptime);
                        //TEC控制kp
                        //TEC control kp
                        if (!This.EthernetController.GetAllCurrentTECRefrigerationControlParameterKp(LaserChannels.ChannelB))
                        {
                            This.NewParameterVM.OtherSettingBtnIsEnable = true;
                            StopWaitAnimation();
                            _IsLoading = false;
                            Application.Current.Dispatcher.Invoke((Action)delegate
                            {
                                Window window = new Window();
                                MessageBox.Show(window, "GetAllCurrentTECRefrigerationControlParameterKp failed.");
                            });
                            return;
                        }
                        Thread.Sleep(sleeptime);
                        //TEC控制ki
                        //TEC control ki
                        if (!This.EthernetController.GetAllCurrentTECRefrigerationControlParameterKi(LaserChannels.ChannelB))
                        {
                            This.NewParameterVM.OtherSettingBtnIsEnable = true;
                            StopWaitAnimation();
                            _IsLoading = false;
                            Application.Current.Dispatcher.Invoke((Action)delegate
                            {
                                Window window = new Window();
                                MessageBox.Show(window, "GetAllCurrentTECRefrigerationControlParameterKi failed.");
                            });
                            return;
                        }
                        Thread.Sleep(sleeptime);
                        //TEC控制kd
                        //TEC control kd
                        if (!This.EthernetController.GetAllCurrentTECRefrigerationControlParameterKd(LaserChannels.ChannelB))
                        {
                            This.NewParameterVM.OtherSettingBtnIsEnable = true;
                            StopWaitAnimation();
                            _IsLoading = false;
                            Application.Current.Dispatcher.Invoke((Action)delegate
                            {
                                Window window = new Window();
                                MessageBox.Show(window, "GetAllCurrentTECRefrigerationControlParameterKd failed.");
                            });
                            return;
                        }
                        if (This.IVVM.WR2 == 532)
                        {
                            Thread.Sleep(sleeptime);
                            //光功率(<=15mW)控制kp
                            //Optical power (<=15mW) control kp
                            if (!This.EthernetController.GetOpticalPowerLessThanOrEqual15mWKp(LaserChannels.ChannelB))
                            {
                                This.NewParameterVM.OtherSettingBtnIsEnable = true;
                                StopWaitAnimation();
                                _IsLoading = false;
                                Application.Current.Dispatcher.Invoke((Action)delegate
                                {
                                    Window window = new Window();
                                    MessageBox.Show(window, "GetOpticalPowerLessThanOrEqual15mWKp failed.");
                                });
                                return;
                            }
                            Thread.Sleep(sleeptime);
                            //光功率(<=15mW)控制ki
                            //Optical power (<=15mW) control ki
                            if (!This.EthernetController.GetOpticalPowerLessThanOrEqual15mWKi(LaserChannels.ChannelB))
                            {
                                This.NewParameterVM.OtherSettingBtnIsEnable = true;
                                StopWaitAnimation();
                                _IsLoading = false;
                                Application.Current.Dispatcher.Invoke((Action)delegate
                                {
                                    Window window = new Window();
                                    MessageBox.Show(window, "GetOpticalPowerLessThanOrEqual15mWKi failed.");
                                });
                                return;
                            }
                            Thread.Sleep(sleeptime);
                            //光功率(<=15mW)控制kd
                            //Optical power (<=15mW) control kd
                            if (!This.EthernetController.GetOpticalPowerLessThanOrEqual15mWKd(LaserChannels.ChannelB))
                            {
                                This.NewParameterVM.OtherSettingBtnIsEnable = true;
                                StopWaitAnimation();
                                _IsLoading = false;
                                Application.Current.Dispatcher.Invoke((Action)delegate
                                {
                                    Window window = new Window();
                                    MessageBox.Show(window, "GetOpticalPowerLessThanOrEqual15mWKd failed.");
                                });
                                return;
                            }
                            Thread.Sleep(sleeptime);
                            //   光功率(>15mW)控制kp
                            //Optical power (>15mW) control kp
                            if (!This.EthernetController.GetAllOpticalPowerGreaterThan15mWKp(LaserChannels.ChannelB))
                            {
                                This.NewParameterVM.OtherSettingBtnIsEnable = true;
                                StopWaitAnimation();
                                _IsLoading = false;
                                Application.Current.Dispatcher.Invoke((Action)delegate
                                {
                                    Window window = new Window();
                                    MessageBox.Show(window, "GetAllOpticalPowerGreaterThan15mWKp failed.");
                                });
                                return;
                            }
                            Thread.Sleep(sleeptime);
                            //   光功率(>15mW)控制ki
                            //Optical power (>15mW) control ki
                            if (!This.EthernetController.GetAllOpticalPowerGreaterThan15mWKi(LaserChannels.ChannelB))
                            {
                                This.NewParameterVM.OtherSettingBtnIsEnable = true;
                                StopWaitAnimation();
                                _IsLoading = false;
                                Application.Current.Dispatcher.Invoke((Action)delegate
                                {
                                    Window window = new Window();
                                    MessageBox.Show(window, "GetAllOpticalPowerGreaterThan15mWKi failed.");
                                });
                                return;
                            }
                            Thread.Sleep(sleeptime);
                            //   光功率(>15mW)控制kd
                            //Optical power (>15mW) control kd
                            if (!This.EthernetController.GetAllOpticalPowerGreaterThan15mWKd(LaserChannels.ChannelB))
                            {
                                This.NewParameterVM.OtherSettingBtnIsEnable = true;
                                StopWaitAnimation();
                                _IsLoading = false;
                                Application.Current.Dispatcher.Invoke((Action)delegate
                                {
                                    Window window = new Window();
                                    MessageBox.Show(window, "GetAllOpticalPowerGreaterThan15mWKd failed.");
                                });
                                return;
                            }
                            Thread.Sleep(sleeptime);
                            //激光器最大电流
                            //Laser maximum current
                            if (!This.EthernetController.GetAllLaserMaximumCurrent(LaserChannels.ChannelB))
                            {
                                This.NewParameterVM.OtherSettingBtnIsEnable = true;
                                StopWaitAnimation();
                                _IsLoading = false;
                                Application.Current.Dispatcher.Invoke((Action)delegate
                                {
                                    Window window = new Window();
                                    MessageBox.Show(window, "GetAllLaserMaximumCurrent failed.");
                                });
                                return;
                            }
                            Thread.Sleep(sleeptime);
                            //激光器最小电流
                            //Laser minimum current
                            if (!This.EthernetController.GetAllLaserMinimumCurrent(LaserChannels.ChannelB))
                            {
                                This.NewParameterVM.OtherSettingBtnIsEnable = true;
                                StopWaitAnimation();
                                _IsLoading = false;
                                Application.Current.Dispatcher.Invoke((Action)delegate
                                {
                                    Window window = new Window();
                                    MessageBox.Show(window, "GetAllLaserMinimumCurrent failed.");
                                });
                                return;
                            }

                        }
                        Thread.Sleep(sleeptime);
                    }

                }
                if (This.IVVM.WL1 == 0 && This.IVVM.WR1 == 0 && This.IVVM.WR2 == 0)
                {
                    Thread.Sleep(3000);
                }
            };
            worker.RunWorkerCompleted += (o, ea) =>
            {
                // Work has completed. You can now interact with the UI
                StopWaitAnimation();
                _IsLoading = false;

                if (Workspace.This.IVVM.WL1 != 0 && Workspace.This.IVVM.WL1 != Workspace.This.NewParameterVM.Uint16Code) //  Determine if the module wavelength exists
                {
                    This.NewParameterVM.LIVFirmwareVersionSN = EthernetController.IVOpticalModuleSerialNumber[IVChannels.ChannelC];
                    This.NewParameterVM.LIVFirmwareSN = EthernetController.IVEstimatedVersionNumberBoard[IVChannels.ChannelC];
                    This.NewParameterVM.LTEControlTemperature = EthernetController.TECControlTemperature[LaserChannels.ChannelC];
                    This.NewParameterVM.LTECMaximumCoolingCurrent = EthernetController.TECMaximumCurrent[LaserChannels.ChannelC];
                    This.NewParameterVM.LTECControlKp = EthernetController.TECRefrigerationControlParameterKp[LaserChannels.ChannelC];
                    This.NewParameterVM.LTECControlKi = EthernetController.TECRefrigerationControlParameterKi[LaserChannels.ChannelC];
                    This.NewParameterVM.LTECControlKd = EthernetController.TECRefrigerationControlParameterKd[LaserChannels.ChannelC];

                    if (Workspace.This.IVVM.WL1 == 532)
                    {
                        This.NewParameterVM.LOpticalPowerLessThanOrEqual15mWKp = EthernetController.AllOpticalPowerLessThanOrEqual15mWKp[LaserChannels.ChannelC];
                        This.NewParameterVM.LOpticalPowerLessThanOrEqual15mWKi = EthernetController.AllOpticalPowerLessThanOrEqual15mWKi[LaserChannels.ChannelC];
                        This.NewParameterVM.LOpticalPowerLessThanOrEqual15mWKd = EthernetController.AllOpticalPowerLessThanOrEqual15mWKd[LaserChannels.ChannelC];

                        This.NewParameterVM.LOpticalPowerGreaterThan15mWKp = EthernetController.AllOpticalPowerGreaterThan15mWKp[LaserChannels.ChannelC];
                        This.NewParameterVM.LOpticalPowerGreaterThan15mWKi = EthernetController.AllOpticalPowerGreaterThan15mWKi[LaserChannels.ChannelC];
                        This.NewParameterVM.LOpticalPowerGreaterThan15mWKd = EthernetController.AllOpticalPowerGreaterThan15mWKd[LaserChannels.ChannelC];

                        This.NewParameterVM.LLaserMaxCurrent = EthernetController.AllGetLaserMaximumCurrent[LaserChannels.ChannelC];
                        This.NewParameterVM.LLaserMinCurrent = EthernetController.AllGetLaserMinimumCurrent[LaserChannels.ChannelC];
                    }
                }
                if (Workspace.This.IVVM.WR1 != 0 && Workspace.This.IVVM.WR1 != Workspace.This.NewParameterVM.Uint16Code)
                {
                    This.NewParameterVM.R1IVFirmwareVersionSN = EthernetController.IVOpticalModuleSerialNumber[IVChannels.ChannelA];
                    This.NewParameterVM.R1IVFirmwareSN = EthernetController.IVEstimatedVersionNumberBoard[IVChannels.ChannelA];
                    This.NewParameterVM.R1TEControlTemperature = EthernetController.TECControlTemperature[LaserChannels.ChannelA];
                    This.NewParameterVM.R1TECMaximumCoolingCurrent = EthernetController.TECMaximumCurrent[LaserChannels.ChannelA];
                    This.NewParameterVM.R1TECControlKp = EthernetController.TECRefrigerationControlParameterKp[LaserChannels.ChannelA];
                    This.NewParameterVM.R1TECControlKi = EthernetController.TECRefrigerationControlParameterKi[LaserChannels.ChannelA];
                    This.NewParameterVM.R1TECControlKd = EthernetController.TECRefrigerationControlParameterKd[LaserChannels.ChannelA];
                    if (Workspace.This.IVVM.WR1 == 532)
                    {
                        This.NewParameterVM.R1OpticalPowerLessThanOrEqual15mWKp = EthernetController.AllOpticalPowerLessThanOrEqual15mWKp[LaserChannels.ChannelA];
                        This.NewParameterVM.R1OpticalPowerLessThanOrEqual15mWKi = EthernetController.AllOpticalPowerLessThanOrEqual15mWKi[LaserChannels.ChannelA];
                        This.NewParameterVM.R1OpticalPowerLessThanOrEqual15mWKd = EthernetController.AllOpticalPowerLessThanOrEqual15mWKd[LaserChannels.ChannelA];

                        This.NewParameterVM.R1OpticalPowerGreaterThan15mWKp = EthernetController.AllOpticalPowerGreaterThan15mWKp[LaserChannels.ChannelA];
                        This.NewParameterVM.R1OpticalPowerGreaterThan15mWKi = EthernetController.AllOpticalPowerGreaterThan15mWKi[LaserChannels.ChannelA];
                        This.NewParameterVM.R1OpticalPowerGreaterThan15mWKd = EthernetController.AllOpticalPowerGreaterThan15mWKd[LaserChannels.ChannelA];

                        This.NewParameterVM.R1LaserMaxCurrent = EthernetController.AllGetLaserMaximumCurrent[LaserChannels.ChannelA];
                        This.NewParameterVM.R1LaserMinCurrent = EthernetController.AllGetLaserMinimumCurrent[LaserChannels.ChannelA];

                    }
                }

                if (Workspace.This.IVVM.WR2 != 0 && Workspace.This.IVVM.WR1 != Workspace.This.NewParameterVM.Uint16Code)
                {
                    This.NewParameterVM.R2IVFirmwareVersionSN = EthernetController.IVOpticalModuleSerialNumber[IVChannels.ChannelB];
                    This.NewParameterVM.R2IVFirmwareSN = EthernetController.IVEstimatedVersionNumberBoard[IVChannels.ChannelB];
                    This.NewParameterVM.R2TEControlTemperature = EthernetController.TECControlTemperature[LaserChannels.ChannelB];
                    This.NewParameterVM.R2TECMaximumCoolingCurrent = EthernetController.TECMaximumCurrent[LaserChannels.ChannelB];
                    This.NewParameterVM.R2TECControlKp = EthernetController.TECRefrigerationControlParameterKp[LaserChannels.ChannelB];
                    This.NewParameterVM.R2TECControlKi = EthernetController.TECRefrigerationControlParameterKi[LaserChannels.ChannelB];
                    This.NewParameterVM.R2TECControlKd = EthernetController.TECRefrigerationControlParameterKd[LaserChannels.ChannelB];

                    if (Workspace.This.IVVM.WR2 == 532)
                    {
                        This.NewParameterVM.R2OpticalPowerLessThanOrEqual15mWKp = EthernetController.AllOpticalPowerLessThanOrEqual15mWKp[LaserChannels.ChannelB];
                        This.NewParameterVM.R2OpticalPowerLessThanOrEqual15mWKi = EthernetController.AllOpticalPowerLessThanOrEqual15mWKi[LaserChannels.ChannelB];
                        This.NewParameterVM.R2OpticalPowerLessThanOrEqual15mWKd = EthernetController.AllOpticalPowerLessThanOrEqual15mWKd[LaserChannels.ChannelB];

                        This.NewParameterVM.R2OpticalPowerGreaterThan15mWKp = EthernetController.AllOpticalPowerGreaterThan15mWKp[LaserChannels.ChannelB];
                        This.NewParameterVM.R2OpticalPowerGreaterThan15mWKi = EthernetController.AllOpticalPowerGreaterThan15mWKi[LaserChannels.ChannelB];
                        This.NewParameterVM.R2OpticalPowerGreaterThan15mWKd = EthernetController.AllOpticalPowerGreaterThan15mWKd[LaserChannels.ChannelB];

                        This.NewParameterVM.R2LaserMaxCurrent = EthernetController.AllGetLaserMaximumCurrent[LaserChannels.ChannelB];
                        This.NewParameterVM.R2LaserMinCurrent = EthernetController.AllGetLaserMinimumCurrent[LaserChannels.ChannelB];

                    }
                }
                This.NewParameterVM.OtherSettingBtnIsEnable = true;
            };

            worker.RunWorkerAsync();

        }

        #region DisconnectDeviceCommand   （FW Version 1.1.0.0）
        private RelayCommand _DisconnectDeviceCommand = null;
        public ICommand DisconnectDeviceCommand
        {
            get
            {
                if (_DisconnectDeviceCommand == null)
                {
                    _DisconnectDeviceCommand = new RelayCommand(ExecuteDisconnectDeviceCommand, CanExecuteDisconnectDeviceCommand);
                }

                return _DisconnectDeviceCommand;
            }
        }
        public void ExecuteDisconnectDeviceCommand(object parameter)
        {
            DisconnectDevice();
        }

        public bool CanExecuteDisconnectDeviceCommand(object parameter)
        {
            return true;
        }
        #endregion

        #endregion

        #region ActiveDocument

        //public event EventHandler ActiveDocumentChanged;

        public FileViewModel ActiveDocument
        {
            get { return _ActiveDocument; }
            set
            {
                if (_ActiveDocument != value)
                {
                    SetPixelDefaultValue("SelectImage");
                    _ActiveDocument = value;

                    /*#region === Work-around ===
                    // Work-around for RGB image crop application crash
                    // For some reason this got rid of the access violation error
                    // TODO: find a better solution
                    // NOTE: cSeries error, commented out because not seeing the same error in Sapphire
                    if (_ActiveDocument != null)
                    {
                        if (_ActiveDocument.IsRGBImageCropped)
                        {
                            if (_ActiveDocument.Image.Format.BitsPerPixel == 48)
                            {
                                int iColorGradation = 3;
                                WriteableBitmap dspBitmap = _ActiveDocument.Image;
                                dspBitmap = ImageProcessing.Scale(dspBitmap,
                                                                  _ActiveDocument.ImageInfo.MixChannel.BlackValue,
                                                                  _ActiveDocument.ImageInfo.MixChannel.WhiteValue,
                                                                  _ActiveDocument.ImageInfo.MixChannel.GammaValue,
                                                                  _ActiveDocument.ImageInfo.MixChannel.IsInvertChecked,
                                                                  _ActiveDocument.ImageInfo.MixChannel.IsSaturationChecked,
                                                                  _ActiveDocument.ImageInfo.SaturationThreshold,
                                                                  iColorGradation);
                                dspBitmap = null;
                                _ActiveDocument.IsRGBImageCropped = false;
                            }
                        }
                    }
                    #endregion*/

                    RaisePropertyChanged("ActiveDocument");
                    RaisePropertyChanged("IsActiveDocument");
                    RaisePropertyChanged("IsRGBImage");
                    //RaisePropertyChanged("Is8bpp");
                    RaisePropertyChanged("Is32bppImage");
                    RaisePropertyChanged("IsCopyAndPasteAllowed");

                    //if (ActiveDocumentChanged != null)
                    //{
                    //    ActiveDocumentChanged(this, EventArgs.Empty);
                    //}
                }
            }
        }
        
        #endregion

        #region public bool IsActiveDocument
        public bool IsActiveDocument
        {
            get
            {
                bool bIsActiveDocument = false;

                if (Files.Count > 0 && ActiveDocument != null)
                {
                    {
                        bIsActiveDocument = true;
                    }
                }

                return bIsActiveDocument;
            }
        }
        #endregion

        #region public bool NewDocument(WriteableBitmap image, ImageInfo imageInfo, string imageTitle)\
        public  List<string> LGifImageTitle = new List<string>();
        public List<string> R1GifImageTitle = new List<string>();
        public List<string> R2GifImageTitle = new List<string>();
        /// <summary>
        /// Add new document.
        /// </summary>
        /// <param name="image"></param>
        /// <param name="imageInfo"></param>
        /// <param name="title"></param>
        /// <returns></returns>
        public bool NewDocument(WriteableBitmap image, ImageInfo imageInfo, string title, bool bIsCropped, bool bIsGetMinMax = true, bool ScanImage = false)
        {
            StartWaitAnimation("Loading...");
            _IsLoading = true;
            bool bResult = false;
            FileViewModel fileViewModel = null;
            //创建GIF图像所用到的三个通道图片临时的文件夹位置
            //The temporary folder location of the three channel pictures used to create the GIF image
            string LImageGifPath = this.ImageRotatingPrcessVM.LImageGifPath + @"\" + title;
            string R1ImageGifPath = this.ImageRotatingPrcessVM.R1ImageGifPath + @"\" + title;
            string R2ImageGifPath = this.ImageRotatingPrcessVM.R2ImageGifPath + @"\" + title;
            int Resolution = Workspace.This.ScannerVM.SelectedResolution.Value;
            double ResolutionMove = ((double)Workspace.This.ScannerVM.SelectedResolution.Value / (double)10);//MoveImageResolution
            string[] split = title.Split(new Char[] { '_' });
            //像素左右偏移参数值
            //Pixel left and right offset parameter value
            int pixelOddx = SettingsManager.ConfigSettings.XOddNumberedLine;//X odd Line
            int pixelEvenx = SettingsManager.ConfigSettings.XEvenNumberedLine;//X even Line
            int pixelOddy = SettingsManager.ConfigSettings.YOddNumberedLine;//Y odd Line
            int pixelEveny = SettingsManager.ConfigSettings.YEvenNumberedLine;//Y Even Line
            double YCompenOffset = SettingsManager.ConfigSettings.YCompenOffset;
            //Y轴截取像素前几行
            ////Y-axis intercepts the first few rows of pixels
            int Y_MovePixelDY = (int)((double)(YCompenOffset * 1000) / (double)Resolution);
            string Name = "";
            if (split.Length > 1)
            {
                Name = split[1];
            }
            else {
                Name = "";
            }
            try
            {
                //（因为在设计中将R1透镜的位置和R2透镜的位置更换了，然后参数名没有更改成对应的通道，所以下面参数名R1其实是R2，R2其实是R1）....
                // (in the design, the position of R1 lens and R2 lens is changed, and the parameter name is not changed to the corresponding channel, so the following parameter name R1 is actually R2, R2 is actually R1)....
                switch (Name)
                {
                    case "L":
                        if (ScanImage)
                        {
                            if (SettingsManager.ConfigSettings.PixelOffsetProcessing)
                            {
                                if (Workspace.This.ScannerVM.SelectedResolution.Value < SettingsManager.ConfigSettings.PixelOffsetProcessingRes)
                                {
                                    //处理高分辨率图片出现的锯齿
                                    //Handle the sawtooth caused by high resolution
                                    FileViewModel.UpdatePixelSingleMoveDisplayImage(ref image, pixelOddx, pixelEvenx, pixelOddy, pixelEveny);
                                }
                            }
                            if (SettingsManager.ConfigSettings.ImageOffsetProcessing)
                            {
                                //根据当前分辨率计算L到R2之间需要截取掉多少列像素
                                //Calculate how many columns of pixels need to be intercepted between L and R2 according to the current resolution
                                int LeffectivePixel = (int)(((double)this.NewParameterVM.OpticalL_R1Distance * 1000) / (double)Resolution);
                                if (LeffectivePixel != 0)
                                {
                                    //将图片从左到右截取的像素列，保持图片位置对齐
                                    //The pixel column of the picture taken from left to right, keeping the position aligned with the picture
                                    FileViewModel.UpdatePixeleffectiveMoveDisplayImage(ref image, LeffectivePixel);//image effective
                                }
                                //图片像素向右或者向下移动
                                ////Move the picture pixel to the right or down
                                int LMovePixelDX = (int)((double)this.NewParameterVM.Pixel_10_L_DX / ResolutionMove);
                                int LMovePixelDY = (int)((double)this.NewParameterVM.Pixel_10_L_DY / ResolutionMove);
                                if (LMovePixelDX != 0 || LMovePixelDY != 0)
                                {
                                    FileViewModel.UpdatePixelSingleMoveDisplayImage(ref image, LMovePixelDX, LMovePixelDX, LMovePixelDY, LMovePixelDY);//Align the three images
                                }
                            }
                            if (SettingsManager.ConfigSettings.YCompenSationBitAt && (int)((double)(YCompenOffset * 1000) / (double)Resolution) >= 1)
                            {
                                //去掉图像前几行，避免图片前几行造成的倾斜
                                //Remove the first few lines of the image to avoid the tilt caused by the first few lines of the image
                                FileViewModel.YAxisUpdatePixeleffectiveMoveDisplayImage(ref image, Y_MovePixelDY);
                            }
                            if (Workspace.This.ZAutomaticallyFocalVM.IsCreateGif)
                            {
                                //Convert the image to PNG
                                FileViewModel.ImageToPngSave(image, LImageGifPath);
                                //将png图片添加到列表
                                //Add png image to the list
                                LGifImageTitle.Add(LImageGifPath + ".png");
                                if (Workspace.This.ZAutomaticallyFocalVM.Ofimages >= Workspace.This.ZAutomaticallyFocalVM._FocusOptionsList.Count)
                                {
                                    string s1 = title.Remove(title.LastIndexOf("_"));
                                    //创建GIF图像并保存在文件夹里
                                    ////Create a GIF image and save it in the folder
                                    FileViewModel.CreateGif(this.ImageRotatingPrcessVM.ImageGifPath + @"\" + Workspace.This.ImageRotatingPrcessVM.GenerateFileName(s1, ".gif"), LGifImageTitle);
                                    LGifImageTitle.Clear();
                                }
                            }
                        }
                        break;
                    case "R1":
                        if (ScanImage)
                        {
                            if (SettingsManager.ConfigSettings.PixelOffsetProcessing)
                            {
                                if (Workspace.This.ScannerVM.SelectedResolution.Value < SettingsManager.ConfigSettings.PixelOffsetProcessingRes)
                                {
                                    //处理高分辨率图片出现的锯齿
                                    //Handle the sawtooth caused by high resolution
                                    FileViewModel.UpdatePixelSingleMoveDisplayImage(ref image, pixelOddx, pixelEvenx, pixelOddy, pixelEveny);//Handle the sawtooth caused by high resolution
                                }
                            }
                            if (SettingsManager.ConfigSettings.ImageOffsetProcessing)
                            {
                                //根据当前分辨率计算R1到R2之间需要截取掉的像素列
                                //Calculate the pixel column between R1 and R2 that needs to be intercepted according to the current resolution
                                int R2MovePixelX = (int)(((double)this.NewParameterVM.OpticalR2_R1Distance * 1000) / (double)Resolution);
                                int R2MovePixelY = 0;
                                int R2effectivePixel = R2MovePixelX * 2;
                                if (R2MovePixelX != 0)
                                {
                                    //将图片向右移动R1到R2透镜之间的距离，保持图像里内容对齐
                                    //The pixel column of the picture taken from left to right, keeping the position aligned with the picture
                                    FileViewModel.UpdatePixelSingleMoveDisplayImage(ref image, R2MovePixelX, R2MovePixelX, R2MovePixelY, R2MovePixelY);//image move
                                                                                                                                                       //将图片从左到右截取掉多补偿的宽度部分
                                                                                                                                                       //Cut off the multi-compensated width of the picture from left to right
                                    FileViewModel.UpdatePixeleffectiveMoveDisplayImage(ref image, R2effectivePixel);//image effective
                                }
                                //图片像素向右或者向下移动
                                ////Move the picture pixel to the right or down
                                int R2PixelDX = (int)((double)this.NewParameterVM.Pixel_10_R2_DX / ResolutionMove);
                                int R2PixelDY = (int)((double)this.NewParameterVM.Pixel_10_R2_DY / ResolutionMove);
                                if (R2PixelDX != 0 || R2PixelDY != 0)
                                {
                                    FileViewModel.UpdatePixelSingleMoveDisplayImage(ref image, R2PixelDX, R2PixelDX, R2PixelDY, R2PixelDY);//image move
                                }
                            }
                            if (SettingsManager.ConfigSettings.YCompenSationBitAt && ((double)(YCompenOffset * 1000) / (double)Resolution) >= 1)
                            {  
                                //去掉图像前几行，避免图片前几行造成的倾斜
                                //Remove the first few lines of the image to avoid the tilt caused by the first few lines of the image
                                FileViewModel.YAxisUpdatePixeleffectiveMoveDisplayImage(ref image, Y_MovePixelDY);
                            }
                            if (Workspace.This.ZAutomaticallyFocalVM.IsCreateGif)
                            {
                                //Convert the image to PNG
                                FileViewModel.ImageToPngSave(image, R1ImageGifPath);
                                 //将png图片添加到列表
                                //Add png image to the list
                                R1GifImageTitle.Add(R1ImageGifPath + ".png");
                                if (Workspace.This.ZAutomaticallyFocalVM.Ofimages >= Workspace.This.ZAutomaticallyFocalVM._FocusOptionsList.Count)
                                {
                                    //创建GIF图像并保存在文件夹里
                                    ////Create a GIF image and save it in the folder
                                    string s1 = title.Remove(title.LastIndexOf("_"));
                                    FileViewModel.CreateGif(this.ImageRotatingPrcessVM.ImageGifPath + @"\" + Workspace.This.ImageRotatingPrcessVM.GenerateFileName(s1, ".gif"), R1GifImageTitle);
                                    R1GifImageTitle.Clear();
                                }
                            }
                        }
                        break;
                    case "R2":
                        if (ScanImage)
                        {
                            if (SettingsManager.ConfigSettings.PixelOffsetProcessing)
                            {
                                if (Workspace.This.ScannerVM.SelectedResolution.Value < SettingsManager.ConfigSettings.PixelOffsetProcessingRes)
                                {
                                    //处理高分辨率图片出现的锯齿
                                    //Handle the sawtooth caused by high resolution
                                    FileViewModel.UpdatePixelSingleMoveDisplayImage(ref image, pixelOddx, pixelEvenx, pixelOddy, pixelEveny);//Handle the sawtooth caused by high resolution
                                }
                            }
                            if (SettingsManager.ConfigSettings.ImageOffsetProcessing)
                            {
                                //根据当前分辨率计算L到R2之间需要截取掉多少列像素
                                //Calculate how many columns of pixels need to be intercepted between L and R2 according to the current resolution
                                int R1MovePixelDX = (int)(((double)this.NewParameterVM.OpticalL_R1Distance * 1000) / (double)Resolution);
                                int R1MovePixelDY = 0;
                                if (R1MovePixelDX != 0)
                                {
                                    //将图片向右移动L到R2透镜之间的距离，保持图像里内容对齐
                                    //The pixel column of the picture taken from left to right, keeping the position aligned with the picture
                                    FileViewModel.UpdatePixelSingleMoveDisplayImage(ref image, R1MovePixelDX, R1MovePixelDX, R1MovePixelDY, R1MovePixelDY);
                                    //将图片从左到右截取掉多补偿的宽度部分
                                    //Cut off the multi-compensated width of the picture from left to right
                                    FileViewModel.UpdatePixeleffectiveMoveDisplayImage(ref image, R1MovePixelDX);
                                }
                                ///
                            }
                            if (SettingsManager.ConfigSettings.YCompenSationBitAt && ((double)(YCompenOffset * 1000) / (double)Resolution) >= 1)
                            {  
                                //去掉图像前几行，避免图片前几行造成的倾斜
                                //Remove the first few lines of the image to avoid the tilt caused by the first few lines of the image
                                FileViewModel.YAxisUpdatePixeleffectiveMoveDisplayImage(ref image, Y_MovePixelDY);//Cut out the first few lines of the X-axis
                            }
                            if (Workspace.This.ZAutomaticallyFocalVM.IsCreateGif)
                            {
                                //Convert the image to PNG
                                FileViewModel.ImageToPngSave(image, R2ImageGifPath);
                                  //将png图片添加到列表
                                //Add png image to the list
                                R2GifImageTitle.Add(R2ImageGifPath + ".png");
                                if (Workspace.This.ZAutomaticallyFocalVM.Ofimages >= Workspace.This.ZAutomaticallyFocalVM._FocusOptionsList.Count)
                                {
                                    //创建GIF图像并保存在文件夹里
                                    ////Create a GIF image and save it in the folder
                                    string s1 = title.Remove(title.LastIndexOf("_"));
                                    FileViewModel.CreateGif(this.ImageRotatingPrcessVM.ImageGifPath + @"\" + Workspace.This.ImageRotatingPrcessVM.GenerateFileName(s1, ".gif"), R2GifImageTitle);
                                    R2GifImageTitle.Clear();
                                }
                            }
                        }
                        break;
                }
                fileViewModel = new FileViewModel(image, imageInfo, title, bIsCropped, bIsGetMinMax);
                if (fileViewModel != null)
                {
                    Files.Add(fileViewModel);        // Add to the end
                    ActiveDocument = fileViewModel;
                    //ActiveDocument = Files.Last();
                    ActiveDocument.IsDirty = true;
                    ActiveDocument.IsAutoContrast = true;
                    ExecuteAutoContrastCommand(null); 
                    //ActiveDocument.Title = ActiveDocument.FileName; // Update title with IsDirty flag set
                    bResult = true;
                }
            }
            catch
            {
                StopWaitAnimation();
                _IsLoading = false;
            }
            StopWaitAnimation();
            _IsLoading = false;
            return bResult;
        }
        #endregion

        public double EthernetDataRate
        {
            get { return _EthernetTransactionRate; }
            set
            {
                if (_EthernetTransactionRate != value)
                {
                    _EthernetTransactionRate = value;
                    RaisePropertyChanged(nameof(EthernetDataRate));
                }
            }
        }

        public int MaxOpenFileAllowed
        {
            get { return _MaxOpenFileAllowed; }
        }

        public int FileNameCount
        {
            get { return _FileNameCount; }
            set { _FileNameCount = value; }
        }

        public int FileNameSetCount
        {
            get { return _FileNameSetCount; }
            set { _FileNameSetCount = value; }
        }

        /// <summary>
        /// Software production version string
        /// </summary>
        /*public string ProductVersion
        {
            get
            {
                string productVersion = string.Empty;

                if (Owner != null)
                {
                    productVersion = Owner.ProductVersion;
                }
                return productVersion;
            }
        }*/

        public bool IsAuthenticated
        {
            get { return _IsAuthenticated; }
            set
            {
                _IsAuthenticated = value;
                RaisePropertyChanged("IsAuthenticated");
            }
        }

        public bool Is8bpp
        {
            get
            {
                if (ActiveDocument != null)
                {
                    int bpp = ActiveDocument.Image.Format.BitsPerPixel;
                    _Is8bpp = (bpp == 8 || bpp == 24 || bpp == 32) ? true : false;
                }
                return _Is8bpp;
            }
            set
            {
                if (_Is8bpp != value)
                {
                    _Is8bpp = value;
                    RaisePropertyChanged("Is8bpp");
                }
            }
        }

        public bool Is16bpp
        {
            get
            {
                if (ActiveDocument != null)
                {
                    int bpp = ActiveDocument.Image.Format.BitsPerPixel;
                    _Is16bpp = (bpp == 16 || bpp == 48) ? true : false;
                }
                return _Is16bpp;
            }
            set
            {
                if (_Is16bpp != value)
                {
                    _Is16bpp = value;
                    RaisePropertyChanged("Is16bpp");
                }
            }
        }

        public bool Is32bppImage
        {
            get
            {
                if (ActiveDocument != null)
                {
                    int bpp = ActiveDocument.Image.Format.BitsPerPixel;
                    _Is32bppImage = (bpp == 32) ? true : false;
                }
                return _Is32bppImage;
            }
            set
            {
                if (_Is32bppImage != value)
                {
                    _Is32bppImage = value;
                    RaisePropertyChanged("Is32bppImage");
                }
            }
        }

        public bool IsShowCloseAll
        {
            get
            {
                bool bIsShowCloseAll = false;
                if (this.Files != null)
                {
                    if (this.Files.Count > 1)
                    {
                        bIsShowCloseAll = true;
                    }
                }
                return bIsShowCloseAll;
            }
        }

        #region public bool IsRGBImage
        //private bool _IsRGBImage = false;
        public bool IsRGBImage
        {
            get
            {
                bool bResult = false;
                if (ActiveDocument != null)
                {
                    bResult = ActiveDocument.IsRgbImage;
                    if (bResult)
                    {
                        _tempRGBImage = ActiveDocument.Image;
                        string ChannelRemark = ActiveDocument.ImageInfo.ChannelRemark;
                        if (ChannelRemark != null)
                        {
                            string[] split = ChannelRemark.Split(new Char[] { '_' }, StringSplitOptions.RemoveEmptyEntries);
                            if (split.Length >= 5)
                            {
                                if (split[1] == "L" || split[1] == "R1" || split[1] == "R2")
                                {
                                    IsRGBSaveCommand = true;
                                }
                                if (split[3] == "L" || split[3] == "R1" || split[3] == "R2")
                                {
                                    IsRGBSaveCommand = true;
                                }
                                if (split[5] == "L" || split[5] == "R1" || split[5] == "R2")
                                {
                                    IsRGBSaveCommand = true;
                                }
                            }
                        }
                        else
                        {
                            IsRGBSaveCommand = false;

                        }

                    }
                }
                return bResult;
            }
        }
        #endregion

        public bool IsProcessingContrast
        {
            get { return _IsProcessingContrast; }
            set
            {
                if (_IsProcessingContrast != value)
                {
                    _IsProcessingContrast = value;
                    RaisePropertyChanged("IsProcessingContrast");
                }
            }
        }
        //public int TitleCount { get; set; }
        //public string ControllerFWVersion { get; set; }
        //public string CameraFWVersion { get; set; }

        /***#region public bool IsControlBoardInitialized
        private bool _IsControlBoardInitialized = false;
        public bool IsControlBoardInitialized
        {
            get { return _IsControlBoardInitialized; }
            set
            {
                if (_IsControlBoardInitialized != value)
                {
                    _IsControlBoardInitialized = value;
                    RaisePropertyChanged("IsControlBoardInitialized");
                }
            }
        }
        #endregion***/

        /***#region public bool IsCameraInitialized
        private bool _IsCameraInitialized = false;
        public bool IsCameraInitialized
        {
            get { return _IsCameraInitialized; }
            set
            {
                if (_IsCameraInitialized != value)
                {
                    _IsCameraInitialized = value;
                    RaisePropertyChanged("IsCameraInitialized");
                }
            }
        }
        #endregion***/

        /***#region public bool InitControlBoard()
        /// <summary>
        /// Initialize the imaging system serial control board
        /// </summary>
        /// <returns>Returns true if successfully initialized, otherwise returns false</returns>
        public bool InitControlBoard()
        {
            bool bIsControlBoardInitialized = false;
            try
            {
                if (!_Owner.Global.bSimulationMode)
                {
                    MVController controller = MVController.CreateInstance();

                    if (controller.Initialize(_Owner.Global.iComPort) == false)
                    {
                        bIsControlBoardInitialized = false;
                    }
                    else
                    {
                        bIsControlBoardInitialized = true;
                        int firmwareVersion = controller.GetDeviceFirmwareVersion();
                        ControllerFWVersion = string.Format("{0:D4}", firmwareVersion);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
                return false;
            }

            return bIsControlBoardInitialized;
        }
        #endregion***/

        /***#region public bool InitCamera()
        /// <summary>
        /// Initialize the imaging system camera
        /// </summary>
        /// <returns>Returns true if successfully initialized, otherwise returns false</returns>
        public bool InitCamera()
        {
            bool bIsCameraInitialized = false;
            try
            {
                if (!_Owner.Global.bSimulationMode || _Owner.Global.bStandAlone)
                {
                    if (Workspace.This.IsC200)
                    {
                        //mPGRCamera = new PGRCamera(c200Settings.OffsetX, c200Settings.OffsetY, c200Settings.Width, c200Settings.Height);

                        PGRCamera pgrCamera = PGRCamera.CreateInstance();
                        pgrCamera.OffsetX = _Owner.c200Settings.OffsetX;
                        pgrCamera.OffsetY = _Owner.c200Settings.OffsetY;
                        pgrCamera.Width = _Owner.c200Settings.Width;
                        pgrCamera.Height = _Owner.c200Settings.Height;

                        bIsCameraInitialized = pgrCamera.Connect();

                        if (bIsCameraInitialized)
                        {
                            CameraFWVersion = pgrCamera.CameraInfo.firmwareVersion;
                        }
                    }
                    else
                    {
                        MVAltCamera mvAltCamera = MVAltCamera.CreateInstance();
                        if (false == mvAltCamera.Initialize(_Owner.Global))
                        {
                            bIsCameraInitialized = false;
                        }
                        else
                        {
                            bIsCameraInitialized = true;
                            CameraFWVersion = string.Format("{0}", mvAltCamera.AltaCamera.FirmwareVersion);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
                bIsCameraInitialized = false;
            }

            return bIsCameraInitialized;
        }
        #endregion***/

        /***#region public void InitializeHW(bool bIsSuppressWarning = false)
        /// <summary>
        /// Initialize the controller board and camera
        /// </summary>
        public void InitializeHW(bool bIsSuppressWarning = false)
        {
            if (!IsSimulationMode)
            {
                try
                {
                    _ConnectThread = new Thread(ConnectWorker);
                    _ConnectThread.Start();

                    if (_ConnectThread.Join(_ConnectWaitTime))
                    {
                        //Console.WriteLine("connect thread terminated.");

                        if (IsControlBoardInitialized == false && !IsStandAloneMode)
                        {
                            if (!bIsSuppressWarning)
                            {
                                Application.Current.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, (Action)(() =>
                                {
                                    string strMessage = "Error connecting to the system controller board.\n" +
                                                        "Please make sure the system power is turned on.";
                                    string strCaption = "Cabinet not detected...";
                                    MessageBox.Show(_Owner, strMessage, strCaption, MessageBoxButton.OK, MessageBoxImage.Error);
                                }));
                            }
                        }
                        else if (!IsCameraInitialized)
                        {
                            if (!bIsSuppressWarning)
                            {
                                Application.Current.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, (Action)(() =>
                                {
                                    string strMessage = "Error connecting to the system camera.\n" +
                                                        "Please make sure the system power is turned on.";
                                    string strCaption = "Camera not detected...";
                                    MessageBox.Show(_Owner, strMessage, strCaption, MessageBoxButton.OK, MessageBoxImage.Error);
                                }));
                            }
                        }
                    }
                    else
                    {
                        //Console.WriteLine("Join timed out.");

                        if (IsControlBoardInitialized == false && !IsStandAloneMode)
                        {
                            if (!bIsSuppressWarning)
                            {
                                Application.Current.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, (Action)(() =>
                                {
                                    string strMessage = "Error connecting to the system controller board.\n" +
                                                        "Please make sure the system power is turned on.";
                                    string strCaption = "Cabinet not detected...";
                                    MessageBox.Show(_Owner, strMessage, strCaption, MessageBoxButton.OK, MessageBoxImage.Error);
                                }));
                            }
                        }
                        else if (!IsCameraInitialized)
                        {
                            if (!bIsSuppressWarning)
                            {
                                Application.Current.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, (Action)(() =>
                                {
                                    string strMessage = "Error connecting to the system camera.\n" +
                                                        "Please make sure the system power is turned on.";
                                    string strCaption = "Camera not detected...";
                                    MessageBox.Show(_Owner, strMessage, strCaption, MessageBoxButton.OK, MessageBoxImage.Error);
                                }));
                            }
                        }
                    }
                }
                catch
                {
                }
            }
        }
        #endregion***/

        /// <summary>
        /// Hardware connection worker thread
        /// </summary>
        /***private void ConnectWorker()
        {
            if (IsSimulationMode) { return; }

            if (!IsStandAloneMode && !IsControlBoardInitialized)
            {
                // Initialize control board
                IsControlBoardInitialized = InitControlBoard();
            }

            if (!IsCameraInitialized)
            {
                if (IsControlBoardInitialized || IsStandAloneMode)
                {
                    // Initialize the the camera
                    IsCameraInitialized = InitCamera();
                }
            }
        }***/

        /*internal void SuccessfulInit()
        {
            EnableAppsBasedOnImagerType();
        }*/

        /*internal void FailedInit()
        {
            if (IsSimulationMode == true)
            {
                IsGelEnabled = true;
                IsChemiEnabled = true;
                IsRgbEnabled = true;
                IsNirEnabled = true;
                IsCustomEnabled = true;
                IsSettingsEnabled = true;
            }
            else
            {
                IsGelEnabled = false;
                IsChemiEnabled = false;
                IsRgbEnabled = false;
                IsNirEnabled = false;
                IsCustomEnabled = false;
                IsSettingsEnabled = false;
            }
        }*/


        /// <summary>
        /// Check if connection was lost after the camera has been initialized
        /// </summary>
        /***public bool IsCameraLostConnection
        {
            get
            {
                bool bIsLostConnection = false;
                try
                {
                    MVAltCamera mvAltCamera = MVAltCamera.CreateInstance();
                    ICamera2 camera = mvAltCamera.AltaCamera;

                    // Check camera connection status
                    // Once the camera is connected, it should never be in idle state
                    if (camera.ImagingStatus == Apn_Status.Apn_Status_Idle ||
                        camera.ImagingStatus == Apn_Status.Apn_Status_ConnectionError ||
                        camera.ImagingStatus == Apn_Status.Apn_Status_DataError ||
                        camera.ImagingStatus == Apn_Status.Apn_Status_PatternError)
                    {
                        bIsLostConnection = true;
                    }
                }
                catch
                {
                    bIsLostConnection = true;
                }
                return bIsLostConnection;
            }
        }***/

        /// <summary>
        /// Close the current connection and re-initialize the camera (if there's a camera connection error)
        /// </summary>
        /***public void InitApogeeCamera()
        {
            try
            {
                MVAltCamera mvAltCamera = MVAltCamera.CreateInstance();
                ICamera2 camera = mvAltCamera.AltaCamera;
                if (mvAltCamera != null)
                {
                    mvAltCamera.Finish();   // close connection
                    
                    System.Threading.Thread.Sleep(100);

                    if (!mvAltCamera.Initialize(_Owner.Global))
                    {
                        throw new Exception("Error connecting to the camera.\nPlease restart your system and try again.");
                    }
                }

                // Check camera connection status
                // Once the camera is connected, it should never be in idle state
                //if (camera.ImagingStatus == Apn_Status.Apn_Status_Idle ||
                //    camera.ImagingStatus == Apn_Status.Apn_Status_ConnectionError)
                //{
                //    mvAltCamera.Finish();
                //    System.Threading.Thread.Sleep(100);
                //    if (!mvAltCamera.Initialize(_Owner.Global))
                //    {
                //        throw new Exception("Error connecting to the camera.\nPlease restart your system and try again.");
                //    }
                //}
            }
            catch
            {
                MVAltCamera mvAltCamera = MVAltCamera.CreateInstance();
                ICamera2 camera = mvAltCamera.AltaCamera;

                mvAltCamera.Finish();
                System.Threading.Thread.Sleep(100);
                if (!mvAltCamera.Initialize(_Owner.Global))
                {
                    throw new Exception("Error connecting to the camera.\nPlease restart your system and try again.");
                }
            }
        }***/

        #region public bool IsBusyIndicator
        private bool _IsBusyIndicator = false;
        public bool IsBusyIndicator
        {
            get { return _IsBusyIndicator; }
            set
            {
                if (_IsBusyIndicator != value)
                {
                    _IsBusyIndicator = value;
                    RaisePropertyChanged("IsBusyIndicator");
                }
            }
        }
        #endregion

        #region public string BusyIndicatorContent
        private string _BusyIndicatorContent = "Loading....";
        public string BusyIndicatorContent
        {
            get { return _BusyIndicatorContent; }
            set
            {
                if (_BusyIndicatorContent != value)
                {
                    _BusyIndicatorContent = value;
                    RaisePropertyChanged("BusyIndicatorContent");
                }
            }
        }
        #endregion

        public void EnableBusyIndicator(string content)
        {
            BusyIndicatorContent = content;
            IsBusyIndicator = true;
        }

        public void DisableBusyIndicator()
        {
            BusyIndicatorContent = string.Empty;
            IsBusyIndicator = false;
        }

        #region public void StartWaitAnimation()
        /// <summary>
        /// Start animation window thread.
        /// </summary>
        /// <param name="content"></param>
        public void StartWaitAnimation(string content)
        {
            // Center the animation window in the center
            // of the dock manager grid container
            //var element = _Owner as Window;
            Point relativePoint = new Point(0, 0);
            double width = 0.0;
            double height = 0.0;
            if (Workspace.This.SelectedTabIndex == (int)ApplicationTabType.Gallery)   //Gallery tab
            {
                relativePoint = Owner.DockManagerContainer.TransformToAncestor(Owner).Transform(new Point(0, 0));
                width = Owner.DockManagerContainer.ActualWidth;
                height = Owner.DockManagerContainer.ActualHeight;
            }
            else if (Workspace.This.SelectedTabIndex == (int)ApplicationTabType.ScanChart)  //Scanchart tab
            {
                relativePoint = Owner.ScanChartContainer.TransformToAncestor(Owner).Transform(new Point(0, 0));
                width = Owner.ScanChartContainer.ActualWidth;
                height = Owner.ScanChartContainer.ActualHeight;
            }
            else if (Workspace.This.SelectedTabIndex == (int)ApplicationTabType.Imaging)  //Imaging tab
            {
                relativePoint = Owner.ImageViewerContainer.TransformToAncestor(Owner).Transform(new Point(0, 0));
                width = Owner.ImageViewerContainer.ActualWidth;
                height = Owner.ImageViewerContainer.ActualHeight;
            }
            if (Workspace.This.NewParameterVM.IsShowParameterWindow) //otheSetting Window Open
            {
                width = Owner.ActualWidth;
                height = Owner.ActualHeight;
            }
            //var location = element.PointToScreen(new Point(0, 0));
            //var location = new Point(element.Top, element.Left);
            var location = new Point(relativePoint.X, relativePoint.Y);
            //double width = element.ActualWidth;
            //double height = element.ActualHeight;
            //width = Owner.DockManagerContainer.ActualWidth;
            //eight = Owner.DockManagerContainer.ActualHeight;

            // Start animation
            _LoadingAnimManager = new LoadingAnimationManager(location, new Size(width, height), content);
            _LoadingAnimManager.BeginWaiting();
        }
        #endregion

        #region public void StopWaitAnimation()
        /// <summary>
        /// Close animation window thread.
        /// </summary>
        public void StopWaitAnimation()
        {
            if (_LoadingAnimManager != null)
            {
                // Close the animation window
                _LoadingAnimManager.EndWaiting();
                // Make sure the animation is closed
                while (_LoadingAnimManager.Thread.IsAlive)
                {
                    Thread.Sleep(100);
                    _LoadingAnimManager.EndWaiting();
                }
            }
        }
        #endregion


        #region NewCommand
        public ICommand NewCommand
        {
            get
            {
                if (_NewCommand == null)
                {
                    _NewCommand = new RelayCommand((p) => OnNew(p), (p) => CanNew(p));
                }

                return _NewCommand;
            }
        }

        private bool CanNew(object parameter)
        {
            return true;
        }

        private void OnNew(object parameter)
        {
            Files.Add(new FileViewModel());  // Add to the end
            ActiveDocument = Files.Last();   // Make the last item the active document
        }

        #endregion 

        #region OpenCommand
        public ICommand OpenCommand
        {
            get
            {
                if (_OpenCommand == null)
                {
                    _OpenCommand = new RelayCommand((p) => OnOpen(p), (p) => CanOpen(p));
                }

                return _OpenCommand;
            }
        }

        private bool CanOpen(object parameter)
        {
            return !_IsLoading;
        }

        private void OnOpen(object parameter)
        {
            //ExecuteRGBSaveCommand(null);
            var dlg = new OpenFileDialog();
            dlg.Filter = "TIF Files(.TIFF)|*.tif|JPG Files(.JPG)|*.jpg|BMP Files(.BMP)|*.bmp|All Files|*.*";
            dlg.Title = "Open File";
            dlg.CheckFileExists = true;
            dlg.CheckPathExists = true;
            dlg.Multiselect = false;
            if (dlg.ShowDialog().GetValueOrDefault())
            {
                SetPixelDefaultValue("open");
                BackgroundWorker worker = new BackgroundWorker();
                // Open the document in a different thread
                worker.DoWork += (o, ea) =>
                {
                    Application.Current.Dispatcher.Invoke((Action)delegate
                    {

                        var fileViewModel = Open(dlg.FileName);
                        ActiveDocument = fileViewModel;
                        ActiveDocument.IsFileLoaded = true;
                        ActiveDocument.IsAutoContrast = true;
                        ExecuteAutoContrastCommand(null);
                    });
                };
                worker.RunWorkerCompleted += (o, ea) =>
                {
                    // Work has completed. You can now interact with the UI
                    StopWaitAnimation();
                    _IsLoading = false;
                };

                StartWaitAnimation("Loading...");
                _IsLoading = true;

                worker.RunWorkerAsync();
            }
        }

        public FileViewModel Open(string filePath)
        {
            var fileViewModel = Files.FirstOrDefault(fm => fm.FilePath == filePath);
            if (fileViewModel != null)
            {
                return fileViewModel;
            }

            if (!File.Exists(filePath))
            {
                return null;
            }

            fileViewModel = new FileViewModel(filePath);
            if (Files != null)
            {
                Files.Add(fileViewModel);
            }

            
            // Remember initial directory
            SettingsManager.ApplicationSettings.InitialDirectory = System.IO.Path.GetDirectoryName(filePath);

            return fileViewModel;
        }

        #endregion 

        #region SaveCommand
        private RelayCommand _SaveCommand = null;
        public ICommand SaveCommand
        {
            get
            {
                if (_SaveCommand == null)
                {
                    _SaveCommand = new RelayCommand((p) => OnSave(p), (p) => CanSave(p));
                }

                return _SaveCommand;
            }
        }

        private bool CanSave(object parameter)
        {
            return (_ActiveDocument != null);
        }

        private void OnSave(object parameter)
        {
            //SaveAsync(ActiveDocument);
            SaveSync(ActiveDocument);
        }

        #endregion 

        #region SaveAsCommand
        private RelayCommand _SaveAsCommand = null;
        public ICommand SaveAsCommand
        {
            get
            {
                if (_SaveAsCommand == null)
                {
                    _SaveAsCommand = new RelayCommand((p) => OnSaveAs(p), (p) => CanSaveAs(p));
                }

                return _SaveAsCommand;
            }
        }

        private bool CanSaveAs(object parameter)
        {
            return (_ActiveDocument != null);
        }

        private void OnSaveAs(object parameter)
        {
            //SaveAsync(ActiveDocument, true);
            SaveSync(ActiveDocument, true, showOsKb : true);
        }

        #endregion 

        #region SaveAllCommand
        private RelayCommand _SaveAllCommand = null;
        public ICommand SaveAllCommand
        {
            get
            {
                if (_SaveAllCommand == null)
                {
                    _SaveAllCommand = new RelayCommand((p) => OnSaveAll(p), (p) => CanSaveAll(p));
                }

                return _SaveAllCommand;
            }
        }

        private bool CanSaveAll(object parameter)
        {
            return (_ActiveDocument != null);
        }

        private void OnSaveAll(object parameter)
        {
            bool bIsUnsavedFile = false;
            string destinationPath = string.Empty;
            string fileType = ".tif";
            bool bIsSaveAsCompressed = false;

            if (Files.Count != 0)
            {
                // Is there an unsaved files?
                for (int index = 0; index < Files.Count; index++)
                {
                    if (string.IsNullOrEmpty(Files[index].FilePath))
                    {
                        bIsUnsavedFile = true;
                        break;
                    }
                }

                // Get the destination folder if there's an unsaved file
                if (bIsUnsavedFile)
                {
                    destinationPath = SettingsManager.ApplicationSettings.InitialDirectory;

                    if (String.IsNullOrEmpty(destinationPath))
                    {
                        string commonPictureFolder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                        //string appCommonPictureFolder = commonPictureFolder + "\\" + Owner.ProductName;
                        //destinationPath = appCommonPictureFolder;
                    }

                    SelectFolder selectDestFolder = new SelectFolder(destinationPath);
                    selectDestFolder.Owner = Owner;
                    bool? dialogResult = selectDestFolder.ShowDialog();
                    if (dialogResult == true)
                    {
                        destinationPath = selectDestFolder.DestinationFolder;
                        string Path = destinationPath;
                        string volume = Path.Substring(0, Path.IndexOf(':'));
                        double freespace = GetHardDiskSpace(volume);
                        double imagemaxmin = 0;
                        for (int index = 0; index < Files.Count; index++)
                        {
                            if (Files[index].IsDirty)
                            {
                                if (string.IsNullOrEmpty(Files[index].FilePath))
                                {
                                    imagemaxmin += GetImageSpace(Files[index].Image);
                                }
                            }
                        }
                        if (freespace < imagemaxmin)
                        {
                            MessageBox.Show("The hard disk is short of space and needs extra space  " + (imagemaxmin - freespace + 2) + " MB");
                            return;
                        }
                        fileType = selectDestFolder.SelectedFileType;
                        bIsSaveAsCompressed = selectDestFolder.IsSaveAsCompressed;

                        if (!System.IO.Directory.Exists(destinationPath))
                        {
                            // Create destination folder (if it doesn't exist)
                            try
                            {
                                System.IO.Directory.CreateDirectory(destinationPath);
                            }
                            catch (Exception ex)
                            {
                                System.Windows.MessageBox.Show(ex.Message, "ERROR: Creating the specified directory...",
                                    MessageBoxButton.OK, MessageBoxImage.Stop);
                                return;
                            }
                        }

                        // Remember initial directory
                        SettingsManager.ApplicationSettings.InitialDirectory = System.IO.Path.GetDirectoryName(destinationPath);
                    }
                    else
                    {
                        return; // Cancel saving operation
                    }
                } //bIsUnsavedFile

                // Show wait window
                StartWaitAnimation("Saving...");

                try
                {
                    SaveAllFiles(destinationPath, fileType, bIsSaveAsCompressed);
                }
                catch
                {
                    // Rethrow to preserve stack details
                    // Satisfies the rule. 
                    throw;
                }
                finally
                {
                    StopWaitAnimation();
                }
            }
        }

        internal void SaveAllFiles(string destinationPath, string fileType, bool bIsSaveAsCompressed)
        {
            for (int index = 0; index < Files.Count; index++)
            {
                if (Files[index].IsDirty)
                {
                    if (string.IsNullOrEmpty(Files[index].FilePath))
                    {
                        string fileName = string.Empty;
                        string filePath = string.Empty;

                        while (true)
                        {
                            fileName = GenerateFileName(Files[index].Title, fileType);
                            filePath = Path.Combine(destinationPath, fileName);

                            // Make sure we don't have the duplicate file name
                            if (System.IO.File.Exists(filePath))
                            {
                                System.Threading.Thread.Sleep(1000);
                                continue;
                            }
                            else
                            {
                                break;
                            }
                        }
                        Files[index].FilePath = filePath;
                    }

                    // Save the image to disk
                    SaveFile(Files[index], bIsSaveAsCompressed);

                    Files[index].IsDirty = false;
                    Files[index].Title = Files[index].FileName;
                }
            }
        }

        private string GetLastOpenSaveFile(string extention)
        {
            RegistryKey regKey = Registry.CurrentUser;
            string lastUsedFolder = string.Empty;
            regKey = regKey.OpenSubKey("Software\\Microsoft\\Windows\\CurrentVersion\\Explorer\\ComDlg32\\OpenSavePidMRU");

            if (string.IsNullOrEmpty(extention))
                extention = "tif";

            RegistryKey myKey = regKey.OpenSubKey(extention);

            if (myKey == null && regKey.GetSubKeyNames().Length > 0)
                myKey = regKey.OpenSubKey(regKey.GetSubKeyNames()[regKey.GetSubKeyNames().Length - 2]);

            if (myKey != null)
            {
                string[] names = myKey.GetValueNames();
                if (names != null && names.Length > 0)
                {
                    lastUsedFolder = (string)myKey.GetValue(names[names.Length - 2]);
                }
            }

            return lastUsedFolder;
        }

        #endregion 


        #region CloseCommand
        private RelayCommand _CloseCommand = null;
        public ICommand CloseCommand
        {
            get
            {
                if (_CloseCommand == null)
                {
                    _CloseCommand = new RelayCommand((p) => ExecuteCloseCommand(p), (p) => CanExecuteCloseCommand(p));
                }

                return _CloseCommand;
            }
        }

        private void ExecuteCloseCommand(object parameter)
        {
            if (_ActiveDocument != null)
            {
                Close(_ActiveDocument);
            }
        }

        private bool CanExecuteCloseCommand(object parameter)
        {
            return (_ActiveDocument != null);
        }

        #endregion 

        #region CloseAllCommand
        private RelayCommand _CloseAllCommand = null;
        public ICommand CloseAllCommand
        {
            get
            {
                if (_CloseAllCommand == null)
                {
                    _CloseAllCommand = new RelayCommand((p) => ExecuteCloseAllCommand(p), (p) => CanExecuteCloseAllCommand(p));
                }

                return _CloseAllCommand;
            }
        }

        private void ExecuteCloseAllCommand(object parameter)
        {
            if (_ActiveDocument != null)
            {
                CloseAll();
            }
        }

        private bool CanExecuteCloseAllCommand(object parameter)
        {
            return (_ActiveDocument != null);
        }

        public bool CloseAll()
        {
            bool bResult = true;
            TaskDialogOptions config = new TaskDialogOptions();
            config.Owner = Owner;
            config.Title = "Save changes?";
            config.MainInstruction = "Save changes before closing?";
            //config.ExpandedInfo = "Any expanded content text for the " +
            //                      "task dialog is shown here and the text " +
            //                      "will automatically wrap as needed.";
            //config.VerificationText = "Don't show me this message again";
            //config.FooterText = "Optional footer text with an icon can be included.";
            config.MainIcon = VistaTaskDialogIcon.Shield;
            //config.FooterIcon = VistaTaskDialogIcon.Warning;

            while (Files.Count > 0)
            {
                config.Content = string.Format("Do you want save changes to '{0}'", Files[0].Title);

                if (Files.Count > 1)
                {
                    config.CustomButtons = new string[] { "&Save", "Do&n't save", "No to &All", "&Cancel" };
                }
                else
                {
                    config.CustomButtons = new string[] { "&Save", "Do&n't save", "&Cancel" };
                }

                TaskDialogResult taskDlgResult = null;

                if (Files[0].IsDirty)
                {
                    taskDlgResult = TaskDialog.Show(config);
                }
                else
                {
                    Remove(Files[0]);
                    GC.Collect();
                    continue;
                }

                if (taskDlgResult != null)
                {
                    if (taskDlgResult.CustomButtonResult == 0)
                    {
                        // Close the file: save before closing.
                        SaveSync(Files[0], closeAfterSaved : true);
                    }
                    else if (taskDlgResult.CustomButtonResult == 1)
                    {
                        // Close the file: don't save before closing.
                        Remove(Files[0]);
                        GC.Collect();
                    }
                    else if (taskDlgResult.CustomButtonResult == 2 && config.CustomButtons.Length == 4)
                    {
                        // Close all file: don't save before closing.
                        while (Files.Count > 0)
                        {
                            Remove(Files[0]);
                            GC.Collect();
                        }
                        break;
                    }
                    else if ((taskDlgResult.CustomButtonResult == 2 && config.CustomButtons.Length == 3) ||
                             taskDlgResult.CustomButtonResult == 3)
                    {
                        // Cancel is selected
                        bResult = false;
                        break;
                    }
                }
            } //while

            return bResult;
        }
        #region Memory recovery
        [DllImport("kernel32.dll", EntryPoint = "SetProcessWorkingSetSize")]
        public static extern int SetProcessWorkingSetSize(IntPtr process, int minSize, int maxSize);
        /// <summary>
        /// release memory.
        /// </summary>
        public static void ClearMemory()
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
            if (Environment.OSVersion.Platform == PlatformID.Win32NT)
            {
                SetProcessWorkingSetSize(System.Diagnostics.Process.GetCurrentProcess().Handle, -1, -1);
            }
        }
        #endregion
        /// <summary>
        /// Remove file and release memory.
        /// </summary>
        /// <param name="fileToRemove"></param>
        internal void Remove(FileViewModel fileToRemove)
        {
            if (ImageTitle == ActiveDocument.FileName)
            {
                SetPixelDefaultValue("Remove");
            }
         
            int nextItem = Files.IndexOf(fileToRemove) - 1;
            bool bIsRemoveActiveDoc = (fileToRemove == ActiveDocument) ? true : false;

            Files.Remove(fileToRemove);
            fileToRemove.ReleaseMemory();
            fileToRemove = null;

            // Forces a garbage collection
            GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced);
            GC.WaitForPendingFinalizers();
            GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced);

            // Retrieves the number of bytes currently thought to be allocated (true: force full collection)
            GC.GetTotalMemory(true);
            ClearMemory();
            if (Files.Count == 0)
            {
                ActiveDocument = null;
            }
            else
            {
                if (bIsRemoveActiveDoc)
                {
                    if (nextItem < 0)
                    {
                        nextItem = 0;
                    }
                    ActiveDocument = Files[nextItem];
                }
            }

            RaisePropertyChanged("IsActiveDocument");
            RaisePropertyChanged("IsShowCloseAll");
        }

        #endregion 

        #region PrintCommand
        private RelayCommand _PrintCommand = null;
        /// <summary>
        /// Get the print command.
        /// </summary>
        public ICommand PrintCommand
        {
            get
            {
                if (_PrintCommand == null)
                {
                    _PrintCommand = new RelayCommand(this.ExecutePrintCommand, null);
                }

                return _PrintCommand;
            }
        }
        #region protected void ExecutePrintCommand(object parameter)
        /// <summary>
        /// Print command implementation.
        /// </summary>
        /// <param name="parameter"></param>
        protected void ExecutePrintCommand(object parameter)
        {
            if (ActiveDocument == null) { return; }

            PrintDialog printDlg = new PrintDialog();
            if (printDlg.ShowDialog() != true) { return; }

            System.Windows.Controls.Image image = new System.Windows.Controls.Image();
            image.Source = ActiveDocument.DisplayImage;
            string fileName = ActiveDocument.FileName;
            string dateTime = ActiveDocument.ImageInfo.DateTime;
            //double margin = 20.0;
            Thickness marginPage = new Thickness(25);

            // Calculate rectangle for image
            //double width = printDlg.PrintableAreaWidth / 2;
            //double height = width * image.Source.Height / image.Source.Width;
            //double left = (printDlg.PrintableAreaWidth - width) / 2;
            //double top = (printDlg.PrintableAreaHeight - height) / 2;

            double width = printDlg.PrintableAreaWidth - (marginPage.Left + marginPage.Right);
            double height = width * image.Source.Height / image.Source.Width;
            double fontSize = 12;
            double left = marginPage.Left;
            double top = marginPage.Right;

            //Rect rect = new Rect(left, top, width, height);

            // Create DrawingVisual and get its drawing context
            DrawingVisual vs = new DrawingVisual();
            DrawingContext dc = vs.RenderOpen();

            // Create formatted text--in a particular font at a particular size
            FormattedText formtxt = new FormattedText(
                fileName,
                CultureInfo.CurrentCulture,
                FlowDirection.LeftToRight,
                new Typeface(new FontFamily("Trebuchet MS"), FontStyles.Normal, FontWeights.Normal, FontStretches.Normal),
                fontSize,
                Brushes.Black);

            // Draw the text at a location (file name : left justify)
            dc.DrawText(formtxt, new Point(marginPage.Left, marginPage.Top));

            // Create formatted text--in a particular font at a particular size
            formtxt = new FormattedText(
                dateTime,
                CultureInfo.CurrentCulture,
                FlowDirection.LeftToRight,
                new Typeface(new FontFamily("Trebuchet MS"), FontStyles.Normal, FontWeights.Normal, FontStretches.Normal),
                fontSize,
                Brushes.Black);

            // Get size of text.
            Size sizeText = new Size(formtxt.Width, formtxt.Height);
            double destX = width - sizeText.Width + marginPage.Left;

            // Draw the text at a location (date-time : right justify)
            dc.DrawText(formtxt, new Point(destX, marginPage.Top));

            top = marginPage.Top + sizeText.Height + 2;
            Rect rect = new Rect(left, top, width, height);

            // Draw image
            dc.DrawImage(image.Source, rect);

            //double scale = width / image.Source.Width;

            // Keep old existing actual scale and set new actual scale.
            //double oldActualScale = ActiveDocument.DrawingCanvas.ActualScale;
            //ActiveDocument.DrawingCanvas.ActualScale = scale;

            // Remove clip in the canvas - we set our own clip.
            //ActiveDocument.DrawingCanvas.RemoveClip();

            // Prepare drawing context to draw graphics
            //dc.PushClip(new RectangleGeometry(rect));
            //dc.PushTransform(new TranslateTransform(left, top));
            //dc.PushTransform(new ScaleTransform(scale, scale));

            // Ask canvas to draw overlays
            //ActiveDocument.DrawingCanvas.Draw(dc);

            // Restore old actual scale.
            //ActiveDocument.DrawingCanvas.ActualScale = oldActualScale;

            // Restore clip
            //ActiveDocument.DrawingCanvas.RefreshClip();

            //dc.Pop();
            //dc.Pop();
            //dc.Pop();

            dc.Close();

            // Print DrawVisual
            printDlg.PrintVisual(vs, fileName);
        }
        #endregion

        //#region protected bool CanExecutePrintCommand(object parameter)
        ///// <summary>
        ///// 
        ///// </summary>
        ///// <param name="parameter"></param>
        ///// <returns></returns>
        //protected bool CanExecutePrintCommand(object parameter)
        //{
        //    return (Workspace.This.ActiveDocument != null);
        //}
        //#endregion

        /// <summary>
        /// This function prints graphics with background image.
        /// </summary>
        /*void PrintImageWithAnnotations(Image image)
        {
            PrintDialog dlg = new PrintDialog();

            if (dlg.ShowDialog().GetValueOrDefault() != true)
            {
                return;
            }

            // Calculate rectangle for image
            double width = dlg.PrintableAreaWidth / 2;
            double height = width * image.Source.Height / image.Source.Width;

            double left = (dlg.PrintableAreaWidth - width) / 2;
            double top = (dlg.PrintableAreaHeight - height) / 2;

            Rect rect = new Rect(left, top, width, height);

            // Create DrawingVisual and get its drawing context
            DrawingVisual vs = new DrawingVisual();
            DrawingContext dc = vs.RenderOpen();

            // Draw image
            dc.DrawImage(image.Source, rect);

            double scale = width / image.Source.Width;

            // Keep old existing actual scale and set new actual scale.
            double oldActualScale = ActiveDocument.DrawingCanvas.ActualScale;
            ActiveDocument.DrawingCanvas.ActualScale = scale;

            // Remove clip in the canvas - we set our own clip.
            ActiveDocument.DrawingCanvas.RemoveClip();

            // Prepare drawing context to draw graphics
            dc.PushClip(new RectangleGeometry(rect));
            dc.PushTransform(new TranslateTransform(left, top));
            dc.PushTransform(new ScaleTransform(scale, scale));

            // Ask canvas to draw overlays
            ActiveDocument.DrawingCanvas.Draw(dc);

            // Restore old actual scale.
            ActiveDocument.DrawingCanvas.ActualScale = oldActualScale;

            // Restore clip
            ActiveDocument.DrawingCanvas.RefreshClip();

            dc.Pop();
            dc.Pop();
            dc.Pop();

            dc.Close();

            // Print DrawVisual
            dlg.PrintVisual(vs, "Graphics");
        }*/

        #endregion

        #region QuitCommand
        private RelayCommand _QuitCommand = null;
        public ICommand QuitCommand
        {
            get
            {
                if (_QuitCommand == null)
                {
                    _QuitCommand = new RelayCommand((p) => ExecuteQuitCommand(p), (p) => CanExecuteQuitCommand(p));
                }

                return _QuitCommand;
            }
        }

        private void ExecuteQuitCommand(object parameter)
        {
            this.CloseAction();
        }

        private bool CanExecuteQuitCommand(object parameter)
        {
            return true;
        }

        #endregion 

        #region BlackSliderContrastCommand

        private RelayCommand _BlackSliderContrastCommand = null;
        public ICommand BlackSliderContrastCommand
        {
            get
            {
                if (_BlackSliderContrastCommand == null)
                {
                    _BlackSliderContrastCommand = new RelayCommand(ExecuteBlackSliderContrastCommand, CanExecuteBlackSliderContrastCommand);
                }

                return _BlackSliderContrastCommand;
            }
        }

        protected void ExecuteBlackSliderContrastCommand(object parameter)
        {
            if (!IsActiveDocument) { return; }

            if (ActiveDocument.IsAutoContrast)
            {
                ActiveDocument.IsManualContrast = true; // manual contrast, don't restore saved B/W/G values
                ActiveDocument.IsAutoContrast = false;
            }

            if (ActiveDocument.IsRgbImage && ActiveDocument.SelectedChannelType == ImageChannelType.Mix)
            {
                // composite channel manual contrast; de-select auto in the individual channel
                ActiveDocument.ImageInfo.RedChannel.IsAutoChecked = false;
                ActiveDocument.ImageInfo.GreenChannel.IsAutoChecked = false;
                ActiveDocument.ImageInfo.BlueChannel.IsAutoChecked = false;
            }

            ActiveDocument.UpdateDisplayImage();

            ///*if (btnHistogram.IsChecked == true)
            //{
            //    int iPixelType;
            //    // create the histogram
            //    iPixelType = MVImage.GetPixelType(bitmap.Format);
            //    if (MVImage.P8uC3_1 == iPixelType)
            //    {
            //        int[,] p8uLevels = new int[3, 256];
            //        for (int i = 0; i < 256; ++i)
            //        {
            //            p8uLevels[0, i] = i;
            //            p8uLevels[1, i] = i;
            //            p8uLevels[2, i] = i;
            //        }
            //        int[,] p8uHist = new int[3, 256];
            //        int[] pn8uLevels = new int[3];
            //        for (int i = 0; i < 3; ++i)
            //        {
            //            pn8uLevels[i] = 256;
            //        }
            //        double iScaleX = canvasHistogram.ActualWidth / 256;
            //        MVImage.ImageHistogram(bitmap, p8uHist, p8uLevels, pn8uLevels);
            //        DrawHistogram(bitmap, iPixelType, p8uHist, p8uLevels, pn8uLevels, iScaleX);
            //    }
            //}*/
        }

        protected bool CanExecuteBlackSliderContrastCommand(object parameter)
        {
            return true;
        }

        #endregion

        #region WhiteSliderContrastCommand

        private RelayCommand _WhiteSliderContrastCommand = null;
        public ICommand WhiteSliderContrastCommand
        {
            get
            {
                if (_WhiteSliderContrastCommand == null)
                {
                    _WhiteSliderContrastCommand = new RelayCommand(ExecuteWhiteSliderContrastCommand, CanExecuteWhiteSliderContrastCommand);
                }

                return _WhiteSliderContrastCommand;
            }
        }

        protected void ExecuteWhiteSliderContrastCommand(object parameter)
        {
            if (!IsActiveDocument) { return; }

            if (ActiveDocument.IsAutoContrast == true)
            {
                ActiveDocument.IsManualContrast = true; // manual contrast, don't restore saved B/W/G values
                ActiveDocument.IsAutoContrast = false;
            }

            if (ActiveDocument.IsRgbImage && ActiveDocument.SelectedChannelType == ImageChannelType.Mix)
            {
                // composite channel manual contrast; de-select auto in the individual channel
                ActiveDocument.ImageInfo.RedChannel.IsAutoChecked = false;
                ActiveDocument.ImageInfo.GreenChannel.IsAutoChecked = false;
                ActiveDocument.ImageInfo.BlueChannel.IsAutoChecked = false;
            }

            ActiveDocument.UpdateDisplayImage();

            /*if (btnHistogram.IsChecked == true)
            {
                int iPixelType;
                // create the histogram
                iPixelType = MVImage.GetPixelType(bitmap.Format);

                //int iMax = MVImage.Max(m_MainWindow.imageTabControl.CurrentBitmapSource);

                if (iPixelType == MVImage.P8uC1)
                {
                    int[,] p8uLevels = new int[1, 256];
                    for (int i = 0; i < 256; ++i)
                    {
                        p8uLevels[0, i] = i;
                    }
                    int[,] p8uHist = new int[1, 256];
                    int[] pn8uLevels = new int[1];
                    for (int i = 0; i < 1; ++i)
                    {
                        pn8uLevels[i] = 256;
                    }
                    double iScaleX = canvasHistogram.ActualWidth / 256;
                    MVImage.ImageHistogram(_MainWindow.imageTabControl.CurrentBitmap, p8uHist, p8uLevels, pn8uLevels);
                    DrawHistogram(_MainWindow.imageTabControl.CurrentBitmap, iPixelType, p8uHist, p8uLevels, pn8uLevels, iScaleX);
                }
                else if (MVImage.P8uC3_1 == iPixelType)
                {
                    int[,] p8uLevels = new int[3, 256];
                    for (int i = 0; i < 256; ++i)
                    {
                        p8uLevels[0, i] = i;
                        p8uLevels[1, i] = i;
                        p8uLevels[2, i] = i;
                    }
                    int[,] p8uHist = new int[3, 256];
                    int[] pn8uLevels = new int[3];
                    for (int i = 0; i < 3; ++i)
                    {
                        pn8uLevels[i] = 256;
                    }

                    double iScaleX = canvasHistogram.ActualWidth / 256;
                    MVImage.ImageHistogram(bitmap, p8uHist, p8uLevels, pn8uLevels);
                    DrawHistogram(_MainWindow.imageTabControl.CurrentBitmap, iPixelType, p8uHist, p8uLevels, pn8uLevels, iScaleX);
                }
                else if (iPixelType == MVImage.P16uC1)
                {
                    int[,] pLevels = new int[1, 65536];
                    for (int i = 0; i < 65536; ++i)
                    {
                        pLevels[0, i] = i;
                    }
                    int[,] pHist = new int[1, 65536];
                    int[] pnLevels = new int[1];
                    for (int i = 0; i < 1; ++i)
                    {
                        pnLevels[i] = 65536;
                    }
                    double iScaleX = canvasHistogram.ActualWidth / 65536;
                    MVImage.ImageHistogram(_MainWindow.imageTabControl.CurrentBitmap, pHist
                        , pLevels, pnLevels);
                    DrawHistogram(_MainWindow.imageTabControl.CurrentBitmap, iPixelType, pHist, pLevels, pnLevels, iScaleX);
                }
                else if (MVImage.P16uC3 == iPixelType)
                {
                    double iScaleX = canvasHistogram.ActualWidth / 65536;
                    //MVImage.ImageHistogram(m_MainWindow.imageTabControl.CurrentBitmapSource, p16uHist, p16uLevels, pn16uLevels);
                }
            }*/
        }

        protected bool CanExecuteWhiteSliderContrastCommand(object parameter)
        {
            return true;
        }

        #endregion

        #region GammaSliderContrastCommand

        private RelayCommand _GammaSliderContrastCommand = null;
        public ICommand GammaSliderContrastCommand
        {
            get
            {
                if (_GammaSliderContrastCommand == null)
                {
                    _GammaSliderContrastCommand = new RelayCommand(ExecuteGammaSliderContrastCommand, CanExecuteGammaSliderContrastCommand);
                }

                return _GammaSliderContrastCommand;
            }
        }

        protected void ExecuteGammaSliderContrastCommand(object parameter)
        {
            if (!IsActiveDocument) { return; }

            if (ActiveDocument.IsAutoContrast == true)
            {
                ActiveDocument.IsManualContrast = true; // manual contrast, don't restore the saved B/W/G values
                ActiveDocument.IsAutoContrast = false;
            }

            if (ActiveDocument.IsRgbImage && ActiveDocument.SelectedChannelType == ImageChannelType.Mix)
            {
                // composite channel manual contrast; de-select auto in the individual channel
                ActiveDocument.ImageInfo.RedChannel.IsAutoChecked = false;
                ActiveDocument.ImageInfo.GreenChannel.IsAutoChecked = false;
                ActiveDocument.ImageInfo.BlueChannel.IsAutoChecked = false;
            }

            ActiveDocument.UpdateDisplayImage();
        }

        protected bool CanExecuteGammaSliderContrastCommand(object parameter)
        {
            return true;
        }

        #endregion

        #region AutoContrastCommand
        private RelayCommand _AutoContrastCommand = null;
        /// <summary>
        /// Get the auto-contrast command.
        /// </summary>
        public ICommand AutoContrastCommand
        {
            get
            {
                if (_AutoContrastCommand == null)
                {
                    _AutoContrastCommand = new RelayCommand(ExecuteAutoContrastCommand, CanExecuteAutoContrastCommand);
                }

                return _AutoContrastCommand;
            }
        }
        #region protected void ExecuteAutoContrastCommand(object parameter)
        /// <summary>
        ///Flip vertical command implementation.
        /// </summary>
        /// <param name="parameter"></param>
        protected void ExecuteAutoContrastCommand(object parameter)
        {
            //FileViewModel activeDocument = Workspace.This.ActiveDocument;
            if (ActiveDocument != null && !IsProcessingContrast)
            {
                try
                {
                    IsProcessingContrast = true;

                    ActiveDocument.UpdateDisplayImage();
                }
                catch (Exception ex)
                {
                    throw new Exception(string.Format("Error creating the display image.\n{0}", ex.Message));
                }
                finally
                {
                    IsProcessingContrast = false;
                }
            }
        }
        #endregion

        #region protected bool CanExecuteAutoContrastCommand(object parameter)
        /// <summary>
        /// 
        /// </summary>
        /// <param name="parameter"></param>
        /// <returns></returns>
        protected bool CanExecuteAutoContrastCommand(object parameter)
        {
            return (IsProcessingContrast != true);
        }
        #endregion

        #endregion

        #region InvertCommand
        private RelayCommand _InvertCommand = null;
        /// <summary>
        /// Get the inversion command.
        /// </summary>
        public ICommand InvertCommand
        {
            get
            {
                if (_InvertCommand == null)
                {
                    _InvertCommand = new RelayCommand(ExecuteInvertCommand, CanExecuteInvertCommand);
                }

                return _InvertCommand;
            }
        }
        #region protected void ExecuteInvertCommand(object parameter)
        /// <summary>
        ///Invert command implementation.
        /// </summary>
        /// <param name="parameter"></param>
        protected void ExecuteInvertCommand(object parameter)
        {
            //FileViewModel activeDocument = Workspace.This.ActiveDocument;
            if (ActiveDocument != null && !IsProcessingContrast)
            {
                try
                {
                    IsProcessingContrast = true;

                    ActiveDocument.UpdateDisplayImage();
                }
                catch (Exception ex)
                {
                    throw new Exception(string.Format("Error creating the display image.\n{0}", ex.Message));
                }
                finally
                {
                    IsProcessingContrast = false;
                }
            }
        }
        #endregion

        #region protected bool CanExecuteInvertCommand(object parameter)
        /// <summary>
        /// 
        /// </summary>
        /// <param name="parameter"></param>
        /// <returns></returns>
        protected bool CanExecuteInvertCommand(object parameter)
        {
            return (IsProcessingContrast != true);
        }
        #endregion

        #endregion

        #region SaturationCommand
        private RelayCommand _SaturationCommand = null;
        /// <summary>
        /// Get the saturation command.
        /// </summary>
        public ICommand SaturationCommand
        {
            get
            {
                if (_SaturationCommand == null)
                {
                    _SaturationCommand = new RelayCommand(this.ExecuteSaturationCommand, CanExecuteSaturationCommand);
                }

                return _SaturationCommand;
            }
        }
        #region protected void ExecuteSaturationCommand(object parameter)
        /// <summary>
        ///Saturation command implementation.
        /// </summary>
        /// <param name="parameter"></param>
        protected void ExecuteSaturationCommand(object parameter)
        {
            //FileViewModel activeDocument = Workspace.This.ActiveDocument;
            if (ActiveDocument != null && !IsProcessingContrast)
            {
                try
                {
                    IsProcessingContrast = true;
                    ActiveDocument.UpdateDisplayImage();
                }
                catch (Exception ex)
                {
                    throw new Exception(string.Format("Error creating the display image.\n{0}", ex.Message));
                }
                finally
                {
                    IsProcessingContrast = false;
                }
            }
        }
        #endregion

        #region protected bool CanExecuteSaturationCommand(object parameter)
        /// <summary>
        /// 
        /// </summary>
        /// <param name="parameter"></param>
        /// <returns></returns>
        protected bool CanExecuteSaturationCommand(object parameter)
        {
            return (IsProcessingContrast != true);
        }
        #endregion

        #endregion

        #region RGBSaveCommand
        private bool _IsRGBSaveCommand = false;
        public bool IsRGBSaveCommand
        {
            get { return _IsRGBSaveCommand; }
            set
            {
                if (_IsRGBSaveCommand != value)
                {
                    _IsRGBSaveCommand = value;
                    RaisePropertyChanged("IsRGBSaveCommand");
                }
            }
        }
        private RelayCommand _RGBSaveCommand = null;
        public ICommand RGBSaveCommand
        {
            get
            {
                if (_RGBSaveCommand == null)
                {
                    _RGBSaveCommand = new RelayCommand(ExecuteRGBSaveCommand, CanExecuteRGBSaveCommand);
                }

                return _RGBSaveCommand;
            }
        }
        protected void ExecuteRGBSaveCommand(object parameter)
        {
            try
            {
                if (ActiveDocument != null)
                {
                    string ChannelRemake = ActiveDocument.ImageInfo.ChannelRemark;
                    if (ChannelRemake == null)
                    {
                        return;
                    }
                    //When manually moving vertically or horizontally in RGB color 3 channels, ChannelRemake needs to be used to distinguish which bin the current moving channel is in
                    //当在RGB彩色3通道手动垂直移动或者水平移动时，需要用到ChannelRemake来区分当前移动的通道是哪个仓位的。
                    if (PixelRedX != 0 || PixelRedY != 0 || PixelBlueX != 0 || PixelBlueY != 0 || PixelGreenX != 0 || PixelGreenY != 0)
                    {
                        string[] split = ChannelRemake.Split(new Char[] { '_' }, StringSplitOptions.RemoveEmptyEntries);

                        #region 通道类型1
                        if (split[0] == "Red" && split[1] == "L")
                        {
                            This.NewParameterVM.Pixel_10_L_DX = PixelRedX;
                            This.NewParameterVM.Pixel_10_L_DY = PixelRedY;
                        }
                        if (split[0] == "Green" && split[1] == "L")
                        {
                            This.NewParameterVM.Pixel_10_L_DX = PixelGreenX;
                            This.NewParameterVM.Pixel_10_L_DY = PixelGreenY;
                        }
                        if (split[0] == "Blue" && split[1] == "L")
                        {
                            This.NewParameterVM.Pixel_10_L_DX = PixelBlueX;
                            This.NewParameterVM.Pixel_10_L_DY = PixelBlueY;
                        }
                        if (split[0] == "Red" && split[1] == "R1")
                        {
                            This.NewParameterVM.Pixel_10_R2_DX = PixelRedX;
                            This.NewParameterVM.Pixel_10_R2_DY = PixelRedY;
                        }
                        if (split[0] == "Green" && split[1] == "R1")
                        {
                            This.NewParameterVM.Pixel_10_R2_DX = PixelGreenX;
                            This.NewParameterVM.Pixel_10_R2_DY = PixelGreenY;
                        }
                        if (split[0] == "Blue" && split[1] == "R1")
                        {
                            This.NewParameterVM.Pixel_10_R2_DX = PixelBlueX;
                            This.NewParameterVM.Pixel_10_R2_DY = PixelBlueY;
                        }
                        #endregion

                        #region 通道类型2
                        if (split[2] == "Red" && split[3] == "L")
                        {
                            This.NewParameterVM.Pixel_10_L_DX = PixelRedX;
                            This.NewParameterVM.Pixel_10_L_DY = PixelRedY;
                        }
                        if (split[2] == "Green" && split[3] == "L")
                        {
                            This.NewParameterVM.Pixel_10_L_DX = PixelGreenX;
                            This.NewParameterVM.Pixel_10_L_DY = PixelGreenY;
                        }
                        if (split[2] == "Blue" && split[3] == "L")
                        {
                            This.NewParameterVM.Pixel_10_L_DX = PixelBlueX;
                            This.NewParameterVM.Pixel_10_L_DY = PixelBlueY;
                        }
                        if (split[2] == "Red" && split[3] == "R1")
                        {
                            This.NewParameterVM.Pixel_10_R2_DX = PixelRedX;
                            This.NewParameterVM.Pixel_10_R2_DY = PixelRedY;
                        }
                        if (split[2] == "Green" && split[3] == "R1")
                        {
                            This.NewParameterVM.Pixel_10_R2_DX = PixelGreenX;
                            This.NewParameterVM.Pixel_10_R2_DY = PixelGreenY;
                        }
                        if (split[2] == "Blue" && split[3] == "R1")
                        {
                            This.NewParameterVM.Pixel_10_R2_DX = PixelBlueX;
                            This.NewParameterVM.Pixel_10_R2_DY = PixelBlueY;
                        }
                        #endregion

                        #region 通道类型3
                        if (split[4] == "Red" && split[5] == "L")
                        {
                            This.NewParameterVM.Pixel_10_L_DX = PixelRedX;
                            This.NewParameterVM.Pixel_10_L_DY = PixelRedY;
                        }
                        if (split[4] == "Green" && split[5] == "L")
                        {
                            This.NewParameterVM.Pixel_10_L_DX = PixelGreenX;
                            This.NewParameterVM.Pixel_10_L_DY = PixelGreenY;
                        }
                        if (split[4] == "Blue" && split[5] == "L")
                        {
                            This.NewParameterVM.Pixel_10_L_DX = PixelBlueX;
                            This.NewParameterVM.Pixel_10_L_DY = PixelBlueY;
                        }
                        if (split[4] == "Red" && split[5] == "R1")
                        {
                            This.NewParameterVM.Pixel_10_R2_DX = PixelRedX;
                            This.NewParameterVM.Pixel_10_R2_DY = PixelRedY;
                        }
                        if (split[4] == "Green" && split[5] == "R1")
                        {
                            This.NewParameterVM.Pixel_10_R2_DX = PixelGreenX;
                            This.NewParameterVM.Pixel_10_R2_DY = PixelGreenY;
                        }
                        if (split[4] == "Blue" && split[5] == "R1")
                        {
                            This.NewParameterVM.Pixel_10_R2_DX = PixelBlueX;
                            This.NewParameterVM.Pixel_10_R2_DY = PixelBlueY;
                        }
                        #endregion

                        This.NewParameterVM.ExecuteParametersWriteCommand(null);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception(string.Format("Error creating the display image.\n{0}", ex.Message));
            }
            finally
            {
                IsProcessingContrast = false;
            }
        }
        protected bool CanExecuteRGBSaveCommand(object parameter)
        {
            return (IsProcessingContrast != true);
        }

        #endregion

        #region RGBMoveCommand
        private int _PixelX = 0;
        public int PixelX
        {
            get { return _PixelX; }
            set
            {
                if (_PixelX != value)
                {
                    _PixelX = value;
                    RaisePropertyChanged("PixelX");
                }
            }
        }
        private int _PixelY = 0;
        public int PixelY
        {
            get { return _PixelY; }
            set
            {
                if (_PixelY != value)
                {
                    _PixelY = value;
                    RaisePropertyChanged("PixelY");
                }
            }
        }
        public int PixelRedX
        {
            get;
            set;
        }
        public int PixelRedY
        {
            get;
            set;
        }
        public int PixelBlueX
        {
            get;
            set;
        }
        public int PixelBlueY
        {
            get;
            set;
        }
        public int PixelGreenX
        {
            get;
            set;
        }
        public int PixelGreenY
        {
            get;
            set;
        }
        public int PixelGrayX 
        {
            get;
            set;
        }
        public int PixelGrayY
        {
            get;
            set;
        }
        private string ImageTitle = "";
        private RelayCommand _RGBMoveCommand = null;
        public ICommand RGBMoveCommand
        {
            get
            {
                if (_RGBMoveCommand == null)
                {
                    _RGBMoveCommand = new RelayCommand(ExecuteRGBMoveCommand, CanExecuteRGBMoveCommand);
                }

                return _RGBMoveCommand;
            }
        }
        public WriteableBitmap _tempSrcImg = null;
        public WriteableBitmap _tempRGBImage = null;
        private WriteableBitmap _tempRedChannel = null;
        private WriteableBitmap _tempGreenChannel = null;
        private WriteableBitmap _tempBlueChannel = null;
        private WriteableBitmap _tempMixChannel = null;
        private WriteableBitmap[] ImageChannels = { null, null, null };
        protected void ExecuteRGBMoveCommand(object parameter)
        {
            try
            {
                if (ActiveDocument != null)
                {
                    ImageTitle = ActiveDocument.FileName;
                    BackgroundWorker worker = new BackgroundWorker();
                    // Open the document in a different thread
                    worker.DoWork += (o, ea) =>
                    {
                        Application.Current.Dispatcher.Invoke((Action)delegate
                        {
                            if (!IsActiveDocument || !ActiveDocument.IsRgbImage)
                            {
                                PixelGrayX = PixelX;
                                PixelGrayY = PixelY;
                                ActiveDocument.UpdatePixelMoveDisplayImage(PixelX, PixelX, PixelY, PixelY);
                                return;
                            }
                            if (ActiveDocument.Image.Format.BitsPerPixel != 24 &&
                   ActiveDocument.Image.Format.BitsPerPixel != 48)
                            {
                                return;
                            }
                            WriteableBitmap[] _tempImageChannels = { null, null, null }; // color order BGR
                            _tempImageChannels = ImageProcessing.GetChannel(_tempRGBImage);
                            if (_tempImageChannels == null)
                            {
                                return;
                            }
                            if (ActiveDocument.SelectedChannelType == ImageChannelType.Red)
                            {
                                // red channel
                                if (_tempImageChannels[0] != null)
                                {
                                    if (PixelRedX != PixelX || PixelY != PixelRedY)
                                    {
                                        PixelRedX = PixelX;
                                        PixelRedY = PixelY;
                                        if (PixelX == 0 && PixelY == 0)
                                        {
                                            ImageChannels[0] = null;
                                           ActiveDocument.Image = _tempRGBImage;
                                        }
                                        else
                                        {
                                            ActiveDocument.UpdatePixelMoveDisplayImage(ref _tempImageChannels[0], PixelRedX, PixelRedX, PixelRedY, PixelRedY);
                                            ImageChannels[0] = _tempImageChannels[0];
                                            _tempRedChannel = ImageProcessing.SetChannel(_tempImageChannels);//Move
                                            ActiveDocument.Image = _tempRedChannel;
                                        }
                                    }
                                    else if (PixelX == 0 && PixelY == 0)
                                    {
                                        PixelRedX = PixelX;
                                        PixelRedY = PixelY;
                                        ImageChannels[0] = null;
                                        ActiveDocument.Image = _tempRGBImage;
                                    }
                                    else
                                    {
                                        ActiveDocument.Image = _tempRedChannel;
                                    }
                                    ActiveDocument.UpdateDisplayImage();
                                }
                            }
                            if (ActiveDocument.SelectedChannelType == ImageChannelType.Green)
                            {
                                // Green channel
                                if (_tempImageChannels[1] != null)
                                {
                                    if (PixelGreenX != PixelX || PixelY != PixelGreenY)
                                    {
                                        PixelGreenX = PixelX;
                                        PixelGreenY = PixelY;
                                        if (PixelX == 0 && PixelY == 0)
                                        {
                                            ImageChannels[1] = null;
                                            ActiveDocument.Image = _tempRGBImage;
                                        }
                                        else
                                        {
                                            ActiveDocument.UpdatePixelMoveDisplayImage(ref _tempImageChannels[1], PixelGreenX, PixelGreenX, PixelGreenY, PixelGreenY);
                                            ImageChannels[1] = _tempImageChannels[1];
                                            _tempGreenChannel = ImageProcessing.SetChannel(_tempImageChannels);
                                            ActiveDocument.Image = _tempGreenChannel;
                                        }
                                    }
                                    else if (PixelX == 0 && PixelY == 0)
                                    {
                                        PixelGreenX = PixelX;
                                        PixelGreenY = PixelY;
                                        ImageChannels[1] = null;
                                        ActiveDocument.Image = _tempRGBImage;
                                    }
                                    else
                                    {
                                        ActiveDocument.Image = _tempGreenChannel;
                                    }
                                    ActiveDocument.UpdateDisplayImage();
                                }
                            }
                            if (ActiveDocument.SelectedChannelType == ImageChannelType.Blue)
                            {
                                // blue channel
                                if (_tempImageChannels[2] != null)
                                {
                                    if (PixelBlueX != PixelX || PixelY != PixelBlueY)
                                    {
                                        PixelBlueX = PixelX;
                                        PixelBlueY = PixelY;
                                        if (PixelX == 0 && PixelY == 0)
                                        {
                                            ImageChannels[2] = null;
                                            ActiveDocument.Image = _tempRGBImage;
                                        }
                                        else
                                        {
                                            ActiveDocument.UpdatePixelMoveDisplayImage(ref _tempImageChannels[2], PixelBlueX, PixelBlueX, PixelBlueY, PixelBlueY);
                                            ImageChannels[2] = _tempImageChannels[2];
                                            _tempBlueChannel = ImageProcessing.SetChannel(_tempImageChannels);
                                            ActiveDocument.Image = _tempBlueChannel;
                                        }
                                    }
                                    else if (PixelX == 0 && PixelY == 0)
                                    {
                                        PixelBlueX = PixelX;
                                        PixelBlueY = PixelY;
                                        ImageChannels[2] = null;
                                        ActiveDocument.Image = _tempRGBImage;
                                    }
                                    else
                                    {
                                        ActiveDocument.Image = _tempBlueChannel;
                                    }
                                    ActiveDocument.UpdateDisplayImage();
                                }
                            }
                            if (ActiveDocument.SelectedChannelType == ImageChannelType.Mix)
                            {
                                if (ImageChannels[0] != null)
                                {
                                    _tempImageChannels[0] = ImageChannels[0];
                                }
                                if (ImageChannels[1] != null)
                                {
                                    _tempImageChannels[1] = ImageChannels[1];
                                }
                                if (ImageChannels[2] != null)
                                {
                                    _tempImageChannels[2] = ImageChannels[2];
                                }
                                _tempMixChannel = ImageProcessing.SetChannel(_tempImageChannels);
                                ActiveDocument.Image = _tempMixChannel;
                                int nColorGradation = ActiveDocument.ImageInfo.NumOfChannels;
                                ActiveDocument.UpdateDisplayImage(nColorGradation, true);
                            }
                        }); 
                    };
                    worker.RunWorkerCompleted += (o, ea) =>
                    {
                        // Work has completed. You can now interact with the UI
                        StopWaitAnimation();
                        _IsLoading = false;

                    };
                    StartWaitAnimation("Loading...");
                    _IsLoading = true;
                    worker.RunWorkerAsync();

                }
            }
            catch (Exception ex)
            {
                throw new Exception(string.Format("Error Pic Move.\n{0}", ex.Message));
            }
            finally
            {
                IsProcessingContrast = false;
            }

        }
        protected bool CanExecuteRGBMoveCommand(object parameter)
        {
            return (IsProcessingContrast != true);
        }
        private void SetPixelDefaultValue(string handle) 
        {
            if (PixelRedX != 0 || PixelRedY != 0 || PixelBlueX != 0 || PixelBlueY != 0 || PixelGreenX != 0 || PixelGrayX != 0)
            {
                if (_ActiveDocument.FileName==null) {
                    return;
                }
                MessageBoxResult boxResult = MessageBoxResult.None;
                boxResult = MessageBox.Show(ImageTitle + ",The image has been modified but not saved. Save or not？", "warning", MessageBoxButton.YesNo);
                if (boxResult == MessageBoxResult.Yes)
                {
                    OnSave(null);
                   // ActiveDocument.UpdateDisplayImage();
                }
                else
                {
                    if (handle == "open"|| handle == "SelectImage")
                    {
                        ImageChannels = new WriteableBitmap[3] { null,null,null};
                        ActiveDocument.Image = _tempRGBImage;
                        ActiveDocument.UpdateDisplayImage();
                    }
                }
                PixelX = 0;
                PixelY = 0;
                PixelRedX = 0;
                PixelRedY = 0;
                PixelBlueX = 0;
                PixelBlueY = 0;
                PixelGreenX = 0;
                PixelGreenY = 0;
                PixelGrayX = 0;
                PixelGrayY = 0;
            }
           
        }
        #endregion

        #region RGBPixelMoveCommand
        private RelayCommand _RGBPixelMoveCommand = null;
        public ICommand RGBPixelMoveCommand
        {
            get
            {
                if (_RGBPixelMoveCommand == null)
                {
                    _RGBPixelMoveCommand = new RelayCommand(ExecuteRGBPixelSaveCommand, CanExecuteRGBPixelSaveCommand);
                }

                return _RGBPixelMoveCommand;
            }
        }
        protected void ExecuteRGBPixelSaveCommand(object parameter)
        {
            try
            {
                if (ActiveDocument != null)
                {
                    BackgroundWorker worker = new BackgroundWorker();
                    // Open the document in a different thread
                    worker.DoWork += (o, ea) =>
                    {
                        Application.Current.Dispatcher.Invoke((Action)delegate
                        {
                            int pixelOddx = SettingsManager.ConfigSettings.XOddNumberedLine;//X odd Line
                            int pixelEvenx = SettingsManager.ConfigSettings.XEvenNumberedLine;//X even Line
                            int pixelOddy = SettingsManager.ConfigSettings.YOddNumberedLine;//Y odd Line
                            int pixelEveny = SettingsManager.ConfigSettings.YEvenNumberedLine;//Y even Line
                            //FileViewModel.UpdatePixelSingleMoveDisplayImage(_tempSrcImg, pixelOddx, pixelEvenx, pixelOddy, pixelEveny);//Pixel Move
                           // ActiveDocument.UpdatePixelMoveDisplayImage(pixelOddx, pixelEvenx, pixelOddy, pixelEveny);
                        });
                    };
                    worker.RunWorkerCompleted += (o, ea) =>
                    {
                        // Work has completed. You can now interact with the UI
                        StopWaitAnimation();
                        _IsLoading = false;
                    };
                    StartWaitAnimation("Loading...");
                    _IsLoading = true;
                    worker.RunWorkerAsync();

                }
            }
            catch (Exception ex)
            {
                throw new Exception(string.Format("Pixel Move Error.\n{0}", ex.Message));
            }
            finally
            {
                IsProcessingContrast = false;
            }

        }

        protected bool CanExecuteRGBPixelSaveCommand(object parameter)
        {
            return (IsProcessingContrast != true);
        }

        #endregion

        #region DisplayRedChCommand

        private RelayCommand _DisplayRedChCommand = null;
        public ICommand DisplayRedChCommand
        {
            get
            {
                if (_DisplayRedChCommand == null)
                {
                    _DisplayRedChCommand = new RelayCommand(ExecuteDisplayRedChCommand, CanExecuteDisplayRedChCommand);
                }

                return _DisplayRedChCommand;
            }
        }

        protected void ExecuteDisplayRedChCommand(object parameter)
        {
            if (ActiveDocument != null && !IsProcessingContrast)
            {
                if (!IsActiveDocument || !ActiveDocument.IsImageChannelChanged ||
                    ActiveDocument.SelectedChannelType != ImageChannelType.Red)
                {
                    return;
                }

                try
                {
                    IsProcessingContrast = true;

                    // Reset the overall contrast buttons
                    ActiveDocument.ImageInfo.MixChannel.IsAutoChecked = false;
                    ActiveDocument.ImageInfo.MixChannel.IsInvertChecked = false;
                    ActiveDocument.ImageInfo.MixChannel.IsSaturationChecked = false;
                    //ActiveDocument.UpdateDisplayImage();
                    ExecuteRGBMoveCommand(null);
                    PixelX = PixelRedX;
                    PixelY = PixelRedY;
                }
                catch (Exception ex)
                {
                    throw new Exception(string.Format("Error creating the display image.\n{0}", ex.Message));
                }
                finally
                {
                    IsProcessingContrast = false;
                }
            }
        }

        protected bool CanExecuteDisplayRedChCommand(object parameter)
        {
            return (IsProcessingContrast != true);
        }

        #endregion

        #region DisplayGreenChCommand

        private RelayCommand _DisplayGreenChCommand = null;
        public ICommand DisplayGreenChCommand
        {
            get
            {
                if (_DisplayGreenChCommand == null)
                {
                    _DisplayGreenChCommand = new RelayCommand(ExecuteDisplayGreenChCommand, CanExecuteDisplayGreenChCommand);
                }

                return _DisplayGreenChCommand;
            }
        }

        protected void ExecuteDisplayGreenChCommand(object parameter)
        {
            if (ActiveDocument != null && !IsProcessingContrast)
            {
                if (!IsActiveDocument || !ActiveDocument.IsImageChannelChanged ||
                    ActiveDocument.SelectedChannelType != ImageChannelType.Green)
                {
                    return;
                }

                try
                {
                    IsProcessingContrast = true;

                    // Reset the overall contrast buttons
                    ActiveDocument.ImageInfo.MixChannel.IsAutoChecked = false;
                    ActiveDocument.ImageInfo.MixChannel.IsInvertChecked = false;
                    ActiveDocument.ImageInfo.MixChannel.IsSaturationChecked = false;
                    //ActiveDocument.UpdateDisplayImage();
                    ExecuteRGBMoveCommand(null);
                    PixelX = PixelGreenX;
                    PixelY = PixelGreenY;
                    //ExecuteRGBMoveCommand(1);
                }
                catch (Exception ex)
                {
                    throw new Exception(string.Format("Error creating the display image.\n{0}", ex.Message));
                }
                finally
                {
                    IsProcessingContrast = false;
                }
            }
        }

        protected bool CanExecuteDisplayGreenChCommand(object parameter)
        {
            return (IsProcessingContrast != true);
        }

        #endregion

        #region DisplayBlueChCommand

        private RelayCommand _DisplayBlueChCommand = null;
        public ICommand DisplayBlueChCommand
        {
            get
            {
                if (_DisplayBlueChCommand == null)
                {
                    _DisplayBlueChCommand = new RelayCommand(ExecuteDisplayBlueChCommand, CanExecuteDisplayBlueChCommand);
                }

                return _DisplayBlueChCommand;
            }
        }

        protected void ExecuteDisplayBlueChCommand(object parameter)
        {
            if (ActiveDocument != null && !IsProcessingContrast)
            {
                if (!IsActiveDocument || !ActiveDocument.IsImageChannelChanged ||
                    ActiveDocument.SelectedChannelType != ImageChannelType.Blue)
                {
                    return;
                }

                try
                {
                    IsProcessingContrast = true;

                    // Reset the overall contrast buttons
                    ActiveDocument.ImageInfo.MixChannel.IsAutoChecked = false;
                    ActiveDocument.ImageInfo.MixChannel.IsInvertChecked = false;
                    ActiveDocument.ImageInfo.MixChannel.IsSaturationChecked = false;
                    //ActiveDocument.UpdateDisplayImage();
                    ExecuteRGBMoveCommand(null);
                    PixelX = PixelBlueX;
                    PixelY = PixelBlueY;
                    //ExecuteRGBMoveCommand(1);
                }
                catch (Exception ex)
                {
                    throw new Exception(string.Format("Error creating the display image.\n{0}", ex.Message));
                }
                finally
                {
                    IsProcessingContrast = false;
                }
            }
        }

        protected bool CanExecuteDisplayBlueChCommand(object parameter)
        {
            return (IsProcessingContrast != true);
        }

        #endregion

        #region DisplayCompositeCommand

        private RelayCommand _DisplayCompositeCommand = null;
        public ICommand DisplayCompositeCommand
        {
            get
            {
                if (_DisplayCompositeCommand == null)
                {
                    _DisplayCompositeCommand = new RelayCommand(ExecuteDisplayCompositeCommand, CanExecuteDisplayCompositeCommand);
                }

                return _DisplayCompositeCommand;
            }
        }

        protected void ExecuteDisplayCompositeCommand(object parameter)
        {
            if (ActiveDocument != null && !IsProcessingContrast)
            {
                if (!IsActiveDocument || !ActiveDocument.IsImageChannelChanged ||
                    ActiveDocument.SelectedChannelType != ImageChannelType.Mix)
                {
                    return;
                }

                if (ActiveDocument.Image.Format.BitsPerPixel != 24 &&
                    ActiveDocument.Image.Format.BitsPerPixel != 48 &&
                    ActiveDocument.Image.Format.BitsPerPixel != 64)
                {
                    return;
                }

                try
                {
                    IsProcessingContrast = true;

                    /*if (ActiveDocument.ImageInfo.RedChannel.IsAutoChecked ||
                        ActiveDocument.ImageInfo.GreenChannel.IsAutoChecked ||
                        ActiveDocument.ImageInfo.BlueChannel.IsAutoChecked)
                    {
                        WriteableBitmap[] imageChannels = ImageProcessing.GetChannel(ActiveDocument.Image);
                        if (imageChannels != null)
                        {
                            // GetChannel returns RGB color order
                            //
                            int nBlackValue = 0;
                            int nWhiteValue = 0;
                            if (ActiveDocument.ImageInfo.RedChannel.IsAutoChecked)
                            {
                                ImageProcessing.GetAutoScaleValues(imageChannels[0], ref nWhiteValue, ref nBlackValue);
                                ActiveDocument.ImageInfo.RedChannel.BlackValue = nBlackValue;
                                ActiveDocument.ImageInfo.RedChannel.WhiteValue = nWhiteValue;
                                ActiveDocument.ImageInfo.RedChannel.GammaValue = 1.0;
                            }
                            if (ActiveDocument.ImageInfo.GreenChannel.IsAutoChecked)
                            {
                                ImageProcessing.GetAutoScaleValues(imageChannels[1], ref nWhiteValue, ref nBlackValue);
                                ActiveDocument.ImageInfo.GreenChannel.BlackValue = nBlackValue;
                                ActiveDocument.ImageInfo.GreenChannel.WhiteValue = nWhiteValue;
                                ActiveDocument.ImageInfo.GreenChannel.GammaValue = 1.0;
                            }
                            if (ActiveDocument.ImageInfo.BlueChannel.IsAutoChecked)
                            {
                                ImageProcessing.GetAutoScaleValues(imageChannels[2], ref nWhiteValue, ref nBlackValue);
                                ActiveDocument.ImageInfo.BlueChannel.BlackValue = nBlackValue;
                                ActiveDocument.ImageInfo.BlueChannel.WhiteValue = nWhiteValue;
                                ActiveDocument.ImageInfo.BlueChannel.GammaValue = 1.0;
                            }
                            imageChannels = null;
                        }
                    }*/

                    //int nColorGradation = ActiveDocument.ImageInfo.NumOfChannels;
                    //ActiveDocument.UpdateDisplayImage(nColorGradation, true);
                    ExecuteRGBMoveCommand(null);
                    PixelX = 0;
                    PixelY = 0;
                }
                catch (Exception ex)
                {
                    throw new Exception(string.Format("Error creating the display image.\n{0}", ex.Message));
                }
                finally
                {
                    IsProcessingContrast = false;

                    // Forces a garbage collection
                    //GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced);
                    //GC.WaitForPendingFinalizers();
                    //GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced);
                }
            }
        }

        protected bool CanExecuteDisplayCompositeCommand(object parameter)
        {
            return (IsProcessingContrast != true);
        }

        #endregion

        #region ExtractGrayscaleCommand

        private RelayCommand _ExtractGrayscaleCommand = null;
        public ICommand ExtractGrayscaleCommand
        {
            get
            {
                if (_ExtractGrayscaleCommand == null)
                {
                    _ExtractGrayscaleCommand = new RelayCommand(ExecuteExtractGrayscaleCommand, CanExecuteExtractGrayscaleCommand);
                }

                return _ExtractGrayscaleCommand;
            }
        }

        protected void ExecuteExtractGrayscaleCommand(object parameter)
        {
            if (!IsActiveDocument || !ActiveDocument.IsRgbImage) { return; }

            if (ActiveDocument.Image != null)
            {
                if (ActiveDocument.Image.Format.BitsPerPixel != 24 &&
                    ActiveDocument.Image.Format.BitsPerPixel != 48)
                {
                    return;
                }

                WriteableBitmap[] imageChannels = { null, null, null }; // color order BGR

                imageChannels = ImageProcessing.GetChannel(ActiveDocument.Image);

                if (imageChannels == null)
                {
                    return;
                }

                string currentTitle = ActiveDocument.Title;
                string newTitle = string.Empty;
                ImageInfo cpyInfo = (ImageInfo)ActiveDocument.ImageInfo.Clone();

                #region === Blue Channel ===
                // blue channel
                if (imageChannels[0] != null)
                {
                    ImageInfo imageInfoBCh = new ImageInfo();
                    imageInfoBCh.SoftwareVersion = cpyInfo.SoftwareVersion;
                    imageInfoBCh.DateTime = cpyInfo.DateTime;
                    imageInfoBCh.CaptureType = cpyInfo.CaptureType + "-B-Channel";
                    imageInfoBCh.RedChannel.Exposure = cpyInfo.BlueChannel.Exposure;
                    imageInfoBCh.RedChannel.LightSource = cpyInfo.BlueChannel.LightSource;
                    imageInfoBCh.RedChannel.FilterPosition = cpyInfo.BlueChannel.FilterPosition;
                    imageInfoBCh.Aperture = cpyInfo.Aperture;
                    imageInfoBCh.RedChannel.FocusPosition = cpyInfo.BlueChannel.FocusPosition;
                    imageInfoBCh.BinFactor = cpyInfo.BinFactor;
                    imageInfoBCh.Calibration = cpyInfo.Calibration;
                    imageInfoBCh.SelectedChannel = ImageChannelType.Mix;
                    imageInfoBCh.RedChannel.BlackValue = cpyInfo.BlueChannel.BlackValue;
                    imageInfoBCh.RedChannel.WhiteValue = cpyInfo.BlueChannel.WhiteValue;
                    imageInfoBCh.RedChannel.GammaValue = cpyInfo.BlueChannel.GammaValue;
                    imageInfoBCh.RedChannel.IsAutoChecked = cpyInfo.BlueChannel.IsAutoChecked;
                    imageInfoBCh.RedChannel.IsInvertChecked = cpyInfo.BlueChannel.IsInvertChecked;
                    imageInfoBCh.RedChannel.IsSaturationChecked = cpyInfo.BlueChannel.IsSaturationChecked;
                    newTitle = System.IO.Path.GetFileNameWithoutExtension(currentTitle) + "-BCh";

                    // Don't add new document if already exists.
                    if (!DocumentExists(newTitle))
                    {
                        NewDocument(imageChannels[0], imageInfoBCh, newTitle, false);
                    }
                    else
                    {
                        imageChannels[0] = null;
                    }
                }
                #endregion

                #region === Green channel ===
                // green channel
                if (imageChannels[1] != null)
                {
                    ImageInfo imageInfoGCh = new ImageInfo();
                    imageInfoGCh.SoftwareVersion = cpyInfo.SoftwareVersion;
                    imageInfoGCh.DateTime = cpyInfo.DateTime;
                    imageInfoGCh.CaptureType = cpyInfo.CaptureType + "-G-Channel";
                    imageInfoGCh.RedChannel.Exposure = cpyInfo.GreenChannel.Exposure;
                    imageInfoGCh.RedChannel.LightSource = cpyInfo.GreenChannel.LightSource;
                    imageInfoGCh.RedChannel.FilterPosition = cpyInfo.GreenChannel.FilterPosition;
                    imageInfoGCh.Aperture = cpyInfo.Aperture;
                    imageInfoGCh.RedChannel.FocusPosition = cpyInfo.GreenChannel.FocusPosition;
                    imageInfoGCh.BinFactor = cpyInfo.BinFactor;
                    imageInfoGCh.Calibration = cpyInfo.Calibration;
                    imageInfoGCh.SelectedChannel = ImageChannelType.Mix;
                    imageInfoGCh.RedChannel.BlackValue = cpyInfo.GreenChannel.BlackValue;
                    imageInfoGCh.RedChannel.WhiteValue = cpyInfo.GreenChannel.WhiteValue;
                    imageInfoGCh.RedChannel.GammaValue = cpyInfo.GreenChannel.GammaValue;
                    imageInfoGCh.RedChannel.IsAutoChecked = cpyInfo.GreenChannel.IsAutoChecked;
                    imageInfoGCh.RedChannel.IsInvertChecked = cpyInfo.GreenChannel.IsInvertChecked;
                    imageInfoGCh.RedChannel.IsSaturationChecked = cpyInfo.GreenChannel.IsSaturationChecked;
                    newTitle = System.IO.Path.GetFileNameWithoutExtension(currentTitle) + "-GCh";

                    // Don't add new document if already exists.
                    if (!DocumentExists(newTitle))
                    {
                        NewDocument(imageChannels[1], imageInfoGCh, newTitle, false);
                    }
                    else
                    {
                        imageChannels[1] = null;
                    }
                }
                #endregion

                #region === Red channel ===
                // red channel
                if (imageChannels[2] != null)
                {
                    ImageInfo imageInfoRCh = new ImageInfo();
                    imageInfoRCh.SoftwareVersion = cpyInfo.SoftwareVersion;
                    imageInfoRCh.DateTime = cpyInfo.DateTime;
                    imageInfoRCh.CaptureType = cpyInfo.CaptureType + "-R-Channel";
                    imageInfoRCh.RedChannel.Exposure = cpyInfo.RedChannel.Exposure;
                    imageInfoRCh.RedChannel.LightSource = cpyInfo.RedChannel.LightSource;
                    imageInfoRCh.RedChannel.FilterPosition = cpyInfo.RedChannel.FilterPosition;
                    imageInfoRCh.Aperture = cpyInfo.Aperture;
                    imageInfoRCh.RedChannel.FocusPosition = cpyInfo.RedChannel.FocusPosition;
                    imageInfoRCh.BinFactor = cpyInfo.BinFactor;
                    imageInfoRCh.Calibration = cpyInfo.Calibration;
                    imageInfoRCh.SelectedChannel = ImageChannelType.Mix;
                    imageInfoRCh.RedChannel.BlackValue = cpyInfo.RedChannel.BlackValue;
                    imageInfoRCh.RedChannel.WhiteValue = cpyInfo.RedChannel.WhiteValue;
                    imageInfoRCh.RedChannel.GammaValue = cpyInfo.RedChannel.GammaValue;
                    imageInfoRCh.RedChannel.IsAutoChecked = cpyInfo.RedChannel.IsAutoChecked;
                    imageInfoRCh.RedChannel.IsInvertChecked = cpyInfo.RedChannel.IsInvertChecked;
                    imageInfoRCh.RedChannel.IsSaturationChecked = cpyInfo.RedChannel.IsSaturationChecked;
                    newTitle = System.IO.Path.GetFileNameWithoutExtension(currentTitle) + "-RCh";

                    // Don't add new document if already exists.
                    if (!DocumentExists(newTitle))
                    {
                        Workspace.This.NewDocument(imageChannels[2], imageInfoRCh, newTitle, false);
                    }
                    else
                    {
                        imageChannels[2] = null;
                    }
                }
                #endregion
            }
        }

        protected bool CanExecuteExtractGrayscaleCommand(object parameter)
        {
            return true;
        }

        #endregion

        #region RotateLeft90Command

        private RelayCommand _RotateLeft90Command = null;
        public ICommand RotateLeft90Command
        {
            get
            {
                if (_RotateLeft90Command == null)
                {
                    _RotateLeft90Command = new RelayCommand(ExecuteRotateLeft90Command, CanExecuteRotateLeft90Command);
                }

                return _RotateLeft90Command;
            }
        }

        /// <summary>
        /// Rotate left 90 degree
        /// </summary>
        /// <param name="parameter"></param>
        protected void ExecuteRotateLeft90Command(object parameter)
        {
            if (!IsActiveDocument) { return; }

            double dAngle = 90.0;
            ActiveDocument.Image = ImageProcessing.WpfRotate(ActiveDocument.Image, dAngle);

            if (ActiveDocument.DocDirtyType != DirtyType.NewCreate)
            {
                ActiveDocument.DocDirtyType = DirtyType.Modified;
            }

            ActiveDocument.UpdateDisplayImage();
        }

        protected bool CanExecuteRotateLeft90Command(object parameter)
        {
            return true;
        }

        #endregion

        #region FlipHorizontalCommand

        private RelayCommand _FlipHorizontalCommand = null;
        public ICommand FlipHorizontalCommand
        {
            get
            {
                if (_FlipHorizontalCommand == null)
                {
                    _FlipHorizontalCommand = new RelayCommand(ExecuteFlipHorizontalCommand, CanExecuteFlipHorizontalCommand);
                }

                return _FlipHorizontalCommand;
            }
        }

        protected void ExecuteFlipHorizontalCommand(object parameter)
        {
            if (!IsActiveDocument) { return; }

            if (ActiveDocument.Image.Format.BitsPerPixel == 32)
            {
                string caption = "Image type not supported...";
                string message = "This operation is current not supported for 32-bit image.";
                System.Windows.MessageBox.Show(message, caption, MessageBoxButton.OK, MessageBoxImage.Stop);
                return;
            }

            ActiveDocument.Image = ImageProcessing.Flip(ActiveDocument.Image, IppiAxis.ippAxsHorizontal);

            if (ActiveDocument.DocDirtyType != DirtyType.NewCreate)
            {
                ActiveDocument.DocDirtyType = DirtyType.Modified;
            }

            ActiveDocument.UpdateDisplayImage();
        }

        protected bool CanExecuteFlipHorizontalCommand(object parameter)
        {
            return true;
        }

        #endregion

        #region FlipVerticalCommand

        private RelayCommand _FlipVerticalCommand = null;
        public ICommand FlipVerticalCommand
        {
            get
            {
                if (_FlipVerticalCommand == null)
                {
                    _FlipVerticalCommand = new RelayCommand(ExecuteFlipVerticalCommand, CanExecuteFlipVerticalCommand);
                }

                return _FlipVerticalCommand;
            }
        }

        protected void ExecuteFlipVerticalCommand(object parameter)
        {
            if (!IsActiveDocument) { return; }

            if (ActiveDocument.Image.Format.BitsPerPixel == 32)
            {
                string caption = "Image type not supported...";
                string message = "This operation is current not supported for 32-bit image.";
                System.Windows.MessageBox.Show(message, caption, MessageBoxButton.OK, MessageBoxImage.Stop);
                return;
            }

            ActiveDocument.Image = ImageProcessing.Flip(ActiveDocument.Image, IppiAxis.ippAxsVertical);

            if (ActiveDocument.DocDirtyType != DirtyType.NewCreate)
            {
                ActiveDocument.DocDirtyType = DirtyType.Modified;
            }

            ActiveDocument.UpdateDisplayImage();
        }

        protected bool CanExecuteFlipVerticalCommand(object parameter)
        {
            return true;
        }

        #endregion

        #region CropCommand
        private RelayCommand _CropCommand = null;
        /// <summary>
        /// Get the cropping command.
        /// </summary>
        public ICommand CropCommand
        {
            get
            {
                if (_CropCommand == null)
                {
                    _CropCommand = new RelayCommand(this.ExecuteCropCommand, CanExecuteCropCommand);
                }

                return _CropCommand;
            }
        }
        #region protected void ExecuteCropCommand(object parameter)
        /// <summary>
        ///Image crop command implementation.
        /// </summary>
        /// <param name="parameter"></param>
        protected void ExecuteCropCommand(object parameter)
        {
            if (ActiveDocument != null)
            {
                if (ActiveDocument.IsCropping)
                {
                    Rect cropRect = ActiveDocument.CropRect;

                    cropRect.X = Math.Round(cropRect.X);
                    cropRect.Y = Math.Round(cropRect.Y);
                    int width = (int)Math.Round(cropRect.Width);
                    int height = (int)Math.Round(cropRect.Height);
                    cropRect.Width = (width % 2 != 0) ? width + 1 : width;
                    cropRect.Height = (height % 2 != 0) ? height + 1 : height;

                    ActiveDocument.IsCropping = false;
                    WriteableBitmap cropImage = null;
                    ImageInfo newImageInfo = null;

                    if (cropRect.Width > 0 && cropRect.Height > 0)
                    {
                        if (ActiveDocument.ImageInfo != null)
                        {
                            newImageInfo = (ImageInfo)ActiveDocument.ImageInfo.Clone();
                            //newImageInfo = new ImageInfo(ActiveDocument.ImageInfo);
                        }

                        // Crop the selected region of the active document
                        cropImage = ImageProcessing.Crop(ActiveDocument.Image, cropRect);

                        if (cropImage != null)
                        {
                            string newTitle = Path.GetFileNameWithoutExtension(ActiveDocument.Title) + "_crop";

                            // check if document name already exists/opened
                            int docCounter = 0;
                            int result = 0;
                            foreach (var doc in Files)
                            {
                                string title = doc.Title;
                                if (title.Contains(newTitle))
                                {
                                    char lastChar = title[title.Length - 1];
                                    if (!Char.IsDigit(lastChar))
                                    {
                                        docCounter = 1;
                                    }
                                    else
                                    {
                                        result = (int)Char.GetNumericValue(lastChar);
                                        if (result > docCounter)
                                        {
                                            docCounter = result;
                                        }
                                    }
                                }
                            }
                            if (docCounter > 0)
                            {
                                newTitle = newTitle + "_" + (docCounter + 1);
                            }

                            ActiveDocument.IsRGBImageCropped = true;
                            NewDocument(cropImage, newImageInfo, newTitle, true, false);
                        }
                    }

                }
            }
        }
        #endregion

        #region protected bool CanExecuteCropCommand(object parameter)
        /// <summary>
        /// 
        /// </summary>
        /// <param name="parameter"></param>
        /// <returns></returns>
        protected bool CanExecuteCropCommand(object parameter)
        {
            return (ActiveDocument != null && ActiveDocument.IsCropping);
        }
        #endregion

        #endregion

        #region ShowCropAdornerCommand
        /// <summary>
        /// Get the show crop adorner command.
        /// </summary>
        public ICommand ShowCropAdornerCommand
        {
            get
            {
                if (_ShowCropAdornerCommand == null)
                {
                    _ShowCropAdornerCommand = new RelayCommand(this.ExecuteShowCropAdornerCommand, null);
                }

                return _ShowCropAdornerCommand;
            }
        }
        #region protected void ExecuteShowCropAdornerCommand(object parameter)
        /// <summary>
        ///Show crop adorner command implementation.
        /// </summary>
        /// <param name="parameter"></param>
        protected void ExecuteShowCropAdornerCommand(object parameter)
        {
            if (ActiveDocument != null)
            {
                if (!ActiveDocument.IsCropping)
                {
                    ActiveDocument.IsCropping = true;
                }
                else
                {
                    ActiveDocument.IsCropping = false;
                }
            }
        }
        #endregion

        //#region protected bool CanExecuteZoomInCommand(object parameter)
        ///// <summary>
        ///// 
        ///// </summary>
        ///// <param name="parameter"></param>
        ///// <returns></returns>
        //protected bool CanExecuteZoomInCommand(object parameter)
        //{
        //    return (Workspace.This.ActiveDocument != null);
        //}
        //#endregion

        #endregion

        #region ZoomInCommand
        /// <summary>
        /// Get the ZoomIn command.
        /// </summary>
        public ICommand ZoomInCommand
        {
            get
            {
                if (_ZoomInCommand == null)
                {
                    _ZoomInCommand = new RelayCommand(this.ExecuteZoomInCommand, null);
                }

                return _ZoomInCommand;
            }
        }
        #region protected void ExecuteZoomInCommand(object parameter)
        /// <summary>
        ///Image zoom-in command implementation.
        /// </summary>
        /// <param name="parameter"></param>
        protected void ExecuteZoomInCommand(object parameter)
        {
            if (ActiveDocument != null)
            {
                //double currentZoom = ActiveDocument.ZoomLevel;
                //currentZoom += _DefaultZoomFactor;
                //if (currentZoom >= 0.95 && currentZoom <= 1.05)
                //{
                //    currentZoom = 1.0;
                //}
                //ActiveDocument.ZoomLevel = currentZoom;
                //ActiveDocument.ImageViewer.ZoomIn();

                ActiveDocument.ZoomIn();
            }
        }
        #endregion

        //#region protected bool CanExecuteZoomInCommand(object parameter)
        ///// <summary>
        ///// 
        ///// </summary>
        ///// <param name="parameter"></param>
        ///// <returns></returns>
        //protected bool CanExecuteZoomInCommand(object parameter)
        //{
        //    return (Workspace.This.ActiveDocument != null);
        //}
        //#endregion

        #endregion

        #region ZoomOutCommand
        /// <summary>
        /// Get the ZoomOut command.
        /// </summary>
        public ICommand ZoomOutCommand
        {
            get
            {
                if (_ZoomOutCommand == null)
                {
                    _ZoomOutCommand = new RelayCommand(this.ExecuteZoomOutCommand, null);
                }

                return _ZoomOutCommand;
            }
        }

        #region protected void ExecuteZoomOutCommand(object parameter)
        /// <summary>
        ///Image zoom-in command implementation.
        /// </summary>
        /// <param name="parameter"></param>
        protected void ExecuteZoomOutCommand(object parameter)
        {
            if (ActiveDocument != null)
            {
                //if (Math.Round(ActiveDocument.ZoomLevel, 10) > Math.Round(ActiveDocument.MinimumZoom, 10))
                //{
                //    double currentZoom = ActiveDocument.ZoomLevel;
                //    currentZoom -= _DefaultZoomFactor;
                //    if (currentZoom >= 0.95 && currentZoom <= 1.05)
                //    {
                //        currentZoom = 1.0;
                //    }
                //    if (currentZoom < ActiveDocument.MinimumZoom)
                //    {
                //        currentZoom = ActiveDocument.MinimumZoom;
                //    }
                //    ActiveDocument.ZoomLevel = currentZoom;
                //}

                ActiveDocument.ZoomOut();
            }
        }
        #endregion

        //#region protected bool CanExecuteZoomOutCommand(object parameter)
        ///// <summary>
        ///// 
        ///// </summary>
        ///// <param name="parameter"></param>
        ///// <returns></returns>
        //protected bool CanExecuteZoomOutCommand(object parameter)
        //{
        //    return (Workspace.This.ActiveDocument != null);
        //}
        //#endregion

        #endregion

        #region ZoomFitCommand
        /// <summary>
        /// Get the ZoomFit command.
        /// </summary>
        public ICommand ZoomFitCommand
        {
            get
            {
                if (_ZoomFitCommand == null)
                {
                    _ZoomFitCommand = new RelayCommand(this.ExecuteZoomFitCommand, this.CanExecuteZoomFitCommand);
                }

                return _ZoomFitCommand;
            }
        }
        #region protected void ExecuteZoomFitCommand(object parameter)
        /// <summary>
        ///Image zoom-in command implementation.
        /// </summary>
        /// <param name="parameter"></param>
        protected void ExecuteZoomFitCommand(object parameter)
        {
            if (ActiveDocument != null)
            {
                ActiveDocument.ZoomLevel = ActiveDocument.MinimumZoom;
            }
        }
        #endregion

        #region protected bool CanExecuteZoomFitCommand(object parameter)
        /// <summary>
        /// 
        /// </summary>
        /// <param name="parameter"></param>
        /// <returns></returns>
        protected bool CanExecuteZoomFitCommand(object parameter)
        {
            return (Workspace.This.ActiveDocument != null);
        }
        #endregion

        #endregion


        internal bool Close(FileViewModel fileToClose)
        {
            if (fileToClose.IsDirty)
            {
                //var productName = _Owner.ProductName;
                var productName = string.Empty;
                var res = MessageBox.Show(string.Format("Do you want to save changes to '{0}'?", fileToClose.Title), productName, MessageBoxButton.YesNoCancel, MessageBoxImage.Warning);
                if (res == MessageBoxResult.Cancel)
                    return false;
                if (res == MessageBoxResult.Yes)
                {
                    try
                    {
                        //SaveAsync(fileToClose, closeAfterSaved: true);
                        SaveSync(fileToClose, closeAfterSaved: true, showOsKb : true);
                    }
                    catch
                    {
                        // Rethrow to preserve stack details
                        // Satisfies the rule. 
                        throw;
                    }
                }
                else
                {
                    Remove(fileToClose);
                    GC.Collect();
                }
            }
            else
            {
                Remove(fileToClose);
                GC.Collect();
            }
            return true;
        }

        internal void SaveAsync(FileViewModel fileToSave, bool saveAsFlag = false, bool closeAfterSaved = false, bool showOsKb = true)
        {
            bool bIsCompressed = false;

            if (fileToSave.FilePath == null || saveAsFlag)
            {
                var dlg = new SaveFileDialog();
                dlg.Filter = "TIF Files(.TIFF)|*.tif|TIF Files(Compressed)|*.tif|JPG Files(.JPG)|*.jpg|BMP Files(.BMP)|*.bmp";
                dlg.Title = "Save an Image File";
                if (fileToSave.FilePath == null)
                {
                    dlg.FileName = GenerateFileName(fileToSave.Title, ".tif");
                }
                else
                {
                    dlg.FileName = fileToSave.Title;
                }


                if (showOsKb)
                {
                    ShowOnscreenKeyboard();
                }

                if (dlg.ShowDialog().GetValueOrDefault())
                {
                    fileToSave.FilePath = dlg.FileName;
                    bIsCompressed = (dlg.FilterIndex == 2);
                    
                    if (showOsKb)
                    {
                        HideOnscreenKeyboard();
                    }
                }
                else
                {
                    if (showOsKb)
                    {
                        HideOnscreenKeyboard();
                    }

                    return;
                }
            }

            BackgroundWorker saveFileWorker = new BackgroundWorker();

            // Save the document in a different thread
            saveFileWorker.DoWork += (o, ea) =>
            {
                SaveFile(fileToSave, bIsCompressed);
            };
            saveFileWorker.RunWorkerCompleted += (o, ea) =>
            {
                // Work has completed. You can now interact with the UI
                StopWaitAnimation();

                if (ea.Cancelled)
                {
                    // operation cancelled
                }
                else if (ea.Error != null)
                {
                    // error occurred
                    throw ea.Error;
                }
                else
                {
                    Application.Current.Dispatcher.Invoke((Action)delegate
                    {
                        fileToSave.IsDirty = false;
                        fileToSave.Title = fileToSave.FileName;

                        if (closeAfterSaved)
                        {
                            Remove(fileToSave);
                        }
                    });
                }
            };

            saveFileWorker.RunWorkerAsync();
            StartWaitAnimation("Saving...");
        }

        internal void SaveSync(FileViewModel fileToSave, bool saveAsFlag = false, bool closeAfterSaved = false, bool showOsKb = false)
        {
            bool bIsCompressed = false;
            if (fileToSave.FilePath == null || saveAsFlag)
            {
                var dlg = new SaveFileDialog();
                dlg.Filter = "TIF Files(.TIFF)|*.tif|TIF Files(Compressed)|*.tif|JPG Files(.JPG)|*.jpg|BMP Files(.BMP)|*.bmp";
                dlg.Title = "Save an Image File";
                if (fileToSave.FilePath == null)
                {
                    dlg.FileName = GenerateFileName(fileToSave.Title, ".tif");
                    showOsKb = true;    // unsaved file, show on-screen keyboard
                }
                else
                {
                    dlg.FileName = fileToSave.Title;
                }

                if (showOsKb)
                {
                    ShowOnscreenKeyboard();
                }

                if (dlg.ShowDialog().GetValueOrDefault())
                {
                    string Path = dlg.FileName;
                    string volume = Path.Substring(0, Path.IndexOf(':'));
                    double freespace = GetHardDiskSpace(volume);
                    double imagemaxmin = GetImageSpace(fileToSave.Image);
                    if (freespace < imagemaxmin)
                    {
                        MessageBox.Show("The hard disk is short of space and needs extra space  " + (imagemaxmin - freespace + 2) + " MB");
                        if (showOsKb)
                        {
                            HideOnscreenKeyboard();
                        }
                        return;
                    }
                    fileToSave.FilePath = dlg.FileName;
                    bIsCompressed = (dlg.FilterIndex == 2);

                    if (showOsKb)
                    {
                        HideOnscreenKeyboard();
                    }
                }
                else
                {
                    if (showOsKb)
                    {
                        HideOnscreenKeyboard();
                    }

                    return;
                }
            }

            try
            {
                StartWaitAnimation("Saving...");

                SaveFile(fileToSave, bIsCompressed);

                fileToSave.IsDirty = false;
                fileToSave.Title = fileToSave.FileName;

                if (closeAfterSaved)
                {
                    Remove(fileToSave);
                }
            }
            catch
            {
                // Rethrow to preserve stack details
                // Satisfies the rule. 
                throw;
            }
            finally
            {
                StopWaitAnimation();
            }

            /***
            BackgroundWorker saveFileWorker = new BackgroundWorker();
            AutoResetEvent resetEvent = new AutoResetEvent(false);

            // Save the document in a different thread
            saveFileWorker.DoWork += (o, ea) =>
            {
                resetEvent.WaitOne();

                SaveFile(fileToSave, bIsCompressed);
            };
            saveFileWorker.RunWorkerCompleted += (o, ea) =>
            {
                // Work has completed. You can now interact with the UI
                //_IsSaving = false;
                StopWaitAnimation();

                if (ea.Cancelled)
                {
                    // operation cancelled
                }
                else if (ea.Error != null)
                {
                    // error occurred
                    throw ea.Error;
                }
                else
                {
                    Application.Current.Dispatcher.Invoke((Action)delegate
                    {
                        fileToSave.IsDirty = false;
                        fileToSave.Title = fileToSave.FileName;

                        if (closeAfterSaved)
                        {
                            Remove(fileToSave);
                        }
                    });
                }

                resetEvent.Set(); // signal that worker is done

            };

            StartWaitAnimation("Saving....");

            saveFileWorker.RunWorkerAsync();
            ***/
        }

        internal void SaveFile(FileViewModel fileToSave, bool bIsCompressed)
        {
            if (string.IsNullOrEmpty(fileToSave.FilePath)) { return; }
            string fileExtension = Path.GetExtension(fileToSave.FilePath);
            string pathName = Path.GetDirectoryName(fileToSave.FilePath);
            int iFileType = ImageProcessing.TIFF_FILE;

            if (fileExtension.ToLower() == ".tif" ||
                fileExtension.ToLower() == ".tiff")
            {
                iFileType = ImageProcessing.TIFF_FILE;
            }
            else if (fileExtension.ToLower() == ".jpg" ||
                     fileExtension.ToLower() == ".jpeg")
            {
                iFileType = ImageProcessing.JPG_FILE;
            }
            else if (fileExtension.ToLower() == ".bmp")
            {
                iFileType = ImageProcessing.BMP_FILE;
            }
            else
            {
                throw new NotSupportedException("File type not supported.");
            }

            try
            {
                ImageInfo imageInfo = fileToSave.ImageInfo;
                if (imageInfo == null)
                {
                    imageInfo = new ImageInfo();
                }

                WriteableBitmap imageToBeSaved = null;
                if (iFileType == ImageProcessing.BMP_FILE || iFileType == ImageProcessing.JPG_FILE)
                {
                    imageToBeSaved = fileToSave.DisplayImage;   // Save the display image
                }
                else
                {
                    if (fileToSave.Image.Format == PixelFormats.Gray16)
                    {
                        imageToBeSaved = fileToSave.Image;
                    }
                    else
                    {
                        WriteableBitmap[] imageChannels = { null, null, null }; // color order BGR
                        imageChannels = ImageProcessing.GetChannel(_tempRGBImage);
                        if (imageChannels == null)
                        {
                            return;
                        }
                        // red channel
                        if (PixelRedX != 0 || PixelRedY != 0)
                        {
                            ActiveDocument.UpdatePixelMoveDisplayImage(ref imageChannels[0], PixelRedX, PixelRedX, PixelRedY, PixelRedY);
                        }
                        // blue channel
                        if (PixelBlueX != 0 || PixelBlueY != 0)
                        {
                            ActiveDocument.UpdatePixelMoveDisplayImage(ref imageChannels[2], PixelBlueX, PixelBlueX, PixelBlueY, PixelBlueY);
                        }
                        // green channel
                        if (PixelGreenX != 0 || PixelGreenY != 0)
                        {
                            ActiveDocument.UpdatePixelMoveDisplayImage(ref imageChannels[1], PixelGreenX, PixelGreenX, PixelGreenY, PixelGreenY);
                        }
                        if (PixelRedX != 0 || PixelRedY != 0 || PixelBlueX != 0 || PixelBlueY != 0 || PixelGreenX != 0 || PixelGreenY != 0)
                        {
                            ActiveDocument.SelectedChannelType = ImageChannelType.Mix;
                            imageToBeSaved = ImageProcessing.SetChannel(imageChannels);
                            _tempRGBImage = imageToBeSaved;
                            ActiveDocument.Image = imageToBeSaved;
                            int nColorGradation = ActiveDocument.ImageInfo.NumOfChannels;
                            ActiveDocument.UpdateDisplayImage(nColorGradation, true);
                        }
                        else
                        {
                            imageToBeSaved = _tempRGBImage;
                        }
                    }
                }
                    ImageProcessing.Save(fileToSave.FilePath, imageToBeSaved, imageInfo, iFileType, bIsCompressed);
                // Save the image file


                // Save the annotation
                //if (fileToSave.DrawingCanvas.IsDirty)
                //{
                //    try
                //    {
                //        var filePath = Path.ChangeExtension(fileToSave.FilePath, ".aan");
                //        fileToSave.DrawingCanvas.Save(filePath);
                //        fileToSave.IsDirty = false;
                //    }
                //    catch (Exception ex)
                //    {
                //        string caption = "Error loading annotations";
                //        string message = string.Format("Error loading annotations:\n{0}", ex.Message);
                //        MessageBox.Show(message, caption, MessageBoxButton.OK, MessageBoxImage.Warning);
                //    }
                //}

                // Remember initial directory
                SettingsManager.ApplicationSettings.InitialDirectory = System.IO.Path.GetDirectoryName(fileToSave.FilePath);
                PixelX = 0;
                PixelY = 0;
                PixelRedX = 0;
                PixelRedY = 0;
                PixelBlueX = 0;
                PixelBlueY = 0;
                PixelGreenX = 0;
                PixelGreenY = 0;
                PixelGrayX = 0;
                PixelGrayY = 0;
            }
            catch
            {
                throw;
            }
        }

        internal bool DocumentExists(string strTitle)
        {
            bool bResult = false;

            if (Files.Count > 0)
            {
                foreach (var file in Files)
                {
                    //file.Title.Trim( new Char[] {'*'} );
                    if (file.Title.Equals(strTitle))
                    {
                        bResult = true;
                        break;
                    }
                }
            }

            return bResult;
        }
        /// <summary>
        /// internal method
        /// </summary>
        /// <returns></returns>
        internal bool ConnectEthernetSlave()
        {
            return _EthernetController.Connect(new System.Net.IPAddress(new byte[] { 192, 168, 1, 110 }), 5000, 8000, new System.Net.IPAddress(new byte[] { 192, 168, 1, 100 }));
        }
        internal bool ReConnectEthernetSlave()
        {
            return _EthernetController.ReConnect(new System.Net.IPAddress(new byte[] { 192, 168, 1, 110 }), 5000, 8000, new System.Net.IPAddress(new byte[] { 192, 168, 1, 100 }));
        }

        internal EthernetController EthernetController
        {
            get { return _EthernetController; }
        }

        /// <summary>
        /// Generate default file name using timestamp (default file type is: .TIFF)
        /// </summary>
        /// <param name="headerTitle"></param>
        /// <param name="fileType"></param>
        /// <returns></returns>
        internal string GenerateFileName(string headerTitle, string fileType = ".tif")
        {
            string strFileName = string.Empty;
            string strFileFullPath = string.Empty;
            //int[] intArray = null;
            //bool bIsFramePartOfSet = false;

            //
            // Get set and frame number
            //
            //保存时在后面添加保存时间
            //DateTime dt = DateTime.Now;
            //string pattern = @"S\d";
            //if (Regex.IsMatch(headerTitle, pattern))
            //{
            //    //bIsFramePartOfSet = true;
            //    strFileName = headerTitle + "_" + string.Format("{0}-{1:D2}{2:D2}-{3:D2}{4:D2}{5:D2}", dt.Year, dt.Month, dt.Day, dt.Hour, dt.Minute, dt.Second);
            //}
            //else
            //{
            //    strFileName = headerTitle + "_"+string.Format("{0}-{1:D2}{2:D2}-{3:D2}{4:D2}{5:D2}", dt.Year, dt.Month, dt.Day, dt.Hour, dt.Minute, dt.Second);
            //}
            strFileName = headerTitle;
            /*
            if (headerTitle.Contains("Frame"))
            {
                bIsFramePartOfSet = true;

                // Split on one or more non-digit characters.
                string[] numbers = Regex.Split(headerTitle, @"\D+");
                intArray = new int[numbers.Count()];
                int setIndex = 0;
                for (int index = 0; index < numbers.Count(); index++)
                {
                    if (!string.IsNullOrEmpty(numbers[index]))
                    {
                        int number = int.Parse(numbers[index]);
                        if (number > 0)
                        {
                            intArray[setIndex++] = number;
                        }
                    }
                }
            }

            //
            // Generate default image name using timestamp
            //
            DateTime dt = DateTime.Now;

            if (bIsFramePartOfSet)
            {
                strFileName = string.Format("{0:D2}{1:D2}-{2:D2}{3:D2}{4:D2}", dt.Month, dt.Day, dt.Hour, dt.Minute, dt.Second);
                strFileName = string.Format("S{0}F{1}-{2}", intArray[0], intArray[1], strFileName);
            }
            else
            {
                strFileName = string.Format("{0}-{1:D2}{2:D2}-{3:D2}{4:D2}{5:D2}", dt.Year, dt.Month, dt.Day, dt.Hour, dt.Minute, dt.Second);
            }
            */

            switch (fileType.ToLower())
            {
                case ".tif":
                    strFileName = strFileName + ".tif";
                    break;
                case ".jpg":
                    strFileName = strFileName + ".jpg";
                    break;
                case ".bmp":
                    strFileName = strFileName + ".bmp";
                    break;
            }

            return strFileName;
        }

        public WriteableBitmap LoadImage(string filePath)
        {
            WriteableBitmap wbBitmap = null;
            ImageInfo imageInfo = null;

            try
            {
                wbBitmap = ImageProcessing.LoadImageFile(filePath);

                if (wbBitmap != null)
                {
                    try
                    {
                        // Get image info from the comments section of the image metadata
                        imageInfo = ImageProcessing.ReadMetadata(filePath);
                    }
                    catch
                    {
                    }

                    if (imageInfo == null)
                    {
                        imageInfo = new ImageInfo();
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return wbBitmap;
        }

        #region ShowOSKBCommand
        /// <summary>
        /// Get the ZoomIn command.
        /// </summary>
        public ICommand ShowOSKBCommand
        {
            get
            {
                if (_ShowOSKBCommand == null)
                {
                    _ShowOSKBCommand = new RelayCommand(this.ExecuteShowOSKBCommand, this.CanExecuteShowOSKBCommand);
                }

                return _ShowOSKBCommand;
            }
        }
        #region protected void ExecuteShowOSKBCommand(object parameter)
        /// <summary>
        ///Image resize command implementation.
        /// </summary>
        /// <param name="parameter"></param>
        protected void ExecuteShowOSKBCommand(object parameter)
        {
            if (ActiveDocument != null)
            {
                ShowOnscreenKeyboard();
            }
        }
        #endregion

        #region protected bool CanExecuteShowOSKBCommand(object parameter)
        /// <summary>
        /// 
        /// </summary>
        /// <param name="parameter"></param>
        /// <returns></returns>
        protected bool CanExecuteShowOSKBCommand(object parameter)
        {
            return (ActiveDocument != null);
        }
        #endregion

        #endregion


        public bool IsScannerMode
        {
            get
            {
                return _IsScannerMode;
            }
            set
            {
                if (_IsScannerMode != value)
                {
                    _IsScannerMode = value;
                    if (_IsScannerMode)
                    {
                        IsImagingMode = true;
                    }
                    RaisePropertyChanged("IsScannerMode");
                }
            }
        }

        public bool IsImagingMode
        {
            get
            {
                return _IsImagingMode;
            }
            set
            {
                if (_IsImagingMode != value)
                {
                    _IsImagingMode = value;
                    if (_IsImagingMode)
                    {
                        SelectedTabIndex = (int)ApplicationTabType.Imaging;
                    }
                    RaisePropertyChanged("IsImagingMode");
                }
            }
        }
 
      
        public bool IsPreparing
        {
            get { return _IsPreparing; }
            set
            {
                if (_IsPreparing != value)
                {
                    _IsPreparing = value;
                    RaisePropertyChanged("IsPreparing");
                }
            }
        }
        //public bool IsReadyScanning
        //{
        //    get { return _IsReadyScanning; }
        //    set
        //    {
        //        if (_IsReadyScanning != value)
        //        {
        //            _IsReadyScanning = value;
        //            RaisePropertyChanged("IsReadyScanning");
        //        }
        //    }
        //}

        public bool IsScanning
        {
            get { return _IsScanning; }
            set
            {
                if (_IsScanning != value)
                {
                    _IsScanning = value;
                    RaisePropertyChanged("IsScanning");
                }
            }
        }

        public bool IsCapturing
        {
            get { return _IsCapturing; }
            set
            {
                if (_IsCapturing != value)
                {
                    _IsCapturing = value;
                    RaisePropertyChanged("IsCapturing");
                }
            }
        }

        public bool IsContinuous
        {
            get { return _IsContinuous; }
            set
            {
                if (_IsContinuous != value)
                {
                    _IsContinuous = value;
                    RaisePropertyChanged("IsContinuous");
                }
            }
        }

        #region DisplayImage (Imaging Tab)
        private BitmapSource _DisplayImage = null;
        public BitmapSource DisplayImage
        {
            get
            {
                return _DisplayImage;
            }
            set
            {
                _DisplayImage = value;
                RaisePropertyChanged("DisplayImage");
            }
        }
        #endregion

        public string DoorStatus
        {
            get { return _DoorStatus; }
            set
            {
                if (_DoorStatus != value)
                {
                    _DoorStatus = value;
                    RaisePropertyChanged("DoorStatus");
                }
            }
        }

        private MultiplexViewModel _MultiplexVm = null;
        public MultiplexViewModel MultiplexVm
        {
            get { return _MultiplexVm; }
            set
            {
                _MultiplexVm = value;
                RaisePropertyChanged("MultiplexVm");
            }
        }

        private bool _IsMultiplexChecked = false;
        public bool IsMultiplexChecked
        {
            get { return _IsMultiplexChecked; }
            set
            {
                if (value == true)
                    if (this.Files == null || this.Files.Count == 0)
                        return;

                _IsMultiplexChecked = value;
                RaisePropertyChanged("IsMultiplexChecked");
                if (_IsMultiplexChecked)
                {
                    _MultiplexVm.Files = this.Files;
                    ImageMergeWindow mergeWind = new ImageMergeWindow();
                    mergeWind.DataContext = MultiplexVm;
                    mergeWind.ShowDialog();
                    // Set default selection.
                    /*for (int index = 0; index < this.Files.Count; index++)
                    {
                        if (index == 0)
                            _MultiplexVm.SelectedImageC1 = this.Files[index];
                        if (index == 1)
                            _MultiplexVm.SelectedImageC2 = this.Files[index];
                        if (index == 2)
                        {
                            _MultiplexVm.SelectedImageC3 = this.Files[index];
                            break;
                        }
                    }*/
                }
                else
                {
                    _MultiplexVm.ResetSelection();
                }
            }
        }

        #region Scale
       
        public static event EventHandler ArrowClearEvent;
        private bool _IsArrowClear = false;
        /// <summary>
        /// 清除刻度
        /// Know all the scales
        /// </summary>
        public bool IsArrowClear
        {
            get { return _IsArrowClear; }
            set
            {
                _IsArrowClear = value;
                RaisePropertyChanged("IsArrowClear");
                if (_IsArrowClear)
                {
                    ArrowClearEvent(null, null);
                    _IsArrowClear = false;
                }
               
            }
        }

        public static event EventHandler ArrowBackWardEvent;
        private bool _IsBackWard = false;
        /// <summary>
        /// Next scale
        /// </summary>
        public bool IsBackWard
        {
            get { return _IsBackWard; }
            set
            {
                _IsBackWard = value;
                RaisePropertyChanged("IsBackWard");
                if (_IsBackWard)
                {
                    ArrowBackWardEvent(null, null);
                    _IsBackWard = false;
                }

            }
        }

        public static event EventHandler ArrowForWardEvent;
        private bool _IsForWard = false;
        /// <summary>
        /// 上一个刻度
        /// </summary>
        public bool IsForWard
        {
            get { return _IsForWard; }
            set
            {
                _IsForWard = value;
                RaisePropertyChanged("IsForWard");
                if (_IsForWard)
                {
                    ArrowForWardEvent(null, null);
                    _IsForWard = false;
                }

            }
        }
        #endregion

        private bool _IsImageRotating = false;
        /// <summary>
        /// 
        /// </summary>
        public bool IsImageRotating
        {
            get { return _IsImageRotating; }
            set
            {
                _IsImageRotating = value;
                RaisePropertyChanged("IsImageRotating");
                if (_IsImageRotating)
                {
                    //rotafree
                    Rotating();
                    _IsImageRotating = false;
                }

            }
        }

        private bool _IsImageGif = false;
        public bool IsImageGif
        {
            get { return _IsImageGif; }
            set
            {
                _IsImageGif = value;
                RaisePropertyChanged("IsImageGif");
                if (_IsImageGif)
                {
                    //gif meth
                    GifWindown();
                    _IsImageGif = false;
                }

            }
        }

        public MotorViewModel MotorVM
        {
            get { return _MotorViewModel; }
        }

        public ScannerViewModel ScannerVM
        {
            get { return _ScannerViewModel; }
        }

        public ApdViewModel ApdVM
        {
            get { return _ApdViewModel; }
        }
        public IvViewModel IVVM
        {
            get { return _IVSensorViewModel; }
        }
        public ParameterSetupViewModel ParameterVM
        {
            get { return _ParamViewModel; }
        }
        public NewParameterSetupViewModel NewParameterVM
        {
            get { return _NewParamViewModel; }
        }
        public TransportLockViewModel TransportLockVM
        {
            get { return _TransportLockViewModel; }
        }
        public ZAutomaticallyFocalViewModel ZAutomaticallyFocalVM
        {
            get { return _ZAutomaticallyFocal; }
        }
        public ImageRotatingPrcessViewModel ImageRotatingPrcessVM
        {
            get { return _ImageRotatingPrcess; }
        }
        public AgingViewModel AgingVM
        {
            get { return _AgingVM; }
        }
        public string CapturingTopStatusText
        {
            get { return _CapturingTopStatusText; }
            set
            {
                if (_CapturingTopStatusText != value)
                {
                    _CapturingTopStatusText = value;
                    RaisePropertyChanged("CapturingTopStatusText");
                }
            }
        }



        #region public DateTime CaptureStartTime
        /// <summary>
        /// Get/set the image capture start time.
        /// </summary>
        public DateTime CaptureStartTime
        {
            get { return _CaptureStartTime; }
            set
            {
                if (_CaptureStartTime != value)
                {
                    _CaptureStartTime = value;
                    RaisePropertyChanged("CaptureStartTime");
                }
            }
        }
        #endregion

        #region public double EstimatedCaptureTime
        /// <summary>
        /// Get/set estimate capture time.
        /// </summary>
        public double EstimatedCaptureTime
        {
            get { return _EstimatedCaptureTime; }
            set
            {
                if (_EstimatedCaptureTime != value)
                {
                    _EstimatedCaptureTime = value;
                    RaisePropertyChanged("EstimatedCaptureTime");
                }
            }
        }
        #endregion

        #region public string EstimatedTimeRemaining
        /// <summary>
        /// Get/set the Darkroom estimated capture time remaining status display string.
        /// </summary>
        public string EstimatedTimeRemaining
        {
            get { return _EstimatedTimeRemaining; }
            set
            {
                _EstimatedTimeRemaining = value;
                RaisePropertyChanged("EstimatedTimeRemaining");
            }
        }
        #endregion

        #region public double PercentComplete
        /// <summary>
        /// get/set the percent capture completed.
        /// </summary>
        public double PercentComplete
        {
            get { return _PercentComplete; }
            set
            {
                if (_PercentComplete != value)
                {
                    _PercentComplete = value;
                    RaisePropertyChanged("PercentComplete");
                }
            }
        }
        #endregion

        public DispatcherTimer CaptureCountdownTimer
        {
            get { return _DispatcherTimer; }
        }

        public string CurrentScanWorkIndexTitle
        {
            get { return _CurrentScanWorkIndex; }
            set
            {
                if (_CurrentScanWorkIndex != value)
                {
                    _CurrentScanWorkIndex = value;
                    RaisePropertyChanged("CurrentScanWorkIndexTitle");
                }
            }
        }

        public Visibility WorkIndexTitleVisBility
        {
            get
            {
                return _WorkIndexTitleVisBility;

            }
            set
            {
                _WorkIndexTitleVisBility = value;
                RaisePropertyChanged("WorkIndexTitleVisBility");
            }


        }
        #region private void dispatcherTimer_Tick(object sender, EventArgs e)
        /// <summary>
        /// Upate progress bar text
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void _DispatcherTimer_Tick(object sender, EventArgs e)
        {
            double estTimeRemain = 0;
            double percentCompete = 0;
            double estimatedCaptureTimeInSec = 0;

            EstimatedTimeRemaining = string.Empty;

            if (IsCapturing || IsScanning)
            {
                //
                // We are capturing an image and NOT in "live" mode:
                //
                TimeSpan elapsedTime = DateTime.Now - CaptureStartTime;
                estimatedCaptureTimeInSec = EstimatedCaptureTime;

                if (estimatedCaptureTimeInSec > 0.0)
                {
                    estTimeRemain = Math.Max(0, estimatedCaptureTimeInSec - elapsedTime.TotalSeconds);
                    percentCompete = 100.0 * elapsedTime.TotalSeconds / estimatedCaptureTimeInSec;
                }
                else
                {
                    estTimeRemain = 0;
                    percentCompete = 0;
                }

                Owner.Dispatcher.Invoke((Action)delegate
                {
                    if (estTimeRemain > 0.0)
                    {
                        this.EstimatedTimeRemaining = "Estimated Time Remaining: " + estTimeRemain.ToString("F1") + " [sec]";
                        this.PercentComplete = percentCompete;
                    }
                    else
                    {
                        this.EstimatedTimeRemaining = string.Empty;
                    }
                });

                // Forcing the CommandManager to raise the RequerySuggested event
                CommandManager.InvalidateRequerySuggested();
            }
        }
        #endregion


        #region Helper Methods

        public void ShowOnscreenKeyboard()
        {
            try
            {
                string progFiles = @"C:\Program Files\Common Files\Microsoft Shared\ink";
                string keyboardPath = System.IO.Path.Combine(progFiles, "TabTip.exe");

                System.Diagnostics.Process.Start(keyboardPath);
            }
            catch
            {
            }
        }

        public void HideOnscreenKeyboard()
        {
            // retrieve the handler of the window
            int iHandle = Utilities.WindowsInvoke.FindWindow("IPTIP_Main_Window", "");
            if (iHandle > 0)
            {
                // close the window using API
                Utilities.WindowsInvoke.SendMessage(iHandle, Utilities.WindowsInvoke.WM_SYSCOMMAND, Utilities.WindowsInvoke.SC_CLOSE, 0);
            }
        }

        #endregion


        #region  Rotating
        private void Rotating()
        {
            if (ActiveDocument.Image.Format != PixelFormats.Gray16)
            {
                MessageBox.Show("This format is not supported!", "warning");
                return;
            }
            ImageRotatingProcess imageRotating = new ImageRotatingProcess();
            imageRotating.Init();
            imageRotating.Show();
        }
    
        #endregion

        #region Gif
        private void GifWindown()
        {
            GifAnimatedWindow gifAnimatedWindow = new GifAnimatedWindow();
            gifAnimatedWindow.ShowDialog();
        }

        #endregion
        //单位ＭＢ
        public static double GetHardDiskSpace(string str_HardDiskName)
        {
            double totalSize = 0;
            str_HardDiskName = str_HardDiskName + ":\\";
            System.IO.DriveInfo[] drives = System.IO.DriveInfo.GetDrives();
            foreach (System.IO.DriveInfo drive in drives)
            {
                if (drive.Name == str_HardDiskName)
                {
                    totalSize = drive.TotalFreeSpace / (1024 * 1024);
                }
            }
            return totalSize;
        }
        public static double GetImageSpace(WriteableBitmap _wbimage)
        {
            int bpp = _wbimage.Format.BitsPerPixel;
            int width = _wbimage.PixelWidth;
            int hight = _wbimage.PixelHeight;
            double max = width * hight * 2;
            double value = max / 1024 / 1024;
            if (bpp == 24 || bpp == 48)
            {
                value = value * 3;
            }
            return Math.Round(value);
        }

        /*#region Version Info

        public string SoftwareVersion
        {
            get { return string.Format("Software Version: {0}", _Owner.ProductVersion); }
        }

        public string CameraFirmware
        {
            get { return string.Format("Camera Firmware: {0}", CameraFWVersion); }
        }

        public string ControllerFirmware
        {
            get { return string.Format("Control Board Firmware: {0}", ControllerFWVersion); }
        }

        #endregion*/


    }

}
